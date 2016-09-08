using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using UXLib.Models;

namespace UXLib.Devices.VC.Cisco
{
    public class Cameras : UXCollection<Camera>
    {
        internal Cameras(CiscoCodec codec)
        {
            Codec = codec;
            SpeakerTrack = new SpeakerTrack(Codec);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        public override Camera this[uint cameraID]
        {
            get
            {
                return base[cameraID];
            }
            internal set
            {
                base[cameraID] = value;
            }
        }

        public IEnumerable<Camera> Connected
        {
            get
            {
                return InternalDictionary.Values.Where(c => c.Connected);
            }
        }

        public SpeakerTrack SpeakerTrack { get; private set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            try
            {
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/Cameras");

                foreach (XElement element in statusInfo.Elements("Camera"))
                {
                    uint cameraId = uint.Parse(element.Attribute("item").Value);
#if DEBUG
                    CrestronConsole.PrintLine("Info for Camera {0}:", cameraId);

                    foreach (XElement innerElement in element.Elements().Where(e => !e.HasElements))
                    {
                        CrestronConsole.PrintLine("   Camera.{0} = {1}", innerElement.XName.LocalName, innerElement.Value);
                    }
#endif
                    if (bool.Parse(element.Element("Connected").Value))
                    {
                        if (!this.Contains(cameraId))
                        {
                            Camera newCamera = new Camera(Codec, cameraId,
                                bool.Parse(element.Element("Connected").Value),
                                element.Element("MacAddress").Value,
                                element.Element("Manufacturer").Value,
                                element.Element("Model").Value,
                                element.Element("SerialNumber").Value,
                                element.Element("SoftwareID").Value);
                            this[cameraId] = newCamera;
                        }
                        else
                        {
                            this[cameraId].Connected = bool.Parse(element.Element("Connected").Value);
                            this[cameraId].MacAddress = element.Element("MacAddress").Value;
                            this[cameraId].Manufacturer = element.Element("Manufacturer").Value;
                            this[cameraId].Model = element.Element("Model").Value;
                            this[cameraId].SerialNumber = element.Element("SerialNumber").Value;
                            this[cameraId].SoftwareID = element.Element("SoftwareID").Value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error parsing camera information for CiscoCodec", e);
            }
        }
    }
}