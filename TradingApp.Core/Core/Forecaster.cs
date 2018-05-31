using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.ServerRelated;
using TradingApp.Domain.ViewModels;
using Microsoft.Extensions.Logging;

namespace TradingApp.Core.Core
{
    public class Forecaster : IForecaster
    {
        private readonly IProcessModel _processModel;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        private readonly IPythonExec _pythonExec;
        private readonly IUtility _utility;
        private readonly IRequests _requestHelper;
        private readonly ILogger _logger;

        public Forecaster(IProcessModel processModel,
            IDirectoryManager directoryManager,
            IFileManager fileManager,
            IPythonExec pythonExec,
            IUtility utility,
            IRequests requests,
            ILoggerFactory logger)
        {
            _processModel = processModel;
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _pythonExec = pythonExec;
            _utility = utility;
            _requestHelper = requests;
            _logger = logger.CreateLogger("Forecasts");
        }

        public ServerRequestsStats GetStats()
        {
            return _requestHelper.GetStats();
        }

        public async Task<ManualViewModel> MakeManualForecast(string asset, int dataHours, int periods,
            bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new ManualViewModel();

            var directory = _directoryManager;
            var file = _fileManager;

            try
            {
                var normalized = _processModel.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Manual);

                var csv = _fileManager.CreateDataCsv(normalized, location);
                if (string.IsNullOrEmpty(csv))
                {
                    throw new Exception("Not enough data: " + asset);
                }

                _directoryManager.SaveDataFile(csv, location);

                _pythonExec.RunPython(location, periods, hourlySeasonality, dailySeasonality);

                var pathToOut = directory.FilePathOut(location);
                var pathToComponents = directory.FileComponentsOut(location);
                var pathToForecast = directory.FileForecastOut(location);
                var outCreated = await directory.WaitForFile(pathToOut, 60);
                var componentsCreated = await directory.WaitForFile(pathToComponents, 10);
                var forecastCreated = await directory.WaitForFile(pathToForecast, 10);
                var images = directory.ImagePath(DirSwitcher.Manual);

                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    throw new Exception("forecast.png not found");
                }

                if (componentsCreated)
                {
                    viewModel.ComponentsPath = images.ComponentsImage;
                }
                else
                {
                    throw new Exception("components.png not found");
                }

                if (outCreated)
                {
                    var stats = file.BuildOutTableRows(pathToOut, periods);
                    var performance = _utility.DefinePerformance(stats);
                    viewModel.Table = stats.Table;
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("N2");
                    viewModel.Width = performance.Width.ToString();
                    var marketFeatures = _utility.GetFeatures(normalized, asset);
                    viewModel.Volume = marketFeatures.Volume.ToString();
                    viewModel.Change = marketFeatures.Change.ToString("N2");
                    var rsi = _utility.Rsi(normalized);
                    viewModel.Rsi = rsi.ToString("N2");
                }
                else
                {
                    throw new Exception("out.csv not found");
                }

