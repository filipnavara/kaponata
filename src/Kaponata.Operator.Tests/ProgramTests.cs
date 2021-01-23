// <copyright file="ProgramTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine.IO;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests
{
    /// <summary>
    /// Tests the <see cref="Program"/> class.
    /// </summary>
    public class ProgramTests
    {
        /// <summary>
        /// <see cref="Program.Main(string[])"/> returns exit code 0 when invoked with the <c>--version</c>
        /// arguments.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Main_WithVersion_Works_Async()
        {
            Assert.Equal(0, await Program.Main(new string[] { "--version" }).ConfigureAwait(true));
        }

        /// <summary>
        /// <see cref="Program.MainAsync(string[])"/> returns exit code 0 and prints the version number.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MainAsync_WithVersion_ReturnsVersion_Async()
        {
            var console = new TestConsole();
            var program = new Program(console);
            Assert.Equal(0, await program.MainAsync(new string[] { "--version" }).ConfigureAwait(false));
            Assert.Empty(console.Error.ToString());

            // In unit tests, this will return the vstest version (e.g. 16.8.3) instead of the Kaponata version, because that's
            // the application which is execution. Just assert it's not null.
            Assert.NotEmpty(console.Out.ToString());
        }

        /// <summary>
        /// <see cref="Program.CreateHostBuilder(string[])"/> creates a host which can be stopped
        /// and started without error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateHostBuilder_Works_Async()
        {
            var builder = Program.CreateHostBuilder(Array.Empty<string>());
            var host = builder.Build();
            var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
            Assert.NotNull(hostEnvironment);

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            Assert.NotNull(configuration);

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            await host.StartAsync(default).ConfigureAwait(false);
            await host.StopAsync(default).ConfigureAwait(false);
        }
    }
}
