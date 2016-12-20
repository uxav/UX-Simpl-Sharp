using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Audio
    {
        internal Audio(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
            Microphones = new Microphones(codec);
        }

        CiscoCodec Codec;
        public Microphones Microphones;

        bool _Mute;
        bool Mute
        {
            get
            {
                return _Mute;
            }
            set
            {
                XDocument xml;
                if (value)
                    xml = Codec.SendCommand("Audio/Volume/Mute");
                else
                    xml = Codec.SendCommand("Audio/Volume/Unmute");
                if (xml.Root.Elements().FirstOrDefault().Attribute("status").Value == "OK")
                {
                    _Mute = value;
                    OnVolumeChange();
                }
            }
        }

        public event CodecAudioVolumeMuteChangeEventHandler MuteChange;

        void OnMuteChange()
        {
            if (MuteChange != null)
                MuteChange(Codec, Mute);

            this.Codec.FusionUpdate();
        }

        int _Volume;
        public int Volume
        {
            get
            {
                return _Volume;
            }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    XDocument xml = Codec.SendCommand("Audio/Volume/Set", new CommandArgs("Level", value));
                    if (xml.Root.Elements().FirstOrDefault().Attribute("status").Value == "OK")
                        _Volume = value;
                    OnVolumeChange();
                }
            }
        }

        public event CodecAudioVolumeChangeEventHandler VolumeChange;

        void OnVolumeChange()
        {
            if (VolumeChange != null)
                VolumeChange(Codec, Volume);
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/Audio").Elements().Where(e => !e.HasElements))
            {
                switch (element.XName.LocalName)
                {
                    case "Volume": _Volume = int.Parse(element.Value); break;
                    case "VolumeMute":

                        if (element.Value == "On") _Mute = true;
                        else _Mute = false;
                        break;
                }
            }
#if DEBUG
            CrestronConsole.PrintLine("Volume = {0}", Volume);
            CrestronConsole.PrintLine("Volume Mute = {0}", Mute);
#endif
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            switch (args.Path)
            {
                case @"Status/Audio":
                    foreach (XElement element in args.Data.Elements())
                    {
                        switch (element.XName.LocalName)
                        {
                            case "Volume":
                                int level = int.Parse(element.Value);
                                if (level != Volume)
                                {
                                    _Volume = level;
                                    OnVolumeChange();
                                }
                                break;
                            case "VolumeMute":
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

    public delegate void CodecAudioVolumeChangeEventHandler(CiscoCodec codec, int VolumeLevel);
    public delegate void CodecAudioVolumeMuteChangeEventHandler(CiscoCodec codec, bool VolumeMute);
}