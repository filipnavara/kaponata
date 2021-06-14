// <copyright file="Startup.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kaponata.Api
{
    /// <summary>
    /// This class contains startup information for the Kaponata API server.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures services that are used by the Kaponata API server.
        /// </summary>
        /// <param name="services">
        /// The service collection to which to add the services.
        /// </param>
        /// <seealso href="http://go.microsoft.com/fwlink/?LinkID=398940"/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddControllers();
            services.AddKubernetes();
        }

        /// <summary>
        /// Specifies how the ASP.NET application will respond to individual HTTP requests.
        /// </summary>
        /// <param name="app">
        /// The application to configure.
        /// </param>
        /// <param name="env">
        /// The current hosting environment.
        /// </param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #if DEBUG
            app.UseCors(o =>
            {
                o.AllowAnyOrigin();
                o.AllowAnyHeader();
                o.AllowAnyMethod();
            });
            #endif

            app.Use((context, next) =>
            {
                context.Response.Headers["X-Kaponata-Version"] = ThisAssembly.AssemblyInformationalVersion;
                return next.Invoke();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health/ready");
                endpoints.MapHealthChecks("/health/alive");
            });
        }
    }
}
