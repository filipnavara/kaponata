import { Component, OnInit, Version } from '@angular/core';
import { Injectable } from '@angular/core';
import { DeveloperCertificateService } from './developer-certificate.service';
import { DeveloperDiskService } from './developer-disk-service';
import { Identity } from './identity';
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
  developerCertificates: Identity[] | undefined;
  developerDisks: Version[] | undefined;
  license: string | undefined;

  constructor(
    private provisioningProfileService: ProvisioningProfileService,
    private developerCertificateService: DeveloperCertificateService,
    private developerDiskService: DeveloperDiskService) { }

  ngOnInit(): void {
    this.showProvisioningProfiles();
    this.showDeveloperDisks();
    this.showDeveloperCertificates();
  }

  showDeveloperCertificates(): void {
    this.developerCertificateService.getDeveloperCertificates()
      .subscribe(
        (data: Identity[]) => this.developerCertificates = data);
  }

  deleteDeveloperCertificate(developerCertificate: Identity): void {
    if (developerCertificate) {
      const thumbprint = developerCertificate?.thumbprint!;
      this.developerCertificateService.deleteDeveloperCertificate(thumbprint)
        .subscribe(() => this.showDeveloperCertificates());
    }
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
