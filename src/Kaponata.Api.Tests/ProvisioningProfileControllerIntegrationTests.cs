// <copyright file="ProvisioningProfileControllerIntegrationTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Kaponata.iOS.DeveloperProfiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Kaponata.Api.Tests
{
    /// <summary>
    /// Integration tests for the <see cref="ProvisioningProfileController"/> class.
    /// </summary>
    public class ProvisioningProfileControllerIntegrationTests
    {
        /// <summary>
        /// Tests the provisioning profile lifecycle.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Trait("TestCategory", "IntegrationTest")]
        [Fact]
        public async Task ProvisioningProfile_Lifecycle_Async()
        {
            const string uuid = "98264c6b-5151-4349-8d0f-66691e48ae35";

            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:80/");

            var provisioningProfiles = await client.GetFromJsonAsync<List<ProvisioningProfile>>("/api/ios/provisioningProfiles", default).ConfigureAwait(false);

            // This test should always run in an "empty" cluster.
            Assert.Empty(provisioningProfiles);

            var response = await client.GetAsync($"/api/ios/provisioningProfiles/{uuid}", cancellationToken: default).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Upload the provisioning profile
            response = await client.PostAsync("/api/ios/provisioningProfiles", new ByteArrayContent(File.ReadAllBytes("test.mobileprovision")), default).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The LIST and GET operations should return the requested profile.
            provisioningProfiles = await client.GetFromJsonAsync<List<ProvisioningProfile>>("/api/ios/provisioningProfiles", default).ConfigureAwait(false);
            var provisioningProfile = Assert.Single(provisioningProfiles);
            Assert.Equal(new Guid(uuid), provisioningProfile.Uuid);

            response = await client.GetAsync($"/api/ios/provisioningProfiles/{uuid}", cancellationToken: default).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Delete the provisioning profile
            response = await client.DeleteAsync($"/api/ios/provisioningProfiles/{uuid}", default).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The LIST and GET operations should return an empty list
            provisioningProfiles = await client.GetFromJsonAsync<List<ProvisioningProfile>>("/api/ios/provisioningProfiles", default).ConfigureAwait(false);
            Assert.Empty(provisioningProfiles);

            response = await client.GetAsync($"/api/ios/provisioningProfiles/{uuid}", cancellationToken: default).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
