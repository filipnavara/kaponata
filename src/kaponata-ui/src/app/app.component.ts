import { Component, Injectable, OnInit } from '@angular/core';
import { ErrorService } from './error.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})

export class AppComponent{
  title = 'kaponata-ui';
  constructor(public errorService: ErrorService) { }
}
