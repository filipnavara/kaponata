import { Component, OnInit, Version } from '@angular/core';
import { Injectable } from '@angular/core';
import { DeveloperDiskService } from './developer-disk-service';
import { ProvisioningProfile } from './provisioning-profile';
import { ProvisioningProfileService } from './provisioning-profile.service';

@Component({
  selector: 'app-root',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})

@Injectable()
export class SettingsComponent implements OnInit {
  title = 'Settings';
  provisioningProfiles: ProvisioningProfile[] | undefined;
  developerDisks: Version[] | undefined;
  license: string | undefined;

  constructor(
    private provisioningProfileService: ProvisioningProfileService,
    private developerDiskService: DeveloperDiskService) { }

  ngOnInit(): void {
    this.showProvisioningProfiles();
    this.showDeveloperDisks();
  }

  showDeveloperDisks(): void {
    this.developerDiskService.getDeveloperDisks()
      .subscribe(
        (data: Version[]) => this.developerDisks = data);
  }

  showProvisioningProfiles(): void {
    this.provisioningProfileService.getProvisioningProfiles()
      .subscribe((data: ProvisioningProfile[]) => this.provisioningProfiles = data);
  }

  uploadProvisioningProfile(file: File): void {
    this.provisioningProfileService.uploadProvisioningProfile(file)
      .subscribe(() => this.showProvisioningProfiles());
  }

  deleteProvisioningProfile(provisioningProfile: ProvisioningProfile): void {
    if (provisioningProfile) {
      const uuid = provisioningProfile?.uuid!;
      this.provisioningProfileService.deleteProvisioningProfile(uuid)
        .subscribe(() => this.showProvisioningProfiles());
    }
  }
}
