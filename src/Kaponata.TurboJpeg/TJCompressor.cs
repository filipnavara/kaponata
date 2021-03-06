// <copyright file="TJCompressor.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Microsoft;
using System;
using System.Runtime.InteropServices;

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format.
    /// </summary>
    public class TJCompressor : IDisposableObservable
    {
        private IntPtr compressorHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJCompressor"/> class.
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created.
        /// </exception>
        public TJCompressor()
        {
            this.compressorHandle = TurboJpegImport.TjInitCompress();
            TJUtils.ThrowOnError(this.compressorHandle);
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="srcBuf">
        /// Image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.
        /// </param>
        /// <param name="destBuf">
        /// A <see cref="byte"/> array containing the compressed image.
        /// </param>
        /// <param name="stride">
        /// Bytes per line in the source image.
        /// Normally, this should be <c>width * BytesPerPixel</c> if the image is unpadded,
        /// or <c>TJPAD(width * BytesPerPixel</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * BytesPerPixel</c>.
        /// </param>
        /// <param name="width">Width (in pixels) of the source image.</param>
        /// <param name="height">Height (in pixels) of the source image.</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="TJPixelFormat"/> "Pixel formats").</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// A <see cref="Span{T}"/> which is a slice of <paramref name="destBuf"/> which holds the compressed image.
        /// </returns>
        /// <exception cref="TJException">
        /// Throws if compress function failed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public unsafe Span<byte> Compress(Span<byte> srcBuf, Span<byte> destBuf, int stride, int width, int height, TJPixelFormat pixelFormat, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            Verify.NotDisposed(this);

            CheckOptionsCompatibilityAndThrow(subSamp, pixelFormat);

            ulong destBufSize = (ulong)destBuf.Length;

            fixed (byte* srcBufPtr = srcBuf)
            fixed (byte* destBufPtr = destBuf)
            {
                IntPtr destBufPtr2 = (IntPtr)destBufPtr;

                var result = TurboJpegImport.TjCompress2(
                    this.compressorHandle,
                    (IntPtr)srcBufPtr,
                    width,
                    stride,
                    height,
                    (int)pixelFormat,
                    ref destBufPtr2,
                    ref destBufSize,
                    (int)subSamp,
                    quality,
                    (int)flags);

                TJUtils.ThrowOnError(result);
            }

            return destBuf.Slice(0, (int)destBufSize);
        }

        /// <summary>
        /// The size of the buffer (in bytes) required to hold a YUV image plane with
        /// the given parameters.
        /// </summary>
        /// <param name="componentID">
        /// ID number of the image plane (0 = Y, 1 = U/Cb, 2 = V/Cr).
        /// </param>
        /// <param name="width">
        /// width (in pixels) of the YUV image.  NOTE: this is the width of
        /// the whole image, not the plane width.
        /// </param>
        /// <param name="stride">
        /// bytes per line in the image plane.  Setting this to 0 is the
        /// equivalent of setting it to the plane width.
        /// </param>
        /// <param name="height">
        /// height (in pixels) of the YUV image.  NOTE: this is the height
        /// of the whole image, not the plane height.
        /// </param>
        /// <param name="subsamp">
        /// level of chrominance subsampling in the image.
        /// </param>
        /// <returns>
        /// the size of the buffer (in bytes) required to hold the YUV image
        /// plane, or -1 if the arguments are out of bounds.
        /// </returns>
        public nuint PlaneSizeYUV(TJPlane componentID, int width, int stride, int height, int subsamp) => TurboJpegImport.TjPlaneSizeYUV(componentID, width, stride, height, subsamp);

        /// <summary>
        /// The plane width of a YUV image plane with the given parameters.
        /// </summary>
        /// <param name="componentID">
        /// ID number of the image plane (0 = Y, 1 = U/Cb, 2 = V/Cr).
        /// </param>
        /// <param name="width">
        /// width (in pixels) of the YUV image.
        /// </param>
        /// <param name="subsamp">
        /// level of chrominance subsampling in the image.
        /// </param>
        /// <returns>
        /// The plane width of a YUV image plane with the given parameters, or
        /// -1 if the arguments are out of bounds.
        /// </returns>
        public int PlaneWidth(TJPlane componentID, int width, int subsamp) => TurboJpegImport.TjPlaneWidth(componentID, width, subsamp);

        /// <summary>
        /// The plane height of a YUV image plane with the given parameters.
        /// </summary>
        /// <param name="componentID">
        /// ID number of the image plane (0 = Y, 1 = U/Cb, 2 = V/Cr).
        /// </param>
        /// <param name="height">
        /// Height (in pixels) of the YUV image.
        /// </param>
        /// <param name="subsamp">
        /// Level of chrominance subsampling in the image.
        /// </param>
        /// <returns>
        /// The plane height of a YUV image plane with the given parameters, or
        /// -1 if the arguments are out of bounds.
        /// </returns>
        public int PlaneHeight(TJPlane componentID, int height, int subsamp) => TurboJpegImport.TjPlaneHeight(componentID, height, subsamp);

        /// <summary>
        /// Compress a set of Y, U (Cb), and V (Cr) image planes into a JPEG image.
        /// </summary>
        /// <param name="yPlane">
        /// A pointer to the Y image planes of the YUV image to be decoded.
        /// The size of the plane should match the value returned by <see cref="PlaneSizeYUV"/> for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="uPlane">
        /// A pointer to the U (Cb) image plane (or just <see langword="null"/>, if decoding a grayscale image) of the YUV image to be decoded.
        /// The size of the plane should match the value returned by <see cref="PlaneSizeYUV"/> for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="vPlane">
        /// A pointer to the V (Cr)image plane (or just <see langword="null"/>, if decoding a grayscale image) of the YUV image to be decoded.
        /// The size of the plane should match the value returned by <see cref="PlaneSizeYUV"/> for the given image width, height, strides, and level of chrominance subsampling.
        /// </param>
        /// <param name="width">
        /// The width (in pixels) of the source image. If the width is not an even multiple of the MCU block width (see tjMCUWidth), then an intermediate buffer copy will be performed within TurboJPEG.
        /// </param>
        /// <param name="strides">
        /// An array of integers, each specifying the number of bytes per line in the corresponding plane of the YUV source image.
        /// Setting the stride for any plane to 0 is the same as setting it to the plane width (see YUV Image Format Notes.)
        /// If strides is <see langword="null"/>, then the strides for all planes will be set to their respective plane widths.
        /// You can adjust the strides in order to specify an arbitrary amount of line padding in each plane or to decode a subregion of a larger YUV planar image.
        /// </param>
        /// <param name="height">
        /// The height (in pixels) of the source image. If the height is not an even multiple of the MCU block height (see tjMCUHeight), then an intermediate buffer copy will be performed within TurboJPEG.
        /// </param>
        /// <param name="subsamp">
        /// The level of chrominance subsampling used in the source image (see Chrominance subsampling options.)
        /// </param>
        /// <param name="jpegBuf">
        /// A pointer to an image buffer that will receive the JPEG image. TurboJPEG has the ability to reallocate the JPEG buffer to accommodate the size of the JPEG image. Thus, you can choose to:
        /// <list type="number">
        ///   <item>
        ///      pre-allocate the JPEG buffer with an arbitrary size using <see cref="TurboJpegImport.TjAlloc"/> and let TurboJPEG grow the buffer as needed,
        ///   </item>
        ///   <item>
        ///     set* jpegBuf to NULL to tell TurboJPEG to allocate the buffer for you, or
        ///   </item>
        ///   <item>
        ///     pre-allocate the buffer to a "worst case" size determined by calling tjBufSize(). This should ensure that the buffer never has to be re-allocated(setting TJFLAG_NOREALLOC guarantees that it won't be.)
        ///   </item>
        /// </list>
        /// If you choose option 1, * jpegSize should be set to the size of your pre-allocated buffer. In any case, unless you have set TJFLAG_NOREALLOC, you should always check *jpegBuf upon return from this function, as it may have changed.
        /// </param>
        /// <param name="jpegQual">
        /// The image quality of the generated JPEG image (1 = worst, 100 = best).
        /// </param>
        /// <param name="flags">
        /// The bitwise OR of one or more of the flags.
        /// </param>
        /// <returns>
        /// A <see cref="Span{T}"/> which holds the compressed image.
        /// </returns>
        public unsafe Span<byte> CompressFromYUVPlanes(
                Span<byte> yPlane,
                Span<byte> uPlane,
                Span<byte> vPlane,
                int width,
                int[] strides,
                int height,
                TJSubsamplingOption subsamp,
                Span<byte> jpegBuf,
                int jpegQual,
                TJFlags flags)
        {
            Verify.NotDisposed(this);

            nuint destBufSize = (nuint)jpegBuf.Length;
            IntPtr jpegBufPtr2 = IntPtr.Zero;

            fixed (byte* yPlanePtr = yPlane)
            fixed (byte* uPlanePtr = uPlane)
            fixed (byte* vPlanePtr = vPlane)
            {
                byte*[] planes = new byte*[] { yPlanePtr, uPlanePtr, vPlanePtr };

                fixed (int* stridesPtr = strides)
                fixed (byte* jpegBufPtr = jpegBuf)
                fixed (byte** planesPtr = planes)
                {
                    jpegBufPtr2 = (IntPtr)jpegBufPtr;

                    var result = TurboJpegImport.TjCompressFromYUVPlanes(
                        this.compressorHandle,
                        planesPtr,
                        width,
                        stridesPtr,
                        height,
                        (int)subsamp,
                        ref jpegBufPtr2,
                        ref destBufSize,
                        jpegQual,
                        (int)flags);

                    TJUtils.ThrowOnError(result);
                }
            }

            return new Span<byte>(jpegBufPtr2.ToPointer(), (int)destBufSize);
        }

        /// <summary>
        /// The maximum size of the buffer (in bytes) required to hold a JPEG image with
        /// the given parameters.  The number of bytes returned by this function is
        /// larger than the size of the uncompressed source image.  The reason for this
        /// is that the JPEG format uses 16-bit coefficients, and it is thus possible
        /// for a very high-quality JPEG image with very high-frequency content to
        /// expand rather than compress when converted to the JPEG format.  Such images
        /// represent a very rare corner case, but since there is no way to predict the
        /// size of a JPEG image prior to compression, the corner case has to be handled.
        /// </summary>
        /// <param name="width">Width (in pixels) of the image.</param>
        /// <param name="height">Height (in pixels) of the image.</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image(see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <returns>
        /// The maximum size of the buffer (in bytes) required to hold the image,
        /// or -1 if the arguments are out of bounds.
        /// </returns>
        public int GetBufferSize(int width, int height, TJSubsamplingOption subSamp)
        {
            Verify.NotDisposed(this);

            return (int)TurboJpegImport.TjBufSize(width, height, (int)subSamp);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // If for whathever reason, the handle was not initialized correctly (e.g. an exception
            // in the constructor), we shouldn't free it either.
            if (this.compressorHandle != IntPtr.Zero)
            {
                TurboJpegImport.TjDestroy(this.compressorHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                this.compressorHandle = IntPtr.Zero;
            }

            this.IsDisposed = true;
        }

        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        private static void CheckOptionsCompatibilityAndThrow(TJSubsamplingOption subSamp, TJPixelFormat srcFormat)
        {
            if (srcFormat == TJPixelFormat.Gray && subSamp != TJSubsamplingOption.Gray)
            {
                throw new NotSupportedException(
                    $"Subsampling differ from {TJSubsamplingOption.Gray} for pixel format {TJPixelFormat.Gray} is not supported");
            }
        }
    }
}
