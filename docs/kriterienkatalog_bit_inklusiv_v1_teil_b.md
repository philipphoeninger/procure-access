# Kriterienkatalog ProcureAccess – Teil B: PDF-Test

Gehört zu kriterienkatalog_bit_inklusiv_v1.md (Grundlagen, Filter, Bewertungsskala, Darstellung). Quellen und Nutzungsrechte siehe dort.

## Kriterien aus dem PDF-Test

Der PDF-Test folgt EN 301 549 Kapitel 10 (Dokumente) und ergänzt Prüfschritte aus PDF/UA-1 (Nummern „UA1 …“). Positive und negative Beispiele stehen im PDF-Test zusammen mit den Bewertungsalternativen; sie sind hier unverändert übernommen. Der Funktionsbereich ist ein Vorschlag: übernommen vom entsprechenden Kriterium aus Kapitel 11 (10.x.y.z ↔ 11.x.y.z), UA1-Prüfschritte als Basisfunktionalität.

<a id="k-10-1-1-1"></a>

#### 10.1.1.1 Nicht-Text-Inhalt

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#nicht-text-inhalt) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#nicht-text-inhalt) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument nicht textlichen Inhalt besitzt bzw. Inhalt, der zwar in textlicher Form sichtbar, aber beispielsweise mit einem Screenreader-Programm nur dann lesbar ist, wenn er über einen sogenannten ActualText verfügt.

**Anforderung**

Für alle nicht text-basierten Inhalte (z. B. informative Grafiken und Bilder) steht eine Textalternative zur Verfügung, welche die gleichen Informationen abdeckt.

**Warum wird das geprüft?**

Durch Textalternativen kann sichergestellt werden, dass auch Personen ohne bzw. mit eingeschränktem Sehvermögen die visuellen Inhalte einer Medienpräsentation (sowie die damit direkt oder implizit verbundenen Informationen) vollumfänglich zur Verfügung stehen.

Darüber hinaus ist Text als Medienalternative vielfältig einsetzbar, wie insbesondere auch für Übersetzungen, und kann in diverse Ausgabeformate, wie z. B. Audio, Braille etc., transformiert werden, um den unterschiedlichsten Bedürfnissen gerecht zu werden. Darüber hinaus unterstützt Text die Möglichkeit, nach Medieninhalten zu suchen und Inhalte auf eine Vielzahl von Arten zu einem neuen Zweck zu verbinden.

Dem Zwei-Sinne-Prinzip folgend erleichtert eine Medienalternative auch Menschen mit kognitiven Einschränkungen bzw. mit Sprach- bzw. Lernschwierigkeiten den Zugang zu den dargestellten Inhalten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Für informative nicht text-basierte Inhalte existieren Textalternativen mit zutreffendem Inhalt in angemessenem Umfang.
- leichte Einschränkung: Für informative nicht text-basierte Inhalte existieren zwar Textalternativen, diese ufern aber unnötig aus.
- Einschränkung: Für informative nicht text-basierte Inhalte existieren Textalternativen mit zutreffenden Inhalten, bei denen jedoch wesentliche Informationen fehlen.
- Barriere: Für informative nicht text-basierte Inhalte existieren Textalternativen mit zutreffenden Inhalten, die jedoch unnötig ausufern und bei denen wesentliche Informationen fehlen.
- Blockade: Das PDF-Dokument enthält informative nicht text-basierte Inhalte, die über keinerlei Textalternative verfügt.

Wenn obige Anforderungen nicht voll erfüllt sind, kann mindestens mit „leichte Einschränkung“ bewertet werden.

<a id="k-10-1-2-1"></a>

#### 10.1.2.1 Alternativen für aufgezeichnetes Audio und Videos ohne Ton

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Video |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#alternativen-für-aufgezeichnetes-audio-und-videos-ohne-ton) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald im PDF-Dokument voraufgezeichnete Audio-Dateien ohne Video bzw. Video-Dateien ohne Tonspur bereitgestellt werden.

**Anforderung**

Für voraufgezeichnete Nur-Audio- und Nur-Video-Medien gilt Folgendes, es sei denn, die Audio- oder Video-Datei stellt eine Medienalternative für Text dar und wird auch deutlich als solche gekennzeichnet:

- Voraufgezeichnetes Nur-Audio: Es wird eine Alternative für zeitbasierte Medien bereitgestellt, die gleichwertige Informationen für voraufgezeichnete reine Audio-Inhalte bietet.
- Voraufgezeichnetes Nur-Video: Es wird entweder eine Alternative für zeitbasierte Medien oder eine Audiospur bereitgestellt, die gleichwertige Informationen für voraufgezeichnete reine Videoinhalte bietet.

**Warum wird das geprüft?**

Bei der Bereitstellung einer Textalternative zu allen reinen Audio-Dateien, bei denen es sich nicht um eine Medienalternative für Text handelt, geht es darum, insbesondere gehörlosen oder schwerhörigen Menschen vollständigen Zugang zu den Inhalten zu ermöglichen. Textalternativen liefern dabei den über die Tonspur verfügbaren Inhalt. Eine Textalternative muss nicht nur Dialoge wiedergeben, sondern auch angeben, wer spricht. Textalternativen enthalten darüber hinaus nichtsprachliche Informationen, die durch Ton vermittelt werden, wie insbesondere Soundeffekte, aber auch Stille.

Bei der Bereitstellung einer Textalternative oder alternativ einer Audio-Datei zu allen reinen Video-Dateien, bei denen es sich nicht um eine Medienalternative für Text handelt, geht es darum, insbesondere Personen, die Schwierigkeiten damit haben, bewegte Bilder zu erfassen, sowie Personen ohne bzw. mit eingeschränktem Sehvermögen vollständigen Zugang zu den Inhalten zu ermöglichen.

Dem Zwei-Sinne-Prinzip folgend erleichtert eine Medienalternative auch Menschen mit kognitiven Einschränkungen bzw. mit Sprach- bzw. Lernschwierigkeiten den Zugang zu den dargestellten Inhalten.

Text als Medienalternative ist darüber hinaus vielfältig einsetzbar, wie insbesondere auch für Übersetzungen, und kann in diverse Ausgabeformate, wie z. B. Audio, Braille etc., transformiert werden, um den unterschiedlichsten Bedürfnissen gerecht zu werden. Darüber hinaus unterstützt Text die Möglichkeit, nach Medieninhalten zu suchen und Inhalte auf eine Vielzahl von Arten zu einem neuen Zweck zu verbinden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Der Inhalt einer reinen Audio-Datei wird über eine Textalternative vollumfänglich wiedergegeben. Die zugehörige Textalternative lässt sich rasch identifizieren und eindeutig zuordnen.
- erfüllt: Der Inhalt einer reinen Audio-Datei wird über eine Textalternative vollumfänglich wiedergegeben. Die Textalternative kann über einen Link mit aussagekräftiger Beschriftung unmittelbar im Anschluss an die reine Audio-Datei aufgerufen werden.
- erfüllt: Der Inhalt einer reinen Video-Datei ohne Tonspur wird über eine Textalternative vollumfänglich wiedergegeben. Die zugehörige Textalternative lässt sich rasch identifizieren und eindeutig zuordnen.
- erfüllt: Der Inhalt einer reinen Video-Datei ohne Tonspur wird über eine Audioaufnahme vollumfänglich wiedergegeben. Die zugehörige Audioaufnahme lässt sich rasch identifizieren und eindeutig zuordnen.
- erfüllt: Der Inhalt einer reinen Video-Datei ohne Tonspur dient (z. B. in einem Tutorial) zur Illustration einer bereits in Textform vorliegenden ausführlichen Beschreibung. Die zugehörige Textalternative weist darauf in geeigneter Weise und aller Kürze hin.
- leichte Einschränkung: Es existiert eine gut auffindbare Textalternative bzw. eine gut auffindbare Audioaufnahme. Der Inhalt der reinen Audio-Datei bzw. reinen Video-Datei ohne Tonspur ist darin jedoch nicht vollumfänglich wiedergegeben, wobei als unwesentlich einzustufende Informationen fehlen.
- Einschränkung: Es existiert eine gut auffindbare Textalternative bzw. eine gut auffindbare Audioaufnahme. Der Inhalt der reinen Audio-Datei bzw. reinen Video-Datei ohne Tonspur ist darin jedoch nicht vollumfänglich wiedergegeben, wobei die fehlenden Informationen als wesentlich einzustufen sind.
- Einschränkung: Der Inhalt einer reinen Audio-Datei bzw. einer reinen Video-Datei ohne Tonspur wird über eine Textalternative vollumfänglich wiedergegeben. Es ist jedoch aufwändig, die zugehörige Textalternative zu ermitteln und eindeutig zuzuordnen.
- Einschränkung: Der Inhalt einer reinen Video-Datei ohne Tonspur wird über eine Audioaufnahme vollumfänglich wiedergegeben. Es ist jedoch aufwändig, die zugehörige Audioaufnahme zu ermitteln und eindeutig zuzuordnen.
- Barriere: Zu einer reinen Audio-Datei bzw. einer reinen Video-Datei ohne Tonspur existiert eine Textalternative bzw. eine Audioaufnahme und der Inhalt ist darin vollumfänglich wiedergegeben. Doch die Zuordnung ist nicht eindeutig.
- Blockade: Für den Inhalt einer reinen Audio-Datei steht keine Textalternative zur Verfügung.
- Blockade: Für den Inhalt einer reinen Video-Datei ohne Tonspur steht weder eine Audioaufnahme noch eine Textalternative zur Verfügung.

<a id="k-10-1-2-2"></a>

#### 10.1.2.2 Untertitel für aufgezeichnete Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Video |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#untertitel-für-aufgezeichnete-videos) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald im PDF-Dokument voraufgezeichnete Video-Dateien mit synchronisiertem Ton und Bild bereitgestellt werden.

**Anforderung**

Untertitel werden für alle voraufgezeichneten Audio-Inhalte in synchronisierten Medien bereitgestellt, es sei denn, die Video-Datei stellt eine Medienalternative für Text dar und wird auch deutlich als solche gekennzeichnet.

**Warum wird das geprüft?**

In diesem Prüfschritt geht es darum, gehörlosen oder schwerhörigen Menschen die Möglichkeit zu bieten, synchronisierte Medienpräsentationen vollständig zu erfassen. Untertitel liefern dabei den Teil des Inhalts, der über die Tonspur verfügbar ist. Untertitel müssen nicht nur Dialoge wiedergeben, sondern auch angeben, wer spricht. Sie enthalten darüber hinaus nichtsprachliche Informationen, die durch Ton vermittelt werden, einschließlich aussagekräftiger Soundeffekte.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Der Prüfschritt ist erfüllt, wenn alle per Video mit Tonspur zur Verfügung gestellten Informationen durch Untertitel allen Menschen (mit und ohne Behinderung) zugänglich sind.
- leichte Einschränkung: Eine leichte Einschränkung liegt vor, wenn Untertitel zwar nicht alle Informationen der Tonspur wiedergeben, das Verständnis des gesamten PDF-Dokuments dadurch aber nicht eingeschränkt wird.
- Einschränkung: Eine Einschränkung liegt vor, wenn Untertitel zwar nicht alle Informationen der Tonspur wiedergeben, das Verständnis des gesamten PDF-Dokuments dadurch aber nur in geringem Maß eingeschränkt wird.
- Barriere: Eine Barriere liegt vor, wenn Untertitel nicht alle Informationen der Tonspur wiedergeben und das Verständnis des gesamten PDF-Dokuments dadurch in nicht geringem Maß eingeschränkt wird.
- Blockade: Eine Blockade liegt vor, wenn Untertitel nicht vorhanden sind oder nicht alle Informationen der Tonspur wiedergeben, obwohl dies zum Verständnis des gesamten PDF-Dokuments notwendig wäre.

Sofern die Informationen für eine oder mehrere Gruppen von Menschen mit Behinderung nicht zugänglich sind (z. B. durch fehlende Untertitel), ist der Prüfschritt nicht erfüllt.

Sofern die Informationen für eine oder mehrere Gruppen von Menschen mit Behinderung nicht vollständig zugänglich sind, ist der Prüfschritt nicht voll erfüllt.

<a id="k-10-1-2-3"></a>

#### 10.1.2.3 Audiodeskription oder Medienalternative für aufgezeichnete Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Video |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#audiodeskription-oder-medienalternative-für-aufgezeichnete-videos) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#audiodeskription-oder-medienalternative-für-aufgezeichnete-videos) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald im PDF-Dokument aufgezeichnete Video-Dateien mit synchronisiertem Ton und Bild bereitgestellt werden.

**Anforderung**

Für alle voraufgezeichneten Videoinhalte in synchronisierten Medien steht eine Textalternative zur Verfügung, die gleichwertige Informationen für die voraufgezeichneten Videoinhalte bietet, es sei denn, die Video-Datei stellt eine Medienalternative für Text dar und wird auch deutlich als solche gekennzeichnet. Die Textalternative kann in Form von Audiodeskription oder einer Alternative für zeitbasierte Medien bereitgestellt werden.

**Warum wird das geprüft?**

Durch Audiodeskription kann sichergestellt werden, dass auch Personen, die Schwierigkeiten damit haben, bewegte Bilder zu erfassen, sowie Personen ohne bzw. mit eingeschränktem Sehvermögen die visuellen Inhalte einer synchronisierten Medienpräsentation vollumfänglich zur Verfügung stehen. Die Audiodeskription ergänzt den Audioteil der Medienpräsentation um alle Informationen, die zusätzlich benötigt werden, wenn der Videoteil nicht verfügbar ist.

Volltext-Alternativen können ebenfalls sowohl für Menschen, die Schwierigkeiten damit haben, bewegte Bilder zu erfassen, sowie für Menschen ohne bzw. mit eingeschränktem Hörvermögen als auch für Menschen ohne bzw. mit Sehvermögen von Vorteil sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Wenn alle Videoinformationen bereits in vorhandenem Audio enthalten sind, ist keine zusätzliche Audiodeskription erforderlich.
- erfüllt: Das PDF-Dokument enthält zwar Videoinhalte, welche ohne Audiodeskription nicht zugänglich sind. Jedoch werden sämtliche Videoinhalte mittels Textalternative (Fließtext o. Ä.) erläutert.
- Blockade: Das PDF-Dokument enthält Videoinhalte ohne zusätzliche Audiodeskription, welche auch nicht durch eine Volltextalternative zugänglich gemacht werden.

<a id="k-10-1-2-4"></a>

#### 10.1.2.4 Untertitel für Live-Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Video |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#untertitel-für-live-videos) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#untertitel-für-live-videos) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald innerhalb des PDF-Dokuments die Möglichkeit besteht, Echtzeitübertragungen via Video-Datei mit synchroner Tonübertragung abzurufen.

**Anforderung**

Untertitel werden für alle Live-Videoinhalte in synchronisierten Medien bereitgestellt.

**Warum wird das geprüft?**

In diesem Prüfschritt geht es darum, gehörlosen oder schwerhörigen Menschen die Möglichkeit zu bieten, synchronisierte Medienpräsentationen vollständig zu erfassen. Untertitel liefern dabei den Teil des Inhalts, der über die Tonspur verfügbar ist. Untertitel müssen nicht nur Dialoge wiedergeben, sondern auch angeben, wer spricht. Sie enthalten darüber hinaus nichtsprachliche Informationen, die durch Ton vermittelt werden, einschließlich aussagekräftiger Soundeffekte.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Menschen unterhalten sich, während im Hintergrund ein Unfall mit einem lauten Knall passiert. Die Information, dass sich im Hintergrund ein Unfall ereignet, wird im Untertitel angegeben.
- Einschränkung: Menschen unterhalten sich, während im Hintergrund ein Unfall mit einem lauten Knall passiert. Die Videoinhalte werden vorab in einer Textalternative erklärt. Doch die synchrone Vermittlung visueller und akustischer Informationen fehlt.
- Blockade: Für die Echtzeitübertragung werden keine Untertitel bereitgestellt.

<a id="k-10-1-2-5"></a>

#### 10.1.2.5 Audiodeskription für aufgezeichnete Videos

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Video |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#audiodeskription-für-aufgezeichnete-videos) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald im PDF-Dokument aufgezeichnete Video-Dateien mit synchronisiertem Ton und Bild bereitgestellt werden.

**Anforderung**

Für alle voraufgezeichneten Videoinhalte in synchronisierten Medien wird Audiodeskription bereitgestellt.

**Warum wird das geprüft?**

Durch Audiodeskription kann sichergestellt werden, dass auch Personen, die Schwierigkeiten damit haben, bewegte Bilder zu erfassen, sowie Personen ohne bzw. mit eingeschränktem Sehvermögen die visuellen Inhalte einer synchronisierten Medienpräsentation vollumfänglich zur Verfügung stehen. Die Audiodeskription ergänzt den Audioteil der Medienpräsentation um alle Informationen, die zusätzlich benötigt werden, wenn der Videoteil nicht verfügbar ist.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Der Prüfschritt ist erfüllt, wenn Audiodeskription zum Verständnis des Inhalts benötigt wird und vorhanden ist.
- leichte Einschränkung: Eine leichte Einschränkung liegt vor, wenn Audiodeskription zwar nur teilweise oder nicht vorhanden ist, aber alle zum Verständnis des PDF-Dokuments notwendigen Informationen aus dem Kontext erschlossen werden können.
- Einschränkung: Eine Einschränkung liegt vor, wenn die nur teilweise vorhandene Audiodeskription zwar zu offenen Fragen führt, aber ein Großteil der Informationen noch aus dem Kontext erschlossen werden können.
- Barriere: Eine Barriere liegt vor, wenn die nur teilweise vorhandene Audiodeskription zu offenen Fragen führt, die sich nicht aus dem Kontext erschließen lassen.
- Blockade: Eine Blockade liegt vor, wenn Audiodeskription zum Verständnis des PDF-Dokuments benötigt wird, jedoch nicht vorhanden ist.

Der Prüfschritt ist nicht voll erfüllt, wenn Audiodeskription zum Verständnis des Inhalts benötigt wird, aber nur teilweise vorhanden ist.

<a id="k-10-1-3-1a"></a>

#### 10.1.3.1a Info und Beziehungen: Getaggtes Dokument

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#a-info-und-beziehungen-getaggtes-dokument) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#a-info-und-beziehungen-getaggtes-dokument) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Informationen, Strukturen und Beziehungen, welche die Darstellung vermittelt, können programmatisch festgelegt und ermittelt werden oder sind im Text verfügbar.

**Warum wird das geprüft?**

PDF-Dokumente benötigen Strukturinformationen, damit optisch unterscheidbare Element-Typen wie Überschriften, Listen, Tabellen, etc. auch auf einer semantischen Ebene und damit bei Screenreader-Nutzung ermittelbar unterscheidbar sind.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn das PDF-Dokument für alle Inhalte Tags enthält.
- leichte Einschränkung: Inhalte ohne oder mit geringem informativem Charakter sind nicht getaggt.
- Einschränkung: Inhalte, die zum Verständnis der Struktur des PDF-Dokuments erforderlich sind (Überschriften, Listen, …), sind in geringem Umfang nicht oder nicht korrekt getaggt.
- Barriere: Das PDF-Dokument ist nicht getaggt. Der Inhalt kann - wenn auch in unstrukturierter Form - als sinnvoll linearisierter Text per Screenreader-Programm erfasst werden. Strukturinformationen müssen aus dem inhaltlichen Kontext geschlossen werden.
- Blockade: Das PDF-Dokument ist nicht getaggt und der sichtbare Inhalt lässt sich auch per Screenreader-Programm nicht erschließen.

Dieser Prüfschritt ist nicht voll erfüllt, wenn das PDF-Dokument nicht für alle Inhalte Tags enthält, die Struktur-Informationen vermitteln könnten.

Dieser Prüfschritt ist nicht erfüllt, wenn das PDF-Dokument keine Tags enthält.

<a id="k-10-1-3-1b"></a>

#### 10.1.3.1b Info und Beziehungen: OCR

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#b-info-und-beziehungen-ocr) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#b-info-und-beziehungen-ocr) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Text enthält, der mittels OCR in eine maschinenlesbare Form übertragen wurde.

**Anforderung**

Der durch OCR in eine maschinenlesbare Form übertragene Text muss mit dem gescannten Text vollständig übereinstimmen.

Der Suspect-Eintrag wird gesetzt, wenn bei der optischen Zeichenerkennung (OCR) die Interpretation eines Zeichens nicht schlüssig ist.

**Warum wird das geprüft?**

PDF-Dokumente, die mittels OCR maschinenlesbar gemacht wurden, können orthografische Fehler enthalten, die von der Texterkennung herrühren.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Sofern die textlichen Informationen des PDF-Dokuments auch für Screenreader-Programme brauchbar sind, ist dieser Prüfschritt erfüllt.
- leichte Einschränkung: Vereinzelte OCR-Fehler erschweren das Verständnis von Worten wie beispielsweise „imrner“ statt „immer“.
- Einschränkung: Einige OCR-Fehler erschweren das Verständnis des erkannten Textes, machen das Verständnis aber nicht unmöglich.
- Barriere: Viele OCR-Fehler erschweren das Verständnis des erkannten Textes, machen das Verständnis aber nicht unmöglich.
- Blockade: Die im PDF-Dokument enthaltenen textlichen Informationen sind sowohl beim Lesen als auch für Screenreader-Programme vollkommen unbrauchbar.

Sofern die textlichen Informationen, die in maschinenlesbarer Form bereitgestellt werden, in Teilen nicht den optisch erkennbaren Inhalten entsprechen, ist dieser Prüfschritt nicht voll erfüllt.

<a id="k-10-1-3-1c"></a>

#### 10.1.3.1c Info und Beziehungen: Gruppierende Strukturelemente/Rootelemente

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#c-info-und-beziehungen-gruppierende-strukturelementerootelemente) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#c-info-und-beziehungen-gruppierende-strukturelementerootelemente) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Jedes PDF-Dokument benötigt genau ein gruppierendes Element als Root. Das gruppierende Element muss als erster Tag im PDF-Dokument platziert sein und alle anderen Tags umfassen.

