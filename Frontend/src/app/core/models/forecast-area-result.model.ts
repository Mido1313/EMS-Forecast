import { IncidentTypeProbability } from './incident-type-probability.model';

export type RiskLevel = 1 | 2 | 3 | 4 | 5;
export type ForecastTrend = 'falling' | 'stable' | 'rising';

export interface ForecastAreaResult {
  areaId: number;
  areaName: string;
  horizonHours: number;
  population: number;
  probability: number;
  expectedIncidents: number;
  comparisonExpectedIncidents: number;
  incidentsPerTenThousand: number;
  trendPercent: number;
  riskLevel: RiskLevel;
  trend: ForecastTrend;
  topIncidentTypes: IncidentTypeProbability[];
}
