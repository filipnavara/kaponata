import { Entitlements } from './entitlements';
import { Identity } from './identity';

export class ProvisioningProfile {
    /**
     * Gets or sets the application ID name.
     */
    appIdName: string | undefined;

    /**
     * Gets or sets the application identifier prefixes that this provisioning profile applies to.
     */
    applicationIdentifierPrefix: string[] | undefined;

    /**
     * Gets or sets the platforms which this application targets.
     */
    platform: string[] | undefined;

    /**
     * Gets or sets the date the provisioning profile was created.
     */
    creationDate: Date | undefined;

    /**
     * Gets or sets a list of developer certificates to which this provisioning profile applies.
     */
    developerCertificates: Identity[] = [];

    /**
     * Gets or sets the entitlements contained in this provisioning profile.
     */
    entitlements: Entitlements = new Entitlements();

    /**
     * Gets or sets the date this provisioning profile expires.
     */
    expirationDate: Date | undefined;

    /**
     * Gets or sets the name of this provisioning profile.
     */
    name: string | undefined;

    /**
     * Gets or sets a value indicating whether this provisioning profile can be used to provision
     * applications on any device. This is the case for enterprise profiles.
     */
    provisionsAllDevices: boolean | null | undefined;

    /**
     * Gets or sets a list of devices provisioned in this provisioning profile. Devices are identified by their UUID.
     */
    provisionedDevices: string[] = [];

    /**
     * Gets or sets the names of the teams implied by this provisioning profile.
     */
    teamIdentifier: string[] = [];

    /**
     * Gets or sets the name of the team which owns the provisioning profile.
     */
    teamName: string[] | undefined;

    /**
     * Gets or sets the number of days this provisioning profile is valid.
     */
    timeToLive: number | undefined;

    /**
     * Gets or sets the unique ID of this provisioning profile.
     */
    uuid: string | undefined;

    /**
     * Gets or sets the version of this provisioning profile.
     */
    version: number | undefined;
}
