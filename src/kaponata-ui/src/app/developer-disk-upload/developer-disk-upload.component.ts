import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DeveloperDiskService } from '../developer-disk-service';
import { ErrorService } from '../error.service';

@Component({
    selector: 'developer-disk-upload',
    templateUrl: './developer-disk-upload.component.html'
})

export class DeveloperDiskUploadComponent {
    formImport: FormGroup | undefined;
    developerDiskToUpload: File | undefined;
    developerDiskSignatureToUpload: File | undefined;
    @Output() refeshDeveloperDisks = new EventEmitter<File>();

    constructor(private modalService: NgbModal, private developerDiskService: DeveloperDiskService, public errorService: ErrorService)
    { }

    openModal(contentModal: any) {
        this.modalService.open(contentModal);
    }

    onDeveloperDiskSignatureFileChange(files: FileList) {
        this.developerDiskSignatureToUpload = files!.item(0)!;
    }

    onDeveloperDiskFileChange(files: FileList) {
        this.developerDiskToUpload = files!.item(0)!;
    }

    import(): void {
        this.developerDiskService.importDeveloperDisk(this.developerDiskToUpload!, this.developerDiskSignatureToUpload!)
        .pipe(catchError(this.errorService.handleError))
        .subscribe(
            next =>
            {
                this.refeshDeveloperDisks.emit();
                this.modalService.dismissAll();
            },
            (err: string) =>
            {
               this.errorService.addError(err);
               this.modalService.dismissAll();
            });
    }
}