using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UXLib.UI;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebChannelUIMuteButton : UIButton
    {
        public SoundWebChannelUIMuteButton(BoolOutputSig pressDigitalJoin, BoolInputSig feedbackDigitalJoin, SoundWebChannel channel)
            : base(pressDigitalJoin, feedbackDigitalJoin)
        {
            this.Channel = channel;
            this.Channel.ChangeEvent += new SoundWebChannelEventHandler(Channel_ChangeEvent);
            this.Channel.Owner.Device.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            if (this.Channel.Owner.Device.Socket.Connected)
                Channel.Subscribe(SoundWebChannelParamType.Mute);
        }

        public SoundWebChannelUIMuteButton(BoolOutputSig pressDigitalJoin, BoolInputSig feedbackDigitalJoin,
            BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin, SoundWebChannel channel)
            : base(pressDigitalJoin, feedbackDigitalJoin, enableDigitalJoin, visibleDigitalJoin)
        {
            this.Channel = channel;
            this.Channel.ChangeEvent += new SoundWebChannelEventHandler(Channel_ChangeEvent);
            this.Channel.Owner.Device.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            if (this.Channel.Owner.Device.Socket.Connected)
                Channel.Subscribe(SoundWebChannelParamType.Mute);
        }

        public SoundWebChannel Channel { get; protected set; }

        void Channel_ChangeEvent(SoundWebChannel channel, SoundWebChannelEventArgs args)
        {
            if (args.EventType == SoundWebChannelEventType.MuteChange)
            {
                this.Feedback = channel.Mute;
            }
        }

        void Socket_SocketConnectionEvent(UXLib.Sockets.SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                Channel.Subscribe(SoundWebChannelParamType.Mute);
            }
        }

        protected override void OnPress()
        {
            this.Channel.Mute = !this.Channel.Mute;
            base.OnRelease();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Channel.ChangeEvent -= new SoundWebChannelEventHandler(Channel_ChangeEvent);
                this.Channel.Owner.Device.Socket.SocketConnectionEvent -= new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Call to self subscribe to sig events on smart object or device
        /// <remarks>if adding smart object do this before calling this method</remarks>
        /// </summary>
        public new void SubscribeToSigChanges()
        {
            base.SubscribeToSigChanges();
        }
    }
}