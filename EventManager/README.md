# Meridian 59 Event Manager

Ein C# Windows Forms Tool zur Verwaltung von Game-Events auf dem Meridian 59 Server.

## Features

- **Server-Verbindung** via Admin-Socket (Port 9998)
- **Event-Planung** mit Datum/Uhrzeit-Auswahl
- **Automatisches Scheduling** - Events werden zum geplanten Zeitpunkt gestartet
- **Event-Typen**:
  - OrcInvasion
  - RatInvasion
  - SpiderInvasion
  - SkeletonInvasion
  - TriggeredInvasion
  - ChaosNight
  - WarEvent
  - NodeAttack
  - EasterEggHunt
  - Qormas
  - Custom (eigene Blakod-Klasse)

- **Recurring Events** - Automatische Wiederholung in definierten Intervallen
- **Live-Monitoring** - Echtzeit-Statusupdates
- **Activity Log** - Vollständiges Logging aller Aktionen

## Installation

### Voraussetzungen

- .NET 8.0 SDK oder höher
- Windows 10/11
- Meridian 59 Server läuft auf localhost:9998 (Maintenance Port)

### Build

```bash
cd EventManager
dotnet build
```

### Run

```bash
dotnet run
```

Oder öffnen Sie `Meridian59EventManager.csproj` in Visual Studio 2022.

## Verwendung

### 1. Server-Verbindung

1. Stellen Sie sicher, dass der Meridian 59 Server läuft
2. Überprüfen Sie die `blakserv.cfg` Konfiguration:
   ```ini
   [Maintenance]
   Port 9998
   Mask ::ffff:127.0.0.1
   ```
3. Klicken Sie auf "Connect" in der Anwendung
4. Bei erfolgreicher Verbindung wird der Status grün

### 2. Event hinzufügen

1. Klicken Sie auf "Add Event"
2. Geben Sie einen Namen ein (z.B. "Orc Invasion Saturday Night")
3. Wählen Sie den Event-Typ (z.B. "OrcInvasion")
4. Setzen Sie Start-Datum und -Zeit
5. Optional: Aktivieren Sie "Schedule End Time" für automatisches Beenden
6. Optional: Aktivieren Sie "Recurring Event" für wiederholende Events
7. Klicken Sie auf "OK"

### 3. Event starten

**Manuell (sofort):**
- Wählen Sie das Event aus der Liste
- Klicken Sie auf "Start Now"

**Geplant (zum festgelegten Zeitpunkt):**
- Event wird automatisch vom Scheduler gestartet
- Oder: Wählen Sie das Event und klicken "Schedule Event" um es zum Server zu senden

### 4. Event abbrechen

- Wählen Sie das Event aus der Liste
- Klicken Sie auf "Cancel"

## Admin-Commands

Die Anwendung sendet folgende Commands an den Server:

### Event sofort starten
```
Send o 0 startevent event_type class <ClassName>
```
Beispiel:
```
Send o 0 startevent event_type class OrcInvasion
```

### Event planen (in X Minuten)
```
SEND OBJECT 3 ScheduleEventInMinutes iClass=&<ClassName> minutes=<N>
```
Beispiel:
```
SEND OBJECT 3 ScheduleEventInMinutes iClass=&OrcInvasion minutes=60
```

### Event beenden
```
SEND OBJECT 3 EventEnd parm1=<EventObjectID>
```

### Server-Status abfragen
```
SHOW STATUS
```

### Aktive Timer anzeigen
```
SHOW TIMERS
```

## Architektur

### Komponenten

```
Meridian59EventManager
│
├── Models/
│   └── GameEvent.cs          # Event-Datenmodell
│
├── Core/
│   ├── AdminSocketConnector.cs  # TCP-Kommunikation mit Server
│   └── EventScheduler.cs        # Event-Scheduling-Logik
│
├── MainForm.cs                   # Haupt-GUI
├── AddEventDialog.cs             # Event-Erstellungs-Dialog
├── EventDetailsDialog.cs         # Event-Details-Ansicht
└── Program.cs                    # Entry Point
```

### Kommunikationsfluss

```
[C# Event Manager]
        ↓
    TCP Socket (Port 9998)
        ↓
[Meridian 59 Admin Interface]
        ↓
[Admin Command Processor]
        ↓
[Blakod GameEventEngine (Object 3)]
        ↓
[Specific Event Class (z.B. OrcInvasion)]
```

## Event-Status

- **Scheduled** - Event ist geplant, wartet auf Ausführung
- **Active** - Event läuft aktuell
- **Completed** - Event wurde erfolgreich beendet
- **Cancelled** - Event wurde manuell abgebrochen
- **Failed** - Event-Start ist fehlgeschlagen

## Troubleshooting

### "Connection failed"
- Prüfen Sie, ob der Server läuft
- Prüfen Sie den Maintenance-Port in `blakserv.cfg`
- Prüfen Sie die Firewall-Einstellungen

### "Event failed to start"
- Prüfen Sie das Activity Log für Details
- Überprüfen Sie, ob die Event-Klasse im Server existiert
- Prüfen Sie, ob ein Event mit `vbUnique = TRUE` bereits läuft

### Events starten nicht automatisch
- Prüfen Sie, ob der Scheduler läuft (Status sollte "Connected" sein)
- Überprüfen Sie die geplante Zeit (muss in der Zukunft liegen)
- Prüfen Sie das Activity Log für Fehler

## Erweiterte Features

### Recurring Events

Für regelmäßige Events (z.B. wöchentliche Orc-Invasion):

1. Event erstellen
2. "Recurring Event" aktivieren
3. Intervall in Stunden festlegen (z.B. 168 für wöchentlich)
4. Nach jedem Event wird automatisch ein neues für die nächste Wiederholung erstellt

### Custom Events

Für eigene Blakod-Event-Klassen:

1. Event-Typ "Custom" wählen
2. Blakod-Klassennamen eingeben (z.B. "MyCustomInvasion")
3. Stellen Sie sicher, dass die Klasse von `GameEvent` erbt

## Server-Konfiguration

### blakserv.cfg

```ini
[Maintenance]
Enabled Yes
Port 9998
Mask ::ffff:127.0.0.1
DisableTimeout No
```

### Blakod Event-Engine

Der Server nutzt die `GameEventEngine` (Objekt-ID 3) zur Event-Verwaltung.

Relevante Dateien:
- `kod/util/gameeventengine.kod` - Event-Engine
- `kod/util/gameevent.kod` - Basis-Event-Klasse
- `kod/util/gameevent/invasion/*.kod` - Invasion-Events

## Lizenz

Dieses Tool ist Teil des Meridian 59 Server-Projekts.

## Support

Bei Problemen oder Fragen:
1. Prüfen Sie das Activity Log in der Anwendung
2. Prüfen Sie die Server-Logs in `run/server/channel*.txt`
3. Konsultieren Sie die Blakod-Dokumentation

## Version

Version 1.0 - Initial Release
