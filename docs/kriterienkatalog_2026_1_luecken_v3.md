# Release 2026.1 – fehlende Inhalte je Kriterium

> **Version:** v3 (12. November 2026) – v3 nennt das neue Feld für die Kurzbeschreibung (Aufgabe S4-24, Owner-Entscheidungen O-S4-6 (a) und O-S4-80 (a)). v2 (11. November 2026) – v2 ergänzt Z-7, eine Arbeitsmarkierung im Text von 11.1.3.2 (Aufgabe S4-15b, Owner-Entscheidung O-S4-39 (a)). v1 (9. November 2026) – erstellt mit Aufgabe S3-27 (Sprint 03) aus dem Ordner `catalog/releases/2026.1/`. Für das Projektteam (SPEC_v2 §13 O-1, Kriterienkatalog K-1 und K-6). Quellen: `kriterienkatalog_bit_inklusiv_v1_teil_a.md`, `kriterienkatalog_bit_inklusiv_v1_teil_b.md`.

## 1. Worum es geht

Der Ordner `catalog/releases/2026.1/` enthält alle Kriterien aus Teil A und Teil B mit den Texten von BIT inklusiv, die Ausschlussregeln (Kriterienkatalog §3.1), die Zuordnung der Prüfverfahren zu den Anwendungstypen (§4) und je Prüfverfahren ein Kriterium „Geprüft nach …“ (§2.4). Das Release lässt sich importieren, aber noch nicht aktivieren: Dafür braucht jedes Kriterium eine empfohlene Priorität mit Begründung. Bis dahin sieht niemand diese Kriterien in ProcureAccess.

Es fehlt bei **jedem Kriterium**:

- **Verständlicher Titel** (K-1, Feld `plainLanguageDe`). Bis dahin gilt der Titel von BIT inklusiv.
- **Empfohlene Priorität** (K-6, O-1): eine Stufe für alle Anwendungstypen und Verwendungszwecke (A, B oder Info) mit Begründung, Feld `priorities`. Abweichungen für einzelne Anwendungstypen oder Verwendungszwecke sind möglich, jeweils mit eigener Begründung und optional mit Gewichtung.
- **Kurzbeschreibung** (K-1, Kriterienkatalog §6.1, Feld `shortDescriptionDe`): ein bis zwei Sätze, abgeleitet aus „Anforderung“, als reiner Text ohne Formatierung. Sie steht in der Spalte „Kurzbeschreibung“ der exportierten Liste; bis sie eingetragen ist, bleibt die Spalte leer.
- Optional: eine **Erläuterung des Projektteams** zum Kriterium (Feld `rationaleDe`).

Bei den Kriterien „Geprüft nach …“ ist die Stufe **B – Bewertungskriterium** schon entschieden (Kriterienkatalog §2.4, 12. Oktober 2026); es fehlt nur die Begründung.

Die Spalte **Zu bestätigen** nennt, was der Ordner aus einem Vorschlag des Kriterienkatalogs übernommen hat. Die leeren Spalten sind zum Ausfüllen gedacht; eingetragen wird am Ende im Ordner, als Pull Request (SPEC R-ADM-01).

## 2. Was für alle Kriterien gilt und bestätigt werden sollte

