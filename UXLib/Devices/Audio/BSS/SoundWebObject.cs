using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebObject
    {
        public SoundWeb Device { get; protected set; }
        public string HiQAddress { get; protected set; }

        bool setPacketCapture = false;

        public virtual void Subscribe()
        {
            if (!setPacketCapture)
            {
                setPacketCapture = true;
                this.Device.Socket.ReceivedPacketEvent += new UXLib.Sockets.SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
            }
        }

        void Socket_ReceivedPacketEvent(UXLib.Sockets.SimpleClientSocket socket, UXLib.Sockets.SimpleClientSocketReceiveEventArgs args)
        {
            string receivedString = Encoding.UTF7.GetString(args.ReceivedPacket, 1, args.ReceivedPacket.Length - 3);
            // string receivedString = new string(args.ReceivedPacket.Select(b => (char)b).ToArray());

            string address = receivedString.Substring(1, 6);

            if (address == HiQAddress)
            {
#if false
                var bytes = new byte[receivedString.Length];

                for (int i = 0; i < receivedString.Length; i++)
                {
                    bytes[i] = unchecked((byte)receivedString[i]);
                }
                
                CrestronConsole.Print("Soundweb Rx (Len={0}): ", receivedString.Length);
                foreach (byte b in bytes)
                {
                    if (b >= 32 && b <= 27)
                        CrestronConsole.Print("{0}", (char)b);
                    else
                        CrestronConsole.Print("\\x{0}", b.ToString("X2"));
                }
                CrestronConsole.PrintLine("");
#endif
                try
                {
                    if (receivedString.Length >= 13)
                    {
                        char[] c = receivedString.Substring(7, 2).ToCharArray();
                        int paramID = c[0] << 8 | c[1];

                        c = receivedString.Substring(9, 4).ToCharArray();
                        int value = c[0] << 24 | c[1] << 16 | c[2] << 8 | c[3];

                        OnReceive(paramID, value);
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    CrestronConsole.PrintLine("Error in Soundweb Rx Event, {0}", e.Message);
#endif
                    ErrorLog.Error("Error in Soundweb Rx Event, {0}", e.Message);
                }
            }
        }

        protected virtual void OnReceive(int paramID, int value)
        {
            if (this.FeedbackReceived != null)
            {
                this.FeedbackReceived(this, new SoundWebObjectFeedbackEventArgs(paramID, value));
            }
        }

        public void Send(string messageType, string paramID, string value)
        {
            string str = messageType + this.HiQAddress + paramID + value;
#if false
            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = unchecked((byte)str[i]);
            }

            CrestronConsole.Print("Soundweb Tx: ");
            foreach (byte b in bytes)
            {
                CrestronConsole.Print("\\x{0}", b.ToString("X2"));
            }
            CrestronConsole.PrintLine("");
#endif
            this.Device.Socket.Send(str);
        }

        public event SoundWebObjectFeedbackEventHandler FeedbackReceived;
    }

    public delegate void SoundWebObjectFeedbackEventHandler(SoundWebObject soundWebObject, SoundWebObjectFeedbackEventArgs args);

    public class SoundWebObjectFeedbackEventArgs : EventArgs
    {
        public SoundWebObjectFeedbackEventArgs(int paramID, int value)
        {
            this.ParamID = paramID;
            this.Value = value;
        }

        public int ParamID { get; private set; }
        public int Value { get; private set; }
    }
}