using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIButtonKeys
    {
        public static UIKey Power = new UIKey(@"Power", 1);
        public static UIKey Home = new UIKey(@"Home", 2);
        public static UIKey Lights = new UIKey(@"Lights", 3);
        public static UIKey Up = new UIKey(@"Up", 4);
        public static UIKey Down = new UIKey(@"Down", 5);
    }
}