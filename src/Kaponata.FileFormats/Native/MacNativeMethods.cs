// <copyright file="MacNativeMethods.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace Packaging.Targets.Native
{
    /// <summary>
    /// Native methods which can be used for function loading on macOS.
    /// </summary>
    internal static class MacNativeMethods
    {
        /// <summary>
        /// If this value is specified, or the environment variable LD_BIND_NOW is set to a nonempty string,
        /// all undefined symbols in the library are resolved before dlopen() returns. If this cannot be done,
        /// an error is returned.
        /// </summary>
        public const int RTLD_NOW = 0x002;

        private const string Libdl = "libdl";

        /// <summary>
        /// The function dlsym() takes a "handle" of a dynamic library returned by <see cref="dlopen(string, int)"/>
        /// and the null-terminated symbol name, returning the address where that symbol is loaded into memory.
        /// If the symbol is not found, in the specified library or any of the libraries that were automatically
        /// loaded by dlopen() when that library was loaded, dlsym() returns <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <param name="handle">
        /// The handle of a dynamic library returned by <see cref="dlopen(string, int)"/>.
        /// </param>
        /// <param name="symbol">
        /// The null-terminated symbol name.
        /// </param>
        /// <returns>
        /// The address where that symbol is loaded into memory.
        /// </returns>
        [DllImport(Libdl)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        /// <summary>
        /// The function dlopen() loads the dynamic library file named by the null-terminated string filename and
        /// returns an opaque "handle" for the dynamic library.
        /// </summary>
        /// <param name="fileName">
        /// The location of the library to load.
        /// </param>
        /// <param name="flag">
        /// Additional flags which influence the load behavior.
        /// </param>
        /// <returns>
        /// An opaque handle for the dynamic library.
        /// </returns>
        [DllImport(Libdl)]
        public static extern IntPtr dlopen(string fileName, int flag);
    }
}
