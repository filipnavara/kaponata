// <copyright file="Program.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DependencyInjection;
using Kaponata.iOS.Muxer;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.PairingRecords;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Sidecars
{
    /// <summary>
    /// Represents the main entry point to the Kaponata.Sidecars program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Runs the Kaponata Sidecar.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public static Task Main(string[] args)
        {
            Console.WriteLine($"{ThisAssembly.AssemblyTitle} version {ThisAssembly.AssemblyInformationalVersion}");

            return BuildCommandLine()
            .InvokeAsync(args);
        }

        /// <summary>
        /// Builds the command-line interface.
        /// </summary>
        /// <returns>
        /// A <see cref="CommandLineBuilder"/> which represents the command-line application.
        /// </returns>
        public static Parser BuildCommandLine()
        {
            var root = new RootCommand();
            root.AddOption(new Option<string>("--pod-name", "The name of the pod in which the sidecar container is running."));
            root.Handler = CommandHandler.Create<IHost, string>(RunAsync);
            return new CommandLineBuilder(root)
                .UseHost(
                    _ => Host.CreateDefaultBuilder(),
                    ConfigureHost)
                .UseDefaults()
                .Build();
        }

        /// <summary>
        /// Configures the host command-line host.
        /// </summary>
        /// <param name="hostBuilder">
        /// The host builder to configure.
        /// </param>
        public static void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddKubernetes();
                services.AddAppleServices();
                services.AddSingleton<KubernetesPairingRecordStore>();
                services.AddSingleton<PairingRecordProvisioner>();
                services.AddSingleton<DeveloperDiskProvisioner>();
                services.AddSingleton<UsbmuxdSidecar>();
                services.AddSingleton<UsbmuxdSidecarConfiguration>();
            });
        }

        /// <summary>
        /// Runs the command-line application.
        /// </summary>
        /// <param name="host">
        /// The application host.
        /// </param>
        /// <param name="podName">
        /// The name of the pod in which the program is running.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RunAsync(IHost host, string podName)
        {
            var serviceProvider = host.Services;

            var configuration = serviceProvider.GetRequiredService<UsbmuxdSidecarConfiguration>();
            configuration.PodName = podName;

            if (string.IsNullOrWhiteSpace(configuration.PodName))
            {
                configuration.PodName = Environment.MachineName;
            }

            var sidecar = serviceProvider.GetRequiredService<UsbmuxdSidecar>();
            await sidecar.StartAsync(CancellationToken.None).ConfigureAwait(false);

            await host.WaitForShutdownAsync().ConfigureAwait(false);

            await sidecar.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