                viewModel.AssetName = asset;


                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<AutoViewModel> MakeAutoForecast(int dataHours, int periods, bool hourlySeasonality,
            bool dailySeasonality, string readFrom)
        {
            var viewModel = new AutoViewModel();
            _logger.LogWarning($"Creating Auto Forecast for hours {dataHours}, " +
                               $"for periods {periods}, " +
                               $"from file {readFrom}, " +
                               $"hourly seasonality: {hourlySeasonality}, " +
                               $"daily seasonality: {dailySeasonality}.");

            string lastFolder;
            try
            {
                IEnumerable<string> assets;
                if (readFrom.ToLower() == "assets")
                {
                    assets = _fileManager.ReadAssetsFromExcel(_directoryManager.AsstesLocation);
                }
                else
                {
                    assets = _fileManager.ReadAssetsFromExcel(_directoryManager.ObservablesLocation);
                }

                var currentTime = DateTime.Now;
                Parallel.ForEach(assets, asset =>
                    {
                        var pathToFolder =
                            _directoryManager.GenerateForecastFolder(asset, periods, DirSwitcher.Auto, currentTime);

                        var normalized = _processModel.GetDataAuto(asset, dataHours);
                        if (normalized == null || !normalized.Any())
                        {
                            _directoryManager.RemoveFolder(pathToFolder);
                            var zeroResults = new ExcelLog()
                            {
                                AssetName = asset,
                                Log = Indicator.ZeroRezults.ToString(),
                                Rate = "0",
                                Width = "0",
                                Volume = "0",
                                Change = "0",
                                Rsi = "0"
                            };
                            Shared.Log(zeroResults);
                            return;
                        }

                        var csv = _fileManager.CreateDataCsv(normalized, pathToFolder);
                        if (string.IsNullOrEmpty(csv))
                        {
                            var zeroResults = new ExcelLog()
                            {
                                AssetName = asset,
                                Log = Indicator.ZeroRezults.ToString(),
                                Rate = "0",
                                Width = "0",
                                Volume = "0",
                                Change = "0",
                                Rsi = "0"
                            };
                            Shared.Log(zeroResults);
                            return;
                        }

                        _directoryManager.SaveDataFile(csv, pathToFolder);

                        _pythonExec.RunPython(pathToFolder, periods, hourlySeasonality, dailySeasonality);

                        var pathToOut = _directoryManager.FilePathOut(pathToFolder);
                        var pathToComponents = _directoryManager.FileComponentsOut(pathToFolder);
                        var pathToForecast = _directoryManager.FileForecastOut(pathToFolder);

                        var outCreated = _directoryManager.WaitForFile(pathToOut, 60);
                        var componentsCreated = _directoryManager.WaitForFile(pathToComponents, 10);
                        var forecastCreated = _directoryManager.WaitForFile(pathToForecast, 10);

                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;

                        var stats = _fileManager.BuildOutTableRows(pathToOut, periods);
                        var performance = _utility.DefinePerformance(stats);
                        var marketFeatures = _utility.GetFeatures(normalized, asset);
                        var rsi = _utility.Rsi(normalized);
                        var log = new ExcelLog
                        {
                            AssetName = asset,
                            Log = performance.Indicator.ToString(),
                            Rate = performance.Rate.ToString(),
                            Width = performance.Width.ToString(),
                            Volume = marketFeatures.Volume.ToString() + "BTC",
                            Change = marketFeatures.Change.ToString("N2"),
                            Rsi = rsi.ToString("N2") + "%"
                        };
                        Shared.Log(log);
                        _directoryManager.SpecifyDirByTrend(performance.Indicator, pathToFolder);
                    }
                );

                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                var results = _directoryManager.GetListByIndicator(lastFolder);
                viewModel.NegativeAssets = results.NegativeAssets;
                viewModel.NeutralAssets = results.NeutralAssets;
                viewModel.PositiveAssets = results.PositiveAssets;
                viewModel.StrongPositiveAssets = results.StrongPositiveAssets;
                var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetLog);
                _directoryManager.WriteLogToExcel(lastFolder, res);
                Shared.ClearLog();
                viewModel.Report = _directoryManager.GetReport(lastFolder);

                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                _logger.LogWarning($"Finished auto forecast from file {readFrom}");
            }
            catch (Exception e)
            {
                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                if (Shared.GetLog.Any())
                {
                    var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetLog);
                    _directoryManager.WriteLogToExcel(lastFolder, res);
                    Shared.ClearLog();
                }

                _logger.LogError($"Error in Auto Forecast for hours {dataHours}, " +
                                 $"for periods {periods}, " +
                                 $"from file {readFrom}, " +
                                 $"hourly seasonality: {hourlySeasonality}, " +
                                 $"daily seasonality: {dailySeasonality}.");
                throw new Exception(e.Message);
            }

