# Vorhersagesystem fuer Einsatzwahrscheinlichkeiten (Rettungsdienst OOE)

Lauffaehiger Diplomarbeits-Prototyp mit transparenter Modelllogik (kein Black Box-Ansatz):
- FastAPI-Backend mit klaren API-Endpunkten
- Hotspot-basierte Verdichtung der Verkehrs- und Einflussdaten
- Erklaerbares, regelbasiertes Risikomodell (0.0-1.0 + 5 Risikostufen)
- Web-Visualisierung mit Kartenansicht (Leaflet) und automatischem Diagramm-Fallback
- Seed-Daten fuer alle 15 definierten Gebiete in Oberoesterreich

## Gebiete (15)
1 Linz + Urfahr  
2 Wels  
3 Rohrbach  
4 Freistadt  
5 Perg  
6 Wels-Land Nord + Linz-Land  
7 Steyr + Steyr-Land Nord + Kirchdorf Nord  
8 Kirchdorf Süd + Steyr-Land Süd  
9 Gmunden Nord + Vöcklabruck Süd + Wels-Land Süd  
10 Gmunden Süd  
11 Ried + Vöcklabruck Nord  
12 Braunau  
13 Schärding  
14 Grieskirchen + Eferding  
15 Urfahr-Umgebung

## Projektstruktur

```text
backend/
  app/
    api/routes.py
    config.py
    constants.py
    domain/models.py
    schemas/api.py
    services/
      data_repository.py
      feature_engineering.py
      geometry_service.py
      risk_model.py
      risk_service.py
  requirements.txt
frontend/
  index.html
  app.js
  styles.css
data/
  seed/
    areas.csv
    hotspots.csv
    traffic.csv
    constructions.csv
    accidents.csv
    weather.csv
  geo/
    oo_areas_mock.geojson
  scripts/
    generate_seed_data.py
docs/
  technical_documentation.md
README.md
```

## Setup und Start

### 1) Python-Umgebung

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r backend/requirements.txt
```

### 2) Seed-Daten erzeugen (deterministisch)

```bash
python data/scripts/generate_seed_data.py
```

### 3) API + Frontend starten

```bash
uvicorn app.main:app --app-dir backend --reload
```

Danach im Browser:
- `http://127.0.0.1:8000/` (Visualisierung)
- `http://127.0.0.1:8000/docs` (Swagger API)

## API-Endpunkte

- `GET /api/areas`
- `GET /api/risk/current`
- `GET /api/risk/{areaId}`
- `POST /api/recalculate`
- `GET /api/hotspots` (optional, implementiert)
- `GET /api/geometry` (liefert GeoJSON oder 404)

## Datenfluss

1. CSV-Seed-Daten werden aus `data/seed/` geladen.
2. Hotspot-basierte Aggregation pro Gebiet bildet Kennzahlen:
   - `avgSpeed`
   - `minSpeed`
   - `avgTravelTime`
   - `unknownRatioTrafficStatus`
   - `activeConstructionCount`
   - `accidentCount24h`
   - `accidentCount7d`
   - `hotspotCriticalityScore`
3. Wetter- und Zeitmerkmale werden je Gebiet ergaenzt.
4. Transparentes Scoring berechnet `riskScore` (0.0-1.0).
5. `riskCategory` wird aus festen Schwellwerten abgeleitet.
6. API liefert Ergebnis inkl. textlicher Erklaerung je Gebiet.

## Risikomodell (transparent)

Komponenten:
- Verkehr
- Baustellen
- Unfaelle
- Wetter
- Zeitfaktoren
- Hotspot-Struktur

Gewichtete Endformel:

```text
RiskScore =
  0.38 * traffic
+ 0.12 * construction
+ 0.22 * accident
+ 0.14 * weather
+ 0.09 * time
+ 0.05 * hotspot
```

Risikostufen:
- `0.00 - 0.20` -> 1 (sehr gering)
- `0.21 - 0.40` -> 2 (gering)
- `0.41 - 0.60` -> 3 (mittel)
- `0.61 - 0.80` -> 4 (hoch)
- `0.81 - 1.00` -> 5 (kritisch)

Farbcodes:
- 1 `#1b5e20`
- 2 `#7cb342`
- 3 `#fdd835`
- 4 `#fb8c00`
- 5 `#d32f2f`

## Visualisierung

Primär:
- Leaflet-Karte mit GeoJSON-Flächen
- Tooltip/Klick zeigt Risiko pro Gebiet

Fallback (automatisch):
- Wenn `/api/geometry` kein GeoJSON liefert (404), schaltet das Frontend auf ein horizontales Balkendiagramm um
- Balken sind farbcodiert und nach `riskScore` absteigend sortiert

Hinweis zur Geometrie:
- `data/geo/oo_areas_mock.geojson` ist bewusst als **MOCK** gekennzeichnet und kein offizieller Verwaltungszuschnitt.

## Austausch gegen Echtdaten

Folgende CSV-Dateien koennen 1:1 durch echte Pipelines ersetzt werden:
- `traffic.csv` (z. B. EVIS/Verkehrsprovider)
- `constructions.csv` (Baustellenfeed)
- `accidents.csv` (Unfallfeed)
- `weather.csv` (Wetter-API)

Die Logik bleibt in den Services isoliert:
- Laden/Validieren: `backend/app/services/data_repository.py`
- Aggregation: `backend/app/services/feature_engineering.py`
- Scoring: `backend/app/services/risk_model.py`

Fuer echte Gebietsgeometrien:
- `AREA_GEOJSON_PATH` auf ein reales OOE-GeoJSON setzen.

## Bezug zu `PLZ_Liste.xlsx`

Die Datei `PLZ_Liste.xlsx` kann als zusaetzliche Mapping-Quelle fuer `PLZ -> areaId` genutzt werden, falls Einsatz-/Verkehrsdaten nur PLZ-basiert vorliegen. Der Prototyp arbeitet aktuell direkt mit `areaId`.
