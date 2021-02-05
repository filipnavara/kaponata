// <copyright file="ForwardSpecTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Android.Adb;
using System;
using Xunit;

namespace Kaponata.Android.Tests.Adb
{
    /// <summary>
    /// Tests the <see cref="ForwardSpec"/> class.
    /// </summary>
    public class ForwardSpecTests
    {
        /// <summary>
        /// The <see cref="ForwardSpec.Parse(string)"/> method parses the specification.
        /// </summary>
        /// <param name="input">
        /// The forward specification.
        /// </param>
        /// <param name="protocol">
        /// The expected protocol.
        /// </param>
        /// <param name="port">
        /// The expected port.
        /// </param>
        /// <param name="socketName">
        /// The expected socket name.
        /// </param>
        /// <param name="processId">
        /// the expected process id.
        /// </param>
        [Theory]
        [InlineData("jdwp:1234", ForwardProtocol.JavaDebugWireProtocol, 0, null, 1234)]
        [InlineData("localabstract:/tmp/1234", ForwardProtocol.LocalAbstract, 0, "/tmp/1234", 0)]
        [InlineData("tcp:1234", ForwardProtocol.Tcp, 1234, null, 0)]
        public void Parse_ParsesSpecification(string input, ForwardProtocol protocol, int port, string socketName, int processId)
        {
            var value = ForwardSpec.Parse(input);

            Assert.NotNull(value);
            Assert.Equal(protocol, value.Protocol);
            Assert.Equal(port, value.Port);
            Assert.Equal(processId, value.ProcessId);
            Assert.Equal(socketName, value.SocketName);

            Assert.Equal(input, value.ToString());
        }

        /// <summary>
        /// The <see cref="ForwardSpec.Parse(string)"/> method throws an excpetion when null is passed.
        /// </summary>
        [Fact]
        public void Parse_Null()
        {
            Assert.Throws<ArgumentNullException>(() => ForwardSpec.Parse(null));
        }

        /// <summary>
        /// The <see cref="ForwardSpec.Parse(string)"/> method throws an exception when the input is invalid.
        /// </summary>
        /// <param name="spec">
        /// The invalid specification.
        /// </param>
        [Theory]
        [InlineData("xyz:1234")]
        [InlineData("jdwp:abc")]
        [InlineData("tcp:xyz")]
        [InlineData("abc")]
        public void Parse_InvalidSpec(string spec)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ForwardSpec.Parse(spec));
        }

        /// <summary>
        /// THe <see cref="ForwardSpec.GetHashCode"/> method is able to identify differnces in specifications.
        /// </summary>
        /// <param name="spec1">
        /// The first forwards specification.
        /// </param>
        /// <param name="spec2">
        /// The second forwards specification.
        /// </param>
        [Theory]
        [InlineData("tcp:1234", "tcp:4321")]
        [InlineData("jdwp:1234", "jdwp:4321")]
        [InlineData("localabstract:/tmp/4321", "localabstract:/tmp/1234")]
        public void GetHashCode_CalculatesHashCode(string spec1, string spec2)
        {
            var forwardSpec1 = ForwardSpec.Parse(spec1);
            var forwardSpec1bis = ForwardSpec.Parse(spec1);
            var forwardSpec2 = ForwardSpec.Parse(spec2);

            Assert.Equal(forwardSpec1.GetHashCode(), forwardSpec1bis.GetHashCode());
            Assert.NotEqual(forwardSpec1.GetHashCode(), forwardSpec2.GetHashCode());
        }

        /// <summary>
        /// THe <see cref="ForwardSpec.Equals(object)"/> method compares two <see cref="ForwardSpec"/> instances.
        /// </summary>
        /// <param name="spec1">
        /// The first forwards specification.
        /// </param>
        /// <param name="spec2">
        /// The second forwards specification.
        /// </param>
        [Theory]
        [InlineData("tcp:1234", "tcp:4321")]
        [InlineData("jdwp:1234", "jdwp:4321")]
        [InlineData("localabstract:/tmp/4321", "localabstract:/tmp/1234")]
        [InlineData("tcp:1234", "jdwp:4321")]
        [InlineData("tcp:1234", "localabstract:/tmp/1234")]
        [InlineData("jdwp:1234", "localabstract:/tmp/1234")]
        public void Equals_ComparesForwardSpecifications(string spec1, string spec2)
        {
            var forwardSpec1 = ForwardSpec.Parse(spec1);
            var forwardSpec1bis = ForwardSpec.Parse(spec1);
            var forwardSpec2 = ForwardSpec.Parse(spec2);

            Assert.Equal(forwardSpec1, forwardSpec1bis);
            Assert.NotEqual(forwardSpec1, forwardSpec2);

            var dummy = new ForwardSpec()
            {
                Protocol = (ForwardProtocol)99,
            };
            Assert.NotEqual(dummy, forwardSpec1);
            Assert.NotEqual(dummy, forwardSpec2);

            Assert.False(forwardSpec1.Equals(null));
            Assert.False(forwardSpec2.Equals(null));
        }
    }
}