**Warum wird das geprüft?**

Der Einsatz eines Rootelements (in den meisten Fällen "Document") als erster Tag eines PDF-Dokuments ist unverzichtbar. Der Einsatz weiterer gruppierender Rootelemente zur Unterteilung in zusammengehörende Teile ist für ein konformes PDF-Dokument nicht gefordert, steigert jedoch die Überschaubarkeit.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Einschränkung

Dieser Prüfschritt ist erfüllt, wenn das PDF-Dokument ein gruppierendes Element als Root besitzt (und zwar vorzugsweise "Document"). Das gruppierende Element muss als erster Tag im PDF-Dokument platziert sein. Andernfalls handelt es sich um eine Einschränkung.

<a id="k-10-1-3-1d"></a>

#### 10.1.3.1d Info und Beziehungen: Gruppierende Strukturelemente/Inhaltsverzeichnis

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#d-info-und-beziehungen-gruppierende-strukturelementeinhaltsverzeichnis) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#d-info-und-beziehungen-gruppierende-strukturelementeinhaltsverzeichnis) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument ein Inhaltsverzeichnis enthält.

**Anforderung**

Wenn im PDF-Dokument ein Inhaltsverzeichnis vorhanden ist, muss es korrekt getaggt sein.

**Warum wird das geprüft?**

Inhaltsverzeichnisse informieren über den Aufbau des PDF-Dokuments und geben Hinweise zur Positionierung der Inhalte des PDF-Dokuments. Wenn ein Inhaltsverzeichnis vorhanden ist, muss es als solches erkennbar sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Das PDF-Dokument enthält ein Inhaltsverzeichnis und dieses ist mit TOC ausgezeichnet und dessen Einträge sind mit TOCI ausgezeichnet.
- Einschränkung: Das Inhaltsverzeichnis ist nicht mit TOC ausgezeichnet bzw. dessen Einträge sind nicht mit TOCI ausgezeichnet.

<a id="k-10-1-3-1e"></a>

#### 10.1.3.1e Info und Beziehungen: Gruppierende Strukturelemente/BlockQuote, Index, NonStruct, Private

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#e-info-und-beziehungen-gruppierende-strukturelementeblockquote-index-nonstruct-private) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#e-info-und-beziehungen-gruppierende-strukturelementeblockquote-index-nonstruct-private) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument BlockQuote-, Index-, NonStruct- oder Private-Elemente enthält oder der Einsatz dieser Elemente erforderlich gewesen wäre.

**Anforderung**

Durch geeignetes Taggen wird auf semantischer Ebene eine Entsprechung des sichtbaren Inhalts geschaffen.

**Warum wird das geprüft?**

Gruppierende Strukturelemente informieren über den Aufbau des PDF-Dokuments. Es kommt zu Fehlinterpretationen, wenn sie nicht korrekt verwendet werden. Wird beispielsweise ein Text als Zitat markiert, obwohl es sich bei dem Text nicht um ein Zitat handelt, so ist dies irreführend.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn im PDF-Dokument gruppierende Strukturelemente korrekt verwendet werden.
- leichte Einschränkung: Gruppierende Elemente wie NonStruct oder Private werden verwendet, ohne die Übersichtlichkeit der Tag-Struktur merklich zu verbessern.
- Einschränkung: Gruppierende Strukturelemente könnten das PDF-Dokument leichter navigierbar machen, doch werden an Stelle von Div, Art, Part oder Sect nur die Elemente NonStruct und Private verwendet.
- Barriere: Gruppierende Strukturelemente werden nicht verwendet, obwohl dies der Übersichtlichkeit der Tag-Struktur dienlich wäre. Block-Zitate werden nicht mit BlockQuote ausgezeichnet, Indizes nicht mit Index.

Werden gruppierende Strukturelemente nicht korrekt verwendet, ist die aus deren Verwendung abzuleitende Fehlinterpretation ausschlaggebend für die Bewertung leichte Einschränkung bis Barriere.

<a id="k-10-1-3-1f"></a>

#### 10.1.3.1f Info und Beziehungen: Blocklevel Strukturelemente/Überschriften

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#f-info-und-beziehungen-blocklevel-strukturelementeüberschriften) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#f-info-und-beziehungen-blocklevel-strukturelementeüberschriften) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Überschriften enthält oder der Einsatz der Elemente H1-H6 erforderlich gewesen wäre.

**Anforderung**

Wenn im PDF-Dokument Überschriften verwendet werden, sollten sie korrekt ausgezeichnet werden. Die Kennzeichnung als Überschrift sollte nicht rein zu Layout-Zwecken eingesetzt werden, falls es sich beim entsprechend als Überschrift ausgewiesenen Bereich semantisch nicht um eine Überschrift handelt.

**Warum wird das geprüft?**

Visuell werden PDF-Dokumente in der Regel durch Überschriften strukturiert. Dank dieser Strukturierung ist erkennbar, welche Bereiche semantisch zusammengehören, und Nutzende können die Inhalte des PDF-Dokuments leichter überblicken und gezielt auf Inhalte zugreifen, die für sie von Interesse sind.

Menschen, die eine visuell erkennbare Strukturierung nicht nutzen können, zum Beispiel, weil sie blind sind oder nur einen kleinen Ausschnitt der Seite sehen können, sind darauf angewiesen, dass die Struktur unabhängig von der visuellen Darstellung auf dem Bildschirm zugänglich und nutzbar ist. Die Verwendung von Überschriften-Elementen ist dafür eine wesentliche Voraussetzung.

Screenreader-Programme bieten darüber hinaus die Funktionalität, dass durch einzelne Elementgruppen getabbt werden kann. D. h. mit einem Screenreader-Programm kann man Überschriften als zentrale Gliederungselemente nutzen und von einer Überschrift zur nächsten springen. Wenn keine Überschriften ausgezeichnet sind, muss eine blinde Person den ganzen Text auslesen lassen, um zum nächsten Bereich zu gelangen.

Tags, die nicht entsprechend ihrer Semantik verwendet werden (z. B. ein Überschriften-Tag wie H1 zum Hervorheben von Teilen eines Absatzes), führen bei Menschen, die auf semantische Informationen angewiesen sind (z. B. bei Screenreader-Nutzung), zu Missverständnissen bis hin zum Informationsverlust. Das soll durch korrektes Taggen vermieden werden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn im PDF-Dokument Überschriften verwendet und korrekt ausgezeichnet werden.
- leichte Einschränkung: Wird nur eine Überschrift verwendet und diese als H, nicht als H1 ausgezeichnet, handelt es sich um eine leichte Einschränkung.
- Einschränkung: Werden Überschriften verwendet, jedoch in der Nummerierung eine Überschriftenebene ausgelassen, handelt es sich um eine Einschränkung.
- Einschränkung: Ziehen sich Überschriften über mehrere Zeilen hin und wird jede Zeile neu mit einem H-Tag versehen, handelt es sich ebenfalls um eine Einschränkung wie beispielsweise bei: <H2> Dies ist eine </H2> (nächste Zeile) <H2> Überschrift </H2>

<a id="k-10-1-3-1g"></a>

#### 10.1.3.1g Info und Beziehungen: Blocklevel Strukturelemente/Listen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#g-info-und-beziehungen-blocklevel-strukturelementelisten) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#g-info-und-beziehungen-blocklevel-strukturelementelisten) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Listen enthält oder der Einsatz von Listen-Elementen erforderlich gewesen wäre.

**Anforderung**

Wenn im PDF-Dokument Listen und Auflistungen verwendet werden, sollten sie korrekt ausgezeichnet werden.

**Warum wird das geprüft?**

Mit Listen werden Informationen gruppiert. Sind Listen nicht korrekt ausgezeichnet, werden Menschen, die auf assistive Technologien angewiesen sind, entscheidende Informationen vorenthalten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn im PDF-Dokument Listen verwendet werden und die Listen korrekt ausgezeichnet sind.
- leichte Einschränkung: Listen sind ausgezeichnet, besitzen jedoch keine Lbl-Tags.
- Einschränkung: Nicht alle Listen des PDF-Dokuments sind ausgezeichnet.
- Barriere: Keine der im PDF-Dokument enthaltenen Listen ist als Liste ausgezeichnet, sodass die Strukturierung des Textes bei Nutzung von assistiven Technologien nicht erkennbar ist.
- Barriere: Listeneinträge sind nicht korrekt ausgezeichnet und können nicht navigiert werden.

<a id="k-10-1-3-1h"></a>

#### 10.1.3.1h Info und Beziehungen: Blocklevel Strukturelemente/Tabellen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#h-info-und-beziehungen-blocklevel-strukturelementetabellen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#h-info-und-beziehungen-blocklevel-strukturelementetabellen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Tabellen enthält oder der Einsatz von Tabellen-Elementen erforderlich gewesen wäre.

**Anforderung**

Wenn im PDF-Dokument Tabellen verwendet werden, sollten sie korrekt ausgezeichnet werden. Tabellen sollten nicht rein zu Layout-Zwecken eingesetzt werden, wenn dies semantisch nicht für die darin enthaltenen Strukturelemente geeignet ist.

**Warum wird das geprüft?**

Personen mit der Fähigkeit, sich visuell zu orientieren, können für die Orientierung in einer Datentabelle sowohl Überschriften als auch den Wertebereich nutzen. Für diesen Personenkreis ist es ggf. auch möglich, strukturelle Mängel wie zum Beispiel einen Wechsel in der Bedeutung von Zeilen oder Spalten zu erkennen und damit umzugehen.

Sehbehinderte und blinde Menschen erschließen sich das Angebot von Datentabellen dagegen eher analytisch. Sie entwickeln ausgehend von den Überschriften-Zellen und anderen im Kontext verfügbaren Informationen eine Vorstellung vom Aufbau der Tabelle. Diese Vorstellung ist die Grundlage für den Zugriff auf die angebotenen Daten. Damit das möglich ist und verlässlich funktioniert, müssen die folgenden Bedingungen erfüllt sein:

- Die Tabelle muss eine klare Struktur haben.
- Die Bedeutung der Zeilen und Spalten muss erfassbar und den Überschriften oder unterstützenden Kontextinformationen zu entnehmen sein.
- Überschriften müssen korrekt ausgezeichnet sein. D. h. Überschriften müssen auffindbar sein und es muss klar sein, auf welche Daten sie sich beziehen.

Eine klare Struktur ist die Grundlage für die Barrierefreiheit von Datentabellen. Es ist nicht möglich, eine mangelhaft strukturierte Datentabelle allein durch spezielle Auszeichnung barrierefrei zugänglich zu machen. Eine klare, nachvollziehbare Struktur ermöglicht die korrekte Auszeichnung, die z. B. bei Screenreader-Nutzung nützlich und unerlässlich ist.

Mögliche Anwendungen der Auszeichnung von Tabellenüberschriften in Screenreader-Programmen:

- Das Screenreader-Programm informiert über die Position und die Anzahl der Überschriftenreihen.
- Das Screenreader-Programm liest beim Wechsel der Tabellenzeile oder der Tabellenspalte die (neue) Zeilen- oder Spaltenüberschrift vor.
- Überschriften werden in einer besser geeigneten Form hervorgehoben.

Bei komplexen Tabellen können Screenreader-Programme allein aus dem Tabellengerüst nicht erschließen, welche Bezüge es zwischen Daten- und Überschriften-Zellen gibt. Deshalb müssen diese Verknüpfungen mithilfe der in PDF-Dokumenten zur Verfügung stehenden Tabellen-Attribute ausdrücklich definiert werden.

In der Praxis sind manchmal auch einfache Datentabellen mit header- und id-Attributen ausgezeichnet. Screenreader-Programme richten sich dann nicht nach den allgemeinen Regeln, sondern allein nach der vorliegenden spezifischen Auszeichnung. Deshalb muss eine vorgenommene Auszeichnung auch dann richtig sein, wenn sie eigentlich nicht nötig wäre. Denn bei falscher direkter Zuordnung von Datenzellen über Tabellen-Attribute werden die Überschriften nicht oder fehlerhaft ausgegeben.

Selbst bei korrekter Auszeichnung sind komplexe Tabellen bei Screenreader-Nutzung schwerer zu erfassen als einfache Tabellen. Daher ist es empfehlenswert, Tabellen mit mehreren logischen Ebenen generell zu vermeiden. Komplexe Tabellen können in vielen Fällen geteilt und durch mehrere einfache Tabellen ersetzt werden. Als Faustregel gilt: Nur wenn es einen semantischen Grund für eine komplexe Datentabelle gibt, sollte eine komplexe Datentabelle eingesetzt werden. Wenn immer möglich, sollten einfache Tabellen bevorzugt werden. Davon profitieren alle Personengruppen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn im PDF-Dokument Tabellen verwendet und korrekt ausgezeichnet wurden.
- leichte Einschränkung: Die Tabelle ist zwar korrekt ausgezeichnet, den Zeilen- oder Spalten-Überschriften fehlt jedoch das Scope-Attribut.
- Einschränkung: Eine Tabelle mit offensichtlichen Überschriften-Zellen am Anfang einer jeden Zeile besitzt nur Spalten-Überschriften.
- Barriere: Eine Tabelle besteht nur aus TD-Tags, besitzt aber visuell erkennbar Spalten-Überschriften.
- Blockade: In einer komplexen Tabelle mit mehrstufigen Überschriften-Zeilen wird kein Bezug zwischen Daten-Zellen und den korrespondierenden Überschriften-Zellen hergestellt.

Sind Tabellen nicht korrekt ausgezeichnet, handelt es sich mindestens um eine leichte Einschränkung, da die Zuordnung der Informationen in den einzelnen Tabellenzellen, wenn überhaupt, nur im Kontext möglich ist. Falls auch das nicht gelingen kann, handelt es sich um eine Blockade.

<a id="k-10-1-3-1i"></a>

#### 10.1.3.1i Info und Beziehungen: Blocklevel Strukturelemente/Absätze

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#i-info-und-beziehungen-blocklevel-strukturelementeabsätze) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#i-info-und-beziehungen-blocklevel-strukturelementeabsätze) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Absätze enthält oder der Einsatz von P-Elementen erforderlich gewesen wäre.

**Anforderung**

In einem PDF-Dokument sollten Absätze mit dem P-Tag gekennzeichnet sein. Separate Absätze sollten dabei nicht in einem einzigen P-Tag zusammengefasst sein. Darüber hinaus sollten leere P-Elemente vermieden werden.

**Warum wird das geprüft?**

Visuell werden PDF-Dokumente in der Regel durch Absätze strukturiert. Dank dieser Strukturierung ist erkennbar, welche Bereiche semantisch zusammengehören. Menschen, die eine visuelle erkennbare Strukturierung nicht nutzen können, zum Beispiel, weil sie blind sind oder nur einen kleinen Ausschnitt der Seite sehen können, sind darauf angewiesen, dass die Struktur unabhängig von der visuellen Darstellung auf dem Bildschirm zugänglich und nutzbar ist. Die Verwendung von Absatz-Elementen ist dafür eine wesentliche Voraussetzung.

Mit der Verwendung von P-Tags für jeden einzelnen Absatz ist die klare Abgrenzung von unterschiedlichen Absätzen auch beim Reflow gewährleistet (siehe Prüfschritt 10.1.4.10).

Leere P-Elemente sollten vermieden werden, um Irritationen zu meiden. Insbesondere könnte bei Screenreader-Nutzung diesbezüglich der Eindruck entstehen, dass vorhandene und ggf. relevante Inhalte fälschlicherweise nicht getaggt wurden und für das optimale Verständnis fehlen könnten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Dieser Prüfschritt ist erfüllt, wenn im PDF-Dokument alle Absätze korrekt ausgezeichnet wurden.
- leichte Einschränkung: Absätze sind vereinzelt nicht korrekt ausgezeichnet (z. B. mit Div)
- Einschränkung: Viele aufeinander folgende Absätze sind als nur ein Absatz ausgezeichnet.
- Barriere: Kein Absatz im PDF-Dokument ist mit P ausgezeichnet.

Wenn im PDF-Dokument nicht alle Absätze korrekt ausgezeichnet wurden, handelt es sich mindestens um eine leichte Einschränkung, da die Textstruktur nicht korrekt erfasst werden kann.

<a id="k-10-1-3-2"></a>

#### 10.1.3.2 Sinnvolle Reihenfolge

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sinnvolle-reihenfolge) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sinnvolle-reihenfolge) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument informationstragende Elemente enthält.

**Anforderung**

Wenn die Reihenfolge, in welcher Text präsentiert wird, dessen Bedeutung beeinflusst, muss die korrekte Textreihenfolge programmatisch ermittelbar sein.

**Warum wird das geprüft?**

Der Dokumenteninhalt muss auch mit assistiven Technologien so präsentiert werden, dass sein Inhalt verstanden werden kann. Wird beispielsweise von der assistiven Technologie nicht erkannt, dass ein Zeitungsartikel in zwei Spalten dargestellt wird, wird immer eine Zeile der ersten Spalte und folgend eine Zeile der zweiten Spalte vorgelesen. Die einzelnen Teile eines solchen Artikels heißen Beads. Der Faden, der die Beads verbindet, ist der Thread. Ein Screenreader-Programm kann mithilfe des Fadens von einem Bead zum nächsten führen.

Ein Mensch, der ein Screenreader-Programm benutzt oder das PDF-Dokument mithilfe einer Braille-Zeile ausliest, soll sich mit einem Menschen, der das PDF-Dokument visuell erfasst, optimal über die Inhalte austauschen können. Um unmissverständlich über den fünften Absatz oder aber den Absatz, der auf die dritte Bild-Datei folgt, diskutieren zu können, ist eine möglichst identische Abfolge unerlässlich.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Insbesondere darf bei für sich selbst stehenden Angaben innerhalb eines Bereichs eine gegenüber der visuellen Darstellung abweichende Reihenfolge übermittelt werden, beispielsweise bei der Angabe der verfassenden Person und der Angabe des Publikationsdatums.
- erfüllt: Die erste Seite eines PDF-Dokumentes ist aufgeteilt in eine Marginalspalte und einen Contentbereich, der auf der ersten Seite vollständig abgebildet ist (z. B. Angaben zu Titel sowie Abstract). Die Marginalspalte steht rechts. Die Reihenfolge, die an die assistive Technologie übermittelt wird, ist nicht relevant.
  - Menschen, die das PDF-Dokument visuell erschließen, könnten sich bei einer Schriftart, die von links nach rechts gelesen wird, zunächst der Marginalspalte zuwenden, denn hier stehen die Metadaten. Das ist aber nicht gesagt. Menschen, die das PDF-Dokument visuell erschließen, könnten aufgrund der Anordnung und der Lesart der Schriftart genauso gut mit dem Erfassen des Content-Bereichs beginnen, der links steht.
- erfüllt: Die erste Seite eines PDF-Dokumentes ist aufgeteilt in eine Marginalspalte und einen Contentbereich, der auf der ersten Seite nicht vollständig abgebildet ist, sondern erst auf der zweiten Seite. Auf der zweiten Seite folgt im Anschluss eine Überschrift. Die Marginalspalte steht rechts. Die Reihenfolge, die an die assistive Technologie übermittelt wird, ist nicht relevant, solange der Inhalt des Content-Bereichs zusammenhängend übermittelt wird.
- Einschränkung: Die erste Seite eines PDF-Dokumentes ist aufgeteilt in eine Marginalspalte und einen Contentbereich, der auf der ersten Seite nicht vollständig abgebildet ist. Der assistiven Technologie wird zuerst der Content-Bereich übermittelt. Der Inhalt der Marginalspalte wird nach Abschluss eines Absatzes, jedoch an einer inhaltlich nicht klar abgegrenzten Stelle eingeschoben. D. h. inmitten eines zusammengehörigen Bereichs.
- Barriere: Die erste Seite eines PDF-Dokumentes ist aufgeteilt in eine Marginalspalte und einen Contentbereich, der auf der ersten Seite nicht vollständig abgebildet ist. Der assistiven Technologie wird zuerst der Content-Bereich übermittelt. Der Inhalt der Marginalspalte wird inmitten eines Absatzes und ggf. überdies inmitten einer Silbentrennung eingeschoben. Damit lässt sich die Information des unterbrochenen Absatzes nur mit größter Anstrengung sinnvoll vervollständigen.
- leichte Einschränkung: Ein PDF-Dokument enthält eine Bild-Datei und eine optisch direkt folgende Bildunterschrift sowie einen Copyright-Vermerk. Der Alternativtext für das Bild sowie die Bildunterschrift werden direkt aufeinander folgend an die assistive Technologie übermittelt. Anschließend folgen zunächst andere Textpassagen, bevor schließlich auch der Copyright-Vermerk übermittelt wird.
- Einschränkung: In einem Change Log werden Daten zur Veröffentlichung von unterschiedlichen Versionen der im PDF-Dokument dargestellten Inhalte sowie Hinweise auf die jeweils vorgenommenen Änderungen aufgeführt. An die assistive Technologie werden zunächst alle Datumsangaben und anschließend alle Änderungshinweise übermittelt. Gültigkeit hat der jeweils letzte, vorliegende Stand. In diesem Fall lässt sich die Information, wann genau welche Anpassung im Einzelnen vorgenommen worden war, als nebensächlich einstufen.
- Einschränkung: Die optische Darstellung der einzelnen Bereiche des PDF-Dokuments ist nicht eingehalten. Die Information im PDF-Dokument kann dennoch sinnvoll aufgenommen werden. Ein sehender Mensch nimmt allgemeine Informationen, die z. B. in einer Prüfungssituation oder für das Ausfüllen von Formularfeldern relevant sind, deutlich früher wahr als Menschen, die ein Screenreader-Programm nutzen. Die Anforderung wäre auch im umgekehrten Fall als nicht erfüllt anzusehen.
- Einschränkung: Das PDF-Dokument enthält eine Information zu einer Kontoverbindung. Diesbezüglich sind eine Bezeichnung und der zugehörige Wert visuell nebeneinander angeordnet und ein erstes Thema (z. B. IBAN) steht dabei vor einem zweiten Thema (z. B. BIC). Der assistiven Technologie wird die Beziehung zwischen Bezeichnung und zugehörigem Wert korrekt übermittelt, jedoch in vertauschter Reihenfolge. Die Reihenfolge der Themen ist korrekt eingehalten.
  - visuell:
    - IBAN DE12 3456 7890 1234 5678 09
    - BIC BANKDEXXX
  - Screenreader-Programm: DE12 3456 7890 1234 5678 09 IBAN BANKDEXXX BIC
