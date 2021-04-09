// <copyright file="LockdownClient.Pair.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.Lockdown
{
    /// <content>
    /// Methods to pair a host with a device.
    /// </content>
    public partial class LockdownClient
    {
        /// <summary>
        /// Asynchronously sends a request to pair a host with the device.
        /// </summary>
        /// <param name="pairingRecord">
        /// A pairing record which represents the pairing request.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<PairingResult> PairAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            return this.PairAsync(
                "Pair",
                pairingRecord,
                new PairingOptions()
                {
                    ExtendedPairingErrors = true,
                },
                cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a request to unpair a host with the device.
        /// </summary>
        /// <param name="pairingRecord">
        /// A pairing record which represents the paired host.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel teh asynchronous
        /// operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<PairingResult> UnpairAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            return this.PairAsync("Unpair", pairingRecord, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a request to validate a pairing record.
        /// </summary>
        /// <param name="pairingRecord">
        /// A pairing record which represents the pairing record to validate.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous
        /// operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<PairingResult> ValidatePairAsync(PairingRecord pairingRecord, CancellationToken cancellationToken)
        {
            return this.PairAsync("ValidatePair", pairingRecord, null, cancellationToken);
        }

        private async Task<PairingResult> PairAsync(string request, PairingRecord pairingRecord, PairingOptions options, CancellationToken cancellationToken)
        {
            if (pairingRecord == null)
            {
                throw new ArgumentNullException(nameof(pairingRecord));
            }

            var pairRequest = new PairRequest()
            {
                Request = request,
                PairRecord = pairingRecord,
                PairingOptions = options,
                Label = this.Label,
            };

            await this.protocol.WriteMessageAsync(
                pairRequest,
                cancellationToken).ConfigureAwait(false);

            var message = await this.protocol.ReadMessageAsync(
                cancellationToken);

            if (message == null)
            {
                return null;
            }

            var response = PairResponse.Read(message);

            if (response.Error == null)
            {
                return new PairingResult()
                {
                    Status = PairingStatus.Success,
                    EscrowBag = response.EscrowBag,
                };
            }
            else if (Enum.TryParse(response.Error, out PairingStatus status))
            {
                return new PairingResult()
                {
                    Status = status,
                };
            }
            else
            {
                throw new LockdownException(response.Error);
            }
        }
    }
}
