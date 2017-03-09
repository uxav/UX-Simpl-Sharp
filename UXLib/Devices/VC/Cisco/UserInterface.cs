using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class UserInterface
    {
        internal UserInterface(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
            ContactInfo = new UserInterfaceContactInfo();
        }

        CiscoCodec Codec;
        public UserInterfaceContactInfo ContactInfo { get; internal set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/UserInterface").Elements())
            {
                switch (element.XName.LocalName)
                {
                    case "ContactInfo":
                        foreach (XElement contactInfoElement in element.Elements())
                        {
                            switch (contactInfoElement.XName.LocalName)
                            {
                                case "Name": this.ContactInfo.Name = contactInfoElement.Value; break;
                                case "ContactMethod":
                                    uint index = uint.Parse(contactInfoElement.Attribute("item").Value);
                                    this.ContactInfo._ContactMethods[index] = new UserInterfaceContactInfoMethod(contactInfoElement.Element("Number").Value);
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}