- Barriere: Eine Bezeichnung und der zugehörige Wert sind visuell nebeneinander angeordnet und ein erstes Thema (z. B. IBAN) steht dabei weiterhin vor einem zweiten Thema (z. B. BIC). Der assistiven Technologie wird jedoch die Reihenfolge Bezeichnung Bezeichnung Wert Wert übermittelt.
  - Screenreader-Programm: IBAN BIC DE12 3456 7890 1234 5678 09 BANKDEXXX
- Barriere: Bei Kontoverbindungen handelt es sich um Inhalte mit besonderer Relevanz, die ggf. schwerwiegende Konsequenzen nach sich ziehen können. Diesbezüglich könnte es als kritisch eingestuft werden, wenn für eine blinde Person die erste Angabe zu den Kontodaten etwas anderes ist als die erste Angabe zu den Kontodaten, die die sehende Assistenz dieser blinden Person wahrnimmt.
  - Eine Bezeichnung und der zugehörige Wert sind visuell nebeneinander angeordnet und ein erstes Thema (z. B. IBAN) steht dabei weiterhin vor einem zweiten Thema (z. B. BIC). Der assistiven Technologie wird die Beziehung zwischen Bezeichnung und zugehörigem Wert korrekt übermittelt, doch die Reihenfolge der Themen ist vertauscht.
    - Screenreader-Programm: BIC BANKDEXXX IBAN DE12 3456 7890 1234 5678 09
- Barriere: Eine Vielzahl von Einschränkungen ist als Barriere zu bewerten.
- Blockade: Der Text wird derart durcheinander übermittelt, dass beispielsweise Uhrzeiten einer Agenda dem jeweiligen Punkt nicht mehr zugeordnet werden können.
- Blockade: Bei einem Wahlgang sind mehrere Stimmzettel auszufüllen, z. B. für die Landeslisten sowie für die Wahlkreislisten und außerdem zu einem Volksentscheid. Je nach Stimmzettel können unterschiedlich viele Stimmen abgegeben werden. Die Information dazu, wie viele Stimmen jeweils abgegeben werden können, lässt sich nicht zuordnen.

<a id="k-10-1-3-3"></a>

#### 10.1.3.3 Sensorische Merkmale

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sensorische-merkmale) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sensorische-merkmale) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument sensorische Merkmale vorhanden sind.

**Anforderung**

Angaben zur Verständlichkeit der Inhalte bzw. zur Bearbeitung des PDF-Dokuments sollen nicht ausschließlich über sensorische Merkmale (wie insbesondere über Form, Farbe, Größe, Position, Ausrichtung oder Klang) gegeben werden.

**Warum wird das geprüft?**

Ziel dieses Prüfschritts ist sicherzustellen, dass auch Menschen, die nicht in der Lage sind, akustische Hinweise (Klang) bzw. Nutzung von Form, Farbe, Größe, Position und Ausrichtung als Unterscheidungsmerkmal wahrzunehmen, Verweise zur Nutzung des PDF-Dokuments verstehen können. Wird beispielsweise Wichtiges farblich hervorgehoben, können blinde oder sehbehinderte Menschen das nicht ohne Weiteres mit ihrer assistiven Technologie erkennen. Werden Pflichteingaben nur mit einem Hinweiston angezeigt, haben Menschen, die schlecht oder gar nicht hören können, keine Chance, dies wahrzunehmen.

Grundsätzlich sollte immer das Zwei-Sinne-Prinzip zum Einsatz kommen. Wichtiges kann beispielsweise farblich und zusätzlich mit dem Wort „Wichtig“, gekennzeichnet werden. Pflichteingaben sollten neben einem Hinweiston auch optisch, beispielsweise durch ein Sternchen-Zeichen (*) im Eingabefeld, gekennzeichnet werden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit nicht erfüllter Anforderung:

- leichte Einschränkung: Das PDF-Dokument enthält zwar Hinweise, die nur sehend bzw. hörend erfasst werden können (Form, Farbe, Größe bzw. Klang), doch schränken diese das Verstehen des PDF-Dokuments für blinde Menschen sowie Menschen mit Sehbeeinträchtigung bzw. Menschen mit eingeschränktem Hörvermögen nicht ein.
- Einschränkung: Das PDF-Dokument enthält Hinweise, die nur sehend bzw. hörend erfasst werden können (Form, Farbe, Größe bzw. Klang), welche das Verstehen des PDF-Dokuments für blinde Menschen sowie Menschen mit Sehbeeinträchtigung bzw. Menschen mit eingeschränktem Hörvermögen in der Form einschränken, dass die notwendigen Informationen aus dem Kontext gewonnen werden müssen („Hinweise sind rot markiert“).
- Barriere: Das PDF-Dokument enthält Hinweise, die nur sehend bzw. hörend erfasst werden können (Form, Farbe, Größe bzw. Klang), wodurch das Verstehen des PDF-Dokuments für blinde Menschen sowie Menschen mit Sehbeeinträchtigung bzw. Menschen mit eingeschränktem Hörvermögen nur mit erheblichem Aufwand möglich ist („Beachten Sie den grau hinterlegten Text“).
- Blockade: Das PDF-Dokument enthält Hinweise, die nur sehend bzw. hörend erfasst werden können (Form, Farbe, Größe bzw. Klang), wodurch das Verstehen des PDF-Dokuments für blinde Menschen sowie Menschen mit Sehbeeinträchtigung bzw. Menschen mit eingeschränktem Hörvermögen unmöglich ist (z. B. Fehlerhinweise).
- Blockade: Das PDF-Dokument enthält mehrere gleichartig gestaltete Objekte und es soll ein durch eine bestimmte Position gekennzeichnetes Objekt markiert werden, was für Menschen, die Informationen über die räumliche Position bzw. Ausrichtung nicht verwerten können unmöglich ist („Klicken Sie auf den Pfeil oben rechts.“).

<a id="k-10-1-3-4"></a>

#### 10.1.3.4 Bildschirmausrichtung anpassbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#bildschirmausrichtung-anpassbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#bildschirmausrichtung-anpassbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Die korrekte Darstellung des PDF-Dokuments darf nicht durch die gewählte Bildschirmausrichtung beeinflusst werden.

**Warum wird das geprüft?**

Einige Menschen wählen aufgrund ihrer Beeinträchtigung eine fixe Bildschirmausrichtung oder sind nicht in der Lage, diese anzupassen. Es gibt Augenkrankheiten, die das Gesichtsfeld begrenzen. Einige körperbeeinträchtigte Personen können die Ausrichtung ihres Bildschirms nicht selbstständig ändern.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Einschränkung
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Ein PDF-Dokument wird nach Drehen des Bildschirms ins Querformat oder ins Hochformat vollständig angezeigt. Eventuell abgeschnittene Inhalte lassen sich durch Scrollen anzeigen. Die Vergrößerungsstufe lässt sich anpassen.
- Einschränkung: Ein PDF-Dokument wird nach Drehen des Bildschirms ins Querformat bzw. ins Hochformat vergrößert dargestellt. Es kann zwar waagerecht und senkrecht gescrollt werden, aber die vergrößerte Darstellung lässt sich nicht ändern.
- Blockade: Ein PDF-Dokument wird nach Drehen des Bildschirms ins Hochformat bzw. ins Querformat nicht mehr vollständig angezeigt und waagerechtes bzw. senkrechtes Scrollen ist nicht möglich.

Nichtbeachtung bedeutet eine Blockade, unzureichende Umsetzung eine Einschränkung.

<a id="k-10-1-3-5"></a>

#### 10.1.3.5 Eingabezweck bestimmen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#eingabezweck-bestimmen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#eingabezweck-bestimmen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente zur Eingabe von personenbezogenen Daten wie Eingabefelder oder Auswahlschalter enthält.

**Anforderung**

Der Eingabezweck bei interaktiven Elementen zur Eingabe von personenbezogenen Daten kann programmatisch festgelegt und ermittelt sowie konsistent an assistive Technologien übermittelt werden oder ist im Text verfügbar.

**Warum wird das geprüft?**

Bei personenbezogenen Angaben gilt es, besondere Umsicht walten zu lassen.

Die Möglichkeit, die in einem bestimmten Feld erwarteten Daten programmatisch festzulegen, erleichtert das Ausfüllen von Formularen, insbesondere für Menschen mit kognitiven Einschränkungen.

Verständliche sichtbare Beschriftungen und Anweisungen sind notwendig, um den Zweck von Eingabefeldern zu verstehen. Eine Ausfüllhilfe mit passenden Vorschlägen, um sich häufig wiederholende Eingaben automatisch ausführen zu lassen, erleichtert allen Personenkreisen die Bedienung und stellt darüber hinaus insbesondere für Menschen mit kognitiven aber auch körperlichen Einschränkungen eine ganz besondere Erleichterung dar.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle Formularfelder für personenbezogene Daten besitzen korrekte sichtbare Beschriftungen (Label) und Tooltipps.
- leichte Einschränkung: Alle Formularfelder für personenbezogene Daten besitzen an sich korrekte sichtbare Beschriftungen (Label) und Tooltipps, die sich jedoch teilweise unterscheiden.
- Einschränkung: Formularfelder für personenbezogene Daten besitzen zwar sichtbare Beschriftungen, aber teilweise keine Tooltipps.
- Einschränkung: Formularfelder für personenbezogene Daten besitzen zwar Tooltipps, aber teilweise keine sichtbaren Beschriftungen.
- Einschränkung: Die sichtbaren Beschriftungen und Tooltipps von Formularfeldern für personenbezogene Daten sind teilweise unverständlich.
- Barriere: Formularfelder für personenbezogene Daten besitzen zwar sichtbare Beschriftungen, aber keine Tooltipps. Die Beschriftungen können von Screenreadern erfasst werden.
- Barriere: Formularfelder für personenbezogene Daten besitzen zwar geeignete Tooltipps, aber keine sichtbaren Beschriftungen.
- Barriere: Die sichtbaren Beschriftungen und gleichlautenden Tooltipps von Formularfeldern für personenbezogene Daten sind unverständlich.
- Blockade: Formularfelder für personenbezogene Daten besitzen zwar sichtbare Beschriftungen, aber keine Tooltipps, und die Beschriftungen können darüber hinaus von Screenreadern nicht erfasst werden.
- Blockade: Formularfelder für personenbezogene Daten besitzen zwar Tooltipps, aber keine sichtbaren Beschriftungen. Die Tooltipps sind darüber hinaus teilweise oder vollständig unstimmig oder fehlerhaft.
- Blockade: Die sichtbaren Beschriftungen und gleichlautenden Tooltipps von Formularfeldern für personenbezogene Daten sind unverständlich und weichen darüber hinaus voneinander ab.

<a id="k-10-1-4-1"></a>

#### 10.1.4.1 Verwendung von Farbe

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#verwendung-von-farbe) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#verwendung-von-farbe) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument Text bzw. Symbole sowie Bereiche oder sonstige visuelle Elemente mit Farbauszeichnungen verwendet werden.

**Anforderung**

Farbe wird nicht als einziges visuelles Mittel verwendet, um Informationen zu übermitteln, eine Aktion (Handlung) anzuzeigen, eine Antwort (Reaktion) auszulösen oder ein visuelles Element zu unterscheiden.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass alle relevanten Informationen insbesondere auch für ältere Personen bzw. Personen mit einer Sehbehinderung, bei der Farben nur eingeschränkt bzw. nicht ausreichend erkannt werden, zugänglich sind.

Dabei geht es explizit nicht darum, ob die entsprechenden Informationen von einem Screenreader-Programm ausgelesen werden könnten. Denn es kann nicht vorausgesetzt werden, dass dieser Personenkreis Screenreader-Programme nutzt.

Darüber hinaus können auch Personen, die Nur-Text-, Farb- oder Monochrom-Displays (z. B. eBook-Reader) und Browser verwenden, nicht auf Informationen zugreifen, welche nur in Farbe dargestellt werden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- Barriere: Text-Links im Fließtext werden nur farblich markiert und der Kontrast-Unterschied zwischen den Links und dem Fließtext ist geringer als 3:1.
- erfüllt: Bei einer Projektampel werden Zustände mit einem farblichen Kreis markiert (rot, orange, grün). In einem konkreten Fall wird der rote Kreis an oberster Position angezeigt, wobei sich der Farbwert hinsichtlich des Kontrastwerts von den unteren beiden Farbwerten (ggf. Schwarz oder Grau für „inaktiv“) unterscheidet. Darüber hinaus wird erläutert, dass das Projekt bei Aktivierung der oberen Position in eine kritische Phase geraten ist.
- Blockade: Bei einem Raumbelegungsplan werden Zustände nur mit einem farblichen Kreis markiert (rot, orange, grün).
- erfüllt: Bei einem Terminplan werden Zustände durch unterschiedlich farbige Formen markiert (rote Dreiecke, orange Rechtecke und grüne Kreise). Die Bedeutung der Merkmalskombinationen wird in Textform erläutert.
- erfüllt: Der aktive Menüpunkt in einem Anwendungsprogramm erhält als Abgrenzung zu den übrigen Menüpunkten eine grafische Markierung, etwa einen Rahmen oder einen Punkt. Der Menüeintrag erfolgt dabei in Form von Text.
- Blockade: Der aktive Menüeintrag unterscheidet sich nur durch Farbe von den anderen Menüeinträgen und der Kontrast zwischen aktiven und nicht-aktiven Menüeinträgen ist geringer als 3:1.
- erfüllt: Als Markierung wird eine Invertierung von Vorder- und Hintergrundfarbe bei ausreichendem Farbkontrast verwendet. Die Bedeutung der Markierung ist in Textform ersichtlich.
- erfüllt: Als Markierung wird eine Textfarbe verwendet. Zusätzlich wird z. B. ein Haken, Rahmen, eine Unterstreichung etc. als Unterscheidungsmerkmal verwendet. Die Bedeutung der Merkmalskombinationen wird in Textform erläutert.
- erfüllt: Rot wird verwendet, um zu warnen, dass eine Anwendung nicht betriebsbereit ist, oder um eine Notsituation anzuzeigen. Die Farbinformation wird durch einen Text ergänzt, beispielsweise „Warnung“ oder „Notfall“.
- erfüllt: Grün wird verwendet, um eine erfolgreiche Aktion zu bestätigen. Die Farbe wird durch ein grafisches Symbol (z. B. einen OK-Haken) ergänzt.
- Blockade: Ein Belegungsplan für Veranstaltungen verwendet für die Darstellung der Räume die Anordnung in Spalten. Zeiten werden in Zeilen angeordnet. Ob eine Raum-Zeit-Kombination frei oder gebucht ist, wird nur über unterschiedliche Farben in der jeweiligen Zelle angezeigt. Der Kontrast-Unterschied ist geringer als 3:1 und es gibt kein Merkmal wie ein Symbol, Fettschrift, Unterstreichung, Einrückung etc. als zusätzliches Kennzeichen. In der Legende wird überdies erläutert: Grün bedeutet „noch buchbar“, Rot bedeutet „ausgebucht“. Eine Identifizierungsmöglichkeit in Form eines Farbmusters gibt es in der Legende nicht.
- erfüllt: Ein Belegungsplan für Veranstaltungen verwendet für die Darstellung der Räume die Anordnung in Spalten. Zeiten werden in Zeilen angeordnet. Ob eine Raum-Zeit-Kombination frei oder gebucht ist, wird nur über unterschiedliche Farben in der jeweiligen Zelle angezeigt. Es gibt kein Merkmal wie ein Symbol, Fettschrift, Unterstreichung, Einrückung etc. als zusätzliches Kennzeichen, aber der Kontrast-Unterschied beträgt mindestens 3:1. In der Legende wird erläutert: Grün bedeutet „noch buchbar“, Rot bedeutet „ausgebucht“. Zusätzlich ist jeweils ein Farbmusters in der Legende abgebildet.
- Blockade: Das Ergebnis einer Befragung ist in einem Kreisdiagramm mit unterschiedlichen Farbtönen dargestellt. Die einzelnen Bereiche sind darüber hinaus nicht voneinander abgegrenzt. Ein Mensch, der die Farbwerte selbst nicht wahrnehmen kann, kann die einzelnen Bereiche nicht voneinander unterscheiden und hat demzufolge keinen Zugang zu den dargestellten Ergebnissen.
- Barriere: In einer tabellarischen Darstellung werden die Spalten abwechselnd mit zwei unterschiedlichen Hintergrundfarben gekennzeichnet. Die einzelnen Tabellenzellen haben keinen Rahmen. Die einzelnen Spalten folgen mit ausreichend großem Abstand aufeinander, sodass auch allein anhand der Position ermittelt werden kann, zu welcher Spalte die Inhalte gehören. Die Hintergrundfarbe soll das Erfassen der zu einer Spalte zugehörigen Inhalte erleichtern. Der Kontrast-Unterschied der gewählten Farbwerte ist geringer als 3:1. Die Einstufung als Barriere (im Unterschied zur Einstufung als Blockade) erfolgt hierbei aufgrund der Einschätzung, dass ein als unwesentlich eingestuftes Merkmal betroffen ist, denn durch den fehlenden Kontrast gehen keine Informationen verloren: Es kann auch ohne Farbwahrnehmung eindeutig ermittelt werden, welcher Spalte die einzelnen Inhalte zugeordnet sind.

<a id="k-10-1-4-2"></a>

#### 10.1.4.2 Ton abschaltbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Audio |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ton-abschaltbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ton-abschaltbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument Tonelemente verwendet werden, die automatisch abgespielt werden.

**Anforderung**

Automatisch abgespielte Töne, die nicht nach drei Sekunden enden, sollen abschaltbar sein.

**Warum wird das geprüft?**

Automatisch abgespielte Töne stören Personen, die sich auf einer Seite mittels Sprachausgabe orientieren.

Falls ein Tonelement automatisch startet, ist es deshalb wichtig, dass es sich über einen am Seitenbeginn befindlichen barrierefreien Mechanismus abschalten, anhalten oder herunterregeln lässt.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Barriere
- Blockade

Beispiele für Umsetzungen mit nicht erfüllter Anforderung:

- Barriere: Der Mechanismus zum Abschalten des Tons befindet sich nicht unmittelbar am Seitenbeginn. Es gibt am Seitenbeginn lediglich eine Erklärung, wie der Ton mittels Tastatur abschaltbar ist.
- Blockade: Automatisch abgespielter Ton enthält von Beginn an längere Sprachpassagen oder andere laute und störende Ton-Ereignisse. Die Sprachausgabe des Abschaltmechanismus kann deshalb nur schlecht wahrgenommen werden.

<a id="k-10-1-4-3"></a>

#### 10.1.4.3 Kontrast von Texten

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#kontrast-von-texten) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#kontrast-von-texten) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Das Kontrastverhältnis zwischen Vorder- und Hintergrundfarbe des Textes bzw. bei Schriftgrafiken in einem PDF-Dokument muss mindestens 4,5:1 betragen, bei großer Schrift mindestens 3:1. Davon ausgenommen sind die folgenden Fälle:

- Text bei deaktivierten Funktionalitäten,
- Text als Bestandteil von Schmuckelementen (Artefakt),
- Text als Bestandteil von Bildern, bei denen andere visuelle Inhalte überwiegen,
- Text als Bestandteil von Firmenlogos sowie
- Text, der für niemanden sichtbar ist.

**Warum wird das geprüft?**

Wenn Vordergrund- und Hintergrundfarbe sich in der Helligkeit ähneln, haben Texte unter Umständen zu wenig Kontrast. Gute Kontraste können für bessere Lesbarkeit von Texten sorgen. Insbesondere Menschen, die aufgrund einer verringerten Sehschärfe, einer Farbfehlsichtigkeit oder aufgrund des Alters eine verminderte Kontrastempfindlichkeit haben, profitieren von guten Kontrasten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle im PDF-Dokument enthaltenen Texte haben in allen Zuständen einen ausreichenden Kontrast.
- erfüllt: Ein Absende-Button kann erst ausgelöst werden, wenn alle erforderlichen Eintragungen in den Formularfeldern vorgenommen werden. Im deaktivierten Zustand müssen die Kontrastanforderungen nicht erfüllt sein.
- erfüllt: Die Beschriftung auf einem Straßenschild als Bestandteil eines Fotos einer verkehrsberuhigten Zone muss die Kontrastanforderungen nicht erfüllen.
- Barriere: Unwesentliche Textinhalte haben einen unzureichenden Kontrast.
- Blockade: Ganze Textabsätze haben einen unzureichenden Kontrast.

<a id="k-10-1-4-4"></a>

#### 10.1.4.4 Textgröße ändern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#textgröße-ändern) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#textgröße-ändern) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Text enthält.

**Anforderung**

Text soll um bis zu 200 % vergrößert werden können, ohne den Einsatz einer zusätzlichen Vergrößerungssoftware. Dabei darf kein Inhalt und keine Funktionalität verloren gehen und die Schrift darf außerdem nicht verpixelt dargestellt werden. Die verwendete Schriftart muss eine Skalierung ermöglichen.

**Warum wird das geprüft?**

