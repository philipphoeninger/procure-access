# PRD: ProcureAccess (Projekt WERTE.IT)

> **Status:** Product Requirements Document v5 (13. Oktober 2026). Ersetzt v4. v5 übernimmt die Produktentscheidungen D1–D7 aus der Prüfung der Hauptdokumente vor SPEC_v1 (`pruefung_hauptdokumente_v1.md`): eigene Kopie der Kriterienliste in jedem Beschaffungsprojekt, Einträge der Anbieterseite ohne Einschätzungen, Löschung persönlicher Inhalte beim Widerruf der Einwilligung, Anwendungstyp bei Erfahrungsberichten, neue Liste nach einer neuen Katalogversion, Freigabelinks ohne Konto. v4 (13. Oktober 2026) ersetzte v3 und übernahm die Entscheidungen aus dem Architektur-Review (Einwilligung für jeden Erfahrungsbericht, mehrere Prüfnachweise je Produkt). v3 (12. Oktober 2026) ersetzte v2. v3 übernimmt die Produktentscheidungen aus Phase 2 (Architektur, Testing, UI/UX-Design einschließlich der Reviews vom 6. und 12. Oktober 2026), damit die Feature-Spezifikation (`SPEC_v1.md`) auf einer konsistenten Grundlage steht. Ziele, Personas und Grundprinzipien bleiben unverändert; geändert haben sich vor allem Begriffe, die Filterstruktur, die Grundlage der Kriterien und einzelne Prioritäten. Die Änderungen gegenüber den Vorfassungen stehen am Ende (Abschnitte 15 bis 17).
> **Grundlage:**
> - `anforderungen_beschaffungstool_procureaccess_werteit_v2.md` (Ergebnis Chat 1.1)
> - `brainstorm_validierung_procureaccess_werteit.md` (Ergebnis Chat 1.2, überarbeitete Fassung mit eingearbeitetem Team-Feedback)
> - Klärungs-Chats vom 18.07.2026 und 02.08.2026
> - Entscheidungen aus Phase 2: `ARCHITECTURE_v10.md`, `ux_copy_procureaccess_v12.md`, `kriterienkatalog_bit_inklusiv_v1.md` (1.5), `architektur_review_v6.md`, `pruefung_hauptdokumente_v1.md`
> **Hinweis zur Begründungstiefe:** Dieses Dokument enthält zu jeder wichtigen Entscheidung eine kurze Begründung. Die vollständige Entscheidungshistorie inkl. verworfener Alternativen, Team-Feedback und Marktrecherche findet sich in `brainstorm_validierung_procureaccess_werteit.md`, die Entscheidungen aus Phase 2 in den genannten Dokumenten.
> **Nicht Gegenstand dieses Dokuments:** Datenmodell, Berechtigungskonzept im Detail, UI/UX-Gestaltung, technische Architektur. Diese stehen in den Phase-2-Dokumenten.

---

## 1. Kontext & Ausgangslage

**WERTE.IT** ist ein Forschungsprojekt, das gemeinsam mit Kooperationspartnern Wege erforscht, über einen unternehmensweit ausgerollten inklusiven Ansatz die Voraussetzungen für die Beschäftigung von Menschen mit Behinderungen zu schaffen und zu verbessern.

**ProcureAccess** ist eines der Module von WERTE.IT: ein Software-Tool, das beschaffende Stellen bei der Beschaffung barrierefreier Software unterstützt. Es wird als **öffentlich gefördertes Projekt** entwickelt und **Open Source** auf GitHub veröffentlicht, um Transparenz und Nachnutzbarkeit zu fördern. Das Konzept wurde dem Projektteam vorgestellt (schriftliches Feedback + Feedback-Meeting) und **zur konkreten Umsetzung freigegeben**.

---

## 2. Problemstellung

In Unternehmen und öffentlichen Stellen ist noch zu viel Software im Einsatz, die nicht barrierefrei ist. Auch wenn der Wille vorhanden ist, Menschen mit Behinderung einzustellen, scheitert die Eingliederung häufig daran, dass die eingesetzte Software von diesen Personen nicht bedient werden kann — mit der Folge, dass sie das Unternehmen nach kurzer Zeit wieder verlassen.

Das Problem liegt dabei nicht immer am fehlenden Willen der beschaffenden Stellen. Es fehlt an:

- **Orientierung:** Welche Barrierefreiheitskriterien gelten für welche Art von Software?
- **Verständlichkeit:** Bestehende Normen (EN 301 549, BITV) sind für Nicht-Fachleute schwer zugänglich.
- **Produktwissen:** Welche am Markt verfügbaren Produkte erfüllen diese Kriterien überhaupt?
- **Prozessintegration:** Wie fließen Barrierefreiheitsanforderungen strukturiert in Beschaffungsverfahren ein?

ProcureAccess setzt genau hier an: Es hilft beschaffenden Stellen, die für ihre Situation relevanten Barrierefreiheitskriterien in verständlicher Sprache zu finden, und unterstützt sie dabei, passende Produkte zu identifizieren und zu bewerten.

---

## 3. Positionierung & Abgrenzung

