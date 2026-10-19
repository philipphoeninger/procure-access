# Design-Review ProcureAccess, Runde 2 (Schritt 7e)

**Gegenstand:** Figma-Datei „ProcureAccess“ (`rPeSyK8NNe3d8AnUEXW9mt`), Seiten „Komponenten“ und „Screens“, Stand 6. Oktober 2026 nach den Review-Punkten 1–6 und den Nachträgen (S10 mit vier Schritten und S10-R, S11, S34 Überblick, S34a-J, S34b, S40, S41, S42, K1, K6, Eintragszeile, `pa-rating-legend`)
**Grundlagen:** ux_copy_procureaccess_v6.md, kriterienkatalog_bit_inklusiv_v1.md, ARCHITECTURE_v5.md, TESTING_v3.md, design_handoff_procureaccess_v1.md (v1.3)
**Methode:** Skill „design-critique“ (erster Eindruck, Nutzbarkeit, Hierarchie, Konsistenz, Barrierefreiheit)
**Vorgänger:** design_critique_procureaccess_v1.md (Runde 1, Befunde U1–U8)
**Stand:** 6. Oktober 2026. Alle Befunde sind in Figma umgesetzt.

---

## 1. Gesamteindruck

Die Filterschichten in S10 funktionieren: Ausgeschlossene Optionen bleiben sichtbar und nennen den Grund, und der Hinweis nach dem Zurücksetzen ist klar. Die Regel „kein Gesamturteil“ ist auch mit acht Zählwerten eingehalten.

Den größten Hebel hatten zwei Stellen:
- **Kriterienkarte (S11):** Sie hatte zu viele aufklappbare Bereiche.
- **Eintragszeile (S41):** Die Zählwerte waren schwer zu vergleichen.

## 2. Nutzbarkeit – Befunde und Umsetzung

| Nr. | Befund | Schwere | Empfehlung | Status |
|---|---|---|---|---|
| U28 | **S11, Kriterienkarte:** Zwei aufklappbare Bereiche pro Karte („Begründung anzeigen“ in K5 und „Details anzeigen“). Aufgeklappt stehen zwei Panels untereinander, und die Karte wird sehr lang. | 🟡 | Ein einziger Bereich „Details anzeigen“, mit „Warum diese Priorität?“ als erstem Abschnitt. K5 behält nur das Menü zum Ändern der Stufe. | ✅ K5 hat die neue Eigenschaft `Show why button` (in S11 und S11-M aus). Die Begründung ist der erste Abschnitt der Details und bleibt damit erreichbar (CAT-02). |
| U29 | **S11, Karte „Geprüft nach …“:** Die Karte hatte von ihrer Vorlage „Details anzeigen“ ohne Inhalt sowie „Von Ihnen geändert“ und „Empfehlung wiederherstellen“ geerbt. | 🟡 | Ohne Details-Button und ohne Änderungszustand. | ✅ |
| U30 | **S41, Eintragszeile:** Die acht Zählwerte liefen als Fließtext und brachen in jeder Zeile anders um, also nicht vergleichbar. | 🟡 | Ein festes Raster, damit jeder Wert in jeder Zeile an derselben Stelle steht. | ✅ Zwei Spalten in fester Reihenfolge: Erfüllt, Leichte Einschränkung, Einschränkung, Barriere, Blockade, Nicht anwendbar, Nicht eingeschätzt. „Damals nicht im Katalog“ steht wie bisher abgesetzt darunter. Die Trenner „·“ entfallen. Vier Spalten wären bei 437 px Spaltenbreite umgebrochen. |
| U31 | **S41:** Bewertungsstufen ohne Erklärung, während S42 und S34b sie bereits haben. | 🟡 | `pa-rating-legend`, zugeklappt, unter „Mit welcher Kriterienliste vergleichen?“. | ✅ In S41 und S41-M. |
| U32 | **S11, Zusammenfassung:** Ein Satz mit vier Schichten über drei Zeilen lässt sich schlecht überfliegen. | 🟢 | Als Beschreibungsliste. | ✅ „48 Kriterien für Ihre Auswahl“ und vier Zeilen Begriff/Wert. In S11-M stehen Begriff und Wert untereinander. |
| U33 | **S34 Überblick:** Der Hinweis zur Veröffentlichung stand doppelt (Kopf und Karte). Ungleich hohe Spalten mit viel Leerraum. | 🟢 | Hinweis in der Karte streichen; Prüfnachweis und Einschätzung untereinander. | ✅ |
| U34 | **S40:** Der Filter „Prüfverfahren“ sah aus wie eine Einfachauswahl, laut Spezifikation ist er eine Mehrfachauswahl. | 🟢 | Mehrfachauswahl kennzeichnen. | ✅ Hilfetext „Mehrfachauswahl möglich“ bei Anwendungstyp und Prüfverfahren, „Tippen, um zu filtern“ beim Verwendungszweck. |

