// <copyright file="LockdownClientTests.SetValue.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <content>
    /// Tests for the <see cref="LockdownClient.SetValueAsync(string, string, string, CancellationToken)"/> family of methods.
    /// </content>
    public partial class LockdownClientTests
    {
        /// <summary>
        /// <see cref="LockdownClient.SetValueAsync(string, string, string, CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SetValue_Works_Async()
        {
            var dict = new NSDictionary();
            dict.Add("Request", "GetValue");
            dict.Add("Key", "my-key");
            dict.Add("Value", "my-value");

            var protocol = new Mock<LockdownProtocol>(MockBehavior.Strict);
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<LockdownMessage>(), default))
                .Callback<LockdownMessage, CancellationToken>(
                (message, cancellationToken) =>
                {
                    var setValueRequest = Assert.IsType<SetValueRequest>(message);
                    Assert.Equal("my-domain", setValueRequest.Domain);
                    Assert.Equal("my-key", setValueRequest.Key);
                    Assert.Equal("my-value", setValueRequest.Value);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            protocol
                .Setup(p => p.ReadMessageAsync(default))
                .ReturnsAsync(dict)
                .Verifiable();

            protocol.Setup(p => p.ReadMessageAsync<GetValueResponse<string>>(default)).CallBase();
            protocol.Setup(p => p.DisposeAsync()).Returns(ValueTask.CompletedTask);

            await using (var client = new LockdownClient(protocol.Object, NullLogger<LockdownClient>.Instance))
            {
                await client.SetValueAsync(domain: "my-domain", key: "my-key", value: "my-value", default).ConfigureAwait(false);
            }

            protocol.Verify();
        }
    }
}
