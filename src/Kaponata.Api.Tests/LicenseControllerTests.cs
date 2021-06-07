// <copyright file="LicenseControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Kubernetes.Licensing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="LicenseController"/> class.
    /// </summary>
    public class LicenseControllerTests
    {
        /// <summary>
        /// The <see cref="LicenseController"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new LicenseController(null, NullLogger<LicenseController>.Instance));
            Assert.Throws<ArgumentNullException>(() => new LicenseController(Mock.Of<LicenseStore>(), null));
        }

        /// <summary>
        /// <see cref="LicenseController.UpdateLicenseAsync(IFormFile, CancellationToken)"/> returns <see cref="BadRequestObjectResult"/>
        /// when no license file is specified.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateLicenseAsync_ValidatesArguments_Async()
        {
            var controller = new LicenseController(Mock.Of<LicenseStore>(), NullLogger<LicenseController>.Instance);

            Assert.IsType<BadRequestObjectResult>(await controller.UpdateLicenseAsync(null, default).ConfigureAwait(false));
        }

        /// <summary>
        /// <see cref="LicenseController.UpdateLicenseAsync(IFormFile, CancellationToken)"/> correctly adds the license
        /// to the store.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateLicense_Works_Async()
        {
            var store = new Mock<LicenseStore>(MockBehavior.Strict);
            store.Setup(s => s.AddLicenseAsync(It.IsAny<XDocument>(), default)).Returns(Task.CompletedTask).Verifiable();
            var controller = new LicenseController(store.Object, NullLogger<LicenseController>.Instance);

            using (Stream licenseStream = new MemoryStream(Encoding.UTF8.GetBytes("<license></license>")))
            {
                var license = new FormFile(licenseStream, 0, licenseStream.Length, ".license", ".license");

                Assert.IsType<OkResult>(await controller.UpdateLicenseAsync(license, default).ConfigureAwait(false));
            }

            store.Verify();
        }

        /// <summary>
        /// <see cref="LicenseController.GetLicenseAsync(CancellationToken)"/> returns <see cref="NotFoundResult"/>
        /// when no license is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLicense_NoLicense_ReturnsNotFound_Async()
        {
            var store = new Mock<LicenseStore>(MockBehavior.Strict);
            store.Setup(s => s.GetLicenseAsync(default)).ReturnsAsync((XDocument)null).Verifiable();
            var controller = new LicenseController(store.Object, NullLogger<LicenseController>.Instance);

            Assert.IsType<NotFoundResult>(await controller.GetLicenseAsync(default).ConfigureAwait(false));

            store.Verify();
        }

        /// <summary>
        /// <see cref="LicenseController.GetLicenseAsync(CancellationToken)"/> returns the currently available license.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLicense_HasLicenseLicense_ReturnsLicense_Async()
        {
            var store = new Mock<LicenseStore>(MockBehavior.Strict);
            store.Setup(s => s.GetLicenseAsync(default)).ReturnsAsync(XDocument.Parse("<license></license>")).Verifiable();
            var controller = new LicenseController(store.Object, NullLogger<LicenseController>.Instance);

            var result = Assert.IsType<FileContentResult>(await controller.GetLicenseAsync(default).ConfigureAwait(false));
            Assert.Equal("<license></license>", Encoding.UTF8.GetString(result.FileContents));

            store.Verify();
        }
    }
}
