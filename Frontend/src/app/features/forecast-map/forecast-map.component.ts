import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import * as L from 'leaflet';
import type { Feature, FeatureCollection, Geometry } from 'geojson';
import { ForecastAreaResult } from '../../core/models/forecast-area-result.model';

interface AreaFeatureProperties {
  areaId?: number | string;
  areaName?: string;
  gebietId?: number | string;
  gebietID?: number | string;
  id?: number | string;
  ID?: number | string;
  name?: string;
  gemeinde?: string;
  gemeindeName?: string;
  Gemeinde?: string;
  NAME?: string;
  gebiet?: string;
  Gebiet?: string;
  municipalityName?: string;
  forecastAreaId: number;
  forecastAreaName: string;
  forecastMunicipalityName: string;
  [key: string]: unknown;
}

interface AreaSelection {
  areaId: number;
  areaName: string;
}

type AreaFeature = Feature<Geometry, AreaFeatureProperties>;
type AreaFeatureCollection = FeatureCollection<Geometry, AreaFeatureProperties>;
type FeatureLayer = L.Layer & { feature?: AreaFeature };

@Component({
  selector: 'app-forecast-map',
  templateUrl: './forecast-map.component.html',
  styleUrl: './forecast-map.component.scss',
})
export class ForecastMapComponent implements AfterViewInit, OnDestroy {
  readonly forecasts = input<readonly ForecastAreaResult[]>([]);
  readonly selectedAreaId = input<number | null>(null);
  readonly areaSelected = output<AreaSelection>();

  @ViewChild('mapContainer', { static: true })
  private readonly mapContainer?: ElementRef<HTMLElement>;

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal('');

  private map?: L.Map;
  private geoJsonLayer?: L.GeoJSON<AreaFeatureProperties, Geometry>;
  private forecastByArea = new Map<number, ForecastAreaResult>();

  constructor() {
    effect(() => {
      this.forecastByArea = new Map(this.forecasts().map((forecast) => [forecast.areaId, forecast]));
      this.refreshLayerStyles();
    });

    effect(() => {
      this.selectedAreaId();
      this.refreshLayerStyles();
    });
  }

