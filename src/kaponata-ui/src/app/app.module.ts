import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { SettingsComponent } from './settings.component';
import { AppRoutingModule} from './app-routing.module';
import { PageNotFoundComponent } from './page-not-found.component';
import { ProvisioningProfileService } from './provisioning-profile.service';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FileUploadComponent } from './file-upload/file-upload.component';

import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faTrash } from '@fortawesome/free-solid-svg-icons';

@NgModule({
  declarations: [
    AppComponent,
    SettingsComponent,
    PageNotFoundComponent,
    FileUploadComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
    FontAwesomeModule,
  ],
  providers: [
    ProvisioningProfileService
  ],
  bootstrap: [AppComponent]
})
export class AppModule
{
  constructor(library: FaIconLibrary) {
      // Add an icon to the library for convenient access in other components
      library.addIcons(faTrash);
  }
}
