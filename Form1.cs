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
            txtRpl_mm.Visible = isTransportLug;
            label14.Visible = isTransportLug;
            txtRch_mm.Visible = isTransportLug;
            label16.Visible = isTransportLug;
            txtCheekPlateWeldA_mm.Visible = isTransportLug;
            label17.Visible = isTransportLug;

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
                .OrderBy(m => m.Name)
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

            if (!TryReadDouble(txtLoad_kN.Text, out double fEd_kN))
            {
                txtBasicCheckResult.Text = "Invalid input: F_Ed [kN]";
                return;
            }

            if (!TryReadDouble(txtLoadSer_kN.Text, out double fEdSer_kN))
            {
                txtBasicCheckResult.Text = "Invalid input: F_Ed,ser [kN]";
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

            if (!TryReadOptionalDoubleControl("txtRpl_mm", 0.0, out double rpl_mm, out string rplError))
            {
                txtBasicCheckResult.Text = rplError;
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
                MessageBox.Show("Invalid input: F_Ed,ser [kN]");
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
                .OrderBy(m => m.Name)
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

    }
}
