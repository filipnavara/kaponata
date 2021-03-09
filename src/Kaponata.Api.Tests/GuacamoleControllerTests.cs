// <copyright file="GuacamoleControllerTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.Api.Guacamole;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Tests the <see cref="GuacamoleController"/> class.
    /// </summary>
    public class GuacamoleControllerTests
    {
        /// <summary>
        /// <see cref="GuacamoleController.Authorize(AuthorizationRequest)"/> always returns an empty list.
        /// </summary>
        [Fact]
        public void Authorize_ReturnsEmptyList()
        {
            var controller = new GuacamoleController(NullLogger<GuacamoleController>.Instance);
            var result = controller.Authorize(new AuthorizationRequest());
            var objectResult = Assert.IsType<OkObjectResult>(result);

            var authorizationResult = Assert.IsType<AuthorizationResult>(objectResult.Value);
            Assert.True(authorizationResult.Authorized);
            Assert.Empty(authorizationResult.Configurations);
        }
    }
}
