// <copyright file="ChildOperatorBuilderExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s.Models;
using Kaponata.Kubernetes;
using Kaponata.Kubernetes.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Kaponata.Operator.Operators
{
    /// <summary>
    /// Extensions for the <see cref="ChildOperatorBuilder{TParent, TChild}"/> class.
    /// </summary>
    public static class ChildOperatorBuilderExtensions
    {
        /// <summary>
        /// Adds a feedback loop which initializes a new Appium session to the operator.
        /// </summary>
        /// <param name="builder">
        /// The operator builder.
        /// </param>
        /// <param name="appiumPort">
        /// The port at which the Appium server is listening.
        /// </param>
        /// <param name="initializer">
        /// Optionally, a delegate which can be used to initialize the pod before the session
        /// is created.
        /// </param>
        /// <returns>
        /// An operator builder which can be used to further configure the operator.
        /// </returns>
        public static ChildOperatorBuilder<WebDriverSession, V1Pod> CreatesSession(
            this ChildOperatorBuilder<WebDriverSession, V1Pod> builder,
            int appiumPort,
            SessionPodInitializer initializer = null)
        {
            return builder.PostsFeedback(
                async (context, cancellationToken) =>
                {
                    var kubernetes = context.Kubernetes;
                    var logger = context.Logger;
                    var session = context.Parent;
                    var pod = context.Child;

                    Feedback<WebDriverSession, V1Pod> feedback = null;

                    if (session?.Spec?.Capabilities == null)
                    {
                        // This is an invalid session; we need at least desired capabilities.
                        logger.LogWarning("Session {session} is missing desired capabilities.", session?.Metadata?.Name);
                    }
                    else if (session.Status?.SessionId != null)
                    {
                        // Do nothing if the session already exists.
                        logger.LogDebug("Session {session} already has a session ID.", session?.Metadata?.Name);
                    }
                    else if (pod?.Status?.Phase != "Running" || !pod.Status.ContainerStatuses.All(c => c.Ready))
                    {
                        // Do nothing if the pod is not yet ready
                        logger.LogInformation("Not creating a session for session {session} because pod {pod} is not ready yet.", session?.Metadata?.Name, pod?.Metadata?.Name);
                    }
                    else
                    {
                        if (initializer != null)
                        {
                            await initializer(context, cancellationToken).ConfigureAwait(false);
                        }

                        var requestedCapabilities = JsonConvert.DeserializeObject(context.Parent.Spec.Capabilities);
                        var request = JsonConvert.SerializeObject(
                            new
                            {
                                capabilities = requestedCapabilities,
                            });

                        var content = new StringContent(request, Encoding.UTF8, "application/json");

                        using (var httpClient = kubernetes.CreatePodHttpClient(context.Child, appiumPort))
                        using (var remoteResult = await httpClient.PostAsync("wd/hub/session/", content, cancellationToken).ConfigureAwait(false))
                        {
                            var sessionJson = await remoteResult.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                            var sessionObject = JObject.Parse(sessionJson);
                            var sessionValue = (JObject)sessionObject.GetValue("value");

                            feedback = new Feedback<WebDriverSession, V1Pod>()
                            {
                                ParentFeedback = new JsonPatchDocument<WebDriverSession>(),
                            };

                            if (context.Parent.Status == null)
                            {
                                feedback.ParentFeedback.Add(s => s.Status, new WebDriverSessionStatus());
                            }

                            // Check whether we should store this as a Kubernetes object.
                            if (sessionValue.TryGetValue("sessionId", out var sessionId))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.SessionId, sessionId.Value<string>());
                                feedback.ParentFeedback.Add(s => s.Status.SessionReady, true);
                                feedback.ParentFeedback.Add(s => s.Status.SessionPort, appiumPort);
                            }

                            if (sessionValue.TryGetValue("capabilities", out var capabilities))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.Capabilities, capabilities.ToString(Formatting.None));
                            }

                            if (sessionValue.TryGetValue("error", out var error))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.Error, error.Value<string>());
                            }

                            if (sessionValue.TryGetValue("message", out var message))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.Message, message.Value<string>());
                            }

                            if (sessionValue.TryGetValue("stacktrace", out var stackTrace))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.StackTrace, stackTrace.Value<string>());
                            }

                            if (sessionValue.TryGetValue("data", out var data))
                            {
                                feedback.ParentFeedback.Add(s => s.Status.Data, data.ToString(Formatting.None));
                            }
                        }
                    }

                    return feedback;
                });
        }
    }
}
