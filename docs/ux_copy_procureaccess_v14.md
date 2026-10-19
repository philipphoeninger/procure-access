# UX-Copy ProcureAccess (Projekt WERTE.IT)

> **Status:** v14 – Ergebnis von Phase 2, Schritt 7b („UX copy for key flows“) aus `ai_development_process.md`, angepasst an `ARCHITECTURE_v11.md` und ergänzt um die Texte aus dem Design-Review (7e, `design_critique_procureaccess_v1.md`) und der Barrierefreiheitsprüfung (7f, `accessibility_review_procureaccess_v1.md`), ergänzt um die nachgereichten Screens S40, S35 (Schritt 2), S42, S53 und S37 sowie um die Review-Punkte 1–6 vom 6. Oktober 2026 (Anwendungstypen, Funktionsbereiche, Verwendungszweck, Prüfverfahren, Ausschlussregeln, Kriterien aus BIT inklusiv) und die Nachträge desselben Tages (Überblick eines Produkts im Projekt, Erklärung der Bewertungsstufen, Filterreihenfolge in S40) sowie um die Texte aus Design-Review 2 (`design_critique_procureaccess_v2.md`), Barrierefreiheitsprüfung 2 (`accessibility_review_procureaccess_v2.md`) und den Mobile-first-Screens (12. Oktober 2026) sowie um die Korrekturen aus der Konsistenzprüfung vor SPEC_v1 und aus dem Architektur-Review (12. Oktober 2026) sowie um die Entscheidungen vom 13. Oktober 2026 (Anmeldemodell, Kriterienliste anpassen, Sitzungsdauer) und um die Projektübersicht S32 sowie um die Entscheidungen D1–D7 und die Befunde aus der Prüfung der Hauptdokumente (`pruefung_hauptdokumente_v1.md`, 13. Oktober 2026) sowie um die Texte aus der Feature-Spezifikation (`SPEC_v1.md` §14, 15. Oktober 2026); v14 übernimmt die Bestätigung der Vorschläge vom 19. Oktober 2026 (`SPEC_v2.md` SD19–SD23). Ersetzt v13. Enthält als Grundlage zusätzlich das Screen- und Komponenteninventar (Ersatz für den verschlankten Schritt 7a).
> **Grundlage:**
> - `prd_procureaccess_werteit_v5.md` (Produktumfang, Personas)
> - `ARCHITECTURE_v12.md` (Abläufe A–E, Domänenmodell, UI-Regel §8.4, offene Punkte §16.5)
> - `kriterienkatalog_bit_inklusiv_v1.md` (1.5; Filterschichten, Ausschlussregeln, Bewertungsskala, Darstellung der Kriterien)
> - `TESTING_v10.md` (Invarianten, die Texte und Komponenten betreffen: WSP-01–03, SEC-03, SEC-07, SIG-08–SIG-14, CFG-01–CFG-04, CAT-02, CAT-16, CAT-17, CAT-20, CAT-22, DIR-01, PUB-14, PUB-15, PUB-16, PRV-09, PRV-10, TEN-09, API-02, EXP-05, A11Y-03, A11Y-04)
> - `pruefung_hauptdokumente_v1.md` (Prüfung der Hauptdokumente vom 13. Oktober 2026: Entscheidungen D1–D7 und gemeinsame Inhaltsentscheidungen für Texte und Figma)
> - Figma-Datei „ProcureAccess“ (`rPeSyK8NNe3d8AnUEXW9mt`), Stand 13. Oktober 2026 (inkl. S32, S32-M, S34a-L, S55-1, S55-2, S40-V; diese Frames werden parallel nach denselben Entscheidungen angepasst): Screens S10 (vier Schritte, S10-R, S10-E) bis S55 einschließlich Fehlerzuständen (S13-E, S51-E, S55-E), S34 Überblick, S34a-J, Mobile-first-Fassungen mit 360 px (S42-M, S40-M, S10-M1, S34-M, S13-M, S34b-M, S51-M, S51-M2) und 320-px-Fassungen (S41-M, S11-M, S55-M)
>
> **Sprache:** Deutsch. Englische Texte (`en.json`) folgen später.
> **Nicht Gegenstand:** die Kriterientexte selbst (`PlainLanguageDe`, `RationaleDe` sowie die wörtlich übernommenen Texte von BIT inklusiv). Sie sind Katalog-Inhalt (`kriterienkatalog_bit_inklusiv_v1.md`) und werden redaktionell im Katalog gepflegt, nicht in den UI-Texten. Dasselbe gilt für die Erklärungen der Funktionsbereiche; sie stehen hier nur zur Orientierung.
> **Nicht im Umfang dieser Fassung:** Moderation (P1: Eintrag melden, Stellungnahme), Upload von Konformitätsberichten (P1). Beide werden ergänzt, wenn der jeweilige Migrationsschritt ansteht.

---

### Änderungen gegenüber v13

| # | Änderung | Anlass |
|---|---|---|
| U81 | Kennzeichnung „Vorschlag“ bei S81 (`admin.*`, §5.18) und bei `candidate.remove.*` entfernt: Verwaltung der Produktvorschläge und Entfernen von Produkten aus einem Projekt sind bestätigt; Texte unverändert | SPEC_v2 SD21, SD22; ARCHITECTURE_v12 C87 |
| U82 | Verweise auf ARCHITECTURE_v12, TESTING_v10, Handoff 1.14 | – |

### Änderungen gegenüber v12

| # | Änderung | Anlass |
|---|---|---|
| U71 | **E-Mails** als eigene Texte (§5.17, `email.*`): Bestätigung der Adresse, Passwort zurücksetzen, Hinweis „bereits registriert“, Einladung in eine Organisation, Antwort auf einen Produktvorschlag | SPEC_v1 SD13, §7.6; ARCHITECTURE_v11 C71 |
| U72 | **Bestätigung der E-Mail-Adresse und Sperre nach Fehlversuchen** (`auth.confirm.*`, `auth.login.locked`); Versandfehler (`errors.email.sendFailed`) | SPEC_v1 R-AUTH-01–03; C72 |
| U73 | **Erscheinungsbild** hell, dunkel oder wie im Betriebssystem (`settings.appearance.*`); Sprache vorerst nur Deutsch (`settings.language.hint`); `settings.textSize` entfällt | SPEC_v1 SD11, SD12 |
| U74 | **Organisation:** Einladung an andere Adresse, letztes Mitglied mit „Verwalten“, Organisation verlassen (`org.invite.mismatch`, `.invalid`, `org.members.lastOwner`, `org.leave.*`); Konto löschen als einziges Mitglied mit „Verwalten“ (`account.delete.soleOwner`) | SPEC_v1 R-ORG-02–04, R-AUTH-10 |
| U75 | **Kriterienliste löschen** (`criteria.saved.delete.*`); Gültigkeit von Freigabelinks (`criteria.share.expiry.hint`, `.error`) | SPEC_v1 R-SAV-04, R-SAV-06, SD15 |
| U76 | **Produkt aus dem Projekt entfernen**, doppeltes Produkt, archivierte Projekte (`candidate.remove.*`, `candidate.duplicate`, `projects.filter.archived`, `projects.archived.note`) | SPEC_v1 R-CAN-01, R-CAN-09 (Vorschlag O-S3), R-PRJ-05 |
| U77 | **Produktseite:** Kriterienliste eines Projekts im Vergleich, Link kopieren, „Zu einem Projekt hinzufügen“, Hinweis auf frühere Fassung (`directory.product.compare.projectCopy`, `.copyLink`, `.addToProject.*`, `directory.entry.olderVersion*`) | SPEC_v1 R-ENT-04, R-ENT-09–11 |
| U78 | **Datenexport** beschreibt den Umfang nach SPEC_v1 §8.4 (`account.export.text`) | SPEC_v1 SD14 |
| U79 | **Administration der Produktvorschläge** (`admin.*`, §5.18) und **rechtliche Seiten** (`legal.*`, Seitentitel); Screens S71 und S81 im Inventar | SPEC_v1 R-ADM-02 (Vorschlag O-S2), R-SHL-07 |
| U80 | **Offene Punkte:** O8 erledigt (18 Monate, SPEC_v1 SD7); O2 und O19 bleiben, die Texte gelten vorläufig (SD6, SD8); Verweise auf ARCHITECTURE_v11, TESTING_v9, Handoff 1.13 | SPEC_v1 §0 |

### Änderungen gegenüber v11

| # | Änderung | Anlass |
|---|---|---|
| U54 | **Zählzeile mit Bezeichnung vorn:** „Erfüllt: 22 · Leichte Einschränkung: 3 · …“ in S32 (`projects.detail.row.counts`) und in der Eintragszeile des Produktverzeichnisses (`directory.entry.row.*`, dazu „Damals nicht im Katalog: N“); immer alle Werte in fester Reihenfolge, Trenner nur per CSS, kein Umbruch innerhalb eines Werts | Prüfung C-F15 |
| U55 | **Umfang einer Liste ohne „Geprüft nach …“:** `projects.detail.criteria.scope.value` nennt die Kriterien „Geprüft nach …“ getrennt („– dazu 2 × „Geprüft nach …“ (zählen im Abgleich nicht mit)“); neuer Schlüssel `.scope.value.noProcedures` | Prüfung C-F1 |
| U56 | **Prüfnachweis-Karten** in S34a und S55 Schritt 2 einheitlich: H2 `candidate.evidence.list.title`, H3 je Nachweis, Felder Art der Prüfung, prüfende Stelle, Datum mit Hinweis, Geprüfte Version (optional), Link zum Prüfbericht (optional), in S34a die private Notiz; neue Schlüssel `candidate.evidence.version`, `vendor.step2.version`, `vendor.step2.rule`, `vendor.step2.intro`; `vendor.step2.add` lautet „Weiteren Prüfnachweis hinzufügen“; `vendor.step1.version` entfällt (Version gehört zum Nachweis) | Prüfung A-F17, C-F6, C-F7, C-F8, C-F9 |
| U57 | **S55 Schritt 1:** Reihenfolge Produktname → Treffer → Anbieter, Webseite → Verwendungszwecke → Anwendungstypen (Frage als `legend` in der Feldgruppe) → Hinweis; zweite Aktion „Abbrechen“ (`common.actions.cancel`); erledigte Schritte mit Häkchen, nur der aktuelle Schritt fett | Prüfung C-F2, C-F10–C-F13; Figma S55-1 |
| U58 | **S40-V:** ein Hinweis mit zwei Absätzen (`directory.results.useCaseNote`, dann `directory.results.orderNote`) statt zwei Hinweisen | Prüfung C-F21 |
| U59 | **S51:** neuer Text für `contribute.report.consent.withdraw`; `contribute.report.publicNote` entfällt in S51; sichtbare Fragen über den Feldern auch auf dem Desktop; neues optionales Feld `contribute.report.appType` nach der Version; `contribute.report.assessment.appType` entfällt | D5; Prüfung C-F18, C-F19 |
| U60 | **S32:** „Produkt hinzufügen“ mit Pfeil-Symbol und `aria-haspopup="menu"`; der Produktname ist der einzige Link zu S34 (`projects.detail.row.open` entfällt); bei veröffentlichten Produkten Link `projects.detail.lookup`; Prüfstatus „Ja – 2 Prüfnachweise: …“; S32-M mit Trennlinien und Abgleich als Liste | Prüfung C-F14, C-F16, C-F17 |
| U61 | **K2 und K4 mit mehreren Prüfnachweisen:** `signals.tested.evidence.item` (eine Zeile je Nachweis) ersetzt `signals.tested.details`; `signals.documentation.testType` nennt alle Arten der Prüfung | Prüfung A-F1 |
| U62 | **Eintrag der Anbieterseite ohne Einschätzungen:** `signals.fulfillment.none` statt K1 | D2 |
| U63 | **Eigene Kopie der Kriterienliste im Projekt:** Hinweise in S31 (`projects.create.criteria.hint`), S32 (`projects.detail.criteria.copyNote`) und S11 (`criteria.list.projectCopy`); `projects.detail.criteriaLocked` angepasst; ohne Kriterienliste kein Veröffentlichen (`publish.blocked.noCriteria`) | D1, D2 |
| U64 | **Aktualisieren nur unter dem eigenen Beitragsprofil:** `candidate.published.otherProfile`; andere Mitglieder sehen „Veröffentlichung zurückziehen“ | D3 |
| U65 | **Widerruf löscht persönliche Inhalte:** `contribute.mine.withdraw.reportNote` und `account.delete.text` nennen die Löschung von Text und Angaben zu Hilfsmitteln | D4 |
| U66 | **„Neue Liste mit derselben Auswahl“ funktioniert** (S11 bei älterer Katalogversion): Ergebnis- und Hinweistexte `criteria.list.copy.*` | D6 |
| U67 | **Geteilte Kriterienliste ohne Konto lesbar** (S16); keine Freigabelinks für Projekte und für Listen in Projekten (`criteria.share.intro`, `criteria.shared.note`, `criteria.list.share`) | D7 |
| U68 | **Sitzung:** Anmeldung nach Ablauf im Overlay ohne Seitenwechsel, danach wird die Aktion wiederholt (`shell.session.expired.*`, neu `shell.session.restored`); `shell.session.warning.body` sagt nicht mehr, dass Eingaben verloren gehen | ARCHITECTURE_v10 C60 |
| U69 | **Korrekturen:** Glossar „Geprüft nach …“ (abgeleitet: Ja / Nein / Nicht angegeben); E4 und E10 als erledigt bzw. abgelöst gekennzeichnet; `publish.error.testStatus` auf Feld `testStatus`; `publish.submit` mit `clientRequestId` statt Idempotency-Key; `vendor.republish.intro` mit `PublishedEntryVersion`; S34b mit einer Zeile je Prüfverfahren und Textbutton `assessment.procedure.derived.edit`; `directory.entry.context.contributorDeclared`; englischer Begriff für „Nicht angegeben“ | Prüfung A-F4, A-F5, A-F16, A-F21, C-F4, C-F20 |
| U70 | **Offene Punkte:** O7 (D6) und O17 (Figma S55-1 `2145:3653`, S55-2 `2145:4312`) erledigt; O2, O8, O19 mit ARCHITECTURE_v10 §16.5 O-14 bis O-16 verknüpft; in anderen Dokumenten werden die Punkte als UX-O1 … UX-O22 zitiert. Verweise auf PRD v5, ARCHITECTURE_v10, TESTING_v8, Handoff 1.12, Mapping 1.10 und Katalog 1.5 | Prüfung A-F14, A-F15, A-F18 |

### Änderungen gegenüber v10

| # | Änderung | Anlass |
|---|---|---|
| U52 | **S32 Projektübersicht gestaltet** (Figma S32, S32-M): Kopf mit Stand, Karte „Kriterienliste“ mit Beschreibungsliste und „Kriterienliste anpassen“, Karte „Produkte in diesem Projekt“ mit Menü „Produkt hinzufügen“ und einer Zeile je Produkt (Prüfstatus, Fortschritt, Abgleich, Veröffentlichung) – `projects.detail.*` | Rückmeldung 13.10. |
| U53 | **Sitzung:** 30 Minuten ohne Aktivität, höchstens 8 Stunden; Warnung 5 Minuten vorher; eigener Text vor dem Ende der Höchstdauer (`shell.session.warning.absolute`) | ARCHITECTURE_v9 C52 |

### Änderungen gegenüber v9

| # | Änderung | Anlass |
|---|---|---|
| U49 | **Kriterienliste anpassen statt wechseln:** `projects.detail.criteriaLocked` erklärt, dass die zugeordnete Liste weiter angepasst werden kann; neuer Hinweis nach dem Aufnehmen von Kriterien (`projects.detail.criteriaAdded`) | ARCHITECTURE_v8 C51 |
| U50 | **Sitzung:** Texte zur Sitzungswarnung bleiben; Hinweis, dass „Angemeldet bleiben“ die Sitzung über `POST /auth/session/extend` verlängert | ARCHITECTURE_v8 C50 |
| U51 | Verweise auf ARCHITECTURE_v8, TESTING_v6, PRD v4 und Handoff 1.10 aktualisiert (außer in den Änderungstabellen, die den damaligen Stand dokumentieren) | Rückmeldung 13.10. |

### Änderungen gegenüber v8

| # | Änderung | Anlass |
|---|---|---|
| U45 | **Mehrere Prüfnachweise je Produkt (S34a, S55 Schritt 2):** Liste von Nachweisen mit „Weiteren Prüfnachweis hinzufügen“, „Entfernen“, Fehler bei doppeltem Prüfverfahren, Rückfrage beim Wechsel von „Ja“ auf „Nein“/„Nicht bekannt“ (`candidate.evidence.list.*`, `vendor.step2.add`) | ARCHITECTURE_v7 C46 (Entscheidung E-R6) |
| U46 | **„Geprüft nach …“ wird nicht mehr eingeschätzt, sondern aus dem Prüfnachweis übernommen:** Anzeige statt Auswahl in S34b (`assessment.procedure.derived.*`); `assessment.procedure.legend/yes/no/unknown/hint` entfallen; O22 erledigt | C46 |
| U47 | **Einwilligung für jeden Erfahrungsbericht** (nicht nur bei Hilfsmitteln); Rückzug = Widerruf (`contribute.report.consent*`, `contribute.mine.withdraw.reportNote`) | C48 (Entscheidung E-Y3) |
| U48 | **Kriterienliste gesperrt, sobald Einschätzungen vorliegen** (`projects.detail.criteriaLocked`) | C47 (Entscheidung E-Y1) |

### Änderungen gegenüber v7

| # | Änderung | Anlass |
|---|---|---|
| U34 | **„Geprüft nach …“ als eigenes Kriterium:** Priorität B, abwählbar, im Export; in K6 mit Ja / Nein / Nicht eingeschätzt (`assessment.procedure.*`); zählt im Abgleich (K1) nicht mit (`criteria.item.testedBy.note`) | Konsistenzprüfung E-A |
| U35 | **Erlaubte Stufen bei mehreren Anwendungstypen:** K6 zeigt alle Stufen, die für mindestens einen gewählten Anwendungstyp erlaubt sind (Vereinigung); Hinweis `candidate.assessment.unionNote` | E-B |
| U36 | **Anwendungstyp und Verwendungszweck am Eintrag (Variante A):** S55 Schritt 1 fragt beides ab (`vendor.step1.appTypes`, `.useCases`), Kennzeichnung „vom Anbieter angegeben“ (`directory.entry.context.*`), Hinweis in S40 bei gewähltem Verwendungszweck (`directory.results.useCaseNote`), Zeile `directory.result.useCaseLine` | E-C, Entscheidung 12.10. |
| U37 | **Erfahrungsberichte ohne Zählwerte:** `signals.fulfillment.reportOnly` entfällt; die Berichtskarte zeigt auf Wunsch die Liste der eingeschätzten Kriterien (`directory.product.reports.assessment`, `.assessment.list.*`) | E-D |
| U38 | **K1 im Projekt mit sieben Zeilen** (ohne „Damals nicht im Katalog“) | E-E |
| U39 | **Zurücksetzen:** ein Text je Schicht statt `{{layer}}` (`criteria.select.reset.text.*`; Grammatik) | M5 |
| U40 | **Leerzustand für Anwendungstypen ohne Kriterien** (Hardware, Vermittlungs- oder Notrufdienst): `criteria.list.noCriteriaFor` statt Fehlertext | M3 |
| U41 | Fehlende Schlüsselzeilen ergänzt: `directory.filter.appType.all`, `directory.filter.testType.all`, `directory.filter.toggle.none`, `testType.other.hint` | Kosmetik |
| U42 | S51: Anwendungstyp nur, wenn einzelne Kriterien eingeschätzt werden (`contribute.report.assessment.appType`) | ARCHITECTURE_v6 C32 |
| U43 | Glossar bereinigt (`UseCaseTemplate` = Daten „Produkt-Typen“, doppelte Zeile `ProductPart` entfernt, neue Zeilen für Prüfverfahrens-Kriterium und Herkunft des Kontexts); §4.2 ohne Code Connect; §7 englische Begriffe nach der Skala von BIT inklusiv; E13-Zeile aktualisiert; Verweise auf ARCHITECTURE_v6 / TESTING_v4 | Konsistenzprüfung W10, W13, Kosmetik |
| U44 | Offene Punkte: O17 (S55 Schritt 2 in Figma) auf Migrationsschritt 6 vorgezogen; neuer Punkt O22 (Verhältnis „Geprüft nach …“ zum Prüfnachweis) | L10; ARCHITECTURE_v6 §16.5 O-9 |

### Änderungen gegenüber v6

| # | Änderung | Anlass |
|---|---|---|
| U28 | **S11:** Zusammenfassung als Beschreibungsliste (`criteria.list.summary.*`); „Warum diese Priorität?“ ist der erste Abschnitt der Details (`criteria.item.detail.priority`), K5 zeigt in der Liste keinen eigenen Begründungs-Button | design_critique_procureaccess_v2 U28, U32 |
| U29 | **S34 Überblick:** `candidate.overview.status.unknownHint` entfällt (der Hinweis im Kopf genügt) | design_critique_procureaccess_v2 U33 |
| U30 | **Mehrfachauswahl kennzeichnen** (`common.multiSelect.hint`) in S40 | design_critique_procureaccess_v2 U34 |
| U31 | **Mobile-first (unter 600 px):** Kurzlabel für den Tab „Einschätzung“, kompakte Einschätzung mit Auswahlfeld (`candidate.assessment.select`, `.note.add/.edit`), Filter-Button in S40 (`directory.filter.toggle`), Kurzlabels in S51 mit der Frage als sichtbarem Label darüber (`contribute.report.*.short`), Textzeile statt Stepper (`criteria.select.stepLabel`) | Mobile-first-Screens, Handoff §2.2 |
| U32 | **Links:** „Verfahren ansehen ({{domain}})“ in S10 Schritt 4 (`testType.link`), unsichtbarer Hinweis für Links in neuem Tab (`common.newTab`), Link „Was sagt ein Konformitätsbericht aus?“ in K2 (`signals.documentation.explain.toggle`; bisher ohne Schlüssel) | accessibility_review_procureaccess_v2 B7, B8 |
| U33 | **S10-E:** Fehlerzustand von Schritt 1 in Figma; Text unverändert (`criteria.select.error.noUseCase`) | accessibility_review_procureaccess_v2 B9 |

### Änderungen gegenüber v5

| # | Änderung | Anlass |
|---|---|---|
| U24 | **Erklärung der Bewertungsstufen** (`assessment.legend.*`, `assessment.value.*.explain`), Komponente `pa-rating-legend` in S34b, S42, S11 (Details) und im Überblick | Rückmeldung 06.10.: Unterschiede Einschränkung/Barriere/Blockade/Nicht anwendbar nicht intuitiv |
| U25 | **S34 Überblick** (Einstiegs-Tab eines Produkts im Projekt): Produktangaben, Stand der Bewertung, Veröffentlichung, Bereich „Nur für Ihre Organisation“ (`candidate.overview.*`) | Screen nachgereicht |
| U26 | **S40:** Filter in der Reihenfolge der Schichten; Verwendungszweck als durchsuchbares Feld (`directory.filter.useCase.*`) | Rückmeldung 06.10. |
| U27 | E13 in ARCHITECTURE und TESTING übernommen (O14 erledigt); Ausschluss HR → Mobile Anwendung entfällt (O15 erledigt); Hardware/Vermittlungsdienst ohne Prüfverfahren als offener Punkt O18 | Entscheidungen 06.10. |

### Änderungen gegenüber v4

| # | Änderung | Anlass |
|---|---|---|
| U17 | **„Anwendungstyp“ statt „Softwaretyp“** überall. Werte aus dem Katalog: Website oder Webanwendung, Software (offen), Software (geschlossen), Mobile Anwendung (offen), Mobile Anwendung (geschlossen), Dokument, Hardware, Vermittlungs- oder Notrufdienst. Anzeige ohne Schrägstrich. Hinweise zu offen/geschlossen. Neutrale Wörter („Produkt“ statt „Software“), der Claim bleibt | Review-Punkt 1 |
| U18 | **S10 mit vier Schritten in neuer Reihenfolge:** 1 Verwendungszweck (durchsuchbare Einfachauswahl mit „Keiner davon passt“), 2 Anwendungstyp, 3 Funktionsbereiche, 4 Prüfverfahren (optional). Schalter „Erklärungen anzeigen“ mit den Erklärungen aus dem Standardanforderungskatalog. Basisfunktionalität immer ausgewählt | Review-Punkte 2–4 |
| U19 | **Ausschlussregeln und Zurücksetzen:** Gründe für ausgeschlossene Optionen und Hinweis nach dem Zurücksetzen (`criteria.select.excluded.*`, `criteria.select.reset.*`) | Review-Punkt 5 |
| U20 | **Prüfverfahren:** Liste für „Art der Prüfung“ (S34a, S55 Schritt 2, Filter S40, K2, K4) inklusive „Andere Prüfung“ und „Eigene Prüfung ohne festes Verfahren“; Kriterium „Geprüft nach …“ (`testType.*`, `criteria.item.testedBy.*`) | Review-Punkt 4 |
| U21 | **Bewertungsskala von BIT inklusiv** ersetzt die Einschätzungswerte (`assessment.value.*`, K1, Eintragszeile, S42). K1 zeigt acht Zählwerte | Review-Punkt 6, Kriterienkatalog §5 |
| U22 | **Kriterium kompakt und ausführlich** (S11): Originaltitel, Metazeile, „Details anzeigen“, Abschnitte der Detailansicht (`criteria.item.detail.*`) | Review-Punkt 6, Kriterienkatalog §6 |
| U23 | Produktverzeichnis S40: Filter „Anwendungstyp“ und „Prüfverfahren“, Einleitung mit „Produkten“ | Review-Punkte 1 und 4 |

