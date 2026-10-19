# Architektur-Review von ARCHITECTURE_v6

**Stand:** 12. Oktober 2026
**Geprüft:** `ARCHITECTURE_v6.md` gegen `prd_procureaccess_werteit_v3.md`, `TESTING_v4.md` und den Kriterienkatalog 1.2
**Vorgehen:** Skill `engineering:architecture` (Bewertung eines Entwurfs). Die Prüfung hat ein separater Prüfdurchgang gemacht, der die Architektur nicht geschrieben hat. Schwerpunkt waren Entscheidungen, deren Änderung später am teuersten wäre: Datenmodell, Modulgrenzen, API-Verträge, Versionierung, Datenschutzgrenzen und Reihenfolge der Migration.

**Schwere:**
- 🔴 = würde später teuren Umbau oder eine falsche Umsetzung verursachen
- 🟡 = Lücke oder Widerspruch, vor SPEC_v1 zu klären
- 🟢 = klein

## Gesamturteil

Der Kern trägt und sollte bleiben:
- versionierter Katalog,
- Veröffentlichung als Kopie,
- drei Schreibwege über eine gemeinsame Factory,
- Kontext am Eintrag statt am Produkt,
- kein gespeicherter Gesamtwert.

Die Befunde betreffen vor allem das Schema und die Modulgrenzen. Ein Befund liegt außerhalb des Dokuments: **.NET 8 erreicht am 10. November 2026 das Support-Ende**, also in vier Wochen. Die sechs 🔴-Befunde sollten vor SPEC_v1 behoben sein. Nach den ersten Veröffentlichungen würden sie zu Datenmigrationen von unveränderlichen Snapshots.

## Befunde