            return viewModel;
        }

        public async Task<BtcViewModel> InstantForecast()
        {
            var viewModel = new BtcViewModel();
            const int periods = 24;
            const int dataHours = 230;
            const bool hourlySeasonality = false;
            const bool dailySeasonality = false;
            var numFormat = new CultureInfo("en-US", false).NumberFormat;
            numFormat.PercentDecimalDigits = 2;

            var settingsJson = _directoryManager.CustomSettings;
            var settings = _fileManager.ReadCustomSettings(settingsJson);
            var asset = settings.Btc;

            try
            {
                var normalized = _processModel.GetDataManual(asset, dataHours);
                var location = _directoryManager.GenerateForecastFolder(asset, periods, DirSwitcher.Instant);

                var csv = _fileManager.CreateDataCsv(normalized, location);
                if (string.IsNullOrEmpty(csv))
                {
                    throw new Exception("Not enough data: " + asset);
                }

                _directoryManager.SaveDataFile(csv, location);

                _pythonExec.RunPython(location, periods, hourlySeasonality, dailySeasonality);

                var pathToOut = _directoryManager.FilePathOut(location);
                var pathToComponents = _directoryManager.FileComponentsOut(location);
                var pathToForecast = _directoryManager.FileForecastOut(location);

                var outCreated = await _directoryManager.WaitForFile(pathToOut, 60);
                var componentsCreated = await _directoryManager.WaitForFile(pathToComponents, 10);
                var forecastCreated = await _directoryManager.WaitForFile(pathToForecast, 10);

                var images = _directoryManager.ImagePath(DirSwitcher.Instant);

                if (outCreated)
                {
                    var stats = _fileManager.BuildOutTableRows(pathToOut, periods);

                    var performance = _utility.DefinePerformance(stats);
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("P", numFormat);
                    viewModel.Width = performance.Width.ToString();
                    var marketFeatures = _utility.GetFeatures(normalized, asset);
                    viewModel.Volume = marketFeatures.Volume.ToString();
                    viewModel.Change = marketFeatures.Change.ToString("N2");
                    var rsi = _utility.Rsi(normalized);
                    viewModel.Rsi = rsi.ToString("N2");
                }
                else
                {
                    throw new Exception("out.csv not found");
                }

                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    throw new Exception("forecast.png not found");
                }

                if (!componentsCreated)
                {
                    throw new Exception("components.png not found");
                }

                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                viewModel.AssetName = asset;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return viewModel;
        }

        public async Task<BotViewModel> MakeBotForecast(int rsi, List<int> trend, List<int> border)
        {
            var viewModel = new BotViewModel();
            if (trend.Count <= 0)
            {
                throw new Exception("Select at least one trend option!");
            }

            if (border.Count <= 0)
            {
                throw new Exception("Select at least one border option!");
            }

            const int periods = 24;
            const int dataHours = 230;

            IEnumerable<string> assets;
            string lastFolder;
            try
            {
                //assets = _fileManager.ReadAssetsFromExcel(_directoryManager.AsstesLocation);
                assets = _fileManager.ReadAssetsFromExcel(_directoryManager.ObservablesLocation);
                var currentTime = DateTime.Now;
                Parallel.ForEach(assets, asset =>
                    {
                        var pathToFolder =
                            _directoryManager.GenerateForecastFolder(asset, periods, DirSwitcher.BotForecast,
                                currentTime);

                        var normalized = _processModel.GetDataAuto(asset, dataHours);
                        if (normalized == null || !normalized.Any())
                        {
                            _directoryManager.RemoveFolder(pathToFolder);
                            var zeroResults = new ExcelBotArrangeLog()
                            {
                                AssetName = asset,
                                Log = Indicator.ZeroRezults.ToString(),
                                Rate = "1",
                                Width = "0",
                                Volume = "0",
                                Change = "0",
                                Rsi = "0",
                                BotArrange = BotArrange.DontBuy.ToString()
                            };
                            Shared.ArrangeBotLog(zeroResults);
                            return;
                        }

                        var csv = _fileManager.CreateDataCsv(normalized, pathToFolder);
                        if (string.IsNullOrEmpty(csv))
                        {
                            var zeroResults = new ExcelBotArrangeLog()
                            {
                                AssetName = asset,
                                Log = Indicator.ZeroRezults.ToString(),
                                Rate = "1",
                                Width = "0",
                                Volume = "0",
                                Change = "0",
                                Rsi = "0",
                                BotArrange = BotArrange.DontBuy.ToString()
                            };
                            Shared.ArrangeBotLog(zeroResults);
                            return;
                        }

                        _directoryManager.SaveDataFile(csv, pathToFolder);

                        _pythonExec.RunPython(pathToFolder, periods, false, false);

                        var pathToOut = _directoryManager.FilePathOut(pathToFolder);
                        var pathToComponents = _directoryManager.FileComponentsOut(pathToFolder);
                        var pathToForecast = _directoryManager.FileForecastOut(pathToFolder);

                        var outCreated = _directoryManager.WaitForFile(pathToOut, 60);
                        var componentsCreated = _directoryManager.WaitForFile(pathToComponents, 10);
                        var forecastCreated = _directoryManager.WaitForFile(pathToForecast, 10);

                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;

                        var stats = _fileManager.BuildOutTableRows(pathToOut, periods);
                        var performance = _utility.DefinePerformance(stats);
                        var marketFeatures = _utility.GetFeatures(normalized, asset);
                        var rsiCalculated = _utility.Rsi(normalized);

                        var botArrange = _directoryManager.SpecifyDirByIndicators(pathToFolder, rsi, trend, border,
                            performance, rsiCalculated);
                        var log = new ExcelBotArrangeLog()
                        {
                            AssetName = asset,
                            Log = performance.Indicator.ToString(),
                            Rate = performance.Rate.ToString(),
                            Width = performance.Width.ToString(),
                            Volume = marketFeatures.Volume.ToString() + "BTC",
                            Change = marketFeatures.Change.ToString("N2"),
                            Rsi = rsiCalculated.ToString("N2") + "%",
                            BotArrange = botArrange.ToString()
                        };
                        Shared.ArrangeBotLog(log);
                    }
                );

                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.BotForecast);
                var results = _directoryManager.GetListByBotArrange(lastFolder);
                viewModel.Buy = results.BuyAssets;
                viewModel.Consider = results.ConsiderAssets;
                viewModel.DontBuy = results.DontBuyAssets;

                var res = _fileManager.WriteArrangeBotLogExcel(lastFolder, Shared.GetArrangedBotLog);
                _directoryManager.WriteArrangeBotLogToExcel(lastFolder, res);
                Shared.ClearArrangeBotLog();
                viewModel.Report = _directoryManager.GetArrangeBotReport(lastFolder);

                var requests = _requestHelper.GetStats();
                viewModel.CallsMadeHisto = requests.CallsMade.Histo;
                viewModel.CallsLeftHisto = requests.CallsLeft.Histo;
                return viewModel;
            }
            catch (Exception e)
            {
                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.BotForecast);
                if (Shared.GetArrangedBotLog.Any())
                {
                    var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetArrangedBotLog);
                    _directoryManager.WriteLogToExcel(lastFolder, res);
                    Shared.ClearArrangeBotLog();
                }

                throw new Exception(e.Message);
            }
        }
    }
}