### Änderungen gegenüber v3

| # | Änderung | Anlass |
|---|---|---|
| U12 | **Erfahrungsberichte nur noch unter „Erfahrungsberichte“** auf der Produktseite, nicht zusätzlich als Eintragszeile. Statt des Prüfstatus steht die Beitragsart (`contribute.report.typeNote`). Neue Zählung der Einträge mit Berichten | E13 |
| U13 | **S40 Produktsuche:** Hinweis zur Reihenfolge, Texte der Ergebniszeile (`directory.results.orderNote`, `directory.result.*`) | Design nachgereicht |
| U14 | **S35 Schritt 2 „Inhalt prüfen“:** Button zum Bearbeiten der Einschätzungen, Rückweg, unsichtbarer Zusatz „erledigt“ im Stepper | Design nachgereicht |
| U15 | **S42 Eintrag-Detail:** Titel, Tabelle „Einschätzung je Kriterium“ mit Filter, Link auf frühere Fassungen (`directory.entry.*`) | Design nachgereicht |
| U16 | **S53 und S37:** Statuszeile und geteilte Texte für Beiträge (`contribute.mine.*`), Ansage nach dem Kopieren im Konfliktdialog (`conflict.copied`) | Design nachgereicht |

### Änderungen gegenüber v2

| # | Änderung | Anlass |
|---|---|---|
| U6 | **Texte aus dem Design-Review:** Eintragszeile im Produktverzeichnis (`directory.entry.row.*`), Statuszeile der Aktionsleiste in S11 (`criteria.list.status.count`), Hinweise in S34 (`candidate.publish.cta.hint`, `candidate.evidence.details.hint`), Titel und nächste Schritte im gesperrten Veröffentlichen (`publish.blocked.*`), Titel für Hinweise | design_critique_procureaccess_v1 §8 |
| U7 | **Fehlerübersicht erhält den Fokus** (vorher: erstes fehlerhaftes Feld). Neue Schlüssel für Zeilen der Übersicht; Feldfehler und Zeile der Übersicht lauten gleich. Konkrete Fehlertexte für S51, S55 und den Export | E11, E12; accessibility_review_procureaccess_v1 V1 |
| U8 | **Seitentitel** pro Screen (§5.16) und Präfix „Fehler:“ nach fehlgeschlagenem Absenden | accessibility_review O4 |
| U9 | **Ansagen für Screenreader** ergänzt: Treffer bei Suche und Filter, Prioritätsänderung, Wechsel von Vergleichsliste und Sortierung, Fortschritt in S34b | accessibility_review R1 |
| U10 | **Kleinere Ergänzungen:** unsichtbare Präfixe für Hinweise, Satz zu Pflichtfeldern, Formathinweis und Kalender-Button für Datumsfelder, Button „Menü“ für schmale Bildschirme, Filter „Nur noch nicht eingeschätzte anzeigen“ (S34b) | accessibility_review P2, V2, V3, P3, O7 |
| U11 | Komponenten-Inventar um `pa-notice`, `pa-entry-row` und `pa-app-header-compact` ergänzt | 7e, 7f |

Schlüsselvorschläge aus den beiden Review-Dokumenten wurden, wo es schon passende Schlüssel gab, auf diese abgebildet (z. B. Sprunglink → `shell.skipLink`, „Erneut versuchen“ → `common.actions.retry`, „Noch nicht gespeichert“ → `criteria.list.unsaved`).

### Änderungen gegenüber v1

| # | Änderung | Anlass |
|---|---|---|
| U1 | **Export ohne Konto.** Kriterien zusammenstellen und exportieren geht ohne Anmeldung. Speichern, Teilen und Projekte brauchen weiterhin Konto und Organisation. Neue Texte für den Hinweis „Auswahl wird nicht gespeichert“ und für die Übernahme der Auswahl nach der Anmeldung. Betrifft S11, S12, S13, S01 | E7 (O1 entschieden), ARCHITECTURE_v4 D15 |
| U2 | **Prüfstatus „Nicht bekannt“** im Beschaffungsprojekt als dritte Option und Vorauswahl. Veröffentlichen ist erst möglich, wenn Ja oder Nein gewählt ist. Neue Fehlermeldung. Betrifft S34, S35, Glossar | E8 (O6 entschieden), ARCHITECTURE_v5 §5.4 |
| U3 | **Veröffentlichung aus dem Projekt immer als „Unabhängige Angaben“** ist jetzt bestätigt (vorher Annahme A1) | E9, ARCHITECTURE_v4 D16 |
| U4 | **Zurückziehen und Aktualisieren unter „Meine Beiträge“** für Erfahrungsberichte und Prüfergebnisse, da diese kein Projekt haben. Neuer Text für die erneute Veröffentlichung eines Prüfergebnisses mit neuer Zustimmung. Betrifft S53, S55 | ARCHITECTURE_v5 §4.6, §7.2 |
| U5 | Verweise auf Architektur und Tests auf v4 bzw. TESTING_v2 umgestellt; offene Punkte O1, O3, O6 geschlossen, O5 und O7 auf die neuen Architektur-Abschnitte verwiesen | – |

---

## 0. Entscheidungen (Sitzung 01.10.2026)

| # | Entscheidung | Folge für dieses Dokument | Folge für andere Dokumente |
|---|---|---|---|
| E1 | ARCHITECTURE §16.1 → **Option A**: sechs Zählwerte pro Eintrag (seit v5: acht, Bewertungsskala von BIT inklusiv) | Komponente K1 zeigt alle Kategorien als Zahlen, ohne Summe und ohne Prozent | Umgesetzt: ARCHITECTURE_v11 §16.4.1, TESTING_v9 SIG-07 |
| E2 | ARCHITECTURE §16.3.1 → **Option C**: „Vorschlagen“ von Kriterien über einen externen Kanal (Kontakt zu WERTE.IT) | Link „Kriterium vorschlagen“ in Kriterienliste und Fußzeile (S70) | Umgesetzt: ARCHITECTURE_v11 §16.4.2. Offen: PRD-Akzeptanzkriterium „Vorschlagen“ in SPEC_v1 präzisieren |
| E3 | ARCHITECTURE §16.3.2 → **Option B**: eigener direkter Weg für Kanal 1 („Prüfergebnis veröffentlichen“) ohne Organisation und Projekt. Der Schritt zur Produktidentität wird aus Flow B übernommen (suchen, Treffer bestätigen, sonst neu anlegen) | Neuer Ablauf S55 | Umgesetzt: ARCHITECTURE_v11 §4.6 (Flow E), D5, D13, ADR-008; TESTING_v9 PUB-08–PUB-10, J7. FigJam-Board 17 |
| E4 | Die **Echtheit von Anbieterangaben** wird in die gebündelte rechtliche Prüfung (PRD §11) aufgenommen: Weder Option A noch B prüft, ob jemand tatsächlich Anbieter oder Prüfstelle ist oder ob die Zustimmung des Auftraggebers wirklich vorliegt. `PublicationConsent` ist eine Selbstauskunft | Texte in S55 sind als vorläufig markiert (⚖️) | Umgesetzt: ARCHITECTURE_v11 §16.2.7; PRD §11, gebündelte rechtliche Prüfung Punkt (5) (erledigt) |
| E5 | Anrede **„Sie“** | durchgängig | – |
| E6 | **Neutrale Formen** statt Genderzeichen (Screenreader lesen Sternchen und Doppelpunkt teils vor) | siehe Grundsatz G3 | Gilt sinngemäß auch für Nutzerhandbuch |
| E7 | **Kriterienliste ohne Konto herunterladen** (O1) | U1: S11, S13 ohne Anmeldung; Hinweis, dass die Auswahl ohne Konto nicht gespeichert wird | ARCHITECTURE_v11 D15, ADR-009; TESTING_v9 EXP-05–EXP-07 |
| E8 | **Prüfstatus „Nicht bekannt“** im Beschaffungsprojekt (O6) | U2: S34, S35 | ARCHITECTURE_v11 §5.4 (`TestStatus`); TESTING_v9 SIG-03, SIG-10 |
| E9 | **Veröffentlichung aus dem Projekt immer als unabhängige Angabe** (A1 bestätigt) | U3: S35 ohne Kanalauswahl | ARCHITECTURE_v11 D16; TESTING_v9 PUB-09 |
| E10 | ~~**Einwilligung bei Angaben zu Hilfsmitteln:** Platzhaltertext in S51 bleibt vorerst~~ **Abgelöst durch U47:** Einwilligung für jeden Bericht; beim Widerruf werden Text und Angaben zu Hilfsmitteln gelöscht (U65) | – | ARCHITECTURE_v11 §16.2.8, C56 |
| E11 | **Nach dem Absenden erhält die Fehlerübersicht den Fokus**, nicht das erste fehlerhafte Feld (Sitzung 02.10.2026) | §6.1 angepasst | accessibility_review_procureaccess_v1 §7; Figma S51-E, S55-E |
| E12 | **Feldfehler und Zeile in der Fehlerübersicht lauten gleich** („{{Feldname}}: {{Feldfehler}}“), damit Gesehenes und Gehörtes übereinstimmt | §6.1 | – |
| E13 | **Erfahrungsberichte erscheinen auf der Produktseite nur unter „Erfahrungsberichte“** (nicht zusätzlich als Eintragszeile). Statt „Auf Barrierefreiheit geprüft: Ja/Nein“ zeigen sie die Beitragsart, weil ein Bericht keine Prüfung behauptet (Sitzung 02.10.2026) | U12; S41, S41-M | Umgesetzt: ARCHITECTURE_v11 C28 (seit v6 zusätzlich ohne Zählwerte, C33); TESTING_v9 SIG-12, TESTING_v9 SIG-14 |

**Zu E9 (vorher Annahme A1):** Da Kanal 1 einen eigenen Weg hat, braucht die Veröffentlichung aus dem Beschaffungsprojekt keine Kanalauswahl. Beschaffende Organisationen veröffentlichen immer als **unabhängige Quelle** (Kanal 2). Das vereinfacht den Veröffentlichungsdialog und verhindert, dass eine beschaffende Stelle versehentlich als „Anbieterseite“ veröffentlicht.

---

## 1. Grundsätze für alle Texte

| # | Grundsatz | Beispiel |
|---|---|---|
| G1 | **Einfache, klare Sprache.** Kurze Sätze, ein Gedanke pro Satz, aktive Formulierungen. Fachbegriffe nur, wenn die Zielgruppe sie aus dem Vergabealltag kennt (z. B. „Vergabeunterlagen“). Normbezeichnungen (EN 301 549, BITV) nur als Zusatzinformation, nie als Voraussetzung zum Verstehen | „Wählen Sie, welche Art von Software Sie beschaffen möchten.“ statt „Selektieren Sie den App-Typ gemäß Standardanforderungskatalog.“ |
| G2 | **Anrede „Sie“**, freundlich-sachlich, ohne Ausrufezeichen und ohne Werbesprache | „Liste gespeichert.“ statt „Super, Ihre Liste ist gespeichert!“ |
| G3 | **Neutrale Formen.** Personen möglichst nicht benennen, sondern direkt ansprechen oder über Rolle/Organisation formulieren: „Mitglieder“, „Nutzende“, „Prüfstelle“, „Anbieterseite“, „Auftraggebende Stelle“. Kein Genderstern, kein Doppelpunkt, kein Binnen-I | „Mitglieder Ihrer Organisation“ statt „Kolleg*innen“ |
| G4 | **Schaltflächen beginnen mit einem Verb** und nennen das Ergebnis | „Liste exportieren“, „Eintrag veröffentlichen“ |
| G5 | **Fehlermeldungen:** Was ist passiert – warum – was können Sie tun. Nie nur Farbe oder Symbol | siehe §6 |
| G6 | **Hörbar verständlich.** Texte werden auch mit Screenreader gehört. Keine Zeichen, die als Wort vorgelesen werden und stören (z. B. „/“, „&“, „→“, „*“). Abkürzungen ausschreiben oder beim ersten Vorkommen erklären. Zahlen immer mit Bezugswort („12 Kriterien“, nicht nur „12“) | „Anbieter oder Prüfstelle“ statt „Anbieter/Prüfstelle“ |
| G7 | **Keine Bewertungssprache für Produkte.** Das Tool sagt nie, ob ein Produkt „barrierefrei“, „gut“ oder „empfehlenswert“ ist. Verboten in allen Texten zu Produkten und Einträgen: „Score“, „Bewertung von x Sternen“, „Gesamtergebnis“, „Prozent erfüllt“, „Ampel“, „Barrierefreiheitsgrad“, „Testsieger“, „empfohlen“ (für Produkte) | ARCHITECTURE §8.4 |
| G8 | **Empfehlung ist keine Norm.** Jede Priorität wird als fachliche Einschätzung gekennzeichnet | „Empfehlung des Projektteams – keine Vorgabe aus einer Norm“ |
| G9 | **Herkunft immer benennen.** Zu jedem Eintrag wird gesagt, wer ihn veröffentlicht hat (Anbieterseite oder unabhängige Quelle) – im Text, nicht nur über Farbe oder Symbol | K3 |
| G10 | **Konsistente Begriffe.** Es gelten ausschließlich die Begriffe aus §2. Interne Projektbegriffe („Kanal 1“, „Workspace“, „Konfiguration“, „Release“, „Kandidat“) erscheinen nie in der Oberfläche | – |
| G11 | **Platzhalter** in ngx-translate-Syntax: `{{name}}`. Zahlen und Daten werden im Code lokalisiert formatiert (z. B. `14.05.2026`), nicht im Text | – |

---

## 2. Begriffe (Glossar Oberfläche)

Verbindlich für alle Texte, Figma-Screens und das Nutzerhandbuch.

| Interner Begriff (Code / Architektur) | Begriff in der Oberfläche | Nicht verwenden |
|---|---|---|
| Kriterien-Konfigurator (Feature `configurator`) | **Kriterien zusammenstellen** (Navigation: „Kriterien“) | Konfigurator, Filter-Tool |
| `CriteriaConfiguration` | **Kriterienliste** | Konfiguration, Filterset |
| `CriteriaConfiguration` mit `ProcurementProjectId` (Kopie im Projekt, D1) | **Kriterienliste dieses Projekts** (Kopie der Liste „…“) | Projektkonfiguration, Klon |
| `AppType` (Daten: „App-Typen“) | **Anwendungstyp** | Softwaretyp, App-Typ, Art der IT-Lösung |
| `UseCaseTemplate` (im bestehenden Repository: `ProductType`, Daten „Produkt-Typen“) | **Verwendungszweck** | Produkttyp, Einsatzgebiet, Template, Anwendungstyp-Template |
| `ProductPart` (Daten: „Produkt-Teile“) | **Funktionsbereich** (z. B. Audio, Video) | Produktteil, Modul, Komponente |
| `PriorityRecommendation.Level` A / B / Info | **A – Mindestanforderung**, **B – Bewertungskriterium**, **Hinweis** (siehe Annahme A2) | Muss/Soll, Pflicht |
| `Source = ExpertJudgement` | **Empfehlung des Projektteams** | geprüft, normativ, verbindlich |
| `CatalogRelease` | **Katalogversion** | Release, Stand |
| `ShareLink` | **Freigabelink** | Share-Link, Token |
| Workspace / `ProcurementProject` | **Beschaffungsprojekt** (Navigation: „Projekte“) | Workspace, Arbeitsbereich, Beschaffungsvorgang |
| `CandidateProduct` | **Produkt** (im Projekt: „Produkte in diesem Projekt“) | Kandidat, Kandidatenprodukt |
| `CandidateAssessment` | **Einschätzung** je Kriterium | Bewertung (zu nah an „Produktbewertung“), Rating |
| `TestEvidence` | **Prüfnachweis** | Testbeleg, Evidenz |
| `TestStatus` (im Projekt) | **Auf Barrierefreiheit geprüft: Ja / Nein / Nicht bekannt** | getestet (allein), zertifiziert, offen |
| `WasTested` (veröffentlicht) | **Auf Barrierefreiheit geprüft: Ja / Nein** – „Nicht bekannt“ kommt im Verzeichnis nie vor | getestet (allein), zertifiziert |
| `Criterion.Kind = TestProcedure` | **„Geprüft nach {{Prüfverfahren}}“** (Kriterium; wird nicht eingeschätzt, sondern aus den Prüfnachweisen abgeleitet: Ja / Nein / Nicht angegeben) | Testkriterium, Zertifikat |
| `ContextSource = VendorDeclared` | **vom Anbieter angegeben** (bei Anwendungstyp und Verwendungszweck eines Eintrags) | Herstellerangabe, Eigenangabe |
| `ContextSource = ContributorDeclared` | **von der beitragenden Person angegeben** (nur Anwendungstyp eines Erfahrungsberichts, D5) | Nutzerangabe |
| `TestType` (Daten: „Test-Typen“) | **Prüfverfahren** (als Filter und in S10), **Art der Prüfung** (beim Nachweis einer durchgeführten Prüfung) | Testtyp, Testverfahren |
| Directory | **Produktverzeichnis** (Navigation: „Produkte“) | Datenbank, Marktübersicht, Katalog |
| `SoftwareProduct` | **Produkt** | – |
| `PublishedEntry` | **Eintrag** | Bewertung, Review |
| `Channel = VendorOrAuditor` (Kanal 1) | **Angaben der Anbieterseite** (Kurzform: „Anbieterseite“) | Kanal 1, Herstellerbewertung |
| `Channel = Independent` (Kanal 2) | **Unabhängige Angaben** (Kurzform: „Unabhängig“) | Kanal 2, Nutzerbewertung, Community |
| `ExperienceReport` | **Erfahrungsbericht** | Review, Betroffenenmeinung |
| `ContributorProfile` | **Beitragsprofil**, darin **Anzeigename** | Pseudonym-Account, Alias |
| `PublicationConsent` | **Zustimmung zur Veröffentlichung** | Freigabe (doppeldeutig mit Freigabelink) |
| Dokumentationsgrad | **Nachweise** (Überschrift: „Welche Nachweise gibt es?“) | Dokumentationsgrad |
| Erfüllungsgrad | **Abgleich mit Kriterien** (Überschrift: „Abgleich mit Ihrer Kriterienliste“) | Erfüllungsgrad, Score, Ergebnis |
| `Fulfillment` Fulfilled | **Erfüllt** | bestanden, unterstützt |
| SlightRestriction | **Leichte Einschränkung** | eher erfüllt |
| Restriction | **Einschränkung** | teilweise erfüllt, teilweise unterstützt |
| Barrier | **Barriere** | eher nicht erfüllt |
| Blockade | **Blockade** | nicht erfüllt, durchgefallen, nicht unterstützt |
| NotApplicable | **Nicht anwendbar** | nicht zutreffend, n/a |
| Unknown | **Nicht eingeschätzt** | unbekannt, offen, ? |
| `notCovered` / NotCoveredByThisRelease | **Damals nicht im Katalog** | fehlt, nicht erfüllt |
| `ConformanceReport` (VPAT/ACR) | **Konformitätsbericht (VPAT oder ACR)** | – |
| `ProductIdentityRequest` | **Produkt zur Aufnahme vorschlagen** | Identitätsanfrage |
| Withdraw | **Veröffentlichung zurückziehen** | löschen |
| `Organization` | **Organisation** | Mandant, Tenant, Team |
| Rollen Owner / Editor / Reader | **Verwalten / Bearbeiten / Lesen** | Owner, Admin |

**Annahme A2 – Prioritätsstufen:** „A – Mindestanforderung“ und „B – Bewertungskriterium“ lehnen sich an die übliche Vergabelogik an (A-Kriterien führen bei Nichterfüllung zum Ausschluss, B-Kriterien gehen gewichtet in die Wertung ein). Das sollte mit der Person mit Barrierefreiheits-Expertise im Projektteam abgestimmt werden, bevor die Begriffe in den Export übernommen werden.

---

## 3. Screen-Inventar

Priorität: P0 = Teil des MVP, P1 = später. „Anmeldung“: ✗ = ohne Konto nutzbar, ✓ = Konto nötig, ✓O = Konto und Organisation nötig.

| ID | Screen | Route (Vorschlag) | Ablauf | Persona | Anmeldung | Prio |
|---|---|---|---|---|---|---|
| S00 | App-Rahmen (Kopfbereich, Navigation, Fußzeile, Sprunglink, Ansagen, Sitzungswarnung) | – | alle | alle | – | P0 |
| S01 | Anmelden | `/anmelden` (Outlet `login`) | – | alle | ✗ | P0 |
| S02 | Registrieren | `/registrieren` | – | alle | ✗ | P0 |
| S03 | Passwort vergessen / zurücksetzen | `/passwort` | – | alle | ✗ | P0 |
| S04 | Startseite | `/` | Einstieg | alle | ✗ | P0 |
| S10 | Kriterien zusammenstellen – Auswahl (4 Schritte) | `/kriterien` | A | Beschaffende | ✗ | P0 |
| S11 | Kriterienliste – Ergebnis | `/kriterien/ergebnis` bzw. `/kriterienlisten/:id` | A | Beschaffende | ✗ lesen, anpassen und exportieren; ✓O speichern | P0 |
| S12 | Dialog: Kriterienliste speichern | – | A | Beschaffende | ✓O | P0 |
| S13 | Dialog: Kriterienliste exportieren | – | A | Ausschreibende | ✗ (E7) | P0 |
| S14 | Gespeicherte Kriterienlisten | `/kriterienlisten` | A | Beschaffende | ✓O | P0 |
| S15 | Dialog: Freigabelink erstellen | – | A | Beschaffende | ✓O | P0 |
| S16 | Geteilte Kriterienliste (Ansicht über Freigabelink) | `/geteilt/:token` | A | Empfangende | ✗ (D7) | P0 |
| S20 | Organisation anlegen oder beitreten | `/organisation/neu` | Workspace | Beschaffende | ✓ | P0 |
| S21 | Organisation: Mitglieder und Einladungen | `/organisation` | Workspace | Verwaltende | ✓O | P0 |
| S30 | Beschaffungsprojekte – Liste | `/projekte` | B | Beschaffende | ✓O | P0 |
| S31 | Dialog: Projekt anlegen | – | B | Beschaffende | ✓O | P0 |
| S32 | Projekt – Übersicht | `/projekte/:id` | B | Beschaffende | ✓O | P0 |
| S33 | Produkt im Projekt anlegen oder bearbeiten | `/projekte/:id/produkte/neu` | B | Beschaffende | ✓O | P0 |
| S34 | Produkt im Projekt – Überblick, Prüfnachweis und Einschätzungen (drei Tabs) | `/projekte/:id/produkte/:pid` | B | Beschaffende | ✓O | P0 |
| S35 | Eintrag veröffentlichen (aus dem Projekt) | `/projekte/:id/produkte/:pid/veroeffentlichen` | B | Beschaffende | ✓O | P0 |
| S36 | Dialog: Veröffentlichung zurückziehen | – | B | Beschaffende | ✓O | P0 |
| S37 | Dialog: Bearbeitungskonflikt | – | B | Beschaffende | ✓O | P0 |
| S40 | Produktverzeichnis – Suche | `/produkte` | C | alle | ✗ | P0 |
| S41 | Produktseite mit Einträgen | `/produkte/:id` | C | alle | ✗ öffentlich, ✓O Abgleich mit eigener Liste | P0 |
| S42 | Eintrag – Detail | `/eintraege/:id` | C | alle | ✗ | P0 |
| S50 | Beitragsprofil anlegen | `/beitragen/profil` | D, Kanal 1 | Beitragende, Anbieterseite | ✓ | P0 |
| S51 | Erfahrungsbericht schreiben | `/produkte/:id/erfahrungsbericht` | D | Beitragende | ✓ | P0 |
| S52 | Produkt zur Aufnahme vorschlagen | `/beitragen/produkt-vorschlagen` | D | Beitragende | ✓ | P0 |
| S53 | Meine Beiträge | `/beitragen` | D, Kanal 1 | Beitragende, Anbieterseite | ✓ | P0 |
| S55 | Prüfergebnis veröffentlichen (Anbieterseite) | `/beitragen/pruefergebnis` | Kanal 1 direkt (E3) | Anbieterseite | ✓ | P0 (Konformitätsbericht: P1) |
| S60 | Einstellungen | `/einstellungen` | – | alle | ✓ | P0 |
| S61 | Konto (Datenexport, Konto löschen) | `/konto` | – | alle | ✓ | P0 |
| S70 | Kriterium vorschlagen (Link nach außen, E2) | externer Link / `mailto:` | – | alle | ✗ | P0 |
| S80 | Fehlerseiten (nicht gefunden, kein Zugriff, Serverfehler) | – | – | alle | – | P0 |
| S71 | Rechtliche Seiten (Impressum, Datenschutz, Erklärung zur Barrierefreiheit) | `/impressum`, `/datenschutz`, `/barrierefreiheit` | – | alle | ✗ | P0 |
| S81 | Administration: Produktvorschläge und Zusammenführen (SPEC_v2 SD22) | `/verwaltung/produktvorschlaege` | D | PlatformAdmin | ✓ (Plattformrolle) | P0 |
| S90 | Eintrag melden, Stellungnahme | – | Moderation | alle, Anbieterseite | ✓ | **P1 – nicht in v1 dieses Dokuments** |

