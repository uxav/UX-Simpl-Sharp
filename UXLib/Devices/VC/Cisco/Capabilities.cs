using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Capabilities
    {
        public Capabilities(CiscoCodec codec)
        {
            Codec = codec;
            Conference = new CapabilitiesConference(codec);
        }

        CiscoCodec Codec;

        public CapabilitiesConference Conference { get; protected set; }
    }
}