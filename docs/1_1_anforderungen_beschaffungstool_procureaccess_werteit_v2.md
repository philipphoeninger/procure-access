# Anforderungen Beschaffungstool „ProcureAccess" (Projekt WERTE.IT)

> **Status:** Anforderungsdokument v2 — bereinigt & strukturiert für Übergabe an Chat 2 (Brainstorm & Validate)
> **Offene Punkte** sind explizit markiert (→ 🔲) und für den Brainstorming-Chat vorgesehen.

---

## 1. Projektkontext

**WERTE.IT** ist ein Forschungsprojekt, das gemeinsam mit Kooperationspartnern Wege erforscht, über einen unternehmensweit ausgerollten inklusiven Ansatz die Voraussetzungen für die Beschäftigung von Menschen mit Behinderungen zu schaffen und zu verbessern. Im Rahmen des Projekts werden verschiedene Module, Handreichungen, Weiterbildungsangebote, Podcasts und Beratungsangebote entwickelt.

**ProcureAccess** ist eines dieser Module: ein Software-Tool, das beschaffende Stellen bei der Beschaffung barrierefreier Software unterstützt.

Das Tool wird als **öffentlich gefördertes Projekt** entwickelt und **Open Source** veröffentlicht (GitHub). Dies ist eine bewusste strategische Entscheidung: Transparenz, Nachnutzbarkeit und Zugänglichkeit für andere Stellen sollen aktiv gefördert werden.

---

## 2. Problem

In Unternehmen und öffentlichen Stellen ist noch zu viel Software im Einsatz, die nicht barrierefrei ist. Das führt zu einer konkreten Situation: Auch wenn der Wille vorhanden ist, Menschen mit Behinderung einzustellen, scheitert die Eingliederung häufig daran, dass die eingesetzte Software von diesen Personen nicht bedient werden kann — und sie das Unternehmen nach kurzer Zeit wieder verlassen.

Das Problem liegt nicht immer am fehlenden Willen der beschaffenden Stellen. Es fehlt an:

- **Orientierung:** Welche Barrierefreiheitskriterien gelten für welche Art von Software?
- **Verständlichkeit:** Bestehende Normen (EN 301 549, BITV) sind für Nicht-Fachleute schwer zugänglich.
- **Produktwissen:** Welche am Markt verfügbaren Produkte erfüllen diese Kriterien überhaupt?
- **Prozessintegration:** Wie fließen Barrierefreiheitsanforderungen strukturiert in Beschaffungsverfahren ein?

**ProcureAccess** setzt genau hier an: Es hilft beschaffenden Stellen, die für ihre Situation relevanten Barrierefreiheitskriterien zu finden — in verständlicher Sprache — und zeigt passende Produkte, die diese Kriterien erfüllen.

---

## 3. Zielgruppe

Das Tool richtet sich an **öffentliche Stellen und private Unternehmen** gleichermaßen — beide sind verpflichtet bzw. angehalten, barrierefreie Software einzusetzen.

### Primäre Nutzer-Personas

**Einkäuferin / Beschaffungsverantwortliche**
Recherchiert Produkte und holt Angebote ein. Hat i.d.R. kein oder wenig Vorwissen zu Barrierefreiheitsnormen. Braucht schnelle, verständliche Orientierung: „Welche Kriterien muss ich fordern?" und „Welche Produkte kommen infrage?"

**Ausschreibungsverantwortliche Person**
Erstellt Ausschreibungen und muss Barrierefreiheitskriterien als Anforderungen formulieren. Benötigt strukturierte, exportierbare Kriterienlisten, die direkt in Vergabeunterlagen übernommen werden können.

### Kein primärer Nutzer

Entwicklerinnen und Entwickler, die technische Implementierungsdetails oder Lösungsvorschläge für konkrete Programmierfragen suchen. Das Tool ist kein technisches Referenzwerk.

### Vorwissen

