using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UIColor
    {
        public UIColor()
            : this(0, 0, 0) { }

        public UIColor(uint red, uint green, uint blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public UIColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length != 6)
                throw new Exception("Hex color code is not correct length");
            Red = uint.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            Green = uint.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            Blue = uint.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }

        public string ToHex()
        {
            return "#" + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        public uint Red;
        public uint Green;
        public uint Blue;
    }

    public static class UIColors
    {
        public static UIColor Black { get { return new UIColor("#000000"); } }
        public static UIColor Gray { get { return new UIColor("#808080"); } }
        public static UIColor Red { get { return new UIColor("#FF0000"); } }
        public static UIColor DarkRed { get { return new UIColor("#8B0000"); } }
        public static UIColor DarkBlue { get { return new UIColor("#00008B"); } }
        public static UIColor DarkCyan { get { return new UIColor("#008B8B"); } }
        public static UIColor Green { get { return new UIColor("#008000"); } }
        public static UIColor DarkGreen { get { return new UIColor("#006400"); } }
        public static UIColor ForestGreen { get { return new UIColor("#228B22"); } }
        public static UIColor GreenYellow { get { return new UIColor("#ADFF2F"); } }
        public static UIColor HotPink { get { return new UIColor("#FF69B4"); } }
        public static UIColor IndianRed { get { return new UIColor("#CD5C5C"); } }
        public static UIColor Indigo { get { return new UIColor("#4B0082"); } }
        public static UIColor Lime { get { return new UIColor("#00FF00"); } }
        public static UIColor LimeGreen { get { return new UIColor("#32CD32"); } }
        public static UIColor Maroon { get { return new UIColor("#800000"); } }
        public static UIColor MediumBlue { get { return new UIColor("#0000CD"); } }
        public static UIColor Orange { get { return new UIColor("#FFA500"); } }
        public static UIColor Purple { get { return new UIColor("#800080"); } }
        public static UIColor RoyalBlue { get { return new UIColor("#4169E1"); } }
        public static UIColor SteelBlue { get { return new UIColor("#4682B4"); } }
        public static UIColor Violet { get { return new UIColor("#EE82EE"); } }
        public static UIColor Yellow { get { return new UIColor("#FFFF00"); } }
        public static UIColor YellowGreen { get { return new UIColor("#9ACD32"); } }
        public static UIColor WhiteSmoke { get { return new UIColor("#F5F5F5"); } }
        public static UIColor White { get { return new UIColor("#FFFFFF"); } }
        public static UIColor SaddleBrown { get { return new UIColor("#8B4513"); } }
        public static UIColor SandyBrown { get { return new UIColor("#F4A460"); } }
        public static UIColor Salmon { get { return new UIColor("#FA8072"); } }
        public static UIColor OliveDrab { get { return new UIColor("#6B8E23"); } }
        public static UIColor Olive { get { return new UIColor("#808000"); } }
        public static UIColor MediumVioletRed { get { return new UIColor("#C71585"); } }
        public static UIColor MidnightBlue { get { return new UIColor("#191970"); } }
        public static UIColor MediumTurquoise { get { return new UIColor("#48D1CC"); } }
        public static UIColor MediumPurple { get { return new UIColor("#9370DB"); } }
        public static UIColor LawnGreen { get { return new UIColor("#7CFC00"); } }
        public static UIColor Gold { get { return new UIColor("#FFD700"); } }
        public static UIColor GhostWhite { get { return new UIColor("#F8F8FF"); } }
        public static UIColor FireBrick { get { return new UIColor("#B22222"); } }
        public static UIColor DarkSlateBlue { get { return new UIColor("#483D8B"); } }
        public static UIColor DimGray { get { return new UIColor("#696969"); } }
        public static UIColor DeepSkyBlue { get { return new UIColor("#00BFFF"); } }
        public static UIColor DeepPink { get { return new UIColor("#FF1493"); } }
        public static UIColor DarkOrange { get { return new UIColor("#FF8C00"); } }
        public static UIColor DarkMagenta { get { return new UIColor("#8B008B"); } }
        public static UIColor Fuchsia { get { return new UIColor("#FF00FF"); } }
        public static UIColor DodgerBlue { get { return new UIColor("#1E90FF"); } }
    }
}