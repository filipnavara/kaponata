// <copyright file="ScrCpyProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using Kaponata.Android.ScrCpy;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="ScrCpyProtocol"/> class.
    /// </summary>
    public class ScrCpyProtocolTests
    {
        /// <summary>
        /// The <see cref="ScrCpyProtocol.SendControlMessageAsync(IControlMessage, System.Threading.CancellationToken)"/> writes the controlmessage to the underlying stream.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task SendControlMessage_WritesControlMessage_Async()
        {
            using (var memoryStream = new MemoryStream())
            await using (var protocol = new ScrCpyProtocol(memoryStream, false, NullLogger<ScrCpyProtocol>.Instance))
            {
                var message = new InjectKeycodeControlMessage()
                {
                    Action = KeyEventAction.UP,
                    KeyCode = KeyCode.ENTER,
                    Metastate = Metastate.SHIFT_ON | Metastate.SHIFT_LEFT_ON,
                };

                await protocol.SendControlMessageAsync(message, CancellationToken.None).ConfigureAwait(false);

                var expected = new byte[]
                {
                                    (byte)ControlMessageType.INJECT_KEYCODE,
                                    0x01, // AKEY_EVENT_ACTION_UP
                                    0x00, 0x00, 0x00, 0x42, // AKEYCODE_ENTER
                                    0x00, 0x00, 0x00, 0x41, // AMETA_SHIFT_ON | AMETA_SHIFT_LEFT_ON
                };

                var receivedMessage = new byte[expected.Length];
                memoryStream.Position = 0;
                Assert.NotEqual(0, await memoryStream.ReadAsync(receivedMessage).ConfigureAwait(false));
                Assert.Equal(expected, receivedMessage);
            }
        }

        /// <summary>
        /// The <see cref="ScrCpyProtocol.DisposeAsync"/> disposes the underlying stream if the stream is owned.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Dispose_DisposesIfOwned_Async()
        {
            using (var memoryStream = new MemoryStream())
            {
                await using (var protocol = new ScrCpyProtocol(memoryStream, true, NullLogger<ScrCpyProtocol>.Instance))
                {
                }

                Assert.Throws<ObjectDisposedException>(() => memoryStream.Length);
            }
        }

        /// <summary>
        /// The <see cref="ScrCpyProtocol.DisposeAsync"/> does not dispose the underlying stream if the stream is not owned.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task Dispose_NoDisposeIfNotOwned_Async()
        {
            using (var memoryStream = new MemoryStream())
            {
                await using (var protocol = new ScrCpyProtocol(memoryStream, false, NullLogger<ScrCpyProtocol>.Instance))
                {
                }

                Assert.Equal(0, memoryStream.Length);
            }
        }
    }
}
