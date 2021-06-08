// <copyright file="DeveloperProfileControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using Kaponata.Kubernetes.DeveloperProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="DeveloperProfileController"/> class.
    /// </summary>
    public class DeveloperProfileControllerTests
    {
        /// <summary>
        /// The <see cref="DeveloperProfileController"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeveloperProfileController(null, NullLogger<DeveloperProfileController>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DeveloperProfileController(Mock.Of<KubernetesDeveloperProfile>(), null));
        }

        /// <summary>
        /// <see cref="DeveloperProfileController.ImportDeveloperProfileAsync(IFormFile, string, CancellationToken)"/> correctly imports the data.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Import_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.AddCertificateAsync(It.IsAny<X509Certificate2>(), default))
                .Callback<X509Certificate2, CancellationToken>((cert, ct) =>
                {
                    Assert.Equal("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", cert.Thumbprint);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            profile
                .Setup(p => p.AddProvisioningProfileAsync(It.IsAny<SignedCms>(), default))
                .Callback<SignedCms, CancellationToken>((profile, ct) =>
                {
                    Assert.Equal("2ACCA1611444CAB838B02A433FB31BE0B3120CB0", profile.SignerInfos[0].Certificate.Thumbprint);
                })
                .Returns(Task.FromResult(new ProvisioningProfile()))
                .Verifiable();

            var controller = new DeveloperProfileController(profile.Object, NullLogger<DeveloperProfileController>.Instance);

            using (Stream stream = File.OpenRead("developer.zip"))
            {
                var file = new FormFile(stream, 0, stream.Length, "developer.zip", "developer.zip");
                await controller.ImportDeveloperProfileAsync(file, string.Empty, default).ConfigureAwait(false);

                profile.Verify();
            }
        }

        /// <summary>
        /// <see cref="DeveloperProfileController.GetDeveloperProfileAsync(CancellationToken)"/> returns 404 NOT FOUND
        /// if no provisioning profiles and no developer certificates are present.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDeveloperProfileAsync_Empty_ReturnsNotFound_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile.Setup(p => p.GetCertificatesAsync(default)).ReturnsAsync(Array.Empty<X509Certificate2>());
            profile.Setup(p => p.GetProvisioningProfilesAsync(default)).ReturnsAsync(Array.Empty<SignedCms>());

            var controller = new DeveloperProfileController(profile.Object, NullLogger<DeveloperProfileController>.Instance);

            var result = Assert.IsType<NotFoundResult>(await controller.GetDeveloperProfileAsync(default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="DeveloperProfileController.GetDeveloperProfileAsync(CancellationToken)"/> returns a valid developer profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDeveloperProfileAsync_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            var controller = new DeveloperProfileController(profile.Object, NullLogger<DeveloperProfileController>.Instance);
            profile
                .Setup(p => p.GetCertificatesAsync(default))
                .ReturnsAsync(
                    new X509Certificate2[]
                    {
                        new X509Certificate2(File.ReadAllBytes("E7P4EE896K.cer")),
                    });

            var signedCms = new SignedCms();
            signedCms.Decode(File.ReadAllBytes("test.mobileprovision"));

            profile
                .Setup(p => p.GetProvisioningProfilesAsync(default))
                .ReturnsAsync(new SignedCms[] { signedCms });

            var result = Assert.IsType<FileStreamResult>(await controller.GetDeveloperProfileAsync(default).ConfigureAwait(false));
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal("kaponata.developerprofile", result.FileDownloadName);
            Assert.NotNull(result.FileStream);
            Assert.NotEqual(0, result.FileStream.Length);
        }
    }
}
