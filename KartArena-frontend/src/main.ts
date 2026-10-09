import { platformBrowser } from '@angular/platform-browser';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs';
import { AppModule } from './app/app-module';
import { BUSINESS_CULTURE } from './app/core/services/business-date-time';

import 'zone.js'; // Razvoj softvera 1 setup, prethodno instalirati "npm install zone.js"

// Register Bosnian locale for currency and date formatting
registerLocaleData(localeBs, BUSINESS_CULTURE);

platformBrowser().bootstrapModule(AppModule, {

})
  .catch(err => console.error(err));
