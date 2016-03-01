using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
{
    public class SoundWebMixerChannel
    {
        public SoundWebMixerChannel(SoundWebMixer mixer, uint index)
        {
            this.Mixer = mixer;
            this.Index = index;
            this.Mixer.FeedbackReceived += new SoundWebObjectFeedbackEventHandler(Mixer_FeedbackReceived);
        }

        public SoundWebMixer Mixer { get; protected set; }
        public uint Index { get; protected set; }

        public void Send(string messageType, SoundWebMixerChannelParamType paramType, string value)
        {
            ushort pVal = Convert.ToUInt16(((this.Index - 1) * 100) + (int)paramType);
            byte upper = (byte) (pVal >> 8);
            byte lower = (byte) (pVal & 0xff);
            this.Mixer.Send(messageType, string.Format("{0}{1}", (char)upper, (char)lower), value);
        }

        int _gain;
        public int Gain
        {
            set
            {
                if (value >= -280617 && value <= 100000)
                {
                    _gain = value;

                    byte[] bytes = new byte[4];

                    bytes[0] = (byte)(value >> 24);
                    bytes[1] = (byte)(value >> 16);
                    bytes[2] = (byte)(value >> 8);
                    bytes[3] = (byte)(value & 0xff);

                    this.Send("\x88", (uint)SoundWebMixerChannelParamType.Gain, string.Format("{0}{1}{2}{3}", (char)bytes[0], (char)bytes[1], (char)bytes[2], (char)bytes[3]));

                    if (ChangeEvent != null)
                        ChangeEvent(this, new SoundWebMixerChannelEventArgs(SoundWebMixerChannelEventType.GainChange));
                }
            }
            get
            {
                return _gain;
            }
        }

        bool _mute;
        public bool Mute
        {
            set
            {
                if (_mute != value)
                {
                    _mute = value;
                    if (value)
                        this.Send("\x88", SoundWebMixerChannelParamType.Mute, "\x00\x00\x00\x01");
                    else
                        this.Send("\x88", SoundWebMixerChannelParamType.Mute, "\x00\x00\x00\x00");
                    if (ChangeEvent != null)
                        ChangeEvent(this, new SoundWebMixerChannelEventArgs(SoundWebMixerChannelEventType.MuteChange));
                }
            }
            get
            {
                return _mute;
            }
        }

        public void Subscribe()
        {
            this.Subscribe(SoundWebMixerChannelParamType.Gain);
            this.Subscribe(SoundWebMixerChannelParamType.Mute);
        }

        public void Subscribe(SoundWebMixerChannelParamType paramType)
        {
            Mixer.Subscribe();
            CrestronConsole.PrintLine("Mixer Channel[{0}].Subscribe({1});", this.Index, paramType.ToString());
            this.Send("\x89", paramType, "\x00\x00\x00\x00");
        }

        void Mixer_FeedbackReceived(SoundWebObject soundWebObject, SoundWebObjectFeedbackEventArgs args)
        {
            uint channel = (uint)(args.ParamID / 100) + 1;
            uint channelControlType = (uint)(args.ParamID % 100);

            if (channel == this.Index)
            {
                switch ((SoundWebMixerChannelParamType)channelControlType)
                {
                    case SoundWebMixerChannelParamType.Gain:
                        _gain = args.Value;
                        if (ChangeEvent != null)
                            ChangeEvent(this, new SoundWebMixerChannelEventArgs(SoundWebMixerChannelEventType.GainChange));
                        break;
                    case SoundWebMixerChannelParamType.Mute:
                        if (args.Value == 1)
                            _mute = true;
                        else
                            _mute = false;
                        if (ChangeEvent != null)
                            ChangeEvent(this, new SoundWebMixerChannelEventArgs(SoundWebMixerChannelEventType.MuteChange));
                        break;
                }
            }
        }

        public event SoundWebMixerChannelEventHandler ChangeEvent;
    }

    public delegate void SoundWebMixerChannelEventHandler(SoundWebMixerChannel channel, SoundWebMixerChannelEventArgs args);

    public class SoundWebMixerChannelEventArgs : EventArgs
    {
        public SoundWebMixerChannelEventArgs(SoundWebMixerChannelEventType eventType)
        {
            EventType = eventType;
        }

        public SoundWebMixerChannelEventType EventType;
    }

    public enum SoundWebMixerChannelEventType
    {
        MuteChange,
        GainChange
    }

    public enum SoundWebMixerChannelParamType
    {
        Gain,
        Mute
    }
}