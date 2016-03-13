using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.Samsung
{
    public class SamsungMDCSocket : SimpleClientSocket
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

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(byte[] bytes)
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
#if DEBUG
                CrestronConsole.Print("Samsung Tx: ");
                Tools.PrintBytes(packet, packet.Length);
#endif
                return base.Send(packet);
            }
            else
            {
                throw new FormatException("Packet did not begin with correct value");
            }
        }

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
                bytes[i] = unchecked((byte)str[i]);

            return this.Send(bytes);
        }

        protected override object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[this.BufferSize];
            int byteIndex = 0;
            int dataLength = 0;

            while (true)
            {
                try
                {
                    byte b = rxQueue.Dequeue();

                    if (b == 0xAA)
                    {
                        byteIndex = 0;
                        dataLength = 0;
                    }
                    else
                        byteIndex++;

                    bytes[byteIndex] = b;
                    if (byteIndex == 3)
                        dataLength = bytes[byteIndex];

                    if (byteIndex == (dataLength + 4))
                    {
                        int chk = bytes[byteIndex];

                        int test = 0;
                        for (int i = 1; i < byteIndex; i++)
                            test = test + bytes[i];

                        if (chk == (byte)test)
                        {
                            byte[] copiedBytes = new byte[byteIndex];
                            Array.Copy(bytes, copiedBytes, byteIndex);
                            OnReceivedPacket(copiedBytes);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("{0} - Error in thread: {1}", GetType().ToString(), e.Message);
                }
            }
        }
    }
}