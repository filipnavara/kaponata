// <copyright file="LockdownProtocolTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.Lockdown;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="LockdownProtocol"/> class.
    /// </summary>
    public class LockdownProtocolTests
    {
        /// <summary>
        /// <see cref="LockdownProtocol.WriteMessageAsync(LockdownMessage, CancellationToken)"/> validates its arguments.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task WriteMessageAsync_ValidatesArgumentsAsync()
        {
            await using (var protocol = new LockdownProtocol(Stream.Null, false, NullLogger.Instance))
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => protocol.WriteMessageAsync(null, default)).ConfigureAwait(false);
            }
        }
    }
}
