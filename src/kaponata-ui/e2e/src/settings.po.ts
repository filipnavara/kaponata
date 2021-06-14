import { Console } from 'console';
import { browser, by, element, ElementArrayFinder } from 'protractor';

export class SettingsPage {
  async navigateTo(): Promise<unknown> {
    return browser.get('/settings');
  }

  async getTitleText(): Promise<string> {
    return element(by.css('h1')).getText();
  }

  async getErrorItems(): Promise<string[]>{
    const errorlist = element(by.id('errorlist'));

    const errorElements = errorlist.all(by.css('li'));
    return errorElements.map<string>((el =>  el!.getText()));
  }
}
