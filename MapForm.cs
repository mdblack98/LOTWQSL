using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpMap;
using System.Collections.ObjectModel;
using GeoAPI.CoordinateSystems;
using ProjNet.CoordinateSystems;
using SharpMap.Data;

namespace LOTWQSL
{
    public partial class MapForm : Form
    {
        HashSet<string> bandStates; // this will be used by GetStyleForShape to determine which states to paint
        HashSet<string> statesLabeled;
        bool flagFirst = true;
        public string bandSelected = String.Empty;
        SharpMap.Layers.VectorLayer vlay; // our state outlines
        SharpMap.Layers.LabelLayer llay; // Our state labels
        List<string> states;
        private List<string> modes; //All the modes in our ADIF file
        bool modeCanChange = false;
        string modeSelected = MODEALL;
        double myLat = Properties.Settings.Default.Latitude;
        double myLon = Properties.Settings.Default.Longitude;
        bool azimuthOn = Properties.Settings.Default.AzimuthOn;
        public bool showLabels = Properties.Settings.Default.StatesLabeled;
        public Font fontStateLabels = Properties.Settings.Default.FontStateLabels;
        public Color fontStateColor = Properties.Settings.Default.FontStateColor;
        public Font fontAzimuthLabel = Properties.Settings.Default.AzimuthLabelFont;
        public Color fontAzimuthColor = Properties.Settings.Default.AzimuthLabelColor;
        Dictionary<string, bool> modeIsWAS;
        const string MODEALL = "ALL";
        const string BANDALL = "All";
        const string BANDTRIPLEPLAY = "TriplePlay";

