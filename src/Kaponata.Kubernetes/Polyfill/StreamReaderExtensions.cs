// <copyright file="StreamReaderExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Kubernetes.Polyfill
{
    /// <summary>
    /// Provides extension methods for the <see cref="StreamReader"/> class.
    /// </summary>
    public static class StreamReaderExtensions
    {
        /// <summary>
        /// Asynchronously read a line, or returns <see langword="null"/> when the operation is being
        /// cancelled.
        /// </summary>
        /// <param name="reader">
        /// The reader from which to read the data.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation;
        /// after which the method will return <see langword="null"/>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<string?> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string?>();
            cancellationToken.Register(() => tcs.SetResult(null));

            var completedTask = await Task.WhenAny(reader.ReadLineAsync(), tcs.Task).ConfigureAwait(false);
            return await completedTask.ConfigureAwait(false);
        }
    }
}
