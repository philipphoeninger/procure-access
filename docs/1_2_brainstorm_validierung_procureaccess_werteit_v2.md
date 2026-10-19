# Brainstorm & Validierung: ProcureAccess (Projekt WERTE.IT)

> **Status:** Ergebnisdokument aus Chat "1-2 Brainstorm & Validate", Phase 1 des Entwicklungsprozesses
> **Grundlage:** `anforderungen_beschaffungstool_procureaccess_werteit_v2.md`
> **Zweck:** Einstiegspunkt für Chat 3 ("Create the PRD"). Enthält kein Datenmodell, keine Architektur, keine UI-Details — diese entstehen erst in Phase 2.
> **Team-Feedback:** Konzept wurde dem Projektteam vorgestellt (schriftlich + Feedback-Meeting) und positiv validiert; zur konkreten Umsetzung freigegeben. Details siehe Abschnitt 8.

---

## 1. Verfeinertes Konzept

Das Kernkonzept aus v2 (Kriterien-Konfigurator + priorisierte Empfehlungen je Softwaretyp/Verwendungszweck + Export) bleibt bestehen und wurde in diesem Chat nicht infrage gestellt. Die zentrale Weiterentwicklung betrifft die **Produkt-Komponente**:

Statt einer von WERTE.IT zentral gepflegten, globalen Produkt-Datenbank wird ein **Workspace-Modell** vorgeschlagen: "Produkt" ist kein globaler Katalogeintrag mit Wahrheitsanspruch von WERTE.IT, sondern ein Objekt im konkreten Beschaffungsvorgang der Nutzerin. Nutzerinnen legen eigene Kandidatenprodukte an, bewerten sie anhand der vorgeschlagenen Kriterien und können ihre Einträge optional veröffentlichen. Aus diesen optional geteilten Einträgen entsteht mit der Zeit organisch eine Produktübersicht — ohne dass WERTE.IT eine globale Datenbank aktualisiert halten muss.

Damit besteht ProcureAccess im Kern aus zwei Säulen:
1. **Kriterien-Konfigurator** (Auswahl Softwaretyp/Verwendungszweck → priorisierte, verständliche Kriterien → Export)
2. **Workspace mit optionaler Veröffentlichung** (projektbezogene Produktrecherche, die bei Freigabe zu einer community-getragenen Referenz wird)

---

## 2. Zentrale Entscheidungen & Begründungen

### 2.1 Verworfen: Zentrale, von WERTE.IT gepflegte Produkt-Datenbank

**Ursprüngliche Idee (aus v2):** Eine von WERTE.IT selbst befüllte und aktuell gehaltene Datenbank bekannter Softwareprodukte mit Barrierefreiheits-Metadaten.

**Grund der Verwerfung:** Klassisches Henne-Ei-Problem eines Verzeichnis-Produkts — ohne kritische Masse an Einträgen ist die Datenbank für Nutzerinnen nicht wertvoll, ohne Wert keine Motivation zur Mitwirkung. Zusätzlich: laufende Wartungslast (Software-Versionen und Barrierefreiheitsstatus ändern sich kontinuierlich), unklare Zuständigkeit nach Ende der Projektförderung. Als Konzeptionierungsprojekt trägt WERTE.IT aktuell keine Verpflichtung gegenüber dem Fördergeber, eine aktive Marktübersicht aufzubauen — das Konzept darf sich also an dieser Stelle frei ändern.

**Ersetzt durch:** Workspace-Modell (siehe 1.).

**Bestätigung aus der Team-Rückmeldung:** Aus Business-/Beratungsperspektive wurden zwei weitere, unabhängige Argumente für das Workspace-Modell genannt: (1) niedrigere Hemmschwelle zur Nutzung, da Wettbewerber nicht mitbekommen, in welchem Stadium der digitalen Barrierefreiheit sich ein Unternehmen befindet; (2) Datenschutzvorteile, die auch eine mögliche **Fortführung von ProcureAccess bei einem geeigneten Träger nach Ende der Projektförderung** erleichtern würden (siehe dazu auch den offenen Punkt in Abschnitt 6). Aus dem Feedback-Meeting wurde ergänzend hervorgehoben, dass das Workspace-Modell keinen **"Meldestellen-Charakter"** hat — ein Wahrnehmungsvorteil, der zur in Abschnitt 4 diskutierten Positionierung passt.

### 2.2 Verworfen: Reiner Verweis auf externe Produktverzeichnisse ohne eigene Produktfunktion

**Idee:** ProcureAccess bleibt reiner Kriterien-Konfigurator, Produktrecherche erfolgt vollständig über Links zu bestehenden externen Verzeichnissen.