| Nr. | Punkt | Umsetzung im Ordner |
| --- | --- | --- |
| Z-1 | Funktionsbereiche, die der Kriterienkatalog als Vorschlag markiert (Teil A §7: 11.1.4.2, 11.7, 11.8.1–11.8.5, 12.x; Teil B: alle PDF-Prüfschritte) | wie vorgeschlagen übernommen (Entscheidung S3-27, 1) |
| Z-2 | 11.1.2.1 betrifft Audio und Video (K-4) | nur Video, wie im Kriterienkatalog bis zur Entscheidung |
| Z-3 | Der PDF-Test nennt Beispiele je Bewertungsstufe statt positiver und negativer Beispiele | „erfüllt: …“ als positives Beispiel, alle anderen Stufen als negative Beispiele, mit ihrer Stufe am Anfang; Anmerkungen zur Bewertung im Anschluss an die negativen Beispiele (Entscheidung S3-27, PDF-Beispiele) |
| Z-4 | 10.2.5.1, 10.2.5.4 und 10.4.1.3 sind im PDF-Test „nicht anwendbar“ und nennen keine Bewertungsstufen | als Kriterium für Dokumente enthalten, einzige Stufe „Nicht anwendbar“ (Entscheidung S3-27, Nicht-anwendbare Prüfschritte). Brauchen sie überhaupt eine Priorität, oder sollen sie im nächsten Stand fehlen? |
| Z-5 | Hardware sowie Vermittlungs- oder Notrufdienst haben keine Kriterien (K-2) | unverändert: keine Kriterien für diese beiden Anwendungstypen |
| Z-6 | Die Reihenfolge folgt den Dokumenten: Teil A, dann Teil B, dann „Geprüft nach …“ | Feld `sortOrder` in Zehnerschritten |
| Z-7 | 11.1.3.2, Software-Test, „Anforderung“: der dritte Absatz beginnt mit der Markierung „#todo#“ („#todo#Bei einem Seitenaufruf soll bei der Tastaturnavigation …“); der App-Test hat denselben Absatz ohne sie | unverändert übernommen; die Kriterienliste zeigt den Text wörtlich, also mit „#todo#“ (Entscheidung S4-15b, O-S4-39). Bitte klären, ob der Absatz so gilt; die Korrektur kommt mit dem nächsten Release, weil ein gespeichertes Release sich nicht ändert |

## 3. Kriterien

### 3.1 Teil A – Software-Test und App-Test

#### Allgemeine Anforderungen

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 5.2 | Aktivierung von Barrierefreiheitsfunktionen | Basisfunktionalität |  |  |  |  |
| 5.3 | Biometrie | Biometrie |  |  |  |  |
| 5.4 | Erhaltung von Barrierefreiheitsinformationen bei Konvertierung | Basisfunktionalität |  |  |  |  |
| 5.5.1 | Möglichkeiten der Bedienung | Basisfunktionalität |  |  |  |  |
| 5.5.2 | Unterscheidbarkeit der bedienbaren Elemente | Basisfunktionalität |  |  |  |  |
| 5.6.1 | Taktiler oder auditiver Status | Basisfunktionalität |  |  |  |  |
| 5.6.2 | Visueller Status | Basisfunktionalität |  |  |  |  |
| 5.7 | Tastenwiederholung | Basisfunktionalität |  |  |  |  |
| 5.8 | Annahme eines zweifachen Tastenanschlags | Basisfunktionalität |  |  |  |  |
| 5.9 | Gleichzeitige Benutzerhandlungen | Basisfunktionalität |  |  |  |  |

#### Anforderungen an Technologien mit Zwei-Wege-Sprachkommunikation

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 6.1 | Audio-Bandbreite für Sprache | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.2.1.1 | RTT-Kommunikation | Echtzeittext (RTT) |  |  |  |  |
| 6.2.1.2 | Gleichzeitige Verwendung von Sprache und Text | Echtzeittext (RTT) |  |  |  |  |
| 6.2.2.1 | Visuell unterscheidbare Darstellung | Echtzeittext (RTT) |  |  |  |  |
| 6.2.2.2 | Programmatisch bestimmbare Sende- und Empfangsrichtung | Echtzeittext (RTT) |  |  |  |  |
| 6.2.2.3 | Sprecheridentifizierung | Echtzeittext (RTT) |  |  |  |  |
| 6.2.2.4 | Anzeige von Audioaktivität in Echtzeit | Echtzeittext (RTT) |  |  |  |  |
| 6.2.3 | Interoperabilität | Echtzeittext (RTT) |  |  |  |  |
| 6.2.4 | Reaktionsgeschwindigkeit von RTT | Echtzeittext (RTT) |  |  |  |  |
| 6.3 | Anrufer-Identifizierung | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.4 | Alternativen zu sprachbasierten Diensten | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.5.2 | Auflösung bei Videotelefonie | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.5.3 | Bildwiederholfrequenz bei Videotelefonie | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.5.4 | Synchronisation von Audio und Video bei Videotelefonie | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.5.5 | Visuelle Anzeige von Audio-Aktivität | Zweiwege-Sprachkommunikation |  |  |  |  |
| 6.5.6 | Sprecheridentifizierung bei Gebärdensprachen-Kommunikation | Zweiwege-Sprachkommunikation |  |  |  |  |

