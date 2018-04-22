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
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string _currentLocation;

        public Helpers(IOptions<ApplicationSettings> settings, string env)
        {
            _appSettings = settings;
            _currentLocation = env;
        }

        public SettingsViewModel LoadExchanges()
        {
            try
            {
                var viewModel = new SettingsViewModel();
                IRequests exchanges = new Requests();
                IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
                IFileManager file = new FileManager(_appSettings);
                var settingsJson = directory.CustomSettings;
                var settings = file.ReadCustomSettings(settingsJson);

                viewModel.Exchanges = exchanges.GetExchanges();
                viewModel.Btc = settings.Btc;
                viewModel.LastExchange = settings.Exchange;
                viewModel.LowerBorder = settings.LowerBorder;
                viewModel.UpperBorder = settings.UpperBorder;

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
                IRequests update = new Requests();
                IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);

                IFileManager file = new FileManager(_appSettings);
                var model = update.GetAssets(settings.LastExchange);
                file.WriteAssetsToExcel(directory.AsstesUpdateLocation, model);
                newSettings.Btc = model.Btc;
                newSettings.Exchange = model.ExchangeName;
                newSettings.LowerBorder = settings.LowerBorder;
                newSettings.UpperBorder = settings.UpperBorder;

                var json = file.ConvertCustomSettings(newSettings);
                directory.UpdateCustomSettings(json);

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
                IDirectoryManager folder = new DirectoryManager(_appSettings, _currentLocation);
                var lastFolder = folder.GetLastFolder(DirSwitcher.Auto);
                var results = folder.GetListByIndicator(lastFolder);
                viewModel.NegativeAssets = results.NegativeAssets;
                viewModel.NeutralAssets = results.NeutralAssets;
                viewModel.PositiveAssets = results.PositiveAssets;
                viewModel.StrongPositiveAssets = results.StrongPositiveAssets;
                viewModel.Report = folder.GetReport(lastFolder);
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
                IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
                IFileManager file = new FileManager(_appSettings);
                var assets = file.ReadAssetsFromExcel(directory.AsstesLocation);
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

                IDirectoryManager manager = new DirectoryManager(_appSettings, _currentLocation);
                IFileManager file = new FileManager(_appSettings);
                var folder = manager.GetLastFolder(DirSwitcher.Auto);

                var dir = manager.GetDirByIndicator(folder, indicator);

                var targetFolder = manager.GetForecastFolderByName(dir, assetName);
                var images = manager.ImagePath(DirSwitcher.Auto, indicator, targetFolder, folder);
                viewModel.ComponentsPath = images.ComponentsImage;
                viewModel.ForecastPath = images.ForecastImage;


                viewModel.AssetName = assetName;
                viewModel.Indicator = indicator;
                var pathToOut = manager.FilePathOut(Path.Combine(dir, targetFolder));
                var stats = file.BuildOutTableRows(pathToOut, periods);
                viewModel.Table = stats.Table;
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public void WriteObservables(List<string> observableList)
        {         
            try
            {
                IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
                IFileManager file = new FileManager(_appSettings);
                file.WriteObservables(observableList, directory.ObservablesLocationUpdate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}