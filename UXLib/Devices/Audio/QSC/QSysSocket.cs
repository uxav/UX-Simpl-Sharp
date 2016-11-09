using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    internal class QSysSocket : SimpleClientSocket
    {
        public QSysSocket(string address)
            : base(address, 1702, 1000)
        {
            this.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(QSysSocket_SocketConnectionEvent);
        }

        private CTimer PollTimer;

        void QSysSocket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                PollTimer = new CTimer(GetStatus, null, 10, 10000);
            }
            else if (PollTimer != null)
            {
                PollTimer.Stop();
                PollTimer.Dispose();
            }
        }

        public static List<string> ElementsFromString(string str)
        {
            List<string> elements = new List<string>();

            Regex r = new Regex("(['\"])((?:\\\\\\1|.)+?)\\1|([^\\s\"']+)");

            foreach (Match m in r.Matches(str))
            {
                int gCount = 0;
                foreach (Group g in m.Groups)
                {
                    if (g.Value.Length > 0 && g.Value != "\"" && gCount > 0)
                    {
                        elements.Add(g.Value);
                        break;
                    }
                    gCount++;
                }
            }

            return elements;
        }

        void GetStatus(object o)
        {
            this.Send("sg");
        }

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
#if DEBUG
            if (str != "sg")
                CrestronConsole.PrintLine("QSys Tx: {0}", str);
#endif

            str = str + "\x0a";

            return base.Send(str);
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

                    // skip any CR chars
                    if (b == 13) { }
                    // If find byte = LF
                    else if (b == 10)
                    {
                        // Copy bytes to new array with length of packet and ignoring the CR.
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        byteIndex = 0;

                        if (Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length) != "cgpa")
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