#### Videofähigkeiten

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 7.1.1 | Wiedergabe von Untertiteln | Video |  |  |  |  |
| 7.1.2 | Synchronisation der Untertitel | Video |  |  |  |  |
| 7.1.3 | Erhaltung von Untertiteln | Video |  |  |  |  |
| 7.1.4 | Anpassungen für Untertitel | Video |  |  |  |  |
| 7.1.5 | Gesprochene Untertitel | Video |  |  |  |  |
| 7.2.1 | Wiedergabe der Audiodeskription | Video |  |  |  |  |
| 7.2.2 | Synchronisation der Audiodeskription | Video |  |  |  |  |
| 7.2.3 | Erhaltung der Audiodeskription | Video |  |  |  |  |
| 7.3 | Bedienelemente für Untertitel und Audiodeskription | Video |  |  |  |  |

#### Wahrnehmbarkeit

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.1.1.1a | Alternativtexte für informative Grafiken | Basisfunktionalität |  |  |  |  |
| 11.1.1.1b | Alternativtexte für dekorative Grafiken | Basisfunktionalität |  |  |  |  |
| 11.1.1.1c | Alternativen für CAPTCHAs | Basisfunktionalität |  |  |  |  |
| 11.1.2.1 | Alternativen für aufgezeichnetes Audio und Videos ohne Ton | Video | Funktionsbereich (Z-2) |  |  |  |
| 11.1.2.2 | Untertitel für aufgezeichnete Videos | Video |  |  |  |  |
| 11.1.2.3 | Audiodeskription oder Medienalternative für aufgezeichnete Videos | Video |  |  |  |  |
| 11.1.2.4 | Untertitel für Live-Videos | Video |  |  |  |  |
| 11.1.2.5 | Audiodeskription für aufgezeichnete Videos | Video |  |  |  |  |
| 11.1.3.1a | Strukturelemente für Überschriften | Basisfunktionalität |  |  |  |  |
| 11.1.3.1b | Strukturelemente für Listen, Auflistungen und Aufzählungen | Basisfunktionalität |  |  |  |  |
| 11.1.3.1c | Strukturelemente für weitere Objekte | Basisfunktionalität |  |  |  |  |
| 11.1.3.1d | Keine Strukturelemente für Layouttabellen | Basisfunktionalität | Funktionsbereich vom übergeordneten Prüfschritt übernommen |  |  |  |
| 11.1.3.1e | Strukturelemente für weitere Objekte | Basisfunktionalität | Funktionsbereich vom übergeordneten Prüfschritt übernommen |  |  |  |
| 11.1.3.2 | Sinnvolle Reihenfolge | Basisfunktionalität | Z-7 |  |  |  |
| 11.1.3.3 | Sensorische Eigenschaften | Basisfunktionalität |  |  |  |  |
| 11.1.3.4 | Keine Beschränkung der Bildschirmausrichtung | Basisfunktionalität |  |  |  |  |
| 11.1.3.5 | Eingabezweck bestimmen | Basisfunktionalität |  |  |  |  |
| 11.1.4.1 | Verwendung von Farbe | Basisfunktionalität |  |  |  |  |
| 11.1.4.2 | Audio-Steuerelement | Audio | Funktionsbereich (Z-1) |  |  |  |
| 11.1.4.3 | Kontrast von Texten | Basisfunktionalität |  |  |  |  |
| 11.1.4.4 | Textgröße ändern | Basisfunktionalität |  |  |  |  |
| 11.1.4.5 | Schriftgrafiken | Basisfunktionalität |  |  |  |  |
| 11.1.4.10 | Automatischer Umbruch (Reflow) | Basisfunktionalität |  |  |  |  |
| 11.1.4.11 | Nicht-Text-Kontrast | Basisfunktionalität |  |  |  |  |
| 11.1.4.12 | Textabstand | Basisfunktionalität |  |  |  |  |
| 11.1.4.13 | Eingeblendete Inhalte (Hover oder Fokus) | Basisfunktionalität |  |  |  |  |

