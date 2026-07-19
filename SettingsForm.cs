using LascheApp.Materials;
using LascheApp.Shackles;
using System.Globalization;

namespace LascheApp
{
    internal sealed class SettingsForm : Form
    {
        private readonly MaterialDatabase _materials;
        private readonly ShackleDatabase _shackles;
        private readonly bool _german;

        private readonly ComboBox _materialSelector = new();
        private readonly TextBox _materialStandard = new();
        private readonly TextBox _materialComment = new();
        private readonly NumericUpDown _materialE = new();
        private readonly NumericUpDown _materialBetaW = new();
        private readonly DataGridView _ranges = new();
        private readonly DataGridView _shackleGrid = new();
        private bool _loadingMaterial;
        private string? _lastSelectedMaterialId;

        public SettingsForm(
            MaterialDatabase materials,
            ShackleDatabase shackles,
            bool german,
            int selectedDatabaseTab)
        {
            _materials = materials;
            _shackles = shackles;
            _german = german;

            Text = T("Database settings", "Datenbankeinstellungen");
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 620);
            ClientSize = new Size(1120, 700);

            TabControl tabs = new() { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateMaterialTab());
            tabs.TabPages.Add(CreateShackleTab());
            tabs.SelectedIndex = Math.Clamp(selectedDatabaseTab, 0, 1);
            Controls.Add(tabs);

            LoadMaterials();
            LoadShackles();
        }

        private TabPage CreateMaterialTab()
        {
            TabPage tab = new(T("Material database", "Werkstoffdatenbank"));
            TableLayoutPanel editor = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 4,
                RowCount = 7
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            _materialE.Maximum = 1000000;
            _materialE.DecimalPlaces = 0;
            _materialBetaW.Maximum = 10;
            _materialBetaW.DecimalPlaces = 3;
            _materialBetaW.Increment = 0.01M;

            _materialSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            _materialSelector.SelectedIndexChanged += (_, _) => MaterialSelectionChanged();
            AddLabeled(editor, T("Material Id", "Werkstoff-Id"), _materialSelector, 0, 0, 3);
            AddLabeled(editor, T("Standard", "Norm"), _materialStandard, 0, 1, 3);
            AddLabeled(editor, "E [N/mm²]", _materialE, 0, 2);
            AddLabeled(editor, "BetaW", _materialBetaW, 2, 2);
            AddLabeled(editor, T("Comment", "Kommentar"), _materialComment, 0, 3, 3);

            Label rangeLabel = new()
            {
                Text = T("Thickness-dependent properties", "Dickenabhängige Werkstoffwerte"),
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            editor.Controls.Add(rangeLabel, 0, 4);
            editor.SetColumnSpan(rangeLabel, 4);

            ConfigureRangeGrid();
            editor.Controls.Add(_ranges, 0, 5);
            editor.SetColumnSpan(_ranges, 4);

            FlowLayoutPanel actions = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(2)
            };
            actions.Controls.Add(Button(T("Restore defaults", "Originalliste wiederherstellen"), (_, _) => RestoreMaterials(), 185));
            actions.Controls.Add(Button(T("Save material", "Werkstoff speichern"), (_, _) => SaveMaterial(), 135));
            actions.Controls.Add(Button(T("Delete material", "Werkstoff löschen"), (_, _) => DeleteMaterial(), 135));
            editor.Controls.Add(actions, 0, 6);
            editor.SetColumnSpan(actions, 4);
            tab.Controls.Add(editor);
            return tab;
        }

