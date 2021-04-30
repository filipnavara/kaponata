//-----------------------------------------------------------------------
// <copyright file="CpioFile.cs" company="Quamotion">
//     Copyright (c) 2016 Quamotion bv. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Nerdbank.Streams;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.FileFormats.Cpio
{
    /// <summary>
    /// The cpio archive format collects any number of files, directories, and
    /// other file system objects(symbolic links, device nodes, etc.) into a
    /// single stream of bytes.
    /// </summary>
    /// <remarks>
    /// Each file system object in a cpio archive comprises a header record with
    /// basic numeric metadata followed by the full pathname of the entry and the
    /// file data.The header record stores a series of integer values that generally
    /// follow the fields in struct stat.  (See stat(2) for details.)  The
    /// variants differ primarily in how they store those integers(binary,
    /// octal, or hexadecimal).  The header is followed by the pathname of the
    /// entry(the length of the pathname is stored in the header) and any file
    /// data.The end of the archive is indicated by a special record with the
    /// pathname <c>TRAILER!!!</c>.
    /// </remarks>
    /// <seealso href="https://people.freebsd.org/~kientzle/libarchive/man/cpio.5.txt"/>
    public class CpioFile : IDisposable
    {
        /// <summary>
        /// The <see cref="Stream"/> around which this <see cref="CpioFile"/> wraps.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Indicates whether <see cref="stream"/> should be disposed of when this <see cref="CpioFile"/> is disposed of.
        /// </summary>
        private readonly bool leaveOpen;

        private long nextHeaderOffset = 0;
        private Stream? childStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="CpioFile"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the CPIO data.
        /// </param>
        /// <param name="leaveOpen">
        /// <see langword="true"/> to leave the underlying <paramref name="stream"/> open when this <see cref="CpioFile"/>
        /// is disposed of; otherwise, <see langword="false"/>.
        /// </param>
        public CpioFile(Stream stream, bool leaveOpen)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.stream = stream;
            this.leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Reads the next entry in the <see cref="CpioFile"/>.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if more data is available; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<(CpioHeader? header, string? name, Stream? childStream)> ReadAsync(CancellationToken cancellationToken)
        {
            if (this.childStream != null)
            {
                if (!this.stream.CanSeek)
                {
                    var buffer = new byte[1024];
                    while ((await this.childStream.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        // Keep seeking
                    }
                }

                await this.childStream.DisposeAsync();
            }

            if (this.stream.CanSeek)
            {
                this.stream.Seek(this.nextHeaderOffset, SeekOrigin.Begin);
            }

            CpioHeader header = default;
            string? name = null;
            this.childStream = null;

            using (var headerBuffer = MemoryPool<byte>.Shared.Rent(CpioHeader.Size))
            {
                await this.stream.ReadBlockOrThrowAsync(headerBuffer.Memory.Slice(0, CpioHeader.Size), cancellationToken).ConfigureAwait(false);
                header.ReadFrom(headerBuffer.Memory.Span);

                if (header.Signature != 0x71C7 /* 070707 in octal */)
                {
                    throw new InvalidDataException("The magic for the file entry is invalid");
                }

                using (var nameBuffer = MemoryPool<byte>.Shared.Rent((int)header.Namesize))
                {
                    var nameMemory = nameBuffer.Memory.Slice(0, (int)header.Namesize);
                    await this.stream.ReadBlockAsync(nameMemory, cancellationToken).ConfigureAwait(false);
                    name = Encoding.UTF8.GetString(nameMemory.Slice(0, nameMemory.Length - 1).Span);
                }

                if (this.stream.CanSeek)
                {
                    this.nextHeaderOffset = this.stream.Position + header.Filesize;
                }

                this.childStream = this.stream.ReadSlice(header.Filesize);
            }

            if (name == "TRAILER!!!")
            {
                return (null, null, null);
            }
            else if (header.Filesize > 0)
            {
                return (header, name, this.childStream);
            }
            else
            {
                return (header, name, null);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.stream.Dispose();
            }
        }
    }
}
