using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebChannel
    {
        public SoundWebChannel(SoundWebMultiChannelObject owner, uint index)
        {
            this.Owner = owner;
            this.Index = index;
            this.Owner.FeedbackReceived += new SoundWebObjectFeedbackEventHandler(Mixer_FeedbackReceived);
        }

        public virtual void Send(string messageType, SoundWebChannelParamType paramType, string value)
        {
            
        }

        public SoundWebMultiChannelObject Owner { get; protected set; }
        public uint Index { get; protected set; }

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

                    Send("\x88", SoundWebChannelParamType.Gain, string.Format("{0}{1}{2}{3}", (char)bytes[0], (char)bytes[1], (char)bytes[2], (char)bytes[3]));

                    if (ChangeEvent != null)
                        ChangeEvent(this, new SoundWebChannelEventArgs(SoundWebChannelEventType.GainChange));
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
                        Send("\x88", SoundWebChannelParamType.Mute, "\x00\x00\x00\x01");
                    else
                        Send("\x88", SoundWebChannelParamType.Mute, "\x00\x00\x00\x00");
                    if (ChangeEvent != null)
                        ChangeEvent(this, new SoundWebChannelEventArgs(SoundWebChannelEventType.MuteChange));
                }
            }
            get
            {
                return _mute;
            }
        }

        public void Subscribe()
        {
            this.Subscribe(SoundWebChannelParamType.Gain);
            this.Subscribe(SoundWebChannelParamType.Mute);
        }

        public void Subscribe(SoundWebChannelParamType paramType)
        {
            Owner.Subscribe();
            Send("\x89", paramType, "\x00\x00\x00\x00");
        }

        protected virtual uint GetChannelFromParamID(int paramID)
        {
            // this should be overriden
            return 0;
        }

        protected virtual SoundWebChannelParamType GetSoundWebChannelParamType(int paramID)
        {
            // this should be overriden
            return SoundWebChannelParamType.Gain;
        }

        void Mixer_FeedbackReceived(SoundWebObject soundWebObject, SoundWebObjectFeedbackEventArgs args)
        {
            uint channel = GetChannelFromParamID(args.ParamID);
            SoundWebChannelParamType channelControlType = GetSoundWebChannelParamType(args.ParamID);

            if (channel == this.Index)
            {
                switch (channelControlType)
                {
                    case SoundWebChannelParamType.Gain:
                        _gain = args.Value;
                        if (ChangeEvent != null)
                            ChangeEvent(this, new SoundWebChannelEventArgs(SoundWebChannelEventType.GainChange));
                        break;
                    case SoundWebChannelParamType.Mute:
                        if (args.Value == 1)
                            _mute = true;
                        else
                            _mute = false;
                        if (ChangeEvent != null)
                            ChangeEvent(this, new SoundWebChannelEventArgs(SoundWebChannelEventType.MuteChange));
                        break;
                }
            }
        }

        public event SoundWebChannelEventHandler ChangeEvent;
        
        /// <summary>
        /// Unregister from any sig changes and dispose of resources
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization. - Sandbox litits this currently
            // GC.SuppressFinalize(this);
        }
        
        bool disposed = false;

        public bool Disposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>
        /// Override this to free resources
        /// </summary>
        /// <param name="disposing">true is Dispose() has been called</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.Owner.FeedbackReceived -= new SoundWebObjectFeedbackEventHandler(Mixer_FeedbackReceived);
            }

            // Free any unmanaged objects here.
            //
            

            disposed = true;
        }
    }

    public delegate void SoundWebChannelEventHandler(SoundWebChannel channel, SoundWebChannelEventArgs args);

    public class SoundWebChannelEventArgs : EventArgs
    {
        public SoundWebChannelEventArgs(SoundWebChannelEventType eventType)
        {
            EventType = eventType;
        }

        public SoundWebChannelEventType EventType;
    }

    public enum SoundWebChannelEventType
    {
        MuteChange,
        GainChange
    }

    public enum SoundWebChannelParamType
    {
        Gain,
        Mute
    }
}