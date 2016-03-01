using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UXLib.UI;

namespace UXLib.Audio.BSS
{
    public class SoundWebMixerChannelUILevel : UILevel
    {
        /// <summary>
        /// Feedback level only
        /// </summary>
        /// <param name="analogFeedbackJoin">Analog feedback join to level</param>
        /// <param name="channel">The mixer channel for the soundweb to follow</param>
        /// <param name="gainMin">Value from -280617 to 100000</param>
        /// <param name="gainMax">Value from -280617 to 100000</param>
        /// <remarks>-100000 to 100000 = -10dB to +10dB, values below -100000 are log scaled down to -80dB
        /// -160204 is -20dB
        /// </remarks>
        public SoundWebMixerChannelUILevel(UShortInputSig analogFeedbackJoin, SoundWebMixerChannel channel, int gainMin, int gainMax)
            : base(analogFeedbackJoin)
        {
            this.Channel = channel;
            if (gainMin >= -280617 && gainMin <= 100000)
                this.GainMinValue = gainMin;
            else
                this.GainMinValue = -280617;
            if (gainMax >= -280617 && gainMax <= 100000)
                this.GainMaxValue = gainMax;
            else
                this.GainMaxValue = 100000;
            this.Channel.ChangeEvent += new SoundWebMixerChannelEventHandler(Channel_ChangeEvent);
            this.Channel.Mixer.Device.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            if (this.Channel.Mixer.Device.Socket.Connected)
                Channel.Subscribe(SoundWebMixerChannelParamType.Gain);
        }

        /// <summary>
        /// Control level
        /// </summary>
        /// <param name="analogFeedbackJoin">Analog feedback join to level</param>
        /// <param name="analogTouchJoin">Analog touch join from level</param>
        /// <param name="channel">The mixer channel for the soundweb to follow</param>
        /// <param name="gainMin">Value from -280617 to 100000</param>
        /// <param name="gainMax">Value from -280617 to 100000</param>
        /// <remarks>-100000 to 100000 = -10dB to +10dB, values below -100000 are log scaled down to -80dB
        /// -160204 is -20dB
        /// </remarks>
        public SoundWebMixerChannelUILevel(UShortInputSig analogFeedbackJoin, UShortOutputSig analogTouchJoin,
            SoundWebMixerChannel channel, int gainMin, int gainMax)
            : base(analogFeedbackJoin, analogTouchJoin)
        {
            this.Channel = channel;
            if (gainMin >= -280617 && gainMin <= 100000)
                this.GainMinValue = gainMin;
            else
                this.GainMinValue = -280617;
            if (gainMax >= -280617 && gainMax <= 100000)
                this.GainMaxValue = gainMax;
            else
                this.GainMaxValue = 100000;
            this.Channel.ChangeEvent += new SoundWebMixerChannelEventHandler(Channel_ChangeEvent);
            this.Channel.Mixer.Device.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            if (this.Channel.Mixer.Device.Socket.Connected)
                Channel.Subscribe(SoundWebMixerChannelParamType.Gain);
        }

        public SoundWebMixerChannel Channel { get; protected set; }

        protected new ushort LevelMinimumValue
        {
            get
            {
                return base.LevelMinimumValue;
            }
        }

        protected new ushort LevelMaximumValue
        {
            get
            {
                return base.LevelMaximumValue;
            }
        }

        public int GainMinValue { get; protected set; }
        public int GainMaxValue { get; protected set; }

        protected override void OnValueChange(ushort newValue)
        {
            if (newValue == this.LevelMinimumValue)
                this.Channel.Gain = -280617;
            else
            {
                this.Channel.Gain = (int)UILevel.ScaleRange(
                    newValue, this.LevelMinimumValue, this.LevelMaximumValue, this.GainMinValue, this.GainMaxValue);
            }
            base.OnValueChange(newValue);
        }

        void Channel_ChangeEvent(SoundWebMixerChannel channel, SoundWebMixerChannelEventArgs args)
        {
            if (args.EventType == SoundWebMixerChannelEventType.GainChange)
            {
                if (channel.Gain <= this.GainMinValue)
                    this.AnalogFeedbackValue = this.LevelMinimumValue;
                else if (channel.Gain >= this.GainMaxValue)
                    this.AnalogFeedbackValue = this.LevelMaximumValue;
                else
                    this.SetLevel(channel.Gain, this.GainMinValue, this.GainMaxValue);
            }
        }

        void Socket_SocketConnectionEvent(UXLib.Sockets.SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                this.Channel.Subscribe(SoundWebMixerChannelParamType.Gain);
            }
        }
    }
}