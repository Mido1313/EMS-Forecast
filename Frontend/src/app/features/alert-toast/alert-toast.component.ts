import { Component, computed, effect, input, output, signal } from '@angular/core';
import { ForecastAreaResult } from '../../core/models/forecast-area-result.model';

interface AreaSelection {
  areaId: number;
  areaName: string;
}

@Component({
  selector: 'app-alert-toast',
  templateUrl: './alert-toast.component.html',
  styleUrl: './alert-toast.component.scss',
})
export class AlertToastComponent {
  readonly areas = input.required<ForecastAreaResult[]>();
  readonly areaSelected = output<AreaSelection>();

  protected readonly dismissed = signal(false);
  protected readonly visible = computed(() => !this.dismissed() && this.areas().length > 0);
  protected readonly critical = computed(() => this.areas().filter((a) => a.riskLevel >= 5));
  protected readonly displayAreas = computed(() => this.areas().slice(0, 4));

  private prevKey = '';

  constructor() {
    effect(() => {
      const key = this.areas()
        .map((a) => a.areaId)
        .sort()
        .join(',');
      if (key !== this.prevKey && key !== '') {
        this.prevKey = key;
        this.dismissed.set(false);
      }
    });
  }

  protected dismiss(): void {
    this.dismissed.set(true);
  }

  protected selectArea(area: ForecastAreaResult, event: MouseEvent): void {
    event.stopPropagation();
    this.areaSelected.emit({ areaId: area.areaId, areaName: area.areaName });
  }
}
