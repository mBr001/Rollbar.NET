﻿namespace Sample.AspNetCore2.WebApi
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rollbar;
    using Rollbar.NetCore.AspNet;
    using Swashbuckle.AspNetCore.Swagger;

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
            //{"Endpoint Routing does not support 'IApplicationBuilder.UseMvc(...)'. To use 'IApplicationBuilder.UseMvc' set 'MvcOptions.EnableEndpointRouting = false' inside 'ConfigureServices(...)."}
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddOptions();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            ConfigureRollbarSingleton();

            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            //});

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRollbarMiddleware();

            // Any other middleware component intended to be "monitored" by Rollbar.NET middleware
            // go below this line:

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            //app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            //});

            app.UseMvc();
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private void ConfigureRollbarSingleton()
        {
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            RollbarConfig rollbarConfig = 
                new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment };
            rollbarConfig.ScrubFields = new string[] // data fields which values we want to mask as "***"
            {
                //"url",
                //"method",
            };

            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                // if you remove line below the logger's configuration will be auto-loaded from appsettings.json
                //.Configure(new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment })
                .Configure(rollbarConfig)
                // optional step if you would like to monitor Rollbar internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;
        }

        /// <summary>
        /// Called when Rollbar internal event detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            Console.WriteLine(e.TraceAsString());

            RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
            if (apiErrorEvent != null)
            {
                //TODO: handle/report Rollbar API communication error event...
                return;
            }
            CommunicationEventArgs commEvent = e as CommunicationEventArgs;
            if (commEvent != null)
            {
                //TODO: handle/report Rollbar API communication event...
                return;
            }
            CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
            if (commErrorEvent != null)
            {
                //TODO: handle/report basic communication error while attempting to reach Rollbar API service... 
                return;
            }
            InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
            if (internalErrorEvent != null)
            {
                //TODO: handle/report basic internal error while using the Rollbar Notifier... 
                return;
            }
        }

    }
}
