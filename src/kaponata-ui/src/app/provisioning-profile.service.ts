import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ProvisioningProfile } from './provisioning-profile';

@Injectable()
export class ProvisioningProfileService {

  constructor(public http: HttpClient) { }

  getProvisioningProfiles(): Observable<ProvisioningProfile[]> {
    return this.http.get<ProvisioningProfile[]>('http://localhost/api/ios/provisioningProfiles');
  }

  uploadProvisioningProfile(file: File): Observable<object>
  {
    return this.http.post(
      'http://localhost/api/ios/provisioningProfiles',
      file,
      {
        headers:
        {
            'content-type': 'application/octet-stream'
        },
      },
    );
  }

  deleteProvisioningProfile(uuid: string): Observable<any>
  {
    const url = `http://localhost/api/ios/provisioningProfiles/${ uuid }`;
    return this.http.delete(url);
  }
}