Nutzende sollen die Schriftgröße ohne großen Aufwand gemäß ihren individuellen Bedürfnissen einstellen können.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Der Text lässt sich mit einem PDF-Anzeigeprogramm auf mindestens 200 % vergrößern, ohne dass es zu Textverlusten oder Verpixelungen kommt.
- Barriere: Eine Einstellung verhindert die Vergrößerung des PDF-Dokuments.
- Blockade: Bei einem Zoomfaktor von 200 % kommt es zu Verpixelungen bzw. zu Überlagerungen bzw. zu Textverlust, sodass der Text nicht mehr lesbar ist.

<a id="k-10-1-4-5"></a>

#### 10.1.4.5 Schriftgrafiken

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#schriftgrafiken) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#schriftgrafiken) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Schriftgrafiken enthält.

**Anforderung**

Grafiken sollen nicht für die Darstellung von Text verwendet werden. Logos sind hiervon ausgenommen.

**Warum wird das geprüft?**

Bilder bzw. Grafiken von Text (Schriftgrafiken), die als Bitmap-Grafik eingebunden werden (z. B. als JPEG, PNG, oder GIF), können nicht oder nur eingeschränkt an die individuellen Anforderungen von Nutzenden angepasst werden. Ihre Farben können nicht individuell eingestellt werden, auch die individuelle Anpassung der Schriftgröße wirkt nicht auf grafische Schriften. Bei Vergrößerung werden die Schriftkanten unscharf.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit nicht erfüllter Anforderung:

- Blockade: Für wichtige Texte, zum Beispiel Überschriften, Menüoptionen oder für Schaltflächen, deren Beschriftung optisch gelesen werden muss, werden Schriftgrafiken verwendet. Ein vorhandener Alt-Text reduziert die fehlende Zugänglichkeit nicht, da die optischen Nachteile verbleiben.

Der Prüfschritt ist nicht voll erfüllt, wenn für Schalter oder Schaltflächen Schriftgrafiken verwendet werden.

<a id="k-10-1-4-10"></a>

#### 10.1.4.10 Automatischer Umbruch (Reflow)

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#automatischer-umbruch-reflow) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#automatischer-umbruch-reflow) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Die Anpassung der Vergrößerung bei einer „normalen“ Fenster-Breite von 1280px nach Zoom auf 400 % (oder bei ungezoomter Ansicht bei einer Fenster-Breite von 320px) führt nicht zu einem Verlust an Inhalten oder Funktionalitäten. Die Inhalte sind ohne horizontales (waagerechtes) Scrollen vollständig erreichbar.

**Warum wird das geprüft?**

Personen mit Sehbehinderung vergrößern PDF-Dokumente häufig über die Zoomfunktion. Eine responsive Gestaltung, die abhängig vom Zoomfaktor und der verfügbaren Bildschirmbreite dynamisch den Zeilen-/Seitenumbruch für PDF-Dokumente anpasst, würde die Lesbarkeit von vergrößerten PDF-Dokumenten auf jeglichen Geräten verbessern.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Die Umfließen-Ansicht im PDF-Anzeigeprogramm lässt sich nicht aktivieren.
- erfüllt: In der Umfließen-Ansicht kommt es weder zu Textverlusten noch zu Verpixelungen und alle Funktionalitäten bleiben bedienbar.
- Blockade: In der Umfließen-Ansicht kommt es zu Textverlusten, weil Inhalte z. B. nicht mehr vollständig erreichbar sind, bzw. zu Verpixelungen und/oder zum Verlust von Funktionalitäten.

<a id="k-10-1-4-11"></a>

#### 10.1.4.11 Nicht-Text-Kontrast

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#nicht-text-kontrast) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#nicht-text-kontrast) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente, grafische Bedienelemente oder informationstragende Grafiken enthält.

**Anforderung**

Die visuelle Darstellung der folgenden Elemente hat ein Kontrastverhältnis von mindestens 3:1 zu benachbarten Farben:

- visuelle Informationen, die erforderlich sind, um Komponenten und Zustände zu identifizieren, mit Ausnahme interaktiver Elemente zur Eingabe von Daten oder wenn das Erscheinungsbild der Komponente vom PDF-Anzeigeprogramm bestimmt und bei der Erstellung des PDF-Dokuments nicht explizit geändert wird;
- Teile von Grafiken, die erforderlich sind, um den Inhalt einer Grafik zu verstehen, es sei denn, eine bestimmte Präsentation von Grafiken ist für die übermittelten Informationen wesentlich.

**Warum wird das geprüft?**

Wenn Vordergrund- und Hintergrundfarbe sich in der Helligkeit ähneln, haben Grafiken und grafische Bedienelemente unter Umständen zu wenig Kontrast. Gute Kontraste sorgen für bessere Bedienbarkeit und Verständnis von Dokumentinhalten

Insbesondere Menschen, die aufgrund einer verringerten Sehschärfe, einer Farbfehlsichtigkeit oder aufgrund des Alters eine verminderte Kontrastempfindlichkeit haben, profitieren von guten Kontrasten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Das PDF-Dokument enthält keine grafischen Bedienelemente und keine informationstragenden Grafiken.
- erfüllt: Alle grafischen Darstellungen bieten zum Umfeld und innerhalb des Bereichs ein Kontrastverhältnis von mindestens 3:1, wenn dies für Auffinden, Identifizieren und/oder inhaltliches Verständnis geboten ist.
- leichte Einschränkung: In einem Diagramm wird für Flächen die Kontrastanforderung nicht erfüllt, aber die Flächen sind zusätzlich eindeutig abgegrenzt und direkt beschriftet, und zwar nicht nur über eine gesonderte Legende.
- Einschränkung: In grafischen Darstellungen mit inhaltlicher Relevanz, z. B. in Diagrammen, erreicht der Kontrast in Teilen nicht die Mindestanforderung, aber der dargestellte Sachverhalt wird in einem unmittelbar an die jeweilige grafische Darstellung angrenzenden Absatz in Textform mit direkter Bezugnahme zusätzlich umfänglich dargestellt.
- Barriere: In grafischen Darstellungen mit inhaltlicher Relevanz, z. B. in Diagrammen, erreicht der Kontrast in Teilen nicht die Mindestanforderung. Der dargestellte Sachverhalt wird zusätzlich in Textform dargestellt, jedoch in einem nicht unmittelbar angrenzenden Bereich bzw. mit ungenauer Bezugnahme und/oder nicht vollumfänglich.
- Blockade: Interaktive Elemente und/oder deren Zustände weisen zur Umgebung und/oder zum alternativen Zustand nur Unterscheidungsmerkmale auf, deren Kontrastverhältnis unter 3:1 liegt.
- Blockade: Textfelder, Checkboxen etc. werden ohne Rahmen dargestellt. Das Steuerelement ist im nicht ausgefüllten bzw. nicht aktivierten Zustand nicht auffindbar.
- Blockade: Für das Verständnis benötigte Informationen werden nur bildhaft dargestellt und bieten zur Identifikation der sachlichen Information insgesamt oder in Teilen nur ein Kontrastverhältnis unterhalb 3:1. Eine stets sichtbare alternative textliche Wiedergabe des Sachverhalts ist nicht vorhanden.

<a id="k-10-1-4-12"></a>

#### 10.1.4.12 Textabstand

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#textabstand) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#textabstand) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Text enthält.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

Der Prüfschritt ist mit „nicht anwendbar“ zu bewerten, so lange kein gängiges PDF-Anzeigeprogramm in der Lage ist, Textabstände zuverlässig umzusetzen.

**Anforderung**

Die Anpassung der Zeilenhöhe auf das 1,5-Fache der Textgröße, des Abstands nach Absätzen auf das 2-Fache der Textgröße, von Buchstabenabständen auf das 0,12-Fache der Textgröße und von Wortabständen auf das 0,16-Fache der Textgröße führt nicht zu einem Verlust an Inhalten oder Funktionalitäten.

**Warum wird das geprüft?**

Für Menschen mit Sehbehinderung sowie mit Lesebeeinträchtigung, kann die Lesbarkeit von Texten durch individuelle Textgestaltung verbessert werden. Dies erfolgt durch Änderung der Abstände zwischen Zeilen, Absätzen, Zeichen und Worten. Solche Anpassungen führen dazu, dass Texte ggf. mehr Platz benötigen und Inhalte entsprechend dynamisch angelegt sein müssen, um den längeren Text ohne Verlust zu zeigen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Der Zeilenabstand lässt sich im PDF-Anzeigeprogramm nicht verändern.
- erfüllt: Mit geändertem Zeilenabstand kommt es nicht zu Textverlusten und alle Funktionalitäten bleiben bedienbar.
- Blockade: Mit geändertem Zeilenabstand kommt es zu Textverlusten, weil Inhalte z. B. nicht mehr vollständig erreichbar sind bzw. nicht mehr vollständig abgebildet werden, und/oder zum Verlust von Funktionalitäten.

<a id="k-10-1-4-13"></a>

#### 10.1.4.13 Eingeblendete Inhalte

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#eingeblendete-inhalte) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#eingeblendete-inhalte) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument Inhalte beim Überfahren mit der Maus (Hover) oder bei aktivem Tastaturfokus eingeblendet werden.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Wenn der Maus- oder der Tastaturfokus dazu führt, dass zusätzlicher Inhalt sichtbar und dann wieder ausgeblendet wird, gelten die folgenden Prinzipien:

- Ausblendbar: Es steht ein Mechanismus zur Verfügung, um den zusätzlichen Inhalt zu entfernen, ohne den Mauszeiger oder den Tastaturfokus zu bewegen, es sei denn, der zusätzliche Inhalt weist auf einen Eingabefehler hin bzw. verdeckt oder ersetzt keinen anderen Inhalt.
- Schwebend: Wenn der Mauszeiger den zusätzlichen Inhalt auslösen kann, kann der Zeiger über den zusätzlichen Inhalt bewegt werden, ohne dass der zusätzliche Inhalt verschwindet.
- Fixiert: Der zusätzliche Inhalt bleibt sichtbar, bis der Hover- oder Fokus-Trigger entfernt wird bzw. bis das Schließen durch eine bewusste Aktion veranlasst wurde oder bis die Informationen des zusätzlichen Inhalts nicht mehr gültig sind.

**Warum wird das geprüft?**

Menschen mit einer Sehbehinderung arbeiten häufig mit einer sehr starken Vergrößerung. Sie müssen die Möglichkeit haben, (ggf. unabsichtlich ausgelöste) zusätzliche Inhalte auszublenden bzw. zu schließen, ohne dafür den Fokus bewegen zu müssen, insbesondere wenn der zusätzliche Inhalt darunterliegende Informationen überlagert.

Für das Lesen einer Information kann unter Umständen besonders viel Zeit benötigt werden, wie beispielsweise aufgrund einer spezifischen Einschränkung aber auch bei einer Arbeitsunterbrechung oder Pausenzeit bzw. bei einer Störung von außen. Daher sollte der zusätzliche Inhalt nicht automatisch nach einer bestimmten Zeit ausgeblendet werden, sondern nur dann, wenn das Schließen in geeigneter Weise durch eine bewusste Aktion ausgelöst wurde.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Beim Überfahren mit der Maus (Hover) oder bei aktivem Tastaturfokus wird z. B. bei einem externen Verweis (Link) zusätzlicher Inhalt in Form einer Miniatur-Ansicht eingeblendet. Der eingeblendete Inhalt kann durch Drücken der Escape-Taste wieder ausgeblendet werden.
- erfüllt: Beim Überfahren mit der Maus (Hover) oder bei aktivem Tastaturfokus wird z. B. bei einem externen Verweis (Link) zusätzlicher Inhalt in Form einer Miniatur-Ansicht eingeblendet, ohne dass dadurch auch bei starker Vergrößerung andere Inhalte überdeckt werden.
- erfüllt: Zusätzlich eingeblendeter Inhalt ist programmatisch ermittelbar und wird von einem Screenreader-Programm korrekt wiedergegeben.
- leichte Einschränkung: Der zusätzlich eingeblendete Inhalt wird bei der kleinsten Mausbewegung geschlossen, obwohl sich der Zeiger noch immer über dem zusätzlichen Inhalt befindet.
- Einschränkung: Beim Überfahren mit der Maus (Hover) oder bei aktivem Tastaturfokus wird z. B. bei einem Button zusätzlicher Inhalt in Form eines Hilfetextes eingeblendet, der andere Inhalte überdeckt. Der zusätzlich eingeblendete Inhalt lässt sich nur durch eine Bewegung des Mauszeigers oder eine Verschiebung des Tastaturfokus ausblenden.
- Barriere: Der zusätzlich eingeblendete Inhalt wird unerwartet (z. B. aufgrund einer zeitlichen Limitierung) geschlossen, ohne dass dies durch eine bewusste Aktion veranlasst wurde.
- Blockade: Der zusätzlich eingeblendete Inhalt wird unerwartet (z. B. aufgrund einer zeitlichen Limitierung) geschlossen und lässt sich nur durch Schließen und erneutes Öffnen des PDF-Dokuments erneut aufrufen.
- Blockade: Der zusätzlich eingeblendete Inhalt, der andere Inhalte überdeckt, lässt sich nur durch Schließen und erneutes Öffnen des PDF-Dokuments ausblenden.
- Blockade: Zusätzlich eingeblendeter Inhalt ist programmatisch nicht ermittelbar und kann von einem Screenreader-Programm nicht wiedergegeben werden.

<a id="k-10-2-1-1"></a>

#### 10.2.1.1 Ohne Maus nutzbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ohne-maus-nutzbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ohne-maus-nutzbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Die gesamte Interaktion im PDF-Dokument muss ohne zeitliche Limitierung mittels Tastatur möglich sein, es sei denn die zugrundeliegende Funktionalität ist von einer konkreten Bewegung abhängig (auf dem Weg hin zu einem Endpunkt).

**Warum wird das geprüft?**

Ziel dieses Prüfschritts ist sicherzustellen, dass das PDF-Dokument mittels Tastatur oder eines Tastatursimulators bearbeitet werden kann. Die Bedienung soll geräteunabhängig möglich sein. Das bedeutet: Sie muss sowohl mit der Maus als auch mit der Tastatur möglich sein. Auf Tastaturbedienbarkeit angewiesen sind zum Beispiel viele motorisch eingeschränkte oder blinde Menschen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle Elemente, die für die Nutzung und das Verständnis des Inhalts des PDF-Dokuments notwendig sind, können erwartungskonform mit der Tastatur erreicht werden.
- leichte Einschränkung: Einige Elemente, die für die Nutzung und das Verständnis des Inhalts des PDF-Dokuments notwendig sind, können nur über ungewöhnliche Tasten-Kombinationen (z. B. zweimal Pfeil-Taste, einmal Tab-Taste) mit der Tastatur erreicht werden.
- Einschränkung: Alle Elemente, die für die Nutzung und das Verständnis des Inhalts des PDF-Dokuments notwendig sind, können nur über ungewöhnliche Tasten-Kombinationen (z. B. zweimal Pfeil-Taste, einmal Tab-Taste) mit der Tastatur erreicht werden.
- Barriere: Elemente, die für die Nutzung und das Verständnis des Inhalts des PDF-Dokuments notwendig sind, können nur über Umwege (z. B. die Objektnavigation im Screenreader-Programm NVDA) mit der Tastatur erreicht werden.
- Blockade: Elemente, die für die Nutzung und das Verständnis des Inhalts des PDF-Dokuments notwendig sind, können mit der Tastatur nicht erreicht werden.

Darüber hinaus je nach Relevanz der Funktionalität, die nicht mittels Tastatur ausführbar ist, Einschränkung bis Blockade.

<a id="k-10-2-1-2"></a>

#### 10.2.1.2 Keine Tastaturfalle

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#keine-tastaturfalle) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#keine-tastaturfalle) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Kann der Tastaturfokus über eine Tastaturschnittstelle zu einer Komponente des PDF-Dokuments verschoben werden, muss er von dort auch mit den Standardbeendigungsmethoden wieder wegbewegt werden können.

**Warum wird das geprüft?**

Ziel dieses Prüfschritts ist sicherzustellen, dass der Fokus nicht an einer bestimmten Stelle im PDF-Dokument fixiert und von dort mit der Tastatur nicht mehr gelöst werden kann. Die Bedienung soll geräteunabhängig möglich sein. Das bedeutet: Sie muss sowohl mit der Maus als auch mit der Tastatur möglich sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Es ist eine Standardbeendigungsmethode realisiert und man kann sich mithilfe der Tastatur vollständig durch das gesamte PDF-Dokument bewegen.
- Einschränkung: Es wurde eine nicht standardisierte Beendigungsmethode realisiert. Auf diese Methode wird explizit hingewiesen.
- Barriere: Es wurde eine nicht standardisierte Beendigungsmethode realisiert und die gewählte Beendigungsmethode nicht dokumentiert.
- Blockade: Der Tastaturfokus kann an einer Position gar nicht mehr bewegt werden.

<a id="k-10-2-1-4"></a>

#### 10.2.1.4 Tastatur-Kurzbefehle abschaltbar oder anpassbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#tastatur-kurzbefehle-abschaltbar-oder-anpassbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#tastatur-kurzbefehle-abschaltbar-oder-anpassbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument Tastatur-Kurzbefehle etabliert sind, welche allein auf Buchstaben (Groß- oder Kleinbuchstaben), Satzzeichen, Ziffern oder Symbolen basieren.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Sind Tastatur-Kurzbefehle etabliert, welche allein auf Buchstaben (Groß- und Kleinbuchstaben), Satzzeichen, Ziffern oder Symbolen basieren, muss mindestens einer der folgenden Punkte zutreffen:

- Deaktivierbar: Der Tastatur-Kurzbefehl muss abschaltbar sein.
- Anpassbar: Der Tastatur-Kurzbefehl muss dahingehend anpassbar sein, dass auf eine Tastenkombination mit Modifikator-Tasten umgestellt werden kann (z. B. nicht druckbare Tasten wie Alt oder Strg).
- Aktiv nur bei Fokussierung: Der Tastatur-Kurzbefehl darf nur aktiv sein bei Fokussierung des ihn betreffenden Objekts.

**Warum wird das geprüft?**

Das Arbeiten mit Tastatur-Kurzbefehlen ist besonders geeignet für Menschen, die geübt darin sind, die Tastatur zu bedienen. Die Anwendung von Tastatur-Kurzbefehlen kann andererseits aber auch eine Hürde darstellen, und zwar insbesondere für Menschen, die überwiegend mit Spracheingabe arbeiten oder Schwierigkeiten bei der Bedienung der Tastatur haben und Tasten bzw. eine Kombination aus Tasten z. B. nicht genau treffen können.

Es soll vermieden werden, dass Tastatur-Kurzbefehle unbeabsichtigt ausgeführt werden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Der Prüfschritt ist mit „nicht anwendbar“ zu bewerten, sofern mithilfe des angegebenen Testverfahrens keine Tastatur-Kurzbefehle dieser Art aufgespürt werden konnten.
- erfüllt: Zu Beginn des PDF-Dokuments wird auf geeignete Weise auf die Verwendung von Tastatur-Kurzbefehlen hingewiesen, welche allein auf Buchstaben (Groß- und Kleinbuchstaben), Satzzeichen, Ziffern oder Symbolen basieren. Darüber hinaus wird darauf hingewiesen, wie sich diese Tastatur-Kurzbefehle mit einer einfachen Aktion deaktivieren lassen.
- erfüllt: Zu Beginn des PDF-Dokuments wird auf geeignete Weise auf die Verwendung von Tastatur-Kurzbefehlen hingewiesen, welche allein auf Buchstaben (Groß- und Kleinbuchstaben), Satzzeichen, Ziffern oder Symbolen basieren. Darüber hinaus wird darauf hingewiesen, wie sich diese Tastatur-Kurzbefehle mit einer einfachen Aktion anpassen lassen.
- erfüllt: Im PDF-Dokument wird eine Video-Datei bereitgestellt. Der Maus- bzw. Tastaturfokus befindet sich auf der Video-Datei und das Abspielen der Video-Datei lässt sich mittels Leerzeichen-Taste starten bzw. stoppen.
- Barriere: Im PDF-Dokument wird eine Video-Datei bereitgestellt. Der Maus- bzw. Tastaturfokus befindet sich nicht auf der Video-Datei. Dennoch startet bzw. stoppt die Betätigung der Leerzeichen-Taste das Abspielen der Video-Datei und der Tastatur-Kurzbefehl lässt sich nicht deaktivieren bzw. anpassen. Dabei befindet sich der Maus- bzw. Tastaturfokus nicht auf einem Formularfeld.
- Blockade: Im PDF-Dokument wird eine Video-Datei bereitgestellt. Das Betätigen der Leerzeichen-Taste startet bzw. stoppt das Abspielen der Video-Datei und der Tastatur-Kurzbefehl lässt sich nicht deaktivieren bzw. anpassen, auch wenn sich der Maus- bzw. Tastaturfokus auf einem Formularfeld befindet.
- Barriere: Im PDF-Dokument ist als Tastatur-Kurzbefehl der Großbuchstabe P für das Drucken des gesamten Dokuments bzw. der Kleinbuchstabe p für das Drucken der aktuellen Seite implementiert (ohne weitere Modifikator-Taste) und der Tastatur-Kurzbefehl lässt sich nicht deaktivieren bzw. anpassen. Dabei befindet sich der Maus- bzw. Tastaturfokus nicht auf einem Formularfeld.
- Blockade: Im PDF-Dokument ist als Tastatur-Kurzbefehl der Großbuchstabe P für das Drucken des gesamten Dokuments bzw. der Kleinbuchstabe p für das Drucken der aktuellen Seite implementiert (ohne weitere Modifikator-Taste) und der Tastatur-Kurzbefehl lässt sich nicht deaktivieren bzw. anpassen, auch wenn sich der Maus- bzw. Tastaturfokus auf einem Formularfeld befindet.

<a id="k-10-2-2-1"></a>

#### 10.2.2.1 Zeitvorgaben anpassbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#zeitvorgaben-anpassbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#zeitvorgaben-anpassbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn im PDF-Dokument Tastatureingaben möglich sind und eine zeitliche Limitation vorgesehen ist.

**Anforderung**

