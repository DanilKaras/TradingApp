using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TradingApp.Core.Core;
using TradingApp.Domain.Interfaces;
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
            services.AddMvc();
            services.Configure<ApplicationSettings>(options => Configuration.GetSection("ApplicationSettings").Bind(options));
            services.AddTransient<IForecaster, Forecaster>();
            services.AddTransient<IHelpers, Helpers>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            var appSetting = Configuration["ApplicationSettings:ForecastDir"];
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), appSetting)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), appSetting));
            }
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), appSetting)),
                RequestPath = Path.DirectorySeparatorChar + appSetting
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