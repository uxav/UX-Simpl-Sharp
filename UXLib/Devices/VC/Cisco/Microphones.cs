using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Microphones
    {
        public Microphones(Codec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        Codec Codec;

        bool _Mute;
        public bool Mute
        {
            get
            {
                return _Mute;
            }
            set
            {
                if (_Mute != value)
                {
                    XDocument xml;
                    if (value)
                        xml = Codec.SendCommand("Command/Audio/Microphones/Mute", new CommandArgs());
                    else
                        xml = Codec.SendCommand("Command/Audio/Microphones/Unmute", new CommandArgs());

                    if (xml.Root.Elements().FirstOrDefault().Attribute("status").Value == "OK")
                    {
                        _Mute = value;
                    }
                }
            }
        }

        public event CodecAudioMicrophonesMuteChangeEventHandler MuteChange;

        void OnMuteChange()
        {
            if (MuteChange != null)
                MuteChange(Codec, Mute);
        }

        void Codec_HasConnected(Codec codec)
        {
            XElement element = Codec.RequestPath("Status/Audio/Microphones", true).Elements().FirstOrDefault();
            if (element.XName.LocalName == "Mute")
            {
                if (element.Value == "On") _Mute = true;
                else _Mute = false;
            }
#if DEBUG
            CrestronConsole.PrintLine("Mic Mute = {0}", Mute);
#endif
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            switch (args.Path)
            {
                case "Status/Audio/Microphones":
                    foreach (XElement element in args.Data.Elements())
                    {
                        switch (element.XName.LocalName)
                        {
                            case "Mute":
                                if (element.Value == "Off" && Mute)
                                {
                                    _Mute = false;
                                    OnMuteChange();
                                }
                                else if (element.Value == "On" && !Mute)
                                {
                                    _Mute = true;
                                    OnMuteChange();
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public delegate void CodecAudioMicrophonesMuteChangeEventHandler(Codec codec, bool MuteValue);
}