// <copyright file="TJDecompressor.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Microsoft;
using System;

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format.
    /// </summary>
    public unsafe class TJDecompressor : IDisposableObservable
    {
        private IntPtr decompressorHandle = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJDecompressor"/> class.
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created.
        /// </exception>
        public TJDecompressor()
        {
            this.decompressorHandle = TurboJpegImport.TjInitDecompress();

            if (this.decompressorHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="outBuf">The buffer into which to store the decompressed JPEG image.</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="TJPixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <param name="width">Width of image in pixels.</param>
        /// <param name="height">Height of image in pixels.</param>
        /// <param name="stride">Bytes per line in the destination image.</param>
        public unsafe void Decompress(Span<byte> jpegBuf, Span<byte> outBuf, TJPixelFormat destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            Verify.NotDisposed(this);

            fixed (byte* jpegBufPtr = jpegBuf)
            fixed (byte* outBufPtr = outBuf)
            {
                int subsampl;
                int colorspace;
                var funcResult = TurboJpegImport.TjDecompressHeader(
                    this.decompressorHandle,
                    jpegBufPtr,
                    (nuint)jpegBuf.Length,
                    out width,
                    out height,
                    out subsampl,
                    out colorspace);

                if (funcResult == -1)
                {
                    TJUtils.GetErrorAndThrow();
                }

                var targetFormat = destPixelFormat;
                stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[targetFormat]);
                var bufSize = stride * height;

                if (outBuf.Length < bufSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(outBuf));
                }

                funcResult = TurboJpegImport.TjDecompress(
                    this.decompressorHandle,
                    jpegBufPtr,
                    (nuint)jpegBuf.Length,
                    outBufPtr,
                    width,
                    stride,
                    height,
                    (int)targetFormat,
                    (int)flags);

                if (funcResult == -1)
                {
                    TJUtils.GetErrorAndThrow();
                }
            }
        }

        /// <summary>
        /// Decode a set of Y, U (Cb), and V (Cr) image planes into an RGB or grayscale image.
        /// </summary>
        /// <param name="yPlane">
        /// A pointer to the Y image planes of the YUV image to be decoded.
        /// The size of the plane should match the value returned by tjPlaneSizeYUV() for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="uPlane">
        /// A pointer to the U (Cb) image plane (or just <see langword="null"/>, if decoding a grayscale image) of the YUV image to be decoded.
        /// The size of the plane should match the value returned by tjPlaneSizeYUV() for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="vPlane">
        /// A pointer to the V (Cr)image plane (or just <see langword="null"/>, if decoding a grayscale image) of the YUV image to be decoded.
        /// The size of the plane should match the value returned by tjPlaneSizeYUV() for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="strides">
        /// An array of integers, each specifying the number of bytes per line in the corresponding plane of the YUV source image.
        /// Setting the stride for any plane to 0 is the same as setting it to the plane width (see YUV Image Format Notes.)
        /// If strides is <see langword="null"/>, then the strides for all planes will be set to their respective plane widths.
        /// You can adjust the strides in order to specify an arbitrary amount of line padding in each plane or to decode a subregion of a larger YUV planar image.
        /// </param>
        /// <param name="subsamp">
        /// The level of chrominance subsampling used in the YUV source image (see Chrominance subsampling options.)
        /// </param>
        /// <param name="dstBuf">
        /// A pointer to an image buffer that will receive the decoded image. This buffer should normally be <paramref name="pitch"/> * <paramref name="height"/> bytes in size,
        /// but the <paramref name="dstBuf"/> pointer can also be used to decode into a specific region of a larger buffer.
        /// </param>
        /// <param name="width">
        /// Width (in pixels) of the source and destination images.
        /// </param>
        /// <param name="pitch">
        /// Bytes per line in the destination image. Normally, this should be <paramref name="width"/> * <c>tjPixelSize[pixelFormat]</c>
        /// if the destination image is unpadded, or <c>TJPAD(width * tjPixelSize[pixelFormat])</c> if each line of the destination image
        /// should be padded to the nearest 32-bit boundary, as is the case for Windows bitmaps. You can also be clever and use the
        /// pitch parameter to skip lines, etc.
        /// Setting this parameter to <c>0</c> is the equivalent of setting it to <paramref name="width"/> * <c>tjPixelSize[pixelFormat]</c>.
        /// </param>
        /// <param name="height">
        /// Height (in pixels) of the source and destination images.
        /// </param>
        /// <param name="pixelFormat">
        /// Pixel format of the destination image.
        /// </param>
        /// <param name="flags">
        /// The bitwise OR of one or more of the flags.
        /// </param>
        /// <remarks>
        /// <para>
        /// This function uses the accelerated color conversion routines in the underlying codec but does not execute any of the other steps in the JPEG decompression process.
        /// </para>
        /// <para>
        /// The <paramref name="yPlane"/>, <paramref name="uPlane"/> and <paramref name="vPlane"/> planes can be contiguous or non-contiguous in memory.
        /// Refer to YUV Image Format Notes for more details.
        /// </para>
        /// </remarks>
        public unsafe void DecodeYUVPlanes(
            Span<byte> yPlane,
            Span<byte> uPlane,
            Span<byte> vPlane,
            int[] strides,
            TJSubsamplingOption subsamp,
            Span<byte> dstBuf,
            int width,
            int pitch,
            int height,
            TJPixelFormat pixelFormat,
            TJFlags flags)
        {
            Verify.NotDisposed(this);

            fixed (byte* yPlanePtr = yPlane)
            fixed (byte* uPlanePtr = uPlane)
            fixed (byte* vPlanePtr = vPlane)
            {
                byte*[] planes = new byte*[] { yPlanePtr, uPlanePtr, vPlanePtr };

                fixed (int* stridesPtr = strides)
                fixed (byte** planesPtr = planes)
                fixed (byte* dstBufPtr = dstBuf)
                {
                    if (TurboJpegImport.TjDecodeYUVPlanes(
                        this.decompressorHandle,
                        planesPtr,
                        stridesPtr,
                        (int)subsamp,
                        (IntPtr)dstBufPtr,
                        width,
                        pitch,
                        height,
                        (int)pixelFormat,
                        (int)flags) == -1)
                    {
                        TJUtils.GetErrorAndThrow();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing a JPEG image.  This buffer is not modified.
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image.
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image.
        /// </param>
        /// <param name="stride">
        /// Pointer to an integer variable that will receive the stride (in bytes) of the JPEG image.
        /// </param>
        /// <param name="bufSize">
        /// The size of a buffer that can receive the uncompressed JPEG image.
        /// </param>
        public void GetImageInfo(Span<byte> jpegBuf, TJPixelFormat destPixelFormat, out int width, out int height, out int stride, out int bufSize)
        {
            Verify.NotDisposed(this);

            int subsampl;
            int colorspace;

            fixed (byte* jpegBufPtr = jpegBuf)
            {
                var funcResult = TurboJpegImport.TjDecompressHeader(
                    this.decompressorHandle,
                    jpegBufPtr,
                    (nuint)jpegBuf.Length,
                    out width,
                    out height,
                    out subsampl,
                    out colorspace);

                stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[destPixelFormat]);
                bufSize = stride * height;
            }
        }

        /// <summary>
        /// Given the size of an image, determines the size of a decompressed image.
        /// </summary>
        /// <param name="height">
        /// The height of the image.
        /// </param>
        /// <param name="width">
        /// The width of the image.
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <returns>
        /// The size of a buffer that can hold the uncompressed image.
        /// </returns>
        public int GetBufferSize(int height, int width, TJPixelFormat destPixelFormat)
        {
            Verify.NotDisposed(this);

            int stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[destPixelFormat]);
            return stride * height;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // If for whathever reason, the handle was not initialized correctly (e.g. an exception
            // in the constructor), we shouldn't free it either.
            if (this.decompressorHandle != IntPtr.Zero)
            {
                TurboJpegImport.TjDestroy(this.decompressorHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                this.decompressorHandle = IntPtr.Zero;
            }

            this.IsDisposed = true;
        }
    }
}
