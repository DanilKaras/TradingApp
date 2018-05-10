using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingApp.Core.Core;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Core.Scheduler
{
    public class ForecasterSheduled: HostedService
    {
        private readonly ITelegram _messenger;
        private readonly IProcessModel _processModel;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        private readonly IPythonExec _pythonExec;
        private readonly IUtility _utility;
        private readonly IRequests _requestHelper;
        
        public ForecasterSheduled(ITelegram messenger, 
            IProcessModel processModel, 
            IDirectoryManager directoryManager, 
            IFileManager fileManager, 
            IPythonExec pythonExec, 
            IUtility utility, 
            IRequests requests)
        {
            _messenger = messenger;
            _processModel = processModel;
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _pythonExec = pythonExec;
            _utility = utility;
            _requestHelper = requests;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);
                await AutoForecaset();
            }
        }

        private async Task AutoForecaset()
        {
            string lastFolder;
            const int dataHours = 200;
            const int periods = 24;
            const bool hourlySeasonality = false;
            const bool dailySeasonality = false;
            const string readFrom = "assets";
            try
            {
                IEnumerable<string> assets; 
                if (readFrom.ToLower().Equals("assets"))
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
                            Shared.Log(asset, Indicator.ZeroRezults, 0, "0", 0, 0, 0);
                            return;
                        }
                        
                        var csv = _fileManager.CreateDataCsv(normalized, pathToFolder);
                        if (string.IsNullOrEmpty(csv))
                        {
                            Shared.Log(asset, Indicator.ZeroRezults, 0, "0", 0, 0, 0);
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
                        Shared.Log(asset, performance.Indicator, performance.Rate, performance.Width.ToString(),marketFeatures.Volume, marketFeatures.Change, rsi);
                        _directoryManager.SpecifyDirByTrend(performance.Indicator, pathToFolder);
                    }
                );

                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                var results = _directoryManager.GetListByIndicator(lastFolder);
               
                var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetLog);
                _directoryManager.WriteLogToExcel(lastFolder, res);
                Shared.ClearLog();
                var model = _requestHelper.GetStats();
                await _messenger.SendMessage("Done Assets");
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
                await _messenger.SendMessage("Failed Assets");
                throw new Exception(e.Message);
            }
        }
    }
}