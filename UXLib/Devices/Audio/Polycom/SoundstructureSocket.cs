using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Polycom
{
    public class SoundstructureSocket : SimpleClientSocket
    {
        public SoundstructureSocket(Soundstructure device, string hostAddress)
            : base (hostAddress, 52774, 1000)
        {
            Device = device;
        }

        Soundstructure Device;

        protected override string Name
        {
            get
            {
                return "Soundstructure Socket Handler";
            }
        }

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
            str = str + "\x0d";

#if DEBUG
            CrestronConsole.PrintLine("Soundstructure Tx: {0}", str);
#endif
            return base.Send(str);
        }

        public static List<string> ElementsFromString(string str)
        {
            List<string> elements = new List<string>();

            Regex r = new Regex("(['\"])((?:\\\\\\1|.)*?)\\1|([^\\s\"']+)");

            foreach (Match m in r.Matches(str))
            {
                if (m.Groups[1].Length > 0)
                    elements.Add(m.Groups[2].Value);
                else
                    elements.Add(m.Groups[3].Value);
            }

            return elements;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, double value)
        {
            string str = string.Format("set {0} \"{1}\" {2:0.00}", type.ToString().ToLower(),
                channel.Name, value);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, bool value)
        {
            string str = string.Format("set {0} \"{1}\" {2}", type.ToString().ToLower(),
                channel.Name, value ? 1 : 0);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public bool Set(ISoundstructureItem rowChannel, ISoundstructureItem colChannel, SoundstructureCommandType type, bool value)
        {
            string str = string.Format("set {0} \"{1}\" \"{2}\" {3}", type.ToString().ToLower(),
                rowChannel.Name, colChannel.Name, value ? 1 : 0);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, string value)
        {
            string str = string.Format("set {0} \"{1}\" \"{2}\"", type.ToString().ToLower(),
                channel.Name, value);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type)
        {
            string str = string.Format("set {0} \"{1}\"", type.ToString().ToLower(),
                channel.Name);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public void Get(ISoundstructureItem channel, SoundstructureCommandType type)
        {
            string str = string.Format("get {0} \"{1}\"", type.ToString().ToLower(), channel.Name);
            this.Send(str);

            if (type == SoundstructureCommandType.FADER)
            {
                str = string.Format("get fader min \"{0}\"", channel.Name);
                this.Send(str);
                
                str = string.Format("get fader max \"{0}\"", channel.Name);
                this.Send(str);
            }
        }
    }
}