import { Component, computed, input } from '@angular/core';

import { createOrderTimeline } from '../models/tracking-view-state';

@Component({
  selector: 'app-order-status-timeline',
  standalone: true,
  templateUrl: './order-status-timeline.html',
  styleUrl: './order-status-timeline.scss'
})
export class OrderStatusTimeline {
  readonly status = input.required<string>();
  protected readonly steps = computed(() => createOrderTimeline(this.status()));
}
