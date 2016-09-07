using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class SystemUnitSoftware
    {
        public SystemUnitSoftware(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        public string Application { get; protected set; }
        public int MaxAudioCalls { get; protected set; }
        public int MaxVideoCalls { get; protected set; }
        public string Name { get; protected set; }
        public DateTime ReleaseDate { get; protected set; }
        public string Version { get; protected set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/SystemUnit/Software")
                .Elements().Where(e => !e.HasElements))
            {
#if DEBUG
                CrestronConsole.PrintLine("SystemUnit.Software.{0} = {1}", element.XName.LocalName, element.Value);
#endif
                switch (element.XName.LocalName)
                {
                    case "Application": Application = element.Value; break;
                    case "MaxAudioCalls": MaxAudioCalls = int.Parse(element.Value); break;
                    case "MaxVideoCalls": MaxVideoCalls = int.Parse(element.Value); break;
                    case "Name": Name = element.Value; break;
                    case "ReleaseDate": ReleaseDate = DateTime.Parse(element.Value); break;
                    case "Version": Version = element.Value; break;
                }
            }
        }
    }
}