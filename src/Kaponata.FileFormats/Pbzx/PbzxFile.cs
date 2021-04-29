// <copyright file="PbzxFile.cs" company="Quamotion bv">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using Nerdbank.Streams;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.FileFormats.Pbzx
{
    /// <summary>
    /// Provides methods for working with a pbzx file. The pbzx file format embeds an xz file in chunks.
    /// </summary>
    /// <seealso href="http://newosxbook.com/articles/OTA.html"/>
    /// <seealso href="http://newosxbook.com/src.jl?tree=listings&amp;file=pbzx.c"/>
    public static class PbzxFile
    {
        /// <summary>
        /// The magic for the PBZX file.
        /// </summary>
        private const int Magic = 0x787a6270; // xzbp

        /// <summary>
        /// The header of a xz chunk.
        /// </summary>
        private const int ZxHeader = 0x587a37fd;

        /// <summary>
        /// The footer of a xz chunk.
        /// </summary>
        private const short ZxFooter = 0x5a59;

        /// <summary>
        /// Unpacks the xz stream contained in a pbzx file.
        /// </summary>
        /// <param name="input">
        /// A pbxz file.
        /// </param>
        /// <param name="output">
        /// A <see cref="Stream"/> to which the output will be written.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UnpackAsync(Stream input, Stream output, CancellationToken cancellationToken)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            byte[] buffer = new byte[16];

            await input.ReadBlockAsync(buffer.AsMemory(0, 4), cancellationToken).ConfigureAwait(false);
            var magic = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(0, 4));

            if (magic != Magic)
            {
                throw new InvalidDataException("The file is not a pbzx file");
            }

            ulong length = 0;
            ulong flags = 0;

            await input.ReadBlockAsync(buffer.AsMemory(0, 8), cancellationToken).ConfigureAwait(false);
            flags = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(0, 8));

            while ((flags & 0x01000000) != 0)
            {
                await input.ReadBlockAsync(buffer.AsMemory(0, 16), cancellationToken).ConfigureAwait(false);
                flags = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(0, 8));
                length = BinaryPrimitives.ReadUInt64BigEndian(buffer.AsSpan(8, 8));

                using (var dataBufferOwner = MemoryPool<byte>.Shared.Rent((int)length))
                {
                    var dataBuffer = dataBufferOwner.Memory.Slice(0, (int)length);

                    await input.ReadBlockAsync(dataBuffer, cancellationToken).ConfigureAwait(false);

                    var header = BinaryPrimitives.ReadInt32LittleEndian(dataBuffer.Slice(0, 4).Span);

                    if (header != ZxHeader)
                    {
                        throw new InvalidDataException();
                    }

                    var footer = BinaryPrimitives.ReadInt16LittleEndian(dataBuffer.Slice(dataBuffer.Length - 2, 2).Span);

                    if (footer != ZxFooter)
                    {
                        throw new InvalidDataException();
                    }

                    await output.WriteAsync(dataBuffer, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
