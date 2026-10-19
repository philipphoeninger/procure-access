# Kriterienkatalog ProcureAccess auf Basis von BIT inklusiv

**Version:** 1.5 · **Stand:** 13. Oktober 2026
**Quellen:** [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/), [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/), [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/) (Übersicht: [bit-inklusiv.de/testverfahren](https://www.bit-inklusiv.de/testverfahren/)), abgerufen am 6. Oktober 2026
**Nutzungsrechte:** Die Texte von BIT inklusiv werden mit Erlaubnis wörtlich übernommen. Die Nutzungsrechte hat Detlef Girke erteilt, der an BIT inklusiv mitgewirkt hat und im Projekt WERTE.IT mitarbeitet.
**Bezug:** prd_procureaccess_werteit_v5.md, ux_copy_procureaccess_v12.md, design_handoff_procureaccess_v1.md (1.12), ARCHITECTURE_v10.md, TESTING_v8.md, pruefung_hauptdokumente_v1.md

---

## 1. Zweck und Aufbau

Dieses Dokument ist die fachliche Grundlage für den Kriterienkatalog von ProcureAccess. Es legt fest:

- welche Filter es gibt und wie sie zusammenhängen (Abschnitt 2 und 3),
- welche Prüfverfahren für welchen Anwendungstyp gelten (Abschnitt 4),
- welche Bewertungsskala verwendet wird (Abschnitt 5),
- wie Kriterien in der Oberfläche dargestellt werden (Abschnitt 6),
- und verweist auf alle Kriterien mit ihren Texten (Teil A und Teil B).

**Umfang je Kriterium („Kern + Beispiele“):** Anwendbarkeit, Anforderung, „Warum wird das geprüft?“, Bewertungsalternativen sowie positive und negative Beispiele, jeweils wörtlich. Testvorgehen, Abgrenzung und Verweise werden nicht kopiert. Stattdessen führt ein direkter Link (Anker) zum Prüfschritt bei BIT inklusiv.

**Ein Kriterium, Texte je Verfahren:** Jede Nummer aus EN 301 549 ist genau ein Kriterium. Software-Test und App-Test verwenden dieselben Nummern, formulieren aber unterschiedlich („die Software“ bzw. „die Mobile App“). Deshalb stehen unter einem Kriterium die Texte je Prüfverfahren. Wortgleiche Felder stehen nur einmal.

**Formatierung:** Die Texte wurden automatisch aus den Webseiten von BIT inklusiv übernommen. Überschriften, Listen und Links sind in Markdown umgewandelt. Inhaltlich ist nichts verändert, auch nicht offensichtliche Tippfehler der Quelle.

Wegen des Umfangs stehen die Kriterien in zwei eigenen Dokumenten:

- **Teil A** (`kriterienkatalog_bit_inklusiv_v1_teil_a.md`): 122 Kriterien aus Software-Test und App-Test (EN 301 549 Kapitel 5, 6, 7, 11 und 12), dazu 4 übergeordnete Prüfschritte ohne eigene Inhalte.
- **Teil B** (`kriterienkatalog_bit_inklusiv_v1_teil_b.md`): 61 Kriterien aus dem PDF-Test (EN 301 549 Kapitel 10 und PDF/UA-1).

---

## 2. Filter in vier Schichten

Die Filter bauen aufeinander auf. Eine Auswahl in einer höheren Schicht kann Werte in tieferen Schichten ausschließen (Abschnitt 3).

| Schicht | Name in der Oberfläche | Name in den Daten | Auswahl |
| --- | --- | --- | --- |
| 1 | Verwendungszweck | Produkt-Typen (FilterTypeId 1) | **Einfachauswahl**, durchsuchbare Auswahlliste |
| 2 | Anwendungstyp | App-Typen (FilterTypeId 2) | Mehrfachauswahl |
| 3 | Funktionsbereiche | Produkt-Teile (FilterTypeId 3) | Mehrfachauswahl, „Basisfunktionalität“ immer ausgewählt |
| 4 | Prüfverfahren (optional) | Test-Typen (FilterTypeId 4) | Mehrfachauswahl |

Nur der Verwendungszweck ist eine Einfachauswahl. Alle anderen Schichten sind Mehrfachauswahl.

### 2.1 Verwendungszweck

Werte:

- HR-Anwendung
- Reporting-Anwendung
- Fortbildungsmanagement
- Dienstreiseanträge
- Gehaltsabrechnungen
- Interne Kommunikations- und Datenaustauschplattform
- **Keiner davon passt.** Es gelten dann die allgemeinen Empfehlungen ohne zweckbezogene Gewichtung.

„Datenbankgestützte Fachanwendung“ (im Repository als Id 16 vorhanden) wird vorerst nicht angeboten (im Datenmodell als inaktiver Verwendungszweck, `UseCaseTemplate.IsActive = false`, ARCHITECTURE_v10 C36).

**Vorschlagsliste für das Projektteam (noch nicht in der Oberfläche):**

- Dokumentenmanagement und E-Akte
- Zeiterfassung und Urlaubsverwaltung
- Bewerbermanagement
- E-Learning und Lernplattform
- Videokonferenz und Zusammenarbeit
- Intranet und Wissensmanagement
- Ticketsystem und Servicedesk
- Terminvergabe und Raumbuchung
- Online-Antrag und Bürgerportal
- Finanzbuchhaltung und Haushalt
- E-Vergabe und Beschaffung
- Projektmanagement
- Office-Anwendungen

### 2.2 Anwendungstyp

Der frühere Begriff „Softwaretyp“ entfällt. Angezeigt werden die Werte ohne Schrägstrich:

| Anzeige | Wert im Repository | Hinweis in der Oberfläche |
| --- | --- | --- |
| Website oder Webanwendung | Website / Webanwendung | – |
| Software (offen) | Software (offen) | Eigene Hilfsmittel wie Screenreader lassen sich verwenden. |
| Software (geschlossen) | Software (geschlossen) | Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals. |
| Mobile Anwendung (offen) | Mobile Anwendung (offen) | Eigene Hilfsmittel wie Screenreader lassen sich verwenden. |
| Mobile Anwendung (geschlossen) | Mobile Anwendung (geschlossen) | Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden. |
| Dokument | Dokument | – |
| Hardware | Hardware | – |
| Vermittlungs- oder Notrufdienst | Vermittlungs-/Notrufdienst | – |

Begriffe nach BIT inklusiv (Software-Test, Einführung, wörtlich):

> **Offene Funktionalität** trifft zu, wenn es sich um Nicht-Web-Software handelt, die eine Benutzerschnittstelle bereitstellt und den Zugriff auf Assistenztechnologien zum Vorlesen des Bildschirms unterstützt.
>
> **Geschlossene Funktionalität** trifft zu, wenn es sich um Nicht-Web-Software handelt, die eine Benutzerschnittstelle bereitstellt, welche für Assistenztechnologien zum Vorlesen des Bildschirms geschlossen ist

Neutrale Formulierungen in der Oberfläche: Die Frage in S10 lautet „Was möchten Sie beschaffen?“. Im Produktverzeichnis heißt es „Produkte“ statt „Software“. Der Projektclaim „Barrierefreie Software beschaffen“ bleibt.

### 2.3 Funktionsbereiche

„Basisfunktionalität“ ist immer ausgewählt und lässt sich nicht abwählen. Die Erklärungen lassen sich über einen Schalter („Erklärungen anzeigen“) ein- und ausblenden. Wortlaut aus dem Standardanforderungskatalog:

| Funktionsbereich | Erklärung |
| --- | --- |
| Basisfunktionalität | Basisfunktionalitäten sind alle Funktionalitäten, die erfahrungsgemäß in allen Anwendungen vorhanden sind. Der Filter „Basisfunktionalität" ist standardmäßig immer ausgewählt. |
| Audio | Audiofunktionalität ist die akustische oder klangliche Wiedergabe von Informationen. |
| Video | Videofunktionalität ist die synchrone Wiedergabe von visuellen (sichtbaren) und optional akustischen (hörbaren) Informationen. |
| Zweiwege-Sprachkommunikation | Zweiwege-Sprachkommunikation ist der Austausch von Sprach- und/oder Videoinformationen in zwei Richtungen (z. B. Telefonie und Videokonferenzsysteme). |
| Echtzeittext (RTT) | Echtzeittext, Englisch "real-time-text" (RTT), bezeichnet die unmittelbare Übertragung von einzelnen schriftlichen Zeichen. Empfänger von Echtzeittext können somit den Schreibprozess direkt mitverfolgen. Echtzeittext findet vor allem in Kommunikations- und Kollaborationslösungen Anwendung, z. B. in Form einer Live-Untertitelung. |
| Biometrie | Biometrie ist die Verwendung von biologischen Merkmalen zur Benutzeridentifikation oder Steuerung, z. B. Fingerabdruck oder Gesichtserkennung. |
| Autorenwerkzeug | Ein Autorenwerkzeug ist jede Möglichkeit, Text in strukturierter Form zu erstellen oder zu bearbeiten, z. B. Textverarbeitungsprogramme, White Boards, aber auch Texteingabefelder mit Strukturierungsmöglichkeiten (z. B. Kommentarfunktion). |

### 2.4 Prüfverfahren

Im Datenmodell heißt die Schicht „Test-Typen“, in der Oberfläche „Prüfverfahren“. In S10 ist sie Schritt 4 und optional. Jedes gewählte Prüfverfahren fügt der Kriterienliste ein Kriterium hinzu: „Geprüft nach {{Prüfverfahren}}“ mit Link zum Verfahren.

**Was dieses Kriterium ist (Entscheidung 12. Oktober 2026):** eine eigene Art von Kriterium ohne Texte von BIT inklusiv.

- Es steht in der Kriterienliste und kann abgewählt werden.
- Empfohlene Priorität: **B – Bewertungskriterium**.
- Es kommt mit in den Export.
- Im Beschaffungsprojekt wird es nicht eingeschätzt: **Ja / Nein / Nicht angegeben** ergibt sich aus den Prüfnachweisen des Produkts (Entscheidung 12. Oktober 2026, ARCHITECTURE_v10 C46). Ein Produkt kann mehrere Prüfnachweise haben, einen je Prüfverfahren.
- Im Abgleich mit Kriterien (K1) zählt es **nicht** mit, weil der Prüfstatus dort schon unter „Auf Barrierefreiheit geprüft“ steht.
- Auch im **Umfang einer Kriterienliste** und im Fortschritt der Einschätzung zählt es nicht mit. Der Umfang nennt es getrennt, zum Beispiel in der Projektübersicht: „46 Kriterien: 12 Mindestanforderungen (A), 28 Bewertungskriterien (B), 6 Hinweise – dazu 2 × „Geprüft nach …“ (zählen im Abgleich nicht mit)“. Der Fortschritt („36 von 46 Kriterien eingeschätzt“) zählt nur Anforderungskriterien.

| Prüfverfahren | Link | Passt zu Anwendungstyp |
| --- | --- | --- |
| BIT inklusiv Software-Test | [softwaretest.bit-inklusiv.de](https://softwaretest.bit-inklusiv.de/) | Software (offen/geschlossen); vorerst auch Website oder Webanwendung |
| BIT inklusiv App-Test | [apptest.bit-inklusiv.de](https://apptest.bit-inklusiv.de/) | Mobile Anwendung (offen/geschlossen) |
| BIT inklusiv PDF-Test | [pdftest.bit-inklusiv.de](https://pdftest.bit-inklusiv.de/) | Dokument |
| BIK BITV-Test (Web) | [bitvtest.de – BITV 2.0 Web](https://bitvtest.de/pruefverfahren/bitv-20-web) | Website oder Webanwendung |
| BIK BITV-Test (App) | [bitvtest.de – BITV 2.0 App](https://bitvtest.de/pruefverfahren/bitv-20-app) | Mobile Anwendung (offen/geschlossen) |

**Art der Prüfung** (S34a Prüfnachweis, S55 Schritt 2 Prüfergebnis, Filter in S40, Anzeige in K2 und K4): dieselbe Liste, ergänzt um

- **Andere Prüfung** mit Freitextfeld „Name des Prüfverfahrens“
- **Eigene Prüfung ohne festes Verfahren**

---

## 3. Ausschlussregeln

### 3.1 Regeln

| Auswahl (Schicht) | schließt aus |
| --- | --- |
| HR-Anwendung (1) | Anwendungstyp: Dokument, Hardware, Vermittlungs- oder Notrufdienst · Prüfverfahren: BIT inklusiv PDF-Test |
| Website oder Webanwendung (2) | Prüfverfahren: BIT inklusiv App-Test, BIT inklusiv PDF-Test, BIK BITV-Test (App) |

Die Regeln wirken über alle Schichten: Eine Auswahl schließt Werte in jeder tieferen Schicht aus, nicht nur bei den Prüfverfahren. Im Datenmodell ist jede Regel ein Eintrag in `CriteriaFilterExclusion` (CriteriaFilterId → ExclusionId).

**Entschieden (6. Oktober 2026):** HR-Anwendung schließt Mobile Anwendung **nicht** aus. Die beiden Einträge in `SampleData.cs` (`CriteriaFilterExclusion` Id 2 und 3: HR-Anwendung → Mobile Anwendung offen/geschlossen) werden entfernt.

### 3.2 Verhalten in der Oberfläche

- **Ausgeschlossene Werte bleiben sichtbar.** Sie sind ausgegraut, nicht auswählbar (`aria-disabled="true"`, weiterhin fokussierbar) und nennen den Grund, zum Beispiel „Passt nicht zum Verwendungszweck HR-Anwendung“ oder „Passt nicht zum Anwendungstyp Website oder Webanwendung“.
- **Änderung in einer höheren Schicht:** Alle Auswahlen in den tieferen Schichten werden zurückgesetzt (Basisfunktionalität bleibt ausgewählt). Ein Hinweis erscheint, mit einem eigenen Text je geänderter Schicht (UX-Copy `criteria.select.reset.text.*`), zum Beispiel: „Sie haben den Verwendungszweck geändert. Ihre Auswahl bei Anwendungstyp, Funktionsbereichen und Prüfverfahren wurde deshalb zurückgesetzt. Bitte wählen Sie erneut aus.“ Der Hinweis wird per Live-Region angesagt.
- Ist ein Wert durch mehrere Auswahlen ausgeschlossen, nennt der Grund alle Auslöser.

---

## 4. Welche Texte gelten für welchen Anwendungstyp?

| Anwendungstyp | Texte aus | Bemerkung |
| --- | --- | --- |
| Website oder Webanwendung | BIT inklusiv Software-Test | Übergangslösung. Kriterien aus dem BIK BITV-Test (Web) können später ergänzt werden. |
| Software (offen), Software (geschlossen) | BIT inklusiv Software-Test | Der Software-Test unterscheidet offen und geschlossen nicht in eigenen Prüfschritten, sondern innerhalb der Beispiele. |
| Mobile Anwendung (offen), Mobile Anwendung (geschlossen) | BIT inklusiv App-Test | wie oben |
| Dokument | BIT inklusiv PDF-Test | gilt für PDF-Dokumente |
| Hardware | – | Kein BIT-inklusiv-Verfahren. Offen (Abschnitt 8). |
| Vermittlungs- oder Notrufdienst | – | Kein BIT-inklusiv-Verfahren. Offen (Abschnitt 8). |

Sind mehrere Anwendungstypen gewählt, erscheint jedes Kriterium einmal. In der Detailansicht stehen die Texte aller passenden Verfahren.

---

## 5. Bewertungsskala

ProcureAccess übernimmt die sechs Stufen von BIT inklusiv. Sie ersetzen die bisherigen Einschätzungswerte von K6 (Unterstützt, Teilweise unterstützt, Nicht unterstützt, Nicht zutreffend). Tabelle aus der Einführung des Software-Tests (wörtlich):

| Bewertung | Synonym | Konformität |
| --- | --- | --- |
| Erfüllt | Erfüllt | Konform |
| Leichte Einschränkung | Eher erfüllt | Konform |
| Einschränkung | Teilweise erfüllt | Nicht konform |
| Barriere | Eher nicht erfüllt | Nicht konform |
| Blockade | Nicht erfüllt | Nicht konform |
| Nicht anwendbar | Nicht anwendbar | Nicht anwendbar |

Ergänzend für ProcureAccess:

- **„Nicht eingeschätzt“** bleibt die Vorauswahl in K6 (noch keine Bewertung). Das ist keine Stufe von BIT inklusiv.
- **Erlaubte Stufen je Kriterium:** Nicht jedes Kriterium erlaubt alle sechs Stufen. Die erlaubten Stufen stehen bei jedem Kriterium unter „Bewertungsalternativen“. K6 bietet nur diese an.
- **Mehrere Anwendungstypen (Entscheidung 12. Oktober 2026):** Gelten für ein Kriterium mehrere Prüfverfahren mit unterschiedlichen erlaubten Stufen (zum Beispiel Website und Mobile Anwendung), bietet K6 die **Vereinigung** an: jede Stufe, die in mindestens einem der Verfahren erlaubt ist. So fehlt nie eine zutreffende Stufe. Gilt kein Verfahren (Hardware, Vermittlungs- oder Notrufdienst), stehen alle sechs Stufen zur Auswahl.
- **Sonderfälle:** 11.4.1.2 und 11.8.1 nennen zusätzlich „Nicht getestet“, weil sie über andere Prüfschritte abgedeckt sind. ProcureAccess bildet das als „Nicht eingeschätzt“ ab.
- **Schreibweise:** Der PDF-Test schreibt die Stufen klein („erfüllt“, „leichte Einschränkung“). In der Oberfläche gilt die Schreibweise aus der Tabelle oben.
- **Auswirkung auf K1 (Abgleich mit Kriterien):** Aus sechs Zählwerten werden acht: die sechs Stufen, „Nicht eingeschätzt“ und „Damals nicht im Katalog“. Im Beschaffungsprojekt sind es sieben (ohne „Damals nicht im Katalog“). Erfahrungsberichte zeigen keine Zählwerte. Einträge der Anbieterseite ohne Einschätzungen einzelner Kriterien zeigen keinen Abgleich, sondern einen Hinweis (Entscheidung D2, 13. Oktober 2026). Die Regeln „immer alle Kategorien, keine Summe, kein Prozent“ bleiben (ARCHITECTURE_v10, TESTING_v8 SIG-07, SIG-08, SIG-13, SIG-14, PUB-14).

### 5.1 Erklärung der Stufen in der Oberfläche

BIT inklusiv erklärt die Stufen nur über die Synonyme in der Tabelle oben. Damit Nutzende den Unterschied zwischen Einschränkung, Barriere, Blockade und Nicht anwendbar verstehen, zeigt ProcureAccess eine aufklappbare Erklärung (Komponente `pa-rating-legend`). Sie steht einmal über der Liste in S34b, über der Tabelle in S42, im Abschnitt „Wie wird bewertet?“ der Kriteriendetails (S11) und im Überblick eines Produkts im Projekt.

**Vorschlag, bitte fachlich prüfen** (Projektteam, ggf. mit Detlef Girke, damit die Texte zur Prüfpraxis von BIT inklusiv passen):

| Stufe | Erklärung |
| --- | --- |
| Erfüllt | Die Anforderung ist vollständig umgesetzt. Niemand wird bei dieser Funktion behindert. |
| Leichte Einschränkung | Kleine Mängel. Die Nutzung ist kaum erschwert. |
| Einschränkung | Spürbare Mängel. Betroffene Menschen können die Funktion nutzen, aber nur mit Mehraufwand oder über Umwege. |
| Barriere | Erhebliche Mängel. Betroffene Menschen können die Funktion nur mit großer Mühe oder mit fremder Hilfe nutzen. |
| Blockade | Betroffene Menschen können die Funktion gar nicht nutzen. |
| Nicht anwendbar | Das Produkt hat die Funktion nicht, um die es im Kriterium geht, zum Beispiel keine Videos. Das ist weder ein Mangel noch ein Pluspunkt. |
| Nicht eingeschätzt | Zu diesem Kriterium gibt es noch keine Bewertung. Das heißt nicht, dass es nicht erfüllt ist. |

Fußzeile der Erklärung: „Welche Stufen bei einem Kriterium möglich sind, steht in seinen Details. Die Stufen stammen aus den Prüfverfahren von BIT inklusiv. ProcureAccess bildet daraus kein Gesamturteil.“

Konkrete Beispiele je Stufe liefern die Kriterien selbst: Der PDF-Test nennt bei vielen Prüfschritten Beispiele pro Stufe („Barriere: …“, „Blockade: …“), Software- und App-Test nennen positive und negative Beispiele. Diese stehen in der Detailansicht des Kriteriums.

---

## 6. Darstellung der Kriterien (Vorschlag)

### 6.1 Kompakt in der Liste (S11)

Reihenfolge innerhalb einer Kriterienkarte:

1. Checkbox und **Titel in verständlicher Sprache** (z. B. „Bilder haben eine Textbeschreibung“). Darunter klein der Originaltitel mit Nummer: „11.1.1.1a Alternativtexte für informative Grafiken“.
2. **Kurzbeschreibung**, ein bis zwei Sätze, abgeleitet aus „Anforderung“.
3. Zeile mit Metadaten: Funktionsbereich · EN-Nummer.
4. **K5 Priorität** mit „Empfehlung des Projektteams“.
5. Text-Button **„Details anzeigen“** (`aria-expanded`, `aria-controls`).

Der verständliche Titel und die Kurzbeschreibung sind eigene Texte des Projektteams. Sie stehen noch nicht in diesem Dokument und werden je Kriterium ergänzt (Abschnitt 8).

### 6.2 Ausführlich (klappt in der Karte auf)

| Bereich | Inhalt | Quelle |
| --- | --- | --- |
| Was wird verlangt? | Anforderung | BIT inklusiv, je Verfahren |
| Wann gilt das? | Anwendbarkeit | BIT inklusiv, je Verfahren |
| Wen betrifft es? | „Warum wird das geprüft?“ | BIT inklusiv, je Verfahren |
| Warum diese Priorität? | Begründung der Priorität | Projektteam (K5) |
| Woran erkennt man es? | Beispiele für positive und negative Bewertung | BIT inklusiv, je Verfahren |
| Wie wird bewertet? | Erlaubte Bewertungsstufen | BIT inklusiv |
| So wird es geprüft | Link zum Prüfschritt (öffnet bit-inklusiv.de) | BIT inklusiv |
| Gilt für | Anwendungstypen, Prüfverfahren | ProcureAccess |
| Notiz | eigenes Notizfeld | Nutzende |

- Bei mehreren Verfahren zeigen Tabs („Software-Test“, „App-Test“, „PDF-Test“) die jeweiligen Texte. Wortgleiche Abschnitte stehen über den Tabs.
- Jedes Kriterium hat einen eigenen Anker (`#k-11-1-1-1a`), damit Links direkt darauf zeigen können.
- Die Quelle „BIT inklusiv“ steht sichtbar unter den übernommenen Texten.

---

## 7. Abweichungen zum Repository

| Thema | Repository (`SampleData.cs`) | Dieses Dokument |
| --- | --- | --- |
| Beschreibung der Kriterien | Nur „Anforderung“, teils ohne Unterüberschriften und Listen | Kern + Beispiele vollständig, je Verfahren |
| PDF-Kriterien | fehlen | Teil B (61 Prüfschritte) |
| App-Test-Texte | fehlen (nur Software-Texte) | in Teil A je Kriterium |
| Funktionsbereich Audio | keinem Kriterium zugeordnet | 11.1.4.2 Audio-Steuerelement → Audio (Vorschlag) |
| 11.8.1–11.8.5 (Autorenwerkzeuge) | Basisfunktionalität | Autorenwerkzeug (Vorschlag) |
| 12.1.1–12.2.4 (Dokumentation und Support) | Autorenwerkzeug | Basisfunktionalität (Vorschlag) |
| 11.7 Individuelle Benutzerpräferenzen | Name „11.7. …“ mit Punkt | Basisfunktionalität |
| Ausschluss HR → Mobile Anwendung | vorhanden | entfällt (entschieden 6. Oktober 2026, Abschnitt 3.1) |
| Bewertungsskala | nicht im Datenmodell | sechs Stufen (Abschnitt 5) |

Vorschläge sind in den Kriterien als „(Vorschlag, abweichend vom Repository)“ markiert.

---

## 8. Offene Punkte für das Projektteam

| Nr. | Punkt | Vorschlag |
| --- | --- | --- |
| K-1 | Verständliche Titel und Kurzbeschreibungen je Kriterium fehlen (Abschnitt 6.1). | Projektteam formuliert sie, beginnend mit den Kriterien mit Priorität A. |
| K-2 | **Offenes Todo:** Hardware sowie Vermittlungs- oder Notrufdienst haben kein BIT-inklusiv-Verfahren. Vorerst akzeptiert (6. Oktober 2026). | Später: Software-Test-Kriterien der Kapitel 5 und 6 zuordnen oder die Typen als „noch ohne Kriterien“ kennzeichnen. |
| ~~K-3~~ | ~~Ausschluss HR-Anwendung → Mobile Anwendung~~ **Erledigt:** kein Ausschluss; `SampleData.cs` anpassen | – |
| K-4 | Ein Kriterium kann im Datenmodell nur einem Funktionsbereich angehören. 11.1.2.1 betrifft Audio und Video. | Später Mehrfachzuordnung erlauben; bis dahin Video. |
| K-5 | Kriterien aus BIK BITV-Test (Web) für Websites. | Spätere Version dieses Dokuments. |
| K-6 | **Vorrangig:** Prioritäten (K5) und Begründungen je Kriterium, Verwendungszweck und Anwendungstyp. Ohne sie gibt es keine empfohlenen Gewichtungen, die die PRD als P0 verlangt. | Eigenes Dokument des Projektteams, **vor dem Katalog-Migrationsschritt** (ARCHITECTURE_v10 §16.5 O-1). |
| K-8 | Erklärungstexte der Bewertungsstufen (Abschnitt 5.1) fachlich prüfen (= ARCHITECTURE_v10 §16.5 O-16, UX-Copy UX-O19) | Projektteam, ggf. Detlef Girke |
| K-7 | Aktualität: BIT inklusiv entwickelt die Verfahren weiter. | Bei jeder Katalogversion neu abgleichen; Abrufdatum oben anpassen. |

## 9. Änderungsverlauf

| Version | Datum | Änderung |
| --- | --- | --- |
| 1.5 | 13. Oktober 2026 | Prüfung der Hauptdokumente (`pruefung_hauptdokumente_v1.md`): Text zum Zurücksetzen wie in der UX-Copy (ein Text je Schicht), Abschnitt 6.1 nur S11, Zählregel für den Umfang einer Liste (Kriterien „Geprüft nach …“ zählen nicht mit), Hinweis zu Einträgen ohne Einschätzungen (D2); Verweise auf PRD v5, ARCHITECTURE_v10, TESTING_v8, UX-Copy v12, Handoff 1.12 |
| 1.4 | 13. Oktober 2026 | Verweise auf ARCHITECTURE_v9, TESTING_v7, UX-Copy v11 |
| 1.3 | 13. Oktober 2026 | Verweise auf die neuesten Dokumente aktualisiert |
| 1.2 | 12. Oktober 2026 | Konsistenzprüfung vor SPEC_v1: „Geprüft nach …“ als eigene Kriterienart (Abschnitt 2.4), Vereinigung der erlaubten Stufen (Abschnitt 5), K-6 vorrangig, Verweise aktualisiert |
| 1.1 | 6. Oktober 2026 | Ausschluss HR → Mobile Anwendung entfällt; Hardware/Vermittlungsdienst als offenes Todo; Erklärung der Bewertungsstufen (Abschnitt 5.1) |
| 1.0 | 6. Oktober 2026 | Erste Fassung: Filterschichten, Ausschlussregeln, Bewertungsskala, Darstellungsvorschlag, Kriterien aus Software-, App- und PDF-Test (Kern + Beispiele) |
