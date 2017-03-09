using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Polycom
{
    public class SoundstructureSocket : TCPSocketClient
    {
        public SoundstructureSocket(Soundstructure device, string hostAddress)
            : base (hostAddress, 52774, 1000)
        {
            Device = device;
        }

        Soundstructure Device;

        public override void Send(string str)
        {
            str = str + "\x0d";

#if DEBUG
            CrestronConsole.PrintLine("Soundstructure Tx: {0}", str);
#endif
            base.Send(str);
        }

        public override event TCPSocketReceivedDataEventHandler ReceivedData;

        protected override object ReceiveThreadProcess(object o)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.ReceiveThreadProcess() Start", this.GetType().Name);
#endif
            int index = 0;
            byte[] bytes = new Byte[this.BufferSize];

            while (this.ProgramRunning && this.Status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                byte b = ReceiveQueue.Dequeue();

                if (b == 10) { }
                // skip
                else if (b == 13)
                {
                    // Copy bytes to new array with length of packet and ignoring the CR.
                    Byte[] copiedBytes = new Byte[index];
                    Array.Copy(bytes, copiedBytes, index);

                    index = 0;

#if DEBUG
                    CrestronConsole.PrintLine("{0} Processed reply: {1}", this.GetType().Name, Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length));
#endif

                    if (ReceivedData != null)
                        ReceivedData(this, copiedBytes);

                    if (ReceiveQueue.IsEmpty)
                        break;
                }
                else
                {
                    if (index < bytes.Length)
                    {
                        bytes[index] = b;
                        index++;
                    }
                    else
                    {
#if DEBUG
                        CrestronConsole.PrintLine("Buffer overflow, index = {0}, b = {1}", index, b);
#endif
                        ErrorLog.Error("{0}.ReceiveThreadProcess - Buffer overflow error", this.GetType().Name);
                        index = 0;
                        break;
                    }
                }

                CrestronEnvironment.AllowOtherAppsToRun();
                Thread.Sleep(0);
            }
#if DEBUG
            CrestronConsole.PrintLine("{0}.ReceiveThreadProcess() End", this.GetType().Name);
#endif
            return null;
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
            if (this.Connected)
            {
                this.Send(str);
                return true;
            }
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, bool value)
        {
            string str = string.Format("set {0} \"{1}\" {2}", type.ToString().ToLower(),
                channel.Name, value ? 1 : 0);
            if (this.Connected)
            {
                this.Send(str);
                return true;
            }
            return false;
        }

        public bool Set(ISoundstructureItem rowChannel, ISoundstructureItem colChannel, SoundstructureCommandType type, bool value)
        {
            string str = string.Format("set {0} \"{1}\" \"{2}\" {3}", type.ToString().ToLower(),
                rowChannel.Name, colChannel.Name, value ? 1 : 0);
            if (this.Connected)
            {
                this.Send(str);
                return true;
            }
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type, string value)
        {
            string str = string.Format("set {0} \"{1}\" \"{2}\"", type.ToString().ToLower(),
                channel.Name, value);
            if (this.Connected)
            {
                this.Send(str);
                return true;
            }
            return false;
        }

        public bool Set(ISoundstructureItem channel, SoundstructureCommandType type)
        {
            string str = string.Format("set {0} \"{1}\"", type.ToString().ToLower(),
                channel.Name);
            if (this.Connected)
            {
                this.Send(str);
                return true;
            }
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