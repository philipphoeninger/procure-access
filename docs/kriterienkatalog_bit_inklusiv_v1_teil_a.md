# Kriterienkatalog ProcureAccess – Teil A: Software-Test und App-Test

Gehört zu kriterienkatalog_bit_inklusiv_v1.md (Grundlagen, Filter, Bewertungsskala, Darstellung). Quellen und Nutzungsrechte siehe dort.

## Kriterien aus Software-Test und App-Test

Reihenfolge und Nummern folgen EN 301 549. Ein Kriterium pro Nummer. Wo Software-Test und App-Test ein Feld wortgleich formulieren, steht es nur einmal unter „Gleichlautend in beiden Verfahren“.

### Kapitel: Allgemeine Anforderungen

<a id="k-5-2"></a>

#### 5.2 Aktivierung von Barrierefreiheitsfunktionen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#aktivierung-von-barrierefreiheitsfunktionen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#aktivierung-von-barrierefreiheitsfunktionen) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Barrierefreiheitsfunktionen können Nutzergruppen mit verschiedenen Beeinträchtigungen helfen, die Inhalte der Seite nach den jeweiligen Bedürfnissen anzupassen. Hierzu ist es nötig, dass diese Funktionen von der betroffenen Nutzergruppe selbständig gefunden und aktiviert werden können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Funktion zum Aktivieren von leichter Sprache ist einfach identifizierbar und aktivierbar.
- Die Taste zum Ändern von Kontrasten, Schriftarten oder Farben erfüllt die Anforderungen des Prüfschritts 11.1.4.11 Nicht-Text-Kontrast.
- Die Taste zum Ändern von Schriftgröße oder Zeilenabstand ist programmatisch ermittelbar und gibt korrekte Werte zum Aktivitätszustand aus.

**Beispiele für negative Bewertung**

- Die Bedienelemente sind versteckt und nur schwer auffindbar.
- Die Bedienelemente sind nicht eindeutig gekennzeichnet oder nicht aktivierbar.
- Die Bedienelemente verstoßen gegen weitere Anforderungen der EN 301549.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#aktivierung-von-barrierefreiheitsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software eigene, vom Standardumfang des Betriebssystems unabhängige, assistive Bedienungshilfen zur Verfügung stellt.

**Anforderung**

Bietet die Software eigene assistive Unterstützungen an, so müssen die Funktionen für die jeweilige Zielgruppe barrierefrei auffindbar und aktivierbar sein. Nachdem verschiedene Funktionen für verschiedene Nutzergruppen bereitgestellt werden können, deren Anforderungen unterschiedlich sind, kann kein allgemein gültiges Vorgehen definiert werden.

Beispiele für individuelle Funktionen:

- Die Software bietet eine Vorlesefunktion an.
- Die Software bietet Inhalte in leichter Sprache oder Gebärdensprache an.
- Die Software bietet das Verändern von Farben oder Kontrasten an.
- Die Software bietet das Verändern von Schriftart, Schriftgröße oder Zeilenabstand an.
- Die Software bietet Funktionen zum Vergrößern oder Verkleinern von Inhalten an.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#aktivierung-von-barrierefreiheitsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App eigene, vom Standardumfang des Betriebssystems unabhängige, assistive Bedienungshilfen zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App eigene assistive Unterstützungen an, so müssen die Funktionen für die jeweilige Zielgruppe barrierefrei auffindbar und aktivierbar sein. Nachdem verschiedene Funktionen für verschiedene Nutzergruppen bereitgestellt werden können, deren Anforderungen unterschiedlich sind, kann kein allgemein gültiges Vorgehen definiert werden.

Beispiele für individuelle Funktionen:

- Die Mobile App bietet eine Vorlesefunktion an.
- Die Mobile App bietet Inhalte in leichter Sprache oder Gebärdensprache an.
- Die Mobile App bietet das Verändern von Farben oder Kontrasten an.
- Die Mobile App bietet das Verändern von Schriftart, Schriftgröße oder Zeilenabstand an.
- Die Mobile App bietet Funktionen zum Vergrößern oder Verkleinern von Inhalten an.

<a id="k-5-3"></a>

#### 5.3 Biometrie

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Biometrie |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#biometrie) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#biometrie) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Anmeldeprozesse, die auf der Auswertung biometrischer Merkmale, wie zum Beispiel Fingerabdruck, Sprache oder Gesicht basieren, können dem Nutzenden den Zugang zum digitalen Produkt erleichtern. Für Menschen, die aufgrund einer Beeinträchtigung eine bestimmte biometrische Authentifizierung nicht nutzen können, ist eine geeignete Alternative nötig. Eine nicht biometrische Methode ist für die meisten Nutzenden bedienbar.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Zusätzlich zu einer biometrischen Methode gibt es mindestens eine weitere biometrische (beispielsweise Gesichtserkennung, Fingerabdruck) oder nicht-biometrische Methode (beispielsweise Passworteingabe, PIN-Eingabe).

**Beispiele für negative Bewertung**

- Es stehen keine Alternativen zu einer biometrischen Methode zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#biometrie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software biometrische Merkmale zur Identifikation eines Nutzenden oder zum Log-In eines Nutzenden zur Verfügung stellt, beziehungsweise auf die biometrische Identifikation des Betriebssystems zurückgreift.

**Anforderung**

Bietet die Software eine biometrische Identifizierungsmethode an, so darf der Identifikationsvorgang nicht nur auf das biometrische Merkmal beschränkt sein. Es muss mindestens eine weitere Identifizierungsmethode unterstützt werden. Eine Alternative zu einer biometrischen Identifizierungsmethode kann sowohl eine weitere biometrische Methode als auch eine nicht biometrische Methode sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#biometrie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App biometrische Merkmale zur Identifikation eines Nutzenden oder zum Log-In eines Nutzenden zur Verfügung stellt, beziehungsweise auf die biometrische Identifikation des Betriebssystems zurückgreift.

**Anforderung**

Bietet die Mobile App eine biometrische Identifizierungsmethode an, so darf der Identifikationsvorgang nicht nur auf das biometrische Merkmal beschränkt sein. Es muss mindestens eine weitere Identifizierungsmethode unterstützt werden. Eine Alternative zu einer biometrischen Identifizierungsmethode kann sowohl eine weitere biometrische Methode als auch eine nicht biometrische Methode sein.

<a id="k-5-4"></a>

#### 5.4 Erhaltung von Barrierefreiheitsinformationen bei Konvertierung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#erhaltung-von-barrierefreiheitsinformationen-bei-konvertierung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#erhaltung-von-barrierefreiheitsinformationen-bei-konvertierung) |

##### Gleichlautend in beiden Verfahren

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#erhaltung-von-barrierefreiheitsinformationen-bei-konvertierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Konvertierungen von Inhalten der Software in andere Formate zur Verfügung stellt.

**Anforderung**

Bietet die Software Konvertierungsmöglichkeiten in andere Formate an, müssen bereits vorhandene Barrierefreiheitsinformationen und -Funktionen im Zielformat übernommen werden, wenn das Zielformat diese Informationen und Funktionen grundsätzlich unterstützt.

**Warum wird das geprüft?**

Wenn das digitale Produkt Dokumente in Formate konvertiert, die grundsätzlich Barrierefreiheitsinformationen unterstützen, dann sollen diese im Zieldokument korrekt übernommen werden. Beim Konvertieren von Dokumenten in andere Formate soll beispielsweise sichergestellt sein, dass das Zieldokument auch mit Screenreadern erfassbar und bedienbar ist.

**Beispiele für positive Bewertung**

- Das Zielformat enthält alle Barrierefreiheitsinformationen, die in der Software verfügbar waren, und die im Zielformat unterstützt werden.
- Nach einem Export in das Zielformat PDF sind im PDF die Überschriftenebenen, Alternativtexte und weitere semantische Informationen erhalten geblieben.
- Nach dem Speichern eines Videos auf dem Mobiltelefon sind die Untertitel im Video erhalten geblieben.

**Beispiele für negative Bewertung**

- Das Zielformat enthält keine oder nur einige Barrierefreiheitsinformationen, die in der Software verfügbar waren.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#erhaltung-von-barrierefreiheitsinformationen-bei-konvertierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte aus der Mobile App heruntergeladen werden können und diese Inhalte außerdem in anderer Form in der Mobile App dargestellt werden. Vorschauansichten von herunterladbaren Inhalten zählen hierbei nicht zur In-App-Darstellung.

**Anforderung**

Bietet die Mobile App Konvertierungsmöglichkeiten in andere Formate an, müssen bereits vorhandene Barrierefreiheitsinformationen und -Funktionen im Zielformat übernommen werden, wenn das Zielformat diese Informationen und Funktionen grundsätzlich unterstützt.

**Warum wird das geprüft?**

Wenn das digitale Produkt Inhalte in Dokumente konvertiert, die grundsätzlich Barrierefreiheitsinformationen unterstützen, dann sollen diese im Zieldokument korrekt übernommen werden. Beim Konvertieren von Inhalten in andere Formate soll beispielsweise sichergestellt sein, dass das Zieldokument auch mit Screenreadern erfassbar und bedienbar ist.

**Beispiele für positive Bewertung**

- Das Zielformat enthält alle Barrierefreiheitsinformationen, die in der Mobile App verfügbar waren, und die im Zielformat unterstützt werden.
- Nach einem Export in das Zielformat PDF sind im PDF die Überschriftenebenen, Alternativtexte und weitere semantische Informationen erhalten geblieben.

**Beispiele für negative Bewertung**

- Das Zielformat enthält keine oder nur einige Barrierefreiheitsinformationen, die in der Mobile App verfügbar waren.

<a id="k-5-5-1"></a>

#### 5.5.1 Möglichkeiten der Bedienung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#möglichkeiten-der-bedienung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#möglichkeiten-der-bedienung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Motorisch beeinträchtigte Menschen können komplexe Bedien-Gesten gegebenenfalls nicht ausführen. Eine alternative Bedienmethode muss zur Verfügung gestellt werden, damit entsprechende Nutzergruppen die Bedienung durchführen können.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für entsprechende Funktionalitäten steht eine alternative Bedienmethode zur Verfügung, weder Greifen, Zusammendrücken oder Drehen des Handgelenks erfordert.

**Beispiele für negative Bewertung**

- Für entsprechende Funktionalitäten steht keine alternative Bedienmethode zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#möglichkeiten-der-bedienung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software zum Ausführen bestimmter Funktionalitäten auf physische Bedienelemente des Endgeräts zurückgreift.

**Anforderung**

Greift die Software zum Ausführen bestimmter Funktionalitäten auf physische Bedienelemente des Endgeräts zurück, die zur Bedienung ein Greifen, Zusammendrücken oder Drehen des Handgelenks erfordern, muss eine barrierefreie, alternative Möglichkeit der Bedienung zur Verfügung stehen, für die diese Handlungen nicht erforderlich sind. Alternative Bedienmethoden können Hardware- oder Software-Lösungen sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#möglichkeiten-der-bedienung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App zum Ausführen bestimmter Funktionalitäten auf physische Bedienelemente des Endgeräts zurückgreift.

**Anforderung**

Greift die Mobile App zum Ausführen bestimmter Funktionalitäten auf physische Bedienelemente des Endgeräts zurück, die zur Bedienung ein Greifen, Zusammendrücken oder Drehen des Handgelenks erfordern, muss eine barrierefreie, alternative Möglichkeit der Bedienung zur Verfügung stehen, für die diese Handlungen nicht erforderlich sind. Alternative Bedienmethoden können Hardware- oder Software-Lösungen sein.

<a id="k-5-5-2"></a>

#### 5.5.2 Unterscheidbarkeit der bedienbaren Elemente

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#unterscheidbarkeit-der-bedienbaren-elemente) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#unterscheidbarkeit-der-bedienbaren-elemente) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#unterscheidbarkeit-der-bedienbaren-elemente) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Wenn die Software bedienbare Teile enthält, muss sie eine Möglichkeit bieten, jedes bedienbare Teil zu erkennen, ohne dass eine Sicht erforderlich ist und ohne dass die mit dem bedienbaren Teil verbundene Handlung ausgeführt werden muss.

Eine Möglichkeit, diese Anforderung zu erfüllen, besteht darin, die bedienbaren Teile taktil wahrnehmbar zu machen, z.B. bei Geld- oder Fahrscheinautomaten mit Touchscreen und Drucktasten. Der Prüfschritt ist bei der Prüfung von Software nicht anwendbar, da die Software keine Möglichkeit hat, ein physikalisches Bedienelement des Endgeräts zu beschriften

**Warum wird das geprüft?**

Sehbeeinträchtigte und blinde Menschen können den Status von Bedienelementen visuell nicht unterscheiden und sind auf andere, geeignete Methoden zur Erkennung des Status angewiesen.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Tasten sind durch unterschiedliche Formen bzw. Beschriftungen eindeutig unterscheidbar.

**Beispiele für negative Bewertung**

- Die Tasten sind taktil nicht unterscheidbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#unterscheidbarkeit-der-bedienbaren-elemente) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist bei der Prüfung von Mobile Apps nicht anwendbar, da die Mobile App keine Möglichkeit hat, ein physikalisches Bedienelement des Endgeräts zu beschriften.

**Bewertungsalternativen:** Nicht anwendbar

<a id="k-5-6-1"></a>

#### 5.6.1 Taktiler oder auditiver Status

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#taktiler-oder-auditiver-status) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#taktiler-oder-auditiver-status) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Sehbeeinträchtigte und blinde Menschen können den Status von Bedienelementen visuell nicht unterscheiden und sind auf andere, geeignete Methoden zur Erkennung des Status angewiesen.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Das Steuerelement vermittelt seinen Status visuell und durch Audioausgabe. Die verschiedenen Status sind dabei durch unterschiedliche Audioausgaben eindeutig unterscheidbar.
- Das Steuerelement vermittelt seinen Status durch Braille-Beschriftung. Die verschiedenen Status sind dabei durch unterschiedliche Beschriftungen eindeutig unterscheidbar.

**Beispiele für negative Bewertung**

- Das Steuerelement vermittelt seinen Status ausschließlich visuell.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#taktiler-oder-auditiver-status) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Funktionalität einer Software mit einem physischen Bedienelement des Endgeräts gesteuert werden kann und der Status des Steuerelements visuell dargestellt wird.

**Anforderung**

Kann eine Funktionalität der Software mit einem physischen Bedienelement des Endgeräts gesteuert werden, und wird der Status des Steuerelements visuell dargestellt, muss zusätzlich zur visuellen Erkennung mindestens ein weiterer Modus zum Identifizieren des jeweiligen Status des Steuerelementes zur Verfügung stehen, entweder durch Berührung oder durch Audioausgabe.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#taktiler-oder-auditiver-status) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Funktionalität einer Mobile App mit einem physischen Bedienelement des Endgeräts gesteuert werden kann und der Status des Steuerelements visuell dargestellt wird.

**Anforderung**

Kann eine Funktionalität der Mobile App mit einem physischen Bedienelement des Endgeräts gesteuert werden, und wird der Status des Steuerelements visuell dargestellt, muss zusätzlich zur visuellen Erkennung mindestens ein weiterer Modus zum Identifizieren des jeweiligen Status des Steuerelementes zur Verfügung stehen, entweder durch Berührung oder durch Audioausgabe.

<a id="k-5-6-2"></a>

#### 5.6.2 Visueller Status

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#visueller-status) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#visueller-status) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Einige Menschen unterschiedlicher Beeinträchtigungen sind auf die visuelle Erkennung des Status von Steuerelementen angewiesen, da andere Erkennungsmethoden für sie unter Umständen nicht zugänglich sind. So können beispielsweise Menschen mit Hörbeeinträchtigung eine Audioausgabe nicht wahrnehmen, oder Menschen ohne Sehbeeinträchtigung kein Braille lesen.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Das Steuerelement vermittelt seinen Status durch Audioausgabe und durch eine visuelle LED. Die verschiedenen Status sind dabei durch unterschiedlich positionierte LED‘s eindeutig unterscheidbar.
- Das Steuerelement vermittelt seinen Status durch Braille-Beschriftung und durch einen beleuchteten Schalter. Die verschiedenen Status sind dabei durch unterschiedliche Schalterpositionen eindeutig unterscheidbar.

**Beispiele für negative Bewertung**

- Das Bedienelement vermittelt seinen Status nicht visuell.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#visueller-status) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Bestandteil eines physischen Endgeräts ist, das ein physisches Steuerelement zum Verriegeln oder Umschalten zur Verfügung stellt und den Status des Steuerelements non-visuell (beispielsweise bei Berührung oder durch Audiowiedergabe) wiedergibt.

**Anforderung**

Bietet die Software ein physisches Steuerelement zum Sperren oder Umschalten, und stellt den Status des Steuerelements non-visuell dar, muss zusätzlich zur vorhandenen Erkennungsmethode eine visuelle Identifizierung des jeweiligen Status des Steuerelementes zur Verfügung stehen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#visueller-status) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Bestandteil eines physischen Endgeräts ist, das ein physisches Steuerelement zum Verriegeln oder Umschalten zur Verfügung stellt und den Status des Steuerelements non-visuell (beispielsweise bei Berührung oder durch Audiowiedergabe) wiedergibt.

**Anforderung**

Bietet die Mobile App ein physisches Steuerelement zum Sperren oder Umschalten, und stellt den Status des Steuerelements non-visuell dar, muss zusätzlich zur vorhandenen Erkennungsmethode eine visuelle Identifizierung des jeweiligen Status des Steuerelementes zur Verfügung stehen.

<a id="k-5-7"></a>

#### 5.7 Tastenwiederholung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#tastenwiederholung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#tastenwiederholung) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#tastenwiederholung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Wenn die Software über eine nicht abschaltbare Tastenwiederholungsfunktion verfügt:

- a) muss die Verzögerung vor der Tastenwiederholung auf mindestens 2 Sekunden einstellbar sein; und
- b) muss die Tastenwiederholungsrate bis auf ein Zeichen pro 2 Sekunden einstellbar sein.

Die entsprechenden Einstellungen sind im Windows-Betriebssystem unter „Systemsteuerung / Center für erleichterte Bedienung / Bedienung mit der Tastatur erleichtern / Anschlagverzögerung einrichten / Wiederhol- und Anschlaggeschwindigkeit einrichten“ zu finden. Die Windows Bildschirmtastatur bietet keine entsprechende Unterstützung dieser Funktionalität, sondern bietet bei längerem Tastendruck die Auswahl von Sonderzeichen an.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für negative Bewertung**

- Die Software bietet keine Möglichkeit die Tastenwiederholung einzustellen oder abzuschalten.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#tastenwiederholung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Bei iOS- und Android-Smartphones ist die Zeitverzögerung für anhaltende Tastatureingaben im Betriebssystem konfigurierbar als auch vollständig abschaltbar. Durch die Möglichkeit des Abschaltens dieser Funktionalität in den genannten Betriebssystemen ist dieser Prüfschritt immer nicht anwendbar.

Tipps zur Bedienung von iOS:

In iOS findet sich diese Einstellung unter „Bedienungshilfen > Tastaturen & Texteingabe > Tastenwiederholung > Wiederholungsintervall“.

Tipps zur Bedienung von Android:

Sofern eine Tastatur verbunden ist, findet sich diese Einstellung in Android unter „System > Tastatur > Physische Tastatur > Bedienungshilfen für die physische Tastatur > Tastatureingabe Wiederholung“.

**Bewertungsalternativen:** Nicht anwendbar

<a id="k-5-8"></a>

#### 5.8 Annahme eines zweifachen Tastenanschlags

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#annahme-eines-zweifachen-tastenanschlags) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#annahme-eines-zweifachen-tastenanschlags) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die aufgrund motorischer Beeinträchtigungen mehr Zeit für Bewegungen mit den Armen oder Fingern benötigen, sind unter Umständen auf ein längeres Intervall bei Verwendung von Tastaturen angewiesen. Mit genug Zeit vor der Annahme eines weiteren Tastenanschlags werden so Fehleingaben vermieden.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Einstellungen des Betriebssystems werden korrekt übernommen.
- Eigene Konfigurationsmöglichkeiten werden angeboten und korrekt übernommen.

**Beispiele für negative Bewertung**

- Die Einstellungen des Betriebssystems werden nicht oder fehlerhaft übernommen.
- Eigene Konfigurationsmöglichkeiten werden nicht oder fehlerhaft übernommen.
- Es werden keine eigenen Konfigurationsmöglichkeiten angeboten, und die Einstellungen des Betriebssystems werden nicht oder fehlerhaft übernommen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#annahme-eines-zweifachen-tastenanschlags) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Eingabefelder für Texteingaben zur Verfügung stellt. Die Anwendbarkeit gilt auch für Software, die Bestandteil eines physischen Endgeräts sind, das eine physische Tastatur zur Verfügung stellt.

**Anforderung**

Individuelle Benutzereinstellungen zur Anschlagsverzögerung, die im Betriebssystem konfigurierbar sind, müssen von der Software übernommen werden. Die Mindestforderungen lauten:

- Zeitverzögerung von mindestens 0,5 Sekunden vor der Annahme eines weiteren Tastenanschlags

Sofern die Software die Einstellungen des Betriebssystems nicht übernimmt, kann sie eine eigene Konfigurationsmöglichkeit zur Verfügung stellen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#annahme-eines-zweifachen-tastenanschlags) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Eingabefelder für Texteingaben zur Verfügung stellt. Die Anwendbarkeit gilt gleichermaßen für Mobile Apps auf Mobiltelefonen, als auch für Mobile Apps, die Bestandteil eines physischen Endgeräts sind, das eine physische Tastatur zur Verfügung stellt.

**Anforderung**

Individuelle Benutzereinstellungen zur Anschlagsverzögerung, die im Betriebssystem konfigurierbar sind, müssen von der Mobile App übernommen werden. Die Mindestforderungen lauten:

- Zeitverzögerung von mindestens 0,5 Sekunden vor der Annahme eines weiteren Tastenanschlags

Sofern die Mobile App die Einstellungen des Betriebssystems nicht übernimmt, kann sie eine eigene Konfigurationsmöglichkeit zur Verfügung stellen.

<a id="k-5-9"></a>

#### 5.9 Gleichzeitige Benutzerhandlungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#gleichzeitige-benutzerhandlungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#gleichzeitige-benutzerhandlungen) |

##### Gleichlautend in beiden Verfahren

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#gleichzeitige-benutzerhandlungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Funktionalitäten zur Verfügung stellt, für deren Bedienung gleichzeitige Benutzeraktionen oder komplexe Touch-Gesten mit mindestens zwei Fingern erforderlich sind.

**Anforderung**

Sofern die Software zum Betrieb oder zur Bedienung gleichzeitige Benutzerhandlungen oder komplexe Touch-Gesten mit mindestens zwei Fingern erfordert, muss sie einen alternativen Bedienmodus bereitstellen, der keine gleichzeitigen Benutzerhandlungen für den Betrieb oder die Bedienung benötigt.

**Warum wird das geprüft?**

Motorisch beeinträchtigte Menschen können komplexe oder gleichzeitige Benutzerhandlungen gegebenenfalls nicht ausführen. Daher soll es in Software mindestens eine einfach anzuwendende Alternative geben, um entsprechende Aktionen auszuführen.

**Beispiele für positive Bewertung**

- Es wird ein alternativer Bedienmodus zur Verfügung gestellt, für den zum Ausführen der entsprechenden Aktion keine der in den Anforderungen definierten, gleichzeitigen Benutzerhandlungen erfordert.

**Beispiele für negative Bewertung**

- Es wird kein alternativer Bedienmodus zur Verfügung gestellt. Entsprechende Aktionen erfordern, die in den Anforderungen definierten, gleichzeitigen Benutzerhandlungen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#gleichzeitige-benutzerhandlungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Funktionalitäten zur Verfügung stellt, für deren Bedienung gleichzeitige Benutzeraktionen oder komplexe Touch-Gesten mit mindestens zwei Fingern erforderlich sind.

**Anforderung**

Sofern die Mobile App zum Betrieb oder zur Bedienung gleichzeitige Benutzerhandlungen oder komplexe Touch-Gesten mit mindestens zwei Fingern erfordert, muss sie einen alternativen Bedienmodus bereitstellen, der keine gleichzeitigen Benutzerhandlungen für den Betrieb oder die Bedienung benötigt.

Anmerkung: beide Hände benutzen zu müssen, um beispielsweise ein Mobiltelefon aufzuklappen, oder um eine ausklappbare Tastatur zu öffnen, zwei oder mehr Tasten gleichzeitig drücken zu müssen, oder eine Oberfläche mit mehr als einem Finger berühren zu müssen sind Beispiele für gleichzeitige Benutzerhandlungen.

**Warum wird das geprüft?**

Motorisch beeinträchtigte Menschen können komplexe oder gleichzeitige Benutzerhandlungen gegebenenfalls nicht ausführen. Daher soll es in Mobile Apps mindestens eine einfach anzuwendende Alternative geben, um entsprechende Aktionen auszuführen.

**Beispiele für positive Bewertung**

- Es wird ein alternativer Bedienmodus zur Verfügung gestellt, für den zum Ausführen der entsprechenden Aktion keine der in den Anforderungen definierten, gleichzeitigen Benutzerhandlungen erfordert.
- Eine Kamera stellt zum Auslösen einen zeitverzögerten Selbstauslöser zur Verfügung, damit vor dem Auslösen die Kamera entsprechend justiert werden kann.

**Beispiele für negative Bewertung**

- Es wird kein alternativer Bedienmodus zur Verfügung gestellt. Entsprechende Aktionen erfordern, die in den Anforderungen definierten, gleichzeitigen Benutzerhandlungen.
- Eine Kamera stellt zum Auslösen keinen zeitverzögerten Selbstauslöser zur Verfügung; die Kamera muss zeitgleich mit dem Auslöser justiert werden.

### Kapitel: Anforderungen an Technologien mit Zwei-Wege-Sprachkommunikation

<a id="k-6-1"></a>

#### 6.1 Audio-Bandbreite für Sprache

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#audio-bandbreite-für-sprache) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#audio-bandbreite-für-sprache) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Zum Sicherstellen von entsprechender Sprachverständlichkeit bei der Zwei-Wege-Sprachkommunikation soll zum Kodieren und Dekodieren von Audio ein Frequenzbereich genutzt werden, dessen obere Grenze bis mindestens 7.000 Hz reicht. Dieser Frequenzbereich kann beispielsweise mit dem Standard-Codec G.722 [i.21] erreicht werden.

**Warum wird das geprüft?**

Klare und gut verständliche Audioqualität bei der Zwei-Wege-Kommunikation ist besonders für Menschen mit Hörbeeinträchtigungen essenziell. Über einen verbindlichen Mindeststandard für Audioqualität soll diese Anforderung erfüllt werden.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Qualität von mindestens zwei der drei getätigten Anrufe wurde subjektiv als hochwertig beurteilt.

**Beispiele für negative Bewertung**

- Die Qualität von mindestens zwei der drei getätigten Anrufe wurde subjektiv als nicht hochwertig beurteilt.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#audio-bandbreite-für-sprache) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software eine Funktion zur Zwei-Wege-Sprachkommunikation zur Verfügung stellt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#audio-bandbreite-für-sprache) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App eine Funktion zur Zwei-Wege-Sprachkommunikation zur Verfügung stellt.

<a id="k-6-2-1-1"></a>

#### 6.2.1.1 RTT-Kommunikation

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#rtt-kommunikation) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#rtt-kommunikation) |

##### Gleichlautend in beiden Verfahren

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Eingaben des Versenders können in Echtzeit, bzw. mit kaum merkbarer Latenz eingesehen werden.

**Beispiele für negative Bewertung**

- Die Eingaben des Versenders können nicht, bzw. nur mit merkbarer Latenz eingesehen werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#rtt-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software eine Zwei-Wege-Sprachkommunikation zur Verfügung stellt.

**Anforderung**

Zusätzlich zur Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie, muss die Software eine Textkommunikation in Echtzeit (RTT = Real-Time Text) zur Verfügung stellen.

Bei RTT handelt es sich um Text, der sofort übertragen wird, während er eingegeben oder erstellt wird. Dadurch können die Empfänger die Nachricht sofort lesen, während sie geschrieben wird.

**Warum wird das geprüft?**

Menschen, die Sprache zur Kommunikation nicht nutzen können oder wollen, erhalten die Möglichkeit, über Echtzeit-Textkommunikation (RTT) an Konversationen in der entsprechenden Software teilzunehmen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#rtt-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App eine Zwei-Wege-Sprachkommunikation zur Verfügung stellt.

**Anforderung**

Zusätzlich zur Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie, muss die Mobile App eine Textkommunikation in Echtzeit (RTT = Real-Time Text) zur Verfügung stellen.

Bei RTT handelt es sich um Text, der sofort übertragen wird, während er eingegeben oder erstellt wird. Dadurch können die Empfänger die Nachricht sofort lesen, während sie geschrieben wird.

**Warum wird das geprüft?**

Menschen, die Sprache zur Kommunikation nicht nutzen können oder wollen, erhalten die Möglichkeit, über Echtzeit-Textkommunikation (RTT) an Konversationen in der entsprechenden Mobile App teilzunehmen.

<a id="k-6-2-1-2"></a>

#### 6.2.1.2 Gleichzeitige Verwendung von Sprache und Text

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#gleichzeitige-verwendung-von-sprache-und-text) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#gleichzeitige-verwendung-von-sprache-und-text) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die Sprache zur Kommunikation nicht nutzen können oder wollen, sollen im Vergleich zu Sprachnutzenden gleichwertig behandelt werden.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#gleichzeitige-verwendung-von-sprache-und-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt.

**Anforderung**

Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), sollen beide Kommunikationskanäle gleichwertig genutzt werden können.

**Beispiele für positive Bewertung**

- Funktionen wie „Handheben“ oder „abwechselnde Kommunikation” (beispielsweise die Darstellung des aktuellen Sprechers im Fokus) werden gegenüber Sprach- und Textnutzenden gleichwertig behandelt.
- Chat-Funktion und Echtzeit-Text (RTT) sind separiert voneinander verwendbar.

**Beispiele für negative Bewertung**

- Funktionen wie „Handheben“ oder „abwechselnde Kommunikation” (beispielsweise die Darstellung des aktuellen Sprechers im Fokus) werden gegenüber Sprach- und Textnutzenden nicht gleichwertig behandelt.
- Chat-Funktion und Echtzeit-Text (RTT) sind nicht separiert voneinander verwendbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#gleichzeitige-verwendung-von-sprache-und-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), sollen beide Kommunikationskanäle gleichwertig genutzt werden können.

**Beispiele für positive Bewertung**

- Funktionen wie „Handheben“ oder „abwechselnde Kommunikation (beispielsweise die Darstellung des aktuellen Sprechers im Fokus)“ werden gegenüber Sprach- und Textnutzenden gleichwertig behandelt.
- Chat-Funktion und Echtzeit-Text (RTT) sind separiert voneinander verwendbar.

**Beispiele für negative Bewertung**

- Funktionen wie „Handheben“ oder „abwechselnde Kommunikation (beispielsweise die Darstellung des aktuellen Sprechers im Fokus)“ werden gegenüber Sprach- und Textnutzenden nicht gleichwertig behandelt.
- Chat-Funktion und Echtzeit-Text (RTT) sind nicht separiert voneinander verwendbar.

<a id="k-6-2-2-1"></a>

#### 6.2.2.1 Visuell unterscheidbare Darstellung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#visuell-unterscheidbare-darstellung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#visuell-unterscheidbare-darstellung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die Echtzeit-Textkommunikation verwenden, sollen gesendete und empfangene Nachrichten visuell einfach und hinreichend voneinander unterscheiden können.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Gesendete und empfangene Nachrichten sind deutlich identifizierbar und unterscheidbar.

**Beispiele für negative Bewertung**

- Gesendete und empfangene Nachrichten sind nicht deutlich unterscheidbar.
- Gesendete und empfangene Nachrichten sind ausschließlich über die optische Darstellung unterscheidbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#visuell-unterscheidbare-darstellung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Echtzeit-Textkommunikation (RTT = Real-Time Text) zur Verfügung stellt.

**Anforderung**

Bietet die Software Echtzeit-Textkommunikation (RTT) an, sollen gesendete und empfangene Nachrichten visuell hinreichend voneinander unterscheidbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#visuell-unterscheidbare-darstellung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Echtzeit-Textkommunikation (RTT = Real-Time Text) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Echtzeit-Textkommunikation (RTT) an, sollen gesendete und empfangene Nachrichten visuell hinreichend voneinander unterscheidbar sein.

<a id="k-6-2-2-2"></a>

#### 6.2.2.2 Programmatisch bestimmbare Sende- und Empfangsrichtung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#programmatisch-bestimmbare-sende--und-empfangsrichtung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#programmatisch-bestimmbare-sende--und-empfangsrichtung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die Echtzeit-Textkommunikation in Verbindung mit einem Screenreader verwenden, sollen gesendete und empfangene Nachrichten hinreichend voneinander unterscheiden können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Sender und Empfänger können eindeutig voneinander unterschieden werden.
- Gesendete und empfangene Nachrichten können im Kontext eindeutig dem Sender bzw. Empfänger zugeordnet werden.

**Beispiele für negative Bewertung**

- Sender und Empfänger können nicht eindeutig voneinander unterschieden werden.
- Gesendete und empfangene Nachrichten werden vorgelesen, können aber im Kontext nicht dem Sender bzw. Empfänger zugeordnet werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#programmatisch-bestimmbare-sende--und-empfangsrichtung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt.

**Anforderung**

Bietet die Software Echtzeit-Textkommunikation (RTT) an, sollen Sende- und Empfangsrichtung der gesendeten und empfangenen Nachrichten programmatisch ermittelbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#programmatisch-bestimmbare-sende--und-empfangsrichtung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Echtzeit-Textkommunikation (RTT) an, sollen Sende- und Empfangsrichtung der gesendeten und empfangenen Nachrichten programmatisch ermittelbar sein.

<a id="k-6-2-2-3"></a>

#### 6.2.2.3 Sprecheridentifizierung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sprecheridentifizierung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sprecheridentifizierung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Für Teilnehmer in einer Kommunikation ist die Identifikation des aktiven Redners essenziell, um im Kontext entsprechend reagieren zu können. Sprache und Text müssen daher gleichwertig behandelt werden.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Sprach- und Texteingabe werden gleichwertig identifiziert, markiert bzw. hervorgehoben.

**Beispiele für negative Bewertung**

- Nur eine der beiden Kommunikationsarten wird identifiziert, markiert bzw. hervorgehoben.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sprecheridentifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt, und dabei den aktiven Sprecher anhand der Sprache identifiziert:

**Anforderung**

Bietet die Software Echtzeit-Textkommunikation (RTT) an und identifiziert den aktiven Sprecher anhand der Sprache, sollen auch Menschen, die Texteingaben anstatt Sprache verwenden, gleichwertig identifiziert werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sprecheridentifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt, und dabei den aktiven Sprecher anhand der Sprache identifiziert:

**Anforderung**

Bietet die Mobile App Echtzeit-Textkommunikation (RTT) an und identifiziert den aktiven Sprecher anhand der Sprache, sollen auch Menschen, die Texteingaben anstatt Sprache verwenden, gleichwertig identifiziert werden.

<a id="k-6-2-2-4"></a>

#### 6.2.2.4 Anzeige von Audioaktivität in Echtzeit

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#anzeige-von-audioaktivität-in-echtzeit) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#anzeige-von-audioaktivität-in-echtzeit) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Für Teilnehmer in einer Kommunikation ist die Identifikation von Audioaktivität in Verbindung mit dem entsprechenden Teilnehmer essenziell, um im Kontext die jeweilige Aktivität dem Teilnehmer zuordnen zu können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Audioaktivität eines Teilnehmers ist visuell identifizierbar.
- Die Audioaktivität eines Teilnehmers ist programmatisch ermittelbar.

**Beispiele für negative Bewertung**

- Die Audioaktivität eines Teilnehmers ist visuell nicht identifizierbar.
- Die Audioaktivität eines Teilnehmers ist programmatisch nicht ermittelbar.
- Die Audioaktivität eines Teilnehmers ist visuell nur manchmal, und nicht bei jeder Audioaktivität identifizierbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#anzeige-von-audioaktivität-in-echtzeit) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt.

**Anforderung**

Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator muss gleichermaßen programmatisch ermittelbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#anzeige-von-audioaktivität-in-echtzeit) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator muss gleichermaßen programmatisch ermittelbar sein.

<a id="k-6-2-3"></a>

#### 6.2.3 Interoperabilität

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#interoperabilität) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#interoperabilität) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Verwendung technischer Standards stellt soweit möglich eine gleichbleibende und verlässliche Funktionalität auch anwendungsübergreifend sicher. Mit Hilfe derartiger Standards sollen Menschen bei der Wahl der Anwendung nicht unnötig eingeschränkt werden.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für negative Bewertung**

- Ein oder mehrere der verwendeten Standards entsprechen nicht den Empfehlungen der EN 301549.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#interoperabilität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt, und wenn die Software in der Lage ist, mit einer anderen Software des gleichen Funktionsumfangs zu kommunizieren.

**Anforderung**

Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), und unterstützt sie zudem die Nutzung dieser Funktionen in Kombination mit anderer Software desselben Funktionsumfangs, so muss die Interoperabilität auf Basis der relevanten, technischen Standards unterstützt werden.

Die EN 301549 benennt dabei folgende Standards:

- Kommunikation über das öffentliche Telefonnetz: ITU-T V.18 [i.23] oder dessen Text-Telefonie-Standards in den Anhängen werden verwendet
- Voice over IP (VoIP) über das Session Initiation Protocol (SIP): Empfehlungen der IETF zu Textkommunikation in Echtzeit (RTT = Real Time Text) RFC 4103 [i.13] werden verwendet
- Voice over IP (VoIP) über das IP Multimedia Sub-System (IMS): Protokolle ETSI TS 126 114 [i.10], ETSI TS 122 173 [i.11] und ETSI TS 134 229 [i.12] werden verwendet
- Andere Kommunikationsmethoden: Gemeinsame, relevante und veröffentlichte Standards werden verwendet

**Beispiele für positive Bewertung**

- In der Dokumentation sind die Empfehlungen der EN 301549 als verwendete Standards benannt.
- Nach Kontaktaufnahme mit dem Anbieter der Software werden die Empfehlungen der EN 301549 als verwendete Standards bestätigt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#interoperabilität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Zwei-Wege-Sprachkommunikation und Textkommunikation in Echtzeit (RTT) zur Verfügung stellt, und wenn die Mobile App in der Lage ist, mit einer anderen Mobile App des gleichen Funktionsumfangs zu kommunizieren.

**Anforderung**

Bietet die Mobile App Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), und unterstützt sie zudem die Nutzung dieser Funktionen in Kombination mit anderen Mobile Apps desselben Funktionsumfangs, so muss die Interoperabilität auf Basis der relevanten, technischen Standards unterstützt werden.

Die EN 301549 benennt dabei folgende Standards:

- Kommunikation über das öffentliche Telefonnetz: ITU-T V.18 [i.23] oder dessen Text-Telefonie-Standards in den Anhängen werden verwendet
- Voice over IP (VoIP) über das Session Initiation Protocol (SIP): Empfehlungen der IETF zu Textkommunikation in Echtzeit (RTT = Real Time Text) RFC 4103 [i.13] werden verwendet
- Voice over IP (VoIP) über das IP Multimedia Sub-System (IMS): Protokolle ETSI TS 126 114 [i.10], ETSI TS 122 173 [i.11] und ETSI TS 134 229 [i.12] werden verwendet
- Andere Kommunikationsmethoden: Gemeinsame, relevante und veröffentlichte Standards werden verwendet

**Beispiele für positive Bewertung**

- In der Dokumentation sind die Empfehlungen der EN 301549 als verwendete Standards benannt.
- Nach Kontaktaufnahme mit dem Anbieter der Mobile App werden die Empfehlungen der EN 301549 als verwendete Standards bestätigt.

<a id="k-6-2-4"></a>

#### 6.2.4 Reaktionsgeschwindigkeit von RTT

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Echtzeittext (RTT) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#reaktionsgeschwindigkeit-von-rtt) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#reaktionsgeschwindigkeit-von-rtt) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die über Echtzeit-Textkommunikation (RTT = Realtime Text) und nicht über Sprache an einer Online-Kommunikation teilnehmen, dürfen gegenüber Sprachnutzenden nicht benachteiligt werden. Die Texteingaben sollen gleichwertig zu Sprache unmittelbar bzw. nur mit geringer Latenz für andere Teilnehmer in der Konferenz angezeigt werden.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Texteingaben werden in Echtzeit bzw. mit nicht merkbarer Latenz empfangen.

**Beispiele für negative Bewertung**

- Die Texteingaben werden nicht empfangen.
- Die Texteingaben werden mit merkbarer Latenz (> 1000ms) empfangen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#reaktionsgeschwindigkeit-von-rtt) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt.

**Anforderung**

Bietet die Software Echtzeit-Textkommunikation (RTT = Realtime Text), so darf die Dauer der Übertragung von einem Endgerät auf ein anderes Endgerät für die kleinste Texteinheit (einzelne Zeichen, kurze Worte) maximal 1000 Millisekunden (= 1 Sekunde) betragen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#reaktionsgeschwindigkeit-von-rtt) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Echtzeit-Textkommunikation (RTT = Realtime Text) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Echtzeit-Textkommunikation (RTT = Realtime Text), so darf die Dauer der Übertragung von einem Endgerät auf ein anderes Endgerät für die kleinste Texteinheit (einzelne Zeichen, kurze Worte) maximal 1000 Millisekunden (= 1 Sekunde) betragen.

<a id="k-6-3"></a>

#### 6.3 Anrufer-Identifizierung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#anrufer-identifizierung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#anrufer-identifizierung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Über die Anruferkennung wird sichergestellt, dass sowohl visuelle Menschen als auch Nutzende von assistiven Systemen einen Anrufer eindeutig und schnell identifizieren können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der Anrufer wird visuell und programmatisch problemlos identifiziert.

**Beispiele für negative Bewertung**

- Der Anrufer ist nur auf einem der geforderten Wege identifizierbar.
- Der Anrufer ist nur mit Zwischenschritten programmatisch identifizierbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#anrufer-identifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Sprach- oder Videotelefonie mit Anrufer-Identifikation zur Verfügung stellt.

**Anforderung**

Bietet die Software Sprach- oder Videotelefonie mit Anrufer-Identifikation, so muss die Anrufer-Identifikation visuell in Textform, und programmatisch identifizierbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#anrufer-identifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Sprach- oder Videotelefonie mit Anrufer-Identifikation zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Sprach- oder Videotelefonie mit Anrufer-Identifikation, so muss die Anrufer-Identifikation visuell in Textform, und programmatisch identifizierbar sein.

<a id="k-6-4"></a>

#### 6.4 Alternativen zu sprachbasierten Diensten

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#alternativen-zu-sprachbasierten-diensten) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#alternativen-zu-sprachbasierten-diensten) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen mit Beeinträchtigungen des Hörvermögens und des Sprechens können sprachbasierte Funktionen nicht nutzen. Es muss eine zugängliche Alternative bereitstehen, in der Zugang zu Informationen und Bedienung der Konfigurationsmöglichkeiten ohne Sprech- oder Hörvermögen möglich ist.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Alle Konfigurationsmöglichkeiten, die sprachbasiert zur Verfügung stehen, lassen sich visuell über Bedien-Menüs oder Formularsteuerung bedienen.
- Konfigurationsmöglichkeiten, die sprachbasiert zur Verfügung stehen, lassen sich visuell über Bedien-Menüs oder Formularsteuerung bedienen, jedoch ist der Funktionsumfang geringfügig eingeschränkt.

**Beispiele für negative Bewertung**

- Konfigurationsmöglichkeiten, die sprachbasiert zur Verfügung stehen, lassen sich nicht visuell über Bedien-Menüs oder Formularsteuerung bedienen.
- Die Konfigurationsmöglichkeiten, die sprachbasiert zur Verfügung stehen, stehen visuell über Bedien-Menüs oder Formularsteuerung bedienbar nur in reduziertem Funktionsumfang zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#alternativen-zu-sprachbasierten-diensten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Sprach- oder Videotelefonie mit Voicemail-Funktion zur Verfügung stellt.

**Anforderung**

Bietet die Software Sprach- oder Videotelefonie mit Voicemail-Funktion, so muss die Nutzung dieser Dienste, die in aller Regel sprachbasiert gesteuert werden, auch ohne Sprech- oder Hörvermögen nutzbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#alternativen-zu-sprachbasierten-diensten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Sprach- oder Videotelefonie mit Voicemail-Funktion zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Sprach- oder Videotelefonie mit Voicemail-Funktion, so muss die Nutzung dieser Dienste, die in aller Regel sprachbasiert gesteuert werden, auch ohne Sprech- oder Hörvermögen nutzbar sein.

<a id="k-6-5-2"></a>

#### 6.5.2 Auflösung bei Videotelefonie

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#auflösung-bei-videotelefonie) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#auflösung-bei-videotelefonie) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Hochwertige Bildqualität bei Videotelefonie ermöglicht es manchen Beeinträchtigungsgruppen, Informationen wie Mimik oder Gestik wahrzunehmen, die bei reiner Sprach- oder Textkommunikation im Kontext fehlen. Zudem können Menschen mit Beeinträchtigung des Gehörs zusätzlich von den Lippen lesen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Auflösung ist in der Dokumentation enthalten und die Qualität eines Testanruf erscheint plausibel.
- Die Auflösung ist nicht in der Dokumentation enthalten, jedoch wirkt die Qualität eines Testanrufs subjektiv hochwertig.

**Beispiele für negative Bewertung**

- Die Auflösung ist in der Dokumentation enthalten, jedoch erscheint die Qualität eines Testanrufs nicht plausibel.
- Die Auflösung ist nicht in der Dokumentation enthalten und die Qualität eines Testanrufs wirkt subjektiv minderwertig.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#auflösung-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videotelefonie zur Verfügung stellt.

**Anforderung**

Software, die Videotelefonie zur Verfügung stellen, sollen mindestens die QVGA-Auflösung bei 320x240 Pixel, vorzugsweise die VGA-Auflösung bei 640x480 Pixel für die Videoübertragung unterstützen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#auflösung-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videotelefonie zur Verfügung stellt.

**Anforderung**

Mobile Apps, die Videotelefonie zur Verfügung stellen, sollen mindestens die QVGA-Auflösung bei 320x240 Pixel, vorzugsweise die VGA-Auflösung bei 640x480 Pixel für die Videoübertragung unterstützen.

<a id="k-6-5-3"></a>

#### 6.5.3 Bildwiederholfrequenz bei Videotelefonie

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#bildwiederholfrequenz-bei-videotelefonie) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#bildwiederholfrequenz-bei-videotelefonie) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Hochwertige Bildqualität bei Videotelefonie ermöglicht es manchen Beeinträchtigungsgruppen, Informationen wie Mimik oder Gestik wahrzunehmen, die bei reiner Sprach- oder Textkommunikation im Kontext fehlen. Zudem können Menschen mit Beeinträchtigung des Gehörs zusätzlich von den Lippen lesen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Bildwiederholfrequenz (frame rate) ist in der Dokumentation enthalten und die Qualität eines Testanruf erscheint plausibel.
- Die Bildwiederholfrequenz (frame rate) ist nicht in der Dokumentation enthalten, jedoch wirkt die Qualität eines Testanrufs subjektiv hochwertig.

**Beispiele für negative Bewertung**

- Die Bildwiederholfrequenz (frame rate) ist in der Dokumentation enthalten, jedoch erscheint die Qualität eines Testanrufs nicht plausibel.
- Die Bildwiederholfrequenz (frame rate) ist nicht in der Dokumentation enthalten und die Qualität eines Testanrufs wirkt subjektiv minderwertig.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#bildwiederholfrequenz-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videotelefonie zur Verfügung stellt.

**Anforderung**

Software, die Videotelefonie zur Verfügung stellen, sollen eine Bildwiederholfrequenz (frame rate) von mindestens 20 Bildern pro Sekunde, vorzugsweise 30 Bildern pro Sekunde für die Videoübertragung unterstützen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#bildwiederholfrequenz-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videotelefonie zur Verfügung stellt.

**Anforderung**

Mobile Apps, die Videotelefonie zur Verfügung stellen, sollen eine Bildwiederholfrequenz (frame rate) von mindestens 20 Bildern pro Sekunde, vorzugsweise 30 Bildern pro Sekunde für die Videoübertragung unterstützen.

<a id="k-6-5-4"></a>

#### 6.5.4 Synchronisation von Audio und Video bei Videotelefonie

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#synchronisation-von-audio-und-video-bei-videotelefonie) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#synchronisation-von-audio-und-video-bei-videotelefonie) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Zur optimalen Verständlichkeit von Videotelefonie ist Lippensynchronität essenziell. Nur so können Informationen im Kontext wie Gestik und Mimik mit den gesprochenen Inhalten erfasst werden. Menschen mit kognitiven Beeinträchtigungen und Beeinträchtigungen des Hörvermögens sind auf Synchronität von Bewegung und Sprache angewiesen, und innerhalb einer Kommunikation alle Informationen verstehen zu können. Untersuchungen haben zudem gezeigt, dass Video nach Audio deutlich schwerer zu verstehen ist als umgekehrt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Informationen zur Latenz sind in der Dokumentation enthalten und die Qualität eines Testanruf erscheint plausibel.
- Die Informationen zur Latenz sind nicht in der Dokumentation enthalten, jedoch wirkt die Qualität eines Testanrufs subjektiv hochwertig.

**Beispiele für negative Bewertung**

- Die Informationen zur Latenz sind in der Dokumentation enthalten, jedoch erscheint die Qualität eines Testanrufs nicht plausibel, da Audio und Video asynchron sind.
- Die Informationen zur Latenz sind nicht in der Dokumentation enthalten und die Qualität eines Testanrufs wirkt subjektiv minderwertig, da Audio und Video asynchron sind.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#synchronisation-von-audio-und-video-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videotelefonie zur Verfügung stellt.

**Anforderung**

Software, die Videotelefonie zur Verfügung stellen, sollen sicherstellen, dass die Latenz zwischen Audio- und Videowiedergabe nicht mehr als 100 Millisekunden beträgt und somit eine hochwertige Lippensynchronität besteht.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#synchronisation-von-audio-und-video-bei-videotelefonie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videotelefonie zur Verfügung stellt.

**Anforderung**

Mobile Apps, die Videotelefonie zur Verfügung stellen, sollen sicherstellen, dass die Latenz zwischen Audio- und Videowiedergabe nicht mehr als 100 Millisekunden beträgt und somit eine hochwertige Lippensynchronität besteht.

<a id="k-6-5-5"></a>

#### 6.5.5 Visuelle Anzeige von Audio-Aktivität

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#visuelle-anzeige-von-audio-aktivität) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#visuelle-anzeige-von-audio-aktivität) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Für Teilnehmer in einer Kommunikation ist die Identifikation von Audioaktivität in Verbindung mit dem entsprechenden Teilnehmer essenziell, um im Kontext die jeweilige Aktivität dem Teilnehmer zuordnen zu können.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Audioaktivität eines Teilnehmers ist visuell identifizierbar.

**Beispiele für negative Bewertung**

- Die Audioaktivität eines Teilnehmers ist visuell nicht identifizierbar.
- Die Audioaktivität eines Teilnehmers ist visuell nur manchmal, und nicht bei jeder Audioaktivität identifizierbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#visuelle-anzeige-von-audio-aktivität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Zwei-Wege-Sprachkommunikation und Videotelefonie zur Verfügung stellt.

**Anforderung**

Bietet die Software Zwei-Wege-Sprachkommunikation mittels Videotelefonie, so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator der Audio-Aktivität kann ein visueller Punkt, eine Rahmenmarkierung des Nutzenden oder eine andere Art von Ein- / Aus-Anzeige sein, die flackert.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#visuelle-anzeige-von-audio-aktivität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Zwei-Wege-Sprachkommunikation und Videotelefonie zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Zwei-Wege-Sprachkommunikation mittels Videotelefonie, so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator der Audio-Aktivität kann ein visueller Punkt, eine Rahmenmarkierung des Nutzenden oder eine andere Art von Ein- / Aus-Anzeige sein, die flackert.

<a id="k-6-5-6"></a>

#### 6.5.6 Sprecheridentifizierung bei Gebärdensprachen-Kommunikation

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Zweiwege-Sprachkommunikation |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sprecheridentifizierung-bei-gebärdensprachen-kommunikation) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sprecheridentifizierung-bei-gebärdensprachen-kommunikation) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Sprecheridentifizierung für gebärdende Teilnehmende ermöglicht Menschen mit Beeinträchtigung des Gehörs die Wahrnehmung des entsprechenden Teilnehmers.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Eingabe von Gebärden wird automatisiert erkannt und aktiviert die Sprecheridentifizierung.
- Eine manuelle Funktion aktiviert die Sprecheridentifizierung.

**Beispiele für negative Bewertung**

- Eine Funktion zum automatischen Erkennen von Gebärdeneingaben steht zur Verfügung, aktiviert aber die Sprecheridentifizierung nicht oder mit Verspätung.
- Es steht keine manuelle Funktion zum Aktivieren der Sprecheridentifizierung zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sprecheridentifizierung-bei-gebärdensprachen-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Sprecheridentifizierung für Sprachnutzende zur Verfügung stellt.

**Anforderung**

Bietet die Software Sprecheridentifizierung für sprechende Teilnehmende, sollen auch gebärdende Teilnehmende und Echtzeit-Gebärden gleichwertig identifiziert werden, sobald die Eingabe von Gebärden gestartet wurde. Das Sprecheridentifizierung für Gebärdensprache kann manuell aktiviert oder automatisch identifiziert werden:

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sprecheridentifizierung-bei-gebärdensprachen-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Sprecheridentifizierung für Sprachnutzende zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Sprecheridentifizierung für sprechende Teilnehmende, sollen auch gebärdende Teilnehmende und Echtzeit-Gebärden gleichwertig identifiziert werden, sobald die Eingabe von Gebärden gestartet wurde. Das Sprecheridentifizierung für Gebärdensprache kann manuell aktiviert oder automatisch identifiziert werden:

### Kapitel: Videofähigkeiten

<a id="k-7-1-1"></a>

#### 7.1.1 Wiedergabe von Untertiteln

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#wiedergabe-von-untertiteln) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#wiedergabe-von-untertiteln) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen mit Hörbeeinträchtigung sind auf Untertitel angewiesen, um den Inhalt von Videos mit Tonspur zu verstehen. Gleichzeitig können Untertitel beispielsweise kognitiv beeinträchtigte Menschen beim Verstehen der Inhalte stören. Zuschaltbare Untertitel (closed captions) müssen somit durch den Videoplayer ein- und ausschaltbar sein, um den Bedürfnissen der Nutzenden gerecht zu werden.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der Videoplayer stellt ein Bedienelement zum Ein- und Ausblenden der Untertitel zur Verfügung.
- Der Videoplayer stellt ein Bedienelement zum Ein- und Ausblenden der Untertitel, und eine Ausgabemöglichkeit für Braille-Zeilen zur Verfügung, oder leitet die Ausgabe an Braille-Zeilen automatisch weiter.

**Beispiele für negative Bewertung**

- Der Videoplayer stellt ein Bedienelement zum Ein- und Ausblenden der Untertitel zur Verfügung.
- Der Videoplayer stellt ein Bedienelement zur Ausgabe an Braille-Zeilen zur Verfügung, oder leitet die Ausgabe automatisch weiter, stellt jedoch kein Bedienelement zum Ein- und Ausblenden der Untertitel zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#wiedergabe-von-untertiteln) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software aufgenommene Videoinhalte mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln zur Verfügung stellt.

**Anforderung**

Bietet die Software aufgenommene Videoinhalte mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln an, benötigt der Videoplayer die Möglichkeit, die Untertitel über ein Bedienelement ein- und auszublenden (closed captions). Eine Funktion zur Ausgabe der Untertitel auf Braille-Zeilen ist notwendig.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#wiedergabe-von-untertiteln) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App aufgenommene Videoinhalte mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App aufgenommene Videoinhalte mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln an, benötigt der Videoplayer die Möglichkeit, die Untertitel über ein Bedienelement ein- und auszublenden (closed captions). Eine Funktion zur Ausgabe der Untertitel auf Braille-Zeilen ist notwendig.

<a id="k-7-1-2"></a>

#### 7.1.2 Synchronisation der Untertitel

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#synchronisation-der-untertitel) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#synchronisation-der-untertitel) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Untertitel sollen synchron zur Bewegung der Lippen eines Sprechers angezeigt werden, da hörbeeinträchtigte Menschen neben der Untertitelung auch auf Mundbewegungen achten. Es wichtig, dass die Untertitelung klar dem Gesprochenen zuzuordnen ist. Weiterhin fördert die Synchronität die Gesamtverständlichkeit eines Videos.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Untertitel sind synchron zur Tonspur.

**Beispiele für negative Bewertung**

- Die Untertitel sind nicht synchron zur Tonspur.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#synchronisation-der-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Nicht-Live-Videos mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln zur Verfügung stellt.

**Anforderung**

Bietet die Software Nicht-Live-Videos mit in Verbindung mit Untertiteln an, müssen Untertitel und Tonspur synchron wiedergegeben werden.

Die EN 301549 konkretisiert dabei die zwei folgenden Anforderungen:

- Die Anzeige des Untertitels weicht nicht mehr als 100ms vom Zeitstempel des Untertitels ab.
- Die Anforderung besteht gleichermaßen für Live-Untertitel. Es ist zu erwarten, dass bei Live-Untertiteln mit Verzögerungen durch die Konvertierung von gesprochener Sprache in Text zu rechnen ist.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#synchronisation-der-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Nicht-Live-Videos mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Nicht-Live-Videos mit in Verbindung mit Untertiteln an, müssen Untertitel und Tonspur synchron wiedergegeben werden.

Die EN 301549 konkretisiert dabei die zwei folgenden Anforderungen:

- Die Anzeige des Untertitels weicht nicht mehr als 100ms vom Zeitstempel des Untertitels ab.
- Die Anforderung besteht gleichermaßen für Live-Untertitel. Es ist zu erwarten, dass bei Live-Untertiteln mit Verzögerungen durch die Konvertierung von gesprochener Sprache in Text zu rechnen ist.

<a id="k-7-1-3"></a>

#### 7.1.3 Erhaltung von Untertiteln

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#erhaltung-von-untertiteln) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#erhaltung-von-untertiteln) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Barrierefreiheitsinformationen, die in Quelldateien bereits vorhanden sind, sollen auch in neu generierten Dateien vorhanden und korrekt übernommen werden. So wird sichergestellt, dass Menschen, die auf die jeweilige Unterstützung angewiesen sind, das Angebot entsprechend weiter nutzen können.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Untertitel und Darstellung sind in der generierten Datei analog zur Quelldatei enthalten.
- Untertitel und Darstellung sind in der generieten Datei enthalten, jedoch hat sich die Darstellung geringfügig verändert (beispielsweise wurde die Schriftart ersetzt).

**Beispiele für negative Bewertung**

- Untertitel oder Darstellung sind in der generierten Datei nicht mehr enthalten.
- Die Farbe der Untertitel wurde so verändert, dass die Untertitel nicht mehr lesbar sind.
- Die Darstellung hat sich in der generierten Datei in mehr als einem Aspekt verändert.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#erhaltung-von-untertiteln) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Videos mit Untertiteln der Software auf das Endgerät gespeichert, konvertiert oder aufgenommen werden können.

**Anforderung**

Bietet die Software die Möglichkeit, Videos mit Untertiteln zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Untertitel und die Synchronität zum Ton erhalten bleiben. Die Prüfschritte 7.1.1 Wiedergabe von Untertiteln und 7.1.2 Synchronisation der Untertitel benötigen den Status „Erfüllt“. Zusätzliche Darstellungsaspekte des Textes, wie beispielsweise die Position, die Textfarben, der Textstil und die Schriftarten, können je nach regionalen Konventionen eine Bedeutung vermitteln. Eine Änderung dieser Darstellungsaspekte könnte die Bedeutung verändern und sollte nach Möglichkeit vermieden werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#erhaltung-von-untertiteln) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Videos mit Untertiteln in der Mobile App aufgenommen, konvertiert oder auf das Endgerät heruntergeladen werden können.

**Anforderung**

Bietet die Mobile App die Möglichkeit, Videos mit Untertiteln zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Untertitel und die Synchronität zum Ton erhalten bleiben. Die Prüfschritte 7.1.1 Wiedergabe von Untertiteln und 7.1.2 Synchronisation der Untertitel benötigen den Status „Erfüllt“. Zusätzliche Darstellungsaspekte des Textes, wie beispielsweise die Position, die Textfarben, der Textstil und die Schriftarten, können je nach regionalen Konventionen eine Bedeutung vermitteln. Eine Änderung dieser Darstellungsaspekte könnte die Bedeutung verändern und sollte nach Möglichkeit vermieden werden.

<a id="k-7-1-4"></a>

#### 7.1.4 Anpassungen für Untertitel

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#anpassungen-für-untertitel) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#anpassungen-für-untertitel) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die flexible Anpassung der Darstellung von Untertiteln, beziehungsweise das Übernehmen von entsprechenden Benutzerpräferenzen stellt eine wichtige Funktion für alle Nutzergruppen dar, da auf diese Weise die Darstellung vom Nutzenden selbst an die eigenen Bedürfnisse gezielt angepasst werden kann.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Darstellung der Untertitel ist für mindestens eines der genannten Beispiele konfigurierbar.
- Der Videoplayer bietet selbst keine Konfigurationsmöglichkeiten zur Darstellung der Untertitel, die Untertitel reagieren jedoch auf entsprechende Benutzerpräferenzen.

**Beispiele für negative Bewertung**

- Es sind keine Konfigurationsmöglichkeiten zur Darstellung der Untertitel vorhanden, und die Untertitel reagieren nicht auf entsprechende Benutzerpräferenzen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#anpassungen-für-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit ein- und ausblendbaren Untertiteln (closed captions) zur Verfügung stellt.

**Anforderung**

Bietet die Software Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, muss die Darstellung der Untertitel individuell konfiguriert werden können. Die EN 301 549 definiert weder Art noch Umfang der Möglichkeiten, sondern benennt folgende Beispiele: Hintergrund- und Vordergrundfarbe, Schriftart, Kontur oder Ränder der Schriftarten und Deckkraft des Hintergrunds. Somit gilt der Prüfschritt als erfüllt, wenn wenigstens eine der Möglichkeiten konfigurierbar ist. Der Prüfschritt gilt auch dann als erfüllt, wenn die Untertitel systemseitige Benutzerpräferenzen wie Schriftgröße oder Farbe übernehmen. Bei dauerhaft angezeigten Untertiteln (open captions) ist der Prüfschritt nicht anwendbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#anpassungen-für-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videos mit synchroner Bild- und Tonspur in Verbindung mit ein- und ausblendbaren Untertiteln (closed captions) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, muss die Darstellung der Untertitel individuell konfiguriert werden können. Die EN 301 549 definiert weder Art noch Umfang der Möglichkeiten, sondern benennt folgende Beispiele: Hintergrund- und Vordergrundfarbe, Schriftart, Kontur oder Ränder der Schriftarten und Deckkraft des Hintergrunds. Somit gilt der Prüfschritt als erfüllt, wenn wenigstens eine der Möglichkeiten konfigurierbar ist. Der Prüfschritt gilt auch dann als erfüllt, wenn die Untertitel systemseitige Benutzerpräferenzen wie Schriftgröße oder Farbe übernehmen. Bei dauerhaft angezeigten Untertiteln (open captions) ist der Prüfschritt nicht anwendbar.

<a id="k-7-1-5"></a>

#### 7.1.5 Gesprochene Untertitel

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#gesprochene-untertitel) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#gesprochene-untertitel) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen, die den Originalton eines Videos nicht verstehen, benötigen eine für ihre Bedürfnisse passende Untertitelung. Diese Untertitel sollen auch für sehbeeinträchtigte Menschen als akustische Ausgabe zugänglich sein.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#gesprochene-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit ein- und ausblendbaren Untertiteln (closed captions) zur Verfügung stellt.

**Anforderung**

Bietet die Software Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, müssen diese Untertitel als Ton ausgegeben werden können. Die Originaltonspur soll ersetzt oder zumindest stark abgedimmt werden. Untertitel, die fester Bestandteil des Videos sind (open captions), sind von dieser Anforderung ausgenommen.

**Beispiele für positive Bewertung**

- Die Software stellt Tonspuren bzw. Sprachausgaben für die Untertitel zur Verfügung.
- Die im Video gesprochenen Inhalte entsprechen genau den Untertiteln.

**Beispiele für negative Bewertung**

- Die Software stellt keine Tonspuren bzw. Sprachausgaben für die Untertitel zur Verfügung.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#gesprochene-untertitel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videos mit synchroner Bild- und Tonspur in Verbindung mit ein- und ausblendbaren Untertiteln (closed captions) zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, müssen diese Untertitel als Ton ausgegeben werden können. Die Originaltonspur soll ersetzt oder zumindest stark abgedimmt werden. Untertitel, die fester Bestandteil des Videos sind (open captions), sind von dieser Anforderung ausgenommen.

**Beispiele für positive Bewertung**

- Die Mobile App stellt Tonspuren bzw. Sprachausgaben für die Untertitel zur Verfügung.
- Die im Video gesprochenen Inhalte entsprechen genau den Untertiteln.

**Beispiele für negative Bewertung**

- Die Mobile App stellt keine Tonspuren bzw. Sprachausgaben für die Untertitel zur Verfügung.

<a id="k-7-2-1"></a>

#### 7.2.1 Wiedergabe der Audiodeskription

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#wiedergabe-der-audiodeskription) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#wiedergabe-der-audiodeskription) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Werden Audiodeskriptionen für Videos zur Verfügung gestellt, müssen die Audiodeskriptionen entsprechend aktivierbar und deaktivierbar sein.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Das Video enthält die Audiodeskription als eigene Tonspur.
- Der Videoplayer bietet eine Funktionalität zum Aktivieren und Deaktivieren der Audiodeskription.
- Es steht ein separates Video zur Verfügung, das als Tonspur die Audiodeskription enthält.

**Beispiele für negative Bewertung**

- Es existiert keine Möglichkeit, die Audiodeskription zu aktivieren, bzw. das Video mit Audiodeskription zu konsumieren.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#wiedergabe-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit einer Audiodeskription zur Verfügung stellt.

**Anforderung**

Bietet die Software eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription als eigene Tonspur auswählbar und aktivierbar sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#wiedergabe-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videos mit synchroner Bild- und Tonspur in Verbindung mit einer Audiodeskription zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription als eigene Tonspur auswählbar und aktivierbar sein.

<a id="k-7-2-2"></a>

#### 7.2.2 Synchronisation der Audiodeskription

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#synchronisation-der-audiodeskription) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#synchronisation-der-audiodeskription) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Unterstützende Bildbeschreibungen müssen synchron zur Handlung zur Verfügung gestellt werden. Dies erhöht die Verständlichkeit entsprechender Inhalte enorm.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Audiodeskription ist bei allen angebotenen Videos synchron zu Bild und Ton.

**Beispiele für negative Bewertung**

- Die Audiodeskription ist nur bei einigen angebotenen Videos synchron zu Bild und Ton.
- Die Audiodeskription ist bei allen angebotenen Videos nicht synchron zu Bild und Ton.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#synchronisation-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit einer Audiodeskription zur Verfügung stellt.

**Anforderung**

Bietet die Software eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription synchron zu Bild und Ton wiedergegeben werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#synchronisation-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videos mit synchroner Bild- und Tonspur in Verbindung mit einer Audiodeskription zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription synchron zu Bild und Ton wiedergegeben werden.

<a id="k-7-2-3"></a>

#### 7.2.3 Erhaltung der Audiodeskription

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#erhaltung-der-audiodeskription) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#erhaltung-der-audiodeskription) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Barrierefreiheitsinformationen, die in Quelldateien bereits vorhanden sind, sollen auch in neu generierten Dateien vorhanden und korrekt übernommen werden. So wird sichergestellt, dass Menschen, die auf die jeweilige Unterstützung angewiesen sind, das Angebot entsprechend weiter nutzen können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Audiodeskription und Synchronität sind in der generierten Datei analog zur Quelldatei enthalten.

**Beispiele für negative Bewertung**

- Audiodeskription ist in der generierten Datei analog zur Quelldatei enthalten, die Synchronität hat sich während des Vorgangs marginal verschlechtert.
- Audiodeskription ist in der generierten Datei nicht mehr enthalten.
- Synchronität hat sich in der generierten Datei verschlechtert.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#erhaltung-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Videos mit Audiodeskription der Software übertragen, konvertiert oder aufgenommen werden können.

**Anforderung**

Bietet die Software die Möglichkeit, Videos mit Audiodeskription zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Audiodeskription und die Synchronität zu Bild und Ton beim generierten Video erhalten bleiben.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#erhaltung-der-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Videos mit Audiodeskription in der Mobile App aufgenommen, konvertiert oder auf das Endgerät heruntergeladen werden können.

**Anforderung**

Bietet die Mobile App die Möglichkeit, Videos mit Audiodeskription zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Audiodeskription und die Synchronität zu Bild und Ton beim generierten Video erhalten bleiben.

<a id="k-7-3"></a>

#### 7.3 Bedienelemente für Untertitel und Audiodeskription

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#bedienelemente-für-untertitel-und-audiodeskription) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#bedienelemente-für-untertitel-und-audiodeskription) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Bedienelemente zum Steuern von Untertiteln und Audiodeskriptionen müssen für ihre jeweilige Zielgruppe so einfach wie möglich auffindbar und aktivierbar sein. Die wichtigste Ebene bei der Bedienung von Videos ist die Wiedergabesteuerung. Die Kombination beider Steuerungen erleichtert Menschen die Bedienung des Angebots.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und/oder Audiodeskription befinden sich auf gleicher Interaktionsebene wie die Wiedergabesteuerung.

**Beispiele für negative Bewertung**

- Es werden keine Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und/oder Audiodeskription zur Verfügung gestellt.
- Es werden Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und/oder Audiodeskription auf einer anderen Ebene als die Wiedergabesteuerung zur Verfügung gestellt.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#bedienelemente-für-untertitel-und-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Videos mit synchroner Bild- und Tonspur und mit ein- und ausblendbaren Untertiteln und/oder Audiodeskriptionen zur Verfügung stellt.

**Anforderung**

Bietet die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln und/oder Audiodeskriptionen an, müssen die Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und Audiodeskription auf derselben Interaktionsebene wie die Wiedergabesteuerung zur Verfügung gestellt werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#bedienelemente-für-untertitel-und-audiodeskription) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Videos mit synchroner Bild- und Tonspur und mit ein- und ausblendbaren Untertiteln und/oder Audiodeskriptionen zur Verfügung stellt.

**Anforderung**

Bietet die Mobile App Videos mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln und/oder Audiodeskriptionen an, müssen die Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und Audiodeskription auf derselben Interaktionsebene wie die Wiedergabesteuerung zur Verfügung gestellt werden.

### Kapitel: Wahrnehmbarkeit

<a id="k-11-1-1-1"></a>

#### 11.1.1.1 Nicht-Text-Inhalt

*Übergeordneter Prüfschritt. Die Inhalte stehen in den folgenden Unterschritten.*

<a id="k-11-1-1-1a"></a>

#### 11.1.1.1a Alternativtexte für informative Grafiken

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#a-alternativtexte-für-informative-grafiken) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#a-alternativtexte-für-informative-grafiken) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Medienalternativen helfen Menschen mit Sinnesbeeinträchtigungen, die visuelle oder auditive Inhalte nicht oder nur eingeschränkt wahrnehmen können. Text ist als Medienalternative vielfältig einsetzbar. So können beispielsweise Menschen, die taub-blind sind, den Text in Braille lesen. Darüber hinaus unterstützt Text die Möglichkeit, nach Medieninhalten zu suchen und Inhalte auf eine Vielzahl von Arten zu einem neuen Zweck zu verbinden.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Informative Grafiken und Objekte verfügen über einen zum Inhalt passenden Alternativtext.
- Der Alternativtext von Multimedia-Objekten wie Audio- oder Video liefert eine Beschreibung des Inhalts.

**Beispiele für negative Bewertung**

- Informative Grafiken und Objekte verfügen über keinen oder einen nicht zum Inhalt passenden Alternativtext.
- Der Alternativtext entsprechender Objekte ist zu lang.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#a-alternativtexte-für-informative-grafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software informative, nicht interaktive Grafiken oder Objekte enthält, die zudem keine Interaktion ermöglichen.

**Anforderung**

Alle informativen Grafiken und Objekte verfügen über eine Textalternative, die den Inhalt passend beschreibt. Die Software bleibt vollständig durch Alternativtexte nutzbar, auch wenn Grafiken oder Objekte nicht geladen werden oder vom Nutzenden aus technischen Gründen deaktiviert sind.

Alternativtexte ersetzen das Objekt, wenn es beispielsweise nicht angezeigt werden kann oder mittels Screenreader fokussiert wird. Sie sind möglichst kurz. Hierbei ist darauf zu achten, dass der Alternativtext keine Bildbeschreibung darstellt, sondern ausschließlich die Kerninformation vermittelt. Der Text soll in seiner Struktur möglichst simpel und zusammengefasst formuliert sein. Ausführliche Beschreibungen, beziehungsweise weiterführende Informationen werden im Kontext der Abbildung erläutert.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#a-alternativtexte-für-informative-grafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App informative, nicht interaktive Grafiken oder Objekte enthält, die zudem keine Interaktion ermöglichen.

**Anforderung**

Alle informativen Grafiken und Objekte verfügen über eine Textalternative, die den Inhalt passend beschreibt. Die Mobile App bleibt vollständig durch Alternativtexte nutzbar, auch wenn Grafiken oder Objekte nicht geladen werden oder vom Nutzenden aus technischen Gründen deaktiviert sind.

Alternativtexte ersetzen das Objekt, wenn es beispielsweise nicht angezeigt werden kann oder mittels Screenreader fokussiert wird. Sie sind möglichst kurz. Hierbei ist darauf zu achten, dass der Alternativtext keine Bildbeschreibung darstellt, sondern ausschließlich die Kerninformation vermittelt. Der Text soll in seiner Struktur möglichst simpel und zusammengefasst formuliert sein. Ausführliche Beschreibungen, beziehungsweise weiterführende Informationen werden im Kontext der Abbildung erläutert.

<a id="k-11-1-1-1b"></a>

#### 11.1.1.1b Alternativtexte für dekorative Grafiken

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#b-alternativtexte-für-dekorative-grafiken) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#b-alternativtexte-für-dekorative-grafiken) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Alle Grafiken und Objekte (Objekte ohne auslösbare Funktion, beispielsweise dekorative Bilder, Farbflächen, Abstandshalter oder Symbole mit sichtbarer Beschriftung ohne Funktion), die keinen informativen Mehrwert bieten, sind als dekorative Objekte gekennzeichnet. Die entsprechende Kennzeichnung als dekoratives Objekt wird an den Screenreader übermittelt. Der Screenreader ignoriert das Objekt und fokussiert es nicht.

**Warum wird das geprüft?**

Text ist als Medienalternative vielfältig einsetzbar. So können bspw. Menschen, die taub-blind sind, den Text in Braille lesen. Rein dekorative Grafiken und Bilder sind hierbei anders zu behandeln. Sofern sie einem rein dekorativen Zweck dienen, sollen diese Elemente so implementiert sein, dass sie von assistiven Technologien wie Screenreadern ignoriert werden.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Eine grafisch dargestellte Trennlinie wird vom Screenreader nicht erfasst, da sie als dekorativ gekennzeichnet wurde.
- Grafiken und Objekte ohne informativen Mehrwert sind mit aktivem Screenreader nicht erreichbar.

**Beispiele für negative Bewertung**

- Eine grafisch dargestellte Trennlinie wird vom Screenreader bei der Navigation erfasst, da ihr der Alternativtext „Trennlinie“ gegeben wurde.
- Grafiken und Objekte ohne informativen Mehrwert sind mit aktivem Screenreader erreichbar und übermitteln einen Alternativtext.
- Grafiken und Objekte ohne informativen Mehrwert sind mit aktivem Screenreader erreichbar, übermitteln aber keinen Alternativtext.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#b-alternativtexte-für-dekorative-grafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software dekorative, nicht informative Grafiken oder Objekte enthält, die zudem keine Interaktion ermöglichen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#b-alternativtexte-für-dekorative-grafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App dekorative, nicht informative Grafiken oder Objekte enthält, die zudem keine Interaktion ermöglichen.

<a id="k-11-1-1-1c"></a>

#### 11.1.1.1c Alternativen für CAPTCHAs

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#c-alternativen-für-captchas) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#c-alternativen-für-captchas) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Für bildbasierte oder Audio-CAPTCHAs sind Alternativtexte verfügbar, die den Zweck des Objekts beschreiben und über Alternativen zum jeweiligen CAPTCHA informieren. Es muss mindestens eine Alternative für grafische oder Audio-CAPTCHAs vorhanden sein. Das alternative CAPTCHA muss im unmittelbaren Kontext zur Verfügung stehen.

**Warum wird das geprüft?**

Sehbeeinträchtigte bzw. blinde Nutzende können bildbasierte CAPTCHAS unter Umständen nicht bedienen. Audiobasierte CAPTCHAS sind hingegen für Nutzende mit eingeschränktem oder fehlendem Hörvermögen nicht bedienbar. Entsprechende Alternativtexte schaffen Orientierung, CAPTCHA-Alternativen sorgen für entsprechende Nutzungsmöglichkeiten des Angebots.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der Alternativtext eines CAPTCHAs informiert über den Zweck des CAPTCHAs und über entsprechende Alternativen. Die Alternativen sind im unmittelbaren Kontext auffindbar.

**Beispiele für negative Bewertung**

- Der Alternativtext eines CAPTCHAs informiert über den Zweck des CAPTCHAs, jedoch nicht über entsprechende Alternativen.
- Der Alternativtext eines CAPTCHAs informiert nicht über den Zweck des CAPTCHAs.
- Der Alternativtext eines CAPTCHAs informiert über den Zweck des CAPTCHAs und über entsprechende Alternativen. Die Alternativen sind jedoch nicht im unmittelbaren Kontext auffindbar.
- Es existiert keine Alternative zu einem bildbasierten oder Audio-CAPTCHA.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#c-alternativen-für-captchas) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software CAPTCHA-Funktionalität einsetzt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#c-alternativen-für-captchas) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App CAPTCHA-Funktionalität einsetzt.

<a id="k-11-1-2-1"></a>

#### 11.1.2.1 Alternativen für aufgezeichnetes Audio und Videos ohne Ton

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Videoinhalte sind für sehbeeinträchtigte Menschen nicht zugänglich und benötigen eine äquivalente Medienalternative, etwa als Text- oder Audioinhalt. Audioinhalte sind für hörbeeinträchtigte Menschen nicht zugänglich und benötigen eine äquivalente Transkription. Darüber hinaus können Textalternativen vielen Nutzenden helfen, die aufgezeichneten Inhalte zu verstehen.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der Inhalt einer Audioaufnahme wird über eine Transkription, die im unmittelbaren Kontext der Audioaufnahme zu finden ist, vollumfänglich wiedergegeben.
- Der Inhalt eines Videos ohne Ton wird über eine Audioaufnahme, die im unmittelbaren Kontext des Videos zu finden ist, vollumfänglich wiedergegeben.
- Der Inhalt eines Videos ohne Ton wird über eine Transkription, die im unmittelbaren Kontext des Videos zu finden ist, vollumfänglich wiedergegeben.
- Die Medienalternative referenziert auf eine externe Quelle, ist aber im unmittelbaren Kontext zu finden.

**Beispiele für negative Bewertung**

- Für Audio- oder Videoinhalte steht keine Medienalternative zur Verfügung.
- Es existiert eine Medienalternative, sie kann aber nicht eindeutig dem Audio- oder Videoinhalt zugeordnet werden.
- Es existiert eine Medienalterative, jedoch ist sie nicht im unmittelbaren Kontext auffindbar oder aufrufbar.
- Es existiert eine Medienalternative, jedoch sind die Inhalte nicht vollumfänglich wiedergegeben.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software aufgezeichnete „Nur“-Audio- oder „Nur“-Video-Inhalte zur Verfügung stellt.

**Anforderung**

Aufgezeichnete Audioinhalte und Videos ohne Ton, die Informationen übermitteln, beziehungsweise einen informativen Mehrwert bieten, benötigen äquivalente Medienalternativen im unmittelbaren Kontext, es sei denn, sie stellen bereits eine Medienalternative für Textinhalte dar. Die Medienalternative kann direkt in der Software, oder in einer externen Quelle bereitgestellt werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App aufgezeichnete „Nur“-Audio- oder „Nur“-Video-Inhalte zur Verfügung stellt.

**Anforderung**

Aufgezeichnete Audioinhalte und Videos ohne Ton, die Informationen übermitteln, beziehungsweise einen informativen Mehrwert bieten, benötigen äquivalente Medienalternativen im unmittelbaren Kontext, es sei denn, sie stellen bereits eine Medienalternative für Textinhalte dar. Die Medienalternative kann direkt in der Mobile App, oder in einer externen Quelle bereitgestellt werden.

<a id="k-11-1-2-2"></a>

#### 11.1.2.2 Untertitel für aufgezeichnete Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen mit Hörbeeinträchtigung sind auf Untertitel angewiesen, um den Inhalt von Videos mit Tonspur zu verstehen. Darüber hinaus können Untertitel vielen Nutzenden helfen, die aufgezeichneten Inhalte zu verstehen.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für Videos mit informativer Tonspur werden standardmäßig korrekte Untertitel angezeigt.
- Für Videos mit informativer Tonspur können korrekte Untertitel aktiviert werden.
- Für Videos mit informativer Tonspur existieren keine Untertitel, im unmittelbaren Kontext ist jedoch eine korrekte Medienalternative verfügbar.

**Beispiele für negative Bewertung**

- Für Videos mit informativer Tonspur existieren weder Untertitel noch Medienalternativen.
- Für Videos mit informativer Tonspur existieren Untertitel oder Medienalternativen im unmittelbaren Kontext, die jedoch den Inhalt der Tonspur nicht adäquat wiedergeben.
- Es existiert eine Medienalterative, jedoch ist sie nicht im unmittelbaren Kontext auffindbar oder aufrufbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software aufgezeichnete Videos mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Software aufgezeichnete Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden, es sei denn, sie stellen bereits eine Medienalternative für andere Inhalt dar. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers etc.) bereitgestellt werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App aufgezeichnete Videos mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Mobile App aufgezeichnete Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden, es sei denn, sie stellen bereits eine Medienalternative für andere Inhalt dar. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers etc.) bereitgestellt werden.

<a id="k-11-1-2-3"></a>

#### 11.1.2.3 Audiodeskription oder Medienalternative für aufgezeichnete Videos (App-Test: „Medienalternative für aufgezeichnete Videos“)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#audiodeskription-oder-medienalternative-für-aufgezeichnete-videos) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#medienalternative-für-aufgezeichnete-videos) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Durch entsprechende Medienalternativen wird sichergestellt, dass Nutzende mit Seh- oder Hörbeeinträchtigung Inhalte von Videos vollumfänglich nutzen können.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#audiodeskription-oder-medienalternative-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software aufgezeichnete Videos mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Software aufgezeichnete Videos mit informativer Tonspur, deren visuelle Inhalte zusätzliche Informationen übermitteln (beispielsweise Informationen zu Aktionen, Charakteren, Szenenänderungen, Geräusche, Bildschirmtext), müssen diese Inhalte in einer Medienalternative als Audiodeskription (WCAG AA) wie in Prüfschritt 11.1.2.5 Audiodeskription für aufgezeichnete Videos beschrieben verfügbar sein. Wurde der Prüfschritt 11.1.2.5 Audiodeskription für aufgezeichnete Videos erfolgreich bewertet, ist keine weitere Prüfung von Medienalternativen notwendig, selbst wenn zusätzlich zur Audiodeskription weitere Alternativen angeboten werden.

Ist keine Audiodeskription verfügbar, muss mindestens eine Volltext-Alternative (WCAG A) bereitgestellt werden.

Bieten die visuellen Inhalte keinen weiteren Informationen, die zum Gesamtverständnis wichtig sind, oder stellt das Video bereits eine Medienalternative dar, oder ist das Video ein Gebärdensprachenvideo, ist keine der genannten Alternativen notwendig.

**Beispiele für positive Bewertung**

- Prüfschritt 11.1.2.5 Audiodeskription für aufgezeichnete Videos ist erfüllt.
- Die Volltext-Analyse ist vorhanden und beschreibt Handlung, Dialoge, Geräusche und weitere informationstragende oder stille Bildinhalte zutreffend und vollumfänglich.

**Beispiele für negative Bewertung**

- Handlung, Dialoge oder Geräusche sind fehlerhaft oder unvollständig beschrieben.
- Informationstragende Bildinhalte sind nicht, fehlerhaft oder unvollständig beschrieben.
- Es steht keine Medienalternative zur Verfügung.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#medienalternative-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App aufgezeichnete Videos mit informativem Inhalt bereitstellt.

**Anforderung**

Die von der Mobile App zur Verfügung gestellten Volltext-Alternativen für aufgezeichnete Videos mit informativem Inhalt müssen sämtliche Informationen, die visuell im Video enthalten sind, vollumfänglich und inhaltlich zutreffend beschreiben.

Volltext-Alternativen für aufgezeichnete Videos sind optional, daher gilt dieser Prüfschritt als erfüllt, wenn keine Volltext-Alternativen zur Verfügung gestellt werden.

**Beispiele für positive Bewertung**

- Die vorhandene Volltext-Alternative beschreibt Handlung, Dialoge, Geräusche und weitere informationstragende oder stille Bildinhalte zutreffend und vollumfänglich.
- Es steht keine Volltext-Alternative zur Verfügung.

**Beispiele für negative Bewertung**

- Handlung, Dialoge oder Geräusche sind fehlerhaft oder unvollständig beschrieben.
- Informationstragende Bildinhalte sind nicht, fehlerhaft oder unvollständig beschrieben.

<a id="k-11-1-2-4"></a>

#### 11.1.2.4 Untertitel für Live-Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#untertitel-für-live-videos) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#untertitel-für-live-videos) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen mit Hörbeeinträchtigung sind auf Untertitel angewiesen, um den Inhalt von Live-Videos mit Tonspur zu verstehen. Darüber hinaus können Untertitel vielen Nutzenden helfen, die aufgezeichneten Inhalte zu verstehen.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für Live-Videos mit informativer Tonspur werden standardmäßig korrekte Untertitel angezeigt.
- Für Live-Videos mit informativer Tonspur können korrekte Untertitel aktiviert werden.
- Für Live-Videos mit informativer Tonspur existieren keine Untertitel, im unmittelbaren Kontext ist jedoch eine korrekte Medienalternative verfügbar.

**Beispiele für negative Bewertung**

- Für Live-Videos mit informativer Tonspur existieren weder Untertitel noch Medienalternativen.
- Für Live-Videos mit informativer Tonspur existieren Untertitel oder Medienalternativen im unmittelbaren Kontext, die jedoch den Inhalt der Tonspur nicht adäquat wiedergeben.
- Es existiert eine Medienalterative, jedoch ist sie nicht im unmittelbaren Kontext auffindbar oder aufrufbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#untertitel-für-live-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software Live-Übertragungen mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Software Live-Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers, etc.) bereitgestellt werden.

Bei Liveübertragungen können die Untertitel aufgrund der technischen Machbarkeit mit leichter Latenz angezeigt werden. Die Bewertung liegt im Ermessensspielraum des Prüfers.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#untertitel-für-live-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Live-Übertragungen mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Mobile App Live-Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers, etc.) bereitgestellt werden.

Bei Liveübertragungen können die Untertitel aufgrund der technischen Machbarkeit mit leichter Latenz angezeigt werden. Die Bewertung liegt im Ermessensspielraum des Prüfers.

<a id="k-11-1-2-5"></a>

#### 11.1.2.5 Audiodeskription für aufgezeichnete Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Video |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Durch entsprechende Medienalternativen wird sichergestellt, dass Nutzende mit Sehbeeinträchtigung Inhalte von Videos vollumfänglich nutzen können. Darüber hinaus bieten Audiodeskriptionen auch Nutzenden mit kognitiven Beeinträchtigungen eine alternative Möglichkeit, auf diese Inhalte zuzugreifen.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für Videos, deren visuelle Inhalte zusätzliche Informationen übermitteln, steht eine Audiodeskription zur Verfügung, die die visuellen Inhalte zutreffend und vollumfänglich beschreibt.

**Beispiele für negative Bewertung**

- Für Videos, deren visuelle Inhalte zusätzliche Informationen übermitteln, steht keine Audiodeskription zur Verfügung.
- Für Videos, deren visuelle Inhalte zusätzliche Informationen übermitteln, steht eine Audiodeskription zur Verfügung, die jedoch die visuellen Inhalte fehlerhaft oder unvollständig beschreibt.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software aufgezeichnete Videos mit synchronisierter, informativer Tonspur enthält.

**Anforderung**

Bietet die Software aufgezeichnete Videos mit informativer Tonspur, deren visuelle Inhalte zusätzliche Informationen übermitteln (beispielsweise Informationen zu Aktionen, Charakteren, Szenenänderungen, Geräusche, Bildschirmtext), muss eine Audiodeskription als Medienalternative zur Verfügung stehen.

Bieten die visuellen Inhalte keinen weiteren Informationen, die zum Gesamtverständnis wichtig sind, oder stellt das Video bereits eine Medienalternative dar, oder ist das Video ein Gebärdensprachenvideo, ist keine der genannten Alternativen notwendig.

Die Audiodeskription kann auf verschiedenen Wegen angeboten werden:

- Die Audiodeskription ist in der Standard-Tonspur enthalten.
- Die Audiodeskription ist in einer separaten Tonspur aktivierbar.
- Die Audiodeskription ist als weitere Videoversion im unmittelbaren Kontext des Videos verfügbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App aufgezeichnete Videos mit informativem Inhalt bereitstellt.

**Anforderung**

Bietet die Mobile App aufgezeichnete Videos mit informativem Inhalt an, muss eine Audiodeskription zur Verfügung gestellt werden.

Die zur Verfügung gestellte Audiodeskription muss sämtliche Informationen, die visuell im Video enthalten sind, vollumfänglich und inhaltlich zutreffend beschreiben.

Die Audiodeskription kann auf verschiedenen Wegen angeboten werden:

- Die Audiodeskription ist in der Standard-Tonspur enthalten.
- Die Audiodeskription ist in einer separaten Tonspur aktivierbar.
- Die Audiodeskription ist als weitere Videoversion im unmittelbaren Kontext des Videos verfügbar.

Enthalten die visuellen Inhalte des Videos keine weiteren Informationen, die zum Gesamtverständnis wichtig sind (beispielsweise Informationen zu Aktionen, Charakteren, Szenenänderungen, Geräusche, Bildschirmtext), oder stellt das Video bereits eine Medienalternative dar, oder ist das Video ein Gebärdensprachenvideo, ist keine Audiodeskription notwendig.

<a id="k-11-1-3-1"></a>

#### 11.1.3.1 Info und Beziehungen

*Übergeordneter Prüfschritt. Die Inhalte stehen in den folgenden Unterschritten.*

<a id="k-11-1-3-1a"></a>

#### 11.1.3.1a Strukturelemente für Überschriften

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#a-strukturelemente-für-überschriften) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#a-strukturelemente-für-überschriften) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Überschriften strukturieren Inhalte für sehende Nutzende und schaffen so schnelle Orientierung. Inhalte sind einfach erfassbar und gezielt abrufbar. Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, müssen diese auch auf einer semantischen Ebene vermitteln. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können beispielsweise auch Nutzende von Screenreadern den Aufbau der Inhalte wahrnehmen und schnell durchsuchen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#a-strukturelemente-für-überschriften) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte der Software durch Überschriften strukturiert werden.

**Anforderung**

Überschriften und Hierarchieebenen werden durch geeignete Objekte visualisiert und repräsentiert. Diese visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.

Die Ausgabe von Überschriftsebenen ist unter Umständen abhängig vom verwendeten Framework technisch noch nicht vollumfänglich möglich, daher darf das Vorhandensein von Überschriftsebenen derzeit noch nicht gefordert werden. Falls Ebenen ausgegeben werden, müssen sie hierarchisch korrekt verschachtelt sein (beispielsweise H1 gefolgt von H2, gefolgt von H3, etc.). Das Auslassen von Überschriftsebenen wird nicht als Fehler gewertet, sofern die hierarchische Reihenfolge der Überschriften logisch ist.

Wortwahl oder Stil der Überschriften sind nicht Teil der Prüfung.

**Beispiele für positive Bewertung**

- Überschriften und Ebenen sind programmatisch ermittelbar.
- Überschriften sind programmatisch ermittelbar, jedoch werden keine Ebenen ausgegeben.
- Überschriften und Ebenen sind programmatisch ermittelbar, jedoch werden Ebenen innerhalb der Hierarchie übersprungen.

**Beispiele für negative Bewertung**

- Überschriften und Ebenen sind programmatisch nicht ermittelbar.
- Überschriften und Ebenen sind programmatisch ermittelbar, jedoch ist die Hierarchie der Ebenen unlogisch oder entspricht nicht der visuellen oder inhaltlichen Gliederung.
- Die Überschriften sind bei einzelner Fokussierung programmatisch ermittelbar, können aber mittels Screenreader nicht annavigiert werden (von Überschrift zu Überschrift springen).

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#a-strukturelemente-für-überschriften) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte der Mobile App durch Überschriften strukturiert werden.

**Anforderung**

Überschriften werden über geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.

Falls bei vorhandenen Überschriften Ebenen ausgegeben werden, müssen diese hierarchisch nachvollziehbar verschachtelt sein (beispielsweise Überschrift Ebene 1 gefolgt von Ebene 2, gefolgt von Ebene 3, etc.). Das Auslassen von Ebenen wird nicht als Fehler gewertet, sofern die hierarchische Reihenfolge der Überschriften logisch ist.

Das Vorhandensein von Ebenen kann derzeit nicht gefordert werden, da die Implementierung von Überschriftebenen unter Umständen, abhängig vom verwendeten Framework, technisch nicht vollumfänglich möglich ist.

Wortwahl oder Stil der Überschriften sind nicht Teil der Prüfung.

**Beispiele für positive Bewertung**

- Überschriften und vorhandene Ebenen sind programmatisch ermittelbar.
- Überschriften sind programmatisch ermittelbar, jedoch werden keine Ebenen ausgegeben.
- Überschriften und vorhandene Ebenen sind programmatisch ermittelbar, jedoch werden Ebenen innerhalb der Hierarchie übersprungen.

**Beispiele für negative Bewertung**

- Die Überschriften sind durch die Nutzung der Komfortfunktion des jeweiligen Screenreaders (VoiceOver-Rotor und TalkBack-Menü) nicht erreichbar und daher programmatisch nicht ermittelbar.
- Überschriften sind programmatisch ermittelbar, jedoch ist die Hierarchie der ausgegebenen Ebenen unlogisch oder entspricht nicht der visuellen Darstellung oder inhaltlichen Gliederung.

<a id="k-11-1-3-1b"></a>

#### 11.1.3.1b Strukturelemente für Listen, Auflistungen und Aufzählungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#b-strukturelemente-für-listen-auflistungen-und-aufzählungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#b-strukturelemente-für-listen-auflistungen-und-aufzählungen) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Strukturelemente für Listen strukturieren Inhalte für sehende Nutzende und schaffen so Orientierung. Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, müssen diese auch auf einer semantischen Ebene vermitteln. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können beispielsweise auch Nutzende von Screenreadern den Aufbau der Inhalte wahrnehmen und schnell navigieren.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#b-strukturelemente-für-listen-auflistungen-und-aufzählungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte der Software listenartig strukturiert werden.

**Anforderung**

Listen, Auflistungen, Aufzählungen und andere listenartige Inhalte werden über geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.

Listeneinträge sollen möglichst ein einzelnes, fokussierbares Objekt sein, um die Navigation zwischen Einträgen für Screenreadernutzende zu vereinfachen. Die im Eintrag gegebenenfalls vorhandenen Interaktionsmöglichkeiten sollen über die Steuerungsmöglichkeiten des Screenreaders (in der Regel mittels Wischnavigation nach oben und unten) erreichbar sein. Zusätzliche Informationen wie Anfang und Ende der Liste, Anzahl der Einträge und die Position eines einzelnen Eintrags innerhalb der Liste sollen ermittelbar sein.

Beispielhafte Umsetzung:

- Beim Fokussieren eines Listeneintrags gibt der Screenreader folgenden Inhalt aus: [Name des Listeneintrags]; Listeneintrag X von Y;
- Gibt es weitere Interaktionsmöglichkeiten mit Listeneinträgen, wie beispielsweise im Navigationsbereich oder in Formularen, werden die konkreten Rollen zusätzlich zur Position in der Liste ausgegeben.

Wortwahl oder Stil der Inhalte sind nicht Teil der Prüfung.

**Beispiele für positive Bewertung**

- Listenartige Elemente sind programmatisch ermittelbar.
- Listenartige Elemente sind programmatisch ermittelbar, jedoch werden die Einträge nicht ein einzelnes Objekt fokussiert.

**Beispiele für negative Bewertung**

- Listenartige Elemente sind programmatisch nicht ermittelbar.
- Listenartige Elemente sind programmatisch ermittelbar, jedoch werden keine der weiteren Informationen ausgegeben.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#b-strukturelemente-für-listen-auflistungen-und-aufzählungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte der Mobile App listenartig strukturiert werden.

**Anforderung**

Listen, Auflistungen, Aufzählungen und andere listenartige Inhalte werden über geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.

Listeneinträge sollen möglichst ein einzelnes, fokussierbares Objekt sein, um die Navigation zwischen Einträgen für Screenreadernutzende zu vereinfachen. Die im Eintrag gegebenenfalls vorhandenen Interaktionsmöglichkeiten sollen über die Steuerungsmöglichkeiten des Screenreaders (in der Regel mittels Wischnavigation nach oben und unten) erreichbar sein. Zusätzliche Informationen wie Anfang und Ende der Liste, Anzahl der Einträge und die Position eines einzelnen Eintrags innerhalb der Liste sollen ermittelbar sein.

Beispielhafte Umsetzung:

- Beim Fokussieren eines Listeneintrags gibt der Screenreader folgenden Inhalt aus: [Name des Listeneintrags]; Listeneintrag x von Y; zum Öffnen doppeltippen; zum Ändern der Aktion mit einem Finger nach oben oder unten streichen.
- Wischt der Nutzende nun beispielsweise nach oben, ändert sich die Interaktionsmöglichkeit und der Screenreader gibt folgenden Inhalt aus:
 [Name des Listeneintrags]; Listeneintrag x von Y; zum Auswählen doppeltippen; zum Ändern der Aktion mit einem Finger nach oben oder unten streichen.

Wortwahl oder Stil der Inhalte sind nicht Teil der Prüfung.

**Beispiele für positive Bewertung**

- Listenartige Elemente sind programmatisch ermittelbar.
- Listenartige Elemente sind programmatisch ermittelbar, jedoch werden die Einträge nicht ein einzelnes Objekt fokussiert.
- Listenartige Elemente sind programmatisch ermittelbar, es werden jedoch nicht alle weiteren Informationen (beispielsweise Anfang der Liste) ausgegeben.

**Beispiele für negative Bewertung**

- Listenartige Elemente sind programmatisch nicht ermittelbar.
- Listenartige Elemente sind programmatisch ermittelbar, jedoch werden keine der weiteren Informationen ausgegeben.
- Listenartige Elemente sind bei einzelner Fokussierung programmatisch ermittelbar, können aber mittels VoiceOver-Rotor oder Android TalkBack-Menü nicht annavigiert werden (von Liste zu Liste springen).

<a id="k-11-1-3-1c"></a>

#### 11.1.3.1c Strukturelemente für weitere Objekte (App-Test: „Strukturelemente für Bedien- und Formularelemente“)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#c-strukturelemente-für-weitere-objekte) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#c-strukturelemente-für-bedien--und-formularelemente) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#c-strukturelemente-für-weitere-objekte) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn für Inhalte der Software weitere Strukturelemente, die nicht explizit in den Prüfschritten 11.1.3.1a Strukturelemente für Überschriften und 11.1.3.1b Strukturelemente für Listen, Auflistungen und Aufzählungen genannt sind, vorhanden sind.

**Anforderung**

Die Gliederung und Strukturierung von Inhalten werden durch geeignete Objekte visualisiert. Diese visuell erkennbaren Objekte sind zudem programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar. Assistive Technologien wie beispielsweise Screenreader bieten umfangreiche Möglichkeiten, um durch Strukturobjekte zu navigieren, ohne den gesamten Inhalt durchlaufen zu müssen. Dazu gehören beispielsweise Container, Links, Formularelemente, Tabellen, Artikel, Tasten, Text- und Suchfelder oder Bilder. Wortwahl oder Stil der Objekte sind nicht Teil der Prüfung.

Tabellen dürfen nicht für Layout- oder Designzwecke verwendet werden. Falls tabellarischer Aufbau rein zu Layoutzwecken verwendet wird, soll die Tabelle nicht als Strukturelement gekennzeichnet sein.

Die Beschriftung von Formularelementen soll beim Erreichen programmatisch ermittelbar sein. Der Screenreader gibt dabei den Namen des jeweiligen Elements aus. Gruppen von Formularelementen sollen eine Gruppenbeschriftung beinhalten, um den Kontext der einzelnen Elemente besser erfassen zu können.

**Warum wird das geprüft?**

Strukturobjekte strukturieren Inhalte für sehende Nutzende und schaffen so Orientierung. Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, müssen diese auch auf einer semantischen Ebene vermitteln. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können beispielsweise auch Nutzende von Screenreadern den Aufbau der Inhalte wahrnehmen und schnell navigieren.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Alle vorhandenen Strukturobjekte können mittels Screenreader separat annavigiert werden, ohne den gesamten Inhalt bis zum Erreichen des Objekts durchlaufen zu müssen.
- Eine Gruppe von Formularelementen, die eine Eingabe einer Lieferadresse ermöglichen, haben eine Gruppenüberschrift „Lieferadresse“ (oder ähnlich).

**Beispiele für negative Bewertung**

- Keine oder nicht alle vorhandenen Strukturobjekte können mittels Screenreader separat annavigiert werden, ohne den gesamten Inhalt bis zum Erreichen des Objekts durchlaufen zu müssen.
- Nach Auswahl eines Strukturobjekts mittels Screenreader wird die Meldung ausgegeben, dass das gewählte Strukturobjekt nicht vorhanden sei.
- Das ausgewählte Strukturobjekt kann nicht vom Screenreader identifiziert werden.
- Der Screenreader kann über das entsprechende Strukturelement eine Tabelle annavigieren, die keine tabellarischen Informationen enthält.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#c-strukturelemente-für-bedien--und-formularelemente) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App Bedien- oder Formularelemente enthält.

**Anforderung**

Bedien- und Formularelemente werden über geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.

**Warum wird das geprüft?**

Korrekt implementierte Bedienelemente und Formularelemente tragen zu einer wesentlich effizienteren Orientierung und Bedienung bei. Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, müssen diese auch auf einer semantischen Ebene vermitteln. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können beispielsweise auch Nutzende von Screenreadern den Aufbau der Inhalte wahrnehmen und schnell navigieren.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Bedien- und Formularelemente sind über die Komfortfunktion des jeweiligen Screenreaders (VoiceOver-Rotor und TalkBack-Menü) erreichbar und somit programmatisch ermittelbar.

**Beispiele für negative Bewertung**

- Bedien- und Formularelemente sind durch die Nutzung der Komfortfunktion des jeweiligen Screenreaders (VoiceOver-Rotor und TalkBack-Menü) nicht erreichbar und daher programmatisch nicht ermittelbar.

<a id="k-11-1-3-1d"></a>

#### 11.1.3.1d Keine Strukturelemente für Layouttabellen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (vom übergeordneten Prüfschritt übernommen) |
| Prüfverfahren | BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#d-keine-strukturelemente-für-layouttabellen) |

##### BIT inklusiv App-Test (nur in diesem Verfahren)

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#d-keine-strukturelemente-für-layouttabellen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Tabellen dürfen nicht für Layout- oder Designzwecke verwendet werden. Falls tabellarischer Aufbau rein zu Layoutzwecken verwendet wird, soll die Tabelle nicht als Strukturelement gekennzeichnet sein.

**Warum wird das geprüft?**

Tabellen ohne Dateninhalt können sowohl die Reihenfolge der Fokussierung mittels assistiver Technologien, als auch das Verständnis und das mentale Modell der Mobile App negativ beeinflussen. Daher ist auf die Verwendung tabellarischer Strukturen für reine Layoutzwecke zu verzichten, mindestens jedoch sind solche Tabellen für assistive Technologien auszublenden.

**Bewertungsalternativen:** Erfüllt · Barriere

**Beispiele für positive Bewertung**

- Die Mobile App enthält keine Layouttabellen.
- Über die Komfortfunktion des jeweiligen Screenreaders (VoiceOver-Rotor und TalkBack-Menü) sind keine Layouttabellen auffindbar.

**Beispiele für negative Bewertung**

- Der VoiceOver-Rotor oder das Android TalkBack-Menü können über das entsprechende Strukturelement eine Tabelle an navigieren, die keine tabellarischen Informationen enthält.

<a id="k-11-1-3-1e"></a>

#### 11.1.3.1e Strukturelemente für weitere Objekte

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (vom übergeordneten Prüfschritt übernommen) |
| Prüfverfahren | BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#e-strukturelemente-für-weitere-objekte) |

##### BIT inklusiv App-Test (nur in diesem Verfahren)

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#e-strukturelemente-für-weitere-objekte) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn in der Mobile App weitere Strukturelemente vorhanden sind, die nicht explizit in den Prüfschritten 11.1.3.1a Strukturelemente für Überschriften, 11.1.3.1b Strukturelemente für Listen, Auflistungen und Aufzählungen, 11.1.3.1c Strukturelemente für Bedien- und Formularelemente und 11.1.3.1d Keine Strukturelemente für Layouttabellen genannt sind.

**Anforderung**

Gliederung und Strukturierung von Inhalten werden durch geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar. Die Betriebssysteme iOS und Android bieten innerhalb der Bedienungshilfen umfangreiche Möglichkeiten, um durch Strukturobjekte zu navigieren, ohne den gesamten Inhalt durchlaufen zu müssen. Dazu gehören beispielsweise Container, Links, Formularelemente, Datentabellen, Artikel, Tasten, Text- und Suchfelder oder Bilder.

**Warum wird das geprüft?**

Strukturobjekte strukturieren Inhalte für sehende Nutzende und schaffen so Orientierung. Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, müssen diese auch auf einer semantischen Ebene vermitteln. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können beispielsweise auch Nutzende von Screenreadern den Aufbau der Inhalte wahrnehmen und schnell navigieren.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Alle vorhandenen Strukturobjekte können mittels VoiceOver-Rotor beziehungsweise Android TalkBack-Menü separat annavigiert werden, ohne den gesamten Inhalt bis zum Erreichen des Objekts durchlaufen zu müssen.

**Beispiele für negative Bewertung**

- Keine oder nicht alle vorhandenen Strukturobjekte können mittels VoiceOver-Rotor beziehungsweise Android TalkBack-Menü separat annavigiert werden, ohne den gesamten Inhalt bis zum Erreichen des Objekts durchlaufen zu müssen.
- Nach Auswahl eines Strukturobjekts mittels VoiceOver-Rotor beziehungsweise Android TalkBack-Menü gibt der Screenreader die Meldung aus, dass das gewählte Strukturobjekt nicht vorhanden sei.

<a id="k-11-1-3-2"></a>

#### 11.1.3.2 Sinnvolle Reihenfolge

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sinnvolle-reihenfolge) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sinnvolle-reihenfolge) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Screenreadernutzende erhalten die Information der Reihenfolge dargestellter Informationen in gesprochener Form und können somit den Kontext von Inhalten besser verstehen. Durch eine nicht nachvollziehbare Reihenfolge der fokussierten Elemente wird die Bedienbarkeit für diese Nutzenden erheblich beeinträchtigt. Sehbeeinträchtigte oder blinde Nutzende werden darüber hinaus in ihrer Fähigkeit, sich ein mentales Modell vom Aufbau der der Inhalte zu machen, eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade

**Beispiele für positive Bewertung**

- Beim Öffnen einer Ansicht ist die Zurück-Taste das fokussierte Element.
- Beim Öffnen einer Ansicht ist der Seitentitel das fokussierte Element.
- Beim Öffnen einer Such-Ansicht ist das Suchfeld, das in der Mitte der Ansicht lokalisiert ist, das fokussierte Element.

**Beispiele für negative Bewertung**

- Die Bedienung ist über Wischnavigation möglich, jedoch nicht mit externer Tastatur, oder umgekehrt.
- Nach Öffnen des Menüs wird nicht das erste Inhaltselement des Menüs fokussiert.
- Nach Schließen eines Dialogs muss der Nutzende zusätzliche Schritte verfolgen, um zur Ausgangsposition zurückzukehren.
- Elemente werden in unlogischer Reihenfolge fokussiert.
- Dekorative Objekte, beispielsweise Trennlinien, werden fokussiert.
- Aufzählungspunkte innerhalb einer Liste werden fokussiert.
- Der Fokus kann von einem Header-Objekt nicht in den Inhalt versetzt werden.
- In einem Karussell-Widget werden die Bedienelemente zum Blättern erst nach den Inhalten fokussiert.
- In Teaser-Kacheln wird das verlinkte Bild oder das Datum vor der Überschrift fokussiert.
- Bei einem Formularelement wird zuerst das Eingabefeld, danach die Beschriftung fokussiert.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sinnvolle-reihenfolge) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Beim Einsatz von assistiven Technologien wie beispielsweise Screenreadern oder auch bei der Tastaturnutzung erhalten die Inhalte den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Screenreadernutzenden optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei von Element zu Element, bei Tastaturbedienung (mit aktivem Screenreader) von interaktivem Element zu interaktivem Element.

Ausgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden.

#todo#Bei einem Seitenaufruf soll bei der Tastaturnavigation das erste Element im Fokus die Seitenüberschrift sein. Ist keine Seitenüberschrift vorhanden, soll die Zurück-Taste als erstes Element fokussiert werden. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.

Bei Bedienung mit externer Tastatur soll das erste Element im Fokus das erste interaktive Element sein. Kontextelemente sind mit den Pfeiltasten erreichbar.

Bei modalen Dialogen muss das erste Element innerhalb des Dialogs fokussiert werden. Eine Wischnavigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Element befinden. Sollte der Dialog beim Start der Software oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Element der Seite befinden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sinnvolle-reihenfolge) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bei aktivem Screenreader erhalten die Inhalte sowohl bei der Wischnavigation als auch bei der Navigation mit externer Tastatur den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Screenreadernutzenden optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei bei der Wischnavigation von Element zu Element, bei Bedienung mit externer Tastatur mit aktivem Screenreader von interaktivem Element zu interaktivem Element.

Ausgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden.

Bei einem Seitenaufruf soll bei der Wischnavigation das erste Element im Fokus die Seitenüberschrift sein. Ist keine Seitenüberschrift vorhanden, soll die Zurück-Taste als erstes Element fokussiert werden. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.

Bei Bedienung mit externer Tastatur soll das erste Element im Fokus das erste interaktive Element sein. Kontextelemente sind mit den Pfeiltasten erreichbar.

Bei modalen Dialogen muss das erste Element innerhalb des Dialogs fokussiert werden. Eine Wischnavigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Element befinden. Sollte der Dialog beim Start der Mobile App oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Element der Seite befinden.

<a id="k-11-1-3-3"></a>

#### 11.1.3.3 Sensorische Eigenschaften

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sensorische-eigenschaften) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sensorische-eigenschaften) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Anweisungen für die Bedienung oder Verwendung der Inhalte dürfen nicht ausschließlich sensorische Merkmale (Farbe, Form, Größe, Position oder Töne) enthalten. Durch zusätzliche Informationen kann sichergestellt werden, dass auch blinde und sehbeeinträchtigte, sowie hörbeeinträchtigte Nutzende die entsprechenden Inhalte verstehen. Alle Nutzenden müssen auf alle Inhalte zugreifen können. Dies gilt auch dann, wenn sie beispielsweise Form, Größe oder Ton nicht wahrnehmen oder Informationen über die räumliche Position bzw. Ausrichtung nicht verwenden können. Manche Nutzende mit Beeinträchtigungen können Form oder Position aufgrund der Art der von ihnen verwendeten Hilfstechnologien nicht wahrnehmen. Es müssen spezifischere oder erweiterte Informationen zur Verfügung gestellt werden.

**Bewertungsalternativen:** Erfüllt · Blockade

**Beispiele für positive Bewertung**

- Es sind keine ausschließlichen Verweise mittels sensorischer Merkmale vorhanden.
- Für einen Verweis mittels sensorischer Merkmale sind weitere Anweisungen vorhanden, die keine Sinneswahrnehmung ansprechen.
- Anstatt „Klicken Sie die rechte, untere Taste“ lautet die Anweisung „Klicken Sie die Weiter-Taste, die Sie im unteren Teil der App auf der rechten Seite finden“.

**Beispiele für negative Bewertung**

- Eine Tabelle weist „Details können Sie in der rechten Spalte nachlesen“ an.
- Eine Funktion weist „Mit der runden Taste können Sie die Aktion jederzeit abbrechen“ an.
- Ein Umfrageergebnis weist „Der größere Balken zeigt die Antwort der meisten Teilnehmer“ an.
- Ein Video-/Sprach-Konferenzsystem informiert „Wenn Sie einen Piepton hören, betritt ein weiterer Teilnehmer den Raum“.
- Zum Seitenwechsel steht ein Pfeilsymbol deutlicher Kennzeichnung „Weiter“ zur Verfügung, die Anweisung im Kontext lautet jedoch „Zum Seitenwechsel wählen Sie das grüne Pfeilsymbol unteren rechten Ecke unter der Umfrage.“

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sensorische-eigenschaften) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Inhalte der Software werden auf inhaltliche Verweise mittels sensorischer Merkmale wie Form, Größe, Position, Ausrichtung, Orientierung oder Ton geprüft. Solche Verweise dürfen nicht ausschließlich mittels der jeweiligen Sinneswahrnehmung verständlich sein.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sensorische-eigenschaften) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Anweisungen, welche zum Verständnis oder zur Bedienung der Mobile App dienen, dürfen ihre Inhalte nicht ausschließlich durch Verweise auf sensorische Merkmale vermitteln. Zu diesen Merkmalen gehören Verweise auf die Form, Größe, Position, Ausrichtung, Orientierung oder den ausgegebenen Ton.

<a id="k-11-1-3-4"></a>

#### 11.1.3.4 Keine Beschränkung der Bildschirmausrichtung (App-Test: „Ausrichtung“)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#keine-beschränkung-der-bildschirmausrichtung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#ausrichtung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Nutzende richten Endgeräte in Bezug zu ihren Anforderungen entsprechend aus. Inhalte sollen die jeweilige Ausrichtung des Endgeräts unterstützen, um Zugänglichkeit für die Nutzenden sicherzustellen. Menschen mit Sehbeeinträchtigung oder kognitiv beeinträchtige Menschen profitieren von einem besseren Lesefluss, auch bei gegebenenfalls stark erhöhter Textgröße. Motorisch eingeschränkte Nutzende haben zudem das Endgerät oftmals in einer festen Ausrichtung am Rollstuhl montiert und sind auf eine flexible Ausrichtung von Inhalten angewiesen.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#keine-beschränkung-der-bildschirmausrichtung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Inhalte der Software sind hinsichtlich Ansicht und Bedienung nicht auf eine einzelne Anzeigeausrichtung im Hoch- oder Querformat beschränkt. Inhalte und Funktionen müssen bei Änderung der Fenstergröße nicht zwingenderweise in derselben Form angeboten werden. Situationsbedingt dürfen spezifische Seiten ausschließlich eine Ausrichtung unterstützen, sofern die Ausrichtung zur korrekten Darstellung, zum Verständnis oder zur Bedienung essenziell ist, beispielsweise bei einem Bankscheck, einer Klaviatur einer Klavieranwendung oder bei Inhalten der virtuellen Realität.

**Beispiele für positive Bewertung**

- Die Inhalte der Software sind nicht auf eine spezielle Ausrichtung beschränkt.
- Die Inhalte der Software sind nicht auf eine spezielle Ausrichtung beschränkt, die Darstellung der Inhalte ist jedoch verändert.
- Eine Ansicht der Software, die eine entsprechende Ausrichtung benötigt, ist situationsbedingt auf diese spezielle Ausrichtung beschränkt, alle anderen Inhalte reagieren auf die Änderung der Ausrichtung.

**Beispiele für negative Bewertung**

- Inhalte der Software sind auf eine Ausrichtung beschränkt, benötigen diese Ausrichtung jedoch nicht situationsbedingt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#ausrichtung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Inhalte der Mobile App sind hinsichtlich Ansicht und Bedienung nicht auf eine einzelne Anzeigeausrichtung im Hoch- oder Querformat beschränkt. Inhalte und Funktionen müssen bei der Neuausrichtung nicht zwingenderweise in derselben Form angeboten werden. Situationsbedingt dürfen spezifische Seiten ausschließlich eine Ausrichtung unterstützen, sofern die Ausrichtung zur korrekten Darstellung, zum Verständnis oder zur Bedienung essenziell ist, beispielsweise bei einem Bankscheck, einer Klaviatur einer Klavieranwendung oder bei Inhalten der virtuellen Realität.

**Beispiele für positive Bewertung**

- Die Inhalte der Mobile App sind nicht auf ein spezielles Format beschränkt.
- Die Inhalte der Mobile App sind nicht auf ein spezielles Format beschränkt, die Darstellung der Inhalte ist jedoch verändert.
- Eine Ansicht der Mobile App, die das entsprechende Format benötigt, ist situationsbedingt ist auf dieses spezielle Format beschränkt, alle anderen Inhalte reagieren auf die Änderung der Ausrichtung.

**Beispiele für negative Bewertung**

- Inhalte der Mobile App sind auf eine Ausrichtung beschränkt, benötigen diese Ausrichtung jedoch nicht situationsbedingt.

<a id="k-11-1-3-5"></a>

#### 11.1.3.5 Eingabezweck bestimmen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#eingabezweck-bestimmen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#eingabezweck-bestimmen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Eingabefelder für Nutzerdaten, oder Eingabefelder, die ein spezielles Format erwarten, vorhanden sind.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#eingabezweck-bestimmen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Eingabefelder, die sich eindeutig auf Nutzerdaten beziehen und Informationen wie beispielsweise Name, E-Mailadresse, postalische Adresse, Rufnummer oder Kreditkartendaten anfordern, müssen programmatisch ermittelbar sein, damit unterstützende Technologien Nutzenden den Zweck der Eingabe erläutern können.

**Warum wird das geprüft?**

Korrekte Autocomplete- oder Autofill-Vorschläge sorgen dafür, dass Nutzende die Informationen nicht mehr, beziehungsweise nicht mehr vollumfänglich selbst eingeben müssen und stattdessen den Wert des Feldes bestätigen oder bei Bedarf ändern können. Dies ist ein erheblicher Vorteil für Nutzende mit kognitiven und motorischen Beeinträchtigungen.

Da die Daten für die automatische Vervollständigung sprachunabhängig sind, können Nutzende aufgrund der festgelegten Taxonomie der Begriffe den Zweck der Eingabefelder erkennen, auch wenn sie mit der Sprache nicht vertraut sind.

Zusätzlich zur Nutzung dieser Taxonomien können Technologien bei Verwendung der Attributtechnik den korrekten Inhalt vorschlagen und automatisch ausfüllen, indem sie diese Felder basierend auf früheren Eingaben, die im System gespeichert sind, automatisch vervollständigen.

**Beispiele für positive Bewertung**

- In einem Kontaktformular werden die korrekten hinterlegten Daten für die Felder Name, E-Mailadresse und Telefon zum automatischen Befüllen vorgeschlagen.
- Im Rahmen eines Bestellvorgangs werden die korrekten hinterlegten Daten für Adresse und Lieferadresse zum automatischen Befüllen vorgeschlagen.

**Beispiele für negative Bewertung**

- In entsprechenden Eingabefeldern, für die hinterlegte Daten zur Verfügung stehen, erscheint kein Vorschlag für automatische Befüllung.
- In entsprechenden Eingabefeldern, für die hinterlegte Daten zur Verfügung stehen, werden die falschen Daten zur automatischen Befüllung vorgeschlagen.
- Es wurden Authentifizierungsdaten bei einem Login gespeichert, bei einem erneuten Aufruf der Eingabefelder zum Login wird jedoch nicht vorgeschlagen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#eingabezweck-bestimmen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Eingabefelder, die sich eindeutig auf Nutzerdaten beziehen und Informationen wie beispielsweise Name, E-Mailadresse, postalische Adresse, Rufnummer oder Kreditkartendaten anfordern, müssen programmatisch ermittelbar sein, damit unterstützende Technologien Nutzenden den Zweck der Eingabe erläutern können.

Eingabefelder, die eine Eingabe in einem speziellen Format erwarten wie beispielsweise Postleitzahlen, Rufnummern oder E-Mailadressen, sollen das für die jeweilige Eingabe optimierte Tastaturlayout ansteuern.

Anmeldedaten, die zur Authentifizierung in der Mobile App notwendig sind, werden beim Login durch den Passwortmanager abrufbar, sofern sie vorab gespeichert werden konnten.

**Warum wird das geprüft?**

Korrekte Autocomplete- oder Autofill-Vorschläge sorgen dafür, dass Nutzende die Informationen nicht mehr, beziehungsweise nicht mehr vollumfänglich selbst eingeben müssen und stattdessen den Wert des Feldes bestätigen oder bei Bedarf ändern können. Dies ist ein erheblicher Vorteil für Nutzende mit kognitiven und motorischen Beeinträchtigungen.

Da die Daten für die automatische Vervollständigung sprachunabhängig sind, können Nutzende aufgrund der festgelegten Taxonomie der Begriffe den Zweck der Eingabefelder erkennen, auch wenn sie mit der Sprache nicht vertraut sind.

Zusätzlich zur Nutzung dieser Taxonomien können Technologien bei Verwendung der Attributtechnik den korrekten Inhalt vorschlagen und automatisch ausfüllen, indem sie diese Felder basierend auf früheren Eingaben, die im Betriebssystem gespeichert sind, automatisch vervollständigen.

Optimiertes Tastaturlayout sorgt dafür, dass Nutzende direkt mit der Eingabe der entsprechenden Daten beginnen können, ohne manuell auf das Ziellayout innerhalb der Tastatur umschalten zu müssen.

**Beispiele für positive Bewertung**

- In einem Kontaktformular werden die korrekten hinterlegten Daten für die Felder Name, E-Mailadresse und Telefon zum automatischen Befüllen vorgeschlagen.
- Im Rahmen eines Bestellvorgangs werden die korrekten hinterlegten Daten für Adresse und Lieferadresse zum automatischen Befüllen vorgeschlagen.
- Bei der Eingabe einer Auftragsnummer, die ausschließlich aus Zahlen besteht, wird die numerische Tastatur als optimiertes Tastaturlayout angesteuert.
- Bei der Eingabe einer deutschen Postleitzahl oder einer Telefonnummer wird die numerische Tastatur als optimiertes Tastaturlayout angesteuert.
- Bei der Eingabe einer E-Mailadresse wird das optimierte Tastaturlayout angesteuert, das unter anderem Tasten für „@“ und die Top-Leveldomain („.com“, „.de“) enthält.
- Beim den Eingabefeldern zur Authentifizierung wird vorgeschlagen, über den Passwortmanager des Betriebssystems nach hinterlegten Logindaten zu suchen.

**Beispiele für negative Bewertung**

- In entsprechenden Eingabefeldern, für die hinterlegte Daten zur Verfügung stehen, erscheint kein Vorschlag für automatische Befüllung.
- In entsprechenden Eingabefeldern, für die hinterlegte Daten zur Verfügung stehen, werden die falschen Daten zur automatischen Befüllung vorgeschlagen.
- Entsprechende Eingabefelder steuern kein optimiertes Tastaturlayout an.
- Es wurden Authentifizierungsdaten bei einem Login gespeichert, bei einem erneuten Aufruf der Eingabefelder zum Login wird jedoch nicht vorgeschlagen, über den Passwortmanager des Betriebssystems nach entsprechenden Logindaten zu suchen.

<a id="k-11-1-4-1"></a>

#### 11.1.4.1 Verwendung von Farbe

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#verwendung-von-farbe) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#verwendung-von-farbe) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Farbe wird nicht als einzige visuelle Möglichkeit zur Übermittlung von Informationen, zur Anzeige einer Aktion, zum Anfordern einer Reaktion oder zur Unterscheidung eines visuellen Elements verwendet.

**Warum wird das geprüft?**

Farbe ist ein wichtiges Gestaltungselement, das die ästhetische Anziehungskraft, das Nutzungserlebnis und die Barrierefreiheit verbessert. Einige Nutzende haben jedoch Schwierigkeiten, Farben wahrzunehmen. Menschen mit Sehbeeinträchtigungen haben oftmals eine eingeschränkte Farbwahrnehmung. Ältere Menschen sehen Farben möglicherweise nicht gut. Manche Menschen sehen Farben gar nicht. Darüber hinaus können Nutzende, die Bildschirme mit begrenzten Farben oder monochromen Displays und Browser verwenden, möglicherweise nicht auf Informationen zugreifen, die nur in Farbe präsentiert werden. Die Bereitstellung der durch Farbe vermittelten Information über ein weiteres visuelles Mittel gewährleistet, dass Nutzende, die Farbe nicht sehen, die Information dennoch wahrnehmen können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade

**Beispiele für positive Bewertung**

- Überschriften sind zusätzlich zur Farbgebung eingerückt oder durch eine andere Schriftgröße hervorgehoben.
- Linien in Schaubildern sind zusätzlich gestrichelt oder durchgezogen.
- Flächen haben unterscheidbare Muster.
- Ein ausgewählter Menüeintrag erhält als Abgrenzung zu den anderen Einträgen eine grafische Markierung, etwa einen Rahmen oder einen Punkt.
- Als Markierung wird eine Invertierung von Vorder- und Hintergrundfarbe verwendet.
- Farbliche Warnhinweise werden durch Text ergänzt, beispielsweise „Warnung“.
- Farbliche Hinweise werden durch ein grafisches Symbol ergänzt, beispielsweise einen OK-Haken.
- Zustände werden nicht nur durch farbliche Kreise markiert, sondern zusätzlich durch unterschiedliche Formen wie beispielsweise Dreieck, Rechteck, Kreis.
- Pflichtfelder sind zusätzlich mit einem Stern-Symbol oder dem Begriff „Pflichtfeld“ gekennzeichnet.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#verwendung-von-farbe) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Beispiele für negative Bewertung**

- Informationen werden ausschließlich über Farbgebung vermittelt.
- Zustände werden ausschließlich über Farbe vermittelt.
- Ausgewählte Menüeinträge unterscheiden sich nur durch Farbe von anderen Einträgen.
- Links in einem Fließtext werden nur farblich markiert.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#verwendung-von-farbe) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Beispiele für negative Bewertung**

- Informationen werden ausschließlich über Farbgebung vermittelt.
- Zustände werden ausschließlich über Farbe vermittelt.
- Ausgewählte Menüeinträge unterscheiden sich nur durch Farbe von anderen Einträgen.
- Links in einem Fließtext werden nur farblich markiert.
- Die Kennzeichnung eines Fehlers, beispielsweise bei Fehleingabe von Informationen in einem Kontaktformular, erfolgt lediglich durch eine farbliche Markierung.

<a id="k-11-1-4-2"></a>

#### 11.1.4.2 Audio-Steuerelement

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Audio (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#audio-steuerelement) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#audio-steuerelement) |

##### Gleichlautend in beiden Verfahren

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Automatisch abgespielte Audioinhalte dauern nicht länger als drei Sekunden an.
- Es steht ein Steuerelement zum Anhalten und Abspielen des Audioinhalts zur Verfügung.
- Es steht eine Lautstärkeregelung zur Verfügung.

**Beispiele für negative Bewertung**

- Es stehen keine Steuerelemente zum Stoppen oder zur Regelung der Lautstärke zur Verfügung.
- Die Regelung der Lautstärke beeinflusst auch die Systemlautstärke.
- Audio wird beim Aufruf der Seite gestartet, die Steuerelemente befinden sich jedoch nicht unmittelbar am Beginn der Seite.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#audio-steuerelement) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn in der Software automatisch Audio abgespielt wird.

**Anforderung**

Für Audio-Inhalte, die in der Software automatisch und länger als 3 Sekunden abgespielt werden, muss ein Steuerelement zum Pausieren, Stoppen des Audios, oder eine Lautstärkeregelung zum Steuern der Audiolautstärke unabhängig von der Lautstärke des Gesamtsystems zur Verfügung stehen. Entsprechende Steuerelemente müssen am Seitenbeginn, beziehungsweise im unmittelbaren Kontext des Elements auffindbar sein.

**Warum wird das geprüft?**

Nutzende von Screenreadern werden durch automatisch abgespielte Audioinhalte gestört. Daher müssen derartige Inhalte kurz sein oder abgeschaltet beziehungsweise heruntergeregelt werden können. Die Lautstärke muss unabhängig von der Systemlautstärke geregelt werden können, damit die verschiedenen Audioquellen ausreichend kontrastieren. Von diesem Kriterium profitieren auch Nutzende mit Aufmerksamkeitsstörungen, die bei laufendem Ton Schwierigkeiten haben, sich auf die Bedienung oder die Inhalte der Software zu konzentrieren.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#audio-steuerelement) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn in der Mobile App automatisch Audio abgespielt wird.

**Anforderung**

Für Audio-Inhalte, die in der Mobile App automatisch und länger als 3 Sekunden abgespielt werden, muss ein Steuerelement zum Pausieren, Stoppen des Audios, oder eine Lautstärkeregelung zum Steuern der Audiolautstärke unabhängig von der Lautstärke des Gesamtsystems zur Verfügung stehen. Entsprechende Steuerelemente müssen am Seitenbeginn, beziehungsweise im unmittelbaren Kontext des Elements auffindbar sein.

**Warum wird das geprüft?**

Nutzende von Screenreadern werden durch automatisch abgespielte Audioinhalte gestört. Daher müssen derartige Inhalte kurz sein oder abgeschaltet beziehungsweise heruntergeregelt werden können. Die Lautstärke muss unabhängig von der Systemlautstärke geregelt werden können, damit die verschiedenen Audioquellen ausreichend kontrastieren. Von diesem Kriterium profitieren auch Nutzende mit Aufmerksamkeitsstörungen, die bei laufendem Ton Schwierigkeiten haben, sich auf die Bedienung oder die Inhalte der Mobile App zu konzentrieren.

<a id="k-11-1-4-3"></a>

#### 11.1.4.3 Kontrast von Texten

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#kontrast-von-texten) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#kontrast-von-texten) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#kontrast-von-texten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Kontraste von Text sind in allen Zuständen des Texts ausreichend. Hierbei wird bei Software grundsätzlich ein Kontrastverhältnis von 4,5:1 zugrunde gelegt.

Die Anforderung gilt nicht für Textinhalte, die Teil einer inaktiven Benutzeroberflächenkomponente, rein dekorativ, für niemanden sichtbar oder Teil eines Bildes sind, das bedeutende andere visuelle Inhalte enthält. Weiterhin sind Texte eines Logos oder Markennamen ausgenommen.

Die Prüfung erfolgt in der Standarddarstellung der Software. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.

Trotz ausreichender Kontrastwerte ist die Kombination von gesättigtem Rot mit Schwarz als Vorder- und Hintergrundfarbe für Texte nicht empfehlenswert (vgl. ISO 9241-125). Es existieren keine klaren Vorgaben für Farbwerte von gesättigtem Rot, daher kann der Prüfende nur aufgrund seiner subjektiven Wahrnehmung urteilen. Ein entsprechender Befund sollte nur als Empfehlung mitgeteilt werden.

Bei Texten, die auf einem Bild oder einem Farbverlauf liegen, ist der vermeintlich schwächste Kontrastzustand zu messen.

**Warum wird das geprüft?**

Gute Kontrastverhältnisse zwischen Text- und Hintergrundfarbe sorgen dafür, dass Nutzende die Inhalte besser erkennen und lesen können. Menschen mit Sehbeeinträchtigung und Farbfehlsichtigkeit profitieren von guten Kontrastverhältnissen genauso wie ältere Menschen, die unter verminderter Kontrastempfindlichkeit leiden. Letztendlich kommen gute Kontraste allen Menschen zugute, beispielsweise beim Verwenden der Software bei starker Sonneneinstrahlung oder bei Darstellung über einen Projektor.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade

**Beispiele für positive Bewertung**

- Das Kontrastverhältnis zwischen Text- und Hintergrundfarbe liegt bei 4,5:1 oder höher.

**Beispiele für negative Bewertung**

- Das Kontrastverhältnis zwischen Text- und Hintergrundfarbe liegt unter 4,5:1.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#kontrast-von-texten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Kontraste von Text sind in allen Zuständen des Texts ausreichend. Hierbei wird bei Mobile Apps grundsätzlich ein Kontrastverhältnis von 4,5:1 zugrunde gelegt, da eine pixelgenaue Betrachtung von Schriftgrößen auf mobilen Endgeräten nicht möglich ist. Zudem ist der Bildschirm eines mobilen Endgeräts kleiner als der Monitor eines Desktop-Systems, was zu Forderung der genannten Kontrastwerte führt.

Die Anforderung gilt nicht für Textinhalte, die Teil einer inaktiven Benutzeroberflächenkomponente, rein dekorativ, für niemanden sichtbar oder Teil eines Bildes sind, das bedeutende andere visuelle Inhalte enthält. Weiterhin sind Texte eines Logos oder Markennamen ausgenommen.

Die Prüfung erfolgt in der Standarddarstellung der App. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.

Trotz ausreichender Kontrastwerte ist die Kombination von gesättigtem Rot mit Schwarz als Vorder- und Hintergrundfarbe für Texte nicht empfehlenswert (vgl. ISO 9241-125). Es existieren keine klaren Vorgaben für Farbwerte von gesättigtem Rot, daher kann der Prüfende nur aufgrund seiner subjektiven Wahrnehmung urteilen. Ein entsprechender Befund sollte nur als Empfehlung mitgeteilt werden.

Bei Texten, die auf einem Bild oder einem Farbverlauf liegen, ist der vermeintlich schwächste Kontrastzustand zu messen.

**Warum wird das geprüft?**

Gute Kontrastverhältnisse zwischen Text- und Hintergrundfarbe sorgen dafür, dass Nutzende die Inhalte besser erkennen und lesen können. Menschen mit Sehbeeinträchtigung und Farbfehlsichtigkeit profitieren von guten Kontrastverhältnissen genauso wie ältere Menschen, die unter verminderter Kontrastempfindlichkeit leiden. Letztendlich kommen gute Kontraste allen Menschen zugute, beispielsweise beim Verwenden des mobilen Endgeräts bei starker Sonneneinstrahlung.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade

**Beispiele für positive Bewertung**

- Das Kontrastverhältnis zwischen Text- und Hintergrundfarbe liegt bei 4,5:1 oder höher.
- Wenige Kontrastverhältnisse zwischen Text- und Hintergrundfarbe weisen einen Grenzwert von 4,3:1 oder höher auf.

**Beispiele für negative Bewertung**

- Das Kontrastverhältnis zwischen Text- und Hintergrundfarbe liegt unter 4,5:1.
- Zu viele Kontrastverhältnisse zwischen Text- und Hintergrundfarbe weisen einen Grenzwert von 4,3:1 oder geringer auf.

<a id="k-11-1-4-4"></a>

#### 11.1.4.4 Textgröße ändern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#textgröße-ändern) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#textgröße-ändern) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Für Nutzende mit Sehbeeinträchtigung ist das Anpassen der Textgröße nach ihren Bedürfnissen ein essenzieller Bestandteil, um die Lesbarkeit von Inhalten zu verbessern. Nutzende mit Legasthenie profitieren zudem von einer erhöhten Lesegeschwindigkeit. Sämtlicher Text muss ohne Verlust von Inhalten mit der individuellen Textgröße des Systems skalieren.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#textgröße-ändern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Textgröße kann auf mindestens 200% skaliert werden, ohne dass Inhalt oder Funktionalität dabei verloren gehen. Die Vergrößerung kann über die Einstellungen der Textgröße im Betriebssystem, oder über eine softwarespezifische Funktionalität zur Verfügung gestellt werden. Beim Bereitstellen einer eigenen Funktionalität müssen die zugehörigen Steuerelemente am Seitenbeginn auffindbar sein.

**Beispiele für positive Bewertung**

- Alle Textinhalte skalieren entsprechend der Einstellung. Es entsteht keine Überlappung oder Verdrängung bei Inhalten.
- Die Skalierung führt zu scrollbaren Bereichen.

**Beispiele für negative Bewertung**

- Alle Textinhalte skalieren entsprechend der Einstellung, jedoch entsteht eine Überlappung oder Verdrängung bei Inhalten.
- Die skalierte Schrift wird unscharf dargestellt.
- Die Steuerelemente der softwarespezifischen Funktionalität sind nicht am Seitenanfang auffindbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#textgröße-ändern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Textgröße kann auf mindestens 200% skaliert werden, ohne dass Inhalt oder Funktionalität dabei verloren gehen. Die Vergrößerung kann über die Einstellungen der Textgröße im Betriebssystem, oder über eine App-eigene Funktionalität zur Verfügung gestellt werden. Beim Bereitstellen einer eigenen Funktionalität müssen die zugehörigen Steuerelemente am Seitenbeginn auffindbar sein. Eine Skalierung auf genau 200% ist in Android das mögliche Maximum, bei iOS können genau 200% nicht erreicht werden. Hier liegt der zu testende Wert zwischen 190% und 235%.

**Beispiele für positive Bewertung**

- Alle Textinhalte skalieren entsprechend der Einstellung. Es entsteht keine Überlappung oder Verdrängung bei Inhalten.
- Die Skalierung führt zu scrollbaren Bereichen.
- In iOS werden die Inhalte bei 190% Skalierung korrekt angezeigt, bei 235% entstehen Darstellungsfehler.

**Beispiele für negative Bewertung**

- Alle Textinhalte skalieren entsprechend der Einstellung, jedoch entsteht eine Überlappung oder Verdrängung bei Inhalten.
- Die skalierte Schrift wird unscharf dargestellt.
- Die Steuerelemente der eigenen App-Funktionalität sind nicht am Seitenanfang auffindbar.

<a id="k-11-1-4-5"></a>

#### 11.1.4.5 Schriftgrafiken

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#schriftgrafiken) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#schriftgrafiken) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Bilder oder Grafiken werden nicht zur Darstellung von Schrift verwendet. Logos, beziehungsweise Text, der Teil eines Logos oder Markennamens ist, ist hiervon ausgenommen. Eine Schriftgrafik ist eine visuelle Gestaltung und Anordnung von Buchstaben, Zeichen und Textelementen innerhalb eines grafischen Elements. Wird der Informationsinhalt der Schriftgrafik auch im Kontext als Text wiedergeben, gilt dies als Medienalternative.

**Warum wird das geprüft?**

Die Darstellung von textuellen Inhalten soll nicht in Form von grafischen Elementen erfolgen, sondern immer als Klartext. So kann sichergestellt werden, dass die Darstellung an individuelle Bedürfnisse angepasst werden kann, und assistive Technologien uneingeschränkt darauf zugreifen können.

Nutzende mit einer Sehbeeinträchtigung oder mit kognitiver Beeinträchtigung erleben aufgrund der Schriftfamilie, Schriftgröße oder -Farbe eventuell Einschränkungen hinsichtlich der Lesbarkeit und haben somit keinen vollumfänglichen Zugang zu den Informationen.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

**Beispiele für positive Bewertung**

- Es werden keine Schriftgrafiken verwendet.
- Es werden Schriftgrafiken verwendet, der Informationsinhalt ist im Kontext zusätzlich als Klartext verfügbar.

**Beispiele für negative Bewertung**

- Ein Bedienelement ist mit einer Schriftgrafik beschriftet.
- Überschriften werden durch Bilder dargestellt.
- Zur Darstellung von Text werden grafische Custom Fonts verwendet.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#schriftgrafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#schriftgrafiken) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-1-4-10"></a>

#### 11.1.4.10 Automatischer Umbruch (Reflow)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#automatischer-umbruch-reflow) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#automatischer-umbruch-reflow) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#automatischer-umbruch-reflow) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

- Alle Schriften der Anwendung können mit den Schriftgröße-Einstellungen der Plattform oder der Anwendung auf 400% vergrößert werden, ohne dass dabei Inhalt oder Funktionalität verloren gehen.

**Warum wird das geprüft?**

- Das Ziel ist die Bereitstellung eines Bedienmodus, in dem der Text groß genug ist, um von Benutzern mit stark vermindertem Sehvermögen genutzt werden zu können, ohne dass diese Benutzer in zwei Richtungen scrollen müssen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Alle Inhalte der Anwendung können gelesen und bedient werden, ohne dass Nutzende horizontal scrollen müssen.
- Nur Segmente mit Inhalten wie beispielsweise Tabellen, Diagramme, Präsentationen usw. können horizontal gescrollt werden.

**Beispiele für negative Bewertung**

- Wichtige Bereiche der Anwendung sind abgeschnitten, so dass Nutzende sie nicht mehr lesen und bedienen können.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#automatischer-umbruch-reflow) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist zum aktuellen Stand der Technik bei Mobile Apps nicht prüfbar (Stand Januar 2024).

**Bewertungsalternativen:** Nicht anwendbar

<a id="k-11-1-4-11"></a>

#### 11.1.4.11 Nicht-Text-Kontrast

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#nicht-text-kontrast) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#nicht-text-kontrast) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn grafische Bedienelemente oder informationstragende Grafiken vorhanden sind.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#nicht-text-kontrast) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Kontraste von grafischen Bedienelementen und informationstragenden Grafiken sind mit einem Kontrastverhältnis von mindestens 3:1 ausreichend. Bei der Kontrastmessung steht der informationstragende Bereich oder die Hervorhebung des Zustands eines Elements im Fokus.

Die Anforderung gilt nicht für inaktive Benutzeroberflächenkomponenten, inaktive Zustände von Bedienelementen, Logos beziehungsweise Markennamen, und grafische Inhalte, deren Inhalt nicht verändert werden darf, beispielsweise Flaggen, Screenshots oder Heatmaps.

Die Anforderung gilt weiterhin nicht für native grafische Bedienelemente, die in ihrem Erscheinungsbild nicht beeinflusst werden können, beispielsweise native Switches in ihren jeweiligen Zuständen, Checkboxen oder Radio-Buttons.

Die Prüfung erfolgt in der Standarddarstellung der Software. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.

**Warum wird das geprüft?**

Gute Kontrastverhältnisse sorgen dafür, dass Nutzende die Inhalte besser erkennen, lesen und bedienen können. Menschen mit Sehbeeinträchtigung und Farbfehlsichtigkeit profitieren von guten Kontrastverhältnissen genauso wie ältere Menschen, die unter verminderter Kontrastempfindlichkeit leiden. Letztendlich kommen gute Kontraste allen Menschen zugute, beispielsweise beim Verwenden der Software bei starker Sonneneinstrahlung oder bei Darstellung über einen Projektor.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Gemessene Kontrastverhältnisse liegen bei 3:1 oder höher.

**Beispiele für negative Bewertung**

- Gemessene Kontrastverhältnisse liegen unter 3:1.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#nicht-text-kontrast) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Kontraste von grafischen Bedienelementen und informationstragenden Grafiken sind mit einem Kontrastverhältnis von mindestens 3:1 ausreichend. Bei der Kontrastmessung steht der informationstragende Bereich oder die Hervorhebung des Zustands eines Elements im Fokus.

Die Anforderung gilt nicht für inaktive Benutzeroberflächenkomponenten, inaktive Zustände von Bedienelementen, Logos beziehungsweise Markennamen, und grafische Inhalte, deren Inhalt nicht verändert werden darf, beispielsweise Flaggen, Screenshots oder Heatmaps.

Die Anforderung gilt weiterhin nicht für native grafische Bedienelemente, die in ihrem Erscheinungsbild nicht beeinflusst werden können, beispielsweise native Switches in ihren jeweiligen Zuständen, Checkboxen oder Radio-Buttons.

Die Prüfung erfolgt in der Standarddarstellung der App. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.

**Warum wird das geprüft?**

Gute Kontrastverhältnisse sorgen dafür, dass Nutzende die Inhalte besser erkennen, lesen und bedienen können. Menschen mit Sehbeeinträchtigung und Farbfehlsichtigkeit profitieren von guten Kontrastverhältnissen genauso wie ältere Menschen, die unter verminderter Kontrastempfindlichkeit leiden. Letztendlich kommen gute Kontraste allen Menschen zugute, beispielsweise beim Verwenden des mobilen Endgeräts bei starker Sonneneinstrahlung.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade

**Beispiele für positive Bewertung**

- Gemessene Kontrastverhältnisse liegen bei 3:1 oder höher.
- Wenige Kontrastverhältnisse weisen einen Grenzwert von 2,85:1 oder höher auf.

**Beispiele für negative Bewertung**

- Gemessene Kontrastverhältnisse liegen unter 3:1.
- Zu viele Kontrastverhältnisse weisen einen Grenzwert von 2,85:1 oder geringer auf.

<a id="k-11-1-4-12"></a>

#### 11.1.4.12 Textabstand

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#textabstand) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#textabstand) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Für Nutzende mit Sehbeeinträchtigung ist das Anpassen von Abständen nach ihren Bedürfnissen ein essenzieller Bestandteil, um die Lesbarkeit von Inhalten zu verbessern. Nutzende mit Legasthenie profitieren zudem von einer erhöhten Lesegeschwindigkeit.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#textabstand) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Software eine softwarespezifische Funktion zum Verändern von Textabständen zur Verfügung stellt. Gängige Betriebssysteme wie beispielsweise Windows 11 und macOS bieten zum aktuellen Stand der Technik keine eigenen Möglichkeiten zum Anpassen von Textabständen (Stand Mai 2025). Bei Webanwendungen können Browsererweiterungen genutzt werden, um Textabstände zu anzupassen.

**Anforderung**

Das Anpassen der Zeilenhöhe auf mindestens das 1,5-fache der Textgröße, des Abstands nach Absätzen auf mindestens das 2-fache der Textgröße, des Zeichenabstands auf mindestens das 0,12-fache der Textgröße und des Wortabstands auf mindestens das 0,16-fache der Textgröße führt nicht zu Verlust von Inhalt oder Funktionalität.

**Beispiele für positive Bewertung**

- Alle Inhalte skalieren entsprechend der Anpassung. Es entsteht keine Überlappung oder Verdrängung bei Inhalten.
- Die Skalierung führt zu vertikal scrollbaren Bereichen.

**Beispiele für negative Bewertung**

- Alle Inhalte skalieren entsprechend der Einstellung, jedoch entsteht eine Überlappung oder Verdrängung bei Inhalten.

- Die Steuerelemente sind nicht am Seitenanfang auffindbar.
- Die Skalierung führt zu horizontal scrollbaren Bereichen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#textabstand) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die Mobile App eine App-eigene Funktion zum Verändern von Textabständen zur Verfügung stellt. Die Betriebssysteme iOS und Android bieten zum aktuellen Stand der Technik keine eigenen Möglichkeiten zum Anpassen von Textabständen (Stand Januar 2024).

**Anforderung**

Das Anpassen der Zeilenhöhe auf mindestens das 1,5-fache der Textgröße, des Abstands nach Absätzen auf mindestens das 2-fache der Textgröße, des Zeichenabstands auf mindestens das 0,12-fache der Textgröße und des Wortabstands auf mindestens das 0,16-fache der Textgröße führt nicht zu Verlust von Inhalt oder Funktionalität. Beim Bereitstellen der Funktion müssen die zugehörigen Steuerelemente am Seitenbeginn auffindbar sein.

**Beispiele für positive Bewertung**

- Alle Inhalte skalieren entsprechend der Anpassung. Es entsteht keine Überlappung oder Verdrängung bei Inhalten.
- Die Skalierung führt zu scrollbaren Bereichen.

**Beispiele für negative Bewertung**

- Alle Inhalte skalieren entsprechend der Einstellung, jedoch entsteht eine Überlappung oder Verdrängung bei Inhalten.

- Die Steuerelemente sind nicht am Seitenanfang auffindbar.

<a id="k-11-1-4-13"></a>

#### 11.1.4.13 Eingeblendete Inhalte (Hover oder Fokus)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#eingeblendete-inhalte-hover-oder-fokus) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#eingeblendete-inhalte-hover-oder-fokus) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn zusätzliche Inhalte beim Erhalt des Fokus, oder bei weiterführender Interaktion eingeblendet werden, beispielsweise Tooltips, Ausklapp-Menüs oder kontextbezogene Menüs.

**Warum wird das geprüft?**

Nutzende mit einer Sehbeeinträchtigung arbeiten häufig mit einer sehr starken Vergrößerung. müssen gegebenenfalls unabsichtlich ausgelöste, zusätzliche Inhalte auszublenden bzw. schließen können, ohne den Fokus zu bewegen, da häufig Informationen im Hintergrund überlagert werden. Der Inhalt darf nicht automatisch ausgeblendet werden, um genug Zeit zum Lesen sicherzustellen. Menschen mit motorischen Einschränkungen, die Schwierigkeiten beim präzisen Navigieren oder Steuern haben, können durch korrekte Interaktionsmethoden wie besser auf Inhalte zugreifen. Eine klare und vorhersehbare Benutzererfahrung ist besonders für Menschen mit kognitiven Beeinträchtigungen wichtig. Die Vermeidung unerwarteter Veränderungen durch eingeblendete Inhalte trägt dazu bei, eine weniger verwirrende Umgebung zu schaffen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#eingeblendete-inhalte-hover-oder-fokus) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Inhalte, die durch Tastatur-Fokus oder Mauszeiger-Hover aktiviert oder eingeblendet werden, sind für Nutzende navigierbar. Die eingeblendeten Inhalte werden nicht unerwartet oder automatisch geschlossen, sind jedoch jederzeit durch Nutzende schließbar. Das Schließen eines eingeblendeten Inhalts versetzt nicht den ursprünglichen Fokus.

**Beispiele für positive Bewertung**

- Eingeblendeter Inhalt wird nicht unerwartet geschlossen.
- Eingeblendeter Inhalt kann durch Mausklick außerhalb des eingeblendeten Inhalts, oder durch eine „Schließen-Funktion“, oder durch die Escape-Taste bei Verwendung einer externen Tastatur geschlossen werden.
- Eingeblendeter Inhalt ist programmatisch ermittelbar und wird durch den Screenreader korrekt wiedergegeben.

**Beispiele für negative Bewertung**

- Eingeblendeter Inhalt wird unerwartet geschlossen.
- Eingeblendeter Inhalt kann nicht durch Mausklick außerhalb des eingeblendeten Inhalts oder durch die Escape-Taste geschlossen werden.
- Eingeblendeter Inhalt ist programmatisch nicht ermittelbar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#eingeblendete-inhalte-hover-oder-fokus) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Inhalte, die durch Touch-Interaktion, Tastatur-Fokus oder Mauszeiger-Hover aktiviert oder eingeblendet werden, sind für den Nutzenden navigierbar. Sie werden nicht unerwartet oder automatisch geschlossen, sind jedoch jederzeit durch den Nutzenden schließbar. Das Schließen eines eingeblendeten Inhalts versetzt nicht den ursprünglichen Fokus.

**Beispiele für positive Bewertung**

- Eingeblendeter Inhalt wird nicht unerwartet geschlossen.
- Eingeblendeter Inhalt kann durch Tippen außerhalb des eingeblendeten Inhalts, oder durch eine „Schließen-Funktion“, oder durch die Escape-Taste bei Verwendung einer externen Tastatur geschlossen werden.
- Eingeblendeter Inhalt ist programmatisch ermittelbar und wird durch den Screenreader korrekt wiedergegeben.
- Eingeblendeter Inhalt kann bei aktivem Screenreader über die entsprechende Wischgeste geschlossen werden.

**Beispiele für negative Bewertung**

- Eingeblendeter Inhalt wird unerwartet geschlossen.
- Eingeblendeter Inhalt kann nicht durch Tippen außerhalb des eingeblendeten Inhalts oder durch die Escape-Taste bei Verwendung einer externen Tastatur geschlossen werden.
- Eingeblendeter Inhalt ist programmatisch nicht ermittelbar.
- Eingeblendeter Inhalt kann bei aktivem Screenreader nicht über die entsprechende Wischgeste geschlossen werden.

### Kapitel: Bedienbarkeit

<a id="k-11-2-1-1"></a>

#### 11.2.1.1 Ohne Maus nutzbar (App-Test: „Fokusbasierte Bedienung“)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#ohne-maus-nutzbar) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fokusbasierte-bedienung) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#ohne-maus-nutzbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Die Software ist ohne Verwendung einer Maus ausschließlich über eine Tastatur bedienbar. Ein eventuell auf der Tastatur vorhandenes Track-Pad ist nicht Bestandteil der Prüfung. Falls die Seite scrollbare Bereiche enthält, sollen die nicht sichtbaren Inhalte über die Tastatur erreichbar sein. Die Reihenfolge, in der die Elemente fokussiert werden, spielt in diesem Prüfschritt keine Rolle.

Prüfende müssen mit der Funktionsweise der Tastaturbedienung vertraut sein.

**Warum wird das geprüft?**

Viele motorisch beeinträchtigte Menschen sind auf die Tastaturbedienung angewiesen. Ihre Feinmotorik reicht für die Bedienung mit Händen und Fingern nicht aus. Die Tastaturbedienbarkeit erhöht zudem die Gebrauchstauglichkeit für alle Menschen, da sie einen effizienten, alternativen Bedienweg bei intensiver Nutzung bereitstellt.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade

**Beispiele für positive Bewertung**

- Die Software kann mit einer Tastatur bedient werden.
- Ein Schieberegler zur Steuerung der Lautstärke kann mit TAB angesprungen und wieder verlassen werden, mit den Pfeiltasten kann die Einstellung geändert werden.
- Ein geöffnetes Dialogfenster kann mit dem Schließen-Button oder mit der ESC-Taste geschlossen werden.
- In einem Eingabefeld kann Text mit der Tastatur zeichenweise fokussiert und markiert werden.
- Eine Software verwendet eine Scrollleiste, um lange Inhalte auf begrenztem Raum anzuzeigen. Der nicht sichtbare Bereich kann auch mit den Pfeiltasten und mit den Tasten SEITE AUF und SEITE AB sichtbar gemacht werden.
- Ein Programm zum rechnergestützten Zeichnen (CAD) wird üblicherweise mit einer Maus verwendet, es bietet aber auch die Möglichkeit, Punkte anhand ihrer x-, y- und z-Koordinaten festzulegen und Zeichnungselemente aus einer hierarchischen Liste von Unterbaugruppen und Teilen auszuwählen oder mit Hilfe der Pfeiltasten zwischen den auf der Anzeige dargestellten Elementen zu navigieren.
- Bedienelemente können auch bei Screenreader-Nutzung mit TAB oder Pfeiltasten fokussiert und mit EINGABE oder LEER ausgelöst werden.
- Tastaturfunktionen zur Bewegung in Daten und zur Selektion von Daten funktionieren auch mit dem Screenreader.
- Die Standardfunktionen STRG+LEER zum Ab- und Hinzuwählen von Daten aus einer Liste, falls in der Anwendung verwendet, funktionieren auch mit dem Screenreader.
- Shortcuts der Anwendung funktionieren weiterhin. Bei Konflikten mit den Shortcuts des Screenreaders greift das „Tastatur durchreichen“-Kommando des Screenreaders.
- Die Standardfunktionen wie F3 (Suchen) bzw. F4 (Suche wiederholen) unter Windows, falls in der Anwendung verwendet, funktionieren auch bei Screenreader-Nutzung.
- Die Standardfunktionen wie STRG+c (Kopieren), STRG+v (Einfügen), STRG+z (Rückgängig), falls in der Anwendung verwendet, funktionieren auch mit dem Screenreader.

**Beispiele für negative Bewertung**

- Die Software kann mit einer Tastatur nicht oder nicht vollumfänglich bedient werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fokusbasierte-bedienung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-2-1-1a"></a>

#### 11.2.1.1a Mit externer Tastatur bedienbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (vom übergeordneten Prüfschritt übernommen) |
| Prüfverfahren | BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#a-mit-externer-tastatur-bedienbar) |

##### BIT inklusiv App-Test (nur in diesem Verfahren)

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#a-mit-externer-tastatur-bedienbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Die Mobile App ist ohne Verwendung von Touch- oder Wischgesten ausschließlich über eine externe Tastatur bedienbar. Alle Bedienelemente der App können über die externe Tastatur angesteuert werden. Falls die Seite scrollbare Bereiche enthält, sollen die nicht sichtbaren Inhalte über die Tastatur erreichbar sein.

Ein eventuell auf der Tastatur vorhandenes Track-Pad ist nicht Bestandteil der Prüfung.

Falls die Seite scrollbare Bereiche enthält, sollen die nicht sichtbaren Inhalte über die Tastatur erreichbar sein. Die Reihenfolge, in der die Elemente fokussiert werden, spielt in diesem Prüfschritt keine Rolle.

Prüfende müssen mit der Funktionsweise der Tastaturbedienung vertraut sein.

**Warum wird das geprüft?**

Viele motorisch beeinträchtigte Menschen sind auf die Tastaturbedienung angewiesen. Ihre Feinmotorik reicht für die Bedienung mit Händen und Fingern nicht aus. Die Tastaturbedienbarkeit erhöht zudem die Gebrauchstauglichkeit für alle Menschen, da sie einen effizienten, alternativen Bedienweg bei intensiver Nutzung bereitstellt.

**Bewertungsalternativen:** Erfüllt · Blockade

**Beispiele für positive Bewertung**

- Alle Inhalte der App können mit externer Tastatur angesteuert werden.
- Alle Inhalte der App können mit externer Tastatur bedient werden.

**Beispiele für negative Bewertung**

- Die Mobile App kann mit externer Tastatur nicht oder nicht vollumfänglich bedient werden.

<a id="k-11-2-1-1b"></a>

#### 11.2.1.1b Mit assistiven Technologien bedienbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (vom übergeordneten Prüfschritt übernommen) |
| Prüfverfahren | BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#b-mit-assistiven-technologien-bedienbar) |

##### BIT inklusiv App-Test (nur in diesem Verfahren)

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#b-mit-assistiven-technologien-bedienbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Alle nicht ausschließlich dekorativen Inhalte einer Mobile App sind mit dem Screenreader ohne die Verwendung von koordinatenbasierten Gesten (das direkte Antippen von Inhalten) erreichbar. Die Interaktionsmöglichkeit, als auch die Reihenfolge, in der die Elemente fokussiert werden, spielt in diesem Prüfschritt keine Rolle.

Prüfende müssen mit der Funktionsweise des Screenreaders vertraut sein.

**Warum wird das geprüft?**

Viele physisch beeinträchtigte Menschen sind auf die Nutzung diverser assistiver Technologien angewiesen. Diese sind die wichtigste Schnittstelle zwischen beeinträchtigten Nutzenden und der Software und können, je nach Bedarf, sowohl bei der Wahrnehmbarkeit als auch bei der Bedienbarkeit unterstützen.

**Bewertungsalternativen:** Erfüllt · Blockade

**Beispiele für positive Bewertung**

- Alle Inhalte der App können mit aktivem Screenreader erreicht werden.
- Alle Inhalte der App können mit Screenreader über die Nutzung einer externen Tastatur erreicht werden.

**Beispiele für negative Bewertung**

- Manche Inhalte der App lassen sich mit dem Screenreader nicht erreichen.

<a id="k-11-2-1-2"></a>

#### 11.2.1.2 Keine Tastaturfalle

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#keine-tastaturfalle) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#keine-tastaturfalle) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Viele motorisch beeinträchtigte Menschen sind auf die Tastaturbedienung angewiesen. Ihre Feinmotorik reicht für die Bedienung mit Händen und Fingern nicht aus. Die Tastaturbedienbarkeit erhöht zudem die Gebrauchstauglichkeit für alle Menschen, da sie einen effizienten, alternativen Bedienweg bei intensiver Nutzung bereitstellt.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#keine-tastaturfalle) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Fokus bei Tastaturnavigation muss nach Erreichen eines Elements auch wieder vom Element wegbewegt werden können. Die Inhalte dürfen an keiner Stelle Elemente, Bereiche oder Menüs enthalten, die Mausinterkationen oder einen erzwungenen Neustart der Software benötigen, um wieder verlassen werden zu können.

**Beispiele für positive Bewertung**

- Die Software enthält keine Tastaturfallen.

**Beispiele für negative Bewertung**

- Die Software enthält Tastaturfallen.

- Nach Erreichen eines Elements kann dieses mit der Tabulatortaste nicht mehr verlassen werden. Es muss ein anderes Element per Mausklick fokussiert werden, um das Element zu verlassen.
- Nach Aufruf eines Optionsmenüs (in aller Regel durch ein Icon mit 3 Punkten gekennzeichnet) kann das Menü ohne Neustart der Software nicht mehr verlassen werden, da die Option „Abbrechen/Beenden/Schließen“ fehlt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#keine-tastaturfalle) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Fokus bei Tastaturnavigation als auch bei Wischnavigation bei aktivem Screenreader muss beim Erreichen eines Elements auch wieder vom Element wegbewegt werden können. Die Inhalte dürfen an keiner Stelle Elemente, Bereiche oder Menüs enthalten, die nicht ohne Wischen, direktes Antippen eines anderen Elements oder einen erzwungenen Neustart der Mobile App wieder verlassen werden können.

Im Betriebssystem iOS sind in aller Regel Seitengrenzen definiert. Beim Erreichen des ersten und letzten Elements einer Seite ist das jeweilige Ende der Seite erreicht, diese Grenzen sind nicht als Fehler zu werten.

**Beispiele für positive Bewertung**

- Die Mobile App enthält keine Tastaturfallen.

**Beispiele für negative Bewertung**

- Die Mobile App enthält Tastaturfallen.

- Nach Erreichen eines Elements kann dieses durch Rechts- oder Linkswisch nicht mehr verlassen werden. Es muss ein anderes Element per direktem Antippen fokussiert werden, um das Element zu verlassen.
- Nach Aufruf eines Optionsmenüs (in aller Regel durch ein Icon mit 3 Punkten gekennzeichnet) kann das Menü ohne Neustart der Mobile App nicht mehr verlassen werden, da die Option „Abbrechen/Beenden/Schließen“ fehlt.

<a id="k-11-2-1-4"></a>

#### 11.2.1.4 Tastaturkürzel

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#tastaturkürzel) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#tastaturkürzel) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Tastenkombinationen können mit den Tastenkombinationen von Assistenztechnologien kollidieren. Tastenkürzel müssen leicht zugänglich oder deaktivierbar sein, damit sie nicht in Konflikt mit den Funktionen von Hilfstechnologien geraten. Auch Nutzende, die Spracheingabe verwenden, können so unerwünschte Konflikte mit Sprachsteuerungsfunktionen vermeiden.

**Bewertungsalternativen:** Erfüllt · Blockade

**Beispiele für positive Bewertung**

- Das Drücken der Tasten löst keine Funktionen aus, es sind also keine Tastaturkürzel implementiert.
- Interaktive Bedienelemente sind mit einem Tastenkürzel implementiert, das in den Einstellungen abgeschaltet oder zu einer Mehrtastenkombination gemappt werden kann.
- Spezielle Bedienelemente, wie beispielsweise eine Auswahlliste, springen mit Drücken auf die entsprechende Taste zur Auswahloption.
- Es sind Tastaturkürzel implementiert, die in den Einstellungen abgeschaltet oder zu Mehrtasten-Kombinationen gemappt werden können.

**Beispiele für negative Bewertung**

- Es sind Tastaturkürzel implementiert, die in den Einstellungen nicht abgeschaltet oder zu Mehrtasten-Kombinationen gemappt werden können.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#tastaturkürzel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Verwendet die Software Tastaturkürzel über Einzeltasten (Buchstaben, Zahlen, Satzzeichen, Symbole), müssen die Tastaturkürzel abschaltbar sein oder auf eine Tastenkombination mit Verwendung einer zusätzlichen Modifikator-Taste umgestellt werden können, oder das Tastaturkürzel ist nur aktiv, wenn die entsprechende Benutzeroberflächenkomponente den Fokus erhält. Für Mehrtastenkombinationen darf die Umschalttaste nur in Kombination mit einer weiteren Modifikationstaste verwendet werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#tastaturkürzel) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Verwendet die Mobile App Tastaturkürzel über Einzeltasten (Buchstaben, Zahlen, Satzzeichen, Symbole), müssen die Tastaturkürzel abschaltbar sein oder auf eine Tastenkombination mit Verwendung einer zusätzlichen Modifikator-Taste umgestellt werden können, oder das Tastaturkürzel ist nur aktiv, wenn die entsprechende Benutzeroberflächenkomponente den Fokus erhält. Für Mehrtastenkombinationen darf die Umschalttaste nur in Kombination mit einer weiteren Modifikationstaste verwendet werden.

<a id="k-11-2-2-1"></a>

#### 11.2.2.1 Zeitvorgaben anpassbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#zeitvorgaben-anpassbar) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#zeitvorgaben-anpassbar) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Manche Menschen benötigen mehr Zeit, um eine Aktion abzuschließen, insbesondere wenn sie motorisch oder kognitiv beeinträchtigt sind. Die individuelle Anpassung von Zeitlimits stellt sicher, dass Nutzende Aktionen bedienen und abschließen können.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

**Beispiele für positive Bewertung**

- Zeitbegrenzungen erfüllen mindestens eine der in den Anforderungen genannten Möglichkeiten.

**Beispiele für negative Bewertung**

- Zeitbegrenzungen erfüllen keine der in den Anforderungen genannten Möglichkeiten, oder erfüllen sie nicht ausreichend.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#zeitvorgaben-anpassbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Bei zeitbegrenzten Inhalten kann die Zeitbegrenzung abgeschaltet, angepasst oder verlängert werden. Diese Anforderung gilt nicht für Limitierungen, die unverzichtbar sind oder ein Echtzeit-Ereignis darstellen. Es gelten folgende Anforderungen:

- Abschalten Die Zeitbegrenzung kann abgeschaltet werden, bevor sie eintritt.
- Anpassen Die Zeitbegrenzung kann angepasst werden, bevor sie eintritt, mindestens bis zum Zehnfachen der Standardeinstellung.
- Verlängern Die Zeitbegrenzung warnt vor Ablauf und stellt mindestens 20 Sekunden Zeit zur Verfügung, um die Zeitbegrenzung mit einer einfachen Handlung zu verlängern. Die Verlängerung kann mindestens 10mal vorgenommen werden.
- Echtzeit-Ausnahme Die Zeitbegrenzung ist ein erforderlicher Teil eines Echtzeit-Ereignisses und es ist keine Alternative zur Zeitbegrenzung möglich.
- Unverzichtbare Ausnahmen Die Zeitbegrenzung ist unverzichtbar und ihre Verlängerung würde den Vorgang ungültig machen.
- 20 Stunden Ausnahme Die Zeitbegrenzung beträgt standardmäßig mehr als 20 Stunden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#zeitvorgaben-anpassbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar, wenn Inhalte in der Mobile App zeitbegrenzt bereitgestellt werden.

**Anforderung**

Bei zeitbegrenzten Inhalten kann die Zeitbegrenzung abgeschaltet, angepasst oder verlängert werden. Diese Anforderung gilt nicht für Limitierungen, die unverzichtbar sind oder ein Echtzeit-Ereignis darstellen. Es gelten folgende Anforderungen:

- Abschalten
 Die Zeitbegrenzung kann abgeschaltet werden, bevor sie eintritt.
- Anpassen
 Die Zeitbegrenzung kann angepasst werden, bevor sie eintritt, mindestens bis zum Zehnfachen der Standardeinstellung.
- Verlängern
 Die Zeitbegrenzung warnt vor Ablauf und stellt mindestens 20 Sekunden Zeit zur Verfügung, um die Zeitbegrenzung mit einer einfachen Handlung zu verlängern. Die Verlängerung kann mindestens 10mal vorgenommen werden.
- Echtzeit-Ausnahme
 Die Zeitbegrenzung ist ein erforderlicher Teil eines Echtzeit-Ereignisses und es ist keine Alternative zur Zeitbegrenzung möglich.
- Unverzichtbare Ausnahmen
 Die Zeitbegrenzung ist unverzichtbar und ihre Verlängerung würde den Vorgang ungültig machen.
- 20 Stunden Ausnahme
 Die Zeitbegrenzung beträgt standardmäßig mehr als 20 Stunden.

<a id="k-11-2-2-2"></a>

#### 11.2.2.2 Bewegte Inhalte abschaltbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Ablenkender Inhalt durch Blinken oder Bewegung soll vermieden werden, zeitlich auf maximal 5 Sekunden begrenzt oder abschaltbar sein. Bewegter Inhalt, der Teil einer essenziellen Aktivität ist, ist von dieser Anforderung ausgenommen. Die Anforderung gilt für jegliche Inhalte, die blinken oder automatisch aktualisiert oder bewegt werden.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Bewegter Inhalt ist abschaltbar oder zeitlich auf maximal 5 Sekunden begrenzt.
- Die Animation einer Ladephase ist nicht zeitlich begrenzt und kann nicht gestoppt werden. Sie gilt jedoch als essenziell.

**Beispiele für negative Bewertung**

- Bewegter Inhalt ist nicht abschaltbar.
- Die Dauer von bewegtem Inhalt ist länger als 5 Sekunden, bewegter Inhalt ist nicht abschaltbar.
- Die Schaltfläche zum Starten und Stoppen von bewegtem Inhalt ist nicht im unmittelbaren Kontext auffindbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn in der Software gleichzeitig bewegte Inhalte und nicht bewegte Inhalte darstellt werden.

**Warum wird das geprüft?**

Blinkende oder sich bewegende Inhalte führen bei vielen Menschen zu Ablenkung. Sie können sich nicht auf andere Inhalte konzentrieren und verlieren den Fokus. Menschen mit beeinträchtigtem Sehvermögen oder Leseschwierigkeiten benötigen zudem ausreichend Zeit zur Verarbeitung der Informationen, um sie zu verstehen. Menschen, die einen Screenreader verwenden, können durch automatische Aktualisierungen gestört werden und die Orientierung verlieren. Zudem kann es auch zu Verschiebungen der Fokusposition kommen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn in der Mobile App gleichzeitig bewegte Inhalte und nicht bewegte Inhalte darstellt werden.

**Warum wird das geprüft?**

Blinkende oder sich bewegende Inhalte führen bei vielen Menschen zu Ablenkung. Sie können sich nicht auf andere Inhalte konzentrieren und verlieren den Fokus. Menschen mit beeinträchtigtem Sehvermögen oder Leseschwierigkeiten benötigen zudem ausreichend Zeit zur Verarbeitung der Informationen, um sie zu verstehen. Screenreadernutzende sind durch automatische Aktualisierungen gestört und verlieren Wahrnehmung und Orientierung bis hin zu Verschiebungen des Fokus.

<a id="k-11-2-3-1"></a>

#### 11.2.3.1 Verzicht auf Flackern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#verzicht-auf-flackern) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#verzicht-auf-flackern) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Grelles Flackern in bestimmten Frequenzen kann bei Menschen mit Epilepsie einen Anfall auslösen.

**Bewertungsalternativen:** Erfüllt · Blockade

**Beispiele für positive Bewertung**

- Es sind keine flackernden Elemente vorhanden.
- Flackernde Elemente flackern weniger als dreimal pro Sekunde.

**Beispiele für negative Bewertung**

- Flackernde Elemente flackern öfter als dreimal pro Sekunde.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#verzicht-auf-flackern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Software enthält keine Elemente, die in einem Zeitraum von 1 Sekunde öfter als dreimal aufblitzen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#verzicht-auf-flackern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Mobile App enthält keine Elemente, die in einem Zeitraum von 1 Sekunde öfter als dreimal aufblitzen.

<a id="k-11-2-4-3"></a>

#### 11.2.4.3 Sinnvolle Reihenfolge bei Bedienung mit externer Tastatur

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-bedienung-mit-externer-tastatur) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-bedienung-mit-externer-tastatur) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Viele motorisch beeinträchtigte Menschen sind auf die Tastaturbedienung angewiesen. Ihre Feinmotorik reicht für die Bedienung mit Händen und Fingern nicht aus. Durch eine nicht nachvollziehbare Reihenfolge der fokussierten Elemente wird die Bedienbarkeit für diese Nutzenden erheblich beeinträchtigt. Die Tastaturbedienbarkeit erhöht zudem die Gebrauchstauglichkeit für alle Menschen, da sie einen effizienten, alternativen Bedienweg bei intensiver Nutzung bereitstellt.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

**Beispiele für positive Bewertung**

- Beim Öffnen einer Ansicht ist die Zurück-Taste das fokussierte Element.
- Beim Öffnen einer Ansicht ist das erste Bedienelement das fokussierte Element.
- Beim Öffnen einer Such-Ansicht ist das Suchfeld, das in der Mitte der Ansicht lokalisiert ist, das fokussierte Element.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-bedienung-mit-externer-tastatur) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bei Bedienung der Software mit einer Tastatur erhalten interaktive Bedienelemente den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Tastaturnutzende optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei anhand der Bedienelemente von Element zu Element.

Ausgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden. Bei einem Seitenaufruf soll das erste Element im Fokus das erste Bedienelement sein. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.

Bei modalen Dialogen muss das erste Bedienelement innerhalb des Dialogs fokussiert werden. Eine Navigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Bedienelement befinden. Sollte der Dialog beim Start der Software oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Bedienelement der Seite befinden.

**Beispiele für negative Bewertung**

- Die Bedienung ist mit Tastatur nicht oder nicht vollumfänglich möglich.
- Nach Öffnen des Menüs wird nicht das erste interaktive Element des Menüs fokussiert.
- Nach Schließen eines Dialogs muss der Nutzende zusätzliche Schritte verfolgen, um zur Ausgangsposition zurückzukehren.
- Interaktive Elemente werden in unlogischer Reihenfolge fokussiert.
- Ein oder mehrere interaktive Elemente sind nicht erreichbar.
- Dekorative Objekte, beispielsweise Trennlinien, werden fokussiert.
- Der Fokus kann von einem Header-Objekt nicht in den Inhalt versetzt werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-bedienung-mit-externer-tastatur) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bei Bedienung der Mobile App mit einer externen Tastatur erhalten interaktive Bedienelemente den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Tastaturnutzende optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei anhand der Bedienelemente von Element zu Element.

Ausgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden. Bei einem Seitenaufruf soll das erste Element im Fokus das erste Bedienelement sein. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.

Bei modalen Dialogen muss das erste Bedienelement innerhalb des Dialogs fokussiert werden. Eine Navigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Bedienelement befinden. Sollte der Dialog beim Start der Mobile App oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Bedienelement der Seite befinden.

**Beispiele für negative Bewertung**

- Die Bedienung ist mit externer Tastatur nicht oder nicht vollumfänglich möglich.
- Nach Öffnen des Menüs wird nicht das erste interaktive Element des Menüs fokussiert.
- Nach Schließen eines Dialogs muss der Nutzende zusätzliche Schritte verfolgen, um zur Ausgangsposition zurückzukehren.
- Interaktive Elemente werden in unlogischer Reihenfolge fokussiert.
- Ein oder mehrere interaktive Elemente sind nicht erreichbar.
- Dekorative Objekte, beispielsweise Trennlinien, werden fokussiert.
- Der Fokus kann von einem Header-Objekt nicht in den Inhalt versetzt werden.

<a id="k-11-2-4-4"></a>

#### 11.2.4.4 Aussagekräftige Beschriftungen von Links

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Links vorhanden sind.

**Warum wird das geprüft?**

Nutzende von Screenreadern lesen Links einzeln und sind auf eine eindeutige Beschriftung angewiesen, um Zweck oder Ziel zu erkennen. Weiterhin bieten die Betriebssysteme die Möglichkeit, sämtliche Links einer Seite zu fokussieren. Der Zweck dieser Technik scheitert, wenn Linkbezeichnungen mehrfach vorkommen. Eine eindeutige Beschriftung unterstützt zudem Nutzende mit kognitiven Beeinträchtigungen oder Lernschwierigkeiten dabei, den Inhalt besser zu verstehen und zu verarbeiten.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für negative Bewertung**

- Die Beschriftung von Links ist nicht eindeutig.
- Die Informationen im unmittelbaren Kontext sind nicht eindeutig genug.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Zweck oder das Ziel eines Links kann direkt aus Beschriftung des Links, oder im unmittelbaren Kontext des Links bestimmt werden. Die Beschriftung von Links ist eindeutig. Werden innerhalb eines Kontextes mehrere Links verwendet, muss eine Unterscheidung durch die Beschriftung des Links gewährleistet sein, oder es wird ein anderes unterscheidbares Merkmal verwendet. Dies kann über kontextbezogene, weitere Informationen wie beispielsweise eine verständliche Information auf der Seite, die Handhabung oder Besonderheiten der Links erklärt, durch den Alternativtext einer verlinkten Grafik, durch eine vorhergehende Überschrift, oder durch Piktogramme im Fall von Dokumenten-Links umgesetzt werden.

Wird zum Öffnen des Ziels eine andere Anwendung außerhalb der zu testenden Software geöffnet, oder ist das Ziel ein Dokument, erhält der Nutzende eine entsprechende Information, sofern das Verlassen der Software nicht aus dem Kontext heraus ersichtlich ist.

Verlinkte URLs, E-Mailadressen und Telefonnummern übermitteln ihren Zweck automatisch über das Format und werden nicht bewertet.

**Beispiele für positive Bewertung**

- Die Beschriftung von Links ist eindeutig.
- Die Beschriftung von Links ist nicht eindeutig, aber im unmittelbaren Kontext werden eindeutige Zusatzinformationen bereitgestellt.
- Die Beschriftung von Links ist visuell nicht eindeutig, aber bei aktivem Screenreader werden Zusatzinformationen übermittelt.
- Das Öffnen eines Links außerhalb der Software ist über die visuelle Beschriftung nicht identifizierbar, aber bei aktivem Screenreader werden entsprechende Zusatzinformationen übermittelt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Zweck oder das Ziel eines Links kann direkt aus Beschriftung des Links, oder im unmittelbaren Kontext des Links bestimmt werden. Die Beschriftung von Links ist eindeutig. Werden innerhalb eines Kontextes mehrere Links verwendet, muss eine Unterscheidung durch die Beschriftung des Links gewährleistet sein, oder es wird ein anderes unterscheidbares Merkmal verwendet. Dies kann über kontextbezogene, weitere Informationen wie beispielsweise eine verständliche Information auf der Seite, die Handhabung oder Besonderheiten der Links erklärt, durch den Alternativtext einer verlinkten Grafik, durch eine vorhergehende Überschrift, oder durch Piktogramme im Fall von Dokumenten-Links umgesetzt werden.

Wird zum Öffnen des Ziels eine andere Anwendung außerhalb der Mobile App des Betriebssystems geöffnet, oder ist das Ziel ein Dokument, erhält der Nutzende eine entsprechende Information, sofern das Verlassen der Mobile App nicht aus dem Kontext heraus ersichtlich ist.

Verlinkte URLs, E-Mailadressen und Telefonnummern übermitteln ihren Zweck automatisch über das Format und werden nicht bewertet.

**Beispiele für positive Bewertung**

- Die Beschriftung von Links ist eindeutig.
- Die Beschriftung von Links ist nicht eindeutig, aber im unmittelbaren Kontext werden eindeutige Zusatzinformationen bereitgestellt.
- Die Beschriftung von Links ist visuell nicht eindeutig, aber bei aktivem Screenreader werden Zusatzinformationen übermittelt.
- Das Öffnen eines Links außerhalb der Mobile App ist über die visuelle Beschriftung nicht identifizierbar, aber bei aktivem Screenreader werden entsprechende Zusatzinformationen übermittelt.

<a id="k-11-2-4-6"></a>

#### 11.2.4.6 Überschriften und Beschriftungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#überschriften-und-beschriftungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#überschriften-und-beschriftungen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Nutzende können durch Strukturierung Inhalte leicht überblicken und gezielt auf Inhalte zugreifen. Knappe und zutreffende Beschriftungen unterstützen jeden Nutzenden, den Sinn eines Elements oder einer Gruppe von Elementen schnell zu erfassen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#überschriften-und-beschriftungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Inhalte werden visuell durch Überschriften strukturiert und beschreiben den nachfolgenden Inhalt zutreffend. Formularelemente sind aussagekräftig und zum Element passend beschriftet.

**Beispiele für positive Bewertung**

- Inhalte werden durch Überschriften strukturiert.
- Überschriften beschreiben den nachfolgenden Inhalt zutreffend.
- Formularelemente sind zutreffend beschriftet.

**Beispiele für negative Bewertung**

- Inhalte werden nicht durch Überschriften strukturiert.
- Überschriften beschreiben den nachfolgenden Inhalt unzutreffend.
- Formularelemente sind nicht oder nicht zutreffend beschriftet.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#überschriften-und-beschriftungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

In der Mobile App vorhandene Überschriften entsprechen der visuellen Strukturierung des Inhalts und beschreiben den nachfolgenden Inhalt zutreffend.

Das Vorhandensein von Haupt- und Unterüberschriften ist kein Teil der zugrundeliegenden Anforderungen, wird jedoch dringend empfohlen, da diese sowohl die visuelle als auch die semantische Strukturierung der Mobile App deutlich verbessern.

Visuelle Beschriftungen von Formularelementen sind aussagekräftig und entsprechen der Funktion des zugehörigen Elements.

**Beispiele für positive Bewertung**

- Überschriften beschreiben den nachfolgenden Inhalt zutreffend.
- Inhalte werden nicht durch Überschriften strukturiert.
- Formularelemente sind zutreffend beschriftet.

**Beispiele für negative Bewertung**

- Hauptüberschriften beschreiben den Seiteninhalt unzutreffend.
- Überschriften beschreiben den nachfolgenden Inhalt unzutreffend.
- Formularelemente sind nicht oder nicht zutreffend beschriftet.

<a id="k-11-2-4-7"></a>

#### 11.2.4.7 Fokus sichtbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#fokus-sichtbar) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fokus-sichtbar) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#fokus-sichtbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Bedienung mit einer Tastatur, mit inaktivem Screenreader oder mit Tastatur und aktivem Screenreader muss der Fokus zu jeder Zeit deutlich sichtbar sein. Der Fokus wird hierbei durch einen deutlich sichtbaren Rahmen, der das Element umrahmt, oder durch farbliches Ausfüllen der Elementfläche visualisiert.

Seiten, die mehr Inhalt bereitstellen als auf dem Bildschirm des Endgeräts angezeigt werden kann, müssen den Bildschirminhalt den Fokus verfolgend automatisch verschieben.

In Eingabefeldern muss zusätzlich zur Fokussierung des Elements der Cursor im Eingabefeld angezeigt werden.

Versteckte Sprunglinks sollen bei Fokuserhalt eingeblendet werden.

Für die Darstellung des Fokus gilt generell eine Kontrastanforderung von 3:1 zum Hintergrund und zum interaktiven Element.

**Warum wird das geprüft?**

Gerade für Menschen mit Beeinträchtigung des Sehvermögens, aber auch für Menschen mit motorischer Einschränkung ist es wichtig zu erkennen, welche Stelle aktuell fokussiert wird, welches Element gerade aktiv ist oder wo Eingaben erscheinen werden. Ohne sichtbaren Fokus ist die Software für optisch orientierte Menschen nicht bedienbar.

**Beispiele für positive Bewertung**

- Der Fokus ist an allen Stellen beziehungsweise in allen Elementzuständen deutlich sichtbar.
- Der Fokus bei Tastaturbedienung wechselt bei manchen Elementen von einem Rahmen auf eine ausgefüllte Hintergrundfläche.

**Beispiele für negative Bewertung**

- Der Fokus ist nur schwach sichtbar.
- Der Fokus bei Zustandsänderungen ist nicht deutlich erkennbar.
- Der Fokus ist für mehr als 3 Navigationsschritte nicht sichtbar.
- Der Fokus ist in ganzen Bereichen der Software nicht sichtbar.
- Bei Tastaturbedienung ohne aktiven Screenreader wird ein Eingabefeld wird durch einen Rahmen fokussiert, jedoch wird der Cursor im Eingabefeld nicht angezeigt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fokus-sichtbar) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Bedienung mit einer externen Tastatur, mit aktivem Screenreader oder mit externer Tastatur in Kombination mit aktivem Screenreader muss der visuelle Fokusindikator zu jeder Zeit deutlich sichtbar sein. Der Fokusindikator wird hierbei durch einen deutlich sichtbaren Rahmen, der das Element umrahmt, oder durch farbliches Ausfüllen der Elementfläche visualisiert.

In Eingabefeldern im Eingabemodus muss zusätzlich zur Fokussierung des Elements der Cursor im Eingabefeld angezeigt werden.

Versteckte Sprunglinks sollen bei Fokuserhalt eingeblendet werden.

Bei aktiver Bildschirmlupe soll der angezeigte Bildschirmausschnitt stets dem fokussierten Element folgen.

Für die Darstellung des Fokusindikators gilt generell eine Kontrastanforderung von 3:1 zum Hintergrund und zum fokussierten Element.

**Warum wird das geprüft?**

Gerade für Menschen mit Beeinträchtigung des Sehvermögens, aber auch für Menschen mit motorischer Einschränkung ist es wichtig zu erkennen, welche Stelle aktuell fokussiert wird, welches Element gerade aktiv ist oder wo Eingaben erscheinen werden. Ohne sichtbaren Fokus ist die Mobile App für optisch orientierte Menschen nicht bedienbar.

**Beispiele für positive Bewertung**

- Der Fokusindikator ist an allen Stellen beziehungsweise in allen Elementzuständen deutlich sichtbar.
- Der Fokusindikator bei Tastaturbedienung wechselt bei manchen Elementen von einem Rahmen auf eine ausgefüllte Hintergrundfläche.

**Beispiele für negative Bewertung**

- Der Fokusindikator ist nur schwach sichtbar.
- Der Fokusindikator umschließt nicht das fokussierte Element, da er versetzt angezeigt wird.
- Der Fokusindikator überlappt deutlich mit dem fokussierten Element.
- Bei Zustandsänderungen ist der Fokusindikator nicht deutlich erkennbar.
- Der Fokusindikator ist in einem Bereich der Mobile App nicht sichtbar.
- Bei Tastaturbedienung ohne aktiven Screenreader wird der Cursor im Eingabefeld nicht angezeigt.

<a id="k-11-2-4-11"></a>

#### 11.2.4.11 Fokus nicht verdeckt (Minimum)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#fokus-nicht-verdeckt-minimum) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fokus-nicht-verdeckt-minimum) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#fokus-nicht-verdeckt-minimum) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Bedienung mit einer Tastatur, mit inaktivem Screenreader oder mit Tastatur und aktivem Screenreader darf der Fokusrahmen, genauso wenig wie lediglich ein deutlich sichtbarerer Teil des Fokusrahmens zu keiner Zeit von anderen Inhalten oder Inhaltselementen der Software teilweise oder vollständig verdeckt werden. Dies gilt auch für durch Nutzende eingeblendete Inhalte, wie Ausklapplisten oder Menüs.

**Warum wird das geprüft?**

Gerade für Menschen mit Beeinträchtigung des Sehvermögens, aber auch für Menschen mit motorischer Einschränkung ist es wichtig zu erkennen, welche Stelle aktuell fokussiert wird, welches Element gerade aktiv ist oder wo Eingaben erscheinen werden. Wird der Fokusrahmen ganz oder teilweise von anderen Elementen oder Inhalten verdeckt, verlieren optisch orientierte Menschen leicht die Orientierung.

**Beispiele für positive Bewertung**

- Der Fokusrahmen wird zu keinem Zeitpunkt, auch nicht teilweise, verdeckt.
- Textblöcke verdecken zu keinem Zeitpunkt den sichtbaren Fokusrahmen von benachbarten Tasten.
- Eine erweiterte Ausklappliste verdeckt kein anderes Element, sobald dieses Element fokussiert wird.

**Beispiele für negative Bewertung**

- Der Fokusrahmen wird an manchen Stellen teilweise oder komplett verdeckt.
- Textblöcke verdecken teilweise den sichtbaren Fokusrahmen von benachbarten Tasten.
- Eine erweiterte Ausklappliste verdeckt andere Elemente auch dann, wenn diese fokussiert werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fokus-nicht-verdeckt-minimum) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Bedienung mit einer externen Tastatur, mit aktivem Screenreader oder mit externer Tastatur in Kombination mit aktivem Screenreader muss das fokussierte Element für den Nutzenden zu jeder Zeit sichtbar und in seinem Inhalt und seiner Funktion identifizierbar sein.

Das fokussierte Element soll zu keiner Zeit von anderen Inhalten verdeckt werden. Dies gilt auch für durch den Nutzenden eingeblendete Inhalte, wie Ausklapplisten oder Menüs. Vom Nutzenden eingeblendete Inhalte sollen hierfür entweder manuell ausgeblendet werden können oder bei der Fokussierung eines Elements, welches hierdurch verdeckt wird, automatisch ausgeblendet werden.

Seiten, die mehr Inhalt bereitstellen, als auf dem Bildschirm des Endgeräts angezeigt werden kann, müssen den Bildschirminhalt den Fokus verfolgend automatisch verschieben.

**Warum wird das geprüft?**

Gerade für Menschen mit Beeinträchtigung des Sehvermögens, aber auch für Menschen mit motorischer Einschränkung ist es wichtig zu erkennen, welche Stelle aktuell fokussiert wird, welches Element gerade aktiv ist oder wo Eingaben erscheinen werden. Werden aktuell fokussierte Elemente ganz oder teilweise von anderen Elementen oder Inhalten verdeckt, verlieren optisch orientierte Menschen leicht die Orientierung.

**Beispiele für positive Bewertung**

- Alle Elemente sind bei Fokuserhalt sichtbar.
- Eine fokussierte Taste wird an einer Stelle zum Teil von einer erweiterten Ausklappliste verdeckt, ist jedoch weiterhin eindeutig als Taste erkennbar und die Beschriftung ist vollständig lesbar.
- Seiteninhalte werden beim Navigieren passend gescrollt, sodass alle Elemente bei Fokuserhalt stets sichtbar bleiben.

**Beispiele für negative Bewertung**

- Ein Element wird bei Fokuserhalt durch einen vorangehenden Textblock vollständig verdeckt.
- Eine fokussierte Taste wird an einer Stelle zum Teil von einer erweiterten Ausklappliste verdeckt. Die Beschriftung der Taste ist nicht mehr vollständig lesbar.
- Fokussierte Elemente sind nicht sichtbar, da der Anzeigebereich nicht rechtzeitig automatisch scrollt.

<a id="k-11-2-5-1"></a>

#### 11.2.5.1 Zeigergesten (Alternativen für komplexe Zeiger-Gesten) (App-Test: „Alternativen für komplexe Gesten“)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#zeigergesten-alternativen-für-komplexe-zeiger-gesten) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#alternativen-für-komplexe-gesten) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Komplexe Gesten umfassen pfadbasierte Bewegungen und Mehrpunkt-Gesten. Funktionen, zu deren Bedienung komplexe Gesten notwendig sind, benötigen eine zugängliche Alternative zum Auslösen der Funktion, beispielsweise Tippen, Doppeltippen, Tippen und Halten, oder entsprechende Bedienelemente, beispielsweise Tasten.

Pfadbasierte Bewegungen sind beispielsweise Wischbewegungen, Schieberegler und Karussells, die von der Richtung der Interaktion abhängen, sowie andere Gesten, die einen vorgeschriebenen Pfad verfolgen, wie das Zeichnen einer spezifischen Form. Solche Pfade können mit einem Finger oder Stift auf einem Touchscreen, Grafiktablett oder Trackpad gezeichnet werden.

Ziehen ist eine Bewegung, bei der der Nutzende ein Objekt mit einem Finger aufnimmt und es an eine andere Position bewegt. Diese Bewegung vom Startpunkt zum Endpunkt erfordert nicht, dass der Nutzende einem bestimmten Pfad oder einer bestimmten Richtung folgt. Das Ziehen ist daher nicht pfadbasiert.

Mehrpunkt-Gesten sind beispielsweise das Zoomen durch Zusammenziehen von zwei Fingern, Steuerungen, bei denen ein Finger auf dem Bildschirm ruht und ein zweiter Finger tippt, oder ein Tipp oder Wisch mit zwei oder mehr Fingern.

Von der Anforderung ausgenommen sind essenzielle Funktionen, für die pfadbasierte oder Mehrpunkt-Gesten unverzichtbar sind, beispielsweise das Signieren von Dokumenten mittels Unterschrift.

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen können komplexe Gesten oftmals nicht oder nur eingeschränkt ausführen. Sie benötigen einfache, zugängliche Alternativen zum Ausführen solcher Funktionen.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Als Alternative für eine Zoom-Geste zum Vergrößern und Verkleinern von Inhalt sind Tasten verfügbar.
- Als Alternative für pfadbasierte Gesten wie „nach oben und dann nach links wischen“ oder „nach oben und dann nach rechts wischen“ zum Bedienen von E-Mails sind Tasten verfügbar.
- Als Alternative für das Bewegen innerhalb einer Karte kann ein Ort gesucht und mit einem Doppeltipp angeflogen werden.
- Als Alternative für die Mehrpunkt-Geste „ein Finger ruht, ein zweiter Finger wischt“ zum Drehen von Bildern kann das Bild fokussiert und mit einfachem Wisch gedreht werden.
- Als Alternative für Wischen vom Rand in die Mitte zum Ausklappen eines seitlichen Menüs reicht ein Tippen aus.
- Als Alternative für Wischen von oben nach unten zum Aktualisieren von Inhalt ist eine Taste verfügbar.

**Beispiele für negative Bewertung**

- Als Alternative für komplexe Gesten stehen Bedienelemente zur Verfügung, die sich jedoch nicht im unmittelbaren Kontext befinden.
- Es stehen keine Alternativen für komplexe Gesten zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#zeigergesten-alternativen-für-komplexe-zeiger-gesten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Anwendung, die auf Geräten mit Maus oder Touch-Bedienung funktioniert, Gesten zur Nutzung vorsieht, die über die vom Betriebssystem vorgesehenen Gesten hinausgehen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#alternativen-für-komplexe-gesten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn zur Bedienung der Mobile App komplexe Gesten nötig sind, die über die Standards des Betriebssystems hinausgehen.

<a id="k-11-2-5-2"></a>

#### 11.2.5.2 Abbrechen oder Widerrufen einer Touch-Geste

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Interaktive Elemente erfüllen mindestens eine der Anforderungen.

**Beispiele für negative Bewertung**

- Ein oder mehrere interaktive Elemente erfüllen keine der Anforderungen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Interaktive Elemente erfüllen bei Berührung des Elements mit einem einzelnen Finger, Mausklick oder Teilklick (Mouse-Down) mindestens eine der folgenden Bedingungen:

- Kein Down-Event Das Down-Event führt nicht direkt zum Auslösen der Funktion. Das bedeutet, die Funktion wird nicht durch das reine Drücken des interaktiven Elements ausgelöst. Erst das Loslassen des Elements, indem der Finger nach oben bewegt bzw. die Maustaste wieder losgelassen wird, löst die Funktion aus.
- Abbrechen oder Rückgängig machen Die Ausführung der Funktion erfolgt beim Up-Event. Es steht ein Mechanismus zur Verfügung, um die Funktion vor Abschluss abzubrechen, oder die Funktion nach Auslösen rückgängig zu machen.
- Up Reversal Das Loslassen des Elements (Up-Event) kehrt die Auswirkungen des vorherigen Drückens (Down-Event) mit dem Finger um. Wenn beispielsweise das Drücken einer Taste etwas aktiviert, wird durch das Loslassen der Taste der Vorgang rückgängig gemacht.
- Essenziell Die Funktion muss beim Drücken des Elements (Down-Event) durchgeführt werden. Es gibt keine Option zum Abbrechen oder Rückgängigmachen. Die Funktion wird unmittelbar beim Drücken abgeschlossen (beispielsweise beim Schreiben mit einem Finger oder einem Stift, oder beim Tippen mit einer virtuellen Tastatur).

Die Anforderung gilt nicht für Kontextmenüs, die bei längerem Gedrückthalten eines interaktiven Elements geöffnet werden.

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen können Maus- oder Touch-Gesten evtl. nicht punktgenau oder mit notwendiger Genauigkeit ausführen. Zur Fehlervermeidung müssen entsprechende Mechanismen verfügbar sein, um ungewollte Aktionen zu vermeiden, beziehungsweise um sie rückgängig zu machen oder abzubrechen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Interaktive Elemente erfüllen bei Berührung des Elements mit einem einzelnen Finger mindestens eine der folgenden Bedingungen:

- Kein Down-Event
 Das Down-Event des Fingers führt nicht direkt zum Auslösen der Funktion. Das bedeutet, die Funktion wird nicht durch das reine Drücken des interaktiven Elements ausgelöst. Erst das Loslassen des Elements, indem der Finger nach oben bewegt wird, löst die Funktion aus.
- Abbrechen oder Rückgängig machen
 Die Ausführung der Funktion erfolgt beim Up-Event. Es steht ein Mechanismus zur Verfügung, um die Funktion vor Abschluss abzubrechen, oder die Funktion nach Auslösen rückgängig zu machen.
- Up Reversal
 Das Loslassen des Elements (Up-Event) kehrt die Auswirkungen des vorherigen Drückens (Down-Event) mit dem Finger um. Wenn beispielsweise das Drücken einer Taste etwas aktiviert, wird durch das Loslassen der Taste der Vorgang rückgängig gemacht.
- Essenziell
 Die Funktion muss beim Drücken des Elements (Down-Event) durchgeführt werden. Es gibt keine Option zum Abbrechen oder Rückgängigmachen. Die Funktion wird unmittelbar beim Drücken abgeschlossen (beispielsweise beim Schreiben mit einem Finger oder einem Stift, oder beim Tippen mit einer virtuellen Tastatur).

Die Anforderung gilt nicht für Kontextmenüs, die bei längerem Gedrückthalten eines interaktiven Elements geöffnet werden.

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen können Touch-Gesten evtl. nicht punktgenau oder mit notwendiger Genauigkeit ausführen. Zur Fehlervermeidung müssen entsprechende Mechanismen verfügbar sein, um ungewollte Aktionen zu vermeiden, beziehungsweise um sie rückgängig zu machen oder abzubrechen.

<a id="k-11-2-5-3"></a>

#### 11.2.5.3 Sichtbare Beschriftung als Teil der programmatischen Beschriftung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sichtbare-beschriftung-als-teil-der-programmatischen-beschriftung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sichtbare-beschriftung-als-teil-der-programmatischen-beschriftung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Anforderung**

Visuell sichtbare Beschriftungen von interaktiven Elementen, unabhängig davon, ob sie mit reinem Text oder mit Text in grafischen Objekten beschriftet sind, müssen die visuelle in der programmatisch ermittelbaren Beschriftung enthalten. Idealerweise wird die visuelle Beschriftung direkt am Anfang der programmatischen Beschriftung genannt.

Die programmatische Beschriftung kann hierbei weitere Informationen enthalten, die visuell nicht ersichtlich ist.

**Warum wird das geprüft?**

Nutzende von Spracheingabe aktivieren Bedienelemente, indem sie die visuell sichtbare Beschriftung als Sprachbefehl diktieren. Weicht die programmatische von der visuellen Beschriftung ab, können Sprachbefehle unter Umständen nicht oder nur über Umwege platziert werden.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die programmatische Beschriftung ist analog zur visuellen Beschriftung.
- Die programmatische Beschriftung ist analog zur visuellen Beschriftung, enthält jedoch weitere Informationen.
- Die visuelle Beschriftung ist in der programmatischen Beschriftung enthalten, wird jedoch nicht am Anfang ausgegeben.

**Beispiele für negative Bewertung**

- Die programmatische Beschriftung lautet anders als die visuelle Beschriftung (beispielsweise „Nächste Seite“ und „Weiter“).
- Die programmatische Beschriftung weicht von der visuellen Beschriftung ab (beispielsweise „Nachname“ und „Geben Sie hier Ihren Nachnamen ein“).
- Es ist keine programmatische Beschriftung vorhanden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sichtbare-beschriftung-als-teil-der-programmatischen-beschriftung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sichtbare-beschriftung-als-teil-der-programmatischen-beschriftung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-2-5-4"></a>

#### 11.2.5.4 Steuerung durch Bewegung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#steuerung-durch-bewegung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#steuerung-durch-bewegung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Funktionen durch Bewegung des Endgeräts ausgelöst werden können. Beispiele für Bewegungsaktivierung sind Schütteln, Drehen, Kippen des Endgeräts, oder Bewegen des Endgeräts durch den Raum.

**Anforderung**

Funktionen, die durch die Bewegung des Endgeräts ausgelöst werden, müssen auch durch Steuerelemente auf der Benutzeroberfläche bedient werden können. Die Reaktion auf die Bewegungsaktivierung kann deaktiviert werden, um ein versehentliches Betätigen zu verhindern, es sei denn, die Bewegungsaktivierung ist für die jeweilige Funktion essenziell (beispielsweise bei einer Schrittzähler-App, oder bei Navigation durch Orte mit unterstützender Anzeige von Informationen zum Ort).

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen können Bewegungseingaben eventuell nicht oder nicht zielgerichtet mit notwendiger Genauigkeit ausführen. Oftmals sind Endgeräte montiert, was Bewegungseingaben unmöglich macht. Es müssen entsprechende Mechanismen verfügbar sein, um Funktionen auch ohne Bewegungssteuerung nutzen zu können. Zudem können ungewollte Bewegungen bei anderen motorischen Beeinträchtigungsbildern zu nicht gewünschten Eingaben führen. Zur Vermeidung von Fehlern müssen ungewollte Aktionen über eine Deaktivierung der Bewegungssteuerung verhindert werden.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für Funktionen, die durch Bewegung gesteuert werden, sind Bedienelemente verfügbar.
- Für Funktionen, die durch Bewegung gesteuert werden, sind Bedienelemente verfügbar, und die Bewegungssteuerung kann abgeschaltet werden.
- Ein Anmeldeprozess, der das Scannen eines QR-Codes voraussetzt, bietet eine weitere Alternative, für die keine Bewegung notwendig ist.

**Beispiele für negative Bewertung**

- Für Funktionen, die durch Bewegung gesteuert werden, sind keine Bedienelemente verfügbar.
- Die Bewegungssteuerung kann nicht abgeschaltet werden.
- Ein Anmeldeprozess kann ausschließlich über das Scannen eines QR-Codes durchgeführt werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#steuerung-durch-bewegung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#steuerung-durch-bewegung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-2-5-7"></a>

#### 11.2.5.7 Ziehbewegungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#ziehbewegungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#ziehbewegungen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Funktionen durch Ziehbewegungen (Drag & Drop) ausgeführt werden können.

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen können komplexe Gesten und Ziehbewegungen oftmals nicht oder nur eingeschränkt ausführen. Sie benötigen einfache, zugängliche Alternativen zum Ausführen solcher Funktionen.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Das Verschieben einer Datei in einen anderen Ordner kann entweder über Drag & Drop oder über ein Kontextmenü durchgeführt werden.
- Das Ändern der Reihenfolge von fest definierten Objekten in einer Liste kann entweder über Drag & Drop oder über dafür bereitgestellte Tasten durchgeführt werden.

**Beispiele für negative Bewertung**

- Das Verschieben einer Datei in einen anderen Ordner kann ausschließlich über Drag & Drop durchgeführt werden, es stehen keine alternativen Methoden zur Verfügung.
- Das Ändern der Reihenfolge von fest definierten Objekten in einer Liste kann ausschließlich über Drag & Drop durchgeführt werden, es stehen keine alternativen Methoden zur Verfügung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#ziehbewegungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Sämtliche Funktionen, die über Ziehbewegungen (Drag & Drop) ausgeführt werden können, müssen über eine alternative Eingabemethode (beispielsweise durch Tasten oder durch ein Kontextmenü) durchführbar sein.

Pfadbasierte Gesten sind nicht Bestandteil dieses Prüfschritts.

- Definition einer Ziehbewegung: Der Start- und der Zielpunkt der Geste sind an feste Koordinaten gebunden. Die Route zwischen den Punkten spielt hingegen keine Rolle.
- Definition einer pfadbasierte Geste: Der Start- und der Zielpunkt der Geste spielen keine Rolle, die Richtung oder Form der Route hingegen schon.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#ziehbewegungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Sämtliche Funktionen, die über Ziehbewegungen (Drag & Drop) ausgeführt werden können, müssen über eine alternative Eingabemethode (beispielsweise durch Tasten oder durch ein Kontextmenü) durchführbar sein.

Pfadbasierte Gesten sind nicht Bestandteil dieses Prüfschritts.

- Definition einer Ziehbewegung:
 Der Start- und der Zielpunkt der Geste sind an feste Koordinaten gebunden. Die Route zwischen den Punkten spielt hingegen keine Rolle.
- Definition einer pfadbasierte Geste:
 Der Start- und der Zielpunkt der Geste spielen keine Rolle, die Richtung oder Form der Route hingegen schon.

<a id="k-11-2-5-8"></a>

#### 11.2.5.8 Zielgröße (Minimum)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#zielgröße-minimum) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#zielgröße-minimum) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Menschen mit motorischen Beeinträchtigungen benötigen zum Ausführen von Touch-Funktionen ausreichend Platz. Sie haben Schwierigkeiten, kleine Ziele punktgenau zu erreichen. Hinreichende Größenverhältnisse verringern die Fehleranfälligkeit, ungewollt falsche Funktionen auszuführen oder falsche Ziele zu treffen.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Ein entsprechendes User-Interface-Element weist subjektiv eine ausreichende Größe auf.
- Für ein entsprechendes User-Interface-Element greifen eine oder mehrere der Ausnahmen.

**Beispiele für negative Bewertung**

- Ein entsprechendes User-Interface-Element weist subjektiv keine ausreichende Größe auf, und es greift nicht mindestens eine der Ausnahmen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#zielgröße-minimum) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt kann nur mit Zugriff auf den Quellcode der Software belastbar getestet werden. Während eines Blackbox-Tests in der Praxis, kann nur eine subjektive Überprüfung vorgenommen werden.

**Anforderung**

User-Interface-Elemente, die die Funktion eines Ziels übernehmen (beispielsweise bei Drag & Drop Events), müssen während der Nutzung eine Zielgröße von mindestens 24 x 24 CSS-Pixel aufweisen. Hierbei gelten folgende Ausnahmen:

- Abstand Ziele, die kleiner als 24 x 24 CSS-Pixel sind, bieten ausreichend Abstand zu anderen Zielen. Sie werden so positioniert, dass bei Zentrierung eines Kreises mit einem Durchmesser von 24 CSS-Pixeln auf dem Begrenzungsrahmen des jeweiligen Ziels, der positionierte Kreis keine anderen Ziele, oder andere Kreise für zu kleine Ziele überschneidet
- Gleichwertige Alternative Die Funktion kann durch ein anderes Steuerelement (beispielsweise durch eine Taste oder ein Kontextmenü), das sich auf der gleichen Seite befindet, ausgeführt werden
- Inline Das Ziel befindet sich in innerhalb eines Fließtexts, oder die Größe des Ziels wird durch die Zeilenhöhe von umgebendem, nicht interaktivem Text eingeschränkt
- Kontrolle durch User-Agent Die Größe des Ziels wird vom User-Agent bestimmt und wird vom Autor:in nicht verändert
- Essenziell Die Darstellung des Ziels ist unerlässlich oder gesetzlich vorgeschrieben

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#zielgröße-minimum) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt kann nur mit Zugriff auf den Quellcode der Mobile App belastbar getestet werden. Während eines Blackbox-Tests in der Praxis kann nur eine subjektive Überprüfung vorgenommen werden.

**Anforderung**

User-Interface-Elemente, die die Funktion eines Ziels übernehmen (beispielsweise bei Drag & Drop Events), müssen während der Nutzung eine Zielgröße von mindestens 24 x 24 CSS-Pixel aufweisen. Hierbei gelten folgende Ausnahmen:

- Abstand
 Ziele, die kleiner als 24 x 24 CSS-Pixel sind, bieten ausreichend Abstand zu anderen Zielen. Sie werden so positioniert, dass bei Zentrierung eines Kreises mit einem Durchmesser von 24 CSS-Pixeln auf dem Begrenzungsrahmen des jeweiligen Ziels, der positionierte Kreis keine anderen Ziele, oder andere Kreise für zu kleine Ziele überschneidet
- Gleichwertige Alternative
 Die Funktion kann durch ein anderes Steuerelement (beispielsweise durch eine Taste oder ein Kontextmenü), das sich auf der gleichen Seite befindet, ausgeführt werden
- Inline
 Das Ziel befindet sich in innerhalb eines Fließtexts, oder die Größe des Ziels wird durch die Zeilenhöhe von umgebendem, nicht interaktivem Text eingeschränkt
- Kontrolle durch User-Agent
 Die Größe des Ziels wird vom User-Agent bestimmt und wird vom Autor:in nicht verändert
- Essenziell
 Die Darstellung des Ziels ist unerlässlich oder gesetzlich vorgeschrieben

### Kapitel: Verständlichkeit

<a id="k-11-3-1-1"></a>

#### 11.3.1.1 Sprache der Software

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#sprache-der-software) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#sprache-der-software) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Bewertungsalternativen:** Erfüllt · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#sprache-der-software) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Software soll die Sprache, in der sie verwendet wird, an das Betriebssystem übermitteln, um die lokalisierte Anzeige von Text und die korrekte Auswahl des Sprachpakets und der korrekten Aussprache des Screenreaders sicherzustellen.

Dies betrifft mindestens die Standardsprache der Software. Bietet sie über eine Sprachauswahl entsprechende Lokalisierungen an, sind auch diese Sprachen Bestandteil der Anforderung.

Worte oder einzelne Passagen oder Phrasen des Inhalts, die von der verwendeten Sprache abweichen, sind nicht Teil der Anforderung. Es wird jedoch empfohlen, entsprechende Inhalte zu kennzeichnen.

Die Qualität von Lokalisierungen ist nicht Bestandteil der Prüfung.

**Warum wird das geprüft?**

Screenreader verwenden verschiedene Sprachausgaben für die jeweiligen natürlichen Sprachen. Die Sprachausgabe bestimmt, ob die Inhalte einer Software korrekt ausgesprochen werden. Die Sprache der Software soll programmatisch ermittelbar sein, damit Inhalte korrekt angezeigt und vom Screenreader automatisch das korrekte Sprachpaket gewählt wird. Muss der Nutzende eingreifen, um die Sprachausgabe zu ändern, beeinträchtigt dies Arbeitsabläufe.

**Beispiele für positive Bewertung**

- Die Software übermittelt die verwendete Sprache korrekt an das Betriebssystem. Der Inhalt wird in der gewählten Sprache angezeigt, und es wird das entsprechende Sprachpaket des Screenreaders verwendet.
- Das Betriebssystem in deutscher Sprache konfiguriert. Ein internationaler Newsticker ist in englischer Sprache verfasst. Die Sprachausgabe wechselt in die englische Sprache, sobald der Benutzende den Newsticker öffnet.
- Beim Wechsel zwischen mehreren offenen Anwendungen, wechselt die Sprache automatisch, sobald die zu prüfende Anwendung aktiv wird.

**Beispiele für negative Bewertung**

- Die Software übermittelt die verwendete Sprache nicht an das Betriebssystem. Der Inhalt wird zwar in der gewählten Sprache angezeigt, jedoch wird nicht das entsprechende Sprachpaket des Screenreaders verwendet.
- Der Benutzende muss zunächst im Screenreader die Sprache umstellen, bevor er die Anwendung verwenden kann.
- Geschlossene Funktionalität: Die Anwendung wird in Deutsch angezeigt. Deutsch ist auch im Betriebssystem des Nutzenden voreingestellt. Der Screenreader gibt jedoch fremdsprachliche Begriffe in deutscher Sprache unverständlich aus.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#sprache-der-software) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Mobile App soll die Sprache, in der sie verwendet wird, an das Betriebssystem übermitteln, um die lokalisierte Anzeige von Text und die korrekte Auswahl des Sprachpakets und der korrekten Aussprache des Screenreaders sicherzustellen.

Dies betrifft mindestens die Standardsprache der Mobile App. Bietet sie über eine Sprachauswahl entsprechende Lokalisierungen an, sind auch diese Sprachen Bestandteil der Anforderung.

Worte oder einzelne Passagen oder Phrasen des Inhalts, die von der verwendeten Sprache abweichen, sind nicht Teil der Anforderung. Es wird jedoch empfohlen, entsprechende Inhalte zu kennzeichnen.

Die Qualität von Lokalisierungen ist nicht Bestandteil der Prüfung.

**Warum wird das geprüft?**

Screenreader verwenden verschiedene Sprachausgaben für die jeweiligen natürlichen Sprachen. Die Sprachausgabe bestimmt, ob die Inhalte einer Mobile App korrekt ausgesprochen werden. Die Sprache der Mobile App soll programmatisch ermittelbar sein, damit Inhalte korrekt angezeigt und vom Screenreader automatisch das korrekte Sprachpaket gewählt werden. Muss der Nutzende eingreifen, um die Sprachausgabe zu ändern, beeinträchtigt dies Arbeitsabläufe.

**Beispiele für positive Bewertung**

- Die Mobile App übermittelt die verwendete Sprache korrekt an das Betriebssystem. Der Inhalt wird in der gewählten Sprache angezeigt, und es wird das entsprechende Sprachpaket des Screenreaders verwendet.

**Beispiele für negative Bewertung**

- Die Mobile App übermittelt die verwendete Sprache nicht an das Betriebssystem. Der Inhalt wird zwar in der gewählten Sprache angezeigt, jedoch wird nicht das entsprechende Sprachpaket des Screenreaders verwendet.

<a id="k-11-3-2-1"></a>

#### 11.3.2.1 Kontextänderung bei Fokus

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#kontextänderung-bei-fokus) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#kontextänderung-bei-fokus) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Warum wird das geprüft?**

Unerwartete oder unangekündigte Kontextänderungen beeinträchtigen die Orientierung von Nutzenden. Sie lenken ab, verwirren, oder bleiben gerade bei Menschen mit Sehbeeinträchtigungen unbemerkt.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#kontextänderung-bei-fokus) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Fokuserhalt von Benutzeroberflächenkomponenten bei Tastaturbedienung (Tabulator oder Pfeiltasten) und bei Tastaturbedienung mit aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise plötzliches Öffnen von neuen Ansichten oder weiteren Applikationen oder Einblenden von neuen Bedienelementen).

Das Anzeigen von Tooltips oder anderen Hinweisen, die sich beim Weiternavigieren selbständig wieder schließen, gilt nicht als unerwartete Kontextänderung.

Pop-Ups oder modale Dialoge sind nicht Bestandteil der Prüfung.

**Beispiele für positive Bewertung**

- Interaktive Elemente lösen bei Fokuserhalt keine unerwartete Kontextänderung aus.
- Interaktive Elemente lösen bei Fokuserhalt die Anzeige von Hinweisen aus, die bei Fokusversetzung automatisch wieder ausgeblendet werden.
- Interaktive Elemente lösen bei Fokuserhalt eine Kontextänderung aus, über die der Nutzende im Vorfeld informiert wurde.
- Eine Kontextänderung wird explizit ausgelöst, z.B. mit der Eingabetaste auf einem Bedienelement (Menüpunkt, Button, Link), oder durch das Verlassen eines Eingabefeldes mit Tabulatortaste.

**Beispiele für negative Bewertung**

- Interaktive Elemente lösen bei Fokuserhalt eine unerwartete Kontextänderung aus, über die der Nutzende im Vorfeld nicht informiert wurde.
- Ein Formular wird automatisch abgeschickt, sobald ein bestimmtes Eingabefeld den Fokus erhält.
- Ein Dropdown-Menü enthält Sprunglinks, die direkt ausgelöst werden, sobald ein Menüeintrag mit der Pfeiltaste fokussiert wird.
- Bei Fokus eines Radiobuttons öffnet sich automatisch ein Pop-up mit einem Hilfetext.
- Erhält ein Eingabefeld den Fokus, wird ein Hilfedialogfenster geöffnet, der das Feld beschreibt. Bei Navigation mit der Tabulatortaste durch die Anwendung, wird das Dialogfeld geöffnet und der Tastaturfokus beim Verlassen des Feldes direkt in das Pop-up versetzt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#kontextänderung-bei-fokus) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Der Fokuserhalt von Benutzeroberflächenkomponenten bei Bedienung mit einer externen Tastatur und bei Wischnavigation bei aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise plötzliches Öffnen von neuen Ansichten oder weiteren Applikationen oder Einblenden von neuen Bedienelementen).

Das Anzeigen von Tooltips oder anderen Hinweisen, die sich beim Weiternavigieren selbständig wieder schließen, gilt nicht als unerwartete Kontextänderung.

Pop-Ups oder modale Dialoge sind nicht Bestandteil der Prüfung.

**Beispiele für positive Bewertung**

- Interaktive Elemente lösen bei Fokuserhalt keine unerwartete Kontextänderung aus.
- Interaktive Elemente lösen bei Fokuserhalt die Anzeige von Hinweisen aus, die bei Fokusversetzung automatisch ausgeblendet werden.
- Interaktive Elemente lösen bei Fokuserhalt eine Kontextänderung aus, über die der Nutzende im Vorfeld informiert wurde.

**Beispiele für negative Bewertung**

- Interaktive Elemente lösen bei Fokuserhalt eine unerwartete Kontextänderung aus, über die der Nutzende im Vorfeld nicht informiert wurde.

<a id="k-11-3-2-2"></a>

#### 11.3.2.2 Kontextänderung bei Eingabe

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#kontextänderung-bei-eingabe) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#kontextänderung-bei-eingabe) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularelemente vorhanden sind.

**Warum wird das geprüft?**

Unerwartete oder unangekündigte Kontextänderungen beeinträchtigen die Orientierung von Nutzenden. Sie lenken ab, verwirren, oder bleiben gerade bei Menschen mit Sehbeeinträchtigungen unbemerkt.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Inhalte einer nachfolgenden Auswahlliste werden dynamisch nach einer zuvor getroffenen Auswahl aktualisiert.
- Das Aktivieren einer Checkbox blendet neue Eingabefelder unterhalb ein.

**Beispiele für negative Bewertung**

- Das Aktivieren einer Checkbox blendet neue Eingabefelder unterhalb ein, die neue Auswahlliste wird fokussiert.
- Das Aktivieren einer Checkbox blendet neue Eingabefelder oberhalb ein.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#kontextänderung-bei-eingabe) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Bedienung von Formularelementen (beispielsweise Checkbox, Radio-Button, Auswahlliste) und die Eingabe von Inhalten in Textfelder (Eingabefelder) bei Tastaturbedienung und bei Tastaturbedienung mit aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise unerwartetes Öffnen neuer Ansichten, unerwartetes Ausblenden der Tastatur während Texteingabe, oder automatische Großschreibung).

Sinnvolle oder notwendige Kontextänderungen müssen unterhalb des auslösenden Elements erfolgen. Der Fokus darf dabei nicht versetzt werden.

Warnhinweise erscheinen als Antwort aufgrund von Eingaben der Nutzenden. Sofern Sie als Warnung in einem modalen Dialog implementiert wurden, sind sie nicht Bestandteil der Anforderung.

Eindeutige Kontextänderungen sind beispielsweise das Öffnen neuer Ansichten und das Einblenden neuer Inhalte auf derselben Ansicht, wenn diese Inhalte wie eine neue Seite wirken oder Inhalte in stark veränderter Anordnung erscheinen.

Reine Inhaltsänderungen gelten nicht als unerwartete Kontextänderungen bei Eingabe.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#kontextänderung-bei-eingabe) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die Bedienung von Formularelementen (beispielsweise Checkbox, Radio-Button, Auswahlliste) und die Eingabe von Inhalten in Textfelder (Eingabefelder) bei Bedienung mit einer externen Tastatur und bei Bedienung mit aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise unerwartetes Öffnen neuer Ansichten, unerwartetes Ausblenden der Tastatur während Texteingabe, oder automatische Großschreibung).

Sinnvolle oder notwendige Kontextänderungen müssen unterhalb des auslösenden Elements erfolgen. Der Fokus darf dabei nicht versetzt werden.

Warnhinweise erscheinen als Antwort aufgrund von Eingaben der Nutzenden. Sofern Sie als Warnung in einem modalen Dialog implementiert wurden, sind sie nicht Bestandteil der Anforderung.

Eindeutige Kontextänderungen sind beispielsweise das Öffnen neuer Ansichten und das Einblenden neuer Inhalte auf derselben Ansicht, wenn diese Inhalte wie eine neue Seite wirken oder Inhalte in stark veränderter Anordnung erscheinen.

Reine Inhaltsänderungen gelten nicht als unerwartete Kontextänderungen bei Eingabe.

<a id="k-11-3-2-6"></a>

#### 11.3.2.6 Konsistente Hilfe

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#konsistente-hilfe) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#konsistente-hilfe) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn auf mehreren Seiten eine Hilfefunktion angeboten wird. Die Hilfefunktion kann hierbei auf allen Seiten, oder nur auf Seiten, die ein gemeinsamer Zweck verbindet (beispielsweise ein Teilprozess wie ein mehrstufiger Warenkorb-Checkout), angeboten werden.

**Anforderung**

Wird eine seitenübergreifende Hilfefunktion angeboten, muss diese stets an der gleichen Position relativ zu den Inhalten angezeigt werden, es sei denn, der Nutzende verändert die Reihenfolge der Darstellung. Als Hilfefunktion gelten:

- Kontakt-Angaben (beispielsweise Telefonnummern, E-Mailadressen)
- Persönliche Kontaktaufnahme (beispielsweise Messenger-Dienste, Kontaktformulare, Social Media)
- Möglichkeiten zur Selbsthilfe (beispielsweise Frequently Asked Questions, Hilfe-Seiten)
- Automatisierte Kontaktmechanismen (beispielsweise Chat-Bots)

**Warum wird das geprüft?**

Die Positionierung von Hilfefunktionen an gleichbleibender Stelle unterstützt vor allem Menschen mit kognitiver Beeinträchtigung. Sie erleben weniger Ablenkung und werden so befähigt, sich einfacher auf die aktuelle Aufgabe zu konzentrieren. Zudem werden sie beim leichteren Auffinden der Hilfefunktionen unterstützt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Entsprechende Hilfefunktionen werden auf allen Seiten gleichbleibend an der gleichen Position angeboten.
- Entsprechende Hilfefunktionen werden nur auf einigen Seiten, die ein gemeinsamer Zweck verbindet, gleichbleibend an der gleichen Position angeboten.
- Die Taste zum Aktivieren des Hilfe-Chat-Bots findet sich stets an der unteren rechten Ecke des Anzeigebereichs.

**Beispiele für negative Bewertung**

- Entsprechende Hilfefunktionen werden auf entsprechenden Seiten an unterschiedlichen Positionen angeboten.
- Die Taste zum Aktivieren des Hilfe-Chat-Bots befindet auf der einen Seite in der unteren rechten Ecke, auf einer anderen Seite in der unteren linken Ecke des Anzeigebereichs.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#konsistente-hilfe) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#konsistente-hilfe) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-3-3-1"></a>

#### 11.3.3.1 Fehlererkennung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#fehlererkennung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fehlererkennung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularfelder vorhanden sind, die bei fehlerhafter oder unvollständiger Eingabe Fehlermeldungen erzeugen.

**Warum wird das geprüft?**

Für blinde Menschen und Menschen mit Farbfehlsichtigkeit ist eine Fehlerbeschreibung in Textform unerlässlich, um entsprechende Formularfelder identifizieren zu können. Menschen mit Legasthenie vertauschen häufiger Zahlen, Menschen mit motorischen Beeinträchtigungen drücken häufiger versehentlich falsche Tasten. Menschen, die nur einen kleinen Ausschnitt der Anzeige sehen, bemerken einen Fehlerzustand möglicherweise nicht.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der Fehler wird in Textform am Seitenanfang oder am Formularfeld beschrieben. Zusätzlich sind die entsprechenden Formularfelder in Form und Farbe markiert.
- Der Fehler wird in einem Warnhinweis dargestellt, der den Fehler und das betroffene Formularfeld eindeutig beschreibt.
- Die Beschriftung eines Formularfelds hat sich aufgrund des Fehlers verändert.
- Der Nutzende wird durch einen Fehlerdialog geführt, der den Fokus erhält.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#fehlererkennung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bei fehlerhafter oder unvollständiger Eingabe in Formularfeldern müssen die betroffenen Formularfelder eindeutig identifizierbar sein und der Fehler in Textform beschrieben werden, sofern eine Fehlererkennung implementiert ist. Das Identifizieren der betroffenen Formularfelder kann hierbei auf verschiedene Weise erfolgen:

- Die betroffenen Formularfelder werden am Seitenbeginn beschrieben.
- Die betroffenen Formularfelder werden zusätzlich zur Fehlerbeschreibung deutlich hervorgehoben (Form und Farbe).
- Die Beschriftung der betroffenen Formularfelder ändert sich.
- Die Fehlerbeschreibung wird in unmittelbarer Nähe des betroffenen Formularfelds angezeigt.
- Die Fehlerbeschreibung wird zusammen mit den betroffenen Formularfeldern in einem Warnhinweis angezeigt.

Ist keine Fehlererkennung implementiert, führt das nicht zu einer negativen Bewertung und der Prüfschritt ist nicht anwendbar.

**Beispiele für negative Bewertung**

- Das betroffene Formularfeld wird lediglich farblich markiert.
- Die Fehlermeldung in Textform lautet „Fehler im Formularfeld“, das Formularfeld wird nicht zusätzlich markiert.
- Nach Anzeige der Fehlermeldungen werden Formularfelder neu geladen, die Eingaben müssen erneut vorgenommen werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fehlererkennung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bei fehlerhafter oder unvollständiger Eingabe in Formularfeldern müssen die betroffenen Formularfelder eindeutig identifizierbar sein, die Fehlermeldungen müssen dauerhaft sichtbar sein und in Textform beschrieben werden. Das Identifizieren der betroffenen Formularfelder kann hierbei auf verschiedene Weise erfolgen:

- Die betroffenen Formularfelder werden am Seitenbeginn beschrieben.
- Die betroffenen Formularfelder werden zusätzlich zur Fehlerbeschreibung deutlich hervorgehoben (Form und Farbe).
- Die Beschriftung der betroffenen Formularfelder ändert sich.
- Die Fehlerbeschreibung wird in unmittelbarer Nähe des betroffenen Formularfelds angezeigt.
- Die Fehlerbeschreibung wird zusammen mit den betroffenen Formularfeldern in einem Warnhinweis angezeigt.

Ist keine Fehlererkennung implementiert, führt das nicht zu einer negativen Bewertung und der Prüfschritt ist nicht anwendbar.

**Beispiele für negative Bewertung**

- Die Fehlermeldung in Textform lautet „Fehler im Formularfeld“, das Formularfeld wird nicht zusätzlich markiert.
- Nach Anzeige der Fehlermeldungen werden Formularfelder neu geladen, die Eingaben müssen erneut vorgenommen werden.

<a id="k-11-3-3-2"></a>

#### 11.3.3.2 Beschriftungen oder Anweisungen bei Formularfeldern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#beschriftungen-oder-anweisungen-bei-formularfeldern) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#beschriftungen-oder-anweisungen-bei-formularfeldern) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularfelder vorhanden sind.

**Warum wird das geprüft?**

Für jeden Nutzenden ist es von Vorteil, wenn Formularfelder und die geforderten Eingabeformate am Formularfeld beschrieben werden, denn es ist aufwändig, ein Formular anhand der Fehlermeldungen zu korrigieren. Die geforderte Positionierung von Beschriftungen entspricht den üblichen Gestaltungsrichtlinien. Auch in ausschnitthaften Ansichten ist so einfach erkennbar, welche Beschriftung zu welchem Formularfeld gehört.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Am Anfang des Formulars werden alle Felder als Pflichtfelder deklariert.
- Alle Formularfelder sind beschriftet.
- Die Anweisung eines Datumsfeldes beschreibt das erwartete Format, beispielsweise „Geburtsdatum (TT.MM.JJJJ)“.
- Die Anweisung eines numerischen Feldes beschreibt den erwarteten Wertebereich, beispielsweise „Alter in Jahren (von 18 bis 35)“.
- Die Anweisung eines numerischen Feldes für eine Personalnummer beschreibt die erwartete Anzahl an Zeichen.

**Beispiele für negative Bewertung**

- Die Formularfelder sind nicht beschriftet.
- Nur einige Formularfelder sind beschriftet.
- Pflichtfelder werden durch ein Sternchen-Symbol gekennzeichnet, das nicht in einer Legende erklärt wird.
- Pflichtfelder werden durch ein Sternchen-Symbol gekennzeichnet, die Legende dazu befindet sich jedoch am Ende des Formulars.
- Die Beschriftung von Eingabefeldern erfolgt ausschließlich über die Vorbelegung des Formularfeldes.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#beschriftungen-oder-anweisungen-bei-formularfeldern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Beschriftungen, oder falls notwendig Anweisungen, werden für Formularfelder bereitgestellt. Die jeweilige Beschriftung oder Anweisung soll bei Eingabefeldern links neben dem Element oder darüber, bei Checkboxen und Radio-Buttons rechts neben dem Element lokalisiert sein.

Pflichtfelder sind als solche eindeutig gekennzeichnet, beispielsweise durch die zusätzliche Beschriftung „(Pflichtfeld)“. Werden Sie mit Symbolen gekennzeichnet, ist eine Legende zu Beginn des Formulars oder in der Dokumentation der Software verfügbar, die die Symbole erklärt.

Sind spezielle Formate bei der Eingabe notwendig, wird das entsprechende Format vor dem Formularelement oder in der Vorbelegung des Formularelements beschrieben.

Eindeutig identifizierbare Formularfelder wie beispielsweise ein Suchfeld, das aus einem Eingabefeld und der Suchentaste besteht, benötigen unter Umständen keine eigene visuelle Beschriftung, da der Einsatzzweck aus dem Kontext ermittelt werden kann (beispielsweise durch Verwendung eines Lupe-Symbols als Taste). Sie müssen jedoch zu jederzeit programmatisch ermittelbar sein.

Die Inhalte von Placeholder-Texten in Formularelementen gelten nicht als Beschriftung.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#beschriftungen-oder-anweisungen-bei-formularfeldern) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Beschriftungen, oder falls notwendig Anweisungen, werden für Formularfelder bereitgestellt. Die jeweilige Beschriftung oder Anweisung soll bei Eingabefeldern links neben dem Element oder darüber, bei Checkboxen und Radio-Buttons rechts neben dem Element lokalisiert sein.

Pflichtfelder sind als solche eindeutig gekennzeichnet, beispielsweise durch die zusätzliche Beschriftung „(Pflichtfeld)“. Werden Sie mit Symbolen gekennzeichnet, ist eine Legende zu Beginn des Formulars verfügbar, die die Symbole erklärt.

Sind spezielle Formate bei der Eingabe notwendig, wird das entsprechende Format vor dem Formularelement oder in der Vorbelegung des Formularelements beschrieben.

Eindeutig identifizierbare Formularfelder wie beispielsweise ein Suchfeld, das aus einem Eingabefeld und der Suchentaste besteht, benötigen unter Umständen keine eigene visuelle Beschriftung, da der Einsatzzweck aus dem Kontext ermittelt werden kann (beispielsweise durch Verwendung eines Lupe-Symbols als Taste). Sie müssen jedoch zu jederzeit programmatisch ermittelbar sein.

Die Inhalte von Placeholder-Texten in Formularelementen gelten nicht als Beschriftung.

<a id="k-11-3-3-3"></a>

#### 11.3.3.3 Fehlerkorrektur

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#fehlerkorrektur) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fehlerkorrektur) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularfelder vorhanden sind, die bei fehlerhafter oder unvollständiger Eingabe Fehlermeldungen erzeugen.

**Anforderung**

Sofern eine Fehlerkennung bei Formularfeldern implementiert ist, müssen die Fehlermeldungen eindeutig sein und Korrekturvorschläge zur Fehlerbehebung benannt werden, es sei denn, dies würde die Sicherheit oder den Zweck des Inhalts gefährden.

**Warum wird das geprüft?**

Sofern bereits eine Fehlererkennung implementiert ist, soll sie die Nutzenden optimal unterstützen. Nutzende können Fehler leichter beheben, wenn konkrete Hinweise zur Fehlerbehebung verfügbar sind.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für negative Bewertung**

- Die Beschreibung der Fehler geben keine konkreten Hinweise zur Behebung des Eingabefehlers, beispielsweise lautet die Fehlermeldung lediglich „Fehler im Formularfeld“.
- Nach Anzeige der Fehlermeldungen werden Formularfelder neu geladen, die Eingaben müssen erneut vorgenommen werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#fehlerkorrektur) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Beispiele für positive Bewertung**

- Die Beschreibung der Fehler gibt konkrete Hinweise zur Behebung des Eingabefehlers, beispielsweise durch das Benennen der geforderten Formate.
- Die Fehlermeldung lautet z.B. „Nachname: Eine Eingabe ist erforderlich“.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fehlerkorrektur) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Beispiele für positive Bewertung**

- Die Beschreibung der Fehler gibt konkrete Hinweise zur Behebung des Eingabefehlers, beispielsweise durch das Benennen der geforderten Formate.

<a id="k-11-3-3-4"></a>

#### 11.3.3.4 Fehlervermeidung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#fehlervermeidung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#fehlervermeidung) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Überprüfung und Korrektur von sensiblen Daten sind für jeden Nutzenden wichtig. Bei Menschen mit Beeinträchtigung im Speziellen ist das Risiko von Fehleingaben allerdings deutlich erhöht. Menschen mit Legasthenie vertauschen häufiger Zahlen, Menschen mit motorischen Beeinträchtigungen drücken häufiger versehentlich falsche Tasten. Menschen, die nur einen kleinen Ausschnitt der Anzeige sehen, bemerken eine Fehleingabe möglicherweise nicht.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der betroffene Prozess erfüllt mindestens eine der genannten Anforderungen.

**Beispiele für negative Bewertung**

- Der betroffene Prozess erfüllt keine der genannten Anforderungen.
- Anstatt einer Übersicht der erfassten Daten ist lediglich eine Checkbox verfügbar, über die die Richtigkeit der erfassten Daten bestätigt werden muss.
- In einer Übersicht werden die erfassten Daten angezeigt, es gibt jedoch keine Möglichkeit, an dieser Stelle Korrekturen vorzunehmen.
- Es erfolgt keine Bestätigung nach der erfolgreichen Übermittlung.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#fehlervermeidung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn über entsprechende Prozesse rechtlich bindende Verpflichtungen eingegangen, finanzielle Transaktionen ausgelöst, oder sensible Nutzerdaten übermittelt werden.

**Anforderung**

Beinhaltet die Software Prozesse, über die rechtlich bindende Verpflichtungen eingegangen, finanzielle Transaktionen ausgelöst, oder sensible Nutzerdaten übermittelt werden, so muss mindestens eine der folgenden Anforderungen erfüllt sein:

- Vorgang kann rückgängig gemacht werden Das Übermitteln der Daten kann rückgängig gemacht werden.

- Daten werden vor Übermittlung überprüft und bestätigt Vor Übermittlung der Daten können die Eingaben geprüft und korrigiert werden. Nach einer Bestätigung werden die Daten übermittelt.
- Übermittlung nach Bestätigung Die Übermittlung der Daten erfolgt erst nach Bestätigung eines Dialogs, der die Folgen der Übermittlung beschreibt.

Erfolgreiche Übermittlungen sollten nach der Übertragung bestätigt werden.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#fehlervermeidung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn über entsprechende Prozesse rechtlich bindende Verpflichtungen eingegangen, finanzielle Transaktionen ausgelöst, oder sensible Daten von Nutzenden übermittelt werden.

**Anforderung**

Beinhaltet die Mobile App Prozesse, über die rechtlich bindende Verpflichtungen eingegangen, finanzielle Transaktionen ausgelöst, oder sensible Nutzerdaten übermittelt werden, so muss mindestens eine der folgenden Anforderungen erfüllt sein:

- Vorgang kann rückgängig gemacht werden
 Das Übermitteln der Daten kann rückgängig gemacht werden.

- Daten werden vor Übermittlung überprüft und bestätigt
 Vor Übermittlung der Daten können die Eingaben geprüft und korrigiert werden. Nach einer Bestätigung werden die Daten übermittelt.
- Übermittlung nach Bestätigung
 Die Übermittlung der Daten erfolgt erst nach Bestätigung eines Dialogs, der die Folgen der Übermittlung beschreibt.

Erfolgreiche Übermittlungen sollten nach der Übertragung bestätigt werden.

<a id="k-11-3-3-7"></a>

#### 11.3.3.7 Redundanter Eintrag

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#redundanter-eintrag) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#redundanter-eintrag) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularfelder vorhanden sind.

**Anforderung**

Müssen innerhalb eines Prozesses bestimmte, bereits zuvor eingegebene Daten eines Nutzenden erneut eingegeben werden, muss der Nutzende bei der erneuten Eingabe unterstützt werden. Als Unterstützung zählen:

- Die Daten werden bei der Eingabe als Ergänzungsvorschlag angeboten
- Es wird eine Funktion zum automatischen Ausfüllen bereitgestellt
- Die Daten können innerhalb der gleichen Seite kopiert und eingefügt werden

Daten, deren erneute Eingabe unerlässlich oder sicherheitsrelevant ist, oder falls die vorherige Eingabe nicht mehr gültig ist, sind hiervon ausgenommen.

**Warum wird das geprüft?**

Vermeidung von wiederholter Eingabe durch Nutzende unterstützt Menschen mit kognitiver Beeinträchtigung bei der Vermeidung von Fehleingaben. Sie werden befähigt, sich einfacher auf die aktuelle Aufgabe zu konzentrieren. Auch werden Menschen mit motorischen Beeinträchtigungen, die beispielsweise eine Spracheingabe verwenden, entlastet.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Für entsprechende Formularfelder wird mindestens eine der Eingabehilfen angeboten.
- Auf Seite 2 eines Registrierungsprozesses kann der Wert des Eingabefelds für Nachname vom Wert des Eingabefelds für Nachname auf Seite 1 über das Aktivieren eines Kontrollkästchens automatisch übernommen werden.

**Beispiele für negative Bewertung**

- Für entsprechende Formularfelder wird keine der Eingabehilfen angeboten.
- Ein Registrierungsprozess fordert mehrmals die Eingabe des Nachnamens, bietet aber keine Eingabeunterstützung an.
- Zum Kopieren des Inhalts des Formularfelds Nachname muss auf eine vorherige Seite gewechselt werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#redundanter-eintrag) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#redundanter-eintrag) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-3-3-8"></a>

#### 11.3.3.8 Zugängliche Authentifizierung (Minimum)

*Übergeordneter Prüfschritt. Die Inhalte stehen in den folgenden Unterschritten.*

<a id="k-11-3-3-8a"></a>

#### 11.3.3.8a Zugänglicher Anmeldeprozess

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#a-zugänglicher-anmeldeprozess) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#a-zugänglicher-anmeldeprozess) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Der Anmeldeprozess kann ohne Einsatz kognitiver Fähigkeiten (beispielsweise das Merken eines Passworts, oder das Lösen eines Rätsels) durchgeführt werden, es sei denn, es wird mindestens eine der folgenden Möglichkeiten angeboten:

- Einfügen des Passworts durch Kopieren und Einfügen von Text
- Automatisches Einfügen des Passworts über den Passwortmanager des Betriebssystems
- Anmeldung über einen Drittanbieterdienst (beispielsweise Anmeldung mit Apple-ID oder Google-Account)
- Alternative Anmeldung über ein biometrisches Merkmal (beispielsweise Touch-ID oder Face-ID)
- Alternative Anmeldung über das Scannen eines QR-Codes

Es ist nicht ausreichend, wenn zur Anmeldung ausschließlich die Option zum Scannen eines QR-Codes zur Verfügung steht.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Es werden eine oder mehrere der Möglichkeiten im Anmeldeprozess unterstützt.
- Die Passworteingabe für vom Passwortmanager des Betriebssystems übernommen.
- Statt einer Passworteingabe kann Face-ID verwendet werden.

**Beispiele für negative Bewertung**

- Das Passwort kann ausschließlich manuell in das dafür vorgesehene Eingabefeld eingegeben werden. Es gibt keinerlei Unterstützungsfunktionen oder Alternativen.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#a-zugänglicher-anmeldeprozess) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Software eine Anmeldung voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

**Warum wird das geprüft?**

Nutzende benötigen eine einfache und zugängliche, und zugleich sichere Methode, um sich in einer Software anmelden zu können. Passworteingaben, die die Nutzung kognitiver Fähigkeiten ohne entsprechende Alternativen vorsehen, stellen für manche Nutzende eine Barriere oder sogar eine Blockade dar.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#a-zugänglicher-anmeldeprozess) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Mobile App eine Anmeldung voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

**Warum wird das geprüft?**

Nutzende benötigen eine einfache und zugängliche, und zugleich sichere Methode, um sich in einer Mobile App anmelden zu können. Passworteingaben, die die Nutzung kognitiver Fähigkeiten ohne entsprechende Alternativen vorsehen, stellen für manche Nutzende eine Barriere oder sogar eine Blockade dar.

<a id="k-11-3-3-8b"></a>

#### 11.3.3.8b Zugängliche Zwei-Faktor-Authentifizierung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#b-zugängliche-zwei-faktor-authentifizierung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#b-zugängliche-zwei-faktor-authentifizierung) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Verwendet der Anmeldeprozess eine Zwei-Faktor-Authentifizierung und versendet im Rahmen der Anmeldung einen Verifizierungscode direkt an das genutzte Endgerät, muss der Verifizierungscode über die Kopieren- und Einfügeoption des Betriebssystems, oder über Komfortfunktionen des Betriebssystems in das Eingabefeld der Anmeldung übertragen werden können.

Zwei-Faktor-Authentifizierungen, die ausschließlich über ein zweites Endgerät durchgeführt werden können, sind von dieser Anforderung ausgenommen.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Ein Verifizierungscode, der als E-Mail auf das Endgerät geschickt wurde, kann über Kopieren und Einfügen direkt in das vorgesehene Eingabefeld übernommen werden
- Ein Verifizierungscode, der als SMS auf das Endgerät geschickt wurde, kann über die Komfortfunktion des Betriebssystems direkt in das vorgesehene Eingabefeld übernommen werden.

**Beispiele für negative Bewertung**

- Ein Verifizierungscode muss manuell in das Eingabefeld eingetragen werden, obwohl er an das gleiche Endgerät versendet wurde, da das Eingabefeld das direkte Einfügen verweigert.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#b-zugängliche-zwei-faktor-authentifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Software eine Anmeldung mittels Zwei-Faktor-Authentifizierung voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#b-zugängliche-zwei-faktor-authentifizierung) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Mobile App eine Anmeldung mittels Zwei-Faktor-Authentifizierung voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

<a id="k-11-3-3-8c"></a>

#### 11.3.3.8c Zugängliche CAPTCHAs in Anmeldeprozessen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#c-zugängliche-captchas-in-anmeldeprozessen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#c-zugängliche-captchas-in-anmeldeprozessen) |

##### Gleichlautend in beiden Verfahren

**Anforderung**

Verwendet der Anmeldeprozess die Lösung eines CAPTCHAs, kann das CAPTCHA auch ohne Einsatz kognitiver Fähigkeiten durchgeführt werden.

CAPTCHAs, die den Einsatz kognitiver Fähigkeiten, sogenannte kognitive Funktionstests voraussetzen, erfordern logisches Schlussfolgern, sowie Fähigkeiten zum sprachlichen, numerischen und figural-bildhaften Denken. Zusätzlich kann spezielles Wissen, wie etwa Sprachkompetenzen und technisches Verständnis, abgefragt werden:

- Merken und Wiedergeben von Buchstaben, Zeichen oder Mustern
- Manuelles Abschreiben von Buchstaben und Zeichen
- Prüfung der korrekten Rechtschreibung
- Lösen von Rechenaufgaben
- Lösen von Rätseln

Bei kognitiven Funktionstests handelt es sich explizit nicht um:

- Erkennen von Bildern, Audio oder Video (beispielsweise Markieren aller PKWs, Markieren aller Bilder mit Ampeln)
- Methoden zur Zwei-Faktor-Authentifizierung

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Zusätzlich zu einem kognitiven Funktionstest wird ein vollautomatisches CAPTCHA bereitgestellt (beispielsweise das Markieren von Bildern als Alternative zu einer Rechenaufgabe).
- Es wird ausschließlich ein vollautomatisches CAPTCHA bereitgestellt (beispielsweise nur das Markieren von Bildern).

**Beispiele für negative Bewertung**

- Es wird ausschließlich ein kognitiver Funktionstest bereitgestellt (beispielsweise eine Prüfung auf korrekte Rechtschreibung).

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#c-zugängliche-captchas-in-anmeldeprozessen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Software eine Anmeldung mittels eines CAPTCHAs voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#c-zugängliche-captchas-in-anmeldeprozessen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn die vollständige Nutzung der Mobile App eine Anmeldung mittels eines CAPTCHAs voraussetzt. Der Prüfschritt bezieht sich hierbei nicht auf Registrierungsprozesse.

### Kapitel: Robustheit

<a id="k-11-4-1-1"></a>

#### 11.4.1.1 Syntaxanalyse

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#syntaxanalyse) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#syntaxanalyse) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#syntaxanalyse) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar, kann aber nicht detailliert bewertet werden. Eventuell auf fehlerhaften Code zurückführbares Fehlerverhalten sollte als Hinweis dokumentiert werden. Ob solche Fehler auf einen falschen oder fehlerhaften Code zurückzuführen sind, kann aber nicht bewertet werden. Fehler im Code werden bereits in anderen Prüfschritten deutlich.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#syntaxanalyse) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist durch einen Blackbox-Test nicht prüfbar (Stand Januar 2024). Eventuell auf fehlerhaften Code zurückführbares Fehlerverhalten sollte als Hinweis dokumentiert werden. Ob solche Fehler auf einen falschen oder fehlerhaften Code zurückzuführen sind, kann aber nicht bewertet werden.

**Bewertungsalternativen:** Nicht anwendbar · Nicht getestet

<a id="k-11-4-1-2"></a>

#### 11.4.1.2 Name, Rolle, Wert

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#name-rolle-wert) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#name-rolle-wert) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#name-rolle-wert) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt wird über die Prüfschritte 11.5.2.5 Objektinformationen, 11.5.2.7 Werte, 11.5.2.15 Änderungsbenachrichtigung und 11.5.2.16 Änderungen von Zuständen und Eigenschaften abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Sofern einer der genannten Prüfschritte als nicht voll erfüllt gewertet wird, wird auch dieser Prüfschritt entsprechend gewertet, oder dieser Prüfschritt erhält die Bewertung „Nicht getestet“.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht getestet

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#name-rolle-wert) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt wird über die Prüfschritte 11.5.2.5 Objektinformationen, 11.5.2.7 Werte, 11.5.2.15 Änderungsbenachrichtigung und 11.5.2.16 Änderungen von Zuständen und Eigenschaften abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Der Prüfschritt übernimmt das negativste Bewertungsresultat aus den zuvor referenzierten Prüfschritten. Sofern alle referenzierten Prüfschritte mit „nicht anwendbar“ bewertet wurden, gilt der Prüfschritt als „nicht anwendbar“.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

<a id="k-11-4-1-3"></a>

#### 11.4.1.3 Statusmeldungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#statusmeldungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#statusmeldungen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Statusmeldungen oder ähnliche Aktualisierungen dynamisch generiert werden.

**Warum wird das geprüft?**

Dynamisch erzeugte Statusmeldungen erscheinen oftmals ungeplant oder nur vorübergehend. Nutzende von assistiven Technologien benötigen grundsätzlich die Information der Statusmeldung, sollen dadurch jedoch nicht die Orientierung oder den aktuellen verlieren.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Statusmeldungen werden automatisch ausgegeben und erhalten dabei nicht den Fokus (beispielsweise nach Hinzufügen eines Artikels in den Warenkorb, nach Hinzufügen eines Produkts zu einer Merkliste, bei Fehlermeldungen in Formularen, nach Abschicken eines Formulars oder bei einem Ladebalken eines Ladeprozesses).

**Beispiele für negative Bewertung**

- Statusmeldungen werden nicht, oder nur nach manueller Fokussierung vom Screenreader ausgegeben.
- Statusmeldungen werden ausgegeben, die Meldung wird hierbei jedoch fokussiert.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#statusmeldungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Dynamische Aktualisierungen des Inhalts, die keine Kontextänderung darstellen und dem Nutzenden Informationen über beispielsweise den Status einer Aktion, den Fortschritt eines Prozesses oder das Erkennen von Fehlern bereitstellen, müssen programmatisch anhand geeigneter Rollen oder Eigenschaften durch assistive Technologien ermittelt werden können, ohne explizit den Fokus zu erhalten. Die Benachrichtigung zu solchen Ereignissen muss automatisch an das assistive System übermittelt und ausgegeben werden.

Kontextänderungen, beispielsweise neue Ansichten, neue Interaktionsmöglichkeiten; neue Inhalte auf einer Ansicht, oder Hinweise über Dialoge, werden durch assistive Technologien durch aktive Fokussierung wahrgenommen und sind hiervon nicht betroffen.

Statusmeldungen, die durch das Betriebssystem selbst generiert ausgegeben werden, sind hiervon nicht betroffen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#statusmeldungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Dynamische Aktualisierungen des Inhalts, die keine Kontextänderung darstellen und dem Nutzenden Informationen über beispielsweise den Status einer Aktion, den Fortschritt eines Prozesses oder das Erkennen von Fehlern bereitstellen, müssen programmatisch anhand geeigneter Rollen oder Eigenschaften durch assistive Technologien ermittelt werden können, ohne explizit den Fokus zu erhalten. Die Benachrichtigung zu solchen Ereignissen muss automatisch an das assistive System übermittelt und ausgegeben werden.

Kontextänderungen, beispielsweise neue Ansichten, neue Interaktionsmöglichkeiten; neue Inhalte auf einer Ansicht, oder Hinweise über Dialoge, werden durch assistive Technologien durch aktive Fokussierung wahrgenommen und sind hiervon nicht betroffen.

Statusmeldungen oder Push-Mitteilungen, die durch das Betriebssystem selbst generiert ausgegeben werden, sind hiervon nicht betroffen.

### Kapitel: Interoperabilität mit assistiven Technologien

<a id="k-11-5-2-3"></a>

#### 11.5.2.3 Verwendung von Barrierefreiheitsdiensten

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#verwendung-von-barrierefreiheitsdiensten) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#verwendung-von-barrierefreiheitsdiensten) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

**Beispiele für positive Bewertung**

- Die Bedienungshilfen werden unterstützt.

**Beispiele für negative Bewertung**

- Mindestens eine der Bedienungshilfen wird nicht unterstützt.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#verwendung-von-barrierefreiheitsdiensten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Assistive Systeme werden durch die Software unterstützt, indem sie mindestens die Anforderungen in den Prüfschritten 11.5.2.5 bis 11.5.2.17 bedient. Im Fokus steht die generelle Unterstützung der gängigen assistiven Systeme Screenreader, Bildschirmlupe und Sprachsteuerung, gegebenenfalls zusätzlich in Verbindung mit einer Tastatur. Die Detailbetrachtung erfolgt in den nachfolgenden Anforderungen.

**Warum wird das geprüft?**

Die Unterstützung der assistiven Systeme stellt sicher, dass digitale Inhalte für eine breite Palette von Nutzenden zugänglich und nutzbar sind, unabhängig von deren individuellen Fähigkeiten oder Einschränkungen. Das Setzen auf Standards ermöglicht eine Bedienung der Produkte in einem gewohnten Umfeld.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#verwendung-von-barrierefreiheitsdiensten) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Die assistiven Systeme des jeweiligen Betriebssystems werden durch die Mobile App unterstützt, indem sie mindestens die Anforderungen in den Prüfschritten 11.5.2.5 bis 11.5.2.17 bedient. Im Fokus steht die generelle Unterstützung der gängigen assistiven Systeme Screenreader, Bildschirmlupe und Sprachsteuerung, gegebenenfalls zusätzlich in Verbindung mit einer externen Tastatur. Die Detailbetrachtung erfolgt in den nachfolgenden Anforderungen.

**Warum wird das geprüft?**

Die Unterstützung der assistiven Systeme der Betriebssysteme stellt sicher, dass digitale Inhalte für eine breite Palette von Nutzenden zugänglich und nutzbar sind, unabhängig von deren individuellen Fähigkeiten oder Einschränkungen. Das Setzen auf Standards ermöglicht eine Bedienung der Produkte in einem gewohnten Umfeld.

<a id="k-11-5-2-5"></a>

#### 11.5.2.5 Objektinformationen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#objektinformationen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#objektinformationen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Anforderung**

Interaktive Bedienelemente müssen ihren Namen, ihre Rolle, und gegebenenfalls vorhandene Grenzen oder Beschreibungen korrekt an die assistiven Systeme übermitteln und programmatisch ermittelbar sein. Die programmatische Beschriftung muss die sichtbare Beschriftung mindestens zu Beginn der Beschriftung beinhalten. Idealerweise sind die Beschriftungen in beiden Varianten analog. Dies betrifft beispielsweise:

- Tasten
- Tasten mit eingebetteter Grafik
- Umschalttasten
- Kontrollkästchen
- Optionsfeld
- Registerkarten
- Ausklapplisten
- Formularfelder
- Links
- Links mit eingebetteter Grafik

**Warum wird das geprüft?**

Das Identifizieren und die damit verbundene, korrekte Implementierung von Elementtypen und zugehörigen Kontextinformationen ist gerade für Nutzende von einem Screenreader, als auch für Nutzende von Sprachsteuerung essenziell, denn nur so können die Nutzenden das jeweilige Element im Kontext zuordnen, verstehen und bedienen. Sind Informationen am Element unvollständig oder fehlerhaft, oder weicht die programmatische Beschriftung von der visuellen Beschriftung ab, werden Verständlichkeit, Wahrnehmbarkeit und Bedienbarkeit erheblich eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Interaktive Elemente geben einen zutreffenden Namen und eine zutreffende Rolle korrekt und vollumfänglich wieder.
- Ein Kontrollkästchen wird als Checkbox, Kontrollfeld oder ähnlich wiedergegeben.
- Ein Optionsfeld wird als Auswahlschalter, Radio-Button, oder ähnlich wiedergegeben.
- Ein Eingabefeld wird als Inputfeld, Textfeld oder ähnlich wiedergegeben.
- Ein Dropdown-Listenfeld wird als Ausklappliste, Auswahlliste, Dropdown-Element, Klappliste oder ähnlich wiedergegeben.
- Eine Register einer Registerkartengruppe wird als Tab, Register, Registerkarte oder ähnlich wiedergegeben.
- Ein Umschalter wird als Umschalttaste; Toggle, Switch oder ähnlich wiedergegeben.
- Eine Schaltfläche wird als Schalter, Taste, Button oder ähnlich wiedergegeben.
- Ein Eingabefeld gibt Namen, Rolle und eine Beschreibung zur erwarteten Eingabe wieder.
- Ein Eingabefeld, dass auf eine bestimmte Anzahl an Zeichen begrenzt ist, gibt die erwartete Zeichenanzahl als Grenze wieder.
- Entsprechende Eingabefelder geben notwendige Kontextinformationen zu ihren Grenzen wieder (beispielsweise verbotene Zeichen bei der Eingabe einer E-Mailadresse).
- Ein Eingabefeld gibt wieder, welche Art der Eingabe es erwartet (beispielsweise nur Ziffern bei einem Feld für eine Telefonnummer).
- Die programmatische Beschriftung eines Bedienelements ist analog zur sichtbaren Beschriftung.
- Die programmatische Beschriftung eines Bedienelements beinhaltet die sichtbare Beschriftung.
- Eine verlinkte Grafik enthält das Ziel des Links.
- Ein Diagramm informiert im Alternativtext über den Titel des Diagramms und weist darauf hin, dass eine alternative Ansicht über einen Doppeltipp auf das Diagramm aktiviert werden kann.
- Ein News-Ticker übermittelt die jeweiligen relevanten Informationen.
- Eine Grafik, die die Funktion einer Taste übernimmt, übermittelt die auszulösende Funktion.
- Ein Home-Piktogramm übermittelt „Zurück zur Startseite“.
- Ein Lupen-Piktogramm übermittelt „Suchen”.
- Ein verlinktes Bild, dessen Inhalt zusätzliche Information enthält, übermittelt Bildinhalt und Ziel.

**Beispiele für negative Bewertung**

- Interaktive Elemente geben ihren Namen oder ihre Rolle nicht korrekt oder nicht vollumfänglich wieder.
- Ein Link wird als Taste, Schalter oder ähnlich wiedergegeben.
- Eine Auswahlliste wird als Schaltfläche oder ähnlich wiedergegeben.
- Ein Eingabefeld gibt seine Rolle korrekt wieder, übermittelt jedoch keinen Namen.
- Ein Eingabefeld, das auf eine bestimmte Anzahl an Zeichen begrenzt ist, gibt die erwartete Zeichenanzahl als Grenze nicht wieder.
- Ein Eingabefeld, dass nur gewisse Zeichen oder Ziffern zulässt, gibt die erlaubten Zeichen oder Ziffern nicht wieder.
- Die programmatische Beschriftung eines Bedienelements beinhaltet nicht die sichtbare Beschriftung.
- Die programmatische Beschriftung eines Bedienelements beinhaltet die sichtbare Beschriftung nicht zu Beginn der Beschriftung.
- Entsprechende Objekte übermitteln keine oder keine geeignete Information.
- Entsprechende Objekte übermitteln den Namen des Elements.
- Der Alternativtext entsprechender Objekte ist zu lang.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#objektinformationen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#objektinformationen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-6"></a>

#### 11.5.2.6 Datentabellen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#datentabellen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#datentabellen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Datentabellen oder eine visuelle Strukturierung von Informationen in Tabellenform vorhanden sind.

**Anforderung**

Der Aufbau von Tabellen ist strukturell korrekt und programmatisch ermittelbar. Die Zuordnung jeder Zelle zu ihrer zugehörigen Zeile und Spalte, der Umfang an Zeilen und Spalten, als auch der Anfang und das Ende der Tabelle sind identifizierbar. Falls vorhanden, werden die Überschriften von Zeile und Spalte wiedergegeben.

**Warum wird das geprüft?**

Screenreadernutzende und Menschen mit kognitiver Beeinträchtigung benötigen zur Orientierung in Tabellen besonderes Abstraktionsvermögen. Sie benötigen zu jeder Datenzelle Informationen zu Kontext und Bedeutung. Sind diese Informationen nicht vorhanden, ist die Bedeutung der erreichten Datenzelle unter Umständen nicht verständlich.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Alle Informationen sind programmatisch korrekt ermittelbar.
- Die Überschrift wird bei der ersten Fokussierung einer Zelle korrekt wiedergegeben. Bei weiterer Bewegung durch die Tabelle wird die Wiedergabe gleichlautender Überschriften unterdrückt.
- Lediglich der Anfang oder das Ende einer Tabelle sind nicht korrekt ermittelbar.

**Beispiele für negative Bewertung**

- Einige Informationen sind programmatisch nicht korrekt ermittelbar.
- Überschriften sind vorhanden, werden aber nicht als solche ermittelt.
- Es sind keine Überschriften für Zeilen und Kopfspalten vorhanden, obwohl die Daten nicht direkt verständlich sind.
- Die Daten verändern ihre Bedeutung, ohne dass dies durch neue Überschriften deutlich gemacht wird.
- Eine inhaltliche Änderung der Daten wird ausschließlich durch leere Tabellenzeilen verdeutlicht.
- Eine Tabelle wird ausschließlich für Layout oder Design verwendet.
- Die Tabelle ist nicht zellenweise navigierbar.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#datentabellen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#datentabellen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-7"></a>

#### 11.5.2.7 Werte

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#werte) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#werte) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind, die Werte aufweisen.

**Anforderung**

Interaktive Bedienelemente, die Werte aufweisen, übermitteln den aktuellen Wert und gegebenenfalls die Position oder die entsprechenden Grenzen korrekt an die assistiven Systeme. Dies betrifft beispielsweise:

- Umschalttasten (aktiviert, nicht aktiviert)
- Kontrollkästchen (ausgewählt, nicht ausgewählt)
- Auswahlschalter (aktiviert, nicht aktiviert)
- Registerkarten (ausgewählt, nicht ausgewählt)
- Ausklapplisten (erweitert, nicht erweitert)
- Formularelemente (Wert des Elements, beispielsweise eingetragener Text)
- Optionen in Ausklapplisten (Eintrag x von y)

Der aktuelle Wert eines entsprechenden Elements wird bei Fokussierung des Elements mit aktivem Screenreader proaktiv wiedergegeben.

**Warum wird das geprüft?**

Das Identifizieren und die damit verbundene, korrekte Implementierung von Werten ist gerade für Screenreadernutzende essenziell, denn nur so können die Nutzenden das jeweilige Element im Kontext zuordnen, verstehen und bedienen. Sind Informationen am Element unvollständig oder fehlerhaft, werden Verständlichkeit und Wahrnehmbarkeit erheblich eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Interaktive Elemente geben ihren Wert bei Fokussierung korrekt und vollumfänglich wieder.

**Beispiele für negative Bewertung**

- Interaktive Elemente geben ihren Wert bei Fokussierung nicht, nicht korrekt oder nicht vollumfänglich wieder.
- Die Optionen einer Auswahlliste geben ihre Position oder Grenze nicht wieder (beispielsweise 3 von 5).
- Eine Registerkarte gibt nicht wieder, ob sie aktuell ausgewählt oder nicht ausgewählt ist.
- Bei mehreren Registerkarten gibt die jeweilige Registerkarte ihre Position oder Grenze nicht wieder (beispielsweise 2 von 6).
- Ein Eingabefeld gibt den vorbelegten Text nicht wieder.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#werte) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#werte) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-8"></a>

#### 11.5.2.8 Label-Beziehungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#label-beziehungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#label-beziehungen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularfelder oder interaktive Elemente vorhanden sind, die zum logischen Verständnis einen Bezug zu einer anderen Beschriftung oder einem anderen Inhalt auf der Benutzeroberfläche benötigen.

**Anforderung**

Formularfelder oder interaktive Elemente, die durch ihre Beschriftung im Kontext nicht direkt oder eindeutig identifizierbar sind und indirekt oder zusätzlich durch ein anderes Element gelabelt werden, oder ein Label für ein anderes Element sind, übermitteln diese verknüpfenden Kontextinformationen an die assistiven Systeme, um den logischen Bezug auch programmatisch ermittelbar nachvollziehen zu können.

**Warum wird das geprüft?**

Label-Beschriftungen erhöhen die Verständlichkeit entsprechender Elemente. Ein intelligentes Arrangement von Label-Beziehungen zu Formular- und Bedienelementen stellt sicher, dass die Elemente der Benutzeroberfläche für Hilfsmitteltechnologien ebenso verständlich und effizient nutzbar sind wie bei der Benutzung ohne Hilfsmittel.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Formularfelder oder interaktive Elemente, die durch ihre Beschriftung nicht eindeutig identifizierbar sind, geben die notwendigen Kontextinformationen wieder, um sie eindeutig zu identifizieren.
- Formularfelder, die in Gruppen angeordnet sind, geben das Gruppenpräfix beziehungsweise die Gruppenüberschrift wieder (beispielsweise Lieferadresse für die Eingabefelder einer Lieferadresse, und Rechnungsadresse für die Eingabe einer Rechnungsadresse).
- Auswahlfelder einer Umfrage, die „Ja“ und „Nein“ lauten, geben die entsprechende Frage, der sie zugeordnet sind, wieder.

**Beispiele für negative Bewertung**

- Formularfelder oder interaktive Elemente, die durch ihre Beschriftung nicht eindeutig identifizierbar sind, geben keine notwendigen Kontextinformationen wieder, um sie eindeutig zu identifizieren.
- Formularfelder, die in Gruppen angeordnet sind, geben das Gruppenpräfix beziehungsweise die Gruppenüberschrift nicht wieder.
- Auswahlfelder einer Umfrage, die „Ja“ und „Nein“ lauten, geben die entsprechende Frage, der sie zugeordnet sind, nicht wieder.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#label-beziehungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#label-beziehungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-9"></a>

#### 11.5.2.9 Eltern-Kind-Beziehungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#eltern-kind-beziehungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#eltern-kind-beziehungen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Elemente vorhanden sind, die ihnen übergeordnete oder untergeordnete Elemente beinhalten.

**Anforderung**

Beziehungen zwischen Elementen und ihnen übergeordneten oder untergeordneten Elementen, die zum logischen Verständnis des jeweiligen Elements notwendig sind, werden an die assistiven Systeme übermittelt und vom Screenreader wiedergegeben.

**Warum wird das geprüft?**

Visuell identifizierbare Hierarchien und Zusammengehörigkeiten von Elementen sind für Screenreadernutzende nicht zugänglich. Diese Beziehungen müssen auch programmatisch ermittelbar sein.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die zum Verständnis notwendige Beziehung zwischen einem Element und einem ihm übergeordneten oder untergeordneten Element wird durch den Screenreader eindeutig wiedergegeben.
- In einer Gruppe von Auswahlfeldern, die von der jeweiligen Auswahl des vorherigen Elements abhängen, wird der Bezug zum vorherigen Element eindeutig hergestellt und vom Screenreader wiedergegeben.
- In einem Kalender werden die einzelnen Tage eindeutig dem jeweiligen Monat zugeordnet und vom Screenreader ausgegeben.
- Die Formularfelder zur Eingabe einer Lieferadresse können eindeutig der Erfassung einer Lieferadresse zugeordnet werden.

**Beispiele für negative Bewertung**

- Die zum Verständnis notwendige Beziehung zwischen einem Element und einem ihm übergeordneten oder untergeordneten Element wird durch den Screenreader nicht eindeutig wiedergegeben.
- In einer Gruppe von Auswahlfeldern, die von der jeweiligen Auswahl des vorherigen Elements abhängen, wird der Bezug zum vorherigen Element nicht oder nicht eindeutig hergestellt.
- In einer mobilen Applikation zum Erlernen einer Fremdsprache können die Abschlussfragen der jeweiligen Lektion nicht der entsprechenden Lektion zugeordnet werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#eltern-kind-beziehungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#eltern-kind-beziehungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-10"></a>

#### 11.5.2.10 Text

*Übergeordneter Prüfschritt. Die Inhalte stehen in den folgenden Unterschritten.*

<a id="k-11-5-2-10a"></a>

#### 11.5.2.10a Nicht bearbeitbarer Text

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#a-nicht-bearbeitbarer-text) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#a-nicht-bearbeitbarer-text) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn nicht bearbeitbarer Text vorhanden ist.

**Anforderung**

Textinhalte werden analog zur visuellen Darstellung durch den Screenreader wiedergegeben. Längerer Text wird bei aktivem Screenreader sinnvoll und absatzweise in Blöcken fokussiert. Visuell hervorgehobene Textinhalte (beispielsweise fett, kursiv, unterstrichen) werden auch innerhalb eines Absatzes vom Screenreader separat fokussiert. Unnötige Sonderzeichen werden vom Screenreader ignoriert.

Abkürzungen werden vom Screenreader ungekürzt ausgesprochen. Spezielle Formate, beispielsweise Informationen zu Datum oder Uhrzeit, werden im jeweiligen Format korrekt wiedergegeben.

Es wird dringend empfohlen, entsprechend relevante Inhalte, beispielsweise eine IBAN, eine Postleitzahl oder eine Telefonnummer, auf eine Art zu implementieren, dass sie vom Screenreader zeichenweise buchstabiert ausgegeben werden. Dies ist technisch unter Umständen nicht in allen Umgebungen umsetzbar und kann somit nicht gefordert werden.

**Warum wird das geprüft?**

Nicht nur der Inhalt von Text, sondern auch die Formatierung vermittelt Informationen. Fetter oder kursiver Text deutet wichtige Informationen innerhalb eines Absatzes an. Hilfsmitteltechnologien müssen diese Informationen erfassen können, damit das dahinter liegende Kommunikationskonzept von allen Nutzenden nachvollzogen werden kann. Weiterhin ist bei speziellen Akronymen, Begriffen oder Werten die zeichenweise Ausgabe über den Screenreader zum besseren Verständnis essenziell. Hilfsmitteltechnologien müssen diese Informationen erfassen können, damit der Kontext von allen Nutzenden nachvollzogen werden kann.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der vom Screenreader wiedergegebene Text entspricht der visuellen Darstellung.
- Textinhalte werden gemäß der sichtbaren Trennung durch Absätze in den entsprechenden Abschnitten fokussiert und wiedergegeben
- Längerer Text wird in sinnvollen Blöcken vom Screenreader fokussiert und wiedergegeben.
- Spezielle Formate, beispielsweise eine Telefonnummer, wird vom Screenreader zeichenweise wiedergegeben.
- In einem Kalender werden die einzelnen Tage visuell abgekürzt (Mo, Di, Mi etc.). Der Screenreader spricht die Tage aus (Montag, Dienstag, Mittwoch etc.).
- Ein Datum (beispielsweise 10/10/2000) wird im korrekten Format vom Screenreader wiedergegeben (beispielsweise „Zehnter Oktober Zweitausend“).
- Das Satzzeichen „Punkt“ wird nicht wiedergegeben, da die Ausgabeeinstellung des Screenreaders auf „Keine“ eingestellt wurde.
- Ein fettgedruckter Textinhalt wird innerhalb des Absatzes separat fokussiert und wiedergegeben.
- Relevante Inhalte, beispielsweise eine IBAN, wird nicht zeichenweise buchstabiert ausgegeben.

**Beispiele für negative Bewertung**

- Textinhalte werden abweichend von der visuellen Darstellung vom Screenreader wiedergegeben.
- Ein in sichtbare Absätze unterteilter Text wird vom Screenreader am Stück fokussiert und wiedergegeben.
- Textinhalte werden vom Screenreader zu kleinteilig fokussiert und wiedergegeben.
- Eine Telefonnummer wird als einzelne lange Zahl wiedergegeben.
- Die Abkürzung „Mo“ wird unverändert wiedergegeben.
- Ein Datum (beispielsweise 10.10.2000) wird unverständlich, beispielsweise als „Zehn Punkt Zehn Punkt Zweitausend“ wiedergegeben.
- Das Satzzeichen „Punkt“ wird wiedergegeben, obwohl die Ausgabeeinstellung des Screenreaders auf „Keine“ eingestellt wurde.
- Ein fettgedruckter Textinhalt innerhalb eines Absatzes wird nicht separat fokussiert

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#a-nicht-bearbeitbarer-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#a-nicht-bearbeitbarer-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-10b"></a>

#### 11.5.2.10b Bearbeitbarer Text

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#b-bearbeitbarer-text) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#b-bearbeitbarer-text) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn bearbeitbarer Text, beispielsweise in Texteingabefeldern, vorhanden ist.

**Anforderung**

Textinhalte werden analog zur visuellen Darstellung durch den Screenreader wiedergegeben.

Textformatierungen werden idealerweise vom Screenreader nach der Ausgabe des Inhalts des fokussierten Textabschnitts wiedergegeben, sofern diese vom Nutzenden geändert werden können (beispielsweise Fett, Kursiv, Schriftgröße, Schriftart).

**Warum wird das geprüft?**

Nicht nur der Inhalt von Text, sondern auch die Formatierung vermittelt Informationen. Fetter oder kursiver Text deutet wichtige Informationen innerhalb eines Absatzes an. Hilfsmitteltechnologien müssen diese Informationen erfassen können, damit das dahinter liegende Kommunikationskonzept von allen Nutzenden nachvollzogen werden kann. Weiterhin ist bei speziellen Akronymen, Begriffen oder Werten die zeichenweise Ausgabe über den Screenreader zum besseren Verständnis essenziell. Hilfsmitteltechnologien müssen diese Informationen erfassen können, damit der Kontext von allen Nutzenden nachvollzogen werden kann.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Der durch den Screenreader wiedergegebene Text entspricht der visuellen Darstellung.
- Der Screenreader gibt zusätzlich zum fokussierten Textinhalts Informationen zur vorgenommenen Formatierung aus.

**Beispiele für negative Bewertung**

- Der durch den Screenreader wiedergegebene Text entspricht nicht der visuellen Darstellung.
- Der Screenreader gibt keine Informationen zur vorgenommenen Formatierung aus.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#b-bearbeitbarer-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#b-bearbeitbarer-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-11"></a>

#### 11.5.2.11 Übersicht verfügbarer Aktionen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#übersicht-verfügbarer-aktionen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#übersicht-verfügbarer-aktionen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#übersicht-verfügbarer-aktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Interaktive Bedienelemente übermitteln eine Übersicht ihrer verfügbaren Aktionen, als auch die Art und Weise, wie die jeweilige Aktion bei aktivem Screenreader ausgeführt wird, korrekt an die assistiven Systeme. Dies betrifft beispielsweise:

- Tasten (Zum Ausführen doppelklicken)
- Umschalttasten (Zum Umschalten doppelklicken)
- Kontrollkästchen (Zum Ändern doppelklicken)
- Auswahlschalter (Zum Auswählen doppelklicken)
- Registerkarten (Zum Auswählen doppelklicken)
- Ausklapplisten (Zum Auswählen doppelklicken)
- Formularfelder (Zum Bearbeiten doppelklicken, oder Zum Einfügen von Text doppelklicken)
- Links (Zum Öffnen doppelklicken)

**Warum wird das geprüft?**

Während der Bedienung mit aktivem Screenreader ist oftmals nicht klar, welche Aktionen an einem Bedienelement auf welche Art zum Ziel führt. Sofern die entsprechenden Elemente korrekte Aufforderungen anbieten, können Screenreader die jeweils notwendige Aktion ausgeben. Konsistente Aktionsaufforderungen, beispielsweise “Zum Öffnen doppelklicken”, an Bedienelementen helfen vor allem unerfahrenen Nutzenden bei der Bedienung der diversen Steuerelemente.

**Beispiele für positive Bewertung**

- Die verfügbaren Aktionen interaktiver Bedienelemente werden vom Screenreader korrekt und vollumfänglich wiedergegeben.

**Beispiele für negative Bewertung**

- Die verfügbaren Aktionen interaktiver Bedienelemente werden vom Screenreader nicht, fehlerhaft oder nicht vollumfänglich wiedergegeben.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#übersicht-verfügbarer-aktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bedien- und Formularelemente übermitteln eine Übersicht aller verfügbaren Aktionen korrekt und vollumfänglich an assistive Technologien. Dies beinhaltet, auf welche Art und Weise die jeweilige Aktion bei aktivem Screenreader ausgeführt werden kann. Beispiele für einfache Aktionsaufforderungen sind:

- Tasten („Zum Aktivieren doppeltippen“)
- Umschalttasten („Zum Umschalten doppeltippen“)
- Kontrollkästchen („Zum Ändern doppeltippen“)
- Auswahlschalter („Zum Auswählen doppeltippen“)
- Registerkarten („Zum Auswählen doppeltippen“)
- Ausklapplisten („Zum Erweitern doppeltippen“)
- Eingabefelder („Zum Bearbeiten doppeltippen“)
- Schieberegler („Zum Anpassen des Werts mit einem Finger nach oben oder unten streichen“)
- Karusselle („Zum Ändern der Seite mit einem Finger nach oben oder unten streichen“)
- Links („Zum Öffnen doppeltippen“)

Da das Betriebssystem iOS bei manchen einfachen Bedienelementen wie Tasten auf die Ausgabe der verfügbaren Aktionen verzichtet, kann dies auch im Rahmen des Tests explizit für diese Komponenten vernachlässigt werden, sofern sie über keine weiteren Aktionen verfügen.

Verfügt ein Bedien- oder Formularelement über mehrere Aktionen, muss die Aktionsaufforderung außerdem über das Vorhandensein dieser Aktionen informieren. Beispiele für Aktionsaufforderungen bei mehreren verfügbaren Aktionen sind:

- Listeneinträge („Zum Öffnen doppeltippen; Aktionen verfügbar“)
- Eingabefelder („Zum Bearbeiten doppeltippen; Aktionen verfügbar“)

Nachdem der Nutzende die aktive, verfügbare Aktion gewechselt hat, muss die aktualisierte Aktionsaufforderung automatisch ausgegeben werden. Beispiele für aktualisierte Aktionsaufforderungen sind:

- Listeneinträge („Zum Löschen doppeltippen“)
- Eingabefelder („Zum Einfügen von Text doppeltippen“)

Da die Betriebssysteme iOS und Android bei einigen zur Verfügung gestellten Komponenten mit mehreren Aktionen keine Informationen über die Bedienung zum Ändern der Aktionen ausgeben (beispielsweise „Zum Ändern der Aktion mit einem Finger nach oben oder unten streichen“), ist dies im Rahmen dieses Prüfschritts keine notwendige Anforderung, aber empfehlenswert.

**Warum wird das geprüft?**

Während der Bedienung mit aktivem Screenreader ist oftmals nicht klar, welche Aktionen an einem Bedienelement auf welche Art zum Ziel führt. Sofern die entsprechenden Elemente korrekte Aufforderungen anbieten, können Screenreader die jeweils notwendige Aktion ausgeben. Konsistente Aktionsaufforderungen, beispielsweise “Zum Öffnen Doppeltippen”, an Bedienelementen helfen vor allem unerfahrenen Nutzenden bei der Bedienung der diversen Steuerelemente.

**Beispiele für positive Bewertung**

- Die verfügbaren Aktionen von Bedien- und Formularelementen werden vom Screenreader korrekt und vollumfänglich wiedergegeben.
- Tasten werden von VoiceOver ohne Aktionsaufforderung ausgegeben.
- Bei Bedienelementen mit mehreren Aktionen wird eine Information über das Vorhandensein weiterer Aktionen ausgegeben.
- Bei Bedienelementen mit mehreren Aktionen werden alle verfügbaren Aktionen vom Screenreader bei Aktivierung korrekt und vollumfänglich wiedergegeben.

**Beispiele für negative Bewertung**

- Die verfügbaren Aktionen interaktiver Bedienelemente werden vom Screenreader nicht, fehlerhaft oder nicht vollumfänglich wiedergegeben.
- Bedienelemente mit mehreren verfügbaren Aktionen weisen nicht auf das Vorhandensein weiterer Aktionen hin.

<a id="k-11-5-2-12"></a>

#### 11.5.2.12 Ausführung verfügbarer Aktionen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#ausführung-verfügbarer-aktionen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#ausführung-verfügbarer-aktionen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind.

**Warum wird das geprüft?**

Interaktive Elemente, die bei aktivem Screenreader oder mittels Sprachsteuerung nicht bedient werden können, blockieren den Nutzenden bei der Verwendung des digitalen Produkts. Bei Blockaden steht in aller Regel keine, oder nur eine über Umwege erreichbare Behelfslösung zur Verfügung, der Nutzende ist dann bei der Verwendung des Produkts unter Umständen stark eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#ausführung-verfügbarer-aktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Interaktive Bedienelemente sind über die assistiven Systeme bedienbar, unabhängig davon, ob verfügbare Aktionen ausgegeben werden oder nicht. Dies betrifft beispielsweise:

- Tasten (Zum Ausführen doppelklicken)
- Umschalttasten (Zum Umschalten doppelklicken)
- Kontrollkästchen (Zum Ändern doppelklicken)
- Auswahlschalter (Zum Auswählen doppelklicken)
- Registerkarten (Zum Auswählen doppelklicken)
- Ausklapplisten (Zum Auswählen doppelklicken)
- Optionen von Ausklapplisten (Ändern der Optionen mittels Pfeiltasten)
- Formularfelder (Zum Bearbeiten doppelklicken, Einfügen von Text mittels Tastatur)
- Links (Zum Öffnen doppelklicken)

**Beispiele für positive Bewertung**

- Interaktive Bedienelemente können bei aktivem Screenreader vollumfänglich bedient werden, die Aktionen werden korrekt ausgelöst.
- Interaktive Bedienelemente können mittels Ansage der Beschriftung korrekt ausgelöst werden.

**Beispiele für negative Bewertung**

- Interaktive Bedienelemente können bei aktivem Screenreader nicht, fehlerhaft oder nicht vollumfänglich bedient werden.
- Interaktive Elemente können mittels Sprachsteuerung nur über die Rasteransicht bedient werden, jedoch nicht mittels Ansage der Beschriftung.
- Die Auswahl von Optionen in Auswahlfeldern kann über Wischgeste nach rechts oder links verändert werden, jedoch nicht über die Wischgeste nach oben oder unten mit 3 Fingern.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#ausführung-verfügbarer-aktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Bedien- und Formularelemente sind über assistive Technologien vollumfänglich bedienbar, unabhängig davon, ob verfügbare Aktionen ausgegeben werden oder nicht. Dies betrifft beispielsweise:

- Tasten („Zum Aktivieren doppeltippen“)
- Umschalttasten („Zum Umschalten doppeltippen“)
- Kontrollkästchen („Zum Ändern doppeltippen“)
- Auswahlschalter („Zum Auswählen doppeltippen“)
- Registerkarten („Zum Auswählen doppeltippen“)
- Ausklapplisten („Zum Erweitern doppeltippen“)
- Eingabefelder („Zum Bearbeiten doppeltippen“, oder „Zum Einfügen von Text doppeltippen“)
- Schieberegler („Zum Anpassen des Werts mit einem Finger nach oben oder unten streichen“)
- Karusselle („Zum Ändern der Seite mit einem Finger nach oben oder unten streichen“)
- Links („Zum Öffnen doppeltippen“)

Bei Elementen mit mehreren verfügbaren Aktionen müssen alle Funktionen mit assistiven Technologien ausführbar sein. Dies betrifft auch Funktionen, welche vom Nutzenden manuell eingeblendet werden müssen, beispielsweise die Löschen-Funktion eines Listeneintrags, welcher erst durch eine Wischgeste angezeigt wird.

**Beispiele für positive Bewertung**

- Bedienelemente können bei aktivem Screenreader vollumfänglich bedient werden, die Aktionen werden korrekt ausgelöst.
- Bedienelemente können mittels Ansage der Beschriftung korrekt ausgelöst werden.

**Beispiele für negative Bewertung**

- Bedienelemente können bei aktivem Screenreader nicht, fehlerhaft oder nicht vollumfänglich bedient werden.
- Interaktive Elemente können mittels Sprachsteuerung nur über die Rasteransicht bedient werden, jedoch nicht mittels Ansage der Beschriftung.

<a id="k-11-5-2-13"></a>

#### 11.5.2.13 Nachverfolgung des Fokus und der Auswahlattribute

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#nachverfolgung-des-fokus-und-der-auswahlattribute) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#nachverfolgung-des-fokus-und-der-auswahlattribute) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Texteingabefelder vorhanden sind.

**Anforderung**

Bei der Eingabe von Text in Texteingabefeldern sind der aktuelle Fokus und die aktuelle Cursorposition während der Bearbeitung mit aktivem Screenreader jederzeit im sichtbaren Bereich des Texteingabefelds. Der Anzeigebereich bei zusätzlicher, aktiver Bildschirmlupe folgt jederzeit der aktuellen Cursorposition. Textmarkierungen und Anfang und Ende des Texteingabefelds sind identifizierbar.

**Warum wird das geprüft?**

Zur Eingabe von Text in Formularfeldern muss für Nutzende von assistiven Systemen eine optimale Orientierung geschaffen werden. Durch die programmatische Ermittelbarkeit der Informationen über den Fokus und die Auswahlattribute können assistive Technologien wie Screenreader, Sprachsteuerung, Bildschirmlupe oder Braillezeile besser verstehen und interpretieren.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Während der Eingabe von Zeichen oder Worten in einem Texteingabefeld wird der Fokus verfolgt und die Cursorposition ist sichtbar.
- Anfang und Ende des Texteingabefelds werden identifiziert.
- Der Anzeigebereich bei aktiver Bildschirmlupe folgt in einem Texteingabefeld stets der Cursorposition.

**Beispiele für negative Bewertung**

- Während der Eingabe von Zeichen oder Worten in einem Texteingabefeld verlässt die Cursorposition den sichtbaren Bereich des Eingabefelds, sobald der Cursor den Anzeigebereich verlässt.

- Anfang und Ende des Eingabefelds werden nicht identifiziert.
- Der Anzeigebereich bei aktiver Bildschirmlupe folgt nicht der aktuellen Cursorposition.
- Bei aktiver Bildschirmlupe ist der Cursor kaum sichtbar, weil er nicht mit vergrößert wird.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#nachverfolgung-des-fokus-und-der-auswahlattribute) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#nachverfolgung-des-fokus-und-der-auswahlattribute) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-14"></a>

#### 11.5.2.14 Änderung des Fokus und der Auswahlattribute

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#änderung-des-fokus-und-der-auswahlattribute) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#änderung-des-fokus-und-der-auswahlattribute) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Texteingabefelder vorhanden sind.

**Warum wird das geprüft?**

Zur Eingabe und Bearbeitung von Text in Formularfeldern muss für Nutzende von assistiven Systemen eine optimale Orientierung geschaffen werden. Durch die programmatische Ermittelbarkeit der Informationen über den Fokus und die Auswahlattribute, und die entsprechende Unterstützung bei Bearbeitung können assistive Technologien wie Screenreader, Sprachsteuerung, Bildschirmlupe oder Braillezeilen besser verstehen, interpretieren und mit der Benutzeroberfläche interagieren.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#änderung-des-fokus-und-der-auswahlattribute) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Eingabe von Text in Formularfeldern bei aktivem Screenreader und in Kombination mit aktiver Sprachsteuerung oder Tastatur sind der aktuelle Fokus und die aktuelle Cursorposition jederzeit sichtbar, sämtliche Bewegungsoptionen des Cursors (beispielsweise die wort- und zeichenweise Navigation) werden unterstützt. Textmarkierungen und Änderungen der Markierungen sind möglich, und markierte Stellen sind identifizierbar.

**Beispiele für positive Bewertung**

- Die Anforderungen werden auch in den diversen Kombinationen der assistiven Systeme unterstützt.
- Die wort- oder zeichenweise Navigation mittels Screenreader wird unterstützt.
- Das Markieren eines Wortes ist bei Nutzung einer Tastatur möglich.

**Beispiele für negative Bewertung**

- Mindestens eine der Anforderungen, beziehungsweise eine der Kombinationen der assistiven Systeme wird nicht, fehlerhaft oder nicht vollumfänglich unterstützt.
- Die wort- oder zeichenweise Navigation mittels Screenreader wird nicht, nicht vollumfänglich oder nur fehlerhaft unterstützt.
- Das Markieren eines Wortes ist bei Nutzung der Tastatur nicht möglich.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#änderung-des-fokus-und-der-auswahlattribute) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Während der Eingabe von Text in Formularfeldern bei aktivem Screenreader und in Kombination mit aktiver Sprachsteuerung oder externer Tastatur sind der aktuelle Fokus und die aktuelle Cursorposition jederzeit sichtbar, sämtliche Bewegungsoptionen des Cursors (beispielsweise die wort- und zeichenweise Navigation) werden unterstützt. Textmarkierungen und Änderungen der Markierungen sind möglich, und markierte Stellen sind identifizierbar.

**Beispiele für positive Bewertung**

- Die Anforderungen werden auch in den diversen Kombinationen der assistiven Systeme unterstützt.
- Die wort- oder zeichenweise Navigation mittels VoiceOver-Rotor oder Android TalkBack-Menü wird unterstützt.
- Das Markieren eines Wortes ist auch bei Nutzung einer externen Tastatur möglich.

**Beispiele für negative Bewertung**

- Mindestens eine der Anforderungen, beziehungsweise eine der Kombinationen der assistiven Systeme wird nicht, fehlerhaft oder nicht vollumfänglich unterstützt.
- Die wort- oder zeichenweise Navigation mittels VoiceOver-Rotor oder Android TalkBack-Menü wird nicht, nicht vollumfänglich oder nur fehlerhaft unterstützt.
- Das Markieren eines Wortes ist bei Nutzung einer externen Tastatur nicht möglich.

<a id="k-11-5-2-15"></a>

#### 11.5.2.15 Änderungsbenachrichtigung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#änderungsbenachrichtigung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#änderungsbenachrichtigung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind, die Werte aufweisen oder Zustände einnehmen, und deren Wert oder Zustand durch Interaktion verändert werden kann.

**Anforderung**

Interaktive Bedienelemente, die Werte aufweisen oder Zustände einnehmen, und deren Wert oder Zustand durch Interaktion verändert werden kann, übermitteln den aktuellen Wert oder Zustand nach Änderung, gegebenenfalls die veränderte Position oder die veränderten Grenzen korrekt an die assistiven Systeme. Dies betrifft beispielsweise:

- Umschalttasten (aktiviert, nicht aktiviert)
- Kontrollkästchen (ausgewählt, nicht ausgewählt)
- Auswahlschalter (aktiviert, nicht aktiviert)
- Registerkarten (ausgewählt, nicht ausgewählt)
- Ausklapplisten (erweitert, nicht erweitert)
- Optionen in Ausklapplisten (Eintrag x von y)

**Warum wird das geprüft?**

Das Identifizieren und die damit verbundene, korrekte Implementierung von Werten und Zuständen ist gerade für Nutzende eines Screenreader essenziell, denn nur so können die Nutzenden das jeweilige Element im Kontext zuordnen, verstehen und bedienen. Sind Informationen am Element unvollständig oder fehlerhaft, werden Verständlichkeit und Wahrnehmbarkeit erheblich eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Interaktive Elemente geben ihren Wert oder Zustand nach Änderung korrekt und vollumfänglich wieder.

**Beispiele für negative Bewertung**

- Interaktive Elemente geben ihren Wert oder Zustand nach Änderung nicht korrekt oder vollumfänglich wieder.
- Eine Registerkarte gibt nicht wieder, ob sie aktuell ausgewählt oder nicht ausgewählt ist.
- Eine Ausklappliste gibt nach dem Ausklappen den aktuellen Zustand (ausgeklappt) nicht wieder.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#änderungsbenachrichtigung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#änderungsbenachrichtigung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-16"></a>

#### 11.5.2.16 Änderungen von Zuständen und Eigenschaften

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#änderungen-von-zuständen-und-eigenschaften) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#änderungen-von-zuständen-und-eigenschaften) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn interaktive Elemente vorhanden sind, die Eigenschaften aufweisen oder Zustände einnehmen und deren Wert oder Zustand durch Interaktion verändert werden kann.

**Anforderung**

Die Eigenschaften oder Zustände interaktiver Bedienelemente, die durch Interaktion verändert werden können, können auch bei Nutzung assistiver Systeme verändert werden. Dies betrifft beispielsweise:

- Umschalttasten (aktiviert, nicht aktiviert)
- Kontrollkästchen (ausgewählt, nicht ausgewählt)
- Auswahlschalter (aktiviert, nicht aktiviert)
- Registerkarten (ausgewählt, nicht ausgewählt)
- Ausklapplisten (erweitert, nicht erweitert)
- Optionen in Ausklapplisten (Eintrag x von y)

**Warum wird das geprüft?**

Interaktive Elemente, die bei aktivem Screenreader, mittels Sprachsteuerung oder bei aktiver Bildschirmlupe nicht bedient werden können, blockieren den Nutzenden bei der Verwendung des digitalen Produkts. Bei Blockaden steht in aller Regel keine, oder nur eine über Umwege erreichbare Behelfslösung zur Verfügung, der Nutzende ist dann bei der Verwendung des Produkts unter Umständen stark eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Eigenschaften oder Zustände von interaktiven Bedienelementen können durch assistive Systeme einzeln oder in Kombination erwartungsgemäß verändert werden.

**Beispiele für negative Bewertung**

- Die Eigenschaften oder Zustände von interaktiven Bedienelementen können durch assistive Systeme einzeln oder in Kombination nicht erwartungsgemäß verändert werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#änderungen-von-zuständen-und-eigenschaften) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#änderungen-von-zuständen-und-eigenschaften) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-5-2-17"></a>

#### 11.5.2.17 Änderungen von Werten und Text

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#änderungen-von-werten-und-text) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#änderungen-von-werten-und-text) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Formularelemente zur Eingabe von Text vorhanden sind.

**Anforderung**

Die Werte von Formularelementen, die durch Interaktion verändert werden können, können auch bei Nutzung assistiver Systeme verändert werden.

**Warum wird das geprüft?**

Formularelemente, die bei aktivem Screenreader, mittels Sprachsteuerung oder bei aktiver Bildschirmlupe zur Eingabe von Text nicht genutzt werden können, blockieren den Nutzenden bei der Verwendung des digitalen Produkts. Bei Blockaden steht in aller Regel keine, oder nur eine über Umwege erreichbare Behelfslösung zur Verfügung, der Nutzende ist dann bei der Verwendung des Produkts unter Umständen stark eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Werte oder Texte von Formularfeldern können durch assistive Systeme einzeln oder in Kombination erwartungsgemäß verändert werden.

**Beispiele für negative Bewertung**

- Werte oder Texte von Formularfeldern können durch assistive Systeme einzeln oder in Kombination nicht erwartungsgemäß verändert werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#änderungen-von-werten-und-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#änderungen-von-werten-und-text) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

### Kapitel: Dokumentierte Nutzung

<a id="k-11-6-2"></a>

#### 11.6.2 Keine Unterbrechung der Barrierefreiheitsfunktionen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#keine-unterbrechung-der-barrierefreiheitsfunktionen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#keine-unterbrechung-der-barrierefreiheitsfunktionen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Anforderung**

Die assistiven Systeme des Betriebssystems, im Schwerpunkt die Systeme Screenreader, Bildschirmlupe, Sprachsteuerung und externe Tastatur, werden bei aktiver Nutzung nicht unterbrochen, verändert und frieren nicht ein, oder werden unerwartet beendet, ohne dass der Nutzende eine entsprechende Aktion ausführt oder im Vorfeld informiert wurde.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade

**Beispiele für positive Bewertung**

- Die assistiven Systeme werden während der aktiven Nutzung nicht unterbrochen, verändert, frieren nicht ein und werden nicht unerwartet beendet.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#keine-unterbrechung-der-barrierefreiheitsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Warum wird das geprüft?**

Aktive assistive Systeme, und gegebenenfalls ihre jeweilige Einstellung, die durch den Nutzenden konfiguriert wurde, dürfen durch die Software nicht negativ beeinflusst oder verändert werden. Ansonsten werden Nutzende, die auf assistive Systeme angewiesen sind, eingeschränkt oder irritiert. Schlimmstenfalls ist die Software für manche Menschen nicht mehr nutzbar.

**Beispiele für negative Bewertung**

- Ein assistives System wird während der aktiven Nutzung unterbrochen, verändert oder beendet.
- Ein assistives System friert während der Verwendung ein.
- Eine vergrößerte Darstellung, beispielsweise Schriftgröße, wird bei Aufruf einer neuen Seite beendet oder auf 100% zurückgesetzt.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#keine-unterbrechung-der-barrierefreiheitsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Warum wird das geprüft?**

Aktive assistive Systeme, und gegebenenfalls ihre jeweilige Einstellung, die durch den Nutzenden konfiguriert wurde, dürfen durch die mobile Applikation nicht negativ beeinflusst oder verändert werden. Ansonsten werden Nutzende, die auf die assistiven Systeme angewiesen sind, eingeschränkt oder irritiert. Schlimmstenfalls ist die mobile Applikation für manche Menschen nicht mehr nutzbar.

**Beispiele für negative Bewertung**

- Ein assistives System wird während der aktiven Nutzung unterbrochen, verändert oder beendet.
- Ein assistives System friert während de4 Verwendung ein.
- Eine aktive Bildschirmlupe wird bei Aufruf einer neuen Seite beendet oder auf 100% zurückgesetzt.

### Kapitel: Benutzerpräferenzen

<a id="k-11-7"></a>

#### 11.7 Individuelle Benutzerpräferenzen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#individuelle-benutzerpräferenzen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#individuelle-benutzerpräferenzen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist immer anwendbar.

**Warum wird das geprüft?**

Für Menschen mit Beeinträchtigungen sind individuelle, ihren Bedürfnissen entsprechende Benutzereinstellungen ein essenzieller Bestandteil, um die Nutzung digitaler Produkte zu verbessern und die Darstellung von Inhalten gezielt anzupassen. Diese Einstellungen ermöglichen eine globale Verwaltung der Darstellung von Inhalten und erhöhen entscheidend die Effizienz während der Nutzung.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#individuelle-benutzerpräferenzen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Über das Betriebssystem konfigurierte, individuelle Benutzereinstellungen werden durch die Software übernommen. Das beinhaltet obligatorisch:

- Maßeinheiten (beispielsweise Zeit, Datum, Gewicht, Größe, Distanz, Temperatur)
- Farbeinstellungen
- Kontrasteinstellungen
- Schriftarten
- Farbe des Fokusrahmens

Ausnahme: Es handelt sich um Software, die dafür vorgesehen ist, von ihren zugrundeliegenden Plattformen isoliert zu sein und daher keinen Zugriff auf die Benutzervorgaben auf der Plattform besteht.

Die genannten Benutzerpräferenzen sind unter Umständen nicht vollumfänglich. Je nach Betriebssystem und Art der Software stehen unter Umständen weniger, weitere oder andere Präferenzen zur Auswahl.

**Beispiele für positive Bewertung**

- Alle individuellen Benutzerpräferenzen werden korrekt übernommen.
- Die individuellen Farbeinstellungen sind nicht erkennbar, oder werden nicht übernommen.

**Beispiele für negative Bewertung**

- Individuelle Benutzerpräferenzen werden nicht, nicht vollumfänglich oder fehlerhaft vollumfänglich übernommen.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#individuelle-benutzerpräferenzen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Über das Betriebssystem konfigurierte, individuelle Benutzereinstellungen werden durch die Mobile App übernommen. Das beinhaltet obligatorisch:

- Maßeinheiten (beispielsweise Zeit, Datum, Gewicht, Größe, Distanz, Temperatur)
- Farbeinstellungen
- Kontrasteinstellungen
- Schriftarten
- Farbe des Fokusrahmens

Es wird dringend empfohlen, folgende Benutzereinstellungen fakultativ zu unterstützen:

- Dunkler Modus (Dark Mode)

Die Prüfung auf Ändern der Textgröße ist nicht Bestandteil dieses Prüfschritts.

Die genannten Benutzerpräferenzen sind unter Umständen nicht vollumfänglich. Je nach Betriebssystem und Art der Mobile App stehen unter Umständen weniger, weitere oder andere Präferenzen zur Auswahl.

**Beispiele für positive Bewertung**

- Alle individuellen Benutzerpräferenzen werden korrekt übernommen.
- Die individuellen Farbeinstellungen sind nicht erkennbar, oder werden nicht übernommen.
- Der aktive, dunkle Modus wird nicht übernommen.

**Beispiele für negative Bewertung**

- Individuelle Benutzerpräferenzen werden nicht, nicht vollumfänglich oder fehlerhaft übernommen.

### Kapitel: Autorenwerkzeuge

<a id="k-11-8-1"></a>

#### 11.8.1 Inhaltstechnologie

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Autorenwerkzeug (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#inhaltstechnologie) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#inhaltstechnologie) |

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#inhaltstechnologie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt wird über die Prüfschritte 11.8.2 Erstellung barrierefreier Inhalte, 11.8.3 Erhaltung von Barrierefreiheit bei Umwandlungen, 11.8.4 Reparaturunterstützung und 11.8.5 Vorlagen abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Sofern einer der genannten Prüfschritte als nicht voll erfüllt gewertet wird, wird auch dieser Prüfschritt entsprechend gewertet.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht getestet

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#inhaltstechnologie) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt wird über die Prüfschritte 11.8.2 Erstellung barrierefreier Inhalte, 11.8.3 Erhaltung von Barrierefreiheit bei Umwandlungen, 11.8.4 Reparaturunterstützung und 11.8.5 Vorlagen abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Der Prüfschritt übernimmt das negativste Bewertungsresultat aus den zuvor referenzierten Prüfschritten. Sofern alle referenzierten Prüfschritte mit „nicht anwendbar“ bewertet wurden, gilt der Prüfschritt als „nicht anwendbar“.

**Anforderung**

Ein Autorenwerkzeug ist eine spezialisierte Softwareanwendung, mit der digitale Inhalte – wie Texte, Bilder, Videos und interaktive Elemente erstellt, bearbeitet und organisiert werden können. Diese Tools ermöglichen es auch Nutzenden ohne tiefgehende Programmierkenntnisse, professionelle und multimediale Inhalte zu gestalten. Zu den typischen Funktionen gehören das Einfügen und Formatieren von Text, die Integration von Medien, die Gestaltung des Seitenlayouts sowie die Verwaltung und Verlinkung von Inhalten. Moderne Autorenwerkzeuge bieten oft eine intuitive Benutzeroberfläche und unterstützen die Zusammenarbeit mehrerer Autoren, sodass Inhalte effizient und flexibel online publiziert werden können.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

<a id="k-11-8-2"></a>

#### 11.8.2 Erstellung barrierefreier Inhalte

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Autorenwerkzeug (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#erstellung-barrierefreier-inhalte) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#erstellung-barrierefreier-inhalte) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Funktionalitäten vorhanden sind, durch die Inhalte erstellt werden können. Solche Autorenwerkzeuge unterstützen Autoren dabei, Inhalte zu erstellen, zu organisieren und zu verwalten. Autorenwerkzeuge sind beispielsweise:

- Texteditoren und Markdown-Editoren
- Grafikdesign-Tools
- Kollaborative Schreibwerkzeuge
- Storyboard-Tools
- Content-Management-Systeme (CMS)
- Übersetzungs- und Lokalisierungstools
- Rich Media-Editoren
- Social Media Publishing-Tools
- Blog Publishing-Tools

**Warum wird das geprüft?**

Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, und interaktive Elemente, die bestimmte Funktionalitäten bereitstellen, müssen diese Informationen auch an die assistiven Systeme vermitteln. Dies gilt auch für neu generierten Inhalt. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können auch Nutzende von assistiven Systemen den Aufbau der Inhalte wahrnehmen, durchsuchen und bedienen.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Eine neu generierte Überschrift wird vom Screenreader als Überschrift inklusive Angabe der korrekten Ebene wiedergegeben.
- Beim Hochladen von Bildern wird die Eingabe eines Alternativtexts gefordert, der nach dem Hochladen vom Screenreader wiedergegeben wird.
- Beim Generieren eines Videos können Untertitel manuell hinzugefügt oder nachbearbeitet werden.
- Beim Generieren eines Links kann eine eindeutige Beschriftung hinzugefügt werden.

**Beispiele für negative Bewertung**

- Eine neu generierte Überschrift kann nur durch Änderung der Schriftgröße optisch identifiziert werden, der Screenreader erkennt die Überschrift nicht.
- Beim Hochladen von Bildern wird die Eingabe eines Alternativtexts nicht gefordert, oder das Eingabefeld ist ein Pflichtfeld.
- Nach dem Hochladen von Bildern wird der Alternativtext nicht vom Screenreader wiedergegeben.
- Beim Generieren eines Videos können Untertitel nicht manuell hinzugefügt oder nachbearbeitet werden.
- Nach dem Generieren einer Liste identifiziert der Screenreader die generierten Inhalte nicht als Liste.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#erstellung-barrierefreier-inhalte) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Autorenwerkzeuge müssen die Erstellung von barrierefreien Inhalten ermöglichen und die Autoren bei der Erstellung entsprechend anleiten. Der neu generierte Inhalt muss dabei innerhalb der Software ausgegeben werden. Autorenwerkzeuge dürfen dabei zusätzliche Werkzeuge einbinden, wenn die Konformität bestimmter Anforderungen von einem einzelnen Werkzeug nicht erreicht werden kann.

Ein Export in andere Dateiformate ist nicht Bestandteil dieser Prüfung.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#erstellung-barrierefreier-inhalte) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anforderung**

Autorenwerkzeuge müssen die Erstellung von barrierefreien Inhalten ermöglichen und die Autoren bei der Erstellung entsprechend anleiten. Der neu generierte Inhalt muss dabei innerhalb der Mobile App ausgegeben werden. Autorenwerkzeuge dürfen dabei zusätzliche Werkzeuge einbinden, wenn die Konformität bestimmter Anforderungen von einem einzelnen Werkzeug nicht erreicht werden kann.

Ein Export in andere Dateiformate ist nicht Bestandteil dieser Prüfung.

<a id="k-11-8-3"></a>

#### 11.8.3 Erhaltung von Barrierefreiheit bei Umwandlungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Autorenwerkzeug (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#erhaltung-von-barrierefreiheit-bei-umwandlungen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#erhaltung-von-barrierefreiheit-bei-umwandlungen) |

##### Gleichlautend in beiden Verfahren

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#erhaltung-von-barrierefreiheit-bei-umwandlungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte neu strukturiert, exportiert oder in neue Datenformate konvertiert werden können.

**Anforderung**

Im Ausgangsformat vorhandene Unterstützungen und Informationen zur Barrierefreiheit bleiben bei Neustrukturierung, Export oder Konvertierung des Inhalts erhalten und gehen nicht verloren oder werden fehlerhaft verändert.

Eine vollumfängliche Prüfung von neu generierten Dateiformaten ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

**Warum wird das geprüft?**

Oberflächen-Elemente, die visuell bestimmte Charakteristiken aufweisen, und interaktive Elemente, die bestimmte Funktionalitäten bereitstellen, müssen diese Informationen auch an die assistiven Systeme vermitteln. Dies gilt auch für neu generierte Inhalte nach einer Neustrukturierung, einem Export oder einer Konvertierung in ein neues Dateiformat. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können auch Nutzende von assistiven Systemen den Aufbau der Inhalte wahrnehmen, durchsuchen und bedienen.

**Beispiele für positive Bewertung**

- Bereits vorhandene Informationen zur Barrierefreiheit bleiben nach Strukturierung, Export oder Konvertierung erhalten.
- Nach Konvertierung eines barrierefreien Inhalts in das Dateiformat PDF und anschließender Prüfung der Datei, ist im Bericht von PAC lediglich die PDF/UA-Konformität gescheitert.

**Beispiele für negative Bewertung**

- Bereits vorhandene Informationen zur Barrierefreiheit bleiben nach Strukturierung, Export oder Konvertierung nicht, fehlerhaft oder nicht vollumfänglich erhalten.
- Nach Konvertierung eines barrierefreien Inhalts in das Dateiformat PDF und anschließender Prüfung der Datei sind im Bericht von PAC-Fehler enthalten.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#erhaltung-von-barrierefreiheit-bei-umwandlungen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte mithilfe eines App-internen Autorenwerkzeugs importiert, neu strukturiert oder in neue Datenformate konvertiert werden können.

**Anforderung**

Im Ausgangsformat vorhandene Unterstützungen und Informationen zur Barrierefreiheit bleiben bei Neustrukturierung, Import oder Konvertierung des Inhalts erhalten und gehen nicht verloren oder werden fehlerhaft verändert.

Eine vollumfängliche Prüfung von neu generierten Inhalten ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

**Warum wird das geprüft?**

Elemente, die visuell bestimmte Charakteristiken aufweisen oder bestimmte Funktionalitäten bereitstellen, müssen diese Informationen auch an assistive Systeme vermitteln. Dies gilt auch für neu generierte Inhalte nach einer Neustrukturierung, einem Import oder einer Konvertierung in ein neues Dateiformat. Hilfsmitteltechnologien können diese Informationen interpretieren und schaffen somit Orientierung und gezielte Navigationsmöglichkeiten für ihre jeweilige Nutzergruppe. So können auch Nutzende von assistiven Systemen den Aufbau der Inhalte wahrnehmen, durchsuchen und bedienen.

**Beispiele für positive Bewertung**

- Bereits vorhandene Informationen zur Barrierefreiheit bleiben nach Import, Neustrukturierung oder Konvertierung erhalten.
- Nach Konvertierung eines barrierefreien Inhalts in das Dateiformat PDF und anschließender Prüfung der Datei ist im Bericht von PAC 2021 lediglich die PDF/UA-Konformität gescheitert.

**Beispiele für negative Bewertung**

- Bereits vorhandene Informationen zur Barrierefreiheit bleiben nach Import, Neustrukturierung oder Konvertierung nicht, fehlerhaft oder nicht vollumfänglich erhalten.
- Nach Konvertierung eines barrierefreien Inhalts in das Dateiformat PDF und anschließender Prüfung der Datei sind im Bericht von PAC 2021 Fehler enthalten.

<a id="k-11-8-4"></a>

#### 11.8.4 Reparaturunterstützung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Autorenwerkzeug (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#reparaturunterstützung) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#reparaturunterstützung) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Funktionalitäten vorhanden sind, durch die Inhalte erstellt werden können, und zu deren barrierefreien Erstellung Hinweise und Unterstützungen angeboten werden.

**Anforderung**

Sofern eine Unterstützung bei der Erstellung barrierefreier Inhalte implementiert ist, müssen die Hinweise oder Fehlermeldungen eindeutig sein und Korrekturvorschläge zur Fehlerbehebung benannt werden.

**Warum wird das geprüft?**

Sofern eine Unterstützung zur Erstellung barrierefreier Inhalte implementiert ist, soll sie die Nutzenden optimal unterstützen. Nutzende können Fehler leichter beheben oder Konsequenzen verstehen, wenn konkrete Hinweise verfügbar sind.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Beschreibung der Hinweise oder Fehler gibt konkrete Anweisungen zur Behebung des Eingabefehlers oder zur entsprechenden Konsequenz.

**Beispiele für negative Bewertung**

- Die Beschreibung der Hinweise oder Fehler gibt keine konkreten Hinweise zur Behebung des Eingabefehlers oder zur entsprechenden Konsequenz.
- Nach Anzeige der Fehlermeldungen werden Formularfelder neu geladen, die Eingaben müssen erneut vorgenommen werden.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#reparaturunterstützung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#reparaturunterstützung) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-11-8-5"></a>

#### 11.8.5 Vorlagen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Autorenwerkzeug (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#vorlagen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#vorlagen) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Vorlagen zur Erstellung von Inhalten angeboten werden.

**Anforderung**

Werden Vorlagen zur Erstellung von Inhalten angeboten, so stellt mindestens eine der Vorlagen die Erstellung von barrierefreien Inhalten sicher und ist als solche eindeutig erkennbar und gekennzeichnet.

**Warum wird das geprüft?**

Sofern Vorlagen zur Erstellung von Inhalten vorhanden sind, soll mindestens eine barrierefreie Vorlage die Nutzenden optimal unterstützen und die Erstellung von Inhalt vereinfachen.

**Bewertungsalternativen:** Erfüllt · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Mindestens eine Vorlage ist eindeutig als Vorlage zum Erstellen barrierefreier Inhalte gekennzeichnet.
- Nach Anwendung der barrierefreien Vorlage entsprechen die neu generierten Inhalte den Anforderungen des Testvorgehens.

**Beispiele für negative Bewertung**

- Keine Vorlage ist als Vorlage zum Erstellen barrierefreier Inhalte gekennzeichnet.
- Es existiert eine Vorlage zum Erstellen barrierefreier Inhalte, jedoch ist sie nicht eindeutig als solche gekennzeichnet.
- Nach Generieren von Inhalt durch eine entsprechend als barrierefrei gekennzeichnete Vorlage, entsprechen die generierten Inhalt nicht den Anforderungen des Testvorgehens.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#vorlagen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#vorlagen) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

### Kapitel: Dokumentation und unterstützende Dienste

<a id="k-12-1-1"></a>

#### 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#dokumentation-von-barrierefreiheit-und-kompatibilität) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#dokumentation-von-barrierefreiheit-und-kompatibilität) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Dokumentation, beziehungsweise die Erklärung zur Barrierefreiheit, ist oftmals der erste Anlaufpunkt für Menschen mit Beeinträchtigungen. Die entsprechende Dokumentation unterstützt sie dabei, die angebotenen Funktionalitäten zu verstehen und sie leichter aufzufinden. Dabei haben Betroffene einen größeren Aufwand, die Inhalte und Funktionen aufzufinden. Eine vorhandene Dokumentation erleichtert dabei deutlich die Orientierung.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#dokumentation-von-barrierefreiheit-und-kompatibilität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Produktdokumentation beispielsweise in Form einer Hilfeseite, eines Handbuches oder eine Erklärung zur Barrierefreiheit vorhanden ist, und zusätzliche Barrierefreiheitsfunktionen in der Software angeboten werden.

Beispiele

- Einstellungen zu hohem Kontrast, anderen Farbschemata oder größere Schriftgröße, ggf. mit entsprechender Tastenkombination.
- Spezielle Tastenkombinationen für das Überspringen von Bereichen
- Bedienung einer Vorlesefunktion: Start, Stopp, Lesegeschwindigkeit, Lautstärke.

**Anforderung**

Die Verwendung spezieller Barrierefreiheitsfunktionen wird in der Produktdokumentation, in der Erklärung zur Barrierefreiheit oder in beiden Dokumentationen erklärt.

**Beispiele für positive Bewertung**

- Eine Erklärung der Barrierefreiheitsfunktionen ist in mindestens einer der Dokumentation vollumfänglich erhalten.

**Beispiele für negative Bewertung**

- Es ist keine Erklärung der Barrierefreiheitsfunktionen in mindestens einer der Dokumentation erhalten.
- Die Erklärung der Barrierefreiheitsfunktionen ist fehlerhaft oder unvollständig.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#dokumentation-von-barrierefreiheit-und-kompatibilität) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Produktdokumentation beispielsweise in Form einer Hilfeseite, eines Handbuches, oder eine Erklärung zur Barrierefreiheit vorhanden ist, und gleichzeitig App-eigene, assistive Unterstützung angeboten wird.

**Anforderung**

Die Verwendung App-eigener, assistiver Unterstützungen werden in der Produktdokumentation, in der Erklärung zur Barrierefreiheit oder in beiden Dokumentationen erklärt.

**Beispiele für positive Bewertung**

- Eine Erklärung der App-eigenen, assistiven Systeme ist in mindestens einer der Dokumentation vollumfänglich erhalten.
- Eventuelle Einschränkungen hinsichtlich unterstützter, assistiver Systeme der Betriebssysteme in Verbindung mit der Mobile App sind in einer der Dokumentation beschrieben.

**Beispiele für negative Bewertung**

- Es ist keine Erklärung der App-eigenen, assistiven Systeme ist in mindestens einer der Dokumentation erhalten.
- Die Erklärung der App-eigenen, assistiven Systeme ist fehlerhaft oder unvollständig.

<a id="k-12-1-2"></a>

#### 12.1.2 Barrierefreie Dokumentation

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#barrierefreie-dokumentation) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#barrierefreie-dokumentation) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Dokumentation, beziehungsweise die Erklärung zur Barrierefreiheit, ist oftmals der erste Anlaufpunkt für Menschen mit Beeinträchtigungen. Die entsprechende Dokumentation unterstützt sie dabei, einen Überblick über das Produkt zu bekommen. Erfüllt die Dokumentation nicht die jeweils relevanten Anforderungen, ist bereits der unter Umständen erste Kontakt zwischen dem Nutzenden und dem Produkt eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#barrierefreie-dokumentation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Produktdokumentation beispielsweise als Handbuch, oder eine Erklärung zur Barrierefreiheit vorhanden sind.

**Anforderung**

Sofern eine Dokumentation vorhanden ist, muss sie in mindestens einem der folgenden Formate verfügbar sein. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:

- Eigener Bestandteil der Software, konform mit Kapitel 11 der EN 301549.
- Webformat, konform mit Kapitel 9 der EN 301549.
- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.

Weitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.

Eine Funktionalität, die eine automatische Medienkonvertierung unterstützt, ist vorteilhaft für die Verbesserung der Zugänglichkeit.

Eine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Software im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

Ist die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ([https://pac.pdf-accessibility.org/de](https://pac.pdf-accessibility.org/de)).

**Beispiele für positive Bewertung**

- Eine bereitgestellte Dokumentation erfüllt die entsprechend anwendbaren Kriterien dieses Testvorgehens.
- Die Prüfung einer im Dateiformat PDF bereitgestellten Produktdokumentation mittels PAC ergibt keine Fehler.

**Beispiele für negative Bewertung**

- Eine bereitgestellte Dokumentation erfüllt die entsprechend anwendbaren Kriterien dieses Testvorgehens nicht oder nicht vollumfänglich.
- Die Prüfung einer im Dateiformat PDF bereitgestellten Produktdokumentation mittels PAC ergibt Fehler.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#barrierefreie-dokumentation) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Produktdokumentation beispielsweise als Handbuch, oder eine Erklärung zur Barrierefreiheit vorhanden sind

**Anforderung**

Sofern eine Dokumentation vorhanden ist, muss sie in mindestens einem der folgenden Formate verfügbar sein. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:

- Eigener Bestandteil der Mobile App, konform mit Kapitel 11 der EN 301549.
- Webformat, konform mit Kapitel 9 der EN 301549.
- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.

Weitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.

Eine Funktionalität, die eine automatische Medienkonvertierung unterstützt, ist vorteilhaft für die Verbesserung der Zugänglichkeit.

Eine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Mobile App im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

Ist die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC 2021 (Stand Januar 2024) der PDF/UA Foundation eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ([https://pdfua.foundation/de/](https://pdfua.foundation/de/)).

**Beispiele für positive Bewertung**

- Eine bereitgestellte Dokumentation erfüllt die entsprechend anwendbaren Kriterien dieses Testvorgehens.
- Die Prüfung einer im Dateiformat PDF bereitgestellten Produktdokumentation mittels PAC 2021 ergibt keine Fehler.

**Beispiele für negative Bewertung**

- Eine bereitgestellte Dokumentation erfüllt die entsprechend anwendbaren Kriterien dieses Testvorgehens nicht oder nicht vollumfänglich.
- Die Prüfung einer im Dateiformat PDF bereitgestellten Produktdokumentation mittels PAC 2021 ergibt Fehler.

<a id="k-12-2-2"></a>

#### 12.2.2 Informationen zu Barrierefreiheits- und Kompatibilitätsfunktionen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#informationen-zu-barrierefreiheits--und-kompatibilitätsfunktionen) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#informationen-zu-barrierefreiheits--und-kompatibilitätsfunktionen) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Selbst eine barrierefreie Produktdokumentation ist für manche Menschen nicht ohne Aufwand nutzbar. Informationen in der Dokumentation sind zudem schnell veraltet oder nicht vollumfänglich. Es ist für einige Menschen deutlich einfacher, einen direkten Kontakt zum Produktsupport herzustellen, um individuell Fragen stellen zu können.

**Bewertungsalternativen:** Erfüllt · Einschränkung · Barriere · Blockade · Nicht anwendbar

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#informationen-zu-barrierefreiheits--und-kompatibilitätsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Barrierefreiheitsfunktionen wie in Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben vorhanden sind, und eine Möglichkeit zur Kontaktaufnahme für Produktsupport vorhanden sind.

**Anforderung**

Die durch den Produktsupport übermittelten Informationen zur Software entsprechen mindestens dem Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

Als Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).

**Beispiele für positive Bewertung**

- Der Produktsupport ist zur Software auskunftsfähig.
- Die durch den Produktsupport übermittelten Informationen zur Software entsprechen oder übertreffen den Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

**Beispiele für negative Bewertung**

- Innerhalb von 48 Stunden nach Anfrage erfolgt keine Antwort.
- Der Produktsupport ist zur Software nicht auskunftsfähig.
- Die durch den Produktsupport übermittelten Informationen zur Software sind geringer als der Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#informationen-zu-barrierefreiheits--und-kompatibilitätsfunktionen) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn App-eigene, assistive Unterstützungen wie in Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben, und eine Möglichkeit zur Kontaktaufnahme für Produktsupport vorhanden sind.

**Anforderung**

Die durch den Produktsupport übermittelten Informationen zu den App-eigenen, assistiven Systemen entsprechen mindestens dem Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

Als Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).

**Beispiele für positive Bewertung**

- Der Produktsupport ist zu den App-eigenen, assistiven Systemen auskunftsfähig.
- Die durch den Produktsupport übermittelten Informationen zu den App-eigenen, assistiven Systemen entsprechen oder übertreffen den Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

**Beispiele für negative Bewertung**

- Innerhalb von 48 Stunden nach Anfrage erfolgt keine Antwort.
- Der Produktsupport ist zu den App-eigenen, assistiven Systemen nicht auskunftsfähig.
- Die durch den Produktsupport übermittelten Informationen zu den App-eigenen, assistiven Systemen sind geringer als der Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.

<a id="k-12-2-3"></a>

#### 12.2.3 Effektive Kommunikation

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#effektive-kommunikation) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#effektive-kommunikation) |

##### Gleichlautend in beiden Verfahren

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Möglichkeit zur Kontaktaufnahme für Produktsupport vorhanden ist.

**Anforderung**

Die Erreichbarkeit des Produktsupports soll den Bedürfnissen von Menschen mit Beeinträchtigungen gerecht werden, indem mindestens zwei Supportkanäle angeboten werden, die unterschiedliche Sinne ansprechen. Dabei ist es irrelevant, ob der Produktsupport durch den Hersteller selbst, oder durch einen Dienstleister erbracht wird.

Die Anforderung ist bei jedem Niveau der Unterstützung verschiedener Kommunikationsarten erfüllt. Anbieter können angeben, in welchem Umfang die Kommunikationsarten unterstützt werden, um eine aussagekräftigere Bewertung der Angemessenheit und Qualität der Unterstützung zu ermöglichen.

Als Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).

**Warum wird das geprüft?**

Menschen mit Beeinträchtigung beim Hören oder Sprechen können unter Umständen einen telefonischen Supportkanal nicht nutzen. Dagegen kann es kognitiv beeinträchtigten Menschen schwerfallen, schriftlich zu kommunizieren. Um den Bedürfnissen der verschiedenen Menschen zu begegnen, müssen Alternativen nach dem Mehr-Sinne-Prinzip angeboten werden.

**Bewertungsalternativen:** Erfüllt · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Es werden mindestens zwei Supportkanäle, die unterschiedliche Sinne ansprechen, angeboten.

**Beispiele für negative Bewertung**

- Es werden kein oder nur ein Supportkanal angeboten.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#effektive-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#effektive-kommunikation) (dort auch Testvorgehen, Abgrenzung, Verweise)

*Alle Texte gleichlautend, siehe oben.*

<a id="k-12-2-4"></a>

#### 12.2.4 Barrierefreie Dokumentation durch den Produktsupport

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich | Basisfunktionalität (Vorschlag, abweichend vom Repository) |
| Prüfverfahren | BIT inklusiv Software-Test · BIT inklusiv App-Test |
| Gilt für Anwendungstypen | Website oder Webanwendung · Software (offen) · Software (geschlossen) · Mobile Anwendung (offen) · Mobile Anwendung (geschlossen) |
| Fundstellen | [BIT inklusiv Software-Test](https://softwaretest.bit-inklusiv.de/#barrierefreie-dokumentation-durch-den-produktsupport) · [BIT inklusiv App-Test](https://apptest.bit-inklusiv.de/#barrierefreie-dokumentation-durch-den-produktsupport) |

##### Gleichlautend in beiden Verfahren

**Warum wird das geprüft?**

Die Dokumentation, beziehungsweise die Erklärung zur Barrierefreiheit, ist oftmals der erste Anlaufpunkt für Menschen mit Beeinträchtigungen. Die entsprechende Dokumentation unterstützt sie dabei, einen Überblick über das Produkt zu bekommen. Erfüllt die Dokumentation nicht die jeweils relevanten Anforderungen, ist bereits der unter Umständen erste Kontakt zwischen dem Nutzenden und dem Produkt eingeschränkt.

**Bewertungsalternativen:** Erfüllt · Leichte Einschränkung · Einschränkung · Barriere · Blockade · Nicht anwendbar

**Beispiele für positive Bewertung**

- Die Bewertung dieses Prüfschritts liegt im Ermessen des Testers und hängt maßgeblich vom getesteten Umfang und den Testergebnissen ab.

**Beispiele für negative Bewertung**

- Die Bewertung dieses Prüfschritts liegt im Ermessen des Testers und hängt maßgeblich vom getesteten Umfang und den Testergebnissen ab.

##### BIT inklusiv Software-Test

[Prüfschritt im BIT inklusiv Software-Test öffnen](https://softwaretest.bit-inklusiv.de/#barrierefreie-dokumentation-durch-den-produktsupport) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Möglichkeit zur Kontaktaufnahme für Produktsupport vorhanden ist, und der Produktsupport im Testvorgehen unter Prüfschritt 12.2.2 Informationen zu Barrierefreiheit- und Kompatibilitätsfunktionen eine generelle Dokumentation zur Software zur Verfügung gestellt hat.

**Anforderung**

Die durch den Produktsupport übermittelte Dokumentation zur Software muss in mindestens einem der folgenden Formate übermittelt werden. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:

- Webformat, konform mit Kapitel 9 der EN 301549.
- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.

Als Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).

Weitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.

Eine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Software im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

Ist die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ([https://pac.pdf-accessibility.org/de](https://pac.pdf-accessibility.org/de)).

##### BIT inklusiv App-Test

[Prüfschritt im BIT inklusiv App-Test öffnen](https://apptest.bit-inklusiv.de/#barrierefreie-dokumentation-durch-den-produktsupport) (dort auch Testvorgehen, Abgrenzung, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn eine Möglichkeit zur Kontaktaufnahme für Produktsupport vorhanden ist, und der Produktsupport im Testvorgehen unter Prüfschritt 12.2.2 Informationen zu Barrierefreiheit- und Kompatibilitätsfunktionen eine generelle Dokumentation zur Mobile App zur Verfügung gestellt hat.

**Anforderung**

Die durch den Produktsupport übermittelte Dokumentation zum Produkt muss in mindestens einem der folgenden Formate übermittelt werden. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:

- Webformat, konform mit Kapitel 9 der EN 301549.
- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.

Als Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).

Weitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.

Eine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Mobile App im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.

Ist die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC 2021 (Stand Januar 2024) der PDF/UA Foundation eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ([https://pdfua.foundation/de/](https://pdfua.foundation/de/)).
