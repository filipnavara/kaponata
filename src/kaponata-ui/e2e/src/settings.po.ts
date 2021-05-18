import { browser, by, element } from 'protractor';

export class SettingsPage {
  async navigateTo(): Promise<unknown> {
    return browser.get('/settings');
  }

  async getTitleText(): Promise<string> {
    return element(by.css('h1')).getText();
  }
}
