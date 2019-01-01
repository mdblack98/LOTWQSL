using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace LOTWQSL
{
    public partial class MainWindow2 : Form
    {
        MapForm mapForm = new MapForm();
        GridForm gridForm = new GridForm();
        HelpForm help;
        string mylogin = String.Empty; // can be just callsign or callsign+callsign
        string mypassword = String.Empty;
        string sinceLOTW = String.Empty;
        string endLOTW = String.Empty;
        string keeperFile = String.Empty;
        string appData = String.Empty;
        string errorStr = String.Empty;
        string sinceLOTWStart = String.Empty;
        HashSet<string> history;
        HashSet<string> callsigns;
        //public static HashSet<string> states; // List contains State/Band/Mode
        public static HashSet<string> allWAS = new HashSet<string>();
        HashSet<string> dxccMixed;
        HashSet<string> dxccChallenge;
        HashSet<string> dxccChallengeBands;
        HashSet<string> prefixes;
        public static Dictionary<string, Dictionary<string, int>> popularStates;
        //Dictionary<string,int> popularCalls;
        int nstates = 0;
        int ndxccMixed = 0;
        int nprefixes = 0;
        int nqsls = 0;
        int ncallsigns = 0;
        int ndxccChallenge = 0;
        int mismatches = Properties.Settings.Default.Mismatches;
        Boolean mergeCalls = true; // if user does call+call login do we want to merge or keep separate?
        private string qslMismatchStr;

        public MainWindow2()
        {
            InitializeComponent();
            //InstallUpdateSyncWithInfo();
            appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LOTWQSL");
            string s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "W9MDB");
            try
            {
                if (!Directory.Exists(appData))
                {
                    Directory.CreateDirectory(appData);
                }
                if (Properties.Settings.Default.CallUpgrade)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.CallUpgrade = false;
                }
            }
            catch (Exception e)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show(appData+":"+e.Message);
            }
            mypassword = Properties.Settings.Default.password;
            mylogin = Properties.Settings.Default.login;
            keeperFile = appData + "\\" + mylogin.Replace('/','_') + ".txt";
            if (mylogin.Contains("+"))
            { // then this is a merged file
                string[] tokens = Regex.Split(mylogin, "[@+]");
                keeperFile = appData + "\\" + tokens[0].Replace('/','_') + ".txt";
            }

            string keeperFileOld = appData + "\\lotwqsl.txt";
            if (File.Exists(keeperFileOld))
            {
                try
                {
                    File.Move(keeperFileOld, keeperFile);
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            history = new HashSet<string>();
            //states = new HashSet<string>();
            dxccMixed = new HashSet<string>();
            dxccChallenge = new HashSet<string>();
            dxccChallengeBands = new HashSet<string>();
            prefixes = new HashSet<string>();
            callsigns = new HashSet<string>();
            popularStates = new Dictionary<string,Dictionary<string,int>>();
            dxccChallengeBands.Add("160M");
            dxccChallengeBands.Add("80M");
            dxccChallengeBands.Add("40M");
            dxccChallengeBands.Add("30M");
            dxccChallengeBands.Add("20M");
            dxccChallengeBands.Add("17M");
            dxccChallengeBands.Add("15M");
            dxccChallengeBands.Add("12M");
            dxccChallengeBands.Add("10M");
            dxccChallengeBands.Add("6M");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.RestoreBounds.Top;
            this.Left = Properties.Settings.Default.RestoreBounds.Left;
            this.Height = Properties.Settings.Default.RestoreBounds.Height;
            this.Width = Properties.Settings.Default.RestoreBounds.Width;
            if (this.Width < 185) this.Width = 185;
            if (this.Height < 180) this.Height = 180;
            Rectangle rect = SystemInformation.VirtualScreen;
            if (this.Location.X < 0 || this.Location.Y < 0)
            {
                this.Top = 0;
                this.Left = 0;
                this.Width = 300;
                this.Height = 400;
            }
            if (this.Location.X > rect.Width || this.Location.Y > rect.Bottom)
            {
                this.Top = 0;
                this.Left = 0;
            }
            sinceLOTW = Properties.Settings.Default.lastLOTW;
            //sinceLOTWStart = Properties.Settings.Default.
            textBoxSince.Text = sinceLOTW;
            textBoxLogin.Text = mylogin; // this will force keeperLoad()
            textBoxPassword.Text = mypassword;
            KeeperLoad(keeperFile);
            timer1.Interval = 3000;
            timer1.Start();
            //Thread oThread = new Thread(new ThreadStart(checkForUpdate));
            //oThread.Start();
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
            toolTip1.SetToolTip(this.textBoxLogin, "Your LOTW login name -- See \"Multiple Callsigns\" in Help");
            toolTip1.SetToolTip(this.labelLogin, "Your LOTW login name");
            toolTip1.SetToolTip(this.textBoxPassword, "Your LOTW password");
            toolTip1.SetToolTip(this.labelPassword, "Your LOTW password");
            toolTip1.SetToolTip(this.buttonHelp, "Help");
            toolTip1.SetToolTip(this.textBoxSince, "QSL since date YYYY-MM-DD\n(updates itself automatically)\n1900-01-01 resets everything");
            toolTip1.SetToolTip(this.labelSince, "QSL since date YYYY-MM-DD\n(updates itself automatically)\n1900-01-01 resets everything");
            toolTip1.SetToolTip(this.textBoxEnd, "QSL end date YYYY-MM-DD\nOnly needed if having download problems\nto limit # of QSOs");
            toolTip1.SetToolTip(this.labelEnd, "QSL end date YYYY-MM-DD\nOnly needed if having download problems\nto limit # of QSOs");
            toolTip1.SetToolTip(this.buttonRefresh, "Download new LOTW QSLs");
            toolTip1.SetToolTip(this.buttonMap, "Show map window");
            toolTip1.SetToolTip(this.buttonGrid, "Show grid window");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Rectangle restore;
            if (gridForm.IsHandleCreated)
            {
                gridForm.WindowState = FormWindowState.Normal;
                gridForm.Update();
                restore = new Rectangle(gridForm.Left, gridForm.Top, gridForm.Width, gridForm.Height);
                Properties.Settings.Default.GridRestoreBounds = restore;
            }
            gridForm.Close();
            if (mapForm.IsHandleCreated)
            {
                mapForm.WindowState = FormWindowState.Normal;
                mapForm.Update();
                restore = new Rectangle(mapForm.Left, mapForm.Top, mapForm.Width, mapForm.Height);
                Properties.Settings.Default.MapRestoreBounds = restore;
                Properties.Settings.Default.StatesLabeled = mapForm.showLabels;
            }
            mapForm.Close();
            this.WindowState = FormWindowState.Normal;
            this.Update();
            restore = new Rectangle(this.Left,this.Top,this.Width,this.Height);
            Properties.Settings.Default.RestoreBounds = restore;
            // RestoreBounds doesn't seemt to get the current location
            //Properties.Settings.Default.RestoreBounds = this.RestoreBounds;
            Properties.Settings.Default.lastLOTW = sinceLOTW;
            Properties.Settings.Default.Save();
        }

        void AddToPopular(string state,string callsign)
        {
            if (!popularStates.ContainsKey(state))
            {
                Dictionary<string, int> newdict = new Dictionary<string, int>
                {
                    { callsign, 1 }
                };
                popularStates.Add(state, newdict);
                return;
            }
            Dictionary<string, int> dict = popularStates[state];

            if (dict.ContainsKey(callsign))
            {
                ++dict[callsign];
            }
            else
            {
                dict[callsign] = 1;
            }
            popularStates[state] = dict;
        }

        void KeeperLoad(string callsign)
        {
            string lastDate = "19000101";
            allWAS.Clear();
            history.Clear();
            callsigns.Clear();
            popularStates.Clear();
            prefixes.Clear();
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(keeperFile);
                string line;
                nqsls = 0;
                ncallsigns = 0;
                line = file.ReadLine();
                if (!line.Equals("Version 1.1"))
                {
                    file.Close();
                    file.Dispose();
                    File.Delete(keeperFile);
                    richTextBox1.AppendText("QSL file " + keeperFile + " is wrong version\n");
                    richTextBox1.AppendText("Old version deleted\nPlease check date and click Refresh\n");
                    sinceLOTW = textBoxSince.Text = "1990-01-01";
                    Properties.Settings.Default.lastLOTW = sinceLOTW;
                }
                else
                {
                    // Somehow we're getting dups...this is a temporary fix until figured out
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Length < 2) continue;
                        //if (line.Contains("70CM")) continue;
                        if (line.Length < 9)
                        {
                            MessageBox.Show("Error getting date @8 offset in '" + line + "'");
                            continue;
                        }
                        string thisDate = line.Substring(0, 8);
                        if (string.Compare(thisDate, lastDate) > 0)
                        {
                            lastDate = thisDate;
                            sinceLOTW = lastDate;
                        }
                        var tokens = line.Split(new[] { ',', '\r', '\n' });
                        if (!line.Contains("none") && !allWAS.Contains(line))
                        {
                            allWAS.Add(tokens[1]);
                        }
                        ++nqsls;
                        history.Add(tokens[0]);
                        // [0] = "20131203 13:19 40M JT65 AJ4HW"
                        var tokens1 = tokens[0].Split(' ');
                        callsign = tokens1[4];
                        if (!callsigns.Contains(callsign))
                            callsigns.Add(callsign);
                        var tokens2 = tokens[2].Split(' ');
                        string country = "";
                        if (tokens2.Count() > 2) country = tokens2[2]; // should be country info
                        if (tokens2.Count() > 3)
                        {
                            for (int i = 3; i < tokens2.Count(); ++i)
                            {
                                country += " " + tokens2[i];
                            }
                        }
                        if (country.Length > 0 )
                        {
                            string band;
                            String[] tokens3 = tokens[2].Split(' ');
                            band = tokens3[0];
                            if (!dxccMixed.Contains(country))
                            {
                                dxccMixed.Add(country);
                            }
                            if (!band.Equals("60M") && tokens.Count() > 2)
                            {
                                if (dxccChallengeBands.Contains(band))
                                {
                                    string dxccKey = tokens3[0] + " " + country;
                                    if (!dxccChallenge.Contains(dxccKey)) {
                                        dxccChallenge.Add(dxccKey);
                                        ++ndxccChallenge;
                                    }
                                }
                            }
                        }
                        if (!prefixes.Contains(tokens[3]))
                        {
                            prefixes.Add(tokens[3]);
                            //richTextBox1.AppendText(tokens[3] + "\n");
                        }
                        tokens1 = tokens[1].Split(' ');
                        if (tokens1.Count() < 3) continue;
                        string state = tokens1[2].Substring(0,2);
                        AddToPopular(state,callsign);
                        //if ((!states.Contains(tokens[1])) && (!tokens[1].Equals("none")))
                        //{
                        //    states.Add(tokens[1]);
                        //}
                        
                    }
                }
                file.Close();
                // Sort our popularStates for the grid display
                foreach (KeyValuePair<string, Dictionary<string, int>> kvpair in popularStates)
                {
                    //richTextBox1.AppendText(kvpair.Key + "\n");
                    Dictionary<string, int> tdict = popularStates[kvpair.Key];
                    //tdict = tdict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                    //popularStates[kvpair.Key] = tdict;
                    //foreach (KeyValuePair<string, int> kvpair2 in tdict)
                    //{
                    //    richTextBox1.AppendText(kvpair2.Key + ":" + kvpair2.Value + "\n");
                    //}
                }
                richTextBox1.AppendText(nqsls + " QSLs loaded\n");
                richTextBox1.AppendText(mismatches + " QSLs with mode mismatch\n");
                richTextBox1.AppendText(callsigns.Count() + " callsigns\n");
                richTextBox1.AppendText(prefixes.Count() + " prefixes\n");
                //richTextBox1.AppendText(states.Count() + " state entries\n");
                richTextBox1.AppendText(dxccChallenge.Count() + " DXCC challenge entries\n");
                richTextBox1.AppendText(dxccMixed.Count() + " DXCC mixed entries\n");
                ncallsigns = callsigns.Count();
                //nstates = states.Count();
                nstates = allWAS.Count();
                nprefixes = prefixes.Count();
                ndxccMixed = dxccMixed.Count();
                ndxccChallenge = dxccChallenge.Count();
                //foreach (string s in dxcc)
                //{
                //    richTextBox1.AppendText(s + "\n");
                //}
            }
            catch (FileNotFoundException e)
            {
                if (textBoxLogin.Text.Length < 3)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("Please enter LOTW login information");
                    sinceLOTW = "1900-01-01";
                    textBoxSince.Text = sinceLOTW;
                    richTextBox1.Clear();
                }
                else
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show(e.Message + "\n" + "Resetting Since date to 1900-01-01...please Refresh\nIf this is first time you've run this error is expected");
                    sinceLOTW = "1900-01-01";
                    textBoxSince.Text = sinceLOTW;
                    richTextBox1.Clear();
                }
            }
            catch (Exception e)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show(keeperFile + ":" + e.Message+"\n"+e.StackTrace);
            }
            textBoxSince.Text = lastDate.Substring(0, 4) + "-" + lastDate.Substring(4, 2) + "-" + lastDate.Substring(6, 2);
        }

        private void CheckForUpdate() {
            int currentVersion = 197; // Matches 3-digit version number
            try
            {
                string uri1 = "https://www.dropbox.com/s/s78p4i7yyng1rg9/LOTWQSL.ver?dl=1";
                HttpWebRequest connection = (HttpWebRequest)HttpWebRequest.Create(uri1);
                connection.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)connection.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string version = sr.ReadLine(); // 1st line is one-up version #
                string uri2 = sr.ReadLine(); // 2nd line is reference for user to download
                sr.Close();
                response.Close();
                int newVersion = Int32.Parse(version);
                if (newVersion > currentVersion)
                {
                    Update myform = new Update();
                    myform.ShowDialog();
                    /*
                    //MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    DialogResult result = MessageBox.Show("New version available\n"+uri2,
                        "LOTWQSL Update",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        0,
                        "http://www.qrz.com/db/w9mdb",
                        ""
                        );
                     * */

                }
                //else
                //{
                //    MessageBox.Show("No new version");
                //}
            }
            catch (Exception ex)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Error checking for update: "+ex.Message);
            }
        }

        private void ControlsDisable()
        {
            buttonRefresh.Enabled = false;
            buttonMap.Enabled = false;
            buttonGrid.Enabled = false;
            buttonHelp.Enabled = false;
            textBoxLogin.Enabled = false;
            textBoxSince.Enabled = false;
            textBoxEnd.Enabled = false;
            textBoxPassword.Enabled = false;
            Application.DoEvents();
        }

        private void ControlsEnable()
        {
            buttonRefresh.Enabled = true;
            buttonMap.Enabled = true;
            buttonGrid.Enabled = true;
            buttonHelp.Enabled = true;
            textBoxLogin.Enabled = true;
            textBoxSince.Enabled = true;
            textBoxEnd.Enabled = true;
            textBoxPassword.Enabled = true;
            Application.DoEvents();
        }

        private void DoADIF(ref string responseData)
        {
            bool qslMisMatch = false;
            bool qslMisMatchHeaderFlag = true;
            bool updateFile = false;
            var lines = responseData.Split(new[] { '\r', '\n' });
            int nLines = 0;
            int nNew = 0;
            int nQslMismatches = 0;
            int nHistory = history.Count();
            string callsign = String.Empty;
            string band = String.Empty;
            string mode = String.Empty;
            string modegroup = "MODEGROUP";
            string qsodate = String.Empty;
            string timeon = String.Empty;
            string creditGranted = String.Empty;
            string state = String.Empty;
            string country = String.Empty;
            string prefix = String.Empty;

            Cursor.Current = Cursors.WaitCursor;

            richTextBox1.Clear();
            bool qsl_rcvd = false;
            richTextBox1.AppendText("Processing ADIF file\n");
            this.Update();
            string QSODate = textBoxSince.Text;
            bool resetall = QSODate.Equals("1900-01-01") || QSODate.Substring(0, 1) == "!";

            if (QSODate.Substring(0, 1) == "!")
            {
                QSODate = QSODate.Substring(1);
                textBoxSince.Text = QSODate;
            }
            string QSODate2 = QSODate.Remove(7, 1); // we need this format to compare with the ADIF file
            QSODate2 = QSODate2.Remove(4, 1);
            qslMismatchStr = "";
            if (resetall)
            { // This indicates we're downloading the whole history so clear everything out
                // Remember our first date entered as the QSO date for base reference
                Properties.Settings.Default.QSODate = QSODate;
                Properties.Settings.Default.Save();

                try
                {
                    File.Delete(keeperFile);
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show(ex.Message);
                    errorStr = ex.Message;
                }
                dxccMixed.Clear();
                dxccChallenge.Clear();
                //states.Clear();
                allWAS.Clear();
                history.Clear();
                prefixes.Clear();
                callsigns.Clear();
                nqsls = 0;
                ndxccMixed = 0;
                nstates = 0;
                nprefixes = 0;
                ncallsigns = 0;
            }
            else
            {
                // if not a reset then set our QSO date to the saved value
                QSODate = Properties.Settings.Default.QSODate;
            }
            // No longer accurate with random ADIF files
            //richTextBox1.AppendText("QSO Start: " + QSODate + "\n");
            Boolean verbose = true;
            if (lines.Length > 10000)
            {
                verbose = false;
            }
            String updateFileData = "";
            int ndups = 0;
            foreach (string s in lines)
            {
                if (s.Contains("Generated"))
                {
                    richTextBox1.AppendText("Generated: " + s.Substring(13) + "\n");
                    // Gotta parse two format
                    // EQSL.CC gives this: Generated on Friday, August 8, 2014 at 18:57:47 PM UTC
                    // LOTW gives this: Generated at 2014-08-08 18:59:08
                    if (s.Contains("Generated on")) // ESQL.CC format
                    {
                        string[] tokens = s.Split(new[] { ',', ' ' });
                        string myDate = tokens[4] + " " + tokens[5] + ", " + tokens[7];
                        DateTime myDateTime = DateTime.Parse(myDate);
                        sinceLOTW = myDateTime.ToString("yyyy-MM-dd");
                    }
                    else // LOTW Format
                    {
                        if (textBoxEnd.Text.Length != 10) // Only if we didn't set the end date
                        {
                            sinceLOTW = s.Substring(13, 10);
                            textBoxSince.Text = sinceLOTW;
                        }
                    }
                }
                else if (s.Contains("LASTQSL:"))
                {
                    if (verbose)
                    {
                        richTextBox1.AppendText("Last QSL  : " + s.Substring(21) + "\n");
                    }
                }
                else if (s.Contains("<CALL:"))
                {
                    if ((++nLines % 100) == 0)
                    {
                        //Application.DoEvents();
                    }
                    if (!verbose && ((nLines % 1000) == 0))
                    {
                        richTextBox1.AppendText("#QSLs=" + nLines + "\r");
                        this.Update();
                    }
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    callsign = tokens[1];
                    if (!callsigns.Contains(callsign))
                    {
                        callsigns.Add(callsign);
                        //++ncallsigns;
                    }
                    // If we need to debug anyone in particular
                    //if (callsign.Equals("W9OAB"))
                    //{
                    //    MessageBox.Show("Got 'em");
                    //}
                }
                else if (s.Contains("<PFX:"))
                {
                    prefix = s.Substring(7);
                }
                else if (s.Contains("<BAND:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    band = tokens[1];
                }
                else if (s.Contains("<MODE:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    mode = tokens[1];
                }
                else if (s.Contains("<APP_LoTW_MODEGROUP"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    modegroup = tokens[1];
                }
                else if (s.Contains("<QSO_DATE:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    qsodate = tokens[1];
                }
                else if (s.Contains("<TIME_ON:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    timeon = tokens[1].Substring(0, 2) + ":" + tokens[1].Substring(2, 2) + ":" + tokens[1].Substring(4, 2);
                }
                else if (s.Contains("<QSL_RCVD:1>Y"))
                {
                    qsl_rcvd = true;
                }
                else if (s.Contains("<STATE:"))
                {
                    int offset = s.IndexOf(">");
                    state = s.Substring(offset + 1, 2);
                }
                /* this appears to be fixed as of 12/3/18 where some FT8 QSOs may contain LOTW_QSLMODE of DATA */
                else if (s.Contains("<APP_LoTW_QSLMODE:") && mode.Equals("DATA"))
                {
                    mismatches++;
                    Properties.Settings.Default.Mismatches = mismatches;
                    Properties.Settings.Default.Save();
                    qslMisMatch = true;
                    if (qslMisMatchHeaderFlag)  // only print this header once
                    {
                        String logHdr = "ERRORMSG\tQSODATE\tTIME\tCALL\tYOURMODE\tTHEIRMODE\n";
                        qslMismatchStr += logHdr;
                        richTextBox1.AppendText(logHdr);
                        qslMisMatchHeaderFlag = false;
                    }
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    String qslmode = tokens[1];
                    String logStr = "Mismatched mode\t" + qsodate + "\t" + timeon + "\t" + callsign + "\t" + mode + "  \t" + qslmode + "\n";
                    richTextBox1.AppendText("Mismatched mode\t" + qsodate + "\t" + timeon + "\t" + callsign + "\t" + mode + "  \t" + qslmode + "\n");
                    qslMismatchStr += logStr;

                    string filepath = appData + "\\lotw.adi";
                    StreamWriter writer = new StreamWriter(filepath);
                    writer.Write(responseData);
                    writer.Close();
                }
                else if (s.Contains("<COUNTRY:"))
                {
                    int offset = s.IndexOf(">");
                    country = s.Substring(offset + 1);
                    if (country.Contains("UNITED STATES OF AMERICA"))
                    {
                        country = "USA";
                    }
                    if (country.Contains("FEDERAL REPUBLIC OF GERMANY"))
                    {
                        country = "GERMANY";
                    }
                }
                // The GRANTED is apparently only created after you apply for an award
                // So it's pretty useless
                else if (s.Contains("<APP_LoTW_CREDIT_GRANTED:"))
                {
                    //int offset = s.IndexOf(">");
                    //creditGranted = s.Substring(offset+1);
                }
                else if (s.Contains("<eor>"))
                {
                    //if (band.Contains("70CM")) continue;
                    //String sinceLOTWCompare = sinceLOTW.Remove(7, 1);
                    //sinceLOTWCompare = sinceLOTWCompare.Remove(4, 1);
                    //int mycompare = qsodate.CompareTo(QSODate2);
                    //if (qsodate.CompareTo(QSODate2)<0)
                    //    continue;
                    string key = qsodate + " " + timeon + " " + band + " " + mode + " " + callsign;
                    string keyDisplay = qsodate + " " + timeon + " " + band.PadRight(4) + " " + mode.PadRight(5) + " " + callsign.PadRight(6);
                    // Update our WAS info
                    string wasinfo = "none";
                    bool newWAS = false;
                    bool newDXCC = false;
                    if (!state.Equals(String.Empty) && (country.Equals("USA") || country.Equals("ALASKA") || country.Equals("HAWAII")))
                    {
                        wasinfo = band + " " + mode + " " + state;
                        if (!allWAS.Contains(wasinfo) && !qslMisMatch)
                        {
                            MainWindow2.allWAS.Add(wasinfo);
                            //states.Add(wasinfo);
                            newWAS = true;
                            updateFile = true;
                        }
                    }
                    // Update our Country info and modegroup
                    string dxccinfo = "";
                    if (country.Length > 0) // Avoid /MM calls with no DXCC provided and such
                    {
                        dxccinfo = band + " " + modegroup + " " + country;
                        if (!dxccMixed.Contains(country))
                        {
                            dxccMixed.Add(country);
                            newDXCC = true;
                            if (verbose)
                            {
                                richTextBox1.AppendText("  New Country: " + country + "\n");
                            }
                        }
                    }
                    if (!history.Contains(key) && qsl_rcvd)
                    {
                        {
                            history.Add(key);
                            if (!state.Equals(String.Empty) && country.Equals("USA"))
                            {
                                //country = String.Empty;
                            }
                            if (verbose)
                            {
                                richTextBox1.AppendText(keyDisplay + " " + state + " " + country + "\n");
                            }
                            updateFile = true;
                        }
                        if (!qslMisMatch)
                        //else
                        {
                            nQslMismatches++;
                        }
                        // we can still update prefix since QSL match is good enough for that
                        if ((!prefix.Equals(String.Empty)) && (!prefixes.Contains(prefix)))
                        {
                            prefixes.Add(prefix);
                            if (verbose)
                            {
                                richTextBox1.AppendText("  New Prefix: " + prefix + "\n");
                            }
                        }


                    }
                    else
                    {
                        //richTextBox1.AppendText("Dup: " + key + " qsl_rcvd=" + qsl_rcvd + "\n");
                        ++ndups;
                    }
                    //if (!dxccMixed.Contains(country) && country.Length > 0)
                    //{
                    //    dxccMixed.Add(country);
                    //    if (verbose)
                    //    {
                    //        richTextBox1.AppendText("  New Country: " + country + "\n");
                    //    }
                    //}
                    if (band.Contains("M") && band.Length >= 2 && country.Length > 0)
                    {
                        if (dxccChallengeBands.Contains(band))
                        {
                            string dxccChallengeKey = band + " " + country;
                            if (!dxccChallenge.Contains(dxccChallengeKey))
                            {
                                dxccChallenge.Add(dxccChallengeKey);
                                if (verbose)
                                {
                                    richTextBox1.AppendText("  New DXCC Challenge entry: " + key + "\n");
                                }
                            }
                        }
                    }
                    if (newWAS)
                    {
                        updateFile = true;
                        if (verbose)
                        {
                            richTextBox1.AppendText("  New WAS: " + wasinfo + "\n");
                        }
                        MapForm form2 = new MapForm();
                        HashSet<string> remain = form2.StatesRemain(allWAS);
                        form2.Dispose();
                        if (remain.Count == 0)
                        {
                            if (verbose)
                            {
                                richTextBox1.AppendText("  **** WAS Award Achieved ****\n");
                            }
                        }
                    }
                    if (newDXCC)
                    {
                        //if (verbose)
                        //{
                        //    richTextBox1.AppendText("  New DXCC: " + dxccinfo + "\n");
                        //}
                    }
                    if (!File.Exists(keeperFile))
                    {
                        StreamWriter newfile = File.AppendText(keeperFile);
                        newfile.WriteLine("Version 1.1");
                        newfile.Close();
                        updateFile = true;
                    }
                    if (updateFile)
                    {
                        //StreamWriter file = File.AppendText(keeperFile);
                        //file.WriteLine(key + "," + wasinfo + "," + dxccinfo + "," + prefix);
                        //file.Close();
                        updateFileData += key + "," + wasinfo + "," + dxccinfo + "," + prefix + "\n";
                        ++nNew;
                    }
                    else
                    {
                        // richTextBox1.AppendText("Skipping " + key + "," + wasinfo + "," + dxccinfo + "," + prefix+"\n");
                    }
                    qsl_rcvd = false;
                    creditGranted = String.Empty;
                    state = String.Empty;
                    country = String.Empty;
                    qslMisMatch = false;
                }
            }
            if (updateFile)
            {
                StreamWriter file = File.AppendText(keeperFile);
                //file.WriteLine(key + "," + wasinfo + "," + dxccinfo + "," + prefix);
                file.Write(updateFileData);
                file.Close();
            }

            //if (nLines >= 500) richTextBox1.Undo();
            if (nLines == 0)
            {
                nLines = nqsls; // in case we have an error retrieving data
            }
            //else
            //{
            //    nLines = nqsls + nQslMismatches + nNew;
            //}
            richTextBox1.AppendText(nLines + " QSLs in ADIF data\n");
            richTextBox1.AppendText(ndups + " Duplicates\n");
            if (nNew == 0)
            {
                richTextBox1.AppendText("No New QSLs\n");
            }
            string bumpup = (history.Count() - nHistory).ToString();
            richTextBox1.AppendText(history.Count() + " QSLs \t" + "+" + bumpup + "\n");

            bumpup = (ncallsigns == callsigns.Count()) ? "+0" : ("+" + (callsigns.Count() - ncallsigns));
            if (ncallsigns == 0) ncallsigns = callsigns.Count();
            richTextBox1.AppendText(ncallsigns + " Calls \t" + bumpup + "\n");

            bumpup = (nprefixes == prefixes.Count()) ? "+0" : ("+" + (prefixes.Count() - nprefixes));
            richTextBox1.AppendText(prefixes.Count() + " prefix entries\t" + bumpup + "\n");

            bumpup = (nstates == allWAS.Count()) ? "+0" : ("+" + (allWAS.Count() - nstates));
            richTextBox1.AppendText(allWAS.Count() + " was entries\t" + bumpup + "\n");

            //bumpup = (ndxcc == dxcc.Count()) ? "+0" : ("+" + (dxcc.Count() - ndxcc));
            //richTextBox1.AppendText(dxcc.Count() + " dxcc entries\t" + bumpup + "\n");

            bumpup = (ndxccMixed == dxccMixed.Count()) ? "+0" : ("+" + (dxccMixed.Count() - ndxccMixed));
            richTextBox1.AppendText(dxccMixed.Count() + " DXCC entries\t" + bumpup + "\n");

            bumpup = (ndxccChallenge == dxccChallenge.Count()) ? "+0" : ("+" + (dxccChallenge.Count() - ndxccChallenge));
            richTextBox1.AppendText(dxccChallenge.Count + " DXCC Challenge entries\t" + bumpup + "\n");

            nqsls = nLines;
            nstates = allWAS.Count();
            nprefixes = prefixes.Count();
            ndxccMixed = dxccMixed.Count();
            ncallsigns = callsigns.Count();
            ndxccChallenge = dxccChallenge.Count();

            if (nLines == 0)
            {
                richTextBox1.AppendText("Check your login\n");
            }
            if (qslMismatchStr.Count() > 0)
            {
                SaveFileDialog saveFile = new SaveFileDialog()
                {
                    FileName = "LOTW_Mode_Mismatches.txt",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFile.FileName))
                            sw.Write(qslMismatchStr);
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message);
                    }
                }

            }
            Cursor.Current = Cursors.Default;
        }
        /*
        private void getEQSL()
        {
            //string uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=" + mylogin + "&password=" + mypassword + "&qso_query=1&qso_qsl=yes&qso_qslsince=" + sinceLOTW + "&qso_qsldetail=yes";
            string uri = "http://eqsl.cc/qslcard/DownloadInBox.cfm?UserName=w9mdb&Password=xxxxx&QTHNickname=Home&RcvdSince=201408080000";
            String responseData = String.Empty;
            try
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("Requesting EQSL.CC data...\n");
                //ActiveForm.Update();
                WebClient client = new WebClient();
                // Without adding a header it seemed like LOTW was not liking very many queries together while debugging
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0");
                Stream data = client.OpenRead(uri);
                data.ReadTimeout = 60 * 5 * 1000; // 5 minute timeout
                richTextBox1.AppendText("Request accepted...reading ADIF data\n");
                //ActiveForm.Update();
                StreamReader reader = new StreamReader(data);
                responseData = reader.ReadToEnd();
                data.Close();
                reader.Close();
                // Now we get the link for the ADI file from the response we got
                int offset = responseData.IndexOf("downloadedfiles/");
                offset += responseData.Substring(offset).IndexOf("/");
                int offset2 = responseData.Substring(offset).IndexOf('\"');
                string s = responseData.Substring(offset, offset2);
                uri = "http://eqsl.cc/qslcard/downloadedfiles"+s;
                //string x = "http://eqsl.cc/qslcard/downloadedfiles/2WMWB4618.adi"
                data = client.OpenRead(uri);
                reader = new StreamReader(data);
                responseData = reader.ReadToEnd();
                data.Close();
                reader.Close();
            }
            catch (Exception e)
            {
                richTextBox1.AppendText(e.Message + "\n");
            }
            richTextBox1.WordWrap = false;
            if (responseData.Length == 0)
            {
                richTextBox1.AppendText("Invalid reponse from EQSL,CC...\nTry again\n");
                return;
            }
            string filepath = appData + "\\eqsl.adi";
            StreamWriter writer = new StreamWriter(filepath);
            writer.Write(responseData);
            writer.Close();
            doADIFeQSL(ref responseData);
        }
         * */

        private string ReadAllLines(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            string line = string.Empty;
            int n = 0;
            richTextBox1.AppendText("Downloading log...\r");
            
            while ((line = reader.ReadLine()) != null)
            {
                sb.Append(line+"\n");
                ++n;
                if ((n % 1000) == 0)
                {
                    if (n > 1000) richTextBox1.Undo();
                    richTextBox1.AppendText("# lines="+n+"\r");
                    try
                    {
                        this.Update();
                    }
                    catch (Exception e)
                    {
                        errorStr = e.Message;
                    }
                }
            }
            if (n >= 1000) richTextBox1.Undo();
            return sb.ToString();
        }

        private void GetLOTW()
        {
            sinceLOTW = textBoxSince.Text;
            string startDate = "";
            if (sinceLOTW.Substring(0, 1) == "!") // then we reset things
            {
                sinceLOTW = sinceLOTW.Substring(1);
                startDate = "&qso_qsorxsince=" + sinceLOTW;
                richTextBox1.AppendText("New log starts at "+startDate);
            }
            else {
                Type t = this.GetType();
                System.Reflection.PropertyInfo p = t.GetProperty("QSODate");
                if (p != null)
                {
                    startDate = "&qso_qsorxsince=" + Properties.Settings.Default.QSODate;
                }
                else
                {
                    startDate = "&qso_qsorxsince=" + sinceLOTW;
                }

            }
            //string mylogin = login.ToString();
            //string mypassword = password.ToString();
            // This one producs only some records with all the detail
            // https://lotw.arrl.org/lotwuser/lotwreport.adi?login=w9mdb&password=xxxxxxxx&qso_query=1&qso_qsl=no&qso_startdate=1900-01-01&qso_qsldetail=yes
            // This one gives all QSLs with detail for each that includes state and country
            // https://lotw.arrl.org/lotwuser/lotwreport.adi?login=w9mdb&password=xxxxxxxx&qso_query=1&qso_qsl=yes&qso_startdate=1900-01-01&qso_qsldetail=yes
            //string uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=" + mylogin + "&password=" + mypassword + "&qso_query=1&qso_qsl=yes&qso_startdate=1900-01-01&qso_qsldetail=yes";
            //string uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=w9mdb&password=xxxxxxxx&qso_query=1&qso_qsl=yes&qso_qslsince="2014-06-13 00:00:00"&qso_qsldetail=yes
            string[] tokens = Regex.Split(mylogin, "[+@]");
            string login = tokens[0];
            String mypassword_uri = Uri.EscapeDataString(mypassword);
            string uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=" + login + "&password=" + mypassword_uri + "&qso_query=1&qso_qsl=yes&qso_qsldetail=yes&qso_qslsince=" + sinceLOTW;
            //string ownCall = "&qso_owncall="+login;
            string endDate = "";
            string ownCall = "";

            if (mylogin.Contains("@")) // then we add another filter to the URI
            { // this will restrict records to the requested call sign
               ownCall = "&qso_owncall=" + tokens[1];
            }
            if (textBoxEnd.Text.Length == 10)
            {
                endDate = "&qso_enddate=" + textBoxEnd.Text;
                //uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=" + mylogin + "&password=" + mypassword + "&qso_query=1&qso_qsl=yes&qso_qsldetail=yes&qso_qslsince=" + sinceLOTW + "&qso_startdate=" + sinceLOTW + "&qso_enddate=" + textBoxEnd.Text;
                //uri = "https://lotw.arrl.org/lotwuser/lotwreport.adi?login=" + mylogin + "&password=" + mypassword + "&qso_query=1&qso_qsl=yes&qso_qsldetail=yes&qso_qslsince=" + sinceLOTW + "&qso_enddate=" + textBoxEnd.Text+ownCall;
            }
            uri += ownCall + startDate + endDate; // add these if they were filled in
            String responseData = String.Empty;
            //string[] lines = new string[] {""};
            try
            {
                richTextBox1.Clear();
                DateTime mydate = DateTime.UtcNow;
                richTextBox1.AppendText(mydate.ToString() + "\n");
                richTextBox1.AppendText("Requesting LOTW data...please be patient...\nThis can take several minutes\n");
                richTextBox1.AppendText(uri + "\n");
                Clipboard.SetText(uri);
                richTextBox1.AppendText("URL copied to clipboard\n");
                try
                {
                    MainWindow2.ActiveForm.Update();
                }
                catch (Exception e)
                {
                    errorStr = e.Message;
                }
                MyWebClient client = new MyWebClient();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12;
                //       | SecurityProtocolType.Ssl3;                // Without adding a header it seemed like LOTW was not liking very many queries together while debugging
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0");
                System.Net.WebRequest.DefaultWebProxy = WebRequest.DefaultWebProxy;
                client.Proxy = System.Net.WebRequest.DefaultWebProxy;
                
                using (Stream data = client.OpenRead(uri))
                {
                    richTextBox1.AppendText("Request accepted...reading ADI data\n");
                    try
                    {
                        MainWindow2.ActiveForm.Update();
                    }
                    catch (Exception e)
                    {
                        errorStr = e.Message;
                    }
                    data.ReadTimeout = 60 * 120 * 1000; // 2 hour timeout
                    using (BufferedStream buffer = new BufferedStream(data,4096*64))
                    {
                        using (StreamReader reader = new StreamReader(buffer))
                        {
                            responseData = ReadAllLines(reader);
                        }
                    }
                
                }
            }
            catch (Exception e)
            {
                richTextBox1.AppendText(e.Message+"\n");
            }
            DateTime mydate2 = DateTime.UtcNow;
            richTextBox1.AppendText(mydate2.ToString());
            richTextBox1.WordWrap = false;
            if (responseData.Length == 0)
            {
                richTextBox1.AppendText(" Invalid reponse from LOTW...\nTry again\n");
                return;
            }
            string filepath = appData + "\\lotw.adi";
            StreamWriter writer = new StreamWriter(filepath);
            writer.Write(responseData);
            writer.Close();
            filepath = appData + "\\lotw.log";
            writer = new StreamWriter(filepath,true);
            writer.WriteLine("UTC   "+ DateTime.UtcNow+"\n"+"Local "+DateTime.Now);
            // skip the login/passwd fields for the log info
            writer.WriteLine(uri.Substring(uri.IndexOf("&qso_query")));
            writer.Write(responseData);
            writer.WriteLine("===============================");
            writer.Close();
            if (!responseData.Contains("<CALL"))
            {
                if (responseData.Contains("forgot"))
                {
                    richTextBox1.AppendText("\nLOTW returned an error\n");
                    richTextBox1.AppendText("Looks like wrong password\n");
                }
                else if (responseData.Contains("<APP_LoTW_NUMREC:1>0"))
                {
                    richTextBox1.AppendText("\nNo new QSLs\n");
                }
                else {
                    richTextBox1.AppendText("\nUnknown LOTW error\nFile will display in browser\n");
                    string htmlfilepath = appData + "\\lotw.htm";
                    writer = new StreamWriter(htmlfilepath);
                    writer.Write(responseData);
                    writer.Close();
                    System.Diagnostics.Process.Start(htmlfilepath);
                    richTextBox1.AppendText("Another Refresh may fix it\n");
                }
                return;
            }
            DoADIF(ref responseData);
            if (textBoxEnd.Text.Length == 10)
            {
                textBoxSince.Text = textBoxEnd.Text;
                sinceLOTW = textBoxEnd.Text;
            }
        }

        private void Login_TextChanged(object sender, EventArgs e)
        {
            // no action here -- wait until we leave
        }

        private void Login_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.login = mylogin;
            Properties.Settings.Default.Save();
            if (!textBoxLogin.Text.Equals(mylogin)) // Only do this is if we change what's already there
            {
                mylogin = textBoxLogin.Text;
                string call1 = mylogin; // by default
                string call2 = string.Empty;
                // we've change logins -- check if asking for added callsign filter
                if (mylogin.Contains("+"))
                {
                    string[] tokens = Regex.Split(mylogin,@"\+");
                    call1 = tokens[0];
                    call2 = tokens[1];
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    DialogResult result = MessageBox.Show("This will merge "+call2+" with "+call1+"?\nAre you sure?\nUse callsign@login format to keep separate...see Help file","Merge",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
                    if (result == DialogResult.No) return;
                    mergeCalls = true;
                }
                else if (mylogin.Contains("@")) // the the log willb be separate
                {
                    mergeCalls = false;
                }
                if (mergeCalls) // then we just keep the main login
                {
                    keeperFile = appData + "\\" + call1.Replace('/','_') + ".txt";
                }
                else // we'll keep the call1+call2 in a separate log
                {
                    keeperFile = appData + "\\" + mylogin.Replace('/','_') + ".txt";
                }
                KeeperLoad(keeperFile);
                Properties.Settings.Default.login = mylogin;
                Properties.Settings.Default.Save();
            }
        }

        private void Password_Get(String call, String passwordList)
        {
            passwordList = "w9mdb:mdb001;w9mdb@w9mdb:mdb002";
            String[] tokens = passwordList.Split(';');
            foreach (String s in tokens)
            {

            }
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            mypassword = textBoxPassword.Text;
            Properties.Settings.Default.password = mypassword;
            Properties.Settings.Default.Save();
        }

        private void SinceLOTWBox_Leave(object sender, EventArgs e)
        {
            try
            {
                string tmp = textBoxSince.Text;
                tmp = tmp.ToUpper();
                textBoxSince.Text = tmp;
                if (tmp.Substring(0,1)=="!") tmp=tmp.Substring(1);
                DateTime dt = DateTime.Parse(tmp);
                sinceLOTW = dt.ToString("yyyy-MM-dd");
                //textBoxSince.Text = sinceLOTW;
            }
            catch {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Error parsing date "+textBoxSince.Text+"\nExpecting YYYY-MM-DD");
            }
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            ControlsDisable();
            ADIFChoose choose = new ADIFChoose()
            {
                StartPosition = FormStartPosition.CenterParent
            };
            choose.ShowDialog();
            choose.Hide();
            if (choose.GetChoice() == ADIFChoose.Source.CANCEL)
            {
                ControlsEnable();
                return;
            }
            if (choose.GetChoice() != ADIFChoose.Source.LOTW)
            {
                OpenFileDialog openFile = new OpenFileDialog()
                {
                    Filter = "Text files (*.adi)|*.adi|All files (*.*)|*.*"
                };
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    FileStream adif = new FileStream(openFile.FileName,FileMode.Open,FileAccess.Read);
                    StreamReader reader = new StreamReader(adif);
                    String responseData = reader.ReadToEnd();
                        
                    DoADIF(ref responseData);
                    ControlsEnable();
                    return;
                }
                else
                {
                    ControlsEnable();
                    return;
                }
            }
            if (textBoxSince.Text.Equals("1900-01-01"))
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                if (MessageBox.Show("1900-01-01 will download your entire LOTW history...are you sure?", "New Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    ControlsEnable();
                    return;
                }
                mismatches = 0;
                Properties.Settings.Default.Mismatches = mismatches;
                Properties.Settings.Default.Save();
            }
            if (textBoxSince.Text.Substring(0,1).Equals("!"))
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                if (MessageBox.Show("Exclamation point will set this date as the earliest date for all QSOs...are you sure?", "New Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    ControlsEnable();
                    return;
                }
            }
            Cursor.Current = Cursors.WaitCursor;
            GetLOTW();
            if (gridForm.IsHandleCreated)
            {
                gridForm.FillGrid();
            }
            Cursor.Current = Cursors.Default;
            ControlsEnable();
        }

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            try
            {
                help.Show();
                help.WindowState = FormWindowState.Normal;
            }
            catch (Exception)
            {
                help = new HelpForm();
                help.Show();
            }
        }

        private void ButtonMap_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            mapForm.Show();
            if (mapForm.WindowState == FormWindowState.Minimized)
            {
                mapForm.WindowState = FormWindowState.Normal;
            }
            mapForm.BringToFront();
            Cursor.Current = Cursors.Default;
        }

        private void InstallUpdateSyncWithInfo()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                try
                {
                    info = ad.CheckForDetailedUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                        DialogResult dr = MessageBox.Show("An update is available. Would you like to update the application now?", "Update Available", MessageBoxButtons.OKCancel);
                        if (!(DialogResult.OK == dr))
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                        MessageBox.Show("This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update Available", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            ad.Update();
                            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                            MessageBox.Show("The application has been upgraded, and will now restart.");
                            Application.Restart();
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                            MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                            return;
                        }
                    }
                }
            }
        }

        private void ButtonGrid_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            gridForm.Show();
            if (gridForm.WindowState == FormWindowState.Minimized)
            {
                gridForm.WindowState = FormWindowState.Normal;
            }
            gridForm.BringToFront();
            Cursor.Current = Cursors.Default;
        }

        private void TextBoxEnd_Leave(object sender, EventArgs e)
        {
            if (textBoxEnd.Text.Length == 0) return;
            try
            {
                DateTime dt = DateTime.Parse(textBoxEnd.Text);
                endLOTW = dt.ToString("yyyy-MM-dd");
                textBoxEnd.Text = endLOTW;
            }
            catch
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("Error parsing date " + textBoxEnd.Text + "\nExpecting YYYY-MM-DD");
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Thread oThread = new Thread(new ThreadStart(CheckForUpdate));
            oThread.Start();
        }

//        private void Form1_SizeChanged(object sender, EventArgs e)
//        {
//        }

//        private void Form1_LocationChanged(object sender, EventArgs e)
//        {
//        }


        /*
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            // 
            // Form1
            // 
            this.Headers = ((System.Net.WebHeaderCollection)(resources.GetObject("$this.Headers")));

        }
         * */
    }
    
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest w = base.GetWebRequest(address);
            w.Timeout = 60 * 60 * 10 * 1000; // Ten hour timeout
            return w;
        }
    }
    
}
