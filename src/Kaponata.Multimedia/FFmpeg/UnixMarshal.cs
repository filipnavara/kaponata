// <copyright file="UnixMarshal.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

namespace Kaponata.Multimedia.FFmpeg
{
    /// <summary>
    /// Provides helper methods for working with Unix exceptions.
    /// </summary>
    public static class UnixMarshal
    {
        /// <summary>
        /// Gets a <see cref="string"/> which describes a <see cref="UnixError"/>.
        /// </summary>
        /// <param name="error">
        /// The error to describe.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> which describes the error.
        /// </returns>
        public static string GetErrorMessage(UnixError error) => error switch
        {
            UnixError.EPERM => "Operation not permitted",
            UnixError.ENOENT => "No such file or directory",
            UnixError.ESRCH => "No such process",
            UnixError.EINTR => "Interrupted system call",
            UnixError.EIO => "I/O error",
            UnixError.ENXIO => "No such device or address",
            UnixError.E2BIG => "Arg list too long",
            UnixError.ENOEXEC => "Exec format error",
            UnixError.EBADF => "Bad file number",
            UnixError.ECHILD => "No child processes",
            UnixError.EAGAIN => "Try again",
            UnixError.ENOMEM => "Out of memory",
            UnixError.EACCES => "Permission denied",
            UnixError.EFAULT => "Bad address",
            UnixError.ENOTBLK => "Block device required",
            UnixError.EBUSY => "Device or resource busy",
            UnixError.EEXIST => "File exists",
            UnixError.EXDEV => "Cross-device link",
            UnixError.ENODEV => "No such device",
            UnixError.ENOTDIR => "Not a directory",
            UnixError.EISDIR => "Is a directory",
            UnixError.EINVAL => "Invalid argument",
            UnixError.ENFILE => "File table overflow",
            UnixError.EMFILE => "Too many open files",
            UnixError.ENOTTY => "Not a typewriter",
            UnixError.ETXTBSY => "Text file busy",
            UnixError.EFBIG => "File too large",
            UnixError.ENOSPC => "No space left on device",
            UnixError.ESPIPE => "Illegal seek",
            UnixError.EROFS => "Read-only file system",
            UnixError.EMLINK => "Too many links",
            UnixError.EPIPE => "Broken pipe",
            UnixError.EDOM => "Math argument out of domain of func",
            UnixError.ERANGE => "Math result not representable",
            UnixError.EDEADLK => "Resource deadlock would occur",
            UnixError.ENAMETOOLONG => "File name too long",
            UnixError.ENOLCK => "No record locks available",
            UnixError.ENOSYS => "Function not implemented",
            UnixError.ENOTEMPTY => "Directory not empty",
            UnixError.ELOOP => "Too many symbolic links encountered",
            UnixError.ENOMSG => "No message of desired type",
            UnixError.EIDRM => "Identifier removed",
            UnixError.ECHRNG => "Channel number out of range",
            UnixError.EL2NSYNC => "Level 2 not synchronized",
            UnixError.EL3HLT => "Level 3 halted",
            UnixError.EL3RST => "Level 3 reset",
            UnixError.ELNRNG => "Link number out of range",
            UnixError.EUNATCH => "Protocol driver not attached",
            UnixError.ENOCSI => "No CSI structure available",
            UnixError.EL2HLT => "Level 2 halted",
            UnixError.EBADE => "Invalid exchange",
            UnixError.EBADR => "Invalid request descriptor",
            UnixError.EXFULL => "Exchange full",
            UnixError.ENOANO => "No anode",
            UnixError.EBADRQC => "Invalid request code",
            UnixError.EBADSLT => "Invalid slot",
            UnixError.EBFONT => "Bad font file format",
            UnixError.ENOSTR => "Device not a stream",
            UnixError.ENODATA => "No data available",
            UnixError.ETIME => "Timer expired",
            UnixError.ENOSR => "Out of streams resources",
            UnixError.ENONET => "Machine is not on the network",
            UnixError.ENOPKG => "Package not installed",
            UnixError.EREMOTE => "Object is remote",
            UnixError.ENOLINK => "Link has been severed",
            UnixError.EADV => "Advertise error",
            UnixError.ESRMNT => "Srmount error",
            UnixError.ECOMM => "Communication error on send",
            UnixError.EPROTO => "Protocol error",
            UnixError.EMULTIHOP => "Multihop attempted",
            UnixError.EDOTDOT => "RFS specific error",
            UnixError.EBADMSG => "Not a data message",
            UnixError.EOVERFLOW => "Value too large for defined data type",
            UnixError.ENOTUNIQ => "Name not unique on network",
            UnixError.EBADFD => "File descriptor in bad state",
            UnixError.EREMCHG => "Remote address changed",
            UnixError.ELIBACC => "Can not access a needed shared library",
            UnixError.ELIBBAD => "Accessing a corrupted shared library",
            UnixError.ELIBSCN => ".lib section in a.out corrupted",
            UnixError.ELIBMAX => "Attempting to link in too many shared libraries",
            UnixError.ELIBEXEC => "Cannot exec a shared library directly",
            UnixError.EILSEQ => "Illegal byte sequence",
            UnixError.ERESTART => "Interrupted system call should be restarted",
            UnixError.ESTRPIPE => "Streams pipe error",
            UnixError.EUSERS => "Too many users",
            UnixError.ENOTSOCK => "Socket operation on non-socket",
            UnixError.EDESTADDRREQ => "Destination address required",
            UnixError.EMSGSIZE => "Message too long",
            UnixError.EPROTOTYPE => "Protocol wrong type for socket",
            UnixError.ENOPROTOOPT => "Protocol not available",
            UnixError.EPROTONOSUPPORT => "Protocol not supported",
            UnixError.ESOCKTNOSUPPORT => "Socket type not supported",
            UnixError.EOPNOTSUPP => "Operation not supported on transport endpoint",
            UnixError.EPFNOSUPPORT => "Protocol family not supported",
            UnixError.EAFNOSUPPORT => "Address family not supported by protocol",
            UnixError.EADDRINUSE => "Address already in use",
            UnixError.EADDRNOTAVAIL => "Cannot assign requested address",
            UnixError.ENETDOWN => "Network is down",
            UnixError.ENETUNREACH => "Network is unreachable",
            UnixError.ENETRESET => "Network dropped connection because of reset",
            UnixError.ECONNABORTED => "Software caused connection abort",
            UnixError.ECONNRESET => "Connection reset by peer",
            UnixError.ENOBUFS => "No buffer space available",
            UnixError.EISCONN => "Transport endpoint is already connected",
            UnixError.ENOTCONN => "Transport endpoint is not connected",
            UnixError.ESHUTDOWN => "Cannot send after transport endpoint shutdown",
            UnixError.ETOOMANYREFS => "Too many references: cannot splice",
            UnixError.ETIMEDOUT => "Connection timed out",
            UnixError.ECONNREFUSED => "Connection refused",
            UnixError.EHOSTDOWN => "Host is down",
            UnixError.EHOSTUNREACH => "No route to host",
            UnixError.EALREADY => "Operation already in progress",
            UnixError.EINPROGRESS => "Operation now in progress",
            UnixError.ESTALE => "Stale NFS file handle",
            UnixError.EUCLEAN => "Structure needs cleaning",
            UnixError.ENOTNAM => "Not a XENIX named type file",
            UnixError.ENAVAIL => "No XENIX semaphores available",
            UnixError.EISNAM => "Is a named type file",
            UnixError.EREMOTEIO => "Remote I/O error",
            UnixError.EDQUOT => "Quota exceeded",

            UnixError.ENOMEDIUM => "No medium found",
            UnixError.EMEDIUMTYPE => "Wrong medium type",
            _ => $"An Unix error occurred. Error code: {error}",
        };

