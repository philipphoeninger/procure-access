# Barrierefreiheitsprüfung ProcureAccess, Runde 2 (Schritt 7f)

**Standard:** WCAG 2.1 AA, ergänzt um 2.4.11 und 2.5.8 aus WCAG 2.2 (relevant für Mobile-first) · **Datum:** 12. Oktober 2026
**Gegenstand:** Figma-Datei „ProcureAccess“ (`rPeSyK8NNe3d8AnUEXW9mt`)
- **Seit Runde 1 neu oder geändert:** S10 Schritte 1–4, S10-R, S10-M, S11/S11-M, S34 Überblick, S34a-J, S34b, S40, S41, S42
- **Neue Mobile-first-Frames (360 px):** S42-M, S40-M, S10-M1, S34-M, S13-M, S34b-M, S51-M, S51-M2

**Methode:** Skill „accessibility-review“. Kontraste aus den Farbvariablen berechnet, Struktur und Zielgrößen in Figma geprüft, Tastatur- und Screenreader-Verhalten aus Design und Spezifikation abgeleitet.
**Vorgänger:** accessibility_review_procureaccess_v1.md (Runde 1)
**Ergebnis:** Alle Befunde sind in Figma umgesetzt bzw. als Annotation (Kategorie „Barrierefreiheit“) und im Handoff (v1.6) festgehalten.

---

## Zusammenfassung

**Befunde:** 10 · **Kritisch:** 0 · **Erheblich:** 5 · **Gering:** 5

Die neuen Muster sind grundsätzlich gut zugänglich:
- Ausgeschlossene Optionen bleiben sichtbar und nennen den Grund.
- Zurücksetzungen werden angesagt.
- Kontraste aller neuen Farbpaare erfüllen AA.

Die erheblichen Befunde betreffen vor allem das Smartphone: von Tastatur oder fester Leiste verdeckter Fokus, gleichnamige Buttons und Überschriftenebenen.

## Befunde

### Wahrnehmbar

| Nr. | Befund | WCAG | Schwere | Umsetzung |
|---|---|---|---|---|
| B1 | **S11, Detailbereich:** Die Abschnittsüberschriften waren als `h4` spezifiziert, der Kriterientitel ist aber `h2`. Damit fehlt eine Ebene. | 1.3.1 | 🟡 | ✅ Abschnitte als `h3` (Figma-Ebenen umbenannt, Handoff §5.5, Annotation) |
| B2 | **S34 Überblick:** K1 bringt eine eigene `h3` mit, steht aber unter der `h3` „Einschätzung nach Kriterien“. | 1.3.1 | 🟢 | ✅ K1 erhält hier `headingLevel=4` (Annotation) |
| B3 | **S42-M:** Die Tabelle „Einschätzung je Kriterium“ wird unter 600 px zur Liste. Dabei dürfen keine Daten verloren gehen und die Reihenfolge muss gleich bleiben. | 1.3.1, 1.3.2 | 🟢 | ✅ `<ul>` mit `h3` und `<dl>`, Katalogreihenfolge, gleiche Daten (Annotation) |

### Bedienbar

| Nr. | Befund | WCAG | Schwere | Umsetzung |
|---|---|---|---|---|
| B4 | **S10-M1 und S51-M2:** Die Bildschirmtastatur kann das fokussierte Feld oder die Optionsliste verdecken. | 2.4.11 (2.2) | 🟡 | ✅ Feld beim Fokus an den oberen Rand bzw. in die Mitte scrollen; Listenhöhe aus `visualViewport`; nichts unten fixiert, solange ein Feld fokussiert ist (Handoff §2.2, Annotationen) |
| B5 | **S13-M (Vollbild-Dialog):** Die feste Aktionsleiste kann fokussierte Elemente im Inhalt verdecken. | 2.4.11 (2.2) | 🟡 | ✅ `scroll-padding-bottom` = Höhe der Leiste + `env(safe-area-inset-bottom)`; Fokusfalle, Esc, `aria-labelledby` (Annotation) |
| B6 | **Gestapelte Aktionen in den Mobile-Frames:** Die primäre Aktion stand oben, im DOM aber zuletzt. Das widerspricht der Regel „DOM-Reihenfolge = sichtbare Reihenfolge“. | 2.4.3, 1.3.2 | 🟡 | ✅ In S10-M, S13-M und S51-M steht die primäre Aktion jetzt unten, in DOM-Reihenfolge. Die Regel steht ausdrücklich im Handoff §2.2 |
| B7 | **S10 Schritt 4:** Links in den Hinweisen hießen nur „softwaretest.bit-inklusiv.de“. | 2.4.4 | 🟢 | ✅ „Verfahren ansehen (Domain)“; öffnet in neuem Tab mit Hinweis; eigener Tab-Stopp nach der Checkbox |
| B8 | **Zielgrößen auf dem Smartphone:** Links in K2 waren Text-Buttons, die nicht umbrechen und abgeschnitten wurden. | 2.5.8 (2.2), 1.4.10 | 🟢 | ✅ In K2 jetzt umbrechende Links mit einer Zielhöhe von mindestens 48 px |

