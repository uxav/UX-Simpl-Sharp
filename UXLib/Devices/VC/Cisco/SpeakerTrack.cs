using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class SpeakerTrack
    {
        public SpeakerTrack(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
        }

        CiscoCodec Codec;

        public void Activate()
        {
            if (Codec.SendCommand("Cameras/SpeakerTrack/Activate").Root.Elements().FirstOrDefault().Attribute("status").Value == "OK")
                this.Active = true;
        }

        public void Deactivate()
        {
            if (Codec.SendCommand("Cameras/SpeakerTrack/Deactivate").Root.Elements().FirstOrDefault().Attribute("status").Value == "OK")
                Active = false;
        }

        public SpeakerTrackCamera LeftCamera { get; protected set; }
        public SpeakerTrackCamera RightCamera { get; protected set; }

        public bool Active { get; protected set; }
        public SpeakerTrackAvailability Availability { get; protected set; }

        void Codec_HasConnected(CiscoCodec codec)
        {
            IEnumerable<XElement> status = Codec.RequestPath("Status/Cameras/SpeakerTrack");
            
            foreach (XElement element in status.FirstOrDefault().Elements())
            {
                if (element.HasElements)
                {
                    if (element.XName.LocalName == "LeftCamera")
                        LeftCamera = new SpeakerTrackCamera(uint.Parse(element.Element("VideoInputConnector").Value));
                    else if(element.XName.LocalName == "RightCamera")
                        RightCamera = new SpeakerTrackCamera(uint.Parse(element.Element("VideoInputConnector").Value));
                }
                else
                {
#if DEBUG
                    CrestronConsole.PrintLine("Status.Cameras.SpeakerTrack.{0} = {1}", element.XName.LocalName, element.Value);
#endif

                    switch (element.XName.LocalName)
                    {
                        case "Availability": Availability = (SpeakerTrackAvailability)Enum.Parse(typeof(SpeakerTrackAvailability), element.Value, true); break;
                        case "Status": if (element.Value.ToLower() == "active") this.Active = true; else this.Active = false; break;
                    }
                }
            }

#if DEBUG
            if (LeftCamera != null)
                CrestronConsole.PrintLine("Status.Cameras.SpeakerTrack.LeftCamera.VideoInputConnector = {0}", LeftCamera.VideoInputConnector);
            if (RightCamera != null)
                CrestronConsole.PrintLine("Status.Cameras.SpeakerTrack.RightCamera.VideoInputConnector = {0}", RightCamera.VideoInputConnector);
#endif
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            if (args.Path == "Status/Cameras/SpeakerTrack")
            {
                foreach (XElement element in args.Data.Elements())
                {
                    switch (element.XName.LocalName)
                    {
                        case "Availability": Availability = (SpeakerTrackAvailability)Enum.Parse(typeof(SpeakerTrackAvailability), element.Value, true); break;
                        case "Status": if (element.Value.ToLower() == "active") this.Active = true; else this.Active = false; break;
                    }
                }
            }
        }
    }

    public enum SpeakerTrackAvailability
    {
        Off,
        Unavailable,
        Available
    }
}