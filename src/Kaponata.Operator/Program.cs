// <copyright file="Program.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using Kaponata.Operator.Kubernetes;
using Kaponata.Operator.Kubernetes.Polyfill;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Kaponata.Operator
{
    /// <summary>
    /// Represents the main entry point to the Kaponata.Operator program.
    /// </summary>
    public class Program
    {
        private IConsole console;

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="console">
        /// The console to use by the application.
        /// </param>
        public Program(IConsole console = null)
        {
            this.console = console ?? new SystemConsole();
        }

        /// <summary>
        /// Runs the Kaponata Operator, either in command line or service mode.
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
            var rootCommand = new RootCommand();
            rootCommand.Handler = CommandHandler.Create(() => CreateHostBuilder(args).Build().RunAsync());

            // The "install-extensions" command will install the Kubernetes extensions used by Kaponata
            var installExtensionsCommand = new Command("install-extensions", "Installs Kubernetes extensions used by Kaponata into your cluster.");
            installExtensionsCommand.Handler = CommandHandler.Create<IHost>(ExtensionsInstaller.RunAsync);
            rootCommand.AddCommand(installExtensionsCommand);

            return new CommandLineBuilder(rootCommand)
                .UseHost((host) =>
                {
                    host.ConfigureServices(
                        (services) =>
                        {
                            services.AddKubernetes();
                            services.AddLogging();
                            services.AddScoped<ExtensionsInstaller>();
                        });
                })
                .UseDefaults()
                .Build()
                .InvokeAsync(args, this.console);
        }
    }
}