Die primären Nutzer haben **kein oder selten Vorwissen** zu Normen wie EN 301 549 oder BITV. Sprache und UX des Tools müssen niedrigschwellig und praxisnah gestaltet sein.

---

## 4. Lösungsidee & Kernkonzept

ProcureAccess ist ein webbasiertes Filterwerkzeug mit zwei zentralen Objekten:

**Kriterium**
Ein Barrierefreiheitskriterium aus bestehenden Standards (primär EN 301 549 / BITV-Softwaretest), aufbereitet in verständlicher, nicht zu technischer Sprache. Kriterien sind bewusst nicht zu streng formuliert, damit sie in der Praxis anwendbar bleiben und Produkte gefunden werden können, die sie erfüllen.

**Produkt**
Ein am Markt verfügbares Software-Produkt mit Metadaten zur Barrierefreiheit (getestet? von wem? welche Art der Prüfung?). Produkte sind den relevanten Kriterien zugeordnet.

Der Kernablauf:
1. Nutzer wählt den Typ der zu beschaffenden Software (z.B. Webanwendung, Software offen) und deren Verwendungszweck (z.B. HR-Anwendung, Fachanwendung).
2. Das Tool schlägt die relevanten Barrierefreiheitskriterien vor — priorisiert nach Produkttyp und Verwendungszweck.
3. Nutzer kann Kriterien anpassen, merken und exportieren.
4. Das Tool zeigt passende Produkte aus der Datenbank, die diese Kriterien erfüllen.

---

## 5. Features & Scope

### 5.1 Im Scope (geplante Features)

**Filterung nach Software-Typ**
Auswahl des Produkttyps (Basis-Scope: Website/Webanwendung, Software offen) und der enthaltenen Komponenten (Basisfunktionalität, Audio, Video, Zweiwege-Sprachkommunikation, Echtzeittext/RTT, Biometrie, Autorenwerkzeug).

