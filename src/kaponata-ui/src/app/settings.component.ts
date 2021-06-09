import { Component, EventEmitter, OnInit, Output, Version } from '@angular/core';
import { Injectable } from '@angular/core';
import { ProvisioningProfile } from './provisioning-profile';
import { ProvisioningProfileService } from './provisioning-profile.service';
import { DeveloperDiskService} from './developer-disk-service';
import { ErrorService } from './error.service';
import { catchError } from 'rxjs/operators';
import { LicenseService } from './license.service';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})

@Injectable()
export class SettingsComponent implements OnInit{
  title = 'Settings';
  provisioningProfiles: ProvisioningProfile[] | undefined;
  developerDisks: Version[] | undefined;
  license: string | undefined;

  constructor(
    private provisioningProfileService: ProvisioningProfileService,
    private developerDiskService: DeveloperDiskService,
    public errorService: ErrorService,
    public licenseService: LicenseService) { }

  ngOnInit(): void {
    this.showProvisioningProfiles();
    this.showDeveloperDisks();
    this.showLicense();
  }

  showLicense(): void{
    this.licenseService.getProvisioningProfiles()
    .pipe(catchError((error: HttpErrorResponse) =>
    {
      if (error.status === HttpStatusCode.NotFound)
      {
        return new Observable();
      }
      else
      {
        return this.errorService.handleError;
      }
    }))
    .subscribe(
      data => {
        this.license = this.licenseService.parseLicense(data as string);
      },
      err =>
      {
          this.errorService.addError(err);
      });
  }

  uploadLicense(file: File): void {
    this.licenseService.uploadLicense(file)
    .pipe(catchError(this.errorService.handleError))
    .subscribe(
      res => this.showLicense(),
      err =>
      {
          this.errorService.addError(err);
      });
  }

  showDeveloperDisks(): void {
    this.developerDiskService.getDeveloperDisks()
    .pipe(catchError(this.errorService.handleError))
    .subscribe(
      (data: Version[]) => this.developerDisks = data,
      (err: string) =>
      {
          this.errorService.addError(err);
      });
  }

  showProvisioningProfiles(): void {
    this.provisioningProfileService.getProvisioningProfiles()
    .pipe(catchError(this.errorService.handleError))
    .subscribe(
      (data: ProvisioningProfile[]) => this.provisioningProfiles = data,
      (err: string) =>
      {
          this.errorService.addError(err);
      });
  }

  uploadProvisioningProfile(file: File): void {
    this.provisioningProfileService.uploadProvisioningProfile(file)
    .pipe(catchError(this.errorService.handleError))
    .subscribe(
      next => this.showProvisioningProfiles(),
      (err: string) =>
      {
          this.errorService.addError(err);
      });
  }

  deleteProvisioningProfile(provisioningProfile: ProvisioningProfile): void{
    if (provisioningProfile) {
      const uuid = provisioningProfile?.uuid!;
      this.provisioningProfileService.deleteProvisioningProfile(uuid)
      .pipe(catchError(this.errorService.handleError))
      .subscribe(
        next => this.showProvisioningProfiles(),
        (err: string) =>
        {
            this.errorService.addError(err);
        });
    }
  }
}
