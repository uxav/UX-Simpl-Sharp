using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    public class QSys : ISocketDevice, IDevice
    {
        public QSys(string address)
        {
            Socket = new QSysSocket(address);
            Controls = new QSysControlCollection(this);
            Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
        }

        QSysSocket Socket { get; set; }
        public QSysControlCollection Controls { get; internal set; }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED && HasConnected != null)
                HasConnected(this);
#if DEBUG
            CrestronConsole.PrintLine("QSys.SocketStatus = {0}", status.ToString());
#endif
        }

        public event QSysConnectedEventHandler HasConnected;
        public event QSysReceivedDataEventHandler DataReceived;

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            this.OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        public string DesignName { get; private set; }
        public string DesignID { get; private set; }

        #region ISocketDevice Members

        public void Connect()
        {
            Socket.Connect();
        }

        public bool Connected
        {
            get { return Socket.Connected; }
        }

        public void Disconnect()
        {
            Socket.Disconnect();
        }

        public string HostAddress
        {
            get { return Socket.HostAddress; }
        }

        #endregion

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public void OnReceive(string receivedString)
        {
#if DEBUG
            CrestronConsole.PrintLine("QSys Rx: {0} ", receivedString);
#endif

            List<string> elements = QSysSocket.ElementsFromString(receivedString);

#if DEBUG
            CrestronConsole.Print("  Elements:");
            foreach (string e in elements)
            {
                CrestronConsole.Print(" {0}", e);
            }
            CrestronConsole.PrintLine("  -Count = {0}", elements.Count);
#endif

            if (elements.First() == "sr")
            {
                this.DesignName = elements[1];
                this.DesignID = elements[2];
            }
            else if (DataReceived != null)
            {
                List<string> arguments = new List<string>(elements);
                arguments.RemoveAt(0);
                DataReceived(this, new QSysReceivedDataEventArgs(elements.First(), arguments));
            }
        }

        public void Send(string stringToSend)
        {
            Socket.Send(stringToSend);
        }

        #endregion

        #region IDevice Members

        public string DeviceManufacturer
        {
            get { return "QSC"; }
        }

        public string DeviceModel
        {
            get { throw new NotImplementedException(); }
        }

        public string DeviceSerialNumber
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                return this.DesignName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    public delegate void QSysConnectedEventHandler(QSys device);

    public delegate void QSysReceivedDataEventHandler(QSys device, QSysReceivedDataEventArgs args);

    public class QSysReceivedDataEventArgs : EventArgs
    {
        public QSysReceivedDataEventArgs(string type, List<string> arguments)
        {
            ResponseType = type;
            Arguments = arguments;
        }

        public string ResponseType { get; private set; }
        public List<string> Arguments { get; private set; }
    }
}