import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  imports: [CommonModule],
  template: '<span [class]="\'badge badge-\' + priority.toLowerCase()">{{ priority }}</span>',
  styles: [`
    .badge { padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 500; color: white; }
    .badge-low { background-color: #28a745; }
    .badge-medium { background-color: #ffc107; color: #000; }
    .badge-high { background-color: #fd7e14; }
    .badge-critical { background-color: #dc3545; }
  `]
})
export class PriorityBadgeComponent {
  @Input() priority: string = '';
}
