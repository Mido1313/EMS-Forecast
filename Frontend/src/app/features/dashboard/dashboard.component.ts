import { Component, computed, inject, signal } from '@angular/core';
import { COMPARISON_PERIODS, ComparisonPeriod } from '../../core/data/comparison-periods';
import { FORECAST_STEPS, formatHorizon } from '../../core/data/forecast-steps';
import { MockForecastService } from '../../core/services/mock-forecast.service';
import { AreaDetailPanelComponent } from '../area-detail-panel/area-detail-panel.component';
import { ForecastControlsComponent } from '../forecast-controls/forecast-controls.component';
import { ForecastMapComponent } from '../forecast-map/forecast-map.component';

interface AreaSelection {
  areaId: number;
  areaName: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [AreaDetailPanelComponent, ForecastControlsComponent, ForecastMapComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  private readonly forecastService = inject(MockForecastService);

  protected readonly forecastSteps = FORECAST_STEPS;
  protected readonly incidentTypes = this.forecastService.incidentTypes;
  protected readonly comparisonPeriods = COMPARISON_PERIODS;
  protected readonly horizonIndex = signal(2);
  protected readonly incidentFocus = signal('Alle');
  protected readonly comparisonPeriod = signal<ComparisonPeriod>('same-weekday');
  protected readonly selectedAreaId = signal<number | null>(null);

  protected readonly horizonHours = computed(() => this.forecastSteps[this.horizonIndex()] ?? 24);
  protected readonly horizonLabel = computed(() => formatHorizon(this.horizonHours()));
  protected readonly forecasts = computed(() =>
    this.forecastService.getForecasts(
      this.horizonHours(),
      this.incidentFocus(),
      this.comparisonPeriod(),
    ),
  );
  protected readonly selectedForecast = computed(() => {
    const selectedAreaId = this.selectedAreaId();
    return this.forecasts().find((forecast) => forecast.areaId === selectedAreaId) ?? null;
  });
  protected readonly summary = computed(() =>
    this.forecastService.getSummary(
      this.forecasts(),
      this.horizonHours(),
      this.incidentFocus(),
      this.comparisonPeriod(),
    ),
  );

  protected updateHorizonIndex(index: number): void {
    this.horizonIndex.set(index);
  }

  protected updateIncidentFocus(incidentType: string): void {
    this.incidentFocus.set(incidentType);
  }

  protected updateComparisonPeriod(comparisonPeriod: ComparisonPeriod): void {
    this.comparisonPeriod.set(comparisonPeriod);
  }

  protected selectArea(selection: AreaSelection): void {
    this.selectedAreaId.set(selection.areaId);
  }

  protected clearSelection(): void {
    this.selectedAreaId.set(null);
  }
}
