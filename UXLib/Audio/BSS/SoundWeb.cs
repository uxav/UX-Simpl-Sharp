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
        public SoundWeb(string ipAddress)
        {
            this.Socket = new SoundWebSocket(ipAddress);
        }

        public SoundWebSocket Socket { get; protected set; }
    }
}