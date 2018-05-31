using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using TradingApp.Data.Managers;
using TradingApp.Data.ServerRequests;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Core.Core
{
    public class Helpers : IHelpers
    {
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        private readonly IRequests _requests;
        public Helpers(IDirectoryManager directoryManager, IFileManager fileManager, IRequests requests)
        {
            _directoryManager = directoryManager;
            _fileManager = fileManager;
            _requests = requests;
        }

        public SettingsViewModel LoadExchanges()
        {
            try
            {
                var viewModel = new SettingsViewModel();
                
                
                var settingsJson = _directoryManager.CustomSettings;
                var settings = _fileManager.ReadCustomSettings(settingsJson);

                viewModel.Exchanges = _requests.GetExchanges();
                viewModel.Btc = settings.Btc;
                viewModel.LastExchange = settings.Exchange;
                viewModel.LowerBorder = settings.LowerBorder;
                viewModel.UpperBorder = settings.UpperBorder;
                viewModel.UpperWidth = settings.UpperWidth;
                viewModel.LowerWidth = settings.LowerWidth;
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public CustomSettings UpdateExchanges(SettingsViewModel settings)
        {
            try
            {
                var newSettings = new CustomSettings();
               

                var model = _requests.GetAssets(settings.LastExchange);
                _fileManager.WriteAssetsToExcel(_directoryManager.AsstesUpdateLocation, model);
                newSettings.Btc = model.Btc;
                newSettings.Exchange = model.ExchangeName;
                newSettings.LowerBorder = settings.LowerBorder;
                newSettings.UpperBorder = settings.UpperBorder;
                newSettings.UpperWidth = settings.UpperWidth;
                newSettings.LowerWidth = settings.LowerWidth;
                var json = _fileManager.ConvertCustomSettings(newSettings);
                _directoryManager.UpdateCustomSettings(json);

                return newSettings;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public AutoViewModel GetLatestAssets()
        {
            try
            {
                var viewModel = new AutoViewModel();
                
                var lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                var results = _directoryManager.GetListByIndicator(lastFolder);
                viewModel.NegativeAssets = results.NegativeAssets;
                viewModel.NeutralAssets = results.NeutralAssets;
                viewModel.PositiveAssets = results.PositiveAssets;
                viewModel.StrongPositiveAssets = results.StrongPositiveAssets;
                viewModel.Report = _directoryManager.GetReport(lastFolder);
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public BotViewModel GetLatestArranged()
        {
            try
            {
                var viewModel = new BotViewModel();
                
                var lastFolder = _directoryManager.GetLastFolder(DirSwitcher.BotForecast);
                var results = _directoryManager.GetListByBotArrange(lastFolder);
                viewModel.Buy = results.BuyAssets;
                viewModel.Consider = results.ConsiderAssets;
                viewModel.DontBuy = results.DontBuyAssets;
                viewModel.Report = _directoryManager.GetArrangeBotReport(lastFolder);
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IEnumerable<string> GetAssets()
        {
            try
            {

                var assets = _fileManager.ReadAssetsFromExcel(_directoryManager.AsstesLocation);
                return assets;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public AutoComponentsViewModel GetForecastData(Indicator indicator, string assetName, int periods)
        {
            try
            {
                var viewModel = new AutoComponentsViewModel();
                var folder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                var dir = _directoryManager.GetDirByIndicator(folder, indicator);
                var targetFolder = _directoryManager.GetForecastFolderByName(dir, assetName);
                var images = _directoryManager.ImagePath(DirSwitcher.Auto, indicator, targetFolder, folder);
                viewModel.ComponentsPath = images.ComponentsImage;
                viewModel.ForecastPath = images.ForecastImage;

                viewModel.AssetName = assetName;
                viewModel.Indicator = indicator;
                var pathToOut = _directoryManager.FilePathOut(Path.Combine(dir, targetFolder));
                var stats = _fileManager.BuildOutTableRows(pathToOut, periods);
                viewModel.Table = stats.Table;
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public ArrangeBotComponentViewModel GetBotArrangedForecastData(BotArrange arrange, string assetName, int periods)
        {
            try
            {
                var viewModel = new ArrangeBotComponentViewModel();
                var folder = _directoryManager.GetLastFolder(DirSwitcher.BotForecast);
                var dir = _directoryManager.GetDirByArrange(folder, arrange);
                var targetFolder = _directoryManager.GetForecastFolderByName(dir, assetName);
                var images = _directoryManager.ImagePathByArrange(arrange, targetFolder, folder);
                viewModel.ComponentsPath = images.ComponentsImage;
                viewModel.ForecastPath = images.ForecastImage;
                viewModel.AssetName = assetName;
                var pathToOut = _directoryManager.FilePathOut(Path.Combine(dir, targetFolder));
                var stats = _fileManager.BuildOutTableRows(pathToOut, periods);
                viewModel.Table = stats.Table;
                viewModel.Arrange = arrange.ToString();
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void WriteObservables(IEnumerable<string> assets)
        {         
            try
            {
                _fileManager.WriteAssets(assets, _directoryManager.ObservablesLocationUpdate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void WritAsstesForBot(IEnumerable<string> assets)
        {
            try
            {
                _fileManager.WriteAssets(assets, _directoryManager.AssetsForBotLocation);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}