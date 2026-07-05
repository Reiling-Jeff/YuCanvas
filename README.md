# YuCanvas

**Ein Desktop-Client für Canvas LMS mit einer Notenprognose, die Canvas selbst nicht liefert.**

![Avalonia UI](https://img.shields.io/badge/UI-Avalonia%2011-8B5CF6)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![Plattformen](https://img.shields.io/badge/Plattformen-Linux%20·%20Windows%20·%20macOS-34D399)
![Lizenz](https://img.shields.io/badge/Lizenz-GPL--2.0-blue)

YuCanvas holt deine Kurse, Aufgaben und Fristen live aus der Canvas-API und bündelt
sie in einem Dashboard. Gebaut mit **Avalonia UI** und **C# / .NET 8**,
plattformübergreifend lauffähig auf Linux, Windows und macOS.

---

## Was es kann

- **Live-Sync mit Canvas** -> Kurse, Aufgaben und Einreichungsstatus direkt über die
  Canvas REST API, parallel geladen für kurze Wartezeiten.
- **Notenprognose** -> rechnet Punkte aus bestandenen Intermediate- und
  Advanced-Aufgaben in eine Note um (nach einem gestaffelten Punkte-Schema).
- **Fortschritt pro Kurs** -> Anteil der bestandenen Basic-Übungen, als Balken auf
  jeder Kurskarte.
- **Deadline-Übersicht** -> sammelt alle anstehenden Fristen und hebt die nächste im
  Banner hervor, inklusive „in X Tagen".
- **Sammelfristen-Logik** -> erkennt Aufgaben, deren Deadline stellvertretend für
  einen ganzen Block an Basic-Übungen gilt, und löst sie zur nächsten offenen
  Basic auf.
- **Einstellungen in der App** -> Canvas-Zugang, Sync-Verhalten und Startseite werden
  direkt in der App verwaltet.
- **Update-Erkennung** -> die App merkt selbst, wenn eine neue Version veröffentlicht
  wurde, in den Einstellungen kann man sich den Changelog direkt in der App anschauen.
- **Lokaler Cache** -> beim Start erscheinen sofort die zuletzt geladenen Daten,
  während im Hintergrund frisch synchronisiert wird.

---

## Tech-Stack

| Bereich                 | Technologie                                       |
|-------------------------|---------------------------------------------------|
| UI-Framework            | Avalonia UI 11                                    |
| Sprache / Runtime       | C# · .NET 8                                       |
| MVVM                    | CommunityToolkit.Mvvm                             |
| API-Zugriff             | System.Net.Http.Json                              |
| Markdown-Rendering      | Markdown.Avalonia                                 |
| Konfiguration / Secrets | Microsoft.Extensions.Configuration (User Secrets) |
| Schriftart              | Inter                                             |

---

## Projektstruktur

```
YuCanvas/
├── Calculator/       Notenberechnung (Punkte → Note)
├── Controls/         Wiederverwendbare UI-Bausteine (Sidebar, Topbar)
├── Json/             Datenmodelle für die Canvas-API-Antworten
├── Media/            Modelle und Hilfen (Kurse, Deadlines, App-Einstellungen)
├── Models/
│   └── ViewModels/   Ein ViewModel pro Ansicht
├── Service/          Canvas-Anbindung, Sync, Cache, Einstellungen, Update-Prüfung
├── Styles/           Farbpalette und Control-Styles (Glassmorphism, Hover)
├── Views/
│   └── Components/   Kleine wiederverwendbare Bausteine (z. B. Badge)
├── App.axaml         Anwendungseinstieg, Ressourcen-Registrierung
├── MainWindow.axaml
├── Program.cs        Startpunkt, Konfiguration
├── CHANGELOG.md      Änderungen pro Version (in der App als Popup abrufbar)
└── VERSION           Versionsstand, gegen den die Update-Prüfung vergleicht
```

Die Trennung folgt einem klaren Prinzip: **`Service` redet mit Canvas und liefert
rohe Daten, `Models` entscheidet, was damit passiert, `Views` zeigt es an.**

---

## Einrichtung

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download) oder neuer
- Ein Canvas-Konto
- Ein persönliches Canvas-Token

### Ein Canvas-Token erstellen

1. In Canvas einloggen → **Account → Settings**.
2. Unter **Approved Integrations** auf **+ New Access Token** klicken.
3. Einen Namen vergeben, Token erzeugen und **sofort kopieren**.

### Repo clonen

```bash
git clone https://github.com/Reiling-Jeff/YuCanvas.git
```

> Gehe vorher im Terminal mit `cd` in einen Ordner, in dem du YuCanvas ablegen möchtest.
>
> Optional kannst du mit `git checkout dev` die Version in Entwicklung auswählen -
> bedeutet: schnellere Updates, mehr Features. Features können dort jedoch
> unvollständig sein.

### Starten und verbinden

```bash
dotnet restore
dotnet run
```

Beim ersten Start öffnest du **Einstellungen → Canvas-Verbindung**, trägst deine
Canvas-URL und dein Token ein und klickst auf **Speichern**.

> Solltest du Student*in der SAE sein, ist die URL `https://canvas.sae.edu`.

Beim ersten Start ist der lokale Cache leer, daher lädt die App direkt frisch von
Canvas. Ab dem zweiten Start erscheinen die Kurse sofort aus dem Cache, während im
Hintergrund synchronisiert wird.

---

## Versionierung & Updates

Die App vergleicht ihre eingebaute Version mit der Datei
[`VERSION`](VERSION) auf dem `main`-Branch. Gibt es dort einen neueren Stand,
erscheint in **Einstellungen → Aktualisierungen** ein Hinweis, und der zugehörige
[`CHANGELOG`](CHANGELOG.md) lässt sich direkt in der App ansehen.

---

## Wie die Notenprognose funktioniert

Aufgaben werden nach ihrem Namenspräfix in drei Kategorien eingeteilt und mit
Punkten gewichtet:

| Kategorie    | Präfix | Punkte pro bestandener Aufgabe |
|--------------|--------|--------------------------------|
| Basic        | `B…`   | 1                              |
| Intermediate | `I…`   | 2                              |
| Advanced     | `A…`   | 4                              |

Für die Note zählen die Punkte **über** den Basics - also die aus bestandenen
Intermediate- und Advanced-Aufgaben. Die Summe wird über eine gestaffelte Tabelle
in eine Note übersetzt: Von einem niedrigen Punktestand (z. B. `3-`) steigt es bis zur Bestnote (`1++`) bei voller Punktzahl.

Diese Logik ist bewusst offen gehalten und lässt sich im Ordner `Calculator` an
ein anderes Schema anpassen.

>Zukünftig soll dies in den Einstellungen in der App möglich sein!

---

## Konfigurierbares

- **Sammelfristen** – In der `DashboardViewModel` gibt es eine Liste von
  Aufgaben-IDs, deren Deadline für einen ganzen Block Basics gilt. Trag dort die
  IDs ein, die in deinem Kurs als Sammelabgabe fungieren.
- **Notenschema** – Punktwerte und Schwellen liegen im `Calculator`-Ordner und
  sind zentral änderbar.

>Auch diese Einstellungen sollen in die App integriert werden

---

## Roadmap

### 🧭 Geplant

- [ ] Mehr Einstellungen
- [ ] Noten-Bereich
- [ ] Kalender
- [ ] Module
- [ ] Ankündigungen
- [ ] Dateien
- [ ] Nachrichten
- [ ] „in X Tagen"-Anzeige aktualisiert sich ohne Neustart

### 💡 Ideen

- [ ] Bessere Aufgaben übersicht

### ✅ Umgesetzt

- [x] Aufgaben direkt in YuCanvas einsehen
- [x] Einstellungen in der App
- [x] Update-Erkennung mit Changelog-Popup
- [x] Sync, Fortschritt, Notenprognose, Deadlines, Cache

Alle Details zu abgeschlossenen Versionen stehen im [CHANGELOG](CHANGELOG.md).

---

## Lizenz & Hinweis

YuCanvas steht unter der [GPL-2.0-Lizenz](LICENSE).

YuCanvas ist ein privates Projekt und steht in keiner Verbindung zu Instructure
oder Canvas LMS. „Canvas" ist eine Marke von Instructure, Inc.