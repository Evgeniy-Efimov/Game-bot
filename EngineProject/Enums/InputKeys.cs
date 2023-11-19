using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineProject.Enums
{
    public class InputKeys
    {
        //Keys to use in code
        public static string Esc = "Escape";
        public static string Ctrl = "ControlKey";
        public static string Space = "Space";
        public static string W = "W";
        public static string A = "A";
        public static string S = "S";
        public static string D = "D";
        public static string E = "E";
        public static string Q = "Q";
        public static string R = "R";
        public static string T = "T";
        public static string Insert = "Insert";
        public static string Home = "Home";
        public static string Num1 = "1";
        public static string Num2 = "2";
        public static string Num3 = "3";
        public static string Num4 = "4";
        public static string Num5 = "5";
        public static string Num6 = "6";
        public static string Num7 = "7";
        public static string Num8 = "8";
        public static string Num9 = "9";
        public static string Num10 = "10";
        public static string MouseRotate = "MouseRotate"; //not a real key

        //Keys for win api
        public static Dictionary<string, int> VCodes = new Dictionary<string, int>()
        {
            { W, 0x57 },
            { A, 0x41 },
            { S, 0x53 },
            { D, 0x44 },
            { E, 0x45 },
            { Q, 0x51 },
            { R, 0x52 },
            { T, 0x54 },
            { Insert, 0x2D },
            { Space, 0x20 },
            { Home, 0x24 },
            { Num1, 0x31 },
            { Num2, 0x32 },
            { Num3, 0x33 },
            { Num4, 0x34 },
            { Num5, 0x35 },
            { Num6, 0x36 },
            { Num7, 0x37 },
            { Num8, 0x38 },
            { Num9, 0x39 }
        };
    }
}
