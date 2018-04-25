using Microsoft.Extensions.DependencyInjection;
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
            services.AddScoped<IForecaster, Forecaster>();
            services.AddScoped<IDirectoryManager, DirectoryManager>();
            services.AddScoped<IFileManager, FileManager>(); 
            services.AddScoped<IHelpers, Helpers>();
            services.AddScoped<IProcessModel, ProcessModel>();
            services.AddScoped<IPythonExec, PythonExec>();
            services.AddScoped<IRequests, Requests>();
            services.AddScoped<IUtility, Utility>();
            return services;
        }
    }
    
}