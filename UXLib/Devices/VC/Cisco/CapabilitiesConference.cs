using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class CapabilitiesConference
    {
        internal CapabilitiesConference(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        public int MaxActiveCalls { get; protected set; }
        public int MaxAudioCalls { get; protected set; }
        public int MaxCalls { get; protected set; }
        public int MaxVideoCalls { get; protected set; }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            switch (args.Path)
            {
                case @"Status/Capabilities/Conference":
                    foreach (XElement state in args.Data.Elements())
                    {
                        switch (state.XName.LocalName)
                        {
                            case "MaxActiveCalls": MaxActiveCalls = int.Parse(state.Value); break;
                            case "MaxAudioCalls": MaxAudioCalls = int.Parse(state.Value); break;
                            case "MaxCalls": MaxCalls = int.Parse(state.Value); break;
                            case "MaxVideoCalls": MaxVideoCalls = int.Parse(state.Value); break;
                        }
                    }
                    break;
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            try
            {
                foreach (XElement element in Codec.RequestPath("Status/Capabilities/Conference").Elements().Where(e => !e.HasElements))
                {
#if DEBUG
                    CrestronConsole.PrintLine("Capabilities.Conference.{0} = {1}", element.XName.LocalName, element.Value);
#endif
                    switch (element.XName.LocalName)
                    {
                        case "MaxActiveCalls": MaxActiveCalls = int.Parse(element.Value); break;
                        case "MaxAudioCalls": MaxAudioCalls = int.Parse(element.Value); break;
                        case "MaxCalls": MaxCalls = int.Parse(element.Value); break;
                        case "MaxVideoCalls": MaxVideoCalls = int.Parse(element.Value); break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error in Conference.Capabilities.Codec_HasConnected", e);
            }
        }
    }
}