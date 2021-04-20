// <copyright file="TarReader.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Nerdbank.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.FileFormats.Tar
{
    /// <summary>
    /// Supports forward-only read access to Tar archives.
    /// </summary>
    public class TarReader
    {
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[512];
        private long nextHeaderOffset = 0;
        private Stream? childStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="TarReader" /> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which provides access to the underlying tar archive.
        /// </param>
        public TarReader(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Asynchronously reads an entry in the tar archive.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation. This task returns a <see cref="TarHeader"/> object
        /// which represents the header for the entry, and a <see cref="Stream"/> which provides forward-only access to the
        /// entry. This <see cref="Stream"/> is disposed of when <see cref="ReadAsync(CancellationToken)"/> is invoked subsequently.
        /// </returns>
        public async Task<(TarHeader? header, Stream? entryStream)> ReadAsync(CancellationToken cancellationToken)
        {
            if (this.childStream != null)
            {
                await this.childStream.DisposeAsync();
            }

            this.stream.Seek(this.nextHeaderOffset, SeekOrigin.Begin);

            if (await this.stream.ReadBlockAsync(this.buffer, cancellationToken) != this.buffer.Length)
            {
                return (null, null);
            }

            var header = TarHeader.Read(this.buffer);
            this.nextHeaderOffset = Align(512, this.stream.Position + header.FileSize);

            this.childStream = this.stream.ReadSlice(header.FileSize);

            return (header, this.childStream);
        }

        private static long Align(int multiple, long value)
        {
            if (value % multiple == 0)
            {
                return value;
            }
            else
            {
                return value + multiple - (value % multiple);
            }
        }
    }
}
