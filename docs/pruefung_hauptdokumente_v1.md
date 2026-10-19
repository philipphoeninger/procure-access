# Prüfung der Hauptdokumente vor SPEC_v1

**Stand:** 13. Oktober 2026

**Geprüft:**
- `prd_procureaccess_werteit_v4.md`
- `ARCHITECTURE_v9.md`
- `TESTING_v7.md`
- `ux_copy_procureaccess_v11.md`
- `design_handoff_procureaccess_v1.md` (1.11)
- `figma_code_mapping_procureaccess_v1.md` (1.9)
- `kriterienkatalog_bit_inklusiv_v1.md` (1.4)
- in Figma alle Screens, die am 12. und 13. Oktober neu hinzugekommen sind oder geändert wurden

**Vorgehen:** Drei unabhängige Prüfdurchgänge. Keiner davon hat die Dokumente verfasst.

| Durchgang | Methode (Skill) | Gegenstand |
|---|---|---|
| A | Konsistenzprüfung über alle Dokumente | Widersprüche, Altstände, Schlüssel, IDs, Verweise |
| B | `engineering:architecture` und `engineering:testing-strategy` | Architekturänderungen C41–C52, Teststrategie |
| C | `design:accessibility-review`, `design:design-critique` und `design:ux-copy` | Figma S32, S32-M, S34a-L, S34b, S51, S55-1, S55-2, S40-V |

**Schwere:**
- 🔴 = würde zu falscher Umsetzung führen
- 🟡 = Widerspruch oder Lücke, vor SPEC_v1 zu beheben
- 🟢 = klein

**Ohne Befund:**
- Alle UI-Schlüssel aus Handoff, Mapping und TESTING existieren in der UX-Copy v11.
- Alle Invarianten-IDs existieren in TESTING_v7, ohne Lücken und ohne Doppelungen.
- Jeder P0-Punkt der PRD hat einen Platz in Architektur, UX und Tests.

## 1. Entscheidungen für den Product Owner

| Nr. | Frage | Empfehlung |
|---|---|---|
| D1 | Können mehrere Projekte dieselbe Kriterienliste nutzen? Heute ändert eine Anpassung (C51) dann alle Projekte und auch Freigabelinks. | Nein: Ein Projekt bekommt immer eine eigene Kopie. Die vier Filterschichten sind nach der Zuordnung fest. |
| D2 | Was zeigt ein Eintrag der Anbieterseite ohne Einschätzungen? Heute: Abgleich, in dem alles „Nicht eingeschätzt“ ist. | Keinen Abgleich, nur den Hinweis „Keine Einschätzungen veröffentlicht“. |
| D3 | Wer darf einen aus dem Projekt veröffentlichten Eintrag aktualisieren? | Nur das Beitragsprofil, unter dem veröffentlicht wurde. Kolleginnen können zurückziehen und neu veröffentlichen. |
| D4 | Was passiert beim Widerruf der Einwilligung zu einem Erfahrungsbericht? Heute wird er nur ausgeblendet; Text und Hilfsmittel bleiben gespeichert. | Text und Hilfsmittel werden gelöscht. Ein minimaler Moderationsnachweis bleibt. Den Zeitpunkt legt die rechtliche Prüfung fest. |
| D5 | Erfahrungsbericht: Anwendungstyp und Verwendungszweck. Die UI fragt den Anwendungstyp nur bei Einschätzungen und den Verwendungszweck nie; die Architektur erlaubt beides frei. | Anwendungstyp optional immer anbieten. Kein Verwendungszweck bei Erfahrungsberichten (Architektur und Filter anpassen). |
| D6 | O-3: Neue Liste aus einer alten Liste für eine neue Katalogversion | Wie in §16.3.1 vorgeschlagen übernehmen |
| D7 | O-5: Freigabelinks ohne Konto lesbar? | Für Kriterienlisten ja; für Projekte in v1 keine Freigabelinks |

## 2. Befunde, die ohne Entscheidung behoben werden

### Architektur und Tests (B)

