import { Component } from '@angular/core';

interface RiskLegendItem {
  level: number;
  label: string;
  color: string;
}

@Component({
  selector: 'app-risk-legend',
  templateUrl: './risk-legend.component.html',
  styleUrl: './risk-legend.component.scss',
})
export class RiskLegendComponent {
  protected readonly legendItems: readonly RiskLegendItem[] = [
    { level: 1, label: 'sehr gering', color: 'var(--risk-1)' },
    { level: 2, label: 'gering', color: 'var(--risk-2)' },
    { level: 3, label: 'mittel', color: 'var(--risk-3)' },
    { level: 4, label: 'erhöht', color: 'var(--risk-4)' },
    { level: 5, label: 'hoch', color: 'var(--risk-5)' },
  ];
}
