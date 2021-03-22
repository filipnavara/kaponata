// <copyright file="ProvisioningProfile.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Nerdbank.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.iOS.DeveloperProfiles
{
    /// <summary>
    /// Provides access to an iOS Provisioning Profile. A provisioning profile contains information about the issuer
    /// (who signs the application) and a set of devices, thereby authorizing the applications of the issuer to run
    /// on those devices.
    /// </summary>
    /// <seealso href="https://developer.apple.com/library/ios/documentation/IDEs/Conceptual/AppStoreDistributionTutorial/CreatingYourTeamProvisioningProfile/CreatingYourTeamProvisioningProfile.html"/>
    public class ProvisioningProfile
    {
        /// <summary>
        /// Gets the application ID name.
        /// </summary>
        public string AppIdName { get; private set; }

        /// <summary>
        /// Gets the application identifier prefixes that this provisioning profile applies to.
        /// </summary>
        public IList<string> ApplicationIdentifierPrefix { get; private set; }

        /// <summary>
        /// Gets the platforms which this application targets.
        /// </summary>
        public IList<string> Platform { get; private set; }

        /// <summary>
        /// Gets the date the provisioning profile was created.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets a list of developer certificates to which this provisioning profile applies.
        /// </summary>
        public IEnumerable<X509Certificate2> DeveloperCertificates { get; private set; }

        /// <summary>
        /// Gets the entitlements contained in this provisioning profile.
        /// </summary>
        public Entitlements Entitlements { get; private set; }

        /// <summary>
        /// Gets the date this provisioning profile expires.
        /// </summary>
        public DateTime ExpirationDate { get; private set; }

        /// <summary>
        /// Gets the name of this provisioning profile.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this provisioning profile can be used to provision
        /// applications on any device. This is the case for enterprise profiles.
        /// </summary>
        public bool? ProvisionsAllDevices { get; private set; }

        /// <summary>
        /// Gets a list of devices provisioned in this provisioning profile. Devices are identified
        /// by their UUID.
        /// </summary>
        public IList<string> ProvisionedDevices { get; private set; }

        /// <summary>
        /// Gets the names of the teams implied by this provisioning profile.
        /// </summary>
        public IList<string> TeamIdentifier { get; private set; }

        /// <summary>
        /// Gets the name of the team which owns the provisioning profile.
        /// </summary>
        public string TeamName { get; private set; }

        /// <summary>
        /// Gets the number of days this provisioning profile is valid.
        /// </summary>
        public int TimeToLive { get; private set; }

        /// <summary>
        /// Gets the unique ID of this provisioning profile.
        /// </summary>
        public Guid Uuid { get; private set; }

        /// <summary>
        /// Gets the version of this provisioning profile.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Asynchronously reads a <see cref="ProvisioningProfile"/> off a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the provisioning profile.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation, and returns a <see cref="ProvisioningProfile"/>
        /// when completed.
        /// </returns>
        public static async Task<ProvisioningProfile> ReadAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var length = (int)stream.Length;

            byte[] data = new byte[length];
            await stream.ReadBlockAsync(data.AsMemory(0, length), cancellationToken).ConfigureAwait(false);

            SignedCms signedCms = new SignedCms();
            signedCms.Decode(data);

            return Read(signedCms);
        }

        /// <summary>
        /// Reads a <see cref="ProvisioningProfile"/> off a <see cref="SignedCms"/> object.
        /// </summary>
        /// <param name="signedData">
        /// A <see cref="SignedCms"/> object which represents the signed provisioning
        /// profile.
        /// </param>
        /// <returns>
        /// A <see cref="ProvisioningProfile"/>.
        /// </returns>
        public static ProvisioningProfile Read(SignedCms signedData)
        {
            if (signedData == null)
            {
                throw new ArgumentNullException(nameof(signedData));
            }

            var propertyList = (NSDictionary)XmlPropertyListParser.Parse(signedData.ContentInfo.Content);

            return new ProvisioningProfile()
            {
                AppIdName = propertyList.GetString("AppIDName"),
                ApplicationIdentifierPrefix = propertyList.GetStringArray("ApplicationIdentifierPrefix"),
                Platform = propertyList.GetStringArray("Platform"),
                CreationDate = propertyList.GetDateTime("CreationDate"),
                DeveloperCertificates = propertyList.GetDataArray("DeveloperCertificates").Select(d => new X509Certificate2(d)).ToList(),
                Entitlements = Entitlements.Read(propertyList.GetDict("Entitlements")),
                ExpirationDate = propertyList.GetDateTime("ExpirationDate"),
                Name = propertyList.GetString("Name"),
                ProvisionsAllDevices = propertyList.GetNullableBoolean("ProvisionsAllDevices"),
                ProvisionedDevices = propertyList.GetStringArray("ProvisionedDevices"),
                TeamIdentifier = propertyList.GetStringArray("TeamIdentifier"),
                TeamName = propertyList.GetString("TeamName"),
                TimeToLive = propertyList.GetInt32("TimeToLive"),
                Uuid = new Guid(propertyList.GetString("UUID")),
                Version = propertyList.GetInt32("Version"),
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