**Export ohne Konto (E7):** Kriterien zusammenstellen, anpassen und exportieren geht ohne Anmeldung. Die Auswahl liegt dann nur im Browser und geht beim Neuladen oder Schließen der Seite verloren; S11 sagt das ausdrücklich. Nach der Anmeldung bleibt die Auswahl erhalten und kann gespeichert werden (ARCHITECTURE_v11 §4.4, §8.1). Speichern, Freigabelinks und Beschaffungsprojekte brauchen weiterhin Konto und Organisation.

---

## 4. Komponenten-Inventar

### 4.1 Angular-Material-Komponenten (Standard, keine eigene Gestaltung)

Toolbar und Sidenav (S00), Stepper (S10, S35, S55), Card, List, Table mit Sortierung (S14, S30, S40), Chips (Filter), Checkbox, Radio Group, Select, Autocomplete (Produktsuche, Verwendungszweck in S10), Slide Toggle (Erklärungen in S10), Form Field mit Hint und Error, Dialog (alle Dialoge), Snackbar (nur für nicht kritische Bestätigungen, nie für Fehler), Expansion Panel (Begründung, Details), Tabs (S34), Progress Bar und Spinner (Ladezustände), Menu, Tooltip (nur Zusatzinformation, nie einzige Quelle einer Information), Paginator.

### 4.2 Fachliche Komponenten (neu, werden in Figma als Komponenten angelegt – Schritt 7c)

Diese sechs Komponenten tragen Produktregeln, die die Architektur nicht vollständig erzwingen kann (ARCHITECTURE §8.4). Code Connect steht im genutzten Figma-Tarif nicht zur Verfügung. Die Verbindung zum Angular-Code läuft über Dokumentationslinks an den Figma-Komponenten, `@figma`-Kommentare im Code und eine Prüfung in der CI (`figma_code_mapping_procureaccess_v1.md`).

| ID | Komponente | Zweck | Verbindliche Regeln | Material-Basis | Invarianten |
|---|---|---|---|---|---|
| **K1** | `pa-fulfillment-breakdown` – Abgleich mit Kriterien | Zeigt für einen Eintrag, wie viele Kriterien in welcher Kategorie eingeschätzt wurden | Immer **alle Kategorien** in fester Reihenfolge, auch wenn der Wert 0 ist: im Produktverzeichnis acht (sechs Stufen von BIT inklusiv, „Nicht eingeschätzt“, „Damals nicht im Katalog“), im Beschaffungsprojekt sieben (ohne „Damals nicht im Katalog“, weil Liste und Einschätzung dieselbe Katalogversion nutzen). Kriterien „Geprüft nach …“ werden nicht mitgezählt; Erfahrungsberichte haben kein K1. Hat ein Eintrag der Anbieterseite keine Einschätzungen einzelner Kriterien, steht statt K1 der Hinweis `signals.fulfillment.none` (D2). **Keine Summe, kein Prozentwert, kein Balken, keine Sterne, keine Ampel, keine Farbe als einziger Bedeutungsträger.** „Damals nicht im Katalog“ visuell und sprachlich von den Mängeln getrennt (eigene Zeile mit Erklärung). Darstellung als Beschreibungsliste (`<dl>`), nicht als Diagramm. Eigene Region mit eigenem zugänglichen Namen | keine (eigene Liste in Card) | SIG-07, SIG-08, SIG-09, CAT-04 |
| **K2** | `pa-documentation-signal` – Nachweise | Zeigt, welche Nachweise zu einem Eintrag vorliegen (Konformitätsbericht, Format, Datum, Alter, Arten der Prüfung aller Prüfnachweise) | **Nie im selben Container wie K1.** Eigene Region mit eigenem zugänglichen Namen. Keine Wertung („gut dokumentiert“), nur Fakten | Card, List | SIG-04, SIG-09 |
| **K3** | `pa-source-label` – Herkunft | Kennzeichnet, ob ein Eintrag von der Anbieterseite oder aus unabhängiger Quelle stammt, und wer ihn veröffentlicht hat (Anzeigename) | Text immer sichtbar, Symbol nur ergänzend. Beide Kanäle gleich groß und gleich gewichtet dargestellt (keine Hervorhebung einer Quelle als „vertrauenswürdiger“). Erklärung über Expansion Panel oder Info-Schaltfläche, nicht nur Tooltip | Text (Title Small, nicht interaktiv – seit 7e kein Chip) + Button | PRD §7 |
| **K4** | `pa-tested-status` – Geprüft ja/nein | Zeigt „Auf Barrierefreiheit geprüft: Ja/Nein“, bei Ja mit einer Zeile je Prüfnachweis (Art der Prüfung, prüfende Stelle, Datum) | **Immer sichtbar bei Prüfergebnis-Einträgen, in beiden Kanälen** (nie bei Erfahrungsberichten, E13), auch bei „Nein“. Nie ausblenden, nie einklappen. Im Verzeichnis gibt es nur Ja oder Nein; „Nicht bekannt“ ist ein Arbeitsstand im Projekt und wird dort als Eingabe (S34) dargestellt, nicht mit K4 | List Item | SIG-03, SIG-11, F8 |
| **K5** | `pa-priority-indicator` – Priorität mit Begründung | Zeigt die empfohlene Priorität eines Kriteriums und auf Abruf die Begründung, die angewandte Regel und die Quelle | Begründung immer erreichbar (Expansion Panel, keine reine Hover-Information). Kennzeichnung „Empfehlung des Projektteams“ ist Teil der Komponente, nicht optional. Bei eigener Änderung durch Nutzende wird die ursprüngliche Empfehlung weiter angezeigt | Chip + Expansion Panel | CAT-02, CAT-08 |
| **K6** | `pa-assessment-input` – Einschätzung je Kriterium | Eingabe der Einschätzung eines Produkts zu einem Kriterium im Projekt und im Ablauf S55 | Radio Group mit bis zu sieben Optionen in fester Reihenfolge (Stufen von BIT inklusiv, die für mindestens einen der gewählten Anwendungstypen erlaubt sind, plus „Nicht eingeschätzt“), **Vorauswahl „Nicht eingeschätzt“**. Kriterien „Geprüft nach …“ haben kein K6; sie zeigen Ja / Nein / Nicht angegeben aus dem Prüfnachweis (U46). Optionales Notizfeld „Worauf stützt sich Ihre Einschätzung?“. Keine Schieberegler, keine Sterne | Radio Group, Form Field | `Unknown` als Standard (ARCH §5.4) |

**Ergänzt nach 7e und 7f** (tragen keine Produktregel allein, sichern aber Einheitlichkeit und Barrierefreiheit):

| Komponente | Zweck | Regeln für Texte |
|---|---|---|
| `pa-notice` | Hinweis mit fester Bedeutung: Info, Achtung, Fehler | Bedeutung nie nur über Farbe: Symbol plus unsichtbares Präfix (`common.notice.*`). Bei „Fehler“ ist ein Titel Pflicht |
| `pa-entry-row` | Kompakte Zeile je Eintrag in S41, aufklappbar zur Eintragskarte | Kurzformen der acht Zählungen (`directory.entry.row.*`, Bezeichnung vorn), immer alle acht; ohne Einschätzungen `signals.fulfillment.none`; Layout „Narrow“ unter 840 px |
| `pa-app-header-compact` | Kopfzeile unter 600 px Breite und bei 400 % Zoom | sichtbarer Text „Menü“ statt nur Symbol (`shell.nav.menu`) |
| `pa-product-result` | Ergebnis in der Produktsuche (S40) | nur Fakten (`directory.result.*`), nie eine Bewertung |
| `pa-contribution-item` | Beitrag in „Meine Beiträge“ (S53) | Status als Text (`contribute.mine.status`) |

Zusammengesetzt (keine eigene Figma-Komponente, aber Vorlage in Figma): **Eintragskarte** = K3 + K4 + K2 + K1 (getrennte Regionen) + Veröffentlichungsdatum + Katalogversion + Link „Eintrag ansehen“.

---

## 5. Texte je Screen

Spalten: **Schlüssel** (ngx-translate, Datei `src/assets/i18n/de.json`), **Element**, **Text**, **Hinweis**. Gemeinsam genutzte Texte stehen unter `common.` (§5.0) und werden nicht pro Screen wiederholt.

### 5.0 Gemeinsame Texte (`common.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `common.actions.save` | Button | Speichern | |
| `common.actions.cancel` | Button | Abbrechen | |
| `common.actions.back` | Button | Zurück | |
| `common.actions.next` | Button | Weiter | |
| `common.actions.close` | Button | Schließen | |
| `common.actions.edit` | Button | Bearbeiten | |
| `common.actions.showMore` | Button | Mehr anzeigen | |
| `common.actions.showLess` | Button | Weniger anzeigen | |
| `common.actions.retry` | Button | Erneut versuchen | |
| `common.optional` | Feldzusatz | (optional) | Pflichtfelder werden nicht markiert, optionale schon – weniger Sternchen, besser hörbar |
| `common.loading` | Ladeanzeige (Ansage) | Wird geladen … | per Live-Region, höchstens einmal |
| `common.loaded` | Ansage | {{count}} Ergebnisse geladen. | |
| `common.date.published` | Label | Veröffentlicht am {{date}} | |
| `common.catalogRelease` | Label | Katalogversion {{release}} | |
| `common.yes` / `common.no` | Wert | Ja / Nein | |
| `common.notice.info` | Unsichtbares Präfix in `pa-notice` | Hinweis: | nur für Screenreader (U10) |
| `common.notice.attention` | Unsichtbares Präfix | Achtung: | |
| `common.notice.error` | Unsichtbares Präfix | Fehler: | |
| `common.requiredNote` | Text über Formularen mit Pflichtfeldern | Alle Felder ohne „(optional)“ müssen ausgefüllt werden. | ergänzt `common.optional`; Felder zusätzlich mit `required` |
| `common.date.hint` | Hint unter Datumsfeldern | Format: TT.MM.JJJJ, zum Beispiel {{example}} | Beispiel = heutiges Datum, lokalisiert |
| `common.date.openCalendar` | Name des Kalender-Buttons | Kalender öffnen | `mat-datepicker-toggle`; Texteingabe bleibt möglich |
| `common.results.shown` | Ansage nach Suche oder Filter | {{count}} Kriterien angezeigt. | Live-Region, polite, gedrosselt |
| `common.multiSelect.hint` | Supporting text an Mehrfachauswahl-Feldern | Mehrfachauswahl möglich | U30; S40 Anwendungstyp und Prüfverfahren; unter 600 px stattdessen im Filterbereich `directory.filter.hint.mobile` |
| `common.newTab` | Unsichtbarer Zusatz an Links, die in einem neuen Tab öffnen | (öffnet in neuem Tab) | U32; als `cdk-visually-hidden`-Text am Linkende, nicht als `title` |
| `common.stepper.done` | Unsichtbarer Zusatz an erledigten Schritten | erledigt | U14; Häkchen-Symbol ist `aria-hidden` |

### 5.1 S00 App-Rahmen (`shell.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `shell.skipLink` | Sprunglink | Zum Inhalt springen | erstes fokussierbares Element |
| `shell.appName` | Logo-Text | ProcureAccess | |
| `shell.appClaim` | Untertitel | Barrierefreie Software beschaffen | |
| `shell.nav.label` | `aria-label` Hauptnavigation | Hauptnavigation | |
| `shell.nav.menu` | Button (schmale Kopfzeile) | Menü | `pa-app-header-compact`; `aria-expanded` |
| `shell.nav.menu.close` | Button im geöffneten Menü | Menü schließen | Esc schließt ebenfalls |
| `shell.nav.criteria` | Navigation | Kriterien | |
| `shell.nav.projects` | Navigation | Projekte | nur mit Organisation sichtbar |
| `shell.nav.directory` | Navigation | Produkte | |
| `shell.nav.contribute` | Navigation | Beitragen | |
| `shell.nav.settings` | Menü | Einstellungen | |
| `shell.nav.account` | Menü | Konto | |
| `shell.nav.login` | Button | Anmelden | |
| `shell.nav.logout` | Menü | Abmelden | |
| `shell.userMenu.label` | `aria-label` | Menü für {{displayName}} | |
| `shell.org.active` | Anzeige | Organisation: {{name}} | bei mehreren Mitgliedschaften als Auswahl |
| `shell.org.switch` | `aria-label` | Organisation wechseln | |
| `shell.footer.about` | Link | Über ProcureAccess | |
| `shell.footer.werteit` | Link | Projekt WERTE.IT | |
| `shell.footer.suggest` | Link | Kriterium vorschlagen | → S70 |
| `shell.footer.accessibility` | Link | Erklärung zur Barrierefreiheit | Pflichtseite (BITV) |
| `shell.footer.privacy` | Link | Datenschutz | |
| `shell.footer.imprint` | Link | Impressum | |
| `shell.footer.source` | Link | Quellcode auf GitHub | |
| `shell.pageTitle` | `<title>` und Ansage beim Seitenwechsel | {{page}} – ProcureAccess | A11Y-03: Ansage über `A11yAnnouncerService`, Fokus auf H1. Werte für `{{page}}` in §5.16 |
| `shell.pageTitle.error` | `<title>` nach fehlgeschlagenem Absenden | Fehler: {{page}} – ProcureAccess | U8; wird zurückgesetzt, sobald alle Fehler behoben sind |
| **Sitzungswarnung** (A11Y-04, Dialog mit `role="alertdialog"`) | | | |
| `shell.session.warning.title` | Titel | Ihre Sitzung endet bald | |
| `shell.session.warning.body` | Text (5 Minuten vor Ablauf, nach 30 Minuten ohne Aktivität) | Aus Sicherheitsgründen werden Sie in {{minutes}} Minuten abgemeldet. Danach melden Sie sich erneut an, bevor Sie weiterarbeiten. Ihre Eingaben auf dieser Seite bleiben erhalten. | Minuten nicht sekündlich aktualisieren (sonst Dauer-Ansage); Aktualisierung höchstens einmal pro Minute, ohne erneute Ansage |
| `shell.session.warning.stay` | Button (Fokus) | Angemeldet bleiben | verlängert die Sitzung (`POST /auth/session/extend`, U50) |
| `shell.session.warning.logout` | Button | Jetzt abmelden | |
| `shell.session.extended` | Ansage | Ihre Sitzung wurde verlängert. | |
| `shell.session.warning.absolute` | Text (statt `.body`, 5 Minuten vor Ende der Höchstdauer von 8 Stunden) | Ihre Sitzung endet in {{minutes}} Minuten und kann nicht mehr verlängert werden. Speichern Sie Ihre Eingaben und melden Sie sich danach erneut an. | U53; ohne Button „Angemeldet bleiben“, Fokus auf „Schließen“ (`common.actions.close`) |
| `shell.session.expired.title` | Hinweis im Anmelde-Overlay (S01 als Dialog, nach einer Antwort 401) | Sie wurden abgemeldet | U68; kein Seitenwechsel, die Seite bleibt darunter erhalten |
| `shell.session.expired.body` | Text | Ihre Sitzung ist aus Sicherheitsgründen abgelaufen. Bitte melden Sie sich erneut an. Ihre Eingaben auf dieser Seite bleiben erhalten. | U68; nach der Anmeldung wird die unterbrochene Aktion wiederholt (ARCHITECTURE_v11 §8.3) |
| `shell.session.restored` | Ansage nach erneuter Anmeldung im Overlay | Sie sind wieder angemeldet. Ihre Eingaben sind erhalten. | U68 |

### 5.2 S01–S03 Anmeldung (`auth.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `auth.login.title` | H1 | Anmelden | |
| `auth.login.email` | Label | E-Mail-Adresse | `autocomplete="email"` |
| `auth.login.password` | Label | Passwort | `autocomplete="current-password"`, Button „Passwort anzeigen“ |
| `auth.login.showPassword` | Button | Passwort anzeigen | Umschalter, `aria-pressed` |
| `auth.login.submit` | Button | Anmelden | |
| `auth.login.forgot` | Link | Passwort vergessen? | |
| `auth.login.noAccount` | Text + Link | Noch kein Konto? Jetzt registrieren | |
| `auth.login.error.invalid` | Fehler (über dem Formular) | Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht. Bitte prüfen Sie Ihre Eingaben. | bewusst nicht verraten, welches Feld falsch ist |
| `auth.login.reasonSave` | Hinweis (wenn aus S11 „Liste speichern“ kommend) | Melden Sie sich an, um Ihre Kriterienliste zu speichern. Ihre Auswahl bleibt dabei erhalten. | E7; Anmeldung im `login`-Outlet, die Auswahl im Speicher bleibt bestehen |
| `auth.login.draftKept` | Ansage und Hinweis nach der Anmeldung | Sie sind angemeldet. Ihre Auswahl ist noch da und kann jetzt gespeichert werden. | E7; nur wenn eine nicht gespeicherte Auswahl besteht |
| `auth.register.title` | H1 | Konto erstellen | |
| `auth.register.intro` | Text | Mit einem Konto können Sie Kriterienlisten speichern und exportieren, in Beschaffungsprojekten arbeiten und Beiträge veröffentlichen. Zum Lesen von Kriterien und Produkten brauchen Sie kein Konto. | |
| `auth.register.email` | Label | E-Mail-Adresse | |
| `auth.register.password` | Label | Passwort | |
| `auth.register.password.hint` | Hint | Mindestens {{min}} Zeichen. | Regeln an Backend-Policy anpassen |
| `auth.register.privacyNote` | Text | Ihre E-Mail-Adresse wird nie öffentlich angezeigt. | wichtig für Beitragende (Pseudonymität) |
| `auth.register.consent` | Checkbox | Ich habe die Datenschutzhinweise gelesen. | Link auf „Datenschutzhinweise“ |
| `auth.register.submit` | Button | Konto erstellen | |
| `auth.register.success` | Seite | Fast geschafft. Wir haben Ihnen eine E-Mail an {{email}} geschickt. Bitte bestätigen Sie darin Ihre Adresse. | falls E-Mail-Bestätigung umgesetzt wird |
| `auth.reset.title` | H1 | Passwort zurücksetzen | |
| `auth.reset.intro` | Text | Geben Sie Ihre E-Mail-Adresse ein. Wenn ein Konto dazu existiert, erhalten Sie einen Link zum Zurücksetzen. | keine Konto-Existenz verraten |
| `auth.reset.submit` | Button | Link anfordern | |
| `auth.reset.sent` | Bestätigung | Falls ein Konto zu {{email}} existiert, ist der Link unterwegs. Er ist {{hours}} Stunden gültig. | |
| `auth.reset.newPassword` | Label | Neues Passwort | |
| `auth.reset.done` | Bestätigung | Ihr Passwort wurde geändert. Sie können sich jetzt anmelden. | |
| `auth.login.locked` | Fehler (über dem Formular) | Zu viele Anmeldeversuche. Aus Sicherheitsgründen können Sie sich erst in {{minutes}} Minuten erneut anmelden. | U72; nach 5 Fehlversuchen für 15 Minuten |
| `auth.confirm.required` | Hinweis (`pa-notice` Achtung, bei gesperrter Aktion und auf S61) | Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse. Danach können Sie Listen speichern, Organisationen anlegen, Einladungen annehmen und Beiträge veröffentlichen. | U72; Antwort 403 `email-unconfirmed` |
| `auth.confirm.resend` | Button | Bestätigungs-E-Mail erneut senden | |
| `auth.confirm.sent` | Ansage und Bestätigung | Wir haben die E-Mail erneut an {{email}} geschickt. | |
| `auth.confirm.done` | Seite nach dem Bestätigungslink | Ihre E-Mail-Adresse ist bestätigt. Sie können jetzt alle Funktionen nutzen. | |
| `auth.confirm.invalid` | Seite (Link abgelaufen oder schon benutzt) | Dieser Bestätigungslink ist nicht mehr gültig. Melden Sie sich an und fordern Sie einen neuen Link an. | 24 Stunden gültig |
| `auth.reset.invalid` | Seite (Link abgelaufen oder schon benutzt) | Dieser Link zum Zurücksetzen ist nicht mehr gültig. Bitte fordern Sie einen neuen an. | 2 Stunden gültig |

### 5.3 S04 Startseite (`home.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `home.title` | H1 | Barrierefreie Software beschaffen – Schritt für Schritt | |
| `home.intro` | Text | ProcureAccess hilft Ihnen, die passenden Barrierefreiheitsanforderungen für Ihre Software-Beschaffung zu finden – in verständlicher Sprache und ohne Vorwissen zu Normen. | |
| `home.card.criteria.title` | Karte H2 | Kriterien zusammenstellen | |
| `home.card.criteria.text` | Text | Wählen Sie Verwendungszweck und Anwendungstyp. Sie erhalten eine Liste mit empfohlenen Kriterien, die Sie anpassen und für Ihre Vergabeunterlagen exportieren können. | |
| `home.card.criteria.cta` | Button | Kriterien zusammenstellen | |
| `home.card.directory.title` | Karte H2 | Produkte nachschlagen | |
| `home.card.directory.text` | Text | Sehen Sie, ob ein Produkt bereits auf Barrierefreiheit geprüft wurde und welche Erfahrungen andere damit gemacht haben. | |
| `home.card.directory.cta` | Button | Produktverzeichnis öffnen | |
| `home.card.projects.title` | Karte H2 | Produkte vergleichen im Beschaffungsprojekt | |
| `home.card.projects.text` | Text | Halten Sie fest, welche Produkte infrage kommen, und schätzen Sie diese gemeinsam mit Ihrem Team anhand Ihrer Kriterien ein. | |
| `home.card.projects.cta` | Button | Zu meinen Projekten | |
| `home.card.contribute.title` | Karte H2 | Erfahrungen teilen | |
| `home.card.contribute.text` | Text | Sie nutzen eine Software selbst, zum Beispiel mit Screenreader oder Vergrößerung? Ihr Erfahrungsbericht hilft anderen bei der Auswahl. Sie können unter einem selbst gewählten Namen schreiben. | aktive Einladung, keine Pflicht (PRD §4) |
| `home.card.contribute.cta` | Button | Erfahrungsbericht schreiben | |
| `home.card.vendor.text` | Text-Link (dezent unter den Karten) | Sie bieten Software an oder prüfen sie im Auftrag? Prüfergebnis veröffentlichen | → S55 |
| `home.disclaimer` | Hinweis | ProcureAccess ersetzt keine professionelle Barrierefreiheitsprüfung. Das Tool stellt Kriterien und veröffentlichte Angaben zusammen, prüft aber selbst keine Produkte. | PRD §6 |

### 5.4 S10 Kriterien zusammenstellen – Auswahl (`criteria.select.`)

