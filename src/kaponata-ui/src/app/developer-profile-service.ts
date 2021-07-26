import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Identity } from './identity';

@Injectable()
export class DeveloperProfileService {

  constructor(public http: HttpClient) { }

  getDeveloperProfile(): Observable<File> {
    return this.http.get<File>('api/ios/developerProfile');
  }

  uploadDeveloperProfile(file: File, password: string): Observable<object>
  {
    const formData = new FormData();
    formData.append('developerProfile', file);
    formData.append('password', password);
    return this.http.post<any>('api/ios/developerProfile', formData);
  }
}
