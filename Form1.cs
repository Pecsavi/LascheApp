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

        private void btnTestSelectedShackle_Click(object sender, EventArgs e)
        {
            bool ok = TryGetSelectedShackleGeometry(
                out double dpin_mm,
                out double b1_mm,
                out double hDnv_mm,
                out double wll_kN);

            if (!ok)
            {
                MessageBox.Show("No shackle selected.");
                return;
            }

            MessageBox.Show(
                $"Selected shackle geometry:\n\n" +
                $"Dpin = {dpin_mm:0.0} mm\n" +
                $"B1 = {b1_mm:0.0} mm\n" +
                $"H_DNV = {hDnv_mm:0.0} mm\n" +
                $"WLL = {wll_kN:0.00} kN");
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

            // User input is e = distance from plate end to hole centre.
            // The EC geometry checker still uses the internal values a and c:
            // a = e - d0 / 2
            // c = (b - d0) / 2
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

                if (!TryReadDouble(txtPinMoment_kNmm.Text, out double pinMoment_kNmm))
                {
                    txtBasicCheckResult.Text = "Invalid input: M_Ed [kNmm]";
                    return;
                }

                if (!TryReadDouble(txtPinMomentSer_kNmm.Text, out double pinMomentSer_kNmm))
                {
                    txtBasicCheckResult.Text = "Invalid input: M_Ed,ser [kNmm]";
                    return;
                }

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

                txtBasicCheckResult.Text = FormatTensionLugCheckResult(
                    tensionPadeyeResult,
                    pinResult,
                    material.Name,
                    pinMaterial.Name);

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

                PinClearance_mm = 2.0,

                IsReplaceablePin = chkReplaceablePin.Checked,

                DnvOutOfPlaneAngle_deg = dnvAlpha_deg,
                DnvBeta = dnvBeta,
                DnvGammaM = 1.15,
                GammaM2 = 1.25,

                CheekPlateThickness_mm = cheekPlateThickness_mm,
                Rpl_mm = rpl_mm,
                Rch_mm = rch_mm,
                CheekPlateWeldA_mm = cheekPlateWeldA_mm
            };

            PadeyeCheckResult padeyeResult =
                PadeyeChecker.Check(padeyeInput);

            txtBasicCheckResult.Text =
                FormatPadeyeCheckResult(padeyeResult);

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
        private string FormatLugType(LugType lugType)
        {
            return lugType switch
            {
                LugType.TransportLug => "Transport Lug",
                LugType.TensionLug => "Tension Lug",
                _ => lugType.ToString()
            };
        }
        private string FormatTensionLugCheckResult(
            PadeyeCheckResult plateResult,
            PinCheckResult pinResult,
            string plateMaterialName,
            string pinMaterialName)
        {
            bool overallOk = plateResult.IsOk && pinResult.IsOk;

            List<CheckItem> allItems = new List<CheckItem>();

            if (!plateResult.BasicResult.HasErrors)
                allItems.AddRange(plateResult.BasicResult.CheckItems);

            if (!plateResult.EcGeometryResult.HasErrors)
                allItems.AddRange(plateResult.EcGeometryResult.SummaryCheckItems);

            if (!plateResult.BearingResult.HasErrors)
                allItems.AddRange(plateResult.BearingResult.CheckItems);

            if (!pinResult.HasErrors)
                allItems.AddRange(pinResult.CheckItems);

            List<CheckItem> utilizationItems = allItems
                .Where(i => i.ShowUtilization)
                .ToList();

            double maxUtilization = utilizationItems.Count == 0
                ? 0.0
                : utilizationItems.Max(i => i.Utilization);

            string governingCheckName = utilizationItems.Count == 0
                ? ""
                : utilizationItems.OrderByDescending(i => i.Utilization).First().Name;

            string text =
                "Tension Lug verification\n" +
                "========================\n" +
                $"Overall result: {(overallOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {maxUtilization:0.000}\n" +
                $"Governing check: {governingCheckName}\n\n" +
                $"Plate material = {plateMaterialName}\n" +
                $"Pin material = {pinMaterialName}\n\n" +
                FormatTensionLugCheckSummary(allItems) +
                Environment.NewLine +
                Environment.NewLine +
                "Plate verification\n" +
                "==================\n\n" +
                FormatPadeyeBasicCheckResult(plateResult.BasicResult) +
                Environment.NewLine +
                Environment.NewLine +
                FormatPadeyeEcGeometryResult(plateResult.EcGeometryResult) +
                Environment.NewLine +
                Environment.NewLine +
                FormatPadeyeBearingResult(plateResult.BearingResult) +
                Environment.NewLine +
                Environment.NewLine +
                FormatPinCheckResult(pinResult);

            return text;
        }

        private string FormatTensionLugCheckSummary(List<CheckItem> items)
        {
            return FormatGroupedCheckSummary(items);
        }

        private string FormatGroupedCheckSummary(IEnumerable<CheckItem> items)
        {
            List<CheckItem> geometryItems = items
                .Where(i => !i.ShowUtilization)
                .ToList();

            List<CheckItem> utilizationItems = items
                .Where(i => i.ShowUtilization)
                .OrderByDescending(i => i.Utilization)
                .ToList();

            string text =
                "Check summary\n" +
                "-------------\n";

            if (geometryItems.Count > 0)
            {
                text +=
                    "\nGeometry checks\n" +
                    "---------------\n";

                foreach (CheckItem item in geometryItems)
                    text += FormatGeometrySummaryLine(item) + Environment.NewLine;
            }

            if (utilizationItems.Count > 0)
            {
                text +=
                    "\nUtilization checks\n" +
                    "------------------\n";

                foreach (CheckItem item in utilizationItems)
                    text += FormatUtilizationSummaryLine(item) + Environment.NewLine;
            }

            return text.TrimEnd();
        }

        private string FormatGeometrySummaryLine(CheckItem item)
        {
            string status = item.IsOk ? "OK" : "NOT OK";

            return $"{status,-7}         {item.Name}";
        }

        private string FormatUtilizationSummaryLine(CheckItem item)
        {
            string status = item.IsOk ? "OK" : "NOT OK";

            return $"{status,-7} η = {item.Utilization:0.000}  {item.Name}";
        }

        private string FormatPadeyeCheckResult(PadeyeCheckResult result)
        {
            string text =
                FormatPadeyeOverallResult(result) +
                Environment.NewLine +
                Environment.NewLine +
                FormatCheckSummary(result) +
                Environment.NewLine +
                Environment.NewLine +
                FormatPadeyeBasicCheckResult(result.BasicResult) +
                Environment.NewLine +
                Environment.NewLine +
                FormatPadeyeEcGeometryResult(result.EcGeometryResult);

            if (result.DnvOutOfPlaneCheckRequired)
            {
                text +=
                    Environment.NewLine +
                    Environment.NewLine +
                    FormatPadeyeDnvOutOfPlaneResult(result.DnvOutOfPlaneResult);
            }

            text +=
                Environment.NewLine +
                Environment.NewLine +
                FormatPadeyeBearingResult(result.BearingResult);

            return text;
        }
        private string FormatPadeyeDnvOutOfPlaneResult(PadeyeDnvOutOfPlaneResult result)
        {
            PadeyeDnvOutOfPlaneInput input = result.Input;

            if (!result.IsActive)
            {
                return
                    "DNV out-of-plane checks\n" +
                    "-----------------------\n" +
                    "Not active.";
            }

            if (result.HasErrors)
            {
                return
                    "DNV out-of-plane checks\n" +
                    "-----------------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }

            string text =
                "DNV out-of-plane checks\n" +
                "-----------------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}\n\n" +

                "Load decomposition\n" +
                "------------------\n" +
                $"Fds = {input.F_Ed_kN:0.00} kN\n" +
                $"alpha = {input.Alpha_deg:0.0}°\n" +
                $"Fd = Fds * cos(alpha) = {result.Fd_kN:0.00} kN\n" +
                $"Fdl = Fds * sin(alpha) = {result.Fdl_kN:0.00} kN\n" +
                $"h = {input.H_DNV_mm:0.0} mm\n" +
                $"Me = Fdl * h = {result.Me_kNm:0.00} kNm\n\n" +

                "DNV parameters\n" +
                "--------------\n" +
                $"Dpin = {input.Dpin_mm:0.0} mm\n" +
                $"DH = {input.HoleDiameter_mm:0.0} mm\n" +
                $"tpl = {input.MainPlateThickness_mm:0.0} mm\n" +
                $"tch = {input.CheekPlateThickness_mm:0.0} mm\n" +
                $"t = tpl + 2 * tch = {input.TotalThickness_mm:0.0} mm\n" +
                $"Rpl = {input.Rpl_mm:0.0} mm\n" +
                $"Rch = {input.Rch_mm:0.0} mm\n" +
                $"Rpad = (Rpl * tpl + 2 * Rch * tch) / t = {result.Rpad_mm:0.0} mm\n" +
                $"beta = {input.Beta:0.000}\n" +
                $"delta = 4 * tan(alpha) * h / t + 1 = {result.Delta:0.000}\n" +
                $"beta,eff = {(result.Delta >= 1.3 ? "beta * (delta - 0.3)" : "beta")} = {result.BetaEffective:0.000}\n" +
                $"sigma_Rd = fy / gammaM = {result.SigmaRd_Nmm2:0.0} N/mm²";

            if (result.OutOfPlaneChecksActive)
            {
                text +=
                    "\n\nDNV out-of-plane bearing\n" +
                    "------------------------\n" +
                    $"Dpin / DH = {result.DpinToHoleRatio:0.000}\n" +
                    $"Formula branch: {(result.BearingFormulaWithClearance ? "Dpin / DH < 0.96" : "Dpin / DH >= 0.96")}\n" +
                    $"sigma_Ed,1 = {result.SigmaEd1_Nmm2:0.0} N/mm²\n" +
                    $"Check sigma_Ed,1 <= sigma_Rd: {(result.DnvBearingOk ? "OK" : "NOT OK")}  η = {result.DnvBearingUtilization:0.000}\n\n" +

                    "DNV tear-out\n" +
                    "------------\n" +
                    $"sigma_Ed,2 = 1.7 * Fd / ((2 * Rpl - DH) * t) = {result.SigmaEd2_Nmm2:0.0} N/mm²\n" +
                    $"Check sigma_Ed,2 <= sigma_Rd: {(result.TearOutOk ? "OK" : "NOT OK")}  η = {result.TearOutUtilization:0.000}";
            }

            if (result.CheekPlateWeldCheckActive)
            {
                text +=
                    "\n\nCheek plate weld check\n" +
                    "----------------------\n" +
                    $"DCH = 2 * Rch = {result.Dch_mm:0.0} mm\n" +
                    $"a = {input.WeldA_mm:0.0} mm\n" +
                    $"sigma_Ed,3 = Fd * tch / (1.5 * t * DCH * a) * delta = {result.SigmaEd3_Nmm2:0.0} N/mm²\n" +
                    $"fvwd = fu / (sqrt(3) * betaW * gammaM2) = {result.Fvwd_Nmm2:0.0} N/mm²\n" +
                    $"Check sigma_Ed,3 <= fvwd: {(result.CheekPlateWeldOk ? "OK" : "NOT OK")}  η = {result.CheekPlateWeldUtilization:0.000}";
            }

            return text;
        }

        private string FormatPadeyeBearingResult(PadeyeBearingResult result)
        {
            PadeyeBearingInput input = result.Input;

            if (result.HasErrors)
            {
                return
                    "Pin-hole bearing check\n" +
                    "----------------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }

            string text =
                $"Pin-hole bearing check\n" +
                $"----------------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}\n" +
                $"Replaceable pin checks: {(input.IsReplaceablePin ? "active" : "not active")}\n\n" +

                $"Pin-hole bearing design\n" +
                $"-----------------------\n" +
                $"Fb,Ed = {input.F_Ed_kN:0.00} kN\n" +
                $"Fb,Rd = 1.5 * t * d * fy / gammaM0 = {result.FbRd_kN:0.00} kN\n" +
                $"Check Fb,Ed <= Fb,Rd: {(result.BearingDesignOk ? "OK" : "NOT OK")}  η = {result.BearingDesignUtilization:0.000}";

            if (input.IsReplaceablePin)
            {
                text +=
                    $"\n\nReplaceable pin service bearing\n" +
                    $"-------------------------------\n" +
                    $"Fb,Ed,ser = {input.F_Ed_ser_kN:0.00} kN\n" +
                    $"Fb,Rd,ser = 0.6 * t * d * fy / gammaM6,ser = {result.FbRdSer_kN:0.00} kN\n" +
                    $"Check Fb,Ed,ser <= Fb,Rd,ser: {(result.BearingServiceOk ? "OK" : "NOT OK")}  η = {result.BearingServiceUtilization:0.000}\n\n" +

                    $"Replaceable pin contact stress\n" +
                    $"------------------------------\n" +
                    $"d0 = {input.HoleDiameter_mm:0.0} mm\n" +
                    $"d = {input.PinDiameter_mm:0.0} mm\n" +
                    $"E = {input.E_Nmm2:0.0} N/mm²\n" +
                    $"sigma_h,Ed = 0.591 * sqrt(E * Fb,Ed,ser * (d0 - d) / (d² * t)) = {result.SigmaHEd_Nmm2:0.0} N/mm²\n" +
                    $"fh,Rd = 2.5 * fy / gammaM6,ser = {result.FhRd_Nmm2:0.0} N/mm²\n" +
                    $"Check sigma_h,Ed <= fh,Rd: {(result.HolePinStressOk ? "OK" : "NOT OK")}  η = {result.HolePinStressUtilization:0.000}";
            }

            return text;
        }

        private string FormatPadeyeOutOfPlaneResult(PadeyeOutOfPlaneResult result)
        {
            PadeyeOutOfPlaneInput input = result.Input;
            if (result.HasErrors)
            {
                return
                    "Out-of-plane bending check\n" +
                    "--------------------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }

            return
                $"Out-of-plane bending check\n" +
                $"--------------------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}\n\n" +

                $"F_Ed = {input.F_Ed_kN:0.00} kN\n" +
                $"H_DNV = {input.H_DNV_mm:0.0} mm\n" +
                $"M_Ed = F_Ed * H_DNV = {result.M_Ed_Nmm:0.0} Nmm\n\n" +

                $"b = {input.PlateWidth_mm:0.0} mm\n" +
                $"t = {input.PlateThickness_mm:0.0} mm\n" +
                $"W = b * t² / 6 = {result.SectionModulus_mm3:0.0} mm³\n\n" +

                $"Sigma_bending,Ed = M_Ed / W = {result.SigmaBendingEd_Nmm2:0.0} N/mm²\n" +
                $"Sigma_Rd = fy / gammaM0 = {result.SigmaRd_Nmm2:0.0} N/mm²\n" +
                $"Check Sigma_bending,Ed <= Sigma_Rd: {(result.BendingOk ? "OK" : "NOT OK")}  η = {result.BendingUtilization:0.000}";
        }
        private string FormatCheckSummary(PadeyeCheckResult result)
        {
            return FormatGroupedCheckSummary(result.GoverningCheckItems);
        }
        private string FormatPadeyeOverallResult(PadeyeCheckResult result)
        {
            return
                $"Padeye overall result\n" +
                $"=====================\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}";
        }

        private string FormatPadeyeEcGeometryResult(PadeyeEcGeometryResult result)
        {
            PadeyeEcGeometryInput input = result.Input;
            if (result.HasErrors)
            {
                return
                    "EC geometry check\n" +
                    "-----------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }
            double e_mm = input.EdgeDistanceA_mm + input.HoleDiameter_mm / 2.0;
            double requiredE_mm = result.RequiredEdgeDistanceA_mm + input.HoleDiameter_mm / 2.0;

            double b_mm = 2.0 * input.SideDistanceC_mm + input.HoleDiameter_mm;
            double requiredB_mm = 2.0 * result.RequiredSideDistanceC_mm + input.HoleDiameter_mm;

            return
                $"EC geometry check\n" +
                $"-----------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n\n" +
                $"Governing check: {result.GoverningCheckName}\n\n" +

                $"Method A\n" +
                $"-------------\n" +
                $"Result Method A: {(result.MoglichkeitA_Ok ? "OK" : "NOT OK")}\n" +
                $"Max utilization A: η = {result.MoglichkeitA_MaxUtilization:0.000}\n\n" +
                $"F_Ed = {input.F_Ed_kN:0.00} kN\n" +
                $"fy = {input.Fy_Nmm2:0.0} N/mm²\n" +
                $"gammaM0 = {input.GammaM0:0.00}\n" +
                $"t = {input.PlateThickness_mm:0.0} mm\n" +
                $"d0 = {input.HoleDiameter_mm:0.0} mm\n\n" +

                $"e = {e_mm:0.0} mm\n" +
                $"a = e - d0 / 2 = {input.EdgeDistanceA_mm:0.0} mm\n" +
                $"Required e = required a + d0 / 2 = {requiredE_mm:0.0} mm\n" +
                $"Check e >= required e: {(result.EdgeDistanceA_Ok ? "OK" : "NOT OK")}  η = {result.EdgeDistanceA_Utilization:0.000}\n\n" +

                $"b = {b_mm:0.0} mm\n" +
                $"c = (b - d0) / 2 = {input.SideDistanceC_mm:0.0} mm\n" +
                $"Required b = 2 * required c + d0 = {requiredB_mm:0.0} mm\n" +
                $"Check b >= required b: {(result.SideDistanceC_Ok ? "OK" : "NOT OK")}  η = {result.SideDistanceC_Utilization:0.000}\n\n" +

                $"Method B\n" +
                $"-------------\n" +
                $"Result Method B: {(result.MoglichkeitB_Ok ? "OK" : "NOT OK")}\n" +
                $"Max utilization B: η = {result.MoglichkeitB_MaxUtilization:0.000}\n\n" +
                $"t = {input.PlateThickness_mm:0.0} mm\n" +
                $"Required t = 0.7 * sqrt(F_Ed * gammaM0 / fy) = {result.RequiredThickness_MoglichkeitB_mm:0.0} mm\n" +
                $"Check t >= required t: {(result.ThicknessMoglichkeitB_Ok ? "OK" : "NOT OK")}  η = {result.ThicknessMoglichkeitB_Utilization:0.000}\n\n" +

                $"d0 = {input.HoleDiameter_mm:0.0} mm\n" +
                $"Max d0 = 2.5 * t = {result.MaxHoleDiameter_MoglichkeitB_mm:0.0} mm\n" +
                $"Check d0 <= 2.5 * t: {(result.HoleDiameterMoglichkeitB_Ok ? "OK" : "NOT OK")}  η = {result.HoleDiameterMoglichkeitB_Utilization:0.000}";
        }
        private string FormatPadeyeBasicCheckResult(PadeyeBasicCheckResult result)
        {
            PadeyeBasicCheckInput input = result.Input;
            if (result.HasErrors)
            {
                return
                    "Basic padeye check\n" +
                    "------------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }

            string text =
                $"Basic padeye check\n" +
                $"------------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}\n\n" +
                $"F_Ed = {input.F_Ed_kN:0.00} kN";

            if (input.LugType == LugType.TransportLug)
            {
                text +=
                    $"\nF_Ed,ser = {input.F_Ed_ser_kN:0.00} kN\n" +
                    $"WLL = {input.ShackleWLL_kN:0.00} kN\n" +
                    $"Check F_Ed,ser <= WLL: {(result.WllOk ? "OK" : "NOT OK")}  η = {result.WllUtilization:0.000}\n\n" +

                    $"Dpin = {input.ShackleDpin_mm:0.0} mm\n" +
                    $"d0 = {input.HoleDiameter_mm:0.0} mm\n" +
                    $"Required d0 = Dpin + {input.PinClearance_mm:0.0} mm = {result.RequiredHoleDiameter_mm:0.0} mm\n" +
                    $"Check d0 >= Dpin + clearance: {(result.HoleDiameterOk ? "OK" : "NOT OK")}\n\n" +

                    $"B1 = {input.ShackleB1_mm:0.0} mm\n" +
                    $"t = {input.PlateThickness_mm:0.0} mm\n" +
                    $"Required t = 0.75 * B1 = {result.RequiredThickness_mm:0.0} mm\n" +
                    $"Check t >= 0.75 * B1: {(result.ThicknessOk ? "OK" : "NOT OK")}  η = {result.ThicknessUtilization:0.000}\n\n" +

                    $"H_DNV = {input.ShackleH_DNV_mm:0.0} mm";
            }

            text +=
                $"\n\nGross section tension check\n" +
                $"---------------------------\n" +
                $"b = {input.PlateWidth_mm:0.0} mm\n" +
                $"t = {input.PlateThickness_mm:0.0} mm\n" +
                $"A_gross = b * t = {result.GrossArea_mm2:0.0} mm²\n" +
                $"Sigma_gross,Ed = F_Ed / A_gross = {result.SigmaGrossEd_Nmm2:0.0} N/mm²\n" +
                $"Sigma_Rd = fy / gammaM0 = {result.SigmaRd_Nmm2:0.0} N/mm²\n" +
                $"Check Sigma_gross,Ed <= Sigma_Rd: {(result.GrossSectionTensionOk ? "OK" : "NOT OK")}  η = {result.GrossSectionTensionUtilization:0.000}" +
                $"\n\nNet section tension check\n" +
                $"-------------------------\n" +
                $"b = {input.PlateWidth_mm:0.0} mm\n" +
                $"d0 = {input.HoleDiameter_mm:0.0} mm\n" +
                $"A_net = (b - d0) * t = {result.NetArea_mm2:0.0} mm²\n" +
                $"Sigma_Ed = F_Ed / A_net = {result.SigmaEd_Nmm2:0.0} N/mm²\n" +
                $"Sigma_Rd = fy / gammaM0 = {result.SigmaRd_Nmm2:0.0} N/mm²\n" +
                $"Check Sigma_Ed <= Sigma_Rd: {(result.NetSectionTensionOk ? "OK" : "NOT OK")}  η = {result.NetSectionTensionUtilization:0.000}";

            return text;
        }
        private string FormatPinCheckResult(PinCheckResult result)
        {
            if (result.HasErrors)
            {
                return
                    "Pin verification\n" +
                    "----------------\n" +
                    "Input error\n\n" +
                    string.Join(Environment.NewLine, result.Errors);
            }

            PinCheckInput input = result.Input;

            string text =
                "Pin verification\n" +
                "----------------\n" +
                $"Overall result: {(result.IsOk ? "OK" : "NOT OK")}\n" +
                $"Max utilization: η = {result.MaxUtilization:0.000}\n" +
                $"Governing check: {result.GoverningCheckName}\n" +
                $"Replaceable pin checks: {(input.IsReplaceablePin ? "active" : "not active")}\n\n" +

                "Input\n" +
                "-----\n" +
                $"F_Ed = {input.F_Ed_kN:0.00} kN\n" +
                $"F_Ed,ser = {input.F_Ed_ser_kN:0.00} kN\n" +
                $"M_Ed = {input.M_Ed_kNmm:0.00} · 10⁻³ kNm\n" +
                $"M_Ed,ser = {input.M_Ed_ser_kNmm:0.00} · 10⁻³ kNm\n" +
                $"Pin diameter d = {input.PinDiameter_mm:0.0} mm\n" +
                $"fy,p = {input.PinFy_Nmm2:0.0} N/mm²\n" +
                $"fu,p = {input.PinFu_Nmm2:0.0} N/mm²\n" +
                $"gammaM0 = {input.GammaM0:0.00}\n" +
                $"gammaM2 = {input.GammaM2:0.00}\n" +
                $"gammaM6,ser = {input.GammaM6_ser:0.00}\n\n" +

                "Section values\n" +
                "--------------\n" +
                $"A = π * d² / 4 = {result.Area_mm2:0.0} mm²\n" +
                $"Wel = π * d³ / 32 = {result.SectionModulus_mm3:0.0} mm³\n\n" +

                "Pin shear\n" +
                "---------\n" +
                $"Fv,Ed = F_Ed / 2 = {result.FvEd_kN:0.00} kN\n" +
                $"Fv,Rd = 0.6 * A * fu,p / gammaM2 = {result.FvRd_kN:0.00} kN\n" +
                $"Check Fv,Ed <= Fv,Rd: {(result.ShearOk ? "OK" : "NOT OK")}  η = {result.ShearUtilization:0.000}\n\n" +

                "Pin bending\n" +
                "-----------\n" +
                $"M_Ed = {result.MEd_kNmm:0.00} · 10⁻³ kNm\n" +
                $"M_Rd = 1.5 * Wel * fy,p / gammaM0 = {result.MRd_kNmm:0.00} · 10⁻³ kNm\n" +
                $"Check M_Ed <= M_Rd: {(result.BendingOk ? "OK" : "NOT OK")}  η = {result.BendingUtilization:0.000}\n\n" +

                "Pin shear + bending interaction\n" +
                "-------------------------------\n" +
                $"η = (M_Ed / M_Rd)² + (Fv,Ed / Fv,Rd)² = {result.CombinedUtilization:0.000}\n" +
                $"Check η <= 1.000: {(result.CombinedOk ? "OK" : "NOT OK")}";

            if (input.IsReplaceablePin)
            {
                text +=
                    "\n\nReplaceable pin service bending\n" +
                    "-------------------------------\n" +
                    $"M_Ed,ser = {result.MEdSer_kNmm:0.00} · 10⁻³ kNm\n" +
                    $"M_Rd,ser = 0.8 * Wel * fy,p / gammaM6,ser = {result.MRdSer_kNmm:0.00} · 10⁻³ kNm\n" +
                    $"Check M_Ed,ser <= M_Rd,ser: {(result.ServiceBendingOk ? "OK" : "NOT OK")}  η = {result.ServiceBendingUtilization:0.000}";
            }

            return text;
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
    }
}