Stepper mit vier Schritten in fester Reihenfolge (Filterschichten, `kriterienkatalog_bit_inklusiv_v1.md` §2). Jeder Schritt hat eine eigene H2; der Stepper kündigt „Schritt {{current}} von {{total}}“ an. Nur Schritt 1 ist eine Einfachauswahl, alle anderen sind Mehrfachauswahl. Werte und Erklärungen kommen aus dem Katalog (`CriteriaFilter.Name` bzw. `.Description`).

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `criteria.select.title` | H1 | Kriterien zusammenstellen | |
| `criteria.select.intro` | Text | Beantworten Sie bis zu vier Fragen zu Ihrer Beschaffung. Daraus ergibt sich eine Liste mit Kriterien, die für Sie wichtig sind. | |
| `criteria.select.stepLabel` | Ansage; unter 600 px zusätzlich sichtbare Textzeile statt Stepper (S10-M, S10-M1) | Schritt {{current}} von {{total}}: {{name}} | U31 |
| `criteria.select.summary` | Zeile „Bisherige Auswahl“ ab Schritt 2 | {{layer}}: {{values}} | mehrere Schichten mit „ · “ getrennt; daneben Button `criteria.select.change` |
| `criteria.select.change` | Button | Ändern | springt zum Schritt; Kontext per `aria-describedby` |
| **Schritt 1 – Verwendungszweck** | | | |
| `criteria.select.step1.name` | Schrittname | Verwendungszweck | |
| `criteria.select.step1.question` | H2 / Label | Wofür setzen Sie das Produkt ein? | |
| `criteria.select.step1.hint` | Hint | Wählen Sie einen Verwendungszweck. Er bestimmt, welche Anwendungstypen und Prüfverfahren passen. Wenn nichts passt, wählen Sie „Keiner davon passt“. | |
| `criteria.select.step1.field` | Label Combobox | Verwendungszweck | `mat-autocomplete`, Einfachauswahl |
| `criteria.select.step1.field.hint` | Supporting text | Tippen Sie, um die Liste zu filtern. | |
| `criteria.select.step1.none` | Option (immer letzte, auch beim Filtern) | Keiner davon passt | |
| `criteria.select.step1.none.hint` | Zweite Zeile der Option | Es gelten die allgemeinen Empfehlungen. | |
| `criteria.select.step1.matches` | Ansage | {{count}} Treffer | Live-Region, gedrosselt |
| `criteria.select.step1.noMatch` | Leerzustand der Liste | Kein Verwendungszweck passt zu „{{query}}“. Wählen Sie „Keiner davon passt“. | |
| `criteria.select.error.noUseCase` | Feldfehler | Bitte wählen Sie einen Verwendungszweck oder „Keiner davon passt“. | |
| **Schritt 2 – Anwendungstyp** | | | |
| `criteria.select.step2.name` | Schrittname | Anwendungstyp | |
| `criteria.select.step2.question` | H2 / `legend` | Was möchten Sie beschaffen? | Checkbox-Gruppe |
| `criteria.select.step2.hint` | Hint | Wählen Sie alle Anwendungstypen, die zutreffen. Einige passen nicht zum gewählten Verwendungszweck und lassen sich deshalb nicht auswählen. | zweiter Satz nur, wenn etwas ausgeschlossen ist |
| `criteria.select.step2.open.hint` | Hint bei „(offen)“ | Eigene Hilfsmittel wie Screenreader lassen sich verwenden. | |
| `criteria.select.step2.closed.hint` | Hint bei „Software (geschlossen)“ | Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals. | EN 301 549, BIT inklusiv „Geschlossene Funktionalität“ |
| `criteria.select.step2.closedMobile.hint` | Hint bei „Mobile Anwendung (geschlossen)“ | Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden. | |
| `criteria.select.error.noAppType` | Feldfehler | Bitte wählen Sie mindestens einen Anwendungstyp. | |
| **Schritt 3 – Funktionsbereiche** | | | |
| `criteria.select.step3.name` | Schrittname | Funktionsbereiche | |
| `criteria.select.step3.question` | H2 / `legend` | Welche Funktionen hat das Produkt? | Checkbox-Gruppe |
| `criteria.select.step3.hint` | Hint | Wählen Sie alle Funktionsbereiche, die das Produkt bietet. Basisfunktionalität gilt immer. | |
| `criteria.select.step3.explain.toggle` | Schalter | Erklärungen anzeigen | `mat-slide-toggle`, Vorgabe aus |
| `criteria.select.step3.base.locked` | Beschreibung bei Basisfunktionalität | Immer ausgewählt. | Checkbox `checked` + `aria-disabled` |
| Erklärungen | Text je Option (bei eingeschaltetem Schalter) | Wortlaut aus dem Standardanforderungskatalog, siehe `kriterienkatalog_bit_inklusiv_v1.md` §2.3 | Katalog-Inhalt (`CriteriaFilter.Description`), kein UI-Text |
| **Schritt 4 – Prüfverfahren (optional)** | | | |
| `criteria.select.step4.name` | Schrittname | Prüfverfahren (optional) | |
| `criteria.select.step4.question` | H2 / `legend` | Soll das Produkt nach einem bestimmten Verfahren geprüft sein? (optional) | Checkbox-Gruppe |
| `criteria.select.step4.hint` | Hint | Jedes gewählte Prüfverfahren kommt als eigenes Kriterium in Ihre Liste, zum Beispiel „Geprüft nach BIT inklusiv Software-Test“. Sie können diesen Schritt überspringen. | Kriterium mit Priorität B, in S11 abwählbar (U34) |
| `testType.softwareTest.hint` | Hint je Option | Für Software und vorerst auch für Websites. | danach der Link `testType.link` |
| `testType.link` | Link am Ende der Hint-Zeile | Verfahren ansehen ({{domain}}) | U32; öffnet in neuem Tab (`common.newTab`); eigener Tab-Stopp nach der Checkbox |
| `testType.appTest.hint` | | Für mobile Anwendungen. | |
| `testType.pdfTest.hint` | | Für PDF-Dokumente. | |
| `testType.bikWeb.hint` | | Für Websites und Webanwendungen. | |
| `testType.bikApp.hint` | | Für mobile Anwendungen. | |
| `criteria.select.submit` | Button (Schritt 4) | Kriterienliste anzeigen | auch ohne Auswahl in Schritt 4 |
| `criteria.select.next` | Button | Weiter | |
| `criteria.select.back` | Button | Zurück | `common.actions.back`, falls vorhanden |
| **Ausschlüsse** (`kriterienkatalog_bit_inklusiv_v1.md` §3) | | | |
| `criteria.select.excluded.useCase` | Grund unter einer ausgeschlossenen Option | Passt nicht zum Verwendungszweck {{useCase}} | `aria-describedby`; Option `aria-disabled`, fokussierbar |
| `criteria.select.excluded.appType` | | Passt nicht zum Anwendungstyp {{appTypes}} | mehrere Typen mit „ oder “ verbinden |
| `criteria.select.excluded.both` | | Passt nicht zum Verwendungszweck {{useCase}} und nicht zum Anwendungstyp {{appTypes}} | |
| **Zurücksetzen** | | | |
| `criteria.select.reset.title` | Titel `pa-notice` (Attention) | Auswahl zurückgesetzt | `role="status"` |
| `criteria.select.reset.text.useCase` | Text | Sie haben den Verwendungszweck geändert. Ihre Auswahl bei Anwendungstyp, Funktionsbereichen und Prüfverfahren wurde deshalb zurückgesetzt. Bitte wählen Sie erneut aus. | U39; Basisfunktionalität bleibt ausgewählt |
| `criteria.select.reset.text.appType` | Text | Sie haben den Anwendungstyp geändert. Ihre Auswahl bei Funktionsbereichen und Prüfverfahren wurde deshalb zurückgesetzt. Bitte wählen Sie erneut aus. | U39 |
| `criteria.select.reset.text.productPart` | Text | Sie haben die Funktionsbereiche geändert. Ihre Auswahl bei den Prüfverfahren wurde deshalb zurückgesetzt. Bitte wählen Sie erneut aus. | U39 |

### 5.5 S11 Kriterienliste – Ergebnis (`criteria.list.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `criteria.list.title` | H1 | Ihre Kriterienliste | bei gespeicherter Liste: Name der Liste |
| `criteria.list.summary.count` | Text über der Liste | {{count}} Kriterien für Ihre Auswahl | U28; ersetzt `criteria.list.summary` |
| `criteria.list.summary.useCase` | `<dt>` | Verwendungszweck | Wert `<dd>`: Name oder „Keiner davon passt“ |
| `criteria.list.summary.appTypes` | `<dt>` | Anwendungstyp | Werte mit Komma getrennt |
| `criteria.list.summary.parts` | `<dt>` | Funktionsbereiche | |
| `criteria.list.summary.testTypes` | `<dt>` (nur mit Auswahl in Schritt 4) | Prüfverfahren | |
| `criteria.list.changeSelection` | Button | Auswahl ändern | → S10 mit Vorbelegung; nicht bei der Kriterienliste eines Projekts (Auswahl fest, D1) |
| `criteria.list.projectCopy` | Hinweis (`pa-notice` Info, nur bei der Kriterienliste eines Projekts) | Diese Liste gehört zum Beschaffungsprojekt „{{project}}“. Verwendungszweck, Anwendungstyp, Funktionsbereiche und Prüfverfahren sind fest. Sie können Kriterien aufnehmen oder entfernen und Prioritäten und Notizen ändern. | U63; D1, WSP-03 |
| `criteria.list.priorityNote` | Hinweisbox (oberhalb der Liste, nicht wegklickbar) | Die Prioritäten sind eine Empfehlung des Projektteams auf Grundlage fachlicher Erfahrung. Sie sind keine Vorgabe aus einer Norm. Prüfen Sie, ob sie zu Ihrer Beschaffung passen, und ändern Sie sie bei Bedarf. | PRD §7, G8 |
| `criteria.list.counts` | Text | {{a}} Mindestanforderungen, {{b}} Bewertungskriterien, {{info}} Hinweise | |
| `criteria.list.filter.label` | `aria-label` Filtergruppe | Liste filtern | |
| `criteria.list.filter.priority` | Label | Priorität | |
| `criteria.list.filter.included` | Label | Nur aufgenommene Kriterien anzeigen | |
| `criteria.list.search` | Label | In Kriterien suchen | Trefferzahl über `common.results.shown` ansagen |
| **Kriterium (Listeneintrag)** | | | |
| `criteria.item.include` | Checkbox | In Liste aufnehmen | Label enthält visuell ausgeblendet den Kriterientitel |
| `criteria.item.excluded` | Status | Nicht in Ihrer Liste | |
| `criteria.item.sourceTitle` | Zeile unter dem Titel | {{number}} {{originalTitle}} · BIT inklusiv | Originaltitel des Prüfschritts |
| `criteria.item.meta` | Metazeile | {{productPart}} · EN 301 549 {{number}} | |
| `criteria.item.showDetails` | Button | Details anzeigen | `aria-expanded`, `aria-controls`; Kriteriumstitel per `aria-describedby` |
| `criteria.item.hideDetails` | Button | Details ausblenden | |
| `criteria.item.detail.priority` | H3, erster Abschnitt der Detailansicht | Warum diese Priorität? | U28; Inhalt `criteria.priority.rationale`, `criteria.priority.rule.*`, `criteria.priority.source.expert` |
| `criteria.item.detail.requirement` | H3 in der Detailansicht | Was wird verlangt? | Inhalt: „Anforderung“ (BIT inklusiv) |
| `criteria.item.detail.applicability` | H3 | Wann gilt das? | „Anwendbarkeit“ |
| `criteria.item.detail.why` | H3 | Wen betrifft es? | „Warum wird das geprüft?“ |
| `criteria.item.detail.examples` | H3 | Woran erkennt man es? | |
| `criteria.item.detail.examples.positive` | Zwischenüberschrift | So ist es gut | |
| `criteria.item.detail.examples.negative` | Zwischenüberschrift | So ist es nicht gut | |
| `criteria.item.detail.examples.more` | Button | Alle {{count}} Beispiele anzeigen | nur bei mehr als drei Beispielen |
| `criteria.item.detail.ratings` | H3 | Wie wird bewertet? | erlaubte Stufen mit „ · “ |
| `criteria.item.detail.howTested` | H3 | So wird es geprüft | |
| `criteria.item.detail.howTested.link` | Link | Prüfschritt {{number}} im {{testType}} (öffnet bit-inklusiv.de) | Anker auf den Prüfschritt |
| `criteria.item.detail.appliesTo` | H3 | Gilt für | Anwendungstypen |
| `criteria.item.detail.source` | Quellenzeile | Texte: {{testType}}, Prüfschritt {{number}} „{{originalTitle}}“. Verwendet mit Erlaubnis. | |
| `criteria.item.detail.tabs` | Tabs (nur bei mehreren Verfahren) | Software-Test / App-Test / PDF-Test | wortgleiche Abschnitte über den Tabs |
| `criteria.item.testedBy.title` | Titel des Kriteriums aus Schritt 4 | Geprüft nach {{testType}} | |
| `criteria.item.testedBy.text` | Kurzbeschreibung | Der Anbieter oder eine unabhängige Stelle hat das Produkt nach dem {{testType}} geprüft und kann den Bericht vorlegen. | |
| `criteria.item.testedBy.meta` | Metazeile | Prüfverfahren · {{link}} | |
| `criteria.item.testedBy.note` | Zusatz in den Details | Im Beschaffungsprojekt ergibt sich Ja oder Nein aus dem Prüfnachweis des Produkts. Im Abgleich mit Kriterien zählt dieses Kriterium nicht mit, weil der Prüfstatus dort schon unter „Auf Barrierefreiheit geprüft“ steht. | U34, U46 |
| `criteria.item.note` | Label | Ihre Notiz zu diesem Kriterium (optional) | |
| **K5 Priorität mit Begründung** (`criteria.priority.`) | | | |
| `criteria.priority.A` | Chip | A – Mindestanforderung | A2 |
| `criteria.priority.B` | Chip | B – Bewertungskriterium | |
| `criteria.priority.Info` | Chip | Hinweis | |
| `criteria.priority.weight` | Zusatz (nur B) | Gewichtung {{weight}} | |
| `criteria.priority.why` | Button (Expansion Panel) | Warum diese Priorität? | Kriteriumstitel per `aria-describedby` (gleichnamige Buttons, 7f V5); gilt ebenso für „Notiz hinzufügen“ und „Empfehlung wiederherstellen“. In der Kriterienliste (S11) nicht sichtbar – dort steht die Begründung in den Details (U28) |
| `criteria.priority.rationale` | Text im Panel | {{rationale}} | Inhalt aus `RationaleDe` |
| `criteria.priority.rule.full` | Text im Panel | Diese Empfehlung gilt für {{appType}} mit dem Verwendungszweck {{useCase}}. | CAT-02, Regel 1 |
| `criteria.priority.rule.appType` | | Diese Empfehlung gilt für {{appType}}, unabhängig vom Verwendungszweck. | Regel 2 |
| `criteria.priority.rule.useCase` | | Diese Empfehlung gilt für den Verwendungszweck {{useCase}}, unabhängig vom Anwendungstyp. | Regel 3 |
| `criteria.priority.rule.default` | | Das ist die allgemeine Empfehlung für alle Anwendungstypen. | Regel 4 |
| `criteria.priority.source.expert` | Text im Panel | Grundlage: Empfehlung des Projektteams WERTE.IT (fachliche Einschätzung, keine Vorgabe aus einer Norm). | |
| `criteria.priority.change` | Button | Priorität ändern | öffnet Select |
| `criteria.priority.button` | Zugänglicher Name des Menü-Buttons (K5) | Priorität: {{level}} | sichtbar ist nur `{{level}}`; Kriteriumstitel per `aria-describedby` |
| `criteria.priority.changed` | Ansage | Priorität geändert auf {{level}}. | U9 |
| `criteria.priority.changeLabel` | Label Select | Priorität für dieses Kriterium | |
| `criteria.priority.overridden` | Status | Von Ihnen geändert. Empfohlen war: {{recommended}}. | CAT-08: Empfehlung bleibt sichtbar |
| `criteria.priority.reset` | Button | Empfehlung wiederherstellen | |
| **Aktionen** | | | |
| `criteria.list.save` | Button | Liste speichern | ohne Anmeldung → S01 mit `auth.login.reasonSave` |
| `criteria.list.export` | Button | Liste exportieren | auch ohne Anmeldung (E7) → S13 |
| `criteria.list.draftNote` | Hinweis (nur ohne Anmeldung, oberhalb der Aktionen, nicht wegklickbar) | Ihre Auswahl wird nicht gespeichert, solange Sie nicht angemeldet sind. Wenn Sie die Seite neu laden oder schließen, geht sie verloren. Laden Sie die Liste herunter oder melden Sie sich an, um sie zu speichern. | E7; kein Warndialog beim Verlassen nötig, der Hinweis genügt |
| `criteria.list.unsaved` | Status in der Aktionsleiste (Auswahl noch nicht gespeichert) | Noch nicht gespeichert | in der Aktionsleiste unter `criteria.list.status.count` (7e); ohne Anmeldung zusätzlich `criteria.list.draftNote`. Wechsel zu „Liste gespeichert.“ per Ansage |
| `criteria.list.status.count` | Status in der Aktionsleiste | {{included}} von {{total}} Kriterien in Ihrer Liste | Live-Region, polite, höchstens eine Ansage pro Sekunde |
| `criteria.list.share` | Button | Freigabelink erstellen | nur gespeicherte Listen der Organisation, nicht die Kriterienliste eines Projekts (D1, D7) |
| `criteria.list.suggest` | Link (am Listenende) | Fehlt ein Kriterium oder ist eine Formulierung unklar? Schlagen Sie eine Änderung vor | → S70 (E2). Als umbrechender Link, nicht als Button (bricht sonst bei 400 % Zoom nicht um) |
| `criteria.list.noCriteriaFor` | Hinweis (`pa-notice` Info) über der Liste bzw. Leerzustand | Für {{appTypes}} gibt es noch keine Kriterien, weil BIT inklusiv dafür noch kein Prüfverfahren enthält. Wählen Sie einen weiteren Anwendungstyp oder ein Prüfverfahren, oder ändern Sie Ihre Auswahl. | U40; aus `appTypesWithoutCriteria[]` (ARCHITECTURE_v11 C36); Link „Auswahl ändern“ → S10 Schritt 2; regulärer Zustand, kein Fehler (CAT-20) |
| `criteria.list.empty` | Leerzustand | Für diese Auswahl gibt es keine Kriterien. Das sollte nicht vorkommen. Bitte ändern Sie Ihre Auswahl oder schreiben Sie uns. | Fehlerfall, nur wenn `appTypesWithoutCriteria[]` leer ist |
| `criteria.list.emptyFilter` | Leerzustand (Filter) | Keine Kriterien passen zu Ihrem Filter. Filter zurücksetzen | |
| **Hinweis bei älterer Katalogversion** | | | |
| `criteria.list.olderRelease` | Hinweisbox | Diese Liste beruht auf der Katalogversion {{release}}. Inzwischen gibt es die Version {{latest}}. Ihre Liste bleibt unverändert, damit nachvollziehbar bleibt, was Sie verwendet haben. Wenn Sie die neue Fassung nutzen möchten, erstellen Sie eine neue Liste mit derselben Auswahl. | ADR-002; Button „Neue Liste mit derselben Auswahl“ legt eine neue Liste der Organisation nach der aktuellen Katalogversion an (D6, ARCHITECTURE_v11 §16.3.1, CAT-22) |
| `criteria.list.olderRelease.cta` | Button | Neue Liste mit derselben Auswahl | U66; öffnet danach die neue Liste |
| `criteria.list.copy.done` | Ansage und Hinweis (`pa-notice` Info) in der neuen Liste | Neue Liste nach Katalogversion {{release}} erstellt. Ihre bisherige Liste bleibt unverändert. | U66 |
| `criteria.list.copy.dropped` | Teil des Hinweises (nur wenn zutreffend) | Diese Auswahl gibt es in der neuen Katalogversion nicht mehr oder sie ist dort ausgeschlossen: {{values}}. | U66; Werte mit Komma |
| `criteria.list.copy.notCarried` | Teil des Hinweises (nur wenn zutreffend) | Ihre Änderungen an {{count}} Kriterien ließen sich nicht übernehmen, weil es diese Kriterien in der neuen Katalogversion nicht mehr gibt: {{criteria}}. | U66 |

### 5.6 S12–S16 Speichern, Exportieren, Teilen (`criteria.save.`, `criteria.export.`, `criteria.share.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `criteria.save.title` | Dialog-Titel | Kriterienliste speichern | |
| `criteria.save.name` | Label | Name der Liste | |
| `criteria.save.name.hint` | Hint | Zum Beispiel der Name Ihres Vergabeverfahrens. | |
| `criteria.save.orgNote` | Text | Die Liste ist für alle Mitglieder Ihrer Organisation {{org}} sichtbar. | |
| `criteria.save.submit` | Button | Liste speichern | |
| `criteria.save.success` | Snackbar / Ansage | Liste „{{name}}“ gespeichert. | |
| `criteria.save.needOrg` | Dialog (ohne Organisation) | Um Kriterienlisten zu speichern, brauchen Sie eine Organisation. So können Sie die Liste mit Ihrem Team teilen. | → S20 |
| `criteria.save.needOrg.cta` | Button | Organisation anlegen oder beitreten | |
| `criteria.export.title` | Dialog-Titel | Kriterienliste exportieren | |
| `criteria.export.format` | `legend` | Dateiformat | |
| `criteria.export.xlsx` | Option | Excel-Datei (.xlsx) – für Vergabeunterlagen | Vorauswahl |
| `criteria.export.csv` | Option | CSV-Datei – für andere Programme | |
| `criteria.export.scope` | Text | Exportiert werden {{count}} Kriterien aus Ihrer Liste mit Priorität, Gewichtung und Ihren Notizen. Nicht aufgenommene Kriterien sind nicht enthalten. | |
| `criteria.export.anonymousNote` | Hinweis (nur ohne Anmeldung) | Die Datei enthält Ihre Auswahl und Ihre Änderungen. ProcureAccess speichert davon nichts. Wenn Sie später daran weiterarbeiten möchten, melden Sie sich an und speichern Sie die Liste. | E7; EXP-05 |
| `criteria.export.releaseNote` | Text | Die Datei nennt die Katalogversion {{release}}. So bleibt später nachvollziehbar, welche Fassung der Kriterien Sie verwendet haben. | EXP-03 |
| `criteria.export.legalNote` | Hinweis | Kriterien in Vergabeunterlagen können rechtlich verbindlich werden. Prüfen Sie die Liste vor der Übernahme mit der für Ihr Verfahren zuständigen Stelle. | Text durch rechtliche Prüfung bestätigen lassen (⚖️) |
| `criteria.export.submit` | Button | Datei herunterladen | |
| `criteria.export.success` | Ansage | Die Datei wird heruntergeladen. | |
| `criteria.export.error.title` | Titel des Fehlerhinweises im Dialog | Export fehlgeschlagen | U7; `pa-notice` Fehler, `role="alert"`; Button daneben `common.actions.retry` |
| `criteria.export.error` | Fehler | Die Datei konnte nicht erstellt werden. Bitte versuchen Sie es erneut. Wenn der Fehler bleibt, schreiben Sie uns. | |
| `criteria.export.outdated` | Fehler (Katalogversion hat sich während der Bearbeitung geändert) | Die Datei konnte nicht erstellt werden, weil inzwischen eine neue Katalogversion gilt. Bitte laden Sie die Kriterien neu. Ihre Auswahl von Verwendungszweck, Anwendungstyp, Funktionsbereichen und Prüfverfahren bleibt erhalten; Ihre Änderungen an einzelnen Kriterien müssen Sie erneut vornehmen. | 400 aus EXP-07 bei nicht passenden Kriterien-IDs; selten |
| `criteria.share.title` | Dialog-Titel | Freigabelink erstellen | |
| `criteria.share.intro` | Text | Wer diesen Link hat, kann die Liste lesen, auch ohne Konto, aber nicht ändern. | U67; D7, TEN-09 |
| `criteria.share.expiry` | Label | Gültig bis | Datumsauswahl |
| `criteria.share.expiry.hint` | Hint | Vorgabe: in 90 Tagen. Höchstens ein Jahr. | U75; SD15 |
| `criteria.share.expiry.error` | Feldfehler | Bitte wählen Sie ein Datum zwischen morgen und {{maxDate}}. | TEN-12 |
| `criteria.share.create` | Button | Link erstellen | |
| `criteria.share.link` | Label | Freigabelink | Textfeld, schreibgeschützt |
| `criteria.share.copy` | Button | Link kopieren | |
| `criteria.share.copied` | Ansage | Link kopiert. | |
| `criteria.share.revoke` | Button | Link ungültig machen | |
| `criteria.share.revoked` | Ansage | Der Link ist nicht mehr gültig. | |
| `criteria.shared.title` | H1 (S16) | Geteilte Kriterienliste: {{name}} | |
| `criteria.shared.note` | Hinweis | Diese Liste wurde mit Ihnen geteilt. Sie können sie ohne Konto lesen. Um eine eigene Liste zu erstellen, wählen Sie „Kriterien zusammenstellen“. | U67; S16 ohne Anmeldung (TEN-09) |
| `criteria.shared.invalid` | Seite | Dieser Link ist nicht mehr gültig. Er ist abgelaufen oder wurde zurückgenommen. Bitten Sie die Person, die ihn geteilt hat, um einen neuen Link. | TEN-08 |
| `criteria.saved.title` | H1 (S14) | Gespeicherte Kriterienlisten | |
| `criteria.saved.empty` | Leerzustand | Ihre Organisation hat noch keine Kriterienlisten gespeichert. Stellen Sie eine Liste zusammen und speichern Sie sie, um sie hier wiederzufinden. | |
| `criteria.saved.empty.cta` | Button | Kriterien zusammenstellen | |
| `criteria.saved.delete` | Button (je Liste, Rollen Verwalten und Bearbeiten) | Liste löschen | U75; Name der Liste per `aria-describedby` |
| `criteria.saved.delete.title` | Dialog-Titel | Kriterienliste löschen? | |
| `criteria.saved.delete.body` | Text | Die Liste „{{name}}“ wird gelöscht. Freigabelinks zu dieser Liste funktionieren danach nicht mehr. Beschaffungsprojekte, die eine Kopie der Liste nutzen, sind nicht betroffen. | D1 |
| `criteria.saved.delete.confirm` | Button | Liste löschen | |
| `criteria.saved.deleted` | Ansage | Die Liste „{{name}}“ wurde gelöscht. | |

