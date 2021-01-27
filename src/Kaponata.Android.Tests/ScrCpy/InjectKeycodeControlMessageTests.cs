// <copyright file="InjectKeycodeControlMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using Kaponata.Android.ScrCpy;
using System;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="InjectKeycodeControlMessage"/> class.
    /// </summary>
    public class InjectKeycodeControlMessageTests
    {
        /// <summary>
        /// The <see cref="InjectKeycodeControlMessage.Write(System.Memory{byte})"/> methods writes the message into the buffer.
        /// </summary>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/tests/test_control_msg_serialize.c#L6"/>
        [Fact]
        public void Write_WritesMessage()
        {
            var message = new InjectKeycodeControlMessage()
            {
                Action = KeyEventAction.UP,
                KeyCode = KeyCode.ENTER,
                Metastate = Metastate.SHIFT_ON | Metastate.SHIFT_LEFT_ON,
            };

            var size = message.BinarySize;

            Assert.Equal(10, size);

            var memory = new Memory<byte>(new byte[10]);
            message.Write(memory);

            var expected = new byte[]
            {
                (byte)ControlMessageType.INJECT_KEYCODE,
                0x01, // AKEY_EVENT_ACTION_UP
                0x00, 0x00, 0x00, 0x42, // AKEYCODE_ENTER
                0x00, 0x00, 0x00, 0x41, // AMETA_SHIFT_ON | AMETA_SHIFT_LEFT_ON
            };

            Assert.Equal(expected, memory.Span.ToArray());
        }
    }
}
