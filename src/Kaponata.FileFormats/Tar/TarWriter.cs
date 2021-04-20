// <copyright file="TarWriter.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.FileFormats.Tar
{
    /// <summary>
    /// Enables the creation of tar archives.
    /// </summary>
    public class TarWriter
    {
        private readonly byte[] headerBuffer = new byte[512];
        private readonly Stream tarStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="TarWriter"/> class.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream"/> to which to write the tar data.
        /// </param>
        public TarWriter(Stream stream)
        {
            this.tarStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Asynchronously adds a file to the tar archive.
        /// </summary>
        /// <param name="filename">
        /// The name of the file to add.
        /// </param>
        /// <param name="entryStream">
        /// A <see cref="Stream"/> which represents the data to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task AddFileAsync(string filename, Stream entryStream, CancellationToken cancellationToken)
        {
            return this.AddFileAsync(
                filename,
                LinuxFileMode.S_IROTH | LinuxFileMode.S_IRGRP | LinuxFileMode.S_IWUSR | LinuxFileMode.S_IRUSR | LinuxFileMode.S_IFREG,
                DateTimeOffset.Now,
                entryStream,
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously adds a file to the tar archive.
        /// </summary>
        /// <param name="filename">
        /// The name of the file to add.
        /// </param>
        /// <param name="fileMode">
        /// The <see cref="LinuxFileMode"/> of the file to add.
        /// </param>
        /// <param name="lastModified">
        /// The date and time at which the file was last modified.
        /// </param>
        /// <param name="entryStream">
        /// A <see cref="Stream"/> which represents the data to add.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddFileAsync(string filename, LinuxFileMode fileMode, DateTimeOffset lastModified, Stream entryStream, CancellationToken cancellationToken)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (entryStream == null)
            {
                throw new ArgumentNullException(nameof(entryStream));
            }

            var header = new TarHeader()
            {
                FileName = filename,
                FileMode = fileMode,
                UserId = 0,
                GroupId = 0,
                FileSize = (uint)entryStream.Length,
                LastModified = lastModified,
                TypeFlag = TarTypeFlag.RegType,
                LinkName = string.Empty,
                Magic = "ustar\0",
                Version = 0,
                UserName = string.Empty,
                GroupName = string.Empty,
                DevMajor = 0,
                DevMinor = 0,
                Prefix = string.Empty,
            };

            header.Write(this.headerBuffer);

            // Write the header for the current tar entry
            await this.tarStream.WriteAsync(this.headerBuffer, cancellationToken).ConfigureAwait(false);

            // Write the actual entry
            await entryStream.CopyToAsync(this.tarStream, cancellationToken);

            // Align the stream
            if (entryStream.Length % 512 != 0)
            {
                var length = 512 - ((int)entryStream.Length % 512);
                var buffer = this.headerBuffer.AsMemory(0, length);

                buffer.Span.Clear();
                await this.tarStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            }

            await this.tarStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes the the trailer for a tar archive.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WriteTrailerAsync(CancellationToken cancellationToken)
        {
            this.headerBuffer.AsSpan().Clear();
            await this.tarStream.WriteAsync(this.headerBuffer, cancellationToken).ConfigureAwait(false);
            await this.tarStream.WriteAsync(this.headerBuffer, cancellationToken).ConfigureAwait(false);
            await this.tarStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