### 5.7 S20–S21 Organisation (`org.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `org.start.title` | H1 | Organisation anlegen oder beitreten | |
| `org.start.intro` | Text | Kriterienlisten und Beschaffungsprojekte gehören zu einer Organisation, damit Sie gemeinsam mit Ihrem Team daran arbeiten können. Andere Organisationen sehen Ihre Listen und Projekte nicht. | Mandantentrennung erklären (Wettbewerbsargument) |
| `org.start.contributeNote` | Hinweis | Sie möchten nur einen Erfahrungsbericht schreiben oder ein Prüfergebnis veröffentlichen? Dafür brauchen Sie keine Organisation. | → S53 / S55 |
| `org.create.name` | Label | Name der Organisation | |
| `org.create.type` | `legend` | Art der Organisation | |
| `org.create.type.public` | Option | Öffentliche Stelle | |
| `org.create.type.company` | Option | Unternehmen | |
| `org.create.type.association` | Option | Verband oder Verein | |
| `org.create.submit` | Button | Organisation anlegen | |
| `org.join.text` | Text | Wenn Ihre Organisation schon angelegt ist, bitten Sie ein Mitglied mit der Rolle „Verwalten“ um eine Einladung. | |
| `org.members.title` | H1 | Mitglieder von {{org}} | |
| `org.members.role.owner` | Rolle | Verwalten | |
| `org.members.role.owner.hint` | Hint | Kann Mitglieder einladen, Rollen ändern und alles bearbeiten. | |
| `org.members.role.editor` | Rolle | Bearbeiten | |
| `org.members.role.editor.hint` | Hint | Kann Listen und Projekte anlegen und bearbeiten. | |
| `org.members.role.reader` | Rolle | Lesen | |
| `org.members.role.reader.hint` | Hint | Kann Listen und Projekte ansehen. | |
| `org.invite.email` | Label | E-Mail-Adresse | |
| `org.invite.submit` | Button | Einladung senden | |
| `org.invite.sent` | Ansage | Einladung an {{email}} gesendet. Sie ist {{days}} Tage gültig. | |
| `org.invite.pending` | Status | Eingeladen, noch nicht angenommen | |
| `org.invite.revoke` | Button | Einladung zurücknehmen | |
| `org.invite.accept` | Button (Seite nach dem Einladungslink) | Einladung annehmen | Seite: „Sie wurden in die Organisation {{org}} eingeladen.“ (`org.invite.accept.text`) |
| `org.invite.accept.text` | Text | Sie wurden in die Organisation {{org}} eingeladen. Ihre Rolle: {{role}}. | |
| `org.invite.mismatch` | Fehler | Diese Einladung gilt für eine andere E-Mail-Adresse. Melden Sie sich mit {{email}} an oder bitten Sie um eine neue Einladung. | U74 |
| `org.invite.invalid` | Seite | Diese Einladung ist nicht mehr gültig. Sie ist abgelaufen oder wurde zurückgenommen. Bitten Sie ein Mitglied mit der Rolle „Verwalten“ um eine neue Einladung. | 14 Tage gültig |
| `org.members.lastOwner` | Fehler | Jede Organisation braucht mindestens ein Mitglied mit der Rolle „Verwalten“. Geben Sie diese Rolle zuerst einem anderen Mitglied. | U74; TEN-11 |
| `org.members.remove` | Button | Aus der Organisation entfernen | Name per `aria-describedby` |
| `org.leave` | Button | Organisation verlassen | |
| `org.leave.body` | Dialog-Text | Sie verlieren den Zugriff auf die Kriterienlisten und Projekte von {{org}}. Ihre veröffentlichten Beiträge bleiben unter Ihrem Beitragsprofil sichtbar. | R-ORG-04 |

### 5.8 S30–S34 Beschaffungsprojekte (`projects.`, `candidate.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `projects.title` | H1 | Beschaffungsprojekte | |
| `projects.intro` | Text | Halten Sie hier fest, welche Produkte für eine Beschaffung infrage kommen, und schätzen Sie sie anhand Ihrer Kriterien ein. Alles hier ist nur für Ihre Organisation sichtbar. | |
| `projects.create` | Button | Projekt anlegen | |
| `projects.empty` | Leerzustand | Noch keine Beschaffungsprojekte. Legen Sie ein Projekt an, sobald Sie Produkte für eine Beschaffung vergleichen möchten. | |
| `projects.create.title` | Dialog-Titel | Beschaffungsprojekt anlegen | |
| `projects.create.name` | Label | Name des Projekts | |
| `projects.create.name.hint` | Hint | Zum Beispiel „Neues Bewerbermanagement 2027“. | |
| `projects.create.criteria` | Label | Kriterienliste (optional) | Select aus S14 (nur Listen der Organisation, keine Listen anderer Projekte) |
| `projects.create.criteria.hint` | Hint | Anhand dieser Liste schätzen Sie die Produkte ein. Das Projekt erhält eine eigene Kopie der Liste; Änderungen daran gelten nur für dieses Projekt. Sie können die Liste auch später festlegen. | U63; D1, WSP-02 |
| `projects.status.label` | Label | Stand | |
| `projects.status.open` / `.done` / `.archived` | Werte | Laufend / Abgeschlossen / Archiviert | an `ProcurementProject.Status` anpassen |
| **S32 Projektübersicht** (Figma S32, S32-M) | | | U52 |
| `projects.detail.meta` | Zeile unter H1 | Stand: {{status}} · {{count}} Produkte · nur für Ihre Organisation sichtbar | `status` = `projects.status.*`; Einzahl „1 Produkt“ |
| `projects.detail.edit` | Button (outlined) | Projekt bearbeiten | Name und Stand |
| `projects.detail.criteria.title` | H2 der Karte | Kriterienliste | |
| `projects.detail.criteria.adjust` | Button (Text) | Kriterienliste anpassen | → S11 der zugeordneten Liste; bleibt auch nach Einschätzungen verfügbar (C51) |
| `projects.detail.criteria.list` / `.release` / `.scope` / `.useCase` / `.appTypes` / `.testTypes` | `dt` | Liste / Katalogversion / Umfang / Verwendungszweck / Anwendungstyp / Prüfverfahren | `dd` „Liste“ ist Link zu S11; ohne Verwendungszweck: „Keiner davon passt“; ohne Prüfverfahren Zeile weglassen |
| `projects.detail.criteria.scope.value` | `dd` | {{total}} Kriterien: {{a}} Mindestanforderungen (A), {{b}} Bewertungskriterien (B), {{info}} Hinweise – dazu {{procedures}} × „Geprüft nach …“ (zählen im Abgleich nicht mit) | U55; Zählung der *Liste*, kein Produktmaß; `total`, `a`, `b`, `info` zählen nur Anforderungskriterien. Beispiel: „46 Kriterien: 12 Mindestanforderungen (A), 28 Bewertungskriterien (B), 6 Hinweise – dazu 2 × „Geprüft nach …“ (zählen im Abgleich nicht mit)“ |
| `projects.detail.criteria.scope.value.noProcedures` | `dd` (Liste ohne Prüfverfahren) | {{total}} Kriterien: {{a}} Mindestanforderungen (A), {{b}} Bewertungskriterien (B), {{info}} Hinweise | U55 |
| `projects.detail.criteria.copyNote` | Text unter der Beschreibungsliste | Dieses Projekt nutzt eine eigene Kopie der Liste „{{source}}“. Änderungen daran gelten nur für dieses Projekt. Verwendungszweck, Anwendungstyp, Funktionsbereiche und Prüfverfahren sind fest. | U63; D1, WSP-02, WSP-03 |
| `projects.detail.addProduct.fromDirectory` | Menüpunkt | Aus dem Produktverzeichnis übernehmen | „Merken“ (ARCHITECTURE §5.5) → Suche wie S40, Auswahl legt das Produkt mit Verknüpfung an |
| `projects.detail.addProduct.new` | Menüpunkt | Neu erfassen | → S33 |
| `projects.detail.orderNote` | Text über der Liste | Reihenfolge: zuletzt hinzugefügt zuerst. Die Liste ist kein Ranking – ein Gesamturteil gibt es bewusst nicht. | D3, §8.4 |
| `projects.detail.row.tested` | `dt` | Auf Barrierefreiheit geprüft | `dd`: „Ja – {{count}} Prüfnachweise: {{types}}“ (Arten der Prüfung mit Komma, z. B. „Ja – 2 Prüfnachweise: BIT inklusiv Software-Test, BIK BITV-Test (Web)“; Einzahl „Ja – 1 Prüfnachweis: …“), „Ja“ (ohne Nachweis), „Nein“, „Nicht bekannt“ (U60) |
| `projects.detail.row.progress` | `dt` | Einschätzung | `dd` = `candidate.assessment.progress` |
| `projects.detail.row.counts` | `dt` | Abgleich | `dd`: „Erfüllt: {{fulfilled}} · Leichte Einschränkung: {{slightRestriction}} · Einschränkung: {{restriction}} · Barriere: {{barrier}} · Blockade: {{blockade}} · Nicht anwendbar: {{notApplicable}} · Nicht eingeschätzt: {{unknown}}“ (U54) – immer alle sieben Werte, feste Reihenfolge, auch 0; als Liste ausgezeichnet, jedes Element ohne Umbruch (`white-space: nowrap`), Trennzeichen nur per CSS (wie `directory.entry.row.*`); unter 600 px ein Element je Zeile. Beispiel: „Erfüllt: 22 · Leichte Einschränkung: 3 · Einschränkung: 6 · Barriere: 2 · Blockade: 1 · Nicht anwendbar: 2 · Nicht eingeschätzt: 10“ |
| `projects.detail.row.publication` | `dt` | Veröffentlichung | `dd`: „Im Produktverzeichnis seit {{date}}“ mit Link `projects.detail.lookup`, oder „Nicht veröffentlicht“ (U60) |
| `projects.detail.products` | H2 | Produkte in diesem Projekt | |
| `projects.detail.addProduct` | Menü-Button | Produkt hinzufügen | U60; mit Pfeil-Symbol (`arrow_drop_down`, `aria-hidden`) und `aria-haspopup="menu"`; Menüpunkte `.fromDirectory`, `.new`. Der Produktname (H3) ist der einzige Link zu S34 |
| `projects.detail.noCriteria` | Hinweis | Diesem Projekt ist noch keine Kriterienliste zugeordnet. Ohne Liste können Sie Produkte erfassen, aber nicht anhand von Kriterien einschätzen. | |
| `projects.detail.noCriteria.cta` | Button | Kriterienliste zuordnen | |
| `projects.detail.criteriaLocked` | Hinweis (statt Button „Kriterienliste wechseln“) | Die Kriterienliste kann nicht mehr gegen eine andere ausgetauscht werden, weil schon Produkte danach eingeschätzt wurden. Sie können die Liste aber weiter anpassen: Kriterien aufnehmen oder entfernen, Prioritäten und Notizen ändern. Für eine Liste nach einer neueren Katalogversion legen Sie ein neues Projekt an. | U48, U49, U63; WSP-02, WSP-03; „eingeschätzt“ = mindestens ein Wert außer „Nicht eingeschätzt“ |
| `projects.detail.criteriaAdded` | Ansage und Hinweis nach dem Aufnehmen von Kriterien | {{count}} Kriterien neu in der Liste. Sie sind bei allen Produkten noch nicht eingeschätzt. | U49; WSP-03 |
| `projects.detail.empty` | Leerzustand | Noch keine Produkte in diesem Projekt. Fügen Sie die Produkte hinzu, die Sie in Betracht ziehen. | |
| `projects.detail.lookup` | Link im `dd` „Veröffentlichung“ (bei veröffentlichten Produkten) | Im Produktverzeichnis ansehen | U60; Produktname per `aria-describedby`; → S41 |
| `projects.detail.noResults.werteit` | Hinweis (P1) | Sie finden keine passenden Produkte? Das Projekt WERTE.IT berät Sie gern. | PRD P1 |
| `candidate.form.title.new` | H1 | Produkt hinzufügen | |
| `candidate.form.title.edit` | H1 | Produkt bearbeiten | |
| `candidate.form.name` | Label | Produktname | Autocomplete aus Verzeichnis |
| `candidate.form.name.match` | Hinweis | Dieses Produkt gibt es bereits im Produktverzeichnis. Möchten Sie es verknüpfen? Dann sehen Sie veröffentlichte Angaben direkt hier. | |
| `candidate.form.vendor` | Label | Anbieter | |
| `candidate.form.website` | Label | Webseite des Anbieters | |
| `candidate.form.summary` | Label | Was kann das Produkt? (Stichworte) | |
| `candidate.form.privateSection` | H2 + Text | Nur für Ihre Organisation | |
| `candidate.form.privateSection.text` | Text | Diese Angaben werden nie veröffentlicht, auch nicht, wenn Sie das Produkt später im Verzeichnis veröffentlichen. | PRV-02; visuell klar abgesetzt, Schloss-Symbol nur ergänzend |
| `candidate.form.contacts` | Label | Ansprechpersonen und Kontaktdaten | |
| `candidate.form.internalNotes` | Label | Interne Notizen | |
| `candidate.tabs.evidence` | Tab | Prüfnachweis | |
| `candidate.tabs.assessment` | Tab | Einschätzung nach Kriterien | |
| `candidate.tabs.assessment.short` | Tab unter 600 px | Einschätzung | U31; Seitentitel behält den vollen Namen |
| `candidate.tabs.overview` | Tab | Überblick | Einstiegs-Tab (Route ohne Suffix) |
| **Überblick (S34, Tab „Überblick“)** | | | nur Fakten und Wege zu den anderen Tabs, kein Gesamturteil |
| `candidate.overview.details.title` | H2 | Produktangaben | Button „Bearbeiten“ → S33 |
| `candidate.overview.appType` | `dt` | Anwendungstyp | Wert aus der Kriterienliste des Projekts, mehrere mit Komma; ohne Liste „Noch keine Kriterienliste“ |
| `candidate.overview.directory` | `dt` | Im Produktverzeichnis | |
| `candidate.overview.directory.linked` | `dd` (Link) | Verknüpft: {{product}} – {{count}} veröffentlichte Einträge ansehen | → S41; ohne Verknüpfung: „Nicht verknüpft“ |
| `candidate.overview.status.title` | H2 | Stand der Bewertung | |
| `candidate.overview.status.evidence` | H3 | Prüfnachweis | |
| `candidate.overview.status.tested` | Text | Auf Barrierefreiheit geprüft: {{status}} | Ja / Nein / Nicht bekannt |
| ~~`candidate.overview.status.unknownHint`~~ | – | entfällt (U29) | Hinweis steht im Kopf (`candidate.publish.cta.hint`) |
| `candidate.overview.status.toEvidence` | Button | Zum Prüfnachweis | |
| `candidate.overview.status.assessment` | H3 | Einschätzung nach Kriterien | |
| `candidate.overview.status.progress` | Text | {{done}} von {{total}} Kriterien eingeschätzt · Kriterienliste „{{list}}“ (Katalogversion {{release}}) | |
| `candidate.overview.status.breakdownTitle` | Name der Region (K1, Scope Workspace) | Ihre Einschätzung nach „{{list}}“ | sieben Zeilen, ohne „Damals nicht im Katalog“ (U38) |
| `candidate.overview.status.breakdownIntro` | Text | So haben Sie die Kriterien Ihrer Liste bisher eingeschätzt: | |
| `candidate.overview.status.toAssessment` | Button | Zur Einschätzung | |
| `candidate.overview.publication.title` | H2 | Veröffentlichung | |
| `candidate.overview.publication.none` | Text | Noch nicht im Produktverzeichnis veröffentlicht. | veröffentlicht: `candidate.published.status` |
| `candidate.overview.publication.explain` | Text | Wenn Sie veröffentlichen, erscheinen Prüfnachweis und Einschätzungen als „Unabhängige Angaben“ unter einem Anzeigenamen. Ihre Organisation und die Angaben im Bereich „Nur für Ihre Organisation“ werden nie veröffentlicht. | |
| `candidate.overview.private.title` | H2 | Nur für Ihre Organisation | = `candidate.form.privateSection`; Text `candidate.form.privateSection.text` |
| **Prüfnachweis** | | | |
| `candidate.evidence.tested` | `legend` | Wurde das Produkt auf Barrierefreiheit geprüft? | Radio Group, drei Optionen (E8) |
| `candidate.evidence.tested.yes` | Option | Ja | |
| `candidate.evidence.tested.no` | Option | Nein | |
| `candidate.evidence.tested.unknown` | Option (Vorauswahl) | Nicht bekannt | `TestStatus = Unknown` |
| `candidate.evidence.tested.hint` | Hint | Gemeint ist eine Prüfung durch Anbieter, Prüfstelle oder Ihre Organisation. Wenn Sie es noch nicht wissen, lassen Sie „Nicht bekannt“ stehen und ergänzen Sie es später. Vor einer Veröffentlichung brauchen wir Ja oder Nein. | E8 |
| `candidate.evidence.details.hint` | Text unter der Radio Group (solange nicht „Ja“ gewählt ist) | Angaben zu Art, Stelle und Datum der Prüfung – und Ihre interne Notiz zur prüfenden Stelle – erscheinen, sobald Sie „Ja“ wählen. | 7e; die Felder stehen im DOM direkt nach der Radio Group |
| `candidate.evidence.list.title` | H2 über den Prüfnachweis-Karten (nur bei „Ja“) | Prüfnachweise | U56 |
| `candidate.evidence.testType` | Label | Art der Prüfung | `mat-select`, Werte `testType.*` (siehe unten), keine Ausschlussregeln |
| `candidate.evidence.testType.otherName` | Label (nur bei „Andere Prüfung“, Pflicht) | Name des Prüfverfahrens | |
| `candidate.evidence.testedBy` | Label | Wer hat geprüft? | |
| `candidate.evidence.testDate` | Label | Datum der Prüfung | Hint `common.date.hint` |
| `candidate.evidence.version` | Label | Geprüfte Version (optional) | U56; `TestEvidence.TestedVersion`; bei einer neuen Version des Produkts wird dieser Nachweis aktualisiert (WSP-01) |
| `candidate.evidence.url` | Label | Link zum Prüfbericht (optional) | U56 |
| **Mehrere Prüfnachweise** (`candidate.evidence.list.`) | | | U45, U56; die Felder oben bilden je einen Nachweis (Karte mit H3) in dieser Reihenfolge: Art der Prüfung, Wer hat geprüft?, Datum, Geprüfte Version, Link, private Notiz, Button „Entfernen“. Dieselben Karten nutzt S55 Schritt 2 (ohne private Notiz) |
| `candidate.evidence.list.item` | H3 je Nachweis | Prüfnachweis {{n}}: {{testType}} | ohne gewählte Art: „Prüfnachweis {{n}}“ |
| `candidate.evidence.list.add` | Button | Weiteren Prüfnachweis hinzufügen | z. B. wenn nach zwei Verfahren geprüft wurde; Fokus in das neue Feld „Art der Prüfung“ |
| `candidate.evidence.list.remove` | Button | Prüfnachweis {{n}} entfernen | Ansage `candidate.evidence.list.removed` |
| `candidate.evidence.list.removed` | Ansage | Prüfnachweis entfernt. | |
| `candidate.evidence.list.duplicate` | Feldfehler | Für dieses Prüfverfahren gibt es schon einen Prüfnachweis. | WSP-01 |
| `candidate.evidence.list.statusChange` | Dialog-Text (Wechsel von „Ja“ auf „Nein“ oder „Nicht bekannt“ bei vorhandenen Nachweisen) | Ihre {{count}} Prüfnachweise werden dabei entfernt. Möchten Sie fortfahren? | Buttons „Nachweise entfernen“ / „Abbrechen“ |
| `candidate.evidence.qualification` | Label | Ihre Notiz zur Qualifikation der prüfenden Stelle | Teil des privaten Bereichs, mit `candidate.form.privateSection.text`; nur in S34a |
| **Einschätzung (K6)** | | | |
| `candidate.assessment.intro` | Text | Schätzen Sie für jedes Kriterium ein, wie gut das Produkt es erfüllt. Die Stufen stammen aus den Prüfverfahren von BIT inklusiv. Grundlage können eigene Tests, Prüfberichte oder Angaben des Anbieters sein. Notieren Sie, worauf Sie sich stützen. | |
| `candidate.assessment.progress` | Text | {{done}} von {{total}} Kriterien eingeschätzt | Fortschritt der Arbeit, kein Produktmaß – bewusst nicht als Balken mit Prozent. Änderung als Ansage, gedrosselt |
| `candidate.assessment.filter.unassessed` | Checkbox | Nur noch nicht eingeschätzte anzeigen | 7f O7; Trefferzahl über `common.results.shown` |
| `candidate.assessment.legend` | `legend` je Kriterium | Wie erfüllt das Produkt dieses Kriterium? | Kriterientitel als Teil des Gruppennamens |
| `candidate.assessment.select` | Label `mat-select` (K6 kompakt, unter 600 px) | Einschätzung | U31; Kriteriumstitel ist `legend` der Gruppe |
| `candidate.assessment.note.add` | Button (Disclosure, K6 kompakt) | Begründung hinzufügen | Kriteriumstitel per `aria-describedby` |
| `candidate.assessment.note.edit` | Button (Disclosure, K6 kompakt, Notiz vorhanden) | Begründung bearbeiten | |
| `assessment.value.fulfilled` | Option | Erfüllt | gemeinsame Werte für K6, K1, S42; Skala von BIT inklusiv. K6 zeigt nur die Stufen, die das Kriterium erlaubt |
| `assessment.value.slightRestriction` | Option | Leichte Einschränkung | |
| `assessment.value.restriction` | Option | Einschränkung | |
| `assessment.value.barrier` | Option | Barriere | |
| `assessment.value.blockade` | Option | Blockade | |
| `assessment.value.notApplicable` | Option | Nicht anwendbar | |
| `assessment.value.notApplicable.hint` | Hint | Das Kriterium betrifft eine Funktion, die das Produkt nicht hat. | |
| `assessment.value.unknown` | Option (Vorauswahl) | Nicht eingeschätzt | |
| **Erklärung der Stufen (`pa-rating-legend`)** | | | Texte fachlich prüfen (Kriterienkatalog K-8) |
| `assessment.legend.show` | Button (Disclosure) | Was bedeuten die Bewertungsstufen? | `aria-expanded`, `aria-controls` |
| `assessment.legend.hide` | Button | Erklärung der Bewertungsstufen ausblenden | |
| `assessment.value.fulfilled.explain` | `dd` | Die Anforderung ist vollständig umgesetzt. Niemand wird bei dieser Funktion behindert. | |
| `assessment.value.slightRestriction.explain` | `dd` | Kleine Mängel. Die Nutzung ist kaum erschwert. | |
| `assessment.value.restriction.explain` | `dd` | Spürbare Mängel. Betroffene Menschen können die Funktion nutzen, aber nur mit Mehraufwand oder über Umwege. | |
| `assessment.value.barrier.explain` | `dd` | Erhebliche Mängel. Betroffene Menschen können die Funktion nur mit großer Mühe oder mit fremder Hilfe nutzen. | |
| `assessment.value.blockade.explain` | `dd` | Betroffene Menschen können die Funktion gar nicht nutzen. | |
| `assessment.value.notApplicable.explain` | `dd` | Das Produkt hat die Funktion nicht, um die es im Kriterium geht, zum Beispiel keine Videos. Das ist weder ein Mangel noch ein Pluspunkt. | ersetzt `assessment.value.notApplicable.hint` |
| `assessment.value.unknown.explain` | `dd` | Zu diesem Kriterium gibt es noch keine Bewertung. Das heißt nicht, dass es nicht erfüllt ist. | |
| `assessment.legend.footnote` | Fußzeile | Welche Stufen bei einem Kriterium möglich sind, steht in seinen Details. Die Stufen stammen aus den Prüfverfahren von BIT inklusiv. ProcureAccess bildet daraus kein Gesamturteil. | |
| `candidate.assessment.note` | Label | Worauf stützt sich Ihre Einschätzung? (optional) | |
| `candidate.assessment.saved` | Ansage | Einschätzung gespeichert. | automatisches Speichern; Ansage gedrosselt |
| `candidate.assessment.unionNote` | Hinweis über der Liste (nur bei mehreren Anwendungstypen mit unterschiedlichen Prüfverfahren) | Ihre Liste gilt für mehrere Anwendungstypen. Bei jedem Kriterium stehen alle Stufen zur Auswahl, die für mindestens einen davon vorgesehen sind. | U35; E-B |
| **Kriterien „Geprüft nach …“** (`assessment.procedure.`) | | | U34, U46: Anzeige, keine Eingabe |
| `assessment.procedure.derived` | Zeile in S34b statt K6 | Geprüft nach {{testType}}: {{value}} | `value` = `.yes` / `.no` / `.notStated`; eine Zeile je gewähltem Prüfverfahren, in Katalogreihenfolge; Titel im Stil des Kriteriumstitels von K6, Wert in eigener Zeile (U69) |
| `assessment.procedure.derived.yes` / `.no` / `.notStated` | Wert | Ja / Nein / Nicht angegeben | Ja: Nachweis mit diesem Verfahren vorhanden; Nein: Prüfstatus „Nein“; sonst Nicht angegeben |
| `assessment.procedure.derived.hint` | Text | Ergibt sich aus dem Prüfnachweis. Zählt im Abgleich mit Kriterien nicht mit. | |
| `assessment.procedure.derived.edit` | Textbutton (Link-Semantik) | Prüfnachweis bearbeiten | U69; → S34a; Zielgröße 48 px; Verfahren per `aria-describedby` |
| `candidate.publish.cta` | Button (Kopfbereich) | Im Produktverzeichnis veröffentlichen | → S35; bleibt immer aktiv (7f V4) |
| `candidate.publish.cta.hint` | Hint unter dem Button in S34, S34a und S34b (nur solange Prüfstatus „Nicht bekannt“, nicht bei „Ja“ oder „Nein“) | Erst möglich, wenn angegeben ist, ob das Produkt geprüft wurde. | 7e; per `aria-describedby` am Button |
| `candidate.published.status` | Status | Veröffentlicht am {{date}}. Spätere Änderungen hier werden nicht automatisch übernommen. | ADR-004 |
| `candidate.published.changed` | Hinweis | Sie haben seit der Veröffentlichung Änderungen vorgenommen. Möchten Sie den Eintrag aktualisieren? | |
| `candidate.published.republish` | Button | Eintrag aktualisieren | → S35; nur für das Beitragsprofil, unter dem veröffentlicht wurde (D3, PUB-15) |
| `candidate.published.otherProfile` | Hinweis statt `candidate.published.republish` (Eintrag unter einem anderen Beitragsprofil veröffentlicht) | Dieser Eintrag wurde unter dem Beitragsprofil „{{displayName}}“ veröffentlicht. Aktualisieren kann ihn nur, wer dieses Profil nutzt. Sie können die Veröffentlichung zurückziehen und den Eintrag danach unter Ihrem Profil neu veröffentlichen. | U64; D3, PUB-15; Button `candidate.published.withdraw` bleibt |
| `candidate.published.withdraw` | Button | Veröffentlichung zurückziehen | → S36 |
| `candidate.remove` | Button (S34 Überblick, Rollen Verwalten und Bearbeiten) | Produkt aus dem Projekt entfernen | U76; SPEC_v2 SD21 |
| `candidate.remove.title` | Dialog-Titel | Produkt entfernen? | |
| `candidate.remove.body` | Text | „{{product}}“ wird mit Prüfnachweisen und Einschätzungen aus diesem Projekt entfernt. Das lässt sich nicht rückgängig machen. | |
| `candidate.remove.publishedNote` | Zusatz (nur wenn veröffentlicht) | Der veröffentlichte Eintrag bleibt im Produktverzeichnis sichtbar. Zurückziehen kann ihn danach nur, wer das Beitragsprofil „{{displayName}}“ nutzt. | WSP-04 |
| `candidate.remove.confirm` | Button | Produkt entfernen | |
| `candidate.removed` | Ansage | „{{product}}“ wurde aus dem Projekt entfernt. | Fokus auf H1 von S32 |
| `candidate.duplicate` | Fehler | Dieses Produkt ist schon in diesem Projekt. | R-CAN-01 |
| `projects.filter.archived` | Checkbox (S30) | Archivierte Projekte anzeigen | U76 |
| `projects.archived.note` | Hinweis (S32, S34) | Dieses Projekt ist archiviert. Sie können es ansehen, aber nicht mehr bearbeiten. Um es wieder zu bearbeiten, setzen Sie den Stand auf „Laufend“. | R-PRJ-05 |

