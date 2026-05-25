import { ForecastAreaResult } from './forecast-area-result.model';
import { IncidentTypeProbability } from './incident-type-probability.model';
import { TimeProfileSegment } from './time-profile-segment.model';

export interface ForecastSummary {
  horizonHours: number;
  totalExpectedIncidents: number;
  comparisonExpectedIncidents: number;
  trendPercent: number;
  comparisonLabel: string;
  basisText: string;
  confidenceLabel: string;
  confidenceDescription: string;
  ranking: ForecastAreaResult[];
  topIncidentTypes: IncidentTypeProbability[];
  timeProfile: TimeProfileSegment[];
}
