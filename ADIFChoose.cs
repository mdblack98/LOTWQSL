﻿using System;
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
    public partial class ADIFChoose : Form
    {
        public enum Source { LOTW_UPDATE,LOTW_COMPLETE, LOCAL,CANCEL };
        public Source choice;

        public ADIFChoose()
        {
            InitializeComponent();
            choice = Source.LOTW_UPDATE;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                choice = Source.LOTW_UPDATE;
            }
            else if (radioButton3.Checked)
            {
                choice = Source.LOTW_COMPLETE;
            }
            else
            {
                choice = Source.LOCAL;
            }
            this.Close();
        }

        public Source GetChoice()
        {
            return choice;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            choice = Source.CANCEL;
        }
    }
}
