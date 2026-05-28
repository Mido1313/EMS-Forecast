import { Component, computed, inject, input, output, signal } from '@angular/core';
import { ComparisonPeriod } from '../../core/data/comparison-periods';
import { MockForecastService } from '../../core/services/mock-forecast.service';
import { ForecastAreaResult } from '../../core/models/forecast-area-result.model';
import { ForecastSummary } from '../../core/models/forecast-summary.model';
import { HistoricalRecord } from '../../core/models/historical-record.model';
import { IncidentTypeProbability } from '../../core/models/incident-type-probability.model';
import { TimeProfileSegment } from '../../core/models/time-profile-segment.model';
import { DonutChartComponent, DonutSlice } from '../../shared/donut-chart/donut-chart.component';
import { SparklineComponent } from '../../shared/sparkline/sparkline.component';

type RankingMode = 'absolute' | 'per-capita';
type IncidentView = 'bar' | 'donut';

interface AreaSelection {
  areaId: number;
  areaName: string;
}

const DONUT_COLORS = ['#126b62', '#df8b46', '#a0d47f', '#b83255', '#f1cb6c'];

@Component({
  selector: 'app-area-detail-panel',
  imports: [DonutChartComponent, SparklineComponent],
  templateUrl: './area-detail-panel.component.html',
  styleUrl: './area-detail-panel.component.scss',
})
export class AreaDetailPanelComponent {
  private readonly forecastService = inject(MockForecastService);

  readonly selectedForecast = input<ForecastAreaResult | null>(null);
  readonly summary = input.required<ForecastSummary>();
  readonly horizonLabel = input.required<string>();
  readonly horizonHours = input(24);
  readonly incidentFocus = input.required<string>();
  readonly comparisonPeriod = input<ComparisonPeriod>('same-weekday');
  readonly isLoading = input(false);
  readonly comparisonMode = input(false);
  readonly compareHorizonLabel = input('');
  readonly areaSelected = output<AreaSelection>();

  protected readonly rankingMode = signal<RankingMode>('absolute');
  protected readonly incidentView = signal<IncidentView>('bar');

  protected readonly rankedForecasts = computed(() => {
    const forecasts = [...this.summary().ranking];
    if (this.rankingMode() === 'per-capita') {
      return forecasts.sort((a, b) => b.incidentsPerTenThousand - a.incidentsPerTenThousand);
    }
    return forecasts.sort((a, b) => b.expectedIncidents - a.expectedIncidents);
  });

  protected readonly visibleIncidentTypes = computed(() => {
    return (
      this.selectedForecast()?.topIncidentTypes.slice(0, 5) ??
      this.summary().topIncidentTypes.slice(0, 5)
    );
  });

  protected readonly donutSlices = computed((): DonutSlice[] => {
    return this.visibleIncidentTypes().map((item, i) => ({
      label: item.type,
      value: item.expectedIncidents,
      color: DONUT_COLORS[i] ?? '#ccc',
    }));
  });

  protected readonly sparklineData = computed((): number[] => {
    const forecast = this.selectedForecast();
    if (!forecast) return [];
    return this.forecastService.getSparklineData(
      forecast.areaId,
      this.incidentFocus(),
      this.comparisonPeriod(),
    );
  });

  protected readonly historicalData = computed((): HistoricalRecord[] => {
    const areaId = this.selectedForecast()?.areaId ?? null;
    return this.forecastService.getHistoricalData(areaId, this.incidentFocus());
  });

  protected readonly histMax = computed(() =>
    Math.max(...this.historicalData().map((r) => r.actualIncidents), 1),
  );

  protected setRankingMode(mode: RankingMode): void {
    this.rankingMode.set(mode);
  }

  protected setIncidentView(view: IncidentView): void {
    this.incidentView.set(view);
  }

  protected selectArea(forecast: ForecastAreaResult): void {
    this.areaSelected.emit({ areaId: forecast.areaId, areaName: forecast.areaName });
  }

  protected exportCsv(): void {
    const header = 'Rang,Gebiet,Erwartete Einsätze,pro 10.000 EW,Risikostufe,Trend %';
    const rows = this.rankedForecasts().map((f, i) =>
      [
        i + 1,
        `"${f.areaName}"`,
        Math.round(f.expectedIncidents),
        f.incidentsPerTenThousand.toFixed(2),
        f.riskLevel,
        Math.round(f.trendPercent),
      ].join(','),
    );
    const csv = [header, ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `ems-prognose-${this.horizonLabel().replace(/\s/g, '-')}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  protected formatInteger(value: number): string {
    return `${Math.round(value)}`;
  }

  protected formatDecimal(value: number): string {
    return value.toFixed(1).replace('.', ',');
  }

  protected formatPercent(value: number): string {
    const rounded = Math.round(value);
    return `${rounded > 0 ? '+' : ''}${rounded} %`;
  }

  protected trendClass(value: number): string {
    if (value > 2) return 'positive';
    if (value < -2) return 'negative';
    return 'stable';
  }

  protected riskLabel(level: number): string {
    const labels: Record<number, string> = {
      1: 'sehr gering',
      2: 'gering',
      3: 'mittel',
      4: 'erhöht',
      5: 'hoch',
    };
    return labels[level] ?? 'unbekannt';
  }

  protected planningHint(forecast: ForecastAreaResult): string {
    if (forecast.riskLevel >= 4) {
      return 'Erhöhte Einsatzlast erwartet. Reservekapazitäten und angrenzende Gebiete prüfen.';
    }
    if (forecast.riskLevel === 3) {
      return 'Normale bis leicht erhöhte Belastung. Entwicklung weiter beobachten.';
    }
    return 'Keine auffällige Mehrbelastung im gewählten Zeitraum erkennbar.';
  }

  protected incidentBarWidth(incident: IncidentTypeProbability): string {
    const max = Math.max(...this.visibleIncidentTypes().map((item) => item.expectedIncidents), 1);
    return `${Math.max(7, Math.round((incident.expectedIncidents / max) * 100))}%`;
  }

  protected timeProfileBarWidth(segment: TimeProfileSegment): string {
    const max = Math.max(...this.summary().timeProfile.map((item) => item.expectedIncidents), 1);
    return `${Math.max(8, Math.round((segment.expectedIncidents / max) * 100))}%`;
  }

  protected histBarWidth(record: HistoricalRecord): string {
    return `${Math.max(6, Math.round((record.actualIncidents / this.histMax()) * 100))}%`;
  }

  protected isTopArea(forecast: ForecastAreaResult): boolean {
    return this.rankedForecasts()[0]?.areaId === forecast.areaId;
  }

  protected isSelectedArea(forecast: ForecastAreaResult): boolean {
    return this.selectedForecast()?.areaId === forecast.areaId;
  }
}
