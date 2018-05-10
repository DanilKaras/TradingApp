using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingApp.Core.Scheduler;
using TradingApp.Core.TelegramMessenger;
using TradingApp.Data;
using TradingApp.Data.Managers;
using TradingApp.Data.ServerRequests;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Core.Core
{

    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            
            services.AddSingleton<ISettings, Settings>();
            services.AddSingleton<IForecaster, Forecaster>();
            services.AddSingleton<IDirectoryManager, DirectoryManager>();
            services.AddSingleton<IFileManager, FileManager>(); 
            services.AddSingleton<IHelpers, Helpers>();
            services.AddSingleton<IProcessModel, ProcessModel>();
            services.AddSingleton<IPythonExec, PythonExec>();
            services.AddSingleton<IRequests, Requests>();
            services.AddSingleton<IUtility, Utility>();
            services.AddSingleton<ITelegram, Telegram>();
            //services.AddSingleton<IHostedService, SendScheduledMessage>();
            services.AddSingleton<IHostedService, ForecasterSheduled>();
            
            return services;
        }
    }
    
}