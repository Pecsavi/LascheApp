using LascheApp.Materials;
using LascheApp.Shackles;
using LascheApp.Padeye;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace LascheApp
{
    public partial class Form1 : Form
    {
        private MaterialDatabase? _materialDatabase;
        private ShackleDatabase? _shackleDatabase;
        private PadeyeCheckResult? _lastPadeyeResult;
        private PinCheckResult? _lastPinResult;
        private readonly TextBox _txtProjectNumber = new();
        private readonly TextBox _txtVerificationSubject = new();
        private readonly ComboBox _cmbLanguage = new();
        private readonly GroupBox _projectGroup = new();
        private readonly Label _projectLabel = new();
        private readonly Label _subjectLabel = new();
        private readonly Button _settingsButton = new();
        private readonly ContextMenuStrip _settingsMenu = new();
        private readonly ToolStripMenuItem _languageMenuItem = new();
        private readonly ToolStripMenuItem _englishMenuItem = new("English");
        private readonly ToolStripMenuItem _germanMenuItem = new("Deutsch");
        private readonly ToolStripMenuItem _databaseMenuItem = new();
        private readonly ToolStripMenuItem _materialDatabaseMenuItem = new();
        private readonly ToolStripMenuItem _shackleDatabaseMenuItem = new();
        private readonly TabPage _guideTab = new();
        private readonly Panel _geometryGuide = new();
        private readonly PrintDocument _reportPrintDocument = new();
        private string[] _printLines = Array.Empty<string>();
        private int _nextPrintLine;
        private Bitmap? _printGeometryImage;
        private bool _printGeometryInserted;
        private string _lastEnglishReport = "";
        private bool _updatingRchLimit;
        public Form1()
        {
            InitializeComponent();
            UseProjectGuideResources();
            ConfigureProjectAndReportUi();
        }

        private void UseProjectGuideResources()
        {
            // Use the named project resources instead of the Designer-local
            // Form1.resx copies, so replacing a guide image remains stable.
            pictureBox2.Image = Properties.Resources.Lasche_Schackle;
            pictureBox1.Image = Properties.Resources.Cheek_plate;
            pictureBox3.Image = Properties.Resources.Shackle___Tension_Lug;
        }

        private void ConfigureProjectAndReportUi()
        {
            const int offsetY = 84;
            foreach (Control control in Controls.Cast<Control>().ToArray())
                control.Top += offsetY;

            ClientSize = new Size(ClientSize.Width, ClientSize.Height + offsetY);
            MinimumSize = new Size(1092, 890);
            Text = "Lug verification";

            _projectGroup.Text = "Project information";
            _projectGroup.Location = new Point(23, 12);
            _projectGroup.Size = new Size(ClientSize.Width - _projectGroup.Left - 15, 72);
            _projectGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _projectLabel.Text = "Project number";
            _projectLabel.AutoSize = true;
            _projectLabel.Location = new Point(14, 31);
            _txtProjectNumber.Location = new Point(112, 27);
            _txtProjectNumber.Size = new Size(180, 23);
            _txtProjectNumber.PlaceholderText = "e.g. S-1099";
            _subjectLabel.Text = "Subject of verification";
            _subjectLabel.AutoSize = true;
            _subjectLabel.Location = new Point(322, 31);
            _txtVerificationSubject.Location = new Point(458, 27);
            _txtVerificationSubject.Size = new Size(330, 23);
            _txtVerificationSubject.PlaceholderText = "e.g. Gantry 1 – Gantry 2 connection";
            _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLanguage.Items.AddRange(new object[] { "English", "Deutsch" });
            _cmbLanguage.SelectedIndexChanged += Language_SelectedIndexChanged;
            _settingsButton.Text = "⚙ Settings";
            _settingsButton.Size = new Size(132, 29);
            _settingsButton.Location = new Point(_projectGroup.ClientSize.Width - 147, 25);
            _settingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _settingsButton.Click += SettingsButton_Click;
            ConfigureSettingsMenu();
            _projectGroup.Controls.AddRange(new Control[]
            {
                _projectLabel, _txtProjectNumber, _subjectLabel, _txtVerificationSubject,
                _settingsButton
            });
            Controls.Add(_projectGroup);
            _projectGroup.BringToFront();

            txtBasicCheckResult.Dock = DockStyle.Fill;

            _guideTab.Text = "Geometry guide";
            _guideTab.Padding = new Padding(8);
            Label guideHint = new()
            {
                Text = "Schematic only – dimensions are defined by the input fields.",
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DimGray
            };
            _geometryGuide.Dock = DockStyle.Fill;
            _geometryGuide.BackColor = Color.White;
            _geometryGuide.Paint += GeometryGuide_Paint;
            _guideTab.Controls.Add(_geometryGuide);
            _guideTab.Controls.Add(guideHint);
            tabResults.TabPages.Insert(1, _guideTab);
            cmbLugType.SelectedIndexChanged += (_, _) => _geometryGuide.Invalidate();
            foreach (TextBox geometryInput in new[]
            {
                txtPlateThickness_mm, txtCheekPlateThickness_mm, txtHoleDiameter_mm,
                txtPlateWidth_mm, txtEdgeDistanceA_mm, txtTensionPinDiameter_mm, txtRch_mm
            })
            {
                geometryInput.TextChanged += (_, _) => _geometryGuide.Invalidate();
            }
            txtRch_mm.TextChanged += (_, _) => ApplyRchLimit();
            txtEdgeDistanceA_mm.TextChanged += (_, _) => ApplyRchLimit();
            cmbShackles.SelectedIndexChanged += (_, _) => _geometryGuide.Invalidate();

            _reportPrintDocument.DocumentName = "Lug verification report";
            _reportPrintDocument.BeginPrint += (_, _) =>
            {
                _printLines = txtBasicCheckResult.Lines
                    .Select(PrepareLineForPrint)
                    .ToArray();
                _nextPrintLine = 0;
                _printGeometryInserted = false;
                _printGeometryImage?.Dispose();
                _printGeometryImage = CreateGeometryGuidePrintImage();
            };
            _reportPrintDocument.PrintPage += ReportPrintDocument_PrintPage;
            _reportPrintDocument.EndPrint += (_, _) =>
            {
                _printGeometryImage?.Dispose();
                _printGeometryImage = null;
            };
            _cmbLanguage.SelectedIndex = LoadLanguagePreference();
        }

        private void PrintReport_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBasicCheckResult.Text))
            {
                MessageBox.Show("Run the verification before printing.", "No report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using PrintPreviewDialog preview = new()
            {
                Document = _reportPrintDocument,
                Width = 1100,
                Height = 800,
                KeyPreview = true
            };
            ConfigurePreviewNavigation(preview);
            preview.ShowDialog(this);
        }

        private static string PrepareLineForPrint(string line)
        {
            // The report uses one leading tab for its screen hierarchy. On
            // paper that first level is unnecessary and wastes page width.
            if (line.StartsWith('\t'))
                line = line[1..];

            string[] checklistStatuses = { "OK\t", "WARNING\t", "NOT OK\t", "WARNUNG\t", "NICHT ERFÜLLT\t" };
            foreach (string status in checklistStatuses)
            {
                if (line.StartsWith(status, StringComparison.OrdinalIgnoreCase))
                {
                    int separatorIndex = status.Length - 1;
                    line = line[..separatorIndex] + '\u001f' + line[(separatorIndex + 1)..];
                    break;
                }
            }

            return line.Replace("\t", "    ");
        }

        private static void ConfigurePreviewNavigation(PrintPreviewDialog dialog)
        {
            PrintPreviewControl? previewControl = FindPreviewControl(dialog);
            if (previewControl == null)
                return;

            previewControl.UseAntiAlias = true;
            previewControl.TabStop = true;

            dialog.KeyDown += (_, e) =>
            {
                if (e.KeyCode is Keys.Down or Keys.PageDown or Keys.Right)
                {
                    previewControl.StartPage++;
                    e.Handled = true;
                }
                else if (e.KeyCode is Keys.Up or Keys.PageUp or Keys.Left)
                {
                    previewControl.StartPage = Math.Max(0, previewControl.StartPage - 1);
                    e.Handled = true;
                }
            };

            previewControl.MouseWheel += (_, e) =>
            {
                previewControl.StartPage = e.Delta < 0
                    ? previewControl.StartPage + 1
                    : Math.Max(0, previewControl.StartPage - 1);
            };
        }

        private static PrintPreviewControl? FindPreviewControl(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is PrintPreviewControl previewControl)
                    return previewControl;

                PrintPreviewControl? nested = FindPreviewControl(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private bool IsGerman => _cmbLanguage.SelectedIndex == 1;

        private void ConfigureSettingsMenu()
        {
            _englishMenuItem.Click += (_, _) => _cmbLanguage.SelectedIndex = 0;
            _germanMenuItem.Click += (_, _) => _cmbLanguage.SelectedIndex = 1;
            _materialDatabaseMenuItem.Click += (_, _) => OpenDatabaseSettings(0);
            _shackleDatabaseMenuItem.Click += (_, _) => OpenDatabaseSettings(1);

            _languageMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                _englishMenuItem,
                _germanMenuItem
            });
            _databaseMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                _materialDatabaseMenuItem,
                _shackleDatabaseMenuItem
            });
            _settingsMenu.Items.AddRange(new ToolStripItem[]
            {
                _languageMenuItem,
                _databaseMenuItem
            });
        }

        private void Language_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SaveLanguagePreference();
            ApplyLanguage();

            // Recreate both Summary and Report in the selected language.
            if (_lastPadeyeResult != null)
                btnCheckBasicPadeye.PerformClick();
        }

        private void ApplyLanguage()
        {
            bool de = IsGerman;
            Text = de ? "Laschennachweis" : "Lug verification";
            _projectGroup.Text = de ? "Projektangaben" : "Project information";
            _projectLabel.Text = de ? "Projektnummer" : "Project number";
            _subjectLabel.Text = de ? "Gegenstand der Prüfung" : "Subject of verification";
            _settingsButton.Text = de ? "⚙ Einstellungen" : "⚙ Settings";
            _languageMenuItem.Text = de ? "Sprache" : "Language";
            _databaseMenuItem.Text = de ? "Datenbank" : "Database";
            _materialDatabaseMenuItem.Text = de ? "Werkstoffe" : "Material";
            _shackleDatabaseMenuItem.Text = de ? "Schäkel" : "Shackle";
            _englishMenuItem.Checked = !de;
            _germanMenuItem.Checked = de;
            button1.Text = de ? "Bericht drucken" : "Print report";

            grpLoads.Text = de ? "Lasten / Werkstoff" : "Loads / material";
            grpLugGeometry.Text = de ? "Laschengeometrie" : "Lug geometry";
            grpTransportLug.Text = de ? "Transportlasche / Schäkel / DNV" : "Transport Lug / Shackle / DNV";
            grpTensionLug.Text = de ? "Zuglasche / Bolzen" : "Tension Lug / Pin";
            groupBox5.Text = de ? "Ergebnis" : "Result";
            btnCheckBasicPadeye.Text = de ? "Lasche prüfen" : "Verify lug";
            btnSelectShackleByLoad.Text = de ? "Schäkel nach Last wählen" : "Select shackle by load";
            btnPredesign.Text = de ? "Vorbemessung" : "Predesign";
            toolTip1.SetToolTip(
                btnLoadGuidance,
                de ? "Empfehlung zur Lasteingabe" : "Load input recommendation");
            chkReplaceablePin.Text = de ? "Nachweise: austauschbarer Bolzen" : "Replaceable pin checks";
            chkIncludeCheekPlatesInBearing.Text = de
                ? "Verstärkungsbleche im Tragwiderstand.*"
                : "Cheek plates considered in the calculation.*";
            richTextBox1.Text = de
                ? "*Endgültiges Bohren nach dem Schweißen wird vorausgesetzt!"
                : "*Final drilling after welding is assumed!";
            richTextBox1.Height = de ? 44 : 25;

            label8.Text = de ? "Laschentyp" : "Lug type";
            label6.Text = de ? "Laschenwerkstoff" : "Lug material";
            label10.Text = de ? "Bolzenwerkstoff" : "Pin material";
            label9.Text = de ? "Bolzendurchmesser d [mm]" : "Pin diameter d [mm]";
            lblOuterLugThicknessT2.Text = de ? "Außenlasche t2 [mm]" : "Outer lug t2 [mm]";
            lblGapS.Text = de ? "Spalt s [mm]" : "Gap s [mm]";

            // German engineering terms are considerably longer. Keep a
            // dedicated German layout so labels never run into input fields.
            cmbMaterials.Left = de ? 135 : 100;
            cmbMaterials.Width = de ? 111 : 111;
            cmbPinMaterials.Left = de ? 135 : 105;
            cmbPinMaterials.Width = de ? 115 : 101;

            txtTensionPinDiameter_mm.Left = de ? 190 : 141;
            txtOuterLugThicknessT2_mm.Left = de ? 190 : 141;
            txtGapS_mm.Left = de ? 190 : 141;

            lblPinFy.Left = de ? 280 : 254;
            lblPinFu.Left = de ? 375 : 328;

            btnSelectShackleByLoad.Width = de ? 210 : 177;

            chkReplaceablePin.Left = de ? 265 : 311;
            chkReplaceablePin.AutoSize = true;
            chkIncludeCheekPlatesInBearing.Left = de ? 190 : 203;
            chkIncludeCheekPlatesInBearing.AutoSize = true;

            _subjectLabel.Left = 322;
            _txtVerificationSubject.Left = de ? 475 : 458;
            _txtVerificationSubject.Width = de ? 313 : 330;

            tabSummary.Text = de ? "Übersicht" : "Summary";
            _guideTab.Text = de ? "Sichtprüfung" : "Visual check";
            tabReport.Text = de ? "Bericht" : "Report";

            if (cmbLugType.DataSource != null)
            {
                LugType selectedType = GetSelectedLugType();
                cmbLugType.DataSource = new List<KeyValuePair<LugType, string>>
                {
                    new(LugType.TransportLug, de ? "Transportlasche" : "Transport Lug"),
                    new(LugType.TensionLug, de ? "Zuglasche" : "Tension Lug")
                };
                cmbLugType.DisplayMember = "Value";
                cmbLugType.ValueMember = "Key";
                cmbLugType.SelectedValue = selectedType;
            }

            if (dgvCheckSummary.Columns.Count >= 4)
            {
                dgvCheckSummary.Columns["Group"].HeaderText = de ? "Gruppe" : "Group";
                dgvCheckSummary.Columns["Check"].HeaderText = de ? "Nachweis" : "Check";
                dgvCheckSummary.Columns["Status"].HeaderText = de ? "Status" : "Status";
            }
        }

        private string LocalizeReport(string englishReport)
        {
            _lastEnglishReport = englishReport;
            return IsGerman ? TranslateToGerman(englishReport) : englishReport;
        }

        private string LocalizeUiText(string englishText)
        {
            if (!IsGerman)
                return englishText;

            return englishText switch
            {
                "Geometry" => "Geometrie",
                "Shackle" => "Schäkel",
                "Bearing" => "Lochleibung",
                "Lug t2" => "Außenlaschen t2",
                "Cheek plates" => "Verstärkungsbleche",
                "Pin" => "Bolzen",
                "General" => "Allgemein",
                _ => TranslateToGerman(englishText)
            };
        }

        private static string TranslateToGerman(string text)
        {
            (string English, string German)[] replacements =
            {
                ("Transport Lug verification", "Nachweis der Transportlasche"),
                ("Tension Lug verification", "Nachweis der Zuglasche"),
                ("Project information", "Projektangaben"),
                ("Project:", "Projekt:"), ("Subject:", "Prüfgegenstand:"),
                ("Prepared by:", "Erstellt von:"), ("Date:", "Datum:"),
                ("Overall result:", "Gesamtergebnis:"),
                ("Max utilization:", "Maximale Ausnutzung:"),
                ("Governing utilization:", "Maßgebende Ausnutzung:"),
                ("Governing check:", "Maßgebender Nachweis:"),
                ("Failed check(s):", "Nicht erfüllte Nachweise:"),
                ("Pin-hole geometry not fulfilled: pin diameter d must be smaller than hole diameter d0", "Bolzen-Loch-Geometrie nicht erfüllt: Bolzendurchmesser d muss kleiner als Lochdurchmesser d0 sein"),
                ("Contact stress of the replaceable pin not fulfilled", "Kontaktspannungsnachweis des austauschbaren Bolzens nicht erfüllt"),
                ("DNV bearing pressure for angled pull not fulfilled", "DNV-Flächenpressungsnachweis bei Schrägzug nicht erfüllt"),
                ("Tear-out by angled pull according to DNV standard not fulfilled", "Ausreißnachweis bei Schrägzug nach DNV nicht erfüllt"),
                ("EC geometry check not fulfilled: neither Method A nor Method B is OK", "EC-Geometrienachweis nicht erfüllt: weder Methode A noch Methode B ist erfüllt"),
                ("Shackle WLL not sufficient", "Schäkel-WLL nicht ausreichend"),
                ("Cheek plate thickness limit not fulfilled", "Grenze der Verstärkungsblechdicke nicht erfüllt"),
                ("Cheek plates were requested, but cannot be included in the effective thickness.", "Die Berücksichtigung der Verstärkungsbleche wurde angefordert, ist für die wirksame Dicke jedoch nicht zulässig."),
                ("required e_min", "erforderlich e_min"),
                ("Therefore the B1 thickness recommendation is checked with t = tpl only.", "Daher wird die B1-Dickenempfehlung nur mit t = tpl geprüft."),
                ("Input data", "Eingabedaten"), ("Materials", "Werkstoffe"),
                ("Loads", "Lasten"), ("Geometry", "Geometrie"),
                ("Lug:", "Lasche:"), ("Pin:", "Bolzen:"), ("Shackle:", "Schäkel:"),
                ("requested for resistance; applicability is verified separately", "für den Tragwiderstand vorgesehen; Anwendbarkeit wird separat geprüft"),
                ("considered in geometry only, not in resistance", "nur geometrisch, nicht im Tragwiderstand berücksichtigt"),
                ("Replaceable pin checks: active", "Nachweise für austauschbaren Bolzen: aktiv"),
                ("Replaceable pin checks: not active", "Nachweise für austauschbaren Bolzen: nicht aktiv"),
                ("Check summary", "Nachweisübersicht"),
                ("Geometry checks", "Geometrische Nachweise"),
                ("Utilization checks", "Ausnutzungsnachweise"),
                ("Calculation", "Berechnung"), ("Lug result:", "Ergebnis Lasche:"),
                ("Pin result:", "Ergebnis Bolzen:"),
                ("Outer lug plates t2", "Außenlaschen t2"),
                ("Outer lug plate", "Außenlasche"),
                ("Lug t2 geometry", "Geometrie der Außenlaschen t2"),
                ("increase t2 to at least", "t2 erhöhen auf mindestens"),
                ("Geometry - Method B checked first", "Geometrie – Methode B zuerst geprüft"),
                ("Geometry - Method A alternative", "Geometrie – Alternative Methode A"),
                ("Selected geometry method:", "Gewählte Geometriemethode:"),
                ("Required minimum", "Erforderliches Minimum"),
                ("one outer plate", "eine Außenlasche"),
                ("Lug t2 check", "Nachweis der Außenlaschen t2"),
                ("Required t2 from bearing checks", "Erforderliches t2 aus den Lochleibungsnachweisen"),
                ("Required t2, rounded up to 10 mm", "Erforderliches t2, auf 10 mm aufgerundet"),
                ("Geometry sizing is not continued because a bearing check is", "Die Geometriemessung wird nicht fortgesetzt, da ein Lochleibungsnachweis"),
                ("Geometry - Method B thickness conditions", "Geometrie – Dickenbedingungen der Methode B"),
                ("Method B selected", "Methode B gewählt"),
                ("Method B is not fulfilled; Method A selected", "Methode B nicht erfüllt; Methode A gewählt"),
                ("Minimum distances from load", "Mindestabstände aus Belastung"),
                ("Required overall geometry", "Erforderliche Gesamtgeometrie"),
                ("Provided e", "Vorhanden e"), ("Provided b", "Vorhanden b"),
                ("Method A result:", "Ergebnis Methode A:"),
                ("Method B result:", "Ergebnis Methode B:"),
                ("Method A", "Methode A"), ("Method B", "Methode B"),
                ("Shackle WLL", "Schäkel-WLL"),
                ("Hole diameter clearance recommendation", "Empfehlung zum Lochspiel"),
                ("Hole clearance recommendation", "Empfehlung zum Lochspiel"),
                ("Shackle pin diameter", "Schäkelbolzendurchmesser"),
                ("Provided hole diameter", "Vorhandener Lochdurchmesser"),
                ("Clearance allowance", "Zulässiges Lochspiel"),
                ("Recommended range:", "Empfohlener Bereich:"),
                ("Dpin + clearance", "Dpin + Lochspiel"),
                ("Hole clearance", "Lochspiel"),
                ("EC geometry check", "EC-Geometrienachweis"),
                ("Pin diameter recommendation for significant angled pull", "Bolzendurchmesser-Empfehlung bei Schrägzug"),
                ("Shackle B1 thickness recommendation", "Empfehlung zur Laschendicke aus Schäkelmaß B1"),
                ("Pin-hole bearing design", "Lochleibungsnachweis"),
                ("Replaceable pin service bearing", "Gebrauchslochleibung des austauschbaren Bolzens"),
                ("Replaceable pin contact stress", "Kontaktspannung des austauschbaren Bolzens"),
                ("Bearing at angled pull (according to DNV standard)", "Flächenpressung bei Schrägzug nach DNV"),
                ("tear-out at angled pull (according to DNV standard)", "Ausreißen bei Schrägzug nach DNV"),
                ("Cheek plates are not considered for bearing resistance", "Verstärkungsbleche werden im Tragwiderstand nicht berücksichtigt"),
                ("Service bearing (Replaceable pin)", "Lochleibung im Gebrauchszustand (austauschbarer Bolzen)"),
                ("Contact stress (Replaceable pin)", "Kontaktspannung (austauschbarer Bolzen)"),
                ("Cheek plate weld", "Schweißnaht der Verstärkungsbleche"),
                ("Bearing pressure - only for angled pull", "Flächenpressung bei Schrägzug"),
                ("Tear out - only for angled pull", "Ausreißen bei Schrägzug"),
                ("Section values", "Querschnittswerte"),
                ("Pin shear + bending interaction", "Interaktion aus Bolzenschub und -biegung"),
                ("Replaceable pin service bending", "Bolzenbiegung im Gebrauchszustand"),
                ("Pin shear", "Bolzenschub"), ("Pin bending", "Bolzenbiegung"),
                ("Pin-hole geometry", "Bolzen-Loch-Geometrie"),
                ("Recommendation fulfilled:", "Empfehlung erfüllt:"),
                ("Recommended:", "Empfohlen:"),
                ("This is a recommendation only and does not govern the overall result.", "Dies ist nur eine Empfehlung und beeinflusst das Gesamtergebnis nicht."),
                ("according to DNV standard", "nach DNV"),
                ("not fulfilled", "nicht erfüllt"),
                ("not sufficient", "nicht ausreichend"),
                ("NOT CALCULATED", "NICHT BERECHNET"),
                ("WARNING", "WARNUNG"), ("NOT OK", "NICHT ERFÜLLT"),
                ("Check ", "Prüfung "), (" result:", " Ergebnis:")
            };

            foreach ((string english, string german) in replacements)
                text = text.Replace(english, german, StringComparison.OrdinalIgnoreCase);

            return text;
        }

        private void ReportPrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (e.Graphics is null)
                return;

            using Font font = new("Consolas", 9f);
            float lineHeight = font.GetHeight(e.Graphics);
            float y = e.MarginBounds.Top;
            float pageBottom = e.MarginBounds.Bottom;
            using StringFormat format = new(StringFormatFlags.LineLimit)
            {
                Trimming = StringTrimming.Word
            };

            while (_nextPrintLine < _printLines.Length)
            {
                string line = _printLines[_nextPrintLine];

                if (!_printGeometryInserted &&
                    _printGeometryImage != null &&
                    IsCalculationHeading(line))
                {
                    float imageScale = Math.Min(
                        (float)e.MarginBounds.Width / _printGeometryImage.Width,
                        (e.MarginBounds.Height / 3f) / _printGeometryImage.Height);
                    float imageWidth = _printGeometryImage.Width * imageScale;
                    float imageHeight = _printGeometryImage.Height * imageScale;

                    if (y + imageHeight > pageBottom && y > e.MarginBounds.Top)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    float imageX = e.MarginBounds.Left + (e.MarginBounds.Width - imageWidth) / 2f;
                    e.Graphics.DrawImage(
                        _printGeometryImage,
                        new RectangleF(imageX, y, imageWidth, imageHeight));
                    y += imageHeight + lineHeight;
                    _printGeometryInserted = true;
                }

                int checklistSeparator = line.IndexOf('\u001f');
                bool isChecklistLine = checklistSeparator >= 0;
                string status = isChecklistLine ? line[..checklistSeparator] : "";
                string printableText = isChecklistLine ? line[(checklistSeparator + 1)..] : line;
                float hangingIndent = isChecklistLine
                    ? e.Graphics.MeasureString(
                        IsGerman ? "NICHT ERFÜLLT  " : "WARNING  ",
                        font).Width
                    : 0f;
                int availableWidth = Math.Max(1, e.MarginBounds.Width - (int)Math.Ceiling(hangingIndent));
                float requiredHeight = string.IsNullOrEmpty(printableText)
                    ? lineHeight
                    : Math.Max(
                        lineHeight,
                        e.Graphics.MeasureString(
                            printableText,
                            font,
                            availableWidth,
                            format).Height);

                if (y + requiredHeight > pageBottom && y > e.MarginBounds.Top)
                    break;

                if (!string.IsNullOrEmpty(printableText))
                {
                    if (isChecklistLine)
                    {
                        e.Graphics.DrawString(
                            status,
                            font,
                            Brushes.Black,
                            e.MarginBounds.Left,
                            y);
                    }

                    RectangleF layout = new(
                        e.MarginBounds.Left + hangingIndent,
                        y,
                        availableWidth,
                        requiredHeight);
                    e.Graphics.DrawString(printableText, font, Brushes.Black, layout, format);
                }

                _nextPrintLine++;
                y += requiredHeight;
            }

            e.HasMorePages = _nextPrintLine < _printLines.Length;
        }

        private static string UserSettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LascheApp",
            "settings.json");

        private static int LoadLanguagePreference()
        {
            try
            {
                if (!File.Exists(UserSettingsPath))
                    return 1;

                string json = File.ReadAllText(UserSettingsPath);
                AppPreferences? preferences = System.Text.Json.JsonSerializer.Deserialize<AppPreferences>(json);
                return Math.Clamp(preferences?.LanguageIndex ?? 1, 0, 1);
            }
            catch
            {
                return 1;
            }
        }

        private void SaveLanguagePreference()
        {
            try
            {
                string? directory = Path.GetDirectoryName(UserSettingsPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                string json = System.Text.Json.JsonSerializer.Serialize(
                    new AppPreferences { LanguageIndex = Math.Clamp(_cmbLanguage.SelectedIndex, 0, 1) },
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UserSettingsPath, json);
            }
            catch
            {
                // A read-only profile must not prevent the application from running.
            }
        }

        private sealed class AppPreferences
        {
            public int LanguageIndex { get; set; }
        }

        private static bool IsCalculationHeading(string line)
        {
            string heading = line.TrimStart();
            return heading.StartsWith("2. Calculation", StringComparison.OrdinalIgnoreCase) ||
                   heading.StartsWith("2. Berechnung", StringComparison.OrdinalIgnoreCase);
        }

        private Bitmap CreateGeometryGuidePrintImage()
        {
            int width = Math.Max(320, _geometryGuide.ClientSize.Width);
            int height = Math.Max(360, _geometryGuide.ClientSize.Height);
            Bitmap image = new(width, height);

            using Graphics graphics = Graphics.FromImage(image);
            graphics.Clear(Color.White);
            try
            {
                DrawGeometryGuide(graphics, new Rectangle(0, 0, width, height));
            }
            catch (Exception)
            {
                graphics.Clear(Color.White);
            }

            return image;
        }

        private void GeometryGuide_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            try
            {
                DrawGeometryGuide(e.Graphics);
            }
            catch (Exception)
            {
                // Input fields are updated character by character. During editing the
                // temporary geometry can be impossible to scale or draw. Keep the UI
                // responsive and show an empty guide until the input is valid again.
                e.Graphics.Clear(Color.White);
            }
        }

        private void DrawGeometryGuide(Graphics g, Rectangle? targetArea = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle area = targetArea ?? _geometryGuide.ClientRectangle;
            if (area.Width < 320 || area.Height < 360)
                return;

            double Read(TextBox box, double fallback) =>
                TryReadDouble(box.Text, out double value) && value > 0.0 ? value : fallback;

            double tpl = Read(txtPlateThickness_mm, 60.0);
            double tch = Read(txtCheekPlateThickness_mm, 0.0);
            double d0 = Read(txtHoleDiameter_mm, 85.0);
            double b = Read(txtPlateWidth_mm, 220.0);
            double endDistance = Read(txtEdgeDistanceA_mm, 150.0);
            double rch = Read(txtRch_mm, d0 * 0.75);
            double alpha = TryReadDouble(txtDnvOutOfPlaneAngle_deg.Text, out double alphaValue)
                ? Math.Clamp(alphaValue, 0.0, 89.0)
                : 0.0;
            double pinDiameter = GetSelectedLugType() == LugType.TransportLug
                ? GetSelectedShackle()?.Dpin_mm ?? d0 * 0.95
                : Read(txtTensionPinDiameter_mm, d0 * 0.95);

            using Pen outline = new(Color.FromArgb(45, 75, 105), 2.5f);
            using Pen pinPen = new(Color.FromArgb(65, 65, 65), 2f);
            using Pen cheekPen = new(Color.FromArgb(40, 135, 185), 2f) { DashStyle = DashStyle.Dash };
            using Pen dimension = new(Color.FromArgb(185, 70, 55), 1.4f)
            {
                StartCap = LineCap.ArrowAnchor,
                EndCap = LineCap.ArrowAnchor
            };
            using Brush plateBrush = new SolidBrush(Color.FromArgb(225, 235, 244));
            using Brush cheekBrush = new SolidBrush(Color.FromArgb(80, 120, 195, 225));
            using Font labelFont = new("Segoe UI", 9f, FontStyle.Bold);
            using Font titleFont = new("Segoe UI", 11f, FontStyle.Bold);

            string topTitle = IsGerman ? "DRAUFSICHT" : "TOP VIEW";
            string sectionTitle = IsGerman ? "SCHNITT AM BOLZEN" : "SECTION AT PIN";
            g.DrawString(topTitle, titleFont, Brushes.Black, 14, 10);

            int shapeLeft = 58;
            int shapeRight = area.Width - 35;
            int shapeTop = 55;
            int shapeHeight = Math.Min(205, (int)(area.Height * 0.43));
            int shapeBottom = shapeTop + shapeHeight;
            int radius = shapeHeight / 2;
            int holeCy = shapeTop + radius;
            int holeCx = shapeRight - Math.Clamp((int)(endDistance / Math.Max(b, 1.0) * shapeHeight), radius / 2, radius);

            using GraphicsPath lugPath = new();
            lugPath.AddLine(shapeLeft, shapeTop, shapeRight - radius, shapeTop);
            lugPath.AddArc(shapeRight - 2 * radius, shapeTop, 2 * radius, shapeHeight, 270, 180);
            lugPath.AddLine(shapeRight - radius, shapeBottom, shapeLeft, shapeBottom);
            lugPath.CloseFigure();
            g.FillPath(plateBrush, lugPath);
            g.DrawPath(outline, lugPath);

            int holeRadius = Math.Clamp((int)(d0 / Math.Max(b, 1.0) * shapeHeight / 2.0), 18, radius - 14);
            int pinRadius = Math.Clamp((int)(holeRadius * pinDiameter / Math.Max(d0, 1.0)), 8, holeRadius + 8);

            if (tch > 0.0)
            {
                int cheekRadius = Math.Clamp((int)(rch / Math.Max(d0 / 2.0, 1.0) * holeRadius), holeRadius + 10, radius - 5);
                Rectangle cheekCircle = new(holeCx - cheekRadius, holeCy - cheekRadius, cheekRadius * 2, cheekRadius * 2);
                g.FillEllipse(cheekBrush, cheekCircle);
                g.DrawEllipse(cheekPen, cheekCircle);
                string rchText = $"Rch = {rch:0.0}";
                float rchTextWidth = g.MeasureString(rchText, labelFont).Width;
                float rchTextX = Math.Max(4f, holeCx - cheekRadius - rchTextWidth - 5f);
                g.DrawString(rchText, labelFont, Brushes.SteelBlue, rchTextX, holeCy - 12);
            }

            Rectangle hole = new(holeCx - holeRadius, holeCy - holeRadius, holeRadius * 2, holeRadius * 2);
            Rectangle pin = new(holeCx - pinRadius, holeCy - pinRadius, pinRadius * 2, pinRadius * 2);
            g.FillEllipse(Brushes.White, hole);
            g.DrawEllipse(outline, hole);
            g.FillEllipse(Brushes.LightGray, pin);
            g.DrawEllipse(pinPen, pin);

            DrawDimension(g, dimension, labelFont, shapeLeft - 18, shapeTop, shapeLeft - 18, shapeBottom, $"b={b:0.0}");
            DrawDimension(g, dimension, labelFont, holeCx, shapeBottom + 22, shapeRight, shapeBottom + 22, $"e={endDistance:0.0}");
            DrawDimension(g, dimension, labelFont, holeCx - holeRadius, holeCy - holeRadius - 12, holeCx + holeRadius, holeCy - holeRadius - 12, $"d0={d0:0.0}");

            int sectionTop = shapeBottom + 105;
            g.DrawString(sectionTitle, titleFont, Brushes.Black, 14, sectionTop);
            int sectionCy = Math.Min(sectionTop + 120, area.Height - 105);
            double largestUpperDimension = Math.Max(endDistance, Math.Max(rch, d0 / 2.0));
            double sectionVerticalScale = Math.Min(
                0.55,
                Math.Max(0.1, (sectionCy - sectionTop - 30.0) / Math.Max(largestUpperDimension, 1.0)));
            int plateHeightAbovePin = Math.Max(20, (int)Math.Round(endDistance * sectionVerticalScale));
            int plateHeightBelowPin = Math.Min(125, Math.Max(55, area.Height - sectionCy - 38));
            int tplWidth = Math.Clamp((int)(tpl * 1.15), 45, 105);
            int tchWidth = tch > 0.0 ? Math.Clamp((int)(tch * 1.15), 10, 38) : 0;
            int sectionCx = area.Width / 2;

            Rectangle mainPlate = new(
                sectionCx - tplWidth / 2,
                sectionCy - plateHeightAbovePin,
                tplWidth,
                plateHeightAbovePin + plateHeightBelowPin);
            g.FillRectangle(plateBrush, mainPlate);
            g.DrawRectangle(outline, mainPlate);

            if (tch > 0.0)
            {
                int cheekHeight = Math.Max(20, (int)Math.Round(2.0 * rch * sectionVerticalScale));
                Rectangle leftCheek = new(mainPlate.Left - tchWidth, sectionCy - cheekHeight / 2, tchWidth, cheekHeight);
                Rectangle rightCheek = new(mainPlate.Right, sectionCy - cheekHeight / 2, tchWidth, cheekHeight);
                g.FillRectangle(cheekBrush, leftCheek);
                g.FillRectangle(cheekBrush, rightCheek);
                g.DrawRectangle(cheekPen, leftCheek);
                g.DrawRectangle(cheekPen, rightCheek);
                DrawDimension(g, dimension, labelFont, rightCheek.Left, rightCheek.Bottom + 16, rightCheek.Right, rightCheek.Bottom + 16, $"tch={tch:0.0}");
            }

            int totalThicknessWidth = tplWidth + 2 * tchWidth;
            int sectionHoleHeight = Math.Max(16, (int)Math.Round(d0 * sectionVerticalScale));
            Rectangle sectionHole = new(
                sectionCx - totalThicknessWidth / 2,
                sectionCy - sectionHoleHeight / 2,
                totalThicknessWidth,
                sectionHoleHeight);
            g.FillRectangle(Brushes.White, sectionHole);
            g.DrawRectangle(outline, sectionHole);

            using Pen sectionPin = new(Color.DimGray, Math.Max(5f, (float)(pinDiameter / Math.Max(d0, 1.0) * 12f)));
            g.DrawLine(sectionPin, sectionCx - tplWidth / 2 - tchWidth - 55, sectionCy, sectionCx + tplWidth / 2 + tchWidth + 55, sectionCy);
            DrawDimension(g, dimension, labelFont, mainPlate.Left, mainPlate.Bottom + 12, mainPlate.Right, mainPlate.Bottom + 12, $"tpl={tpl:0.0}");

            if (alpha > 0.0 && GetSelectedLugType() == LugType.TransportLug)
            {
                int arrowLength = 82;
                double alphaRad = alpha * Math.PI / 180.0;
                int forceStartX = sectionCx;
                int forceStartY = sectionCy - 4;
                int forceEndX = forceStartX + (int)(Math.Sin(alphaRad) * arrowLength);
                int forceEndY = forceStartY - (int)(Math.Cos(alphaRad) * arrowLength);

                using Pen referencePen = new(Color.Gray, 1.2f) { DashStyle = DashStyle.Dash };
                using Pen forcePen = new(Color.Black, 4f) { EndCap = LineCap.ArrowAnchor };
                g.DrawLine(referencePen, forceStartX, forceStartY, forceStartX, forceStartY - arrowLength - 8);
                g.DrawLine(forcePen, forceStartX, forceStartY, forceEndX, forceEndY);
                Rectangle arcBox = new(forceStartX - 32, forceStartY - 64, 64, 64);
                g.DrawArc(outline, arcBox, 270, (float)alpha);
                g.DrawString($"α={alpha:0.0}°", labelFont, Brushes.Black, forceStartX + 8, forceStartY - 72);
                g.DrawString("F", titleFont, Brushes.Black, forceEndX + 5, forceEndY - 8);
            }
        }

        private static void DrawDimension(Graphics g, Pen pen, Font font, int x1, int y1, int x2, int y2, string text)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            SizeF size = g.MeasureString(text, font);
            float textX = (x1 + x2 - size.Width) / 2f;
            float textY = (y1 + y2 - size.Height) / 2f;
            RectangleF mask = new(textX - 3f, textY - 1f, size.Width + 6f, size.Height + 2f);
            g.FillRectangle(Brushes.White, mask);
            g.DrawString(text, font, Brushes.Firebrick, textX, textY);
        }
        private MaterialGrade? GetSelectedMaterial()
        {
            return cmbMaterials.SelectedItem as MaterialGrade;
        }
        private MaterialGrade? GetSelectedPinMaterial()
        {
            if (cmbPinMaterials.SelectedItem is MaterialGrade material)
                return material;

            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LascheApp",
                "Data");

            string legacyDataDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data");

            MigrateLegacyDataFile(legacyDataDirectory, dataDirectory, "materials.json");
            MigrateLegacyDataFile(legacyDataDirectory, dataDirectory, "shackles.json");

            string materialPath = Path.Combine(dataDirectory, "materials.json");
            string shacklePath = Path.Combine(dataDirectory, "shackles.json");

            _materialDatabase = new MaterialDatabase(materialPath);
            _materialDatabase.Load();

            _shackleDatabase = new ShackleDatabase(shacklePath);
            _shackleDatabase.Load();

            LoadShackleComboBox();
            LoadMaterialComboBox();
            LoadLugTypeComboBox();
            LoadPinMaterialComboBox();

            UpdateSelectedShackleInfo();
            UpdateSelectedMaterialInfo();
            UpdateSelectedPinMaterialInfo();

            UpdateLugTypeUi();
            ConfigureSummaryGrid();
            ApplyLanguage();
        }

        private static void MigrateLegacyDataFile(string legacyDirectory, string targetDirectory, string fileName)
        {
            string source = Path.Combine(legacyDirectory, fileName);
            string target = Path.Combine(targetDirectory, fileName);

            if (File.Exists(target) || !File.Exists(source))
                return;

            Directory.CreateDirectory(targetDirectory);
            File.Copy(source, target, overwrite: false);
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            _settingsMenu.Show(_settingsButton, new Point(0, _settingsButton.Height));
        }

        private void OpenDatabaseSettings(int selectedTab)
        {
            if (_materialDatabase == null || _shackleDatabase == null)
                return;

            string? selectedMaterialId = (cmbMaterials.SelectedItem as MaterialGrade)?.Id;
            string? selectedPinMaterialId = (cmbPinMaterials.SelectedItem as MaterialGrade)?.Id;
            string? selectedShackleId = (cmbShackles.SelectedItem as ShackleData)?.Id;

            using SettingsForm settings = new(
                _materialDatabase,
                _shackleDatabase,
                IsGerman,
                selectedTab);
            settings.ShowDialog(this);

            LoadMaterialComboBox();
            LoadPinMaterialComboBox();
            LoadShackleComboBox();

            if (selectedMaterialId != null)
                cmbMaterials.SelectedValue = selectedMaterialId;
            if (selectedPinMaterialId != null)
                cmbPinMaterials.SelectedValue = selectedPinMaterialId;
            if (selectedShackleId != null)
                cmbShackles.SelectedValue = selectedShackleId;

            UpdateSelectedMaterialInfo();
            UpdateSelectedPinMaterialInfo();
            UpdateSelectedShackleInfo();
        }

        private void UpdateLugTypeUi()
        {
            bool isTransportLug = GetSelectedLugType() == LugType.TransportLug;
            bool isTensionLug = GetSelectedLugType() == LugType.TensionLug;

            cmbShackles.Visible = isTransportLug;
            lblShackleWll.Visible = isTransportLug;
            lblShackleDpin.Visible = isTransportLug;
            lblShackleB1.Visible = isTransportLug;
            lblShackleHDnv.Visible = isTransportLug;
            lblShackleInfo.Visible = isTransportLug;
            btnSelectShackleByLoad.Visible = isTransportLug;

            txtDnvOutOfPlaneAngle_deg.Visible = isTransportLug;
            label13.Visible = isTransportLug;
            txtRch_mm.Visible = true;
            label16.Visible = true;
            txtCheekPlateWeldA_mm.Visible = true;
            label17.Visible = true;

            cmbPinMaterials.Visible = isTensionLug;
            label10.Visible = isTensionLug;
            lblPinFy.Visible = isTensionLug;
            lblPinFu.Visible = isTensionLug;
            txtTensionPinDiameter_mm.Visible = isTensionLug;
            label9.Visible = isTensionLug;
            txtOuterLugThicknessT2_mm.Visible = isTensionLug;
            lblOuterLugThicknessT2.Visible = isTensionLug;
            txtGapS_mm.Visible = isTensionLug;
            lblGapS.Visible = isTensionLug;

            grpTransportLug.Visible = isTransportLug;
            grpTensionLug.Visible = isTensionLug;

        }

        private void ConfigureSummaryGrid()
        {
            dgvCheckSummary.Columns.Clear();

            dgvCheckSummary.AllowUserToAddRows = false;
            dgvCheckSummary.AllowUserToDeleteRows = false;
            dgvCheckSummary.ReadOnly = true;
            dgvCheckSummary.RowHeadersVisible = false;
            dgvCheckSummary.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCheckSummary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCheckSummary.Columns.Add("Group", "Group");
            dgvCheckSummary.Columns.Add("Check", "Check");
            dgvCheckSummary.Columns.Add("Status", "Status");
            dgvCheckSummary.Columns.Add("Eta", "η");

            dgvCheckSummary.Columns["Group"].FillWeight = 20;
            dgvCheckSummary.Columns["Check"].FillWeight = 55;
            dgvCheckSummary.Columns["Status"].FillWeight = 15;
            dgvCheckSummary.Columns["Eta"].FillWeight = 10;
            dgvCheckSummary.SelectionChanged += dgvCheckSummary_SelectionChanged;

        }

        private void FillCheckSummary(
            PadeyeCheckResult lugResult,
            PinCheckResult? pinResult = null)
        {
            dgvCheckSummary.Rows.Clear();

            List<CheckItem> items = GetUiCheckItems(lugResult, pinResult);

            foreach (CheckItem item in items)
            {
                string group = GetCheckGroup(item.Name);

                string status =
                    item.IsOk
                        ? "OK"
                        : item.IsWarning
                            ? (IsGerman ? "WARNUNG" : "WARNING")
                            : (IsGerman ? "NICHT ERFÜLLT" : "NOT OK");

                string eta =
                    item.ShowUtilization
                        ? item.Utilization.ToString("0.000")
                        : "";

                int rowIndex = dgvCheckSummary.Rows.Add(
                    LocalizeUiText(group),
                    LocalizeUiText(item.Name),
                    status,
                    eta);
                dgvCheckSummary.Rows[rowIndex].Tag = item.Name;
            }
        }

        private List<CheckItem> GetUiCheckItems(
            PadeyeCheckResult lugResult,
            PinCheckResult? pinResult)
        {
            List<CheckItem> items = new();

            if (!lugResult.BasicResult.HasErrors)
                items.AddRange(lugResult.BasicResult.CheckItems);

            if (!lugResult.EcGeometryResult.HasErrors)
                items.AddRange(lugResult.EcGeometryResult.SummaryCheckItems);

            if (!lugResult.BearingResult.HasErrors)
                items.AddRange(lugResult.BearingResult.CheckItems);

            if (lugResult.OuterLugResult.IsActive)
                items.AddRange(lugResult.OuterLugResult.CheckItems);

            if (!lugResult.DnvOutOfPlaneResult.HasErrors &&
                lugResult.DnvOutOfPlaneResult.IsActive)
            {
                items.AddRange(lugResult.DnvOutOfPlaneResult.CheckItems);
            }

            if (pinResult != null && !pinResult.HasErrors)
                items.AddRange(pinResult.CheckItems);
            if (dgvCheckSummary.Rows.Count > 0)
            {
                dgvCheckSummary.Rows[0].Selected = true;
                ShowSelectedCheckDetail();
            }

            return items
                .OrderBy(i => GetCheckGroupSortIndex(GetCheckGroup(i.Name)))
                .ThenByDescending(i => i.ShowUtilization ? i.Utilization : -1.0)
                .ToList();
            
        }

        private string GetCheckGroup(string checkName)
        {
            if (checkName.Contains("Lug t2", StringComparison.OrdinalIgnoreCase))
                return "Lug t2";

            if (checkName.Contains("WLL", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Shackle", StringComparison.OrdinalIgnoreCase))
                return "Shackle";

            if (checkName.Contains("geometry", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Hole diameter", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("clearance", StringComparison.OrdinalIgnoreCase))
                return "Geometry";

            if (checkName.Contains("bearing", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("contact stress", StringComparison.OrdinalIgnoreCase))
                return "Bearing";

            if (checkName.Contains("Cheek", StringComparison.OrdinalIgnoreCase))
                return "Cheek plates";

            if (checkName.Contains("DNV", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("angled pull", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("tear-out", StringComparison.OrdinalIgnoreCase))
                return "DNV";

            if (checkName.Contains("Pin", StringComparison.OrdinalIgnoreCase))
                return "Pin";

            return "General";
        }

        private int GetCheckGroupSortIndex(string group)
        {
            return group switch
            {
                "Geometry" => 1,
                "Shackle" => 2,
                "Bearing" => 3,
                "Lug t2" => 4,
                "Cheek plates" => 5,
                "DNV" => 6,
                "Pin" => 7,
                _ => 99
            };
        }
        private void UpdateSelectedPinMaterialInfo()
        {
            MaterialGrade? pinMaterial = GetSelectedPinMaterial();

            if (pinMaterial == null)
            {
                lblPinFy.Text = "fy,p = -";
                lblPinFu.Text = "fu,p = -";
                return;
            }

            if (!TryReadDouble(txtTensionPinDiameter_mm.Text, out double pinDiameter_mm))
            {
                lblPinFy.Text = "fy,p = -";
                lblPinFu.Text = "fu,p = -";
                return;
            }

            try
            {
                MaterialPropertiesAtThickness props =
                    _materialDatabase!.GetProperties(pinMaterial.Id, pinDiameter_mm);

                lblPinFy.Text = $"fy,p = {props.Fy_Nmm2:0} N/mm²";
                lblPinFu.Text = $"fu,p = {props.Fu_Nmm2:0} N/mm²";
            }
            catch
            {
                lblPinFy.Text = "fy,p = n/a";
                lblPinFu.Text = "fu,p = n/a";
            }
        }
        private void LoadPinMaterialComboBox()
        {
            if (_materialDatabase == null)
                return;

            cmbPinMaterials.DataSource = null;

            cmbPinMaterials.DataSource = _materialDatabase.Materials
                .OrderByDescending(m => string.IsNullOrEmpty(m.Name) ? '\0' : m.Name[0])
                .ThenBy(m => m.Name)
                .ToList();

            cmbPinMaterials.DisplayMember = "Name";
            cmbPinMaterials.ValueMember = "Id";

            MaterialGrade? defaultPinMaterial = _materialDatabase.Materials
                .FirstOrDefault(m => m.Id == "C45E_QT")
                ?? _materialDatabase.Materials.FirstOrDefault(m => m.Id == "C45E")
                ?? _materialDatabase.Materials.FirstOrDefault();

            if (defaultPinMaterial != null)
                cmbPinMaterials.SelectedValue = defaultPinMaterial.Id;
        }
        private void LoadShackleComboBox()
        {
            if (_shackleDatabase == null)
                return;

            cmbShackles.DataSource = null;

            cmbShackles.DataSource = _shackleDatabase.Shackles
                .OrderBy(s => s.WLL_kN)
                .ToList();

            cmbShackles.DisplayMember = "Name";
            cmbShackles.ValueMember = "Id";
        }

        private void cmbShackles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedShackleInfo();
        }
        private ShackleData? GetSelectedShackle()
        {
            return cmbShackles.SelectedItem as ShackleData;
        }

        private void UpdateSelectedShackleInfo()
        {
            if (cmbShackles.SelectedItem is not ShackleData shackle)
                return;

            lblShackleWll.Text = $"WLL: {shackle.WLL_kN:0.00} kN";
            lblShackleDpin.Text = $"Dpin: {shackle.Dpin_mm:0.0} mm";
            lblShackleB1.Text = $"B1: {shackle.B1_mm:0.0} mm";
            lblShackleHDnv.Text = $"H_DNV: {shackle.H_DNV_mm:0.0} mm";

            lblShackleInfo.Text =
                $"Nominal size: {shackle.NominalSize} | " +
                $"h2: {shackle.H2_mm:0.0} mm | " +
                $"b2: {shackle.B2_mm:0.0} mm";
        }
        private bool TryGetSelectedShackleGeometry(
            out double dpin_mm,
            out double b1_mm,
            out double hDnv_mm,
            out double wll_kN)
        {
            dpin_mm = 0.0;
            b1_mm = 0.0;
            hDnv_mm = 0.0;
            wll_kN = 0.0;

            ShackleData? shackle = GetSelectedShackle();

            if (shackle == null)
                return false;

            dpin_mm = shackle.Dpin_mm;
            b1_mm = shackle.B1_mm;
            hDnv_mm = shackle.H_DNV_mm;
            wll_kN = shackle.WLL_kN;

            return true;
        }

        private bool TryReadDouble(string text, out double value)
        {
            text = text.Trim().Replace(',', '.');

            return double.TryParse(
                text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }

        private void ApplyRchLimit()
        {
            if (_updatingRchLimit ||
                !TryReadDouble(txtEdgeDistanceA_mm.Text, out double endDistanceE_mm) ||
                !TryReadDouble(txtRch_mm.Text, out double rch_mm))
            {
                return;
            }

            double maximumRch_mm = endDistanceE_mm - 10.0;
            if (maximumRch_mm < 0.0 || rch_mm <= maximumRch_mm)
                return;

            _updatingRchLimit = true;
            try
            {
                txtRch_mm.Text = maximumRch_mm.ToString(
                    "0.###",
                    System.Globalization.CultureInfo.CurrentCulture);
                txtRch_mm.SelectionStart = txtRch_mm.TextLength;
            }
            finally
            {
                _updatingRchLimit = false;
            }
        }

        private bool TryReadOptionalDoubleControl(
            string controlName,
            double defaultValue,
            out double value,
            out string error)
        {
            value = defaultValue;
            error = "";

            Control[] controls = Controls.Find(controlName, true);

            if (controls.Length == 0)
                return true;

            string text = controls[0].Text;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            if (!TryReadDouble(text, out value))
            {
                error = $"Invalid input: {controlName}";
                return false;
            }

            return true;
        }

        private void btnCheckBasicPadeye_Click(object sender, EventArgs e)
        {
            try
            {
                RunBasicPadeyeCheck();
            }
            catch (Exception ex)
            {
                txtBasicCheckResult.Text = IsGerman
                    ? $"Die Berechnung konnte nicht ausgeführt werden: {ex.Message}"
                    : $"The calculation could not be completed: {ex.Message}";
            }

            // Input validation messages are stored in the report text box. When
            // the Summary tab is active they used to be invisible, which made the
            // Verify button appear to do nothing.
            if (dgvCheckSummary.Rows.Count == 0 &&
                !string.IsNullOrWhiteSpace(txtBasicCheckResult.Text))
            {
                ShowCheckInputError(txtBasicCheckResult.Text);
            }
        }

        private void RunBasicPadeyeCheck()
        {
            ApplyRchLimit();
            txtBasicCheckResult.Clear();
            dgvCheckSummary.Rows.Clear();

            if (!TryReadDouble(txtLoad_kN.Text, out double fEd_kN))
            {
                txtBasicCheckResult.Text = "Invalid input: F_Ed [kN]";
                return;
            }

            if (!TryReadDouble(txtLoadSer_kN.Text, out double fEdSer_kN))
            {
                txtBasicCheckResult.Text = "Invalid input: F_sher [kN]";
                return;
            }

            if (!TryReadDouble(txtPlateThickness_mm.Text, out double t_mm))
            {
                txtBasicCheckResult.Text = "Invalid input: t [mm]";
                return;
            }

            if (!TryReadDouble(txtHoleDiameter_mm.Text, out double holeDiameter_mm))
            {
                txtBasicCheckResult.Text = "Invalid input: d0 [mm]";
                return;
            }

            if (!TryReadDouble(txtPlateWidth_mm.Text, out double plateWidth_mm))
            {
                txtBasicCheckResult.Text = "Invalid input: b [mm]";
                return;
            }

            if (!TryReadDouble(txtEdgeDistanceA_mm.Text, out double endDistanceE_mm))
            {
                txtBasicCheckResult.Text = "Invalid input: e [mm]";
                return;
            }

            double edgeDistanceA_mm = endDistanceE_mm - holeDiameter_mm / 2.0;
            double sideDistanceC_mm = (plateWidth_mm - holeDiameter_mm) / 2.0;

            if (edgeDistanceA_mm <= 0)
            {
                txtBasicCheckResult.Text = "Invalid geometry: e must be greater than d0 / 2.";
                return;
            }

            if (sideDistanceC_mm <= 0)
            {
                txtBasicCheckResult.Text = "Invalid geometry: b must be greater than d0.";
                return;
            }

            MaterialGrade? material = GetSelectedMaterial();

            if (material == null)
            {
                txtBasicCheckResult.Text = "No material selected.";
                return;
            }
            MaterialPropertiesAtThickness materialProps;

            try
            {
                materialProps = _materialDatabase!.GetProperties(material.Id, t_mm);
            }
            catch
            {
                txtBasicCheckResult.Text = "Material properties not available for this thickness.";
                return;
            }
            LugType lugType = GetSelectedLugType();

            if (lugType == LugType.TensionLug)
            {
                if (!TryReadDouble(txtTensionPinDiameter_mm.Text, out double tensionPinDiameter_mm))
                {
                    txtBasicCheckResult.Text = "Invalid input: pin diameter d [mm]";
                    return;
                }

                if (!TryReadDouble(txtOuterLugThicknessT2_mm.Text, out double outerLugThicknessT2_mm))
                {
                    txtBasicCheckResult.Text = "Invalid input: outer lug thickness t2 [mm]";
                    return;
                }

                if (!TryReadDouble(txtGapS_mm.Text, out double gapS_mm))
                {
                    txtBasicCheckResult.Text = "Invalid input: gap s [mm]";
                    return;
                }
                if (!TryReadOptionalDoubleControl("txtCheekPlateThickness_mm", 0.0, out double tensionCheekPlateThickness_mm, out string tensionCheekThicknessError))
                {
                    txtBasicCheckResult.Text = tensionCheekThicknessError;
                    return;
                }

                if (tensionCheekPlateThickness_mm < 0)
                {
                    txtBasicCheckResult.Text = "Invalid input: cheek plate thickness tch must not be negative.";
                    return;
                }

                if (!TryReadOptionalDoubleControl("txtRch_mm", 0.0, out double tensionRch_mm, out string tensionRchError))
                {
                    txtBasicCheckResult.Text = tensionRchError;
                    return;
                }

                if (!TryReadOptionalDoubleControl("txtCheekPlateWeldA_mm", 0.0, out double tensionCheekPlateWeldA_mm, out string tensionWeldAError))
                {
                    txtBasicCheckResult.Text = tensionWeldAError;
                    return;
                }

                if (tensionCheekPlateThickness_mm > 0.0 && tensionRch_mm <= 0.0)
                {
                    txtBasicCheckResult.Text = "Invalid input: Rch must be greater than 0 if cheek plates are present.";
                    return;
                }

                if (tensionCheekPlateThickness_mm > 0.0 && tensionCheekPlateWeldA_mm <= 0.0)
                {
                    txtBasicCheckResult.Text = "Invalid input: cheek plate weld throat a_weld must be greater than 0 if cheek plates are present.";
                    return;
                }

                double innerLugThicknessForPinMoment_mm =
             t_mm + 2.0 * tensionCheekPlateThickness_mm;

                if (chkIncludeCheekPlatesInBearing.Checked && tensionCheekPlateThickness_mm <= 0)
                {
                    txtBasicCheckResult.Text = "Invalid input: cheek plate thickness tch must be greater than 0 if cheek plates are considered.";
                    return;
                }


                if (outerLugThicknessT2_mm <= 0)
                {
                    txtBasicCheckResult.Text = "Invalid input: outer lug thickness t2 must be greater than 0.";
                    return;
                }

                if (gapS_mm < 0)
                {
                    txtBasicCheckResult.Text = "Invalid input: gap s must not be negative.";
                    return;
                }

                MaterialPropertiesAtThickness outerLugMaterialProps;
                try
                {
                    // The outer plates use the same material grade as tpl, but the
                    // strength values must be selected for their own thickness t2.
                    outerLugMaterialProps = _materialDatabase!.GetProperties(material.Id, outerLugThicknessT2_mm);
                }
                catch
                {
                    txtBasicCheckResult.Text = "Material properties not available for outer lug thickness t2.";
                    return;
                }

                double pinMoment_kNmm =
                    fEd_kN / 8.0 *
                    (outerLugThicknessT2_mm + 4.0 * gapS_mm + 2.0 * innerLugThicknessForPinMoment_mm);

                double pinMomentSer_kNmm =
                    pinMoment_kNmm * fEdSer_kN / fEd_kN;

                MaterialGrade? pinMaterial = GetSelectedPinMaterial();

                if (pinMaterial == null)
                {
                    txtBasicCheckResult.Text = "No pin material selected.";
                    return;
                }

                MaterialPropertiesAtThickness pinMaterialProps;

                try
                {
                    pinMaterialProps = _materialDatabase!.GetProperties(pinMaterial.Id, tensionPinDiameter_mm);
                }
                catch
                {
                    txtBasicCheckResult.Text = "Pin material properties not available for this diameter.";
                    return;
                }

                PinCheckInput pinInput = new PinCheckInput
                {
                    F_Ed_kN = fEd_kN,
                    F_Ed_ser_kN = fEdSer_kN,

                    M_Ed_kNmm = pinMoment_kNmm,
                    M_Ed_ser_kNmm = pinMomentSer_kNmm,

                    MomentCalculatedFromTensionLugGeometry = true,
                    InnerLugThicknessT_mm = innerLugThicknessForPinMoment_mm,

                    OuterLugThicknessT2_mm = outerLugThicknessT2_mm,
                    GapS_mm = gapS_mm,

                    PinDiameter_mm = tensionPinDiameter_mm,

                    PinFy_Nmm2 = pinMaterialProps.Fy_Nmm2,
                    PinFu_Nmm2 = pinMaterialProps.Fu_Nmm2,

                    GammaM0 = 1.0,
                    GammaM2 = 1.25,
                    GammaM6_ser = 1.0,

                    IsReplaceablePin = chkReplaceablePin.Checked
                };

                PinCheckResult pinResult = PinChecker.Check(pinInput);

                PadeyeCheckInput tensionPadeyeInput = new PadeyeCheckInput
                {
                    LugType = LugType.TensionLug,

                    F_Ed_kN = fEd_kN,
                    F_Ed_ser_kN = fEdSer_kN,

                    PlateThickness_mm = t_mm,
                    PlateWidth_mm = plateWidth_mm,
                    HoleDiameter_mm = holeDiameter_mm,
                    EndDistanceE_mm = endDistanceE_mm,

                    OuterLugThicknessT2_mm = outerLugThicknessT2_mm,
                    OuterLugFy_Nmm2 = outerLugMaterialProps.Fy_Nmm2,
                    OuterLugFu_Nmm2 = outerLugMaterialProps.Fu_Nmm2,

                    CheekPlateThickness_mm = tensionCheekPlateThickness_mm,
                    IncludeCheekPlatesInBearing = chkIncludeCheekPlatesInBearing.Checked,

                    EdgeDistanceA_mm = edgeDistanceA_mm,
                    SideDistanceC_mm = sideDistanceC_mm,

                    Fy_Nmm2 = materialProps.Fy_Nmm2,
                    Fu_Nmm2 = materialProps.Fu_Nmm2,
                    E_Nmm2 = materialProps.E_Nmm2,
                    BetaW = materialProps.BetaW,
                    GammaM0 = 1.0,
                    GammaM6_ser = 1.0,
                    GammaM2 = 1.25,

                    DnvOutOfPlaneAngle_deg = 0.0,
                    DnvBeta = 0.7,
                    DnvGammaM = 1.15,

                    Rch_mm = tensionRch_mm,
                    CheekPlateWeldA_mm = tensionCheekPlateWeldA_mm,

                    // For Tension Lug this is the real pin diameter, not a shackle pin.
                    ShackleDpin_mm = tensionPinDiameter_mm,

                    PinClearance_mm = 2.0,
                    IsReplaceablePin = chkReplaceablePin.Checked
                };

                PadeyeCheckResult tensionPadeyeResult =
                    PadeyeChecker.Check(tensionPadeyeInput);

                bool overallOk = pinResult.IsOk && tensionPadeyeResult.IsOk;

                double maxUtilization = Math.Max(
                    pinResult.MaxUtilization,
                    tensionPadeyeResult.MaxUtilization);

                string governingCheckName =
                    pinResult.MaxUtilization >= tensionPadeyeResult.MaxUtilization
                        ? pinResult.GoverningCheckName
                        : tensionPadeyeResult.GoverningCheckName;

                txtBasicCheckResult.Text =
                    LocalizeReport(PadeyeTensionLugReportFormatter.Format(
                        tensionPadeyeResult,
                        pinResult,
                    new PadeyeTensionLugReportInfo
                    {
                        Project = _txtProjectNumber.Text.Trim(),
                        Subject = _txtVerificationSubject.Text.Trim(),
                        PreparedBy = Environment.UserName,
                        Date = DateTime.Today,
                        PlateMaterial = material.Name,
                        PinMaterial = pinMaterial.Name
                    }));

                _lastPadeyeResult = tensionPadeyeResult;
                _lastPinResult = pinResult;

                FillCheckSummary(tensionPadeyeResult, pinResult);
                tabResults.SelectedTab = tabSummary;

                return;
            }

            bool hasShackle = TryGetSelectedShackleGeometry(
                out double dpin_mm,
                out double b1_mm,
                out double hDnv_mm,
                out double wll_kN);

            if (!hasShackle)
            {
                txtBasicCheckResult.Text = "No shackle selected.";
                return;
            }


            if (!TryReadOptionalDoubleControl("txtDnvOutOfPlaneAngle_deg", 0.0, out double dnvAlpha_deg, out string dnvAlphaError))
            {
                txtBasicCheckResult.Text = dnvAlphaError;
                return;
            }

            if (!TryReadOptionalDoubleControl("txtDnvBeta", 0.7, out double dnvBeta, out string dnvBetaError))
            {
                txtBasicCheckResult.Text = dnvBetaError;
                return;
            }

            if (!TryReadOptionalDoubleControl("txtCheekPlateThickness_mm", 0.0, out double cheekPlateThickness_mm, out string cheekThicknessError))
            {
                txtBasicCheckResult.Text = cheekThicknessError;
                return;
            }

            if (!TryReadOptionalDoubleControl("txtRch_mm", 0.0, out double rch_mm, out string rchError))
            {
                txtBasicCheckResult.Text = rchError;
                return;
            }

            if (!TryReadOptionalDoubleControl("txtCheekPlateWeldA_mm", 0.0, out double cheekPlateWeldA_mm, out string weldAError))
            {
                txtBasicCheckResult.Text = weldAError;
                return;
            }

            PadeyeCheckInput padeyeInput = new PadeyeCheckInput
            {
                LugType = lugType,

                F_Ed_kN = fEd_kN,
                F_Ed_ser_kN = fEdSer_kN,

                PlateThickness_mm = t_mm,
                PlateWidth_mm = plateWidth_mm,
                HoleDiameter_mm = holeDiameter_mm,

                EdgeDistanceA_mm = edgeDistanceA_mm,
                SideDistanceC_mm = sideDistanceC_mm,

                Fy_Nmm2 = materialProps.Fy_Nmm2,
                Fu_Nmm2 = materialProps.Fu_Nmm2,
                E_Nmm2 = materialProps.E_Nmm2,
                BetaW = materialProps.BetaW,
                GammaM0 = 1.0,
                GammaM6_ser = 1.0,

                ShackleWLL_kN = wll_kN,
                ShackleDpin_mm = dpin_mm,
                ShackleB1_mm = b1_mm,
                ShackleH_DNV_mm = hDnv_mm,

                PinClearance_mm = 3.0,

                IsReplaceablePin = chkReplaceablePin.Checked,

                DnvOutOfPlaneAngle_deg = dnvAlpha_deg,
                DnvBeta = dnvBeta,
                DnvGammaM = 1.15,
                GammaM2 = 1.25,

                CheekPlateThickness_mm = cheekPlateThickness_mm,
                EndDistanceE_mm = endDistanceE_mm,
                Rch_mm = rch_mm,
                CheekPlateWeldA_mm = cheekPlateWeldA_mm,

                IncludeCheekPlatesInBearing = chkIncludeCheekPlatesInBearing.Checked
            };

            PadeyeCheckResult padeyeResult =
                PadeyeChecker.Check(padeyeInput);

            txtBasicCheckResult.Text =
                LocalizeReport(PadeyeTransportLugReportFormatter.Format(
                    padeyeResult,
                    new PadeyeTransportLugReportInfo
                    {
                        Project = _txtProjectNumber.Text.Trim(),
                        Subject = _txtVerificationSubject.Text.Trim(),
                        PreparedBy = Environment.UserName,
                        Date = DateTime.Today,
                        PlateMaterial = material.Name,
                        ShackleName = GetSelectedShackle()?.Name ?? ""
                    }));
            
            _lastPadeyeResult = padeyeResult;
            _lastPinResult = null;

            FillCheckSummary(padeyeResult);
            tabResults.SelectedTab = tabSummary;

        }
        private LugType GetSelectedLugType()
        {
            if (cmbLugType.SelectedValue is LugType lugType)
                return lugType;

            return LugType.TransportLug;
        }
        private void LoadLugTypeComboBox()
        {
            cmbLugType.DataSource = null;

            cmbLugType.DataSource = new List<KeyValuePair<LugType, string>>
            {
                new KeyValuePair<LugType, string>(LugType.TransportLug, "Transport Lug"),
                new KeyValuePair<LugType, string>(LugType.TensionLug, "Tension Lug")
            };

            cmbLugType.DisplayMember = "Value";
            cmbLugType.ValueMember = "Key";
            cmbLugType.SelectedValue = LugType.TransportLug;
        }


        private void btnSelectShackleByLoad_Click(object sender, EventArgs e)
        {
            if (_shackleDatabase == null)
                return;

            if (!TryReadDouble(txtLoadSer_kN.Text, out double fEdSer_kN))
            {
                MessageBox.Show("Invalid input: F_sher [kN]");
                return;
            }

            ShackleData? suitableShackle =
                _shackleDatabase.GetSmallestSuitableByWll(fEdSer_kN);

            if (suitableShackle == null)
            {
                MessageBox.Show("No suitable shackle found for this load.");
                return;
            }

            cmbShackles.SelectedValue = suitableShackle.Id;

            UpdateSelectedShackleInfo();
            btnCheckBasicPadeye_Click(sender, e);
        }

        private void btnPredesign_Click(object sender, EventArgs e)
        {
            if (_materialDatabase == null || _shackleDatabase == null)
                return;

            if (!TryReadDouble(txtLoad_kN.Text, out double fEd_kN) || fEd_kN <= 0.0)
            {
                ShowPredesignMessage(IsGerman
                    ? "Für die Vorbemessung muss F_Ed eingegeben werden."
                    : "F_Ed must be entered for predesign.");
                return;
            }

            MaterialGrade? lugMaterial = GetSelectedMaterial();
            if (lugMaterial == null)
            {
                ShowPredesignMessage(IsGerman
                    ? "Bitte zuerst einen Laschenwerkstoff wählen."
                    : "Select a lug material first.");
                return;
            }

            double pinDiameter_mm;
            if (GetSelectedLugType() == LugType.TensionLug)
            {
                MaterialGrade? pinMaterial = GetSelectedPinMaterial();
                if (pinMaterial == null || !TryFindMinimumPinDiameter(
                        fEd_kN, pinMaterial, out pinDiameter_mm))
                {
                    ShowPredesignMessage(IsGerman
                        ? "Mit dem gewählten Bolzenwerkstoff konnte kein geeigneter Bolzendurchmesser ermittelt werden."
                        : "No suitable pin diameter could be determined for the selected pin material.");
                    return;
                }

                txtTensionPinDiameter_mm.Text = FormatPredesignValue(pinDiameter_mm);
            }
            else
            {
                double shackleLoad_kN =
                    TryReadDouble(txtLoadSer_kN.Text, out double fSher_kN) && fSher_kN > 0.0
                        ? fSher_kN
                        : fEd_kN / 2.0;

                ShackleData? shackle = _shackleDatabase.GetSmallestSuitableByWll(shackleLoad_kN);
                if (shackle == null)
                {
                    ShowPredesignMessage(IsGerman
                        ? "In der Datenbank wurde kein geeigneter Schäkel für diese Last gefunden."
                        : "No suitable shackle was found in the database for this load.");
                    return;
                }

                cmbShackles.SelectedValue = shackle.Id;
                UpdateSelectedShackleInfo();
                pinDiameter_mm = shackle.Dpin_mm;
            }

            double holeDiameter_mm = pinDiameter_mm + 3.0;
            if (!TryFindMinimumBearingThickness(
                    fEd_kN, pinDiameter_mm, lugMaterial,
                    out double plateThickness_mm,
                    out MaterialPropertiesAtThickness? plateProperties))
            {
                ShowPredesignMessage(IsGerman
                    ? "Mit dem gewählten Laschenwerkstoff konnte keine ausreichende Blechdicke ermittelt werden."
                    : "No sufficient plate thickness could be determined for the selected lug material.");
                return;
            }

            double forceTerm_mm =
                fEd_kN * 1000.0 /
                (2.0 * plateThickness_mm * plateProperties!.Fy_Nmm2);
            double aMin_mm = forceTerm_mm + 2.0 * holeDiameter_mm / 3.0;
            double cMin_mm = forceTerm_mm + holeDiameter_mm / 3.0;
            double eMin_mm = Math.Ceiling(aMin_mm + holeDiameter_mm / 2.0);
            double bMin_mm = Math.Ceiling(2.0 * cMin_mm + holeDiameter_mm);

            txtPlateThickness_mm.Text = FormatPredesignValue(plateThickness_mm);
            txtHoleDiameter_mm.Text = FormatPredesignValue(holeDiameter_mm);
            txtEdgeDistanceA_mm.Text = FormatPredesignValue(eMin_mm);
            txtPlateWidth_mm.Text = FormatPredesignValue(bMin_mm);
            UpdateSelectedMaterialInfo();
            _geometryGuide.Invalidate();
        }

        private bool TryFindMinimumPinDiameter(
            double fEd_kN,
            MaterialGrade pinMaterial,
            out double pinDiameter_mm)
        {
            pinDiameter_mm = 0.0;
            double maximumDefinedThickness = pinMaterial.Ranges.Count == 0
                ? 0.0
                : pinMaterial.Ranges.Max(range => range.ThicknessMax_mm);

            for (double candidate_mm = 10.0;
                 candidate_mm < maximumDefinedThickness;
                 candidate_mm += 10.0)
            {
                try
                {
                    MaterialPropertiesAtThickness properties =
                        _materialDatabase!.GetProperties(pinMaterial.Id, candidate_mm);
                    double area_mm2 = Math.PI * candidate_mm * candidate_mm / 4.0;
                    double shearResistance_kN =
                        0.6 * area_mm2 * properties.Fu_Nmm2 / 1.25 / 1000.0;

                    if (shearResistance_kN >= fEd_kN / 2.0)
                    {
                        pinDiameter_mm = candidate_mm;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // A user-defined database may contain gaps between ranges.
                }
            }

            return false;
        }

        private bool TryFindMinimumBearingThickness(
            double fEd_kN,
            double pinDiameter_mm,
            MaterialGrade lugMaterial,
            out double plateThickness_mm,
            out MaterialPropertiesAtThickness? plateProperties)
        {
            plateThickness_mm = 0.0;
            plateProperties = null;
            double maximumDefinedThickness = lugMaterial.Ranges.Count == 0
                ? 0.0
                : lugMaterial.Ranges.Max(range => range.ThicknessMax_mm);

            for (double candidate_mm = 10.0;
                 candidate_mm < maximumDefinedThickness;
                 candidate_mm += 10.0)
            {
                try
                {
                    MaterialPropertiesAtThickness properties =
                        _materialDatabase!.GetProperties(lugMaterial.Id, candidate_mm);
                    double bearingResistance_kN =
                        1.5 * candidate_mm * pinDiameter_mm * properties.Fy_Nmm2 / 1000.0;

                    if (bearingResistance_kN >= fEd_kN)
                    {
                        plateThickness_mm = candidate_mm;
                        plateProperties = properties;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // A user-defined database may contain gaps between ranges.
                }
            }

            return false;
        }

        private static string FormatPredesignValue(double value) =>
            value.ToString("0.###", System.Globalization.CultureInfo.CurrentCulture);

        private void ShowPredesignMessage(string message)
        {
            MessageBox.Show(
                this,
                message,
                IsGerman ? "Vorbemessung" : "Predesign",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        private void LoadMaterialComboBox()
        {
            if (_materialDatabase == null)
                return;

            cmbMaterials.DataSource = null;

            cmbMaterials.DataSource = _materialDatabase.Materials
                .OrderByDescending(m => string.IsNullOrEmpty(m.Name) ? '\0' : m.Name[0])
                .ThenBy(m => m.Name)
                .ToList();

            cmbMaterials.DisplayMember = "Name";
            cmbMaterials.ValueMember = "Id";
        }
        private void UpdateSelectedMaterialInfo()
        {
            if (_materialDatabase == null)
                return;

            MaterialGrade? material = GetSelectedMaterial();

            if (material == null)
                return;

            if (!TryReadDouble(txtPlateThickness_mm.Text, out double thickness_mm))
            {
                lblMaterialFy.Text = "fy: -";
                lblMaterialFu.Text = "fu: -";
                lblMaterialE.Text = $"E: {material.E_Nmm2:0} N/mm²";
                lblMaterialBetaW.Text = $"BetaW: {material.BetaW:0.00}";
                return;
            }

            try
            {
                MaterialPropertiesAtThickness props =
                    _materialDatabase.GetProperties(material.Id, thickness_mm);

                lblMaterialFy.Text = $"fy: {props.Fy_Nmm2:0} N/mm²";
                lblMaterialFu.Text = $"fu: {props.Fu_Nmm2:0} N/mm²";
                lblMaterialE.Text = $"E: {props.E_Nmm2:0} N/mm²";
                lblMaterialBetaW.Text = $"BetaW: {props.BetaW:0.00}";
            }
            catch
            {
                lblMaterialFy.Text = "fy: not available";
                lblMaterialFu.Text = "fu: not available";
                lblMaterialE.Text = $"E: {material.E_Nmm2:0} N/mm²";
                lblMaterialBetaW.Text = $"BetaW: {material.BetaW:0.00}";
            }
        }
        private void ShowSelectedCheckDetail()
        {
            if (txtSelectedCheckDetail == null)
                return;

            if (dgvCheckSummary.SelectedRows.Count == 0)
            {
                txtSelectedCheckDetail.Clear();
                return;
            }

            DataGridViewRow row = dgvCheckSummary.SelectedRows[0];

            string checkName =
                row.Tag as string ?? row.Cells["Check"].Value?.ToString() ?? "";

            txtSelectedCheckDetail.Text =
                LocalizeUiText(GetSelectedCheckDetail(checkName));
        }
        private string GetSelectedCheckDetail(string checkName)
        {
            const string inputErrorPrefix = "INPUT_ERROR:";
            if (checkName.StartsWith(inputErrorPrefix, StringComparison.Ordinal))
                return checkName[inputErrorPrefix.Length..];

            // Section markers are stable in the original English report.
            // The extracted detail is translated only when it is displayed.
            string report = string.IsNullOrWhiteSpace(_lastEnglishReport)
                ? txtBasicCheckResult.Text
                : _lastEnglishReport;

            if (string.IsNullOrWhiteSpace(report))
                return "";

            if (checkName.Contains("WLL", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Shackle WLL");

            if (checkName.Contains("clearance", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Hole diameter", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Lochspiel", StringComparison.OrdinalIgnoreCase))
            {
                PadeyeBasicCheckResult? basicResult = _lastPadeyeResult?.BasicResult;
                if (basicResult != null)
                {
                    PadeyeBasicCheckInput input = basicResult.Input;
                    return
                        "Hole clearance recommendation" + Environment.NewLine +
                        "-----------------------------" + Environment.NewLine +
                        $"Shackle pin diameter Dpin = {input.ShackleDpin_mm:0.0} mm" + Environment.NewLine +
                        $"Provided hole diameter d0 = {input.HoleDiameter_mm:0.0} mm" + Environment.NewLine +
                        $"Clearance allowance = {input.PinClearance_mm:0.0} mm" + Environment.NewLine + Environment.NewLine +
                        "Recommended range:" + Environment.NewLine +
                        "Dpin < d0 <= Dpin + clearance" + Environment.NewLine +
                        $"{input.ShackleDpin_mm:0.0} mm < {input.HoleDiameter_mm:0.0} mm <= " +
                        $"{input.ShackleDpin_mm:0.0} mm + {input.PinClearance_mm:0.0} mm = " +
                        $"{basicResult.RecommendedHoleDiameterMax_mm:0.0} mm" + Environment.NewLine + Environment.NewLine +
                        $"Recommendation fulfilled: {(basicResult.HoleDiameterRecommendationOk ? "OK" : "NOT OK")}";
                }

                return ExtractReportSection(report, "Hole clearance");
            }

            if (checkName.Contains("B1", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Shackle B1 thickness recommendation");

            if (checkName.Contains("Pin-hole geometry", StringComparison.OrdinalIgnoreCase))
            {
                PadeyeBearingInput? bearingInput = _lastPadeyeResult?.BearingResult.Input;

                if (bearingInput != null)
                {
                    bool ok = bearingInput.PinDiameter_mm < bearingInput.HoleDiameter_mm;
                    return
                        $"Pin-hole geometry: {(ok ? "OK" : "NOT OK")}" + Environment.NewLine +
                        "-------------------------" + Environment.NewLine +
                        $"Pin diameter d = {bearingInput.PinDiameter_mm:0.0} mm" + Environment.NewLine +
                        $"Hole diameter d0 = {bearingInput.HoleDiameter_mm:0.0} mm" + Environment.NewLine +
                        $"Check d < d0: {(ok ? "OK" : "NOT OK")}" +
                        (ok
                            ? ""
                            : Environment.NewLine + Environment.NewLine +
                              "The replaceable-pin contact stress cannot be calculated for this geometry.");
                }

                return ExtractReportSection(report, "Pin-hole bearing design");
            }

            if (checkName.Contains("EC geometry", StringComparison.OrdinalIgnoreCase))
                return ExtractReportRange(
                    report,
                    new[] { "Method A", "Möglichkeit A" },
                    new[] { "Pin-hole bearing design" });

            if (checkName.Contains("Lug t2", StringComparison.OrdinalIgnoreCase))
                return ExtractReportRange(
                    report,
                    new[] { "Outer lug plates t2" },
                    new[] { "Cheek plate weld", "2.2. Pin" });

            if (checkName.Contains("bearing design", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Pin-hole bearing", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Pin-hole bearing design");

            if (checkName.Contains("service bearing", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Service bearing");

            if (checkName.Contains("contact stress", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Contact stress");

            if (checkName.Contains("Cheek plate weld", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Cheek plate weld");

            // Match the specific DNV checks before the general angled-pull
            // condition. All three check names contain "angled pull".
            if (checkName.Contains("Pin diameter recommendation", StringComparison.OrdinalIgnoreCase))
            {
                PadeyeDnvOutOfPlaneResult? dnv = _lastPadeyeResult?.DnvOutOfPlaneResult;
                if (dnv != null)
                {
                    return
                        "Pin diameter recommendation for significant angled pull" + Environment.NewLine +
                        "---------------------------------------------------------" + Environment.NewLine +
                        $"Dpin / d0 = {dnv.DpinToHoleRatio:0.000}" + Environment.NewLine +
                        "Recommended: Dpin / d0 >= 0.940" + Environment.NewLine +
                        $"Recommendation fulfilled: {(dnv.PinDiameterRecommendationOk ? "OK" : "NOT OK")}" +
                        Environment.NewLine + Environment.NewLine +
                        "This is a recommendation only and does not govern the overall result.";
                }

                return ExtractReportSection(report, "Pin diameter recommendation");
            }

            if (checkName.Contains("tear-out", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Tear out - only for angled pull");

            if (checkName.Contains("Bearing at angled pull", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("angled pull", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("DNV", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Bearing pressure - only for angled pull");

            if (checkName.Contains("Pin shear + bending", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Pin shear + bending interaction");

            if (checkName.Contains("Pin shear", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Pin shear");

            if (checkName.Contains("Pin bending", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Pin bending");

            if (checkName.Contains("service bending", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Replaceable pin service bending");

            return checkName;
        }
        private string ExtractReportSection(string report, string startMarker)
        {
            return ExtractReportSection(
                report,
                new[] { startMarker });
        }

        private string ExtractReportSection(string report, string[] startMarkers)
        {
            string[] sectionMarkers =
            {
        "Shackle WLL",
        "Hole clearance",
        "Pin diameter recommendation",
        "Shackle B1 thickness recommendation",

        "Method A",
        "Möglichkeit A",
        "Method B",
        "Möglichkeit B",

        "Pin-hole bearing design",
        "Service bearing",
        "Contact stress",

        "Outer lug plates t2",

        "Cheek plate weld",

        "Bearing pressure - only for angled pull",
        "Tear out - only for angled pull",

        "2.2. Pin",
        "Pin result:",
        "Section values",
        "Pin shear",
        "Pin bending",
        "Pin shear + bending interaction",
        "Replaceable pin service bending"
    };

            int calculationStart = GetCalculationStartIndex(report);

            int start = FindFirstSectionHeadingIndex(
                report,
                startMarkers,
                calculationStart);

            if (start < 0)
                return "Detail section not found in calculation report.";

            int end = report.Length;

            foreach (string marker in sectionMarkers)
            {
                int index = FindSectionHeadingIndex(
                    report,
                    marker,
                    start + 1);

                if (index > start && index < end)
                    end = index;
            }

            return report.Substring(start, end - start).Trim();
        }

        private int FindFirstSectionHeadingIndex(
            string report,
            string[] markers,
            int startIndex)
        {
            int result = -1;

            foreach (string marker in markers)
            {
                int index = FindSectionHeadingIndex(report, marker, startIndex);

                if (index >= 0 && (result < 0 || index < result))
                    result = index;
            }

            return result;
        }

        private void ShowCheckInputError(string originalMessage)
        {
            string message = LocalizeCheckInputError(originalMessage.Trim());
            dgvCheckSummary.Rows.Clear();

            int rowIndex = dgvCheckSummary.Rows.Add(
                IsGerman ? "Eingabe" : "Input",
                IsGerman ? "Eingabedaten prüfen" : "Check input data",
                IsGerman ? "NICHT ERFÜLLT" : "NOT OK",
                "");

            DataGridViewRow row = dgvCheckSummary.Rows[rowIndex];
            row.Tag = "INPUT_ERROR:" + message;
            row.Selected = true;
            txtSelectedCheckDetail.Text = message;
            tabResults.SelectedTab = tabSummary;
        }

        private string LocalizeCheckInputError(string message)
        {
            if (!IsGerman)
                return message;

            return message switch
            {
                "Invalid input: cheek plate thickness tch must be greater than 0 if cheek plates are considered." =>
                    "Ungültige Eingabe: Wenn die Verstärkungsbleche im Tragwiderstand berücksichtigt werden, muss tch größer als 0 sein.",
                "Invalid input: Rch must be greater than 0 if cheek plates are present." =>
                    "Ungültige Eingabe: Wenn Verstärkungsbleche vorhanden sind, muss Rch größer als 0 sein.",
                "Invalid input: cheek plate weld throat a_weld must be greater than 0 if cheek plates are present." =>
                    "Ungültige Eingabe: Wenn Verstärkungsbleche vorhanden sind, muss die Schweißnahtdicke a_weld größer als 0 sein.",
                _ => message
            };
        }

        private int FindSectionHeadingIndex(
            string report,
            string marker,
            int startIndex)
        {
            int lineStart = Math.Max(0, startIndex);

            // If startIndex is inside a line, begin with the next complete line.
            if (lineStart > 0 && report[lineStart - 1] != '\n')
            {
                int nextLine = report.IndexOf('\n', lineStart);
                if (nextLine < 0)
                    return -1;

                lineStart = nextLine + 1;
            }

            while (lineStart < report.Length)
            {
                int lineEnd = report.IndexOf('\n', lineStart);
                if (lineEnd < 0)
                    lineEnd = report.Length;

                int textStart = lineStart;
                while (textStart < lineEnd &&
                       (report[textStart] == ' ' || report[textStart] == '\t' || report[textStart] == '\r'))
                {
                    textStart++;
                }

                int lineLength = lineEnd - textStart;
                if (lineLength >= marker.Length &&
                    report.AsSpan(textStart, marker.Length)
                        .Equals(marker.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return textStart;
                }

                lineStart = lineEnd + 1;
            }

            return -1;
        }

        private string ExtractReportRange(
            string report,
            string startMarker,
            string endMarker)
        {
            return ExtractReportRange(
                report,
                new[] { startMarker },
                new[] { endMarker });
        }

        private string ExtractReportRange(
            string report,
            string[] startMarkers,
            string[] endMarkers)
        {
            int calculationStart = GetCalculationStartIndex(report);

            int start = FindFirstMarkerIndex(
                report,
                startMarkers,
                calculationStart);

            if (start < 0)
                return "Detail section not found in calculation report.";

            int end = FindFirstMarkerIndex(
                report,
                endMarkers,
                start + 1);

            if (end < 0)
                end = report.Length;

            return report.Substring(start, end - start).Trim();
        }

        private int GetCalculationStartIndex(string report)
        {
            int index = report.IndexOf(
                "2. Calculation",
                StringComparison.OrdinalIgnoreCase);

            if (index >= 0)
                return index;

            index = report.IndexOf(
                "Calculation",
                StringComparison.OrdinalIgnoreCase);

            return index >= 0 ? index : 0;
        }

        private int FindFirstMarkerIndex(
            string report,
            string[] markers,
            int startIndex)
        {
            int result = -1;

            foreach (string marker in markers)
            {
                int index = report.IndexOf(
                    marker,
                    startIndex,
                    StringComparison.OrdinalIgnoreCase);

                if (index >= 0 && (result < 0 || index < result))
                    result = index;
            }

            return result;
        }
        private void dgvCheckSummary_SelectionChanged(object? sender, EventArgs e)
        {
            ShowSelectedCheckDetail();
        }
        private void cmbMaterials_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedMaterialInfo();
        }

        private void txtPlateThickness_mm_TextChanged(object sender, EventArgs e)
        {
            UpdateSelectedMaterialInfo();
        }

        private void cmbPinMaterials_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedPinMaterialInfo();
        }

        private void txtTensionPinDiameter_mm_TextChanged(object sender, EventArgs e)
        {
            UpdateSelectedPinMaterialInfo();
        }

        private void cmbLugType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLugTypeUi();
        }

        private void btnLoadGuidance_Click(object sender, EventArgs e)
        {
            string message = IsGerman
                ? "Empfohlene Lasteingabe" + Environment.NewLine +
                  Environment.NewLine +
                  "F_sher = 1,50 * F_cable (für Transport: 1,15 * G)" + Environment.NewLine +
                  "oder" + Environment.NewLine +
                  "F_sher = HK-Schleppkraft (1,0-fach)" + Environment.NewLine +
                  Environment.NewLine +
                  "F_Ed = 2,00 * F_sher"
                : "Recommended load input" + Environment.NewLine +
                  Environment.NewLine +
                  "F_sher = 1.50 * F_cable (for transport: 1.15 * G)" + Environment.NewLine +
                  "or" + Environment.NewLine +
                  "F_sher = HK dragging force (1.0-fold)" + Environment.NewLine +
                  Environment.NewLine +
                  "F_Ed = 2.00 * F_sher";

            MessageBox.Show(
                message,
                IsGerman ? "Empfehlung zur Lasteingabe" : "Load input recommendation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        }
    }
}
