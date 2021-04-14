// <copyright file="PairingRecordTests.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using Claunia.PropertyList;
using Kaponata.iOS.Lockdown;
using System;
using System.IO;
using Xunit;

namespace Kaponata.iOS.Tests.Lockdown
{
    /// <summary>
    /// Tests the <see cref="PairingRecord"/> class.
    /// </summary>
    public class PairingRecordTests
    {
        /// <summary>
        /// <see cref="PairingRecord.Read(NSDictionary)"/> correctly parses the property list data.
        /// </summary>
        [Fact]
        public void Read_Works()
        {
            var dict = (NSDictionary)PropertyListParser.Parse("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var pairingRecord = PairingRecord.Read(dict);

            Assert.NotNull(pairingRecord.DeviceCertificate);
            Assert.Equal("879D15EC44D67A89BF0AC3C0311DA035FDD56D0E", pairingRecord.DeviceCertificate.Thumbprint);
            Assert.Equal("MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=", Convert.ToBase64String(pairingRecord.EscrowBag));
            Assert.NotNull(pairingRecord.HostCertificate);
            Assert.Equal("EE63391AA1FBA937E2784CC7DAAA9C22BA223B54", pairingRecord.HostCertificate.Thumbprint);
            Assert.Equal("01234567-012345678901234567", pairingRecord.HostId);
            Assert.NotNull(pairingRecord.HostPrivateKey);
            Assert.Equal(2048, pairingRecord.HostPrivateKey.KeySize);
            Assert.NotNull(pairingRecord.RootCertificate);
            Assert.Equal("DB0F6BAA694FA99879281A388D170CCE1412AC92", pairingRecord.RootCertificate.Thumbprint);
            Assert.NotNull(pairingRecord.RootPrivateKey);
            Assert.Equal(2048, pairingRecord.RootPrivateKey.KeySize);
            Assert.Equal("01234567890123456789012345", pairingRecord.SystemBUID);
            Assert.Equal("01:23:45:67:89:ab", pairingRecord.WiFiMacAddress);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record.
        /// </summary>
        [Fact]
        public void ToPropertyList_Works()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            var serializedDict = pairingRecord.ToPropertyList();

            Assert.Equal(dict.Keys, serializedDict.Keys);

            foreach (var key in dict.Keys)
            {
                Assert.Equal(dict[key], serializedDict[key]);
            }

            var xml = serializedDict.ToXmlPropertyList();

            Assert.Equal(raw, xml, ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record when the private
        /// keys are not available.
        /// </summary>
        [Fact]
        public void ToPropertyList_NoPrivateKeys_Works()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            pairingRecord.HostPrivateKey = null;
            pairingRecord.RootPrivateKey = null;
            var serializedDict = pairingRecord.ToPropertyList();

            Assert.Equal(dict.Keys.Count - 2, serializedDict.Keys.Count);
        }

        /// <summary>
        /// <see cref="PairingRecord.ToPropertyList(bool)"/> correctly serializes the pairing record.
        /// </summary>
        [Fact]
        public void ToPropertyList_ExcludesPrivateKeysIfRequired()
        {
            var raw = File.ReadAllText("Lockdown/0123456789abcdef0123456789abcdef01234567.plist");
            var dict = (NSDictionary)XmlPropertyListParser.ParseString(raw);

            var pairingRecord = PairingRecord.Read(dict);
            var serializedDict = pairingRecord.ToPropertyList(includePrivateKeys: false);

            Assert.False(serializedDict.ContainsKey("HostPrivateKey"));
            Assert.False(serializedDict.ContainsKey("RootPrivateKey"));
        }
    }
}