### 5.9 S35–S37 Veröffentlichen aus dem Projekt (`publish.`)

Stepper mit vier Schritten. Immer als unabhängige Angabe (E9). Solange der Prüfstatus „Nicht bekannt“ ist, erscheint statt des Steppers der Hinweis `publish.blocked.testStatus`.

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `publish.title` | H1 | Eintrag veröffentlichen | |
| `publish.intro` | Text | Wenn Sie Ihre Angaben zu diesem Produkt veröffentlichen, helfen Sie anderen Organisationen bei ihrer Beschaffung. Die Veröffentlichung ist freiwillig. | |
| `publish.blocked.title` | Titel des Hinweises | Prüfstatus fehlt noch | 7e; `pa-notice` Achtung |
| `publish.blocked.testStatus` | Hinweis vor Schritt 1 (statt Stepper, solange „Nicht bekannt“ gewählt ist) | Bevor Sie veröffentlichen, geben Sie bitte an, ob das Produkt auf Barrierefreiheit geprüft wurde. Im Produktverzeichnis wird diese Angabe immer angezeigt, deshalb ist „Nicht bekannt“ dort nicht möglich. | E8, SIG-10; Link `publish.blocked.testStatus.cta` |
| `publish.blocked.testStatus.cta` | Button | Zum Prüfnachweis | → S34, Tab „Prüfnachweis“, Fokus auf die Radio Group (in Figma als Button, 7e) |
| `publish.blocked.noCriteria` | Hinweis vor Schritt 1 (statt Stepper, solange dem Projekt keine Kriterienliste zugeordnet ist) | Bevor Sie veröffentlichen, ordnen Sie dem Projekt eine Kriterienliste zu. Der Eintrag zeigt dann, wie das Produkt diese Kriterien erfüllt. | U63; D2, PUB-14; Button `projects.detail.noCriteria.cta` |
| `publish.blocked.nextSteps` | Text unter dem Button | Danach folgen: Produkt zuordnen, Inhalt prüfen, Namen wählen, bestätigen. | 7e; ersetzt den deaktivierten Stepper |
| `publish.error.testStatus` | Feldfehler (Server, falls der Status sich zwischenzeitlich geändert hat) | Bitte wählen Sie Ja oder Nein. „Nicht bekannt“ kann nicht veröffentlicht werden. | SIG-10; Problem Details auf `testStatus`, an der Radio Group in S34a angezeigt |
| `publish.step1.name` | Schritt | Produkt zuordnen | |
| `publish.step1.question` | `legend` | Welches Produkt im Verzeichnis ist gemeint? | |
| `publish.step1.matches` | Text | Diese Produkte im Verzeichnis haben einen ähnlichen Namen: | Trigram-Treffer (PUB-07) |
| `publish.step1.newProduct` | Option | Keines davon – „{{name}}“ neu ins Verzeichnis aufnehmen | |
| `publish.step2.name` | Schritt | Inhalt prüfen | |
| `publish.step2.shown` | H2 | Das wird veröffentlicht | |
| `publish.step2.shownList` | Liste | Produktname und Anbieter · ob und wie das Produkt geprüft wurde · Ihre Einschätzungen je Kriterium mit Begründung · Katalogversion und Datum | als echte Liste |
| `publish.step2.hidden` | H2 | Das wird nicht veröffentlicht | |
| `publish.step2.hiddenList` | Liste | Ansprechpersonen und Kontaktdaten · interne Notizen · Ihre Notiz zur Qualifikation der prüfenden Stelle · der Name Ihrer Organisation | PRV-01, PRV-03 |
| `publish.step2.editAssessments` | Text-Button | Einschätzungen und Begründungen bearbeiten | U14; → S34b, Tab „Einschätzung nach Kriterien“ |
| `publish.backToPublish` | Text-Button in S34b (nur wenn von S35 kommend) | Zurück zum Veröffentlichen | führt zu Schritt 2, Eingaben bleiben erhalten |
| `publish.step2.checkNotes` | Hinweis | Bitte prüfen Sie die Begründungen Ihrer Einschätzungen: Sie werden veröffentlicht. Formulieren Sie sachlich und nennen Sie überprüfbare Tatsachen. | PRD §7 „faktenbasiert“ |
| `publish.step3.name` | Schritt | Unter welchem Namen? | |
| `publish.step3.question` | `legend` | Unter welchem Beitragsprofil soll der Eintrag erscheinen? | |
| `publish.step3.hint` | Hint | Angezeigt wird nur der Anzeigename des Profils, nicht Ihr Name und nicht Ihre Organisation. | beim Aktualisieren ist das Profil der ersten Veröffentlichung fest (D3) |
| `publish.step3.createProfile` | Button | Neues Beitragsprofil anlegen | → S50 im Dialog |
| `publish.step3.sourceNote` | Hinweis | Ihr Eintrag wird als „Unabhängige Angaben“ gekennzeichnet. | E9, PUB-09 |
| `publish.step4.name` | Schritt | Bestätigen | |
| `publish.step4.confirm` | Checkbox | Ich bestätige, dass meine Angaben nach bestem Wissen zutreffen. | |
| `publish.step4.snapshotNote` | Text | Veröffentlicht wird der jetzige Stand. Spätere Änderungen in Ihrem Projekt werden erst übernommen, wenn Sie den Eintrag aktualisieren. Sie können die Veröffentlichung jederzeit zurückziehen – hier im Projekt oder unter „Meine Beiträge“. | Rückzug über den Verzeichnis-Endpunkt (ARCHITECTURE_v11 §7.2) |
| `publish.submit` | Button | Eintrag veröffentlichen | `clientRequestId` je Formular, doppeltes Absenden erzeugt keinen zweiten Eintrag (PUB-06) |
| `publish.success.title` | H1 (Bestätigung) | Eintrag veröffentlicht | |
| `publish.success.text` | Text | Vielen Dank. Ihr Eintrag ist jetzt im Produktverzeichnis sichtbar. | |
| `publish.success.view` | Button | Eintrag ansehen | |
| `publish.withdraw.title` | Dialog-Titel (S36) | Veröffentlichung zurückziehen? | |
| `publish.withdraw.body` | Text | Der Eintrag ist danach im Produktverzeichnis nicht mehr sichtbar. Ihre Angaben im Projekt bleiben erhalten. | |
| `publish.withdraw.confirm` | Button | Veröffentlichung zurückziehen | |
| `publish.withdraw.keep` | Button | Eintrag behalten | |
| `publish.withdraw.done` | Ansage | Die Veröffentlichung wurde zurückgezogen. | |
| `conflict.title` | Dialog-Titel (S37) | Inzwischen geändert | API-03 |
| `conflict.body` | Text | {{name}} hat diesen Inhalt geändert, während Sie ihn bearbeitet haben. Ihre Änderungen sind noch nicht gespeichert. | |
| `conflict.reload` | Button | Neue Fassung laden | verwirft eigene Änderungen – im Text sagen |
| `conflict.reload.hint` | Hinweis im Dialog (`pa-notice` Achtung) | Wenn Sie „Neue Fassung laden“ wählen, gehen Ihre Änderungen verloren. Kopieren Sie sie vorher, wenn Sie sie behalten möchten. | Wortlaut an Figma S37 angepasst |
| `conflict.copy` | Button (erster Fokus) | Meine Änderungen kopieren | kopiert die eigenen Eingaben als Text; Dialog bleibt offen |
| `conflict.copied` | Ansage | Ihre Änderungen wurden kopiert. | U16 |

### 5.10 S40–S42 Produktverzeichnis (`directory.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `directory.title` | H1 | Produktverzeichnis | |
| `directory.intro` | Text | Hier finden Sie Angaben zur Barrierefreiheit von Produkten – veröffentlicht von Anbietern und Prüfstellen sowie von Organisationen und Menschen, die die Produkte nutzen. ProcureAccess selbst bewertet keine Produkte. | |
| `directory.search` | Label | Produkt oder Anbieter suchen | |
| `directory.filter.useCase` | Label (durchsuchbares Feld, `mat-autocomplete`, Einfachauswahl) | Verwendungszweck | erster Filter (Reihenfolge der Schichten); leer = alle; Löschen-Symbol setzt zurück |
| `directory.filter.useCase.hint` | Supporting text (nur bei Fokus) | Tippen, um zu filtern | |
| `directory.filter.appType` | Label | Anwendungstyp | zweiter Filter; Wert ohne Auswahl: „Alle Anwendungstypen“ (`directory.filter.appType.all`); Ausschlussregeln wie in S10; Supporting text `common.multiSelect.hint` |
| `directory.filter.appType.all` | Wert ohne Auswahl | Alle Anwendungstypen | U41 |
| `directory.filter.toggle` | Button (Disclosure, unter 840 px Containerbreite) | Filter und Sortierung ({{count}} aktiv) | U31; `aria-expanded`/`aria-controls` |
| `directory.filter.toggle.none` | Button ohne aktive Filter | Filter und Sortierung | U41 |
| `directory.filter.hint.mobile` | Hinweis im Filterbereich | Anwendungstyp und Prüfverfahren: Mehrfachauswahl. Filter wirken sofort; die Trefferzahl unten wird angesagt. | U31 |
| `directory.filter.reset` | Ansage | Der Filter {{filter}} wurde zurückgesetzt, weil er nicht zu {{higher}} passt. | wenn ein höherer Filter einen tieferen ungültig macht |
| `directory.filter.testType.all` | Wert ohne Auswahl | Alle Prüfverfahren | U41 |
| `directory.filter.testType` | Label | Prüfverfahren | Wert ohne Auswahl: `directory.filter.testType.all`; Werte `testType.*` inklusive „Andere Prüfung“ und „Eigene Prüfung ohne festes Verfahren“; Supporting text `common.multiSelect.hint` |
| `directory.filter.source` | `legend` | Herkunft der Angaben | |
| `directory.filter.source.vendor` | Option | Angaben der Anbieterseite | |
| `directory.filter.source.independent` | Option | Unabhängige Angaben | |
| `directory.filter.hasReport` | Option | Mit Konformitätsbericht | |
| `directory.results` | Text über der Ergebnisliste und Ansage | {{count}} Produkte gefunden. | sichtbar und als Live-Region (polite, gedrosselt) |
| `directory.results.useCaseNote` | Erster Absatz des Hinweises über der Ergebnisliste (`pa-notice` Info, nur bei gewähltem Verwendungszweck) | Gezeigt werden Produkte mit mindestens einem Eintrag für den Verwendungszweck „{{useCase}}“. Einträge ohne Angabe zum Verwendungszweck sind dabei nicht berücksichtigt. | U36, U58; DIR-01; danach im **selben** Hinweis als zweiter Absatz `directory.results.orderNote` (S40-V) |
| `directory.results.orderNote` | Hinweis über der Ergebnisliste (`pa-notice` Info); bei gewähltem Verwendungszweck zweiter Absatz nach `directory.results.useCaseNote` | Die Reihenfolge richtet sich nur nach Name, Aktualität oder Anzahl der Einträge. Sie sagt nichts darüber aus, wie barrierefrei ein Produkt ist. Ein Gesamturteil gibt es bewusst nicht. | U13; D3, G7 |
| **Ergebnis `pa-product-result`** | | | nur Fakten, keine Bewertung |
| `directory.result.vendorLine` | Zeile | Anbieter: {{vendor}} · {{appTypes}} | Anwendungstypen aus den Einträgen, mit Komma getrennt; ohne Angabe nur „Anbieter: {{vendor}}“ |
| `directory.result.useCaseLine` | Zeile (nur bei gewähltem Verwendungszweck) | {{count}} Eintrag für „{{useCase}}“ / {{count}} Einträge für „{{useCase}}“ | U36; Figma S40 |
| `directory.result.vendorEntries` | Teil der Einträge-Zeile | {{count}} Eintrag der Anbieterseite / {{count}} Einträge der Anbieterseite / Keine Einträge der Anbieterseite | Einzahl, Mehrzahl, 0 (O9) |
| `directory.result.independentEntries` | Teil der Einträge-Zeile | {{count}} unabhängiger Eintrag / {{count}} unabhängige Einträge / keine unabhängigen Einträge | |
| `directory.result.reports` | Teil der Einträge-Zeile | {{count}} Erfahrungsbericht / {{count}} Erfahrungsberichte | bei 0 weglassen |
| `directory.result.updated` | Zeile | Zuletzt veröffentlicht am {{date}} | |
| `directory.sort.label` | Label | Sortieren nach | Nur Name, Aktualität, Anzahl Einträge – **nie** nach Abgleich (D3, G7) |
| `directory.sort.name` / `.recent` / `.entries` | Optionen | Name / Zuletzt veröffentlicht / Anzahl der Einträge | auf S41 Vorauswahl „Zuletzt veröffentlicht“ |
| `directory.sort.updated` | Ansage | Sortiert nach: {{sort}}. | U9 |
| `directory.empty` | Leerzustand | Kein Produkt gefunden. Prüfen Sie die Schreibweise oder entfernen Sie Filter. Wenn das Produkt noch nicht im Verzeichnis ist, können Sie es zur Aufnahme vorschlagen. | |
| `directory.empty.suggest` | Button | Produkt vorschlagen | → S52 |
| `directory.empty.werteit` | Hinweis (P1) | Sie brauchen Unterstützung bei der Produktauswahl? Das Projekt WERTE.IT berät Sie. | |
| **S41 Produktseite** | | | |
| `directory.product.vendor` | Label | Anbieter: {{vendor}} | |
| `directory.product.website` | Link | Webseite des Anbieters | öffnet im selben Tab (7f R3) |
| `directory.product.copyLink` | Button | Link zur Produktseite kopieren | U77; „Teilen“ (R-ENT-10) |
| `directory.product.linkCopied` | Ansage | Link kopiert. | |
| `directory.product.addToProject` | Button (angemeldet, Rollen Verwalten und Bearbeiten) | Zu einem Projekt hinzufügen | „Merken“ (R-ENT-11) |
| `directory.product.addToProject.title` | Dialog-Titel | Zu welchem Projekt hinzufügen? | Liste der laufenden Projekte |
| `directory.product.addToProject.done` | Ansage | „{{product}}“ wurde zum Projekt „{{project}}“ hinzugefügt. | |
| `directory.product.compare.projectCopy` | Option im Vergleich | Kriterienliste des Projekts „{{project}}“ | R-ENT-04 |
| `directory.product.entryCount` | Text | {{vendorCount}} Einträge der Anbieterseite, {{independentCount}} unabhängige Einträge, {{reportCount}} Erfahrungsberichte | E13: Berichte getrennt gezählt; Einzahl/Mehrzahl je Teil (O9) |
| `directory.product.noVerdict` | Hinweis | Die Einträge zeigen, was Anbieter, Prüfstellen und Nutzende veröffentlicht haben. Ein Gesamturteil gibt es bewusst nicht: Ob ein Produkt für Sie geeignet ist, hängt von Ihren Kriterien ab. | G7, erklärt fehlenden Score |
| `directory.product.compare.label` | Label (nur angemeldet mit Organisation) | Mit welcher Kriterienliste vergleichen? | Select; Auswahl ruft konfigurationsbezogene Variante auf |
| `directory.product.compare.none` | Option | Keine – Angaben so anzeigen, wie sie veröffentlicht wurden | öffentliche Variante |
| `directory.product.compare.updated` | Ansage | Abgleich aktualisiert: {{name}}. | U9; bei „Keine“: „Abgleich ausgeschaltet.“ (`directory.product.compare.off`) |
| `directory.product.compare.off` | Ansage | Abgleich ausgeschaltet. | |
| `directory.product.compare.login` | Hinweis (nicht angemeldet) | Melden Sie sich an, um die Einträge mit Ihrer eigenen Kriterienliste abzugleichen. | |
| `directory.product.tabs.vendor` | Abschnitt H2 | Angaben der Anbieterseite | Kanäle **getrennt** (PRD §7) |
| `directory.product.tabs.independent` | Abschnitt H2 | Unabhängige Angaben | |
| `directory.product.vendor.empty` | Leerzustand | Die Anbieterseite hat zu diesem Produkt noch keine Angaben veröffentlicht. | |
| `directory.product.independent.empty` | Leerzustand | Zu diesem Produkt gibt es noch keine unabhängigen Angaben. Nutzen Sie es selbst? Schreiben Sie einen Erfahrungsbericht. | |
| `directory.product.writeReport` | Button | Erfahrungsbericht schreiben | → S51 |
| `directory.product.reports.title` | H3 | Erfahrungsberichte | |
| `directory.product.reports.note` | Text | Jeder Bericht gibt die Erfahrung einer einzelnen Person wieder. Die Berichte werden nicht zusammengefasst. | PRD §4: keine Verallgemeinerung |
| `contribute.report.typeNote` | Zusatz in der Berichtskarte (statt K4) | Erfahrungsbericht – beschreibt die eigene Nutzung, keine Prüfung auf Barrierefreiheit. | E13 |
| `directory.product.reports.assessment` | Button in der Berichtskarte (nur wenn Kriterien eingeschätzt wurden) | Eingeschätzte Kriterien anzeigen ({{count}}) | U37: Disclosure mit einfacher Liste, **kein K1**, keine Zählwerte (E-D) |
| `directory.product.reports.assessment.list` | Liste (`<dl>`) | {{criterion}}: {{value}} | Wert = `assessment.value.*` (nur Anforderungskriterien); Begründung darunter, wenn vorhanden |
| `directory.entry.context` | Zeile in Eintragskarte, Berichtskarte und S42 | Anwendungstyp: {{appTypes}} · Verwendungszweck: {{useCases}} | U36; fehlende Teile weglassen; ohne Angaben ganz weglassen; Erfahrungsberichte haben nur den Anwendungstyp (D5) |
| `directory.entry.context.vendorDeclared` | Zusatz hinter der Zeile | (vom Anbieter angegeben) | U36; nur bei Einträgen der Anbieterseite (Flow E) |
| `directory.entry.context.project` | Zusatz hinter der Zeile | (aus der Kriterienliste des Beschaffungsprojekts) | U36; Flow B |
| `directory.entry.context.contributorDeclared` | Zusatz hinter der Zeile | (von der beitragenden Person angegeben) | U69; Erfahrungsberichte (Flow D, D5) |
| `directory.entry.stale` | Hinweis | Dieser Eintrag ist {{months}} Monate alt. Die Software kann sich seitdem verändert haben. | ab Schwelle, z. B. 18 Monate |
| **S42 Eintrag – Detail** | | | U15 |
| `directory.entry.title` | H1 | Eintrag von {{displayName}} zu {{product}} | |
| `directory.entry.breadcrumb` | Letztes Breadcrumb-Glied | Eintrag von {{displayName}} | |
| `directory.entry.assessments.title` | H2 | Einschätzung je Kriterium | |
| `directory.entry.assessments.intro` | Text, zugleich `<caption>` | Eingeschätzt nach Katalogversion {{release}}. Die Begründungen stammen von der veröffentlichenden Stelle. | |
| `directory.entry.assessments.filter` | Label Select | Anzeigen | |
| `directory.entry.assessments.filter.all` | Option | Alle Einschätzungen | weitere Optionen = `assessment.value.*`, mit Vergleichsliste zusätzlich `signals.fulfillment.notCovered` |
| `directory.entry.assessments.col.criterion` / `.col.assessment` / `.col.rationale` | Spaltenköpfe | Kriterium / Einschätzung / Begründung | leere Begründung = leere Zelle |
| `directory.entry.notCovered.rationale` | Zelle „Begründung“ bei „Damals nicht im Katalog“ | Das Kriterium kam nach der Veröffentlichung in den Katalog. Das ist kein Mangel des Produkts. | nur mit Vergleichsliste |
| `directory.entry.previousVersion` | Text-Button | Frühere Fassung vom {{date}} ansehen | nur wenn Versionen existieren (PUB-05) |
| `directory.entry.olderVersion` | Hinweis (`pa-notice` Info) bei einer früheren Fassung | Sie sehen eine frühere Fassung vom {{date}}. Die aktuelle Fassung ist vom {{currentDate}}. | U77; R-ENT-09 |
| `directory.entry.olderVersion.current` | Link | Aktuelle Fassung ansehen | |
| `directory.entry.view` | Link | Eintrag ansehen | Linktext mit Kontext: visuell ausgeblendet „von {{displayName}}, {{date}}“ |
| **Eintragszeile `pa-entry-row`** (7e) | | | Kurzfassung eines Eintrags; die Langtexte aus §5.11 erscheinen in der aufgeklappten Karte |
| `directory.entry.row.tested` | Spaltenlabel | Geprüft | Wert `common.yes` / `common.no` |
| `directory.entry.row.documentation` | Spaltenlabel | Nachweise | Wert: Kurzform von `signals.documentation.report.yes` („VPAT 2.5 EU vom 01.03.2026“) bzw. „Kein Konformitätsbericht“ |
| `directory.entry.row.matching.config` | Spaltenlabel | Abgleich mit „{{name}}“ | |
| `directory.entry.row.matching.public` | Spaltenlabel | Eingeschätzte Kriterien | ohne Vergleichsliste |
| `directory.entry.row.fulfilled` | Zählung | Erfüllt: {{count}} | U54; Reihenfolge fest (SIG-08); als Liste ausgezeichnet, Trenner „·“ nur per CSS; jedes Element ohne Umbruch (`white-space: nowrap`) |
| `directory.entry.row.slightRestriction` | Zählung | Leichte Einschränkung: {{count}} | |
| `directory.entry.row.restriction` | Zählung | Einschränkung: {{count}} | |
| `directory.entry.row.barrier` | Zählung | Barriere: {{count}} | |
| `directory.entry.row.blockade` | Zählung | Blockade: {{count}} | |
| `directory.entry.row.notApplicable` | Zählung | Nicht anwendbar: {{count}} | |
| `directory.entry.row.unknown` | Zählung | Nicht eingeschätzt: {{count}} | |
| `directory.entry.row.notCovered` | Zählung, abgesetzt | Damals nicht im Katalog: {{count}} | immer anzeigen, auch bei 0; Erklärung in der aufgeklappten Karte. Ohne Einschätzungen (D2) statt aller Zählungen `signals.fulfillment.none` |
| `directory.entry.row.showDetails` | Button | Details anzeigen | `aria-expanded`; Quelle per `aria-describedby` |
| `directory.entry.row.hideDetails` | Button | Details ausblenden | |

