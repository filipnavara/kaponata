// <copyright file="DeviceDataTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="DeviceData"/> class.
    /// </summary>
    public class DeviceDataTests
    {
        /// <summary>
        /// The <see cref="DeviceData"/> properties are initialized.
        /// </summary>
        [Fact]
        public void POCO_PropertiesInitialized()
        {
            var data = new DeviceData()
            {
                Features = "Features",
                Message = "Message",
                Model = "Model",
                Name = "Name",
                Product = "Product",
                Serial = "Serial",
                State = ConnectionState.Host,
                TransportId = "TransportId",
                Usb = "Usb",
            };

            Assert.Equal("Features", data.Features);
            Assert.Equal("Message", data.Message);
            Assert.Equal("Model", data.Model);
            Assert.Equal("Name", data.Name);
            Assert.Equal("Product", data.Product);
            Assert.Equal("Serial", data.Serial);
            Assert.Equal(ConnectionState.Host, data.State);
            Assert.Equal("TransportId", data.TransportId);
            Assert.Equal("Usb", data.Usb);

            Assert.Equal("Serial", data.ToString());
        }

        /// <summary>
        /// The <see cref="DeviceData.Parse(string)"/> throws and exception when invalid data is provided.
        /// </summary>
        [Fact]
        public void Parse_InvalidDataThrows()
        {
            string data = "xyz";

            Assert.Throws<ArgumentException>(() => DeviceData.Parse(data));
        }

        /// <summary>
        /// The <see cref="DeviceData.Parse(string)"/> parses the <c>ADB</c> server device data.
        /// </summary>
        /// <param name="data">
        /// The data received from the <c>ADB</c> server.
        /// </param>
        /// <param name="serial">
        /// The expected serial.
        /// </param>
        /// <param name="product">
        /// The expected product.
        /// </param>
        /// <param name="model">
        /// The expected model.
        /// </param>
        /// <param name="name">
        /// The expected name.
        /// </param>
        /// <param name="state">
        /// The expected state.
        /// </param>
        /// <param name="usb">
        /// The expected usb.
        /// </param>
        /// <param name="features">
        /// The expected features.
        /// </param>
        /// <param name="transportId">
        /// The expected transportId.
        /// </param>
        /// <param name="message">
        /// The expected message.
        /// </param>
        [Theory]
        [InlineData(
            @"169.254.138.177:5555   offline product:VS Emulator Android Device - 480 x 800 model:Android_Device___480_x_800 device:donatello",
            "169.254.138.177:5555",
            "VS Emulator Android Device - 480 x 800",
            "Android_Device___480_x_800",
            "donatello",
            ConnectionState.Offline,
            "",
            "",
            "",
            "")]
        [InlineData(
            @"009d1cd696d5194a        no permissions (user in plugdev group; are your udev rules wrong?); see [http://developer.android.com/tools/device.html",
            "009d1cd696d5194a",
            "",
            "",
            "",
            ConnectionState.NoPermissions,
            "",
            "",
            "",
            " (user in plugdev group; are your udev rules wrong?); see [http://developer.android.com/tools/device.html")]
        [InlineData(
            @"R32D102SZAE            unauthorized",
            "R32D102SZAE",
            "",
            "",
            "",
            ConnectionState.Unauthorized,
            "",
            "",
            "",
            "")]
        [InlineData(
            @"52O00ULA01             authorizing usb:9-1.4.1 transport_id:8149",
            "52O00ULA01",
            "",
            "",
            "",
            ConnectionState.Authorizing,
            "9-1.4.1",
            "",
            "8149",
            "")]
        [InlineData(
            @"emulator-5586          host features:shell_2",
            "emulator-5586",
            "",
            "",
            "",
            ConnectionState.Host,
            "",
            "shell_2",
            "",
            "")]
        [InlineData(
            @"0100a9ee51a18f2b device product:bullhead model:Nexus_5X device:bullhead features:shell_v2,cmd",
            "0100a9ee51a18f2b",
            "bullhead",
            "Nexus_5X",
            "bullhead",
            ConnectionState.Device,
            "",
            "shell_v2,cmd",
            "",
            "")]
        [InlineData(
            @"EAOKCY112414           device usb:1-1 product:WW_K013 model:K013 device:K013_1",
            "EAOKCY112414",
            "WW_K013",
            "K013",
            "K013_1",
            ConnectionState.Device,
            "1-1",
            "",
            "",
            "")]
        [InlineData(
            @"ZY3222LBDC recovery usb:337641472X product:omni_cedric device:cedric",
            "ZY3222LBDC",
            "omni_cedric",
            "",
            "cedric",
            ConnectionState.Recovery,
            "337641472X",
            "",
            "",
            "")]
        [InlineData(
            @"009d1cd696d5194a     no permissions",
            "009d1cd696d5194a",
            "",
            "",
            "",
            ConnectionState.NoPermissions,
            "",
            "",
            "",
            "")]
        [InlineData(
            @"99000000               device product:if_s200n model:NL_V100KR device:if_s200n",
            "99000000",
            "if_s200n",
            "NL_V100KR",
            "if_s200n",
            ConnectionState.Device,
            "",
            "",
            "",
            "")]
        [InlineData(
            @"R32D102SZAE            device transport_id:6",
            "R32D102SZAE",
            "",
            "",
            "",
            ConnectionState.Device,
            "",
            "",
            "6",
            "")]
        [InlineData(
            @"emulator-5554          device product:sdk_google_phone_x86 model:Android_SDK_built_for_x86 device:generic_x86 transport_id:1",
            "emulator-5554",
            "sdk_google_phone_x86",
            "Android_SDK_built_for_x86",
            "generic_x86",
            ConnectionState.Device,
            "",
            "",
            "1",
            "")]
        [InlineData(
            @"00bc13bcf4bacc62 device product:bullhead model:Nexus_5X device:bullhead transport_id:1",
            "00bc13bcf4bacc62",
            "bullhead",
            "Nexus_5X",
            "bullhead",
            ConnectionState.Device,
            "",
            "",
            "1",
            "")]
        [InlineData(
            @"00bc13bcf4bacc62 connecting",
            "00bc13bcf4bacc62",
            "",
            "",
            "",
            ConnectionState.Unknown,
            "",
            "",
            "",
            "")]
        public void Parse_ParsesDeviceData(string data, string serial, string product, string model, string name, ConnectionState state, string usb, string features, string transportId, string message)
        {
            var device = DeviceData.Parse(data);
            Assert.Equal(serial, device.Serial);
            Assert.Equal(product, device.Product);
            Assert.Equal(model, device.Model);
            Assert.Equal(name, device.Name);
            Assert.Equal<ConnectionState>(state, device.State);
            Assert.Equal(usb, device.Usb);
            Assert.Equal(features, device.Features);
            Assert.Equal(transportId, device.TransportId);
            Assert.Equal(message, device.Message);
        }

        /// <summary>
        /// The <see cref="DeviceData.ParseState(string)"/> parses the <c>ADB</c> device state.
        /// </summary>
        /// <param name="data">
        /// The state data.
        /// </param>
        /// <param name="state">
        /// The expected <see cref="ConnectionState"/>.
        /// </param>
        [Theory]
        [InlineData("no permissions", ConnectionState.NoPermissions)]
        [InlineData("device", ConnectionState.Device)]
        [InlineData("no hello", ConnectionState.Unknown)]

        public void ParseState_ParsesConnectionStateData(string data, ConnectionState state)
        {
            Assert.Equal(state, DeviceData.ParseState(data));
        }
    }
}