### Verständlich

| Nr. | Befund | WCAG | Schwere | Umsetzung |
|---|---|---|---|---|
| B9 | **S10:** Für „Weiter“ ohne Auswahl gab es keinen gestalteten Fehlerzustand. | 3.3.1, 3.3.2 | 🟡 | ✅ Neuer Frame **S10-E**: Feld im Fehlerzustand mit `mat-error`, `aria-invalid`, Fokus ins Feld, Seitentitel mit „Fehler:“. Keine Fehlerübersicht, weil es nur ein Feld gibt |
| B10 | **S51-M:** Lange Fragen als schwebendes Label werden in `mat-form-field` abgeschnitten. Die Frage ist dann nicht vollständig lesbar. | 3.3.2, 1.4.10 | 🟢 | ✅ Sichtbares `<label>` über dem Feld, im Feld nur ein Kurzlabel (Handoff §2.2 „Forms“) |

### Robust

Keine neuen Befunde. Hinweise zur Umsetzung, im Design annotiert:
- **Ausgeschlossene Optionen in S10:** `mat-checkbox` mit `disabledInteractive` (Angular Material ≥ 19) bzw. `aria-disabled="true"` statt `disabled`, damit die Option fokussierbar und vorlesbar bleibt.
- **Gleichnamige Buttons:** „Bearbeiten“ (S34 Überblick) und „Begründung hinzufügen“ (K6 kompakt) erhalten den Kontext per `aria-describedby`, also die Kartenüberschrift bzw. den Kriteriumstitel.
- **Schalter „Erklärungen anzeigen“:** `role="switch"` mit `aria-checked` (`mat-slide-toggle`).

## Kontrastprüfung (neue Farbpaare)

| Element | Vordergrund | Hintergrund | Kontrast | Erforderlich | OK? |
|---|---|---|---|---|---|
| Gründe bei ausgeschlossenen Optionen, Hinweise | onSurfaceVariant | surface | 8,88 : 1 | 4,5 : 1 | ✅ |
| Hinweise in Karten (S42-M, K6 kompakt) | onSurfaceVariant | surfaceContainerLowest | 9,34 : 1 | 4,5 : 1 | ✅ |
| Erklärung der Stufen, Filterbereich | onSurfaceVariant | surfaceContainerLow | 8,47 : 1 | 4,5 : 1 | ✅ |
| Platzhalter Bildschirmtastatur | onSurfaceVariant | surfaceContainerHighest | 7,21 : 1 | 4,5 : 1 | ✅ |
| Links und Text-Buttons | primary | surface / surfaceContainerLow / surfaceContainer | 6,12 / 5,84 / 5,6 : 1 | 4,5 : 1 | ✅ |
| Aktive Option in Listen | onSecondaryContainer | secondaryContainer | 7,18 : 1 | 4,5 : 1 | ✅ |
| Schalter an (Track / Griff) | primary / onPrimary | surface / primary | 6,12 / 6,44 : 1 | 3 : 1 | ✅ |
| Feldrahmen | outline | surface / surfaceContainerLow | 4,33 / 4,13 : 1 | 3 : 1 | ✅ |
| Ausgegraute Checkboxen (ausgeschlossen) | onSurface 38 % | surface | – | ausgenommen (inaktives Element) | ✅ |
| Fehlermeldung S10-E | error | surface | wie Runde 1 (≥ 4,5 : 1) | 4,5 : 1 | ✅ |

## Tastaturbedienung (neue Muster)

