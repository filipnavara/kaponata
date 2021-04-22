// <copyright file="FunctionLoader.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

#nullable disable

namespace Kaponata.FileFormats.Native
{
    /// <summary>
    /// Supports loading functions from native libraries. Provides a more flexible alternative to P/Invoke.
    /// </summary>
    public static class FunctionLoader
    {
        /// <summary>
        /// Attempts to load a native library.
        /// </summary>
        /// <param name="windowsNames">
        /// Possible names of the library on Windows.
        /// </param>
        /// <param name="linuxNames">
        /// Possible names of the library on Linux.
        /// </param>
        /// <param name="osxNames">
        /// Possible names of the library on macOS.
        /// </param>
        /// <returns>
        /// A handle to the library when found; otherwise, <see cref="IntPtr.Zero"/>.
        /// </returns>
        public static IntPtr LoadNativeLibrary(IEnumerable<string> windowsNames, IEnumerable<string> linuxNames, IEnumerable<string> osxNames)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return LoadNativeLibrary(windowsNames);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LoadNativeLibrary(linuxNames);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return LoadNativeLibrary(osxNames);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        /// <summary>
        /// Attempts to load a native library.
        /// </summary>
        /// <param name="libraryNames">
        /// Possible names of the library on the current platform.
        /// </param>
        /// <returns>
        /// A handle to the library when found; otherwise, <see cref="IntPtr.Zero"/>.
        /// </returns>
        public static IntPtr LoadNativeLibrary(IEnumerable<string> libraryNames)
        {
            IntPtr lib = IntPtr.Zero;

            // First, attempt to load the native library from the NuGet packages
            var nativeSearchDirectories = AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") as string;
            var delimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";

            if (nativeSearchDirectories != null)
            {
                foreach (var name in libraryNames)
                {
                    foreach (var directory in nativeSearchDirectories.Split(delimiter))
                    {
                        var path = Path.Combine(directory, name);
                        if (NativeLibrary.TryLoad(path, out lib))
                        {
                            break;
                        }
                    }
                }
            }

            // Next, attempt to locate the library in the standard OS search path.
            foreach (var name in libraryNames)
            {
                if (NativeLibrary.TryLoad(name, out lib))
                {
                    break;
                }
            }

            // This function may return a null handle. If it does, individual functions loaded from it will throw a DllNotFoundException,
            // but not until an attempt is made to actually use the function (rather than load it). This matches how PInvokes behave.
            return lib;
        }

        /// <summary>
        /// Creates a delegate which invokes a native function.
        /// </summary>
        /// <typeparam name="T">
        /// The function delegate.
        /// </typeparam>
        /// <param name="nativeLibraryHandle">
        /// The native library which contains the function.
        /// </param>
        /// <param name="functionName">
        /// The name of the function for which to create the delegate.
        /// </param>
        /// <param name="throwOnError">
        /// A value indicating whether to throw when the operation fails.
        /// </param>
        /// <returns>
        /// A new delegate which points to the native function.
        /// </returns>
        public static T LoadFunctionDelegate<T>(IntPtr nativeLibraryHandle, string functionName, bool throwOnError = true)
            where T : class
        {
            if (!NativeLibrary.TryGetExport(nativeLibraryHandle, functionName, address: out IntPtr address))
            {
                if (throwOnError)
                {
                    throw new EntryPointNotFoundException($"Could not find the entrypoint for {functionName}");
                }
                else
                {
                    return null;
                }
            }

            return Marshal.GetDelegateForFunctionPointer<T>(address);
        }
    }
}