#### Bedienbarkeit

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.2.1.1 | Ohne Maus nutzbar | Basisfunktionalität |  |  |  |  |
| 11.2.1.1a | Mit externer Tastatur bedienbar | Basisfunktionalität | Funktionsbereich vom übergeordneten Prüfschritt übernommen |  |  |  |
| 11.2.1.1b | Mit assistiven Technologien bedienbar | Basisfunktionalität | Funktionsbereich vom übergeordneten Prüfschritt übernommen |  |  |  |
| 11.2.1.2 | Keine Tastaturfalle | Basisfunktionalität |  |  |  |  |
| 11.2.1.4 | Tastaturkürzel | Basisfunktionalität |  |  |  |  |
| 11.2.2.1 | Zeitvorgaben anpassbar | Basisfunktionalität |  |  |  |  |
| 11.2.2.2 | Bewegte Inhalte abschaltbar | Basisfunktionalität |  |  |  |  |
| 11.2.3.1 | Verzicht auf Flackern | Basisfunktionalität |  |  |  |  |
| 11.2.4.3 | Sinnvolle Reihenfolge bei Bedienung mit externer Tastatur | Basisfunktionalität |  |  |  |  |
| 11.2.4.4 | Aussagekräftige Beschriftungen von Links | Basisfunktionalität |  |  |  |  |
| 11.2.4.6 | Überschriften und Beschriftungen | Basisfunktionalität |  |  |  |  |
| 11.2.4.7 | Fokus sichtbar | Basisfunktionalität |  |  |  |  |
| 11.2.4.11 | Fokus nicht verdeckt (Minimum) | Basisfunktionalität |  |  |  |  |
| 11.2.5.1 | Zeigergesten (Alternativen für komplexe Zeiger-Gesten) | Basisfunktionalität |  |  |  |  |
| 11.2.5.2 | Abbrechen oder Widerrufen einer Touch-Geste | Basisfunktionalität |  |  |  |  |
| 11.2.5.3 | Sichtbare Beschriftung als Teil der programmatischen Beschriftung | Basisfunktionalität |  |  |  |  |
| 11.2.5.4 | Steuerung durch Bewegung | Basisfunktionalität |  |  |  |  |
| 11.2.5.7 | Ziehbewegungen | Basisfunktionalität |  |  |  |  |
| 11.2.5.8 | Zielgröße (Minimum) | Basisfunktionalität |  |  |  |  |

#### Verständlichkeit

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.3.1.1 | Sprache der Software | Basisfunktionalität |  |  |  |  |
| 11.3.2.1 | Kontextänderung bei Fokus | Basisfunktionalität |  |  |  |  |
| 11.3.2.2 | Kontextänderung bei Eingabe | Basisfunktionalität |  |  |  |  |
| 11.3.2.6 | Konsistente Hilfe | Basisfunktionalität |  |  |  |  |
| 11.3.3.1 | Fehlererkennung | Basisfunktionalität |  |  |  |  |
| 11.3.3.2 | Beschriftungen oder Anweisungen bei Formularfeldern | Basisfunktionalität |  |  |  |  |
| 11.3.3.3 | Fehlerkorrektur | Basisfunktionalität |  |  |  |  |
| 11.3.3.4 | Fehlervermeidung | Basisfunktionalität |  |  |  |  |
| 11.3.3.7 | Redundanter Eintrag | Basisfunktionalität |  |  |  |  |
| 11.3.3.8a | Zugänglicher Anmeldeprozess | Basisfunktionalität |  |  |  |  |
| 11.3.3.8b | Zugängliche Zwei-Faktor-Authentifizierung | Basisfunktionalität |  |  |  |  |
| 11.3.3.8c | Zugängliche CAPTCHAs in Anmeldeprozessen | Basisfunktionalität |  |  |  |  |

#### Robustheit

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.4.1.1 | Syntaxanalyse | Basisfunktionalität |  |  |  |  |
| 11.4.1.2 | Name, Rolle, Wert | Basisfunktionalität |  |  |  |  |
| 11.4.1.3 | Statusmeldungen | Basisfunktionalität |  |  |  |  |

