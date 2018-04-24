using Microsoft.Extensions.DependencyInjection;
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
            services.AddTransient<IForecaster, Forecaster>();
            services.AddTransient<IDirectoryManager, DirectoryManager>();
            services.AddTransient<IFileManager, FileManager>(); 
            services.AddTransient<IHelpers, Helpers>();
            services.AddTransient<IProcessModel, ProcessModel>();
            services.AddTransient<IPythonExec, PythonExec>();
            services.AddTransient<IRequests, Requests>();
            services.AddTransient<IUtility, Utility>();
            return services;
        }
    }
    
}