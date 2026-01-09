import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: '<div class="spinner-center"><mat-spinner></mat-spinner></div>',
  styles: ['.spinner-center { display: flex; justify-content: center; align-items: center; min-height: 400px; }']
})
export class LoadingComponent {}