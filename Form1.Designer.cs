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
            label6 = new Label();
            txtEdgeDistanceA_mm = new TextBox();
            txtSideDistanceC_mm = new TextBox();
            SuspendLayout();
            // 
            // cmbShackles
            // 
            cmbShackles.FormattingEnabled = true;
            cmbShackles.Location = new Point(162, 33);
            cmbShackles.Name = "cmbShackles";
            cmbShackles.Size = new Size(121, 23);
            cmbShackles.TabIndex = 0;
            cmbShackles.SelectedIndexChanged += cmbShackles_SelectedIndexChanged;
            // 
            // lblShackleWll
            // 
            lblShackleWll.AutoSize = true;
            lblShackleWll.Location = new Point(42, 36);
            lblShackleWll.Name = "lblShackleWll";
            lblShackleWll.Size = new Size(77, 15);
            lblShackleWll.TabIndex = 1;
            lblShackleWll.Text = "lblShackleWll";
            // 
            // lblShackleDpin
            // 
            lblShackleDpin.AutoSize = true;
            lblShackleDpin.Location = new Point(42, 69);
            lblShackleDpin.Name = "lblShackleDpin";
            lblShackleDpin.Size = new Size(85, 15);
            lblShackleDpin.TabIndex = 2;
            lblShackleDpin.Text = "lblShackleDpin";
            // 
            // lblShackleB1
            // 
            lblShackleB1.AutoSize = true;
            lblShackleB1.Location = new Point(42, 103);
            lblShackleB1.Name = "lblShackleB1";
            lblShackleB1.Size = new Size(73, 15);
            lblShackleB1.TabIndex = 3;
            lblShackleB1.Text = "lblShackleB1";
            // 
            // lblShackleHDnv
            // 
            lblShackleHDnv.AutoSize = true;
            lblShackleHDnv.Location = new Point(42, 133);
            lblShackleHDnv.Name = "lblShackleHDnv";
            lblShackleHDnv.Size = new Size(90, 15);
            lblShackleHDnv.TabIndex = 3;
            lblShackleHDnv.Text = "lblShackleHDnv";
            // 
            // lblShackleInfo
            // 
            lblShackleInfo.AutoSize = true;
            lblShackleInfo.Location = new Point(42, 167);
            lblShackleInfo.Name = "lblShackleInfo";
            lblShackleInfo.Size = new Size(81, 15);
            lblShackleInfo.TabIndex = 3;
            lblShackleInfo.Text = "lblShackleInfo";
            // 
            // button1
            // 
            button1.Location = new Point(433, 32);
            button1.Name = "button1";
            button1.Size = new Size(177, 23);
            button1.TabIndex = 4;
            button1.Text = "Test selected shackle";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnTestSelectedShackle_Click;
            // 
            // txtLoad_kN
            // 
            txtLoad_kN.Location = new Point(106, 212);
            txtLoad_kN.Name = "txtLoad_kN";
            txtLoad_kN.Size = new Size(100, 23);
            txtLoad_kN.TabIndex = 5;
            // 
            // txtPlateThickness_mm
            // 
            txtPlateThickness_mm.Location = new Point(106, 263);
            txtPlateThickness_mm.Name = "txtPlateThickness_mm";
            txtPlateThickness_mm.Size = new Size(100, 23);
            txtPlateThickness_mm.TabIndex = 6;
            txtPlateThickness_mm.TextChanged += txtPlateThickness_mm_TextChanged;
            // 
            // txtHoleDiameter_mm
            // 
            txtHoleDiameter_mm.AcceptsReturn = true;
            txtHoleDiameter_mm.Location = new Point(106, 311);
            txtHoleDiameter_mm.Name = "txtHoleDiameter_mm";
            txtHoleDiameter_mm.Size = new Size(100, 23);
            txtHoleDiameter_mm.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 220);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 3;
            label1.Text = "F_Ed [kN]";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(42, 266);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 3;
            label2.Text = "t [mm]";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(42, 311);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 3;
            label3.Text = "d0 [mm]";
            // 
            // btnCheckBasicPadeye
            // 
            btnCheckBasicPadeye.Location = new Point(53, 546);
            btnCheckBasicPadeye.Name = "btnCheckBasicPadeye";
            btnCheckBasicPadeye.Size = new Size(177, 23);
            btnCheckBasicPadeye.TabIndex = 4;
            btnCheckBasicPadeye.Text = "Check basic padeye";
            btnCheckBasicPadeye.UseVisualStyleBackColor = true;
            btnCheckBasicPadeye.Click += btnCheckBasicPadeye_Click;
            // 
            // txtBasicCheckResult
            // 
            txtBasicCheckResult.Location = new Point(55, 575);
            txtBasicCheckResult.Multiline = true;
            txtBasicCheckResult.Name = "txtBasicCheckResult";
            txtBasicCheckResult.ReadOnly = true;
            txtBasicCheckResult.ScrollBars = ScrollBars.Vertical;
            txtBasicCheckResult.Size = new Size(704, 218);
            txtBasicCheckResult.TabIndex = 8;
            txtBasicCheckResult.Click += btnCheckBasicPadeye_Click;
            // 
            // btnSelectShackleByLoad
            // 
            btnSelectShackleByLoad.Location = new Point(230, 311);
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
            cmbMaterials.Location = new Point(42, 361);
            cmbMaterials.Name = "cmbMaterials";
            cmbMaterials.Size = new Size(121, 23);
            cmbMaterials.TabIndex = 9;
            cmbMaterials.Click += cmbMaterials_SelectedIndexChanged;
            // 
            // lblMaterialBetaW
            // 
            lblMaterialBetaW.AutoSize = true;
            lblMaterialBetaW.Location = new Point(611, 364);
            lblMaterialBetaW.Name = "lblMaterialBetaW";
            lblMaterialBetaW.Size = new Size(97, 15);
            lblMaterialBetaW.TabIndex = 12;
            lblMaterialBetaW.Text = "lblMaterialBetaW";
            // 
            // lblMaterialE
            // 
            lblMaterialE.AutoSize = true;
            lblMaterialE.Location = new Point(477, 364);
            lblMaterialE.Name = "lblMaterialE";
            lblMaterialE.Size = new Size(69, 15);
            lblMaterialE.TabIndex = 13;
            lblMaterialE.Text = "lblMaterialE";
            // 
            // lblMaterialFu
            // 
            lblMaterialFu.AutoSize = true;
            lblMaterialFu.Location = new Point(348, 364);
            lblMaterialFu.Name = "lblMaterialFu";
            lblMaterialFu.Size = new Size(76, 15);
            lblMaterialFu.TabIndex = 11;
            lblMaterialFu.Text = "lblMaterialFu";
            // 
            // lblMaterialFy
            // 
            lblMaterialFy.AutoSize = true;
            lblMaterialFy.Location = new Point(230, 364);
            lblMaterialFy.Name = "lblMaterialFy";
            lblMaterialFy.Size = new Size(75, 15);
            lblMaterialFy.TabIndex = 10;
            lblMaterialFy.Text = "lblMaterialFy";
            // 
            // txtPlateWidth_mm
            // 
            txtPlateWidth_mm.Location = new Point(137, 404);
            txtPlateWidth_mm.Name = "txtPlateWidth_mm";
            txtPlateWidth_mm.Size = new Size(100, 23);
            txtPlateWidth_mm.TabIndex = 14;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(55, 407);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 15;
            label4.Text = "b [mm]";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(53, 449);
            label5.Name = "label5";
            label5.Size = new Size(46, 15);
            label5.TabIndex = 16;
            label5.Text = "a [mm]";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(53, 497);
            label6.Name = "label6";
            label6.Size = new Size(46, 15);
            label6.TabIndex = 17;
            label6.Text = "c [mm]";
            // 
            // txtEdgeDistanceA_mm
            // 
            txtEdgeDistanceA_mm.Location = new Point(137, 446);
            txtEdgeDistanceA_mm.Name = "txtEdgeDistanceA_mm";
            txtEdgeDistanceA_mm.Size = new Size(100, 23);
            txtEdgeDistanceA_mm.TabIndex = 18;
            // 
            // txtSideDistanceC_mm
            // 
            txtSideDistanceC_mm.Location = new Point(137, 497);
            txtSideDistanceC_mm.Name = "txtSideDistanceC_mm";
            txtSideDistanceC_mm.Size = new Size(100, 23);
            txtSideDistanceC_mm.TabIndex = 19;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 889);
            Controls.Add(txtSideDistanceC_mm);
            Controls.Add(txtEdgeDistanceA_mm);
            Controls.Add(label6);
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
        private Label label6;
        private TextBox txtEdgeDistanceA_mm;
        private TextBox txtSideDistanceC_mm;
    }
}
