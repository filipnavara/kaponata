import { Component } from '@angular/core';
import { Injectable } from '@angular/core';
import { ProvisioningProfile } from './provisioning-profile';
import { ProvisioningProfileService } from './provisioning-profile.service';

@Component({
  selector: 'app-root',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})

@Injectable()
export class SettingsComponent {
  title = 'Settings';
  provisioningProfiles: ProvisioningProfile[] | undefined;

  constructor(private provisioningProfileService: ProvisioningProfileService) {
    this.showProvisioningProfiles();
  }

  showProvisioningProfiles(): void {
    this.provisioningProfileService.getProvisioningProfiles()
      .subscribe((data: ProvisioningProfile[]) => this.provisioningProfiles = data);
  }

  uploadProvisioningProfile(file: File): void {
    this.provisioningProfileService.uploadProvisioningProfile(file).subscribe((data: object) =>
    {
      this.showProvisioningProfiles();
    });
  }

  deleteProvisioningProfile(provisioningProfile: ProvisioningProfile): void{
    if (provisioningProfile) {
      const uuid = provisioningProfile?.uuid!;
      this.provisioningProfileService.deleteProvisioningProfile(uuid).subscribe(() => this.showProvisioningProfiles());
    }
  }
}
