export class Entitlements {
    /**
     * Gets or sets the application identifier.
     */
    applicationIdentifier: string | undefined;

    /**
     * Gets or sets a value indicating whether other processes (like the debugger) can attach to the app.
     */
    getTaskAllow: boolean | null | undefined;

    /**
     * Gets or sets the value of the push notification entitlement.
     */
    apsEnvironment: string | undefined;

    /**
     * Gets or sets a value indicating whether the App Store code signed builds are allowed
     * to be tested using iTunes Connect.
     */
    betaReportsActive: boolean | null | undefined;

    /**
     * Gets or sets the list of keychain access groups. A keychain access group is a group of applications,
     * identified by an ID, that can share data.
     */
    keychainAccessGroups: string[] | undefined;

    /**
     * Gets or sets the identifier of the team.
     */
    teamIdentifier: string | undefined;

    /**
     * Gets or sets a list of domains with which the app is assocated, to access specific services—such
     * as Safari saved passwords and activity continuation, or universal links.
     */
    associatedDomains: string[] | undefined;

    /**
     * Gets or sets a value indicating whether the Enables App Sandbox is enabled for a target in an Xcode project.
     */
    appSandbox: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed access to group containers that are shared
     * among multiple apps produced by a single development team, and allows certain additional interprocess communication between the apps.
     */
    applicationGroups: string[] | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read-only access to the user’s Movies folder and iTunes movies.
     */
    moviesReadOnly: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read/write access to the user’s Movies folder and iTunes movies.
     */
    moviesReadWrite: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read-only access to the user’s Music folder.
     */
    musicReadOnly: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read/write access to the user’s Music folder.
     */
    musicReadWrite: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read-only access to the user’s Pictures folder.
     */
    picturesReadOnly: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read/write access to the user’s Pictures folder.
     */
    picturesReadWrite: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to communicate with AVB devices.
     */
    audioVideoBridging: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to interact with Bluetooth devices.
     */
    bluetooth: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to capture of movies
     * and still images using the built-in camera, if available.
     */
    camera: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to interact with FireWire devices
     * (currently, does not support interaction with audio/video devices such as DV cameras).
     */
    firewire: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to record audio using the built-in microphone,
     * if available, along with access to audio input using any Core Audio API that supports audio input.
     */
    microphone: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to interact with serial devices.
     */
    serial: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to interact with USB devices,
     * including HID devices such as joysticks.
     */
    usb: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read/write access to the user’s Downloads folder.
     */
    downloadsReadWrite: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted to make use of app-scoped bookmarks and URLs.
     */
    bookmarksAppScope: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allows to make use of document-scoped bookmarks and URLs.
     */
    bookmarksDocumentScope: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted
     * read-onlyaccess to files the user has selected using an Open or Save dialog.
     */
    userSelectedFilesReadOnly: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted
     * read/write access to files the user has selected using an Open or Save dialog.
     */
    userSelectedFilesReadWrite: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is allowed to write executable files.
     */
    userSelectedFilesExecutable: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether a child process inherits the parent’s sandbox.
     */
    inheritSecurity: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted access to
     * network sockets for connecting to other machines.
     */
    networkClient: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted access to
     * network sockets for listening for incoming connections initiated by other machines.
     */
    networkServer: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted
     * read/write access to contacts in the user’s address book;
     * allows apps to infer the default address book if more than one is present on a system.
     */
    addressBook: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether the application is granted read/write access to the user’s calendars.
     */
    calendars: boolean | null | undefined;

    /**
     * Gets or sets a value which indicates whether the application is allowd to make
     * use of the Core Location framework for determining the computer’s geographical location.
     */
    location: boolean | null | undefined;

    /**
     * Gets or sets a value which indicates whether the application is able to print.
     */
    print: boolean | null | undefined;

    /**
     * Gets or sets a value which indicates whether the application
     * is able to use specific AppleScript scripting access groups within a specific scriptable app.
     */
    scriptingTargets: boolean | null | undefined;

    /**
     * Gets or sets the merchant IDs to use for in-app payments.
     */
    inAppPayments: string[] | undefined;

    /**
     * Gets or sets the list of network extensions which are enabled.
     */
    networkExtensions: string[] | undefined;

    /**
     * Gets or sets a value indicating whether inter-app audio is enabled for this app.
     * Inter-app audio allows your app to export audio that other apps can use.
     */
    interAppAudio: boolean | null | undefined;

    /**
     * Gets or sets a value indicating whether Health Kit is enabled for this app.
     */
    healthKit: boolean | null | undefined;

    /**
     * Gets or sets the identifier of the app, as used to support iCloud storage of key-value information for your app.
     */
    iCloudKeyValueStore: string | undefined;

    /**
     * Gets or sets the bundle identifiers of the apps whose iCloud Document Store this app can access.
     */
    iCloudDocumentStore: string[] | undefined;

    /**
     * Gets or sets a value indicating whether HomeKit is enabled for this app.
     */
    homeKit: boolean | null | undefined;

    /**
     * Gets or sets the default data protection rule applied to this app.
     */
    defaultDataProtection: string | undefined;

    /**
     * Gets or sets the VPN settings applied to this app.
     */
    vpnApi: string[] | undefined;

    /**
     * Gets or sets a value indicating whether SiriKit is enabled for this app.
     */
    siriKit: boolean | null | undefined;
}
