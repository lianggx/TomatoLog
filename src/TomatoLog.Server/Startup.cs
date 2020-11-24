using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using TomatoLog.Server.BLL;
using TomatoLog.Server.Extensions;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace TomatoLog.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration);
                builder.AddNLog().AddConsole().AddDebug();
            });

            services.AddCustomerDistributedCache(Configuration)
                        .AddSingleton<SysConfigManager>()
                        .AddSingleton<ProConfigManager>()
                        .AddHttpClient()
                        .AddLogWriter(this.Configuration)
                        .AddSingleton<HttpClient>()
                        .AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, HttpClient httpClient, IDistributedCache cache,
                              SysConfigManager sysConfig,
                              ProConfigManager proConfig,
                              IHostApplicationLifetime lifetime)
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
            app.UseRouting();
            app.UseAuthorization();
            app.UseTomatoLog(this.Configuration, cache, sysConfig, proConfig, lifetime, httpClient);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