Für jede im PDF-Dokument festgelegte zeitliche Limitation ist mindestens eine der folgenden Bedingungen erfüllt:

- Deaktivierbar: Das Zeitlimit lässt sich ausschalten, bevor es eintritt.
- Anpassbar: Das Zeitlimit lässt sich mindestens um das 10-Fache erweitern, bevor es eintritt.
- Verlängerbar: Nutzende werden vor Ablauf der Zeit gewarnt und erhalten mindestens 20 Sekunden Zeit, um das Zeitlimit mit einer einfachen Aktion zu verlängern (z. B. „Drücken Sie die Leerzeichen-Taste“), und eine solche Möglichkeit besteht mindestens zehn Mal.
- Echtzeit-Ausnahme: Das Zeitlimit ist ein notwendiger Bestandteil eines Echtzeit-Ereignisses (z. B. einer Auktion), und es gibt keine Alternative zum Zeitlimit.
- Wesentliche Ausnahme: Die Frist ist wesentlich, und ihre Verlängerung würde die Tätigkeit zunichtemachen.
- 20 Stunden-Ausnahme: Das Zeitlimit ist länger als 20 Stunden.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass Aufgaben ohne unerwartete Änderungen des PDF-Dokuments oder des Kontexts, die sich aus einem Zeitlimit ergeben, erledigt werden können.

Wenn sich Zeitbegrenzungen nicht abschalten oder verlängern lassen, können Menschen, die mehr Zeit für Eingaben brauchen, Transaktionen oftmals nicht rechtzeitig abschließen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Ein PDF-Dokument enthält kein Zeitlimit oder das Zeitlimit ist für die Nutzung des PDF-Dokuments notwendig. Das ist z. B. der Fall, wenn Fragen im Rahmen einer Prüfung innerhalb eines bestimmten Zeitraums beantwortet werden müssen.
- erfüllt: Ein im PDF-Dokument vorhandenes Zeitlimit ist länger als 20 Stunden.
- erfüllt: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich ausschalten, bevor es eintritt.
- erfüllt: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich mindestens um das 10-Fache erweitern, bevor es eintritt. Nutzende werden vor Ablauf der Zeit gewarnt und erhalten mindestens 20 Sekunden Zeit, um das Zeitlimit mit einer einfachen Aktion zu verlängern (z. B. „Drücken Sie die Leerzeichen-Taste“), wobei diese Möglichkeit mindestens zehn Mal besteht.
- leichte Einschränkung: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich nicht ausschalten, bevor es eintritt. Es lässt sich aber mindestens um das 10-Fache erweitern, bevor es eintritt. Nutzende werden außerdem vor Ablauf der Zeit gewarnt und erhalten mindestens 20 Sekunden Zeit, um das Zeitlimit mit einer einfachen Aktion zu verlängern (z. B. „Drücken Sie die Leerzeichen-Taste“). Allerdings besteht diese Möglichkeit weniger als zehn Mal.
- Einschränkung: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich nicht ausschalten, bevor es eintritt. Es lässt sich aber mindestens um das 10-Fache erweitern, bevor es eintritt. Nutzende werden auch vor Ablauf der Zeit gewarnt, erhalten aber weniger als 20 Sekunden Zeit, um das Zeitlimit mit einer einfachen Aktion zu verlängern (z. B. „Drücken Sie die Leerzeichen-Taste“), wobei diese Möglichkeit deutlich weniger als zehn Mal besteht.
- Barriere: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich nicht ausschalten, bevor es eintritt. Es lässt sich außerdem um weniger als das 10-Fache erweitern, bevor es eintritt. Nutzende werden zwar vor Ablauf der Zeit gewarnt, erhalten aber deutlich weniger als 20 Sekunden Zeit, um das Zeitlimit mit einer einfachen Aktion zu verlängern (z. B. „Drücken Sie die Leerzeichen-Taste“), wobei diese Möglichkeit nur ein oder zwei Mal besteht.
- Blockade: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich nicht ausschalten, bevor es eintritt. Es lässt sich außerdem um deutlich weniger als das 10-Fache erweitern, bevor es eintritt. Nutzende werden darüber hinaus vor Ablauf der Zeit nicht gewarnt und erhalten keinen Hinweis, wie sich das Zeitlimit mit einer einfachen Aktion verlängern lässt (z. B. „Drücken Sie die Leerzeichen-Taste“).
- Blockade: Ein im PDF-Dokument vorhandenes Zeitlimit lässt sich nicht ausschalten, bevor es eintritt, und lässt sich darüber hinaus auch nicht erweitern.

<a id="k-10-2-2-2"></a>

#### 10.2.2.2 Bewegte Inhalte abschaltbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#bewegte-inhalte-abschaltbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald das PDF-Dokument Inhalte aufweist, welche blinken, animiert sind oder sich selbstständig aktualisieren.

**Anforderung**

Für sich bewegende, blinkende, scrollende oder automatisch aktualisierende Informationen treffen alle folgenden Punkte zu:

**1. Bewegen, Blinken, Scrollen**

Für alle sich bewegenden, blinkenden oder scrollenden Informationen, die

- automatisch starten,
- länger als fünf Sekunden sichtbar sind und
- parallel zu anderen Inhalten präsentiert werden,

gibt es einen Mechanismus, mit dem Nutzende sie anhalten, stoppen oder ausblenden können, es sei denn, die Bewegung, das Blinken oder Scrollen ist Teil einer Aktivität, für die dieses unerlässlich ist.

**2. Automatische Aktualisierung**

Für alle sich automatisch aktualisierenden Informationen, die

- automatisch starten und
- parallel zu anderen Inhalten präsentiert werden,

gibt es einen Mechanismus, mit dem Nutzende sie anhalten, stoppen oder ausblenden oder die Häufigkeit der Aktualisierung kontrollieren können, es sei denn, die automatische Aktualisierung ist Teil einer Aktivität, für die dieses unerlässlich ist.

**Warum wird das geprüft?**

Animationen bzw. automatische Aktualisierungen stellen eine Ablenkung dar. Eine effektive Nutzung des PDF-Dokuments ist dann ggf. nicht möglich.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, dauern aber nicht länger als fünf Sekunden an. Es gibt keine Inhalte, die sich ohne aktive Veranlassung automatisch aktualisieren oder verändern.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, dauern aber nicht länger als fünf Sekunden an, und die automatische Aktualisierung von Inhalten lässt sich anhalten oder in ihrer Häufigkeit anpassen.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, dauern aber nicht länger als fünf Sekunden an, und alle Inhalte, die sich ohne aktive Veranlassung automatisch aktualisieren oder verändern, sind Teil einer Aktivität, für die dieses unerlässlich ist.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, lassen sich aber anhalten oder ausblenden. Es gibt keine Inhalte, die sich ohne aktive Veranlassung automatisch aktualisieren oder verändern.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, lassen sich aber anhalten oder ausblenden, und die automatische Aktualisierung von Inhalten lässt sich anhalten oder in ihrer Häufigkeit anpassen.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch, lassen sich aber anhalten oder ausblenden, und alle Inhalte, die sich ohne aktive Veranlassung automatisch aktualisieren oder verändern, sind Teil einer Aktivität, für die dieses unerlässlich ist.
- erfüllt: Im PDF-Dokument animierte Inhalte beginnen zwar automatisch und lassen sich zudem weder anhalten noch ausblenden, sind aber Teil einer Aktivität, für die dieses unerlässlich ist. Die Anforderungen an zusätzlich vorhandene automatische Aktualisierung sind erfüllt.
- leichte Einschränkung: Im PDF-Dokument animierte Inhalte beginnen mit langsamer Geschwindigkeit automatisch, ohne sich anhalten oder ausblenden zu lassen, und dauern etwas länger als fünf Sekunden (maximal zehn Sekunden) an. Die Anforderungen an zusätzlich vorhandene automatische Aktualisierung sind erfüllt.
- Einschränkung: Im PDF-Dokument animierte Inhalte beginnen mit langsamer Geschwindigkeit automatisch, ohne sich anhalten oder ausblenden zu lassen, und dauern maximal dreißig Sekunden an. Die Anforderungen an zusätzlich vorhandene automatische Aktualisierung sind erfüllt.
- Barriere: Im PDF-Dokument animierte Inhalte, die länger als fünf Sekunden andauern, beginnen automatisch und lassen sich nur umständlich anhalten oder ausblenden. Ggf. sind außerdem die Anforderungen an zusätzlich vorhandene automatische Aktualisierung nicht erfüllt.
- Blockade: Das PDF-Dokument enthält eine Vielzahl animierter Inhalte, die automatisch beginnen und länger als fünf Sekunden andauern, bzw. automatische Aktualisierungen, die alle einzeln angehalten oder ausgeblendet bzw. in ihrer Häufigkeit reguliert werden müssen.
- Blockade: Im PDF-Dokument animierte Inhalte beginnen automatisch und dauern länger als fünf Sekunden an. Sie lassen sich darüber hinaus nicht anhalten oder ausblenden. Ggf. sind außerdem die Anforderungen an zusätzlich vorhandene automatische Aktualisierung nicht erfüllt.

<a id="k-10-2-3-1"></a>

#### 10.2.3.1 Verzicht auf Flackern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#verzicht-auf-flackern) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#verzicht-auf-flackern) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald das PDF-Dokument Inhalte aufweist, welche blinken oder flackern können.

**Anforderung**

Wenn ein PDF-Dokument blinkende oder flackernde Elemente enthält, so dürfen innerhalb einer Sekunde maximal drei Blitze abgegeben werden. Grelles Flackern ist gänzlich zu vermeiden.

**Warum wird das geprüft?**

Bei Menschen mit Epilepsie kann längeres Flackern in bestimmten Frequenzen einen Anfall auslösen. Ferner können flackernde Inhalte das Lesen von PDF-Dokumenten erschweren. Dies betrifft nicht nur Menschen mit Sehbehinderung.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Das PDF-Dokument enthält keinerlei flackernde Elemente.
- Einschränkung: Das PDF-Dokument enthält Elemente, welche periodisch (beispielsweise alle zwei Sekunden) ihre Farbe verändern, aber dabei insbesondere nicht flackern. Diese Elemente befinden sich in unmittelbarer Nähe zu informatorischem Text. Die Farbveränderung kann Menschen mit spezifischen Sehbeeinträchtigungen ablenken und den Zugang zum Text erschweren.
- Barriere: Das PDF-Dokument enthält eines oder mehrere flackernde Elemente, welche ihren Farbzustand innerhalb einer Sekunde mindestens 3 verändern.
- Blockade: Das PDF-Dokument enthält eine Vielzahl an grell flackernden Elemente, welche ihren Farbzustand innerhalb einer Sekunde mindestens 3 verändern.

<a id="k-10-2-4-2"></a>

#### 10.2.4.2 Sinnvoller Dokumenttitel

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sinnvoller-dokumenttitel) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sinnvoller-dokumenttitel) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

PDF-Dokumente haben Dokumenttitel, die Thema oder Zweck des PDF-Dokuments beschreiben.

Der Dokumenttitel soll beim Öffnen des PDF-Dokuments anstelle des Dateinamens angezeigt werden.

**Warum wird das geprüft?**

Ein aussagekräftiger und eindeutiger Dokumenttitel ermöglicht es, fehlerfrei zwischen mehreren Dateien zu navigieren, insbesondere wenn mehrere Dokumente geöffnet sind, und bietet allen Menschen optimale Orientierung.

Der aussagekräftige Dokumenttitel ist außerdem speziell für Menschen, die Screenreader-Programme nutzen, wichtig, um PDF-Dokumente rasch identifizieren und zuordnen zu können. Dies betrifft insbesondere Menschen ohne bzw. mit eingeschränktem Sehvermögen.

Von einem präzisen Dokumenttitel profitieren darüber hinaus aber auch Menschen mit kognitiven Einschränkungen bzw. mit eingeschränktem Erinnerungsvermögen oder mit Leseschwäche sowie Menschen mit eingeschränkter Handhabung bzw. mit eingeschränkter Kraft, die Navigation mittels Audio ausführen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit nicht erfüllter Anforderung:

- leichte Einschränkung: Der Dokumenttitel ist nicht sinnvoll, er bezeichnet den Inhalt nicht aussagekräftig.
- leichte Einschränkung: Der Dokumenttitel ist nicht genau.
- Einschränkung: Der Dokumenttitel ist zu lang bzw. beinhaltet die wichtigen Informationen erst ganz am Ende.
- Einschränkung: Der Dokumenttitel enthält typographischen Schmuck (etwa , ~~, ====).
- Einschränkung: Der Dokumenttitel wird nicht beim Öffnen als Registerkarteninformation angezeigt. Stattdessen wird der Dateiname angezeigt.
- Barriere: Der Dokumenttitel ist bei mehreren Dokumenten mit unterschiedlichem Inhalt identisch.

<a id="k-10-2-4-3"></a>

#### 10.2.4.3 Sinnvolle Reihenfolge bei der Bedienung mit externer Tastatur

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-der-bedienung-mit-externer-tastatur) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sinnvolle-reihenfolge-bei-der-bedienung-mit-externer-tastatur) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Buttons oder Links enthält.

**Anforderung**

Beim Navigieren mit der Tastatur erhalten die Bedienelemente eines PDF-Dokuments den Fokus in einer Reihenfolge, die der logischen Struktur oder dem Bedienablauf entspricht.

**Warum wird das geprüft?**

Durch eine nicht nachvollziehbare Reihenfolge der fokussierten Elemente kann die Tastaturbedienbarkeit erheblich beeinträchtigt werden. Sehbehinderte oder blinde Menschen werden darüber hinaus in ihrer Fähigkeit eingeschränkt, sich ein mentales Modell vom Aufbau des PDF-Dokuments zu machen.

Falls der Tastaturfokus nach Nutzung eines Dialogs nicht automatisch wiederhergestellt wird, sodass zunächst (und ggf. zudem sehr häufig) die Tabulator-Taste betätigt werden muss, um zur vorigen Position zurückzukehren, so erschwert dies die Orientierung und beeinträchtigt die optimale Bedienung des PDF-Dokuments.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle Elemente, die per Tastatur erreichbar sein sollten, werden in einer sinnvollen und nachvollziehbaren Reihenfolge erreicht.
- leichte Einschränkung: Der Tastaturfokus springt bei der Tastaturnavigation bei ein oder zwei Elementen in einer nicht sinnvoll erscheinenden Reihenfolge, die Abfolge liefert aber insgesamt immer noch alle zum Verständnis des PDF-Dokuments notwendigen Informationen.
- Einschränkung: Der Tastaturfokus springt bei der Tastaturnavigation bei mehr als zwei Elementen in einer nicht sinnvoll erscheinenden Reihenfolge, die Abfolge liefert aber insgesamt immer noch alle zum Verständnis des PDF-Dokuments notwendigen Informationen.
- Einschränkung: Ein Formular besteht aus zwei nebeneinander angeordneten Blöcken, wobei der linke Block Eingabefelder für Kontaktdaten mit Name, Anschrift etc. bereithält und der rechte Block aus Checkboxen mit konkreten Auswahlmöglichkeiten zu Statusangaben wie „Sind Sie bereits Bestandskunde oder interessieren Sie sich für ein Angebot?“ oder „Wie sind Sie auf uns aufmerksam geworden?“ etc. besteht. Der Tastaturfokus springt bei der Tastaturnavigation abwechselnd von einem Element des linken zu einem Element des rechten Blocks.
- Einschränkung: Beim Öffnen eines Dialogfelds erhält ein Element den Fokus, das weder der aktuellen Aufgabe bzw. einer zu erwartenden Position, noch dem zuletzt aktivierten Element entspricht.
- Barriere: Beim Schließen eines Dialogfeldes kehrt der Fokus nicht zum auslösenden Element zurück, sondern landet am Anfang des PDF-Dokuments. Hinweis: Dabei kann es sich in seltenen Fällen um tatsächlich beabsichtigtes und sinnvolles Verhalten handeln, beispielsweise dann, wenn ein Eintrag in einem Dialog zusätzliche Formularbereiche oberhalb des auslösenden Elements freigeschaltet hat.
- Blockade: Die vorgegebene Reihenfolge bei der Tastaturnavigation verhindert ein sinnvolles Erfassen des PDF-Dokuments.

<a id="k-10-2-4-4"></a>

#### 10.2.4.4 Aussagekräftige Beschriftungen von Links

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#aussagekräftige-beschriftungen-von-links) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument Verweise (Links) enthält.

**Anforderung**

Der Zweck jedes Links kann allein aus dem Linktext oder aus dem Linktext zusammen mit seinem programmgesteuerten Linkkontext bestimmt werden, es sei denn, der Zweck des Links ist für Nutzende im Allgemeinen nicht eindeutig.

**Warum wird das geprüft?**

Screenreader-Programme lesen jedes Linkelement einzeln und sind auf den Namen des Elements angewiesen, um seinen Zweck zu erkennen. Daher sollten die Namen der Elemente innerhalb eines Kontextes eindeutig sein.

Wenn eine Linkbezeichnung mehrfach vorkommt, sollte ein Unterscheidungsmerkmal hinzugefügt werden, oder die erforderlichen Zusatzinformationen sollten aus dem Kontext einfach zu beschaffen sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Ein Formular ist in mehrere Abschnitte unterteilt, die jeweils durch einen Link „Bearbeiten“ für die Bearbeitung aktiviert werden. Das Screenreader-Programm gibt den Namen des Links aus und es kann mit einem Bedienschritt die Überschrift des Abschnitts ausgeben werden.
- erfüllt: In einer Anwendung symbolisiert der Link „Hilfe“ die kontextsensitive Hilfe zu bestimmten Bereichen. Im Tooltipp wird jeweils das Hilfethema genannt.
- Einschränkung: Ein Link wird nur als URL angezeigt und nicht als Linktext. Hinweis: Dieser Umstand ist nicht negativ zu bewerten, wenn der Link auch im PDF-Dokument als URL angezeigt wird und der Zweck des Links demzufolge für Nutzende im Allgemeinen nicht eindeutiger spezifiziert ist.
- Einschränkung: Es gibt mehrere Links mit der Bezeichnung „weiter“ im PDF-Dokument und der direkt den Link umgebende Text ist an jeder dieser Stellen identisch (z. B. „Gehen Sie weiter zu den Erläuterungen.“).
- Barriere: Ein für das Verständnis des PDF-Dokuments notwendiger Link wird nur als Symbol (z. B. Größerzeichen: >) angezeigt, besitzt aber keinen erläuternden ActualText bzw. „Originaltext“, wie er in Acrobat Pro genannt wird.
- Barriere: Ein Link befindet sich außerhalb eines Link-Tags. Screenreader-Programme geben dadurch beim Tabben nur „Link“ aus.

Nicht eindeutige Linktexte sind eine Barriere oder eine Einschränkung, je nachdem wie aufwändig es ist, notwendige Zusatzinformationen zu ermitteln.

<a id="k-10-2-4-6"></a>

#### 10.2.4.6 Aussagekräftige Überschriften und Beschriftungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#aussagekräftige-überschriften-und-beschriftungen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#aussagekräftige-überschriften-und-beschriftungen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Der Prüfschritt ist anwendbar, wenn Inhalte durch Überschriften strukturiert werden, Beschriftungen zu interaktiven Elementen zur Eingabe von Daten vorhanden sind oder wenn das PDF-Dokument Abkürzungen enthält.

**Anforderung**

**1. Überschriften**

Ein vorhandener sichtbarer Überschriftentext muss den zugehörigen Abschnitt hinreichend genau bzw. aussagekräftig beschreiben. Die jeweiligen Überschriften müssen zudem eindeutig, insbesondere im Gesamtkontext der im PDF-Dokument verwendeten Überschriften sein.

**2. Label/Beschriftungen**

Vorhandene sichtbare Beschriftungen von interaktiven Elementen zur Eingabe von Daten, wie Eingabefelder oder Auswahlschalter, müssen die dazugehörigen Interaktiv-Komponenten hinreichend genau bzw. sinngebend beschreiben. Die jeweiligen Beschriftungen müssen zudem eindeutig, insbesondere im Gesamtkontext der im PDF-Dokument verwendeten Beschriftungen sein. Dies kann auch in Verbindung mit z. B. Bereichsüberschriften hergestellt werden.

**3. Abkürzungen**

Es muss ein Mechanismus vorhanden sein, um die ausgeschriebene Form einer Abkürzung bzw. ihre Bedeutung, zu identifizieren.

**Warum wird das geprüft?**

**1. Überschriften**

Überschriften sind die wichtigsten Gliederungsmerkmale eines PDF-Dokuments. PDF-Dokumente werden durch Überschriften strukturiert. Dank dieser Strukturierung wissen Nutzende, was zusammengehört, können die Inhalte überblicken und gezielt auf Inhalte zugreifen bzw. danach suchen.

Menschen mit Seheinschränkung verwenden Überschriften zur Navigation. Diese technische Navigation in PDF-Dokumenten basiert ausschließlich auf Kennzeichnungen (Tags), die bereits im Quellsystem z. B. über Word-Formatvorlagen gesetzt werden müssen. Eine rein visuelle Kennzeichnung (z. B. Schriftgröße, Fett usw.) erreicht diese Strukturierung nicht.

Wenn Überschriften den Inhalt des folgenden Abschnitts nicht sinnvoll beschreiben bzw. organisieren, ist eine Navigation bzw. Orientierung anhand von Überschriften nicht möglich.

**2. Label/Beschriftungen**

Sinnvolle Beschriftungen von interaktiven Elementen zur Eingabe von Daten ermöglichen das Erkennen der Interaktiv-Komponenten und eine sinngemäße Bedienung.

**3. Abkürzungen**

Bei Abkürzungen, die nicht alltäglich sind, kann nicht vorausgesetzt werden, dass ihre Bedeutung allgemein bekannt ist. Bei Abkürzungen, die einen hohen Kontextbezug zum Dokumentinhalt haben, gilt dies insbesondere nicht nur für Menschen, die Schwierigkeiten mit der Dekodierung von Worten oder ein eingeschränktes Erinnerungsvermögen haben.

