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
            button1 = new Button();
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
            label11 = new Label();
            label12 = new Label();
            txtPinMoment_kNmm = new TextBox();
            txtPinMomentSer_kNmm = new TextBox();
            txtDnvOutOfPlaneAngle_deg = new TextBox();
            label13 = new Label();
            label14 = new Label();
            txtRpl_mm = new TextBox();
            txtCheekPlateThickness_mm = new TextBox();
            label15 = new Label();
            txtRch_mm = new TextBox();
            label16 = new Label();
            txtCheekPlateWeldA_mm = new TextBox();
            label17 = new Label();
            SuspendLayout();
            // 
            // cmbShackles
            // 
            cmbShackles.FormattingEnabled = true;
            cmbShackles.Location = new Point(155, 379);
            cmbShackles.Name = "cmbShackles";
            cmbShackles.Size = new Size(121, 23);
            cmbShackles.TabIndex = 0;
            cmbShackles.SelectedIndexChanged += cmbShackles_SelectedIndexChanged;
            // 
            // lblShackleWll
            // 
            lblShackleWll.AutoSize = true;
            lblShackleWll.Location = new Point(34, 382);
            lblShackleWll.Name = "lblShackleWll";
            lblShackleWll.Size = new Size(77, 15);
            lblShackleWll.TabIndex = 1;
            lblShackleWll.Text = "lblShackleWll";
            // 
            // lblShackleDpin
            // 
            lblShackleDpin.AutoSize = true;
            lblShackleDpin.Location = new Point(34, 415);
            lblShackleDpin.Name = "lblShackleDpin";
            lblShackleDpin.Size = new Size(85, 15);
            lblShackleDpin.TabIndex = 2;
            lblShackleDpin.Text = "lblShackleDpin";
            // 
            // lblShackleB1
            // 
            lblShackleB1.AutoSize = true;
            lblShackleB1.Location = new Point(34, 449);
            lblShackleB1.Name = "lblShackleB1";
            lblShackleB1.Size = new Size(73, 15);
            lblShackleB1.TabIndex = 3;
            lblShackleB1.Text = "lblShackleB1";
            // 
            // lblShackleHDnv
            // 
            lblShackleHDnv.AutoSize = true;
            lblShackleHDnv.Location = new Point(34, 479);
            lblShackleHDnv.Name = "lblShackleHDnv";
            lblShackleHDnv.Size = new Size(90, 15);
            lblShackleHDnv.TabIndex = 3;
            lblShackleHDnv.Text = "lblShackleHDnv";
            // 
            // lblShackleInfo
            // 
            lblShackleInfo.AutoSize = true;
            lblShackleInfo.Location = new Point(34, 513);
            lblShackleInfo.Name = "lblShackleInfo";
            lblShackleInfo.Size = new Size(81, 15);
            lblShackleInfo.TabIndex = 3;
            lblShackleInfo.Text = "lblShackleInfo";
            // 
            // button1
            // 
            button1.Location = new Point(187, 505);
            button1.Name = "button1";
            button1.Size = new Size(177, 23);
            button1.TabIndex = 4;
            button1.Text = "Test selected shackle";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnTestSelectedShackle_Click;
            // 
            // txtLoad_kN
            // 
            txtLoad_kN.Location = new Point(120, 50);
            txtLoad_kN.Name = "txtLoad_kN";
            txtLoad_kN.Size = new Size(100, 23);
            txtLoad_kN.TabIndex = 5;
            txtLoad_kN.Text = "120";
            // 
            // txtPlateThickness_mm
            // 
            txtPlateThickness_mm.Location = new Point(120, 129);
            txtPlateThickness_mm.Name = "txtPlateThickness_mm";
            txtPlateThickness_mm.Size = new Size(100, 23);
            txtPlateThickness_mm.TabIndex = 6;
            txtPlateThickness_mm.Text = "30";
            txtPlateThickness_mm.TextChanged += txtPlateThickness_mm_TextChanged;
            // 
            // txtHoleDiameter_mm
            // 
            txtHoleDiameter_mm.AcceptsReturn = true;
            txtHoleDiameter_mm.Location = new Point(120, 168);
            txtHoleDiameter_mm.Name = "txtHoleDiameter_mm";
            txtHoleDiameter_mm.Size = new Size(100, 23);
            txtHoleDiameter_mm.TabIndex = 7;
            txtHoleDiameter_mm.Text = "27";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(34, 58);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 3;
            label1.Text = "F_Ed [kN]";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(34, 132);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 3;
            label2.Text = "t [mm]";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(34, 168);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 3;
            label3.Text = "d0 [mm]";
            // 
            // btnCheckBasicPadeye
            // 
            btnCheckBasicPadeye.Location = new Point(46, 688);
            btnCheckBasicPadeye.Name = "btnCheckBasicPadeye";
            btnCheckBasicPadeye.Size = new Size(177, 23);
            btnCheckBasicPadeye.TabIndex = 4;
            btnCheckBasicPadeye.Text = "Check basic padeye";
            btnCheckBasicPadeye.UseVisualStyleBackColor = true;
            btnCheckBasicPadeye.Click += btnCheckBasicPadeye_Click;
            // 
            // txtBasicCheckResult
            // 
            txtBasicCheckResult.Location = new Point(48, 717);
            txtBasicCheckResult.Multiline = true;
            txtBasicCheckResult.Name = "txtBasicCheckResult";
            txtBasicCheckResult.ReadOnly = true;
            txtBasicCheckResult.ScrollBars = ScrollBars.Vertical;
            txtBasicCheckResult.Size = new Size(704, 97);
            txtBasicCheckResult.TabIndex = 8;
            txtBasicCheckResult.Click += btnCheckBasicPadeye_Click;
            // 
            // btnSelectShackleByLoad
            // 
            btnSelectShackleByLoad.Location = new Point(244, 168);
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
            cmbMaterials.Location = new Point(35, 551);
            cmbMaterials.Name = "cmbMaterials";
            cmbMaterials.Size = new Size(121, 23);
            cmbMaterials.TabIndex = 9;
            cmbMaterials.Click += cmbMaterials_SelectedIndexChanged;
            // 
            // lblMaterialBetaW
            // 
            lblMaterialBetaW.AutoSize = true;
            lblMaterialBetaW.Location = new Point(604, 554);
            lblMaterialBetaW.Name = "lblMaterialBetaW";
            lblMaterialBetaW.Size = new Size(97, 15);
            lblMaterialBetaW.TabIndex = 12;
            lblMaterialBetaW.Text = "lblMaterialBetaW";
            // 
            // lblMaterialE
            // 
            lblMaterialE.AutoSize = true;
            lblMaterialE.Location = new Point(470, 554);
            lblMaterialE.Name = "lblMaterialE";
            lblMaterialE.Size = new Size(69, 15);
            lblMaterialE.TabIndex = 13;
            lblMaterialE.Text = "lblMaterialE";
            // 
            // lblMaterialFu
            // 
            lblMaterialFu.AutoSize = true;
            lblMaterialFu.Location = new Point(341, 554);
            lblMaterialFu.Name = "lblMaterialFu";
            lblMaterialFu.Size = new Size(76, 15);
            lblMaterialFu.TabIndex = 11;
            lblMaterialFu.Text = "lblMaterialFu";
            // 
            // lblMaterialFy
            // 
            lblMaterialFy.AutoSize = true;
            lblMaterialFy.Location = new Point(223, 554);
            lblMaterialFy.Name = "lblMaterialFy";
            lblMaterialFy.Size = new Size(75, 15);
            lblMaterialFy.TabIndex = 10;
            lblMaterialFy.Text = "lblMaterialFy";
            // 
            // txtPlateWidth_mm
            // 
            txtPlateWidth_mm.Location = new Point(130, 594);
            txtPlateWidth_mm.Name = "txtPlateWidth_mm";
            txtPlateWidth_mm.Size = new Size(100, 23);
            txtPlateWidth_mm.TabIndex = 14;
            txtPlateWidth_mm.Text = "100";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(48, 597);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 15;
            label4.Text = "b [mm]";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(46, 639);
            label5.Name = "label5";
            label5.Size = new Size(46, 15);
            label5.TabIndex = 16;
            label5.Text = "e [mm]";
            // 
            // txtEdgeDistanceA_mm
            // 
            txtEdgeDistanceA_mm.Location = new Point(130, 636);
            txtEdgeDistanceA_mm.Name = "txtEdgeDistanceA_mm";
            txtEdgeDistanceA_mm.Size = new Size(100, 23);
            txtEdgeDistanceA_mm.TabIndex = 18;
            txtEdgeDistanceA_mm.Text = "60";
            // 
            // txtLoadSer_kN
            // 
            txtLoadSer_kN.Location = new Point(120, 86);
            txtLoadSer_kN.Name = "txtLoadSer_kN";
            txtLoadSer_kN.Size = new Size(100, 23);
            txtLoadSer_kN.TabIndex = 21;
            txtLoadSer_kN.Text = "80";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(34, 94);
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
            chkReplaceablePin.Location = new Point(553, 172);
            chkReplaceablePin.Name = "chkReplaceablePin";
            chkReplaceablePin.Size = new Size(148, 19);
            chkReplaceablePin.TabIndex = 22;
            chkReplaceablePin.Text = "Replaceable pin checks";
            chkReplaceablePin.UseVisualStyleBackColor = true;
            // 
            // cmbLugType
            // 
            cmbLugType.FormattingEnabled = true;
            cmbLugType.Location = new Point(102, 20);
            cmbLugType.Name = "cmbLugType";
            cmbLugType.Size = new Size(121, 23);
            cmbLugType.TabIndex = 23;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(34, 20);
            label8.Name = "label8";
            label8.Size = new Size(53, 15);
            label8.TabIndex = 24;
            label8.Text = "Lug type";
            // 
            // txtTensionPinDiameter_mm
            // 
            txtTensionPinDiameter_mm.Location = new Point(565, 23);
            txtTensionPinDiameter_mm.Name = "txtTensionPinDiameter_mm";
            txtTensionPinDiameter_mm.Size = new Size(100, 23);
            txtTensionPinDiameter_mm.TabIndex = 25;
            txtTensionPinDiameter_mm.Text = "=lblShackleDpin";
            txtTensionPinDiameter_mm.TextChanged += txtTensionPinDiameter_mm_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(426, 23);
            label9.Name = "label9";
            label9.Size = new Size(117, 15);
            label9.TabIndex = 26;
            label9.Text = "Pin diameter d [mm]";
            // 
            // cmbPinMaterials
            // 
            cmbPinMaterials.FormattingEnabled = true;
            cmbPinMaterials.Location = new Point(565, 58);
            cmbPinMaterials.Name = "cmbPinMaterials";
            cmbPinMaterials.Size = new Size(121, 23);
            cmbPinMaterials.TabIndex = 27;
            cmbPinMaterials.SelectedIndexChanged += cmbPinMaterials_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(426, 58);
            label10.Name = "label10";
            label10.Size = new Size(70, 15);
            label10.TabIndex = 26;
            label10.Text = "Pin material";
            // 
            // lblPinFy
            // 
            lblPinFy.AutoSize = true;
            lblPinFy.Location = new Point(426, 94);
            lblPinFy.Name = "lblPinFy";
            lblPinFy.Size = new Size(46, 15);
            lblPinFy.TabIndex = 28;
            lblPinFy.Text = "fy,p = -";
            // 
            // lblPinFu
            // 
            lblPinFu.AutoSize = true;
            lblPinFu.Location = new Point(426, 129);
            lblPinFu.Name = "lblPinFu";
            lblPinFu.Size = new Size(47, 15);
            lblPinFu.TabIndex = 29;
            lblPinFu.Text = "fu,p = -";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(383, 382);
            label11.Name = "label11";
            label11.Size = new Size(96, 15);
            label11.TabIndex = 30;
            label11.Text = "M_Ed [10⁻³ kNm]";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(383, 431);
            label12.Name = "label12";
            label12.Size = new Size(114, 15);
            label12.TabIndex = 31;
            label12.Text = "M_Ed,ser [10⁻³ kNm]";
            // 
            // txtPinMoment_kNmm
            // 
            txtPinMoment_kNmm.Location = new Point(532, 382);
            txtPinMoment_kNmm.Name = "txtPinMoment_kNmm";
            txtPinMoment_kNmm.Size = new Size(100, 23);
            txtPinMoment_kNmm.TabIndex = 32;
            txtPinMoment_kNmm.Text = "200";
            // 
            // txtPinMomentSer_kNmm
            // 
            txtPinMomentSer_kNmm.Location = new Point(532, 431);
            txtPinMomentSer_kNmm.Name = "txtPinMomentSer_kNmm";
            txtPinMomentSer_kNmm.Size = new Size(100, 23);
            txtPinMomentSer_kNmm.TabIndex = 33;
            txtPinMomentSer_kNmm.Text = "150";
            // 
            // txtDnvOutOfPlaneAngle_deg
            // 
            txtDnvOutOfPlaneAngle_deg.AcceptsReturn = true;
            txtDnvOutOfPlaneAngle_deg.Location = new Point(209, 246);
            txtDnvOutOfPlaneAngle_deg.Name = "txtDnvOutOfPlaneAngle_deg";
            txtDnvOutOfPlaneAngle_deg.Size = new Size(100, 23);
            txtDnvOutOfPlaneAngle_deg.TabIndex = 35;
            txtDnvOutOfPlaneAngle_deg.Text = "10";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(34, 247);
            label13.Name = "label13";
            label13.Size = new Size(63, 15);
            label13.TabIndex = 34;
            label13.Text = "Angle_deg";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(34, 285);
            label14.Name = "label14";
            label14.Size = new Size(51, 15);
            label14.TabIndex = 36;
            label14.Text = "Rpl_mm";
            // 
            // txtRpl_mm
            // 
            txtRpl_mm.AcceptsReturn = true;
            txtRpl_mm.Location = new Point(209, 282);
            txtRpl_mm.Name = "txtRpl_mm";
            txtRpl_mm.Size = new Size(100, 23);
            txtRpl_mm.TabIndex = 37;
            txtRpl_mm.Text = "60";
            // 
            // txtCheekPlateThickness_mm
            // 
            txtCheekPlateThickness_mm.AcceptsReturn = true;
            txtCheekPlateThickness_mm.Location = new Point(204, 211);
            txtCheekPlateThickness_mm.Name = "txtCheekPlateThickness_mm";
            txtCheekPlateThickness_mm.Size = new Size(100, 23);
            txtCheekPlateThickness_mm.TabIndex = 39;
            txtCheekPlateThickness_mm.Text = "10";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(34, 211);
            label15.Name = "label15";
            label15.Size = new Size(163, 15);
            label15.TabIndex = 38;
            label15.Text = "CheekPlateThickness_mm      ";
            // 
            // txtRch_mm
            // 
            txtRch_mm.AcceptsReturn = true;
            txtRch_mm.Location = new Point(208, 311);
            txtRch_mm.Name = "txtRch_mm";
            txtRch_mm.Size = new Size(100, 23);
            txtRch_mm.TabIndex = 41;
            txtRch_mm.Text = "50";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(34, 314);
            label16.Name = "label16";
            label16.Size = new Size(54, 15);
            label16.TabIndex = 40;
            label16.Text = "Rch_mm";
            // 
            // txtCheekPlateWeldA_mm
            // 
            txtCheekPlateWeldA_mm.AcceptsReturn = true;
            txtCheekPlateWeldA_mm.Location = new Point(209, 342);
            txtCheekPlateWeldA_mm.Name = "txtCheekPlateWeldA_mm";
            txtCheekPlateWeldA_mm.Size = new Size(100, 23);
            txtCheekPlateWeldA_mm.TabIndex = 43;
            txtCheekPlateWeldA_mm.Text = "10";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(35, 345);
            label17.Name = "label17";
            label17.Size = new Size(128, 15);
            label17.TabIndex = 42;
            label17.Text = "CheekPlateWeldA_mm";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 889);
            Controls.Add(txtCheekPlateWeldA_mm);
            Controls.Add(label17);
            Controls.Add(txtRch_mm);
            Controls.Add(label16);
            Controls.Add(txtCheekPlateThickness_mm);
            Controls.Add(label15);
            Controls.Add(txtRpl_mm);
            Controls.Add(label14);
            Controls.Add(txtDnvOutOfPlaneAngle_deg);
            Controls.Add(label13);
            Controls.Add(txtPinMomentSer_kNmm);
            Controls.Add(txtPinMoment_kNmm);
            Controls.Add(label12);
            Controls.Add(label11);
            Controls.Add(lblPinFu);
            Controls.Add(lblPinFy);
            Controls.Add(cmbPinMaterials);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(txtTensionPinDiameter_mm);
            Controls.Add(label8);
            Controls.Add(cmbLugType);
            Controls.Add(chkReplaceablePin);
            Controls.Add(txtLoadSer_kN);
            Controls.Add(label7);
            Controls.Add(txtEdgeDistanceA_mm);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(txtPlateWidth_mm);
            Controls.Add(lblMaterialBetaW);
            Controls.Add(lblMaterialE);
            Controls.Add(lblMaterialFu);
            Controls.Add(lblMaterialFy);
            Controls.Add(cmbMaterials);
            Controls.Add(txtBasicCheckResult);
            Controls.Add(txtHoleDiameter_mm);
            Controls.Add(txtPlateThickness_mm);
            Controls.Add(txtLoad_kN);
            Controls.Add(btnSelectShackleByLoad);
            Controls.Add(btnCheckBasicPadeye);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblShackleInfo);
            Controls.Add(lblShackleHDnv);
            Controls.Add(lblShackleB1);
            Controls.Add(lblShackleDpin);
            Controls.Add(lblShackleWll);
            Controls.Add(cmbShackles);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbShackles;
        private Label lblShackleWll;
        private Label lblShackleDpin;
        private Label lblShackleB1;
        private Label lblShackleHDnv;
        private Label lblShackleInfo;
        private Button button1;
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
        private Label label11;
        private Label label12;
        private TextBox txtPinMoment_kNmm;
        private TextBox txtPinMomentSer_kNmm;
        private TextBox txtDnvOutOfPlaneAngle_deg;
        private Label label13;
        private Label label14;
        private TextBox txtRpl_mm;
        private TextBox txtCheekPlateThickness_mm;
        private Label label15;
        private TextBox txtRch_mm;
        private Label label16;
        private TextBox txtCheekPlateWeldA_mm;
        private Label label17;
    }
}
