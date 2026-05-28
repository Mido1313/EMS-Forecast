import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-sparkline',
  template: `
    <svg
      [attr.viewBox]="'0 0 ' + width() + ' ' + height()"
      [attr.width]="width()"
      [attr.height]="height()"
      class="sparkline"
    >
      @if (points()) {
        <defs>
          <linearGradient [attr.id]="gradId()" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0%" stop-color="currentColor" stop-opacity="0.18" />
            <stop offset="100%" stop-color="currentColor" stop-opacity="0" />
          </linearGradient>
        </defs>
        <polygon [attr.points]="areaPoints()" [attr.fill]="'url(#' + gradId() + ')'" />
        <polyline
          [attr.points]="points()"
          fill="none"
          stroke="currentColor"
          stroke-width="1.8"
          stroke-linejoin="round"
          stroke-linecap="round"
        />
        @if (endPoint()) {
          <circle
            [attr.cx]="endPoint()!.x"
            [attr.cy]="endPoint()!.y"
            r="2.8"
            fill="currentColor"
          />
        }
      }
    </svg>
  `,
  styles: [`:host { display: block; } .sparkline { display: block; overflow: visible; }`],
})
export class SparklineComponent {
  readonly data = input.required<number[]>();
  readonly width = input(150);
  readonly height = input(44);

  private readonly pad = 4;

  private coords = computed((): { x: number; y: number }[] => {
    const d = this.data();
    if (d.length < 2) return [];
    const w = this.width();
    const h = this.height();
    const p = this.pad;
    const min = Math.min(...d);
    const max = Math.max(...d);
    const range = max - min || 1;
    return d.map((v, i) => ({
      x: p + (i / (d.length - 1)) * (w - p * 2),
      y: h - p - ((v - min) / range) * (h - p * 2),
    }));
  });

  protected readonly points = computed(() =>
    this.coords()
      .map((c) => `${c.x.toFixed(1)},${c.y.toFixed(1)}`)
      .join(' '),
  );

  protected readonly areaPoints = computed(() => {
    const cs = this.coords();
    if (!cs.length) return '';
    const h = this.height();
    const p = this.pad;
    const line = cs.map((c) => `${c.x.toFixed(1)},${c.y.toFixed(1)}`).join(' ');
    return `${line} ${cs[cs.length - 1].x.toFixed(1)},${h - p} ${cs[0].x.toFixed(1)},${h - p}`;
  });

  protected readonly endPoint = computed(() => {
    const cs = this.coords();
    return cs.length ? cs[cs.length - 1] : null;
  });

  protected readonly gradId = computed(() => `sg-${this.width()}-${this.height()}`);
}