  ngAfterViewInit(): void {
    if (!this.mapContainer) {
      return;
    }

    this.map = L.map(this.mapContainer.nativeElement, {
      attributionControl: false,
      zoomControl: true,
      minZoom: 7,
      maxZoom: 11,
      preferCanvas: true,
    });

    void this.loadGeoJson();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private async loadGeoJson(): Promise<void> {
    try {
      this.isLoading.set(true);
      this.loadError.set('');

      const response = await fetch('/assets/geo/gebiete.geojson');

      if (!response.ok) {
        throw new Error(`GeoJSON konnte nicht geladen werden (${response.status})`);
      }

      const collection = (await response.json()) as FeatureCollection<Geometry>;
      const normalizedCollection = this.normalizeFeatureCollection(collection);

      this.geoJsonLayer = L.geoJSON<AreaFeatureProperties, Geometry>(normalizedCollection, {
        style: (feature) => this.getFeatureStyle(feature),
        onEachFeature: (feature, layer) => this.bindFeatureInteractions(feature, layer),
      });

      if (this.map) {
        this.geoJsonLayer.addTo(this.map);
        this.map.fitBounds(this.geoJsonLayer.getBounds(), { padding: [22, 22] });
      }

      this.isLoading.set(false);
    } catch (error) {
      this.loadError.set(
        error instanceof Error ? error.message : 'GeoJSON konnte nicht geladen werden',
      );
      this.isLoading.set(false);
    }
  }

  private normalizeFeatureCollection(
    collection: FeatureCollection<Geometry>,
  ): AreaFeatureCollection {
    return {
      ...collection,
      features: collection.features.map((feature, index) => {
        const properties = feature.properties ?? {};
        const normalizedProperties = properties as Partial<AreaFeatureProperties>;
        const areaId = this.extractAreaId(normalizedProperties, index);
        const areaName = this.extractAreaName(normalizedProperties, areaId);
        const municipalityName = this.extractMunicipalityName(normalizedProperties);

        return {
          ...feature,
          properties: {
            ...normalizedProperties,
            forecastAreaId: areaId,
            forecastAreaName: areaName,
            forecastMunicipalityName: municipalityName,
          },
        };
      }),
    };
  }

  private extractAreaId(properties: Partial<AreaFeatureProperties>, index: number): number {
    const idCandidates = [
      properties.areaId,
      properties.gebietId,
      properties.gebietID,
      properties.id,
      properties.ID,
      properties.name,
      properties.gebiet,
      properties.Gebiet,
    ];

    for (const candidate of idCandidates) {
      const numericValue = Number(candidate);
      if (Number.isFinite(numericValue) && numericValue > 0) {
        return numericValue;
      }
    }

    // Fallback fuer GeoJSON-Dateien ohne stabile Gebiet-ID.
    return index + 1;
  }

  private extractAreaName(properties: Partial<AreaFeatureProperties>, areaId: number): string {
    const nameCandidates = [
      properties.areaName,
      properties.gebiet,
      properties.Gebiet,
    ];

    const explicitName = nameCandidates.find((candidate): candidate is string => {
      return typeof candidate === 'string' && candidate.trim().length > 0;
    });

    return explicitName ?? this.forecastByArea.get(areaId)?.areaName ?? `Gebiet ${areaId}`;
  }

  private extractMunicipalityName(properties: Partial<AreaFeatureProperties>): string {
    const municipalityCandidates = [
      properties.name,
      properties.gemeinde,
      properties.gemeindeName,
      properties.Gemeinde,
      properties.NAME,
      properties.municipalityName,
    ];

    const explicitName = municipalityCandidates.find((candidate): candidate is string => {
      return typeof candidate === 'string' && candidate.trim().length > 0;
    });

    return explicitName?.trim() ?? 'Gemeinde unbekannt';
  }

  private bindFeatureInteractions(feature: AreaFeature, layer: L.Layer): void {
    const areaId = feature.properties.forecastAreaId;
    const areaName = feature.properties.forecastAreaName;
    const featureLayer = layer as FeatureLayer;

    featureLayer.bindTooltip(this.getTooltipContent(feature), {
      className: 'area-tooltip',
      direction: 'auto',
      offset: L.point(14, 0),
      sticky: true,
    });

    featureLayer.on({
      click: () => this.areaSelected.emit({ areaId, areaName }),
      mouseover: () => {
        if (featureLayer instanceof L.Path) {
          featureLayer.setStyle({ weight: 3, fillOpacity: 0.86 });
          featureLayer.bringToFront();
        }
      },
      mouseout: () => this.refreshLayerStyles(),
    });
  }

  private refreshLayerStyles(): void {
    if (!this.geoJsonLayer) {
      return;
    }

    this.geoJsonLayer.eachLayer((layer) => {
      const featureLayer = layer as FeatureLayer;
      const feature = featureLayer.feature;

      if (!feature) {
        return;
      }

      if (featureLayer instanceof L.Path) {
        featureLayer.setStyle(this.getFeatureStyle(feature));
      }

      featureLayer.setTooltipContent(this.getTooltipContent(feature));
    });
  }

  private getFeatureStyle(feature?: AreaFeature): L.PathOptions {
    const areaId = feature?.properties.forecastAreaId ?? 0;
    const forecast = this.forecastByArea.get(areaId);
    const selected = this.selectedAreaId() === areaId;

    return {
      color: selected ? '#14211c' : '#f7faf8',
      fillColor: this.getRiskColor(forecast?.riskLevel ?? 1),
      fillOpacity: selected ? 0.9 : 0.76,
      opacity: selected ? 0.9 : 0.38,
      weight: selected ? 1.2 : 0.45,
    };
  }

  private getRiskColor(level: number): string {
    const colors: Record<number, string> = {
      1: '#8fcfc5',
      2: '#a0d47f',
      3: '#f1cb6c',
      4: '#df8b46',
      5: '#b83255',
    };

    return colors[level] ?? colors[1];
  }

  private getTooltipContent(feature: AreaFeature): string {
    const areaId = feature.properties.forecastAreaId;
    const areaName = feature.properties.forecastAreaName || `Gebiet ${areaId}`;
    const municipalityName = feature.properties.forecastMunicipalityName || 'Gemeinde unbekannt';
    const forecast = this.forecastByArea.get(areaId);

    if (!forecast) {
      return `
        <div class="tooltip-title">${this.escapeHtml(municipalityName)}</div>
        <div class="tooltip-area">${this.escapeHtml(areaName)}</div>
        <div class="tooltip-muted">Keine Prognose verfügbar</div>
      `;
    }

    return `
      <div class="tooltip-title">${this.escapeHtml(municipalityName)}</div>
      <div class="tooltip-area">${this.escapeHtml(areaName)}</div>
      <div class="tooltip-grid">
        <span>Erwartete Einsätze</span>
        <strong>${Math.round(forecast.expectedIncidents)}</strong>
        <span>pro 10.000 EW</span>
        <strong>${forecast.incidentsPerTenThousand.toFixed(1).replace('.', ',')}</strong>
        <span>Risikostufe</span>
        <strong>Stufe ${forecast.riskLevel}</strong>
        <span>Trend</span>
        <strong class="${forecast.trendPercent >= 0 ? 'is-rising' : 'is-falling'}">${this.formatTrend(
          forecast.trendPercent,
        )}</strong>
      </div>
    `;
  }

  private formatTrend(value: number): string {
    const rounded = Math.round(value);
    return `${rounded > 0 ? '+' : ''}${rounded} %`;
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}
