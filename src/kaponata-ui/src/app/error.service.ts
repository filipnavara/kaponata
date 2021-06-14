import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';

@Injectable()
export class ErrorService {
    errors: string[] = [];

    constructor() { }

    public clearErrors(): void {
        this.errors = [];
    }

    public addError(error: string): void {
        this.errors.push(error);
    }
}
