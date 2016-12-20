using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Shure
{
    public class ShureSocket : SimpleClientSocket
    {
        public ShureSocket(string hostAddress)
            : base(hostAddress, 2202, 1000)
        {

        }

        protected override object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[this.BufferSize];
            int byteIndex = 0;

            while (true)
            {
                try
                {
                    byte b = rxQueue.Dequeue();

                    if (((TCPClient)obj).ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
#if DEBUG
                        CrestronConsole.PrintLine("{0}.ReceiveBufferProcess exiting thread, Socket.ClientStatus = {1}",
                            this.GetType().Name, ((TCPClient)obj).ClientStatus);
#endif
                        return null;
                    }

                    // if find '<' char reset the count
                    if (b == 0x3c)
                    {
                        byteIndex = 0;
                        bytes[byteIndex] = b;
                        byteIndex++;
                    }
                    // if find '>' char we have a string
                    else if (b == 0x3e)
                    {
                        bytes[byteIndex] = b;
                        byteIndex++;

                        // Copy bytes to new array with length of packet and ignoring the CR.
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        byteIndex = 0;

                        OnReceivedPacket(copiedBytes);
                    }
                    else if (b > 0 && b <= 127)
                    {
                        if (byteIndex < this.BufferSize)
                        {
                            bytes[byteIndex] = b;
                            byteIndex++;
                        }
                        else
                        {
                            ErrorLog.Error("{0} - Buffer overflow error", GetType().ToString());

                            string lastBytes = string.Empty;
                            for (int bt = this.BufferSize - 51; bt < this.BufferSize - 1; bt++)
                            {
                                byte btVal = bytes[bt];
                                if (btVal > 32 && btVal <= 127)
                                    lastBytes = lastBytes + (char)btVal;
                                else
                                    lastBytes = lastBytes + @"\x" + btVal.ToString("X2");
                            }

                            ErrorLog.Notice("Last 50 bytes of buffer: \"{0}\"", lastBytes);
                            ErrorLog.Warn("The buffer was cleared as a result");
                            byteIndex = 0;
                        }
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