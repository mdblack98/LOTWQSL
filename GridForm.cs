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
    public partial class GridForm : Form
    {
        List<string> bands;
        List<string> states;
        HashSet<string> bandStates;
        //HashSet<string> bandCountries;
        private HashSet<string> modes; //All the modes in our ADIF file
        Dictionary<string, bool> modeIsWAS;
        //SortedDictionary<string,int > countries;
        int modeSelected = 0;
        const string BANDALL = "ALL";
        const string MODEALL = "ALL";
        const string MODETRIPLEPLAY = "TRIPLEPLAY";
        const int BANDS = 0;
        const int STATES = 1;
        int gridHeadersColumn = Properties.Settings.Default.GridHeadersColumn; // what we display for the column headers on the grid
        bool gridHeadersDraw = true;

        public GridForm()
        {
            InitializeComponent();
            bandsLoad();
            bandStates = new HashSet<string>();
            //bandCountries = new HashSet<string>();
            modes = new HashSet<string>();
            modeIsWAS = new Dictionary<string, bool>();
            //countries = new SortedDictionary<string, int>();
            statesLoad();
            //countriesLoad();
        }

        private void GridForm_Load(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.GridRestoreBounds.Top;
            this.Left = Properties.Settings.Default.GridRestoreBounds.Left;
            this.Height = Properties.Settings.Default.GridRestoreBounds.Height;
            this.Width = Properties.Settings.Default.GridRestoreBounds.Width;
            //this.Location = Properties.Settings.Default.GridLocation;
            Rectangle rect = SystemInformation.VirtualScreen;
            dataGridView1.Font = new Font("Arial", 7);
            if (this.Location.X < 0 || this.Location.Y < 0)
            {
                this.Top = 0;
                this.Left = 0;
                this.Width = dataGridView1.Width+50;
                this.Height = dataGridView1.Height+100;
                //this.Width = 300;
                //this.Height = 400;
            }
            if (this.Location.X > rect.Width || this.Location.Y > rect.Bottom)
            {
                this.Top = 0;
                this.Left = 0;
            }
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.Width = 35;
                //column.DefaultCellStyle.Font = new Font("Arial", 6,);
            }
            /*
            int i = 0;
            foreach (string state in states)
            {
                dataGridView1.Rows.Add("");
                dataGridView1.Rows[i].HeaderCell.Value = state;
                ++i;
                //DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
                //row.Cells[0].Value = state;
                //dataGridView1.Rows.Add(row);
                //dataGridView1.Rows[i].Height = 10;
            }
            dataGridView1.Rows.Add("");
            dataGridView1.Rows[i].HeaderCell.Value = "Total";
             */
            parseModes();
            modeSelected = Properties.Settings.Default.GridMode;
            fillGrid();
        }
        
        void fixColRow()
        {
            string hiding = Properties.Settings.Default.GridHide;
            string[] ahiding = hiding.Split(';');
            if (ahiding.Count() == 2)
            {
                ahiding[0] = ahiding[0].Replace("col", "row");
                ahiding[1] = ahiding[1].Replace("row", "col");
                Properties.Settings.Default.GridHide = ahiding[1] + ";" + ahiding[0];
            }
            else
            {
                Properties.Settings.Default.GridHide = "col;row";
            }
            Properties.Settings.Default.Save();
        }

        private void hideGridItems()
        {
            // Now restore our hidden cols & rows
            try
            {
                string hiding = Properties.Settings.Default.GridHide;
                string[] ahiding = hiding.Split(';');
                string[] cols = ahiding[0].Split(',');
                string[] rows = ahiding[1].Split(',');
                foreach (string c in cols)
                {
                    if (!c.Equals("col"))
                    {
                        int colnum = Convert.ToInt16(c);
                        dataGridView1.Columns[colnum].Visible = false;
                        buttonUnhide.Visible = true;
                    }
                }
                foreach (string r in rows)
                {
                    if (!r.Equals("row"))
                    {
                        int rownum = Convert.ToInt16(r);
                        dataGridView1.Rows[rownum].Visible = false;
                        buttonUnhide.Visible = true;
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("hideGridItems: "+ex.Message);
            }
        }

        private void bandsLoad()
        {
            bands = new List<string>();
            bands.Add("6M");
            bands.Add("10M");
            bands.Add("12M");
            bands.Add("15M");
            bands.Add("17M");
            bands.Add("20M");
            bands.Add("30M");
            bands.Add("40M");
            bands.Add("60M");
            bands.Add("80M");
            bands.Add("160M");
            bands.Add("ALL");
            bands.Add("Triple");
        }

        private void statesLoad()
        {
            states = new List<string>();
            states.Add("AL");
            states.Add("AK");
            states.Add("AZ");
            states.Add("AR");
            states.Add("CA");
            states.Add("CO");
            states.Add("CT");
            states.Add("DC");
            states.Add("DE");
            states.Add("FL");
            states.Add("GA");
            states.Add("HI");
            states.Add("ID");
            states.Add("IL");
            states.Add("IN");
            states.Add("IA");
            states.Add("KS");
            states.Add("KY");
            states.Add("LA");
            states.Add("ME");
            states.Add("MD");
            states.Add("MA");
            states.Add("MI");
            states.Add("MN");
            states.Add("MS");
            states.Add("MO");
            states.Add("MT");
            states.Add("NE");
            states.Add("NV");
            states.Add("NH");
            states.Add("NJ");
            states.Add("NM");
            states.Add("NY");
            states.Add("NC");
            states.Add("ND");
            states.Add("OH");
            states.Add("OK");
            states.Add("OR");
            states.Add("PA");
            states.Add("RI");
            states.Add("SC");
            states.Add("SD");
            states.Add("TN");
            states.Add("TX");
            states.Add("UT");
            states.Add("VT");
            states.Add("VA");
            states.Add("WA");
            states.Add("WV");
            states.Add("WI");
            states.Add("WY");
            states.Sort();
        }

        public HashSet<string> statesRemain(HashSet<string> myStates)
        {
            HashSet<string> remain = new HashSet<string>();
            foreach (string s in states)
            {
                if (!myStates.Contains(s))
                {
                    remain.Add(s);
                }
            }
            return remain;
        }
        /*
        private void countriesLoad()
        {
            string line = "";
            try
            {
                string[] lines = System.IO.File.ReadAllLines("Countries.txt");
                foreach (string s in lines)
                {
                    line = s;
                    if (s.Contains(" Prefix ")) continue;
                    int offset = s.LastIndexOf('\t');
                    string scode = s.Substring(offset + 1);
                    int code = Convert.ToInt32(scode);
                    offset = s.IndexOf('\t');
                    string cname = "";
                    if (offset > 20)
                    {
                        string first20 = s.Substring(0,20);
                        if (first20.Contains('\t')) {
                            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                            MessageBox.Show(s);
                        }
                        cname = s.Substring(20, offset - 20);
                    }
                    else
                    {
                        MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                        MessageBox.Show(s);
                    }
                    cname = cname.TrimEnd(' ');
                    cname = cname.TrimEnd('\t');
                    countries.Add(cname, code);
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show(ex.Message+"\n"+line+"\n"+ex.StackTrace);
            }
        }
        */
        public void fillGrid()
        {
            Cursor.Current = Cursors.WaitCursor;
            string mode = comboBoxMode.GetItemText(comboBoxMode.SelectedItem);
            bool stategrid = true;
            foreach (string band in bands)
            {
                parseBandMode(band, mode);
                if (stategrid)
                {
                    foreach (string state in states)
                    {
                        HashSet<string> statesRemaining = statesRemain(bandStates);
                        fillGrid(true,band, statesRemaining);
                    }
                }
                    /*  Not implemented
                else // Countries
                {
                    foreach (string country in countries.Keys.ToList())
                    {
                        //HashSet<string> countriesRemaining = countriesRemaining(bandCountries);
                    }
                }
                     */
            }
            hideGridItems();
            loadToolTips();
            Cursor.Current = Cursors.Default;
        }

        private void fillGridold(Boolean byBand, string s, HashSet<string> statesRemaining)
        {
            byBand = true;
            int iCell = -1;
            if (byBand) // do bands in columns
            {
                iCell = bands.IndexOf(s);

            }
            else // do states in columns
            {
                iCell = states.IndexOf(s);
                for (int j = 0; j < dataGridView1.Columns.Count; ++j)
                {
                    if (j < states.Count)
                    {
                        dataGridView1.Columns[j].HeaderText = states[j];
                    }
                    else
                    {
                        dataGridView1.Columns[j].HeaderText = "Total";
                    }
                    dataGridView1.Columns[j].Visible = true;
                }
                dataGridView1.Rows.Clear();
                for (int j = 0; j < bands.Count; ++j)
                {
                    dataGridView1.Rows.Add("");
                    dataGridView1.Rows[j].HeaderCell.Value = bands[j];
                }
                //return;
            }
            int worked = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //if (row.Visible)
                {
                    row.Cells[iCell].Selected = false;
                    object valueToCheck = "";
                    if (byBand)
                    {
                        valueToCheck = row.HeaderCell.Value;
                    }
                    else
                    {
                        valueToCheck = row.Cells[iCell].Value;
                    }
                    if (statesRemaining.Contains(valueToCheck))
                    {
                        row.Cells[iCell].Value = "N";
                        row.Cells[iCell].Style.BackColor = Color.Red;
                    }
                    else
                    {
                        if (!row.HeaderCell.Value.Equals("Total"))
                        {
                            row.Cells[iCell].Value = "W";
                            row.Cells[iCell].Style.BackColor = Color.Green;
                            ++worked;
                        }
                        else
                        {
                            row.Cells[iCell].Value = worked;
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                            row.Cells[iCell].Style.Font = style.Font;
                            row.Cells[iCell].Style.BackColor = Color.Red;
                            if (worked >= 50) row.Cells[iCell].Style.BackColor = Color.LightGreen;
                            else if (worked == 51) row.Cells[iCell].Style.BackColor = Color.Green;
                        }
                    }
                }
            }
            loadToolTips();
        }

        private void fillGridRowByState(int iCell, HashSet<string> statesRemaining)
        {
            int worked = 0;
            //foreach (DataGridViewColumn col in dataGridView1.Columns) 
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //if (row.Visible)
                {
                    //col.Selected = false;
                    row.Cells[iCell].Selected = false;
                    object valueToCheck = "";
                    valueToCheck = row.HeaderCell.Value;
                    if (statesRemaining.Contains(valueToCheck))
                    {
                        row.Cells[iCell].Value = "N";
                        row.Cells[iCell].Style.BackColor = Color.Red;
                    }
                    else
                    {
                        if (!row.HeaderCell.Value.Equals("Total"))
                        {
                            row.Cells[iCell].Value = "W";
                            row.Cells[iCell].Style.BackColor = Color.Green;
                            ++worked;
                        }
                        else
                        {
                            row.Cells[iCell].Value = worked;
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                            row.Cells[iCell].Style.Font = style.Font;
                            row.Cells[iCell].Style.BackColor = Color.Red;
                            if (worked >= 50) row.Cells[iCell].Style.BackColor = Color.LightGreen;
                            else if (worked == 51) row.Cells[iCell].Style.BackColor = Color.Green;
                        }
                    }
                }
            }
        }

        private void fillGridRowByBand(int iCell, HashSet<string> statesRemaining)
        {
            int worked = 0;
            DataGridViewRow row = dataGridView1.Rows[iCell];
            foreach (DataGridViewColumn col in dataGridView1.Columns) 
            //foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //if (row.Visible)
                {
                    iCell = col.Index;
                    row.Cells[iCell].Selected = false;
                    object valueToCheck = "";
                    valueToCheck = col.HeaderCell.Value;
                    if (statesRemaining.Contains(valueToCheck))
                    {
                        row.Cells[iCell].Value = "N";
                        row.Cells[iCell].Style.BackColor = Color.Red;
                    }
                    else
                    {
                        if (!col.HeaderCell.Value.Equals("Total"))
                        {
                            row.Cells[iCell].Value = "W";
                            row.Cells[iCell].Style.BackColor = Color.Green;
                            ++worked;
                        }
                        else
                        {
                            row.Cells[iCell].Value = worked;
                            DataGridViewCellStyle style = new DataGridViewCellStyle();
                            style.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                            row.Cells[iCell].Style.Font = style.Font;
                            row.Cells[iCell].Style.BackColor = Color.Red;
                            if (worked >= 50) row.Cells[iCell].Style.BackColor = Color.LightGreen;
                            else if (worked == 51) row.Cells[iCell].Style.BackColor = Color.Green;
                        }
                    }
                }
            }
        }

        private void fillGrid(Boolean byBand, string s, HashSet<string> statesRemaining)
        {
            byBand = gridHeadersColumn == BANDS;
            int iCell=-1;
            iCell = bands.IndexOf(s);
            if (byBand && gridHeadersDraw) // switch to bands in columns
            {
                gridHeadersDraw = false;
                //dataGridView1.ColumnCount = bands.Count + 1;
                for (int j = 0; j < dataGridView1.Columns.Count; ++j)
                {
                    dataGridView1.Columns[j].Visible = false;
                    if (j < bands.Count)
                    {
                        dataGridView1.Columns[j].HeaderText = bands[j];
                        dataGridView1.Columns[j].Visible = true;
                    }
                    if (j == bands.Count)
                    {
                    }
                }
                dataGridView1.Rows.Clear();
                for (int j = 0; j < states.Count; ++j)
                {
                    dataGridView1.Rows.Add("");
                    dataGridView1.Rows[j].HeaderCell.Value = states[j];
                    dataGridView1.Rows[j].Visible = true;
                }
                dataGridView1.Rows.Add("");
                dataGridView1.Rows[states.Count].HeaderCell.Value = "Total";
                dataGridView1.Rows[states.Count].Visible = true;
            }
            else if (!byBand && gridHeadersDraw) // switch to states in columns
            {
                gridHeadersDraw = false;
                for (int j = 0; j < dataGridView1.Columns.Count; ++j)
                {
                    if (j < states.Count)
                    {
                        dataGridView1.Columns[j].HeaderText = states[j];
                    }
                    else
                    {
                        dataGridView1.Columns[j].HeaderText = "Total";
                    }
                    dataGridView1.Columns[j].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dataGridView1.Columns[j].Visible = true;
                }
                dataGridView1.Rows.Clear();
                for (int j = 0; j < bands.Count; ++j)
                {
                    dataGridView1.Rows.Add("");
                    if (j < bands.Count())
                    {
                        dataGridView1.Rows[j].HeaderCell.Value = bands[j];
                    }
                    else
                    {
                        dataGridView1.Rows[j].HeaderCell.Value = "ALL";
                    }
                }
                return;
            }
            if (byBand)
            {
                fillGridRowByState(iCell, statesRemaining);
            }
            else
            {
                fillGridRowByBand(iCell, statesRemaining);
            }
            //hideGridItems();
            //loadToolTips();
        }

        private void parseModes()
        {
            modes.Clear();
            modes.Add("ALL");
            modes.Add("DIGITAL");
            comboBoxMode.Items.Add("ALL");
            comboBoxMode.Items.Add("DIGITAL");
            int i = 0;
            int iSelected = i;
            comboBoxMode.SelectedIndex = i;
            foreach (string s in MainWindow.states) // find all modes we have done
            {
                string[] tokens = s.Split(new[] { ' ' });
                if (!modes.Contains(tokens[1]))
                {
                    modes.Add(tokens[1]);
                    comboBoxMode.Items.Add(tokens[1]);
                    ++i;
                    if (modeSelected.Equals(tokens[1]))
                    {
                        iSelected = i;
                    }
                }
            }
            comboBoxMode.SelectedIndex = iSelected;
        }

        private void parseBandMode2(string band, string mode)
        {
            bandStates.Clear();
            mode = " " + mode + " ";
            foreach (string s in MainWindow.states)
            {
                //if (s.Substring(0, band.Length).Equals(band) && (s.Contains(mode) || mode.Equals(" ALL ")))
                if ((s.Substring(0, band.Length).Equals(band) || band.Equals(BANDALL)) && (s.Contains(mode) || mode.Equals(" " + MODEALL + " ")))

                {
                    string state = s.Substring(s.Length - 2);
                    bandStates.Add(state);
                }
            }
            //bandCountries.Clear();
            //foreach (string s in countries.Keys.ToList())
            //{
            //}
        }
        /*
        private void parseTriplePlay()
        {
            bandStates.Clear();
            foreach (string s in states)
            {
                int isDigitalMode = 0;
                int isTriplePlay = 0;
                int isCW = 0;
                foreach (string chkState in MainWIndow.states)
                {
                    string[] tokens = s.Split(new string[] { " " }, StringSplitOptions.None);
                    string myband = tokens[0];
                    string mymode = tokens[1];
                    string mystate = tokens[2];
                    if (s.Equals(mystate))
                    {
                        Boolean isDigitalMode = mode.Equals(" DIGITAL ") && (!mode.Equals("SSB")) && !mode.Equals("AM") && !mode.Equals("CW") && !mode.Equals("IMAGE") && !mode.Equals("PHONE");
                        // Phone is PHONE/AM/FM/SSB/ATV
                        Boolean isTriplePlay = mode.Equals("PHONE") || mode.Equals("AM") || mode.Equals("FM") || mode.Equals("SSB");
                    }
                }
            }
        }
        */
        private void parseBandMode(string band, string mode)
        {
            LOTWMode LOTWmode = new LOTWMode();
            bandStates.Clear();
            mode = " " + mode + " ";
            foreach (string s in MainWindow.states)
            {
                string[] tokens = s.Split(new string[] { " " }, StringSplitOptions.None);
                string myband = tokens[0];
                string mymode = tokens[1];
                string mystate = tokens[2];
                LOTWmode.addCallsign(mystate, mymode);
                if (myband.Equals("6M"))
                {
                    myband = "6M";
                }
                //string myband = s.Substring(0, band.Length);
                Boolean modeOK = s.Contains(mode) || mode.Equals(" " + MODEALL + " ") || mode.Equals(" " + MODETRIPLEPLAY + " ");
                Boolean bandOK = myband.Equals(band) || band.Equals(BANDALL);
                Boolean isDigitalMode = LOTWmode.isModeDigital(mymode);
                Boolean isTriplePlay = LOTWmode.isTriplePlay(mystate);

                if ((bandOK && (modeOK || isDigitalMode) || isTriplePlay))
                {
                    string state = s.Substring(s.Length - 2);
                    bandStates.Add(state);
                }
            }
        }

        private void GridForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                Properties.Settings.Default.GridRestoreBounds = this.RestoreBounds;
                Properties.Settings.Default.Save();
                return;
            }
            Hide();
            e.Cancel = true;
        }

        private void comboBoxMode_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            modeSelected = comboBoxMode.SelectedIndex;
            Properties.Settings.Default.GridMode = modeSelected;
            Properties.Settings.Default.Save();
            fillGrid();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGrid();
        }
