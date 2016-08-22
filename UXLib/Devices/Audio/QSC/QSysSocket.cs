using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysSocket : SimpleClientSocket
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
            else
            {
                PollTimer.Stop();
                PollTimer.Dispose();
            }
        }

        public static List<string> ElementsFromString(string str)
        {
            string[] parts = str.Split(' ');
            List<string> elements = new List<string>();

            bool isStringValue = false;
            foreach (string word in parts)
            {
                if (word[0] == '\"')
                {
                    if (word[word.Length - 1] == '\"')
                        elements.Add(word.Substring(1, word.Length - 2));
                    else
                    {
                        isStringValue = true;
                        elements.Add(word.Substring(1, word.Length - 1));
                    }
                }
                else if (word[word.Length - 1] == '\"' && isStringValue)
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

        void GetStatus(object o)
        {
            this.Send("sg");
        }

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
#if DEBUG
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