Darüber hinaus kann eine Abkürzung abhängig vom Kontext ganz unterschiedliche Bedeutungen aufweisen, insbesondere auch innerhalb ein und desselben PDF-Dokuments. Die Kurzform eines Wortes – ein Akronym – kann wiederum identisch sein mit einer Abkürzung bzw. kann wie ein Wort mit einer ganz anderen Bedeutung ausgesprochen werden.

Insbesondere bei der Nutzung eines Screenreader-Programms können Abkürzungen den Lesefluss erschweren und bei der Nutzung einer Bildschirmlupe besteht bei großem Vergrößerungsfaktor die Gefahr, den Kontext zu verlieren.

Indem eine Abkürzung erklärt bzw. mit einem Erweiterungstext versehen wird, werden Missverständnisse und Irritationen von vornherein vermieden. Ein solches Vorgehen erleichtert allen Personengruppen das Verständnis des gemeinten Sachverhalts.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle verwendeten Überschriften und Beschriftungen sind sinnvoll und eindeutig. Abkürzungen, deren Bedeutung zum Verstehen des Textes wichtig ist, werden bei erstmaligem Vorkommen erklärt bzw. mit Erweiterungstext versehen.
- leichte Einschränkung bzw. Einschränkung: Nicht eindeutige Beschriftungen oder Überschriften sind je nachdem zu bewerten, wie aufwändig es ist, notwendige Zusatzinformationen zu ermitteln.
- Einschränkung: Abkürzungen sind nur teilweise oder unzureichend erklärt bzw. mit Erweiterungstexten versehen. Dabei handelt es sich um eindeutige Abkürzungen, die aus anderen Quellen zweifelsfrei erschlossen werden könnten (siehe dazu auch das letzte Beispiel).
- Barriere: Sämtliche Abkürzungen werden weder erklärt noch im Tag-Baum mit Erweiterungstexten versehen. Dabei handelt es sich um eindeutige Abkürzungen, die aus anderen Quellen zweifelsfrei erschlossen werden könnten (erhöhter Aufwand, siehe dazu auch das letzte Beispiel).
- Barriere: Abkürzungen sind nur teilweise oder unzureichend erklärt bzw. mit Erweiterungstexten versehen. Dabei handelt es sich um Abkürzungen, die nicht eindeutig sind bzw. die einen hohen Kontextbezug zum Dokumentinhalt haben, sodass bestimmte Inhalte nicht erschlossen werden können. Diese sind jedoch nicht entscheidend für das Ausführen von Handlungen.
- Blockade: Wesentliche bzw. die meisten verwendeten Überschriften und/oder Beschriftungen sind nicht sinnvoll bzw. eindeutig. Das Verständnis der überschriebenen Inhalte bzw. die Bedienung der interaktiven Elemente zur Eingabe von Daten ist aufgrund der fehlenden Aussagekraft nicht möglich.
- Blockade: Sämtliche Abkürzungen werden weder erklärt noch im Tag-Baum mit Erweiterungstexten versehen. Dabei handelt es sich um Abkürzungen, die nicht eindeutig sind bzw. die einen hohen Kontextbezug zum Dokumentinhalt haben.
- Blockade: Auf einer Rechnung nimmt ein sehender Mensch "USt." automatisch als "Umsatzsteuer" wahr. Zu dieser Abkürzung gibt es im PDF-Dokument keine Erläuterung im Text und es ist auch kein Erweiterungstext vorhanden. Das Screenreader-Programm NVDA liest an dieser Stelle "ust" vor.

<a id="k-10-2-4-7"></a>

#### 10.2.4.7 Fokus sichtbar

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#fokus-sichtbar) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#fokus-sichtbar) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Buttons oder Links enthält.

Die in diesem Prüfschritt zu beurteilenden Kriterien werden ggf. durch das gewählte Anwendungsprogramm bzw. die assistive Technologie beeinflusst.

**Anforderung**

Jedes durch Tastatur bedienbare Bedienelement hat einen Bedienmodus, bei dem der Tastaturfokus sichtbar ist.

**Warum wird das geprüft?**

Für Menschen, die auf die Nutzung der Tastatur angewiesen sind, ist es wichtig zu sehen, wo sich der Tastaturfokus gerade befindet und welche Aktion also ausgelöst wird, wenn die Eingabe-Taste (Enter) gedrückt wird, bzw. an welcher Stelle eingegebene Zeichen erscheinen werden. Ohne sichtbaren Tastaturfokus ist ein PDF-Dokument für Menschen, die sich optisch orientieren, praktisch nicht tastaturbedienbar.

Die sichtbare Rückmeldung liefert Aufschluss darüber, ob ein Bildschirmelement bedienbar ist und ob es mit dem Zeigegerät erfolgreich fokussiert wurde und darüber hinaus ob eine angewählte Funktion verfügbar ist oder aber vorübergehend nicht funktioniert bzw. ob eine beabsichtigte Aktivierung erfolgreich war.

Eine verlässliche Rückmeldung von Bedienelementen gibt Sicherheit in der Bedienung. Dies trifft in besonderem Maß auf Menschen mit kognitiver Beeinträchtigung zu und kommt darüber hinaus vor allem bei selten genutzten Funktionen allen Menschen zugute.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Ein aktiviertes Element ändert sein Aussehen deutlich, so lange die Aktivierung andauert.
- leichte Einschränkung: Ein aktiviertes Element ändert sein Aussehen bei passendem Vergrößerungsfaktor deutlich. Der passende Vergrößerungsfaktor ergibt sich durch maximal zweimaliges Betätigen der Tastenkombination STRG+Pluszeichen.
- Einschränkung: Um die Schaltfläche, die fokussiert wird, erscheint ein gepunkteter Rahmen, der sich nicht deutlich abhebt.
- Einschränkung: Im Eingabefeld erscheint an der Stelle, an der die eingegebenen Zeichen eingefügt werden, als Text-Indikator ein blinkender senkrechter Strich, der nicht deutlich erkennbar ist.
- Einschränkung: Zustandsänderungen sind nicht deutlich erkennbar.
- Einschränkung: Der Tastaturfokus ist nur schwach sichtbar.
- Barriere: Der Tastaturfokus ist für mehr als 3 Tastenanschläge nicht sichtbar.
- Blockade: Der Tastaturfokus ist in ganzen Bereichen des PDF-Dokuments nicht sichtbar.
- Blockade: Zustandsänderungen sind nicht erkennbar.

<a id="k-10-2-5-1"></a>

#### 10.2.5.1 Alternativen für komplexe Gesten

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#alternativen-für-komplexe-gesten) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#alternativen-für-komplexe-gesten) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist nicht anwendbar.

<a id="k-10-2-5-2"></a>

#### 10.2.5.2 Abbrechen oder Widerrufen einer Touch-Geste

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#abbrechen-oder-widerrufen-einer-touch-geste) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Auswahlschalter, Buttons oder Links enthält.

**Anforderung**

Für Funktionalität, die unter Verwendung eines einzelnen Zeigers (d. h. mit einer Einpunkt-Geste) bedient werden können, gilt mindestens eine der folgenden Aussagen:

- Kein Down-Event: Das Down-Event (gedrückte Taste / Finger-Geste auf Element gedrückt) der Zeigeraktion wird nicht verwendet, um irgendeinen Teil der Funktion auszuführen.
- Abbrechen oder rückgängig machen: Der Abschluss der Funktion erfolgt auf dem Up-Event (losgelassene Taste / Finger lässt Element los, gehobener Finger) und es ist ein Mechanismus verfügbar, um die Funktion vor Abschluss abzubrechen oder die Funktion nach Abschluss rückgängig zu machen.
- Umkehrung des Up-Events: Das Up-Event kehrt alle Ergebnisse des vorhergehenden Down-Events um.
- Unverzichtbar: Der Abschluss der Funktion auf dem Down-Event ist unverzichtbar, wie insbesondere bei Funktionen, die einen Tastendruck auf einer Tastatur oder einem Nummernblock nachbilden.

**Warum wird das geprüft?**

Motorisch eingeschränkte Menschen können Zeigergesten mittels Maus, Touchpads, etc. eventuell nicht präzise ausführen. Personen ohne bzw. mit eingeschränktem Sehvermögen sind ggf. nicht in der Lage, eine versehentlich ausgelöste Funktion wahrzunehmen. Zur Fehlervermeidung muss daher gewährleistet werden, dass Eingaben rückgängig gemacht oder abgebrochen werden können. Eine solche Absicherung vor dem Ausführen einer Funktionalität kommt darüber hinaus allen Menschen zugute.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Wenn mit der linken Maustaste auf ein Bedienelement geklickt wird, wobei die Taste gehalten und nicht losgelassen wird, so lässt sich das Auslösen des Ereignisses dadurch verhindern, dass man den Mausfokus bei weiterhin gedrückter linker Maustaste vom Bedienelement entfernt (Anforderung: Kein Down-Event).
- erfüllt: Bedienelemente, die einen Tastendruck auf einem Nummernblock nachbilden, führen diese Funktion bereits beim Down-Event aus (Anforderung: Unverzichtbar).
- erfüllt: Eine Funktion (wie z. B. die Aktivierung einer bestimmten Auswahl) wird bereits dann ausgelöst, wenn mit der linken Maustaste auf das Bedienelement geklickt wird, wobei die Taste gehalten und nicht losgelassen wird. Das Up-Event, d. h. das Loslassen der Maustaste, kehrt die dabei ausgelösten Ereignisse um (und hebt z. B. die Auswahl wieder auf) (Anforderung: Umkehrung des Up-Events).
- erfüllt: Eine Funktion (wie z. B. die Aktivierung einer bestimmten Auswahl) wird erst dann ausgelöst, wenn zunächst mit der linken Maustaste auf das Bedienelement geklickt und die Maustaste anschließend losgelassen wurde (Up-Event). Die Aktion kann rückgängig gemacht bzw. abgebrochen werden (Anforderung: Abbrechen oder rückgängig machen).
- Einschränkung: Eine Funktion wird bereits dann ausgelöst, wenn mit der linken Maustaste auf das Bedienelement geklickt wird, wobei die Taste gehalten und nicht losgelassen wird. D. h. das Auslösen des Ereignisses lässt sich nicht mehr dadurch verhindern, dass man den Mausfokus bei weiterhin gedrückter linker Maustaste vom Bedienelement entfernt und die Aktion lässt sich durch das Up-Event außerdem nicht rückgängig machen, wobei der Abschluss der Funktion nicht als unverzichtbar angesehen wird. Die Auswirkungen der Funktionalität lassen sich jedoch ungeschehen machen, wie z. B. der Aufruf einer Webseite in einem Browserfenster durch das Schließen des neuen Browserfensters, sofern mit dem bloßen Aufruf der Webseite keine weitere Funktionalität verbunden ist (Anforderung: Down-Event und keine Umkehrung des Up-Events sowie nicht unverzichtbar).
- Barriere: Eine Funktion wird erst dann ausgelöst, wenn zunächst mit der linken Maustaste auf das Bedienelement geklickt und die Maustaste anschließend losgelassen wurde. Es wird jedoch kein Bestätigungsdialog angezeigt, über den die Aktion abgebrochen bzw. rückgängig gemacht werden könnte (Anforderung: Up-Event ohne abbrechen oder rückgängig machen).
- Blockade: Ein Schalter, welcher die Daten eines Formulars absendet, führt diese Funktion bereits beim Down-Event aus.
- Blockade: Eine Funktion wird bereits dann ausgelöst, wenn mit der linken Maustaste auf das Bedienelement geklickt wird, wobei die Taste gehalten und nicht losgelassen wird. D. h. das Auslösen des Ereignisses lässt sich nicht mehr dadurch verhindern, dass man den Mausfokus bei weiterhin gedrückter linker Maustaste vom Bedienelement entfernt und die Aktion lässt sich durch das Up-Event außerdem nicht rückgängig machen, wobei der Abschluss der Funktion nicht als unverzichtbar angesehen wird. Die Auswirkungen der Funktionalität lassen sich nicht ungeschehen machen (Anforderung: Down-Event und keine Umkehrung des Up-Events sowie nicht unverzichtbar).
- Blockade: Elemente können mittels Touch-Geste verschoben werden und lösen bereits beim Überfahren eines Feldes eine Aktion aus.

<a id="k-10-2-5-3"></a>

#### 10.2.5.3 Sichtbare Beschriftung Teil der programmatischen Beschriftung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sichtbare-beschriftung-teil-der-programmatischen-beschriftung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sichtbare-beschriftung-teil-der-programmatischen-beschriftung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Auswahlschalter, Buttons oder Links enthält.

**Anforderung**

Bei Bedienelementen, die mit textlichem Inhalt in Form von Text oder mit textlichem Inhalt in grafischer Form beschriftet sind, muss der Name der sichtbaren Bezeichnung entsprechen.

**Warum wird das geprüft?**

Wenn ein PDF-Dokument per Sprache gesteuert wird, dann werden Befehle auf Basis der sichtbaren Beschriftungen von Elementen gesprochen. Wird die sichtbare Beschriftung nicht in gleicher Form an die Accessibility API des Betriebssystems weitergegeben bzw. kommt die sichtbare Beschriftung im Accessible Name nicht vor, so kann das zu Missverständnissen und Fehlern bzw. zur Nicht-Nutzbarkeit des betreffenden Elements führen. Dies ist insbesondere für Menschen, die auf ein Screenreader-Programm angewiesen sind, relevant, um sich optimal mit Menschen, die das PDF-Dokument visuell erfassen, austauschen zu können.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Das PDF-Dokument enthält ausschließlich Bedienelemente, die nicht mit textlichen Inhalten gekennzeichnet sind.
- Einschränkung: Ein Formularfeld ist sichtbar beschriftet mit „Kfz-Kennzeichen“, Screenreader-Programme geben als Beschriftung hingegen „Nummernschild“ aus, wobei der Kontext aus dem umgebenden Text deutlich wird.
- Barriere: Ein Formularfeld ist sichtbar beschriftet mit „Kfz-Kennzeichen“, Screenreader-Programme geben als Beschriftung hingegen „Nummernschild“ aus, wobei der Kontext aus dem umgebenden Text nicht sofort deutlich wird.
- Einschränkung: Ein Button besitzt die sichtbare Beschriftung „weiter“, Screenreader-Programme geben als Beschriftung hingegen „zur nächsten Seite“ aus, wobei es auf der aktuellen Seite tatsächlich auch keine weiteren Inhalte gibt. Mit dem Ausführen der Aktion ist eine Navigation innerhalb des PDF-Dokuments verbunden.
- Barriere: Ein Button besitzt die sichtbare Beschriftung „weiter“, Screenreader-Programme geben als Beschriftung hingegen „zur nächsten Seite“ aus, wobei es auf der aktuellen Seite tatsächlich noch weitere Inhalte gibt und der mit dem Button verknüpfte Inhalt sich nicht auf der nächsten Seite des PDF-Dokuments befindet. Mit dem Ausführen der Aktion ist eine Navigation innerhalb des PDF-Dokuments verbunden und das PDF-Dokument bleibt dennoch nutzbar.
- Blockade: Ein Button besitzt die sichtbare Beschriftung „weiter“, Screenreader-Programme geben als Beschriftung hingegen „zur nächsten Seite“ aus, wobei mit Ausführen der Aktion ein Schließen des PDF-Dokuments verbunden ist.
- Blockade: Ein Formularfeld ist sichtbar beschriftet mit „Kfz-Kennzeichen“, Screenreader-Programme geben als Beschriftung hingegen „Vorname“ aus.

Entspricht der für die Assistenztechnologie zugängliche Name von Bedienelementen nicht der sichtbaren Beschriftung und ist das Bedienelement dadurch nicht vollumfänglich nutzbar bzw. verständlich, dann handelt es sich mindestens um eine Einschränkung.

<a id="k-10-2-5-4"></a>

#### 10.2.5.4 Steuerung durch Bewegung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#steuerung-durch-bewegung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#steuerung-durch-bewegung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist nicht anwendbar.

<a id="k-10-3-1-1"></a>

#### 10.3.1.1 Sprache des PDF-Dokuments

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sprache-des-pdf-dokuments) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sprache-des-pdf-dokuments) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Die Hauptsprache des PDF-Dokuments muss programmatisch ermittelbar sein.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass die textlichen Inhalte des PDF-Dokuments allen Personengruppen vollumfänglich zur Verfügung stehen.

Screenreader-Programme verwenden verschiedene Sprachsynthesizer für die jeweilige natürliche Sprache. Der Synthesizer bestimmt, ob die Texte eines PDF-Dokuments (Inhalte, Beschriftungen, Alternativtexte) richtig ausgesprochen werden. Bei einer falschen Festlegung der Hauptsprache des PDF-Dokuments kann es z. B. passieren, dass deutscher Text mit englischer Sprachkonvention vorgelesen wird und damit nicht mehr verständlich ist.

Wenn Menschen, die auf die Nutzung eines Screenreader-Programms angewiesen sind, manuell eingreifen müssen, um die Sprachausgabe umzuschalten, weil das Screenreader-Programm nicht automatisch zur richtigen Sprachausgabe wechselt, so behindert dies die Arbeitsabläufe.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Das korrekte Sprach-Attribut ist für das PDF-Dokument in den Metadaten hinterlegt.
- Blockade: In den Metadaten wurde kein oder das falsche Sprach-Attribut hinterlegt.

<a id="k-10-3-1-2"></a>

#### 10.3.1.2 Sprache von einzelnen Abschnitten/Sätzen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#sprache-von-einzelnen-abschnittensätzen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#sprache-von-einzelnen-abschnittensätzen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Die Sprache eines jeden Abschnitts des PDF-Dokuments muss programmatisch ermittelbar sein mit Ausnahme von Eigennamen, technischen Fachbegriffen, Wörtern einer unklaren Sprache und Wörtern oder Wendungen, die Teil der Umgangssprache des direkt umliegenden Textes geworden sind.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass die textlichen Inhalte des PDF-Dokuments allen Personengruppen vollumfänglich zur Verfügung stehen.

Screenreader-Programme verwenden verschiedene Sprachsynthesizer für die jeweilige natürliche Sprache. Der Synthesizer bestimmt, ob die Texte eines PDF-Dokuments (Inhalte, Beschriftungen, Alternativtexte usw.) richtig ausgesprochen werden.

Die Hauptsprache des PDF-Dokuments ist automatisch die Sprache für den gesamten textlichen Inhalt, die Metadaten sowie die zusätzlichen textlichen Passagen wie z. B. Alternativtexte. Wenn bei einzelnen Textpassagen, bei denen eine abweichende Sprache verwendet wird, kein programmatischer Sprachwechsel angegeben ist, sodass das Screenreader-Programm automatisch die richtige Sprachausgabe wählen können, so kann es z. B. passieren, dass deutscher Text mit englischer Sprachkonvention vorgelesen wird und damit nicht mehr verständlich ist.

Wenn Menschen, die auf die Nutzung eines Screenreader-Programms angewiesen sind, manuell eingreifen müssen, um die Sprachausgabe umzuschalten, weil das Screenreader-Programm nicht automatisch zur richtigen Sprachausgabe wechselt, so behindert dies die Arbeitsabläufe.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle Dokumentinhalte tragen das korrekte Sprach-Attribut.
- leichte Einschränkung: Wenige bzw. untergeordnete Textinhalte sind mit dem falschen Sprach-Attribut ausgezeichnet.
- Einschränkung: Wichtige Textinhalte, die für das Textverständnis ausschlaggebend sind, sind mit dem falschen Sprach-Attribut ausgezeichnet.
- Barriere: Wesentliche Textinhalte sind mit dem falschen Sprach-Attribut ausgezeichnet.
- Blockade: Alle Textinhalte sind trotzt korrekter Hauptsprache des PDF-Dokuments mit der falschen individuell benötigten Sprache ausgezeichnet.

<a id="k-10-3-2-1"></a>

#### 10.3.2.1 Keine Kontextänderung bei Fokus

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#keine-kontextänderung-bei-fokus) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#keine-kontextänderung-bei-fokus) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Auswahlschalter, Buttons oder Links enthält.

**Anforderung**

Wenn eine Dokumenten-Komponente wie ein Eingabefeld, Link oder Button den Fokus erhält, wird keine Kontextänderung ausgelöst.

**Warum wird das geprüft?**

Die bei Fokuserhalt auftretenden Effekte sollten vorhersehbar sein. Für Menschen, denen es nicht möglich ist, die gesamte Anzeige auf einmal zu sehen bzw. kognitiv zu erfassen, aber auch Menschen mit Leseschwäche sowie Menschen mit eingeschränkter Handhabung bzw. mit eingeschränkter Kraft, ist es unerlässlich, das PDF-Dokument in Ruhe erkunden zu können und zunächst Schritt für Schritt durch alle verfügbaren Bedienelemente navigieren zu können. Jede unbeabsichtigte Kontextänderung könnte zu Orientierungsverlust führen oder aber zunächst gar nicht bemerkt werden.

