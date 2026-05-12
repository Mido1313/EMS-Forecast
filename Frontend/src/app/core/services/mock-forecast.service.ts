import { Injectable } from '@angular/core';
import { AREA_DEFINITIONS } from '../data/area-definitions';
import { ComparisonPeriod, comparisonPeriodLabel } from '../data/comparison-periods';
import { INCIDENT_TYPES } from '../data/incident-types';
import {
  ForecastAreaResult,
  ForecastTrend,
  RiskLevel,
} from '../models/forecast-area-result.model';
import { ForecastSummary } from '../models/forecast-summary.model';
import { IncidentTypeProbability } from '../models/incident-type-probability.model';
import { TimeProfileSegment } from '../models/time-profile-segment.model';

@Injectable({ providedIn: 'root' })
export class MockForecastService {
  readonly areas = AREA_DEFINITIONS;
  readonly incidentTypes = INCIDENT_TYPES;

  getForecastForArea(
    areaId: number,
    horizonHours: number,
    incidentFocus = 'Alle',
    comparisonPeriod: ComparisonPeriod = 'same-weekday',
  ): ForecastAreaResult {
    const area = AREA_DEFINITIONS.find((item) => item.id === areaId) ?? AREA_DEFINITIONS[0];
    const probability = this.computeProbability(
      area.id,
      area.baseRisk,
      horizonHours,
      incidentFocus,
      comparisonPeriod,
    );
    const expectedIncidents = this.computeExpectedIncidents(
      area.population,
      probability,
      horizonHours,
      area.id,
      incidentFocus,
    );
    const comparisonExpectedIncidents = this.computeComparisonExpectedIncidents(
      expectedIncidents,
      area.id,
      horizonHours,
      incidentFocus,
      comparisonPeriod,
    );
    const trendPercent =
      ((expectedIncidents - comparisonExpectedIncidents) / comparisonExpectedIncidents) * 100;

    return {
      areaId: area.id,
      areaName: area.name,
      horizonHours,
      population: area.population,
      probability,
      expectedIncidents,
      comparisonExpectedIncidents,
      incidentsPerTenThousand: (expectedIncidents / area.population) * 10000,
      trendPercent,
      riskLevel: this.toRiskLevel(probability),
      trend: this.toTrend(trendPercent),
      topIncidentTypes: this.getTopIncidentTypes(
        area.id,
        horizonHours,
        incidentFocus,
        expectedIncidents,
        5,
      ),
    };
  }

  getForecasts(
    horizonHours: number,
    incidentFocus = 'Alle',
    comparisonPeriod: ComparisonPeriod = 'same-weekday',
  ): ForecastAreaResult[] {
    return AREA_DEFINITIONS.map((area) =>
      this.getForecastForArea(area.id, horizonHours, incidentFocus, comparisonPeriod),
    );
  }

  getSummary(
    forecasts: readonly ForecastAreaResult[],
    horizonHours: number,
    incidentFocus = 'Alle',
    comparisonPeriod: ComparisonPeriod = 'same-weekday',
  ): ForecastSummary {
    const ranking = [...forecasts].sort((a, b) => b.expectedIncidents - a.expectedIncidents);
    const totalExpectedIncidents = forecasts.reduce(
      (sum, forecast) => sum + forecast.expectedIncidents,
      0,
    );
    const comparisonExpectedIncidents = forecasts.reduce(
      (sum, forecast) => sum + forecast.comparisonExpectedIncidents,
      0,
    );

    return {
      horizonHours,
      totalExpectedIncidents,
      comparisonExpectedIncidents,
      trendPercent:
        ((totalExpectedIncidents - comparisonExpectedIncidents) / comparisonExpectedIncidents) *
        100,
      comparisonLabel: comparisonPeriodLabel(comparisonPeriod),
      basisText: this.getBasisText(horizonHours),
      confidenceLabel: this.getConfidence(horizonHours).label,
      confidenceDescription: this.getConfidence(horizonHours).description,
      ranking,
      topIncidentTypes: this.aggregateIncidentTypes(forecasts, totalExpectedIncidents),
      timeProfile: this.getTimeProfile(totalExpectedIncidents, horizonHours, incidentFocus),
    };
  }

