// <copyright file="Program.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Kaponata.Api
{
    /// <summary>
    /// Represents the main entry point to the Kaponata API server.
    /// </summary>
    public class Program
    {
        private readonly IConsole console;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="console">
        /// The console to use by the application.
        /// </param>
        public Program(IConsole? console = null)
        {
            this.console = console ?? new SystemConsole();
        }

        /// <summary>
        /// Runs the Kaponata API server, either in command line or service mode.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public static Task<int> Main(string[] args)
        {
            var program = new Program();
            return program.MainAsync(args);
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

        /// <summary>
        /// Runs the Kaponata Operator, either in command line or service mode.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public Task<int> MainAsync(string[] args)
        {
            // Create a root command with default options
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() => CreateHostBuilder(args).Build().RunAsync()),
            };

            return new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build()
                .InvokeAsync(args, this.console);
        }
    }
}
