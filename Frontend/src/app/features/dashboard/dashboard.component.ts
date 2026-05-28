import { Component, computed, inject, signal } from '@angular/core';
import { COMPARISON_PERIODS, ComparisonPeriod } from '../../core/data/comparison-periods';
import { FORECAST_STEPS, formatHorizon } from '../../core/data/forecast-steps';
import { MockForecastService } from '../../core/services/mock-forecast.service';
import { AlertToastComponent } from '../alert-toast/alert-toast.component';
import { AreaDetailPanelComponent } from '../area-detail-panel/area-detail-panel.component';
import { ForecastControlsComponent } from '../forecast-controls/forecast-controls.component';
import { ForecastMapComponent } from '../forecast-map/forecast-map.component';

interface AreaSelection {
  areaId: number;
  areaName: string;
}

type MobileTab = 'controls' | 'map' | 'detail';

@Component({
  selector: 'app-dashboard',
  imports: [
    AlertToastComponent,
    AreaDetailPanelComponent,
    ForecastControlsComponent,
    ForecastMapComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  private readonly forecastService = inject(MockForecastService);

  protected readonly forecastSteps = FORECAST_STEPS;
  protected readonly incidentTypes = this.forecastService.incidentTypes;
  protected readonly comparisonPeriods = COMPARISON_PERIODS;

  // Core forecast state
  protected readonly horizonIndex = signal(2);
  protected readonly incidentFocus = signal('Alle');
  protected readonly comparisonPeriod = signal<ComparisonPeriod>('same-weekday');
  protected readonly selectedAreaId = signal<number | null>(null);

  // Comparison mode state
  protected readonly comparisonMode = signal(false);
  protected readonly compareHorizonIndex = signal(8);

  // UI state
  protected readonly isMapLoading = signal(true);
  protected readonly activeMobileTab = signal<MobileTab>('map');

  protected readonly horizonHours = computed(() => this.forecastSteps[this.horizonIndex()] ?? 24);
  protected readonly horizonLabel = computed(() => formatHorizon(this.horizonHours()));
  protected readonly compareHorizonHours = computed(
    () => this.forecastSteps[this.compareHorizonIndex()] ?? 72,
  );
  protected readonly compareHorizonLabel = computed(() =>
    formatHorizon(this.compareHorizonHours()),
  );

  protected readonly forecasts = computed(() =>
    this.forecastService.getForecasts(
      this.horizonHours(),
      this.incidentFocus(),
      this.comparisonPeriod(),
    ),
  );

  protected readonly compareForecasts = computed(() =>
    this.comparisonMode()
      ? this.forecastService.getForecasts(
          this.compareHorizonHours(),
          this.incidentFocus(),
          this.comparisonPeriod(),
        )
      : [],
  );

  protected readonly selectedForecast = computed(() => {
    const id = this.selectedAreaId();
    return this.forecasts().find((f) => f.areaId === id) ?? null;
  });

  protected readonly summary = computed(() =>
    this.forecastService.getSummary(
      this.forecasts(),
      this.horizonHours(),
      this.incidentFocus(),
      this.comparisonPeriod(),
    ),
  );

  protected readonly criticalAreas = computed(() =>
    this.forecasts().filter((f) => f.riskLevel >= 4),
  );

  protected updateHorizonIndex(index: number): void {
    this.horizonIndex.set(index);
  }

  protected updateIncidentFocus(type: string): void {
    this.incidentFocus.set(type);
  }

  protected updateComparisonPeriod(period: ComparisonPeriod): void {
    this.comparisonPeriod.set(period);
  }

  protected updateComparisonMode(active: boolean): void {
    this.comparisonMode.set(active);
  }

  protected updateCompareHorizonIndex(index: number): void {
    this.compareHorizonIndex.set(index);
  }

  protected selectArea(selection: AreaSelection): void {
    this.selectedAreaId.set(selection.areaId);
    this.activeMobileTab.set('detail');
  }

  protected clearSelection(): void {
    this.selectedAreaId.set(null);
  }

  protected onMapLoaded(): void {
    this.isMapLoading.set(false);
  }

  protected setMobileTab(tab: MobileTab): void {
    this.activeMobileTab.set(tab);
  }
}
