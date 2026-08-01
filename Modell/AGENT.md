# AGENT.md – EMS Forecast · Modell v2 (vollständiges Feature-Set)

> Lies diese Datei vollständig bevor du irgendeinen Code schreibst.
> Phase 1–4 sind bereits abgeschlossen. Diese AGENT.md beschreibt ausschließlich Phase 5 (Feature-Erweiterung) und Phase 6 (Modell v2 Training).

---

## Kontext

Das Modell v1 (`models/ems_forecast_rf_v1.pkl`) trainiert nur auf Zeitfeatures:
`['gebiet_id', 'hour', 'weekday', 'month', 'quarter', 'is_weekend', 'season']`

Ziel dieser Phase: alle vorhandenen Kontextdaten einbauen → Modell v2 trainieren → als `models/ems_forecast_rf_v2.pkl` speichern → FastAPI-Service auf v2 umstellen.

---

## Arbeitsregeln (zwingend)

1. **Schritt für Schritt** — einen Schritt umsetzen, Ausgabe zeigen, dann weiter.
2. **Kein Code ohne Erklärung** — jeden Block kurz kommentieren.
3. **Outputs immer verifizieren** — nach jedem Schritt `.shape`, `.head()` oder Statistik ausgeben.
4. **Niemals `models/features_aggregated.pkl` überschreiben** — neue Datei: `models/features_v2.pkl`.
5. **Niemals `ems_forecast_rf_v1.pkl` überschreiben** — neues Modell: `ems_forecast_rf_v2.pkl`.
6. **Pfade relativ zum Modell-Ordner** — alle Datenpfade mit `../Datensammlung/...` referenzieren.

---

## Datenpfade (relativ zum Modell-Ordner)

```
../Datensammlung/FeiertageFerien/datamodeler.json
../Datensammlung/Bevoelkerungsdaten/Bevoelkerungsdaten.json
../Datensammlung/Pflegeheime/Pflegeheime.json
../Datensammlung/Events/Events.json
../Datensammlung/Verkehrsdaten/unfaelle_nach_gebiet_2024.csv
```

---

## Datenstrukturen (exakt so wie in den Dateien)

### datamodeler.json (Feiertage & Ferien)
```json
[
  { "typ": "Feiertag", "name": "Neujahr", "start": "01.01.2025", "ende": "01.01.2025" },
  { "typ": "Ferien",   "name": "Sommerferien", "start": "05.07.2025", "ende": "07.09.2025" }
]
```
Format: `dd.mm.yyyy` — mit `pd.to_datetime(..., format='%d.%m.%Y')` parsen.

### Bevoelkerungsdaten.json
```json
[
  {
    "plz": 4020, "gemeinde": "Leonding", "gesBev": 29134,
    "unter15": 15.2, "ueber65": 20.0,
    "total_agegroup_4": 8047
  }
]
```
Pro PLZ — muss auf Gebiet-ID aggregiert werden (PLZ_TO_GEBIET aus `src/features.py` verwenden).
Relevante Felder: `gesBev` (Gesamtbevölkerung), `ueber65` (Anteil 65+), `total_agegroup_4` (absolut 65+).

### Pflegeheime.json
```json
{ "pflegeheime": [
  { "name": "...", "ort": "Linz", "plz": "4040", "pflegeplaetze": 120 }
]}
```
PLZ als String — mit `int()` konvertieren. Pro Gebiet: Summe der `pflegeplaetze`.

### Events.json
```json
{ "events": [
  { "name": "Woodstock der Blasmusik", "ort": "...", "plz": "4974",
    "start_datum": "2024-06-27", "end_datum": "2024-06-30" }
]}
```
Format: `yyyy-mm-dd`. Pro Datum + Gebiet: `has_major_event` (0/1).

### unfaelle_nach_gebiet_2024.csv
CSV mit Spalte `Gebiet_ID` und Unfallrate pro Gebiet.
Als statisches Feature `accident_rate` pro `gebiet_id` einbauen.

---

## Phase 5 – Feature Engineering v2 (`notebooks/05_feature_engineering_v2.ipynb`)

**Ziel:** `models/features_v2.pkl` erzeugen mit erweitertem Feature-Vektor.

### Schritt 5.1 – Basis laden
- `models/features_aggregated.pkl` laden (das bereits aggregierte v1-DataFrame)
- Shape ausgeben, erste Zeilen zeigen

