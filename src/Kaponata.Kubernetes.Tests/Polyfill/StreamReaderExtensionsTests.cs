// <copyright file="StreamReaderExtensionsTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Operator.Kubernetes.Polyfill;
using Nerdbank.Streams;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Operator.Tests.Kubernetes.Polyfill
{
    /// <summary>
    /// Tests the <see cref="StreamReaderExtensions"/> class.
    /// </summary>
    public class StreamReaderExtensionsTests
    {
        /// <summary>
        /// <see cref="StreamReaderExtensions.ReadLineAsync(StreamReader, CancellationToken)"/> returns <see langword="null"/>
        /// when the task has been cancelled.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task Read_Cancelled_ReturnsNull_Async()
        {
            var stream = new SimplexStream();

            using (var streamReader = new StreamReader(stream))
            {
                var cts = new CancellationTokenSource();
                var task = streamReader.ReadLineAsync(cts.Token);
                cts.Cancel();

                Assert.Null(await task.ConfigureAwait(false));
            }
        }

        /// <summary>
        /// <see cref="StreamReaderExtensions.ReadLineAsync(StreamReader, CancellationToken)"/> returns the underlying data
        /// when data is available.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadLine_HasData_ReturnsValue_Async()
        {
            var stream = new SimplexStream();

            using (var streamReader = new StreamReader(stream))
            using (var streamWriter = new StreamWriter(stream))
            {
                var cts = new CancellationTokenSource();
                var task = streamReader.ReadLineAsync(cts.Token);

                await streamWriter.WriteLineAsync("Hello, World!".ToCharArray(), default).ConfigureAwait(false);
                await streamWriter.FlushAsync().ConfigureAwait(false);

                Assert.Equal("Hello, World!", await task.ConfigureAwait(false));
            }
        }
    }
}
