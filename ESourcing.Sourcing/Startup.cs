using Esourcing.Sourcing.Data;
using Esourcing.Sourcing.Data.Interface;
using Esourcing.Sourcing.Repositories;
using Esourcing.Sourcing.Repositories.Interfaces;
using Esourcing.Sourcing.Settings;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace ESourcing.Sourcing
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
            services.AddControllers();

            services.Configure<SourcingDatabaseSettings>(Configuration.GetSection(nameof(SourcingDatabaseSettings)));
            services.AddSingleton<ISourcingDatabaseSettings>(sp => sp.GetRequiredService<IOptions<SourcingDatabaseSettings>>().Value);

            #region Project Dependencies
            services.AddTransient<ISourcingContext, SourcingContext>();
            services.AddTransient<IBidRepository, BidRepository>();
            services.AddTransient<IAuctionRepository, AuctionRepository>();
            services.AddAutoMapper(typeof(Startup));
            #endregion

            #region Swagger Dependencies
            services.AddSwaggerGen(
                c => c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "ESourcing.Sourcing",
                    Version = "v1"
                })
            );
            #endregion

            #region Event Bus 
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBus:HostName"]
                };

                var userName = Configuration["EventBus:UserName"];
                if (!string.IsNullOrWhiteSpace(userName))
                    factory.UserName = userName;

                var password = Configuration["EventBus:Password"];
                if (!string.IsNullOrWhiteSpace(password))
                    factory.Password = password;

                var retryCount = 5;
                if (!string.IsNullOrWhiteSpace(Configuration["EventBus:RetryCount"]))
                    retryCount = int.Parse(Configuration["EventBus:RetryCount"]);

                return new DefaultRabbitMQPersistentConnection(factory, retryCount, logger);
            });

            services.AddSingleton<EventBusRabbitMQProducer>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ESourcing.Sourcing v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
