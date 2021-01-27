// <copyright file="InjectTextControlMessageTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Common;
using Kaponata.Android.ScrCpy;
using System;
using System.Text;
using Xunit;

namespace Kaponata.Android.Tests.ScrCpy
{
    /// <summary>
    /// Tests the <see cref="InjectTextControlMessage"/> class.
    /// </summary>
    public class InjectTextControlMessageTests
    {
        /// <summary>
        /// The <see cref="InjectTextControlMessage.Write(System.Memory{byte})"/> methods writes the message into the buffer.
        /// </summary>
        /// <seealso href="https://github.com/Genymobile/scrcpy/blob/master/app/tests/test_control_msg_serialize.c"/>
        [Fact]
        public void Write_WritesMessage()
        {
            var message = new InjectTextControlMessage()
            {
                Text = "hello, world!",
            };

            var size = message.BinarySize;

            Assert.Equal(16, size);

            var memory = new Memory<byte>(new byte[size]);
            message.Write(memory);

            var expected = new byte[size];
            expected[0] = (byte)ControlMessageType.INJECT_TEXT;
            expected[1] = 0x00;
            expected[2] = 0x0d;
            Encoding.UTF8.GetBytes("hello, world!").CopyTo(expected, 3);

            Assert.Equal(expected, memory.ToArray());
        }
    }
}
