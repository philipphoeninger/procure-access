using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "FilterTypes",
                columns: new[] { "Id", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, "", false, "Produkt-Typen" },
                    { 2, "", false, "App-Typen" },
                    { 3, "", false, "Produkt-Teile" },
                    { 4, "", false, "Test-Typen" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Products",
                columns: new[] { "Id", "Description", "IsDeleted", "Link", "Name" },
                values: new object[,]
                {
                    { 1, "Keine Beschreibung vorhanden", false, "www.coreflow.org", "CoreFlow" },
                    { 2, "Keine Beschreibung vorhanden", false, "www.intrasphere.com", "IntraSphere" },
                    { 3, "Keine Beschreibung vorhanden", false, "www.proximasuite.com", "ProximaSuite" },
                    { 4, "Keine Beschreibung vorhanden", false, "www.workbridge.com", "WorkBridge" },
                    { 5, "Keine Beschreibung vorhanden", false, "www.syntralogic.it", "SyntraLogic" },
                    { 6, "Keine Beschreibung vorhanden", false, "www.nexaro.de", "Nexaro" },
                    { 7, "Keine Beschreibung vorhanden", false, "www.flowmatrix.com", "FlowMatrix" },
                    { 8, "Keine Beschreibung vorhanden", false, "www.opticore.com", "OptiCore" },
                    { 9, "Keine Beschreibung vorhanden", false, "www.bizstream.ru", "BizStream" },
                    { 10, "Keine Beschreibung vorhanden", false, "www.novadesk.at", "NovaDesk" },
                    { 11, "Keine Beschreibung vorhanden", false, "www.clarioone.fr", "ClarioOne" },
                    { 12, "Keine Beschreibung vorhanden", false, "www.flexisuite.com", "FlexiSuite" },
                    { 13, "Keine Beschreibung vorhanden", false, "www.evomanage.com", "EvoManage" },
                    { 14, "Keine Beschreibung vorhanden", false, "www.taskfusion.com", "TaskFusion" },
                    { 15, "Keine Beschreibung vorhanden", false, "www.primehub.org", "PrimeHub" },
                    { 16, "Keine Beschreibung vorhanden", false, "www.worklineos.de", "WorklineOS" },
                    { 17, "Keine Beschreibung vorhanden", false, "www.aquilaware.com", "AquilaWare" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "CriteriaFilters",
                columns: new[] { "Id", "Description", "FilterTypeId", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, "", 3, false, "Basisfunktionalität" },
                    { 2, "", 3, false, "Audio" },
                    { 3, "", 3, false, "Video" },
                    { 4, "", 3, false, "Zweiwege-Sprachkommunikation" },
                    { 5, "", 3, false, "Echtzeittext (RTT)" },
                    { 6, "", 3, false, "Biometrie" },
                    { 7, "", 3, false, "Autorenwerkzeug" },
                    { 8, "", 2, false, "Website / Webanwendung" },
                    { 9, "", 2, false, "Dokument" },
                    { 10, "", 2, false, "Software (offen)" },
                    { 11, "", 2, false, "Software (geschlossen)" },
                    { 12, "", 2, false, "Mobile Anwendung (offen)" },
                    { 13, "", 2, false, "Mobile Anwendung (geschlossen)" },
                    { 14, "", 2, false, "Hardware" },
                    { 15, "", 2, false, "Vermittlungs-/Notrufdienst" },
                    { 16, "", 1, false, "Datenbankgestützte Fachanwendung" },
                    { 17, "", 1, false, "HR-Anwendung" },
                    { 18, "", 1, false, "Reporting-Anwendung" },
                    { 19, "", 1, false, "Fortbildungsmanagement" },
                    { 20, "", 1, false, "Dienstreiseanträge" },
                    { 21, "", 1, false, "Gehaltsabrechnungen" },
                    { 22, "", 1, false, "Interne Kommunikations- und Datenaustausch-Plattform" },
                    { 23, "", 4, false, "BIT inklusiv Software-Test" },
                    { 24, "", 4, false, "BIT inklusiv App-Test" },
                    { 25, "", 4, false, "BIT inklusiv PDF-Test" },
                    { 26, "", 4, false, "BIK BITV-Test (Web)" },
                    { 27, "", 4, false, "BIK BITV-Test (App)" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Criteria",
                columns: new[] { "Id", "CriteriaFilterId", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Bietet die Software eigene assistive Unterstützungen an, so müssen die Funktionen für die jeweilige Zielgruppe barrierefrei auffindbar und aktivierbar sein. Nachdem verschiedene Funktionen für verschiedene Nutzergruppen bereitgestellt werden können, deren Anforderungen unterschiedlich sind, kann kein allgemein gültiges Vorgehen definiert werden.\n\nBeispiele für individuelle Funktionen:\n\n- Die Software bietet eine Vorlesefunktion an.\n- Die Software bietet Inhalte in leichter Sprache oder Gebärdensprache an.\n- Die Software bietet das Verändern von Farben oder Kontrasten an.\n- Die Software bietet das Verändern von Schriftart, Schriftgröße oder Zeilenabstand an.\n- Die Software bietet Funktionen zum Vergrößern oder Verkleinern von Inhalten an.", false, "5.2 Aktivierung von Barrierefreiheitsfunktionen" },
                    { 2, 6, "Bietet die Software eine biometrische Identifizierungsmethode an, so darf der Identifikationsvorgang nicht nur auf das biometrische Merkmal beschränkt sein. Es muss mindestens eine weitere Identifizierungsmethode unterstützt werden. Eine Alternative zu einer biometrischen Identifizierungsmethode kann sowohl eine weitere biometrische Methode als auch eine nicht biometrische Methode sein.", false, "5.3 Biometrie" },
                    { 3, 1, "Bietet die Software Konvertierungsmöglichkeiten in andere Formate an, müssen bereits vorhandene Barrierefreiheitsinformationen und -Funktionen im Zielformat übernommen werden, wenn das Zielformat diese Informationen und Funktionen grundsätzlich unterstützt.", false, "5.4 Erhaltung von Barrierefreiheitsinformationen bei Konvertierung" },
                    { 4, 1, "Greift die Software zum Ausführen bestimmter Funktionalitäten auf physische Bedienelemente des Endgeräts zurück, die zur Bedienung ein Greifen, Zusammendrücken oder Drehen des Handgelenks erfordern, muss eine barrierefreie, alternative Möglichkeit der Bedienung zur Verfügung stehen, für die diese Handlungen nicht erforderlich sind. Alternative Bedienmethoden können Hardware- oder Software-Lösungen sein.", false, "5.5.1 Möglichkeiten der Bedienung" },
                    { 5, 1, "Wenn die Software bedienbare Teile enthält, muss sie eine Möglichkeit bieten, jedes bedienbare Teil zu erkennen, ohne dass eine Sicht erforderlich ist und ohne dass die mit dem bedienbaren Teil verbundene Handlung ausgeführt werden muss.\n\nEine Möglichkeit, diese Anforderung zu erfüllen, besteht darin, die bedienbaren Teile taktil wahrnehmbar zu machen, z.B. bei Geld- oder Fahrscheinautomaten mit Touchscreen und Drucktasten. Der Prüfschritt ist bei der Prüfung von Software nicht anwendbar, da die Software keine Möglichkeit hat, ein physikalisches Bedienelement des Endgeräts zu beschriften", false, "5.5.2 Unterscheidbarkeit der bedienbaren Elemente" },
                    { 6, 1, "Kann eine Funktionalität der Software mit einem physischen Bedienelement des Endgeräts gesteuert werden, und wird der Status des Steuerelements visuell dargestellt, muss zusätzlich zur visuellen Erkennung mindestens ein weiterer Modus zum Identifizieren des jeweiligen Status des Steuerelementes zur Verfügung stehen, entweder durch Berührung oder durch Audioausgabe.", false, "5.6.1 Taktiler oder auditiver Status" },
                    { 7, 1, "Bietet die Software ein physisches Steuerelement zum Sperren oder Umschalten, und stellt den Status des Steuerelements non-visuell dar, muss zusätzlich zur vorhandenen Erkennungsmethode eine visuelle Identifizierung des jeweiligen Status des Steuerelementes zur Verfügung stehen.", false, "5.6.2 Visueller Status" },
                    { 8, 1, "Wenn die Software über eine nicht abschaltbare Tastenwiederholungsfunktion verfügt:\n\n- a) muss die Verzögerung vor der Tastenwiederholung auf mindestens 2 Sekunden einstellbar sein; und\n- b) muss die Tastenwiederholungsrate bis auf ein Zeichen pro 2 Sekunden einstellbar sein.\n\nDie entsprechenden Einstellungen sind im Windows-Betriebssystem unter „Systemsteuerung / Center für erleichterte Bedienung / Bedienung mit der Tastatur erleichtern / Anschlagverzögerung einrichten / Wiederhol- und Anschlaggeschwindigkeit einrichten“ zu finden. Die Windows Bildschirmtastatur bietet keine entsprechende Unterstützung dieser Funktionalität, sondern bietet bei längerem Tastendruck die Auswahl von Sonderzeichen an.", false, "5.7 Tastenwiederholung" },
                    { 9, 1, "Individuelle Benutzereinstellungen zur Anschlagsverzögerung, die im Betriebssystem konfigurierbar sind, müssen von der Software übernommen werden. Die Mindestforderungen lauten:\n\n- Zeitverzögerung von mindestens 0,5 Sekunden vor der Annahme eines weiteren Tastenanschlags\n\nSofern die Software die Einstellungen des Betriebssystems nicht übernimmt, kann sie eine eigene Konfigurationsmöglichkeit zur Verfügung stellen.", false, "5.8 Annahme eines zweifachen Tastenanschlags" },
                    { 10, 1, "Sofern die Software zum Betrieb oder zur Bedienung gleichzeitige Benutzerhandlungen oder komplexe Touch-Gesten mit mindestens zwei Fingern erfordert, muss sie einen alternativen Bedienmodus bereitstellen, der keine gleichzeitigen Benutzerhandlungen für den Betrieb oder die Bedienung benötigt.", false, "5.9 Gleichzeitige Benutzerhandlungen" },
                    { 11, 4, "Zum Sicherstellen von entsprechender Sprachverständlichkeit bei der Zwei-Wege-Sprachkommunikation soll zum Kodieren und Dekodieren von Audio ein Frequenzbereich genutzt werden, dessen obere Grenze bis mindestens 7.000 Hz reicht. Dieser Frequenzbereich kann beispielsweise mit dem Standard-Codec G.722 [i.21] erreicht werden.", false, "6.1 Audio-Bandbreite für Sprache" },
                    { 12, 5, "Zusätzlich zur Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie, muss die Software eine Textkommunikation in Echtzeit (RTT = Real-Time Text) zur Verfügung stellen.\n\nBei RTT handelt es sich um Text, der sofort übertragen wird, während er eingegeben oder erstellt wird. Dadurch können die Empfänger die Nachricht sofort lesen, während sie geschrieben wird.", false, "6.2.1.1 RTT-Kommunikation" },
                    { 13, 5, "Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), sollen beide Kommunikationskanäle gleichwertig genutzt werden können.", false, "6.2.1.2 Gleichzeitige Verwendung von Sprache und Text" },
                    { 14, 5, "Bietet die Software Echtzeit-Textkommunikation (RTT) an, sollen gesendete und empfangene Nachrichten visuell hinreichend voneinander unterscheidbar sein.", false, "6.2.2.1 Visuell unterscheidbare Darstellung" },
                    { 15, 5, "Bietet die Software Echtzeit-Textkommunikation (RTT) an, sollen Sende- und Empfangsrichtung der gesendeten und empfangenen Nachrichten programmatisch ermittelbar sein.", false, "6.2.2.2 Programmatisch bestimmbare Sende- und Empfangsrichtung" },
                    { 16, 5, "Bietet die Software Echtzeit-Textkommunikation (RTT) an und identifiziert den aktiven Sprecher anhand der Sprache, sollen auch Menschen, die Texteingaben anstatt Sprache verwenden, gleichwertig identifiziert werden.", false, "6.2.2.3 Sprecheridentifizierung" },
                    { 17, 5, "Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator muss gleichermaßen programmatisch ermittelbar sein.", false, "6.2.2.4 Anzeige von Audioaktivität in Echtzeit" },
                    { 18, 5, "Bietet die Software Zwei-Wege-Sprachkommunikation via Sprach- oder Videotelefonie und Textkommunikation in Echtzeit (RTT), und unterstützt sie zudem die Nutzung dieser Funktionen in Kombination mit anderer Software desselben Funktionsumfangs, so muss die Interoperabilität auf Basis der relevanten, technischen Standards unterstützt werden.\n\nDie EN 301549 benennt dabei folgende Standards:\n\n- Kommunikation über das öffentliche Telefonnetz: ITU-T V.18 [i.23] oder dessen Text-Telefonie-Standards in den Anhängen werden verwendet\n- Voice over IP (VoIP) über das Session Initiation Protocol (SIP): Empfehlungen der IETF zu Textkommunikation in Echtzeit (RTT = Real Time Text) RFC 4103 [i.13] werden verwendet\n- Voice over IP (VoIP) über das IP Multimedia Sub-System (IMS): Protokolle ETSI TS 126 114 [i.10], ETSI TS 122 173 [i.11] und ETSI TS 134 229 [i.12] werden verwendet\n- Andere Kommunikationsmethoden: Gemeinsame, relevante und veröffentlichte Standards werden verwendet", false, "6.2.3 Interoperabilität" },
                    { 19, 5, "Bietet die Software Echtzeit-Textkommunikation (RTT = Realtime Text), so darf die Dauer der Übertragung von einem Endgerät auf ein anderes Endgerät für die kleinste Texteinheit (einzelne Zeichen, kurze Worte) maximal 1000 Millisekunden (= 1 Sekunde) betragen.", false, "6.2.4 Reaktionsgeschwindigkeit von RTT" },
                    { 20, 4, "Bietet die Software Sprach- oder Videotelefonie mit Anrufer-Identifikation, so muss die Anrufer-Identifikation visuell in Textform, und programmatisch identifizierbar sein.", false, "6.3 Anrufer-Identifizierung" },
                    { 21, 4, "Bietet die Software Sprach- oder Videotelefonie mit Voicemail-Funktion, so muss die Nutzung dieser Dienste, die in aller Regel sprachbasiert gesteuert werden, auch ohne Sprech- oder Hörvermögen nutzbar sein.", false, "6.4 Alternativen zu sprachbasierten Diensten" },
                    { 22, 4, "Software, die Videotelefonie zur Verfügung stellen, sollen mindestens die QVGA-Auflösung bei 320x240 Pixel, vorzugsweise die VGA-Auflösung bei 640x480 Pixel für die Videoübertragung unterstützen.", false, "6.5.2 Auflösung bei Videotelefonie" },
                    { 23, 4, "Software, die Videotelefonie zur Verfügung stellen, sollen eine Bildwiederholfrequenz (frame rate) von mindestens 20 Bildern pro Sekunde, vorzugsweise 30 Bildern pro Sekunde für die Videoübertragung unterstützen.", false, "6.5.3 Bildwiederholfrequenz bei Videotelefonie" },
                    { 24, 4, "Software, die Videotelefonie zur Verfügung stellen, sollen sicherstellen, dass die Latenz zwischen Audio- und Videowiedergabe nicht mehr als 100 Millisekunden beträgt und somit eine hochwertige Lippensynchronität besteht.", false, "6.5.4 Synchronisation von Audio und Video bei Videotelefonie" },
                    { 25, 4, "Bietet die Software Zwei-Wege-Sprachkommunikation mittels Videotelefonie, so muss Audioaktivität von Teilnehmern in Echtzeit visualisiert werden. Der visuelle Indikator der Audio-Aktivität kann ein visueller Punkt, eine Rahmenmarkierung des Nutzenden oder eine andere Art von Ein- / Aus-Anzeige sein, die flackert.", false, "6.5.5 Visuelle Anzeige von Audio-Aktivität" },
                    { 26, 4, "Bietet die Software Sprecheridentifizierung für sprechende Teilnehmende, sollen auch gebärdende Teilnehmende und Echtzeit-Gebärden gleichwertig identifiziert werden, sobald die Eingabe von Gebärden gestartet wurde. Das Sprecheridentifizierung für Gebärdensprache kann manuell aktiviert oder automatisch identifiziert werden.", false, "6.5.6 Sprecheridentifizierung bei Gebärdensprachen-Kommunikation" },
                    { 27, 3, "Bietet die Software aufgenommene Videoinhalte mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln an, benötigt der Videoplayer die Möglichkeit, die Untertitel über ein Bedienelement ein- und auszublenden (closed captions). Eine Funktion zur Ausgabe der Untertitel auf Braille-Zeilen ist notwendig.", false, "7.1.1 Wiedergabe von Untertiteln" },
                    { 28, 3, "Bietet die Software Nicht-Live-Videos mit in Verbindung mit Untertiteln an, müssen Untertitel und Tonspur synchron wiedergegeben werden.\n\nDie EN 301549 konkretisiert dabei die zwei folgenden Anforderungen:\n\n- Die Anzeige des Untertitels weicht nicht mehr als 100ms vom Zeitstempel des Untertitels ab.\n- Die Anforderung besteht gleichermaßen für Live-Untertitel. Es ist zu erwarten, dass bei Live-Untertiteln mit Verzögerungen durch die Konvertierung von gesprochener Sprache in Text zu rechnen ist.", false, "7.1.2 Synchronisation der Untertitel" },
                    { 29, 3, "Bietet die Software die Möglichkeit, Videos mit Untertiteln zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Untertitel und die Synchronität zum Ton erhalten bleiben. Die Prüfschritte 7.1.1 Wiedergabe von Untertiteln und 7.1.2 Synchronisation der Untertitel benötigen den Status „Erfüllt“. Zusätzliche Darstellungsaspekte des Textes, wie beispielsweise die Position, die Textfarben, der Textstil und die Schriftarten, können je nach regionalen Konventionen eine Bedeutung vermitteln. Eine Änderung dieser Darstellungsaspekte könnte die Bedeutung verändern und sollte nach Möglichkeit vermieden werden.", false, "7.1.3 Erhaltung von Untertiteln" },
                    { 30, 3, "Bietet die Software Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, muss die Darstellung der Untertitel individuell konfiguriert werden können. Die EN 301 549 definiert weder Art noch Umfang der Möglichkeiten, sondern benennt folgende Beispiele: Hintergrund- und Vordergrundfarbe, Schriftart, Kontur oder Ränder der Schriftarten und Deckkraft des Hintergrunds. Somit gilt der Prüfschritt als erfüllt, wenn wenigstens eine der Möglichkeiten konfigurierbar ist. Der Prüfschritt gilt auch dann als erfüllt, wenn die Untertitel systemseitige Benutzerpräferenzen wie Schriftgröße oder Farbe übernehmen. Bei dauerhaft angezeigten Untertiteln (open captions) ist der Prüfschritt nicht anwendbar.", false, "7.1.4 Anpassungen für Untertitel" },
                    { 31, 3, "Bietet die Software Videos mit ein- und ausblendbaren Untertiteln (closed captions) an, müssen diese Untertitel als Ton ausgegeben werden können. Die Originaltonspur soll ersetzt oder zumindest stark abgedimmt werden. Untertitel, die fester Bestandteil des Videos sind (open captions), sind von dieser Anforderung ausgenommen.", false, "7.1.5 Gesprochene Untertitel" },
                    { 32, 3, "Bietet die Software eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription als eigene Tonspur auswählbar und aktivierbar sein.", false, "7.2.1 Wiedergabe der Audiodeskription" },
                    { 33, 3, "Bietet die Software eine Audiodeskription für verfügbare Videos an, muss die Audiodeskription synchron zu Bild und Ton wiedergegeben werden.", false, "7.2.2 Synchronisation der Audiodeskription" },
                    { 34, 3, "Bietet die Software die Möglichkeit, Videos mit Audiodeskription zu übertragen, zu konvertieren oder aufzuzeichnen, müssen die Audiodeskription und die Synchronität zu Bild und Ton beim generierten Video erhalten bleiben.", false, "7.2.3 Erhaltung der Audiodeskription" },
                    { 35, 3, "Bietet die Software Videos mit synchroner Bild- und Tonspur in Verbindung mit Untertiteln und/oder Audiodeskriptionen an, müssen die Bedienelemente zum Aktivieren und Deaktivieren von Untertitel und Audiodeskription auf derselben Interaktionsebene wie die Wiedergabesteuerung zur Verfügung gestellt werden.", false, "7.3 Bedienelemente für Untertitel und Audiodeskription" },
                    { 36, 1, "Keine separate **Anforderung** in diesem Prüfschritt; Details siehe Unterprüfschritte 11.1.1.1a–11.1.1.1c.", false, "11.1.1.1 Nicht-Text-Inhalt" },
                    { 37, 1, "Alle informativen Grafiken und Objekte verfügen über eine Textalternative, die den Inhalt passend beschreibt. Die Software bleibt vollständig durch Alternativtexte nutzbar, auch wenn Grafiken oder Objekte nicht geladen werden oder vom Nutzenden aus technischen Gründen deaktiviert sind.\n\nAlternativtexte ersetzen das Objekt, wenn es beispielsweise nicht angezeigt werden kann oder mittels Screenreader fokussiert wird. Sie sind möglichst kurz. Hierbei ist darauf zu achten, dass der Alternativtext keine Bildbeschreibung darstellt, sondern ausschließlich die Kerninformation vermittelt. Der Text soll in seiner Struktur möglichst simpel und zusammengefasst formuliert sein. Ausführliche Beschreibungen, beziehungsweise weiterführende Informationen werden im Kontext der Abbildung erläutert.", false, "11.1.1.1a Alternativtexte für informative Grafiken" },
                    { 38, 1, "Alle Grafiken und Objekte (Objekte ohne auslösbare Funktion, beispielsweise dekorative Bilder, Farbflächen, Abstandshalter oder Symbole mit sichtbarer Beschriftung ohne Funktion), die keinen informativen Mehrwert bieten, sind als dekorative Objekte gekennzeichnet. Die entsprechende Kennzeichnung als dekoratives Objekt wird an den Screenreader übermittelt. Der Screenreader ignoriert das Objekt und fokussiert es nicht.", false, "11.1.1.1b Alternativtexte für dekorative Grafiken" },
                    { 39, 1, "Für bildbasierte oder Audio-CAPTCHAs sind Alternativtexte verfügbar, die den Zweck des Objekts beschreiben und über Alternativen zum jeweiligen CAPTCHA informieren. Es muss mindestens eine Alternative für grafische oder Audio-CAPTCHAs vorhanden sein. Das alternative CAPTCHA muss im unmittelbaren Kontext zur Verfügung stehen.", false, "11.1.1.1c Alternativen für CAPTCHAs" },
                    { 40, 3, "Aufgezeichnete Audioinhalte und Videos ohne Ton, die Informationen übermitteln, beziehungsweise einen informativen Mehrwert bieten, benötigen äquivalente Medienalternativen im unmittelbaren Kontext, es sei denn, sie stellen bereits eine Medienalternative für Textinhalte dar. Die Medienalternative kann direkt in der Software, oder in einer externen Quelle bereitgestellt werden.", false, "11.1.2.1 Alternativen für aufgezeichnetes Audio und Videos ohne Ton" },
                    { 41, 3, "Bietet die Software aufgezeichnete Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden, es sei denn, sie stellen bereits eine Medienalternative für andere Inhalt dar. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers etc.) bereitgestellt werden.", false, "11.1.2.2 Untertitel für aufgezeichnete Videos" },
                    { 42, 3, "Bietet die Software aufgezeichnete Videos mit informativer Tonspur, deren visuelle Inhalte zusätzliche Informationen übermitteln (beispielsweise Informationen zu Aktionen, Charakteren, Szenenänderungen, Geräusche, Bildschirmtext), müssen diese Inhalte in einer Medienalternative als Audiodeskription (WCAG AA) wie in Prüfschritt 11.1.2.5 Audiodeskription für aufgezeichnete Videos beschrieben verfügbar sein. Wurde der Prüfschritt 11.1.2.5 Audiodeskription für aufgezeichnete Videos erfolgreich bewertet, ist keine weitere Prüfung von Medienalternativen notwendig, selbst wenn zusätzlich zur Audiodeskription weitere Alternativen angeboten werden.\n\nIst keine Audiodeskription verfügbar, muss mindestens eine Volltext-Alternative (WCAG A) bereitgestellt werden.\n\nBieten die visuellen Inhalte keinen weiteren Informationen, die zum Gesamtverständnis wichtig sind, oder stellt das Video bereits eine Medienalternative dar, oder ist das Video ein Gebärdensprachenvideo, ist keine der genannten Alternativen notwendig.", false, "11.1.2.3 Audiodeskription oder Medienalternative für aufgezeichnete Videos" },
                    { 43, 3, "Bietet die Software Live-Videos mit synchronisierter, informativer Tonspur, so müssen Untertitel als Medienalternative bereitgestellt werden. Untertitel müssen sowohl für sprachliche als auch für nichtsprachliche Audioinhalte (Geräuscheffekte, Musik, Lachen, Identifizierung und Position des Sprechers, etc.) bereitgestellt werden.\n\nBei Liveübertragungen können die Untertitel aufgrund der technischen Machbarkeit mit leichter Latenz angezeigt werden. Die Bewertung liegt im Ermessensspielraum des Prüfers.", false, "11.1.2.4 Untertitel für Live-Videos" },
                    { 44, 3, "Bietet die Software aufgezeichnete Videos mit informativer Tonspur, deren visuelle Inhalte zusätzliche Informationen übermitteln (beispielsweise Informationen zu Aktionen, Charakteren, Szenenänderungen, Geräusche, Bildschirmtext), muss eine Audiodeskription als Medienalternative zur Verfügung stehen.\n\nBieten die visuellen Inhalte keinen weiteren Informationen, die zum Gesamtverständnis wichtig sind, oder stellt das Video bereits eine Medienalternative dar, oder ist das Video ein Gebärdensprachenvideo, ist keine der genannten Alternativen notwendig.\n\nDie Audiodeskription kann auf verschiedenen Wegen angeboten werden:\n\n- Die Audiodeskription ist in der Standard-Tonspur enthalten.\n- Die Audiodeskription ist in einer separaten Tonspur aktivierbar.\n- Die Audiodeskription ist als weitere Videoversion im unmittelbaren Kontext des Videos verfügbar.", false, "11.1.2.5 Audiodeskription für aufgezeichnete Videos" },
                    { 45, 1, "Keine separate **Anforderung** in diesem Prüfschritt; Details siehe Unterprüfschritte 11.1.3.1a–11.1.3.1c.", false, "11.1.3.1 Info und Beziehungen" },
                    { 46, 1, "Überschriften und Hierarchieebenen werden durch geeignete Objekte visualisiert und repräsentiert. Diese visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.\n\nDie Ausgabe von Überschriftsebenen ist unter Umständen abhängig vom verwendeten Framework technisch noch nicht vollumfänglich möglich, daher darf das Vorhandensein von Überschriftsebenen derzeit noch nicht gefordert werden. Falls Ebenen ausgegeben werden, müssen sie hierarchisch korrekt verschachtelt sein (beispielsweise H1 gefolgt von H2, gefolgt von H3, etc.). Das Auslassen von Überschriftsebenen wird nicht als Fehler gewertet, sofern die hierarchische Reihenfolge der Überschriften logisch ist.\n\nWortwahl oder Stil der Überschriften sind nicht Teil der Prüfung.", false, "11.1.3.1a Strukturelemente für Überschriften" },
                    { 47, 1, "Listen, Auflistungen, Aufzählungen und andere listenartige Inhalte werden über geeignete Objekte visualisiert. Die visuell erkennbaren Objekte sind programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar.\n\nListeneinträge sollen möglichst ein einzelnes, fokussierbares Objekt sein, um die Navigation zwischen Einträgen für Screenreadernutzende zu vereinfachen. Die im Eintrag gegebenenfalls vorhandenen Interaktionsmöglichkeiten sollen über die Steuerungsmöglichkeiten des Screenreaders (in der Regel mittels Wischnavigation nach oben und unten) erreichbar sein. Zusätzliche Informationen wie Anfang und Ende der Liste, Anzahl der Einträge und die Position eines einzelnen Eintrags innerhalb der Liste sollen ermittelbar sein.\n\nBeispielhafte Umsetzung:\n\n- Beim Fokussieren eines Listeneintrags gibt der Screenreader folgenden Inhalt aus: [Name des Listeneintrags]; Listeneintrag X von Y;\n- Gibt es weitere Interaktionsmöglichkeiten mit Listeneinträgen, wie beispielsweise im Navigationsbereich oder in Formularen, werden die konkreten Rollen zusätzlich zur Position in der Liste ausgegeben.\n\nWortwahl oder Stil der Inhalte sind nicht Teil der Prüfung.", false, "11.1.3.1b Strukturelemente für Listen, Auflistungen und Aufzählungen" },
                    { 48, 1, "Die Gliederung und Strukturierung von Inhalten werden durch geeignete Objekte visualisiert. Diese visuell erkennbaren Objekte sind zudem programmatisch ermittelbar und bedienbar, wichtige Informationen sind im Kontext für alle Nutzenden wahrnehmbar. Assistive Technologien wie beispielsweise Screenreader bieten umfangreiche Möglichkeiten, um durch Strukturobjekte zu navigieren, ohne den gesamten Inhalt durchlaufen zu müssen. Dazu gehören beispielsweise Container, Links, Formularelemente, Tabellen, Artikel, Tasten, Text- und Suchfelder oder Bilder. Wortwahl oder Stil der Objekte sind nicht Teil der Prüfung.\n\nTabellen dürfen nicht für Layout- oder Designzwecke verwendet werden. Falls tabellarischer Aufbau rein zu Layoutzwecken verwendet wird, soll die Tabelle nicht als Strukturelement gekennzeichnet sein.\n\nDie Beschriftung von Formularelementen soll beim Erreichen programmatisch ermittelbar sein. Der Screenreader gibt dabei den Namen des jeweiligen Elements aus. Gruppen von Formularelementen sollen eine Gruppenbeschriftung beinhalten, um den Kontext der einzelnen Elemente besser erfassen zu können.", false, "11.1.3.1c Strukturelemente für weitere Objekte" },
                    { 49, 1, "Beim Einsatz von assistiven Technologien wie beispielsweise Screenreadern oder auch bei der Tastaturnutzung erhalten die Inhalte den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Screenreadernutzenden optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei von Element zu Element, bei Tastaturbedienung (mit aktivem Screenreader) von interaktivem Element zu interaktivem Element.\n\nAusgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden.\n\n#todo#Bei einem Seitenaufruf soll bei der Tastaturnavigation das erste Element im Fokus die Seitenüberschrift sein. Ist keine Seitenüberschrift vorhanden, soll die Zurück-Taste als erstes Element fokussiert werden. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.\n\nBei Bedienung mit externer Tastatur soll das erste Element im Fokus das erste interaktive Element sein. Kontextelemente sind mit den Pfeiltasten erreichbar.\n\nBei modalen Dialogen muss das erste Element innerhalb des Dialogs fokussiert werden. Eine Wischnavigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Element befinden. Sollte der Dialog beim Start der Software oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Element der Seite befinden.", false, "11.1.3.2 Sinnvolle Reihenfolge" },
                    { 50, 1, "Die Inhalte der Software werden auf inhaltliche Verweise mittels sensorischer Merkmale wie Form, Größe, Position, Ausrichtung, Orientierung oder Ton geprüft. Solche Verweise dürfen nicht ausschließlich mittels der jeweiligen Sinneswahrnehmung verständlich sein.", false, "11.1.3.3 Sensorische Eigenschaften" },
                    { 51, 1, "Inhalte der Software sind hinsichtlich Ansicht und Bedienung nicht auf eine einzelne Anzeigeausrichtung im Hoch- oder Querformat beschränkt. Inhalte und Funktionen müssen bei Änderung der Fenstergröße nicht zwingenderweise in derselben Form angeboten werden. Situationsbedingt dürfen spezifische Seiten ausschließlich eine Ausrichtung unterstützen, sofern die Ausrichtung zur korrekten Darstellung, zum Verständnis oder zur Bedienung essenziell ist, beispielsweise bei einem Bankscheck, einer Klaviatur einer Klavieranwendung oder bei Inhalten der virtuellen Realität.", false, "11.1.3.4 Keine Beschränkung der Bildschirmausrichtung" },
                    { 52, 1, "Eingabefelder, die sich eindeutig auf Nutzerdaten beziehen und Informationen wie beispielsweise Name, E-Mailadresse, postalische Adresse, Rufnummer oder Kreditkartendaten anfordern, müssen programmatisch ermittelbar sein, damit unterstützende Technologien Nutzenden den Zweck der Eingabe erläutern können.", false, "11.1.3.5 Eingabezweck bestimmen" },
                    { 53, 1, "Farbe wird nicht als einzige visuelle Möglichkeit zur Übermittlung von Informationen, zur Anzeige einer Aktion, zum Anfordern einer Reaktion oder zur Unterscheidung eines visuellen Elements verwendet.", false, "11.1.4.1 Verwendung von Farbe" },
                    { 54, 3, "Für Audio-Inhalte, die in der Software automatisch und länger als 3 Sekunden abgespielt werden, muss ein Steuerelement zum Pausieren, Stoppen des Audios, oder eine Lautstärkeregelung zum Steuern der Audiolautstärke unabhängig von der Lautstärke des Gesamtsystems zur Verfügung stehen. Entsprechende Steuerelemente müssen am Seitenbeginn, beziehungsweise im unmittelbaren Kontext des Elements auffindbar sein.", false, "11.1.4.2 Audio-Steuerelement" },
                    { 55, 1, "Die Kontraste von Text sind in allen Zuständen des Texts ausreichend. Hierbei wird bei Software grundsätzlich ein Kontrastverhältnis von 4,5:1 zugrunde gelegt.\n\nDie Anforderung gilt nicht für Textinhalte, die Teil einer inaktiven Benutzeroberflächenkomponente, rein dekorativ, für niemanden sichtbar oder Teil eines Bildes sind, das bedeutende andere visuelle Inhalte enthält. Weiterhin sind Texte eines Logos oder Markennamen ausgenommen.\n\nDie Prüfung erfolgt in der Standarddarstellung der Software. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.\n\nTrotz ausreichender Kontrastwerte ist die Kombination von gesättigtem Rot mit Schwarz als Vorder- und Hintergrundfarbe für Texte nicht empfehlenswert (vgl. ISO 9241-125). Es existieren keine klaren Vorgaben für Farbwerte von gesättigtem Rot, daher kann der Prüfende nur aufgrund seiner subjektiven Wahrnehmung urteilen. Ein entsprechender Befund sollte nur als Empfehlung mitgeteilt werden.\n\nBei Texten, die auf einem Bild oder einem Farbverlauf liegen, ist der vermeintlich schwächste Kontrastzustand zu messen.", false, "11.1.4.3 Kontrast von Texten" },
                    { 56, 1, "Die Textgröße kann auf mindestens 200% skaliert werden, ohne dass Inhalt oder Funktionalität dabei verloren gehen. Die Vergrößerung kann über die Einstellungen der Textgröße im Betriebssystem, oder über eine softwarespezifische Funktionalität zur Verfügung gestellt werden. Beim Bereitstellen einer eigenen Funktionalität müssen die zugehörigen Steuerelemente am Seitenbeginn auffindbar sein.", false, "11.1.4.4 Textgröße ändern" },
                    { 57, 1, "Bilder oder Grafiken werden nicht zur Darstellung von Schrift verwendet. Logos, beziehungsweise Text, der Teil eines Logos oder Markennamens ist, ist hiervon ausgenommen. Eine Schriftgrafik ist eine visuelle Gestaltung und Anordnung von Buchstaben, Zeichen und Textelementen innerhalb eines grafischen Elements. Wird der Informationsinhalt der Schriftgrafik auch im Kontext als Text wiedergeben, gilt dies als Medienalternative.", false, "11.1.4.5 Schriftgrafiken" },
                    { 58, 1, "- Alle Schriften der Anwendung können mit den Schriftgröße-Einstellungen der Plattform oder der Anwendung auf 400% vergrößert werden, ohne dass dabei Inhalt oder Funktionalität verloren gehen.", false, "11.1.4.10 Automatischer Umbruch (Reflow)" },
                    { 59, 1, "Die Kontraste von grafischen Bedienelementen und informationstragenden Grafiken sind mit einem Kontrastverhältnis von mindestens 3:1 ausreichend. Bei der Kontrastmessung steht der informationstragende Bereich oder die Hervorhebung des Zustands eines Elements im Fokus.\n\nDie Anforderung gilt nicht für inaktive Benutzeroberflächenkomponenten, inaktive Zustände von Bedienelementen, Logos beziehungsweise Markennamen, und grafische Inhalte, deren Inhalt nicht verändert werden darf, beispielsweise Flaggen, Screenshots oder Heatmaps.\n\nDie Anforderung gilt weiterhin nicht für native grafische Bedienelemente, die in ihrem Erscheinungsbild nicht beeinflusst werden können, beispielsweise native Switches in ihren jeweiligen Zuständen, Checkboxen oder Radio-Buttons.\n\nDie Prüfung erfolgt in der Standarddarstellung der Software. Eventuell vorhandene, alternative Darstellungen oder Mechanismen zum Verändern des Designs sind nicht Teil der Prüfung und werden nicht berücksichtigt.", false, "11.1.4.11 Nicht-Text-Kontrast" },
                    { 60, 1, "Das Anpassen der Zeilenhöhe auf mindestens das 1,5-fache der Textgröße, des Abstands nach Absätzen auf mindestens das 2-fache der Textgröße, des Zeichenabstands auf mindestens das 0,12-fache der Textgröße und des Wortabstands auf mindestens das 0,16-fache der Textgröße führt nicht zu Verlust von Inhalt oder Funktionalität.", false, "11.1.4.12 Textabstand" },
                    { 61, 1, "Inhalte, die durch Tastatur-Fokus oder Mauszeiger-Hover aktiviert oder eingeblendet werden, sind für Nutzende navigierbar. Die eingeblendeten Inhalte werden nicht unerwartet oder automatisch geschlossen, sind jedoch jederzeit durch Nutzende schließbar. Das Schließen eines eingeblendeten Inhalts versetzt nicht den ursprünglichen Fokus.", false, "11.1.4.13 Eingeblendete Inhalte (Hover oder Fokus)" },
                    { 62, 1, "Die Software ist ohne Verwendung einer Maus ausschließlich über eine Tastatur bedienbar. Ein eventuell auf der Tastatur vorhandenes Track-Pad ist nicht Bestandteil der Prüfung. Falls die Seite scrollbare Bereiche enthält, sollen die nicht sichtbaren Inhalte über die Tastatur erreichbar sein. Die Reihenfolge, in der die Elemente fokussiert werden, spielt in diesem Prüfschritt keine Rolle.\n\nPrüfende müssen mit der Funktionsweise der Tastaturbedienung vertraut sein.", false, "11.2.1.1 Ohne Maus nutzbar" },
                    { 63, 1, "Der Fokus bei Tastaturnavigation muss nach Erreichen eines Elements auch wieder vom Element wegbewegt werden können. Die Inhalte dürfen an keiner Stelle Elemente, Bereiche oder Menüs enthalten, die Mausinterkationen oder einen erzwungenen Neustart der Software benötigen, um wieder verlassen werden zu können.", false, "11.2.1.2 Keine Tastaturfalle" },
                    { 64, 1, "Verwendet die Software Tastaturkürzel über Einzeltasten (Buchstaben, Zahlen, Satzzeichen, Symbole), müssen die Tastaturkürzel abschaltbar sein oder auf eine Tastenkombination mit Verwendung einer zusätzlichen Modifikator-Taste umgestellt werden können, oder das Tastaturkürzel ist nur aktiv, wenn die entsprechende Benutzeroberflächenkomponente den Fokus erhält. Für Mehrtastenkombinationen darf die Umschalttaste nur in Kombination mit einer weiteren Modifikationstaste verwendet werden.", false, "11.2.1.4 Tastaturkürzel" },
                    { 65, 1, "Bei zeitbegrenzten Inhalten kann die Zeitbegrenzung abgeschaltet, angepasst oder verlängert werden. Diese Anforderung gilt nicht für Limitierungen, die unverzichtbar sind oder ein Echtzeit-Ereignis darstellen. Es gelten folgende Anforderungen:\n- Abschalten Die Zeitbegrenzung kann abgeschaltet werden, bevor sie eintritt.\n- Anpassen Die Zeitbegrenzung kann angepasst werden, bevor sie eintritt, mindestens bis zum Zehnfachen der Standardeinstellung.\n- Verlängern Die Zeitbegrenzung warnt vor Ablauf und stellt mindestens 20 Sekunden Zeit zur Verfügung, um die Zeitbegrenzung mit einer einfachen Handlung zu verlängern. Die Verlängerung kann mindestens 10mal vorgenommen werden.\n- Echtzeit-Ausnahme Die Zeitbegrenzung ist ein erforderlicher Teil eines Echtzeit-Ereignisses und es ist keine Alternative zur Zeitbegrenzung möglich.\n- Unverzichtbare Ausnahmen Die Zeitbegrenzung ist unverzichtbar und ihre Verlängerung würde den Vorgang ungültig machen.\n- 20 Stunden Ausnahme Die Zeitbegrenzung beträgt standardmäßig mehr als 20 Stunden.", false, "11.2.2.1 Zeitvorgaben anpassbar" },
                    { 66, 1, "Ablenkender Inhalt durch Blinken oder Bewegung soll vermieden werden, zeitlich auf maximal 5 Sekunden begrenzt oder abschaltbar sein. Bewegter Inhalt, der Teil einer essenziellen Aktivität ist, ist von dieser Anforderung ausgenommen. Die Anforderung gilt für jegliche Inhalte, die blinken oder automatisch aktualisiert oder bewegt werden.", false, "11.2.2.2 Bewegte Inhalte abschaltbar" },
                    { 67, 1, "Die Software enthält keine Elemente, die in einem Zeitraum von 1 Sekunde öfter als dreimal aufblitzen.", false, "11.2.3.1 Verzicht auf Flackern" },
                    { 68, 1, "Bei Bedienung der Software mit einer Tastatur erhalten interaktive Bedienelemente den Fokus in einer Reihenfolge, die der logischen beziehungsweise visuellen Struktur, oder einem für Tastaturnutzende optimiertem Bedienablauf entspricht. Die Navigation erfolgt hierbei anhand der Bedienelemente von Element zu Element.\n\nAusgeblendete oder unsichtbare Inhalte, Layout-Objekte oder Inhalte, die im Hintergrund liegen, dürfen dabei nicht fokussiert werden. Bei einem Seitenaufruf soll das erste Element im Fokus das erste Bedienelement sein. Bietet die Seite eine zentrale Funktion (beispielsweise das Suchfeld der Google Suche), so kann das Suchfeld direkt fokussiert werden.\n\nBei modalen Dialogen muss das erste Bedienelement innerhalb des Dialogs fokussiert werden. Eine Navigation ist nur innerhalb des Dialogs möglich, bis er geschlossen wird. Nach Schließen des Dialogs muss sich der Fokus am zuvor auslösenden Bedienelement befinden. Sollte der Dialog beim Start der Software oder beim Aufruf einer Seite automatisch geöffnet werden, muss sich der Fokus nach Schließen des Dialogs am ersten Bedienelement der Seite befinden.", false, "11.2.4.3 Sinnvolle Reihenfolge bei Bedienung mit externer Tastatur" },
                    { 69, 1, "Der Zweck oder das Ziel eines Links kann direkt aus Beschriftung des Links, oder im unmittelbaren Kontext des Links bestimmt werden. Die Beschriftung von Links ist eindeutig. Werden innerhalb eines Kontextes mehrere Links verwendet, muss eine Unterscheidung durch die Beschriftung des Links gewährleistet sein, oder es wird ein anderes unterscheidbares Merkmal verwendet. Dies kann über kontextbezogene, weitere Informationen wie beispielsweise eine verständliche Information auf der Seite, die Handhabung oder Besonderheiten der Links erklärt, durch den Alternativtext einer verlinkten Grafik, durch eine vorhergehende Überschrift, oder durch Piktogramme im Fall von Dokumenten-Links umgesetzt werden.\n\nWird zum Öffnen des Ziels eine andere Anwendung außerhalb der zu testenden Software geöffnet, oder ist das Ziel ein Dokument, erhält der Nutzende eine entsprechende Information, sofern das Verlassen der Software nicht aus dem Kontext heraus ersichtlich ist.\n\nVerlinkte URLs, E-Mailadressen und Telefonnummern übermitteln ihren Zweck automatisch über das Format und werden nicht bewertet.", false, "11.2.4.4 Aussagekräftige Beschriftungen von Links" },
                    { 70, 1, "Inhalte werden visuell durch Überschriften strukturiert und beschreiben den nachfolgenden Inhalt zutreffend. Formularelemente sind aussagekräftig und zum Element passend beschriftet.", false, "11.2.4.6 Überschriften und Beschriftungen" },
                    { 71, 1, "Während der Bedienung mit einer Tastatur, mit inaktivem Screenreader oder mit Tastatur und aktivem Screenreader muss der Fokus zu jeder Zeit deutlich sichtbar sein. Der Fokus wird hierbei durch einen deutlich sichtbaren Rahmen, der das Element umrahmt, oder durch farbliches Ausfüllen der Elementfläche visualisiert.\n\nSeiten, die mehr Inhalt bereitstellen als auf dem Bildschirm des Endgeräts angezeigt werden kann, müssen den Bildschirminhalt den Fokus verfolgend automatisch verschieben.\n\nIn Eingabefeldern muss zusätzlich zur Fokussierung des Elements der Cursor im Eingabefeld angezeigt werden.\n\nVersteckte Sprunglinks sollen bei Fokuserhalt eingeblendet werden.\n\nFür die Darstellung des Fokus gilt generell eine Kontrastanforderung von 3:1 zum Hintergrund und zum interaktiven Element.", false, "11.2.4.7 Fokus sichtbar" },
                    { 72, 1, "Während der Bedienung mit einer Tastatur, mit inaktivem Screenreader oder mit Tastatur und aktivem Screenreader darf der Fokusrahmen, genauso wenig wie lediglich ein deutlich sichtbarerer Teil des Fokusrahmens zu keiner Zeit von anderen Inhalten oder Inhaltselementen der Software teilweise oder vollständig verdeckt werden. Dies gilt auch für durch Nutzende eingeblendete Inhalte, wie Ausklapplisten oder Menüs.", false, "11.2.4.11 Fokus nicht verdeckt (Minimum)" },
                    { 73, 1, "Komplexe Gesten umfassen pfadbasierte Bewegungen und Mehrpunkt-Gesten. Funktionen, zu deren Bedienung komplexe Gesten notwendig sind, benötigen eine zugängliche Alternative zum Auslösen der Funktion, beispielsweise Tippen, Doppeltippen, Tippen und Halten, oder entsprechende Bedienelemente, beispielsweise Tasten.\n\nPfadbasierte Bewegungen sind beispielsweise Wischbewegungen, Schieberegler und Karussells, die von der Richtung der Interaktion abhängen, sowie andere Gesten, die einen vorgeschriebenen Pfad verfolgen, wie das Zeichnen einer spezifischen Form. Solche Pfade können mit einem Finger oder Stift auf einem Touchscreen, Grafiktablett oder Trackpad gezeichnet werden.\n\nZiehen ist eine Bewegung, bei der der Nutzende ein Objekt mit einem Finger aufnimmt und es an eine andere Position bewegt. Diese Bewegung vom Startpunkt zum Endpunkt erfordert nicht, dass der Nutzende einem bestimmten Pfad oder einer bestimmten Richtung folgt. Das Ziehen ist daher nicht pfadbasiert.\n\nMehrpunkt-Gesten sind beispielsweise das Zoomen durch Zusammenziehen von zwei Fingern, Steuerungen, bei denen ein Finger auf dem Bildschirm ruht und ein zweiter Finger tippt, oder ein Tipp oder Wisch mit zwei oder mehr Fingern.\n\nVon der Anforderung ausgenommen sind essenzielle Funktionen, für die pfadbasierte oder Mehrpunkt-Gesten unverzichtbar sind, beispielsweise das Signieren von Dokumenten mittels Unterschrift.", false, "11.2.5.1 Zeigergesten (Alternativen für komplexe Zeiger-Gesten)" },
                    { 74, 1, "Interaktive Elemente erfüllen bei Berührung des Elements mit einem einzelnen Finger, Mausklick oder Teilklick (Mouse-Down) mindestens eine der folgenden Bedingungen:\n\n- Kein Down-Event Das Down-Event führt nicht direkt zum Auslösen der Funktion. Das bedeutet, die Funktion wird nicht durch das reine Drücken des interaktiven Elements ausgelöst. Erst das Loslassen des Elements, indem der Finger nach oben bewegt bzw. die Maustaste wieder losgelassen wird, löst die Funktion aus.\n- Abbrechen oder Rückgängig machen Die Ausführung der Funktion erfolgt beim Up-Event. Es steht ein Mechanismus zur Verfügung, um die Funktion vor Abschluss abzubrechen, oder die Funktion nach Auslösen rückgängig zu machen.\n- Up Reversal Das Loslassen des Elements (Up-Event) kehrt die Auswirkungen des vorherigen Drückens (Down-Event) mit dem Finger um. Wenn beispielsweise das Drücken einer Taste etwas aktiviert, wird durch das Loslassen der Taste der Vorgang rückgängig gemacht.\n- Essenziell Die Funktion muss beim Drücken des Elements (Down-Event) durchgeführt werden. Es gibt keine Option zum Abbrechen oder Rückgängigmachen. Die Funktion wird unmittelbar beim Drücken abgeschlossen (beispielsweise beim Schreiben mit einem Finger oder einem Stift, oder beim Tippen mit einer virtuellen Tastatur).\n\nDie Anforderung gilt nicht für Kontextmenüs, die bei längerem Gedrückthalten eines interaktiven Elements geöffnet werden.", false, "11.2.5.2 Abbrechen oder Widerrufen einer Touch-Geste" },
                    { 75, 1, "Visuell sichtbare Beschriftungen von interaktiven Elementen, unabhängig davon, ob sie mit reinem Text oder mit Text in grafischen Objekten beschriftet sind, müssen die visuelle in der programmatisch ermittelbaren Beschriftung enthalten. Idealerweise wird die visuelle Beschriftung direkt am Anfang der programmatischen Beschriftung genannt.\n\nDie programmatische Beschriftung kann hierbei weitere Informationen enthalten, die visuell nicht ersichtlich ist.", false, "11.2.5.3 Sichtbare Beschriftung als Teil der programmatischen Beschriftung" },
                    { 76, 1, "Funktionen, die durch die Bewegung des Endgeräts ausgelöst werden, müssen auch durch Steuerelemente auf der Benutzeroberfläche bedient werden können. Die Reaktion auf die Bewegungsaktivierung kann deaktiviert werden, um ein versehentliches Betätigen zu verhindern, es sei denn, die Bewegungsaktivierung ist für die jeweilige Funktion essenziell (beispielsweise bei einer Schrittzähler-App, oder bei Navigation durch Orte mit unterstützender Anzeige von Informationen zum Ort).", false, "11.2.5.4 Steuerung durch Bewegung" },
                    { 77, 1, "Sämtliche Funktionen, die über Ziehbewegungen (Drag & Drop) ausgeführt werden können, müssen über eine alternative Eingabemethode (beispielsweise durch Tasten oder durch ein Kontextmenü) durchführbar sein.\n\nPfadbasierte Gesten sind nicht Bestandteil dieses Prüfschritts.\n\n- Definition einer Ziehbewegung: Der Start- und der Zielpunkt der Geste sind an feste Koordinaten gebunden. Die Route zwischen den Punkten spielt hingegen keine Rolle.\n- Definition einer pfadbasierte Geste: Der Start- und der Zielpunkt der Geste spielen keine Rolle, die Richtung oder Form der Route hingegen schon.", false, "11.2.5.7 Ziehbewegungen" },
                    { 78, 1, "User-Interface-Elemente, die die Funktion eines Ziels übernehmen (beispielsweise bei Drag & Drop Events), müssen während der Nutzung eine Zielgröße von mindestens 24 x 24 CSS-Pixel aufweisen. Hierbei gelten folgende Ausnahmen:\n\n- Abstand Ziele, die kleiner als 24 x 24 CSS-Pixel sind, bieten ausreichend Abstand zu anderen Zielen. Sie werden so positioniert, dass bei Zentrierung eines Kreises mit einem Durchmesser von 24 CSS-Pixeln auf dem Begrenzungsrahmen des jeweiligen Ziels, der positionierte Kreis keine anderen Ziele, oder andere Kreise für zu kleine Ziele überschneidet\n- Gleichwertige Alternative Die Funktion kann durch ein anderes Steuerelement (beispielsweise durch eine Taste oder ein Kontextmenü), das sich auf der gleichen Seite befindet, ausgeführt werden\n- Inline Das Ziel befindet sich in innerhalb eines Fließtexts, oder die Größe des Ziels wird durch die Zeilenhöhe von umgebendem, nicht interaktivem Text eingeschränkt\n- Kontrolle durch User-Agent Die Größe des Ziels wird vom User-Agent bestimmt und wird vom Autor:in nicht verändert\n- Essenziell Die Darstellung des Ziels ist unerlässlich oder gesetzlich vorgeschrieben", false, "11.2.5.8 Zielgröße (Minimum)" },
                    { 79, 1, "Die Software soll die Sprache, in der sie verwendet wird, an das Betriebssystem übermitteln, um die lokalisierte Anzeige von Text und die korrekte Auswahl des Sprachpakets und der korrekten Aussprache des Screenreaders sicherzustellen.\n\nDies betrifft mindestens die Standardsprache der Software. Bietet sie über eine Sprachauswahl entsprechende Lokalisierungen an, sind auch diese Sprachen Bestandteil der Anforderung.\n\nWorte oder einzelne Passagen oder Phrasen des Inhalts, die von der verwendeten Sprache abweichen, sind nicht Teil der Anforderung. Es wird jedoch empfohlen, entsprechende Inhalte zu kennzeichnen.\n\nDie Qualität von Lokalisierungen ist nicht Bestandteil der Prüfung.", false, "11.3.1.1 Sprache der Software" },
                    { 80, 1, "Der Fokuserhalt von Benutzeroberflächenkomponenten bei Tastaturbedienung (Tabulator oder Pfeiltasten) und bei Tastaturbedienung mit aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise plötzliches Öffnen von neuen Ansichten oder weiteren Applikationen oder Einblenden von neuen Bedienelementen).\n\nDas Anzeigen von Tooltips oder anderen Hinweisen, die sich beim Weiternavigieren selbständig wieder schließen, gilt nicht als unerwartete Kontextänderung.\n\nPop-Ups oder modale Dialoge sind nicht Bestandteil der Prüfung.", false, "11.3.2.1 Kontextänderung bei Fokus" },
                    { 81, 1, "Die Bedienung von Formularelementen (beispielsweise Checkbox, Radio-Button, Auswahlliste) und die Eingabe von Inhalten in Textfelder (Eingabefelder) bei Tastaturbedienung und bei Tastaturbedienung mit aktivem Screenreader darf nicht zu unerwarteten oder unangekündigten Kontextänderungen führen (beispielsweise unerwartetes Öffnen neuer Ansichten, unerwartetes Ausblenden der Tastatur während Texteingabe, oder automatische Großschreibung).\n\nSinnvolle oder notwendige Kontextänderungen müssen unterhalb des auslösenden Elements erfolgen. Der Fokus darf dabei nicht versetzt werden.\n\nWarnhinweise erscheinen als Antwort aufgrund von Eingaben der Nutzenden. Sofern Sie als Warnung in einem modalen Dialog implementiert wurden, sind sie nicht Bestandteil der Anforderung.\n\nEindeutige Kontextänderungen sind beispielsweise das Öffnen neuer Ansichten und das Einblenden neuer Inhalte auf derselben Ansicht, wenn diese Inhalte wie eine neue Seite wirken oder Inhalte in stark veränderter Anordnung erscheinen.\n\nReine Inhaltsänderungen gelten nicht als unerwartete Kontextänderungen bei Eingabe.", false, "11.3.2.2 Kontextänderung bei Eingabe" },
                    { 82, 1, "Wird eine seitenübergreifende Hilfefunktion angeboten, muss diese stets an der gleichen Position relativ zu den Inhalten angezeigt werden, es sei denn, der Nutzende verändert die Reihenfolge der Darstellung. Als Hilfefunktion gelten:\n\n- Kontakt-Angaben (beispielsweise Telefonnummern, E-Mailadressen)\n- Persönliche Kontaktaufnahme (beispielsweise Messenger-Dienste, Kontaktformulare, Social Media)\n- Möglichkeiten zur Selbsthilfe (beispielsweise Frequently Asked Questions, Hilfe-Seiten)\n- Automatisierte Kontaktmechanismen (beispielsweise Chat-Bots)", false, "11.3.2.6 Konsistente Hilfe" },
                    { 83, 1, "Bei fehlerhafter oder unvollständiger Eingabe in Formularfeldern müssen die betroffenen Formularfelder eindeutig identifizierbar sein und der Fehler in Textform beschrieben werden, sofern eine Fehlererkennung implementiert ist. Das Identifizieren der betroffenen Formularfelder kann hierbei auf verschiedene Weise erfolgen:\n\n- Die betroffenen Formularfelder werden am Seitenbeginn beschrieben.\n- Die betroffenen Formularfelder werden zusätzlich zur Fehlerbeschreibung deutlich hervorgehoben (Form und Farbe).\n- Die Beschriftung der betroffenen Formularfelder ändert sich.\n- Die Fehlerbeschreibung wird in unmittelbarer Nähe des betroffenen Formularfelds angezeigt.\n- Die Fehlerbeschreibung wird zusammen mit den betroffenen Formularfeldern in einem Warnhinweis angezeigt.\n\nIst keine Fehlererkennung implementiert, führt das nicht zu einer negativen Bewertung und der Prüfschritt ist nicht anwendbar.", false, "11.3.3.1 Fehlererkennung" },
                    { 84, 1, "Beschriftungen, oder falls notwendig Anweisungen, werden für Formularfelder bereitgestellt. Die jeweilige Beschriftung oder Anweisung soll bei Eingabefeldern links neben dem Element oder darüber, bei Checkboxen und Radio-Buttons rechts neben dem Element lokalisiert sein.\n\nPflichtfelder sind als solche eindeutig gekennzeichnet, beispielsweise durch die zusätzliche Beschriftung „(Pflichtfeld)“. Werden Sie mit Symbolen gekennzeichnet, ist eine Legende zu Beginn des Formulars oder in der Dokumentation der Software verfügbar, die die Symbole erklärt.\n\nSind spezielle Formate bei der Eingabe notwendig, wird das entsprechende Format vor dem Formularelement oder in der Vorbelegung des Formularelements beschrieben.\n\nEindeutig identifizierbare Formularfelder wie beispielsweise ein Suchfeld, das aus einem Eingabefeld und der Suchentaste besteht, benötigen unter Umständen keine eigene visuelle Beschriftung, da der Einsatzzweck aus dem Kontext ermittelt werden kann (beispielsweise durch Verwendung eines Lupe-Symbols als Taste). Sie müssen jedoch zu jederzeit programmatisch ermittelbar sein.\n\nDie Inhalte von Placeholder-Texten in Formularelementen gelten nicht als Beschriftung.", false, "11.3.3.2 Beschriftungen oder Anweisungen bei Formularfeldern" },
                    { 85, 1, "Sofern eine Fehlerkennung bei Formularfeldern implementiert ist, müssen die Fehlermeldungen eindeutig sein und Korrekturvorschläge zur Fehlerbehebung benannt werden, es sei denn, dies würde die Sicherheit oder den Zweck des Inhalts gefährden.", false, "11.3.3.3 Fehlerkorrektur" },
                    { 86, 1, "Beinhaltet die Software Prozesse, über die rechtlich bindende Verpflichtungen eingegangen, finanzielle Transaktionen ausgelöst, oder sensible Nutzerdaten übermittelt werden, so muss mindestens eine der folgenden Anforderungen erfüllt sein:\n\n- Vorgang kann rückgängig gemacht werden Das Übermitteln der Daten kann rückgängig gemacht werden.\n\n- Daten werden vor Übermittlung überprüft und bestätigt Vor Übermittlung der Daten können die Eingaben geprüft und korrigiert werden. Nach einer Bestätigung werden die Daten übermittelt.\n- Übermittlung nach Bestätigung Die Übermittlung der Daten erfolgt erst nach Bestätigung eines Dialogs, der die Folgen der Übermittlung beschreibt.\n\nErfolgreiche Übermittlungen sollten nach der Übertragung bestätigt werden.", false, "11.3.3.4 Fehlervermeidung" },
                    { 87, 1, "Müssen innerhalb eines Prozesses bestimmte, bereits zuvor eingegebene Daten eines Nutzenden erneut eingegeben werden, muss der Nutzende bei der erneuten Eingabe unterstützt werden. Als Unterstützung zählen:\n\n- Die Daten werden bei der Eingabe als Ergänzungsvorschlag angeboten\n- Es wird eine Funktion zum automatischen Ausfüllen bereitgestellt\n- Die Daten können innerhalb der gleichen Seite kopiert und eingefügt werden\n\nDaten, deren erneute Eingabe unerlässlich oder sicherheitsrelevant ist, oder falls die vorherige Eingabe nicht mehr gültig ist, sind hiervon ausgenommen.", false, "11.3.3.7 Redundanter Eintrag" },
                    { 88, 1, "Keine separate **Anforderung** in diesem Prüfschritt; Details siehe Unterprüfschritte 11.3.3.8a–11.3.3.8c.", false, "11.3.3.8 Zugängliche Authentifizierung (Minimum)" },
                    { 89, 1, "Der Anmeldeprozess kann ohne Einsatz kognitiver Fähigkeiten (beispielsweise das Merken eines Passworts, oder das Lösen eines Rätsels) durchgeführt werden, es sei denn, es wird mindestens eine der folgenden Möglichkeiten angeboten:\n\n- Einfügen des Passworts durch Kopieren und Einfügen von Text\n- Automatisches Einfügen des Passworts über den Passwortmanager des Betriebssystems\n- Anmeldung über einen Drittanbieterdienst (beispielsweise Anmeldung mit Apple-ID oder Google-Account)\n- Alternative Anmeldung über ein biometrisches Merkmal (beispielsweise Touch-ID oder Face-ID)\n- Alternative Anmeldung über das Scannen eines QR-Codes\n\nEs ist nicht ausreichend, wenn zur Anmeldung ausschließlich die Option zum Scannen eines QR-Codes zur Verfügung steht.", false, "11.3.3.8a Zugänglicher Anmeldeprozess" },
                    { 90, 1, "Verwendet der Anmeldeprozess eine Zwei-Faktor-Authentifizierung und versendet im Rahmen der Anmeldung einen Verifizierungscode direkt an das genutzte Endgerät, muss der Verifizierungscode über die Kopieren- und Einfügeoption des Betriebssystems, oder über Komfortfunktionen des Betriebssystems in das Eingabefeld der Anmeldung übertragen werden können.\n\nZwei-Faktor-Authentifizierungen, die ausschließlich über ein zweites Endgerät durchgeführt werden können, sind von dieser Anforderung ausgenommen.", false, "11.3.3.8b Zugängliche Zwei-Faktor-Authentifizierung" },
                    { 91, 1, "Verwendet der Anmeldeprozess die Lösung eines CAPTCHAs, kann das CAPTCHA auch ohne Einsatz kognitiver Fähigkeiten durchgeführt werden.\n\nCAPTCHAs, die den Einsatz kognitiver Fähigkeiten, sogenannte kognitive Funktionstests voraussetzen, erfordern logisches Schlussfolgern, sowie Fähigkeiten zum sprachlichen, numerischen und figural-bildhaften Denken. Zusätzlich kann spezielles Wissen, wie etwa Sprachkompetenzen und technisches Verständnis, abgefragt werden:\n\n- Merken und Wiedergeben von Buchstaben, Zeichen oder Mustern\n- Manuelles Abschreiben von Buchstaben und Zeichen\n- Prüfung der korrekten Rechtschreibung\n- Lösen von Rechenaufgaben\n- Lösen von Rätseln\n\nBei kognitiven Funktionstests handelt es sich explizit nicht um:\n\n- Erkennen von Bildern, Audio oder Video (beispielsweise Markieren aller PKWs, Markieren aller Bilder mit Ampeln)\n- Methoden zur Zwei-Faktor-Authentifizierung", false, "11.3.3.8c Zugängliche CAPTCHAs in Anmeldeprozessen" },
                    { 92, 1, "Der Prüfschritt ist immer anwendbar, kann aber nicht detailliert bewertet werden. Eventuell auf fehlerhaften Code zurückführbares Fehlerverhalten sollte als Hinweis dokumentiert werden. Ob solche Fehler auf einen falschen oder fehlerhaften Code zurückzuführen sind, kann aber nicht bewertet werden. Fehler im Code werden bereits in anderen Prüfschritten deutlich.", false, "11.4.1.1 Syntaxanalyse" },
                    { 93, 1, "Der Prüfschritt wird über die Prüfschritte 11.5.2.5 Objektinformationen , 11.5.2.7 Werte , 11.5.2.15 Änderungsbenachrichtigung und 11.5.2.16 Änderungen von Zuständen und Eigenschaften abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Sofern einer der genannten Prüfschritte als nicht voll erfüllt gewertet wird, wird auch dieser Prüfschritt entsprechend gewertet, oder dieser Prüfschritt erhält die Bewertung „Nicht getestet“.", false, "11.4.1.2 Name, Rolle, Wert" },
                    { 94, 1, "Dynamische Aktualisierungen des Inhalts, die keine Kontextänderung darstellen und dem Nutzenden Informationen über beispielsweise den Status einer Aktion, den Fortschritt eines Prozesses oder das Erkennen von Fehlern bereitstellen, müssen programmatisch anhand geeigneter Rollen oder Eigenschaften durch assistive Technologien ermittelt werden können, ohne explizit den Fokus zu erhalten. Die Benachrichtigung zu solchen Ereignissen muss automatisch an das assistive System übermittelt und ausgegeben werden.\n\nKontextänderungen, beispielsweise neue Ansichten, neue Interaktionsmöglichkeiten; neue Inhalte auf einer Ansicht, oder Hinweise über Dialoge, werden durch assistive Technologien durch aktive Fokussierung wahrgenommen und sind hiervon nicht betroffen.\n\nStatusmeldungen, die durch das Betriebssystem selbst generiert ausgegeben werden, sind hiervon nicht betroffen.", false, "11.4.1.3 Statusmeldungen" },
                    { 95, 1, "Assistive Systeme werden durch die Software unterstützt, indem sie mindestens die Anforderungen in den Prüfschritten 11.5.2.5 bis 11.5.2.17 bedient. Im Fokus steht die generelle Unterstützung der gängigen assistiven Systeme Screenreader, Bildschirmlupe und Sprachsteuerung, gegebenenfalls zusätzlich in Verbindung mit einer Tastatur. Die Detailbetrachtung erfolgt in den nachfolgenden Anforderungen.", false, "11.5.2.3 Verwendung von Barrierefreiheitsdiensten" },
                    { 96, 1, "Interaktive Bedienelemente müssen ihren Namen, ihre Rolle, und gegebenenfalls vorhandene Grenzen oder Beschreibungen korrekt an die assistiven Systeme übermitteln und programmatisch ermittelbar sein. Die programmatische Beschriftung muss die sichtbare Beschriftung mindestens zu Beginn der Beschriftung beinhalten. Idealerweise sind die Beschriftungen in beiden Varianten analog. Dies betrifft beispielsweise:\n\n- Tasten\n- Tasten mit eingebetteter Grafik\n- Umschalttasten\n- Kontrollkästchen\n- Optionsfeld\n- Registerkarten\n- Ausklapplisten\n- Formularfelder\n- Links\n- Links mit eingebetteter Grafik", false, "11.5.2.5 Objektinformationen" },
                    { 97, 1, "Der Aufbau von Tabellen ist strukturell korrekt und programmatisch ermittelbar. Die Zuordnung jeder Zelle zu ihrer zugehörigen Zeile und Spalte, der Umfang an Zeilen und Spalten, als auch der Anfang und das Ende der Tabelle sind identifizierbar. Falls vorhanden, werden die Überschriften von Zeile und Spalte wiedergegeben.", false, "11.5.2.6 Datentabellen" },
                    { 98, 1, "Interaktive Bedienelemente, die Werte aufweisen, übermitteln den aktuellen Wert und gegebenenfalls die Position oder die entsprechenden Grenzen korrekt an die assistiven Systeme. Dies betrifft beispielsweise:\n\n- Umschalttasten (aktiviert, nicht aktiviert)\n- Kontrollkästchen (ausgewählt, nicht ausgewählt)\n- Auswahlschalter (aktiviert, nicht aktiviert)\n- Registerkarten (ausgewählt, nicht ausgewählt)\n- Ausklapplisten (erweitert, nicht erweitert)\n- Formularelemente (Wert des Elements, beispielsweise eingetragener Text)\n- Optionen in Ausklapplisten (Eintrag x von y)\n\nDer aktuelle Wert eines entsprechenden Elements wird bei Fokussierung des Elements mit aktivem Screenreader proaktiv wiedergegeben.", false, "11.5.2.7 Werte" },
                    { 99, 1, "Formularfelder oder interaktive Elemente, die durch ihre Beschriftung im Kontext nicht direkt oder eindeutig identifizierbar sind und indirekt oder zusätzlich durch ein anderes Element gelabelt werden, oder ein Label für ein anderes Element sind, übermitteln diese verknüpfenden Kontextinformationen an die assistiven Systeme, um den logischen Bezug auch programmatisch ermittelbar nachvollziehen zu können.", false, "11.5.2.8 Label-Beziehungen" },
                    { 100, 1, "Beziehungen zwischen Elementen und ihnen übergeordneten oder untergeordneten Elementen, die zum logischen Verständnis des jeweiligen Elements notwendig sind, werden an die assistiven Systeme übermittelt und vom Screenreader wiedergegeben.", false, "11.5.2.9 Eltern-Kind-Beziehungen" },
                    { 101, 1, "Keine separate **Anforderung** in diesem Prüfschritt; Details siehe Unterprüfschritte 11.5.2.10a–11.5.2.10b.", false, "11.5.2.10 Text" },
                    { 102, 1, "Textinhalte werden analog zur visuellen Darstellung durch den Screenreader wiedergegeben. Längerer Text wird bei aktivem Screenreader sinnvoll und absatzweise in Blöcken fokussiert. Visuell hervorgehobene Textinhalte (beispielsweise fett, kursiv, unterstrichen) werden auch innerhalb eines Absatzes vom Screenreader separat fokussiert. Unnötige Sonderzeichen werden vom Screenreader ignoriert.\n\nAbkürzungen werden vom Screenreader ungekürzt ausgesprochen. Spezielle Formate, beispielsweise Informationen zu Datum oder Uhrzeit, werden im jeweiligen Format korrekt wiedergegeben.\n\nEs wird dringend empfohlen, entsprechend relevante Inhalte, beispielsweise eine IBAN, eine Postleitzahl oder eine Telefonnummer, auf eine Art zu implementieren, dass sie vom Screenreader zeichenweise buchstabiert ausgegeben werden. Dies ist technisch unter Umständen nicht in allen Umgebungen umsetzbar und kann somit nicht gefordert werden.", false, "11.5.2.10a Nicht bearbeitbarer Text" },
                    { 103, 1, "Textinhalte werden analog zur visuellen Darstellung durch den Screenreader wiedergegeben.\n\nTextformatierungen werden idealerweise vom Screenreader nach der Ausgabe des Inhalts des fokussierten Textabschnitts wiedergegeben, sofern diese vom Nutzenden geändert werden können (beispielsweise Fett, Kursiv, Schriftgröße, Schriftart).", false, "11.5.2.10b Bearbeitbarer Text" },
                    { 104, 1, "Interaktive Bedienelemente übermitteln eine Übersicht ihrer verfügbaren Aktionen, als auch die Art und Weise, wie die jeweilige Aktion bei aktivem Screenreader ausgeführt wird, korrekt an die assistiven Systeme. Dies betrifft beispielsweise:\n\n- Tasten (Zum Ausführen doppelklicken)\n- Umschalttasten (Zum Umschalten doppelklicken)\n- Kontrollkästchen (Zum Ändern doppelklicken)\n- Auswahlschalter (Zum Auswählen doppelklicken)\n- Registerkarten (Zum Auswählen doppelklicken)\n- Ausklapplisten (Zum Auswählen doppelklicken)\n- Formularfelder (Zum Bearbeiten doppelklicken, oder Zum Einfügen von Text doppelklicken)\n- Links (Zum Öffnen doppelklicken)", false, "11.5.2.11 Übersicht verfügbarer Aktionen" },
                    { 105, 1, "Interaktive Bedienelemente sind über die assistiven Systeme bedienbar, unabhängig davon, ob verfügbare Aktionen ausgegeben werden oder nicht. Dies betrifft beispielsweise:\n\n- Tasten (Zum Ausführen doppelklicken)\n- Umschalttasten (Zum Umschalten doppelklicken)\n- Kontrollkästchen (Zum Ändern doppelklicken)\n- Auswahlschalter (Zum Auswählen doppelklicken)\n- Registerkarten (Zum Auswählen doppelklicken)\n- Ausklapplisten (Zum Auswählen doppelklicken)\n- Optionen von Ausklapplisten (Ändern der Optionen mittels Pfeiltasten)\n- Formularfelder (Zum Bearbeiten doppelklicken, Einfügen von Text mittels Tastatur)\n- Links (Zum Öffnen doppelklicken)", false, "11.5.2.12 Ausführung verfügbarer Aktionen" },
                    { 106, 1, "Bei der Eingabe von Text in Texteingabefeldern sind der aktuelle Fokus und die aktuelle Cursorposition während der Bearbeitung mit aktivem Screenreader jederzeit im sichtbaren Bereich des Texteingabefelds. Der Anzeigebereich bei zusätzlicher, aktiver Bildschirmlupe folgt jederzeit der aktuellen Cursorposition. Textmarkierungen und Anfang und Ende des Texteingabefelds sind identifizierbar.", false, "11.5.2.13 Nachverfolgung des Fokus und der Auswahlattribute" },
                    { 107, 1, "Während der Eingabe von Text in Formularfeldern bei aktivem Screenreader und in Kombination mit aktiver Sprachsteuerung oder Tastatur sind der aktuelle Fokus und die aktuelle Cursorposition jederzeit sichtbar, sämtliche Bewegungsoptionen des Cursors (beispielsweise die wort- und zeichenweise Navigation) werden unterstützt. Textmarkierungen und Änderungen der Markierungen sind möglich, und markierte Stellen sind identifizierbar.", false, "11.5.2.14 Änderung des Fokus und der Auswahlattribute" },
                    { 108, 1, "Interaktive Bedienelemente, die Werte aufweisen oder Zustände einnehmen, und deren Wert oder Zustand durch Interaktion verändert werden kann, übermitteln den aktuellen Wert oder Zustand nach Änderung, gegebenenfalls die veränderte Position oder die veränderten Grenzen korrekt an die assistiven Systeme. Dies betrifft beispielsweise:\n\n- Umschalttasten (aktiviert, nicht aktiviert)\n- Kontrollkästchen (ausgewählt, nicht ausgewählt)\n- Auswahlschalter (aktiviert, nicht aktiviert)\n- Registerkarten (ausgewählt, nicht ausgewählt)\n- Ausklapplisten (erweitert, nicht erweitert)\n- Optionen in Ausklapplisten (Eintrag x von y)", false, "11.5.2.15 Änderungsbenachrichtigung" },
                    { 109, 1, "Die Eigenschaften oder Zustände interaktiver Bedienelemente, die durch Interaktion verändert werden können, können auch bei Nutzung assistiver Systeme verändert werden. Dies betrifft beispielsweise:\n\n- Umschalttasten (aktiviert, nicht aktiviert)\n- Kontrollkästchen (ausgewählt, nicht ausgewählt)\n- Auswahlschalter (aktiviert, nicht aktiviert)\n- Registerkarten (ausgewählt, nicht ausgewählt)\n- Ausklapplisten (erweitert, nicht erweitert)\n- Optionen in Ausklapplisten (Eintrag x von y)", false, "11.5.2.16 Änderungen von Zuständen und Eigenschaften" },
                    { 110, 1, "Die Werte von Formularelementen, die durch Interaktion verändert werden können, können auch bei Nutzung assistiver Systeme verändert werden.", false, "11.5.2.17 Änderungen von Werten und Text" },
                    { 111, 1, "Die assistiven Systeme des Betriebssystems, im Schwerpunkt die Systeme Screenreader, Bildschirmlupe, Sprachsteuerung und externe Tastatur, werden bei aktiver Nutzung nicht unterbrochen, verändert und frieren nicht ein, oder werden unerwartet beendet, ohne dass der Nutzende eine entsprechende Aktion ausführt oder im Vorfeld informiert wurde.", false, "11.6.2 Keine Unterbrechung der Barrierefreiheitsfunktionen" },
                    { 112, 1, "Über das Betriebssystem konfigurierte, individuelle Benutzereinstellungen werden durch die Software übernommen. Das beinhaltet obligatorisch:\n\n- Maßeinheiten (beispielsweise Zeit, Datum, Gewicht, Größe, Distanz, Temperatur)\n- Farbeinstellungen\n- Kontrasteinstellungen\n- Schriftarten\n- Farbe des Fokusrahmens\n\nAusnahme: Es handelt sich um Software, die dafür vorgesehen ist, von ihren zugrundeliegenden Plattformen isoliert zu sein und daher keinen Zugriff auf die Benutzervorgaben auf der Plattform besteht.\n\nDie genannten Benutzerpräferenzen sind unter Umständen nicht vollumfänglich. Je nach Betriebssystem und Art der Software stehen unter Umständen weniger, weitere oder andere Präferenzen zur Auswahl.", false, "11.7. Individuelle Benutzerpräferenzen" },
                    { 113, 1, "Der Prüfschritt wird über die Prüfschritte 11.8.2 Erstellung barrierefreier Inhalte , 11.8.3 Erhaltung von Barrierefreiheit bei Umwandlungen , 11.8.4 Reparaturunterstützung und 11.8.5 Vorlagen abgedeckt. Eine eigene Prüfung dieses Prüfschritts ist nicht notwendig. Sofern einer der genannten Prüfschritte als nicht voll erfüllt gewertet wird, wird auch dieser Prüfschritt entsprechend gewertet.", false, "11.8.1 Inhaltstechnologie" },
                    { 114, 1, "Autorenwerkzeuge müssen die Erstellung von barrierefreien Inhalten ermöglichen und die Autoren bei der Erstellung entsprechend anleiten. Der neu generierte Inhalt muss dabei innerhalb der Software ausgegeben werden. Autorenwerkzeuge dürfen dabei zusätzliche Werkzeuge einbinden, wenn die Konformität bestimmter Anforderungen von einem einzelnen Werkzeug nicht erreicht werden kann.\n\nEin Export in andere Dateiformate ist nicht Bestandteil dieser Prüfung.", false, "11.8.2 Erstellung barrierefreier Inhalte" },
                    { 115, 1, "Im Ausgangsformat vorhandene Unterstützungen und Informationen zur Barrierefreiheit bleiben bei Neustrukturierung, Export oder Konvertierung des Inhalts erhalten und gehen nicht verloren oder werden fehlerhaft verändert.\n\nEine vollumfängliche Prüfung von neu generierten Dateiformaten ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.", false, "11.8.3 Erhaltung von Barrierefreiheit bei Umwandlungen" },
                    { 116, 1, "Sofern eine Unterstützung bei der Erstellung barrierefreier Inhalte implementiert ist, müssen die Hinweise oder Fehlermeldungen eindeutig sein und Korrekturvorschläge zur Fehlerbehebung benannt werden.", false, "11.8.4 Reparaturunterstützung" },
                    { 117, 1, "Werden Vorlagen zur Erstellung von Inhalten angeboten, so stellt mindestens eine der Vorlagen die Erstellung von barrierefreien Inhalten sicher und ist als solche eindeutig erkennbar und gekennzeichnet.", false, "11.8.5 Vorlagen" },
                    { 118, 7, "Die Verwendung spezieller Barrierefreiheitsfunktionen wird in der Produktdokumentation, in der Erklärung zur Barrierefreiheit oder in beiden Dokumentationen erklärt.", false, "12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität" },
                    { 119, 7, "Sofern eine Dokumentation vorhanden ist, muss sie in mindestens einem der folgenden Formate verfügbar sein. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:\n\n- Eigener Bestandteil der Software, konform mit Kapitel 11 der EN 301549.\n- Webformat, konform mit Kapitel 9 der EN 301549.\n- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.\n\nWeitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.\n\nEine Funktionalität, die eine automatische Medienkonvertierung unterstützt, ist vorteilhaft für die Verbesserung der Zugänglichkeit.\n\nEine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Software im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.\n\nIst die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ( https://pac.pdf-accessibility.org/de ).", false, "12.1.2 Barrierefreie Dokumentation" },
                    { 120, 7, "Die durch den Produktsupport übermittelten Informationen zur Software entsprechen mindestens dem Umfang an Informationen, die in der Dokumentation wie unter Prüfschritt 12.1.1 Dokumentation von Barrierefreiheit und Kompatibilität beschrieben ermittelt werden können.\n\nAls Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).", false, "12.2.2 Informationen zu Barrierefreiheits- und Kompatibilitätsfunktionen" },
                    { 121, 7, "Die Erreichbarkeit des Produktsupports soll den Bedürfnissen von Menschen mit Beeinträchtigungen gerecht werden, indem mindestens zwei Supportkanäle angeboten werden, die unterschiedliche Sinne ansprechen. Dabei ist es irrelevant, ob der Produktsupport durch den Hersteller selbst, oder durch einen Dienstleister erbracht wird.\n\nDie Anforderung ist bei jedem Niveau der Unterstützung verschiedener Kommunikationsarten erfüllt. Anbieter können angeben, in welchem Umfang die Kommunikationsarten unterstützt werden, um eine aussagekräftigere Bewertung der Angemessenheit und Qualität der Unterstützung zu ermöglichen.\n\nAls Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).", false, "12.2.3 Effektive Kommunikation" },
                    { 122, 7, "Die durch den Produktsupport übermittelte Dokumentation zur Software muss in mindestens einem der folgenden Formate übermittelt werden. Weiterhin muss sie den Anforderungen der jeweils relevanten Kapitel der EN 301549 entsprechen:\n\n- Webformat, konform mit Kapitel 9 der EN 301549.\n- Nicht-Web-Format (als Datei), konform mit Kapitel 10 der EN 301 549.\n\nAls Produktsupport zählen beispielsweise eine Service-Hotline, ein Helpdesk oder technischer Support. Es ist irrelevant, über welche Kanäle der Produktsupport dabei erreicht werden kann. Einfache Kontaktdaten ohne Kennzeichnung als Produktsupport oder Service-Angebot gelten nicht als Produktsupport (beispielsweise E-Mailadressen im Impressum oder in der Datenschutzerklärung).\n\nWeitere, abweichende Formate für die Dokumentation, auch in nicht barrierefreien Formaten, können zusätzlich bereitgestellt werden.\n\nEine vollumfängliche Prüfung von Web-Formaten (geöffnet außerhalb der Software im Browser des Endgeräts) und von Nicht-Web-Formaten (bereitgestellt als Datei) ist im Rahmen dieses Testvorgehens nicht vorgesehen. Sie benötigen gegebenenfalls eine eigene Prüfung.\n\nIst die Dokumentation im Dateiformat PDF verfügbar, kann mit Hilfe der Anwendung PAC eine erste Analyse zur Barrierefreiheit des Dokuments durchgeführt werden ( https://pac.pdf-accessibility.org/de ).", false, "12.2.4 Barrierefreie Dokumentation durch den Produktsupport" },
                    { 123, 23, "Alle Kriterien des BIT inklusiv Software-Tests sind unter dem folgenden Link zu finden:\n\n [https://www.softwaretest.bit-inklusiv.de/](https://www.softwaretest.bit-inklusiv.de/)", false, "BIT inklusiv Software-Test" },
                    { 124, 24, "Alle Kriterien des BIT inklusiv App-Tests sind unter dem folgenden Link zu finden:\n\n [https://www.apptest.bit-inklusiv.de/](https://www.apptest.bit-inklusiv.de/)", false, "BIT inklusiv App-Test" },
                    { 125, 25, "Alle Kriterien des BIT inklusiv PDF-Tests sind unter dem folgenden Link zu finden:\n\n [https://www.pdftest.bit-inklusiv.de/](https://www.pdftest.bit-inklusiv.de/)", false, "BIT inklusiv PDF-Test" },
                    { 126, 26, "Alle Kriterien des BIK BITV-Tests (Web) sind unter dem folgenden Link zu finden:\n\n [https://www.bitvtest.de/pruefverfahren/bitv-20-web](https://www.bitvtest.de/pruefverfahren/bitv-20-web)", false, "BIK BITV-Test (Web)" },
                    { 127, 27, "Alle Kriterien des BIK BITV-Tests (App) sind unter dem folgenden Link zu finden:\n\n [https://www.bitvtest.de/pruefverfahren/bitv-20-app](https://www.bitvtest.de/pruefverfahren/bitv-20-app)", false, "BIK BITV-Test (App)" }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                columns: new[] { "Id", "CriteriaFilterId", "ExclusionId" },
                values: new object[,]
                {
                    { 1, 17, 9 },
                    { 2, 17, 12 },
                    { 3, 17, 13 },
                    { 4, 17, 14 },
                    { 5, 17, 15 },
                    { 6, 17, 25 },
                    { 7, 8, 24 },
                    { 8, 8, 25 },
                    { 9, 8, 27 }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "ProductCriteriaFilters",
                columns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 1, 2, 2 },
                    { 1, 3, 3 },
                    { 1, 4, 4 },
                    { 1, 5, 5 },
                    { 1, 6, 6 },
                    { 1, 7, 7 },
                    { 1, 8, 8 },
                    { 1, 9, 9 },
                    { 1, 10, 10 },
                    { 1, 11, 11 },
                    { 1, 12, 12 },
                    { 1, 13, 13 },
                    { 1, 14, 14 },
                    { 1, 15, 15 },
                    { 1, 16, 16 },
                    { 1, 17, 17 },
                    { 2, 18, 1 },
                    { 3, 19, 1 },
                    { 2, 20, 6 },
                    { 3, 21, 6 },
                    { 2, 22, 9 },
                    { 3, 23, 9 },
                    { 2, 24, 10 },
                    { 3, 25, 10 },
                    { 2, 26, 11 },
                    { 3, 27, 11 },
                    { 2, 28, 14 },
                    { 3, 29, 14 },
                    { 2, 30, 15 },
                    { 3, 31, 15 },
                    { 4, 32, 5 },
                    { 5, 33, 5 },
                    { 6, 34, 5 },
                    { 4, 35, 12 },
                    { 5, 36, 12 },
                    { 6, 37, 12 },
                    { 7, 38, 13 },
                    { 23, 39, 1 },
                    { 24, 40, 1 },
                    { 24, 41, 2 },
                    { 23, 42, 3 },
                    { 24, 43, 3 },
                    { 25, 44, 3 },
                    { 26, 45, 4 },
                    { 23, 46, 5 },
                    { 25, 47, 7 },
                    { 26, 48, 7 },
                    { 24, 49, 10 },
                    { 26, 50, 10 },
                    { 26, 51, 11 },
                    { 25, 52, 12 },
                    { 23, 53, 14 },
                    { 24, 54, 15 },
                    { 23, 55, 17 },
                    { 26, 56, 17 },
                    { 8, 57, 1 },
                    { 14, 58, 2 },
                    { 15, 59, 3 },
                    { 11, 60, 4 },
                    { 12, 61, 5 },
                    { 8, 62, 6 },
                    { 9, 63, 7 },
                    { 15, 64, 8 },
                    { 8, 65, 9 },
                    { 8, 66, 10 },
                    { 8, 67, 11 },
                    { 13, 68, 12 },
                    { 10, 69, 13 },
                    { 8, 70, 14 },
                    { 8, 71, 15 },
                    { 14, 72, 16 },
                    { 9, 73, 17 },
                    { 17, 74, 13 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilterExclusions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 1, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 2, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 3, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 4, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 5, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 6, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 7, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 8, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 9, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 10, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 11, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 12, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 13, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 14, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 15, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 16, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 1, 17, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 18, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 19, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 20, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 21, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 22, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 23, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 24, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 25, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 26, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 27, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 28, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 29, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 2, 30, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 3, 31, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 4, 32, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 5, 33, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 6, 34, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 4, 35, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 5, 36, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 6, 37, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 7, 38, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 23, 39, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 24, 40, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 24, 41, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 23, 42, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 24, 43, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 25, 44, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 26, 45, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 23, 46, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 25, 47, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 26, 48, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 24, 49, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 26, 50, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 26, 51, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 25, 52, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 23, 53, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 24, 54, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 23, 55, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 26, 56, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 57, 1 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 14, 58, 2 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 15, 59, 3 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 11, 60, 4 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 12, 61, 5 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 62, 6 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 9, 63, 7 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 15, 64, 8 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 65, 9 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 66, 10 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 67, 11 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 13, 68, 12 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 10, 69, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 70, 14 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 8, 71, 15 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 14, 72, 16 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 9, 73, 17 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "ProductCriteriaFilters",
                keyColumns: new[] { "CriteriaFilterId", "Id", "ProductId" },
                keyValues: new object[] { 17, 74, 13 });

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "CriteriaFilters",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "FilterTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "FilterTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "FilterTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "FilterTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
