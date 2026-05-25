export const FORECAST_STEPS: readonly number[] = [
  1, 12, 24, 48, 72, 96, 120, 144, 168, 192, 216, 240, 264, 288, 312, 336, 360,
  384, 408, 432, 456, 480, 504, 528, 552, 576, 600, 624, 648, 672, 696, 720,
];

export function formatHorizon(hours: number): string {
  if (hours < 24) {
    return `${hours}h`;
  }

  if (hours === 24) {
    return '24h';
  }

  const days = Math.round(hours / 24);
  return `${days} Tage`;
}
