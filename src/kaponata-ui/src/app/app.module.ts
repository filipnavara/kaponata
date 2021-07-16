import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faTrash, faSearch, faFileImport, faWindowClose } from '@fortawesome/free-solid-svg-icons';

import { ErrorService } from './error.service';
import { ProvisioningProfileService } from './provisioning-profile.service';

import { AppComponent } from './app.component';
import { SettingsComponent } from './settings.component';
import { FileUploadComponent } from './file-upload/file-upload.component';
import { DeveloperDiskUploadComponent } from './developer-disk-upload/developer-disk-upload.component';
import { PageNotFoundComponent } from './page-not-found.component';

import { AppRoutingModule } from './app-routing.module';
import { httpInterceptorProviders } from './http-interceptors';
import { DeveloperDiskService } from './developer-disk-service';
import { DeveloperCertificateService } from './developer-certificate.service';
import { DeveloperCertificateUploadComponent } from './developer-certificate-upload/developer-certificate-upload';
import { DeveloperProfileService } from './developer-profile-service';
import { DeveloperProfileUploadComponent } from './developer-profile-upload/developer-profile-upload';

@NgModule({
  declarations: [
    AppComponent,
    SettingsComponent,
    PageNotFoundComponent,
    FileUploadComponent,
    DeveloperDiskUploadComponent,
    DeveloperCertificateUploadComponent,
    DeveloperProfileUploadComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
    FontAwesomeModule
  ],
  providers: [
    ErrorService,
    ProvisioningProfileService,
    DeveloperDiskService,
    DeveloperCertificateService,
    DeveloperProfileService,
    httpInterceptorProviders
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor(library: FaIconLibrary) {
    // Add an icon to the library for convenient access in other components
    library.addIcons(faTrash);
    library.addIcons(faSearch);
    library.addIcons(faFileImport);
    library.addIcons(faWindowClose);
  }
}
