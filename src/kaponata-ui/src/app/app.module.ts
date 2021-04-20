import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { SettingsComponent } from './settings.component';
import { AppRoutingModule} from './app-routing.module';
import { PageNotFoundComponent } from './page-not-found.component';
import { ProvisioningProfileService } from './provisioning-profile.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [
    AppComponent,
    SettingsComponent,
    PageNotFoundComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
  ],
  providers: [
    ProvisioningProfileService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
