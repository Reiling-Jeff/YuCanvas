# Changelog

## 0.1-28W01S

### ⚠️ Breaking Changes

- **Canvas-Zugangsdaten werden jetzt in der App verwaltet.** Beim ersten Start
  werden vorhandene User Secrets einmalig in die App-Einstellungen übernommen,
  danach zählt nur noch, was in der App hinterlegt ist. Änderungen über
  `dotnet user-secrets` haben ab sofort **keine Wirkung mehr**. URL und Token
  werden unter *Einstellungen → Canvas-Verbindung* geändert. Beachte: Das Token
  liegt dadurch unverschlüsselt in der lokalen Datei `YuCanvas/settings.json`
  im Konfigurationsordner deines Systems.
- **Format des lokalen Zwischenspeichers geändert.** Durch die Umstellung des
  Kursmodells ist der Cache älterer Versionen nicht mehr vollständig kompatibel.
  Direkt nach dem Update können z. B. Dozentennamen fehlen, bis die erste
  Synchronisierung den Cache neu geschrieben hat. Falls etwas seltsam aussieht:
  *Einstellungen → Daten → Cache leeren* und neu synchronisieren.

### Neu

- **Einstellungen in der App:** Canvas-URL und Zugriffstoken lassen sich jetzt direkt
  in der App eintragen und speichern, der Umweg über `dotnet user-secrets` im
  Terminal entfällt. Bereits hinterlegte Secrets werden beim ersten Start
  automatisch übernommen.
- **Einstellungsseite mit Suche:** Konto-Übersicht (Name, Nutzer-ID, Sprache),
  automatische Synchronisierung an/aus,
  Cache leeren – alles durchsuchbar.
- **Update-Prüfung:** Die App erkennt beim Start und auf Knopfdruck, ob eine neue
  Version verfügbar ist, und zeigt installierte und neueste Version in den EInstellungen an.
- **Changelog in der App:** Die Änderungen einer neuen Version lassen sich direkt
  als Popup ansehen.
- **Sidebar-Navigation:** Dashboard, Aufgaben und Einstellungen wechseln jetzt
  tatsächlich die Seite, der aktive Menüpunkt wird hervorgehoben.
- **„Soon"- und „Beta"-Kennzeichnung:** Noch nicht verfügbare Bereiche (Noten,
  Kalender, Module, Ankündigungen, Dateien, Nachrichten) sind ausgegraut, als
  „Soon" markiert und zeigen beim Klick einen Hinweis.
  Dashboard und Aufgaben tragen ein Beta-Badge. Gilt auch für „Alle Deadlines"
  im Dashboard.

### Verbessert

- **Synchronisierung deutlich schneller:** Die Aufgaben aller Kurse werden
  parallel statt nacheinander geladen.
- **Verbindungsprobleme:** Fehlende oder ungültige Canvas-URLs führen zu einer
  verständlichen Fehlermeldung, hängende Verbindungen laufen nach 60 Sekunden
  in einen Timeout.
- **Echte Daten statt Platzhalter:** In der Sidebar steht jetzt der Name aus dem
  Canvas-Konto statt Beispieldaten, **einige** erfundene Zähler und Hinweispunkte an
  Menüpunkten wurden entfernt.
- **Schnellerer Programmstart:** Ein überflüssiger Testaufruf beim Start wurde
  entfernt.

### Behoben

- Schlug das Laden eines einzelnen Kurses fehl, scheiterte bisher die komplette
  Synchronisierung. Jetzt werden die übrigen Kurse trotzdem geladen und der
  betroffene Kurs übersprungen.

### Intern

- ViewModels in einen eigenen Namespace reorganisiert, Sync-Logik in einem
  `SyncService` gebündelt, `CanvasCourse` und `Course` zusammengeführt,
  wiederverwendbare Badge-Komponente eingeführt, Tippfehler im Dateinamen des
  Notenrechners korrigiert.