        /// <summary>
        /// Throws a .NET exception which represents a given <see cref="UnixError"/>.
        /// </summary>
        /// <param name="error">
        /// The <see cref="UnixError"/> for which to throw the exception.
        /// </param>
        public static void ThrowExceptionForError(UnixError error)
        {
            // The root exception. When using .NET specific error messages, we try to use this as the innerException
            // so that the UnixError value does not get lost.
            var message = UnixMarshal.GetErrorMessage(error);

            switch (error)
            {
                case UnixError.EBADF:
                case UnixError.EINVAL:
                    throw new ArgumentException(message);

                case UnixError.ERANGE:
                    throw new ArgumentOutOfRangeException(message);

                case UnixError.ENOTDIR:
                    throw new DirectoryNotFoundException(message);

                case UnixError.ENOENT:
                    throw new FileNotFoundException(message);

                case UnixError.EOPNOTSUPP:
                case UnixError.EPERM:
                    throw new InvalidOperationException(message);

                case UnixError.ENOEXEC:
                    throw new InvalidProgramException(message);

                case UnixError.EIO:
                case UnixError.ENOSPC:
                case UnixError.ENOTEMPTY:
                case UnixError.ENXIO:
                case UnixError.EROFS:
                case UnixError.ESPIPE:
                    throw new IOException(message);

                case UnixError.EFAULT:
                    throw new NullReferenceException(message);

                case UnixError.EOVERFLOW:
                    throw new OverflowException(message);

                case UnixError.ENAMETOOLONG:
                    throw new PathTooLongException(message);

                case UnixError.EACCES:
                case UnixError.EISDIR:
                    throw new UnauthorizedAccessException(message);

                default:
                    throw new Exception(message);
            }
        }
    }
}
