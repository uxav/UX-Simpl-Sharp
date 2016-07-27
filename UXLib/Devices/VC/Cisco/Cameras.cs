using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Cameras : IEnumerable<Camera>
    {
        public Cameras(CiscoCodec codec)
        {
            Codec = codec;
            SpeakerTrack = new SpeakerTrack(Codec);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;
        Dictionary<int, Camera> _Cameras = new Dictionary<int, Camera>();

        public Camera this[int cameraID]
        {
            get
            {
                return _Cameras[cameraID];
            }
        }

        public SpeakerTrack SpeakerTrack { get; private set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            try
            {
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/Cameras", true);

                foreach (XElement element in statusInfo.Elements("Camera"))
                {
                    int cameraId = int.Parse(element.Attribute("item").Value);
#if DEBUG
                    CrestronConsole.PrintLine("Info for Camera {0}:", cameraId);

                    foreach (XElement innerElement in element.Elements().Where(e => !e.HasElements))
                    {
                        CrestronConsole.PrintLine("   Camera.{0} = {1}", innerElement.XName.LocalName, innerElement.Value);
                    }
#endif

                    Camera newCamera = new Camera(Codec, cameraId,
                        bool.Parse(element.Element("Connected").Value),
                        element.Element("MacAddress").Value,
                        element.Element("Manufacturer").Value,
                        element.Element("Model").Value,
                        element.Element("SerialNumber").Value,
                        element.Element("SoftwareID").Value);

                    _Cameras[cameraId] = newCamera;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error in Cameras.Codec_HasConnected", e);
            }
        }

        #region IEnumerable<Camera> Members

        public IEnumerator<Camera> GetEnumerator()
        {
            return _Cameras.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}