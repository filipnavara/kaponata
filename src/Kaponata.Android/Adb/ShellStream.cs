// <copyright file="ShellStream.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Android.Adb
{
    /// <summary>
    /// <para>
    /// Represents a <see cref="Stream"/> that wraps around an inner <see cref="Stream"/> that contains
    /// output from an Android shell command. In the shell output, the LF character is replaced by a
    /// CR LF character. This stream undoes that change.
    /// </para>
    /// </summary>
    /// <seealso href="http://stackoverflow.com/questions/13578416/read-binary-stdout-data-from-adb-shell"/>
    public class ShellStream : Stream
    {
        private byte? pendingByte;

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ShellStream"/> class.
        /// </summary>
        /// <param name="inner">
        /// The inner stream that contains the raw data retrieved from the shell. This stream
        /// must be readable.
        /// </param>
        public ShellStream(Stream inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            if (!inner.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(inner));
            }

            this.Inner = inner;
        }

        /// <summary>
        /// Gets the inner stream from which data is being read.
        /// </summary>
        public Stream Inner
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotImplementedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken = default)
        {
            return await this.ReadAsync(buffer.AsMemory().Slice(offset, length), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            // Read the raw data from the base stream. There may be a
            // 'pending byte' from a previous operation; if that's the case,
            // consume it.
            int read = 0;

            if (this.pendingByte != null)
            {
                buffer.Span[0] = this.pendingByte.Value;
                read = await this.Inner.ReadAsync(buffer.Slice(1, buffer.Length - 1), cancellationToken).ConfigureAwait(false);
                read++;
                this.pendingByte = null;
            }
            else
            {
                read = await this.Inner.ReadAsync(buffer.Slice(0, buffer.Length), cancellationToken).ConfigureAwait(false);
            }

            byte[] minibuffer = new byte[1];

            // Loop over the data, and find a LF (0x0d) character. If it is
            // followed by a CR (0x0a) character, remove the LF chracter and
            // keep only the LF character intact.
            for (int i = 0; i < read - 1; i++)
            {
                if (buffer.Span[i] == 0x0d && buffer.Span[i + 1] == 0x0a)
                {
                    buffer.Span[i] = 0x0a;

                    for (int j = i + 1; j < read - 1; j++)
                    {
                        buffer.Span[j] = buffer.Span[j + 1];
                    }

                    // Reset unused data to \0
                    buffer.Span[read - 1] = 0;

                    // We have removed one byte from the array of bytes which has
                    // been read; but the caller asked for a fixed number of bytes.
                    // So we need to get the next byte from the base stream.
                    // If less bytes were received than asked, we know no more data is
                    // available so we can skip this step
                    if (read < buffer.Length)
                    {
                        read--;
                        continue;
                    }

                    int miniRead = await this.Inner.ReadAsync(minibuffer, 0, 1, cancellationToken).ConfigureAwait(false);

                    if (miniRead == 0)
                    {
                        // If no byte was read, no more data is (currently) available, and reduce the
                        // number of bytes by 1.
                        read--;
                    }
                    else
                    {
                        // Append the byte to the buffer.
                        buffer.Span[read - 1] = minibuffer[0];
                    }
                }
            }

            // The last byte is a special case, to find out if the next byte is 0x0a
            // we need to read one more byte from the inner stream.
            if (read > 0 && buffer.Span[read - 1] == 0x0d)
            {
                int miniRead = await this.Inner.ReadAsync(minibuffer, 0, 1, cancellationToken).ConfigureAwait(false);
                int nextByte = minibuffer[0];

                if (nextByte == 0x0a)
                {
                    // If the next byte is 0x0a, set the last byte to 0x0a. The underlying
                    // stream has already advanced because of the ReadByte call, so all is good.
                    buffer.Span[read - 1] = 0x0a;
                }
                else
                {
                    // If the next byte was not 0x0a, store it as the 'pending byte' --
                    // the next read operation will fetch this byte. We can't do a Seek here,
                    // because e.g. the network stream doesn't support seeking.
                    this.pendingByte = (byte)nextByte;
                }
            }

            return read;
        }

        /// <inheritdoc/>
        public override void Flush() => throw new NotImplementedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.Inner != null)
            {
                this.Inner.Dispose();
                this.Inner = null;
            }

            this.Inner = null;
            base.Dispose(disposing);
        }
    }
}
