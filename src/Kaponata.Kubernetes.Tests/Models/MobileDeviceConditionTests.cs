// <copyright file="MobileDeviceConditionTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Models;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Models
{
    /// <summary>
    /// Tests the <see cref="MobileDeviceCondition"/> class.
    /// </summary>
    public class MobileDeviceConditionTests
    {
        /// <summary>
        /// The <see cref="MobileDeviceCondition.ToString"/> method works correctly.
        /// </summary>
        [Fact]
        public void ToString_Works()
        {
            Assert.Equal(
                "Paired: True (Reason)",
                new MobileDeviceCondition()
                {
                    Type = MobileDeviceConditions.Paired,
                    Status = ConditionStatus.True,
                    Reason = "Reason",
                }.ToString());
        }
    }
}
