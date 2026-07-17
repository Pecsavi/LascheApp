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
        private readonly Label _languageLabel = new();
        private readonly Button _printButton = new();
        private readonly TabPage _guideTab = new();
        private readonly Panel _geometryGuide = new();
        private readonly PrintDocument _reportPrintDocument = new();
        private string[] _printLines = Array.Empty<string>();
        private int _nextPrintLine;
        private string _lastEnglishReport = "";
        public Form1()
        {
            InitializeComponent();
            ConfigureProjectAndReportUi();
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
            _projectGroup.Size = new Size(1034, 72);
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
            _languageLabel.Text = "Language";
            _languageLabel.AutoSize = true;
            _languageLabel.Location = new Point(810, 31);
            _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLanguage.Items.AddRange(new object[] { "English", "Deutsch" });
            _cmbLanguage.Location = new Point(875, 27);
            _cmbLanguage.Size = new Size(133, 23);
            _cmbLanguage.SelectedIndexChanged += Language_SelectedIndexChanged;
            _projectGroup.Controls.AddRange(new Control[]
            {
                _projectLabel, _txtProjectNumber, _subjectLabel, _txtVerificationSubject,
                _languageLabel, _cmbLanguage
            });
            Controls.Add(_projectGroup);
            _projectGroup.BringToFront();

            _printButton.Text = "Print report…";
            _printButton.Location = new Point(350, 6);
            _printButton.Size = new Size(120, 23);
            _printButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _printButton.Click += PrintReport_Click;
            toolTip1.SetToolTip(_printButton, "Open print preview for the current calculation report");

            Panel reportToolbar = new()
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Control
            };
            reportToolbar.Controls.Add(_printButton);

            TableLayoutPanel reportLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            reportLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            reportLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35f));
            reportLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            txtBasicCheckResult.Dock = DockStyle.Fill;
            tabReport.Controls.Clear();
            reportLayout.Controls.Add(reportToolbar, 0, 0);
            reportLayout.Controls.Add(txtBasicCheckResult, 0, 1);
            tabReport.Controls.Add(reportLayout);

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

            _reportPrintDocument.DocumentName = "Lug verification report";
            _reportPrintDocument.BeginPrint += (_, _) =>
            {
                _printLines = txtBasicCheckResult.Lines;
                _nextPrintLine = 0;
            };
            _reportPrintDocument.PrintPage += ReportPrintDocument_PrintPage;
            _cmbLanguage.SelectedIndex = 0;
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
                Height = 800
            };
            preview.ShowDialog(this);
        }

        private bool IsGerman => _cmbLanguage.SelectedIndex == 1;

        private void Language_SelectedIndexChanged(object? sender, EventArgs e)
        {
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
            _languageLabel.Text = de ? "Sprache" : "Language";
            _printButton.Text = de ? "Bericht drucken…" : "Print report…";

            grpLoads.Text = de ? "Lasten / Werkstoff" : "Loads / material";
            grpLugGeometry.Text = de ? "Laschengeometrie" : "Lug geometry";
            grpTransportLug.Text = de ? "Transportlasche / Schäkel / DNV" : "Transport Lug / Shackle / DNV";
            grpTensionLug.Text = de ? "Zuglasche / Bolzen" : "Tension Lug / Pin";
            groupBox5.Text = de ? "Ergebnis" : "Result";
            btnCheckBasicPadeye.Text = de ? "Lasche prüfen" : "Verify lug";
            btnSelectShackleByLoad.Text = de ? "Schäkel nach Last wählen" : "Select shackle by load";
            chkReplaceablePin.Text = de ? "Nachweise: austauschbarer Bolzen" : "Replaceable pin checks";
            chkIncludeCheekPlatesInBearing.Text = de
                ? "Verstärkungsbleche im Tragwiderstand.*"
                : "Cheek plates considered in the calculation.*";
            richTextBox1.Text = de
                ? "*Endgültiges Bohren nach dem Schweißen wird vorausgesetzt!"
                : "*Final drilling after welding is assumed!";

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
            _guideTab.Text = de ? "Geometriehilfe" : "Geometry guide";
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
            int linesPerPage = Math.Max(1, (int)(e.MarginBounds.Height / lineHeight));
            int printed = 0;
            while (_nextPrintLine < _printLines.Length && printed < linesPerPage)
            {
                e.Graphics.DrawString(_printLines[_nextPrintLine], font, Brushes.Black,
                    e.MarginBounds.Left, e.MarginBounds.Top + printed * lineHeight);
                _nextPrintLine++;
                printed++;
            }
            e.HasMorePages = _nextPrintLine < _printLines.Length;
        }

        private void GeometryGuide_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle area = _geometryGuide.ClientRectangle;
            if (area.Width < 100 || area.Height < 100) return;

            using Pen outline = new(Color.FromArgb(45, 75, 105), 3f);
            using Pen dimension = new(Color.FromArgb(185, 70, 55), 1.5f) { CustomEndCap = new AdjustableArrowCap(4, 5), StartCap = LineCap.ArrowAnchor };
            using Pen load = new(Color.FromArgb(35, 135, 85), 4f) { EndCap = LineCap.ArrowAnchor };
            using Brush plate = new SolidBrush(Color.FromArgb(225, 235, 244));
            using Font labelFont = new("Segoe UI", 10f, FontStyle.Bold);
            using Font titleFont = new("Segoe UI", 12f, FontStyle.Bold);

            int cx = area.Width / 2;
            int top = 85;
            int holeR = Math.Min(48, area.Width / 9);
            Rectangle lug = new(cx - 120, top + holeR, 240, Math.Max(210, area.Height - 205));
            g.FillRectangle(plate, lug);
            g.DrawRectangle(outline, lug);
            g.FillEllipse(plate, cx - 120, top - 72, 240, 240);
            g.DrawArc(outline, cx - 120, top - 72, 240, 240, 180, 180);
            g.FillEllipse(Brushes.White, cx - holeR, top + 48 - holeR, holeR * 2, holeR * 2);
            g.DrawEllipse(outline, cx - holeR, top + 48 - holeR, holeR * 2, holeR * 2);
            g.DrawString(GetSelectedLugType() == LugType.TransportLug ? "TRANSPORT LUG" : "TENSION LUG", titleFont, Brushes.Black, 16, 15);

            DrawDimension(g, dimension, labelFont, cx - 120, lug.Bottom + 18, cx + 120, lug.Bottom + 18, "b");
            DrawDimension(g, dimension, labelFont, cx - holeR, top + 48, cx + holeR, top + 48, "d0");
            DrawDimension(g, dimension, labelFont, cx + 145, top + 48, cx + 145, lug.Bottom, "e");
            g.DrawLine(load, cx, top - 55, cx, top - 5);
            g.DrawString("FEd", labelFont, Brushes.SeaGreen, cx + 10, top - 52);

            if (GetSelectedLugType() == LugType.TransportLug)
            {
                using Pen shackle = new(Color.DarkSlateGray, 7f);
                g.DrawArc(shackle, cx - 78, top - 5, 156, 115, 190, 160);
                g.DrawString("Dpin / B1", labelFont, Brushes.DarkSlateGray, cx + 75, top + 12);
                g.DrawString("α = out-of-plane pull angle", labelFont, Brushes.Black, 16, 45);
            }
            else
            {
                using Pen pin = new(Color.DarkSlateGray, 12f);
                g.DrawLine(pin, cx - 105, top + 48, cx + 105, top + 48);
                g.DrawString("pin d", labelFont, Brushes.DarkSlateGray, cx + 112, top + 36);
                g.DrawString("t2 = outer lug thickness     s = gap", labelFont, Brushes.Black, 16, 45);
            }

            g.DrawString("tpl = main plate thickness", labelFont, Brushes.Black, 16, area.Height - 52);
            g.DrawString("tch / Rch / a_weld = cheek plate data", labelFont, Brushes.Black, 220, area.Height - 52);
        }

        private static void DrawDimension(Graphics g, Pen pen, Font font, int x1, int y1, int x2, int y2, string text)
        {
            g.DrawLine(pen, x1, y1, x2, y2);
            SizeF size = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Firebrick, (x1 + x2 - size.Width) / 2f, (y1 + y2 - size.Height) / 2f);
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
                AppDomain.CurrentDomain.BaseDirectory,
                "Data");

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
                "Cheek plates" => 4,
                "DNV" => 5,
                "Pin" => 6,
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
                $"d1: {shackle.D1_mm:0.0} mm | " +
                $"d3: {shackle.D3_mm:0.0} mm | " +
                $"d4: {shackle.D4_inch}";
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
            string message =
                "Recommended load input" + Environment.NewLine +
                Environment.NewLine +
                "F_char = characteristic rope force from the structural model" + Environment.NewLine +
                Environment.NewLine +
                "F_sher = 1.50 * F_char" + Environment.NewLine +
                "F_Ed = 2.00 * F_sher";

            MessageBox.Show(
                message,
                "Load input recommendation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        }
    }
}
