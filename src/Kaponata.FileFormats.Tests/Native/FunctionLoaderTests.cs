// <copyright file="FunctionLoaderTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Packaging.Targets.Native;
using System;
using Xunit;

namespace Kaponata.FileFormats.Tests.Native
{
    /// <summary>
    /// Tests the <see cref="FunctionLoader"/> class.
    /// </summary>
    public class FunctionLoaderTests
    {
        /// <summary>
        /// <see cref="FunctionLoader.LoadNativeLibrary(System.Collections.Generic.IEnumerable{string})"/>
        /// returns a zero pointer when the library is missing.
        /// </summary>
        [Fact]
        public void LoadNativeLibrary_Missing_ReturnsZero()
        {
            Assert.Equal(IntPtr.Zero, FunctionLoader.LoadNativeLibrary(new string[] { string.Empty }));
        }

        /// <summary>
        /// <see cref="FunctionLoader.LoadFunctionDelegate{T}(IntPtr, string, bool)"/> throws
        /// when the function could not be found.
        /// </summary>
        [Fact]
        public void LoadFunctionDelegate_ThrowsOnError()
        {
            IntPtr library = FunctionLoader.LoadNativeLibrary(
                new string[] { "libzma.dll", "lzma.dll" },
                new string[] { "liblzma.so.5", "liblzma.so" },
                new string[] { "liblzma.dylib" });

            Assert.Throws<EntryPointNotFoundException>(() => FunctionLoader.LoadFunctionDelegate<Action>(library, string.Empty, true));
        }


        /// <summary>
        /// <see cref="FunctionLoader.LoadFunctionDelegate{T}(IntPtr, string, bool)"/> 
        /// returns <see langword="null"/> when the function could not be found.
        /// </summary>
        [Fact]
        public void LoadFunctionDelegate_ReturnsNullOnError()
        {
            IntPtr library = FunctionLoader.LoadNativeLibrary(
                new string[] { "libzma.dll", "lzma.dll" },
                new string[] { "liblzma.so.5", "liblzma.so" },
                new string[] { "liblzma.dylib" });

            Assert.Null(FunctionLoader.LoadFunctionDelegate<Action>(library, string.Empty, false));
        }
    }
}