### 5.11 Fachliche Komponenten K1–K4 (`signals.`)

Diese Texte gelten überall, wo die Komponenten eingesetzt werden (S41, S42, S53, Vorschau in S35 und S55).

**K3 Herkunft**

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `signals.source.vendor` | Kennzeichnung | Angaben der Anbieterseite | |
| `signals.source.independent` | Kennzeichnung | Unabhängige Angaben | |
| `signals.source.by` | Zusatz | veröffentlicht von {{displayName}} | |
| `signals.source.kind.vendor` | Zusatz | Anbieter | Profilart |
| `signals.source.kind.auditor` | Zusatz | Prüfstelle im Auftrag | |
| `signals.source.kind.institution` | Zusatz | Organisation | |
| `signals.source.kind.individual` | Zusatz | Einzelperson | |
| `signals.source.info` | Button | Was bedeutet das? | Expansion Panel |
| `signals.source.vendor.explain` | Text | Diese Angaben stammen vom Anbieter selbst oder von einer Prüfstelle, die er beauftragt hat. Anbieter veröffentlichen freiwillig – meist dann, wenn das Ergebnis gut ausfällt. Nutzen Sie diese Angaben als Hinweis, nicht als unabhängiges Urteil. | PRD §7: strukturell positiv verzerrt; Ton sachlich, nicht abwertend |
| `signals.source.independent.explain` | Text | Diese Angaben stammen von Organisationen oder Personen, die die Software selbst beschaffen oder nutzen. Der Anbieter hat sie nicht freigegeben. Sie können auch kritische Erfahrungen enthalten. | |

**K4 Geprüft ja/nein**

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `signals.tested.label` | Label | Auf Barrierefreiheit geprüft | |
| `signals.tested.yes` | Wert | Ja | |
| `signals.tested.no` | Wert | Nein | immer anzeigen (SIG-03, SIG-11); nicht bei Erfahrungsberichten (SIG-12) |
| `signals.tested.evidence.item` | Zeile je Prüfnachweis (Liste `<ul>` unter „Ja“) | {{testType}} – {{testedBy}}, {{date}} | U61; ersetzt `signals.tested.details`; fehlende Teile samt Trennzeichen weglassen; bei „Andere Prüfung“ der eingegebene Name. Beispiel: „BIT inklusiv Software-Test – Prüfstelle Beispiel, 14.03.2026“ |
| `signals.tested.no.explain` | Zusatz | Zu diesem Eintrag wurde keine Prüfung angegeben. | |

**K2 Nachweise**

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `signals.documentation.title` | H3 / Name der Region | Welche Nachweise gibt es? | eigene Region (SIG-09) |
| `signals.documentation.report.yes` | Wert | Konformitätsbericht vorhanden: {{format}} vom {{date}} | z. B. „VPAT 2.5 EU“ |
| `signals.documentation.report.no` | Wert | Kein Konformitätsbericht angegeben | |
| `signals.documentation.age` | Zusatz | {{months}} Monate alt | |
| `signals.documentation.testType` | Wert | Art der Prüfung: {{testTypes}} | U61; alle Prüfnachweise des Eintrags, mit Komma getrennt; ohne Prüfnachweis weglassen |
| `signals.documentation.report.open` | Link | Konformitätsbericht öffnen (Datei, {{size}}) | P1; Dateiformat im Linktext |
| `signals.documentation.explain.toggle` | Link (Disclosure) | Was sagt ein Konformitätsbericht aus? | U32; Link statt Text-Button (bricht um), `aria-expanded` |
| `signals.documentation.explain` | Text (eingeklappt) | Ein Konformitätsbericht (VPAT oder ACR) ist eine strukturierte Selbstauskunft des Anbieters, welche Anforderungen das Produkt erfüllt. Er sagt etwas darüber, wie gut ein Produkt dokumentiert ist – nicht darüber, wie gut es Ihre Anforderungen erfüllt. | erklärt die Trennung der zwei Signale |

**Werte für Prüfverfahren und Art der Prüfung (`testType.`)**

Gemeinsame Liste für S10 Schritt 4, S34a, S55 Schritt 2, Filter S40, K2 und K4. Die ersten fünf Werte kommen aus dem Katalog (`CriteriaFilter`, FilterTypeId 4); die letzten beiden gibt es nur bei „Art der Prüfung“ und im Filter, nicht in S10.

| Schlüssel | Text | Hinweis |
|---|---|---|
| `testType.softwareTest` | BIT inklusiv Software-Test | Link softwaretest.bit-inklusiv.de |
| `testType.appTest` | BIT inklusiv App-Test | apptest.bit-inklusiv.de |
| `testType.pdfTest` | BIT inklusiv PDF-Test | pdftest.bit-inklusiv.de |
| `testType.bikWeb` | BIK BITV-Test (Web) | bitvtest.de |
| `testType.bikApp` | BIK BITV-Test (App) | bitvtest.de |
| `testType.other` | Andere Prüfung | zweite Zeile `testType.other.hint`; in K2/K4 wird der eingegebene Name angezeigt |
| `testType.other.hint` | Sie geben den Namen des Verfahrens an. | U41 |
| `testType.own` | Eigene Prüfung ohne festes Verfahren | |

**K1 Abgleich mit Kriterien**

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `signals.fulfillment.title.config` | H3 / Name der Region | Abgleich mit Ihrer Kriterienliste „{{name}}“ | konfigurationsbezogene Variante |
| `signals.fulfillment.title.public` | H3 / Name der Region | Eingeschätzte Kriterien | öffentliche Variante |
| `signals.fulfillment.intro.config` | Text | So wurden die Kriterien Ihrer Liste in diesem Eintrag eingeschätzt: | |
| `signals.fulfillment.intro.public` | Text | So wurden die Kriterien in diesem Eintrag eingeschätzt: | |
| `signals.fulfillment.fulfilled` | Begriff + Wert | Erfüllt: {{count}} Kriterien | Reihenfolge fest (SIG-08) |
| `signals.fulfillment.slightRestriction` | | Leichte Einschränkung: {{count}} Kriterien | |
| `signals.fulfillment.restriction` | | Einschränkung: {{count}} Kriterien | |
| `signals.fulfillment.barrier` | | Barriere: {{count}} Kriterien | |
| `signals.fulfillment.blockade` | | Blockade: {{count}} Kriterien | |
| `signals.fulfillment.notApplicable` | | Nicht anwendbar: {{count}} Kriterien | |
| `signals.fulfillment.unknown` | | Nicht eingeschätzt: {{count}} Kriterien | |
| `signals.fulfillment.notCovered` | | Damals nicht im Katalog: {{count}} Kriterien | eigene Zeile, abgesetzt (CAT-04); nur im Produktverzeichnis, nicht im Projekt (U38) |
| `signals.fulfillment.notCovered.explain` | Erklärung (direkt sichtbar, wenn Wert > 0) | Diese Kriterien gab es noch nicht, als der Eintrag veröffentlicht wurde. Sie konnten deshalb nicht eingeschätzt werden. Das ist kein Mangel des Produkts. | „nicht Teil des damaligen Katalogs“ (ARCH §5.1) |
| `signals.fulfillment.unknown.explain` | Erklärung (eingeklappt) | Zu diesen Kriterien hat die veröffentlichende Stelle keine Einschätzung abgegeben. Das heißt nicht, dass das Produkt sie nicht erfüllt. | |
| `signals.fulfillment.details` | Button | Einschätzung je Kriterium anzeigen | aufklappbare Tabelle Kriterium – Einschätzung – Begründung |
| `signals.fulfillment.release` | Zusatz | Eingeschätzt nach Katalogversion {{entryRelease}}. Ihre Liste nutzt Version {{configRelease}}. | nur wenn unterschiedlich |
| ~~`signals.fulfillment.reportOnly`~~ | – | entfällt (U37) | Erfahrungsberichte haben kein K1 |
| `signals.fulfillment.none` | Text statt K1 (Eintrag ohne Einschätzungen einzelner Kriterien) | Keine Einschätzungen einzelner Kriterien veröffentlicht. | U62; D2, PUB-14: Region und Überschrift bleiben, statt der Zählungen steht dieser Text; ebenso in der Eintragszeile |

### 5.12 S50–S53 Beitragen (`contribute.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `contribute.title` | H1 (S53) | Beitragen | |
| `contribute.intro` | Text | Ihre Erfahrungen helfen Organisationen, Software auszuwählen, mit der alle arbeiten können. Sie entscheiden, was Sie teilen. | Mitgestaltung, keine Pflicht |
| `contribute.mine` | H2 | Meine Beiträge | |
| `contribute.mine.empty` | Leerzustand | Sie haben noch nichts veröffentlicht. Wählen Sie ein Produkt im Verzeichnis und schreiben Sie dort einen Erfahrungsbericht. | |
| `contribute.mine.cta` | Button | Produkt suchen | → S40 |
| `contribute.mine.vendorCta` | Text-Link | Sie bieten Software an oder prüfen sie im Auftrag? Prüfergebnis veröffentlichen | → S55 |
| `contribute.mine.item.title` | H3 im Listeneintrag | {{product}} – {{kind}} | U16 (vorher ein Satz); `kind`: „Erfahrungsbericht“, „Prüfergebnis“ oder „Eintrag aus einem Beschaffungsprojekt“ |
| `contribute.mine.item.meta` | Zeile | Veröffentlicht am {{date}} unter „{{displayName}}“ | bei Profilart Anbieter/Prüfstelle Zusatz „({{kind}})“ aus `signals.source.kind.*` |
| `contribute.mine.item.version` | Zusatz | Version {{n}} | nur Prüfergebnisse mit mehr als einer Fassung |
| `contribute.mine.item.withdrawnAt` | Zusatz | zurückgezogen am {{date}} | |
| `contribute.mine.status` | Statuszeile | Status: {{state}} | `state` = `contribute.mine.state.*`; nicht nur Farbe |
| `contribute.mine.state.published` / `.restricted` / `.withdrawn` | Status | Sichtbar / Eingeschränkt / Zurückgezogen | Moderationsstatus (P1) |
| `contribute.mine.view` | Link | Eintrag ansehen | Linktext mit visuell ausgeblendetem Produktnamen; nicht bei zurückgezogenen Beiträgen |
| `contribute.mine.update` | Button (nur Prüfergebnisse) | Neues Prüfergebnis veröffentlichen | → S55, vorbelegt |
| `contribute.mine.withdraw` | Button | Veröffentlichung zurückziehen | Dialog wie S36 (`publish.withdraw.*`); TEN-10 |
| `contribute.mine.withdraw.reportNote` | Text im Dialog (Erfahrungsbericht) | Ihr Bericht ist danach im Produktverzeichnis nicht mehr sichtbar. Damit widerrufen Sie auch Ihre Einwilligung zur Veröffentlichung. Ihr Text und Ihre Angaben zu Hilfsmitteln werden gelöscht. Ihr Beitragsprofil bleibt bestehen. | U47, U65; D4, PRV-10; Zeitpunkt der Löschung legt die rechtliche Prüfung fest (vorläufig: sofort) |
| **S50 Beitragsprofil** | | | |
| `contribute.profile.title` | H1 | Beitragsprofil anlegen | |
| `contribute.profile.intro` | Text | Unter diesem Profil erscheinen Ihre Beiträge. Ihre E-Mail-Adresse, Ihr Name und Ihre Organisation werden nie angezeigt. | PRV-03/04 |
| `contribute.profile.name` | Label | Anzeigename | |
| `contribute.profile.name.hint` | Hint | Wenn Sie nicht erkannt werden möchten, wählen Sie einen Namen, der keine Rückschlüsse auf Sie zulässt – also nicht Ihren echten Namen oder Ihren Benutzernamen aus anderen Diensten. | Pseudonymität (PRD §4) |
| `contribute.profile.kind` | `legend` | Als wer schreiben Sie? | |
| `contribute.profile.kind.individual` | Option | Als Einzelperson | |
| `contribute.profile.kind.institution` | Option | Für eine Organisation | |
| `contribute.profile.kind.vendor` | Option | Für einen Anbieter | → S55 |
| `contribute.profile.kind.auditor` | Option | Für eine Prüfstelle | → S55 |
| `contribute.profile.submit` | Button | Profil anlegen | |
| **S51 Erfahrungsbericht** | | | |
| `contribute.report.title` | H1 | Erfahrungsbericht zu {{product}} | darunter `common.requiredNote` |
| `contribute.report.intro` | Text | Beschreiben Sie, wie gut Sie mit der Software arbeiten konnten. Ihr Bericht erscheint als einzelner Beitrag unter Ihrem Anzeigenamen und wird nicht mit anderen Berichten zusammengefasst. | PRD §4 |
| `contribute.report.profile` | Label | Veröffentlichen als | Select Beitragsprofil |
| `contribute.report.narrative` | Label | Ihre Erfahrung | |
| `contribute.report.narrative.error` | Feldfehler (leer) | Bitte beschreiben Sie Ihre Erfahrung. Ein oder zwei Sätze genügen. | U7; Figma S51-E |
| `contribute.report.narrative.hint` | Hint | Was hat gut funktioniert, was nicht? Beschreiben Sie konkrete Situationen, zum Beispiel: „Das Formular für Urlaubsanträge lässt sich nur mit der Maus absenden.“ Bitte keine Namen von anderen Personen. | faktenbasiert (PRD §7) |
| `contribute.report.at` | Label (sichtbar über dem Feld, auf allen Breiten) | Mit welchen Hilfsmitteln haben Sie die Software genutzt? (optional) | `AssistiveTechnologyContext`; per `aria-labelledby` am Feld; U59: Muster von S51-M auch auf dem Desktop |
| `contribute.report.at.short` | `mat-label` im Feld | Hilfsmittel | U31; nur visuell, der Name kommt aus dem Label darüber |
| `contribute.report.at.hint` | Hint | Zum Beispiel Screenreader, Bildschirmlupe, Spracheingabe oder nur Tastatur. Diese Angabe hilft anderen einzuschätzen, ob Ihre Erfahrung auf sie übertragbar ist. Eine Behinderung müssen Sie nicht nennen. | PRD P0: pro Beitrag ausweisbar, nicht aggregiert; keine Pflicht |
| `contribute.report.version` | Label (sichtbar über dem Feld) | Welche Version der Software haben Sie genutzt? (optional) | per `aria-labelledby` |
| `contribute.report.version.short` | `mat-label` im Feld | Version | U31 |
| `contribute.report.appType` | Label (sichtbar über dem Feld, Mehrfachauswahl, optional) | Wie haben Sie die Software genutzt? (optional) | U59; D5; Werte = Anwendungstypen, Supporting text `common.multiSelect.hint`; erscheint am Bericht als `directory.entry.context` mit `.contributorDeclared`; bestimmt bei Einschätzungen die Kriterien und die erlaubten Stufen (ohne Angabe alle sechs Stufen); Erfahrungsberichte haben keinen Verwendungszweck |
| `contribute.report.appType.short` | `mat-label` im Feld | Anwendungstyp | U59 |
| `contribute.report.assessment` | Button (optional) | Zusätzlich einzelne Kriterien einschätzen | öffnet K6-Liste mit den Kriterien der angegebenen Anwendungstypen; ohne Angabe die Kriterien aller Anwendungstypen mit allen sechs Stufen (D5) |
| `contribute.report.assessment.note` | Hinweis | Ihre Einschätzungen erscheinen als Liste bei Ihrem Bericht. Sie werden nicht gezählt und nicht mit anderen Berichten zusammengefasst. | U37 |
| `contribute.report.consent` | Checkbox (immer sichtbar, Pflicht) | ⚖️ Ich willige ein, dass mein Bericht mit allen Angaben veröffentlicht wird. Ich weiß, dass daraus Rückschlüsse auf meine Gesundheit möglich sind, zum Beispiel über die genannten Hilfsmittel. | U47; **Platzhalter**: Bericht und Hilfsmittel können Gesundheitsdaten nach Art. 9 DSGVO sein (ARCHITECTURE_v11 §4.5, PRV-09); Wortlaut legt die rechtliche Prüfung fest; die Version des Textes wird gespeichert |
| `contribute.report.consent.withdraw` | Hint an der Checkbox | Ihr Bericht ist öffentlich sichtbar. Sie können die Einwilligung jederzeit widerrufen, indem Sie den Bericht unter „Meine Beiträge“ zurückziehen. Text und Angaben zu Hilfsmitteln werden dann gelöscht. | U47, U59, U65; ersetzt in S51 den Hinweis `contribute.report.publicNote` |
| `contribute.report.consent.error` | Feldfehler | Bitte willigen Sie in die Veröffentlichung ein. Ohne Einwilligung können wir den Bericht nicht veröffentlichen. | U7, U47 |
| `contribute.report.submit` | Button | Erfahrungsbericht veröffentlichen | |
| `contribute.report.success` | Bestätigung | Vielen Dank für Ihren Bericht. Er ist jetzt beim Produkt {{product}} sichtbar. | |
| `contribute.report.rateLimited` | Fehler | Sie haben in kurzer Zeit sehr viele Beiträge veröffentlicht. Bitte warten Sie {{minutes}} Minuten und versuchen Sie es dann erneut. | API-05 |
| `contribute.report.noProfile` | Hinweis | Um einen Bericht zu veröffentlichen, legen Sie zuerst ein Beitragsprofil an. Das dauert eine Minute. | |
| **S52 Produkt vorschlagen** | | | |
| `contribute.productRequest.title` | H1 | Produkt zur Aufnahme vorschlagen | |
| `contribute.productRequest.intro` | Text | Das Produkt, zu dem Sie schreiben möchten, ist noch nicht im Verzeichnis? Schlagen Sie es vor. Das Team von ProcureAccess nimmt es auf, und Sie erhalten eine Nachricht, sobald Sie Ihren Bericht schreiben können. | ARCH §16.2.6: Wartezeit ehrlich ankündigen |
| `contribute.productRequest.name` | Label | Produktname | |
| `contribute.productRequest.vendor` | Label | Anbieter | |
| `contribute.productRequest.website` | Label | Webseite des Produkts (optional) | |
| `contribute.productRequest.submit` | Button | Vorschlag senden | |
| `contribute.productRequest.sent` | Bestätigung | Danke. Wir prüfen Ihren Vorschlag, in der Regel innerhalb von {{days}} Werktagen. | Zahl erst festlegen, wenn Ablauf steht |

### 5.13 S55 Prüfergebnis veröffentlichen – Anbieterseite (`vendor.`) – neu nach E3

Stepper mit fünf Schritten. ⚖️ = Text vorläufig, abhängig von der gebündelten rechtlichen Prüfung (E4). Jeder Schritt beginnt mit `common.requiredNote`, sofern er Pflichtfelder hat. Unter 600 px Breite ersetzt `vendor.stepIndicator` den waagerechten Stepper. Erledigte Schritte zeigen ein Häkchen (mit `common.stepper.done`), nur der aktuelle Schritt ist fett. In Schritt 1 ist die zweite Aktion „Abbrechen“ (`common.actions.cancel`), nicht „Zurück“ (U57).

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `vendor.title` | H1 | Prüfergebnis veröffentlichen | |
| `vendor.intro` | Text | Sie bieten Software an oder haben sie im Auftrag geprüft? Veröffentlichen Sie das Ergebnis im Produktverzeichnis. Beschaffende Stellen sehen dann, dass sich Ihr Produkt mit Barrierefreiheit befasst. | Reputationskanal |
| `vendor.intro.label` | Hinweis | Ihr Eintrag wird als „Angaben der Anbieterseite“ gekennzeichnet und getrennt von unabhängigen Angaben angezeigt. | Transparenz vorab |
| `vendor.noOrgNote` | Hinweis | Dafür brauchen Sie weder eine Organisation noch ein Beschaffungsprojekt. | E3 |
| `vendor.stepIndicator` | Text (schmale Bildschirme) | Schritt {{n}} von 5: {{step}} | Figma S55-M |
| `vendor.step1.name` | Schritt | Produkt | |
| `vendor.step1.search` | Label | Produktname | Autocomplete; erstes Feld (U57) |
| `vendor.step1.matches` | Text | Diese Produkte im Verzeichnis haben einen ähnlichen Namen: | Identitätsschritt wie Flow B (E3); direkt unter dem Produktnamen, mit Radio-Liste der Treffer und `vendor.step1.new` (U57) |
| `vendor.step1.new` | Option | Keines davon – „{{name}}“ neu ins Verzeichnis aufnehmen | letzte Option der Radio-Liste |
| `vendor.step1.vendorName` | Label | Anbieter | |
| `vendor.step1.website` | Label | Webseite des Produkts | |
| `vendor.step1.useCases` | Label (Mehrfachauswahl, optional, Chips) | Für welche Verwendungszwecke ist das Produkt gedacht? (optional) | U36, U57; durchsuchbar wie `directory.filter.useCase`; steht vor den Anwendungstypen, weil ein Verwendungszweck Anwendungstypen ausschließen kann (wie S10) |
| `vendor.step1.appTypes` | `legend` der Checkbox-Gruppe (Pflicht) | Für welche Anwendungstypen gilt das Prüfergebnis? | U36, U57; die Frage ist `legend` innerhalb der Feldgruppe; ausgeschlossene Typen bleiben sichtbar, `aria-disabled` mit Grund (`criteria.select.excluded.*`); Supporting text `common.multiSelect.hint` |
| `vendor.step1.appTypes.error` | Feldfehler | Bitte wählen Sie mindestens einen Anwendungstyp. | |
| `vendor.step1.context.hint` | Hinweis (`pa-notice` Info) am Ende von Schritt 1 | Beides erscheint am Eintrag mit dem Zusatz „vom Anbieter angegeben“. Beschaffende Stellen finden Ihr Produkt dann über die Filter im Produktverzeichnis. | U36, U57 |
| `vendor.step2.name` | Schritt | Prüfung | |
| `vendor.step2.intro` | Text | Geben Sie an, ob und wie das Produkt geprüft wurde. Wurde nach mehreren Verfahren geprüft, legen Sie für jedes Verfahren einen eigenen Prüfnachweis an. | U56 |
| `vendor.step2.tested` | `legend` | Wurde das Produkt auf Barrierefreiheit geprüft? | Pflicht (F8) |
| `vendor.step2.rule` | Hinweis über den Karten (bei „Ja“) | Bei „Ja“ geben Sie mindestens einen Prüfnachweis an. Jedes Prüfverfahren kann nur einmal vorkommen. | U56; Fehler bei doppeltem Verfahren: `candidate.evidence.list.duplicate` |
| `vendor.step2.testType` | Label | Art der Prüfung | Werte `testType.*` wie in S34a; „Andere Prüfung“ blendet `candidate.evidence.testType.otherName` ein. Je Nachweis eine Karte mit H3 `candidate.evidence.list.item` und Button `candidate.evidence.list.remove` (Muster S34a, U56) |
| `vendor.step2.testedBy` | Label | Prüfende Stelle | |
| `vendor.step2.testedBy.hint` | Hint | Name der Prüfstelle oder „Eigene Prüfung“. | |
| `vendor.step2.date` | Label | Datum der Prüfung | Hint `common.date.hint` |
| `vendor.step2.version` | Label | Geprüfte Version (optional) | U56; `TestedVersion` je Prüfnachweis (früher in Schritt 1) |
| `vendor.step2.url` | Label | Link zum Prüfbericht (optional) | U56 |
| `vendor.step2.add` | Button (unter der letzten Karte) | Weiteren Prüfnachweis hinzufügen | U45, U56; Fokus in das Feld „Art der Prüfung“ der neuen Karte |
| `vendor.step2.report` | Label (P1) | Konformitätsbericht (VPAT oder ACR) hochladen (optional) | Dateitypen und Größe im Hint |
| `vendor.step2.report.hint` | Hint (P1) | PDF, ODF oder Office-Datei, höchstens {{maxMb}} MB. | SEC-04 |
| `vendor.step3.name` | Schritt | Kriterien (optional) | |
| `vendor.step3.intro` | Text | Sie können zusätzlich angeben, wie Ihr Produkt die Kriterien des Katalogs erfüllt. Es gelten die Anwendungstypen aus Schritt 1; wählen Sie noch die Funktionsbereiche. Kriterien ohne Angabe erscheinen als „Nicht eingeschätzt“. | U36; ohne Kriterien zeigt der Eintrag keinen Abgleich, sondern `signals.fulfillment.none` (D2, PUB-14); mit Kriterien sind die Funktionsbereiche Pflicht |
| `vendor.step3.skip` | Button | Ohne Kriterien weiter | |
| `vendor.step4.name` | Schritt | Zustimmung | |
| `vendor.step4.intro` | Text | ⚖️ Prüfergebnisse dürfen nur mit Zustimmung der Stelle veröffentlicht werden, die die Prüfung beauftragt hat. | PRD §4 |
| `vendor.step4.givenBy` | Label | ⚖️ Zustimmung erteilt von | Person oder Funktion |
| `vendor.step4.onBehalfOf` | Label | ⚖️ für (auftraggebende Stelle) | |
| `vendor.step4.date` | Label | ⚖️ Datum der Zustimmung | Hint `common.date.hint`; Fehler `errors.field.dateFuture` |
| `vendor.step4.note` | Label | ⚖️ Wie wurde zugestimmt? (optional) | z. B. „schriftlich per E-Mail“ |
| `vendor.step4.confirm` | Checkbox | ⚖️ Ich bestätige, dass die auftraggebende Stelle der Veröffentlichung zugestimmt hat und dass die Angaben dem Prüfergebnis entsprechen. | Selbstauskunft – genau dieser Punkt ist Teil von E4 |
| `vendor.step4.confirm.error` | Feldfehler | Bitte bestätigen Sie die Zustimmung der auftraggebenden Stelle. | U7; Figma S55-E |
| `vendor.step4.notShown` | Hinweis | Die Angaben zur Zustimmung werden nicht öffentlich angezeigt. Sie dienen als Nachweis, falls es Rückfragen gibt. | an Architektur abgleichen: `PublicationConsent` ist nicht Teil der öffentlichen DTOs |
| `vendor.step5.name` | Schritt | Vorschau und Veröffentlichen | |
| `vendor.step5.preview` | H2 | So erscheint Ihr Eintrag | Eintragskarte mit K1–K4 |
| `vendor.step5.profile` | Label | Veröffentlichen als | Beitragsprofil der Art Anbieter oder Prüfstelle; beim Aktualisieren fest (PUB-15) |
| `vendor.submit` | Button | Prüfergebnis veröffentlichen | |
| `vendor.success` | Bestätigung | Ihr Prüfergebnis ist veröffentlicht. Sie finden es unter „Meine Beiträge“. Wenn eine neue Prüfung vorliegt, veröffentlichen Sie dort ein neues Prüfergebnis. | |
| `vendor.republish.title` | H1 (S55 im Modus „aktualisieren“) | Neues Prüfergebnis für {{product}} | |
| `vendor.republish.intro` | Text | Ihre bisherigen Angaben sind vorausgefüllt. Die neue Fassung ersetzt die bisherige Anzeige; die frühere Fassung bleibt als Version erhalten. | PUB-05, `PublishedEntryVersion` |
| `vendor.republish.consentNote` | Hinweis in Schritt 4 | ⚖️ Für jede neue Fassung brauchen wir die Zustimmung der auftraggebenden Stelle erneut. Die Angaben aus der letzten Fassung sind nicht vorausgefüllt. | PUB-05, PUB-08; bewusst keine Vorbelegung, damit die Zustimmung aktiv bestätigt wird |

