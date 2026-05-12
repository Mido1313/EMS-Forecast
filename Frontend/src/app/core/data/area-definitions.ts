export interface AreaDefinition {
  id: number;
  name: string;
  baseRisk: number;
  population: number;
}

export const AREA_DEFINITIONS: readonly AreaDefinition[] = [
  { id: 1, name: 'Linz + Urfahr', baseRisk: 0.46, population: 228000 },
  { id: 2, name: 'Wels', baseRisk: 0.42, population: 72000 },
  { id: 3, name: 'Rohrbach', baseRisk: 0.28, population: 57000 },
  { id: 4, name: 'Freistadt', baseRisk: 0.31, population: 68000 },
  { id: 5, name: 'Perg', baseRisk: 0.35, population: 72000 },
  { id: 6, name: 'Wels-Land Nord + Linz-Land', baseRisk: 0.44, population: 232000 },
  { id: 7, name: 'Steyr + Steyr-Land Nord + Kirchdorf Nord', baseRisk: 0.39, population: 137000 },
  { id: 8, name: 'Kirchdorf Süd + Steyr-Land Süd', baseRisk: 0.27, population: 52000 },
  { id: 9, name: 'Gmunden Nord + Vöcklabruck Süd + Wels-Land Süd', baseRisk: 0.37, population: 165000 },
  { id: 10, name: 'Gmunden Süd', baseRisk: 0.26, population: 47000 },
  { id: 11, name: 'Ried + Vöcklabruck Nord', baseRisk: 0.36, population: 154000 },
  { id: 12, name: 'Braunau', baseRisk: 0.34, population: 103000 },
  { id: 13, name: 'Schärding', baseRisk: 0.29, population: 57000 },
  { id: 14, name: 'Grieskirchen + Eferding', baseRisk: 0.33, population: 98000 },
  { id: 15, name: 'Urfahr-Umgebung', baseRisk: 0.32, population: 86000 },
];
