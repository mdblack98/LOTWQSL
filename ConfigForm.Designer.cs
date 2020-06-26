namespace LOTWQSL
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxLabels = new System.Windows.Forms.CheckBox();
            this.labelLon = new System.Windows.Forms.Label();
            this.labelLat = new System.Windows.Forms.Label();
            this.textBoxLon = new System.Windows.Forms.TextBox();
            this.textBoxLat = new System.Windows.Forms.TextBox();
            this.checkBoxAzimuth = new System.Windows.Forms.CheckBox();
            this.buttonStateFont = new System.Windows.Forms.Button();
            this.labelStateFont = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelAzimuthFont = new System.Windows.Forms.Label();
            this.buttonAzimuthFont = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxLabels
            // 
            this.checkBoxLabels.AutoSize = true;
            this.checkBoxLabels.Location = new System.Drawing.Point(19, 46);
            this.checkBoxLabels.Name = "checkBoxLabels";
            this.checkBoxLabels.Size = new System.Drawing.Size(85, 17);
            this.checkBoxLabels.TabIndex = 12;
            this.checkBoxLabels.Text = "State Labels";
            this.checkBoxLabels.UseVisualStyleBackColor = true;
            this.checkBoxLabels.CheckedChanged += new System.EventHandler(this.checkBoxLabels_CheckedChanged);
            // 
            // labelLon
            // 
            this.labelLon.AutoSize = true;
            this.labelLon.Location = new System.Drawing.Point(108, 12);
            this.labelLon.Name = "labelLon";
            this.labelLon.Size = new System.Drawing.Size(25, 13);
            this.labelLon.TabIndex = 14;
            this.labelLon.Text = "Lon";
            // 
            // labelLat
            // 
            this.labelLat.AutoSize = true;
            this.labelLat.Location = new System.Drawing.Point(16, 12);
            this.labelLat.Name = "labelLat";
            this.labelLat.Size = new System.Drawing.Size(22, 13);
            this.labelLat.TabIndex = 13;
            this.labelLat.Text = "Lat";
            // 
            // textBoxLon
            // 
            this.textBoxLon.Location = new System.Drawing.Point(132, 9);
            this.textBoxLon.Name = "textBoxLon";
            this.textBoxLon.Size = new System.Drawing.Size(68, 20);
            this.textBoxLon.TabIndex = 11;
            this.textBoxLon.TextChanged += new System.EventHandler(this.textBoxLon_TextChanged);
            // 
            // textBoxLat
            // 
            this.textBoxLat.Location = new System.Drawing.Point(37, 9);
            this.textBoxLat.Name = "textBoxLat";
            this.textBoxLat.Size = new System.Drawing.Size(64, 20);
            this.textBoxLat.TabIndex = 10;
            this.textBoxLat.TextChanged += new System.EventHandler(this.textBoxLat_TextChanged);
            // 
            // checkBoxAzimuth
            // 
            this.checkBoxAzimuth.AutoSize = true;
            this.checkBoxAzimuth.Location = new System.Drawing.Point(19, 80);
            this.checkBoxAzimuth.Name = "checkBoxAzimuth";
            this.checkBoxAzimuth.Size = new System.Drawing.Size(100, 17);
            this.checkBoxAzimuth.TabIndex = 15;
            this.checkBoxAzimuth.Text = "Azimuth Display";
            this.checkBoxAzimuth.UseVisualStyleBackColor = true;
            this.checkBoxAzimuth.CheckedChanged += new System.EventHandler(this.checkBoxAzimuth_CheckedChanged);
            // 
            // buttonStateFont
            // 
            this.buttonStateFont.Location = new System.Drawing.Point(125, 43);
            this.buttonStateFont.Name = "buttonStateFont";
            this.buttonStateFont.Size = new System.Drawing.Size(76, 23);
            this.buttonStateFont.TabIndex = 16;
            this.buttonStateFont.Text = "State Font";
            this.buttonStateFont.UseVisualStyleBackColor = true;
            this.buttonStateFont.Click += new System.EventHandler(this.buttonStateFont_Click);
            // 
            // labelStateFont
            // 
            this.labelStateFont.AutoSize = true;
            this.labelStateFont.Location = new System.Drawing.Point(207, 47);
            this.labelStateFont.Name = "labelStateFont";
            this.labelStateFont.Size = new System.Drawing.Size(47, 13);
            this.labelStateFont.TabIndex = 17;
            this.labelStateFont.Text = "Example";
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(12, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(291, 2);
            this.label1.TabIndex = 18;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label2.Location = new System.Drawing.Point(12, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(291, 2);
            this.label2.TabIndex = 19;
            this.label2.Text = "label2";
            // 
            // labelAzimuthFont
            // 
            this.labelAzimuthFont.AutoSize = true;
            this.labelAzimuthFont.Location = new System.Drawing.Point(207, 81);
            this.labelAzimuthFont.Name = "labelAzimuthFont";
            this.labelAzimuthFont.Size = new System.Drawing.Size(47, 13);
            this.labelAzimuthFont.TabIndex = 21;
            this.labelAzimuthFont.Text = "Example";
            // 
            // buttonAzimuthFont
            // 
            this.buttonAzimuthFont.Location = new System.Drawing.Point(125, 77);
            this.buttonAzimuthFont.Name = "buttonAzimuthFont";
            this.buttonAzimuthFont.Size = new System.Drawing.Size(76, 23);
            this.buttonAzimuthFont.TabIndex = 20;
            this.buttonAzimuthFont.Text = "Azimuth Font";
            this.buttonAzimuthFont.UseVisualStyleBackColor = true;
            this.buttonAzimuthFont.Click += new System.EventHandler(this.buttonAzimuthFont_Click);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 133);
            this.Controls.Add(this.labelAzimuthFont);
            this.Controls.Add(this.buttonAzimuthFont);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelStateFont);
            this.Controls.Add(this.buttonStateFont);
            this.Controls.Add(this.checkBoxAzimuth);
            this.Controls.Add(this.checkBoxLabels);
            this.Controls.Add(this.labelLon);
            this.Controls.Add(this.labelLat);
            this.Controls.Add(this.textBoxLon);
            this.Controls.Add(this.textBoxLat);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Config";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxLabels;
        private System.Windows.Forms.Label labelLon;
        private System.Windows.Forms.Label labelLat;
        private System.Windows.Forms.TextBox textBoxLon;
        private System.Windows.Forms.TextBox textBoxLat;
        private System.Windows.Forms.CheckBox checkBoxAzimuth;
        private System.Windows.Forms.Button buttonStateFont;
        private System.Windows.Forms.Label labelStateFont;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelAzimuthFont;
        private System.Windows.Forms.Button buttonAzimuthFont;
    }
}