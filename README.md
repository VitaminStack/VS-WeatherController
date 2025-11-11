# Weather Controller

Der Weather Controller ist eine Beispiel-Mod für Vintage Story, die ein grafisches Bedienfeld zum Verwalten des Wetters bereitstellt.

## Funktionen
- Öffne die Wettersteuerung mit der Standard-Hotkey-Belegung `O`.
- Wähle Wettermuster, Ereignisse und Windprofile aus und setze sie entweder für die aktuelle Region oder für alle geladenen Regionen.
- Konfiguriere temporale Stürme, indem du ihre Häufigkeit (oder "Aus") direkt im Dialog auswählst.
- Schalte das automatische Wechseln der Wetterlagen ein oder aus.
- Lege eine Niederschlags-Intensität fest oder entferne den Override wieder.

Nur Spieler:innen mit dem Server-Privileg `controlserver` (oder `root`) können Änderungen anwenden.

## Installation

1. Baue das Projekt mit Visual Studio oder `msbuild WeatherController.csproj`.
2. Kopiere die erzeugte `WeatherController.dll` zusammen mit der beiliegenden `modinfo.json` in einen neuen Ordner unterhalb von `%APPDATA%/VintagestoryData/Mods`.
3. Starte Vintage Story neu, um die Mod zu laden.
