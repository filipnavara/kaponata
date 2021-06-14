import { SettingsPage } from './settings.po';
import { browser, logging } from 'protractor';
import { ProvisioningProfile } from '../../src/app/provisioning-profile';
import nock = require('nock');

describe('workspace-project App', () => {
  let page: SettingsPage;
  beforeEach(() => {
    page = new SettingsPage();
  });

  it('should display welcome message', async () => {
    const provisioningProfile = new ProvisioningProfile();
    provisioningProfile.name = 'pp1';

    const provisioningProfiles = [provisioningProfile];

    const scope = nock('http://localhost:9999')
    .get('/api/ios/provisioningProfiles')
    .reply(200, provisioningProfiles);


    await page.navigateTo();
    const text = await page.getTitleText();

    expect(text).toEqual('Settings');

    // Assert that there are no errors emitted from the browser
    const logs = await browser.manage().logs().get(logging.Type.BROWSER);
    expect(logs).not.toContain(jasmine.objectContaining({
      level: logging.Level.SEVERE,
    } as logging.Entry));
  });

  it('should show a message when a http error occurs', async () => {
    const scope = nock('http://localhost:9999')
    .get('/api/ios/provisioningProfiles')
    .reply(404, 'test');

    await page.navigateTo();
    const text = await page.getErrorItems();
    console.log(text);
    expect(text).toEqual(['test']);
  });

  afterEach(async () => {
  });
});
