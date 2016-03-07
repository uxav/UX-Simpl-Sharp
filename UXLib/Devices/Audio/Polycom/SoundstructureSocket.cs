using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
            str = str + "\x0d";

            //CrestronConsole.PrintLine("Soundstructure Tx: {0}", str);
            return base.Send(str);
        }

        public static List<string> ElementsFromString(string str)
        {
            string[] parts = str.Split(' ');
            List<string> elements = new List<string>();

            bool isStringValue = false;
            foreach (string word in parts)
            {
                if (word[0] == '\x22')
                {
                    if (word[word.Length - 1] == '\x22')
                        elements.Add(word.Substring(1, word.Length - 2));
                    else
                    {
                        isStringValue = true;
                        elements.Add(word.Substring(1, word.Length - 1));
                    }
                }
                else if (word[word.Length - 1] == '\x22' && isStringValue)
                {
                    elements[elements.Count() - 1] = elements.Last() + ' ' + word.Substring(0, word.Length - 1);
                    isStringValue = false;
                }
                else if (isStringValue)
                {
                    elements[elements.Count() - 1] = elements.Last() + ' ' + word;
                }
                else
                {
                    elements.Add(word);
                }
            }

            return elements;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, double value)
        {
            string str = string.Format("set {0} \x22{1}\x22 {2:N2}", type.ToString().ToLower(),
                channel.Name, value);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, bool value)
        {
            int b;
            if (value) b = 1;
            else b = 0;
            string str = string.Format("set {0} \x22{1}\x22 {2}", type.ToString().ToLower(),
                channel.Name, b);
            if (this.Send(str) == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                return true;
            return false;
        }

        public void Get(ISoundstructureItem channel, SoundstructureCommandType type)
        {
            string str = string.Format("get {0} \x22{1}\x22", type.ToString().ToLower(),
                channel.Name);
            this.Send(str);
        }
    }
}