using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace UXLib.Audio.BSS
{
    public class SoundWeb
    {
        public SoundWebSocket Socket { get; private set; }
        public SoundWebMatrix SkypeMicMatrix { get; private set; }
        public SoundWebMatrix SkypeDisplayMatrix { get; private set; }
        public SoundWebMatrix SkypeHeadsetMatrix { get; private set; }
        public SoundWebMixer LoftMixer { get; private set; }

        public SoundWeb(string ipAddress)
        {
            this.Socket = new SoundWebSocket(ipAddress);
            this.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            this.SkypeMicMatrix = new SoundWebMatrix(this, 24, 72, "\x96\x7E\x03\x00\x01\x1D");
            this.SkypeDisplayMatrix = new SoundWebMatrix(this, 6, 16, "\x00\x01\x03\x00\x01\x0C");
            this.SkypeHeadsetMatrix = new SoundWebMatrix(this, 6, 16, "\x00\x01\x03\x00\x01\x08");
            this.LoftMixer = new SoundWebMixer(this, 8, "\x00\x01\x03\x00\x01\x1B");
        }

        void Socket_SocketConnectionEvent(UXLib.Sockets.SimpleClientSocket socket, SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                //this.SkypeMicMatrix.SubScribe();
                //this.SkypeDisplayMatrix.SubScribe();
                //this.SkypeHeadsetMatrix.SubScribe();
                this.LoftMixer.Subscribe(5);

                for (uint output = 1; output <= SkypeMicMatrix.OutputCount; output++)
                {
                    SkypeMicMatrix[output] = 0;
                }

                for (uint output = 1; output <= SkypeDisplayMatrix.OutputCount; output++)
                {
                    SkypeDisplayMatrix[output] = 0;
                }

                for (uint output = 1; output <= SkypeHeadsetMatrix.OutputCount; output++)
                {
                    SkypeHeadsetMatrix[output] = 0;
                }
            }
        }
    }
}