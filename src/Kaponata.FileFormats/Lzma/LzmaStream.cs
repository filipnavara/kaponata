// <copyright file="LzmaStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace Kaponata.FileFormats.Lzma
{
    /// <summary>
    ///  Passing data to and from liblzma.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The lzma_stream structure is used for
    ///  - passing pointers to input and output buffers to liblzma;
    ///  - defining custom memory hander functions; and
    ///  - holding a pointer to coder-specific internal data structures.
    /// </para>
    /// <para>
    /// Typical usage:
    /// </para>
    /// <para>
    ///  - After allocating <see cref="LzmaStream"/> (on stack or with malloc()), it must be
    ///    initialized to LZMA_STREAM_INIT (see LZMA_STREAM_INIT for details).
    /// </para>
    /// <para>
    ///  - Initialize a coder to the lzma_stream, for example by using
    ///    lzma_easy_encoder() or lzma_auto_decoder(). Some notes:
    ///      - In contrast to zlib, <see cref="LzmaStream.NextIn"/> and <see cref="LzmaStream.NextOut"/> are
    ///        ignored by all initialization functions, thus it is safe
    ///        to not initialize them yet.
    ///      - The initialization functions always set strm->total_in and
    ///        strm->total_out to zero.
    ///      - If the initialization function fails, no memory is left allocated
    ///        that would require freeing with <see cref="NativeMethods.lzma_end(ref LzmaStream)"/> even if some memory was
    ///        associated with the <see cref="LzmaStream"/> structure when the initialization
    ///        function was called.
    /// </para>
    /// <para>
    ///  - Use <see cref="NativeMethods.lzma_code(ref LzmaStream, LzmaAction)"/> to do the actual work.
    /// </para>
    /// <para>
    ///  - Once the coding has been finished, the existing lzma_stream can be
    ///    reused. It is OK to reuse <see cref="LzmaStream"/> with different initialization
    ///    function without calling <see cref="NativeMethods.lzma_end(ref LzmaStream)"/> first. Old allocations are
    ///    automatically freed.
    /// </para>
    /// <para>
    ///  - Finally, use <see cref="NativeMethods.lzma_end(ref LzmaStream)"/> to free the allocated memory. <see cref="NativeMethods.lzma_end(ref LzmaStream)"/> never
    ///    frees the <see cref="LzmaStream"/> structure itself.
    /// </para>
    /// <para>
    /// Application may modify the values of <see cref="TotalIn"/> and <see cref="TotalOut"/> as it wants.
    /// They are updated by liblzma to match the amount of data read and
    /// written, but aren't used for anything else.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LzmaStream
    {
        /// <summary>
        /// Pointer to the next input byte.
        /// </summary>
        public byte* NextIn;

        /// <summary>
        /// Number of available input bytes in next_in.
        /// </summary>
        public nuint AvailIn;

        /// <summary>
        /// Total number of bytes read by liblzma.
        /// </summary>
        public ulong TotalIn;

        /// <summary>
        /// Pointer to the next output position.
        /// </summary>
        public byte* NextOut;

        /// <summary>
        /// Amount of free space in next_out.
        /// </summary>
        public nuint AvailOut;

        /// <summary>
        /// Total number of bytes written by liblzma.
        /// </summary>
        public ulong TotalOut;

        /// <summary>
        /// Custom memory allocation functions.
        /// </summary>
        /// <remarks>
        /// In most cases this is NULL which makes liblzma use
        /// the standard malloc() and free().
        /// </remarks>
        public void* Allocator;

        /// <summary>
        /// Internal state is not visible to applications.
        /// </summary>
#pragma warning disable SA1214 // Readonly fields must appear before non-readonly fields
        public readonly void* InternalState;

        /// <summary>
        /// This is the first pointer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly void* ReservedPtr1;

        /// <summary>
        /// This is the second pointer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly void* ReservedPtr2;

        /// <summary>
        /// This is the third pointer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly void* ReservedPtr3;

        /// <summary>
        /// This is the fourth pointer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly void* ReservedPtr4;

        /// <summary>
        /// New seek input position for <see cref="LzmaResult.SeekNeeded"/>.
        /// </summary>
        public ulong SeekPos;

        /// <summary>
        /// This is the second integer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly ulong ReservedInt2;

        /// <summary>
        /// This is the third integer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly nuint ReservedInt3;

        /// <summary>
        /// This is the fourth integer field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly nuint ReservedInt4;

        /// <summary>
        /// This is the first enum field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly uint ReservedEnum1;

        /// <summary>
        /// This is the second enum field of the reserved space of the <see cref="LzmaStream"/> struct.
        /// </summary>
        public readonly uint ReservedEnum2;
    }
}