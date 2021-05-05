// <copyright file="TJUtils.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// Helper method for TurboJpeg code.
    /// </summary>
    internal static class TJUtils
    {
        /// <summary>
        /// Retrieves last error from underlying turbo-jpeg library and throws exception.</summary>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails. </exception>
        public static void GetErrorAndThrow()
        {
            var error = Marshal.PtrToStringAnsi(TurboJpegImport.TjGetErrorStr());
            throw new TJException(error!);
        }

        /// <summary>
        /// Converts array of managed structures to the unmanaged pointer.
        /// </summary>
        /// <typeparam name="T">
        /// The type of struct to convert.
        /// </typeparam>
        /// <param name="structArray">
        /// An arrey of structs of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// A <see cref="IntPtr"/> which points to an unmanaged copy of the structs.
        /// </returns>
        public static IntPtr StructArrayToIntPtr<T>(T[] structArray)
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var result = Marshal.AllocHGlobal(structArray.Length * structSize);
            var longPtr = result.ToInt64(); // Must work both on x86 and x64
            foreach (var s in structArray)
            {
                var structPtr = new IntPtr(longPtr);
                Marshal.StructureToPtr(s!, structPtr, false); // You do not need to erase struct in this case
                longPtr += structSize;
            }

            return result;
        }

        /// <summary>
        /// Allocate an image buffer for use with TurboJPEG.
        /// </summary>
        /// <param name="bytes">The number of bytes to allocate.</param>
        /// <returns>A pointer to a newly-allocated buffer with the specified number of bytes.</returns>
        /// <seealso cref="Free"/>
        public static IntPtr Alloc(int bytes) => TurboJpegImport.TjAlloc(bytes);

        /// <summary>
        /// Free an image buffer previously allocated by TurboJPEG.
        /// </summary>
        /// <param name="buffer">Address of the buffer to free.</param>
        /// <seealso cref="Alloc"/>
        public static void Free(IntPtr buffer) => TurboJpegImport.TjFree(buffer);

        /// <summary>
        /// Frees unmanaged pointer using allocator.
        /// </summary>
        /// <param name="ptr">
        /// A pointer to the memory to free.
        /// </param>
        public static void FreePtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            Marshal.FreeHGlobal(ptr);
        }
    }
}