        public MapForm()
        {
            InitializeComponent();
            modeIsWAS = new Dictionary<string, bool>();
            //textBoxLat.Text = Properties.Settings.Default.Latitude.ToString(); 
            //textBoxLon.Text = Properties.Settings.Default.Longitude.ToString();
            modeSelected = Properties.Settings.Default.Mode;
            bandStates = new HashSet<string>();
            statesLabeled = new HashSet<string>();
            modes = new List<string>();
            statesLoad();
            try
            {

                vlay = new SharpMap.Layers.VectorLayer("States");
                llay = new SharpMap.Layers.LabelLayer("StateLabels");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show(ex.Source+":"+ex.Message);
            }
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            //mapBox1.Refresh();
            this.Top = Properties.Settings.Default.MapRestoreBounds.Top;
            this.Left = Properties.Settings.Default.MapRestoreBounds.Left;
            this.Height = Properties.Settings.Default.MapRestoreBounds.Height;
            this.Width = Properties.Settings.Default.MapRestoreBounds.Width;
            Rectangle rect = SystemInformation.VirtualScreen;
            if (this.Location.X < 0 || this.Location.Y < 0)
            {
                this.Top = 0;
                this.Left = 0;
                
                //this.Width = 300;
                //this.Height = 400;
            }
            if (this.Location.X > rect.Width || this.Location.Y > rect.Bottom)
            {
                this.Top = 0;
                this.Left = 0;
            }
            bandSelected = Properties.Settings.Default.mapBand;
            comboBoxBand.SelectedIndex = comboBoxBand.FindStringExact(bandSelected);
            labelAzimuth.Font = fontAzimuthLabel;
            labelAzimuth.ForeColor = fontAzimuthColor;
            if (!azimuthOn)
            {
                labelAzimuth.Visible = false;
            }
            System.Windows.Forms.ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
            toolTip1.SetToolTip(this.buttonConfig, "Configuration");
            try
            {
                vlay.DataSource = new SharpMap.Data.Providers.ShapeFile("tl_2013_us_state.shp", true);
                vlay.Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetStyleForShape);

                llay = new SharpMap.Layers.LabelLayer("STUSPS");
                llay.DataSource = new SharpMap.Data.Providers.ShapeFile("statelabels.shp", true);
                llay.LabelStringDelegate = LabelDelegate;
                llay.Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetStyleForStateLabel);
                llay.Enabled = true;
                llay.LabelColumn = "Label";
                llay.Style = new SharpMap.Styles.LabelStyle();
                llay.Style.CollisionDetection = true;
                llay.Style.CollisionBuffer = new SizeF(20, 20);
                llay.Style.ForeColor = Color.Black;
                llay.Style.Font = new Font(FontFamily.GenericSerif, 8);
                llay.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
            }
            catch (Exception ex)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("StateLabels.mdb problem: " + ex.Message);
            }
            parseModes();
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background 
            e.DrawBackground();
            // Get the item text    
            string mode = ((ComboBox)sender).Items[e.Index].ToString();
            // Determine the forecolor based on whether or not the item is selected    
            Brush brush = Brushes.Black;
            Font myfont = new Font(((Control)sender).Font, FontStyle.Regular);
            if (modeIsWAS[mode])
            {
                brush = Brushes.Green;
                myfont.Dispose();
                myfont = new Font(((Control)sender).Font, FontStyle.Bold);
            }
            // Draw the text    
            e.Graphics.DrawString(mode, myfont, brush, e.Bounds.X, e.Bounds.Y);
        }
        private void parseModes()
        {
            modeCanChange = false;
            modes.Clear();
            modes.Add(MODEALL);
            modes.Add("DIGITAL");
            modes.Add("TRIPLEPLAY");
            bool modeMatch = false;
            foreach (string s in MainWindow2.allWAS) // find all modes we have done
            {
                string[] tokens = s.Split(new[] { ' ' });
                bool need = !modes.Contains(tokens[1]); // do we already have this mode?
                if (!need) continue;
                // Whether or not wee add a band to our combobox depends on both selections
                // if bandSelect is All then we need all modes we see
                need = bandSelected.Equals("All");
                // If we match our band and we want all modes
                need |= bandSelected.Equals(tokens[0]) && modeSelected.Equals("ALL");
                // If we want all of everything
                need |= bandSelected.Equals("All") && modeSelected.Equals("ALL");
                //need |= (modeSelected.Equals("Any") || modeSelected.Equals("ALL")) || (bandSelected.Equals("All")  && modeSelected.Equals(tokens[1]));
                //need |= bandSelected.Equals(tokens[0]) || bandSelected.Equals("All");
                need |= bandSelected.Equals("Digital") || bandSelected.Equals("TriplePlay");
                need |= bandSelected.Equals(tokens[0]) && !modes.Contains(tokens[1]);
                //if (!modes.Contains(tokens[1]) && (modeSelected.Equals("Any") || modeSelected.Equals("ALL") || bandSelected.Equals(tokens[0]) || (bandSelected.Equals("All") && modeSelected.Equals(tokens[1])) || bandSelected.Equals("Digital") || bandSelected.Equals("TriplePlay")))
                if (need)
                {
                    //List<String> remainStates = statesRemain(bandStates);
                    modes.Add(tokens[1]);
                    if (modeSelected.Equals("Any") || modeSelected.Equals(tokens[1]) || modeSelected.Equals(MODEALL) || modeSelected.Equals("DIGITAL") || modeSelected.Equals("TRIPLEPLAY"))
                        modeMatch = true;
                }
            }
            if (!modeMatch)
            {
                //if (modes.Count == 0)
                //{
                //    modes.Add(MODEALL);
                //}
                modes.Sort();
                modeSelected = modes[0]; // if no such mode default to 1st
                comboBoxMode.Items.Clear();
                foreach (string s in modes)
                {
                    comboBoxMode.Items.Add(s);
                }
                comboBoxMode.SelectedIndex = 0;
                modeCanChange = false;
                return;
            }
            if (modes.Count > 0)
            {
                modes.Sort();
                comboBoxMode.Items.Clear();
                comboBoxMode.SelectedIndex = -1;
                modeIsWAS.Clear();
                foreach (string s in modes)
                {
                    bandStates.Clear();
                    parseBandMode(bandSelected, s);
                    HashSet<String> remainStates = statesRemain(bandStates);
                    string modeStatus = s;
                    if (remainStates.Count == 0)
                    {
                        //modeStatus += " WAS";
                        modeIsWAS[modeStatus] = true;
                    }
                    else {
                        //modeStatus += " " + (50 - remainStates.Count);
                        modeIsWAS[modeStatus] = false;
                    }
                    comboBoxMode.Items.Add(modeStatus);
                    if (s.Equals(modeSelected))
                    {
                        comboBoxMode.Text = s;
                        comboBoxMode.SelectedIndex = comboBoxMode.Items.Count-1;
                    }
                }
            }
            comboBoxMode.SelectedIndex = comboBoxMode.FindStringExact(modeSelected);
            comboBoxBand.SelectedIndex = comboBoxBand.FindStringExact(bandSelected);
            if (comboBoxMode.SelectedIndex < 0)
            {
                comboBoxMode.SelectedIndex = 0;
                modeSelected = comboBoxMode.Text.Split(new string[] { " " }, StringSplitOptions.None)[0];
            }
            modeCanChange = true;
        }

        // Will add a band to comboBoxBand if it's not already there
        private void addBand(string bandChk)
        {
            if (comboBoxBand.Items.Contains(bandChk)) return;
            int i = 0;
            int insertAt = -1;
            int metersChk = int.Parse(bandChk.Split('M')[0]);
            foreach (String band in comboBoxBand.Items)
            {
                if (band.Equals("All"))
                {
                    insertAt = i;
                    break;
                }
                int meters = int.Parse(band.Split('M')[0]);
                if (metersChk < meters)
                {
                    insertAt = i;
                    break;
                }
                ++i;
            }
            if (insertAt >= 0)
            {
                comboBoxBand.Items.Insert(insertAt, bandChk);
            }
            comboBoxBand.SelectedIndex = comboBoxBand.FindStringExact(bandSelected);
        }

        private void parseBand(string band)
        {
            bandStates.Clear();
            LOTWMode LOTWmode = new LOTWMode();
            //band = band.ToUpper();
            foreach(string s in MainWindow2.allWAS) {
                string[] tokens = s.Split(new string[] { " "},StringSplitOptions.None);
                string thisBand = tokens[0];
                string mode = tokens[1];
                string state = tokens[2];
                LOTWmode.addCallsign(state, mode);
                bool isTriplePlay = LOTWmode.isTriplePlay(state);
                if (thisBand.Equals(band) || band.Equals(BANDALL) || (band.Equals(BANDTRIPLEPLAY)&&isTriplePlay))
                {
                    string mystate = s.Substring(s.Length-2);
                    if (!bandStates.Contains(mystate))
                    {
                        bandStates.Add(mystate);
                    }
                }
                addBand(thisBand);
            }
        }

        private void parseBandMode(string band, string mode)
        {
            LOTWMode LOTWmode = new LOTWMode();
            if ((!mode.Equals("ALL") && !band.Equals("All")) && (!band.Equals("Digital") && !band.Equals("TriplePlay")))
            {
                bandStates.Clear();
            }
            //band = band.ToUpper();
            mode = " " + mode + " ";
            foreach (string s in MainWindow2.allWAS)
            {
                string[] tokens = s.Split(new string[] { " " }, StringSplitOptions.None);
                string myband = tokens[0];
                string mymode = tokens[1];
                string mystate = tokens[2];
                LOTWmode.addCallsign(mystate, mymode);
                Boolean modeOK = s.Contains(mode) || mode.Equals(" " + MODEALL + " ");
                Boolean bandOK = myband.Equals(band) || band.Equals(BANDALL);
                Boolean isDigitalMode = LOTWmode.isModeDigital(mymode) && (band.Contains("Digital") || mode.Contains("DIGITAL"));
                Boolean isTriplePlay = LOTWmode.isTriplePlay(mystate) && band.Contains("TriplePlay");
                if ((bandOK && modeOK) || (isDigitalMode && bandOK) || isTriplePlay)
                {
                    string state = s.Substring(s.Length - 2);
                    if (!bandStates.Contains(state))
                    {
                        bandStates.Add(state);
                    }
                }
            }
            //MessageBox.Show("Bandstates=" + bandStates.Count);
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            //parseBand(band);
            mapBox1.Refresh();
        }

        private String LabelDelegate(FeatureDataRow row)
        {
            String state = "";
            state = (String)row.ItemArray[3];
            return state;
        }

        private SharpMap.Styles.LabelStyle GetStyleForStateLabel(SharpMap.Data.FeatureDataRow row)
        {
            SharpMap.Styles.LabelStyle style = new SharpMap.Styles.LabelStyle();
            style.ForeColor = fontStateColor;
            if (row.ItemArray[3].Equals("HI"))
            {
                style.ForeColor = Color.Black;
            }
            style.Font = fontStateLabels;
            style.CollisionDetection = true;
            style.CollisionBuffer = new SizeF(50, 50);
            style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
            style.Enabled = false;
            return style;
        }

        private SharpMap.Styles.VectorStyle GetStyleForShape(SharpMap.Data.FeatureDataRow row)
        {
            SharpMap.Styles.VectorStyle style = new SharpMap.Styles.VectorStyle();
            if (bandStates.Contains(row.ItemArray[6])) 
            { 
                style.Fill = Brushes.Green;
                style.Outline = new Pen(Color.Black, 1);
            }
            else
            {
                style.Fill = Brushes.DarkRed;
                style.Outline = new Pen(Color.Black, 1);
            }
            if (!states.Contains(row.ItemArray[6]))
            //if (row.ItemArray[6].Equals("PR"))
            {
                style.Fill = Brushes.White; // ignore non-50 states for WAS award coloring
                style.Outline = new Pen(Color.White, 1);
            }
            //style.Line = new Pen(Color.Black, 1);
            style.EnableOutline = true;
            return style;
        }

        private void mapBox1_Paint(object sender, PaintEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left) return;
            //parseModes();
            statesLabeled.Clear();
            if (modeSelected.Contains(MODEALL))
            {  // then for WAS mode
                //parseModes();
                bandStates.Clear();
                parseBand(comboBoxBand.Text);
            }
            else // by selected mode
            {
                //string[] myMode = modeSelected.Split(new string[] {" "},StringSplitOptions.None);
                parseBand(comboBoxBand.Text); // to fill up the modes we've done
                bandStates.Clear();
                parseBandMode(comboBoxBand.Text, modeSelected);
            }
            HashSet<String> remainStates = statesRemain(bandStates);
            mapBox1.Map.Layers.Clear();
            mapBox1.Map.Layers.Add(vlay);
            mapBox1.Map.Layers.Add(llay);
            vlay.Enabled = true;
            llay.Enabled = showLabels;
            // Make our needed states list
            string txt = bandSelected;
            if (remainStates.Count() == 0)
            {
                txt += " WAS";
            }
            else
            {
                txt += " (" + remainStates.Count() + ") ";
            }
            bool twolines = false;
            foreach (string s in remainStates)
            {
                txt += " " + s;
                if (txt.Length > 75 && !twolines)
                {
                    txt += "\n     ";
                    twolines = true;
                }
            }
            label1.Font = fontAzimuthLabel;
            label1.ForeColor = fontAzimuthColor;
            label1.Text = txt;

            if (flagFirst)
            {
                flagFirst = false;
                GeoAPI.Geometries.Coordinate p1 = new GeoAPI.Geometries.Coordinate(-170, 24);
                GeoAPI.Geometries.Coordinate p2 = new GeoAPI.Geometries.Coordinate(-62, 71);
                GeoAPI.Geometries.Envelope bbox = new GeoAPI.Geometries.Envelope(p1, p2);
                mapBox1.Map.ZoomToBox(bbox);
                mapBox1.Enabled = true;
            }
            mapBox1.ActiveTool = SharpMap.Forms.MapBox.Tools.Pan;
            button1.Enabled = true;
            // Add state labels

        }
        /// Not used right now but keeping for future reference -- couldn't figure out how to get this to change the projection
        /// <summary>
        /// Creates a UTM projection for the northern/// hemisphere based on the WGS84 datum
        /// </summary>
        /// <param name="utmZone">Utm Zone</param>
        /// <returns>Projection</returns>
        private IProjectedCoordinateSystem CreateUtmProjection(int utmZone)
        {
            CoordinateSystemFactory cFac = new CoordinateSystemFactory();
            //Create geographic coordinate system based on the WGS84 datum
            IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("WGS 84", 6378137, 298.257223563, LinearUnit.Metre);
            IHorizontalDatum datum = cFac.CreateHorizontalDatum("WGS_1984", DatumType.HD_Geocentric, ellipsoid, null);
            IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("WGS 84", AngularUnit.Degrees, datum,
              PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
              new AxisInfo("Lat", AxisOrientationEnum.North));

            //Create UTM projection
            List<ProjectionParameter> parameters = new List<ProjectionParameter>();
            parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
            parameters.Add(new ProjectionParameter("central_meridian", -183 + 6 * utmZone));
            parameters.Add(new ProjectionParameter("scale_factor", 0.9996));
            parameters.Add(new ProjectionParameter("false_easting", 500000));
            parameters.Add(new ProjectionParameter("false_northing", 0.0));
            IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse Mercator", parameters);

            return cFac.CreateProjectedCoordinateSystem("WGS 84 / UTM zone " + utmZone.ToString() + "N", gcs,
               projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East),
               new AxisInfo("North", AxisOrientationEnum.North));
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = false;
            bandSelected = comboBoxBand.Text;
            if (bandSelected.Equals("TriplePlay") || bandSelected.Equals("Digital")) comboBoxMode.Enabled = false;
            else comboBoxMode.Enabled = true;
            parseModes();
            Properties.Settings.Default.mapBand = bandSelected;
            Properties.Settings.Default.Save();
            mapBox1.PreviewMode = SharpMap.Forms.MapBox.PreviewModes.Fast;
            mapBox1.Invalidate();
            mapBox1.Refresh();
        }

        private void buttonRedraw_Click(object sender, EventArgs e)
        {
            parseModes();
            bandStates.Clear();
            mapBox1.Invalidate();
            mapBox1.Refresh();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }
            Hide();
            e.Cancel = true;
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
            //states.Add("DC"); // DC does not count for LOTW
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
            foreach(string s in states)
            {
                if (!myStates.Contains(s))
                {
                    remain.Add(s);
                }
            }
            return remain;
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modeCanChange)
            {
                modeSelected = comboBoxMode.Text.Split(new string[] {" "},StringSplitOptions.None)[0];
                bandStates.Clear();
                parseBandMode(bandSelected, modeSelected);
                Properties.Settings.Default.Mode = modeSelected;
                Properties.Settings.Default.Save();
                button1.Enabled = false;
                mapBox1.PreviewMode = SharpMap.Forms.MapBox.PreviewModes.Fast;
                mapBox1.Invalidate();
                mapBox1.Refresh();
            }
        }

        private double azimuth(double lat,double lon)
        {
            double phi1 = myLat * (Math.PI/180);
            double phi2 = lat * (Math.PI/180);
            double lam1 = myLon * (Math.PI/180);
            double lam2 = lon * (Math.PI/180);
            double azimuth = Math.Atan2(Math.Sin(lam2-lam1)*Math.Cos(phi2),Math.Cos(phi1)*Math.Sin(phi2)-Math.Sin(phi1)*Math.Cos(phi2)*Math.Cos(lam2-lam1));
            azimuth = (azimuth % (2 * Math.PI)) * 180 / Math.PI;
            if (azimuth < 0)
            {
                azimuth = 360 + azimuth;
            }
            return azimuth;
        }

        private void mapBox1_MouseMove(GeoAPI.Geometries.Coordinate worldPos, MouseEventArgs imagePos)
        {
            //string x = worldPos.X.ToString(".00000");
            //string y = worldPos.Y.ToString(".00000");
            //labelAzimth.Text = x + "/" + y;
            if (myLat != 0.0 || myLon != 0.0) {
                labelAzimuth.Text = "Azimuth "+azimuth(worldPos.Y, worldPos.X).ToString(".0");
            }
        }

        private void buttonConfig_Click(object sender, EventArgs e)
        {
            ConfigForm configForm = new ConfigForm();
            configForm.ShowDialog();

            myLat = Properties.Settings.Default.Latitude;
            myLon = Properties.Settings.Default.Longitude;
            showLabels = Properties.Settings.Default.StatesLabeled;
            azimuthOn = Properties.Settings.Default.AzimuthOn;
            fontStateLabels = Properties.Settings.Default.FontStateLabels;
            fontStateColor = Properties.Settings.Default.FontStateColor;
            fontAzimuthLabel = Properties.Settings.Default.AzimuthLabelFont;
            fontAzimuthColor = Properties.Settings.Default.AzimuthLabelColor;

            if (azimuthOn)
            {
                Properties.Settings.Default.AzimuthOn = true;
                Properties.Settings.Default.Save();
                labelAzimuth.Visible = true;
                labelAzimuth.Font = fontAzimuthLabel;
                labelAzimuth.ForeColor = fontAzimuthColor;
            }
            else
            {
                Properties.Settings.Default.AzimuthOn = false;
                Properties.Settings.Default.Save();
                labelAzimuth.Visible = false;
            }
            mapBox1.Invalidate();
            mapBox1.Refresh();
        }

        private void mapBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