| Element | Tab-Reihenfolge | Enter/Leertaste | Esc | Pfeiltasten |
|---|---|---|---|---|
| Combobox Verwendungszweck (S10, S40) | ein Tab-Stopp | Enter wählt die aktive Option | schließt die Liste, Wert bleibt | ↑/↓ durch die Optionen, „Keiner davon passt“ zuletzt |
| Checkbox-Gruppe mit ausgeschlossenen Optionen | jede Option ein Tab-Stopp, auch ausgeschlossene | Leertaste: ausgeschlossene Option ändert nichts, der Grund wird gelesen | – | – |
| Schalter „Erklärungen anzeigen“ | ein Tab-Stopp | Leertaste schaltet | – | – |
| „Details anzeigen“ (S11) | nach „Priorität“ | klappt auf/zu, Fokus bleibt | – | – |
| Filter-Disclosure (S40-M) | nach dem Suchfeld | klappt auf/zu | – | – |
| K6 kompakt (`mat-select`) | ein Tab-Stopp je Kriterium | öffnet die Auswahl | schließt | ↑/↓ wählen |
| Vollbild-Dialog (S13-M) | Fokusfalle, Start auf dem gewählten Radio | – | schließt, Fokus zurück | Radio-Gruppe mit ↑/↓ |
| Tabs (S34-M, scrollend) | aktive Registerkarte ein Tab-Stopp | aktiviert | – | ←/→ zwischen Tabs, Leiste scrollt mit |

## Screenreader (neue Muster)

| Element | Wird angesagt als | Hinweis |
|---|---|---|
| Ausgeschlossene Option | „Dokument, Kontrollkästchen, nicht verfügbar, nicht aktiviert, Passt nicht zum Verwendungszweck HR-Anwendung“ | `aria-disabled` + `aria-describedby` |
| Hinweis nach Zurücksetzen | „Auswahl zurückgesetzt. Sie haben den Verwendungszweck geändert …“ (polite) | einmalig, kein Fokuswechsel |
| Zählraster der Eintragszeile | Liste mit 7 Einträgen, zeilenweise: „51 erfüllt, 3 leichte Einschränkung, …“ | DOM-Reihenfolge = feste Reihenfolge; Raster nur per CSS |
| Erklärung der Stufen | „Was bedeuten die Bewertungsstufen?, Schaltfläche, reduziert“ | Panel als Beschreibungsliste |
| K6 kompakt | „Formulare lassen sich …, Gruppe; Einschätzung, Erfüllt, Auswahlfeld“ | `fieldset`/`legend` |

## Was gut funktioniert

- Keine Information nur über Farbe: Ausschlüsse, Fehler, Stufen und Status haben immer Text.
- Die Mobile-first-Regeln vermeiden Hover-Abhängigkeiten. Statt Tooltips gibt es Disclosures.
- Zielgrößen: alle neuen Bedienelemente sind mindestens 48 px groß, auch die Links in K2.

## Offen für die manuelle Prüfung (TESTING_v3 §7.2)

| Punkt | Wie prüfen |
|---|---|
| Listbox der Combobox bei offener Bildschirmtastatur | Android/Chrome + TalkBack, iPhone/Safari + VoiceOver, je mit größter Systemschrift |
| Ansage von Treffern in Combobox und Filter (Häufigkeit, keine Doppelansagen) | NVDA + Firefox, VoiceOver iOS |
| Scrollende Tab-Leiste bei 320 px | Tastatur und Wischgesten |
| Vollbild-Dialog: Fokusfalle, Zurück-Geste unter Android schließt den Dialog | TalkBack |

## Neue UI-Texte (für die nächste UX-Copy-Fassung)

| Vorschlag Schlüssel | Text |
|---|---|
| `candidate.tabs.assessment.short` | Einschätzung |
| `candidate.assessment.select` | Einschätzung |
| `candidate.assessment.note.add` / `.edit` | Begründung hinzufügen / Begründung bearbeiten |
| `directory.filter.toggle` | Filter und Sortierung ({{count}} aktiv) |
| `directory.filter.hint.mobile` | Anwendungstyp und Prüfverfahren: Mehrfachauswahl. Filter wirken sofort; die Trefferzahl unten wird angesagt. |
| `testType.link` | Verfahren ansehen ({{domain}}) |

## Neue Frames

| Frame | Node |
|---|---|
| S10-E (Schritt 1, Fehler) | `2139:3332` |
| Mobile-first 360 px: S42-M, S40-M, S10-M1 (Viewport), S34-M, S13-M, S34b-M, S51-M, S51-M2 (Viewport) | `2132:3382`, `2134:2905`, `2134:3174`, `2134:3184`, `2134:3825`, `2136:3127`, `2136:4005`, `2136:4187` |
| Komponentenvarianten Layout=Narrow: `pa-entry-card`, K3 | `2132:3174`, `2132:3249`; im Set `2010:348` |
