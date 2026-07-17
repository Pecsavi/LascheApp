using LascheApp.Materials;
using LascheApp.Shackles;
using LascheApp.Padeye;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Windows.Forms;

namespace LascheApp
{
    public partial class Form1 : Form
    {
        private MaterialDatabase? _materialDatabase;
        private ShackleDatabase? _shackleDatabase;
        private PadeyeCheckResult? _lastPadeyeResult;
        private PinCheckResult? _lastPinResult;
        public Form1()
        {
            InitializeComponent();

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
                            ? "WARNING"
                            : "NOT OK";

                string eta =
                    item.ShowUtilization
                        ? item.Utilization.ToString("0.000")
                        : "";

                dgvCheckSummary.Rows.Add(
                    group,
                    item.Name,
                    status,
                    eta);
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
                txtBasicCheckResult.Text =
                    PadeyeTensionLugReportFormatter.Format(
                        tensionPadeyeResult,
                        pinResult,
                    new PadeyeTensionLugReportInfo
                    {
                        Project = "S-1099",
                        Gantry = "NL1",
                        Connection = "Gantry 1 - Gantry 2",
                        PreparedBy = Environment.UserName,
                        Date = DateTime.Today,
                        PlateMaterial = material.Name,
                        PinMaterial = pinMaterial.Name
                    });

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

            double rpl_mm = endDistanceE_mm;

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
                Rpl_mm = rpl_mm,
                Rch_mm = rch_mm,
                CheekPlateWeldA_mm = cheekPlateWeldA_mm,

                IncludeCheekPlatesInBearing = chkIncludeCheekPlatesInBearing.Checked
            };

            PadeyeCheckResult padeyeResult =
                PadeyeChecker.Check(padeyeInput);

            txtBasicCheckResult.Text =
                PadeyeTransportLugReportFormatter.Format(
                    padeyeResult,
                    new PadeyeTransportLugReportInfo
                    {
                        Project = "S-1099",
                        Gantry = "NL1",
                        Connection = "Gantry 1 - Gantry 2",
                        PreparedBy = Environment.UserName,
                        Date = DateTime.Today,
                        PlateMaterial = material.Name,
                        ShackleName = GetSelectedShackle()?.Name ?? ""
                    });
            
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
                row.Cells["Check"].Value?.ToString() ?? "";

            txtSelectedCheckDetail.Text =
                GetSelectedCheckDetail(checkName);
        }
        private string GetSelectedCheckDetail(string checkName)
        {
            string report = txtBasicCheckResult.Text;

            if (string.IsNullOrWhiteSpace(report))
                return "";

            if (checkName.Contains("WLL", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Shackle WLL");

            if (checkName.Contains("clearance", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("Hole diameter", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Hole clearance");

            if (checkName.Contains("B1", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Shackle B1 thickness recommendation");

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

            if (checkName.Contains("angled pull", StringComparison.OrdinalIgnoreCase) ||
                checkName.Contains("DNV", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Bearing pressure - only for angled pull");

            if (checkName.Contains("tear-out", StringComparison.OrdinalIgnoreCase))
                return ExtractReportSection(report, "Tear out - only for angled pull");

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

        "Section values",
        "Pin shear",
        "Pin bending",
        "Pin shear + bending interaction",
        "Replaceable pin service bending"
    };

            int calculationStart = GetCalculationStartIndex(report);

            int start = FindFirstMarkerIndex(
                report,
                startMarkers,
                calculationStart);

            if (start < 0)
                return "Detail section not found in calculation report.";

            int end = report.Length;

            foreach (string marker in sectionMarkers)
            {
                int index = report.IndexOf(
                    marker,
                    start + 1,
                    StringComparison.OrdinalIgnoreCase);

                if (index > start && index < end)
                    end = index;
            }

            return report.Substring(start, end - start).Trim();
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
