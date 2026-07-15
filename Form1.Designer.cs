namespace LascheApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmbShackles = new ComboBox();
            lblShackleWll = new Label();
            lblShackleDpin = new Label();
            lblShackleB1 = new Label();
            lblShackleHDnv = new Label();
            lblShackleInfo = new Label();
            txtLoad_kN = new TextBox();
            txtPlateThickness_mm = new TextBox();
            txtHoleDiameter_mm = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnCheckBasicPadeye = new Button();
            txtBasicCheckResult = new TextBox();
            btnSelectShackleByLoad = new Button();
            cmbMaterials = new ComboBox();
            lblMaterialBetaW = new Label();
            lblMaterialE = new Label();
            lblMaterialFu = new Label();
            lblMaterialFy = new Label();
            txtPlateWidth_mm = new TextBox();
            label4 = new Label();
            label5 = new Label();
            txtEdgeDistanceA_mm = new TextBox();
            txtLoadSer_kN = new TextBox();
            label7 = new Label();
            chkReplaceablePin = new CheckBox();
            cmbLugType = new ComboBox();
            label8 = new Label();
            txtTensionPinDiameter_mm = new TextBox();
            label9 = new Label();
            cmbPinMaterials = new ComboBox();
            label10 = new Label();
            lblPinFy = new Label();
            lblPinFu = new Label();
            txtDnvOutOfPlaneAngle_deg = new TextBox();
            label13 = new Label();
            txtCheekPlateThickness_mm = new TextBox();
            label15 = new Label();
            txtRch_mm = new TextBox();
            label16 = new Label();
            txtCheekPlateWeldA_mm = new TextBox();
            label17 = new Label();
            chkIncludeCheekPlatesInBearing = new CheckBox();
            richTextBox1 = new RichTextBox();
            label6 = new Label();
            lblGapS = new Label();
            txtGapS_mm = new TextBox();
            lblOuterLugThicknessT2 = new Label();
            txtOuterLugThicknessT2_mm = new TextBox();
            grpLoads = new GroupBox();
            grpLugGeometry = new GroupBox();
            grpTransportLug = new GroupBox();
            grpTensionLug = new GroupBox();
            groupBox5 = new GroupBox();
            grpLoads.SuspendLayout();
            grpLugGeometry.SuspendLayout();
            grpTransportLug.SuspendLayout();
            grpTensionLug.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // cmbShackles
            // 
            cmbShackles.FormattingEnabled = true;
            cmbShackles.Location = new Point(109, 90);
            cmbShackles.Name = "cmbShackles";
            cmbShackles.Size = new Size(93, 23);
            cmbShackles.TabIndex = 0;
            cmbShackles.SelectedIndexChanged += cmbShackles_SelectedIndexChanged;
            // 
            // lblShackleWll
            // 
            lblShackleWll.AutoSize = true;
            lblShackleWll.Location = new Point(23, 96);
            lblShackleWll.Name = "lblShackleWll";
            lblShackleWll.Size = new Size(64, 15);
            lblShackleWll.TabIndex = 1;
            lblShackleWll.Text = "ShackleWll";
            // 
            // lblShackleDpin
            // 
            lblShackleDpin.AutoSize = true;
            lblShackleDpin.Location = new Point(25, 126);
            lblShackleDpin.Name = "lblShackleDpin";
            lblShackleDpin.Size = new Size(108, 15);
            lblShackleDpin.TabIndex = 2;
            lblShackleDpin.Text = "Shackle Dpin [mm]";
            // 
            // lblShackleB1
            // 
            lblShackleB1.AutoSize = true;
            lblShackleB1.Location = new Point(25, 158);
            lblShackleB1.Name = "lblShackleB1";
            lblShackleB1.Size = new Size(96, 15);
            lblShackleB1.TabIndex = 3;
            lblShackleB1.Text = "Shackle B1 [mm]";
            // 
            // lblShackleHDnv
            // 
            lblShackleHDnv.AutoSize = true;
            lblShackleHDnv.Location = new Point(25, 190);
            lblShackleHDnv.Name = "lblShackleHDnv";
            lblShackleHDnv.Size = new Size(121, 15);
            lblShackleHDnv.TabIndex = 3;
            lblShackleHDnv.Text = "Shackle H_DNV [mm]";
            // 
            // lblShackleInfo
            // 
            lblShackleInfo.AutoSize = true;
            lblShackleInfo.Location = new Point(25, 222);
            lblShackleInfo.Name = "lblShackleInfo";
            lblShackleInfo.Size = new Size(71, 15);
            lblShackleInfo.TabIndex = 3;
            lblShackleInfo.Text = "Shackle Info";
            // 
            // txtLoad_kN
            // 
            txtLoad_kN.Location = new Point(100, 120);
            txtLoad_kN.Name = "txtLoad_kN";
            txtLoad_kN.Size = new Size(100, 23);
            txtLoad_kN.TabIndex = 5;
            txtLoad_kN.Text = "1400";
            // 
            // txtPlateThickness_mm
            // 
            txtPlateThickness_mm.Location = new Point(98, 25);
            txtPlateThickness_mm.Name = "txtPlateThickness_mm";
            txtPlateThickness_mm.Size = new Size(69, 23);
            txtPlateThickness_mm.TabIndex = 6;
            txtPlateThickness_mm.Text = "60";
            txtPlateThickness_mm.TextChanged += txtPlateThickness_mm_TextChanged;
            // 
            // txtHoleDiameter_mm
            // 
            txtHoleDiameter_mm.AcceptsReturn = true;
            txtHoleDiameter_mm.Location = new Point(98, 57);
            txtHoleDiameter_mm.Name = "txtHoleDiameter_mm";
            txtHoleDiameter_mm.Size = new Size(69, 23);
            txtHoleDiameter_mm.TabIndex = 7;
            txtHoleDiameter_mm.Text = "32";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 124);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 3;
            label1.Text = "F_Ed [kN]";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 28);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 3;
            label2.Text = "tpl [mm]";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 60);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 3;
            label3.Text = "d0 [mm]";
            // 
            // btnCheckBasicPadeye
            // 
            btnCheckBasicPadeye.Location = new Point(15, 22);
            btnCheckBasicPadeye.Name = "btnCheckBasicPadeye";
            btnCheckBasicPadeye.Size = new Size(177, 23);
            btnCheckBasicPadeye.TabIndex = 4;
            btnCheckBasicPadeye.Text = "Verify lug";
            btnCheckBasicPadeye.UseVisualStyleBackColor = true;
            btnCheckBasicPadeye.Click += btnCheckBasicPadeye_Click;
            // 
            // txtBasicCheckResult
            // 
            txtBasicCheckResult.Location = new Point(15, 61);
            txtBasicCheckResult.Multiline = true;
            txtBasicCheckResult.Name = "txtBasicCheckResult";
            txtBasicCheckResult.ReadOnly = true;
            txtBasicCheckResult.ScrollBars = ScrollBars.Vertical;
            txtBasicCheckResult.Size = new Size(477, 814);
            txtBasicCheckResult.TabIndex = 8;
            // 
            // btnSelectShackleByLoad
            // 
            btnSelectShackleByLoad.Location = new Point(23, 58);
            btnSelectShackleByLoad.Name = "btnSelectShackleByLoad";
            btnSelectShackleByLoad.Size = new Size(177, 23);
            btnSelectShackleByLoad.TabIndex = 4;
            btnSelectShackleByLoad.Text = "Select shackle by load";
            btnSelectShackleByLoad.UseVisualStyleBackColor = true;
            btnSelectShackleByLoad.Click += btnSelectShackleByLoad_Click;
            // 
            // cmbMaterials
            // 
            cmbMaterials.FormattingEnabled = true;
            cmbMaterials.Location = new Point(100, 71);
            cmbMaterials.Name = "cmbMaterials";
            cmbMaterials.Size = new Size(111, 23);
            cmbMaterials.TabIndex = 9;
            cmbMaterials.SelectedIndexChanged += cmbMaterials_SelectedIndexChanged;
            // 
            // lblMaterialBetaW
            // 
            lblMaterialBetaW.AutoSize = true;
            lblMaterialBetaW.Location = new Point(336, 115);
            lblMaterialBetaW.Name = "lblMaterialBetaW";
            lblMaterialBetaW.Size = new Size(52, 15);
            lblMaterialBetaW.TabIndex = 12;
            lblMaterialBetaW.Text = "BetaW =";
            // 
            // lblMaterialE
            // 
            lblMaterialE.AutoSize = true;
            lblMaterialE.Location = new Point(240, 115);
            lblMaterialE.Name = "lblMaterialE";
            lblMaterialE.Size = new Size(27, 15);
            lblMaterialE.TabIndex = 13;
            lblMaterialE.Text = "E = ";
            // 
            // lblMaterialFu
            // 
            lblMaterialFu.AutoSize = true;
            lblMaterialFu.Location = new Point(336, 79);
            lblMaterialFu.Name = "lblMaterialFu";
            lblMaterialFu.Size = new Size(31, 15);
            lblMaterialFu.TabIndex = 11;
            lblMaterialFu.Text = "Fu =";
            // 
            // lblMaterialFy
            // 
            lblMaterialFy.AutoSize = true;
            lblMaterialFy.Location = new Point(240, 79);
            lblMaterialFy.Name = "lblMaterialFy";
            lblMaterialFy.Size = new Size(30, 15);
            lblMaterialFy.TabIndex = 10;
            lblMaterialFy.Text = "Fy =";
            // 
            // txtPlateWidth_mm
            // 
            txtPlateWidth_mm.Location = new Point(98, 88);
            txtPlateWidth_mm.Name = "txtPlateWidth_mm";
            txtPlateWidth_mm.Size = new Size(69, 23);
            txtPlateWidth_mm.TabIndex = 14;
            txtPlateWidth_mm.Text = "100";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(14, 91);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 15;
            label4.Text = "b [mm]";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 122);
            label5.Name = "label5";
            label5.Size = new Size(46, 15);
            label5.TabIndex = 16;
            label5.Text = "e [mm]";
            // 
            // txtEdgeDistanceA_mm
            // 
            txtEdgeDistanceA_mm.Location = new Point(98, 119);
            txtEdgeDistanceA_mm.Name = "txtEdgeDistanceA_mm";
            txtEdgeDistanceA_mm.Size = new Size(69, 23);
            txtEdgeDistanceA_mm.TabIndex = 18;
            txtEdgeDistanceA_mm.Text = "60";
            // 
            // txtLoadSer_kN
            // 
            txtLoadSer_kN.Location = new Point(100, 159);
            txtLoadSer_kN.Name = "txtLoadSer_kN";
            txtLoadSer_kN.Size = new Size(100, 23);
            txtLoadSer_kN.TabIndex = 21;
            txtLoadSer_kN.Text = "700";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 163);
            label7.Name = "label7";
            label7.Size = new Size(75, 15);
            label7.TabIndex = 20;
            label7.Text = "F_Ed,ser [kN]";
            // 
            // chkReplaceablePin
            // 
            chkReplaceablePin.AutoSize = true;
            chkReplaceablePin.Checked = true;
            chkReplaceablePin.CheckState = CheckState.Checked;
            chkReplaceablePin.Location = new Point(14, 201);
            chkReplaceablePin.Name = "chkReplaceablePin";
            chkReplaceablePin.Size = new Size(148, 19);
            chkReplaceablePin.TabIndex = 22;
            chkReplaceablePin.Text = "Replaceable pin checks";
            chkReplaceablePin.UseVisualStyleBackColor = true;
            // 
            // cmbLugType
            // 
            cmbLugType.FormattingEnabled = true;
            cmbLugType.Location = new Point(100, 35);
            cmbLugType.Name = "cmbLugType";
            cmbLugType.Size = new Size(111, 23);
            cmbLugType.TabIndex = 23;
            cmbLugType.SelectedIndexChanged += cmbLugType_SelectedIndexChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(14, 39);
            label8.Name = "label8";
            label8.Size = new Size(53, 15);
            label8.TabIndex = 24;
            label8.Text = "Lug type";
            // 
            // txtTensionPinDiameter_mm
            // 
            txtTensionPinDiameter_mm.Location = new Point(141, 66);
            txtTensionPinDiameter_mm.Name = "txtTensionPinDiameter_mm";
            txtTensionPinDiameter_mm.Size = new Size(65, 23);
            txtTensionPinDiameter_mm.TabIndex = 25;
            txtTensionPinDiameter_mm.Text = "32";
            txtTensionPinDiameter_mm.TextChanged += txtTensionPinDiameter_mm_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(18, 69);
            label9.Name = "label9";
            label9.Size = new Size(117, 15);
            label9.TabIndex = 26;
            label9.Text = "Pin diameter d [mm]";
            // 
            // cmbPinMaterials
            // 
            cmbPinMaterials.FormattingEnabled = true;
            cmbPinMaterials.Location = new Point(105, 34);
            cmbPinMaterials.Name = "cmbPinMaterials";
            cmbPinMaterials.Size = new Size(101, 23);
            cmbPinMaterials.TabIndex = 27;
            cmbPinMaterials.SelectedIndexChanged += cmbPinMaterials_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(18, 38);
            label10.Name = "label10";
            label10.Size = new Size(70, 15);
            label10.TabIndex = 26;
            label10.Text = "Pin material";
            // 
            // lblPinFy
            // 
            lblPinFy.AutoSize = true;
            lblPinFy.Location = new Point(254, 38);
            lblPinFy.Name = "lblPinFy";
            lblPinFy.Size = new Size(46, 15);
            lblPinFy.TabIndex = 28;
            lblPinFy.Text = "fy,p = -";
            // 
            // lblPinFu
            // 
            lblPinFu.AutoSize = true;
            lblPinFu.Location = new Point(328, 38);
            lblPinFu.Name = "lblPinFu";
            lblPinFu.Size = new Size(47, 15);
            lblPinFu.TabIndex = 29;
            lblPinFu.Text = "fu,p = -";
            // 
            // txtDnvOutOfPlaneAngle_deg
            // 
            txtDnvOutOfPlaneAngle_deg.AcceptsReturn = true;
            txtDnvOutOfPlaneAngle_deg.Location = new Point(109, 28);
            txtDnvOutOfPlaneAngle_deg.Name = "txtDnvOutOfPlaneAngle_deg";
            txtDnvOutOfPlaneAngle_deg.Size = new Size(93, 23);
            txtDnvOutOfPlaneAngle_deg.TabIndex = 35;
            txtDnvOutOfPlaneAngle_deg.Text = "10";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(23, 28);
            label13.Name = "label13";
            label13.Size = new Size(67, 15);
            label13.TabIndex = 34;
            label13.Text = "alpha [deg]";
            // 
            // txtCheekPlateThickness_mm
            // 
            txtCheekPlateThickness_mm.AcceptsReturn = true;
            txtCheekPlateThickness_mm.Location = new Point(298, 98);
            txtCheekPlateThickness_mm.Name = "txtCheekPlateThickness_mm";
            txtCheekPlateThickness_mm.Size = new Size(69, 23);
            txtCheekPlateThickness_mm.TabIndex = 39;
            txtCheekPlateThickness_mm.Text = "10";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(213, 101);
            label15.Name = "label15";
            label15.Size = new Size(57, 15);
            label15.TabIndex = 38;
            label15.Text = "tch [mm]";
            // 
            // txtRch_mm
            // 
            txtRch_mm.AcceptsReturn = true;
            txtRch_mm.Location = new Point(298, 129);
            txtRch_mm.Name = "txtRch_mm";
            txtRch_mm.Size = new Size(69, 23);
            txtRch_mm.TabIndex = 41;
            txtRch_mm.Text = "50";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(213, 132);
            label16.Name = "label16";
            label16.Size = new Size(60, 15);
            label16.TabIndex = 40;
            label16.Text = "Rch [mm]";
            // 
            // txtCheekPlateWeldA_mm
            // 
            txtCheekPlateWeldA_mm.AcceptsReturn = true;
            txtCheekPlateWeldA_mm.Location = new Point(298, 160);
            txtCheekPlateWeldA_mm.Name = "txtCheekPlateWeldA_mm";
            txtCheekPlateWeldA_mm.Size = new Size(69, 23);
            txtCheekPlateWeldA_mm.TabIndex = 43;
            txtCheekPlateWeldA_mm.Text = "10";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(213, 163);
            label17.Name = "label17";
            label17.Size = new Size(76, 15);
            label17.TabIndex = 42;
            label17.Text = "a_weld [mm]";
            // 
            // chkIncludeCheekPlatesInBearing
            // 
            chkIncludeCheekPlatesInBearing.AutoSize = true;
            chkIncludeCheekPlatesInBearing.Location = new Point(203, 25);
            chkIncludeCheekPlatesInBearing.Name = "chkIncludeCheekPlatesInBearing";
            chkIncludeCheekPlatesInBearing.Size = new Size(256, 19);
            chkIncludeCheekPlatesInBearing.TabIndex = 44;
            chkIncludeCheekPlatesInBearing.Text = "Cheek plates considered in the calculation.*";
            chkIncludeCheekPlatesInBearing.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = SystemColors.Control;
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Location = new Point(209, 50);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(221, 42);
            richTextBox1.TabIndex = 45;
            richTextBox1.Text = "* Only valid if cheek plates are welded before final drilling/reaming of the hole.";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(14, 75);
            label6.Name = "label6";
            label6.Size = new Size(73, 15);
            label6.TabIndex = 46;
            label6.Text = "Lug material";
            // 
            // lblGapS
            // 
            lblGapS.AutoSize = true;
            lblGapS.Location = new Point(18, 132);
            lblGapS.Name = "lblGapS";
            lblGapS.Size = new Size(69, 15);
            lblGapS.TabIndex = 48;
            lblGapS.Text = "Gap s [mm]";
            // 
            // txtGapS_mm
            // 
            txtGapS_mm.Location = new Point(141, 129);
            txtGapS_mm.Name = "txtGapS_mm";
            txtGapS_mm.Size = new Size(65, 23);
            txtGapS_mm.TabIndex = 47;
            txtGapS_mm.Text = "5";
            // 
            // lblOuterLugThicknessT2
            // 
            lblOuterLugThicknessT2.AutoSize = true;
            lblOuterLugThicknessT2.Location = new Point(18, 103);
            lblOuterLugThicknessT2.Name = "lblOuterLugThicknessT2";
            lblOuterLugThicknessT2.Size = new Size(103, 15);
            lblOuterLugThicknessT2.TabIndex = 50;
            lblOuterLugThicknessT2.Text = "Outer lug t2 [mm]";
            // 
            // txtOuterLugThicknessT2_mm
            // 
            txtOuterLugThicknessT2_mm.Location = new Point(141, 100);
            txtOuterLugThicknessT2_mm.Name = "txtOuterLugThicknessT2_mm";
            txtOuterLugThicknessT2_mm.Size = new Size(65, 23);
            txtOuterLugThicknessT2_mm.TabIndex = 49;
            txtOuterLugThicknessT2_mm.Text = "40";
            // 
            // grpLoads
            // 
            grpLoads.Controls.Add(chkReplaceablePin);
            grpLoads.Controls.Add(cmbLugType);
            grpLoads.Controls.Add(label8);
            grpLoads.Controls.Add(label6);
            grpLoads.Controls.Add(cmbMaterials);
            grpLoads.Controls.Add(lblMaterialFy);
            grpLoads.Controls.Add(lblMaterialFu);
            grpLoads.Controls.Add(lblMaterialE);
            grpLoads.Controls.Add(lblMaterialBetaW);
            grpLoads.Controls.Add(label1);
            grpLoads.Controls.Add(txtLoad_kN);
            grpLoads.Controls.Add(label7);
            grpLoads.Controls.Add(txtLoadSer_kN);
            grpLoads.Location = new Point(23, 23);
            grpLoads.Name = "grpLoads";
            grpLoads.Size = new Size(489, 226);
            grpLoads.TabIndex = 51;
            grpLoads.TabStop = false;
            grpLoads.Text = "Loads / material";
            // 
            // grpLugGeometry
            // 
            grpLugGeometry.Controls.Add(label2);
            grpLugGeometry.Controls.Add(txtPlateThickness_mm);
            grpLugGeometry.Controls.Add(label3);
            grpLugGeometry.Controls.Add(txtHoleDiameter_mm);
            grpLugGeometry.Controls.Add(txtPlateWidth_mm);
            grpLugGeometry.Controls.Add(label4);
            grpLugGeometry.Controls.Add(richTextBox1);
            grpLugGeometry.Controls.Add(txtCheekPlateWeldA_mm);
            grpLugGeometry.Controls.Add(label5);
            grpLugGeometry.Controls.Add(chkIncludeCheekPlatesInBearing);
            grpLugGeometry.Controls.Add(label17);
            grpLugGeometry.Controls.Add(txtEdgeDistanceA_mm);
            grpLugGeometry.Controls.Add(txtCheekPlateThickness_mm);
            grpLugGeometry.Controls.Add(txtRch_mm);
            grpLugGeometry.Controls.Add(label15);
            grpLugGeometry.Controls.Add(label16);
            grpLugGeometry.Location = new Point(23, 269);
            grpLugGeometry.Name = "grpLugGeometry";
            grpLugGeometry.Size = new Size(489, 205);
            grpLugGeometry.TabIndex = 52;
            grpLugGeometry.TabStop = false;
            grpLugGeometry.Text = "Lug geometry";
            // 
            // grpTransportLug
            // 
            grpTransportLug.Controls.Add(label13);
            grpTransportLug.Controls.Add(txtDnvOutOfPlaneAngle_deg);
            grpTransportLug.Controls.Add(btnSelectShackleByLoad);
            grpTransportLug.Controls.Add(cmbShackles);
            grpTransportLug.Controls.Add(lblShackleWll);
            grpTransportLug.Controls.Add(lblShackleDpin);
            grpTransportLug.Controls.Add(lblShackleB1);
            grpTransportLug.Controls.Add(lblShackleHDnv);
            grpTransportLug.Controls.Add(lblShackleInfo);
            grpTransportLug.Location = new Point(23, 494);
            grpTransportLug.Name = "grpTransportLug";
            grpTransportLug.Size = new Size(487, 255);
            grpTransportLug.TabIndex = 53;
            grpTransportLug.TabStop = false;
            grpTransportLug.Text = "Transport Lug / Shackle / DNV";
            // 
            // grpTensionLug
            // 
            grpTensionLug.Controls.Add(cmbPinMaterials);
            grpTensionLug.Controls.Add(label10);
            grpTensionLug.Controls.Add(lblPinFy);
            grpTensionLug.Controls.Add(lblPinFu);
            grpTensionLug.Controls.Add(lblOuterLugThicknessT2);
            grpTensionLug.Controls.Add(label9);
            grpTensionLug.Controls.Add(txtOuterLugThicknessT2_mm);
            grpTensionLug.Controls.Add(txtTensionPinDiameter_mm);
            grpTensionLug.Controls.Add(lblGapS);
            grpTensionLug.Controls.Add(txtGapS_mm);
            grpTensionLug.Location = new Point(23, 494);
            grpTensionLug.Name = "grpTensionLug";
            grpTensionLug.Size = new Size(489, 195);
            grpTensionLug.TabIndex = 54;
            grpTensionLug.TabStop = false;
            grpTensionLug.Text = "Tension Lug / Pin";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(btnCheckBasicPadeye);
            groupBox5.Controls.Add(txtBasicCheckResult);
            groupBox5.Location = new Point(543, 23);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(514, 881);
            groupBox5.TabIndex = 55;
            groupBox5.TabStop = false;
            groupBox5.Text = "Result";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1076, 929);
            Controls.Add(grpTransportLug);
            Controls.Add(groupBox5);
            Controls.Add(grpTensionLug);
            Controls.Add(grpLugGeometry);
            Controls.Add(grpLoads);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            grpLoads.ResumeLayout(false);
            grpLoads.PerformLayout();
            grpLugGeometry.ResumeLayout(false);
            grpLugGeometry.PerformLayout();
            grpTransportLug.ResumeLayout(false);
            grpTransportLug.PerformLayout();
            grpTensionLug.ResumeLayout(false);
            grpTensionLug.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cmbShackles;
        private Label lblShackleWll;
        private Label lblShackleDpin;
        private Label lblShackleB1;
        private Label lblShackleHDnv;
        private Label lblShackleInfo;
        private TextBox txtLoad_kN;
        private TextBox txtPlateThickness_mm;
        private TextBox txtHoleDiameter_mm;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnCheckBasicPadeye;
        private TextBox txtBasicCheckResult;
        private Button btnSelectShackleByLoad;
        private ComboBox cmbMaterials;
        private Label lblMaterialBetaW;
        private Label lblMaterialE;
        private Label lblMaterialFu;
        private Label lblMaterialFy;
        private TextBox txtPlateWidth_mm;
        private Label label4;
        private Label label5;
        private TextBox txtEdgeDistanceA_mm;
        private TextBox txtLoadSer_kN;
        private Label label7;
        private CheckBox chkReplaceablePin;
        private ComboBox cmbLugType;
        private Label label8;
        private TextBox txtTensionPinDiameter_mm;
        private Label label9;
        private ComboBox cmbPinMaterials;
        private Label label10;
        private Label lblPinFy;
        private Label lblPinFu;
        private TextBox txtDnvOutOfPlaneAngle_deg;
        private Label label13;
        private TextBox txtCheekPlateThickness_mm;
        private Label label15;
        private TextBox txtRch_mm;
        private Label label16;
        private TextBox txtCheekPlateWeldA_mm;
        private Label label17;
        private CheckBox chkIncludeCheekPlatesInBearing;
        private RichTextBox richTextBox1;
        private Label label6;
        private Label lblGapS;
        private TextBox txtGapS_mm;
        private Label lblOuterLugThicknessT2;
        private TextBox txtOuterLugThicknessT2_mm;
        private GroupBox grpLoads;
        private GroupBox grpLugGeometry;
        private GroupBox grpTransportLug;
        private GroupBox grpTensionLug;
        private GroupBox groupBox5;
    }
}
