// <copyright file="LibraryResolver.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Kaponata.TurboJpeg
{
    /// <summary>
    /// Provides helper methods for resolving the native libraries.
    /// </summary>
    internal static class LibraryResolver
    {
        static LibraryResolver()
        {
            NativeLibrary.SetDllImportResolver(typeof(LibraryResolver).Assembly, DllImportResolver);
        }

        /// <summary>
        /// Ensures the library resolver is registered. This is a dummy method used to trigger the static constructor.
        /// </summary>
        public static void EnsureRegistered()
        {
            // Dummy call to trigger the static constructor
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != TurboJpegImport.UnmanagedLibrary)
            {
                return IntPtr.Zero;
            }

            IntPtr lib;
            string nativeLibraryName;

            // On Debian & Ubuntu, there are two out-of-the-box packages:
            // - libjpeg-turbo8 is the libjpeg-turbo, masquerading as the
            //   default jpeg library. It installs as libjpeg.so.8 and is
            //   usually a slightly outdated version. Additionally, it also
            //   does not export most of the tj* functions.
            // - libturbojpeg is the same library, but usually a more
            //   recent version. It installs as libturbojpeg.so.0
            //
            // On CentOS, the same applies, but the names are:
            // libjpeg.so.62
            // libturbojpeg.so.0
            //
            // Require the specialized version.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                nativeLibraryName = "turbojpeg.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                nativeLibraryName = "libturbojpeg.so.0";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                nativeLibraryName = "libturbojpeg.0.dylib";
            }
            else
            {
                return IntPtr.Zero;
            }

            // First, attempt to load the native library from the NuGet packages
            var nativeSearchDirectories = AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") as string;
            var delimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ";" : ":";

            if (nativeSearchDirectories != null)
            {
                foreach (var directory in nativeSearchDirectories.Split(delimiter))
                {
                    var path = Path.Combine(directory, nativeLibraryName);
                    if (NativeLibrary.TryLoad(path, out lib))
                    {
                        return lib;
                    }
                }
            }

            // Next, try to load any OS-provided version of the library
            if (NativeLibrary.TryLoad(nativeLibraryName, out lib))
            {
                return lib;
            }

            return IntPtr.Zero;
        }
    }
}