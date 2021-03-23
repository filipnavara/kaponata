// <copyright file="ProvisioningProfileControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="ProvisioningProfileController"/> class.
    /// </summary>
    public class ProvisioningProfileControllerTests
    {
        /// <summary>
        /// The <see cref="ProvisioningProfile"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ProvisioningProfileController(null, NullLogger<ProvisioningProfileController>.Instance));
            Assert.Throws<ArgumentNullException>(() => new ProvisioningProfileController(Mock.Of<KubernetesDeveloperProfile>(), null));
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.AddProvisioningProfileAsync(CancellationToken)"/> adds the provisioning profile
        /// to the Kubernetes cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddProvisioningProfileAsync_NoContentLength_BadRequest_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.Request.Body = File.OpenRead("test.mobileprovision");
                controller.Request.ContentLength = null;

                var result = Assert.IsType<BadRequestResult>(await controller.AddProvisioningProfileAsync(default).ConfigureAwait(false));
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.AddProvisioningProfileAsync(CancellationToken)"/> adds the provisioning profile
        /// to the Kubernetes cluster.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddProvisioningProfileAsync_Works_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var provisioningProfile = new ProvisioningProfile();
            developerProfile
                .Setup(p => p.AddProvisioningProfileAsync(It.IsAny<SignedCms>(), default))
                .ReturnsAsync(provisioningProfile)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
                controller.Request.Body = File.OpenRead("test.mobileprovision");
                controller.Request.ContentLength = controller.Request.Body.Length;

                var result = Assert.IsType<OkObjectResult>(await controller.AddProvisioningProfileAsync(default).ConfigureAwait(false));
                Assert.Equal(provisioningProfile, result.Value);
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.GetProvisioningProfilesAsync(CancellationToken)"/> lists the available
        /// provisioning profiles.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfilesAsync_Works_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);

            var provisioningProfile = new SignedCms();
            provisioningProfile.Decode(File.ReadAllBytes("test.mobileprovision"));
            var provisioningProfiles = new List<SignedCms>()
            {
                provisioningProfile,
            };

            developerProfile
                .Setup(p => p.GetProvisioningProfilesAsync(default))
                .ReturnsAsync(provisioningProfiles)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                var result = Assert.IsType<OkObjectResult>(await controller.GetProvisioningProfilesAsync(default).ConfigureAwait(false));
                var profiles = Assert.IsType<List<ProvisioningProfile>>(result.Value);
                var value = Assert.Single(profiles);
                Assert.Equal(new Guid("98264c6b-5151-4349-8d0f-66691e48ae35"), value.Uuid);
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.GetProvisioningProfileAsync(Guid, CancellationToken)"/> returns
        /// a <see cref="NotFoundResult"/> when the provisioning profile could not be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfile_NotFound_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var uuid = Guid.NewGuid();

            developerProfile
                .Setup(p => p.GetProvisioningProfileAsync(uuid, default))
                .ReturnsAsync((SignedCms)null)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                var result = Assert.IsType<NotFoundResult>(await controller.GetProvisioningProfileAsync(uuid, default).ConfigureAwait(false));
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.GetProvisioningProfileAsync(Guid, CancellationToken)"/> returns
        /// the requested provisioning profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetProvisioningProfile_Found_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var uuid = Guid.NewGuid();

            var provisioningProfile = new SignedCms();
            provisioningProfile.Decode(File.ReadAllBytes("test.mobileprovision"));

            developerProfile
                .Setup(p => p.GetProvisioningProfileAsync(uuid, default))
                .ReturnsAsync(provisioningProfile)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                var result = Assert.IsType<FileContentResult>(await controller.GetProvisioningProfileAsync(uuid, default).ConfigureAwait(false));
                Assert.Equal(File.ReadAllBytes("test.mobileprovision"), result.FileContents);
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.DeleteProvisioningProfileAsync(Guid, CancellationToken)"/> returns a <see cref="NotFoundResult"/>
        /// when the provisioning profile to delete could not be found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteProvisioningProfile_NotFound_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var uuid = Guid.NewGuid();

            developerProfile
                .Setup(p => p.DeleteProvisioningProfileAsync(uuid, default))
                .ReturnsAsync(false)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                var result = Assert.IsType<NotFoundResult>(await controller.DeleteProvisioningProfileAsync(uuid, default).ConfigureAwait(false));
            }

            developerProfile.Verify();
        }

        /// <summary>
        /// <see cref="ProvisioningProfileController.DeleteProvisioningProfileAsync(Guid, CancellationToken)"/> returns a <see cref="OkResult"/>
        /// when the provisioning profile was deleted.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteProvisioningProfile_Found_Async()
        {
            var developerProfile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var uuid = Guid.NewGuid();

            developerProfile
                .Setup(p => p.DeleteProvisioningProfileAsync(uuid, default))
                .ReturnsAsync(true)
                .Verifiable();

            using (var controller = new ProvisioningProfileController(developerProfile.Object, NullLogger<ProvisioningProfileController>.Instance))
            {
                var result = Assert.IsType<OkResult>(await controller.DeleteProvisioningProfileAsync(uuid, default).ConfigureAwait(false));
            }

            developerProfile.Verify();
        }
    }
}
