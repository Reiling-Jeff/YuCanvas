# YuCanvas

**Ein Desktop-Client für Canvas LMS mit einer Notenprognose, die Canvas selbst nicht liefert.**

YuCanvas holt deine Kurse, Aufgaben und Fristen live aus der Canvas-API und bündelt sie in einem Dashboard.

Gebaut mit **Avalonia UI** und **C# / .NET 8**, plattformübergreifend lauffähig auf Linux, Windows und macOS.

---

## Was es kann

- **Live-Sync mit Canvas** Kurse, Aufgaben und Einreichungsstatus direkt über die Canvas REST API.
- **Notenprognose** rechnet Punkte aus bestandenen Intermediate- und Advanced-Aufgaben in eine Note um (nach einem gestaffelten Punkte-Schema).
- **Fortschritt pro Kurs** Anteil der bestandenen Basic-Übungen, als Balken auf jeder Kurskarte.
- **Deadline-Übersicht** sammelt alle anstehenden Fristen und hebt die nächste im Banner hervor, inklusive „in X Tagen".
- **Sammelfristen-Logik** erkennt Aufgaben, deren Deadline stellvertretend für einen ganzen Block an Basic-Übungen gilt, und löst sie zur nächsten offenen Basic auf.
- **Lokaler Cache** beim Start erscheinen sofort die zuletzt geladenen Daten, während im Hintergrund frisch synchronisiert wird.

---

## Tech-Stack

| Bereich | Technologie |
| --- | --- |
| UI-Framework | Avalonia UI 11 |
| Sprache / Runtime | C# · .NET 8 |
| MVVM | CommunityToolkit.Mvvm |
| API-Zugriff | System.Net.Http.Json |
| Konfiguration / Secrets | Microsoft.Extensions.Configuration (User Secrets) |
| Schriftart | Inter |

---

## Projektstruktur

```
YuCanvas/
├── Calculator/     Notenberechnung (Punkte → Note)
├── Controls/       Wiederverwendbare UI-Bausteine (Sidebar, Topbar)
├── Json/           Datenmodelle für die Canvas-API-Antworten
├── Media/          Farb- und Brush-Hilfen
├── Models/         ViewModels (u. a. DashboardViewModel)
├── Service/        Canvas-Anbindung und lokaler Cache
├── Styles/         Farbpalette und Control-Styles (Glassmorphism, Hover)
├── Views/          Fenster und Ansichten (Dashboard)
├── App.axaml       Anwendungseinstieg, Ressourcen-Registrierung
├── MainWindow.axaml
└── Program.cs      Startpunkt, Konfiguration
```

Die Trennung folgt einem klaren Prinzip: **`Service` redet mit Canvas und liefert rohe Daten, `Models` entscheidet, was damit passiert, `Views` zeigt es an.**

---

## Einrichtung

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download) oder neuer
- Ein Canvas-Konto mit API-Zugriff
- Ein persönliches Canvas-Zugriffstoken

### Ein Canvas-Token erstellen

1. In Canvas einloggen → **Account → Settings**.
2. Unter **Approved Integrations** auf **+ New Access Token** klicken.
3. Einen Namen vergeben, Token erzeugen und **sofort kopieren**

### Repo Clonen

Zuerst musst du das Repository clonen:
```bash
git clone https://github.com/Reiling-Jeff/YuCanvas.git
```
> Gehe vorher im terminal it `cd` in einen Ordner, in dem du YuCanvas Ablegen möchtest.
>
> Optional kannst du mit `git checkout dev` die version die in entwickelung ist auswählen, bedeutet: schnellere updates, mehr features. (features können in dem fall noch unvollständig sein.)

### Secrets hinterlegen

**WICHTIG:** Folgendes kann sich noch ändern!

YuCanvas liest Basis-URL und Token über **User Secrets**. Im Projektverzeichnis:

```bash
dotnet user-secrets init
dotnet user-secrets set "Canvas:BaseUrl" "[URL zu deinem Canvas]"
dotnet user-secrets set "Canvas:Token" "[dein token]"
```

> User Secrets liegen außerhalb des Projektordners (`~/.microsoft/usersecrets/...`) und werden nicht mit committet.
> 
> solltest du Student des SAE's sein, ist die URL "https://canvas.sae.edu"

### Starten

```bash
dotnet restore
dotnet run
```

Beim ersten Start ist der lokale Cache leer, daher lädt die App direkt frisch von Canvas. Ab dem zweiten Start erscheinen die Kurse sofort aus dem Cache, während im Hintergrund synchronisiert wird.

---

## Wie die Notenprognose funktioniert

Aufgaben werden nach ihrem Namenspräfix in drei Kategorien eingeteilt und mit Punkten gewichtet:

| Kategorie | Präfix | Punkte pro bestandener Aufgabe |
| --- | --- | --- |
| Basic | `B…` | 1 |
| Intermediate | `I…` | 2 |
| Advanced | `A…` | 4 |

Für die Note zählen die Punkte **über** den Basics - also die aus bestandenen Intermediate- und Advanced-Aufgaben. Die Summe wird über eine gestaffelte Tabelle in eine Note übersetzt: Von einem niedrigen Punktestand (z. B. `3-`) steigt es in Bändern bis zur Bestnote (`1++`) bei voller Punktzahl. Jede Punktzahl fällt so eindeutig in eine Notenstufe.

Diese Logik ist bewusst offen gehalten und lässt sich im Ordner `Calculator` an ein anderes Schema anpassen.

---

## Konfigurierbares

- **Sammelfristen** - In der `DashboardViewModel` gibt es eine Liste von Aufgaben-IDs, deren Deadline für einen ganzen Block Basics gilt. Trag dort die IDs ein, die in deinem Kurs als Sammelabgabe fungieren.
- **Notenschema** - Punktwerte und Schwellen liegen im `Calculator`-Ordner und sind zentral änderbar.

---

## Status

Frühe, funktionierende Version. Kernfunktionen - Sync, Fortschritt, Notenprognose, Deadlines und Cache - laufen. Auf der Liste für später: Live-Aktualisierung der „in X Tagen"-Anzeige ohne Neustart, Abgaben in YuCanvas Einsehen, In App Einstellungen, ohne im code anpassungen vornehmen zu müssen. Und mehr!

---

## Hinweis

YuCanvas ist ein privates Projekt und steht in keiner Verbindung zu Instructure oder Canvas LMS. „Canvas" ist eine Marke von Instructure, Inc.
