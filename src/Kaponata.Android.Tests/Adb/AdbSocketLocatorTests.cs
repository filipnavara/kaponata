// <copyright file="AdbSocketLocatorTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using Moq;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="AdbSocketLocator"/> class.
    /// </summary>
    public class AdbSocketLocatorTests
    {
        /// <summary>
        /// Tests the default values for the <see cref="AdbSocketLocator.GetAdbSocket"/> class.
        /// </summary>
        [Fact]
        public void GetAdbSocket_DefaultValues()
        {
            var locator = new AdbSocketLocator();

            (Socket socket, EndPoint endPoint) = locator.GetAdbSocket();

            Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
            Assert.Equal(SocketType.Stream, socket.SocketType);

            var ipEndPoint = Assert.IsType<IPEndPoint>(endPoint);
            Assert.Equal(IPAddress.Loopback, ipEndPoint.Address);
            Assert.Equal(5037, ipEndPoint.Port);
        }

        /// <summary>
        /// When the <c>ADB_SOCKET_ADDRESS</c> variable is set, <see cref="AdbSocketLocator.GetAdbSocket"/>
        /// returns a TCP socket.
        /// </summary>
        [Fact]
        public void GetAdbSocket_ForceTcp()
        {
            var locator = new Mock<AdbSocketLocator>()
            {
                CallBase = true,
            };
            locator.Setup(l => l.GetSocketAddressEnvironmentVariable()).Returns("10.0.0.1:1234");

            (Socket socket, EndPoint endPoint) = locator.Object.GetAdbSocket();

            Assert.Equal(ProtocolType.Tcp, socket.ProtocolType);
            Assert.Equal(AddressFamily.InterNetworkV6, socket.AddressFamily);
            Assert.Equal(SocketType.Stream, socket.SocketType);

            var ipEndPoint = Assert.IsType<IPEndPoint>(endPoint);
            Assert.Equal(IPAddress.Parse("10.0.0.1"), ipEndPoint.Address);
            Assert.Equal(1234, ipEndPoint.Port);
        }
    }
}
