﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOTWQSL
{
    public partial class MainWindow : Form
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
        HashSet<string> history;
        HashSet<string> callsigns;
        public static HashSet<string> states; // List contains State/Band/Mode
        HashSet<string> dxcc;
        HashSet<string> countries;
        HashSet<string> prefixes;
        public static Dictionary<string, Dictionary<string, int>> popularStates;
        //Dictionary<string,int> popularCalls;
        int nstates = 0;
        int ndxcc = 0;
        int ncountries = 0;
        int nprefixes = 0;
        int nqsls = 0;
        int ncallsigns = 0;
        Boolean mergeCalls = true; // if user does call+call login do we want to merge or keep separate?

        public MainWindow()
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
            keeperFile = appData + "\\" + mylogin + ".txt";
            if (mylogin.Contains("+"))
            { // then this is a merged file
                string[] tokens = Regex.Split(mylogin, "[@+]");
                keeperFile = appData + "\\" + tokens[0] + ".txt";
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
            states = new HashSet<string>();
            dxcc = new HashSet<string>();
            countries = new HashSet<string>();
            prefixes = new HashSet<string>();
            callsigns = new HashSet<string>();
            popularStates = new Dictionary<string,Dictionary<string,int>>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.RestoreBounds.Top;
            this.Left = Properties.Settings.Default.RestoreBounds.Left;
            this.Height = Properties.Settings.Default.RestoreBounds.Height;
            this.Width = Properties.Settings.Default.RestoreBounds.Width;
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
            textBoxSince.Text = sinceLOTW;
            textBoxLogin.Text = mylogin; // this will force keeperLoad()
            password.Text = mypassword;
            keeperLoad(keeperFile);
            timer1.Interval = 3000;
            timer1.Start();
            //Thread oThread = new Thread(new ThreadStart(checkForUpdate));
            //oThread.Start();
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
            toolTip1.SetToolTip(this.textBoxLogin, "Your LOTW login name");
            toolTip1.SetToolTip(this.labelLogin, "Your LOTW login name");
            toolTip1.SetToolTip(this.password, "Your LOTW password");
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

        void addToPopular(string state,string callsign)
        {
            if (!popularStates.ContainsKey(state))
            {
                Dictionary<string,int> newdict = new Dictionary<string,int>();
                newdict.Add(callsign, 1);
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

        void keeperLoad(string callsign)
        {
            string lastDate = "19000101";
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
                    HashSet<string> uniqueLines = new HashSet<string>();
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!uniqueLines.Contains(line)) {
                            uniqueLines.Add(line);
                            ++nqsls;
                        }
                        string thisDate = line.Substring(0, 8);
                        if (string.Compare(thisDate, lastDate) > 0)
                        {
                            lastDate = thisDate;
                            sinceLOTW = lastDate;
                        }
                        var tokens = line.Split(new[] { ',', '\r', '\n' });
                        history.Add(tokens[0]);
                        // [0] = "20131203 13:19 40M JT65 AJ4HW"
                        var tokens1 = tokens[0].Split(' ');
                        callsign = tokens1[4];
                        if (!callsigns.Contains(callsign))
                            callsigns.Add(callsign);
                        var tokens2 = tokens[2].Split(' ');
                        string country = tokens2[2]; // should be country info
                        if (tokens2.Count() > 3)
                        {
                            for (int i = 3; i < tokens2.Count(); ++i)
                            {
                                country += " " + tokens2[i];
                            }
                        }
                        if (!countries.Contains(country))
                        {
                            countries.Add(country);
                        }
                        tokens1 = tokens[1].Split(' ');
                        if (tokens1.Count() < 3) continue;
                        string state = tokens1[2];
                        addToPopular(state,callsign);
                        if ((!states.Contains(tokens[1])) && (!tokens[1].Equals("none")))
                        {
                            states.Add(tokens[1]);
                        }
                        if (!prefixes.Contains(tokens[3]))
                        {
                            prefixes.Add(tokens[3]);
                        }
                        //if (!dxcc.Contains(tokens[2]))
                        //{
                        //    dxcc.Add(tokens[2]);
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
                richTextBox1.AppendText(callsigns.Count() + " callsigns\n");
                richTextBox1.AppendText(prefixes.Count() + " prefixes\n");
                //richTextBox1.AppendText(states.Count() + " state entries\n");
                //richTextBox1.AppendText(dxcc.Count() + " dxcc entries\n");
                richTextBox1.AppendText(countries.Count() + " countries\n");
                ncallsigns = callsigns.Count();
                nstates = states.Count();
                nprefixes = prefixes.Count();
                ndxcc = dxcc.Count();
                ncountries = countries.Count();
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
                MessageBox.Show(keeperFile + ":" + e.Message);
            }
            textBoxSince.Text = lastDate.Substring(0, 4) + "-" + lastDate.Substring(4, 2) + "-" + lastDate.Substring(6, 2);
        }

        private void checkForUpdate() {
            int currentVersion = 190; // Matches 3-digit version number
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

        private void doADIF(ref string responseData)
        {
            var lines = responseData.Split(new[] { '\r', '\n' });
            int nLines = 0;
            int nNew = 0;
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
            richTextBox1.Clear();
            bool qsl_rcvd = false;
            richTextBox1.AppendText("Processing ADIF file\n");
            this.Update();
            string QSODate = textBoxSince.Text;
            bool resetall = QSODate.Equals("1900-01-01") || QSODate.Substring(0,1)=="!";
            if (QSODate.Substring(0, 1) == "!")
            {
                QSODate = QSODate.Substring(1);
                textBoxSince.Text = QSODate;
            }

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
                countries.Clear();
                dxcc.Clear();
                states.Clear();
                history.Clear();
                prefixes.Clear();
                callsigns.Clear();
                nqsls = 0;
                ncountries = 0;
                ndxcc = 0;
                nstates = 0;
                nprefixes = 0;
                ncallsigns = 0;
            }
            else
            {
                // if not a reset then set our QSO date to the saved value
                QSODate = Properties.Settings.Default.QSODate;
            }
            richTextBox1.AppendText("QSO Start: " + QSODate + "\n");
            Boolean verbose = true;
            if (lines.Length > 2000)
            {
                verbose = false;
            }
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
                if (s.Contains("LASTQSL:"))
                {
                    if (verbose)
                    {
                        richTextBox1.AppendText("Last QSL  : " + s.Substring(21) + "\n");
                    }
                }
                if (s.Contains("<CALL:"))
                {
                    ++nLines;
                    if (!verbose && ((nLines % 500) == 0))
                    {
                        //if (nLines > 500) richTextBox1.Undo();
                        richTextBox1.AppendText("#QSLs=" + nLines + "\r");
                        this.Update();
                    }
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    callsign = tokens[1];
                    if (!callsigns.Contains(callsign))
                    {
                        callsigns.Add(callsign);
                        ++ncallsigns;
                    }
                }
                if (s.Contains("<PFX:"))
                {
                    prefix = s.Substring(7);
                }
                if (s.Contains("<BAND:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    band = tokens[1];
                }
                if (s.Contains("<MODE:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    mode = tokens[1];
                }
                if (s.Contains("<APP_LoTW_MODEGROUP"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    modegroup = tokens[1];
                }
                if (s.Contains("<QSO_DATE:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    qsodate = tokens[1];
                }
                if (s.Contains("<TIME_ON:"))
                {
                    var tokens = s.Split(new[] { '>', '\r', '\n' });
                    timeon = tokens[1].Substring(0, 2) + ":" + tokens[1].Substring(2, 2) + ":" + tokens[1].Substring(4, 2);
                }
                if (s.Contains("<QSL_RCVD:1>Y"))
                {
                    qsl_rcvd = true;
                }
                if (s.Contains("<STATE:"))
                {
                    int offset = s.IndexOf(">");
                    state = s.Substring(offset + 1);
                }
                if (s.Contains("<COUNTRY:"))
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
                if (s.Contains("<APP_LoTW_CREDIT_GRANTED:"))
                {
                    //int offset = s.IndexOf(">");
                    //creditGranted = s.Substring(offset+1);
                }
                if (s.Contains("<eor>"))
                {
                    string key = qsodate + " " + timeon + " " + band + " " + mode + " " + callsign;
                    // Update our WAS info
                    string wasinfo = "none";
                    bool newWAS = false;
                    bool newDXCC = false;
                    bool updateFile = false;
                    if (!state.Equals(String.Empty) && (country.Equals("USA") || country.Equals("ALASKA") || country.Equals("HAWAII")))
                    {
                        wasinfo = band + " " + mode + " " + state;
                        if (!states.Contains(wasinfo))
                        {
                            states.Add(wasinfo);
                            newWAS = true;
                            updateFile = true;
                        }
                    }
                    // Update our Country info and modegroup
                    string dxccinfo = band + " " + modegroup + " " + country;
                    if (!dxcc.Contains(dxccinfo))
                    {
                        dxcc.Add(dxccinfo);
                        newDXCC = true;
                    }
                    if (!history.Contains(key) && qsl_rcvd)
                    {
                        history.Add(key);
                        if (!state.Equals(String.Empty) && country.Equals("USA"))
                        {
                            country = String.Empty;
                        }
                        if (verbose)
                        {
                            richTextBox1.AppendText(key + " " + state + country + "\n");
                        }
                        updateFile = true;
                        if ((!prefix.Equals(String.Empty)) && (!prefixes.Contains(prefix)))
                        {
                            prefixes.Add(prefix);
                            if (verbose)
                            {
                                richTextBox1.AppendText("  New Prefix: " + prefix + "\n");
                            }
                        }


                    }
                    if (!countries.Contains(country) && country.Length > 0)
                    {
                        countries.Add(country);
                        if (verbose)
                        {
                            richTextBox1.AppendText("  New Country: " + country + "\n");
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
                        HashSet<string> remain = form2.statesRemain(states);
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
                        StreamWriter file = File.AppendText(keeperFile);
                        file.WriteLine(key + "," + wasinfo + "," + dxccinfo + "," + prefix);
                        file.Close();
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
                }
            }
            if (nLines >= 500) richTextBox1.Undo();
            if (nLines == 0)
            {
                nLines = nqsls; // in case we have an error retrieving data
            }
            else
            {
                nLines = nqsls + nNew;
            }
            if (nNew == 0)
            {
                richTextBox1.AppendText("No New QSLs\n");
            }
            string bumpup = (nqsls == nLines) ? "+0" : ("+" + (nLines - nqsls));
            richTextBox1.AppendText(nLines + " QSLs \t" + bumpup + "\n");

            bumpup = (ncallsigns == callsigns.Count()) ? "+0" : ("+" + (callsigns.Count() - ncallsigns));
            richTextBox1.AppendText(ncallsigns + " Calls \t" + bumpup + "\n");

            bumpup = (nprefixes == prefixes.Count()) ? "+0" : ("+" + (prefixes.Count() - nprefixes));
            richTextBox1.AppendText(prefixes.Count() + " prefix entries\t" + bumpup + "\n");

            bumpup = (nstates == states.Count()) ? "+0" : ("+" + (states.Count() - nstates));
            richTextBox1.AppendText(states.Count() + " was entries\t" + bumpup + "\n");

            //bumpup = (ndxcc == dxcc.Count()) ? "+0" : ("+" + (dxcc.Count() - ndxcc));
            //richTextBox1.AppendText(dxcc.Count() + " dxcc entries\t" + bumpup + "\n");

            bumpup = (ncountries == countries.Count()) ? "+0" : ("+" + (countries.Count() - ncountries));
            richTextBox1.AppendText(countries.Count() + " country entries\t" + bumpup + "\n");

            nqsls = nLines;
            nstates = states.Count();
            nprefixes = prefixes.Count();
            ndxcc = dxcc.Count();
            ncountries = countries.Count();
            ncallsigns = callsigns.Count();
            if (nLines == 0)
            {
                richTextBox1.AppendText("Check your login\n");
            }
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
                int offset = responseData.IndexOf("downloadedfiles");
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
            string filepath = appData + "\\lotw.adi";
            StreamWriter writer = new StreamWriter(filepath);
            writer.Write(responseData);
            writer.Close();
            doADIF(ref responseData);
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

        private void getLOTW()
        {
            sinceLOTW = textBoxSince.Text;
            string startDate = "";
            if (sinceLOTW.Substring(0, 1) == "!") // then we reset things
            {
                sinceLOTW = sinceLOTW.Substring(1);
                startDate = "&qso_qsorxsince=" + sinceLOTW;
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
                DateTime mydate = DateTime.Now;
                richTextBox1.AppendText(mydate.ToString() + "\n");
                richTextBox1.AppendText("Requesting LOTW data...please be patient...\nThis can take several minutes\n");
                richTextBox1.AppendText(uri + "\n");
                Clipboard.SetText(uri);
                richTextBox1.AppendText("URL copied to clipboard\n");
                try
                {
                    MainWindow.ActiveForm.Update();
                }
                catch (Exception e)
                {
                    errorStr = e.Message;
                }
                myWebClient client = new myWebClient();
                // Without adding a header it seemed like LOTW was not liking very many queries together while debugging
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0");
                System.Net.WebRequest.DefaultWebProxy = null;
                client.Proxy = System.Net.WebRequest.DefaultWebProxy;
                using (Stream data = client.OpenRead(uri))
                {
                    richTextBox1.AppendText("Request accepted...reading ADI data\n");
                    try
                    {
                        MainWindow.ActiveForm.Update();
                    }
                    catch (Exception e)
                    {
                        errorStr = e.Message;
                    }
                    data.ReadTimeout = 60 * 120 * 1000; // 2 hour timeout
                    using (BufferedStream buffer = new BufferedStream(data,4096*16))
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
            DateTime mydate2 = DateTime.Now;
            richTextBox1.AppendText(mydate2.ToString());
            richTextBox1.WordWrap = false;
            if (responseData.Length == 0)
            {
                richTextBox1.AppendText("Invalid reponse from LOTW...\nTry again\n");
                return;
            }
            string filepath = appData + "\\lotw.adi";
            StreamWriter writer = new StreamWriter(filepath);
            writer.Write(responseData);
            writer.Close();
            if (!responseData.Contains("<CALL"))
            {
                if (responseData.Contains("forgot"))
                {
                    richTextBox1.AppendText("\nLOTW returned an error\n");
                    richTextBox1.AppendText("Looks like wrong password\n");
                }
                else
                {
                    richTextBox1.AppendText("\nLOTW returned an unknown error\nDisplaying result in web browser\n");
                    string htmlfilepath = appData + "\\lotw.htm";
                    writer = new StreamWriter(htmlfilepath);
                    writer.Write(responseData);
                    writer.Close();
                    System.Diagnostics.Process.Start(htmlfilepath);
                    richTextBox1.AppendText("Another Refresh may fix it\n");
                }
                return;
            }
            doADIF(ref responseData);
            if (textBoxEnd.Text.Length == 10)
            {
                textBoxSince.Text = textBoxEnd.Text;
                sinceLOTW = textBoxEnd.Text;
            }
        }

        private void login_TextChanged(object sender, EventArgs e)
        {
            // no action here -- wait until we leave
        }

        private void login_Leave(object sender, EventArgs e)
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
                    keeperFile = appData + "\\" + call1 + ".txt";
                }
                else // we'll keep the call1+call2 in a separate log
                {
                    keeperFile = appData + "\\" + mylogin + ".txt";
                }
                keeperLoad(keeperFile);
                Properties.Settings.Default.login = mylogin;
                Properties.Settings.Default.Save();
            }
        }

        private void password_TextChanged(object sender, EventArgs e)
        {
            mypassword = password.Text;
            Properties.Settings.Default.password = mypassword;
            Properties.Settings.Default.Save();
        }

        private void sinceLOTWBox_Leave(object sender, EventArgs e)
        {
            try
            {
                string tmp = textBoxSince.Text;
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

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            buttonRefresh.Enabled = false;
            if (textBoxSince.Text.Equals("1900-01-01"))
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                if (MessageBox.Show("1900-01-01 will download your entire LOTW history...are you sure?", "New Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }
            Cursor.Current = Cursors.WaitCursor;
            getLOTW();
            buttonRefresh.Enabled = true;
            if (gridForm.IsHandleCreated)
            {
                gridForm.fillGrid();
            }
            Cursor.Current = Cursors.Default;
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Michael Black\nW9MDB\nmdblack98@yahoo.com\n"+appData);
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

        private void buttonMap_Click(object sender, EventArgs e)
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

        private void buttonGrid_Click(object sender, EventArgs e)
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

        private void textBoxEnd_Leave(object sender, EventArgs e)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Thread oThread = new Thread(new ThreadStart(checkForUpdate));
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
    
    class myWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest w = base.GetWebRequest(address);
            w.Timeout = 60 * 60 * 10 * 1000; // Ten hour timeout
            return w;
        }
    }
    
}