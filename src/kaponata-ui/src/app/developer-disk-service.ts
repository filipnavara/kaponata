import { EventEmitter, Injectable, Output, Version } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ErrorService } from './error.service';

@Injectable()
export class DeveloperDiskService {

  constructor(private http: HttpClient) { }

  getDeveloperDisks() {
    return this.http.get<Version[]>('http://localhost/api/ios/developerDisks')
  }

  importDeveloperDisk(developerDiskFile: File, developerDiskSignatureFile: File) {
    const formData = new FormData();
    formData.append(developerDiskFile.name, developerDiskFile);
    formData.append(developerDiskSignatureFile.name, developerDiskSignatureFile);
    return this.http.post<any>("http://localhost/api/ios/developerDisk", formData);
  }
}