### 5.14 S60–S61 Einstellungen und Konto (`settings.`, `account.`)

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `settings.title` | H1 | Einstellungen | |
| `settings.intro` | Text | Passen Sie ProcureAccess an Ihre Bedürfnisse an. Die Einstellungen werden in Ihrem Konto gespeichert und gelten auf allen Geräten. | A11Y-06 |
| `settings.language` | Label | Sprache | |
| `settings.language.hint` | Hint | Die Oberfläche gibt es vorerst nur auf Deutsch. | U73; SD12 |
| `settings.appearance` | `legend` | Erscheinungsbild | U73; SD11 |
| `settings.appearance.system` | Option | Wie im Betriebssystem eingestellt | Standard |
| `settings.appearance.light` | Option | Hell | |
| `settings.appearance.dark` | Option | Dunkel | |
| `settings.contrast` | `legend` | Kontrast | |
| `settings.contrast.system` | Option | Wie im Betriebssystem eingestellt | Standard |
| `settings.contrast.high` | Option | Hoher Kontrast | |
| `settings.motion` | `legend` | Bewegung und Animationen | |
| `settings.motion.system` | Option | Wie im Betriebssystem eingestellt | |
| `settings.motion.reduced` | Option | Bewegungen reduzieren | |
| `settings.saved` | Ansage | Einstellungen gespeichert. | |
| `account.title` | H1 | Konto | |
| `account.export.title` | H2 | Ihre Daten herunterladen | PRV-07 |
| `account.export.text` | Text | Sie erhalten eine Datei (JSON) mit Ihren Kontodaten, Ihren Beitragsprofilen und Beiträgen sowie den Kriterienlisten und Beschaffungsprojekten, die Sie angelegt haben. | U78; SPEC_v1 §8.4, PRV-07 |
| `account.export.cta` | Button | Daten herunterladen | |
| `account.delete.title` | H2 | Konto löschen | |
| `account.delete.text` | Text | ⚖️ Wenn Sie Ihr Konto löschen, werden Ihre Anmeldedaten entfernt. Veröffentlichte Beiträge werden zurückgezogen und sind nicht mehr sichtbar. Text und Angaben zu Hilfsmitteln Ihrer Erfahrungsberichte werden gelöscht. | abhängig von ARCH §16.2.1 / PRV-06; Löschung der Berichtsinhalte nach D4 (PRV-10, U65); andere Sitzungen enden (SEC-08) |
| `account.delete.cta` | Button | Konto löschen | |
| `account.delete.confirm.title` | Dialog-Titel | Konto endgültig löschen? | |
| `account.delete.confirm.body` | Text | Das lässt sich nicht rückgängig machen. Geben Sie zur Bestätigung Ihre E-Mail-Adresse ein. | |
| `account.delete.confirm.cta` | Button | Konto endgültig löschen | |
| `account.delete.confirm.keep` | Button | Konto behalten | |
| `account.delete.soleOwner` | Fehler | Sie sind das einzige Mitglied mit der Rolle „Verwalten“ in {{org}}. Geben Sie diese Rolle zuerst einem anderen Mitglied, bevor Sie Ihr Konto löschen. | U74; TEN-11 |

### 5.15 S70 Kriterium vorschlagen (`suggest.`) – nach E2

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `suggest.link` | Link | Kriterium vorschlagen | Footer und Kriterienliste |
| `suggest.linkContext` | Link (Kriterienliste) | Fehlt ein Kriterium oder ist eine Formulierung unklar? Schlagen Sie eine Änderung vor | |
| `suggest.external` | Visuell ausgeblendeter Zusatz | (öffnet Ihr E-Mail-Programm) | bzw. „(öffnet eine externe Seite)“ – je nach Kanal |
| `suggest.mailSubject` | `mailto`-Betreff | Vorschlag für den Kriterienkatalog | |
| `suggest.mailBody` | `mailto`-Text | Mein Vorschlag betrifft das Kriterium: {{criterionKey}}%0A%0ABitte beschreiben Sie Ihren Vorschlag: | Kriterium vorbelegen, wenn aus der Liste aufgerufen |

Offen: Kontaktadresse bzw. Formular-URL bei WERTE.IT (§8).

### 5.16 Seitentitel (`pageTitle.`) – neu in v3

Wert für `{{page}}` in `shell.pageTitle`. Vom Speziellen zum Allgemeinen, damit Screenreader und Browser-Tabs das Unterscheidende zuerst nennen. Nach fehlgeschlagenem Absenden mit `shell.pageTitle.error`.

| Schlüssel | Screen | Text |
|---|---|---|
| `pageTitle.home` | S04 | Barrierefreie Software beschaffen (ohne Präfix: „ProcureAccess – Barrierefreie Software beschaffen“) |
| `pageTitle.criteria.step` | S10 | Kriterien zusammenstellen – Schritt {{n}} von 4 |
| `pageTitle.criteria.list` | S11 | Ihre Kriterienliste (gespeicherte Liste: {{name}}) |
| `pageTitle.criteria.saved` | S14 | Gespeicherte Kriterienlisten |
| `pageTitle.criteria.shared` | S16 | Geteilte Kriterienliste: {{name}} |
| `pageTitle.projects` | S30 | Beschaffungsprojekte |
| `pageTitle.project` | S32 | {{project}} – Beschaffungsprojekte |
| `pageTitle.candidate` | S34 | {{tab}} – {{product}} – {{project}} |
| `pageTitle.publish` | S35 | Eintrag veröffentlichen – {{product}} (mit Stepper: „Schritt {{n}} von 4: {{step}} – Eintrag veröffentlichen – {{product}}“) |
| `pageTitle.directory` | S40 | Produktverzeichnis |
| `pageTitle.directory.product` | S41 | {{product}} – Produktverzeichnis |
| `pageTitle.directory.entry` | S42 | Eintrag von {{displayName}} – {{product}} – Produktverzeichnis |
| `pageTitle.contribute` | S53 | Meine Beiträge |
| `pageTitle.contribute.report` | S51 | Erfahrungsbericht schreiben – {{product}} |
| `pageTitle.vendor.step` | S55 | Schritt {{n}} von 5: {{step}} – Prüfergebnis veröffentlichen |
| `pageTitle.settings` / `.account` | S60, S61 | Einstellungen / Konto |
| `pageTitle.legal.imprint` / `.privacy` / `.accessibility` | S71 | Impressum / Datenschutz / Erklärung zur Barrierefreiheit |
| `pageTitle.admin.requests` | S81 | Produktvorschläge – Verwaltung |
| `pageTitle.notFound` / `.forbidden` / `.server` | S80 | wie die jeweilige H1 (§6.2) |

Dialoge (S12, S13, S15, S31, S36, S37) ändern den Seitentitel nicht.

### 5.17 E-Mails (`email.`) – neu in v13

Nur Text, keine Bilder, kein Tracking (ARCHITECTURE_v11 C71). Jede E-Mail endet mit `email.footer`. Links stehen als vollständige Adresse in einer eigenen Zeile.

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `email.footer` | Fußzeile | Diese E-Mail wurde automatisch von ProcureAccess (Projekt WERTE.IT) versendet. Bitte antworten Sie nicht darauf. | |
| `email.confirm.subject` | Betreff | Bitte bestätigen Sie Ihre E-Mail-Adresse | |
| `email.confirm.body` | Text | Guten Tag,\n\nbitte bestätigen Sie Ihre E-Mail-Adresse für ProcureAccess mit diesem Link:\n{{link}}\n\nDer Link ist 24 Stunden gültig. Wenn Sie kein Konto angelegt haben, können Sie diese E-Mail ignorieren. | |
| `email.alreadyRegistered.subject` | Betreff | Anmeldung bei ProcureAccess | R-AUTH-01 |
| `email.alreadyRegistered.body` | Text | Guten Tag,\n\njemand wollte mit dieser E-Mail-Adresse ein Konto bei ProcureAccess anlegen. Es gibt dafür bereits ein Konto. Sie können sich hier anmelden oder Ihr Passwort zurücksetzen:\n{{link}}\n\nWenn Sie das nicht waren, müssen Sie nichts tun. | |
| `email.reset.subject` | Betreff | Passwort zurücksetzen | |
| `email.reset.body` | Text | Guten Tag,\n\nmit diesem Link legen Sie ein neues Passwort für ProcureAccess fest:\n{{link}}\n\nDer Link ist {{hours}} Stunden gültig. Wenn Sie das nicht angefordert haben, können Sie diese E-Mail ignorieren. Ihr Passwort bleibt dann unverändert. | |
| `email.invitation.subject` | Betreff | Einladung in die Organisation {{org}} | |
| `email.invitation.body` | Text | Guten Tag,\n\nSie wurden in die Organisation {{org}} bei ProcureAccess eingeladen (Rolle: {{role}}). Dort arbeiten Sie gemeinsam an Kriterienlisten und Beschaffungsprojekten.\n\nEinladung annehmen:\n{{link}}\n\nDie Einladung ist {{days}} Tage gültig. | Name der einladenden Person wird nicht genannt (Datensparsamkeit) |
| `email.productRequest.accepted.subject` | Betreff | Ihr Produktvorschlag wurde aufgenommen | |
| `email.productRequest.accepted.body` | Text | Guten Tag,\n\n„{{product}}“ ist jetzt im Produktverzeichnis. Sie können Ihren Erfahrungsbericht hier schreiben:\n{{link}} | |
| `email.productRequest.rejected.subject` | Betreff | Ihr Produktvorschlag | |
| `email.productRequest.rejected.body` | Text | Guten Tag,\n\nwir konnten „{{product}}“ nicht ins Produktverzeichnis aufnehmen. Begründung: {{reason}}\n\nWenn Sie Fragen haben, schreiben Sie uns über die Kontaktseite. | |

### 5.18 S81 Administration (`admin.`) – neu in v13 (bestätigt in v14, SPEC_v2 SD22)

Nur für die Plattformrolle `PlatformAdmin`. Aufbau aus Angular-Material-Standards (Handoff §5.3).

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `admin.requests.title` | H1 | Produktvorschläge | |
| `admin.requests.empty` | Leerzustand | Es gibt keine offenen Produktvorschläge. | |
| `admin.requests.item` | Zeile | „{{product}}“ von {{vendor}} · vorgeschlagen am {{date}} | Beitragsprofil wird nicht angezeigt |
| `admin.requests.accept` | Button | Ins Verzeichnis aufnehmen | sendet `email.productRequest.accepted.*` |
| `admin.requests.reject` | Button | Ablehnen | öffnet Dialog mit Begründung |
| `admin.requests.reason` | Label | Begründung für die vorschlagende Person | Pflicht; wird per E-Mail gesendet |
| `admin.requests.match` | Hinweis | Ähnliche Produkte im Verzeichnis: {{matches}} | Trigram-Treffer (PUB-07) |
| `admin.merge.title` | Dialog-Titel | Produkte zusammenführen | |
| `admin.merge.target` | Label | In welches Produkt soll „{{product}}“ aufgehen? | |
| `admin.merge.note` | Hinweis | Alle Einträge werden danach beim gewählten Produkt angezeigt. Das lässt sich nicht rückgängig machen. | `MergedIntoId` |
| `admin.done` | Ansage | Erledigt. | |

### 5.19 S71 Rechtliche Seiten (`legal.`) – neu in v13

Die Inhalte liefert WERTE.IT; sie liegen als Markdown im Repository (SPEC_v1 R-SHL-07) und sind keine UI-Texte.

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `legal.imprint.title` | H1 | Impressum | |
| `legal.privacy.title` | H1 | Datenschutzhinweise | |
| `legal.accessibility.title` | H1 | Erklärung zur Barrierefreiheit | Pflichtseite (BITV) |
| `legal.placeholder` | Hinweis (nur außerhalb der Produktion) | Platzhalter: Der Inhalt dieser Seite fehlt noch. | Produktions-Build schlägt fehl (R-SHL-07) |

---

## 6. Fehler, Leer- und Systemzustände (`errors.`)

### 6.1 Feldfehler (API-02)

Fehler werden erst beim Absenden geprüft, nicht beim Tippen. Feldfehler stehen direkt am Feld (`mat-error`) und sind über `aria-describedby` verknüpft; das Feld erhält `aria-invalid="true"`.

**Fehlerübersicht (E11, geändert in v3):** Oberhalb des Formulars bzw. am Anfang des Stepper-Schritts erscheint eine Fehlerübersicht (`pa-notice` Fehler mit Titel als H2, `tabindex="-1"`). **Nach dem Absenden erhält die Fehlerübersicht den Fokus** – nicht mehr das erste fehlerhafte Feld. So hören Screenreader-Nutzende zuerst, wie viele Angaben fehlen, und erreichen jedes Feld über die Links. Jede Zeile beginnt mit dem Feldnamen als Link zum Feld und lautet danach genauso wie der Feldfehler (E12). Der Seitentitel erhält das Präfix aus `shell.pageTitle.error`.

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `errors.summary.title` | Überschrift Zusammenfassung | {{count}} Angaben müssen noch korrigiert werden | Einzahl: `errors.summary.title.one` |
| `errors.summary.title.one` | Überschrift (Einzahl) | 1 Angabe muss noch korrigiert werden | |
| `errors.summary.item` | Zeile der Übersicht | {{field}}: {{message}} | `field` = sichtbares Label ohne „(optional)“, als Link `#feld-id`; `message` = Feldfehler (E12) |
| `errors.field.required` | Feldfehler | Bitte geben Sie {{field}} an. | `field` im Akkusativ formulieren, z. B. „einen Namen“ – bei Bedarf feldspezifische Texte |
| `errors.field.requiredChoice` | Feldfehler | Bitte wählen Sie eine Option. | |
| `errors.field.email` | Feldfehler | Bitte geben Sie eine gültige E-Mail-Adresse ein, zum Beispiel name@beispiel.de. | |
| `errors.field.url` | Feldfehler | Bitte geben Sie eine vollständige Webadresse ein, die mit https:// beginnt. | |
| `errors.field.maxLength` | Feldfehler | Bitte kürzen Sie den Text auf höchstens {{max}} Zeichen. Derzeit sind es {{current}}. | |
| `errors.field.date` | Feldfehler | Bitte geben Sie ein Datum im Format TT.MM.JJJJ ein. | |
| `errors.field.dateFuture` | Feldfehler | Das Datum liegt in der Zukunft. Bitte prüfen Sie es. | Prüfdatum |
| `errors.field.server` | Feldfehler | {{message}} | Fallback für Backend-Text aus Problem Details |

### 6.2 Seiten- und Systemfehler

| Schlüssel | Element | Text | Hinweis |
|---|---|---|---|
| `errors.notFound.title` | H1 | Seite nicht gefunden | auch bei fremder Ressource (404 statt 403, TEN-03) |
| `errors.notFound.body` | Text | Diese Seite gibt es nicht oder nicht mehr. Vielleicht wurde sie verschoben, oder der Link ist unvollständig. | |
| `errors.forbidden.title` | H1 | Kein Zugriff | |
| `errors.forbidden.body` | Text | Für diese Seite fehlen Ihnen die Rechte. Wenn Sie glauben, dass das ein Fehler ist, wenden Sie sich an ein Mitglied Ihrer Organisation mit der Rolle „Verwalten“. | |
| `errors.server.title` | H1 | Etwas ist schiefgelaufen | |
| `errors.server.body` | Text | Das ist ein Fehler bei uns. Ihre Eingaben wurden nicht gespeichert. Bitte versuchen Sie es in einigen Minuten erneut. | |
| `errors.offline` | Hinweis | Keine Verbindung zum Server. Bitte prüfen Sie Ihre Internetverbindung. | |
| `errors.rateLimit` | Hinweis | Sie haben diese Aktion in kurzer Zeit sehr oft ausgeführt. Bitte warten Sie {{minutes}} Minuten und versuchen Sie es dann erneut. | 429, API-05 |
| `errors.home` | Button | Zur Startseite | |
| `errors.email.sendFailed` | Hinweis (`pa-notice` Fehler) | Die E-Mail konnte gerade nicht versendet werden. Bitte versuchen Sie es in einigen Minuten erneut. | U72; die Aktion selbst bleibt gespeichert |

---

## 7. Übertragbarkeit (Hinweise für die englische Fassung)

- Deutsche Texte sind im Schnitt länger als englische; Layouts in Figma werden mit den deutschen Texten gebaut, die englische Fassung passt dann in der Regel.
- Pluralformen: ngx-translate hat keine eingebaute Pluralbehandlung. Texte mit `{{count}}` brauchen entweder ICU-Formatierung (z. B. `ngx-translate-messageformat-compiler`) oder getrennte Schlüssel für Einzahl und Mehrzahl. → Entscheidung in der Umsetzung.
- „Anbieterseite“, „Damals nicht im Katalog“ und die Prioritätsstufen sind sinngemäß zu übersetzen, nicht wörtlich. Für die Einschätzungswerte gilt die Skala von BIT inklusiv, nicht die VPAT-Skala (VPAT-Begriffe würden eine Umrechnung nahelegen, die es nicht gibt). Vorschlag: Fulfilled, Slight restriction, Restriction, Barrier, Blockade, Not applicable, Not assessed; „Damals nicht im Katalog“: Not in the catalog at the time; Ja / Nein / Nicht angegeben bei „Geprüft nach …“: Yes / No / Not stated.

---

## 8. Offene Punkte

In anderen Dokumenten werden diese Punkte als UX-O1 … UX-O22 zitiert, damit sie nicht mit den Punkten O-1 … O-16 der Architektur (ARCHITECTURE_v11 §16.5) verwechselt werden.

| # | Punkt | Wer | Bis wann |
|---|---|---|---|
| ~~O1~~ | ~~Export nur mit Konto und Organisation?~~ **Erledigt (E7):** Export ohne Konto | – | – |
| O2 | Prioritätsbegriffe „A – Mindestanforderung“, „B – Bewertungskriterium“, „Hinweis“ fachlich bestätigen (A2; = ARCHITECTURE_v11 O-14) | Person mit Barrierefreiheits-Expertise im Projektteam | vor SPEC_v1 |
| ~~O3~~ | ~~Flow B ohne Kanalauswahl~~ **Erledigt (E9):** in ARCHITECTURE_v11 D16 festgehalten | – | – |
| O4 | Kontaktadresse oder Formular für „Kriterium vorschlagen“ (E2) | Projektteam WERTE.IT | vor Umsetzung |
| O5 | ⚖️ Texte zu Zustimmung (S55), Einwilligung bei Angaben zu Hilfsmitteln (S51), Kontolöschung (S61) und Hinweis zur rechtlichen Verbindlichkeit beim Export (S13) | Gebündelte rechtliche Prüfung (PRD §11; ARCHITECTURE_v11 §16.5 O-4) | vor Live-Gang |
| ~~O6~~ | ~~Prüfstatus ohne „Unbekannt“~~ **Erledigt (E8):** dritter Zustand „Nicht bekannt“ im Projekt, Veröffentlichung erst mit Ja oder Nein | – | – |
| ~~O7~~ | ~~Funktion „Neue Liste mit derselben Auswahl“ bei neuer Katalogversion~~ **Erledigt (D6, U66):** wie in ARCHITECTURE_v11 §16.3.1 vorgeschlagen | – | – |
| ~~O8~~ | ~~Schwellenwert für den Aktualitätshinweis~~ **Erledigt (SPEC_v1 SD7):** 18 Monate | – | – |
| O9 | Pluralbehandlung in ngx-translate (§7) | Umsetzung | Migrationsschritt 1 |
| O10 | Moderationstexte (S90) und Upload-Texte (P1) | dieses Dokument, nächste Fassung | vor Migrationsschritt 7 |
| O11 | Exakte Werte für die Begrenzung beim Export ohne Konto (wie viele Exporte pro Zeitraum), damit wiederholtes Herunterladen beim Verfeinern einer Liste nicht blockiert wird | Umsetzung | Migrationsschritt 2 |
| O12 | Texte für Fehlerzustände weiterer Formulare (S12, S20, S33, S50, S52) nach dem Muster von §6.1 | Umsetzung, beim jeweiligen Screen | Migrationsschritt des Screens |
| ~~O13~~ | ~~Konfliktdialog S37 in Figma gestalten~~ **Erledigt:** Figma S37 | – | – |
| ~~O14~~ | ~~E13 in ARCHITECTURE und TESTING übernehmen~~ **Erledigt:** ARCHITECTURE_v11 C28, TESTING_v9 SIG-12 | – | – |
| ~~O15~~ | ~~Ausschluss HR-Anwendung → Mobile Anwendung~~ **Erledigt:** kein Ausschluss | – | – |
| O16 | Verständliche Titel und Kurzbeschreibungen je Kriterium (`PlainLanguageDe`) für die BIT-inklusiv-Kriterien (Kriterienkatalog K-1) | Projektteam | vor Migrationsschritt Katalog |
| ~~O17~~ | ~~S55 Schritt 2 „Prüfung“ in Figma gestalten und Schritt 1 um Anwendungstypen und Verwendungszwecke ergänzen~~ **Erledigt:** Figma S55-1 `2145:3653`, S55-2 `2145:4312` (U57, U56) | – | – |
| O18 | Hardware sowie Vermittlungs- oder Notrufdienst haben noch kein Prüfverfahren (offenes Todo, vorerst akzeptiert; Kriterienkatalog K-2) | Projektteam | offen |
| O19 | Erklärungstexte der Bewertungsstufen fachlich prüfen (Kriterienkatalog K-8; = ARCHITECTURE_v11 O-16, Handoff H13) | Projektteam | vor SPEC_v1 |
| O20 | Weitere lange Feldlabels nach dem Muster von S51 prüfen (Label darüber, Kurzlabel im Feld), z. B. S55, S33 | Umsetzung | Migrationsschritt des Screens |
| O21 | Ergebnisse der manuellen Prüfung auf echten Geräten (accessibility_review_procureaccess_v2, „Offen für die manuelle Prüfung“) in die Texte zurückführen | Philipp | vor Pilot |
| ~~O22~~ | ~~Verhältnis von „Geprüft nach …“ zum Prüfnachweis~~ **Erledigt (U46):** wird aus den Prüfnachweisen übernommen | – | – |
