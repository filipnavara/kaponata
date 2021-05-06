export class Identity {
    /**
     * Gets or sets the thumbprint of the certificate.
     */
    thumbprint: string | undefined;

    /**
     * Gets or sets the simple name of the subject of the certificate.
     */
    commonName: string | undefined;

    /**
     * Gets or sets a value indicating the expiration date of this certificate.
     */
    notAfter: Date | undefined;

    /**
     * Gets or sets a value indicating whether a private key for this certificate is available.
     */
    hasPrivateKey: boolean | undefined;

    /**
     * Gets or sets the type (development or production) of the certificate.
     */
    type: string | undefined;

    /**
     * Gets or sets the ID of the person for which the certificate was issued.
     */
    personID: string | undefined;

    /**
     * Gets or sets the name of the person to which the certificate was issued.
     */
    name: string | undefined;
}
