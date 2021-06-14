import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})

export class FileUploadComponent {

  constructor() {
  }

  uploading = false;
  @Input() buttonName = '';
  @Output() uploadFile = new EventEmitter<File>();

  onFileSelect(input: HTMLInputElement): void {
    const files =  input.files;
    if (files == null)
    {
      return;
    }
    else
    {
      const file = files[0];
      this.uploading = true;
      this.uploadFile.emit(file);
      this.uploading = false;
    }
  }
}
