﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog.Extensions.Logging;
using System.Net.Http;
using TomatoLog.Server.BLL;
using TomatoLog.Server.Extensions;

namespace TomatoLog.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JsonConvert.DefaultSettings = () =>
            {
                var st = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
                st.Converters.Add(new StringEnumConverter());

                return st;
            };
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
            var disCache = this.Configuration["TomatoLog:Cache-Redis"];
            if (string.IsNullOrEmpty(disCache))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                RedisHelper.Initialization(new CSRedis.CSRedisClient(disCache));
                services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
            }

            services.AddSingleton<SysConfigManager>(new SysConfigManager(this.Configuration));
            services.AddSingleton<ProConfigManager>(new ProConfigManager(this.Configuration));
            services.AddLogWriter(this.Configuration);
            services.AddSingleton<HttpClient>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IHostingEnvironment env,
                              ILoggerFactory factory,
                              IDistributedCache cache,
                              SysConfigManager sysConfig,
                              ProConfigManager proConfig,
                              IApplicationLifetime lifetime)
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
            app.UseTomatoLog(this.Configuration, cache, sysConfig, proConfig, lifetime);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