#### Interoperabilität mit assistiven Technologien

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.5.2.3 | Verwendung von Barrierefreiheitsdiensten | Basisfunktionalität |  |  |  |  |
| 11.5.2.5 | Objektinformationen | Basisfunktionalität |  |  |  |  |
| 11.5.2.6 | Datentabellen | Basisfunktionalität |  |  |  |  |
| 11.5.2.7 | Werte | Basisfunktionalität |  |  |  |  |
| 11.5.2.8 | Label-Beziehungen | Basisfunktionalität |  |  |  |  |
| 11.5.2.9 | Eltern-Kind-Beziehungen | Basisfunktionalität |  |  |  |  |
| 11.5.2.10a | Nicht bearbeitbarer Text | Basisfunktionalität |  |  |  |  |
| 11.5.2.10b | Bearbeitbarer Text | Basisfunktionalität |  |  |  |  |
| 11.5.2.11 | Übersicht verfügbarer Aktionen | Basisfunktionalität |  |  |  |  |
| 11.5.2.12 | Ausführung verfügbarer Aktionen | Basisfunktionalität |  |  |  |  |
| 11.5.2.13 | Nachverfolgung des Fokus und der Auswahlattribute | Basisfunktionalität |  |  |  |  |
| 11.5.2.14 | Änderung des Fokus und der Auswahlattribute | Basisfunktionalität |  |  |  |  |
| 11.5.2.15 | Änderungsbenachrichtigung | Basisfunktionalität |  |  |  |  |
| 11.5.2.16 | Änderungen von Zuständen und Eigenschaften | Basisfunktionalität |  |  |  |  |
| 11.5.2.17 | Änderungen von Werten und Text | Basisfunktionalität |  |  |  |  |

#### Dokumentierte Nutzung

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.6.2 | Keine Unterbrechung der Barrierefreiheitsfunktionen | Basisfunktionalität |  |  |  |  |

#### Benutzerpräferenzen

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.7 | Individuelle Benutzerpräferenzen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |

#### Autorenwerkzeuge

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 11.8.1 | Inhaltstechnologie | Autorenwerkzeug | Funktionsbereich (Z-1) |  |  |  |
| 11.8.2 | Erstellung barrierefreier Inhalte | Autorenwerkzeug | Funktionsbereich (Z-1) |  |  |  |
| 11.8.3 | Erhaltung von Barrierefreiheit bei Umwandlungen | Autorenwerkzeug | Funktionsbereich (Z-1) |  |  |  |
| 11.8.4 | Reparaturunterstützung | Autorenwerkzeug | Funktionsbereich (Z-1) |  |  |  |
| 11.8.5 | Vorlagen | Autorenwerkzeug | Funktionsbereich (Z-1) |  |  |  |

#### Dokumentation und unterstützende Dienste

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 12.1.1 | Dokumentation von Barrierefreiheit und Kompatibilität | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 12.1.2 | Barrierefreie Dokumentation | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 12.2.2 | Informationen zu Barrierefreiheits- und Kompatibilitätsfunktionen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 12.2.3 | Effektive Kommunikation | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 12.2.4 | Barrierefreie Dokumentation durch den Produktsupport | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |

### 3.2 Teil B – PDF-Test

