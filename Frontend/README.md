# EMS-Forecast Frontend

Angular-Dashboard zur Visualisierung von Einsatzwahrscheinlichkeiten in Oberösterreich.

## Start

```bash
npm install
npm start
```

Die Anwendung läuft danach standardmäßig unter `http://localhost:4200/`.

## Inhalt

- Leaflet-Karte mit lokalen GeoJSON-Gebieten aus `src/assets/geo/gebiete.geojson`
- Prognosehorizont von `1h` bis `30 Tage`
- Risiko-Heatmap, Gebietsauswahl und Detailpanel
- Anzeige wahrscheinlicher Einsatztypen
