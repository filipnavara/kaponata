import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Identity } from './identity';

@Injectable()
export class DeveloperCertificateService {

  constructor(public http: HttpClient) { }

  getDeveloperCertificates(): Observable<Identity[]> {
    return this.http.get<Identity[]>('api/ios/identities');
  }

  uploadDeveloperCertificate(file: File, password: string): Observable<object>
  {
    const formData = new FormData();
    formData.append('certificate', file);
    formData.append('password', password);
    return this.http.post<any>('api/ios/identities', formData);
  }

  deleteDeveloperCertificate(thumbprint: string): Observable<any>
  {
    const url = `api/ios/identities/${thumbprint.toLowerCase()}`;
    return this.http.delete(url);
  }
}
