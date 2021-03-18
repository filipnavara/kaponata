// <copyright file="ProgramTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Sidecars.Tests
{
    /// <summary>
    /// Tests the <see cref="Program"/> class.
    /// </summary>
    public class ProgramTests
    {
        /// <summary>
        /// <see cref="Program.ConfigureHost(IHostBuilder)"/> configures a host from which a
        /// <see cref="UsbmuxdSidecar"/> instance can be retrieved.
        /// </summary>
        [Fact]
        public void ConfigureHost_ConfiguresSideCar()
        {
            var builder = new HostBuilder();
            Program.ConfigureHost(builder);
            var host = builder.Build();

            Assert.NotNull(host.Services.GetRequiredService<UsbmuxdSidecar>());
        }

        /// <summary>
        /// <see cref="Program.BuildCommandLine"/> works.
        /// </summary>
        [Fact]
        public void BuildCommandLine_Works()
        {
            var parser = Program.BuildCommandLine();
            Assert.NotNull(parser.Configuration.RootCommand);
        }

        /// <summary>
        /// <see cref="Program.RunAsync(IHost, string)"/> starts and stops the sidecar.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RunAsync_StartsStopsSidecar_Async()
        {
            var sidecar = new Mock<UsbmuxdSidecar>(MockBehavior.Strict);
            sidecar.Setup(s => s.StartAsync(default)).Returns(Task.CompletedTask).Verifiable();
            sidecar.Setup(s => s.StopAsync(default)).Returns(Task.CompletedTask).Verifiable();

            var configuration = new UsbmuxdSidecarConfiguration();

            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureServices(
                s =>
                {
                    s.AddSingleton(sidecar.Object);
                    s.AddSingleton(configuration);
                });

            var host = hostBuilder.Build();

            await host.StartAsync(default);

            var task = Program.RunAsync(host, "my-pod");

            await host.StopAsync(default);
            await task;

            sidecar.Verify();
        }
    }
}
