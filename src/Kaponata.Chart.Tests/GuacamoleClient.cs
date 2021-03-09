// <copyright file="GuacamoleClient.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// A client for the Guacamole client REST API.
    /// </summary>
    public class GuacamoleClient
    {
        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuacamoleClient"/> class.
        /// </summary>
        /// <param name="client">
        /// A <see cref="HttpClient"/> which can be used to send requests to the remote server.
        /// </param>
        public GuacamoleClient(HttpClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Asynchronously generates a Guacamole token.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, and returns a <see cref="GuacamoleToken"/>.</returns>
        /// <seealso href="https://github.com/ridvanaltun/guacamole-rest-api-documentation/blob/master/docs/AUTHENTICATION.md#post-apitokens"/>
        public async Task<GuacamoleToken> GenerateTokenAsync(CancellationToken cancellationToken)
        {
            var response = await this.client.PostAsync("api/tokens", new StringContent(string.Empty), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GuacamoleToken>(json);
        }

        /// <summary>
        /// Gets the connection tree for a data source.
        /// </summary>
        /// <param name="dataSource">
        /// The name of the data source for which to get the connection tree.
        /// </param>
        /// <param name="token">
        /// The token to use to authenticate to the remote service.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which rerpesents the asynchronous operation, and returns a <see cref="GuacamoleTree"/>.
        /// </returns>
        /// <seealso href="https://github.com/ridvanaltun/guacamole-rest-api-documentation/blob/32f8d34af8ee0996a08ed11fa4543d579135671c/docs/CONNECTION-GROUPS.md#headers-1"/>
        public async Task<GuacamoleTree> GetTreeAsync(string dataSource, string token, CancellationToken cancellationToken)
        {
            var response = await this.client.GetAsync($"api/session/data/{dataSource}/connectionGroups/ROOT/tree?token={token}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GuacamoleTree>(json);
        }
    }
}
