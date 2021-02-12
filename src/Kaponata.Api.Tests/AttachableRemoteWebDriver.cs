// <copyright file="AttachableRemoteWebDriver.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// A <see cref="RemoteWebDriver"/> which attaches to an existing session.
    /// </summary>
    public class AttachableRemoteWebDriver : RemoteWebDriver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachableRemoteWebDriver"/> class.
        /// </summary>
        /// <param name="remoteAddress">
        /// The URI of the remote WebDriver.
        /// </param>
        /// <param name="sessionId">
        /// The ID of the session to which to connect.
        /// </param>
        public AttachableRemoteWebDriver(Uri remoteAddress, string sessionId)
        : base(remoteAddress, new AttachableCapabilities(sessionId))
        {
        }

        /// <inheritdoc/>
        protected override Response Execute(string driverCommandToExecute, Dictionary<string, object> parameters)
        {
            if (driverCommandToExecute == DriverCommand.NewSession)
            {
                return new Response
                {
                    SessionId = (string)((Dictionary<string, object>)parameters["desiredCapabilities"])["sessionId"],
                };
            }

            return base.Execute(driverCommandToExecute, parameters);
        }

        private class AttachableCapabilities : DesiredCapabilities
        {
            public AttachableCapabilities(string sessionId)
            {
                this.SessionId = sessionId;
                this.SetCapability("sessionId", sessionId);
            }

            public string SessionId { get; }
        }
    }
}
