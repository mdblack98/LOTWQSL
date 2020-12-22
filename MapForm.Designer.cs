namespace LOTWQSL
{
    partial class MapForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapForm));
            this.mapBox1 = new SharpMap.Forms.MapBox();
            this.comboBoxBand = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.labelAzimuth = new System.Windows.Forms.Label();
            this.buttonConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mapBox1
            // 
            this.mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
            this.mapBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.mapBox1.FineZoomFactor = 10D;
            this.mapBox1.Location = new System.Drawing.Point(-1, 39);
            this.mapBox1.MapQueryMode = SharpMap.Forms.MapBox.MapQueryType.LayerByIndex;
            this.mapBox1.Name = "mapBox1";
            this.mapBox1.PanOnClick = false;
            this.mapBox1.QueryGrowFactor = 5F;
            this.mapBox1.QueryLayerIndex = 0;
            this.mapBox1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.mapBox1.ShowProgressUpdate = false;
            this.mapBox1.Size = new System.Drawing.Size(583, 402);
            this.mapBox1.TabIndex = 0;
            this.mapBox1.Text = "mapBox1";
            this.mapBox1.WheelZoomMagnitude = -2D;
            this.mapBox1.MouseMove += new SharpMap.Forms.MapBox.MouseEventHandler(this.MapBox1_MouseMove);
            this.mapBox1.Click += new System.EventHandler(this.MapBox1_Click);
            this.mapBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.MapBox1_Paint);
            // 
            // comboBoxBand
            // 
            this.comboBoxBand.FormattingEnabled = true;
            this.comboBoxBand.Items.AddRange(new object[] {
            "All",
            "Digital",
            "TriplePlay"});
            this.comboBoxBand.Location = new System.Drawing.Point(9, 12);
            this.comboBoxBand.Name = "comboBoxBand";
            this.comboBoxBand.Size = new System.Drawing.Size(72, 21);
            this.comboBoxBand.TabIndex = 1;
            this.comboBoxBand.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(169, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Redraw";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ButtonRedraw_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 3;
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Items.AddRange(new object[] {
            "6M",
            "10M",
            "12M",
            "15M",
            "17M",
            "20M",
            "30M",
            "40M",
            "60M",
            "80M",
            "160M",
            ""});
            this.comboBoxMode.Location = new System.Drawing.Point(87, 12);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(71, 21);
            this.comboBoxMode.TabIndex = 2;
            this.comboBoxMode.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ComboBox1_DrawItem);
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.ComboBoxMode_SelectedIndexChanged);
            // 
            // labelAzimuth
            // 
            this.labelAzimuth.AutoEllipsis = true;
            this.labelAzimuth.AutoSize = true;
            this.labelAzimuth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAzimuth.Location = new System.Drawing.Point(280, 7);
            this.labelAzimuth.MaximumSize = new System.Drawing.Size(64, 64);
            this.labelAzimuth.Name = "labelAzimuth";
            this.labelAzimuth.Size = new System.Drawing.Size(44, 13);
            this.labelAzimuth.TabIndex = 8;
            this.labelAzimuth.Text = "Azimuth";
            // 
            // buttonConfig
            // 
            this.buttonConfig.Location = new System.Drawing.Point(254, 11);
            this.buttonConfig.Name = "buttonConfig";
            this.buttonConfig.Size = new System.Drawing.Size(20, 23);
            this.buttonConfig.TabIndex = 9;
            this.buttonConfig.Text = "!";
            this.buttonConfig.UseVisualStyleBackColor = true;
            this.buttonConfig.Click += new System.EventHandler(this.ButtonConfig_Click);
            // 
            // MapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 438);
            this.Controls.Add(this.buttonConfig);
            this.Controls.Add(this.labelAzimuth);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBoxBand);
            this.Controls.Add(this.mapBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MapForm";
            this.Text = "Map";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.MapForm_Load);
            this.Shown += new System.EventHandler(this.Form2_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SharpMap.Forms.MapBox mapBox1;
        private System.Windows.Forms.ComboBox comboBoxBand;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label labelAzimuth;
        private System.Windows.Forms.Button buttonConfig;

    }
}