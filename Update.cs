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
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void Update_Load(object sender, EventArgs e)
        {
            linkLabel1.Links.Add(6, 4, "www.qrz.com/db/w9mdb");
        }
    }
}
