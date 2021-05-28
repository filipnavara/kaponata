// <copyright file="MobileDeviceStatusTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kaponata.Kubernetes.Tests.Models
{
    /// <summary>
    /// Tests the <see cref="MobileDeviceStatus"/> class.
    /// </summary>
    public class MobileDeviceStatusTests
    {
        /// <summary>
        /// <see cref="MobileDeviceStatus.SetCondition(string, ConditionStatus, string, string)"/> initializes the <see cref="MobileDeviceStatus.Conditions"/>
        /// properties that property is <see langword="null"/>.
        /// </summary>
        [Fact]
        public void SetCondition_ConditionsNull_AddsCondition()
        {
            var status = new MobileDeviceStatus();
            Assert.True(status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.True, "Reason", "Message"));
            Assert.NotNull(status.Conditions);
            var condition = Assert.Single(status.Conditions);
            Assert.Equal(MobileDeviceConditions.DeveloperDiskMounted, condition.Type);
            Assert.Equal(ConditionStatus.True, condition.Status);
            Assert.Equal("Reason", condition.Reason);
            Assert.Equal("Message", condition.Message);
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.SetCondition(string, ConditionStatus, string, string)"/> updates an existing condition
        /// if the status has changed.
        /// </summary>
        [Fact]
        public void SetCondition_UpdatesCondition()
        {
            var status = new MobileDeviceStatus()
            {
                Conditions = new List<MobileDeviceCondition>()
                {
                    new MobileDeviceCondition()
                    {
                         Type = MobileDeviceConditions.DeveloperDiskMounted,
                         Status = ConditionStatus.False,
                    },
                },
            };

            Assert.True(status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.True, "Reason", "Message"));

            var condition = Assert.Single(status.Conditions);
            Assert.Equal(MobileDeviceConditions.DeveloperDiskMounted, condition.Type);
            Assert.Equal(ConditionStatus.True, condition.Status);
            Assert.Equal("Reason", condition.Reason);
            Assert.Equal("Message", condition.Message);
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.SetCondition(string, ConditionStatus, string, string)"/> updates the timestamp if an existing
        /// condition is renewed with the same status.
        /// </summary>
        [Fact]
        public void SetCondition_UpdatesTimestamp()
        {
            var status = new MobileDeviceStatus()
            {
                Conditions = new List<MobileDeviceCondition>()
                {
                    new MobileDeviceCondition()
                    {
                         Type = MobileDeviceConditions.DeveloperDiskMounted,
                         Status = ConditionStatus.True,
                         Reason = "Reason",
                         Message = "Message",
                         LastHeartbeatTime = DateTimeOffset.Now.AddDays(-1),
                    },
                },
            };

            Assert.True(status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.True, "Reason", "Message"));

            var condition = Assert.Single(status.Conditions);
            Assert.Equal(MobileDeviceConditions.DeveloperDiskMounted, condition.Type);
            Assert.Equal(ConditionStatus.True, condition.Status);
            Assert.Equal("Reason", condition.Reason);
            Assert.Equal("Message", condition.Message);
            Assert.True(condition.LastHeartbeatTime > DateTimeOffset.Now.AddMinutes(-5));
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.SetCondition(string, ConditionStatus, string, string)"/> does nothing if the status
        /// did not change and the heartbeat is recent.
        /// </summary>
        [Fact]
        public void SetCondition_RecentTimestamp_DoesNothing()
        {
            var status = new MobileDeviceStatus()
            {
                Conditions = new List<MobileDeviceCondition>()
                {
                    new MobileDeviceCondition()
                    {
                         Type = MobileDeviceConditions.DeveloperDiskMounted,
                         Status = ConditionStatus.True,
                         Reason = "Reason",
                         Message = "Message",
                         LastHeartbeatTime = DateTimeOffset.Now,
                    },
                },
            };

            Assert.False(status.SetCondition(MobileDeviceConditions.DeveloperDiskMounted, ConditionStatus.True, "Reason", "Message"));

            var condition = Assert.Single(status.Conditions);
            Assert.Equal(MobileDeviceConditions.DeveloperDiskMounted, condition.Type);
            Assert.Equal(ConditionStatus.True, condition.Status);
            Assert.Equal("Reason", condition.Reason);
            Assert.Equal("Message", condition.Message);
            Assert.True(condition.LastHeartbeatTime > DateTimeOffset.Now.AddMinutes(-5));
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.GetConditionStatus(string)"/> returns <see cref="ConditionStatus.Unknown"/>
        /// when the <see cref="MobileDeviceStatus.Conditions"/> is <see langword="null"/>.
        /// </summary>
        [Fact]
        public void GetCondition_ConditionsNull_ReturnsUnknown()
        {
            var status = new MobileDeviceStatus();
            Assert.Equal(ConditionStatus.Unknown, status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.GetConditionStatus(string)"/> returns <see cref="ConditionStatus.Unknown"/>
        /// when the <see cref="MobileDeviceStatus.Conditions"/> is not <see langword="null"/> but does not contain
        /// the requested condition.
        /// </summary>
        [Fact]
        public void GetCondition_MissingCondition_ReturnsUnknown()
        {
            var status = new MobileDeviceStatus() { Conditions = new List<MobileDeviceCondition>() };
            Assert.Equal(ConditionStatus.Unknown, status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));
        }

        /// <summary>
        /// <see cref="MobileDeviceStatus.GetConditionStatus(string)"/> returns the condition status
        /// when the <see cref="MobileDeviceStatus.Conditions"/> contains the requested condition.
        /// </summary>
        [Fact]
        public void GetCondition_HasCondition_ReturnsConditionValue()
        {
            var status = new MobileDeviceStatus()
            {
                Conditions = new List<MobileDeviceCondition>()
                {
                    new MobileDeviceCondition()
                    {
                        Type = MobileDeviceConditions.DeveloperDiskMounted,
                        Status = ConditionStatus.True,
                    },
                },
            };

            Assert.Equal(ConditionStatus.True, status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));
        }
    }
}
