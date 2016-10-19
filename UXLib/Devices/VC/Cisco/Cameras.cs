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
                                element.Element("Manufacturer").Value,
                                element.Element("Model").Value);
                            foreach (XElement property in element.Elements())
                            {
                                switch (property.XName.LocalName)
                                {
                                    case "MacAddress": newCamera.MacAddress = property.Value; break;
                                    case "SerialNumber": newCamera.SerialNumber = property.Value; break;
                                    case "SoftwareID": newCamera.SoftwareID = property.Value; break;
                                }
                            }
                            this[cameraId] = newCamera;
                        }
                        else
                        {
                            foreach (XElement property in element.Elements())
                            {
                                switch (property.XName.LocalName)
                                {
                                    case "Connected": bool.Parse(property.Value); break;
                                    case "MacAddress": this[cameraId].MacAddress = property.Value; break;
                                    case "Manufacturer": this[cameraId].Manufacturer = property.Value; break;
                                    case "Model": this[cameraId].Model = property.Value; break;
                                    case "SerialNumber": this[cameraId].SerialNumber = property.Value; break;
                                    case "SoftwareID": this[cameraId].SoftwareID = property.Value; break;
                                }
                            }
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