/*
        private void GridForm_LocationChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
            }
        }

        private void GridForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.Visible) {
            }
        }
*/
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            buttonRefresh.Enabled = false;
            Application.DoEvents();
            fillGrid();
            buttonRefresh.Enabled = true;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int myrow = e.RowIndex;
            int mycol = e.ColumnIndex;
            int rownum = 0;
            int colnum = 0;
            string colsHide = "col";
            string rowsHide = "row";
            if (myrow > -1 && mycol > -1)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Click on row or column header to hide");
                return;
            }
            foreach (DataGridViewRow rw in dataGridView1.Rows)
            {
                if (myrow == rownum)
                {
                    rw.Visible = false;
                    buttonUnhide.Visible = true;
                }
                if (!rw.Visible)
                {
                    rowsHide = rowsHide + "," + rownum;
                }
                ++rownum;
            }
            foreach (DataGridViewColumn rc in dataGridView1.Columns)
            {
                if (mycol == colnum)
                {
                    rc.Visible = false;
                    buttonUnhide.Visible = true;
                }
                if (!rc.Visible) // Build string for Properties save
                {
                    colsHide = colsHide + "," + colnum;
                }
                ++colnum;
            }
            // Build string for Properties save
            Properties.Settings.Default.GridHide = colsHide + ";" + rowsHide;
            Properties.Settings.Default.Save();
            fillGrid();
            // List call signs?
        }

        private void unHide()
        {
            foreach (DataGridViewRow rw in dataGridView1.Rows)
            {
                rw.Visible = true;
            }
            foreach (DataGridViewColumn rc in dataGridView1.Columns)
            {
                try
                {
                    String val = (String)dataGridView1[rc.Index, 1].Value;
                    if (!val.Equals(""))
                    {
                        rc.Visible = true;
                    }
                }
                catch (Exception)
                {

                }
            }
            buttonUnhide.Visible = false;
            Properties.Settings.Default.GridHide = "col;row";
            Properties.Settings.Default.Save();
            fillGrid();
        }

        private void buttonUnhide_Click(object sender, EventArgs e)
        { // Unhide any hidden columns
            unHide();
        }

        private void loadToolTips()
        {
            if (gridHeadersColumn == BANDS)
            {
                loadToolTipsRows();
            }
            else
            {
                loadToolTipsColumns();
            }
        }
        private void loadToolTipsColumns() 
        {
            foreach(KeyValuePair<string,Dictionary<string,int>> kvpair1 in MainWindow.popularStates) 
            {
                string state = kvpair1.Key;
                Dictionary<string, int> dict = kvpair1.Value;
                dict = dict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                string mystr = "Most popular calls in your QSL log\nTop 10 for "+state+":\n";
                int n = 0;
                foreach (KeyValuePair<string, int> kvpair in dict)
                {
                    ++n;
                    mystr += kvpair.Value + " QSLs" + " " + kvpair.Key + "\n";
                    if (n > 9) break;
                }
                foreach (DataGridViewColumn mycol in dataGridView1.Columns)
                {
                    if (mycol.HeaderCell.Value.ToString() == state)
                    {
                        mycol.HeaderCell.ToolTipText = mystr;
                    }
                }
            }
        }

        private void loadToolTipsRows()
        {
            foreach (KeyValuePair<string, Dictionary<string, int>> kvpair1 in MainWindow.popularStates)
            {
                string state = kvpair1.Key;
                Dictionary<string, int> dict = kvpair1.Value;
                dict = dict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                string mystr = "Most popular calls in your QSL log\nTop 10 for " + state + ":\n";
                int n = 0;
                foreach (KeyValuePair<string, int> kvpair in dict)
                {
                    ++n;
                    mystr += kvpair.Value + " QSLs" + " " + kvpair.Key + "\n";
                    if (n > 9) break;
                }
                foreach (DataGridViewRow myrow in dataGridView1.Rows)
                {
                    if (myrow.HeaderCell.Value.ToString() == state)
                    {
                        myrow.HeaderCell.ToolTipText = mystr;
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
        }

        private void GridForm_VisibleChanged(object sender, EventArgs e)
        {
           // MessageBox.Show("Viz");
        }

        private void GridForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'x')
            {
                gridHeadersColumn = (gridHeadersColumn + 1) % 2;
                Properties.Settings.Default.GridHeadersColumn = gridHeadersColumn;
                Properties.Settings.Default.Save();
                gridHeadersDraw = true;
                fixColRow();
                //unHide();                
                fillGrid();

            }
        }
    }
}
