// <copyright file="TarHeader.Serialization.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Text;

namespace Kaponata.FileFormats.Tar
{
    /// <content>
    /// Supports serializing and deserializing <see cref="TarHeader"/> objects.
    /// </content>
    public partial struct TarHeader
    {
        /// <summary>
        /// Characters which get trimmed. This includes NULL bytes and ASCII spaces.
        /// </summary>
        private static readonly byte[] TrimCharacters = new byte[] { 0, (byte)' ' };

        /// <summary>
        /// Reads a <see cref="TarHeader"/> from a span.
        /// </summary>
        /// <param name="source">
        /// The data from which to read the header.
        /// </param>
        /// <returns>
        /// A <see cref="TarHeader"/> representing the data read.
        /// </returns>
        public static TarHeader Read(Span<byte> source)
        {
            if (source.Length != 512)
            {
                throw new ArgumentOutOfRangeException(nameof(source));
            }

            TarHeader value = default;
            value.FileName = ReadString(source[0..100]);
            value.FileMode = (LinuxFileMode)ReadOctalString(source[100..108]);
            value.UserId = ReadOctalString(source[108..116]);
            value.GroupId = ReadOctalString(source[116..124]);
            value.FileSize = ReadOctalString(source[124..136]);
            value.LastModified = DateTimeOffset.FromUnixTimeSeconds(ReadOctalString(source[136..148]));
            value.Checksum = ReadOctalString(source[148..156]);
            value.TypeFlag = (TarTypeFlag)source[156];
            value.LinkName = ReadString(source[157..257]);
            value.Magic = ReadString(source[257..263]);
            value.Version = ReadOctalStringOrNull(source[263..265]);
            value.UserName = ReadString(source[265..297]);
            value.GroupName = ReadString(source[297..329]);
            value.DevMajor = ReadOctalStringOrNull(source[329..337]);
            value.DevMinor = ReadOctalStringOrNull(source[337..345]);
            value.Prefix = ReadString(source[345..500]);
            return value;
        }

        /// <summary>
        /// Serializes this <see cref="TarHeader"/> to a byte span.
        /// </summary>
        /// <param name="destination">
        /// The span to which to serialize the data.
        /// </param>
        public void Write(Span<byte> destination)
        {
            if (destination.Length != 512)
            {
                throw new ArgumentOutOfRangeException(nameof(destination));
            }

            destination.Clear();

            Encoding.UTF8.GetBytes(this.FileName, destination[0..100]);

            // Most octal numbers have a terminating NULL character, so that's why you'll see
            // lengths like 7, 11,... in calls to WriteOctalString
            WriteOctalString((uint)this.FileMode, 7, destination[100..108]);
            WriteOctalString(this.UserId, 7, destination[108..116]);
            WriteOctalString(this.GroupId, 7, destination[116..124]);
            WriteOctalString(this.FileSize, 11, destination[124..136]);
            WriteOctalString(this.LastModified.ToUnixTimeSeconds(), 11, destination[136..148]);
            destination[156] = (byte)this.TypeFlag;
            Encoding.UTF8.GetBytes(this.LinkName, destination[157..257]);
            Encoding.UTF8.GetBytes(this.Magic.PadRight(6), destination[257..263]);

            if (this.Version != null)
            {
                // The version number is not NULL-terminated (but always 0)
                WriteOctalString(this.Version.Value, 2, destination[263..265]);
            }
            else
            {
                destination[263] = (byte)' ';
            }

            Encoding.UTF8.GetBytes(this.UserName, destination[265..297]);
            Encoding.UTF8.GetBytes(this.GroupName, destination[297..329]);

            if (this.DevMajor != null)
            {
                WriteOctalString(this.DevMajor.Value, 7, destination[329..337]);
            }

            if (this.DevMinor != null)
            {
                WriteOctalString(this.DevMinor.Value, 7, destination[337..345]);
            }

            Encoding.UTF8.GetBytes(this.Prefix, destination[345..500]);

            // "When calculating the checksum, the chksum field is treated as if it were all blanks."
            for (int i = 148; i < 156; i++)
            {
                destination[i] = (byte)' ';
            }

            // Calculate the checksum
            this.Checksum = 0;

            for (int i = 0; i <= 499; i++)
            {
                this.Checksum += destination[i];
            }

            WriteOctalString(this.Checksum, 6, destination[148..156]);
            destination[154] = 0;
        }

        private static void WriteOctalString(long data, int len, Span<byte> destination)
        {
            var octalString = Convert.ToString(data, 8).PadLeft(len, '0');
            Encoding.UTF8.GetBytes(octalString, destination);
        }

        private static string ReadString(Span<byte> source)
        {
            return Encoding.UTF8.GetString(source.Trim(TrimCharacters));
        }

        private static uint ReadOctalString(Span<byte> source)
        {
            var value = ReadString(source);

            if (value.Length == 0)
            {
                return 0;
            }

            return Convert.ToUInt32(ReadString(source), 8);
        }

        private static uint? ReadOctalStringOrNull(Span<byte> source)
        {
            var value = ReadString(source);

            if (value.Length == 0)
            {
                return null;
            }

            return Convert.ToUInt32(ReadString(source), 8);
        }
    }
}
