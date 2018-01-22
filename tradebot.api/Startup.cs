using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tradebot.core;

namespace tradebot.api
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
            services.AddDbContext<TradeBotDbContext>(ServiceLifetime.Transient);

            services.AddCap(cap =>
            {
                cap.UseEntityFramework<TradeBotDbContext>();
                cap.UseRabbitMQ(Configuration["CAP:RabbitMQ:HostName"]);
            });
            services.AddTransient<ICapPublisher, CapPublisher>();
            services.AddMvc();
            services.AddSingleton<IHostedService, TradeBotService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCap();
            app.UseMvc();
        }
    }
}
