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
            SuspendLayout();
            // 
            // cmbShackles
            // 
            cmbShackles.FormattingEnabled = true;
            cmbShackles.Location = new Point(375, 141);
            cmbShackles.Name = "cmbShackles";
            cmbShackles.Size = new Size(121, 23);
            cmbShackles.TabIndex = 0;
            cmbShackles.SelectedIndexChanged += cmbShackles_SelectedIndexChanged;
            // 
            // lblShackleWll
            // 
            lblShackleWll.AutoSize = true;
            lblShackleWll.Location = new Point(80, 47);
            lblShackleWll.Name = "lblShackleWll";
            lblShackleWll.Size = new Size(77, 15);
            lblShackleWll.TabIndex = 1;
            lblShackleWll.Text = "lblShackleWll";
            // 
            // lblShackleDpin
            // 
            lblShackleDpin.AutoSize = true;
            lblShackleDpin.Location = new Point(80, 80);
            lblShackleDpin.Name = "lblShackleDpin";
            lblShackleDpin.Size = new Size(85, 15);
            lblShackleDpin.TabIndex = 2;
            lblShackleDpin.Text = "lblShackleDpin";
            // 
            // lblShackleB1
            // 
            lblShackleB1.AutoSize = true;
            lblShackleB1.Location = new Point(80, 114);
            lblShackleB1.Name = "lblShackleB1";
            lblShackleB1.Size = new Size(73, 15);
            lblShackleB1.TabIndex = 3;
            lblShackleB1.Text = "lblShackleB1";
            // 
            // lblShackleHDnv
            // 
            lblShackleHDnv.AutoSize = true;
            lblShackleHDnv.Location = new Point(80, 144);
            lblShackleHDnv.Name = "lblShackleHDnv";
            lblShackleHDnv.Size = new Size(90, 15);
            lblShackleHDnv.TabIndex = 3;
            lblShackleHDnv.Text = "lblShackleHDnv";
            // 
            // lblShackleInfo
            // 
            lblShackleInfo.AutoSize = true;
            lblShackleInfo.Location = new Point(80, 178);
            lblShackleInfo.Name = "lblShackleInfo";
            lblShackleInfo.Size = new Size(81, 15);
            lblShackleInfo.TabIndex = 3;
            lblShackleInfo.Text = "lblShackleInfo";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
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
    }
}
