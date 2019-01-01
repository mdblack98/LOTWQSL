namespace LOTWQSL
{
    partial class MainWindow2
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow2));
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.labelLogin = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.textBoxSince = new System.Windows.Forms.TextBox();
            this.labelSince = new System.Windows.Forms.Label();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.buttonMap = new System.Windows.Forms.Button();
            this.buttonGrid = new System.Windows.Forms.Button();
            this.textBoxEnd = new System.Windows.Forms.TextBox();
            this.labelEnd = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(44, 4);
            this.textBoxLogin.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(88, 22);
            this.textBoxLogin.TabIndex = 0;
            this.textBoxLogin.TextChanged += new System.EventHandler(this.Login_TextChanged);
            this.textBoxLogin.Leave += new System.EventHandler(this.Login_Leave);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(209, 4);
            this.textBoxPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(73, 22);
            this.textBoxPassword.TabIndex = 1;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 82);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(364, 238);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // labelLogin
            // 
            this.labelLogin.AutoSize = true;
            this.labelLogin.Location = new System.Drawing.Point(0, 7);
            this.labelLogin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(43, 17);
            this.labelLogin.TabIndex = 3;
            this.labelLogin.Text = "Login";
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(139, 7);
            this.labelPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(69, 17);
            this.labelPassword.TabIndex = 4;
            this.labelPassword.Text = "Password";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(145, 39);
            this.buttonRefresh.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(80, 36);
            this.buttonRefresh.TabIndex = 5;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // textBoxSince
            // 
            this.textBoxSince.Location = new System.Drawing.Point(44, 30);
            this.textBoxSince.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSince.Name = "textBoxSince";
            this.textBoxSince.Size = new System.Drawing.Size(88, 22);
            this.textBoxSince.TabIndex = 2;
            this.textBoxSince.Text = "1900-01-01";
            this.textBoxSince.Leave += new System.EventHandler(this.SinceLOTWBox_Leave);
            // 
            // labelSince
            // 
            this.labelSince.AutoSize = true;
            this.labelSince.Location = new System.Drawing.Point(-1, 36);
            this.labelSince.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSince.Name = "labelSince";
            this.labelSince.Size = new System.Drawing.Size(43, 17);
            this.labelSince.TabIndex = 7;
            this.labelSince.Text = "Since";
            // 
            // buttonHelp
            // 
            this.buttonHelp.Location = new System.Drawing.Point(297, 2);
            this.buttonHelp.Margin = new System.Windows.Forms.Padding(4);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(51, 28);
            this.buttonHelp.TabIndex = 4;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
            // 
            // buttonMap
            // 
            this.buttonMap.Location = new System.Drawing.Point(236, 39);
            this.buttonMap.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMap.Name = "buttonMap";
            this.buttonMap.Size = new System.Drawing.Size(51, 36);
            this.buttonMap.TabIndex = 6;
            this.buttonMap.Text = "Map";
            this.buttonMap.UseVisualStyleBackColor = true;
            this.buttonMap.Click += new System.EventHandler(this.ButtonMap_Click);
            // 
            // buttonGrid
            // 
            this.buttonGrid.Location = new System.Drawing.Point(297, 39);
            this.buttonGrid.Margin = new System.Windows.Forms.Padding(4);
            this.buttonGrid.Name = "buttonGrid";
            this.buttonGrid.Size = new System.Drawing.Size(51, 36);
            this.buttonGrid.TabIndex = 7;
            this.buttonGrid.Text = "Grid";
            this.buttonGrid.UseVisualStyleBackColor = true;
            this.buttonGrid.Click += new System.EventHandler(this.ButtonGrid_Click);
            // 
            // textBoxEnd
            // 
            this.textBoxEnd.Location = new System.Drawing.Point(44, 54);
            this.textBoxEnd.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxEnd.Name = "textBoxEnd";
            this.textBoxEnd.Size = new System.Drawing.Size(88, 22);
            this.textBoxEnd.TabIndex = 3;
            this.textBoxEnd.Leave += new System.EventHandler(this.TextBoxEnd_Leave);
            // 
            // labelEnd
            // 
            this.labelEnd.AutoSize = true;
            this.labelEnd.Location = new System.Drawing.Point(0, 60);
            this.labelEnd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelEnd.Name = "labelEnd";
            this.labelEnd.Size = new System.Drawing.Size(33, 17);
            this.labelEnd.TabIndex = 12;
            this.labelEnd.Text = "End";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // MainWindow2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 322);
            this.Controls.Add(this.labelEnd);
            this.Controls.Add(this.textBoxEnd);
            this.Controls.Add(this.buttonGrid);
            this.Controls.Add(this.buttonMap);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.labelSince);
            this.Controls.Add(this.textBoxSince);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.labelLogin);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxLogin);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainWindow2";
            this.Text = "LOTW QSL v1.9.8";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.TextBox textBoxSince;
        private System.Windows.Forms.Label labelSince;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonMap;
        private System.Windows.Forms.Button buttonGrid;
        private System.Windows.Forms.TextBox textBoxEnd;
        private System.Windows.Forms.Label labelEnd;
        private System.Windows.Forms.Timer timer1;
    }
}

