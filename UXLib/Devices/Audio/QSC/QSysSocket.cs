using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    internal class QSysSocket : TCPSocketClient, IQSysCommsHandler
    {
        public QSysSocket(string address)
            : base(address, 1702, 1000)
        { }

        private CTimer _pollTimer;

        protected override void OnConnect(TCPClient client)
        {
            base.OnConnect(client);
            _pollTimer = new CTimer(specific => Send("sg"), null, 1000, 30000);
            if (CommsStatusChange != null)
                CommsStatusChange(this, true);
        }

        protected override void OnDisconnect(TCPClient client)
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }
            base.OnDisconnect(client);
            if (CommsStatusChange != null)
                CommsStatusChange(this, false);
        }

        public override void Send(string str)
        {
            str = str + "\x0a";

            base.Send(str);
        }

        public void Initialize()
        {
            if (!Connected)
                Connect();
        }

        public override event TCPSocketReceivedDataEventHandler ReceivedData;

        protected override object ReceiveThreadProcess(object o)
        {

#if DEBUG
            //CrestronConsole.PrintLine("{0}.ReceiveThreadProcess() Start", this.GetType().Name);
#endif
            int index = 0;
            byte[] bytes = new Byte[this.BufferSize];

            while (this.ProgramRunning && this.Status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                byte b = ReceiveQueue.Dequeue();

                if (b == 13) { }
                // skip
                else if (b == 10)
                {
                    // Copy bytes to new array with length of packet and ignoring the CR.
                    Byte[] copiedBytes = new Byte[index];
                    Array.Copy(bytes, copiedBytes, index);

                    index = 0;

                    if (ReceivedData != null)
                        ReceivedData(this, copiedBytes);

                    if (Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length) != "cgpa")
                    {
                        if (ReceivedControlResponse != null)
                            ReceivedControlResponse(this, copiedBytes);
#if DEBUG
                        CrestronConsole.PrintLine("{0} Processed reply: {1}", this.GetType().Name, Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length));
#endif
                    }

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
            //CrestronConsole.PrintLine("{0}.ReceiveThreadProcess() End", this.GetType().Name);
#endif
            return null;
        }

        #region IQSysCommsHandler Members

        public event IQSysCommsReceiveHandler ReceivedControlResponse;

        public event IQSysCommsStartedHandler CommsStatusChange;

        #endregion
    }
}