Somit muss es möglich sein, den Tastaturfokus im PDF-Dokument zu bewegen, ohne dadurch bei anderen Inhalten eine Wirkung hervorzurufen. Um darüber hinaus eine andere Wirkung auszulösen, sollte immer eine bewusste Aktion erforderlich sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Das PDF-Dokument enthält keine interaktiven Elemente, welche einen Fokus erhalten könnten.
- erfüllt: Alle interaktiven Elemente verhalten sich ihres Kontextes entsprechend.
- erfüllt: Eine Kontextänderung wird explizit ausgelöst, z. B. mit der Eingabe-Taste (Enter) oder einem Leerzeichen auf einem Bedienelement (Eingabefeld, Button, Link), aber nicht durch das Verlassen eines Eingabefeldes mit Tabulator-Taste oder Escape-Taste.
- Blockade: Eines oder mehrere interaktive Elemente verhalten sich entgegen ihrer Bestimmung (unerwartete Kontextänderung).
- Blockade: Ein Formular wird automatisch abgeschickt, sobald ein bestimmtes Eingabefeld den Fokus erhält.
- Blockade: Ein Dropdownmenü enthält Menüeinträge, welche mit Script-Aktionen verknüpft sind, und ein mit der Tastatur oder mittels Pfeil-Tasten angesteuerter Aktions-Menüeintrag wird direkt ausgelöst.
- erfüllt: Wie zuvor, aber die Menüeinträge können mit der Tastatur oder mittels Pfeil-Tasten beliebig durchlaufen werden, ohne dass eine Aktion ausgelöst wird. Die Aktion des fokussierten Menüeintrags wird erst durch Betätigen der Leerzeichen- oder Eingabe-Taste (Enter) ausgelöst. Mittels Tabulator-Taste oder Escape-Taste kann das Pulldownmenü verlassen werden, ohne dass eine Aktion ausgelöst wird.
- Blockade: Bei Fokus einer Checkbox öffnet sich automatisch ein Pop-up mit einem Hilfetext.
- Blockade: Wenn ein Eingabefeld den Fokus erhält, wird ein Hilfedialogfenster geöffnet, das das Feld beschreibt. Bei Navigation mittels Tabulator-Taste wird der Tastaturfokus direkt in das Hilfedialogfenster versetzt und nicht zum nächstfolgenden Element. Das nächstfolgende Element ist damit ggf. unerreichbar.

<a id="k-10-3-2-2"></a>

#### 10.3.2.2 Keine Kontextänderung bei Eingabe

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#keine-kontextänderung-bei-eingabe) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#keine-kontextänderung-bei-eingabe) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente zur Eingabe von Daten, wie Eingabefelder oder Auswahlschalter, enthält.

**Anforderung**

Wenn der Zustand einer Dokumenten-Komponente beispielsweise durch eine Eingabe oder die Auswahl einer Option eines Auswahlschalters bzw. das Ankreuzen einer Checkbox geändert wird, wird keine Kontextänderung ausgelöst, es sei denn, vor der Verwendung der Komponente wurde auf geeignete Weise über das Verhalten informiert.

**Warum wird das geprüft?**

Die bei einer Eingabe oder der Auswahl einer Option eines Auswahlschalters auftretenden Effekte sollten vorhersehbar sein. Jede unbeabsichtigte Kontextänderung könnte zu Orientierungsverlust führen oder aber zunächst gar nicht bemerkt werden. Dies betrifft insbesondere Menschen, denen es nicht möglich ist, die gesamte Anzeige auf einmal zu sehen bzw. kognitiv zu erfassen, aber auch Menschen mit Leseschwäche.

Somit muss es möglich sein, im PDF-Dokument Eingaben vorzunehmen, ohne dass dadurch eine Kontextänderung erfolgt. Um darüber hinaus eine andere Wirkung auszulösen, sollte immer eine bewusste Aktion erforderlich sein, es sei denn, vor der Verwendung der Komponente wurde auf geeignete Weise über das Verhalten informiert.

Indem vor der Bearbeitung ein Hinweis auf möglicherweise mit der Bearbeitung einhergehende Kontextänderungen gegeben wird, wird die Gefahr eines Orientierungsverlustes minimiert.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Das PDF-Dokument enthält keine interaktiven Elemente zur Eingabe von Daten.
- erfüllt: Alle interaktiven Elemente verhalten sich nach Eingabe von Daten bzw. Bedienung eines Auswahlschalters ihres Kontextes entsprechend.
- Blockade: Eines oder mehrere interaktive Elemente verhalten sich entgegen ihrer Bestimmung (unerwartete Kontextänderung nach Eingabe von Daten bzw. Bedienung eines Auswahlschalters).
- erfüllt: Mit einem Formular können Kalendereinträge erstellt werden. Neben den Standardfeldern für Betreff, Zeit und Ort gibt es weitere Eingabefelder, beispielsweise Termin oder Besprechung. Wird das Optionsfeld für Besprechung angewählt, werden unterhalb zusätzliche Felder für die Eingabe der Teilnehmenden angezeigt. Wird darüber hinaus die Reminder-Funktion ausgewählt, erscheinen weitere Auswahlmöglichkeiten, die in direktem Zusammenhang mit der Reminder-Funktion stehen. Derartige Änderungen, die in unmittelbarem Zusammenhang mit der Bestimmung des Elements stehen, sind als geringfügig einzustufen, so lange die generelle Struktur unverändert bleibt. Es ist insbesondere kein sichtbarer Text notwendig, der auf dieses Verhalten hinweist.
- erfüllt: Ein Eingabefeld für 20 Zahlen ist in fünf separate Zahlenblöcke unterteilt. Sobald vier Zahlen eingegeben wurden, springt der Fokus automatisch zum nächsten Zahlenblock weiter. Vor Beginn der Eingabe wird auf dieses Verhalten hingewiesen.
- Blockade: Bei Markierung einer Checkbox öffnet sich automatisch ein Pop-up mit einem Hilfetext.
- Blockade: Nach der Auswahl einer Checkbox werden oberhalb des Feldes weitere Eingabefelder angezeigt. Ein gibt keinen sichtbaren Text, der auf dieses Verhalten hinweist.

<a id="k-10-3-3-1"></a>

#### 10.3.3.1 Fehlererkennung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#fehlererkennung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#fehlererkennung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn in einem PDF-Dokument Fehlermeldungen erzeugt werden, beispielsweise durch Fehleingaben bei Eingabefeldern.

**Anforderung**

Wenn ein Eingabefehler automatisch erkannt wird, wird das fehlerhafte Element identifiziert und der Fehler im Text beschrieben.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass Fehlermeldungen bei einer fehlerhaften Eingabe deutlich gekennzeichnet und gut auffindbar sind.

Menschen, die nur einen kleinen Ausschnitt des Bildes sehen, bemerken einen Fehlerzustand möglicherweise nicht. Sie müssen daher zur Fehlermeldung und zur korrigierenden Eingabe hingeführt werden. Darüber hinaus ist sicherzustellen, dass die Fehlermeldung auch für Menschen mit Farbenblindheit deutlich erkennbar ist sowie für Menschen mit kognitiven Einschränkungen bzw. mit eingeschränktem Sprachvermögen oder mit Leseschwäche.

Manchen Menschen fällt es besonders schwer, einen Hinweis, der allein durch ein Icon gegeben wird, richtig zu interpretieren. Eine unmissverständliche Beschreibung, wofür ein Icon steht, erleichtert allen Menschen das korrekte Verständnis der zugeordneten Bedeutung. Die Kennzeichnung beispielsweise mittels doppeltem Sternchen-Zeichen (*) direkt an einer Fehlerstelle ermöglicht es allen Menschen, Fehlerstellen mittels Suchfunktion aufzufinden.

Für Menschen mit Beeinträchtigungen ist in vielen Fällen das Risiko von Fehleingaben größer. Menschen mit Legasthenie beispielsweise vertauschen häufiger Zahlen, Menschen mit motorischen Einschränkungen drücken häufiger versehentlich falsche Tasten. Deshalb sind alle Maßnahmen zur Überprüfung der Eingaben für diese Personengruppen besonders wichtig. Darüber hinaus ist es für alle Menschen hilfreich, einen deutlich Hinweis darauf zu erhalten, dass sich ein Formular aufgrund einer fehlerhaften oder fehlenden Eingabe nicht abschicken lässt – und nicht etwa aufgrund einer Störung der Funktionalität des Absendebuttons.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Das PDF-Dokument enthält keine interaktiven Elemente zur Eingabe von Daten, wie Eingabefelder oder Auswahlschalter.
- nicht anwendbar: Das PDF-Dokument enthält interaktive Elemente zur Eingabe von Daten, doch für keines der Elemente wurde eine Fehlererkennung eingestellt, sodass im Fehlerfall keine Fehlermeldung ausgegeben wird.
- erfüllt: Alle Fehleingaben werden mit deutlich lesbaren Fehlermeldungen quittiert. Die Gestaltungsregeln sind eingehalten.
- Blockade: In der Fehlermeldung ist nicht klar gekennzeichnet, an welcher Stelle es bei der Eingabe zu einem Fehler gekommen ist.
- erfüllt: Nach dem Auftreten des Fehlers startet ein Fehlerdialog, der den Fokus erhält.
- erfüllt: Der Fehler wird in Textform innerhalb eines Fehlerdialoges beschrieben. Der Fehlerzustand kann zusätzlich beispielsweise durch einen Signalton oder ein Icon oder die Kennzeichnung „Fehler“ im Titel der Meldung erfolgen.
- erfüllt: Ein fehlerhaft ausgefülltes Feld wird deutlich hervorgehoben, etwa durch einen farblich abgesetzten Rahmen sowie zusätzlich ein Zeichen in Textform wie z. B. ein Sternchen-Zeichen (*). In der Label-Beschriftung werden der Fehler und das fehlerhafte Feld beschrieben.
- erfüllt: Es erfolgt eine automatische Eingabeprüfung mit deutlichem Hinweis auf automatisch vorgenommene Korrekturen, sodass die Richtigkeit der korrigierten Daten im Anschluss nochmals überprüft werden kann.
- Blockade: Der Fehlerzustand wird nur durch einen farbigen Rahmen angezeigt.

<a id="k-10-3-3-2"></a>

#### 10.3.3.2 Verständliche Beschriftungen oder Anweisungen bei Formularfeldern

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#verständliche-beschriftungen-oder-anweisungen-bei-formularfeldern) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#verständliche-beschriftungen-oder-anweisungen-bei-formularfeldern) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente zur Eingabe von Daten wie Eingabefelder oder Auswahlschalter enthält.

**Anforderung**

Interaktive Elemente zur Eingabe von Daten sind mit einer sichtbaren Beschriftung (Label) oder einer Anweisung versehen.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass bei interaktiven Elementen zur Eingabe von Daten entweder sichtbare Beschriftungen oder Anweisungen bereitgestellt sind.

Sinnvolle und verständliche sichtbare Beschriftungen und Anweisungen sind notwendig, um den Zweck von Eingabefeldern zu verstehen. Die Anordnung von Beschriftungen direkt vor oder über dem Eingabefeld entspricht den üblichen Gestaltungskonventionen.

Dabei muss auch in ausschnitthaften Ansichten (etwa in Vergrößerungssoftware) deutlich erkennbar sein, welche Beschriftung zu welchem Feld gehört, um insbesondere Menschen, die nur einen kleinen Ausschnitt des Bildes sehen und auf die Nutzung von Vergrößerungssoftware angewiesen sind, alle benötigten Informationen vollumfänglich zur Verfügung zu stellen.

Bei kombinierten Eingabeelementen ist es für Menschen mit Sehbeeinträchtigung bzw. eingeschränktem Sehvermögen insbesondere bei Screenreader-Nutzung unverzichtbar, dass jedes einzelne Auswahlfeld mit einer programmatisch ermittelbaren Beschriftung versehen ist.

Ohne sinnvolle Hinweise auf Pflichtfelder sowie auf ggf. vorgegebene Eingabeformate wird die Bearbeitung des PDF-Dokuments für alle Menschen unnötig erschwert. Konkrete Angaben sowie Platzhalter-Text kommen entsprechend allen Menschen zugute und sind insbesondere für Menschen mit kognitiven Einschränkungen bzw. mit eingeschränktem Sprachvermögen oder mit Lern- bzw. Leseschwäche eine unverzichtbare Unterstützung.

Für Menschen mit Beeinträchtigungen ist in vielen Fällen das Risiko von Fehleingaben größer. Menschen mit Legasthenie beispielsweise vertauschen häufiger Zahlen, Menschen mit motorischen Einschränkungen drücken häufiger versehentlich falsche Tasten. Deshalb sind konkrete Angaben dazu, welche Eingaben erwartet werden und wie Eingabefelder korrekt auszufüllen sind, für diese Personengruppen besonders wichtig.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Alle Formularfelder haben eine sichtbare Beschriftung bzw. Anweisung. Pflichtfelder sind eindeutig gekennzeichnet und Platzhalter-Text entspricht den Vorgaben.
- leichte Einschränkung: Die Pflichtfeld-Markierung, z. B. durch ein Sternchen-Zeichen (*) wird nicht erläutert.
- Einschränkung: Beschriftungen sind optisch nicht eindeutig dem zugehörigen Formularfeld zuzuordnen.
- Barriere: Beschriftungen werden ausschließlich in Form von Platzhalter-Text bereitgestellt.
- Barriere: Vorgegebene Eingabeformate werden ausschließlich in Form von Platzhalter-Text bereitgestellt.
- Blockade: Wichtige Formularelemente (z. B. die Sucheingabe) sind ohne sichtbare Beschriftung. Die Funktion wird auch in angrenzenden Elementen nicht erklärt.

<a id="k-10-3-3-3"></a>

#### 10.3.3.3 Fehlerkorrektur

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#fehlerkorrektur) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#fehlerkorrektur) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn in einem PDF-Dokument Fehlermeldungen erzeugt werden, beispielsweise durch Fehleingaben bei Eingabefeldern.

**Anforderung**

Wenn ein Eingabefehler automatisch erkannt wird und Korrekturvorschläge bekannt sind, werden die Vorschläge konkret benannt, es sei denn, dies würde die Sicherheit oder den Zweck des Inhalts gefährden.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass Fehlermeldungen bei einer fehlerhaften Eingabe hilfreiche Korrekturvorschläge enthalten, falls möglich.

Fehler lassen sich mithilfe von konkreten Hinweisen leichter bzw. überhaupt erst beheben, beispielsweise wenn bei der Festlegung eines Passwortes sehr konkrete Kriterien zugrunde gelegt werden, die an keiner Stelle benannt werden. Der Hinweis darauf, dass diesbezüglich ggf. nur gängige Sonderzeichen zulässig sind, könnte einen entscheidenden Hinweis auf die Korrektur der Eingabe liefern.

Dabei ist sicherzustellen, dass die Fehlermeldungen auch für Menschen mit Sehbeeinträchtigung bzw. eingeschränktem Sehvermögen erfassbar sind sowie für Menschen mit kognitiven Einschränkungen bzw. mit eingeschränktem Sprachvermögen oder mit Lern- bzw. Leseschwäche und darüber hinaus auch für Menschen mit eingeschränkter Handhabung bzw. mit eingeschränkter Kraft.

Für Menschen mit Beeinträchtigungen ist in vielen Fällen das Risiko von Fehleingaben größer. Menschen mit Legasthenie beispielsweise vertauschen häufiger Zahlen, Menschen mit motorischen Einschränkungen drücken häufiger versehentlich falsche Tasten. Deshalb sind alle Maßnahmen zur Überprüfung der Eingaben für diese Personengruppen besonders wichtig. Darüber hinaus ist es für alle Menschen hilfreich, einen deutlich Hinweis darauf zu erhalten, dass sich ein Formular aufgrund einer fehlerhaften oder fehlenden Eingabe nicht abschicken lässt – und nicht etwa aufgrund einer Störung der Funktionalität des Absendebuttons.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: In dem PDF-Dokument werden keinerlei Fehlermeldungen angezeigt bzw. das PDF-Dokument verfügt über keine interaktiven Elemente, die Fehlermeldungen verursachen könnten bzw. die Angabe von konkreten Fehlermeldungen würde Sicherheit oder Zweck des Inhalts gefährden.
- erfüllt: Eine Meldung beschreibt präzise, wie der Fehlerfall behoben werden kann. Dabei werden Sinn und Zweck des Inhalts nicht gefährdet.
- Blockade: Ein Meldungsfenster gibt nur den Begriff „Fehler“ oder „falsches Format“ aus.
- erfüllt: Fehlermeldung „Nachname: Eine Eingabe ist erforderlich.“
- erfüllt: Fehlermeldung „Geburtsdatum: Das Format ist nicht korrekt. Bitte im Format TT.MM.JJJJ ausfüllen.“
- erfüllt: Ein Eingabefeld verlangt die Angabe des Monatsnamens in ausgeschriebener Form. Es wird eine Zahlenangabe gemacht. Die Fehlermeldung listet alle möglichen Monatsnamen auf. Hinweis: Diese Fehlermeldung weist konkret auf die verlangte Sprachversion hin und ist auch bei einem vorliegenden Rechtschreibfehler hilfreich.
- erfüllt: Ein Eingabefeld verlangt die Angabe des Monatsnamens in ausgeschriebener Form. Es wird ‚12‘ eingetragen. Fehlermeldung: „Meinen Sie ‚Dezember‘?“
- erfüllt: Ein Eingabefeld verlangt die Angabe des Monatsnamens in ausgeschriebener Form. Es wird ‚may‘ eingetragen. Fehlermeldung: „Meinen Sie ‚Mai‘?“. Hinweis: Diese Fehlermeldung ist ebenfalls hilfreich, falls die korrekte Eingabe von ‚Mai‘ aufgrund einer fälschlicherweise auf Englisch eingestellten Autokorrektur vom PDF-Anzeigeprogramm falsch korrigiert wurde.
- erfüllt: Ein Eingabefeld verlangt die Angabe des Monats in Zahlenform. Es wird ‚21‘ eingegeben. Die Fehlermeldung listet alle möglichen Zahlenangaben von 1 bis 12 auf. Hinweis: Diese Fehlermeldung ist auch bei einem möglicherweise vorliegenden Zahlendreher hilfreich.
- Blockade: „Eingabe ist ungültig“

<a id="k-10-3-3-4"></a>

#### 10.3.3.4 Fehlervermeidung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#fehlervermeidung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#fehlervermeidung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn ein PDF-Dokument Aktionen enthält, die rechtliche Verpflichtungen oder finanzielle Transaktionen zur Folge haben oder Daten in Datenspeichersystemen ändern bzw. löschen oder Prüfungsfragen übermitteln.

**Anforderung**

Wenn in einem PDF-Dokument Aktionen vorhanden sind, die rechtliche Verpflichtungen oder finanzielle Transaktionen zur Folge haben oder Daten in Datenspeichersystemen ändern bzw. löschen oder Prüfungsfragen übermitteln, gilt mindestens eine der folgenden Aussagen:

- Rückgängig: Übermittlungen sind umkehrbar.
- Geprüft: Die eingegebenen Daten können vor dem Auslösen der Aktion auf Eingabefehler hin überprüft und ggf. korrigiert werden.
- Bestätigt: Die Eingaben können überprüft, bestätigt und korrigiert werden, bevor die Übermittlung abgeschlossen wird.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass im Hinblick auf Aktionen, die rechtliche Verpflichtungen oder finanzielle Transaktionen zur Folge haben oder Daten in Datenspeichersystemen ändern bzw. löschen oder Prüfungsfragen übermitteln, spezifische Anforderungen zur Umkehrung bzw. Prüfung erfüllt sind, bevor ernsthafte Konsequenzen in Kraft treten.

Es ist für alle von Vorteil, wenn bei Eingaben bzw. einer Auswahloption mit rechtskräftigen oder unumkehrbaren Folgen bzw. mit Auswirkungen, die sich nur mit erheblichem Aufwand umkehren lassen würden, ein deutlicher Hinweis auf das Absenden der Eingaben bzw. der gewählten Auswahloption gegeben wird verbunden mit der Möglichkeit zur Überprüfung bzw. zum Abbruch der Aktion.

Bei Menschen mit Beeinträchtigung ist in vielen Fällen das Risiko von Fehleingaben erhöht. Menschen mit Legasthenie beispielsweise vertauschen häufiger Zahlen, Menschen mit motorischen Einschränkungen drücken häufiger versehentlich falsche Tasten, während Menschen mit Sehbehinderungen ggf. nur einen kleinen Teil der Anzeige wahrnehmen können und dadurch leichter unabsichtlich Aktionen auslösen könnten.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: In dem PDF-Dokument werden mit keinerlei interaktiven Elementen finanzielle Daten übermittelt oder Rechtsgeschäfte geschlossen.
- erfüllt: Sämtliche Aktionen oder Daten lassen sich entweder rückgängig machen oder vor dem Absenden überprüfen und mittels Bestätigung legitimieren.
- Blockade: Sämtliche bzw. einzelne Aktionen oder Daten werden direkt übermittelt und lassen sich nicht rückgängig machen oder vor dem Absenden überprüfen und mittels Bestätigung legitimieren.
- Blockade: Ein Formular in einem PDF-Dokument sendet direkt Daten an einen Verarbeitungsserver, ohne die Übermittlung vorher durch einen Bestätigungsdialog abzufragen.
- Blockade: In einer Prüfungssituation wird jede getätigte Antwort ohne Bestätigungsdialog sofort final an den Verarbeitungsserver übermittelt sowie der Prüfling zur nächsten Frage navigiert.
- Blockade: In einer Prüfungssituation erhält der Prüfling vor dem Absenden aller Antworten keine Gelegenheit, die getätigten Eingaben noch ein letztes Mal zu überprüfen.
- erfüllt: Ein Antrag besteht aus mehreren Prozessschritten. Im letzten Prozessschritt bzw. auf der letzten Seite werden alle Daten in einer Übersicht angezeigt. Vor dem Absenden des Antrags können Korrekturen vorgenommen werden.
- erfüllt: In einer Übersicht können Datensätze gelöscht werden. Vor dem Löschen erfolgt eine Rückfrage, sodass der Vorgang noch abgebrochen werden kann.
- erfüllt: Ein Formular in einem PDF-Dokument sendet direkt Daten an einen Verarbeitungsserver. Diese Aktion kann nicht rückgängig gemacht werden. Bevor das Senden beginnt, wird ein Bestätigungsdialog angezeigt. Auf diese Weise wird verhindert, dass die Aktion durch ein versehentliches Ausführen des Shortcuts für das Absenden ausgelöst wird.
- erfüllt: Ein Formular in einem PDF-Dokument sendet Daten zur Ausführung eines Wertpapiergeschäfts. Vor dem Senden findet zunächst eine Prüfung statt, ob der Zeitpunkt innerhalb der Öffnungszeiten der elektronischen Börse liegt und unmittelbar ausgeführt werden könnte. Andernfalls würde beim Ausführen ggf. eine After Hours Trading Aktion (nachbörslicher Handel) angestoßen. Auch für After Hours Trading gelten spezifische Zeiten. Eine Fehlermeldung weist auf die Öffnungszeiten sowie auf ggf. mit After Hours Trading verbundene Risiken hin. Es gibt die Möglichkeit, das Ausführen der Aktion abzubrechen.

