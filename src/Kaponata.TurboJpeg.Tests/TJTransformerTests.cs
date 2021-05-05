// <copyright file="TJTransformerTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.IO;
using Xunit;

namespace Kaponata.TurboJpeg.Tests
{
    /// <summary>
    /// Tests the <see cref="TJTransformer"/> class.
    /// </summary>
    public class TJTransformerTests : IDisposable
    {
        private TJTransformer transformer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJTransformerTests"/> class.
        /// </summary>
        public TJTransformerTests()
        {
            this.transformer = new TJTransformer();
            if (Directory.Exists(this.OutDirectory))
            {
                Directory.Delete(this.OutDirectory, true);
            }

            Directory.CreateDirectory(this.OutDirectory);
        }

        private string OutDirectory
        {
            get { return Path.Combine(TestUtils.BinPath, "transform_images_out"); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.transformer.Dispose();
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> throws when the transforms
        /// list is null or empty.
        /// </summary>
        [Fact]
        public void Transform_ValidatesTransforms()
        {
            Assert.Throws<ArgumentNullException>(() => this.transformer.Transform(Span<byte>.Empty, null, TJFlags.None));
            Assert.Throws<ArgumentException>(() => this.transformer.Transform(Span<byte>.Empty, Array.Empty<TJTransformDescription>(), TJFlags.None));
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> throws when the source image
        /// is null.
        /// </summary>
        [Fact]
        public void Transform_InvalidImage_Throws()
        {
            Assert.Throws<TJException>(() => this.transformer.Transform(Span<byte>.Empty, new TJTransformDescription[] { default }, TJFlags.None));
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> throws when the transforms
        /// list is null or empty.
        /// </summary>
        [Fact]
        public void Transform_InvalidTransform_Throws()
        {
            var data = File.ReadAllBytes("TestAssets/testorig.jpg");

            Assert.Throws<TJException>(() => this.transformer.Transform(data, new TJTransformDescription[] { default }, (TJFlags)(-1)));
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> can transform images to grayscale.
        /// </summary>
        [Fact]
        public void TransformToGrayscaleFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var transforms = new[]
                {
                    new TJTransformDescription
                    {
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Gray,
                        Region = TJRegion.Empty,
                    },
                };
                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);

                Assert.NotNull(result);
                Assert.Single(result);

                var file = Path.Combine(this.OutDirectory, "gray_" + Path.GetFileName(data.Item1));
                File.WriteAllBytes(file, result[0]);
            }
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> can crop a single impage.
        /// </summary>
        [Fact]
        public void TransformToCroppedSingleImageFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var transforms = new[]
                {
                    new TJTransformDescription
                    {
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 0,
                            Y = 0,
                            W = 50,
                            H = 50,
                        },
                    },
                };

                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(this.OutDirectory, $"crop_s_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
                    File.WriteAllBytes(file, result[0]);
                }
            }
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> can crop multiple images at once.
        /// </summary>
        [Fact]
        public void TransformToCroppedMultiplyImagesFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var transforms = new[]
                {
                    new TJTransformDescription
                    {
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 0,
                            Y = 0,
                            W = 100,
                            H = 100,
                        },
                    },
                    new TJTransformDescription
                    {
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 50,
                            Y = 0,
                            W = 100,
                            H = 100,
                        },
                    },
                };

                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(this.OutDirectory, $"crop_m_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
                    File.WriteAllBytes(file, result[idx]);
                }
            }
        }

        /// <summary>
        /// <see cref="TJTransformer.Transform(Span{byte}, TJTransformDescription[], TJFlags)"/> can crop multiple images at once.
        /// </summary>
        /// <param name="x">
        /// The left boundary of the cropping region.
        /// </param>
        /// <param name="y">
        /// The upper boundary of the cropping region.
        /// </param>
        /// <param name="width">
        /// The width of the cropping region.
        /// </param>
        /// <param name="height">
        /// The height of the cropping region.
        /// </param>
        [InlineData(0, 0, 500, 500)]
        [InlineData(50, 50, 100, 100)]
        [Theory]
        public void Transform_CorrectsSize(int x, int y, int width, int height)
        {
            var data = File.ReadAllBytes("TestAssets/testorig.jpg");

            var transforms = new[]
            {
                new TJTransformDescription
                {
                    Operation = TJTransformOperation.None,
                    Options = TJTransformOptions.Crop,
                    Region = new TJRegion
                    {
                        X = x,
                        Y = y,
                        W = width,
                        H = height,
                    },
                },
            };

            var result = this.transformer.Transform(data, transforms, TJFlags.None);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// <see cref="TJDecompressor"/> methods throw when disposed.
        /// </summary>
        [Fact]
        public void Methods_ThrowWhenDisposed()
        {
            this.transformer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => this.transformer.Transform(Span<byte>.Empty, Array.Empty<TJTransformDescription>(), TJFlags.None));
        }
    }
}
