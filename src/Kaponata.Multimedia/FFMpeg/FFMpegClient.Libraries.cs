// <copyright file="FFMpegClient.Libraries.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using FFmpeg.AutoGen;
using FFmpeg.AutoGen.Native;
using FFmpeg.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Kaponata.Multimedia.FFMpeg
{
    /// <summary>
    /// Contains the methods used to load the ffmpeg libraries.
    /// </summary>
    public partial class FFMpegClient
    {
        /// <summary>
        /// Contains the handles of the native FFmpeg libraries loaded into memory. The key is the name of the
        /// library, without any prefix or suffix - e.g. <c>avutil</c>.
        /// </summary>
        private static Dictionary<string, IntPtr> libraryHandles = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Tracks whether the libraries have already been loaded.
        /// </summary>
        private static bool librariesLoaded = false;

        /// <summary>
        /// Initializes static members of the <see cref="FFMpegClient"/> class.
        /// </summary>
        public static void Initialize()
        {
            lock (libraryHandles)
            {
                ffmpeg.GetOrLoadLibrary = GetOrLoadLibrary;

#pragma warning disable CS0618 // Type or member is obsolete
                ffmpeg.av_register_all();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        /// <summary>
        /// Loads the native FFmpeg libraries and populates the <see cref="libraryHandles"/> dictionary.
        /// </summary>
        public static void LoadLibraries()
        {
            if (librariesLoaded)
            {
                return;
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Try to load the libraries from a shared location. Requires libavformat-dev and friends
                var libraryNames = new string[] { "avutil", "avcodec", "avformat", "swscale", "swresample" };

                foreach (var libraryName in libraryNames)
                {
                    var libraryHandle = NativeLibrary.Load($"lib{libraryName}.so");
                    libraryHandles.Add(libraryName, libraryHandle);
                }

                librariesLoaded = true;
                return;
            }

            var path = FFmpegBinaries.FindFFmpegLibrary("avutil");

            if (path == null)
            {
                librariesLoaded = true;
                return;
            }

            // Normalize names
            path = Path.GetFullPath(path);

            var directory = Path.GetDirectoryName(path);

            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory), "$Cannot find the ffmpeg directory. Using {path} to find the ffmpeg libraries.");
            }

            // Make sure we load libwinpthread-1.dll, too
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var pthread = Path.Combine(directory, "libwinpthread-1.dll");
                var pthreadHandle = LibraryLoader.LoadNativeLibrary(pthread);
                libraryHandles.Add("winpthread", pthreadHandle);
            }

            // And libkvazaar
            string? kvazaarName = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                kvazaarName = "libkvazaar-3.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                kvazaarName = "libkvazaar.3.dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                kvazaarName = "libkvazaar.so.3";
            }
            else
            {
                throw new InvalidOperationException($"Unknown os platfrom: {RuntimeInformation.OSDescription}");
            }

            var kvazaar = Path.Combine(directory, kvazaarName);
            var kvazaarHandle = LibraryLoader.LoadNativeLibrary(kvazaar);
            libraryHandles.Add("libkvazaar", kvazaarHandle);

            var librariesToLoad = new string[] { "avutil", "avcodec", "avformat", "swscale", "swresample" };
            foreach (var libraryToLoad in librariesToLoad)
            {
                var libraryPath = FFmpegBinaries.FindFFmpegLibrary(libraryToLoad);
                libraryPath = Path.GetFullPath(libraryPath);

                var libraryHandle = NativeLibrary.Load(libraryPath);
                libraryHandles.Add(libraryToLoad, libraryHandle);
            }

            librariesLoaded = true;
        }

        /// <summary>
        /// Loads a native library into memory, and returns a handle to that library.
        /// </summary>
        /// <param name="libraryName">
        /// The name of the library to load.
        /// </param>
        /// <returns>
        /// A handle to the native library, or a null pointer on error.
        /// </returns>
        public static IntPtr GetOrLoadLibrary(string libraryName)
        {
            LoadLibraries();

            if (!libraryHandles.ContainsKey(libraryName))
            {
                return IntPtr.Zero;
            }

            return libraryHandles[libraryName];
        }
    }
}
