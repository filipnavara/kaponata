import { Console } from 'console';
import { browser, by, element, ElementArrayFinder } from 'protractor';

export class SettingsPage {
  async navigateTo(): Promise<unknown> {
    return browser.get('/settings');
  }

  async getTitleText(): Promise<string> {
    return element(by.css('h1')).getText();
  }

  async getDeveloperDisks(): Promise<string[]> {
    const developerDiskList = element(by.id('developer-disk-list'));

    const developerDisks = developerDiskList.all(by.css('li'));
    return developerDisks.map<string>((el =>  el!.getText()));
  }

  async getProvisioningProfiles(): Promise<string[]> {
    const provisioningProfileTable = element(by.id('provisioning-profile-table-body'));

    const provisioningProfiles = provisioningProfileTable.all(by.css('tr')).all(by.css('td'));
    return provisioningProfiles.map<string>((el =>  el!.getText()));
  }

  async getErrorItems(): Promise<string[]>{
    const errorlist = element(by.id('errorlist'));

    const errorElements = errorlist.all(by.css('li'));
    return errorElements.map<string>((el =>  el!.getText()));
  }
}
