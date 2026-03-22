# Projektbezeichnung:
Vorhersagesystem für Einsatzwahrscheinlichkeiten im Rettungsdienst (Oberösterreich)

# Projektauftraggeber:
Rotes Kreuz Oberösterreich (Praxispartner im Rahmen der Diplomarbeit)  
HTL Leonding – Abteilung Informatik  

# Projekthintergrund:
Rettungsorganisationen wie das Rote Kreuz stehen täglich vor der Herausforderung, Einsatzmittel optimal zu verteilen, um schnelle Reaktionszeiten sicherzustellen.

Derzeit basieren viele Entscheidungen auf Erfahrung, historischen Mustern und situativer Einschätzung. Durch die zunehmende Verfügbarkeit von Daten besteht die Möglichkeit, diese Entscheidungsprozesse durch datenbasierte Analysen zu unterstützen.

Ziel ist es, historische Einsatzdaten auszuwerten und daraus Wahrscheinlichkeiten für zukünftige Einsätze in bestimmten geografischen Gebieten abzuleiten. Dadurch kann eine effizientere Ressourcenplanung ermöglicht werden.

# Projektauslöser / Vorprojekt:
Die Projektidee entstand in Zusammenarbeit mit dem Roten Kreuz, welches Interesse an einer datenbasierten Unterstützung zur Einsatzprognose geäußert hat.

Ein konkretes Vorprojekt existiert nicht. Erste Überlegungen und Konzeptideen wurden im Rahmen der Diplomarbeitsfindung entwickelt.

# Projektendergebnis:
Am Ende des Projekts soll ein funktionsfähiger Prototyp vorliegen, der:

- historische Einsatzdaten analysiert
- daraus Einsatzwahrscheinlichkeiten für definierte Gebiete berechnet
- diese Ergebnisse in einer interaktiven Kartenoberfläche visualisiert

## Messbare Erfolgskriterien:
- Mindestens 20.000 Datensätze (real oder synthetisch) verarbeitet
- Darstellung von mindestens 10 geografischen Gebieten
- Prognosefunktion für zukünftige Zeitfenster (z. B. nächste Stunde)
- Visualisierung als Heatmap oder farblich codierte Karte
- API zur Abfrage von Prognosedaten vorhanden
- Nachvollziehbare Modelllogik (keine Black-Box ohne Erklärung)

# Die Projektorganisation:

Projektteam:
- Projektleiter: Lukas Holzmair 
- Entwickler: Merjem Ramic
- Entwickler: Mido Zieser-Zerenko

Betreuung:
- David Klewein
- externer Ansprechpartner (Rotes Kreuz)

## Organigramm (vereinfacht):

Auftraggeber (Rotes Kreuz)
        |
Projektbetreuung (HTL)
        |
Projektteam 

# Projektziel(e):

## Hauptziele:
- Entwicklung eines Systems zur Analyse historischer Rettungseinsätze
- Berechnung von Wahrscheinlichkeiten für zukünftige Einsätze je Gebiet
- Visualisierung der Ergebnisse in einer interaktiven Karte
- Bereitstellung der Daten über eine API

## Teilziele:
- Aufbau einer geeigneten Datenstruktur (inkl. Geodaten)
- Erstellung und Aufbereitung eines Datensatzes (real oder synthetisch)
- Implementierung eines einfachen, nachvollziehbaren Vorhersagemodells
- Entwicklung eines Backends zur Datenbereitstellung
- Entwicklung eines Frontends zur Darstellung der Ergebnisse

## Nicht-Projektziele:
- Exakte Vorhersage einzelner Einsatzorte
- Echtzeit-Integration in bestehende Rettungssysteme
- Produktivbetrieb oder medizinische Entscheidungsunterstützung

# Projektbeschreibung:

Das Projekt umfasst die Entwicklung eines prototypischen Systems zur Vorhersage von Einsatzwahrscheinlichkeiten im Rettungsdienst.

Auf Basis historischer Einsatzdaten werden Muster in Bezug auf Zeit und Ort analysiert. Diese Muster werden genutzt, um für definierte geografische Gebiete Wahrscheinlichkeiten für zukünftige Einsätze zu berechnen.

Die Ergebnisse werden über eine API bereitgestellt und in einer webbasierten Anwendung visualisiert. Die Darstellung erfolgt über eine Karte, in der Gebiete entsprechend ihrer prognostizierten Einsatzwahrscheinlichkeit eingefärbt werden.

# Projekthauptaufgaben:

## 1. Datenbeschaffung und -aufbereitung
- Erstellung oder Import eines geeigneten Datensatzes
- Bereinigung und Strukturierung der Daten
- Anreicherung mit zusätzlichen Merkmalen (z. B. Wochentag, Uhrzeit)

## 2. Datenanalyse
- Untersuchung von zeitlichen und räumlichen Mustern
- Erstellung von Statistiken und Visualisierungen
- Identifikation relevanter Einflussfaktoren

## 3. Modellentwicklung
- Entwicklung eines einfachen Prognosemodells
- Training und Test des Modells
- Bewertung der Modellgüte

## 4. Backend-Entwicklung
- Aufbau einer API zur Bereitstellung von Daten und Prognosen
- Integration des Modells in das Backend

## 5. Frontend-Entwicklung
- Entwicklung einer interaktiven Kartenansicht
- Darstellung von historischen Daten und Prognosen
- Implementierung von Filter- und Auswahlmöglichkeiten

## 6. Integration und Testing
- Zusammenführung aller Komponenten
- Durchführung von Tests
- Fehlerbehebung und Optimierung

## 7. Dokumentation
- Technische Dokumentation
- Projektdokumentation
- Vorbereitung der Präsentation

# Projektphasen / Meilensteine:

| Phase | Meilenstein / Ergebnis | Soll-Termin | Freigabe |
|------|------------------------|------------|----------|
| 1 | Projektstart & Anforderungsdefinition abgeschlossen | TBD | Projektbetreuer |
| 2 | Datenmodell & Datensatz erstellt | TBD | Projektbetreuer |
| 3 | Erste Datenanalyse & Visualisierungen vorhanden | TBD | Projektbetreuer |
| 4 | Erstes Prognosemodell implementiert | TBD | Projektbetreuer |
| 5 | Backend (API) funktionsfähig | TBD | Projektbetreuer |
| 6 | Frontend mit Kartenvisualisierung fertig | TBD | Projektbetreuer |
| 7 | Gesamtsystem integriert & Demo bereit | TBD | Projektbetreuer |
| 8 | Dokumentation & Präsentation abgeschlossen | TBD | Projektbetreuer |