### Schritt 5.2 – Feiertage & Ferien einbauen
- `datamodeler.json` laden
- Für jeden Eintrag: alle Tage zwischen `start` und `ende` expandieren (mit `pd.date_range`)
- Zwei Sets erstellen: `feiertag_dates` (typ=="Feiertag") und `ferien_dates` (typ=="Ferien")
- Im aggregierten DataFrame:
  - `is_holiday = 1` wenn `date` in `feiertag_dates`, sonst `0`
  - `is_school_holiday = 1` wenn `date` in `ferien_dates`, sonst `0`
- Ausgabe: Anzahl Feiertage und Ferientage die gematcht haben

### Schritt 5.3 – Bevölkerungsdaten einbauen
- `Bevoelkerungsdaten.json` laden
- PLZ → Gebiet-ID mappen (PLZ_TO_GEBIET aus `src/features.py` importieren)
- Pro Gebiet aggregieren:
  - `population` = Summe von `gesBev`
  - `elderly_ratio` = gewichteter Durchschnitt von `ueber65`
  - `elderly_abs` = Summe von `total_agegroup_4`
- Als Dictionary `{gebiet_id: {population, elderly_ratio, elderly_abs}}` speichern
- Per `map` auf das aggregierte DataFrame joinen über `gebiet_id`
- Ausgabe: Statistiken der neuen Spalten

### Schritt 5.4 – Pflegeheime einbauen
- `Pflegeheime.json` laden (`data['pflegeheime']`)
- PLZ (String → int) → Gebiet-ID mappen
- Pro Gebiet: Summe der `pflegeplaetze` → `nursing_home_beds`
- Gebiete ohne Pflegeheim → `nursing_home_beds = 0`
- Per `map` joinen
- Ausgabe: Top 5 Gebiete nach Pflegeplätzen

### Schritt 5.5 – Events einbauen
- `Events.json` laden (`data['events']`)
- Für jeden Event: alle Tage zwischen `start_datum` und `end_datum` expandieren
- PLZ (String → int) → Gebiet-ID mappen
- Set aus `(date, gebiet_id)` Tuples erstellen
- Im DataFrame: `has_major_event = 1` wenn `(date, gebiet_id)` im Set, sonst `0`
- Ausgabe: Anzahl Einsatz-Stunden die von einem Event betroffen sind

### Schritt 5.6 – Unfallstatistik einbauen
- `unfaelle_nach_gebiet_2024.csv` laden
- Spalte `Gebiet_ID` auf `gebiet_id` umbenennen
- Relevante Unfallrate-Spalte identifizieren und als `accident_rate` joinen
- Ausgabe: Wertebereich von `accident_rate`

### Schritt 5.7 – Wetterdaten via Open-Meteo API laden
- Für jedes der 15 Gebiete historische Stundendaten für 2025 laden
- API-Endpunkt (kostenlos, keine Registrierung):
  ```
  https://archive-api.open-meteo.com/v1/archive
    ?latitude={lat}&longitude={lon}
    &start_date=2025-01-01&end_date=2025-12-31
    &hourly=temperature_2m,precipitation,snowfall,windspeed_10m
    &timezone=Europe/Vienna
  ```
- Koordinaten der 15 Gebiete (Messstationen laut Projektdokumentation):

  ```python
  GEBIET_COORDS = {
      1:  (48.3069, 14.2858),  # Linz
      2:  (48.1672, 14.0234),  # Wels
      3:  (48.5706, 13.9980),  # Rohrbach
      4:  (48.5108, 14.5042),  # Freistadt
      5:  (48.2494, 14.6369),  # Perg
      6:  (48.2667, 14.2500),  # Leonding
      7:  (48.0432, 14.4211),  # Steyr
      8:  (47.9000, 14.1167),  # Kirchdorf
      9:  (48.0000, 13.6500),  # Regau/Vöcklabruck
      10: (47.7167, 13.6333),  # Bad Ischl
      11: (48.2167, 13.4833),  # Ried im Innkreis
      12: (48.2289, 13.0356),  # Braunau
      13: (48.4500, 13.4333),  # Schärding
      14: (48.3167, 13.9833),  # Grieskirchen
      15: (48.4500, 14.1667),  # Bad Leonfelden
  }
  ```

- Pro Stunde und Gebiet speichern: `temperature`, `precipitation`, `snowfall`, `windspeed`
- Abgeleitetes Feature: `is_extreme_weather = 1` wenn temperature > 30 ODER temperature < -5 ODER snowfall > 2 ODER windspeed > 50
- API-Aufrufe mit `requests` library, Rate-Limiting beachten: 0.5s Pause zwischen Aufrufen
- Ergebnis als `models/weather_2025.pkl` zwischenspeichern (damit nicht jedes Mal neu geladen werden muss)
- Auf aggregiertes DataFrame joinen über `(gebiet_id, date, hour)`
- Fehlende Wetterwerte (kein Match) → Median der jeweiligen Spalte

