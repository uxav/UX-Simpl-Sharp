using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Site
    {
        public Site(CiscoCodec codec, int id)
        {
            Codec = codec;
            ID = id;
            Capabilities = new SiteCapabilities(this);
        }

        CiscoCodec Codec { get; set; }

        public int ID { get; protected set; }
        public int Appearance { get; set; }
        public bool Hold { get; set; }
        public bool MicrophonesMuted { get; set; }
        public bool AttendedTransfer { get; set; }
        public SiteCapabilities Capabilities { get; protected set; }
    }
}