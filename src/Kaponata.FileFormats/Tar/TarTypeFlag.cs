// <copyright file="TarTypeFlag.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

namespace Kaponata.FileFormats.Tar
{
    /// <summary>
    /// Specifies the type of file archived in a tar archive.
    /// </summary>
    /// <seealso href="https://www.gnu.org/software/tar/manual/html_node/Standard.html"/>
    /// <seealso href="https://www.gnu.org/software/tar/manual/html_node/Extensions.html#Extensions"/>
    public enum TarTypeFlag : byte
    {
        /// <summary>
        /// The file is a regular file.
        /// </summary>
        RegType = (byte)'0',

        /// <summary>
        /// The file is a regular file.
        /// </summary>
        ARegType = (byte)'\0',

        /// <summary>
        /// The file is a file linked to another file, of any type, previously archived.
        /// </summary>
        LnkType = (byte)'1',

        /// <summary>
        /// The file is a symbolic link to another file. The linked-to name is specified in the linkname field with a trailing null.
        /// </summary>
        SymType = (byte)'2',

        /// <summary>
        /// The file is a character special file.
        /// </summary>
        ChrType = (byte)'3',

        /// <summary>
        /// The file is a  block special file.
        /// </summary>
        BlkType = (byte)'4',

        /// <summary>
        /// The file is a directory or sub-directory.
        /// </summary>
        DirType = (byte)'5',

        /// <summary>
        /// The file is a FIFO special file.
        /// </summary>
        FifoType = (byte)'6',

        /// <summary>
        /// The file is a contiguous file.
        /// </summary>
        ConttType = (byte)'7',

        /// <summary>
        /// The next file in the archive has an extended header.
        /// </summary>
        ExtendedHeader = (byte)'x',

        /// <summary>
        /// The archive uses an extended header.
        /// </summary>
        GlobalExtendedHeader = (byte)'g',

        /// <summary>
        /// The next file on the tape has a long name
        /// </summary>
        LongName = (byte)'L',

        /// <summary>
        /// The next file on the tape is a link with a long name.
        /// </summary>
        LongLink = (byte)'K',
    }
}
