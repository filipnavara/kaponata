// <copyright file="NamespacedKubernetesClientExtensionsTests.MobileDevice.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Kubernetes.Tests
{
    /// <summary>
    /// Tests the <see cref="NamespacedKubernetesClientExtensions"/> class.
    /// </summary>
    public class NamespacedKubernetesClientExtensionsTests
    {
        /// <summary>
        /// <see cref="NamespacedKubernetesClientExtensions.SetDeviceConditionAsync(NamespacedKubernetesClient{MobileDevice}, MobileDevice, string, ConditionStatus, string, string, CancellationToken)"/>
        /// validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetDeviceConditionAsync_ValidatesArguments_Async()
        {
            var client = Mock.Of<NamespacedKubernetesClient<MobileDevice>>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SetDeviceConditionAsync(null, "type", ConditionStatus.False, "reason", "message", default)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SetDeviceConditionAsync(new MobileDevice(), null, ConditionStatus.False, "reason", "message", default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClientExtensions.SetDeviceConditionAsync(NamespacedKubernetesClient{MobileDevice}, MobileDevice, string, ConditionStatus, string, string, CancellationToken)"/>
        /// adds the status property if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetDeviceConditionAsync_AddsStatusIfRequired_Async()
        {
            var clientMock = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            var client = clientMock.Object;

            var device = new MobileDevice();
            JsonPatchDocument<MobileDevice> patch = null;

            clientMock
                .Setup(c => c.PatchStatusAsync(device, It.IsAny<JsonPatchDocument<MobileDevice>>(), default))
                .Callback<MobileDevice, JsonPatchDocument<MobileDevice>, CancellationToken>((d, p, ct) => { patch = p; })
                .Returns(Task.FromResult(device)).Verifiable();

            await client.SetDeviceConditionAsync(device, MobileDeviceConditions.Paired, ConditionStatus.True, "reason", "message", default).ConfigureAwait(false);

            clientMock.Verify();

            Assert.NotNull(patch);
            var patchOperation = Assert.Single(patch.Operations);
            Assert.Equal(OperationType.Add, patchOperation.OperationType);
            Assert.Equal("/status", patchOperation.path);

            var condition = Assert.Single(device.Status.Conditions);
            Assert.Equal(MobileDeviceConditions.Paired, condition.Type);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClientExtensions.SetDeviceConditionAsync(NamespacedKubernetesClient{MobileDevice}, MobileDevice, string, ConditionStatus, string, string, CancellationToken)"/>
        /// adds the device condition if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetDeviceConditionAsync_AddsConditionIfRequired_Async()
        {
            var clientMock = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            var client = clientMock.Object;

            var device = new MobileDevice()
            {
                Status = new MobileDeviceStatus(),
            };

            JsonPatchDocument<MobileDevice> patch = null;

            clientMock
                .Setup(c => c.PatchStatusAsync(device, It.IsAny<JsonPatchDocument<MobileDevice>>(), default))
                .Callback<MobileDevice, JsonPatchDocument<MobileDevice>, CancellationToken>((d, p, ct) => { patch = p; })
                .Returns(Task.FromResult(device)).Verifiable();

            await client.SetDeviceConditionAsync(device, MobileDeviceConditions.Paired, ConditionStatus.True, "reason", "message", default).ConfigureAwait(false);

            clientMock.Verify();

            Assert.NotNull(patch);
            var patchOperation = Assert.Single(patch.Operations);
            Assert.Equal(OperationType.Add, patchOperation.OperationType);
            Assert.Equal("/status/conditions", patchOperation.path);

            var condition = Assert.Single(device.Status.Conditions);
            Assert.Equal(MobileDeviceConditions.Paired, condition.Type);
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClientExtensions.SetDeviceConditionAsync(NamespacedKubernetesClient{MobileDevice}, MobileDevice, string, ConditionStatus, string, string, CancellationToken)"/>
        /// updates the device condition if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetDeviceConditionAsync_UpdatesConditionIfRequired_Async()
        {
            var clientMock = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            var client = clientMock.Object;

            var device = new MobileDevice()
            {
                Status = new MobileDeviceStatus()
                {
                    Conditions = new List<MobileDeviceCondition>()
                    {
                        new MobileDeviceCondition()
                        {
                            Type = MobileDeviceConditions.DeveloperDiskMounted,
                            Status = ConditionStatus.False,
                        },
                    },
                },
            };

            JsonPatchDocument<MobileDevice> patch = null;

            clientMock
                .Setup(c => c.PatchStatusAsync(device, It.IsAny<JsonPatchDocument<MobileDevice>>(), default))
                .Callback<MobileDevice, JsonPatchDocument<MobileDevice>, CancellationToken>((d, p, ct) => { patch = p; })
                .Returns(Task.FromResult(device)).Verifiable();

            await client.SetDeviceConditionAsync(device, MobileDeviceConditions.Paired, ConditionStatus.True, "reason", "message", default).ConfigureAwait(false);

            clientMock.Verify();

            Assert.NotNull(patch);
            var patchOperation = Assert.Single(patch.Operations);
            Assert.Equal(OperationType.Replace, patchOperation.OperationType);
            Assert.Equal("/status/conditions", patchOperation.path);

            Assert.Equal(ConditionStatus.False, device.Status.GetConditionStatus(MobileDeviceConditions.DeveloperDiskMounted));
            Assert.Equal(ConditionStatus.True, device.Status.GetConditionStatus(MobileDeviceConditions.Paired));
        }

        /// <summary>
        /// <see cref="NamespacedKubernetesClientExtensions.SetDeviceConditionAsync(NamespacedKubernetesClient{MobileDevice}, MobileDevice, string, ConditionStatus, string, string, CancellationToken)"/>
        /// does not update the device condition if not required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetDeviceConditionAsync_SkipsUpdatedIfNotNeeded_Async()
        {
            var clientMock = new Mock<NamespacedKubernetesClient<MobileDevice>>(MockBehavior.Strict);
            var client = clientMock.Object;

            var device = new MobileDevice()
            {
                Status = new MobileDeviceStatus()
                {
                    Conditions = new List<MobileDeviceCondition>()
                    {
                        new MobileDeviceCondition()
                        {
                            Type = MobileDeviceConditions.Paired,
                            Reason = "reason",
                            Message = "message",
                            Status = ConditionStatus.True,
                            LastHeartbeatTime = DateTimeOffset.Now,
                        },
                    },
                },
            };

            await client.SetDeviceConditionAsync(device, MobileDeviceConditions.Paired, ConditionStatus.True, "reason", "message", default).ConfigureAwait(false);
            clientMock.Verify();
        }
    }
}
