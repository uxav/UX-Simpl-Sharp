using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class SystemUnitHardware
    {
        internal SystemUnitHardware(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        public string ModuleSerialNumber { get; protected set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/SystemUnit/Hardware")
                .Elements().Where(e => !e.HasElements))
            {
#if DEBUG
                CrestronConsole.PrintLine("SystemUnit.Software.{0} = {1}", element.XName.LocalName, element.Value);
#endif
                switch (element.XName.LocalName)
                {
                    case "Hardware":
                        foreach (XElement e in element.Element("Module").Elements())
                        {
                            if (e.XName.LocalName == "SerialNumber")
                                ModuleSerialNumber = e.Value;
                        }
                        Codec.FusionUpdate();
                        break;
                }
            }
        }
    }
}