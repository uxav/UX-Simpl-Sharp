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
}