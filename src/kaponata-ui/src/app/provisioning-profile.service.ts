import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ProvisioningProfile } from './provisioning-profile';

@Injectable()
export class ProvisioningProfileService {

  constructor(public http: HttpClient) { }

  getProvisioningProfiles(): Observable<ProvisioningProfile[]> {
    return this.http.get<ProvisioningProfile[]>('http://localhost/api/ios/provisioningProfiles')
      .pipe(
        catchError(this.handleError)
      );
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
    ).pipe(
      catchError(this.handleError)
    );
  }

  deleteProvisioningProfile(uuid: string): Observable<any>
  {
    const url = `http://localhost/api/ios/provisioningProfiles/${ uuid }`;
    return this.http.delete(url).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, ` +
        `body was: ${error.error}`);
    }
    // Return an observable with a user-facing error message.
    return throwError(
      'Something bad happened; please try again later.');
  }
}
