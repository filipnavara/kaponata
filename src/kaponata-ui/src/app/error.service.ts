import { HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, throwError } from "rxjs";

@Injectable({
    providedIn: 'any',


  })
export class ErrorService {
    errors: string[] = [];

  constructor() { }

  public clearErrors(): void {
    this.errors = [];
  }

  public addError(error: string): void{
    this.errors.push(error);
  }

  public handleError(error: HttpErrorResponse): Observable<never> {
    let message: string;
    if (error.error instanceof ErrorEvent) {
      message = error.error.message;
    }
    else if (error.error instanceof ProgressEvent)
    {
      message = error.message;
    }
    else {
      message = error.error;
    }

    // Return an observable with a user-facing error message.
    return throwError(message);
  }
}