> Der vollständige Standardanforderungskatalog des Bundes (inkl. geschlossener Software, Hardware, Mobile etc.) dient als Referenz: [Standardanforderungskatalog](https://www.barrierefreiheit-dienstekonsolidierung.bund.de/Webs/PB/DE/standardanforderungskatalog/standardanforderungskatalog-node.html)

**Filterung nach Verwendungszweck (Anwendungstyp-Templates)**
Vordefinierte Templates für häufige Anwendungstypen, die eine automatische Priorisierung von Kriterien ermöglichen. Starter-Templates (erweiterbar):
- Datenbankgestützte Fachanwendung
- HR-Anwendung (Stellenausschreibungen, Bewerbermanagement, Assessments)
- Reporting-Anwendungen für das Management
- Fortbildungsmanagement (Seminar-Suche, Seminar-Anmeldung)
- Dienstreiseanträge
- Gehaltsabrechnungen
- Interne Kommunikations- und Datenaustauschplattform

**Kriterien in verständlicher Sprache**
Aufbereitung auf Grundlage des BITV-Softwaretests von BIT inklusiv ([bit-inklusiv.de](https://www.bit-inklusiv.de/bitv-softwaretest/)). Formulierungen sind praxisnah, nicht übermäßig technisch.

**Empfohlene Gewichtungen & Priorisierungen**
Abhängig von Produkttyp und Verwendungszweck werden Kriterien mit Empfehlungen zur Priorität versehen (z.B. A-Kriterium / B-Kriterium).

**Produkt-Datenbank (Marktrecherche)**
Datenbank bekannter Softwareprodukte mit folgenden Metadaten:
- Link zur Website des Herstellers
- Kontaktpersonen
- Funktionsumfang (Stichworte)
- Bereits auf Barrierefreiheit getestet? (Ja/Nein)
- Von wem getestet?
- Art der Prüfung (VPAT, BITV-Test, Kurz-Check, …)
- Interne Notiz zur Qualifikation der Prüferin / des Prüfers

**Merken, Teilen & Vorschlagen**
Nutzer können Kriterien, Produkte und Filtereinstellungen speichern und teilen. Außerdem können neue Produkte und Kriterien zur Aufnahme ins System vorgeschlagen werden.

**Export**
Gefilterte Kriterien inkl. empfohlener Gewichtungen als Excel-Datei exportierbar. Perspektivisch: Import in Ausschreibungssoftware der beschaffenden Stelle (abhängig von offengelegten Schnittstellen).

**Barrierefreie Gestaltung des Tools selbst**
Das Tool muss selbst die Anforderungen erfüllen, die es propagiert.

**Persönliche Einstellungen**
Nutzer können individuelle Präferenzen speichern und die Nutzung an ihre Bedürfnisse anpassen.

### 5.2 Erweiterung für spätere Entwicklungsphasen

**Integration von Bewertungsmodellen**
Nutzer können ihr organisationsinternes Beschaffungs-Bewertungsmodell hinterlegen (z.B. gewichtete Nutzwertanalyse, Scoring-Modell). Die Barrierefreiheitskriterien werden dann mit der entsprechenden Gewichtung exportiert, sodass sie direkt in die bestehende Bewertungslogik der beschaffenden Stelle integriert werden können.

### 5.3 Nicht im Scope (Abgrenzung)

ProcureAccess ist ausdrücklich **kein**:
- Audit-Tool
- Test-Tool für Barrierefreiheit
- Zertifizierungssystem
- Ersatz für professionelle Barrierefreiheitsprüfungen

---

## 6. Scope-Entscheidungen & Begründungen

**Fokus auf Website/Webanwendung & Software (offen)**
In Unternehmen und öffentlichen Stellen fällt der Großteil der eingesetzten Software in diese Kategorien. Geschlossene Software lässt sich schlechter auf Barrierefreiheit prüfen, da keine externe Inspektion möglich ist.

> 🔲 **Diskussionspunkt für Brainstorming:** Sollte auch geschlossene Software in den Scope aufgenommen werden — z.B. um Herstellern einen Anreiz zu bieten, ihr Produkt in die Datenbank eintragen zu lassen?

---

## 7. Offene Punkte für Chat 2 (Brainstorm & Validate)

Die folgenden Punkte sind bewusst offen gehalten und sollen im nächsten Schritt gemeinsam diskutiert werden:

🔲 **Normenerweiterung ISO 9241 / DIN EN 17161**
Inwiefern können Teile der Normenreihe ISO 9241 (z.B. 9241-110, 9241-112 — Dialoggestaltung, Usability) in das Kriterienset aufgenommen werden, um sowohl Barrierefreiheit (EN 301 549) als auch Gebrauchstauglichkeit (9241) abzubilden? Soll zusätzlich die Prozessnorm DIN EN 17161:2019-11 (Design für Alle) berücksichtigt werden?
→ Ziel: eine Matrix mit priorisierbaren Kriterien, die mehrere Normen integriert.

🔲 **Scope geschlossene Software**
Wie unter Punkt 6 beschrieben — Aufnahme in den Scope als Anreizmechanismus für Hersteller?

🔲 **Benutzermodell & Rollen**
Wer darf Produkte und Kriterien vorschlagen? Gibt es moderierte vs. offene Einträge? Wie verhält sich das mit dem Open-Source-Ansatz?

🔲 **Erstbefüllung der Produktdatenbank**
Wie wird die Datenbank initial mit Produkten befüllt? Durch das Projektteam, Kooperationspartner, Community?

---

## 8. Strategische Rahmenbedingungen

- **Open Source:** Der Quellcode wird auf GitHub öffentlich zugänglich gemacht. Transparenz und Nachnutzbarkeit sind explizite Projektziele.
- **Webanwendung:** Niedrigschwellige, plattformunabhängige Nutzung ohne Installation.
- **Forschungskontext:** Das Tool entsteht im Rahmen von WERTE.IT mit Kooperationspartnern. Praxistests und Pilotierungen mit Partnern sind geplant.
- **Sprache:** Deutsch als primäre Sprache des Tools (Zielgruppe im deutschsprachigen Raum).
