# Sprintplanung – Tooling, Architektur & Mockdaten

## Ziel des Sprints
In dieser Iteration liegt der Fokus auf:
- Auswahl und Evaluierung geeigneter Technologien (Sprachen, Frameworks, Libraries)
- Aufsetzen einer klaren technischen Architektur
- Vorbereitung und Generierung realistischer Mockdaten (~10.000 Datensätze)


---

## Kontext
Da aktuell unklar ist, ob reale Einsatzdaten vom Roten Kreuz verfügbar sein werden (Outsourcing der Datenhaltung), wird parallel eine realitätsnahe synthetische Datenbasis aufgebaut.

→ Ziel: Mindestens 10.000 Datensätze als Grundlage für Analyse & Modellierung  
→ Verteilung:
- Merjem: 2.500 Datensätze
- Mido: 2.500 Datensätze
- Lukas: 5.000 Datensätze

Die Daten sollen möglichst realistisch simuliert werden (räumlich, zeitlich, demografisch).

---

## Arbeitspakete nach Personen

### Merjem – Datenhaltung & Struktur

**Ziele:**
- Saubere, skalierbare Datenstruktur
- Vorbereitung für Integration aller Datenquellen

**Tasks:**
- Entscheidung Datenbanksystem
- Entwurf Datenbankschema:
  - Einsätze
  - Gebiete
  - Wetterdaten
  - Demografie
  - Verkehrsdaten
- Normalisierung vs. Performance abwägen
- Definition von Beziehungen (FKs)
- Importstrategie für externe Daten
- Definition von ETL-Prozessen

**Ergebnis:**
- Finales Datenbankschema
- Importprozesse definiert

---

### Mido – Modell & Statistik

**Ziele:**
- Grundlagen für Datenanalyse und Modellierung definieren
- Evaluierung geeigneter Python-Tools

**Tasks:**
- Evaluierung Libraries zB:
  - pandas (Datenverarbeitung)
  - numpy (numerische Operationen)
  - matplotlib / seaborn (Visualisierung)
  - scikit-learn (klassische Modelle)
  - statsmodels (statistische Verfahren)
- Definition möglicher Modellansätze:
  - Zeitreihen (optional)
  - Klassifikation / Regression
  - Poisson-Verteilung für Einsatzhäufigkeit
- Definition von Zielvariablen:
  - Einsätze pro Gebiet / Zeitfenster
- Festlegung von Metriken:
  - MAE, RMSE, Accuracy etc.

**Ergebnis:**
- Dokumentierte Modellstrategie
- Baseline-Modell definiert

---

### Lukas – APIs & Frontend

**Ziele:**
- Evaluierung und Auswahl von APIs
- Festlegung Frontend-Technologie & Architektur

**Ergebnis:**
- Dokumentierte API-Strategie
- Frontend-Stack fixiert
- Test für Frontend imt Hilfe von GeoJSON

---


## Gemeinsames Thema: Mockdaten-Generierung

### Ziel
Erstellung eines realistischen synthetischen Datensatzes mit mindestens 10.000 Einsätzen.

---

## Wichtige Parameter für realistische Simulation

Folgende Faktoren müssen unbedingt berücksichtigt werden:

### Räumlich
1. Gebiet / Gebiets-ID
2. PLZ
3. Bevölkerungsdichte pro Gebiet
4. Nähe zu Ballungszentren (z. B. Linz)
5. Infrastruktur (Stadt vs. Land)
6. Verkehrsknotenpunkte (Autobahnen)

### Zeitlich
7. Uhrzeit (Stundenverteilung – Peaks beachten)
8. Wochentag
9. Wochenende vs. Werktag
10. Saison (Sommer/Winter)
11. Feiertage
12. Ferienzeiten

### Demografisch
13. Altersgruppe (z. B. 0–18, 19–29, 30-59 60+)
14. Geschlecht 
15. Anteil älterer Bevölkerung im Gebiet

### Einsatzbezogen
17. Einsatzgrund (Unfall, internistisch, etc.)
18. Dringlichkeit / Priorität
19. Einsatztyp (RTW, Notarzt etc.)
20. Einsatzdauer
21. Transport ja/nein

### Umwelt & externe Faktoren
22. Wetter (Temperatur, Regen, Schnee)
23. Extreme Wetterlagen (Hitze, Glätte)
24. Events (Feste, Sportveranstaltungen)
25. Tourismus (z. B. Skigebiete im Winter)
26. Verkehrslage (Stau, Unfälle)

### Korrelationen
27. Mehr Einsätze in dicht besiedelten Gebieten
28. Mehr Einsätze bei Events
29. Höhere Einsätze bei älterer Bevölkerung
30. Tageszeitabhängige Einsatzarten

---

## Anforderungen an Mockdaten

- Realistische Verteilungen (keine Zufallsuniformität!)
- Korrelationen zwischen Variablen berücksichtigen
- Konsistente Gebietszuordnung (PLZ)
- Format: CSV oder XLS
- Dokumentation der Generierungslogik