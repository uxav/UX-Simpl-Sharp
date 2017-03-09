using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.Samsung
{
    public class SamsungMDCSocket : TCPSocketClient
    {
        public SamsungMDCSocket(string address)
            : base(address, 1515, 1000)
        {
        }

        public static byte[] BuildCommand(CommandType command, int id, byte[] data)
        {
            byte[] result = new byte[data.Length + 4];

            result[0] = 0xaa;
            result[1] = (byte)command;
            result[2] = (byte)id;
            result[3] = (byte)data.Length;

            for (int i = 4; i < data.Length + 4; i++)
                result[i] = data[i - 4];

            return result;
        }

        public static byte[] BuildCommand(CommandType command, int id)
        {
            byte[] result = new byte[4];

            result[0] = 0xaa;
            result[1] = (byte)command;
            result[2] = (byte)id;
            result[3] = 0x00;

            return result;
        }

        public override void Send(byte[] bytes)
        {
            // Packet must start with correct header
            if (bytes[0] == 0xAA)
            {
                int dLen = bytes[3];
                byte[] packet = new byte[dLen + 5];
                Array.Copy(bytes, packet, bytes.Length);
                int chk = 0;
                for (int i = 1; i < bytes.Length; i++)
                    chk = chk + bytes[i];
                packet[packet.Length - 1] = (byte)chk;
                base.Send(packet);
            }
            else
            {
                throw new FormatException("Packet did not begin with correct value");
            }
        }

        public override void Send(string str)
        {
            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
                bytes[i] = unchecked((byte)str[i]);

            this.Send(bytes);
        }

        protected override SocketErrorCodes SendPacket(TCPClient client, byte[] packet)
        {
            Thread.Sleep(100);
            return base.SendPacket(client, packet);
        }

        public override event TCPSocketReceivedDataEventHandler ReceivedData;

        protected override object ReceiveThreadProcess(object o)
        {

#if DEBUG
            CrestronConsole.PrintLine("{0}.ReceiveThreadProcess() Start", this.GetType().Name);
#endif
            int index = 0;
            byte[] bytes = new Byte[this.BufferSize];
            int dataLength = 0;

            while (this.ProgramRunning && this.Status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                try
                {
                    byte b = ReceiveQueue.Dequeue();

                    if (b == 0xAA)
                    {
                        index = 0;
                        dataLength = 0;
                    }
                    else if (index < this.BufferSize)
                        index++;
                    else
                    {
#if DEBUG
                        CrestronConsole.PrintLine("Buffer overflow, index = {0}, b = {1}", index, b);
#endif
                        ErrorLog.Error("{0}.ReceiveThreadProcess - Buffer overflow error", this.GetType().Name);
                        index = 0;
                        break;
                    }

                    bytes[index] = b;
                    if (index == 3)
                        dataLength = bytes[index];

                    if (index == (dataLength + 4))
                    {
                        int chk = bytes[index];

                        int test = 0;
                        for (int i = 1; i < index; i++)
                            test = test + bytes[i];

                        if (chk == (byte)test)
                        {
                            byte[] copiedBytes = new byte[index];
                            Array.Copy(bytes, copiedBytes, index);
                            if (ReceivedData != null)
                                ReceivedData(this, copiedBytes);
                            if (ReceiveQueue.IsEmpty)
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Exception(string.Format("{0} - Exception in thread", this.GetType().Name), e);
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