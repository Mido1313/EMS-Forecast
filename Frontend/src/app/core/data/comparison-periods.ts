export type ComparisonPeriod = 'yesterday' | 'same-weekday' | 'thirty-day-average';

export interface ComparisonPeriodOption {
  value: ComparisonPeriod;
  label: string;
}

export const COMPARISON_PERIODS: readonly ComparisonPeriodOption[] = [
  { value: 'yesterday', label: 'Gestern' },
  { value: 'same-weekday', label: 'Letzte Woche' },
  { value: 'thirty-day-average', label: '30-Tage-Schnitt' },
];

export function comparisonPeriodLabel(value: ComparisonPeriod): string {
  return COMPARISON_PERIODS.find((option) => option.value === value)?.label ?? 'Vergleich';
}
