import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DeveloperCertificateService } from '../developer-certificate.service';

@Component({
    selector: 'developer-certificate-upload',
    templateUrl: './developer-certificate-upload.component.html'
})

export class DeveloperCertificateUploadComponent {
    formImport: FormGroup | undefined;
    developerCertificateFile: File | undefined;
    developerCertificatePassword: string | undefined;

    @Output() refeshDeveloperCertificates = new EventEmitter<File>();
    constructor(private modalService: NgbModal, private developerCertificateService: DeveloperCertificateService) { }

    openModal(contentModal: any) {
        this.modalService.open(contentModal);
    }

    onFileChange(files: FileList) {
        this.developerCertificateFile = files!.item(0)!;
    }

    onKey(event: any) {
        this.developerCertificatePassword = event.target.value;
    }

    upload(): void {
        this.developerCertificateService.uploadDeveloperCertificate(this.developerCertificateFile!, this.developerCertificatePassword!)
            .subscribe(
                data => {
                    this.refeshDeveloperCertificates.emit();
                    this.modalService.dismissAll();
                },
                err => this.modalService.dismissAll());
    }
}
