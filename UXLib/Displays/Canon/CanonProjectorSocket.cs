using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UXLib.Sockets;

namespace UXLib.Displays.Canon
{
    public class CanonProjectorSocket : SimpleClientSocket
    {
        public CanonProjectorSocket(string address)
            : base(address, 33336, 1000)
        {

        }

        public override SocketErrorCodes Send(string str)
        {
            str = str + "\x0d";

            return base.Send(str);
        }
    }
}