## 3. Visuelle Hierarchie

- **Was zuerst ins Auge fällt:** In allen geprüften Screens sind das H1 und die primäre Aktion. Das ist richtig so.
- **S10, Schritt 3:** Bei eingeschalteten Erklärungen wird die Liste lang, wirkt aber ruhig. Dass der Schalter standardmäßig aus ist, passt.
- **S34b:** Die aufgeklappte Erklärung der Stufen schiebt das erste Kriterium unter die Falz. Das ist vertretbar, weil sie nach der ersten Einschätzung zugeklappt erscheint (Handoff §4.11).
- **S10, Schritt 4:** Die Domains in den Hinweisen sahen aus wie Fließtext. → ✅ Jetzt als Link gestaltet (primary, unterstrichen).

## 4. Konsistenz

| Element | Problem | Empfehlung | Status |
|---|---|---|---|
| S11-M (320 px) | Karten noch im alten Aufbau („Fundstelle in den Normen“, keine Metazeile, kein „Details anzeigen“) | An S11 angleichen | ✅ Originaltitel, Metazeile, K5 ohne eigenen Begründungs-Button, Buttons untereinander, erste Karte aufgeklappt |
| S10 | Keine 320-px-Fassung | S10-M ergänzen | ✅ S10-M (Schritt 2 mit ausgeschlossenen Optionen): Stepper ersetzt durch Textzeile „Schritt 2 von 4: Anwendungstyp“, „Bisherige Auswahl“ untereinander, Aktionen gestapelt |
| S40 | Einleitung sagte „Menschen, die die Software nutzen“ | Text nachziehen | ✅ „… die die Produkte nutzen“ |
| S40 | Verwendungszweck-Feld schmaler als die Auswahlfelder | Gleich breit | ✅ |
| K1 | Variante „Workspace“ nur als abgelöste Kopie in S34 | Echte Variante | ✅ `Scope=Workspace, NotCovered=None` im Komponentenset; S34 nutzt eine Instanz |
| K1 | Beim Prüfen gefunden: Die Zähltexte waren nur teilweise an die Komponenteneigenschaften gebunden. Instanzen konnten deshalb nicht alle Werte über Eigenschaften setzen. | Bindung in allen Varianten | ✅ Alle Zählwerte in allen Varianten gebunden. Der Titel der Region ist nur bei Scope Configuration und Workspace gebunden; Public zeigt fest „Eingeschätzte Kriterien“. |
| Überschriften in Karten | S34 Überblick nutzt Title Large für H2, S34a-J kleinere Titel | Einheitlich | ✅ H2 in Karten = Title Large |

## 5. Barrierefreiheit

| Thema | Ergebnis |
|---|---|
| Kontrast | Die Gründe bei ausgeschlossenen Optionen (onSurfaceVariant) sind gut lesbar ✅. Die ausgegrauten Checkboxen sind als inaktive Bedienelemente von der Kontrastregel ausgenommen ✅ |
| Zielgrößen | Schalter (52 × 32 mit Label), Checkboxen und Text-Buttons sind mindestens 48 px groß ✅. In S11-M sind die Buttons gestapelt, jeder 48 px hoch ✅ |
| Lesbarkeit | Die Spalte mit den Begriffen in `pa-rating-legend` ist 168 px breit; unter 600 px stapeln sich Begriff und Erklärung ✅ |
| Offen für 7f | Fokus-Annotationen für S10 (alle Schritte, S10-M), S34 Überblick und S34a-J; Ansage der Zählwerte im neuen Raster der Eintragszeile (Lesereihenfolge zeilenweise prüfen) |

