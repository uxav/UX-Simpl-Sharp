using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Shure
{
    public class ShureSocket : TCPSocketClient
    {
        public ShureSocket(string hostAddress)
            : base(hostAddress, 2202, 1000)
        {

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

                // if find '<' char reset the count
                if (b == 0x3c)
                {
                    index = 0;
                    bytes[index] = b;
                    index++;
                }
                // if find '>' char we have a string
                else if (b == 0x3e)
                {
                    bytes[index] = b;
                    index++;

                    // Copy bytes to new array with length of packet and ignoring the CR.
                    Byte[] copiedBytes = new Byte[index];
                    Array.Copy(bytes, copiedBytes, index);

                    index = 0;

                    if (ReceivedData != null)
                        ReceivedData(this, copiedBytes);

                    if (ReceiveQueue.IsEmpty)
                        break;
                }
                else if (b > 0 && b <= 127)
                {
                    if (index < this.BufferSize)
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
    }
}