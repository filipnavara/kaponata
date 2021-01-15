// <copyright file="Program.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Kaponata.Operator
{
    /// <summary>
    /// Represents the main entry point to the Kaponata.Operator program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs the Kaponata Operator, either in command line or service mode.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the <see cref="IHostBuilder"/> which configures the <see cref="IHost"/>
        /// in which the Kaponata Operator is hosted.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        /// <returns>
        /// A configured <see cref="IHostBuilder"/>.
        /// </returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
