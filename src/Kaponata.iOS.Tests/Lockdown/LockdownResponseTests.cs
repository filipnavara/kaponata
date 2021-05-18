﻿// <copyright file="LockdownResponseTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using System;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownResponse{T}"/> class.
    /// </summary>
    public class LockdownResponseTests
    {
        /// <summary>
        /// <see cref="LockdownResponse{T}.FromDictionary(NSDictionary)"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Read_ValidatesArguments()
        {
            var response = new LockdownResponse<string>();
            Assert.Throws<ArgumentNullException>(() => response.FromDictionary(null));
        }

        /// <summary>
        /// Tests the <see cref="LockdownResponse{T}.FromDictionary(NSDictionary)"/> method.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "QueryType");
            dict.Add("Result", "Success");
            dict.Add("Type", "com.apple.mobile.lockdown");

            var response = new LockdownResponse<string>();
            response.FromDictionary(dict);

            Assert.Equal("QueryType", response.Request);
            Assert.Equal("Success", response.Result);
            Assert.Equal("com.apple.mobile.lockdown", response.Type);
        }
    }
}