## 6. Was gut funktioniert

- **Ausgeschlossene Optionen** mit Grund statt Verschwinden. So versteht man, warum etwas fehlt.
- **Hinweis nach dem Zurücksetzen (S10-R):** Er nennt die betroffenen Schichten, statt still Auswahlen zu verlieren.
- **Detailansicht eines Kriteriums:** Sie übersetzt die BIT-Texte in Fragen („Wen betrifft es?“), ohne den Wortlaut zu verändern. Jetzt enthält sie auch die Begründung der Priorität, an einem Ort statt zwei.
- **S34 Überblick:** Er zeigt nur Fakten und Wege zu den Tabs. Der Bereich „Nur für Ihre Organisation“ ist deutlich abgesetzt.

## 7. Bewusst offen gelassen

| Punkt | Grund | Wohin |
|---|---|---|
| S55 Schritt 2 „Prüfung“ | Weiterhin nicht gestaltet; Muster S34a-J | Handoff H10 |
| 320-px-Fassungen für S10 Schritte 1, 3, 4, S34 Überblick, S40, S42 | Folgen den Regeln aus S10-M, S11-M und S41-M (Handoff §2.2) | bei Bedarf |
| Erklärungstexte der Bewertungsstufen | Fachliche Prüfung steht aus | Kriterienkatalog K-8 |

## 8. Neue und geänderte UI-Texte für die nächste UX-Copy-Fassung

| Vorschlag Schlüssel | Screen | Text |
|---|---|---|
| `criteria.list.summary.count` | S11, S11-M | {{count}} Kriterien für Ihre Auswahl |
| `criteria.list.summary.useCase` / `.appTypes` / `.parts` / `.testTypes` | S11, S11-M | Begriffe: Verwendungszweck, Anwendungstyp, Funktionsbereiche, Prüfverfahren (als `<dt>`; Werte als `<dd>`). Ersetzt `criteria.list.summary` und `criteria.list.summary.testTypes` |
| `criteria.item.detail.priority` | S11 | Warum diese Priorität? (erster Abschnitt der Details; Inhalt wie `criteria.priority.rationale`, `criteria.priority.rule.*`, `criteria.priority.source.expert`) |
| `common.multiSelect.hint` | S40 | Mehrfachauswahl möglich |

## 9. Neue und geänderte Komponenten (Figma)

| Komponente | Änderung | Angular-Entsprechung |
|---|---|---|
| K5 `pa-priority-indicator` | neue Eigenschaft `Show why button` (Standard: an) | Input `showRationaleToggle` (Standard `true`); in S11 `false`, die Begründung rendert die Kriterienkarte |
| K1 `pa-fulfillment-breakdown` | Variante `Scope=Workspace`; alle Zähltexte an Eigenschaften gebunden | `scope: 'configuration' \| 'public' \| 'workspace'` |
| `pa-entry-row` | Layout Wide: Zählwerte als Raster mit zwei Spalten, ohne „·“ | CSS Grid `grid-template-columns: 1fr 1fr`, Liste bleibt `<ul>` in fester Reihenfolge |

## 10. Neue Frames

| Frame | Node |
|---|---|
| S10-M (Schritt 2, 320 px) | `2103:2940` |
| K1 Variante `Scope=Workspace, NotCovered=None` | `2101:3180` |

## 11. Nächster Schritt

**7f – Barrierefreiheitsprüfung** der neuen und geänderten Screens (S10 inklusive S10-R und S10-M, S11/S11-M, S34 Überblick, S34a-J, S40, S41). Schwerpunkte:
- Fokusreihenfolge in der Combobox und bei ausgeschlossenen Optionen
- Ansage beim Zurücksetzen
- Lesereihenfolge des Zählrasters