| Nr. | Schwere | Stelle | Befund | Empfehlung |
|---|---|---|---|---|
| R1 | 🔴 | §2.1 | .NET 8, EF Core 8 und ASP.NET Core 8 erreichen am 10.11.2026 das Support-Ende. Das ist dasselbe Problem wie bei Angular 20, aber dringender, weil der Server öffentlich erreichbar ist. .NET 10 ist LTS bis November 2028. | Stack auf .NET 10, EF Core 10 und Npgsql 10 umstellen, in Schritt 0/1 vor dem ersten Testprojekt. **→ Entscheidung E-R1** |
| R2 | 🔴 | §5.5, §4.6, §5.6 | Die erneute Veröffentlichung ist doppelt modelliert: `PublishedEntry.Version` als neue Zeile *und* eine Tabelle `EntryVersion` ohne Felder. Legt eine neue Fassung eine neue Zeile an, hängen Meldungen, Stellungnahmen, Moderationsprotokoll, URLs und die Zustimmung an der alten. | Trennen in einen stabilen `PublishedEntry` (Produkt, Kanal, Herkunft, Profil, Status, aktuelle Fassung) und unveränderliche `PublishedEntryVersion`. Die Fassung enthält Katalogversion, Kontext, Einschätzungen, Prüfnachweis, Zustimmung und Bericht. Moderation hängt am Eintrag. |
| R3 | 🔴 | §7.3, §4.6, §5.5 | Der öffentliche Abgleich nutzt den „eigenen Kriterienumfang“ eines Eintrags, aber dieser Umfang wird nirgends gespeichert. Die Funktionsbereiche aus Flow E gehen verloren, und da Einschätzungen optional sind, lässt sich „Nicht eingeschätzt“ nicht berechnen. | Beim Veröffentlichen für jedes Kriterium im Umfang eine Einschätzungszeile schreiben, `Unknown`, wenn leer. Der Umfang ist dann die Menge dieser Zeilen. Regel für Abgleich mit einer Liste: Ein Kriterium, das es in der Katalogversion gab, das aber außerhalb des Umfangs liegt, zählt als „Nicht eingeschätzt“, nicht als „Damals nicht im Katalog“. |
| R4 | 🔴 | §4.3 | Die Abhängigkeitstabelle passt nicht zu den Abläufen; der Architekturtest ARC-03 würde scheitern. Directory nutzt Configurator (Abgleich), Identity (Beitragsprofile) und Workspace (Recht zum Zurückziehen). Workspace ruft die Factory von Directory auf (Flow B). `ShareLink` wird auch für Projekte genutzt. | Jedes Modul bekommt einen öffentlichen Bereich `*.Contracts` (Schnittstellen, DTOs). Die erlaubten Richtungen werden ausdrücklich festgelegt. Das Zurückziehen läuft über eine Schnittstelle, die Workspace implementiert. `ShareLink` wandert in die Plattform. |
| R5 | 🔴 | §5.1 `AppType.TextTestTypeId` | Die Zuordnung Anwendungstyp → Prüfverfahren ist ein einzelner, nicht versionierter Fremdschlüssel. Ändert sie sich, ändern sich rückwirkend die erlaubten Stufen alter Listen (gegen ADR-002). Für Websites sind später zwei Verfahren nötig (BIK BITV-Test Web, Katalog K-5). | Eine an die Katalogversion gebundene Tabelle `CatalogReleaseAppTypeProcedure` (m:n). |
| R6 | 🔴 | §5.4/5.5 `TestEvidence`, C30 | Zwei Quellen für „geprüft nach“: Der Prüfnachweis kennt genau ein Prüfverfahren, die Kriterien „Geprüft nach …“ erlauben mehrere „Ja“. Ein Produkt, das nach Software-Test **und** BITV-Test geprüft wurde, lässt sich nicht abbilden. Offen bleibt auch, worauf der Filter „Prüfverfahren“ im Verzeichnis wirkt. | **→ Entscheidung E-R6** |
| Y1 | 🟡 | §5.4, C34 | „Liste und Einschätzung haben dieselbe Katalogversion“ wird behauptet, aber nicht erzwungen. Die Kriterienliste eines Projekts kann gewechselt werden. | **→ Entscheidung E-Y1** |
| Y2 | 🟡 | §5.4 | Ein CHECK-Constraint kann `Criterion.Kind` nicht lesen (zwei Joins entfernt). | Art des Kriteriums an der Einschätzung mitspeichern (zusammengesetzter Fremdschlüssel) oder Regel nur in der Domäne; CAT-17 anpassen. |
| Y3 | 🟡 | §4.5, §16.2.8 | Einwilligung nur bei Angaben zu Hilfsmitteln. Auch der Freitext kann Gesundheitsdaten enthalten („als Screenreader-Nutzerin …“). Widerruf und Version des Einwilligungstextes fehlen. | **→ Entscheidung E-Y3** |
| Y4 | 🟡 | PRD §9 „Merken, Teilen … von Produkten“ | Dieser P0-Punkt hat kein architektonisches Zuhause. `withFavorites()` entfällt, und es gibt keinen Weg, ein Produkt aus dem Verzeichnis in ein Projekt zu übernehmen. | „Merken“ = Produkt aus dem Verzeichnis einem Projekt hinzufügen (`softwareProductId`); „Teilen“ = öffentliche Produkt-URL und Freigabelink. In SPEC_v1 festhalten. |
| Y5 | 🟡 | §5.1 `FilterExclusion` | Polymorphe Quelle/Ziel ohne Fremdschlüssel-Integrität | Typisierte, nullable Fremdschlüssel je Art mit CHECK „genau einer gesetzt“ |
| Y6 | 🟡 | §7.2 | Ungeklärt sind vier Punkte (siehe Spalte Empfehlung). | Alle vier in v7 festlegen: (a) Werden die Kontexte aus Flow D/E gegen die Ausschlussregeln geprüft? (b) Zählen Erfahrungsberichte für die Verzeichnisfilter? (c) Flow D nimmt einen Verwendungszweck, Flow E eine Liste – überall Arrays verwenden. (d) Flow B Schritt 6 lässt „Keiner davon passt“ so lesen, als fielen auch die Anwendungstypen weg. |
| Y7 | 🟡 | §4.4 Export | Die Regel „nicht passende Kriterien ablehnen“ würde „Geprüft nach …“ ablehnen, weil es dafür keine Zuordnungszeilen gibt. | Prüfverfahrens-Kriterien gelten über `testTypeIds` als passend. |
| Y8 | 🟡 | §5.5 | `SourceCandidateProductId` am öffentlichen Eintrag führt mit einem Join zur Organisation. | Nie serialisieren (PRV-01/03 erweitern); `ON DELETE SET NULL` |
| G1 | 🟢 | §5.1 | `UseCaseTemplate.OwnerOrganizationId` ist ein P2-Feature im Blattmodul Catalog. | Bis P2 entfernen |
| G2 | 🟢 | D8 | Refresh-Cookie, Token im Speicher, stilles Erneuern und CSRF sind viel Aufwand für eine SPA auf derselben Domain. | Prüfen: Cookie-Anmeldung von ASP.NET Core Identity mit Antiforgery. Gleiche Sicherheit, weniger Code. **→ Entscheidung E-G2** |
| G3 | 🟢 | §7.1 | Cursor-Paginierung mit Sortierung nach Anzahl ist schwierig; für `Idempotency-Key` gibt es keinen Speicher. | Offset-Paginierung im Verzeichnis; eindeutige Client-Request-ID je Beitragsprofil |
| G4 | 🟢 | §12 / TESTING §9, §13 | Angular-Upgrade einmal in Schritt 1, einmal davor; „acht Zählwerte“ ohne Einschränkung auf das Verzeichnis | Angleichen |

