using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebSocket : SimpleClientSocket
    {
        public SoundWebSocket(string address)
            : base (address, 1023, 1000)
        {
            
        }

        public override SocketErrorCodes Send(string str)
        {
            str = GetChecksum(str);
            str = "\x02" + str + "\x03";

            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = unchecked((byte)str[i]);
            }
            
            return base.Send(bytes);
        }

        public SocketErrorCodes Send(string messageType, string hiQAddress, string paramID, string value)
        {
            string str = messageType + hiQAddress + paramID + value;
            return this.Send(str);
        }

        public SocketErrorCodes Send(string messageType, string nodeID, string virtualDeviceID, string objectID, string paramID, string value)
        {
            string str = messageType + nodeID + virtualDeviceID + objectID + paramID + value;
            return this.Send(str);
        }

        private static string GetChecksum(string s)
        {
            int chk = 0;
            string result = "";

            var bytes = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                bytes[i] = unchecked((byte)s[i]);
            }

            foreach (byte b in bytes)
            {
                chk = chk ^ b;

                if (b == 2 || b == 3 || b == 6 || b == 21 || b == 27)
                {
                    result = result + "\x1b" + Convert.ToChar(b + 128);
                }
                else
                {
                    result = result + Convert.ToChar(b);
                }
            }

            if (chk == 2 || chk == 3 || chk == 6 || chk == 21 || chk == 27)
            {
                result = result + "\x1b" + Convert.ToChar(chk + 128);
            }
            else
            {
                result = result + Convert.ToChar(chk);
            }

            return result;
        }

        protected override object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[50];
            int byteIndex = 0;

            while (true)
            {
                try
                {
                    Byte b = rxQueue.Dequeue();

                    if (((TCPClient)obj).ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
#if DEBUG
                        CrestronConsole.PrintLine("{0}.ReceiveBufferProcess exiting thread, Socket.ClientStatus = {1}",
                            this.GetType().Name, ((TCPClient)obj).ClientStatus);
#endif
                        return null;
                    }

                    if (b == 2)
                    {
                        byteIndex = 0;
                        bytes[byteIndex] = b;
                    }
                    else if (b == 3)
                    {
                        byteIndex++;
                        bytes[byteIndex] = b;
                        
                        Byte[] processedBytes = new Byte[50];
                        int newIndex = 0;
                        for (int i = 0; i <= byteIndex; i++)
                        {
                            if (bytes[i] == 27)
                            {
                                i++;
                                int value = bytes[i];
                                value = value - 128;
                                processedBytes[newIndex] = (byte)value;
                            }
                            else
                            {
                                processedBytes[newIndex] = bytes[i];
                            }
                            newIndex++;
                        }
                        
                        Byte[] copiedBytes = new Byte[newIndex];
                        Array.Copy(processedBytes, copiedBytes, newIndex);

                        byteIndex = 0;
                        OnReceivedPacket(copiedBytes);

                        CrestronEnvironment.AllowOtherAppsToRun();
                    }
                    else
                    {
                        byteIndex++;
                        bytes[byteIndex] = b;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("{0} - Error in thread: {1}, byteIndex = {2}", GetType().ToString(), e.Message, byteIndex);
                }
            }
        }
    }
}