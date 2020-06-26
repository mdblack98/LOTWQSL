using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOTWQSL
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            String helpFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + "\\LOTWQSLHelp.htm";
            //MessageBox.Show(helpFile);
            webBrowser1.Url = new System.Uri(helpFile);
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
