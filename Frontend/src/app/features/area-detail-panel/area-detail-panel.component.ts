import { Component, computed, input, output, signal } from '@angular/core';
import { ForecastAreaResult } from '../../core/models/forecast-area-result.model';
import { ForecastSummary } from '../../core/models/forecast-summary.model';
import { IncidentTypeProbability } from '../../core/models/incident-type-probability.model';
import { TimeProfileSegment } from '../../core/models/time-profile-segment.model';

type RankingMode = 'absolute' | 'per-capita';

interface AreaSelection {
  areaId: number;
  areaName: string;
}

@Component({
  selector: 'app-area-detail-panel',
  templateUrl: './area-detail-panel.component.html',
  styleUrl: './area-detail-panel.component.scss',
})
export class AreaDetailPanelComponent {
  readonly selectedForecast = input<ForecastAreaResult | null>(null);
  readonly summary = input.required<ForecastSummary>();
  readonly horizonLabel = input.required<string>();
  readonly incidentFocus = input.required<string>();
  readonly areaSelected = output<AreaSelection>();

  protected readonly rankingMode = signal<RankingMode>('absolute');
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

  protected setRankingMode(mode: RankingMode): void {
    this.rankingMode.set(mode);
  }

  protected selectArea(forecast: ForecastAreaResult): void {
    this.areaSelected.emit({ areaId: forecast.areaId, areaName: forecast.areaName });
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

  protected isTopArea(forecast: ForecastAreaResult): boolean {
    return this.rankedForecasts()[0]?.areaId === forecast.areaId;
  }

  protected isSelectedArea(forecast: ForecastAreaResult): boolean {
    return this.selectedForecast()?.areaId === forecast.areaId;
  }
}
