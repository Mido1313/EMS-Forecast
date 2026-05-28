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
  readonly compareForecasts = input<readonly ForecastAreaResult[]>([]);
  readonly comparisonMode = input(false);
  readonly compareHorizonLabel = input('');
  readonly areaSelected = output<AreaSelection>();
  readonly mapLoaded = output<void>();

  @ViewChild('mapContainer', { static: true })
  private readonly mapContainer?: ElementRef<HTMLElement>;

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal('');
  private readonly isGeoJsonLoaded = signal(false);

  private map?: L.Map;
  private geoJsonLayer?: L.GeoJSON<AreaFeatureProperties, Geometry>;
  private forecastByArea = new Map<number, ForecastAreaResult>();
  private compareByArea = new Map<number, ForecastAreaResult>();
  private areaBounds = new Map<number, L.LatLngBounds>();
  private pulseMarkers = new Map<number, L.Marker>();

  constructor() {
    effect(() => {
      this.forecastByArea = new Map(this.forecasts().map((f) => [f.areaId, f]));
      this.refreshLayerStyles();
    });

    effect(() => {
      this.compareByArea = new Map(this.compareForecasts().map((f) => [f.areaId, f]));
      this.refreshLayerStyles();
    });

    effect(() => {
      this.comparisonMode();
      this.refreshLayerStyles();
    });

    effect(() => {
      this.selectedAreaId();
      this.refreshLayerStyles();
    });

    // Auto-fly to selected area when map is ready
    effect(() => {
      const areaId = this.selectedAreaId();
      const loaded = this.isGeoJsonLoaded();
      if (!loaded || areaId === null || !this.map) return;
      const bounds = this.areaBounds.get(areaId);
      if (bounds) {
        const center = bounds.getCenter();
        // One step less than the perfect fit so the area sits in context
        const naturalZoom = this.map.getBoundsZoom(bounds, false);
        const targetZoom = Math.min(10, Math.max(9, naturalZoom - 0.5));
        this.map.flyTo(center, targetZoom, { duration: 0.5 });
      }
    });
  }

  ngAfterViewInit(): void {
    if (!this.mapContainer) return;

    const ooeBounds = L.latLngBounds(L.latLng(47.15, 12.4), L.latLng(49.0, 15.3));

    this.map = L.map(this.mapContainer.nativeElement, {
      attributionControl: false,
      zoomControl: true,
      minZoom: 8.5,
      maxZoom: 13,
      maxBounds: ooeBounds,
      maxBoundsViscosity: 0.85,
      zoomSnap: 0.5,
      preferCanvas: true,
    });

    void this.loadGeoJson();
  }

  ngOnDestroy(): void {
    this.pulseMarkers.forEach((m) => m.remove());
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
        this.map.fitBounds(this.geoJsonLayer.getBounds(), { padding: [32, 32] });
        this.addResetViewControl();
        // Resize pulse rings whenever zoom changes
        this.map.on('zoomend', () => this.updatePulseMarkerSizes());
      }

      this.isLoading.set(false);
      this.isGeoJsonLoaded.set(true);
      this.mapLoaded.emit();
    } catch (error) {
      this.loadError.set(
        error instanceof Error ? error.message : 'GeoJSON konnte nicht geladen werden',
      );
      this.isLoading.set(false);
    }
  }

  private addResetViewControl(): void {
    if (!this.map || !this.geoJsonLayer) return;
    const layer = this.geoJsonLayer;
    const map = this.map;

    const ResetControl = L.Control.extend({
      options: { position: 'topleft' },
      onAdd() {
        const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        const btn = L.DomUtil.create('a', 'reset-view-btn', container);
        btn.title = 'Ansicht zurücksetzen';
        btn.setAttribute('role', 'button');
        btn.setAttribute('tabindex', '0');
        btn.innerHTML =
          '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>';
        L.DomEvent.on(btn, 'click', (e: Event) => {
          L.DomEvent.stopPropagation(e as MouseEvent);
          map.flyToBounds(layer.getBounds(), { padding: [32, 32], duration: 0.5 });
        });
        L.DomEvent.on(btn, 'keydown', (e: Event) => {
          if ((e as KeyboardEvent).key === 'Enter') {
            map.flyToBounds(layer.getBounds(), { padding: [32, 32], duration: 0.5 });
          }
        });
        return container;
      },
    });

    new ResetControl().addTo(this.map);
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
      if (Number.isFinite(numericValue) && numericValue > 0) return numericValue;
    }
    return index + 1;
  }

  private extractAreaName(properties: Partial<AreaFeatureProperties>, areaId: number): string {
    const nameCandidates = [properties.areaName, properties.gebiet, properties.Gebiet];
    const explicitName = nameCandidates.find(
      (c): c is string => typeof c === 'string' && c.trim().length > 0,
    );
    return explicitName ?? this.forecastByArea.get(areaId)?.areaName ?? `Gebiet ${areaId}`;
  }

  private extractMunicipalityName(properties: Partial<AreaFeatureProperties>): string {
    const candidates = [
      properties.name,
      properties.gemeinde,
      properties.gemeindeName,
      properties.Gemeinde,
      properties.NAME,
      properties.municipalityName,
    ];
    const found = candidates.find(
      (c): c is string => typeof c === 'string' && c.trim().length > 0,
    );
    return found?.trim() ?? 'Gemeinde unbekannt';
  }

  private bindFeatureInteractions(feature: AreaFeature, layer: L.Layer): void {
    const areaId = feature.properties.forecastAreaId;
    const areaName = feature.properties.forecastAreaName;
    const featureLayer = layer as FeatureLayer;

    // Accumulate bounds across all municipalities that share this forecastAreaId
    if ('getBounds' in layer) {
      const featureBounds = (layer as L.Polygon).getBounds();
      const existing = this.areaBounds.get(areaId);
      if (existing) {
        existing.extend(featureBounds);
      } else {
        this.areaBounds.set(areaId, L.latLngBounds(featureBounds.getSouthWest(), featureBounds.getNorthEast()));
      }
    }

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
          featureLayer.setStyle({ weight: 3, fillOpacity: 0.9 });
          featureLayer.bringToFront();
        }
      },
      mouseout: () => this.refreshLayerStyles(),
    });
  }

  private refreshLayerStyles(): void {
    if (!this.geoJsonLayer) return;

    this.geoJsonLayer.eachLayer((layer) => {
      const featureLayer = layer as FeatureLayer;
      const feature = featureLayer.feature;
      if (!feature) return;

      const areaId = feature.properties.forecastAreaId;
      const forecast = this.forecastByArea.get(areaId);

      if (featureLayer instanceof L.Path) {
        featureLayer.setStyle(this.getFeatureStyle(feature));
      }
      featureLayer.setTooltipContent(this.getTooltipContent(feature));

      // Pulse markers for elevated/critical risk (hidden in comparison mode)
      const riskLevel = forecast?.riskLevel ?? 0;
      if (riskLevel >= 4 && this.map && !this.comparisonMode()) {
        const bounds = this.areaBounds.get(areaId);
        const center = bounds?.getCenter();
        if (bounds && center) {
          const size = this.computePulseSize(bounds);
          if (this.pulseMarkers.has(areaId)) {
            const m = this.pulseMarkers.get(areaId)!;
            m.setLatLng(center);
            m.setIcon(this.createPulseIcon(riskLevel, size));
          } else {
            const marker = L.marker(center, {
              icon: this.createPulseIcon(riskLevel, size),
              interactive: false,
              zIndexOffset: 400,
            }).addTo(this.map);
            this.pulseMarkers.set(areaId, marker);
          }
        }
      } else {
        const existing = this.pulseMarkers.get(areaId);
        if (existing) {
          existing.remove();
          this.pulseMarkers.delete(areaId);
        }
      }
    });
  }

  private computePulseSize(bounds: L.LatLngBounds): number {
    if (!this.map) return 60;
    const nw = this.map.latLngToLayerPoint(bounds.getNorthWest());
    const se = this.map.latLngToLayerPoint(bounds.getSouthEast());
    const w = Math.abs(se.x - nw.x);
    const h = Math.abs(se.y - nw.y);
    // Ring starts at 85% base size and pulses to ~1.4×, so use 0.75 of shorter dimension
    // so the expanded ring (1.4×) stays close to the area boundary
    const size = Math.min(w, h) * 0.75;
    return Math.round(Math.max(50, Math.min(size, 320)));
  }

  private updatePulseMarkerSizes(): void {
    if (!this.map) return;
    this.pulseMarkers.forEach((marker, areaId) => {
      const bounds = this.areaBounds.get(areaId);
      if (!bounds) return;
      const center = bounds.getCenter();
      const forecast = this.forecastByArea.get(areaId);
      const size = this.computePulseSize(bounds);
      marker.setLatLng(center);
      marker.setIcon(this.createPulseIcon(forecast?.riskLevel ?? 4, size));
    });
  }

  private createPulseIcon(riskLevel: number, size = 40): L.DivIcon {
    const cls = riskLevel >= 5 ? 'level-5' : 'level-4';
    return L.divIcon({
      className: 'risk-pulse-icon',
      html: `<span class="pulse-ring ${cls}" style="width:${size}px;height:${size}px"></span>`,
      iconSize: [size, size],
      iconAnchor: [size / 2, size / 2],
    });
  }

  private getFeatureStyle(feature?: AreaFeature): L.PathOptions {
    const areaId = feature?.properties.forecastAreaId ?? 0;
    const selected = this.selectedAreaId() === areaId;

    let fillColor: string;
    if (this.comparisonMode() && this.compareByArea.size > 0) {
      const base = this.forecastByArea.get(areaId);
      const compare = this.compareByArea.get(areaId);
      const delta = (compare?.riskLevel ?? 0) - (base?.riskLevel ?? 0);
      fillColor = this.getDeltaColor(delta);
    } else {
      const forecast = this.forecastByArea.get(areaId);
      fillColor = this.getRiskColor(forecast?.riskLevel ?? 1);
    }

    return {
      color: selected ? '#14211c' : '#f7faf8',
      fillColor,
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

  private getDeltaColor(delta: number): string {
    if (delta <= -2) return '#0b6b55';
    if (delta === -1) return '#a0d47f';
    if (delta === 0) return '#c4cec8';
    if (delta === 1) return '#df8b46';
    return '#b83255';
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

    if (this.comparisonMode() && this.compareByArea.size > 0) {
      const compare = this.compareByArea.get(areaId);
      const deltaLevel = (compare?.riskLevel ?? 0) - forecast.riskLevel;
      const deltaArrow = deltaLevel > 0 ? '▲' : deltaLevel < 0 ? '▼' : '≈';
      const deltaClass = deltaLevel > 0 ? 'is-rising' : deltaLevel < 0 ? 'is-falling' : '';
      return `
        <div class="tooltip-title">${this.escapeHtml(municipalityName)}</div>
        <div class="tooltip-area">${this.escapeHtml(areaName)}</div>
        <div class="tooltip-grid">
          <span>Basis</span>
          <strong>Stufe ${forecast.riskLevel} | ${Math.round(forecast.expectedIncidents)} Einsätze</strong>
          <span>${this.escapeHtml(this.compareHorizonLabel())}</span>
          <strong>Stufe ${compare?.riskLevel ?? '–'} | ${Math.round(compare?.expectedIncidents ?? 0)} Einsätze</strong>
          <span>Veränderung</span>
          <strong class="${deltaClass}">${deltaArrow} ${Math.abs(deltaLevel) > 0 ? Math.abs(deltaLevel) + ' Stufe(n)' : 'keine'}</strong>
        </div>
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
        <strong class="${forecast.trendPercent >= 0 ? 'is-rising' : 'is-falling'}">${this.formatTrend(forecast.trendPercent)}</strong>
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
