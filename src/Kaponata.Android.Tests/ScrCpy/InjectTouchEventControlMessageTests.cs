// <copyright file="InjectTouchEventControlMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using Kaponata.Android.ScrCpy;
using System;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="InjectTouchEventControlMessage"/> class.
    /// </summary>
    public class InjectTouchEventControlMessageTests
    {
        /// <summary>
        /// The <see cref="InjectTouchEventControlMessage.Write(System.Memory{byte})"/> methods writes the message into the buffer.
        /// </summary>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/tests/test_control_msg_serialize.c"/>
        [Fact]
        public void Write_WritesMessage()
        {
            var message = new InjectTouchEventControlMessage()
            {
                Action = MotionEventAction.DOWN,
                PointerId = 0x1234567887654321L,
                X = 100,
                Y = 200,
                Width = 1080,
                Height = 1920,
                Pressure = 1,
                Buttons = MotionEventButtons.PRIMARY,
            };

            var size = message.BinarySize;

            Assert.Equal(28, size);

            var memory = new Memory<byte>(new byte[size]);
            message.Write(memory);

            var expected = new byte[]
            {
                (byte)ControlMessageType.INJECT_TOUCH_EVENT,
                0x00, // AKEY_EVENT_ACTION_DOWN
                0x12, 0x34, 0x56, 0x78, 0x87, 0x65, 0x43, 0x21, // pointer id
                0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0xc8, // 100 200
                0x04, 0x38, 0x07, 0x80, // 1080 1920
                0xff, 0xff, // pressure
                0x00, 0x00, 0x00, 0x01, // AMOTION_EVENT_BUTTON_PRIMARY
            };

            Assert.Equal(expected, memory.ToArray());
        }

        /// <summary>
        /// The <see cref="InjectTouchEventControlMessage.ToFixedPoint16(float)"/> method converts the given value.
        /// </summary>
        /// <param name="value">
        /// The value to be converted.
        /// </param>
        /// <param name="result">
        /// The expected result.
        /// </param>
        [Theory]
        [InlineData(0.1, 6553)]
        [InlineData(0.0, 0)]
        [InlineData(0.85, 55705)]
        [InlineData(1, 65535)]
        public void ToFixedPoint16_ConvertsValue(float value, ushort result)
        {
            var message = new InjectTouchEventControlMessage();
            Assert.Equal(result, message.ToFixedPoint16(value));
        }

        /// <summary>
        /// The <see cref="InjectTouchEventControlMessage.ToFixedPoint16(float)"/> validates the input value.
        /// </summary>
        /// <param name="value">
        /// The value to be converted.
        /// </param>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void ToFixedPoint16_ValidatesInput(float value)
        {
            var message = new InjectTouchEventControlMessage();
            Assert.Throws<ArgumentOutOfRangeException>(() => message.ToFixedPoint16(value));
        }
    }
}
