// <copyright file="Program.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
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
        public static async Task Main(string[] args) => await BuildCommandLine()
            .UseHost(
                _ => Host.CreateDefaultBuilder(),
                host =>
                {
                    host.ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddKubernetes();
                    });
                })
            .UseDefaults()
            .Build()
            .InvokeAsync(args);

        private static CommandLineBuilder BuildCommandLine()
        {
            return new CommandLineBuilder(null);
        }
    }
}
