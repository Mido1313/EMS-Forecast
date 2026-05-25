import { Component, HostListener, computed, input, output, signal } from '@angular/core';
import {
  Activity,
  Ambulance,
  Baby,
  Bandage,
  Brain,
  Car,
  Check,
  ChevronDown,
  Ellipsis,
  EyeOff,
  FlaskConical,
  HardHat,
  HeartPulse,
  LucideAngularModule,
  Wind,
} from 'lucide-angular';
import type { LucideIconData } from 'lucide-angular';
import { ComparisonPeriod, ComparisonPeriodOption } from '../../core/data/comparison-periods';
import { formatHorizon } from '../../core/data/forecast-steps';
import { RiskLegendComponent } from '../risk-legend/risk-legend.component';

interface IncidentOption {
  type: string;
  icon: LucideIconData;
}

const INCIDENT_ICONS: Record<string, LucideIconData> = {
  Alle: Activity,
  'Herz-Kreislauf': HeartPulse,
  Atemnot: Wind,
  Bewusstlosigkeit: EyeOff,
  Sturz: Bandage,
  Verkehrsunfall: Car,
  Arbeitsunfall: HardHat,
  'Psychische Gründe': Brain,
  Intoxikation: FlaskConical,
  Krankentransport: Ambulance,
  'Schwangerschaft & Geburt': Baby,
  Sonstiges: Ellipsis,
};

@Component({
  selector: 'app-forecast-controls',
  imports: [RiskLegendComponent, LucideAngularModule],
  templateUrl: './forecast-controls.component.html',
  styleUrl: './forecast-controls.component.scss',
})
export class ForecastControlsComponent {
  readonly forecastSteps = input.required<readonly number[]>();
  readonly horizonIndex = input.required<number>();
  readonly incidentTypes = input.required<readonly string[]>();
  readonly incidentFocus = input.required<string>();
  readonly comparisonPeriods = input.required<readonly ComparisonPeriodOption[]>();
  readonly comparisonPeriod = input.required<ComparisonPeriod>();

  readonly horizonIndexChange = output<number>();
  readonly incidentFocusChange = output<string>();
  readonly comparisonPeriodChange = output<ComparisonPeriod>();

  protected readonly checkIcon = Check;
  protected readonly chevronDownIcon = ChevronDown;
  protected readonly isIncidentMenuOpen = signal(false);
  protected readonly horizonLabel = computed(() =>
    formatHorizon(this.forecastSteps()[this.horizonIndex()] ?? 24),
  );
  protected readonly incidentOptions = computed<IncidentOption[]>(() =>
    this.incidentTypes().map((type) => ({
      type,
      icon: INCIDENT_ICONS[type] ?? Ellipsis,
    })),
  );
  protected readonly selectedIncidentOption = computed<IncidentOption>(() => {
    return (
      this.incidentOptions().find((option) => option.type === this.incidentFocus()) ??
      this.incidentOptions()[0] ?? { type: 'Alle', icon: Activity }
    );
  });

  @HostListener('document:click')
  protected closeIncidentMenu(): void {
    this.isIncidentMenuOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  protected closeIncidentMenuOnEscape(): void {
    this.isIncidentMenuOpen.set(false);
  }

  protected onSliderInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.horizonIndexChange.emit(Number(target.value));
  }

  protected toggleIncidentMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.isIncidentMenuOpen.update((isOpen) => !isOpen);
  }

  protected selectIncidentOption(option: IncidentOption, event: MouseEvent): void {
    event.stopPropagation();
    this.incidentFocusChange.emit(option.type);
    this.isIncidentMenuOpen.set(false);
  }

  protected selectComparisonPeriod(period: ComparisonPeriod): void {
    this.comparisonPeriodChange.emit(period);
  }
}
