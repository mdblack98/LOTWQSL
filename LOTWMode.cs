using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOTWQSL
{
    /*  Modes from https://lotw.arrl.org/lotw-help/frequently-asked-questions/?lang=en#modes
     *  As of 2016-10-30
Mode	Mode Group
CW	CW
PHONE	PHONE
AM	PHONE
FM	PHONE
SSB	PHONE
ATV	IMAGE
FAX	IMAGE
IMAGE	IMAGE
SSTV	IMAGE
DATA	DATA
AMTOR	DATA
CHIP	DATA
CLOVER	DATA
CONTESTI	DATA
DOMINO	DATA
FSK31	DATA
FSK441	DATA
GTOR	DATA
HELL	DATA
HFSK	DATA
ISCAT	DATA
JT4	DATA
JT65	DATA
JT6M	DATA
JT9	DATA
MFSK16	DATA
MFSK8	DATA
MINIRTTY	DATA
MT63	DATA
OLIVIA	DATA
OPERA	DATA
PACKET	DATA
PACTOR	DATA
PAX	DATA
PSK10	DATA
PSK125	DATA
PSK2K	DATA
PSK31	DATA
PSK63	DATA
PSK63F	DATA
PSKAM	DATA
PSKFEC31	DATA
Q15	DATA
ROS	DATA
RTTY	DATA
RTTYM	DATA
THOR	DATA
THROB	DATA
VOI	DATA
WINMOR	DATA
WSPR	DATA
     */
    class LOTWMode
    {
        public enum MODES { CW, PHONE, DIGITAL };

        int TRIPLEPLAY = 0;

        List<String> cw      = new List<string>(); 
        List<String> digital = new List<string>();
        List<String> phone   = new List<string>();
        Dictionary<String, int> tripleplay = new Dictionary<String, int>();
        int modeCW = 0;
        int modeDIGITAL = 0;
        int modePHONE = 0;
        public LOTWMode()
        {
            StreamReader reader = new StreamReader("lotwmodes.txt");
            String line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#")) continue; // skip comment
                String[] tokens = line.Split(new[] { ' ', '\t' });
                if (line.Contains("Mode")) continue; // skip Mode line if there
                if (tokens.Count() != 2) continue; // skip lines without exact two tokens
                if (tokens[1].Equals("PHONE")) phone.Add(tokens[0]);
                if (tokens[1].Equals("DATA")) digital.Add(tokens[0]);
                if (tokens[1].Equals("CW")) cw.Add(tokens[0]);
            }
            int i1 = (int)MODES.CW;
            int i2 = (int)MODES.PHONE;
            int i3 = (int)MODES.DIGITAL;
            int ii1 = (int)Math.Round(Math.Pow(2, i1));
            int ii2 = (int)Math.Round(Math.Pow(2, i2));
            int ii3 = (int)Math.Round(Math.Pow(2, i3));
            TRIPLEPLAY = ii1 | ii2 | ii3; // bitmask
            modeCW = ii1;
            modePHONE = ii2;
            modeDIGITAL = ii3;
        }

        ~LOTWMode()
        {
        }

        void Clear()
        {
            tripleplay.Clear();
        }

        public bool isModeCW(String mode)
        {
            return cw.Contains(mode.ToUpper());
        }

        public bool isModeDigital(String mode)
        {
            return digital.Contains(mode.ToUpper());
        }

        public bool isModePhone(String mode)
        {
            return phone.Contains(mode.ToUpper());
        }

        public bool isTriplePlay(String callsign)
        {
            return tripleplay[callsign] == TRIPLEPLAY;
        }

        public int whichMode(String mode)
        {
            if (isModeCW(mode))      return modeCW;
            if (isModePhone(mode))   return modePHONE;
            if (isModeDigital(mode)) return modeDIGITAL;
            return 0;
        }

        public void addCallsign(String state, String mode)
        {
            state = state.ToUpper();
            if (tripleplay.ContainsKey(state))
            {
                int n = 0;
                tripleplay[state] |= whichMode(mode);
                if (isTriplePlay(state))
                {
                    ++n;
                }
            }
            else
            {
                tripleplay.Add(state, whichMode(mode));
            }
        }
    }
}
