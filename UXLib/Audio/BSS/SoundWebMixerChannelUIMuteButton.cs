using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UXLib.UI;

namespace UXLib.Audio.BSS
{
    public class SoundWebMixerChannelUIMuteButton : UIButton
    {
        public SoundWebMixerChannelUIMuteButton(BoolOutputSig pressDigitalJoin, BoolInputSig feedbackDigitalJoin, SoundWebMixerChannel channel)
            : base(pressDigitalJoin, feedbackDigitalJoin)
        {
            this.Channel = channel;
            this.Channel.ChangeEvent += new SoundWebMixerChannelEventHandler(Channel_ChangeEvent);
            this.Channel.Mixer.Device.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            if (this.Channel.Mixer.Device.Socket.Connected)
                Channel.Subscribe(SoundWebMixerChannelParamType.Mute);
        }

        public SoundWebMixerChannel Channel { get; protected set; }

        void Channel_ChangeEvent(SoundWebMixerChannel channel, SoundWebMixerChannelEventArgs args)
        {
            if (args.EventType == SoundWebMixerChannelEventType.MuteChange)
            {
                this.Feedback = channel.Mute;
            }
        }

        void Socket_SocketConnectionEvent(UXLib.Sockets.SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                Channel.Subscribe(SoundWebMixerChannelParamType.Mute);
            }
        }

        protected override void OnRelease()
        {
            this.Channel.Mute = !this.Channel.Mute;
            base.OnRelease();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Channel.ChangeEvent -= new SoundWebMixerChannelEventHandler(Channel_ChangeEvent);
                this.Channel.Mixer.Device.Socket.SocketConnectionEvent -= new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            }
            base.Dispose(disposing);
        }
    }
}