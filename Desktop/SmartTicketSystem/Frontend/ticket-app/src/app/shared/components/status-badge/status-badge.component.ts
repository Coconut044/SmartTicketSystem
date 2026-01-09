import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: '<span [class]="\'badge badge-\' + status.toLowerCase().replace(\' \', \'\')">{{ status }}</span>',
  styles: [`
    .badge { padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 500; color: white; }
    .badge-created { background-color: #6c757d; }
    .badge-assigned { background-color: #17a2b8; }
    .badge-inprogress { background-color: #007bff; }
    .badge-resolved { background-color: #28a745; }
    .badge-closed { background-color: #343a40; }
    .badge-cancelled { background-color: #dc3545; }
  `]
})
export class StatusBadgeComponent {
  @Input() status: string = '';
}