**Grund der Verwerfung:** Reduziert Wartungslast stark, macht das Tool aber inhaltlich zu dünn — der eigenständige Mehrwert gegenüber reinen Leitfäden würde verloren gehen.

**Teilweise übernommen als:** Ergänzende Verlinkung zu externen Quellen *innerhalb* des Workspace-Flows, nicht als Ersatz für die Produktfunktion.

### 2.3 Verworfen (als Ersatz für Produktfunktion): Beratungsverweis statt Produktrecherche

**Idee:** Statt eigener Produktfunktion verweist das Tool an der Stelle "welche Produkte kommen infrage" auf die ohnehin von WERTE.IT angebotenen Beratungsleistungen.

**Grund der Verwerfung:** Würde den eigenständigen Produktwert auf den Kriterien-Konfigurator reduzieren; Mehrwert des Tools würde fraglich. Zudem ein in der Praxis verbreitetes, wenig differenzierendes Muster ("neue Regulierung → Beratungsangebot").

**Teilweise übernommen als:** Optionaler Zusatzlink im Workspace-Flow (z. B. "keine passenden Einträge gefunden? → Beratungsangebot von WERTE.IT"), nicht als Kernfeature.

### 2.4 Verworfen: Wikipedia-Stil-Konsens-Modell für Produktseiten

**Idee:** Eine von einem Redaktionsteam zusammengeführte "Wahrheit" pro Produkt (Modell 1).

**Grund der Verwerfung:** Verschiebt das Wartungslast-Problem der zentralen Datenbank (2.1) lediglich von Dateneingabe zu Moderationsaufwand, statt es zu lösen. Suggeriert zudem eine Zertifizierungs-/Prüfautorität, die dem expliziten Nicht-Ziel des Tools widerspricht (siehe v2, Abschnitt 5.3: kein Audit-/Zertifizierungstool).

**Ersetzt durch:** Review-Modell (Modell 2) — mehrere, auch widersprüchliche Einträge stehen nebeneinander, ohne redaktionellen Konsens-Zwang. Kein Moderationsteam für inhaltliche Wahrheitsfindung nötig, nur Grund-Spam-/Missbrauchsschutz.

### 2.5 Zurückgestellt (nicht verworfen): Vergabetext-Generator

**Idee:** Aus den ausgewählten, priorisierten Kriterien wird ein exportierbarer Vergabetext-Baustein mit Gewichtungen generiert.

**Einschätzung:** Kein Scope-Risiko wie die Produkt-Datenbank, da es sich um eine zustandslose Transformation bereits vorhandener Daten handelt (kein neuer, laufend zu pflegender Datenbestand) — im Kern eine Erweiterung der ohnehin geplanten Exportfunktion. Sinnvoll als **spätere Ausbaustufe**, nicht als Teil des MVP.

**Zu beachten bei späterer Umsetzung:** Ergebnis sollte klar als Formulierungsbaustein zur Anpassung gekennzeichnet werden, nicht als rechtssicherer Fertigtext — um keine falsche Erwartungshaltung an Vollständigkeit/Rechtssicherheit zu wecken.

