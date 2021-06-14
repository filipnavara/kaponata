import { SettingsPage } from './settings.po';
import { browser, logging } from 'protractor';
import { ProvisioningProfile } from '../../src/app/provisioning-profile';
import nock = require('nock');
import { Entitlements } from '../../src/app/entitlements';
import { Identity } from '../../src/app/identity';

describe('workspace-project App', () => {
  let page: SettingsPage;
  beforeEach(() => {
    page = new SettingsPage();
  });

  it('should open without errors', async () => {
    const provisioningProfile = new ProvisioningProfile();
    provisioningProfile.name = 'pp1';

    const provisioningProfiles = [provisioningProfile];
    const developerDisks = ['14.0'];

    const scope = nock('http://localhost:9999')
    .get('/api/ios/provisioningProfiles')
    .reply(200, provisioningProfiles)
    .get('/api/ios/developerDisks')
    .reply(200, developerDisks);

    await page.navigateTo();
    const text = await page.getTitleText();

    expect(text).toEqual('Settings');

    // Assert that there are no errors emitted from the browser
    const logs = await browser.manage().logs().get(logging.Type.BROWSER);
    expect(logs).not.toContain(jasmine.objectContaining({
      level: logging.Level.SEVERE,
    } as logging.Entry));
  });

  it('should list the provisioning profiles', async () => {
    const provisioningProfile = new ProvisioningProfile();
    provisioningProfile.name = 'pp1';
    const entitlements = new Entitlements();
    entitlements.applicationIdentifier = 'app1';
    provisioningProfile.entitlements = entitlements;
    provisioningProfile.provisionedDevices = ['d1'];
    const identity = new Identity();
    identity.name = 'team123';
    provisioningProfile.developerCertificates = [identity];
    const provisioningProfiles = [provisioningProfile];
    const developerDisks = ['14.0', '13.1'];

    const scope = nock('http://localhost:9999')
      .get('/api/ios/provisioningProfiles')
      .reply(200, provisioningProfiles)
      .get('/api/ios/developerDisks')
      .reply(200, developerDisks);

    await page.navigateTo();
    const provisioningProfilesElements = await page.getProvisioningProfiles();

    expect(provisioningProfilesElements).toEqual(['pp1', 'app1', '1 devices', '1 identities', '', '']);
  });

  it('should list the developer disks', async () => {
    const provisioningProfile = new ProvisioningProfile();
    provisioningProfile.name = 'pp1';

    const provisioningProfiles = [provisioningProfile];
    const developerDisks = ['14.0', '13.1'];

    const scope = nock('http://localhost:9999')
      .get('/api/ios/provisioningProfiles')
      .reply(200, provisioningProfiles)
      .get('/api/ios/developerDisks')
      .reply(200, developerDisks);

    await page.navigateTo();
    const developerDisksElements = await page.getDeveloperDisks();

    expect(developerDisksElements).toEqual(developerDisks);
  });

  it('should show a message when a http error occurs', async () => {
    const developerDisks = ['14.0', '13.1'];

    const scope = nock('http://localhost:9999')
    .get('/api/ios/provisioningProfiles')
    .reply(404, 'test')
    .get('/api/ios/developerDisks')
    .reply(200, developerDisks);

    await page.navigateTo();
    const errors = await page.getErrorItems();
    expect(errors).toEqual(['test']);
  });

  afterEach(async () => {
  });
});
