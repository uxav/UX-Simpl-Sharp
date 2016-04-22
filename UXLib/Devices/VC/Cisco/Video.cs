using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Video
    {
        public Video(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec { get; set; }

        public event CodecVideoSelfViewEventHandler SelfViewChanged;

        public void SelfViewSet(SelfViewMode mode)
        {
            Codec.SendCommand("Video/Selfview/Set", new CommandArgs("Mode", mode.ToString()));
        }

        public void SelfViewSet(SelfViewFullscreenMode mode)
        {
            Codec.SendCommand("Video/Selfview/Set", new CommandArgs("FullscreenMode", mode.ToString()));
        }

        SelfViewMode _SelfViewMode;

        public SelfViewMode SelfViewMode
        {
            get { return _SelfViewMode; }
            set
            {
                _SelfViewMode = value;
                SelfViewSet(value);
            }
        }

        SelfViewFullscreenMode _SelfViewFullscreenMode;

        public SelfViewFullscreenMode SelfViewFullscreenMode
        {
            get { return _SelfViewFullscreenMode; }
            set
            {
                _SelfViewFullscreenMode = value;
                SelfViewSet(value);
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            XElement element = Codec.RequestPath("Status/Video/SelfView", true).Elements().FirstOrDefault();
            XElement s = element.Elements().Where(x => x.XName.LocalName == "Selfview").FirstOrDefault();

#if DEBUG
            CrestronConsole.PrintLine("Selfview Status:\r\n{0}", s.ToString());
#endif

            foreach (XElement e in s.Elements())
            {
                switch (e.XName.LocalName)
                {
                    case "Mode": _SelfViewMode = (SelfViewMode)Enum.Parse(typeof(SelfViewMode), e.Value, false);
                        break;
                    case "FullscreenMode": _SelfViewFullscreenMode = (SelfViewFullscreenMode)Enum.Parse(typeof(SelfViewFullscreenMode), e.Value, false);
                        break;
                }
            }
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            if (args.Path.StartsWith("Status/Video"))
            {
                CrestronConsole.PrintLine("Status for {0}", args.Path);
                CrestronConsole.PrintLine(args.Data.ToString());
            }

            switch (args.Path)
            {
                case "Status/Video/SeflView":
                    foreach (XElement e in args.Data.Elements())
                    {
                        switch (e.XName.LocalName)
                        {
                            case "Mode": _SelfViewMode = (SelfViewMode)Enum.Parse(typeof(SelfViewMode), e.Value, false);
                                break;
                            case "FullscreenMode": _SelfViewFullscreenMode = (SelfViewFullscreenMode)Enum.Parse(typeof(SelfViewFullscreenMode), e.Value, false);
                                break;
                        }
                    }

                    if (SelfViewChanged != null)
                    {
                        SelfViewChanged(this);
                    }
                    break;
            }
        }
    }

    public delegate void CodecVideoSelfViewEventHandler(Video video);

    public enum SelfViewMode
    {
        Off,
        On
    }

    public enum SelfViewFullscreenMode
    {
        Off,
        On
    }
}