## Was bleiben soll

- Versionierter Katalog mit Bindung an die Katalogversion, einschließlich `CatalogReleaseFilterExclusion`
- Veröffentlichung als Kopie
- Eine Factory für drei Wege; Kanal 1 nur über Flow E
- Kontext am Eintrag (ADR-010)
- Kein gespeicherter Wert; „Damals nicht im Katalog“ als eigener Wert
- Prüfstatus mit drei Zuständen im Projekt
- Export ohne Konto mit zwei Quellen und einem Formatierer
- Vereinigung der erlaubten Stufen und „Geprüft nach …“ als eigene Kriterienart (nur die Speicherung wird mit R5 und Y2 angepasst)
- Mandantentrennung über globale Filter; öffentlicher Eintrag ohne Organisation
- Migrationsreihenfolge (Katalog und Mandanten zuerst, alle drei Schreibwege in Schritt 6)
- Angular 22

## Entscheidungen

Entscheidungen des Product Owners vom 12. Oktober 2026, übernommen in `ARCHITECTURE_v7.md`:

| Nr. | Frage | Entscheidung | In ARCHITECTURE_v7 |
|---|---|---|---|
| E-R1 | Plattformversion | .NET 10 (LTS bis November 2028) in Migrationsschritt 0 | C41, ADR-013 |
| E-R6 | Wie wird erfasst, nach welchen Prüfverfahren geprüft wurde? | Mehrere Prüfnachweise je Produkt, einer je Verfahren; „Geprüft nach …“ wird daraus abgeleitet | C46, ADR-012 |
| E-Y1 | Kriterienliste eines Projekts wechseln | Gesperrt, sobald Einschätzungen vorliegen (v1) | C47 |
| E-Y3 | Einwilligung bei Erfahrungsberichten | Für jeden Bericht, mit gespeicherter Version des Einwilligungstextes; Zurückziehen = Widerruf | C48 |
| E-G2 | Anmeldemodell (Refresh-Cookie oder Cookie-Anmeldung von ASP.NET Core Identity) | **Offen**, als O-11 in ARCHITECTURE_v7 §16.5; nötig vor Migrationsschritt 4 | – |

Die übrigen Befunde ohne Entscheidungsbedarf sind ebenfalls übernommen: R2 → C42, R3 → C43, R4 → C44, R5 → C45; Y2, Y4–Y8, G1, G3 und G4 → C49.

**Folgedokumente:**
- `TESTING_v5.md`
- `ux_copy_procureaccess_v9.md`
- Design-Handoff 1.9
- Mapping-Dokument 1.7
- Kriterienkatalog 1.2

FigJam-Boards: siehe ARCHITECTURE_v7, Kopfbereich.

**Nachtrag (13. Oktober 2026):** E-G2 ist entschieden. Der Product Owner hat am 13. Oktober 2026 die Cookie-Anmeldung von ASP.NET Core Identity mit Antiforgery gewählt (O-11; `ARCHITECTURE_v8.md` C50, ADR-006). Die Sitzungsdauer folgte in `ARCHITECTURE_v9.md` (C52), die Umsetzungsdetails in `ARCHITECTURE_v10.md` (C60). Die Tabelle oben bleibt als Stand vom 12. Oktober 2026 unverändert.

## Quellen

- [.NET 8 and .NET 9 end of support (Microsoft .NET Blog)](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)