| Nr. | Schwere | Befund | Behebung |
|---|---|---|---|
| B-F7 | 🟡 | Die Höchstdauer von 8 Stunden würde nie greifen. Der Cookie-Handler setzt `IssuedUtc` bei jeder Verlängerung neu. Ein eigener `OnValidatePrincipal` schaltet außerdem den Security-Stamp-Validator ab. | Anmeldezeit in `AuthenticationProperties.Items` speichern; Validator verketten; `ExpiresUtc` beim Verlängern setzen; SEC-08 |
| B-F8 | 🟡 | Die Standardverlängerung greift erst nach der Hälfte der Zeit. Damit läge die tatsächliche Leerlaufzeit zwischen 15 und 30 Minuten. | `OnCheckSlidingExpiration` mit `ShouldRenew = true`, außer für die Sitzungsabfrage; Ablaufzeiten im Antwort-Header |
| B-F9 | 🟡 | Anmeldung und Registrierung brauchen ein Antiforgery-Token, bevor es eines gibt. Ein 401 auf die Sitzungsabfrage würde anonyme Nutzende umleiten. | Anonymes `GET /auth/session` gibt 200 und setzt das Token; Token beim An- und Abmelden erneuern |
| B-F10 | 🟡 | Die Data-Protection-Schlüssel werden nicht dauerhaft gespeichert. Jeder Container-Neustart beendet alle Sitzungen. | Schlüssel in der Datenbank speichern und mit sichern; OPS-04 |
| B-F11 | 🟡 | Schritt 3 baut das Speichern auf dem alten JWT, Schritt 4 ersetzt es. | Cookie-Anmeldung und Antiforgery in Schritt 3 vorziehen |
| B-F12 | 🟡 | Nach einem 401 geht nur der Entwurf der Kriterienauswahl nicht verloren. Bei der Höchstdauer verlieren andere Formulare ihre Eingaben (WCAG 2.2.1). | Anmeldung als Overlay ohne Seitenwechsel, danach die Anfrage wiederholen |
| B-F3 | 🟡 | Meldungen und Moderation lassen nicht erkennen, welche Fassung gemeint war. | Optionale `PublishedEntryVersionId`; erneutes Veröffentlichen behält den Status; zurückgezogene Einträge können nicht neu veröffentlicht werden |
| B-F4 | 🟡 | Der Unique-Index für `clientRequestId` liegt auf einer Spalte, die es nicht gibt. In Flow B fehlt die ID. | Spalte an der Fassung; Flow B ergänzen; gleiche ID mit anderem Inhalt → 409 |
| B-F5 | 🟡 | Die CHECK-Bedingung „nach Herkunftsart“ geht über zwei Tabellen. | Regel nur in der Factory; SIG-03 anpassen |
| B-F6 | 🟡 | Die Modulgrenzen sind nicht technisch durchgesetzt; ARC-03 würde scheitern; zwischen Export und Configurator entsteht ein Zyklus. | Ein Projekt je Modul plus Contracts-Projekt; `PersistedConfigurationSource` in Export hinter `IConfigurationReader` |
| B-F14 | 🟡 | `ShareLink` hat keine `OrganizationId` und keine Autorisierung. | `OrganizationId`, `IShareableResourceProvider`, Token gehasht |
| B-F15 | 🟡 | „neuer Prüfnachweis für neue Version“ widerspricht „ein Nachweis je Verfahren“. | Bestehenden Nachweis aktualisieren |
| B-F16 | 🟡 | PUB-13 ist so nicht testbar. | `SaveChangesInterceptor` gegen Änderungen an Fassungen |
| B-F17 | 🟡 | Sitzungstests mit echten Wartezeiten sind langsam und instabil. | `FakeTimeProvider` und `page.clock`; Zeiten konfigurierbar |
| B-F18, B-F19 | 🟢 | Configurator-Entitäten fehlen im Migrationsplan; mehrere Altstände | Sammelkorrektur |

### Konsistenz (A)

