import { Component, EventEmitter, OnInit, Output, Version } from '@angular/core';
import { Injectable } from '@angular/core';
import { ProvisioningProfile } from './provisioning-profile';
import { ProvisioningProfileService } from './provisioning-profile.service';
import { DeveloperDiskService} from './developer-disk-service';
import { ErrorService } from './error.service';
import { catchError } from 'rxjs/operators';

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

  constructor(private provisioningProfileService: ProvisioningProfileService, private developerDiskService: DeveloperDiskService, public errorService: ErrorService) { }

  ngOnInit(): void {
    this.showProvisioningProfiles();
    this.showDeveloperDisks();
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
