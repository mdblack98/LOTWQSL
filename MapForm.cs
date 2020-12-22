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
        readonly HashSet<string> bandStates; // this will be used by GetStyleForShape to determine which states to paint
        readonly HashSet<string> statesLabeled;
        bool flagFirst = true;
        public string bandSelected = String.Empty;
        readonly SharpMap.Layers.VectorLayer vlay; // our state outlines
        SharpMap.Layers.LabelLayer llay; // Our state labels
        List<string> states;
        private readonly List<string> modes; //All the modes in our ADIF file
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
        readonly Dictionary<string, bool> modeIsWAS;
        const string MODEALL = "ALL";
        const string BANDALL = "All";
        const string BANDTRIPLEPLAY = "TriplePlay";
        const string BAND5BANDWAS = "5-Band WAS";

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
            StatesLoad();
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
            if (this.Width < 200) this.Width = 200;
            if (this.Height < 200) this.Height = 200;
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

                llay = new SharpMap.Layers.LabelLayer("STUSPS")
                {
                    DataSource = new SharpMap.Data.Providers.ShapeFile("statelabels.shp", true),
                    LabelStringDelegate = LabelDelegate,
                    Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetStyleForStateLabel),
                    Enabled = true,
                    LabelColumn = "Label",
                    Style = new SharpMap.Styles.LabelStyle()
                };
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
            ParseModes();
        }

        private void ComboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background 
            e.DrawBackground();
            // Get the item text    
            string mode = ((ComboBox)sender).Items[e.Index].ToString();
            // Determine the forecolor based on whether or not the item is selected    
            Brush brush = Brushes.Black;
            Font myfont = new Font(((Control)sender).Font, FontStyle.Regular);
            if (modeIsWAS.ContainsKey(mode) && modeIsWAS[mode]==true)
            {
                brush = Brushes.Green;
                myfont.Dispose();
                myfont = new Font(((Control)sender).Font, FontStyle.Bold);
            }
            // Draw the text    
            e.Graphics.DrawString(mode, myfont, brush, e.Bounds.X, e.Bounds.Y);
        }
        private void ParseModes()
        {
            modeCanChange = false;
            modes.Clear();
            modes.Add(MODEALL);
            modes.Add("DIGITAL");
            modes.Add("TRIPLEPLAY");
            //modes.Add(BAND5BANDWAS);
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
                need |= bandSelected.Equals("Digital") || bandSelected.Equals("TriplePlay");// || bandSelected.Equals(BAND5BANDWAS);
                need |= bandSelected.Equals(tokens[0]) && !modes.Contains(tokens[1]);
                //if (!modes.Contains(tokens[1]) && (modeSelected.Equals("Any") || modeSelected.Equals("ALL") || bandSelected.Equals(tokens[0]) || (bandSelected.Equals("All") && modeSelected.Equals(tokens[1])) || bandSelected.Equals("Digital") || bandSelected.Equals("TriplePlay")))
                if (need)
                {
                    //List<String> remainStates = statesRemain(bandStates);
                    modes.Add(tokens[1]);
                    if (modeSelected.Equals("Any") || modeSelected.Equals(tokens[1]) || modeSelected.Equals(MODEALL) || modeSelected.Equals("DIGITAL") || modeSelected.Equals("TRIPLEPLAY") /*|| modeSelected.Equals(BAND5BANDWAS)*/)
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
                    if (!s.Equals(BAND5BANDWAS))
                    {
                        comboBoxMode.Items.Add(s);
                    }
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
                    ParseBandMode(bandSelected, s);
                    HashSet<String> remainStates = StatesRemain(bandStates);
                    string modeStatus = s;
                    if (remainStates.Count == 0)
                    {
                        modeIsWAS[modeStatus] = true;
                    }
                    else {
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

        private void GetBandUnit(string bandUnit, out int band, out String unit)
        {
            String[] tokens;
            char[] split = { 'M', 'C' };
            unit = "M";
            if (bandUnit.Contains("CM"))
            {
                unit = "CM";
            }
            tokens = bandUnit.Split(split);
            band = int.Parse(tokens[0]);
        }

        // Will add a band to comboBoxBand if it's not already there
        private void AddBand(string bandChk)
        {
            if (comboBoxBand.Items.Contains(bandChk)) return;
            int i = 0;
            int insertAt = -1;

            GetBandUnit(bandChk, out int ibandChk, out string unitChk);
            if (unitChk.Equals("M"))
            {
                ibandChk *= 100;
            }
            foreach (String band in comboBoxBand.Items)
            {
                if (band.Equals("All"))
                {
                    insertAt = i;
                    break;
                }
                GetBandUnit(band, out int bandChkComboBox, out string unitChkComboBox);
                if (unitChkComboBox.Equals("M"))
                {
                    bandChkComboBox *= 100;
                }
                if (ibandChk < bandChkComboBox)
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

        private void ParseBand(string band)
        {
            bandStates.Clear();
            LOTWMode LOTWmode = new LOTWMode();
            //band = band.ToUpper();
            foreach(string s in MainWindow2.allWAS) {
                string[] tokens = s.Split(new string[] { " "},StringSplitOptions.None);
                string thisBand = tokens[0];
                string mode = tokens[1];
                string state = tokens[2];
                LOTWmode.AddCallsign(state, mode);
                bool isTriplePlay = LOTWmode.IsTriplePlay(state);
                bool is5BandWAS = LOTWmode.Is5BandWAS(thisBand);
                if (thisBand.Equals(band) || band.Equals(BANDALL) || (band.Equals(BANDTRIPLEPLAY)&&isTriplePlay) /* || (band.Equals(BAND5BANDWAS)&&is5BandWAS) */)
                {
                    string mystate = s.Substring(s.Length-2);
                    if (!bandStates.Contains(mystate))
                    {
                        bandStates.Add(mystate);
                    }
                }
                AddBand(thisBand);
            }
        }

        private void ParseBandMode(string band, string mode)
        {
            LOTWMode LOTWmode = new LOTWMode();
            if ((!mode.Equals("ALL") && !band.Equals("All")) && (!band.Equals("Digital") && !band.Equals("TriplePlay") /*&& !band.Equals(BAND5BANDWAS)*/))
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
                LOTWmode.AddCallsign(mystate, mymode);
                Boolean modeOK = s.Contains(mode) || mode.Equals(" " + MODEALL + " ");
                Boolean bandOK = myband.Equals(band) || band.Equals(BANDALL);
                Boolean isDigitalMode = LOTWmode.IsModeDigital(mymode) && band.Contains("Digital");
                Boolean isTriplePlay = LOTWmode.IsTriplePlay(mystate) && band.Contains("TriplePlay");
                Boolean is5BandWAS = LOTWmode.Is5BandWAS(myband) && (band.Contains(BAND5BANDWAS) /*|| mode.Contains(BAND5BANDWAS)*/);
                if ((bandOK && modeOK) || isDigitalMode || isTriplePlay || is5BandWAS)
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
            String state;
            state = (String)row.ItemArray[3];
            return state;
        }

        private SharpMap.Styles.LabelStyle GetStyleForStateLabel(SharpMap.Data.FeatureDataRow row)
        {
            SharpMap.Styles.LabelStyle style = new SharpMap.Styles.LabelStyle()
            {
                ForeColor = fontStateColor
            };
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

        private void MapBox1_Paint(object sender, PaintEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left) return;
            //parseModes();
            statesLabeled.Clear();
            if (modeSelected.Contains(MODEALL))
            {  // then for WAS mode
                //parseModes();
                bandStates.Clear();
                ParseBand(comboBoxBand.Text);
            }
            else // by selected mode
            {
                //string[] myMode = modeSelected.Split(new string[] {" "},StringSplitOptions.None);
                ParseBand(comboBoxBand.Text); // to fill up the modes we've done
                bandStates.Clear();
                ParseBandMode(comboBoxBand.Text, modeSelected);
            }
            HashSet<String> remainStates = StatesRemain(bandStates);
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
            List<ProjectionParameter> parameters = new List<ProjectionParameter>
            {
                new ProjectionParameter("latitude_of_origin", 0),
                new ProjectionParameter("central_meridian", -183 + 6 * utmZone),
                new ProjectionParameter("scale_factor", 0.9996),
                new ProjectionParameter("false_easting", 500000),
                new ProjectionParameter("false_northing", 0.0)
            };
            IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse Mercator", parameters);

            return cFac.CreateProjectedCoordinateSystem("WGS 84 / UTM zone " + utmZone.ToString() + "N", gcs,
               projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East),
               new AxisInfo("North", AxisOrientationEnum.North));
        }


        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = false;
            bandSelected = comboBoxBand.Text;
            if (bandSelected.Equals("TriplePlay") || bandSelected.Equals("Digital") /*|| bandSelected.Equals(BAND5BANDWAS)*/) comboBoxMode.Enabled = false;
            else comboBoxMode.Enabled = true;
            ParseModes();
            Properties.Settings.Default.mapBand = bandSelected;
            Properties.Settings.Default.Save();
            mapBox1.PreviewMode = SharpMap.Forms.MapBox.PreviewModes.Fast;
            mapBox1.Invalidate();
            mapBox1.Refresh();
        }

        private void ButtonRedraw_Click(object sender, EventArgs e)
        {
            ParseModes();
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
        private void StatesLoad()
        {
            states = new List<string>
            {
                "AL",
                "AK",
                "AZ",
                "AR",
                "CA",
                "CO",
                "CT",
                //states.Add("DC"); // DC does not count for LOTW
                "DE",
                "FL",
                "GA",
                "HI",
                "ID",
                "IL",
                "IN",
                "IA",
                "KS",
                "KY",
                "LA",
                "ME",
                "MD",
                "MA",
                "MI",
                "MN",
                "MS",
                "MO",
                "MT",
                "NE",
                "NV",
                "NH",
                "NJ",
                "NM",
                "NY",
                "NC",
                "ND",
                "OH",
                "OK",
                "OR",
                "PA",
                "RI",
                "SC",
                "SD",
                "TN",
                "TX",
                "UT",
                "VT",
                "VA",
                "WA",
                "WV",
                "WI",
                "WY"
            };
            states.Sort();
        }
        public HashSet<string> StatesRemain(HashSet<string> myStates)
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

        private void ComboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modeCanChange)
            {
                modeSelected = comboBoxMode.Text.Split(new string[] {" "},StringSplitOptions.None)[0];
                bandStates.Clear();
                ParseBandMode(bandSelected, modeSelected);
                Properties.Settings.Default.Mode = modeSelected;
                Properties.Settings.Default.Save();
                button1.Enabled = false;
                mapBox1.PreviewMode = SharpMap.Forms.MapBox.PreviewModes.Fast;
                mapBox1.Invalidate();
                mapBox1.Refresh();
            }
        }

        private double Azimuth(double lat,double lon)
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

        private void MapBox1_MouseMove(GeoAPI.Geometries.Coordinate worldPos, MouseEventArgs imagePos)
        {
            //string x = worldPos.X.ToString(".00000");
            //string y = worldPos.Y.ToString(".00000");
            //labelAzimth.Text = x + "/" + y;
            if (myLat != 0.0 || myLon != 0.0) {
                labelAzimuth.Text = "Azimuth "+Azimuth(worldPos.Y, worldPos.X).ToString(".0");
            }
        }

        private void ButtonConfig_Click(object sender, EventArgs e)
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

        private void MapBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