<a id="k-10-4-1-1"></a>

#### 10.4.1.1 Korrekte Syntax

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#korrekte-syntax) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#korrekte-syntax) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Die logische Struktur des PDF-Dokuments sollte korrekt sein.

- Insbesondere benötigen alle Strukturelemente vollständige Start- und End-Tags.
- Strukturelemente müssen darüber hinaus gemäß ihren jeweiligen Spezifikationen verwendet werden.
- Die Verschachtelung mehrerer Strukturelemente muss gemäß ihren jeweiligen Spezifikationen erfolgen.
- Attribute müssen korrekt getaggt sein und gemäß ihren jeweiligen Spezifikationen sowie ohne Doppelungen verwendet werden.
- IDs müssen unique sein bzw. gemäß ihren Spezifikationen vergeben werden.

**Warum wird das geprüft?**

Fehler in der logischen Struktur des PDF-Dokuments können dazu führen, dass assistive Technologien (wie insbesondere Screenreader-Programme) das PDF-Dokument nicht korrekt parsen bzw. auslesen können.

Dabei können Fehler in der logischen Struktur auch das Auslesen von Strukturelementen beeinflussen, die für sich isoliert betrachtet korrekt getaggt wurden:

- Ein Screenreader-Programm kann eine Überschrift nicht korrekt auslesen. Die Überschrift an sich ist korrekt getaggt. Doch der gesamte Abschnitt enthält Fehler in der logischen Struktur.
- Ein Screenreader-Programm kann den Alternativtext zu einem Grafik-Element nicht korrekt auslesen. Das Grafik-Element an sich wurde korrekt getaggt und ein zugehöriger Alternativtext ist hinterlegt. Das Problem des Screenreader-Programms könnte darauf zurückzuführen sein, dass ein vorangehender oder auch nachfolgender Abschnitt Fehler in der logischen Struktur aufweist.

Eine korrekte logische Struktur ist darüber hinaus in jedem Fall erstrebenswert und dient in mehrfacher Hinsicht dazu, Qualitätsansprüche an das PDF-Dokument sicherzustellen.

- Fehler in der logischen Struktur können sich auch auf das Design des PDF-Dokuments auswirken und ggf. unerwünschte Design-Effekte zur Folge haben. Beispielsweise könnte eine H2-Überschrift bei falscher Auszeichnung als H5-Überschrift fälschlicherweise auch wie eine H5-Überschrift dargestellt sein.
- Wird ein automatisiertes Verfahren bei der Erzeugung von PDF-Dokumenten angewendet, so erleichtert eine korrekte logische Struktur den Herstellungsprozess, indem sie die ordnungsgemäße, fehlerfreie Ausführung des automatisierten Verfahrens sicherstellt.

Von einer korrekten logischen Struktur profitieren letztlich alle Personengruppen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Der Test mit dem Prüftool PAC weist keine (möglichen) Fehler in der Rubrik "Logische Struktur" auf.
- leichte Einschränkung: Der Test mit dem Prüftool PAC weist (mögliche) Fehler in der Rubrik "Logische Struktur" auf.
- leichte Einschränkung: Es gibt (keine getaggten Inhalte und damit) keinen Strukturbaum. An dieser Stelle entfällt damit insbesondere explizit die Bewertungsalternative "nicht anwendbar".

Auch wenn in diesem Prüfschritt Fehler bzw. mögliche Fehler in der logischen Struktur lediglich als leichte Einschränkung zu bewerten sind, so ist doch zu beachten, dass sie weitreichende Folgen nach sich ziehen können: Folgen, die in anderen Prüfschritten konkret geprüft werden und dort mit wesentlich stärkeren Einschränkungen bis hin zur Blockade zu bewerten sind.

Damit kann dieser Prüfschritt als "Mittel zum Zweck" angesehen werden: ein Mittel, mit dem sich darüber hinaus anhand der sehr konkreten Fehlermeldungen weitreichende Fehler identifizieren und beheben lassen.

<a id="k-10-4-1-2"></a>

#### 10.4.1.2 Name, Rolle, Wert

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#name-rolle-wert) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#name-rolle-wert) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument interaktive Elemente wie Eingabefelder, Auswahlschalter, Buttons oder Links bzw. von JavaScript-gesteuerte Elemente enthält.

**Anforderung**

Bedienelemente sollen mit geeigneten Namen, Rollen und Eigenschaften ausgezeichnet und bei Nutzung assistiver Technologien in vollem Umfang zugänglich sein.

Darüber hinaus soll die korrekte Übertragung der bei der Bearbeitung vorgenommenen Einstellungen an diesen Elementen bzw. die Auslesbarkeit ihrer Zustände programmatisch gewährleistet sein und konsistent an assistive Technologien übermittelt werden.

**Warum wird das geprüft?**

Menschen, die auf assistive Technologien angewiesen sind, können Bedienelemente in einem PDF-Dokument, die für die assistive Technologie nicht ermittelbar sind, nicht zuverlässig nutzen.

Wenn der Zustand eines Bedienelementes durch Screenreader-Programme falsch oder gar nicht ausgelesen werden kann, hat eine Person, die auf die Nutzung assistiver Technologien angewiesen ist, keine Kontrolle über die mit dem Bedienelement verknüpfte Aktion bzw. Auswirkung.

Abweichende Bezeichnungen in der Sprachausgabe eines Screenreader-Programms und der Funktion eines Bedienelementes bzw. der vom Screenreader-Programm ausgelesenen Beschreibung und der sichtbaren Beschreibung eines Bedienelementes führen zu Irritationen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- nicht anwendbar: Das PDF-Dokument enthält keine interaktiven Bedienelemente.
- erfüllt: Alle Bedienelemente sind mit Namen, ihrer Rolle und dem entsprechenden Wert erkennbar bzw. auslesbar.
- Einschränkung: Wenn der Name des Elements nicht im Feld Name, sondern in einem anderen Feld wie Wert oder Hilfetext steht und hieraus Widersprüche entstehen.
- Barriere: Wenn sich kein Rahmen um das Element bildet oder der Rahmen nicht der visuell wahrnehmbaren Begrenzung des Elements entspricht. Hier ist die Personengruppe mit Sehbehinderungen ggf. komplett ausgeschlossen. Zudem können solche Felder auch für andere Personengruppen nicht direkt erkennbar sein.
- Blockade: Wenn der Name des Elements nicht vorhanden ist, nicht der sichtbaren Beschriftung entspricht, verschiedene Namen in den Teilen eines zusammengesetzten Elements angegeben sind oder verschiedene Namen im fokussierten und nicht fokussierten Zustand angegeben sind.

Alle Mängel, soweit nicht bereits gewichtet, werden nach der Bedeutung für die Arbeitsaufgabe bewertet.

- erfüllt: Eine Ausklappliste ist als solche auch bei einem Vergrößerungsfaktor von beispielsweise 300 % erkennbar und der ausgewählte Eintrag ist für die assistive Technologie ersichtlich.
- erfüllt: Jeder Radiobutton einer Gruppe von Radiobuttons hat einen individuellen Wert. Der ausgewählte Status ist für die assistive Technologie ermittelbar, wie z. B.: "Mittelgroß: Ausgewählt!".
- Barriere: Wenn der Name des Elements im Feld Wert steht und der Wert des Elements sich dynamisch ändern kann (Formularfelder, Statusanzeigen).
- Blockade: Die Rolle eines Elements ist unzutreffend angegeben.
- Blockade: In den verschiedenen, für die Rolle einsetzbaren, Feldern stehen widersprüchliche Angaben.
- Blockade: Teile zusammengesetzter Elemente geben widersprüchliche Rollen an.
- Blockade: Fokussierte und nicht fokussierte Elemente geben unterschiedliche Rollen an.
- Blockade: Die Sprachausgabe eines Screenreader-Programms beschreibt ein Texteingabefeld als Kontrollfeld.
- Blockade: Der Zustand eines Kontrollfeldes wird durch das Screenreader-Programm falsch wiedergegeben.
- Blockade: Der Zustand einer Ausklappliste, eines Kombinationsfeldes oder eines Akkordeons wird nicht wiedergegeben.
- Blockade: Die sichtbare Beschriftung einer Schaltfläche weicht von der Bezeichnung der Screenreader-Ausgabe ab.
- Blockade: Ein Schieberegler gibt den aktuell gewählten Wert nicht wieder.

<a id="k-10-4-1-3"></a>

#### 10.4.1.3 Statusmeldungen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#statusmeldungen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#statusmeldungen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist nicht anwendbar.

<a id="k-ua1-6-2"></a>

#### UA1 6.2 Metadaten und UA-Kennzeichnung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-6.2-metadaten-und-ua-kennzeichnung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-6.2-metadaten-und-ua-kennzeichnung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Eingebundene Metadaten im PDF-Dokument sind korrekt hinterlegt, sodass keine Fehlermeldungen im PAC erscheinen. Eine PDF/UA-Kennzeichnung (Identifier) ist vorhanden.

**Warum wird das geprüft?**

Der PDF/UA Identifier ist ein Hinweis für PDF/UA Konformität sowie die Kennzeichnung für getaggte PDF-Dokumente und sollte daher eingetragen sein.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- leichte Einschränkung
- Einschränkung
- Barriere

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Es sind Metadaten und Dokumenteinstellungen (UA-Kennzeichnung) vorhanden.
- leichte Einschränkung: Es sind keine Metadaten oder Dokumenteinstellungen (UA-Kennzeichnung) vorhanden. Ein Screenreader-Programm identifiziert jedoch alle Inhalte eindeutig und korrekt.
- Einschränkung: Es sind keine Metadaten oder Dokumenteinstellungen (UA-Kennzeichnung) vorhanden. Ein Screenreader-Programm identifiziert unwesentliche Inhalte nicht eindeutig und nicht ganz korrekt.
- Barriere: Es sind keine Metadaten oder Dokumenteinstellungen (UA-Kennzeichnung) vorhanden und Screenreader-Programme identifizieren deshalb nicht alle Inhalte eindeutig.

<a id="k-ua1-7-7"></a>

#### UA1 7.7 Unicode-Zuordnung

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.7-unicode-zuordnung) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.7-unicode-zuordnung) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald das PDF-Dokument durchsuchbaren Text enthält.

**Anforderung**

Alle in durchsuchbarem Text verwendeten Schriftzeichen sollen in ein adäquates Unicode-Zeichen überführt werden können.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass die Inhalte von durchsuchbarem Text eines PDF-Dokuments allen Personengruppen vollumfänglich zur Verfügung stehen. Die Schriftzeichen von getaggtem Text werden in Form von Unicode an assistive Technologien übergeben. Welche Schriftarten konkret verwendet werden, ist für assistive Technologie zunächst nicht von Bedeutung. In den verwendeten Schriften muss eine Übersetzungstabelle eingebettet sein, die alle verwendeten Schriftzeichen umfasst. Andernfalls besteht die Gefahr von Missverständnissen bis hin zu vollständigem Informationsverlust, wenn einzelne Worte oder ganze Abschnitte von assistiven Technologien aufgrund einer unvollständigen bzw. vollständig fehlenden Übersetzungstabelle nicht interpretiert werden können.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Barriere
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: PAC meldet bezüglich der Unicode-Zuordnung keine Fehler. Der gesamte durchsuchbare Text wird vollständig korrekt an assistive Technologien übergeben.
- Einschränkung: PAC meldet ggf. keine Fehler hinsichtlich der Unicode-Zuordnung. Doch nicht der gesamte durchsuchbare Text im PDF-Dokument wird vollständig korrekt an assistive Technologien übergeben. Betroffen sind nur einige wenige Worte. Die fehlenden Bestandteile werden als derart unwesentlich eingestuft, dass es nicht erforderlich erscheint, dass ihre Bedeutung aus dem Kontext erschlossen wird (z. B. Eigennamen).
- Barriere: PAC meldet ggf. keine Fehler hinsichtlich der Unicode-Zuordnung. Doch nicht der gesamte durchsuchbare Text im PDF-Dokument wird vollständig korrekt an assistive Technologien übergeben. Die fehlenden Bestandteile werden als unwesentlich eingestuft. Die Bedeutung des Abschnitts kann aus dem Kontext erschlossen werden.
- Blockade: Nicht der gesamte durchsuchbare Text im PDF-Dokument wird vollständig korrekt an assistive Technologien übergeben. Die fehlenden Bestandteile werden als wesentlich eingestuft.

<a id="k-ua1-7-10"></a>

#### UA1 7.10 Optionaler Inhalt

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.10-optionaler-inhalt) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.10-optionaler-inhalt) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument optionalen Inhalt enthält.

**Anforderung**

Alle Dictionaries für die Konfiguration von optionalem Inhalt im Dokument, einschließlich des Default-Dictionary für die Konfiguration von optionalem Inhalt, müssen einen Name-Eintrag enthalten, dessen Wert ein nicht leerer Text ist, wenn beide der folgenden Bedingungen erfüllt sind:

- Ein Dokument enthält einen Configs-Eintrag im OCProperties-Eintrag des Catalog-Dictionary,
- und der Configs-Eintrag enthält mindestens ein Dictionary für die Konfiguration von optionalem Inhalt.

Der AS-Schlüssel darf in keinem Dictionary für die Konfiguration von optionalem Inhalt erscheinen.

**Warum wird das geprüft?**

Es muss sichergestellt sein, dass alle Nutzenden optionalen Inhalt ein- und ausblenden können.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Die Struktureinträge sind vorhanden und verursachen keinen Fehler im Prüfbericht von PAC.
- Blockade: Die Struktureinträge fehlen und verursachen einen Fehler im Prüfbericht von PAC.

<a id="k-ua1-7-16"></a>

#### UA1 7.16 Sicherheit

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.16-sicherheit) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.16-sicherheit) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist immer anwendbar.

**Anforderung**

Die Sicherheitseinstellungen des PDF-Dokuments sollen assistiven Technologien (z. B. Screenreader-Programmen) einen vollwertigen Zugriff auf das PDF-Dokument ermöglichen.

**Warum wird das geprüft?**

Assistive Technologien benötigen beim Zugriff auf PDF-Dokumente spezielle Zugriffsrechte. Sind die Sicherheitseinstellungen eines PDF-Dokuments zu restriktiv oder falsch gesetzt, können assistive Technologien nicht auf das PDF-Dokument zugreifen und das PDF-Dokument kann nicht gelesen werden.

Indem gewährleistet wird, dass assistive Technologien (wie z. B. Screenreader-Programme) auf das PDF-Dokument zugreifen können, wird sichergestellt, dass die Inhalte des PDF-Dokuments Personen, die auf die Nutzung eines Screenreader-Programms angewiesen sind, vollumfänglich zur Verfügung stehen. Dies betrifft insbesondere Menschen ohne bzw. mit eingeschränktem Sehvermögen.

Darüber hinaus sollte es möglich sein, ein PDF-Dokument in alternative Formate umzuwandeln, um den unterschiedlichsten Bedürfnissen gerecht zu werden.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- erfüllt
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Die Sicherheitseinstellungen ermöglichen einen vollständigen Zugriff von assistiven Technologien.
- Blockade: Die Sicherheitseinstellungen verhindern einen Zugriff von assistiven Technologien.

<a id="k-ua1-7-17"></a>

#### UA1 7.17 Navigation über Lesezeichen

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.17-navigation-über-lesezeichen) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.17-navigation-über-lesezeichen) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument mehrere Seiten hat und mindestens eine optische Überschrift.

**Anforderung**

Ein PDF-Dokument soll eine Übersicht mit Navigationsmöglichkeit - z. B. in Form von Lesezeichen (Bookmarks) - enthalten, die der Lesereihenfolge entspricht und die verwendeten Überschriftenebenen abbildet.

**Warum wird das geprüft?**

Lesezeichen erleichtern allen Nutzenden die Navigation, da auf schnellem Wege zu einer bestimmten Stelle oder Seite im PDF-Dokument gesprungen werden kann. Vor allem in mehrseitigen PDF-Dokumenten ist dies eine sinnvolle Erleichterung, um schnell Inhalte aufzusuchen.

Eine Person mit kognitiven Behinderungen zieht unter Umständen eine hierarchische Kurzdarstellung (Outline) vor, die einen Überblick über das PDF-Dokument gibt, anstatt viele Seiten zu lesen und durchzugehen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- leichte Einschränkung
- Einschränkung

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Lesezeichen sind vollständig vorhanden und funktionieren zuverlässig.
- leichte Einschränkung: Vorhandene Lesezeichen funktionieren zuverlässig, wobei nicht alle wesentlichen Überschriften als Lesezeichen abgebildet sind.
- Einschränkung: Nicht alle Lesezeichen funktionieren zuverlässig.

<a id="k-ua1-7-20"></a>

#### UA1 7.20 Externe Objekte

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.20-externe-objekte) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.20-externe-objekte) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, wenn das PDF-Dokument externe Objekte enthält.

**Anforderung**

In einem PDF/UA konformen PDF-Dokument dürfen Reference XObjects nicht enthalten sein.

**Warum wird das geprüft?**

Wenn externe Objekte korrekt eingebunden sind, können diese vom PDF-Anzeigeprogramm (z. B. Adobe Acrobat Reader) dargestellt werden und assistive Technologien können auf den eingebundenen Inhalt zugreifen.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: Externe Objekte wurden den Anforderungen entsprechend eingebunden und verursachen keinen Fehler im Prüfbericht von PAC.
- Einschränkung: Der Prüfbericht von PAC weist auf Fehler hin. Ein Screenreader-Programm identifiziert jedoch alle externen Objekte eindeutig und korrekt.
- Blockade: Der Prüfbericht von PAC weist auf Fehler hin und Screenreader-Programme können nicht alle externen Objekte eindeutig identifizieren.

<a id="k-ua1-7-21"></a>

#### UA1 7.21 Schriften

| Merkmal | Wert |
| --- | --- |
| Funktionsbereich (Vorschlag) | Basisfunktionalität |
| Prüfverfahren | BIT inklusiv PDF-Test |
| Gilt für Anwendungstypen | Dokument |
| Fundstelle | [BIT inklusiv PDF-Test](https://pdftest.bit-inklusiv.de/#ua1-7.21-schriften) |

[Prüfschritt im BIT inklusiv PDF-Test öffnen](https://pdftest.bit-inklusiv.de/#ua1-7.21-schriften) (dort auch Prüfung, Anmerkungen, PAC-Fehlermeldungen, Einordnung, Korrekturvorschlag, Verweise)

**Anwendbarkeit**

Dieser Prüfschritt ist anwendbar, sobald das PDF-Dokument durchsuchbaren Text in Form von nicht-Symbolen (normale Textzeichen per Schriftarten wie Arial, Verdana, etc.) bzw. Symbolen (Wingdings, Formelzeichen oder Aufzählungszeichen, etc.) enthält.

**Anforderung**

Die verwendeten Schriftarten sollen im PDF-Dokument eingebettet sein.

Enthält ein PDF-Dokument durchsuchbare nicht-Symbol-Textzeichen, wie Buchstaben, Zahlen oder übliche Sonderzeichen, so muss für eingebettete nicht-Symbol-TrueType-Schriftarten ein korrekter Encoding-Eintrag gesetzt sein.

Enthält ein PDF-Dokument durchsuchbare symbolische Zeichen, wie Aufzählungszeichen oder Formelzeichen, so muss für eingebettete symbolische TrueType Schriftarten ein korrekter Encoding-Eintrag gesetzt sein.

**Warum wird das geprüft?**

Mit diesem Prüfschritt soll sichergestellt werden, dass die Inhalte von durchsuchbarem Text eines PDF-Dokuments allen Personengruppen vollumfänglich zur Verfügung stehen.

Damit die im PDF-Dokument verwendeten Schriftarten zuverlässig in allen Ausgabeformen (wie z. B. bei Nutzung von Screenreader-Programmen bzw. Braille-Software) aber auch von sämtlichen PDF-Anzeigeprogrammen optisch korrekt dargestellt werden, ist das Vorhandensein eines entsprechenden Encoding-Eintrages erforderlich.

Andernfalls besteht die Gefahr von Missverständnissen bis hin zu vollständigem Informationsverlust.

**Bewertungsalternativen und Beispiele**

Der Prüfschritt kann wie folgt bewertet werden:

- nicht anwendbar
- erfüllt
- Einschränkung
- Blockade

Beispiele für Umsetzungen mit erfüllter bzw. nicht erfüllter Anforderung:

- erfüllt: PAC meldet bezüglich der Schriften keine Fehler. Die eingebettete nicht-Symbol-TrueType-Schrift bzw. eingebettete Symbol-TrueType-Schrift enthält einen Kodierungseintrag.
- Einschränkung: Nicht alle im PDF-Dokument vorhandenen Inhalte lassen sich in der PAC Screenreader-Vorschau wiederfinden.
- Einschränkung: Die eingebettete nicht-Symbol-TrueType-Schrift bzw. eingebettete Symbol-TrueType-Schrift enthält keinen Kodierungseintrag, kann jedoch von einem Screenreader-Programm korrekt ausgelesen werden und ist auch optisch korrekt erkennbar.
- Blockade: Die PAC Screenreader-Vorschau liefert keine noch als sinnvoll verwertbar anzusehenden Informationen.
- Blockade: Die eingebettete nicht-Symbol-TrueType-Schrift bzw. eingebettete Symbol-TrueType-Schrift enthält keinen Kodierungseintrag und ist, entweder von einem Screenreader-Programm oder rein optisch, nicht korrekt erkennbar.
