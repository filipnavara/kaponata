// <copyright file="TurboJpegImport.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// P/Invoke declarations for turbojpeg.
    /// </summary>
    internal static unsafe class TurboJpegImport
    {
        /// <summary>
        /// The name of the unmanaged library.
        /// </summary>
        public const string UnmanagedLibrary = "turbojpeg";

        /// <summary>
        /// Pixel size (in bytes) for a given pixel format.
        /// </summary>
        public static readonly Dictionary<TJPixelFormat, int> PixelSizes = new Dictionary<TJPixelFormat, int>
        {
            { TJPixelFormat.RGB, 3 },
            { TJPixelFormat.BGR, 3 },
            { TJPixelFormat.RGBX, 4 },
            { TJPixelFormat.BGRX, 4 },
            { TJPixelFormat.XBGR, 4 },
            { TJPixelFormat.XRGB, 4 },
            { TJPixelFormat.Gray, 1 },
            { TJPixelFormat.RGBA, 4 },
            { TJPixelFormat.BGRA, 4 },
            { TJPixelFormat.ABGR, 4 },
            { TJPixelFormat.ARGB, 4 },
            { TJPixelFormat.CMYK, 4 },
        };

        /// <summary>
        /// MCU block width (in pixels) for a given level of chrominance subsampling.
        /// MCU block sizes:
        /// <list type="bullet">
        /// <item><description>8x8 for no subsampling or grayscale</description></item>
        /// <item><description>16x8 for 4:2:2</description></item>
        /// <item><description>8x16 for 4:4:0</description></item>
        /// <item><description>16x16 for 4:2:0</description></item>
        /// <item><description>32x8 for 4:1:1</description></item>
        /// </list>
        /// </summary>
        public static readonly Dictionary<TJSubsamplingOption, Size> MCUSizes = new Dictionary<TJSubsamplingOption, Size>
        {
            { TJSubsamplingOption.Gray, new Size(8, 8) },
            { TJSubsamplingOption.Chrominance444, new Size(8, 8) },
            { TJSubsamplingOption.Chrominance422, new Size(16, 8) },
            { TJSubsamplingOption.Chrominance420, new Size(16, 16) },
            { TJSubsamplingOption.Chrominance440, new Size(8, 16) },
            { TJSubsamplingOption.Chrominance411, new Size(32, 8) },
        };

        static TurboJpegImport()
        {
            LibraryResolver.EnsureRegistered();
        }

        /// <summary>
        /// This is port of TJPAD macros from turbojpeg.h
        /// Pad the given width to the nearest 32-bit boundary.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <returns>Padded width.</returns>
        public static int TJPAD(int width)
        {
            return (width + 3) & (~3);
        }

        /// <summary>
        /// Create a TurboJPEG compressor instance.
        /// </summary>
        /// <returns>
        /// handle to the newly-created instance, or <see cref="IntPtr.Zero"/>
        /// if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitCompress")]
        public static extern IntPtr TjInitCompress();

        /// <summary>
        /// Compress an RGB, grayscale, or CMYK image into a JPEG image.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG compressor or transformer instance.</param>
        ///
        /// <param name="srcBuf">
        /// Pointer to an image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.
        /// </param>
        ///
        /// <param name="width">Width (in pixels) of the source image.</param>
        ///
        /// <param name="pitch">
        /// Bytes per line in the source image.
        /// Normally, this should be <c>width * tjPixelSize[pixelFormat]</c> if the image is unpadded,
        /// or <c>TJPAD(width * tjPixelSize[pixelFormat])</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * tjPixelSize[pixelFormat]</c>.
        /// </param>
        ///
        /// <param name="height">Height (in pixels) of the source image.</param>
        ///
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="TJPixelFormat"/> "Pixel formats").</param>
        ///
        /// <param name="jpegBuf">
        /// Address of a pointer to an image buffer that will receive the JPEG image.
        /// TurboJPEG has the ability to reallocate the JPEG buffer
        /// to accommodate the size of the JPEG image.  Thus, you can choose to:
        /// <list type="number">
        /// <item>
        /// <description>pre-allocate the JPEG buffer with an arbitrary size using <see cref="TjAlloc"/> and let TurboJPEG grow the buffer as needed</description>
        /// </item>
        /// <item>
        /// <description>set <paramref name="jpegBuf"/> to NULL to tell TurboJPEG to allocate the buffer for you</description>
        /// </item>
        /// <item>
        /// <description>pre-allocate the buffer to a "worst case" size determined by calling <see cref="TjBufSize"/>.
        /// This should ensure that the buffer never has to be re-allocated (setting <see cref="TJFlags.NoRealloc"/> guarantees this.).</description>
        /// </item>
        /// </list>
        /// If you choose option 1, <paramref name="jpegSize"/> should be set to the size of your pre-allocated buffer.
        /// In any case, unless you have set <see cref="TJFlags.NoRealloc"/>,
        /// you should always check <paramref name="jpegBuf"/> upon return from this function, as it may have changed.
        /// </param>
        ///
        /// <param name="jpegSize">
        /// Pointer to an unsigned long variable that holds the size of the JPEG image buffer.
        /// If <paramref name="jpegBuf"/> points to a pre-allocated buffer,
        /// then <paramref name="jpegSize"/> should be set to the size of the buffer.
        /// Upon return, <paramref name="jpegSize"/> will contain the size of the JPEG image (in bytes.)
        /// If <paramref name="jpegBuf"/> points to a JPEG image buffer that is being
        /// reused from a previous call to one of the JPEG compression functions,
        /// then <paramref name="jpegSize"/> is ignored.
        /// </param>
        ///
        /// <param name="jpegSubsamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        ///
        /// <param name="jpegQual">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        ///
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        ///
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjCompress2")]
        public static extern int TjCompress2(IntPtr handle, IntPtr srcBuf, int width, int pitch, int height, int pixelFormat, ref IntPtr jpegBuf, ref ulong jpegSize, int jpegSubsamp, int jpegQual, int flags);

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
        /// <param name="jpegSubsamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image(see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <returns>
        /// The maximum size of the buffer (in bytes) required to hold the image,
        /// or -1 if the arguments are out of bounds.
        /// </returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjBufSize")]
        public static extern long TjBufSize(int width, int height, int jpegSubsamp);

        /// <summary>
        ///  Create a TurboJPEG decompressor instance.
        /// </summary>
        /// <returns>A handle to the newly-created instance, or NULL if an error occurred(see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitDecompress")]
        public static extern IntPtr TjInitDecompress();

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG decompressor or transformer instance.</param>
        /// <param name="jpegBuf">Pointer to a buffer containing a JPEG image.  This buffer is not modified.</param>
        /// <param name="jpegSize">Size of the JPEG image (in bytes).</param>
        /// <param name="width">Pointer to an integer variable that will receive the width (in pixels) of the JPEG image.</param>
        /// <param name="height">Pointer to an integer variable that will receive the height (in pixels) of the JPEG image.</param>
        /// <param name="jpegSubsamp">
        /// Pointer to an integer variable that will receive the level of chrominance subsampling used
        /// when the JPEG image was compressed (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="jpegColorspace">Pointer to an integer variable that will receive one of the JPEG colorspace constants,
        /// indicating the colorspace of the JPEG image(see <see cref="TJColorSpace"/> "JPEG colorspaces".)</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompressHeader3")]
        public static extern int TjDecompressHeader(IntPtr handle, byte* jpegBuf, nuint jpegSize, out int width, out int height, out int jpegSubsamp, out int jpegColorspace);

        /// <summary>
        /// Returns a list of fractional scaling factors that the JPEG decompressor in this implementation of TurboJPEG supports.
        /// </summary>
        /// <param name="numscalingfactors">Pointer to an integer variable that will receive the number of elements in the list.</param>
        /// <returns>A pointer to a list of fractional scaling factors, or <see cref="IntPtr.Zero"/> if an error is encountered (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjGetScalingFactors")]
        public static extern IntPtr TjGetScalingFactors(out int numscalingfactors);

        /// <summary>
        /// Decode a set of Y, U (Cb), and V (Cr) image planes into an RGB or grayscale
        /// image.  This function uses the accelerated color conversion routines in the
        /// underlying codec but does not execute any of the other steps in the JPEG
        /// decompression process.
        /// </summary>
        /// <param name="handle">
        /// A handle to a TurboJPEG decompressor or transformer instance.
        /// </param>
        /// <param name="srcPlanes">
        /// An array of pointers to Y, U (Cb), and V (Cr) image planes
        /// (or just a Y plane, if decoding a grayscale image) that contain a YUV image
        /// to be decoded.  These planes can be contiguous or non-contiguous in memory.
        /// The size of each plane should match the value returned by <c>tjPlaneSizeYUV()</c>
        /// for the given image width, height, strides, and level of chrominance
        /// subsampling.
        /// </param>
        /// <param name="strides">
        /// An array of integers, each specifying the number of bytes per
        /// line in the corresponding plane of the YUV source image.  Setting the stride
        /// for any plane to 0 is the same as setting it to the plane width.
        /// If <paramref name="strides"/> is <see langword="null"/>, then
        /// the strides for all planes will be set to their respective plane widths.
        /// You can adjust the strides in order to specify an arbitrary amount of line
        /// padding in each plane or to decode a subregion of a larger YUV planar image.
        /// </param>
        /// <param name="subsamp">
        /// The level of chrominance subsampling used in the YUV source image.
        /// </param>
        /// <param name="dstBuf">
        /// Pointer to an image buffer that will receive the decoded
        /// image.  This buffer should normally be <paramref name="pitch"/>
        /// * <paramref name="height"/> bytes in size, but the <paramref name="dstBuf"/>
        /// pointer can also be used to decode into a specific region of a larger buffer.
        /// </param>
        /// <param name="width">
        /// Width (in pixels) of the source and destination images.
        /// </param>
        /// <param name="pitch">
        /// Bytes per line in the destination image.  Normally, this should be
        /// <paramref name="width"/> * <c>tjPixelSize[pixelFormat]</c> if the destination image is
        /// unpadded, or <c>TJPAD(width * tjPixelSize[pixelFormat]</c> if each line
        /// of the destination image should be padded to the nearest 32-bit boundary, as
        /// is the case for Windows bitmaps.  You can also be clever and use the pitch
        /// parameter to skip lines, etc.  Setting this parameter to 0 is the equivalent
        /// of setting it to <paramref name="width"/> * <c>tjPixelSize[pixelFormat]</c>.
        /// </param>
        /// <param name="height">
        /// Height (in pixels) of the source and destination images.
        /// </param>
        /// <param name="pixelFormat">
        /// Pixel format of the destination image.
        /// </param>
        /// <param name="flags">
        /// The bitwise OR of one or more of the <see cref="TJFlags"/> flags.
        /// </param>
        /// <returns>
        /// 0 if successful, or -1 if an error occurred.
        /// </returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecodeYUVPlanes")]
        public static unsafe extern int TjDecodeYUVPlanes(
            IntPtr handle,
            byte** srcPlanes,
            int* strides,
            int subsamp,
            IntPtr dstBuf,
            int width,
            int pitch,
            int height,
            int pixelFormat,
            int flags);

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG decompressor or transformer instance.</param>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegSize">Size of the JPEG image (in bytes).</param>
        /// <param name="dstBuf">
        /// Pointer to an image buffer that will receive the decompressed image.
        /// This buffer should normally be <c> pitch * scaledHeight</c> bytes in size,
        /// where <c>scaledHeight</c> can be determined by calling <c>TJSCALED</c> with the JPEG image height and one of the scaling factors returned by <see cref="TjGetScalingFactors"/>.
        /// The <paramref name="dstBuf"/> pointer may also be used to decompress into a specific region of a larger buffer.
        /// </param>
        /// <param name="width">
        /// Desired width (in pixels) of the destination image.
        /// If this is different than the width of the JPEG image being decompressed, then TurboJPEG will use scaling in the JPEG decompressor to generate the largest possible image that will fit within the desired width.
        /// If <paramref name="width"/> is set to 0, then only the height will be considered when determining the scaled image size.
        /// </param>
        /// <param name="pitch">
        /// Bytes per line in the destination image.  Normally, this is <c>scaledWidth* tjPixelSize[pixelFormat]</c> if the decompressed image is unpadded, else <c>TJPAD(scaledWidth * tjPixelSize[pixelFormat])</c> if each line of the decompressed image is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// <remarks>Note: <c>scaledWidth</c> can be determined by calling <c>TJSCALED</c> with the JPEG image width and one of the scaling factors returned by <see cref="TjGetScalingFactors"/>
        /// </remarks>
        /// You can also be clever and use the pitch parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to <c>scaledWidth* tjPixelSize[pixelFormat]</c>.
        /// </param>
        /// <param name="height">
        /// Desired height (in pixels) of the destination image.
        /// If this is different than the height of the JPEG image being decompressed, then TurboJPEG will use scaling in the JPEG decompressor to generate the largest possible image that will fit within the desired height.
        /// If <paramref name="height"/> is set to 0, then only the width will be considered when determining the scaled image size.
        /// </param>
        /// <param name="pixelFormat">Pixel format of the destination image (see <see cref="TJPixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompress2")]
        public static extern int TjDecompress(IntPtr handle, byte* jpegBuf, nuint jpegSize, byte* dstBuf, int width, int pitch, int height, int pixelFormat, int flags);

        /// <summary>
        /// Allocate an image buffer for use with TurboJPEG.  You should always use
        /// this function to allocate the JPEG destination buffer(s) for <see cref="TjCompress2"/>
        /// and <see cref="TjTransform"/> unless you are disabling automatic buffer
        /// (re)allocation (by setting <see cref="TJFlags.NoRealloc"/>.)
        /// </summary>
        /// <param name="bytes">The number of bytes to allocate.</param>
        /// <returns>A pointer to a newly-allocated buffer with the specified number of bytes.</returns>
        /// <seealso cref="TjFree"/>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjAlloc")]
        public static extern IntPtr TjAlloc(int bytes);

        /// <summary>
        /// Free an image buffer previously allocated by TurboJPEG.  You should always
        /// use this function to free JPEG destination buffer(s) that were automatically
        /// (re)allocated by <see cref="TjCompress2"/> or <see cref="TjTransform"/> or that were manually
        /// allocated using <see cref="TjAlloc"/>.
        /// </summary>
        /// <param name="buffer">Address of the buffer to free.</param>
        /// <seealso cref="TjAlloc"/>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjFree")]
        public static extern void TjFree(IntPtr buffer);

        /// <summary>
        /// Create a new TurboJPEG transformer instance.
        /// </summary>
        /// <returns>@return a handle to the newly-created instance, or NULL if an error occurred(see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitTransform")]
        public static extern IntPtr TjInitTransform();

        /// <summary>
        /// Losslessly transform a JPEG image into another JPEG image.  Lossless
        /// transforms work by moving the raw DCT coefficients from one JPEG image
        /// structure to another without altering the values of the coefficients.  While
        /// this is typically faster than decompressing the image, transforming it, and
        /// re-compressing it, lossless transforms are not free.  Each lossless
        /// transform requires reading and performing Huffman decoding on all of the
        /// coefficients in the source image, regardless of the size of the destination
        /// image.  Thus, this function provides a means of generating multiple
        /// transformed images from the same source or  applying multiple
        /// transformations simultaneously, in order to eliminate the need to read the
        /// source coefficients multiple times.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG transformer instance.</param>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing the JPEG source image to transform.This buffer is not modified.
        /// </param>
        /// <param name="jpegSize">Size of the JPEG source image (in bytes).</param>
        /// <param name="n">The number of transformed JPEG images to generate.</param>
        /// <param name="dstBufs">
        /// Pointer to an array of n image buffers. <paramref name="dstBufs"/>[i] will receive a JPEG image that has been transformed using the parameters in <paramref name="transforms"/>[i]
        /// TurboJPEG has the ability to reallocate the JPEG buffer
        /// to accommodate the size of the JPEG image.  Thus, you can choose to:
        /// <list type="number">
        /// <item>
        /// <description>pre-allocate the JPEG buffer with an arbitrary size using <see cref="TjAlloc"/> and let TurboJPEG grow the buffer as needed</description>
        /// </item>
        /// <item>
        /// <description>set <paramref name="dstBufs"/>[i] to NULL to tell TurboJPEG to allocate the buffer for you</description>
        /// </item>
        /// <item>
        /// <description>pre-allocate the buffer to a "worst case" size determined by calling <see cref="TjBufSize"/>.
        /// This should ensure that the buffer never has to be re-allocated (setting <see cref="TJFlags.NoRealloc"/> guarantees this.).</description>
        /// </item>
        /// </list>
        /// If you choose option 1, <paramref name="dstSizes"/>[i] should be set to the size of your pre-allocated buffer.
        /// In any case, unless you have set <see cref="TJFlags.NoRealloc"/>,
        /// you should always check <paramref name="dstBufs"/>[i] upon return from this function, as it may have changed.
        /// </param>
        /// <param name="dstSizes">
        /// Pointer to an array of <paramref name="n"/> unsigned long variables that will
        /// receive the actual sizes (in bytes) of each transformed JPEG image.
        /// If <paramref name="dstBufs"/>[i] points to a pre-allocated buffer,
        /// then <paramref name="dstSizes"/>[i] should be set to the size of the buffer.
        /// Upon return, <paramref name="dstSizes"/>[i] will contain the size of the JPEG image (in bytes.)
        /// </param>
        /// <param name="transforms">
        /// Pointer to an array of <see cref="Kaponata.TurboJpeg.TJTransform"/> structures, each of
        /// which specifies the transform parameters and/or cropping region for the
        /// corresponding transformed output image.
        /// </param>
        /// <param name="flags">flags the bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjTransform")]
        public static extern int TjTransform(
            IntPtr handle,
            byte* jpegBuf,
            nuint jpegSize,
            int n,
            IntPtr[] dstBufs,
            uint[] dstSizes,
            IntPtr transforms,
            int flags);

        /// <summary>
        /// Compress a set of Y, U (Cb), and V (Cr) image planes into a JPEG image.
        /// </summary>
        /// <param name="handle">
        /// A handle to a TurboJPEG compressor or transformer instance.
        /// </param>
        /// <param name="srcPlanes">
        /// an array of pointers to Y, U (Cb), and V (Cr) image planes
        /// (or just a Y plane, if compressing a grayscale image) that contain a YUV
        /// image to be compressed.  These planes can be contiguous or non-contiguous in
        /// memory.  The size of each plane should match the value returned by
        /// <c>tjPlaneSizeYUV()</c> for the given image width, height, strides, and level of
        /// chrominance subsampling.
        /// </param>
        /// <param name="width">
        /// Width (in pixels) of the source image.  If the width is not an
        /// even multiple of the MCU block width, then an intermediate
        /// buffer copy will be performed within TurboJPEG.
        /// </param>
        /// <param name="strides">
        /// An array of integers, each specifying the number of bytes per
        /// line in the corresponding plane of the YUV source image.  Setting the stride
        /// for any plane to 0 is the same as setting it to the plane width.
        /// If <paramref name="strides"/> is <see langword="null"/>, then
        /// the strides for all planes will be set to their respective plane widths.
        /// You can adjust the strides in order to specify an arbitrary amount of line
        /// padding in each plane or to create a JPEG image from a subregion of a larger
        /// YUV planar image.
        /// </param>
        /// <param name="height">
        /// Height (in pixels) of the source image.  If the height is not
        /// an even multiple of the MCU block height (see <c>tjMCUHeight</c>), then an
        /// intermediate buffer copy will be performed within TurboJPEG.
        /// </param>
        /// <param name="subsamp">
        /// The level of chrominance subsampling used in the source image.
        /// </param>
        /// <param name="jpegBuf">
        /// address of a pointer to an image buffer that will receive the
        /// JPEG image.  TurboJPEG has the ability to reallocate the JPEG buffer to
        /// accommodate the size of the JPEG image.  Thus, you can choose to:
        /// <list type="number">
        ///   <item>
        ///     pre-allocate the JPEG buffer with an arbitrary size using <see cref="TjAlloc(int)"/> and
        ///     let TurboJPEG grow the buffer as needed,
        ///   </item>
        ///   <item>
        ///     set <paramref name="jpegBuf"/> to <see langword="null"/> to tell TurboJPEG
        ///     to allocate the buffer for you, or
        ///   </item>
        ///   <item>
        ///     pre-allocate the buffer to a "worst case" size determined by calling
        ///     <see cref="TjBufSize(int, int, int)"/>.   This should ensure that the buffer never has to be
        ///     re-allocated (setting <see cref="TJFlags.NoRealloc"/> guarantees that it won't be.)
        ///   </item>
        /// </list>
        /// If you choose option 1, <paramref name="jpegSize"/> should be set to the size of your
        /// pre-allocated buffer.  In any case, unless you have set <see cref="TJFlags.NoRealloc"/>,
        /// you should always check <tt>*jpegBuf</tt> upon return from this function, as
        /// it may have changed.
        /// </param>
        /// <param name="jpegSize">
        /// pointer to an unsigned long variable that holds the size of
        /// the JPEG image buffer.  If <paramref name="jpegBuf"/> points to a pre-allocated
        /// buffer, then <paramref name="jpegSize"/> should be set to the size of the buffer.
        /// Upon return, <paramref name="jpegSize"/> will contain the size of the JPEG image (in
        /// bytes.)  If <paramref name="jpegBuf"/> points to a JPEG image buffer that is being
        /// reused from a previous call to one of the JPEG compression functions, then
        /// <paramref name="jpegSize"/> is ignored.
        /// </param>
        /// <param name="jpegQual">
        /// the image quality of the generated JPEG image (1 = worst, 100 = best).
        /// </param>
        /// <param name="flags">
        /// the bitwise OR of one or more of the <see cref="TJFlags"/> flags.
        /// </param>
        /// <returns>
        /// 0 if successful, or -1 if an error occurred.
        /// </returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjCompressFromYUVPlanes")]
        public static unsafe extern int TjCompressFromYUVPlanes(
            IntPtr handle,
            byte** srcPlanes,
            int width,
            int* strides,
            int height,
            int subsamp,
            ref IntPtr jpegBuf,
            ref nuint jpegSize,
            int jpegQual,
            int flags);

        /// <summary>
        /// Destroy a TurboJPEG compressor, decompressor, or transformer instance.
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG compressor, decompressor or transformer instance.</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDestroy")]
        public static extern int TjDestroy(IntPtr handle);

        /// <summary>
        /// Returns a descriptive error message explaining why the last command failed.
        /// </summary>
        /// <returns>A descriptive error message explaining why the last command failed.</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjGetErrorStr")]
        public static extern IntPtr TjGetErrorStr();

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
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjPlaneSizeYUV")]
        public static extern nuint TjPlaneSizeYUV(TJPlane componentID, int width, int stride, int height, int subsamp);

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
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjPlaneWidth")]
        public static extern int TjPlaneWidth(TJPlane componentID, int width, int subsamp);

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
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjPlaneHeight")]
        public static extern int TjPlaneHeight(TJPlane componentID, int height, int subsamp);
    }
}
