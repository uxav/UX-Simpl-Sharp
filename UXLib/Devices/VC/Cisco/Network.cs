using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Network
    {
        public Network(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;
        public string MacAddress { get; protected set; }
        public string Speed { get; protected set; }
        public string Address { get; protected set; }
        public string SubnetMask { get; protected set; }
        public string Gateway { get; protected set; }
        public string CDPPlatform { get; protected set; }
        public string CDPDeviceId { get; protected set; }
        public string CDPPortID { get; protected set; }
        public int CDPVoIPApplianceVlanID { get; protected set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement networkInfo in Codec.RequestPath("Status/Network", true).FirstOrDefault().Elements())
            {
                switch (networkInfo.XName.LocalName)
                {
                    case "Ethernet":
                        foreach (XElement e in networkInfo.Elements().Where(e => !e.HasElements))
                        {
#if DEBUG
                            CrestronConsole.PrintLine("{0} = {1}", e.XName.LocalName, e.Value);
#endif
                            switch (e.XName.LocalName)
                            {
                                case "Speed": Speed = e.Value; break;
                                case "MacAddress": MacAddress = e.Value; break;
                            }
                        }
                        break;
                    case "IPv4":
                        foreach (XElement e in networkInfo.Elements().Where(e => !e.HasElements))
                        {
#if DEBUG
                            CrestronConsole.PrintLine("{0} = {1}", e.XName.LocalName, e.Value);
#endif
                            switch (e.XName.LocalName)
                            {
                                case "Address": Address = e.Value; break;
                                case "SubnetMask": SubnetMask = e.Value; break;
                                case "Gateway": Gateway = e.Value; break;
                            }
                        }
                        break;
                    case "CDP":
                        foreach (XElement e in networkInfo.Elements().Where(e => !e.HasElements))
                        {
#if DEBUG
                            CrestronConsole.PrintLine("{0} = {1}", e.XName.LocalName, e.Value);
#endif
                            switch (e.XName.LocalName)
                            {
                                case "CDPPlatform": CDPPlatform = e.Value; break;
                                case "CDPDeviceId": CDPDeviceId = e.Value; break;
                                case "CDPPortID": CDPPortID = e.Value; break;
                                case "CDPVoIPApplianceVlanID": CDPVoIPApplianceVlanID = int.Parse(e.Value); break;
                            }
                        }
                        break;
                }
            }
        }

        public void Update()
        {
            Codec_HasConnected(this.Codec);
        }
    }
}