        private TabPage CreateShackleTab()
        {
            TabPage tab = new(T("Shackle database", "Schäkeldatenbank"));
            ConfigureShackleGrid();
            tab.Controls.Add(_shackleGrid);

            FlowLayoutPanel actions = new()
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(4)
            };
            actions.Controls.Add(Button(T("Restore defaults", "Originalliste wiederherstellen"), (_, _) => RestoreShackles(), 185));
            actions.Controls.Add(Button(T("Save database", "Datenbank speichern"), (_, _) => SaveShackles(), 140));
            actions.Controls.Add(Button(T("Delete row", "Zeile löschen"), (_, _) => DeleteShackleRow(), 115));
            actions.Controls.Add(Button(T("Add row", "Zeile hinzufügen"), (_, _) => _shackleGrid.Rows.Add(), 115));
            tab.Controls.Add(actions);
            return tab;
        }

        private void ConfigureRangeGrid()
        {
            _ranges.Dock = DockStyle.Fill;
            _ranges.AllowUserToAddRows = true;
            _ranges.AllowUserToDeleteRows = true;
            _ranges.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _ranges.Columns.Add("Min", "t min [mm]");
            _ranges.Columns.Add("Max", "t max [mm]");
            _ranges.Columns.Add("Fy", "fy [N/mm²]");
            _ranges.Columns.Add("Fu", "fu [N/mm²]");
        }

        private void ConfigureShackleGrid()
        {
            _shackleGrid.Dock = DockStyle.Fill;
            _shackleGrid.AllowUserToAddRows = true;
            _shackleGrid.AllowUserToDeleteRows = true;
            _shackleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            AddShackleColumn("Id", "Id");
            AddShackleColumn("Name", T("Name", "Name"));
            AddShackleColumn("Manufacturer", T("Manufacturer", "Hersteller"));
            AddShackleColumn("Standard", T("Standard", "Norm"));
            AddShackleColumn("NominalSize", T("Nominal size", "Nenngröße"));
            AddShackleColumn("WLL_kg", "WLL [kg]");
            AddShackleColumn("Dpin_mm", "Dpin/d2 [mm]");
            AddShackleColumn("B1_mm", "b1 [mm]");
            AddShackleColumn("B2_mm", "b2 [mm]");
            AddShackleColumn("H2_mm", "h2 [mm]");
            AddShackleColumn("D1_mm", "d1 [mm]");
            AddShackleColumn("D3_mm", "d3 [mm]");
            AddShackleColumn("D4_inch", "d4 [inch]");
            AddShackleColumn("Comment", T("Comment", "Kommentar"));
        }

        private void AddShackleColumn(string name, string header) =>
            _shackleGrid.Columns.Add(name, header);

        private void LoadMaterials(string? selectId = null)
        {
            _loadingMaterial = true;
            try
            {
                _materialSelector.Items.Clear();
                foreach (MaterialGrade material in _materials.Materials.OrderBy(m => m.Name))
                    _materialSelector.Items.Add(material.Id);

                _materialSelector.Items.Add(T("New material…", "Neuer Werkstoff…"));

                string? desiredId = selectId ?? _lastSelectedMaterialId;
                int selectedIndex = -1;
                if (!string.IsNullOrWhiteSpace(desiredId))
                {
                    selectedIndex = _materialSelector.Items.Cast<object>()
                        .Select((item, index) => new { Text = item.ToString(), Index = index })
                        .FirstOrDefault(x => string.Equals(x.Text, desiredId, StringComparison.OrdinalIgnoreCase))?.Index ?? -1;
                }

                _materialSelector.SelectedIndex = selectedIndex >= 0
                    ? selectedIndex
                    : (_materials.Materials.Count > 0 ? 0 : _materialSelector.Items.Count - 1);
            }
            finally
            {
                _loadingMaterial = false;
            }

            MaterialSelectionChanged();
        }

        private void MaterialSelectionChanged()
        {
            if (_loadingMaterial || _materialSelector.SelectedIndex < 0)
                return;

            if (_materialSelector.SelectedIndex == _materialSelector.Items.Count - 1)
            {
                BeginNewMaterial();
                return;
            }

            string id = _materialSelector.SelectedItem?.ToString() ?? "";
            MaterialGrade? material = _materials.TryGetById(id);
            if (material == null)
                return;

            _lastSelectedMaterialId = material.Id;
            _materialStandard.Text = material.Standard;
            _materialComment.Text = material.Comment;
            _materialE.Value = ClampDecimal(material.E_Nmm2, _materialE.Minimum, _materialE.Maximum);
            _materialBetaW.Value = ClampDecimal(material.BetaW, _materialBetaW.Minimum, _materialBetaW.Maximum);
            _ranges.Rows.Clear();
            foreach (MaterialThicknessRange range in material.Ranges.OrderBy(r => r.ThicknessMin_mm))
                _ranges.Rows.Add(range.ThicknessMin_mm, range.ThicknessMax_mm, range.Fy_Nmm2, range.Fu_Nmm2);
        }

        private void BeginNewMaterial()
        {
            string? id = PromptForMaterialId();
            if (string.IsNullOrWhiteSpace(id))
            {
                LoadMaterials(_lastSelectedMaterialId);
                return;
            }

            MaterialGrade? existing = _materials.TryGetById(id);
            if (existing != null)
            {
                LoadMaterials(existing.Id);
                return;
            }

            _loadingMaterial = true;
            try
            {
                _materialSelector.Items.Insert(_materialSelector.Items.Count - 1, id);
                _materialSelector.SelectedItem = id;
            }
            finally
            {
                _loadingMaterial = false;
            }

            _lastSelectedMaterialId = id;
            _materialStandard.Clear();
            _materialComment.Clear();
            _materialE.Value = 210000;
            _materialBetaW.Value = 0.9M;
            _ranges.Rows.Clear();
            _materialStandard.Focus();
        }

        private void SaveMaterial()
        {
            try
            {
                string id = _materialSelector.SelectedItem?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(id) ||
                    _materialSelector.SelectedIndex == _materialSelector.Items.Count - 1)
                    throw new InvalidOperationException(T("Material Id is required.", "Werkstoff-Id ist erforderlich."));

                List<MaterialThicknessRange> ranges = new();
                foreach (DataGridViewRow row in _ranges.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    double min = CellDouble(row, "Min");
                    double max = CellDouble(row, "Max");
                    double fy = CellDouble(row, "Fy");
                    double fu = CellDouble(row, "Fu");
                    if (max <= min || fy <= 0 || fu <= 0)
                        throw new InvalidOperationException(T("Each thickness range requires max > min and positive fy/fu.", "Jeder Dickenbereich benötigt max > min und positive fy/fu-Werte."));
                    ranges.Add(new MaterialThicknessRange
                    {
                        ThicknessMin_mm = min,
                        ThicknessMax_mm = max,
                        Fy_Nmm2 = fy,
                        Fu_Nmm2 = fu
                    });
                }

                if (ranges.Count == 0)
                    throw new InvalidOperationException(T("At least one thickness range is required.", "Mindestens ein Dickenbereich ist erforderlich."));

                if (_materialE.Value <= 0 || _materialBetaW.Value <= 0)
                    throw new InvalidOperationException(T("E and BetaW must be positive.", "E und BetaW müssen positiv sein."));

                List<MaterialThicknessRange> ordered = ranges.OrderBy(r => r.ThicknessMin_mm).ToList();
                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].ThicknessMin_mm < ordered[i - 1].ThicknessMax_mm)
                        throw new InvalidOperationException(T("Thickness ranges must not overlap.", "Dickenbereiche dürfen sich nicht überschneiden."));
                }

                _materials.AddOrUpdate(new MaterialGrade
                {
                    Id = id,
                    Name = _materials.TryGetById(id)?.Name ?? id,
                    Standard = _materialStandard.Text.Trim(),
                    Comment = _materialComment.Text.Trim(),
                    E_Nmm2 = (double)_materialE.Value,
                    BetaW = (double)_materialBetaW.Value,
                    Ranges = ordered
                });
                LoadMaterials(id);
                Info(T("Material saved.", "Werkstoff gespeichert."));
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private void DeleteMaterial()
        {
            string id = _materialSelector.SelectedItem?.ToString() ?? "";
            MaterialGrade? material = _materials.TryGetById(id);
            if (material == null)
            {
                LoadMaterials();
                return;
            }

            if (!Confirm(T($"Delete material '{material.Name}'?", $"Werkstoff '{material.Name}' löschen?")))
                return;

            _materials.Delete(material.Id);
            _lastSelectedMaterialId = null;
            LoadMaterials();
        }

        private void RestoreMaterials()
        {
            if (!Confirm(T("Restore the default material database? All custom materials will be removed.", "Ursprüngliche Werkstoffliste wiederherstellen? Alle eigenen Werkstoffe werden entfernt.")))
                return;
            _materials.RestoreDefaults();
            LoadMaterials();
        }

        private void LoadShackles()
        {
            _shackleGrid.Rows.Clear();
            foreach (ShackleData shackle in _shackles.Shackles.OrderBy(s => s.WLL_kg))
            {
                _shackleGrid.Rows.Add(
                    shackle.Id, shackle.Name, shackle.Manufacturer,
                    shackle.Standard, shackle.NominalSize, shackle.WLL_kg,
                    shackle.Dpin_mm, shackle.B1_mm, shackle.B2_mm, shackle.H2_mm,
                    shackle.D1_mm, shackle.D3_mm, shackle.D4_inch,
                    shackle.Id.StartsWith("HC2_WLL_", StringComparison.OrdinalIgnoreCase)
                        ? "Catalogue data."
                        : shackle.Comment);
            }
        }

        private void SaveShackles()
        {
            try
            {
                List<ShackleData> shackles = new();
                foreach (DataGridViewRow row in _shackleGrid.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    string id = CellText(row, "Id");
                    if (string.IsNullOrWhiteSpace(id))
                        throw new InvalidOperationException(T("Every shackle requires an Id.", "Jeder Schäkel benötigt eine Id."));

                    string name = CellText(row, "Name");
                    if (string.IsNullOrWhiteSpace(name))
                        throw new InvalidOperationException(T($"Shackle '{id}' requires a name.", $"Schäkel '{id}' benötigt einen Namen."));

                    ShackleData? existing = _shackles.TryGetById(id);
                    ShackleData shackle = new()
                    {
                        Id = id,
                        Name = name,
                        Manufacturer = CellText(row, "Manufacturer"),
                        Type = existing?.Type ?? "",
                        Standard = CellText(row, "Standard"),
                        NominalSize = CellText(row, "NominalSize"),
                        WLL_kg = CellDouble(row, "WLL_kg"),
                        Dpin_mm = CellDouble(row, "Dpin_mm"),
                        B1_mm = CellDouble(row, "B1_mm"),
                        B2_mm = CellDouble(row, "B2_mm"),
                        H2_mm = CellDouble(row, "H2_mm"),
                        D1_mm = CellDouble(row, "D1_mm", false),
                        D3_mm = CellDouble(row, "D3_mm", false),
                        D4_inch = CellText(row, "D4_inch"),
                        PinMaterialId = existing?.PinMaterialId ?? "",
                        Comment = CellText(row, "Comment")
                    };
                    if (shackle.WLL_kg <= 0 || shackle.Dpin_mm <= 0 || shackle.B1_mm <= 0 || shackle.B2_mm <= 0 || shackle.H2_mm <= 0)
                        throw new InvalidOperationException(T($"Shackle '{id}' requires positive WLL, Dpin, b1, b2 and h2.", $"Schäkel '{id}' benötigt positive Werte für WLL, Dpin, b1, b2 und h2."));
                    if (shackle.H_DNV_mm <= 0)
                        throw new InvalidOperationException(T($"Shackle '{id}' requires h2 > b2 / 2.", $"Für Schäkel '{id}' muss h2 > b2 / 2 gelten."));
                    shackles.Add(shackle);
                }

                _shackles.ReplaceAll(shackles);
                LoadShackles();
                Info(T("Shackle database saved.", "Schäkeldatenbank gespeichert."));
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private void DeleteShackleRow()
        {
            foreach (DataGridViewRow row in _shackleGrid.SelectedRows.Cast<DataGridViewRow>().ToArray())
            {
                if (!row.IsNewRow)
                    _shackleGrid.Rows.Remove(row);
            }
        }

        private void RestoreShackles()
        {
            if (!Confirm(T("Restore the default shackle database? All custom shackles will be removed.", "Ursprüngliche Schäkelliste wiederherstellen? Alle eigenen Schäkel werden entfernt.")))
                return;
            _shackles.RestoreDefaults();
            LoadShackles();
        }

        private string? PromptForMaterialId()
        {
            using Form prompt = new()
            {
                Text = T("New material", "Neuer Werkstoff"),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(390, 125)
            };
            Label label = new()
            {
                Text = T("Material Id:", "Werkstoff-Id:"),
                AutoSize = true,
                Location = new Point(14, 19)
            };
            TextBox input = new()
            {
                Location = new Point(115, 15),
                Width = 255
            };
            Button ok = new()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(214, 72),
                Width = 75
            };
            Button cancel = new()
            {
                Text = T("Cancel", "Abbrechen"),
                DialogResult = DialogResult.Cancel,
                Location = new Point(295, 72),
                Width = 75
            };
            prompt.Controls.AddRange(new Control[] { label, input, ok, cancel });
            prompt.AcceptButton = ok;
            prompt.CancelButton = cancel;
            prompt.Shown += (_, _) => input.Focus();

            return prompt.ShowDialog(this) == DialogResult.OK
                ? input.Text.Trim()
                : null;
        }

        private static void AddLabeled(TableLayoutPanel panel, string text, Control control, int column, int row, int span = 1)
        {
            Label label = new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(label, column, row);
            panel.Controls.Add(control, column + 1, row);
            if (span > 1)
                panel.SetColumnSpan(control, span);
        }

        private static Button Button(string text, EventHandler click, int width = 90)
        {
            Button button = new() { Text = text, Width = width, Height = 30 };
            button.Click += click;
            return button;
        }

        private static decimal ClampDecimal(double value, decimal min, decimal max) =>
            Math.Clamp((decimal)value, min, max);

        private static string CellText(DataGridViewRow row, string column) =>
            row.Cells[column].Value?.ToString()?.Trim() ?? "";

        private static double CellDouble(DataGridViewRow row, string column, bool required = true)
        {
            string text = CellText(row, column).Replace(',', '.');
            if (string.IsNullOrWhiteSpace(text) && !required)
                return 0.0;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                throw new InvalidOperationException($"Invalid number in column '{row.DataGridView?.Columns[column].HeaderText}'.");
            return value;
        }

        private bool Confirm(string message) =>
            MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

        private void Info(string message) =>
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void Error(string message) =>
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

        private string T(string english, string german) => _german ? german : english;
    }
}
