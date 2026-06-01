Basic ReadMe TODO

## Namensvorschläge:
- EMS-Forecast
- EmergencyTeller


## Timeline

### Datenerfassung

- **05.03.2026** Set-up Github und first commit für weitere Planung (Mido)
- **20.03.2026** Erste **Planung** für die ersten Steps (Lukas)
- **22.03.202** Fertigstellung der **Gebietseinteilung** und der **PLZ-Liste** (PLZ-Name-Gebiets_ID); *Quellen: gemeinden.at, wikipedia.com* (Lukas)
- **19.03.2026** Daten sammeln über **Feiertage** und **Ferien**; *Quellen: ferienwiki.at, feiertage-oesterreich.at* (Merjem)
- **22.03.2026** Daten sammeln über **Pfelegeheime**; *Quellen: land-oberoesterreich.gv.at* (Merjem)
- **23.03.2026** API Anbindung von Wetterdaten; *Quellen: geosphere, OpenWeather* (Lukas)
- **23.03.2026** API Anbindung und Visualisierung; *Quellen: EVIS* (Lukas)
- **23.03.2026** Recherche Daten **Bevölkerung (Alter, Geschlecht, plz, Haushalte, Gebiete)** (2025); *Quellen: Statistik Austria, land-oberoesterreich* (Mido)
- **06.04.2026** Daten sammeln über **Events** und **Veranstaltungen** (inkl. Erweiterung der Daten für 2027); *Quellen: oesterreich.gv.at* (Merjem)
- **06.04.2026** Daten sammeln über **Ausflugziele Natur**; (Merjem)
- **13.04.2026** Zur Simulation wurden **synthetische Daten** (Mockdaten) erstellt; (Merjem)
- **14.04.2026** Kompilierung Daten Bevölkerung in json, PLZ referenz bereinigt(Mido)
- **15.04.2026** Abschluss Generierung von **5k Mockdaten** (Lukas)
- **16.04.2026** 1000 Mockdaten erstellt (Mido)
- **30.04.2026** Auswahl des **Datenbanksystems** (Merjem)
- **02.05.2026** Ertellung eines **Frontend-Prototypen** ohne Anbindung (Lukas)
- **12.05.2026** Daten Generierung zur Verkehrsunfall-Statistik der einzelnen Gebiete; *Quellen: Statistik-Austria, evis.gv.at* (Lukas)
- **11-17.05.2026** Recherche KI-Adaptierung (Mido)
- **21.05.2025** ETL-Script für KI-Training, Trainingsdatenvorbereitung (Mido)
- **28.05.2026** **Entities** erstellt (Merjem)



## Meilensteine

1. Infrastruktur- und demografische Daten sind vollständig erhoben, strukturiert und den Gebieten zugeordnet
2. Ein konsistenter und bereinigter Einsatzdatensatz ist erstellt und enthält alle relevanten Attribute
3. Alle Datenquellen sind in einer zentralen Datenbank integriert und korrekt verknüpft
4. Zeitliche und räumliche Muster sind analysiert und durch Visualisierungen sowie dokumentierte Erkenntnisse nachgewiesen
5. Die Vorhersageaufgabe ist formal definiert und ein geeigneter Modellansatz wurde begründet ausgewählt
6. Ein Prognosemodell ist implementiert, trainiert und anhand definierter Metriken bewertet
7. Eine API stellt Einsatzprognosen für definierte Parameter korrekt bereit
8. Eine Weboberfläche visualisiert die Prognosen in einer interaktiven Kartenansicht
9. Alle Komponenten sind integriert und ermöglichen eine durchgängige Demonstration
10. Das Modellverhalten ist analysiert und hinsichtlich Aussagekraft und Grenzen kritisch bewertet
