// <copyright file="KubernetesWebDriverTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Api.WebDriver;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Kaponata.Kubernetes.Polyfill;
using Kaponata.Tests.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="KubernetesWebDriver"/> class.
    /// </summary>
    public class KubernetesWebDriverTests
    {
        /// <summary>
        /// The data for the <see cref="CreateSessionAsync_InvalidCapabilities_Async(NewSessionRequest, string)"/> test.
        /// </summary>
        /// <returns>
        /// Test data.
        /// </returns>
        public static IEnumerable<object[]> CreateSessionAsync_InvalidCapabilities_Data()
        {
            yield return new object[]
            {
                (NewSessionRequest)null,
                "Required capabilities are missing",
            };

            yield return new object[]
            {
                new NewSessionRequest(),
                "Required capabilities are missing",
            };

            yield return new object[]
            {
                new NewSessionRequest() { Capabilities = new CapabilitiesRequest() },
                "Required capabilities are missing",
            };

            yield return new object[]
            {
                new NewSessionRequest()
                {
                    Capabilities = new CapabilitiesRequest()
                    {
                        AlwaysMatch = new Dictionary<string, object>(),
                    },
                },
                "The platformName capability is required and must be a string.",
            };

            yield return new object[]
            {
                new NewSessionRequest()
                {
                    Capabilities = new CapabilitiesRequest()
                    {
                        AlwaysMatch = new Dictionary<string, object>()
                        {
                            { "platformName", true },
                        },
                    },
                },
                "The platformName capability is required and must be a string.",
            };

            yield return new object[]
            {
                new NewSessionRequest()
                {
                    Capabilities = new CapabilitiesRequest()
                    {
                        AlwaysMatch = new Dictionary<string, object>()
                        {
                            { "platformName", null },
                        },
                    },
                },
                "The platformName capability is required and must be a string.",
            };

            yield return new object[]
            {
                new NewSessionRequest()
                {
                    Capabilities = new CapabilitiesRequest()
                    {
                        AlwaysMatch = new Dictionary<string, object>()
                        {
                            { "platformName", "test" },
                        },
                    },
                },
                "The platform 'test' is not supported",
            };
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.KubernetesWebDriver(KubernetesClient, ILogger{KubernetesWebDriver})"/> validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>("kubernetes", () => new KubernetesWebDriver(null, NullLogger<KubernetesWebDriver>.Instance));
            Assert.Throws<ArgumentNullException>("logger", () => new KubernetesWebDriver(Mock.Of<KubernetesClient>(), null));
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.CreateSessionAsync(NewSessionRequest, CancellationToken)"/> returns an error if the capabilities
        /// are missing or invalid.
        /// </summary>
        /// <param name="request">
        /// The session request.
        /// </param>
        /// <param name="expectedMessage">
        /// The expected error message.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(CreateSessionAsync_InvalidCapabilities_Data))]
        public async Task CreateSessionAsync_InvalidCapabilities_Async(NewSessionRequest request, string expectedMessage)
        {
            var kubernetes = new Mock<KubernetesClient>();
            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            var result = await webDriver.CreateSessionAsync(request, default).ConfigureAwait(false);
            var error = Assert.IsType<WebDriverError>(result.Value);

            Assert.Equal(WebDriverErrorCode.SessionNotCreated, error.ErrorCode);
            Assert.Equal(expectedMessage, error.Message);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.CreateSessionAsync(NewSessionRequest, CancellationToken)"/> can create a new session.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSession_CreatesSession_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            var request = new NewSessionRequest()
            {
                Capabilities = new CapabilitiesRequest()
                {
                    AlwaysMatch = new Dictionary<string, object>()
                     {
                         { "platformName", "fake" },
                     },
                },
            };

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "fake-abcd",
                },
            };

            sessionClient
                .Setup(s => s.CreateAsync(It.IsAny<WebDriverSession>(), default))
                .Returns<WebDriverSession, CancellationToken>((request, ct) =>
                {
                    Assert.Equal("fake-", request.Metadata.GenerateName);
                    Assert.Collection(
                        request.Metadata.Labels,
                        l =>
                        {
                            Assert.Equal(Annotations.AutomationName, l.Key);
                            Assert.Equal("fake", l.Value);
                        });

                    Assert.Equal("{\"alwaysMatch\":{\"platformName\":\"fake\"}}", request.Spec.Capabilities);

                    return Task.FromResult(session);
                });

            var watchHook = sessionClient.WithWatcher(session);

            var task = webDriver.CreateSessionAsync(request, default);
            var watchClient = await watchHook.ClientRegistered.Task.ConfigureAwait(false);

            // Simulate the session going through the various stages of setup
            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Added, session).ConfigureAwait(false));
            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));
            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Bookmark, session).ConfigureAwait(false));

            session.Status = new WebDriverSessionStatus()
            {
                IngressReady = false,
                ServiceReady = false,
                SessionReady = false,
            };

            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));

            session.Status.IngressReady = true;
            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));

            session.Status.SessionReady = true;
            session.Status.Capabilities = "{}";
            Assert.Equal(WatchResult.Continue, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));

            session.Status.ServiceReady = true;
            Assert.Equal(WatchResult.Stop, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));

            sessionClient.Setup(s => s.TryReadAsync(session.Metadata.Name, default)).ReturnsAsync(session);
            watchHook.TaskCompletionSource.SetResult(WatchExitReason.ClientDisconnected);

            var result = await task.ConfigureAwait(false);
            var response = Assert.IsType<NewSessionResponse>(result.Value);

            Assert.Equal("fake-abcd", response.SessionId);
            Assert.Empty(response.Capabilities);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.CreateSessionAsync(NewSessionRequest, CancellationToken)"/> returns an error
        /// when the session creation fails.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSession_HandlesError_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            var request = new NewSessionRequest()
            {
                Capabilities = new CapabilitiesRequest()
                {
                    AlwaysMatch = new Dictionary<string, object>()
                     {
                         { "platformName", "fake" },
                     },
                },
            };

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "fake-abcd",
                },
            };

            sessionClient
                .Setup(s => s.CreateAsync(It.IsAny<WebDriverSession>(), default))
                .ReturnsAsync(session);

            var watchHook = sessionClient.WithWatcher(session);

            var task = webDriver.CreateSessionAsync(request, default);
            var watchClient = await watchHook.ClientRegistered.Task.ConfigureAwait(false);

            // Simulate the session going through the various stages of setup
            session.Status = new WebDriverSessionStatus()
            {
                Error = "error",
                Data = "data",
                Message = "message",
                StackTrace = "stacktrace",
            };
            Assert.Equal(WatchResult.Stop, await watchClient(k8s.WatchEventType.Modified, session).ConfigureAwait(false));

            sessionClient.Setup(s => s.TryReadAsync(session.Metadata.Name, default)).ReturnsAsync(session);
            sessionClient.Setup(s => s.DeleteAsync(session, It.IsAny<TimeSpan>(), default)).Returns(Task.CompletedTask);
            watchHook.TaskCompletionSource.SetResult(WatchExitReason.ClientDisconnected);

            var result = await task.ConfigureAwait(false);
            var response = Assert.IsType<WebDriverError>(result.Value);

            Assert.Equal(WebDriverErrorCode.SessionNotCreated, response.ErrorCode);
            Assert.Equal("data", response.Data);
            Assert.Equal("message", response.Message);
            Assert.Equal("stacktrace", response.StackTrace);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.CreateSessionAsync(NewSessionRequest, CancellationToken)"/> returns an error
        /// when the session creation times out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSession_TimesOut_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);
            webDriver.CreationTimeout = TimeSpan.FromMilliseconds(10);

            var request = new NewSessionRequest()
            {
                Capabilities = new CapabilitiesRequest()
                {
                    AlwaysMatch = new Dictionary<string, object>()
                     {
                         { "platformName", "fake" },
                     },
                },
            };

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "fake-abcd",
                },
            };

            sessionClient
                .Setup(s => s.CreateAsync(It.IsAny<WebDriverSession>(), default))
                .ReturnsAsync(session);

            var watchHook = sessionClient.WithWatcher(session);

            var task = webDriver.CreateSessionAsync(request, default);
            var watchClient = await watchHook.ClientRegistered.Task.ConfigureAwait(false);

            var result = await task.ConfigureAwait(false);
            var response = Assert.IsType<WebDriverError>(result.Value);

            Assert.Equal(WebDriverErrorCode.SessionNotCreated, response.ErrorCode);
            Assert.Equal("The session creation timed out.", response.Message);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.CreateSessionAsync(NewSessionRequest, CancellationToken)"/> returns an error
        /// when the session is deleted while creation is still in progress.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSession_HandlesDeletion_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            var request = new NewSessionRequest()
            {
                Capabilities = new CapabilitiesRequest()
                {
                    AlwaysMatch = new Dictionary<string, object>()
                     {
                         { "platformName", "fake" },
                     },
                },
            };

            var session = new WebDriverSession()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "fake-abcd",
                },
            };

            sessionClient
                .Setup(s => s.CreateAsync(It.IsAny<WebDriverSession>(), default))
                .ReturnsAsync(session);

            var watchHook = sessionClient.WithWatcher(session);

            var task = webDriver.CreateSessionAsync(request, default);
            var watchClient = await watchHook.ClientRegistered.Task.ConfigureAwait(false);

            // Simulate the session going through the various stages of setup
            Assert.Equal(WatchResult.Stop, await watchClient(k8s.WatchEventType.Deleted, session).ConfigureAwait(false));

            sessionClient.Setup(s => s.TryReadAsync(session.Metadata.Name, default)).ReturnsAsync((WebDriverSession)null);
            watchHook.TaskCompletionSource.SetResult(WatchExitReason.ClientDisconnected);

            var result = await task.ConfigureAwait(false);
            var response = Assert.IsType<WebDriverError>(result.Value);

            Assert.Equal(WebDriverErrorCode.SessionNotCreated, response.ErrorCode);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.DeleteSessionAsync(string, CancellationToken)"/> returns an error when an attempt
        /// is made to delete a session which does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteSessionAsync_NoSession_ReturnsError_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            sessionClient.Setup(s => s.TryReadAsync("abc", default)).ReturnsAsync((WebDriverSession)null);
            var result = await webDriver.DeleteSessionAsync("abc", default).ConfigureAwait(false);

            var error = Assert.IsType<WebDriverError>(result.Value);
            Assert.Equal(WebDriverErrorCode.InvalidSessionId, error.ErrorCode);
        }

        /// <summary>
        /// <see cref="KubernetesWebDriver.DeleteSessionAsync(string, CancellationToken)"/> deletes the session.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteSessionAsync_DeletesSession_Async()
        {
            var sessionClient = new Mock<NamespacedKubernetesClient<WebDriverSession>>(MockBehavior.Strict);
            var kubernetes = new Mock<KubernetesClient>(MockBehavior.Strict);
            kubernetes.Setup(c => c.GetClient<WebDriverSession>()).Returns(sessionClient.Object);

            var webDriver = new KubernetesWebDriver(kubernetes.Object, NullLogger<KubernetesWebDriver>.Instance);

            var session = new WebDriverSession();
            sessionClient.Setup(s => s.TryReadAsync("abc", default)).ReturnsAsync(session);
            sessionClient.Setup(s => s.DeleteAsync(session, It.IsAny<V1DeleteOptions>(), It.IsAny<TimeSpan>(), default)).Returns(Task.CompletedTask).Verifiable();
            var result = await webDriver.DeleteSessionAsync("abc", default).ConfigureAwait(false);

            Assert.Null(result.Value);

            sessionClient.Verify();
        }
    }
}
