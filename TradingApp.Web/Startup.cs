using System;
using System.Diagnostics;
using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using TradingApp.Core.Core;
using TradingApp.Domain.Models;

namespace TradingApp.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get;}
        
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
           
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //var connection = Configuration["ConnectionStrings:ConnectionName"];
            services.Configure<DbSettings>(options =>
            {
                options.ConnectionString 
                    = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database 
                    = Configuration.GetSection("MongoConnection:Database").Value;
            });

            services.AddHangfire(x => x.UseMongoStorage(Configuration.GetSection("MongoConnection:ConnectionString").Value, 
                Configuration.GetSection("MongoConnection:Database").Value));
            services.AddMvc();

            services.RegisterServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddLog4Net();

            GlobalConfiguration.Configuration.UseMongoStorage(Configuration.GetSection("MongoConnection:ConnectionString").Value, 
                Configuration.GetSection("MongoConnection:Database").Value);
            
            var options = new BackgroundJobServerOptions
            {
                ServerName = string.Format(
                    "{0}.{1}",
                    Environment.MachineName,
                    Guid.NewGuid().ToString())
            };
            
            app.UseHangfireServer(options);
            app.UseHangfireDashboard();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            var forecastDir = Configuration["ApplicationSettings:ForecastDir"];
            var botDir = Configuration["ApplicationSettings:BotDir"];
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), forecastDir)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), forecastDir));
            }
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), botDir)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), botDir));
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), forecastDir)),
                RequestPath = Path.DirectorySeparatorChar + forecastDir
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), botDir)),
                RequestPath = Path.DirectorySeparatorChar + botDir
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        } 
    }
}