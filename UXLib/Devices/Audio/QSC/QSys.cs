using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    /// <summary>
    /// QSys Remote Control API for QSC Core Device
    /// </summary>
    /// <remarks>Tested on CORE 110f</remarks>
    public class QSys : ISocketDevice, IDevice
    {
        /// <summary>
        /// Contructor for QSys device
        /// </summary>
        /// <param name="address">The IP Address or HostName</param>
        public QSys(string address)
        {
            Socket = new QSysSocket(address);
            Controls = new QSysControlCollection(this);
            Phones = new QSysPhoneCollection(this);
            Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
        }

        QSysSocket Socket { get; set; }

        /// <summary>
        /// Collection of QSysControls
        /// </summary>
        /// <remarks>You need to register controls by using the Register method</remarks>
        public QSysControlCollection Controls { get; internal set; }

        /// <summary>
        /// Collection of QSysSoftPhones
        /// </summary>
        /// <remarks>You need to register phones by using the Register method</remarks>
        public QSysPhoneCollection Phones { get; internal set; }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED && HasConnected != null)
                HasConnected(this);
#if DEBUG
            CrestronConsole.PrintLine("QSys.SocketStatus = {0}", status.ToString());
#endif
        }

        /// <summary>
        /// Event raised when the device connects
        /// </summary>
        public event QSysConnectedEventHandler HasConnected;

        /// <summary>
        /// Event raised when the system receives data on the socket
        /// </summary>
        public event QSysReceivedDataEventHandler DataReceived;

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            this.OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        /// <summary>
        /// The design name of the config
        /// </summary>
        public string DesignName { get; private set; }

        /// <summary>
        /// The unique design ID of the config
        /// </summary>
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
            if (!receivedString.StartsWith("sr ") && !receivedString.Contains("cgpa"))
                CrestronConsole.PrintLine("QSys Rx: {0} ", receivedString);
#endif

            List<string> elements = QSysSocket.ElementsFromString(receivedString);

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
            Crestron.SimplSharp.CrestronSockets.SocketErrorCodes result = Socket.Send(stringToSend);
            if (result != Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                ErrorLog.Error("Could not send command \"{0}\" result = {1}", stringToSend, result.ToString());
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

        public void Initialize()
        {
            if (!this.Connected)
                this.Connect();
        }

        public CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }
    }

    /// <summary>
    /// The Event handler for when a device came online
    /// </summary>
    /// <param name="device">The QSys device which came online</param>
    public delegate void QSysConnectedEventHandler(QSys device);

    /// <summary>
    /// The Event handler for incoming data
    /// </summary>
    /// <param name="device">The QSys device</param>
    /// <param name="args">The QSysReceivedDataEventArgs containing the data</param>
    public delegate void QSysReceivedDataEventHandler(QSys device, QSysReceivedDataEventArgs args);

    /// <summary>
    /// Args for QSysReceivedDataEventHandler
    /// </summary>
    public class QSysReceivedDataEventArgs : EventArgs
    {
        internal QSysReceivedDataEventArgs(string type, List<string> arguments)
        {
            ResponseType = type;
            Arguments = arguments;
        }

        /// <summary>
        /// Response type of the data
        /// </summary>
        /// <example>cv is control value</example>
        public string ResponseType { get; private set; }

        /// <summary>
        /// A list of the arguments received in string format
        /// </summary>
        /// <remarks>These have been processed to remove quotes on string values. Numeric values will need parsing</remarks>
        public List<string> Arguments { get; private set; }
    }
}