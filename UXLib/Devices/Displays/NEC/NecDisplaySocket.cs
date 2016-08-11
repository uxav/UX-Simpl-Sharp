using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.NEC
{
    public class NecDisplaySocket : SimpleClientSocket
    {
        public NecDisplaySocket(string address)
            : base(address, 7142, 1000)
        {

        }

        public static byte[] ValueToBytes(int value)
        {
            string str = value.ToString("X2");
            byte[] result = new byte[2];
            result[0] = unchecked((byte)str[0]);
            result[1] = unchecked((byte)str[1]);
            return result;
        }

        public static byte[] CreateHeader(int address, MessageType messageType, int messageLength)
        {
            byte[] result = new byte[7];

            result[0] = 0x01;
            result[1] = 0x30;
            result[2] = (byte)(address + 64);
            result[3] = 0x30;
            result[4] = (byte)messageType;
            Array.Copy(ValueToBytes(messageLength), 0, result, 5, 2);
            return result;
        }

        public Crestron.SimplSharp.CrestronSockets.SocketErrorCodes SendCommand(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            return this.Send(address, MessageType.Command, str);
        }

        public Crestron.SimplSharp.CrestronSockets.SocketErrorCodes SetParameter(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            return this.Send(address, MessageType.SetParameter, str);
        }

        public Crestron.SimplSharp.CrestronSockets.SocketErrorCodes GetParameter(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            return this.Send(address, MessageType.GetParameter, str);
        }

        public Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(int address, MessageType messageType, string message)
        {
            byte[] messageBytes = new byte[message.Length];
            for (int i = 0; i < message.Length; i++)
            {
                messageBytes[i] = unchecked((byte)message[i]);
            }
            return this.Send(address, messageType, messageBytes);
        }

        public Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(int address, MessageType messageType, byte[] message)
        {
#if DEBUG
            //CrestronConsole.Print("NEC Send display {0}, MessageType.{1}, ", address, messageType.ToString());
            //Tools.PrintBytes(message, message.Length);
#endif
            byte[] header = CreateHeader(address, messageType, message.Length);
            byte[] packet = new byte[7 + message.Length];
            Array.Copy(header, packet, header.Length);
            Array.Copy(message, 0, packet, header.Length, message.Length);

            int chk = 0;
            for (int i = 1; i < packet.Length; i++)
            {
                chk = chk ^ packet[i];
            }

            byte[] finalPacket = new byte[packet.Length + 2];
            Array.Copy(packet, finalPacket, packet.Length);
            finalPacket[packet.Length] = (byte)chk;
            finalPacket[packet.Length + 1] = 0x0D;
#if DEBUG
            //CrestronConsole.Print("NEC Tx: ");
            //Tools.PrintBytes(finalPacket, finalPacket.Length);
#endif
            return base.Send(finalPacket);
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

                    // If find byte = CR
                    if (b == 13)
                    {
                        // Copy bytes to new array with length of packet and ignoring the CR.
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        byteIndex = 0;

                        int chk = 0;

                        for (int i = 1; i < (copiedBytes.Length - 1); i++)
                        {
                            chk = chk ^ copiedBytes[i];
                        }

                        if (chk == (int)copiedBytes.Last())
                        {
#if DEBUG
                            //CrestronConsole.Print("NEC Rx: ");
                            //Tools.PrintBytes(copiedBytes, copiedBytes.Length);
#endif
                            OnReceivedPacket(copiedBytes);
                        }
                        else
                        {
#if DEBUG
                            CrestronConsole.PrintLine("NEC Display Rx - Checksum Error");
#endif
                            ErrorLog.Error("NEC Display Rx - Checksum Error");
                        }
                    }
                    else
                    {
                        bytes[byteIndex] = b;
                        byteIndex++;
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

    public enum MessageType : byte
    {
        Command = 0x41,
        CommandReply = 0x42,
        GetParameter = 0x43,
        GetParameterReply = 0x44,
        SetParameter = 0x45,
        SetParameterReply = 0x46
    }
}