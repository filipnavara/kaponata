import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ProvisioningProfile } from './provisioning-profile';

@Injectable()
export class ProvisioningProfileService {

  constructor(public http: HttpClient) { }

  getProvisioningProfiles(): Observable<ProvisioningProfile[]> {
    return this.http.get<ProvisioningProfile[]>('/api/ios/provisioningProfiles');
  }
}
