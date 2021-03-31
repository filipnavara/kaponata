// <copyright file="StartSessionResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="StartSessionResponse"/> class.
    /// </summary>
    public class StartSessionResponseTests
    {
        /// <summary>
        /// <see cref="StartSessionResponse.Read(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => StartSessionResponse.Read(null));
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.Read(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("SessionID", "abc");

            var response = StartSessionResponse.Read(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.False(response.EnableSessionSSL);
            Assert.Equal("abc", response.SessionID);
            Assert.Null(response.Error);
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.Read(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void Read_WithSSL_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("EnableSessionSSL", true);
            dict.Add("SessionID", "abc");

            var response = StartSessionResponse.Read(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.True(response.EnableSessionSSL);
            Assert.Equal("abc", response.SessionID);
            Assert.Null(response.Error);
        }

        /// <summary>
        /// Tests the <see cref="StartSessionResponse.Read(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void Read_WithError_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "StartSession");
            dict.Add("Error", "abc");

            var response = StartSessionResponse.Read(dict);

            Assert.Equal("StartSession", response.Request);
            Assert.Equal("abc", response.Error);
        }
    }
}
