import { Injectable } from '@angular/core';
import {
    HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { ErrorService } from '../error.service';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(private errorService: ErrorService) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(catchError((error) => this.handleError(error, this.errorService)));
    }

    public handleError(error: HttpErrorResponse, errorService: ErrorService): Observable<never> {
        let message: string;
        if (error.error instanceof ErrorEvent) {
            message = error.error.message;
        }
        else if (error.error instanceof ProgressEvent) {
            message = error.message;
        }
        else {
            message = error.error;
        }
        errorService.addError(message);

        return throwError(message);
    }
}
