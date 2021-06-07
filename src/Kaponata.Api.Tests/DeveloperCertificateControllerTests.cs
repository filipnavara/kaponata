// <copyright file="DeveloperCertificateControllerTests.cs" company="Quamotion bv">
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
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="DeveloperCertificateController"/> class.
    /// </summary>
    public class DeveloperCertificateControllerTests
    {
        /// <summary>
        /// The <see cref="DeveloperCertificateController"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeveloperCertificateController(null, NullLogger<DeveloperCertificateController>.Instance));
            Assert.Throws<ArgumentNullException>(() => new DeveloperCertificateController(Mock.Of<KubernetesDeveloperProfile>(), null));
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.AddCertificateAsync(IFormFile, string, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task AddCertificateAsync_ValidatesArguments_Async()
        {
            var controller = new DeveloperCertificateController(Mock.Of<KubernetesDeveloperProfile>(), NullLogger<DeveloperCertificateController>.Instance);
            Assert.IsType<BadRequestObjectResult>(await controller.AddCertificateAsync(null, "test", default).ConfigureAwait(false));
            Assert.IsType<BadRequestObjectResult>(await controller.AddCertificateAsync(new FormFile(Stream.Null, 0, 0, "name", "test"), null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.AddCertificateAsync(IFormFile, string, CancellationToken)"/> correctly adds the certificate to the
        /// developer profile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddCertificateAsync_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile.Setup(p => p.AddCertificateAsync(It.IsAny<X509Certificate2>(), default)).Returns(Task.CompletedTask).Verifiable();

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            using (Stream certificateStream = File.OpenRead("E7P4EE896K.cer"))
            {
                FormFile certificateFile = new FormFile(certificateStream, 0, certificateStream.Length, "E7P4EE896K.cer", "E7P4EE896K.cer");
                Assert.IsType<OkResult>(await controller.AddCertificateAsync(certificateFile, string.Empty, default).ConfigureAwait(false));
            }

            profile.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.GetCertificateAsync(string, CancellationToken)"/> correctly lists the available
        /// certificates.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificatesAsync_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.GetCertificatesAsync(default))
                .ReturnsAsync(new List<X509Certificate2>()
                {
                    new X509Certificate2(File.ReadAllBytes("E7P4EE896K.cer"), string.Empty),
                })
                .Verifiable();

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            var result = Assert.IsType<OkObjectResult>(await controller.GetCertificatesAsync(default));
            var values = Assert.IsType<List<Identity>>(result.Value);

            var identity = Assert.Single(values);
            Assert.Equal("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", identity.Thumbprint);
            profile.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.GetCertificateAsync(string, CancellationToken)"/> returns a 404 NOT FOUND
        /// result when the requested certificate does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificateAsync_NotFound_ReturnsNotFound_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.GetCertificateAsync("invalid", default))
                .ReturnsAsync((X509Certificate2)null)
                .Verifiable();

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            var result = Assert.IsType<NotFoundResult>(await controller.GetCertificateAsync("invalid", default));

            profile.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.GetCertificateAsync(string, CancellationToken)"/> returns the requested certificate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCertificateAsync_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.GetCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default))
                .ReturnsAsync(new X509Certificate2(File.ReadAllBytes("E7P4EE896K.cer"), string.Empty))
                .Verifiable();

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            var result = Assert.IsType<FileContentResult>(await controller.GetCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default));
            Assert.Equal("application/x-x509-user-cert", result.ContentType);
            Assert.Equal("EF4751CA452094E26A79D6F8BFDC08413CE6C90D.cer", result.FileDownloadName);

            var cert = new X509Certificate2(result.FileContents);
            Assert.Equal("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", cert.Thumbprint);

            profile.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.DeleteCertificateAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCertificateAsync_NotFound_ReturnsError_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.DeleteCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default))
                .ThrowsAsync(new InvalidOperationException());

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            Assert.IsType<NotFoundResult>(await controller.DeleteCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default));

            profile.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperCertificateController.DeleteCertificateAsync(string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteCertificateAsync_Works_Async()
        {
            var profile = new Mock<KubernetesDeveloperProfile>(MockBehavior.Strict);
            profile
                .Setup(p => p.DeleteCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default))
                .Returns(Task.CompletedTask);

            var controller = new DeveloperCertificateController(profile.Object, NullLogger<DeveloperCertificateController>.Instance);

            Assert.IsType<OkResult>(await controller.DeleteCertificateAsync("EF4751CA452094E26A79D6F8BFDC08413CE6C90D", default));

            profile.Verify();
        }
    }
}
