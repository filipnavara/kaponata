import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DeveloperProfileService } from '../developer-profile-service';

@Component({
    selector: 'developer-profile-upload',
    templateUrl: './developer-profile-upload.component.html'
})

export class DeveloperProfileUploadComponent {
    formImport: FormGroup | undefined;
    developerProfileFile: File | undefined;
    developerProfilePassword: string | undefined;

    @Output() refeshDeveloperCertificates = new EventEmitter();
    @Output() refeshProvisioningProfiles = new EventEmitter();
    constructor(private modalService: NgbModal, private developerProfileService: DeveloperProfileService) { }

    openModal(contentModal: any) {
        this.modalService.open(contentModal);
    }

    onFileChange(files: FileList) {
        this.developerProfileFile = files!.item(0)!;
    }

    onKey(event: any) {
        this.developerProfilePassword = event.target.value;
    }

    upload(): void {
        this.developerProfileService.uploadDeveloperProfile(this.developerProfileFile!, this.developerProfilePassword!)
            .subscribe(
                data => {
                    this.refeshDeveloperCertificates.emit();
                    this.refeshProvisioningProfiles.emit();
                    this.modalService.dismissAll();
                },
                err => this.modalService.dismissAll());
    }
}
