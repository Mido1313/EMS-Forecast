import { Component, computed, input } from '@angular/core';

export interface DonutSlice {
  label: string;
  value: number;
  color: string;
}

@Component({
  selector: 'app-donut-chart',
  template: `
    <div class="donut-wrap">
      <svg viewBox="-50 -50 100 100" width="100" height="100" class="donut-svg">
        @for (seg of segments(); track seg.label) {
          <circle
            r="36"
            cx="0"
            cy="0"
            fill="none"
            [attr.stroke]="seg.color"
            stroke-width="18"
            [attr.stroke-dasharray]="seg.dashArray"
            [attr.stroke-dashoffset]="seg.dashOffset"
          />
        }
        <circle r="26" cx="0" cy="0" fill="var(--surface, #fff)" />
      </svg>
      <ul class="donut-legend">
        @for (seg of slices(); track seg.label) {
          <li>
            <span class="dot" [style.background]="seg.color"></span>
            <span class="leg-label">{{ seg.label }}</span>
            <strong>{{ pct(seg.value) }}</strong>
          </li>
        }
      </ul>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }
      .donut-wrap {
        display: flex;
        align-items: center;
        gap: 14px;
        flex-wrap: wrap;
      }
      .donut-svg {
        display: block;
        flex-shrink: 0;
        transform: rotate(-90deg);
      }
      .donut-legend {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 6px;
        min-width: 0;
        flex: 1;
      }
      .donut-legend li {
        display: flex;
        align-items: center;
        gap: 7px;
        font-size: 0.8rem;
        min-width: 0;
      }
      .dot {
        display: block;
        width: 8px;
        height: 8px;
        border-radius: 50%;
        flex-shrink: 0;
      }
      .leg-label {
        flex: 1;
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        color: var(--text-muted);
      }
      strong {
        flex-shrink: 0;
        font-size: 0.8rem;
      }
    `,
  ],
})
export class DonutChartComponent {
  readonly slices = input.required<DonutSlice[]>();

  private readonly r = 36;
  private readonly c = 2 * Math.PI * this.r;

  private readonly total = computed(() =>
    this.slices().reduce((s, item) => s + item.value, 0) || 1,
  );

  protected readonly segments = computed(() => {
    let cumulative = 0;
    return this.slices().map((slice) => {
      const length = (slice.value / this.total()) * this.c;
      const dashOffset = this.c - cumulative;
      cumulative += length;
      return {
        ...slice,
        dashArray: `${length.toFixed(2)} ${(this.c - length).toFixed(2)}`,
        dashOffset: dashOffset.toFixed(2),
      };
    });
  });

  protected pct(value: number): string {
    return `${Math.round((value / this.total()) * 100)} %`;
  }
}