  private computeProbability(
    areaId: number,
    baseRisk: number,
    horizonHours: number,
    incidentFocus: string,
    comparisonPeriod: ComparisonPeriod,
  ): number {
    const horizonFactor = (Math.log1p(horizonHours) / Math.log1p(720)) * 0.24 - 0.06;
    const dailyWave = Math.sin(areaId * 1.41 + horizonHours / 31) * 0.11;
    const regionalWave = Math.cos(areaId * 0.87 - horizonHours / 84) * 0.06;
    const deterministicNoise =
      (this.hashToUnit(areaId, horizonHours, `${incidentFocus}:probability`) - 0.5) * 0.09;
    const focusModifier =
      incidentFocus === 'Alle'
        ? 0
        : (this.hashToUnit(areaId * 3, horizonHours + 17, incidentFocus) - 0.46) * 0.12;
    const comparisonModifier = this.comparisonHeatmapModifier(
      areaId,
      horizonHours,
      incidentFocus,
      comparisonPeriod,
    );

    return this.clamp(
      baseRisk +
        horizonFactor +
        dailyWave +
        regionalWave +
        deterministicNoise +
        focusModifier +
        comparisonModifier,
      0.04,
      0.96,
    );
  }

  private computeExpectedIncidents(
    population: number,
    probability: number,
    horizonHours: number,
    areaId: number,
    incidentFocus: string,
  ): number {
    const horizonDays = horizonHours / 24;
    const baseDailyIncidentsPerTenThousand = 0.74;
    const riskMultiplier = 0.62 + probability;
    const areaOperationalWave = 0.94 + this.hashToUnit(areaId, horizonHours, 'load') * 0.16;
    const focusMultiplier = incidentFocus === 'Alle' ? 1 : 0.78 + this.hashToUnit(areaId, 9, incidentFocus) * 0.22;

    return (
      (population / 10000) *
      baseDailyIncidentsPerTenThousand *
      Math.max(1 / 24, horizonDays) *
      riskMultiplier *
      areaOperationalWave *
      focusMultiplier
    );
  }

  private computeComparisonExpectedIncidents(
    expectedIncidents: number,
    areaId: number,
    horizonHours: number,
    incidentFocus: string,
    comparisonPeriod: ComparisonPeriod,
  ): number {
    const amplitude: Record<ComparisonPeriod, number> = {
      yesterday: 0.22,
      'same-weekday': 0.13,
      'thirty-day-average': 0.075,
    };
    const stabilityBias: Record<ComparisonPeriod, number> = {
      yesterday: 0.015,
      'same-weekday': 0.005,
      'thirty-day-average': -0.012,
    };
    const wave =
      (this.hashToUnit(areaId * 19, horizonHours + 31, `${incidentFocus}:${comparisonPeriod}`) -
        0.5) *
      2;
    const horizonWave = Math.sin(areaId * 0.66 + horizonHours / 96) * 0.035;
    const delta = this.clamp(
      wave * amplitude[comparisonPeriod] + horizonWave + stabilityBias[comparisonPeriod],
      -0.34,
      0.38,
    );

    return expectedIncidents / (1 + delta);
  }

  private getTopIncidentTypes(
    areaId: number,
    horizonHours: number,
    incidentFocus: string,
    expectedIncidents: number,
    limit: number,
  ): IncidentTypeProbability[] {
    const scoredTypes = INCIDENT_TYPES.filter((type) => type !== 'Alle').map((type, index) => {
      const stableBase = 0.8 + this.hashToUnit(areaId + index * 11, horizonHours, type) * 1.7;
      const horizonShape = Math.abs(Math.sin((horizonHours + areaId * 13 + index * 19) / 55));
      const focusBoost = incidentFocus === type ? 2.55 : 1;
      return {
        type,
        score: (stableBase + horizonShape) * focusBoost,
      };
    });

    const totalScore = scoredTypes.reduce((sum, item) => sum + item.score, 0);

    return scoredTypes
      .map((item) => ({
        type: item.type,
        probability: item.score / totalScore,
        expectedIncidents: expectedIncidents * (item.score / totalScore),
      }))
      .sort((a, b) => b.probability - a.probability)
      .slice(0, limit);
  }

