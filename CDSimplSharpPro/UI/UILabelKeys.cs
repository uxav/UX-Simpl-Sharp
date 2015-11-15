using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UILabelKeys
    {
        public static UIKey RoomName = new UIKey("RoomName", 10);
        public static UIKey PageTitle = new UIKey("PageTitle", 11);
        public static UIKey Date = new UIKey("Date", 7);
        public static UIKey Time = new UIKey("Time", 6);
        public static UIKey ProjectName = new UIKey("ProjectName", 20);
        public static UIKey VersionName = new UIKey("VersionName", 21);
        public static UIKey VersionInfo = new UIKey("VersionInfo", 22);
        public static UIKey CDLibVersionNumber = new UIKey("CDLibVersionNumber", 23);
    }
}