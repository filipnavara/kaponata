// <copyright file="DeveloperDiskControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperDisks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="DeveloperDiskController"/> class.
    /// </summary>
    public class DeveloperDiskControllerTests
    {
        /// <summary>
        /// Returns tests data for the <see cref="ImportDeveloperDiskAsync_ValidatesFiles_Async"/> theory.
        /// </summary>
        /// <returns>
        /// Test data for the <see cref="ImportDeveloperDiskAsync_ValidatesFiles_Async"/> theory.
        /// </returns>
        public static IEnumerable<object[]> ImportDeveloperDiskAsync_ValidatesFiles_Data()
        {
            // No files
            yield return new object[]
            {
                new FormFileCollection(),
            };

            // Two developer disk files
            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                },
            };

            // Empty files
            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                },
            };

            // Empty signature
            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                },
            };

            // Signature too large
            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                },
            };

            // Multiple files
            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0x10, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                },
            };

            yield return new object[]
            {
                new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                    new FormFile(Stream.Null, 0, 0x10, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                    new FormFile(Stream.Null, 0, 0x10, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                },
            };
        }

        /// <summary>
        /// The <see cref="DeveloperDiskController"/> constructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new DeveloperDiskController(null, Mock.Of<DeveloperDiskFactory>()));
            Assert.Throws<ArgumentNullException>(() => new DeveloperDiskController(Mock.Of<DeveloperDiskStore>(), null));
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.ImportDeveloperDiskAsync(CancellationToken)"/> returns 415 Unsupported Media Type
        /// when the request does not include a form.
        /// </summary>
        /// <param name="contentType">
        /// The content type of the request.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null)]
        [InlineData("text/json")]
        public async Task ImportDeveloperDiskAsync_ValidatesMediaType_Async(string contentType)
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);

            var controller = new DeveloperDiskController(store.Object, Mock.Of<DeveloperDiskFactory>())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            controller.Request.ContentType = contentType;
            controller.Request.Form = null;

            var result = await controller.ImportDeveloperDiskAsync(default).ConfigureAwait(false);
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, statusCodeResult.StatusCode);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.ImportDeveloperDiskAsync(CancellationToken)"/> validates the files which are being uploaded.
        /// </summary>
        /// <param name="files">
        /// The files being uploaded.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(ImportDeveloperDiskAsync_ValidatesFiles_Data))]
        public async Task ImportDeveloperDiskAsync_ValidatesFiles_Async(FormFileCollection files)
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);

            var controller = new DeveloperDiskController(store.Object, Mock.Of<DeveloperDiskFactory>())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            controller.Request.ContentType = "multipart/form-data";
            controller.Request.Form =
                    new FormCollection(
                        null,
                        files);

            var result = await controller.ImportDeveloperDiskAsync(default).ConfigureAwait(false);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.ImportDeveloperDiskAsync(CancellationToken)"/> correctly adds the disk to the store.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ImportDeveloperDiskAsync_ImportsFiles_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);

            var disk = new DeveloperDisk();
            factory.Setup(f => f
                .FromFileAsync(It.IsAny<TempFileStream>(), It.IsAny<Stream>(), default))
                .ReturnsAsync(disk)
                .Verifiable();

            store.Setup(s => s.AddAsync(disk, default)).Returns(Task.CompletedTask);

            var controller = new DeveloperDiskController(store.Object, factory.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            controller.Request.ContentType = "multipart/form-data";
            controller.Request.Form =
                    new FormCollection(
                        null,
                        new FormFileCollection()
                        {
                            new FormFile(Stream.Null, 0, 0x400_000, "DeveloperDiskImage.dmg", "DeveloperDiskImage.dmg"),
                            new FormFile(Stream.Null, 0, 0x10, "DeveloperDiskImage.dmg.signature", "DeveloperDiskImage.dmg.signature"),
                        });

            var result = await controller.ImportDeveloperDiskAsync(default).ConfigureAwait(false);

            factory.Verify();
            store.Verify();
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.ListDeveloperDisksAsync(CancellationToken)"/> returns the correct values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_Works_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            store.Setup(s => s.ListAsync(default))
                .ReturnsAsync(new List<Version>() { new Version(10, 1), new Version(14, 0) });

            var result = await controller.ListDeveloperDisksAsync(default);
            Assert.Collection(
                Assert.IsType<List<string>>(result.Value),
                r => Assert.Equal("10.1", r),
                r => Assert.Equal("14.0", r));
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskAsync(string, CancellationToken)"/> returns 404 Not Found
        /// when passed an invalid version number.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskAsync_NotAVersion_ReturnsNotFound_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            var result = await controller.DownloadDeveloperDiskAsync("abc", default);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskAsync(string, CancellationToken)"/> returns 404 Not Found
        /// when passed a version number of a disk which does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskAsync_NoSuchVersion_ReturnsNotFound_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            store.Setup(s => s.GetAsync(new Version(14, 0), default)).ReturnsAsync((DeveloperDisk)null);

            var result = await controller.DownloadDeveloperDiskAsync("14.0", default);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskAsync(string, CancellationToken)"/> returns the
        /// disk contents.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskAsync_ReturnsDisk_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            var disk = new DeveloperDisk()
            {
                Image = Stream.Null,
            };

            store.Setup(s => s.GetAsync(new Version(14, 0), default)).ReturnsAsync(disk);

            var result = await controller.DownloadDeveloperDiskAsync("14.0", default);
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);

            Assert.NotNull(fileStreamResult.FileStream);
            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal("DeveloperDiskImage.dmg", fileStreamResult.FileDownloadName);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskSignatureAsync(string, CancellationToken)"/> returns 404 Not Found
        /// when passed an invalid version number.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskSignatureAsync_NotAVersion_ReturnsNotFound_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            var result = await controller.DownloadDeveloperDiskSignatureAsync("abc", default);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskSignatureAsync(string, CancellationToken)"/> returns 404 Not Found
        /// when passed a version number of a disk which does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskSignatureAsync_NoSuchVersion_ReturnsNotFound_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            store.Setup(s => s.GetAsync(new Version(14, 0), default)).ReturnsAsync((DeveloperDisk)null);

            var result = await controller.DownloadDeveloperDiskSignatureAsync("14.0", default);
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// <see cref="DeveloperDiskController.DownloadDeveloperDiskSignatureAsync(string, CancellationToken)"/> returns the developer
        /// disk signature.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DownloadDeveloperDiskSignatureAsync_ReturnsDisk_Async()
        {
            var store = new Mock<DeveloperDiskStore>(MockBehavior.Strict);
            var factory = new Mock<DeveloperDiskFactory>(MockBehavior.Strict);
            var controller = new DeveloperDiskController(store.Object, factory.Object);

            var disk = new DeveloperDisk()
            {
                Signature = Array.Empty<byte>(),
            };

            store.Setup(s => s.GetAsync(new Version(14, 0), default)).ReturnsAsync(disk);

            var result = await controller.DownloadDeveloperDiskSignatureAsync("14.0", default);
            var fileStreamResult = Assert.IsType<FileContentResult>(result);

            Assert.NotNull(fileStreamResult.FileContents);
            Assert.Equal("application/octet-stream", fileStreamResult.ContentType);
            Assert.Equal("DeveloperDiskImage.dmg.signature", fileStreamResult.FileDownloadName);
        }
    }
}
