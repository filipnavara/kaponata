// @ts-check
// Protractor configuration file, see link for more information
// https://github.com/angular/protractor/blob/master/lib/config.ts

const { SpecReporter, StacktraceOption } = require('jasmine-spec-reporter');
var reporters = require('jasmine-reporters');

/**
 * @type { import("protractor").Config }
 */
exports.config = {
  allScriptsTimeout: 11000,
  specs: [
    './src/**/*.e2e-spec.ts'
  ],
  plugins: [
    {
      package: 'protractor-backend-mock-plugin',
      // config
      backend: [80, 'localhost'],
      fake: [9999, 'localhost']
    }
  ],
  capabilities: {
    browserName: 'chrome',
    chromeOptions: {
      args: ["--headless", "--disable-gpu", "--window-size=800,600","--disable-dev-shm-usage","--no-sandbox"]
    }
  },
  directConnect: true,
  SELENIUM_PROMISE_MANAGER: false,
  baseUrl: 'http://localhost:4200/',
  framework: 'jasmine',
  jasmineNodeOpts: {
    showColors: true,
    defaultTimeoutInterval: 30000,
    print: function () { }
  },
  onPrepare() {
    require('ts-node').register({
      project: require('path').join(__dirname, './tsconfig.json')
    });
    jasmine.getEnv().addReporter(new SpecReporter({
      spec: {
        displayStacktrace: StacktraceOption.PRETTY
      }
    }));

    var junitReporter = new reporters.JUnitXmlReporter({
      savePath: 'TestResults/',
      consolidateAll: false
    });
    jasmine.getEnv().addReporter(junitReporter)
  }
};
