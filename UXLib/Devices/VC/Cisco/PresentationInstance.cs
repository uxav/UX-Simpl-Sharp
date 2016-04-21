using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PresentationInstance
    {
        public PresentationInstance(CiscoCodec codec, int instanceID)
        {
            Codec = codec;
            InstanceID = instanceID;
        }

        CiscoCodec Codec { get; set; }

        public int InstanceID { get; protected set; }
        public PresentationSendingMode LocalSendingMode { get; set; }
        public int LocalSource { get; set; }
        public string Mode { get; set; }
    }
}