### Schritt 5.8 – Feature-Vektor finalisieren & speichern
- Finaler Feature-Vektor v2:
  ```python
  FEATURES_V2 = [
      'gebiet_id', 'hour', 'weekday', 'month', 'quarter', 'is_weekend', 'season',
      'is_holiday', 'is_school_holiday',
      'population', 'elderly_ratio', 'nursing_home_beds',
      'has_major_event', 'accident_rate',
      'temperature', 'precipitation', 'snowfall', 'windspeed', 'is_extreme_weather'
  ]
  ```
- Prüfen: keine NaN-Werte im finalen DataFrame (`.isnull().sum()` ausgeben)
- Als `models/features_v2.pkl` speichern
- Ausgabe: Shape des finalen DataFrames, Vergleich mit v1

---

## Phase 6 – Modell v2 Training (`notebooks/06_modell_v2.ipynb`)

**Ziel:** Random Forest v2 trainieren, mit v1 vergleichen, speichern.

### Schritt 6.1 – Daten laden
- `models/features_v2.pkl` laden
- `FEATURES_V2` und Zielvariable `einsatz_count` trennen
- Train/Test-Split: `test_size=0.2, random_state=42` (identisch zu v1 für Vergleichbarkeit)

### Schritt 6.2 – Random Forest v2 trainieren
```python
RandomForestRegressor(
    n_estimators=200,
    max_depth=None,
    min_samples_leaf=2,
    random_state=42,
    n_jobs=-1
)
```

### Schritt 6.3 – Metriken berechnen & vergleichen
- MAE, RMSE, R² berechnen
- Vergleichstabelle ausgeben:

```
| Modell      | Features | MAE    | RMSE   | R²     |
|-------------|----------|--------|--------|--------|
| v1 (Basis)  | 7        | 0.2711 | 0.4799 | -0.024 |
| v2 (Voll)   | 19       | ???    | ???    | ???    |
```

### Schritt 6.4 – Feature Importance v2
- Top-15 wichtigste Features als horizontales Balkendiagramm → `output/feature_importance_v2.png`
- Welche neuen Features haben den größten Einfluss?

### Schritt 6.5 – Modell speichern
```python
joblib.dump(model, 'models/ems_forecast_rf_v2.pkl')
```
Dateigröße ausgeben.

### Schritt 6.6 – FastAPI-Service auf v2 umstellen
- In `main.py`: Modellpfad von `rf_v1.pkl` auf `rf_v2.pkl` ändern
- In `src/features.py`: `FEATURES_V2` Liste hinzufügen
- In `src/model.py`: `build_feature_row()` um alle neuen Features erweitern
  - Statische Features (population, elderly_ratio etc.) aus einem vorberechneten Dictionary laden
  - Wetterdaten: für Live-Forecasts Open-Meteo Forecast-API verwenden:
    ```
    https://api.open-meteo.com/v1/forecast
      ?latitude={lat}&longitude={lon}
      &hourly=temperature_2m,precipitation,snowfall,windspeed_10m
      &forecast_days=2&timezone=Europe/Vienna
    ```
- `MODEL_VERSION` auf `'rf_v2'` setzen
- Service neu starten und `/health` prüfen

---

## Definition of Done

| Phase | Kriterium |
|---|---|
| Phase 5 | `models/features_v2.pkl` existiert, alle 19 Features ohne NaN, Shape größer als v1 |
| Phase 6 | v2 MAE ≤ v1 MAE (0.2711), `rf_v2.pkl` gespeichert, Feature-Importance-Plot vorhanden |
| Service | `/health` zeigt `"model_version": "rf_v2"`, DB-Einträge nach `/forecast/trigger` haben `model_version = 'rf_v2'` |

---

## Wichtige Hinweise

- `requests` für Open-Meteo bereits in venv installiert? Falls nicht: `pip install requests` ausführen.
- Wetterdaten-Download dauert ca. 2–3 Minuten (15 API-Aufrufe mit Pause). Einmalig laden, als `.pkl` cachen.
- Statische Features (population, nursing_home_beds etc.) ändern sich pro Gebiet nie — einmal berechnen, als Konstante in `src/features.py` speichern.
- `accident_rate` aus CSV: falls Spaltenname unklar, zuerst `df.columns` ausgeben und klären bevor weitergemacht wird.