  private aggregateIncidentTypes(
    forecasts: readonly ForecastAreaResult[],
    totalExpectedIncidents: number,
  ): IncidentTypeProbability[] {
    const scores = new Map<string, number>();

    for (const forecast of forecasts) {
      for (const incident of forecast.topIncidentTypes) {
        scores.set(
          incident.type,
          (scores.get(incident.type) ?? 0) + incident.probability * forecast.expectedIncidents,
        );
      }
    }

    const total = [...scores.values()].reduce((sum, value) => sum + value, 0);

    return [...scores.entries()]
      .map(([type, score]) => ({
        type,
        probability: score / total,
        expectedIncidents: totalExpectedIncidents * (score / total),
      }))
      .sort((a, b) => b.expectedIncidents - a.expectedIncidents)
      .slice(0, 5);
  }

  private getTimeProfile(
    totalExpectedIncidents: number,
    horizonHours: number,
    incidentFocus: string,
  ): TimeProfileSegment[] {
    const labels = horizonHours <= 12 ? ['00-06', '06-12', '12-18', '18-24'] : ['Nacht', 'Tag', 'Abend'];
    const baseShares = horizonHours <= 12 ? [0.17, 0.27, 0.31, 0.25] : [0.24, 0.45, 0.31];
    const adjusted = baseShares.map((share, index) => {
      const incidentShift = this.hashToUnit(index + 1, horizonHours, incidentFocus) * 0.08 - 0.04;
      const horizonShift = Math.sin(horizonHours / 72 + index * 1.7) * 0.035;
      return Math.max(0.08, share + incidentShift + horizonShift);
    });
    const totalShare = adjusted.reduce((sum, share) => sum + share, 0);

    return labels.map((label, index) => {
      const share = adjusted[index] / totalShare;
      return {
        label,
        share,
        expectedIncidents: totalExpectedIncidents * share,
      };
    });
  }

  private comparisonHeatmapModifier(
    areaId: number,
    horizonHours: number,
    incidentFocus: string,
    comparisonPeriod: ComparisonPeriod,
  ): number {
    const strength: Record<ComparisonPeriod, number> = {
      yesterday: 0.035,
      'same-weekday': 0.022,
      'thirty-day-average': 0.012,
    };

    return (
      (this.hashToUnit(areaId, horizonHours + 41, `${incidentFocus}:${comparisonPeriod}:map`) - 0.5) *
      strength[comparisonPeriod]
    );
  }

  private getBasisText(horizonHours: number): string {
    if (horizonHours === 1) return 'Basis: aktuelle Lage + kurzfristige Muster';
    if (horizonHours === 12) return 'Basis: aktuelle Lage + Tagesmuster';
    if (horizonHours === 24) return 'Basis: Tagesmuster + Wetterprognose';
    if (horizonHours <= 168) return 'Basis: Wochenmuster + verfügbare Prognosedaten';
    return 'Basis: historische Muster';
  }

  private getConfidence(horizonHours: number): { label: string; description: string } {
    if (horizonHours <= 12) {
      return { label: 'hoch', description: 'aktuelle und kurzfristige Datenbasis' };
    }

    if (horizonHours === 24) {
      return { label: 'mittel bis hoch', description: 'Prognose- und Musterkombination' };
    }

    if (horizonHours <= 168) {
      return { label: 'mittel', description: 'Prognose- und Musterkombination' };
    }

    return { label: 'niedriger', description: 'hauptsächlich historische Erwartungswerte' };
  }

  private toRiskLevel(probability: number): RiskLevel {
    if (probability <= 0.2) return 1;
    if (probability <= 0.4) return 2;
    if (probability <= 0.6) return 3;
    if (probability <= 0.8) return 4;
    return 5;
  }

  private toTrend(trendPercent: number): ForecastTrend {
    if (trendPercent > 2.5) return 'rising';
    if (trendPercent < -2.5) return 'falling';
    return 'stable';
  }

  private hashToUnit(areaId: number, horizonHours: number, label: string): number {
    const labelValue = [...label].reduce((sum, char) => sum + char.charCodeAt(0), 0);
    const value =
      Math.sin(areaId * 12.9898 + horizonHours * 78.233 + labelValue * 0.017) * 43758.5453;
    return value - Math.floor(value);
  }

  private clamp(value: number, min: number, max: number): number {
    return Math.min(max, Math.max(min, value));
  }
}
