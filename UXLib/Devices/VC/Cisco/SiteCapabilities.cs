using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class SiteCapabilities
    {
        public SiteCapabilities(Site site)
        {
            Site = site;
        }

        Site Site { set; get; }

        public bool Presentation { get; set; }
        public bool FECC { get; set; }
    }
}