using LascheApp.Materials;
using LascheApp.Shackles;
using System.Linq;
using System.IO;

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
            UpdateSelectedShackleInfo();
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
    }
}
