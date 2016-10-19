using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXmlLinq;
using UXLib.Models;

namespace UXLib.Devices.VC.Cisco
{
    public class Microphones : IVolumeDevice
    {
        internal Microphones(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

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
            if (VolumeChanged != null)
                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            XElement element = Codec.RequestPath("Status/Audio/Microphones").Elements().FirstOrDefault();
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

        #region IVolumeDevice Members

        public string Name
        {
            get { return this.Codec.SystemUnit.ProductId; }
        }

        public ushort VolumeLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool VolumeMute
        {
            get
            {
                return this.Mute;
            }
            set
            {
                this.Mute = value;
            }
        }

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public bool SupportsVolumeLevel
        {
            get { return false; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }

    public delegate void CodecAudioMicrophonesMuteChangeEventHandler(CiscoCodec codec, bool MuteValue);
}