| Kriterium | Titel (BIT inklusiv) | Funktionsbereich | Zu bestätigen | Verständlicher Titel | Priorität | Begründung |
| --- | --- | --- | --- | --- | --- | --- |
| 10.1.1.1 | Nicht-Text-Inhalt | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.2.1 | Alternativen für aufgezeichnetes Audio und Videos ohne Ton | Video | Funktionsbereich (Z-1) |  |  |  |
| 10.1.2.2 | Untertitel für aufgezeichnete Videos | Video | Funktionsbereich (Z-1) |  |  |  |
| 10.1.2.3 | Audiodeskription oder Medienalternative für aufgezeichnete Videos | Video | Funktionsbereich (Z-1) |  |  |  |
| 10.1.2.4 | Untertitel für Live-Videos | Video | Funktionsbereich (Z-1) |  |  |  |
| 10.1.2.5 | Audiodeskription für aufgezeichnete Videos | Video | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1a | Info und Beziehungen: Getaggtes Dokument | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1b | Info und Beziehungen: OCR | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1c | Info und Beziehungen: Gruppierende Strukturelemente/Rootelemente | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1d | Info und Beziehungen: Gruppierende Strukturelemente/Inhaltsverzeichnis | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1e | Info und Beziehungen: Gruppierende Strukturelemente/BlockQuote, Index, NonStruct, Private | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1f | Info und Beziehungen: Blocklevel Strukturelemente/Überschriften | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1g | Info und Beziehungen: Blocklevel Strukturelemente/Listen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1h | Info und Beziehungen: Blocklevel Strukturelemente/Tabellen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.1i | Info und Beziehungen: Blocklevel Strukturelemente/Absätze | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.2 | Sinnvolle Reihenfolge | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.3 | Sensorische Merkmale | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.4 | Bildschirmausrichtung anpassbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.3.5 | Eingabezweck bestimmen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.1 | Verwendung von Farbe | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.2 | Ton abschaltbar | Audio | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.3 | Kontrast von Texten | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.4 | Textgröße ändern | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.5 | Schriftgrafiken | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.10 | Automatischer Umbruch (Reflow) | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.11 | Nicht-Text-Kontrast | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.12 | Textabstand | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.1.4.13 | Eingeblendete Inhalte | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.1.1 | Ohne Maus nutzbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.1.2 | Keine Tastaturfalle | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.1.4 | Tastatur-Kurzbefehle abschaltbar oder anpassbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.2.1 | Zeitvorgaben anpassbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.2.2 | Bewegte Inhalte abschaltbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.3.1 | Verzicht auf Flackern | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.4.2 | Sinnvoller Dokumenttitel | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.4.3 | Sinnvolle Reihenfolge bei der Bedienung mit externer Tastatur | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.4.4 | Aussagekräftige Beschriftungen von Links | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.4.6 | Aussagekräftige Überschriften und Beschriftungen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.4.7 | Fokus sichtbar | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.5.1 | Alternativen für komplexe Gesten | Basisfunktionalität | Funktionsbereich (Z-1); nicht anwendbar (Z-4) |  |  |  |
| 10.2.5.2 | Abbrechen oder Widerrufen einer Touch-Geste | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.5.3 | Sichtbare Beschriftung Teil der programmatischen Beschriftung | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.2.5.4 | Steuerung durch Bewegung | Basisfunktionalität | Funktionsbereich (Z-1); nicht anwendbar (Z-4) |  |  |  |
| 10.3.1.1 | Sprache des PDF-Dokuments | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.1.2 | Sprache von einzelnen Abschnitten/Sätzen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.2.1 | Keine Kontextänderung bei Fokus | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.2.2 | Keine Kontextänderung bei Eingabe | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.3.1 | Fehlererkennung | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.3.2 | Verständliche Beschriftungen oder Anweisungen bei Formularfeldern | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.3.3 | Fehlerkorrektur | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.3.3.4 | Fehlervermeidung | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.4.1.1 | Korrekte Syntax | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.4.1.2 | Name, Rolle, Wert | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| 10.4.1.3 | Statusmeldungen | Basisfunktionalität | Funktionsbereich (Z-1); nicht anwendbar (Z-4) |  |  |  |
| UA1-6.2 | Metadaten und UA-Kennzeichnung | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.7 | Unicode-Zuordnung | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.10 | Optionaler Inhalt | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.16 | Sicherheit | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.17 | Navigation über Lesezeichen | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.20 | Externe Objekte | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |
| UA1-7.21 | Schriften | Basisfunktionalität | Funktionsbereich (Z-1) |  |  |  |

### 3.3 Geprüft nach …

| Kriterium | Titel | Priorität | Begründung |
| --- | --- | --- | --- |
| PROC-softwareTest | Geprüft nach BIT inklusiv Software-Test | B (entschieden) |  |
| PROC-appTest | Geprüft nach BIT inklusiv App-Test | B (entschieden) |  |
| PROC-pdfTest | Geprüft nach BIT inklusiv PDF-Test | B (entschieden) |  |
| PROC-bikWeb | Geprüft nach BIK BITV-Test (Web) | B (entschieden) |  |
| PROC-bikApp | Geprüft nach BIK BITV-Test (App) | B (entschieden) |  |

## 4. Änderungsverlauf

| Version | Datum | Änderung |
| --- | --- | --- |
| v1 | 9. November 2026 | Erste Fassung mit S3-27 |
| v2 | 11. November 2026 | Z-7 (Arbeitsmarkierung in 11.1.3.2) mit S4-15b |
| v3 | 12. November 2026 | Feld `shortDescriptionDe` für die Kurzbeschreibung mit S4-24 |
