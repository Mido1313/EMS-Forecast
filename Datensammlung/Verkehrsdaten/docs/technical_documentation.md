# Technische Dokumentation

## 1. Feature-Set im Modell

Der Prototyp nutzt pro Gebiet eine kompakte, erklaerbare Feature-Menge:

- Verkehr:
  - `avgSpeed`
  - `minSpeed`
  - `avgTravelTime`
  - `avgDelayIndex`
  - `unknownRatioTrafficStatus`
- Baustellen:
  - `activeConstructionCount`
  - `constructionSeverityIndex`
- Unfaelle:
  - `accidentCount24h`
  - `accidentCount7d`
- Wetter:
  - `temperatureC`
  - `precipitationMm`
  - `windKph`
  - `icyFlag`
- Zeit:
  - `hour`
  - `weekday`
  - `month`
  - `isWeekend`
  - `isHoliday`
- Struktur:
  - `hotspotCriticalityScore`
  - `commuterHotspotRatio`
  - `touristicHotspotRatio`

## 2. Hotspot-Aggregation

Prinzip: Nicht das gesamte Strassennetz, sondern pro Gebiet nur vordefinierte relevante Segmente.

Quelle:
- `data/seed/hotspots.csv`

Je Gebiet werden Segmentdaten verdichtet und in Kennzahlen ueberfuehrt:
- Geschwindigkeit/Travel Time aus den letzten Stunden
- Unknown-Status-Anteil
- Baustellen/Unfallhaeufigkeit im Zeitfenster
- `hotspotCriticalityScore` als Mittelwert aus:
  - statischer Segmentkritikalitaet (`criticalityWeight`)
  - dynamischer Congestion-Komponente

Spezielle Abdeckung im Seed:
- Autobahn/Schnellstrasse
- Pendlerachsen (u. a. Linz/Wels-nah)
- touristische Achsen inkl. B145-Modellsegment in Gebiet 9 und 10

## 3. Bildung der Risikostufen

Das Modell arbeitet regelbasiert und additiv mit expliziten Gewichten:

```text
RiskScore =
  0.38 * traffic
+ 0.12 * construction
+ 0.22 * accident
+ 0.14 * weather
+ 0.09 * time
+ 0.05 * hotspot
```

Jede Komponente ist einzeln nachvollziehbar, z. B.:
- Verkehr aus Speed-/Delay-/Unknown-Penalties
- Baustellen aus Count + Severity
- Unfaelle aus 24h-/7d-Signalen

Schwellwerte:
- 0.00-0.20 -> Stufe 1
- 0.21-0.40 -> Stufe 2
- 0.41-0.60 -> Stufe 3
- 0.61-0.80 -> Stufe 4
- 0.81-1.00 -> Stufe 5

Zusatz zur Erklaerbarkeit:
- Pro Gebiet werden Top-Einflussfaktoren in einem Satz zusammengefasst (`explanation`).

## 4. API-Verhalten

- `GET /api/risk/current`: kompakter Ueberblick pro Gebiet
- `GET /api/risk/{areaId}`: Detailmetriken + Komponenten
- `POST /api/recalculate`: erneute Berechnung mit aktuellem Datenstand
- `GET /api/hotspots`: Hotspot-Metriken zur Nachvollziehbarkeit
- `GET /api/geometry`: GeoJSON fuer Karte, sonst 404 -> Frontend-Fallback

## 5. Einbindung realer Datenquellen (EVIS/Baustellen/Unfall)

Empfohlener Integrationspfad:

1. Ingestion-Layer aufbauen, der Rohdaten in das interne Schema ueberfuehrt.
2. Adapter im `data_repository` ergaenzen (CSV -> DB/API austauschbar halten).
3. Area-Mapping sicherstellen:
   - direkt ueber Gebiets-ID oder
   - ueber PLZ/Segment-Mapping (z. B. mit `PLZ_Liste.xlsx` + Hotspot-Mapping).
4. Zeitstempel vereinheitlichen (UTC intern, Lokalisierung nur fuer Anzeige/Zeitfeatures).
5. Qualitaetssicherung:
   - Pflichtfelder validieren
   - Missing- und Outlier-Handling dokumentieren

## 6. Geodatenstrategie

Aktuell:
- `oo_areas_mock.geojson` (klar als Mock markiert)

Fuer echte Karten:
- offizielles Gebiets-GeoJSON/Shapefile importieren
- `properties.areaId` konsistent mit den 15 Gebiets-IDs pflegen
- `AREA_GEOJSON_PATH` auf reale Datei setzen

Frontend-Fallback bleibt aktiv, falls Geodaten fehlen oder fehlerhaft sind.
