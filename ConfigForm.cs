using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOTWQSL
{
    public partial class ConfigForm : Form
    {
        public double myLat = Properties.Settings.Default.Latitude;
        public double myLon = Properties.Settings.Default.Longitude;
        //public bool showLabels = Properties.Settings.Default.StatesLabeled;

        public ConfigForm()
        {
            InitializeComponent();
            textBoxLat.Text = myLat.ToString();
            textBoxLon.Text = myLon.ToString();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
            toolTip1.SetToolTip(this.labelLat, "Your latitude");
            toolTip1.SetToolTip(this.labelLon, "Your longitude");
            toolTip1.SetToolTip(this.checkBoxLabels, "Turn on state labels");
            toolTip1.SetToolTip(this.checkBoxAzimuth, "Turn on azimuth indicator");
            checkBoxAzimuth.Checked = Properties.Settings.Default.AzimuthOn;
            checkBoxLabels.Enabled = false;
            checkBoxLabels.Checked = Properties.Settings.Default.StatesLabeled;
            checkBoxLabels.Enabled = true;
            labelStateFont.Font = Properties.Settings.Default.FontStateLabels;
            labelStateFont.ForeColor = Properties.Settings.Default.FontStateColor;
            labelAzimuthFont.Font = Properties.Settings.Default.AzimuthLabelFont;
            labelAzimuthFont.ForeColor = Properties.Settings.Default.AzimuthLabelColor;

        }

        private void textBoxLat_TextChanged(object sender, EventArgs e)
        {
            if (textBoxLat.Text.Contains("."))
            {
                myLat = Convert.ToDouble(textBoxLat.Text);
                Properties.Settings.Default.Latitude = myLat;
                Properties.Settings.Default.Save();
            }
        }

        private void textBoxLon_TextChanged(object sender, EventArgs e)
        {
            if (textBoxLon.Text.Contains("."))
            {
                myLon = Convert.ToDouble(textBoxLon.Text);
                Properties.Settings.Default.Longitude = myLon;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBoxLabels_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLabels.Checked)
            {
                buttonStateFont.Enabled = true;
            }
            else
            {
                buttonStateFont.Enabled = false;
            }
            Properties.Settings.Default.StatesLabeled = checkBoxLabels.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBoxAzimuth_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAzimuth.Checked)
            {
                buttonAzimuthFont.Enabled = true;
            }
            else
            {
                buttonAzimuthFont.Enabled = false;
            }
            Properties.Settings.Default.AzimuthOn = checkBoxAzimuth.Checked;
            Properties.Settings.Default.Save();

        }

        private void buttonStateFont_Click(object sender, EventArgs e)
        {
            if (checkBoxLabels.Checked)
            {
                FontDialog fontDialog = new FontDialog();
                fontDialog.ShowColor = true;
                //fontDialog.ShowApply = true;
                fontDialog.ShowEffects = true;
                fontDialog.ShowHelp = true; 
                fontDialog.Font = Properties.Settings.Default.FontStateLabels;
                fontDialog.Color = Properties.Settings.Default.FontStateColor;

                DialogResult result = fontDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    labelStateFont.Font = fontDialog.Font;
                    labelStateFont.ForeColor = fontDialog.Color;
                    Properties.Settings.Default.FontStateLabels = fontDialog.Font;
                    Properties.Settings.Default.FontStateColor = fontDialog.Color;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void buttonAzimuthFont_Click(object sender, EventArgs e)
        {

            if (checkBoxAzimuth.Checked)
            {
                FontDialog fontDialog = new FontDialog();
                fontDialog.ShowColor = true;
                //fontDialog.ShowApply = true;
                fontDialog.ShowEffects = true;
                fontDialog.ShowHelp = true; 
                fontDialog.Font = Properties.Settings.Default.AzimuthLabelFont;
                fontDialog.Color = Properties.Settings.Default.AzimuthLabelColor;

                DialogResult result = fontDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    labelStateFont.Font = fontDialog.Font;
                    labelStateFont.ForeColor = fontDialog.Color;
                    Properties.Settings.Default.AzimuthLabelFont = fontDialog.Font;
                    Properties.Settings.Default.AzimuthLabelColor = fontDialog.Color;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
