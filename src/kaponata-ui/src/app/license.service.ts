import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable } from 'rxjs';
import { Options } from 'selenium-webdriver/chrome';

@Injectable()
export class LicenseService {

  constructor(public http: HttpClient) { }

  getProvisioningProfiles(): Observable<string> {
    return this.http.get('http://localhost/api/license', {responseType: 'text'});
  }

  uploadLicense(file: File): Observable<object>
  {
    const formData = new FormData();
    formData.append('license', file);
    return this.http.post<any>('http://localhost/api/license', formData);
  }

  parseLicense(data: string): string {
    const parser = new DOMParser();
    const license = parser.parseFromString(data, 'application/xml');
    return license.getElementsByTagName('Expiration')[0].innerHTML;
  }
}