**Konkretisierung aus der Team-Rückmeldung:** Die Idee wurde grundsätzlich als attraktiv bewertet. Ein konkreter Umsetzungsvorschlag des Barrierefreiheitsexperten im Team: Statt Vergabetexte vollständig neu zu generieren, könnten bereits veröffentlichte Textbausteine als Grundlage dienen — z. B. "Digitale Barrierefreiheit – Teil 1: Entwicklung von Software" der BFIT-Bund-Handreichungen (https://handreichungen.bfit-bund.de/textbausteine-vergabe-softwareentwicklung/), nach Auskunft aus dem Team unter Creative-Commons-Lizenz. Eine KI-gestützte Chat-Funktion könnte diese Bausteine individuell an die jeweilige Beschaffungssituation anpassbar machen — bewusst eng auf das Tool begrenzt (kein frei lernendes System), da eine vollständig freie Text-Generierung selbst vom Vorschlagenden als möglicherweise zu ambitioniert eingeschätzt wurde.

*Hinweis:* Es existiert zusätzliches, noch nicht veröffentlichtes BFIT-Material zu diesem Thema (Philipp liegt vor, vertraulich, Veröffentlichung voraussichtlich im Herbst) — als weitere Grundlage für eine spätere Umsetzung vorgemerkt, hier bewusst nicht inhaltlich wiedergegeben.

**Begriffliche Parallele aus der US-Praxis:** Das **GPAT (Government Product Accessibility Template)** ist dort das Gegenstück zum VPAT — während das VPAT vom Anbieter zur Beschreibung der Konformität ausgefüllt wird, formuliert die beschaffende Stelle mit einem GPAT die geforderten Kriterien für eine Ausschreibung. Das entspricht strukturell dem, was Kriterien-Konfigurator + Vergabetext-Generator zusammen leisten würden — eine nützliche Einordnung für die spätere Kommunikation dieses Features.

**Weiterhin offen:** Die Sorge um Vollständigkeit/Rechtssicherheit (s. o.) wurde von mehreren Seiten geteilt und explizit als der entscheidende Punkt vor einer Umsetzung benannt — siehe rechtliche Prüfung in Abschnitt 6.

### 2.6 Aufgenommen als Verfeinerung von Kanal 1: VPAT/ACR als strukturiertes Anbieter-Format

**Idee:** Statt Anbieter im Anbieter-/Prüfer-Kanal (Kanal 1, siehe 3.1) nur freie Erfolgsberichte einreichen zu lassen, könnte ein standardisiertes **VPAT (Voluntary Product Accessibility Template)** bzw. das damit ausgefüllte **ACR (Accessibility Conformance Report)** als Format dienen. VPAT/ACR ist ein international etablierter, freiwilliger De-facto-Standard (ursprünglich aus den USA, inzwischen mit eigener EU-Edition für EN 301 549), der Produkte kriterienweise bewertet ("Supports" / "Partially Supports" / "Does Not Support" / "Not Applicable").

**Einschätzung:** Sinnvolle Verfeinerung von Kanal 1, kein Ersatz für Kanal 2. Ein ACR ist zwar ebenfalls eine Selbstauskunft des Anbieters (keine unabhängige Prüfung), aber durch das Berichtsformat entsteht ein realistischerer Zwischenweg zwischen reiner Marketing-Aussage und unabhängiger Prüfung: Da das Genre "Konformitätsbericht" auch das Eingeständnis einzelner nicht erfüllter Kriterien vorsieht, ist die zu erwartende Verzerrung geringer als bei einer freien Erfolgsgeschichte — sie bleibt aber bestehen, da niemand die Angaben unabhängig verifiziert.

**Wichtige Ergänzung zum Zwei-Kanal-Prinzip (siehe auch 3.1):** "Dokumentationsgrad" (wurde ein ACR eingereicht, wie aktuell ist es) und "Erfüllungsgrad" (wie viele der für den jeweiligen Anwendungsfall relevanten Kriterien werden erfüllt) müssen als **zwei getrennt sichtbare Signale** dargestellt werden, nicht als ein gemeinsamer Status/Score. Sonst entsteht ein Fehlanreiz: Ein Anbieter mit einem ehrlichen, aber schlechten ACR dürfte im UI nicht besser aussehen als einer ganz ohne Eintrag, nur weil er mehr Dokumentation geliefert hat.

**Kein rechtliches Pflichtformat in Deutschland/EU:** Das deutsche Barrierefreiheitsstärkungsgesetz (BFSG) kennt kein VPAT/ACR-Äquivalent. Die gesetzlich vorgeschriebene **EU-Konformitätserklärung** (§ 18 BFSG) plus **technische Dokumentation** (Anlage 2 BFSG) ist ein formales Dokument ohne kriterienweise Aufschlüsselung, wird primär beim Hersteller aufbewahrt und nur auf Verlangen an Behörden herausgegeben — kein für Einkäuferinnen durchsuchbares Vergleichsformat wie ein ACR. VPAT/ACR bleibt also der relevante Ansatzpunkt für Kanal 1, unabhängig von der deutschen Gesetzeslage.

**Kritische Rückmeldung aus dem Team (Barrierefreiheitsexperte):** Anbieter-Selbstauskünfte (VPAT/ACR) sind in der Praxis häufig beschönigend dargestellt — mit der Konsequenz, dass Menschen mit Behinderung, die auf Basis solcher Einschätzungen eingestellt werden, am Arbeitsplatz dann doch auf massive Unterstützung durch das Integrationsamt oder auf technische Workarounds (z. B. individuelles Screenreader-Scripting mit kurzer Haltbarkeit durch häufige Update-Zyklen der Hersteller) angewiesen sind. Der reale Nutzen eines VPAT/ACR für die tatsächliche Arbeitsplatz-Barrierefreiheit wird daher als gering eingeschätzt — als **Zusatzinformation neben Kanal 2** aber sinnvoll. Aus dem Feedback-Meeting bestätigt: Kanal 1 zur positiven Anbieter-Selbstdarstellung ist grundsätzlich in Ordnung, Kanal 2 sollte aber juristisch geprüft werden.

**Gegenposition aus der Team-Rückmeldung (Business-/Beratungsperspektive):** Die fehlende rechtliche Verbindlichkeit von VPAT/ACR in der EU muss kein Nachteil sein — entscheidend ist, was ProcureAccess daraus macht. Wird im Tool transparent gemacht, dass referenzierte Kriterien bei Übernahme in den Vergabetext zum verbindlichen Vertragsbestandteil werden, entsteht eine reale rechtliche Verbindlichkeit, unabhängig davon, dass VPAT/ACR selbst kein gesetzlich vorgeschriebenes Format ist. Eine Meeting-Notiz stützt diese Einschätzung: VPATs können in bestimmten Konstellationen bereits heute rechtlich bindend sein, teils mit vertraglich vereinbarten Konventionalstrafen bei Nichteinhaltung — dies müsste für den deutschen/EU-Kontext aber noch juristisch bestätigt werden (siehe Abschnitt 6).

**Einordnung:** Beide Rückmeldungen widersprechen sich nicht, sondern beschreiben zwei unterschiedliche Verwendungsweisen: VPAT/ACR als eigenständige Qualitätsaussage (hier berechtigte Skepsis) versus als Grundlage für vertraglich verbindliche Vergabekriterien (hier ein möglicher Mehrwert). Für die weitere Konzeption spricht das dafür, VPAT/ACR-Daten im Tool klar von der Frage zu trennen, ob und wie sie in einen rechtlich bindenden Vergabetext übernommen werden.

---

## 3. Feste Prinzipien (aus der Diskussion abgeleitet, für PRD/Architektur bindend)

### 3.1 Zwei-Kanal-Prinzip für Produkteinträge

Produktbezogene Einträge im Workspace unterscheiden sich strukturell nach Herkunft und müssen im UI klar getrennt ausgewiesen werden:

- **Kanal 1 — Anbieter-/Prüfer-Kanal:** freiwillig, zustimmungspflichtig (Anbieter/Prüferin muss der Veröffentlichung zustimmen). Strukturell positiv verzerrt — niemand veröffentlicht freiwillig ein schlechtes Ergebnis. Dient als Sichtbarkeits-/Reputationskanal (Werbeeffekt für Prüferin *und* Anbieter), **nicht** als objektive Informationsquelle.
- **Kanal 2 — unabhängige Institutions-/Nutzer-Einträge:** keine Zustimmung des Anbieters nötig (analog zu Kundenbewertungen). Kann auch kritische/negative Erfahrungsberichte enthalten. Schließt die strukturelle Lücke, die Kanal 1 zwangsläufig offenlässt.

**Verpflichtendes, ungefiltertes Basisfeld:** Das Faktum "wurde getestet: ja/nein" muss unabhängig vom Ausgang und unabhängig vom Kanal sichtbar sein. Nur die ausführliche, kuratierte Darstellung ist optional und darf bei Kanal 1 auf positive Ergebnisse beschränkt bleiben.

**Trennung von Dokumentationsgrad und Erfüllungsgrad (siehe auch 2.6):** Innerhalb von Kanal 1 muss zusätzlich zwischen "wie viel Dokumentation wurde eingereicht" (z. B. ACR vorhanden, Einreichungs-/Aktualisierungsdatum) und "wie gut erfüllt das Produkt die relevanten Kriterien" unterschieden werden. Beides als einen gemeinsamen Status darzustellen, würde Transparenz mit Qualität verwechseln und Anbieter mit viel, aber schlechter Dokumentation gegenüber undokumentierten Anbietern bevorzugen.

### 3.2 Prinzipien für die Beteiligung von Menschen mit Behinderung als Beitragende

Als möglicher dritter Beitragenden-Typ (neben den Personas Einkäuferin/Ausschreibungsverantwortliche) wurde die aktive Einladung von Menschen mit Behinderung zur Teilhabe diskutiert — explizit als Mitgestaltungsangebot, nicht als unentgeltliche Qualitätssicherung. Feste Prinzipien dafür:

- **Pseudonymität muss möglich sein.** Kein Zwang zu Klarnamen-Profilen — Risiko von Diskriminierung, Stalking, ungewollter Offenlegung einer Behinderung gegenüber Dritten (z. B. Arbeitgebern).
- **Keine Verallgemeinerung einzelner Stimmen.** Ein Erfahrungsbericht (z. B. von einer Screenreader-Nutzerin) darf nicht als "die Betroffenen-Meinung" zu einem Produkt aggregiert werden. Art der Behinderung/genutzte Assistenztechnologie sollte pro Beitrag ausgewiesen werden, nicht in eine einzige Bewertung verschmolzen.
- **Zugang bevorzugt über Kooperationspartner/Verbände**, nicht über offene Kaltakquise einzelner Personen — wirkt weniger extraktiv und bringt eine gewisse Repräsentativität/Qualitätssicherung mit.

### 3.3 Umgang mit negativen Einträgen und Löschforderungen (Kanal 2)

Kritische/negative Einträge (Kanal 2) bergen das Risiko von Löschforderungen durch betroffene Anbieter, ggf. mit rechtlichen Drohungen — ein bekanntes Risiko bei Bewertungsplattformen. Festgehaltene Grundprinzipien:

- Moderationsrichtlinie von Anfang an nötig: Löschung nur bei nachweislich falschen Tatsachenbehauptungen, nicht bei unliebsamen, aber zutreffenden Einträgen.
- Bevorzugt Recht auf Gegendarstellung statt Löschung (Anbieter kann öffentlich antworten, statt dass der Eintrag entfernt wird).
- Einträge sollten so weit möglich faktenbasiert statt meinungsbasiert formuliert werden (z. B. "getestet am [Datum] nach BITV-Softwaretest-Methode, X von Y Kriterien nicht erfüllt" statt wertender Aussagen) — rechtlich robuster.
- **Für Architekturphase vorgemerkt:** Dieses Risiko braucht eine dokumentierte Moderationsrichtlinie und eine rechtliche Prüfung/Anbindung an die Trägerorganisation, sobald das Tool live geht — nicht Teil der jetzigen Konzeptphase, aber nicht vergessen.

### 3.4 Transparenz der Priorisierungslogik

Die Priorisierung von Kriterien je Anwendungstyp basiert auf Expertenurteil, nicht auf validierter Evidenz. Dies muss im UI klar als solches gekennzeichnet werden (nicht wie eine geprüfte Norm wirken) — konsistent mit dem Nicht-Zertifizierungs-Anspruch des Tools (v2, 5.3). Mögliche spätere Validierungswege (nicht entschieden, nur als Option festgehalten): Experten-Review mit BIT inklusiv, Pilotierung eines Templates an einer realen Beschaffung mit einem Kooperationspartner.

---

## 4. Positionierung & Zielgruppenansprache

- **Kein allgemeines Vendor-/Procurement-Tool.** Der Markt für generische Vendor-Evaluierungssoftware ist dicht und ausgereift; ProcureAccess soll bewusst barrierefreiheits-spezifisch bleiben, um differenziert und im Rahmen der Projektressourcen realistisch zu bleiben.
- **Nach außen:** explizite Positionierung als Barrierefreiheits-Tool — die Zielgruppe sucht aktiv nach Orientierung zu diesem Thema und wird über diesen Rahmen erreicht.
- **Innerhalb des Tools (UX-Ebene, nicht Positionierungsebene):** Barrierefreiheit sollte sich im Workflow wie ein selbstverständlicher, normaler Bewertungspunkt anfühlen, nicht wie ein bürokratischer Zusatzschritt. Diese Abwägung gehört inhaltlich in eine spätere Design-/UX-Copy-Phase, nicht ins Anforderungsdokument.

---

## 5. Marktkontext (Rechercheergebnisse — zur eigenen Prüfung durch Philipp markiert)

Im Rahmen dieses Chats wurde eine kurze Recherche zu vergleichbaren Ansätzen durchgeführt. **Diese Ergebnisse sind vom Projektteam noch gegenzuprüfen**, werden aber als relevanter Kontext festgehalten:

- In den USA existierte mit dem **BuyAccessible Wizard** (GSA) über viele Jahre ein Tool mit sehr ähnlicher Zielsetzung: Klärung, ob eine Beschaffung unter Section 508 fällt, Marktrecherche zu Produkten/Firmen, sowie generierte Vergabetext-Bausteine.
- Dieses Tool wurde inzwischen durch das **Accessibility Requirements Tool (ART)** abgelöst — mit einer strukturellen Änderung: Die Marktrecherche-/Produktverzeichnis-Funktion ist heute von der Anforderungsformulierung getrennt und läuft über eine **ACR Repository / ACR Library** (Sammlung eingereichter Accessibility Conformance Reports), während ART sich auf die Formulierung von Vergabeanforderungen konzentriert.
  - **Korrektur zu einer früheren Aussage in diesem Chat:** Ursprünglich wurde hier ein "Vendor Accessibility Resource Center (VARC)" als das für Marktrecherche zuständige Tool genannt. Dieser Name taucht auf einer älteren Section508.gov-Seite (zuletzt aktualisiert April 2018) auf, dort allerdings **ohne funktionierenden Link** und **nicht mehr in der aktuellen Tool-Übersicht** von Section508.gov. Es ist unklar, ob VARC als eigenständiges Tool noch existiert oder in die heutige ACR Repository/ACR Library aufgegangen ist. Der Name VARC sollte daher **nicht** als gesicherte aktuelle Bezeichnung übernommen werden.
  - **Wichtiger Fund zur ACR Repository selbst:** Laut einer Bekanntmachung im Federal Register (Juni 2026) ist die zentrale ACR Repository der GSA eine **neue, gerade erst im Aufbau befindliche Initiative** — nicht ein seit Jahren etabliertes, bewährtes System. Zudem stellt GSA dort ausdrücklich klar: *"There is no obligation for product owners to submit ACRs for any product"* — die Einreichung ist freiwillig, keine Pflicht. Selbst die GSA mit ihrem großen Beschaffungsvolumen und langjähriger VPAT/ACR-Verbreitung im Markt hat das Anreizproblem für vollständige Anbieter-Teilnahme also noch nicht abschließend gelöst, sondern experimentiert aktuell selbst damit.
- **Einordnung:** Diese Entwicklung von "ein Tool für alles" zu "getrennte, lose gekoppelte Werkzeuge" bei einem deutlich ressourcenstärkeren Akteur als einem Forschungsprojekt stützt die in diesem Chat getroffene Entscheidung, Kriterien-Konfigurator und Produktfunktion konzeptionell zu entkoppeln. Der Befund zur Freiwilligkeit relativiert aber die Annahme, dass "Wartungslast auf Anbieter auslagern" das Henne-Ei-Problem automatisch löst (siehe auch Abschnitt 7).
- **Korrektur nach Team-Feedback — der Standardanforderungskatalog des Bundes existiert bereits als interaktives Tool:** Die frühere Aussage in diesem Abschnitt, für den deutschsprachigen Raum gebe es kein vergleichbares interaktives Tool, war in dieser Pauschalität **falsch** und wurde vom Barrierefreiheitsexperten im Team zu Recht korrigiert. Der **Standardanforderungskatalog** des Bundes ([Link](https://www.barrierefreiheit-dienstekonsolidierung.bund.de/Webs/PB/DE/standardanforderungskatalog/standardanforderungskatalog-node.html)) — der bereits als fachliche Referenz für die Filterstruktur von ProcureAccess dient (siehe v2, Abschnitt 5.1) — verfügt laut eigenem Cookie-Hinweis der Seite über eine **Filterfunktion mit Export der getroffenen Auswahl**, ist also kein statisches Dokument, sondern ein interaktives Werkzeug. Der eigentliche Filterbereich konnte im Rahmen dieser Recherche technisch nicht vollständig eingesehen werden (JavaScript-basiert) — eine praktische Prüfung durch das Projektteam wird empfohlen, um den genauen Funktionsumfang und die Abgrenzung zu ProcureAccess sauber zu bewerten.
  - **Vorläufige Neueinschätzung der Differenzierung:** Der Standardanforderungskatalog liefert nach bisherigem Kenntnisstand eine Filterung nach App-Typ/Produkt-Teil in normnaher Sprache. Der eigenständige Mehrwert von ProcureAccess läge primär in: (a) der Übersetzung der Kriterien in **verständliche, nicht-normnahe Sprache**, (b) der **Priorisierung nach konkretem Verwendungszweck** (Anwendungstyp-Templates wie HR-Anwendung, Fachanwendung), die im Standardanforderungskatalog vermutlich nicht in dieser Form vorgesehen ist, und (c) der **gesamten Produkt-/Workspace-Komponente**, die dort nicht existiert. Diese Einschätzung sollte im PRD-Chat nochmal explizit gegen eine Hands-on-Prüfung des Katalogs durch das Team abgeglichen werden.
- Losgelöst von diesem einen Fund bleibt aber im Kern richtig: Weiteres Material (Handlungsleitfaden des BFIT-Bund, Vergabebaustein des BMI, Leitfäden einzelner Behörden/Hochschulen) besteht überwiegend aus statischen Dokumenten/Checklisten. Die frühere Aussage einer "vollständigen Marktlücke bei interaktiven Tools" muss aber abgeschwächt werden zu: **eine Lücke bei der Kombination aus verständlicher Kriteriensprache, anwendungsfallbezogener Priorisierung und Produktrecherche** — nicht eine generelle Lücke bei interaktiven Tools im DACH-Raum.
- **Kein EU-/DACH-weites VPAT/ACR-Register gefunden.** Der European Accessibility Act (EAA) schreibt VPAT/ACR nicht vor und erwähnt das Format nicht — anders als in den USA (Section 508 + Federal Acquisition Regulation) gibt es in der EU keinen vergleichbaren regulatorischen Sog, der Anbieter zu VPAT/ACR-Einreichungen bewegen würde. Das gesetzlich verankerte deutsche Pendant, die EU-Konformitätserklärung nach BFSG, hat eine andere Funktion und Detailtiefe (siehe 2.6).
- **Marktüberwachungsbehörde (MLBF):** Die "Marktüberwachungsstelle der Länder für die Barrierefreiheit von Produkten und Dienstleistungen" (MLBF, Sitz Magdeburg) ist seit 26.09.2025 als Vollzugsbehörde für das BFSG tätig und seit Januar 2026 in aktiver Kontrollphase (proaktive, teils automatisierte Kontrollen sowie reaktive Beschwerdebearbeitung, Fokus auf reichweitenstarke Angebote). **Wichtige Scope-Einschränkung:** Das BFSG — und damit die Zuständigkeit der MLBF — gilt ausschließlich für **B2C-Angebote** (Verbraucherverträge). Reine B2B-Geschäftsanwendungen sind explizit ausgenommen. Da der Großteil der in v2 genannten Anwendungstyp-Templates (Fachanwendung, HR-Anwendung, Reporting, Dienstreiseanträge, Gehaltsabrechnungen, interne Kommunikationsplattform) interne B2B-Software ist, dürfte die MLBF für diesen Kernbereich von ProcureAccess **nicht zuständig sein**. Relevanz besteht ggf. nur für den Teil des Scopes, der öffentlich zugängliche B2C-Webanwendungen betrifft. Details und Einordnung als Beobachtungspunkt siehe Abschnitt 6.

---

## 6. Weiterhin offene Punkte (aus v2 übernommen bzw. neu entstanden, für Chat 2-Fortsetzung/PRD/Architektur)

- **ISO 9241 / DIN EN 17161-Scope-Frage** (aus v2, Punkt 7): weiterhin unentschieden, ob und wie Teile dieser Normen ins Kriterienset einfließen.
- **Scope geschlossene Software** (aus v2, Punkt 6): weiterhin unentschieden.
- **Rollen-/Rechte-Struktur:** durch den möglichen dritten Beitragenden-Typ (Menschen mit Behinderung) komplexer geworden als in v2 angenommen (dort nur Einkäuferin/Ausschreibungsverantwortliche als Personas). Noch nicht im Detail durchdacht, welche Rechte/Sichtbarkeiten dieser Nutzertyp im Vergleich zu den beiden ursprünglichen Personas hat.
- **Erstbefüllung/Anschubproblem:** Das Workspace-Modell verringert die zentrale Wartungslast, löst aber nicht vollständig das Henne-Ei-Problem der Anfangsphase — wie werden erste Nutzerinnen motiviert, bevor ein Netzwerkeffekt durch geteilte Einträge entsteht? Nicht abschließend beantwortet.
- **Konkrete Ausgestaltung der Marktlagen-Übersicht** (Abschnitt zu Softwaretyp-Übersichten): Idee festgehalten (inkl. Mindestschwelle an Einträgen, bevor eine Aussage angezeigt wird), aber nicht im Detail spezifiziert.
- **Beobachtungsposten — MLBF (Marktüberwachungsstelle der Länder für die Barrierefreiheit von Produkten und Dienstleistungen):** Seit Januar 2026 in aktiver Kontrollphase (siehe Abschnitt 5). Kein aktives Feature für diese Konzeptphase, da (a) ihre Zuständigkeit auf B2C-Angebote beschränkt ist und damit den Großteil der ProcureAccess-Zielgruppen-Software (interne B2B-Fachanwendungen) nicht abdeckt, und (b) noch unklar ist, in welchem Umfang und Format sie Prüfergebnisse überhaupt veröffentlichen wird. Perspektivisch könnten veröffentlichte MLBF-Ergebnisse für den B2C-relevanten Teil des Scopes (z. B. öffentlich zugängliche Webanwendungen) als zusätzliche, besonders glaubwürdige Quelle für Kanal 2 (unabhängige Einträge, siehe 3.1) dienen. Entwicklung beobachten, nicht jetzt schon einbauen.
- **Rechtliche Prüfung ausstehend (aus Team-Feedback gebündelt):** Aus der Team-Rückmeldung ergibt sich übereinstimmend der Wunsch nach einer juristischen Einschätzung zu mehreren, inhaltlich verwandten Punkten: (1) Kanal 2 — rechtliche Tragfähigkeit unabhängiger, ggf. kritischer Nutzer-/Institutionseinträge (bereits in 3.3 vorgemerkt, hier bestätigt); (2) VPAT/ACR-Bindungswirkung, falls referenzierte Kriterien Vertragsbestandteil einer Ausschreibung werden (siehe 2.6); (3) Vollständigkeit/Rechtssicherheit eines späteren Vergabetext-Generators (siehe 2.5); (4) Einordnung des US-amerikanischen BuyAccessible-Wizard-Präzedenzfalls für die eigene Rechtslage. Empfehlung: diese vier Punkte gebündelt in einem Termin mit der bereits im Projekt bekannten juristischen Ansprechperson klären, bevor die betroffenen Features im Detail spezifiziert werden.
- **Trägerschaft/Fortführung nach Projektende:** Aus Business-/Beratungsperspektive wird empfohlen, aktiv nach einem geeigneten Partner für die Fortführung von ProcureAccess nach Ende der Projektförderung zu suchen — mehrere Produktentscheidungen hängen nach dieser Einschätzung von der Klärung dieser Frage ab. Nicht Teil dieser Konzeptphase, aber als strategische Priorität für die Projektleitung vermerkt.

---

## 7. Zentrale Annahmen, die vor/während der Entwicklung geprüft werden sollten

- Nutzerinnen (insb. private Unternehmen) sind bereit, projektbezogene Workspace-Einträge zu veröffentlichen — Motivation vermutlich unterschiedlich stark je nach Nutzertyp (öffentliche Stellen vs. private Unternehmen vs. Einzelpersonen mit Behinderung).
- Prüferinnen/Prüfbüros sind bereit, bereits bezahlte Prüfergebnisse mit Zustimmung ihres Auftraggebers öffentlich sichtbar zu machen, wenn ein Reputations-/Referenzeffekt entsteht.
- Die Priorisierungslogik nach Anwendungstyp (aktuell reines Expertenurteil) hält einer Praxisprüfung stand — bislang nicht getestet.
- Kooperationspartner sind bereit und in der Lage, als vermittelnde Instanz für die Beteiligung von Menschen mit Behinderung zu fungieren.
- Anbieter sind bereit, freiwillig ACRs/VPATs bei ProcureAccess einzureichen, obwohl selbst die GSA (mit deutlich größerem Beschaffungsvolumen und ganz ohne Einreichungspflicht) dieses Anreizproblem für ihre eigene, neu aufgebaute ACR Repository noch nicht als gelöst betrachten kann (siehe Abschnitt 5) — diese Annahme ist für den deutschen Markt eher weniger, nicht mehr, abgesichert als für den US-Kontext, da hier zusätzlich der regulatorische Sog aus Section 508 fehlt.

---

## 8. Validierung durch das Projektteam

Dieses Dokument wurde dem Projektteam vorgestellt (schriftliches Feedback sowie ein dediziertes Feedback-Meeting). **Ergebnis: Die Konzeptrichtung wurde von allen Entscheidungsträgern positiv bewertet und zur konkreten Umsetzung freigegeben.** Ausdrückliche Zustimmung insbesondere zum Workspace-Modell (u. a. mit dem zusätzlichen Argument, dass es keinen "Meldestellen-Charakter" hat) und zum Zwei-Kanal-Prinzip.

Offene Punkte und Bedenken aus dem Feedback sind an den jeweils inhaltlich passenden Stellen in dieses Dokument eingearbeitet (insbesondere 2.1, 2.5, 2.6, 5 sowie die neuen Punkte in Abschnitt 6). Zusammengefasst:

- Die in 2.2–2.4 verworfenen Alternativen wurden in ihrer Ablehnung bestätigt — Grundhaltung des Teams: Nutzerinnen sollen zum eigenständigen Denken/Handeln befähigt werden ("Empowerment"), nicht durch das Tool ersetzt werden.
- Vergabetext-Generator (2.5) und VPAT/ACR (2.6) wurden unterschiedlich bewertet — beide gelten als grundsätzlich attraktiv, aber mit ernstzunehmenden Einwänden (Rechtssicherheit bzw. begrenzte Aussagekraft von Anbieter-Selbstauskünften), die vor einer Detailspezifikation geklärt werden sollten.
- Eine frühere Aussage dieses Dokuments zum Nichtvorhandensein interaktiver Tools im DACH-Raum wurde korrigiert (siehe Abschnitt 5) — der Standardanforderungskatalog des Bundes existiert bereits als Filterwerkzeug.

---

## 9. Nicht Gegenstand dieses Dokuments

Datenmodell, konkrete Feldstrukturen, Seitenaufbau des Workspace, Berechtigungskonzept im Detail, UI/UX-Gestaltung. Diese Punkte gehören in Phase 2 (System Design, Architektur) bzw. in die entsprechenden Design-Skills.
