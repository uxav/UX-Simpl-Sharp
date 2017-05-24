using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro;
using UXLib.Devices;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.QSC
{
    /// <summary>
    /// QSys Remote Control API for QSC Core Device
    /// </summary>
    /// <remarks>Tested on CORE 110f</remarks>
    public class QSys : IDevice
    {
        /// <summary>
        /// Contructor for QSys device using IP control
        /// </summary>
        /// <param name="address">The IP Address or HostName</param>
        public QSys(string address)
        {
            var handler = new QSysSocket(address);
            Controls = new QSysControlCollection(this);
            Phones = new QSysPhoneCollection(this);
            handler.CommsStatusChange += HandlerOnCommsStatusChange;
            handler.ReceivedControlResponse += HandlerOnReceivedControlResponse;
            _commsHandler = handler;
        }

        /// <summary>
        /// Contructor for QSys device using serial
        /// </summary>
        /// <param name="comPort">Comport to use for serial</param>
        public QSys(ComPort comPort)
        {
            var handler = new QSysSerialPort(comPort);
            Controls = new QSysControlCollection(this);
            Phones = new QSysPhoneCollection(this);
            handler.CommsStatusChange += HandlerOnCommsStatusChange;
            handler.ReceivedControlResponse += HandlerOnReceivedControlResponse;
            _commsHandler = handler;
        }

        private readonly IQSysCommsHandler _commsHandler;

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

        private void HandlerOnCommsStatusChange(IQSysCommsHandler handler, bool connected)
        {
            _connected = connected;
            if (_connected && HasConnected != null)
                HasConnected(this);
        }

        /// <summary>
        /// Event raised when the device connects
        /// </summary>
        public event QSysConnectedEventHandler HasConnected;

        /// <summary>
        /// Event raised when the system receives data on the socket
        /// </summary>
        public event QSysReceivedDataEventHandler DataReceived;
        
        private void HandlerOnReceivedControlResponse(IQSysCommsHandler handler, byte[] receivedData)
        {
            OnReceive(Encoding.Default.GetString(receivedData, 0, receivedData.Length));
        }

        /// <summary>
        /// The design name of the config
        /// </summary>
        public string DesignName { get; private set; }

        private string _DesignID = "";

        /// <summary>
        /// The unique design ID of the config
        /// </summary>
        public string DesignID
        {
            get { return _DesignID; }
            private set
            {
                if (_DesignID != value)
                {
                    _DesignID = value;
                    ErrorLog.Notice("New QSys Design ID, \"{0}\" - ID: {1}", this.DesignName, this.DesignID);
                    CrestronConsole.PrintLine("New QSys Design ID, \"{0}\" - ID: {1}", this.DesignName, this.DesignID);
                }
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
        }

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;

        public void OnReceive(string receivedString)
        {
#if DEBUG
            CrestronConsole.PrintLine("QSys Rx: {0} ", receivedString);
#endif

            List<string> elements = ElementsFromString(receivedString);

            switch (elements.First())
            {
                case "sr":
                    DesignName = elements[1];
                    DesignID = elements[2];
                    break;
                case "bad_id":
                    ErrorLog.Error("Received bad_id notification from QSys control \"{0}\"", elements[1]);
                    break;
                default:
                    if (DataReceived != null)
                    {
                        var arguments = new List<string>(elements);
                        arguments.RemoveAt(0);
                        DataReceived(this, new QSysReceivedDataEventArgs(elements.First(), arguments, receivedString));
                    }
                    break;
            }
        }

        public void Send(string stringToSend)
        {
            _commsHandler.Send(stringToSend);
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
            ErrorLog.Notice("{0}.Initialize() called", this.GetType().Name);
            _commsHandler.Initialize();
        }

        public CommDeviceType CommunicationType
        {
            get
            {
                if(_commsHandler is QSysSocket)
                    return CommDeviceType.IP;
                return CommDeviceType.Serial;
            }
        }

        public static List<string> ElementsFromString(string str)
        {
            List<string> elements = new List<string>();

            Regex r = new Regex("(['\"])((?:\\\\\\1|.)*?)\\1|([^\\s\"']+)");

            foreach (Match m in r.Matches(str))
            {
                if (m.Groups[1].Length > 0)
                    elements.Add(m.Groups[2].Value);
                else
                    elements.Add(m.Groups[3].Value);
            }

            return elements;
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
        internal QSysReceivedDataEventArgs(string type, List<string> arguments, string dataString)
        {
            ResponseType = type;
            Arguments = arguments;
            DataString = dataString;
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

        /// <summary>
        /// The raw data string received
        /// </summary>
        public string DataString { get; private set; }
    }
}