Der **Standardanforderungskatalog des Bundes** ([Link](https://www.barrierefreiheit-dienstekonsolidierung.bund.de/Webs/PB/DE/standardanforderungskatalog/standardanforderungskatalog-node.html)) existiert bereits als interaktives Filterwerkzeug mit Exportfunktion und dient ProcureAccess als fachliche Referenz für die Filterstruktur. Eine frühere Annahme, es gebe im DACH-Raum kein vergleichbares interaktives Tool, wurde vom Projektteam korrigiert.

Der Katalog bietet zwei Filterdimensionen: **Art der IT-Lösung** und **Eigenschaften/Funktionalitäten**. ProcureAccess übernimmt beide (in ProcureAccess: **Anwendungstyp** und **Funktionsbereiche**) und ergänzt:

- **Verwendungszweck:** Priorisierung nach konkretem Einsatz (z. B. HR-Anwendung, Reporting) — im Standardanforderungskatalog nicht in dieser Form vorgesehen.
- **Prüfverfahren:** Auswahl und Filterung nach dem Verfahren, mit dem ein Produkt geprüft wurde (z. B. BIT inklusiv Software-Test, BIK BITV-Test).
- **Ausschlussregeln** zwischen diesen Filtern, damit nur sinnvolle Kombinationen wählbar sind.

Hinzu kommen die **Übersetzung der Kriterien in verständliche, nicht-normnahe Sprache** und die **Workspace-/Produktkomponente**, die es im Standardanforderungskatalog nicht gibt.

Nach außen positioniert sich ProcureAccess bewusst als **Barrierefreiheits-Tool**, nicht als generisches Vendor-Evaluierungstool (siehe Nicht-Ziele) — die Zielgruppe sucht aktiv nach Orientierung zu diesem Thema und wird über diesen Rahmen erreicht.

---

## 4. Zielgruppe & Personas

Das Tool richtet sich an **öffentliche Stellen und private Unternehmen** gleichermaßen. Die primären Nutzerinnen haben **kein oder selten Vorwissen** zu Normen wie EN 301 549 oder BITV — Sprache und UX müssen entsprechend niedrigschwellig und praxisnah sein.

**Einkäuferin / Beschaffungsverantwortliche**
Recherchiert Produkte und holt Angebote ein. Braucht schnelle, verständliche Orientierung: „Welche Kriterien muss ich fordern?" und „Welche Produkte kommen infrage?"

**Ausschreibungsverantwortliche Person**
Erstellt Ausschreibungen und muss Barrierefreiheitskriterien als Anforderungen formulieren. Benötigt strukturierte, exportierbare Kriterienlisten.

**Anbieter / Prüferin (Kanal-1-Beitragende)**
Hersteller oder beauftragte Prüfstellen, die mit Zustimmung ihres Auftraggebers Prüfergebnisse zu einem Produkt veröffentlichen — als Sichtbarkeits- und Reputationskanal. Sie brauchen dafür weder eine Organisation noch ein Beschaffungsprojekt im Tool.

**Menschen mit Behinderung als Beitragende (Kanal-2-Beitragende)**
Aktiv eingeladene Mitgestaltende, die Erfahrungsberichte zu Produkten beisteuern — explizit als Mitgestaltungsangebot, nicht als unentgeltliche Qualitätssicherung. Feste Prinzipien für diese Nutzergruppe:
- **Pseudonymität muss möglich sein** (Risiko von Diskriminierung, Stalking, ungewollter Offenlegung einer Behinderung gegenüber Dritten, z. B. Arbeitgebern).
- **Keine Verallgemeinerung einzelner Stimmen** — ein Erfahrungsbericht darf nicht zur pauschalen „Betroffenen-Meinung" zu einem Produkt verschmolzen werden.
- **Widerruf ohne Rückstände:** Für jeden Erfahrungsbericht wird eine Einwilligung eingeholt. Zieht die Person ihren Bericht zurück, gilt das als Widerruf: Text und Angaben zu Hilfsmitteln werden gelöscht, nur ein minimaler Moderationsnachweis bleibt (Entscheidung D4). Den Zeitpunkt der Löschung legt die rechtliche Prüfung fest (vorläufig: sofort).
- **Zugang bevorzugt über Kooperationspartner/Verbände**, nicht über offene Kaltakquise einzelner Personen.

**Kein primärer Nutzer:** Entwicklerinnen und Entwickler, die technische Implementierungsdetails suchen. Das Tool ist kein technisches Referenzwerk.

---

## 5. Ziele

1. Beschaffende Stellen ohne Vorwissen zu Barrierefreiheitsnormen finden innerhalb kurzer Zeit die für ihren Beschaffungsfall relevanten, priorisierten Kriterien.
2. Ausschreibungsverantwortliche können Kriterien direkt und ohne wesentliche Nacharbeit in Vergabeunterlagen übernehmen (Excel-Export).
3. Es entsteht eine wachsende Basis geteilter Produktbewertungen (Kanal 1 + Kanal 2), ohne dass WERTE.IT eine zentrale Produktdatenbank pflegen muss.
4. Das Tool wird in mindestens einem realen Beschaffungsvorgang mit einem Kooperationspartner pilotiert, um die Priorisierungslogik zu validieren.
5. Das Tool erfüllt selbst die Barrierefreiheitsanforderungen, die es propagiert (Vorbildfunktion) — auch auf Smartphones (die Umsetzung erfolgt mobile-first).

---

## 6. Nicht-Ziele

- **Kein Audit-, Test- oder Zertifizierungstool, kein Ersatz für professionelle Barrierefreiheitsprüfungen.** *Begründung:* ProcureAccess bündelt und übersetzt bestehende Kriterien und Prüfergebnisse, führt aber keine eigenen Prüfungen durch — das läge außerhalb der Kompetenz und Verantwortung des Tools und würde mit spezialisierten Prüfstellen (z. B. BIT inklusiv) konkurrieren statt sie zu ergänzen. ProcureAccess bildet auch **kein Gesamturteil** über ein Produkt (keine Punktzahl, kein Prozentwert, keine Ampel).
- **Kein allgemeines Vendor-/Procurement-Tool.** *Begründung:* Der Markt für generische Vendor-Evaluierungssoftware ist dicht und ausgereift. Eine bewusst barrierefreiheits-spezifische Fokussierung hält das Tool differenziert und den Scope im Rahmen der Projektressourcen realistisch.
- **Keine zentrale, von WERTE.IT gepflegte Produktdatenbank.** *Begründung:* Klassisches Henne-Ei-Problem eines Verzeichnis-Produkts plus laufende Wartungslast und unklare Zuständigkeit nach Projektförderende. Ersetzt durch das Workspace-Modell (siehe Abschnitt 7). Ein Produkt im Verzeichnis ist nur eine **Identität** (Name, Anbieter); alle Aussagen über Barrierefreiheit, Anwendungstypen und Verwendungszwecke stammen aus den einzelnen, gekennzeichneten Einträgen. Auch die deutlich ressourcenstärkere US-amerikanische GSA trennt ihr Nachfolgetool (ART) inzwischen strukturell von der Produktrecherche (ACR Repository) — ein zusätzliches Indiz, dass die Entkopplung beider Bereiche der richtige Ansatz ist.
- **Keine formale Integration von ISO 9241 in den strukturierten Kriterienkatalog (v1).** *Begründung:* Das qualitative Usability-Signal zu einem konkreten Produkt entsteht bereits indirekt über die Erfahrungsberichte im Kanal 2. Eine zusätzliche, separat zu pflegende Normintegration in den strukturierten, exportierbaren Kriterienkatalog wäre zum jetzigen Zeitpunkt redundanter Aufwand ohne klaren Mehrwert gegenüber dieser bereits geplanten Funktion.
- **Keine Berücksichtigung von DIN EN 17161 (v1).** *Begründung:* Als Prozessnorm für Organisationen (nicht produktbezogen testbar) passt sie konzeptionell nicht in den aktuellen, produktkriterienbasierten Konfigurator. Bleibt als offene Frage für spätere Phasen bestehen (siehe Abschnitt 11).

---

## 7. Lösungskonzept

ProcureAccess besteht aus zwei Säulen:

1. **Kriterien-Konfigurator** — Die Auswahl von Verwendungszweck, Anwendungstyp, Funktionsbereichen und optional Prüfverfahren führt zu priorisierten, verständlichen Barrierefreiheitskriterien, die angepasst, gemerkt und exportiert werden können.
2. **Workspace mit optionaler Veröffentlichung** — projektbezogene Produktrecherche. „Produkt" ist kein globaler Katalogeintrag mit Wahrheitsanspruch von WERTE.IT, sondern ein Objekt im konkreten Beschaffungsvorgang der Nutzerin. Optional geteilte Einträge bilden mit der Zeit organisch eine community-getragene Produktübersicht — ohne „Meldestellen-Charakter", was vom Team ausdrücklich als Stärke des Modells hervorgehoben wurde.

**Kernablauf:**
1. Nutzerin wählt in vier Schritten:
   1. **Verwendungszweck** (genau einer, oder „Keiner davon passt"),
   2. **Anwendungstyp** (einer oder mehrere, inkl. offener *und* geschlossener Software, siehe unten),
   3. **Funktionsbereiche** (einer oder mehrere; Basisfunktionalität gilt immer),
   4. optional **Prüfverfahren** (eines oder mehrere).
   Höhere Auswahlen schließen unpassende Werte in tieferen Schritten aus (z. B. schließt „HR-Anwendung" den Anwendungstyp „Dokument" aus). Ändert die Nutzerin eine höhere Auswahl, werden die tieferen zurückgesetzt und sie wird gebeten, neu auszuwählen.
2. Das Tool schlägt priorisierte, verständliche Kriterien vor. Jedes gewählte Prüfverfahren kommt als eigenes Kriterium „Geprüft nach …" in die Liste.
3. Nutzerin passt Kriterien an, merkt sie und exportiert sie (z. B. als Excel-Datei für Vergabeunterlagen). Gespeicherte Listen lassen sich über einen Freigabelink teilen; wer den Link hat, kann die Liste ohne Konto lesen (Entscheidung D7). Gibt es eine neuere Katalogversion, legt die Nutzerin eine neue Liste mit derselben Auswahl an; die alte Liste bleibt unverändert (Entscheidung D6).
4. Nutzerin legt im Workspace Kandidatenprodukte an, schätzt sie anhand der Kriterien ein und kann Einträge optional veröffentlichen. Ein Beschaffungsprojekt arbeitet mit einer eigenen Kopie seiner Kriterienliste (siehe unten).

### Kriterienbasis

Die Kriterien basieren auf den Prüfverfahren von **BIT inklusiv** (Software-Test, App-Test, PDF-Test). Deren Texte (Anwendbarkeit, Anforderung, „Warum wird das geprüft?", Beispiele) werden mit Erlaubnis wörtlich übernommen; das Projektteam ergänzt je Kriterium einen verständlichen Titel, eine Kurzbeschreibung und die empfohlene Priorität mit Begründung. Für Websites gelten vorerst die Kriterien des Software-Tests; Kriterien des BIK BITV-Tests (Web) können später ergänzt werden. Details: `kriterienkatalog_bit_inklusiv_v1.md`.

**Bewertungsskala:** Einschätzungen nutzen die sechs Stufen von BIT inklusiv (Erfüllt, Leichte Einschränkung, Einschränkung, Barriere, Blockade, Nicht anwendbar) plus „Nicht eingeschätzt". Je Kriterium sind nur die Stufen wählbar, die das jeweilige Prüfverfahren vorsieht.

### Geschlossene Software im Scope

Geschlossene Software ist bereits in der ersten Version im Scope (Anwendungstypen „Software (geschlossen)" und „Mobile Anwendung (geschlossen)"). *Begründung:* Dies schafft einen Anreiz für Hersteller geschlossener Software, ihr Produkt aktiv im Workspace vertreten zu sehen und ggf. Kanal-1-Prüfergebnisse einzureichen, statt aus dem Tool von vornherein ausgeschlossen zu sein.

### Zwei-Kanal-Prinzip für Produkteinträge

Produktbezogene Einträge unterscheiden sich strukturell nach Herkunft und müssen im UI klar getrennt ausgewiesen werden:

- **Kanal 1 — Anbieter-/Prüfer-Kanal:** freiwillig, zustimmungspflichtig. Strukturell positiv verzerrt (niemand veröffentlicht freiwillig ein schlechtes Ergebnis) und dient als Sichtbarkeits-/Reputationskanal, **nicht** als objektive Informationsquelle. Kann als strukturiertes **VPAT/ACR** eingereicht werden. Anbieter und Prüfstellen veröffentlichen direkt, ohne Organisation oder Beschaffungsprojekt.
- **Kanal 2 — unabhängige Institutions-/Nutzer-Einträge:** keine Zustimmung des Anbieters nötig, kann auch kritische Inhalte enthalten. Veröffentlichungen aus dem Workspace einer beschaffenden Organisation sind immer Kanal 2. Erfahrungsberichte einzelner Personen sind ebenfalls Kanal 2.

Feste Prinzipien:
- Das Basisfeld „Auf Barrierefreiheit geprüft: Ja/Nein" ist bei jedem **Prüfergebnis-Eintrag** (Kanal 1 und Workspace-Veröffentlichungen) **unabhängig von Kanal und Ausgang immer sichtbar**. Im Workspace darf der Status vorübergehend „Nicht bekannt" sein; veröffentlicht werden kann erst mit Ja oder Nein. **Erfahrungsberichte** sind keine Prüfung und haben deshalb keinen Prüfstatus; sie sind als Beitragsart gekennzeichnet.
- **Dokumentationsgrad** (z. B. Konformitätsbericht vorhanden, Aktualität) und **Erfüllungsgrad** (wie die relevanten Kriterien eingeschätzt wurden) werden als **zwei getrennt sichtbare Signale** dargestellt, nie als gemeinsamer Status — sonst würde viel, aber schlechte Dokumentation gegenüber undokumentierten Anbietern unfair bevorzugt. Der Erfüllungsgrad wird als Anzahl je Bewertungsstufe gezeigt, nie als Summe, Prozent oder Ampel.
- Hat die Anbieterseite keine einzelnen Kriterien eingeschätzt, zeigt ihr Eintrag **keinen Abgleich**, sondern nur den Hinweis, dass keine Einschätzungen veröffentlicht wurden (Entscheidung D2). Ein Abgleich, in dem alles „Nicht eingeschätzt“ ist, würde eine Aussage vortäuschen.
- **Anwendungstypen und Verwendungszwecke** werden am einzelnen Eintrag gespeichert, nicht am Produkt: Der Anwendungstyp kommt aus dem Kontext der Veröffentlichung, der Verwendungszweck aus der Kriterienliste des Beschaffungsprojekts bzw. aus der Angabe des Anbieters (gekennzeichnet als „vom Anbieter angegeben"). Im Produktverzeichnis bedeutet der Filter „Verwendungszweck" daher: Produkte mit Einträgen aus diesem Einsatzkontext. Bei Erfahrungsberichten kann die beitragende Person angeben, wie sie die Software genutzt hat (Anwendungstyp, optional); einen Verwendungszweck haben Erfahrungsberichte nicht (Entscheidung D5). Sie erscheinen deshalb nicht, wenn im Produktverzeichnis nach einem Verwendungszweck gefiltert wird.

**Spannungsfeld VPAT/ACR — im Team unterschiedlich bewertet:** Anbieter-Selbstauskünfte gelten im Team teils als in der Praxis beschönigt und daher von begrenztem Nutzen für die tatsächliche Arbeitsplatz-Barrierefreiheit. Zugleich können referenzierte Kriterien bei Übernahme in einen Vergabetext rechtlich bindend werden, teils sogar mit vertraglichen Konventionalstrafen. Beide Sichtweisen widersprechen sich nicht, beschreiben aber unterschiedliche Verwendungsweisen von VPAT/ACR-Daten (eigenständige Qualitätsaussage vs. Grundlage für verbindliche Vergabekriterien). **Wie diese beiden Verwendungsweisen im Tool sauber getrennt dargestellt werden, ist noch offen** (siehe Abschnitt 11, gebündelte rechtliche Prüfung).

### Kriterienliste im Beschaffungsprojekt

Wird einem Beschaffungsprojekt eine gespeicherte Kriterienliste zugeordnet, erhält das Projekt eine **eigene Kopie** (Entscheidung D1, 13. Oktober 2026). Anpassungen im Projekt wirken nur dort, nicht in anderen Projekten und nicht in Freigabelinks. Die Kopie wird nicht für andere Projekte angeboten und nicht per Link geteilt. Nach der Zuordnung stehen Verwendungszweck, Anwendungstypen, Funktionsbereiche und Prüfverfahren fest; Kriterien aufnehmen oder entfernen, Prioritäten und Notizen bleiben änderbar. Sobald ein Produkt nach der Liste eingeschätzt wurde, kann die Liste nicht mehr gegen eine andere ausgetauscht werden. Für eine neuere Katalogversion wird ein neues Projekt mit einer neuen Liste angelegt (Entscheidung D6). *Begründung:* Einschätzungen bleiben nur vergleichbar, wenn sich ihre Grundlage nicht unbemerkt ändert, und kein Projekt soll ein anderes beeinflussen.

### Transparenz der Priorisierungslogik

Die Priorisierung von Kriterien je Verwendungszweck und Anwendungstyp basiert auf Expertenurteil, nicht auf validierter Evidenz. Dies muss im UI klar gekennzeichnet werden, um nicht wie eine geprüfte Norm zu wirken — konsistent mit dem Nicht-Zertifizierungs-Anspruch des Tools. Jede Priorität zeigt ihre Begründung.

### Umgang mit kritischen Einträgen

Kanal-2-Einträge können kritisch/negativ ausfallen und bergen das Risiko von Löschforderungen durch betroffene Anbieter. Grundprinzip: Löschung nur bei nachweislich falschen Tatsachenbehauptungen, bevorzugt Recht auf Gegendarstellung statt Löschung, Einträge möglichst faktenbasiert formulieren. Die vollständige Moderationsrichtlinie inkl. rechtlicher Prüfung ist vor Live-Gang nötig, aber nicht Teil dieses PRD (siehe Abschnitt 11).

---

## 8. Kernabläufe (User Stories)

**Einkäuferin**
- Als Einkäuferin möchte ich Verwendungszweck und Anwendungstyp meiner Beschaffung auswählen, um passende, priorisierte Barrierefreiheitskriterien zu erhalten, auch ohne Vorwissen zu EN 301 549 oder BITV.
- Als Einkäuferin möchte ich sehen, warum eine Auswahl nicht möglich ist, statt dass Optionen kommentarlos fehlen.
- Als Einkäuferin möchte ich vorgeschlagene Kriterien anpassen und für meinen Beschaffungsvorgang merken, um nur relevante Anforderungen weiterzuverwenden.
- Als Einkäuferin möchte ich im Workspace eigene Kandidatenprodukte anlegen und anhand der Kriterien einschätzen, um meine Produktrecherche zu dokumentieren.
- Als Einkäuferin möchte ich sehen, ob und von wem ein Produkt bereits auf Barrierefreiheit geprüft wurde — inkl. getrennt ausgewiesenem Dokumentations- und Erfüllungsgrad — um die Aussagekraft realistisch einschätzen zu können.
- Als Einkäuferin möchte ich im Produktverzeichnis Produkte finden, die andere bereits für denselben Verwendungszweck eingeschätzt haben.

**Ausschreibungsverantwortliche Person**
- Als Ausschreibungsverantwortliche möchte ich die priorisierten Kriterien als Excel-Datei exportieren, um sie direkt in Vergabeunterlagen zu übernehmen — auch ohne Konto.

**Anbieter / Prüferin**
- Als Anbieter/Prüferin möchte ich mit Zustimmung meines Auftraggebers ein Prüfergebnis (optional als VPAT/ACR) veröffentlichen, um Sichtbarkeit für mein geprüftes Produkt zu erhalten — ohne dafür ein Beschaffungsprojekt anlegen zu müssen.

**Mensch mit Behinderung als Beitragende**
- Als Beitragende möchte ich pseudonym einen Erfahrungsbericht zu einem Produkt hinterlassen, ohne meine Identität oder Art meiner Behinderung gegenüber Dritten offenlegen zu müssen.
- Als Beitragende möchte ich, dass mein Erfahrungsbericht als individueller Beitrag sichtbar bleibt und nicht zu einer pauschalen „Betroffenen-Meinung" verschmolzen wird.

**Alle Nutzerinnen**
- Als Nutzerin möchte ich das Tool selbst barrierefrei bedienen können, unabhängig von genutzter Assistenztechnologie und Gerät (Desktop oder Smartphone).
- Als Nutzerin möchte ich Kriterienlisten und Produkte speichern und teilen und Verbesserungen am Kriterienkatalog vorschlagen können.

---

## 9. Anforderungen

### Must-Have (P0)

| Anforderung | Akzeptanzkriterien (Auszug) |
|---|---|
| Auswahl des Verwendungszwecks | Genau ein Wert aus einem durchsuchbaren Starter-Set (HR-Anwendung, Reporting-Anwendung, Fortbildungsmanagement, Dienstreiseanträge, Gehaltsabrechnungen, Interne Kommunikations- und Datenaustauschplattform) oder „Keiner davon passt"; erweiterbar |
| Auswahl des Anwendungstyps (inkl. offener *und* geschlossener Software) | Mehrfachauswahl aus: Website oder Webanwendung, Software (offen/geschlossen), Mobile Anwendung (offen/geschlossen), Dokument, Hardware, Vermittlungs- oder Notrufdienst |
| Auswahl der Funktionsbereiche | Mehrfachauswahl aus Basisfunktionalität (immer ausgewählt), Audio, Video, Zweiwege-Sprachkommunikation, Echtzeittext (RTT), Biometrie, Autorenwerkzeug; Erklärungen einblendbar |
| Auswahl von Prüfverfahren (optional) | Mehrfachauswahl aus BIT inklusiv Software-Test, App-Test, PDF-Test, BIK BITV-Test (Web), BIK BITV-Test (App); jede Auswahl ergibt ein Kriterium „Geprüft nach …" |
| Ausschlussregeln zwischen den Auswahlschritten | Unpassende Werte sind sichtbar, aber nicht wählbar, mit Grund; Änderung eines höheren Schritts setzt tiefere zurück, mit Hinweis |
| Kriterien in verständlicher Sprache | Grundlage: BIT inklusiv Software-, App- und PDF-Test (wörtlich, mit Erlaubnis); ergänzt um verständliche Titel und Kurzbeschreibungen |
| Empfohlene Gewichtungen/Priorisierungen je Verwendungszweck und Anwendungstyp | UI kennzeichnet Priorisierung klar als Expertenurteil, nicht als validierte Norm, mit Begründung je Kriterium |
| Export als Excel-Datei | Gefilterte Kriterien inkl. Gewichtungen sind exportierbar, auch ohne Konto |
| Workspace: eigene Produkteinträge anlegen & einschätzen | Nutzerin kann Kandidatenprodukt anlegen und anhand der Kriterien mit der Skala von BIT inklusiv einschätzen; jedes Projekt arbeitet mit einer eigenen Kopie seiner Kriterienliste (D1) |
| Optionale Veröffentlichung von Workspace-Einträgen | Veröffentlichung als Kanal 2; Kanal 1 und Kanal 2 sind im UI klar getrennt ausgewiesen |
| Direkte Veröffentlichung für Anbieter und Prüfstellen (Kanal 1) | Ohne Organisation und Projekt, mit dokumentierter Zustimmung des Auftraggebers |
| Basisfeld „Auf Barrierefreiheit geprüft: Ja/Nein" | Bei jedem Prüfergebnis-Eintrag immer sichtbar, unabhängig von Kanal und Ausgang; nicht bei Erfahrungsberichten |
| Getrennte Signale für Dokumentationsgrad und Erfüllungsgrad | Nie als gemeinsamer Score dargestellt; ohne Einschätzungen kein Abgleich (D2) |
| Filterung im Produktverzeichnis | Nach Anwendungstyp, Verwendungszweck (Einsatzkontext der Einträge) und Prüfverfahren, mit denselben Ausschlussregeln wie im Konfigurator |
| Pseudonyme Teilnahme für Kanal-2-Beiträge | Kein Klarnamenzwang, Assistenztechnologie pro Beitrag ausweisbar statt aggregiert; Einwilligung für jeden Erfahrungsbericht; Zurückziehen löscht Text und Angaben zu Hilfsmitteln (D4) |
| Merken und Teilen von Kriterienlisten und Produkten; Vorschläge zum Kriterienkatalog | Kriterienlisten speichern und über einen Freigabelink teilen, der ohne Konto lesbar ist (D7); Projekte werden in v1 nicht per Link geteilt; neue Liste mit derselben Auswahl nach einer neuen Katalogversion (D6); Produkt merken = Produkt aus dem Produktverzeichnis in ein Beschaffungsprojekt übernehmen (in der Projektübersicht); Produkt teilen = Link auf die öffentliche Produktseite; Vorschläge zum Katalog über einen externen Kanal (Kontakt zu WERTE.IT). Die genauen Akzeptanzkriterien legt SPEC_v1 fest |
| Barrierefreiheit des Tools selbst | Erfüllt die vom Tool propagierten Anforderungen (WCAG 2.1 AA / EN 301 549), mobile-first umgesetzt |
| Persönliche Einstellungen | Nutzerin kann individuelle Präferenzen speichern |

### Should-Have (P1)

- **VPAT/ACR als strukturiertes Einreichungsformat für Kanal 1** — Anbieter können statt freier Erfolgsberichte ein standardisiertes Format nutzen (Hochladen des Berichts).
- **Verlinkung zu externen Produktverzeichnissen** innerhalb des Workspace-Flows, ergänzend zur eigenen Produktfunktion.
- **Verweis auf WERTE.IT-Beratungsangebot**, wenn im Workspace keine passenden Einträge gefunden werden.
- **Basis-Moderationsfunktion** für Kanal-2-Einträge (Meldefunktion, Recht auf Gegendarstellung) — die vollständige rechtliche Ausgestaltung folgt vor dem Live-Gang.

### Später / Future Considerations (P2)

- **Integration organisationsinterner Bewertungsmodelle:** Kriterien werden mit organisationsspezifischer Gewichtung (z. B. Nutzwertanalyse, Scoring-Modell) exportiert.
- **Vergabetext-Generator:** aus priorisierten Kriterien generierter Formulierungsbaustein. Konkretisierter Ansatz aus dem Team-Feedback: bestehende, unter Creative-Commons-Lizenz stehende Textbausteine der [BFIT-Bund-Handreichungen](https://handreichungen.bfit-bund.de/textbausteine-vergabe-softwareentwicklung/) als Grundlage nutzen, KI-gestützt individuell an die Beschaffungssituation anpassbar — bewusst eng auf das Tool begrenzt (kein frei lernendes System). Strukturell vergleichbar mit dem **GPAT (Government Product Accessibility Template)** aus der US-Praxis, dem nachfrageseitigen Gegenstück zum VPAT. Muss klar als Formulierungsvorschlag zur Anpassung gekennzeichnet werden, nicht als rechtssicherer Fertigtext (siehe Offene Fragen: Rechtssicherheit).
- **Marktlagen-Übersicht:** aggregierte Übersicht je Anwendungstyp aus geteilten Einträgen (konkrete Ausgestaltung, inkl. möglicher Mindestschwelle an Einträgen, ist noch offen).
- **Import in Ausschreibungssoftware** der beschaffenden Stelle, abhängig von offengelegten Schnittstellen.
- **Perspektivische Einbindung veröffentlichter MLBF-Prüfergebnisse** als zusätzliche, glaubwürdige Kanal-2-Quelle für den B2C-relevanten Teil des Scopes.
- **Kriterien des BIK BITV-Tests (Web)** als eigene Texte für Websites.
- **Weitere Verwendungszwecke** aus der Vorschlagsliste des Kriterienkatalogs (z. B. Dokumentenmanagement und E-Akte, Zeiterfassung, Bewerbermanagement).

---

## 10. Erfolgsmetriken

Da ProcureAccess ein neues Tool in einem geförderten Forschungsprojekt ohne Vergleichsbasis ist, werden hier Indikatoren statt fixer Zielwerte definiert. Konkrete Zielwerte sollten nach der ersten Pilotierung gemeinsam mit dem Projektteam festgelegt werden.

**Leading Indicators (kurzfristig beobachtbar):**
- Anzahl abgeschlossener Kriterien-Konfigurationen
- Anzahl angelegter Workspace-Einträge
- Anteil veröffentlichter vs. rein privater Workspace-Einträge
- Anzahl durchgeführter Exporte (mit und ohne Konto)

**Lagging Indicators (langfristig beobachtbar):**
- Anzahl abgeschlossener Beschaffungsvorgänge mit Kooperationspartnern (Pilotierungen)
- Wachstum geteilter Kanal-1- und Kanal-2-Einträge über die Zeit (Indikator für Überwindung des Anschubproblems — auch die GSA hat dieses Problem für ihre eigene, neue ACR Repository trotz freiwilliger Einreichung noch nicht gelöst)
- Qualitatives Feedback aus Experten-Review (z. B. mit BIT inklusiv) bzw. aus Pilotierungen zur Priorisierungslogik

**Erhebung:** Die Indikatoren werden datensparsam erhoben: als Zählung vorhandener Datensätze oder als Nutzungsereignis ohne Personenbezug (nur Datum, keine Nutzer- oder Organisationskennung; ARCHITECTURE_v10 §5.7). Welche Ereignisse dafür nötig sind (z. B. „Kriterienliste angezeigt“, „Export erstellt“, jeweils mit und ohne Konto), legt SPEC_v1 fest.

---

## 11. Offene Fragen

| Frage | Wer sollte entscheiden |
|---|---|
| Prioritäten und Begründungen je Kriterium, Verwendungszweck und Anwendungstyp sowie verständliche Titel und Kurzbeschreibungen für die BIT-inklusiv-Kriterien (Voraussetzung für die P0-Gewichtungen) | Projektteam, vor dem Katalog-Migrationsschritt |
| Kriterien für Hardware sowie Vermittlungs- oder Notrufdienst (kein BIT-inklusiv-Verfahren vorhanden) | Projektteam |
| Fachliche Prüfung der Erklärungstexte zu den Bewertungsstufen | Projektteam, ggf. mit BIT inklusiv |
| Soll DIN EN 17161 in späteren Phasen berücksichtigt werden, und wenn ja, wie? | Projektteam |
| Wie werden erste Nutzerinnen vor Erreichen eines Netzwerkeffekts motiviert (Anschubproblem)? | Projektteam, ggf. Kooperationspartner |
| Wie sieht die vollständige Moderationsrichtlinie für Kanal-2-Einträge aus, inkl. rechtlicher Prüfung? | Rechtliche Prüfung / Trägerorganisation, vor Live-Gang |
| Wie wird die Marktlagen-Übersicht konkret ausgestaltet (z. B. Mindestschwelle an Einträgen)? | Projektteam, spätere Phase |
| Welcher Validierungsweg für die Priorisierungslogik wird gewählt (Experten-Review vs. Pilotierung)? | Projektteam |
| **Gebündelte rechtliche Prüfung** — (1) rechtliche Tragfähigkeit kritischer Kanal-2-Einträge, (2) Bindungswirkung von VPAT/ACR-Daten bei Übernahme in einen Vergabetext, (3) Vollständigkeit/Rechtssicherheit eines späteren Vergabetext-Generators, (4) Einordnung des US-amerikanischen BuyAccessible-Wizard-/ART-Präzedenzfalls, (5) Echtheit von Anbieter- und Prüferangaben inkl. selbst erklärter Zustimmung, (6) Wortlaut und Rechtsgrundlage der Einwilligung bei Erfahrungsberichten einschließlich Angaben zur Assistenztechnologie (Art. 9 DSGVO; entschieden: Einwilligung für jeden Bericht) sowie Zeitpunkt der Löschung nach einem Widerruf (vorläufig: sofort), (7) Löschung vs. Moderationsprotokoll | Rechtliche Prüfung (gebündelt, vor Detailspezifikation der betroffenen Features) |
| Wer übernimmt die Trägerschaft von ProcureAccess nach Ende der Projektförderung? Mehrere Produktentscheidungen hängen laut Team-Einschätzung von dieser Klärung ab | Projektleitung |

---

## 12. Zeitliche Rahmenbedingungen

Aus den Grundlagendokumenten sind keine harten externen Deadlines ersichtlich. ProcureAccess ist Teil des geförderten Forschungsprojekts WERTE.IT; Praxistests und Pilotierungen mit Kooperationspartnern sind geplant, die Suche nach entsprechenden Partnern wurde bereits angestoßen.

**Phasierung:** P0-Umfang vor P1/P2. Eine Pilotierung mit mindestens einem Kooperationspartner ist ein sinnvoller Meilenstein zur Validierung der Priorisierungslogik (siehe Abschnitt 5, Ziel 4) und sollte zeitlich vor einer breiteren Veröffentlichung liegen.

---

## 13. Zentrale Annahmen (zur Prüfung während der Entwicklung)

- Nutzerinnen sind bereit, projektbezogene Workspace-Einträge zu veröffentlichen — vermutlich unterschiedlich stark je nach Nutzertyp.
- Prüferinnen/Prüfbüros sind bereit, bezahlte Prüfergebnisse mit Zustimmung ihres Auftraggebers öffentlich sichtbar zu machen.
- Die Priorisierungslogik nach Verwendungszweck und Anwendungstyp hält einer Praxisprüfung stand — bislang nicht getestet.
- Kooperationspartner sind bereit und in der Lage, als vermittelnde Instanz für die Beteiligung von Menschen mit Behinderung zu fungieren.
- Anbieter sind bereit, freiwillig ACRs/VPATs einzureichen — diese Annahme ist für den deutschen Markt tendenziell weniger abgesichert als im US-Kontext: Selbst die GSA, mit deutlich größerem Beschaffungsvolumen und ganz ohne Einreichungspflicht, betrachtet dieses Anreizproblem für ihre eigene, neu aufgebaute ACR Repository noch nicht als gelöst, und in der EU fehlt zusätzlich der regulatorische Sog aus Section 508.
- Die Texte von BIT inklusiv bleiben stabil genug, dass ein Abgleich je Katalogversion ausreicht.

---

## 14. Grundlagendokumente

- `anforderungen_beschaffungstool_procureaccess_werteit_v2.md` — Anforderungsdokument, Ergebnis Chat 1.1
- `brainstorm_validierung_procureaccess_werteit.md` — Brainstorm & Validierung, vollständige Entscheidungshistorie inkl. verworfener Alternativen und Marktkontext-Recherche
- `kriterienkatalog_bit_inklusiv_v1.md` (1.5, mit Teil A und B) — Kriterienbasis, Filterschichten, Ausschlussregeln, Bewertungsskala
- Phase-2-Dokumente: `ARCHITECTURE_v10.md`, `TESTING_v8.md`, `ux_copy_procureaccess_v12.md`, `design_handoff_procureaccess_v1.md` (1.12), `figma_code_mapping_procureaccess_v1.md` (1.10), `architektur_review_v6.md`
- `pruefung_hauptdokumente_v1.md` — Prüfung der Hauptdokumente vor SPEC_v1 (13. Oktober 2026) mit den Entscheidungen D1–D7

---

## 15. Änderungen gegenüber v4

| # | Änderung | Anlass |
|---|---|---|
| Q5 | Ein Beschaffungsprojekt erhält beim Zuordnen eine eigene Kopie der Kriterienliste; Verwendungszweck, Anwendungstypen, Funktionsbereiche und Prüfverfahren stehen danach fest (Abschnitte 7, 9) | Entscheidung D1, 13.10.2026 |
| Q6 | Einträge der Anbieterseite ohne Einschätzungen zeigen keinen Abgleich (Abschnitte 7, 9) | Entscheidung D2 |
| Q7 | Widerruf der Einwilligung löscht Text und Angaben zu Hilfsmitteln eines Erfahrungsberichts; den Zeitpunkt legt die rechtliche Prüfung fest (Abschnitte 4, 9, 11) | Entscheidung D4 |
| Q8 | Erfahrungsberichte: Anwendungstyp optional, kein Verwendungszweck (Abschnitt 7) | Entscheidung D5 |
| Q9 | Neue Liste mit derselben Auswahl nach einer neuen Katalogversion (Abschnitte 7, 9) | Entscheidung D6 |
| Q10 | Freigabelinks für Kriterienlisten ohne Konto lesbar, keine Freigabelinks für Projekte in v1; „Merken“ und „Teilen“ von Produkten beschrieben (Abschnitt 9) | Entscheidung D7; Prüfung A |
| Q11 | Erhebung der Erfolgsmetriken (Abschnitt 10) | Prüfung A |
| Q12 | Verweise auf die aktuellen Phase-2-Dokumente | Prüfung 13.10.2026 |

## 16. Änderungen gegenüber v3

| # | Änderung | Anlass |
|---|---|---|
| Q1 | Ein Produkt kann nach mehreren Prüfverfahren geprüft sein; „Geprüft nach …“ ergibt sich aus den Prüfnachweisen | Entscheidung 12.10.2026 (Architektur-Review) |
| Q2 | Für jeden Erfahrungsbericht wird eine Einwilligung eingeholt; Zurückziehen des Berichts gilt als Widerruf | Entscheidung 12.10.2026 (Architektur-Review) |
| Q3 | Die Kriterienliste eines Beschaffungsprojekts kann angepasst, aber nach den ersten Einschätzungen nicht mehr gegen eine andere ausgetauscht werden | Entscheidung 12./13.10.2026 |
| Q4 | Verweise auf die aktuellen Phase-2-Dokumente | Rückmeldung 13.10.2026 |

## 17. Änderungen gegenüber v2

| # | Änderung | Anlass |
|---|---|---|
| P1 | **Begriffe:** „Anwendungstyp" bezeichnet jetzt die Art der IT-Lösung (früher „Softwaretyp"); der frühere „Anwendungstyp/Produkt-Typ" heißt durchgehend „Verwendungszweck". „Produktteile" heißen „Funktionsbereiche", „Test-Typen" heißen „Prüfverfahren". Reihenfolge der Auswahl: Verwendungszweck zuerst | Design-Review 06.10.2026 |
| P2 | Starter-Set des Verwendungszwecks ohne „Fachanwendung"; neu „Keiner davon passt" | Entscheidung 06.10.2026 |
| P3 | Kriterienbasis: BIT inklusiv Software-, App- und PDF-Test statt nur „BITV-Softwaretest"; Bewertungsskala mit sechs Stufen | Entscheidung 06.10.2026 |
| P4 | Prüfverfahren und Ausschlussregeln sind P0 (vorher Test-Typ-Filter P1) | Design-Review 06.10.2026 |
| P5 | „Geprüft ja/nein" gilt für Prüfergebnis-Einträge; Erfahrungsberichte haben keinen Prüfstatus; im Workspace zusätzlich „Nicht bekannt" | Entscheidungen E8, E13 |
| P6 | Workspace veröffentlicht nur in Kanal 2; Kanal 1 über direkte Veröffentlichung von Anbietern und Prüfstellen | ARCHITECTURE C17, C18 |
| P7 | „Vorschlagen" bezieht sich auf den Kriterienkatalog und läuft über einen externen Kanal; rechtliche Prüfung um Echtheit von Anbieterangaben, Einwilligung zu Assistenztechnologie und Löschung erweitert | ARCHITECTURE §16 |
| P8 | Gewichtungen je Verwendungszweck und Anwendungstyp | Begriffe |
| P9 | Anwendungstypen und Verwendungszwecke am Eintrag, nicht am Produkt; Filter im Produktverzeichnis | Entscheidung 12.10.2026 (Variante A) |
| P10 | Export auch ohne Konto; mobile-first; kein Gesamturteil ausdrücklich als Nicht-Ziel | ARCHITECTURE D15, Entscheidung 12.10.2026 |