| Nr. | Schwere | Befund | Behebung |
|---|---|---|---|
| A-F1 | 🔴 | K2 und K4 kennen nur *ein* Prüfverfahren, aber die API liefert mehrere Prüfnachweise. | Eingaben als Liste; Darstellung mehrerer Nachweise; neue Texte |
| A-F2, A-F3 | 🟡 | Uneinheitlich, wann die Zeile „Damals nicht im Katalog“ erscheint | Überall: alle Scopes außer Workspace |
| A-F4 | 🟡 | Fehlerfeld heißt noch `testEvidence.testStatus` | `testStatus` |
| A-F5 | 🟡 | Im Glossar wird „Geprüft nach …“ noch eingeschätzt | „abgeleitet aus Prüfnachweis: Ja / Nein / Nicht angegeben“ |
| A-F7 | 🟡 | Filter `testKind` erlaubt nur einen Wert | `testKinds[]` |
| A-F8 | 🟡 | `clientRequestId` fehlt in Flow B | ergänzen (= B-F4) |
| A-F9, A-F11 | 🟡 | Die Einwilligung ist an einzelnen Stellen noch bedingt beschrieben | angleichen |
| A-F10, A-F12 | 🟡 | Sperre der Liste ungenau; „Einschätzung vorhanden“ nicht definiert | Formulierung wie WSP-02/03; „Wert ≠ Nicht eingeschätzt“ |
| A-F13 | 🟡 | `CatalogReleaseAppTypeProcedure` fehlt in Migrationsschritt 2 | ergänzen |
| A-F14 | 🟡 | H10/O17 als offen geführt, obwohl erledigt | schließen |
| A-F15 | 🟡 | Die gemeinsame Liste offener Punkte ist unvollständig: UX O2, O8, O19 fehlen | ergänzen |
| A-F16 | 🟡 | `Idempotency-Key`, `EntryVersion` | ersetzen |
| A-F17 | 🟡 | „Geprüfte Version“ steht in Schritt 1 von S55, gespeichert wird sie je Prüfnachweis | Feld in jede Prüfungskarte in Schritt 2 |
| A-F18 bis A-F25 | 🟢 | Nummerierung O-10, Abhängigkeitstabelle, Header, Zählungen, Skriptpfad, Katalogtexte | Sammelkorrektur |
| Verweise | 🟢 | PRD v4 und eine Stelle in TESTING verweisen auf ältere Fassungen | aktualisieren |

### Gestaltung, Barrierefreiheit, Texte (C)

| Nr. | Schwere | Befund | Behebung |
|---|---|---|---|
| C-F1 | 🔴 | S32 und S34b zählen die Kriterien „Geprüft nach …“ in Fortschritt und Abgleich mit. | Beispiel auf 46 Anforderungen; Umfang „+ 2 × Geprüft nach … (zählen nicht mit)“; Zählregel in SPEC |
| C-F2 | 🔴 | S55-1: Die Frage nach den Anwendungstypen liegt außerhalb der Feldgruppe und benennt sie daher nicht. | Frage als `legend` in die Gruppe; Fehlervariante |
| C-F3 | 🟡 | Die Beispieldaten zu BewerberPro widersprechen sich zwischen S32, S34, S34a-L und S34b. | Ein durchgehender Datensatz |
| C-F4 | 🟡 | S34b zeigt „Geprüft nach …“ nur für eines von zwei Verfahren. | Eine Zeile je Verfahren |
| C-F5 | 🟡 | Der Veröffentlichungshinweis steht auch bei „Ja“. | Nur bei „Nicht bekannt“ |
| C-F6, C-F8 | 🟡 | In den Prüfnachweis-Karten fehlen Link, private Notiz, Datumshinweis und die Anbieter-Beschriftungen. | ergänzen |
| C-F7 | 🟡 | S34a-L springt von H1 zu H3. | H2 „Prüfnachweise“ |
| C-F9 | 🟡 | Mal „Prüfung“, mal „Prüfnachweis“; in S55-2 fehlen Schlüssel. | einheitlich „Prüfnachweis“; Schlüssel |
| C-F10 | 🟡 | S55-1: Der Verwendungszweck steht nach den Anwendungstypen, schließt aber Anwendungstypen aus. | Verwendungszweck zuerst, wie in S10 |
| C-F11, C-F12 | 🟡 | S55-1: Hinweis zum Treffer am falschen Feld, Mehrfachauswahl ohne Chips, Texte weichen von den Schlüsseln ab | korrigieren |
| C-F13 | 🟡 | Stepper: „Zurück“ in Schritt 1, Rest der Fettung, kein Häkchen bei erledigten Schritten | korrigieren |
| C-F14 | 🟡 | S32-M: verschachtelte Karten, die Zahlen im Abgleich brechen mitten im Eintrag um | Trennlinien statt Karten; Abgleich als Liste |
| C-F15 | 🟡 | „3 leichte Einschränkung“ ist grammatisch falsch. | „Leichte Einschränkung: 3“, auch in der Eintragszeile des Produktverzeichnisses |
| C-F16 | 🟡 | „Produkt hinzufügen“ ohne Menü-Kennzeichen | Pfeil-Symbol, `aria-haspopup` |
| C-F17 | 🟡 | Kein Link „Im Produktverzeichnis ansehen“ bei veröffentlichten Produkten | ergänzen |
| C-F18 | 🟡 | S51: Widerrufshinweis und Hinweis „öffentlich sichtbar“ doppeln sich. | Hinweis zusammenführen |
| C-F19 | 🟡 | S51 Desktop: lange Fragen als Beschriftung im Feld | Muster von S51-M auf allen Breiten |
| C-F20 | 🟡 | S34b: Link „Prüfnachweis bearbeiten“ nur 16 px hoch | Textbutton mit 48 px Höhe |
| C-F21 | 🟡 | S40-V: zwei Hinweisboxen hintereinander | zusammenführen |
| C-F22 bis C-F24 | 🟢 | Typografie der abgeleiteten Zeile, doppelte Links, Kanten | angleichen |

