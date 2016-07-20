using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class SpeakerTrackCamera
    {
        public SpeakerTrackCamera(uint connector)
        {
            VideoInputConnector = connector;
        }

        public uint VideoInputConnector { get; protected set; }
    }
}