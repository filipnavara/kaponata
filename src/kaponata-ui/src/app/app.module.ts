import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';

import { AppRoutingModule} from './app-routing.module';
import { PageNotFoundComponent } from './page-not-found.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { FileUploadComponent } from './file-upload/file-upload.component';
import { DeveloperDiskUploadComponent } from './developer-disk-upload/developer-disk-upload.component';
import { SettingsComponent } from './settings.component';

import { ProvisioningProfileService } from './provisioning-profile.service';
import { DeveloperDiskService } from './developer-disk-service';

import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faTrash, faSearch, faFileImport, faWindowClose } from '@fortawesome/free-solid-svg-icons';
import { ErrorService } from './error.service';

@NgModule({
  declarations: [
    AppComponent,
    SettingsComponent,
    PageNotFoundComponent,
    FileUploadComponent,
    DeveloperDiskUploadComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
    FontAwesomeModule,
  ],
  providers: [
    ProvisioningProfileService,
    DeveloperDiskService,
    ErrorService
  ],
  bootstrap: [AppComponent]
})
export class AppModule
{
  constructor(library: FaIconLibrary) {
      // Add an icon to the library for convenient access in other components
      library.addIcons(faTrash);
      library.addIcons(faSearch);
      library.addIcons(faFileImport);
      library.addIcons(faWindowClose);
  }
}