## 3. Entscheidungen (Product Owner, 13. Oktober 2026)

D1 bis D7 wurden wie empfohlen entschieden.

## 4. Umsetzung

Erledigt am 13. Oktober 2026:

| Dokument | Neue Fassung | Änderungen |
|---|---|---|
| PRD | `prd_procureaccess_werteit_v5.md` | Q5–Q12 |
| Architektur | `ARCHITECTURE_v10.md` | C53–C70, §16.4.14 |
| Teststrategie | `TESTING_v8.md` | V32–V45 |
| UX-Copy | `ux_copy_procureaccess_v12.md` | U54–U70 |
| Handoff | 1.12 | |
| Mapping-Dokument | 1.10 | |
| Kriterienkatalog | 1.5 | |

**Figma:** Alle Befunde aus Durchgang C sind umgesetzt. Neue Knoten:
- abgeleitete Zeilen in S34b: `2151:4493`, `2151:4505`
- Button „Produkt hinzufügen“ mit Menü-Pfeil: `2150:4448` (S32) und `2150:4454` (S32-M), vom Komponenten-Original gelöst

Für das veröffentlichte BewerberPro zeigen S34, S34a-L und S34b „Eintrag aktualisieren“.

**FigJam:**
- Die v7-Boards 04, 05, 09 und 17 sind inhaltlich angepasst.
- Neben den Boards 01, 07 und 08 stehen Notizen mit den Änderungen aus ARCHITECTURE_v10.

**Verweise:** Alle Verweise außerhalb von Änderungstabellen zeigen auf die neuen Fassungen. Geprüft per Textsuche, ebenso Invarianten-IDs und UI-Schlüssel.

### Bei der Umsetzung zusätzlich festgelegt

Punkt 1 hat der Product Owner am 13. Oktober 2026 bestätigt.

1. Veröffentlichen aus einem Projekt setzt eine zugeordnete Kriterienliste voraus. Ohne Liste blockiert die API mit 422, und die UI zeigt `publish.blocked.noCriteria`.
2. **Verzeichnisfilter:** `testTypeIds[]` und `testKinds[]` wirken als „oder“.
3. **Erfahrungsbericht ohne Anwendungstyp:** Er bietet die Kriterien aller Anwendungstypen mit allen sechs Stufen an.
4. **Technische Details in ARCHITECTURE_v10:**
   - Sitzungs-Header `X-Session-Expires-At` und `X-Session-Absolute-Expires-At`
   - Anmeldezeit in `Items["pa:signedInUtc"]`
   - Data-Protection-Schlüssel im Schema `identity`
   - Testendpunkt `POST /e2e/clock/advance`
   - `RequestHash` für die Wiederholungsprüfung
   - Freigabelinks werden über ihre ID gelöscht, weil die Token gehasht gespeichert sind
5. **Nutzungsereignisse:** `CriteriaListShown` und `ExportCreated` sind für die PRD-Metriken benannt; die endgültige Liste legt SPEC_v1 fest.
6. **Sitzungswarnung:** Durch die Anmeldung im Overlay gehen Eingaben nicht mehr verloren. Der Text von `shell.session.warning.body` ist entsprechend angepasst.

### Weiterhin offen

- ~~H18~~ **Erledigt:** K4 zeigt eine Liste der Prüfnachweise. K2 nennt alle Prüfverfahren.
- **S55-1:** Für Mehrfachauswahl gibt es in der Figma-Datei keine Chip-Komponente. Eine Development-Annotation am Feld (`2145:3813`) beschreibt die Umsetzung mit `mat-chip-grid` (Handoff H19).
