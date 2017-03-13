using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Sockets
{
    public abstract class TCPSocketClient
    {
        /// <summary>
        /// A TCP Socket Client connection.
        /// </summary>
        /// <param name="address">Address the client should connect to</param>
        /// <param name="port">The remote TCP port number to use</param>
        /// <param name="bufferSize">Size of the buffer</param>
        /// <remarks>The client will automatically deal with program stops and LAN links going down / up.</remarks>
        public TCPSocketClient(string address, int port, int bufferSize)
        {
            ProgramRunning = true;
            _client = new TCPClient(address, port, bufferSize);
            _client.SocketStatusChange += new TCPClientSocketStatusChangeEventHandler(OnSocketStatusChange);
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ProgramStatusEventHandler);
            CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(OnEthernetEvent);
            _sendQueue = new CrestronQueue<byte[]>(50);
            ReceiveQueue = new CrestronQueue<byte>(bufferSize);
        }

        TCPClient _client;
        bool _shouldReconnect = false;
        protected bool ProgramRunning { get; private set; }
        CrestronQueue<byte[]> _sendQueue;
        Thread _sendThread;
        protected CrestronQueue<byte> ReceiveQueue { get; private set; }
        Thread _receiveThread;

        /// <summary>
        /// Connect the client and keep it open until you call disconnect.
        /// </summary>
        public void Connect()
        {
            this.Connect(true);
        }

        /// <summary>
        /// Returns true if client is connected to host
        /// </summary>
        public bool Connected { get { return (Status == SocketStatus.SOCKET_STATUS_CONNECTED); } }

        /// <summary>
        /// Connect the client
        /// </summary>
        /// <param name="shouldReconnect">Set to true to stay connected</param>
        public void Connect(bool shouldReconnect)
        {
            _shouldReconnect = shouldReconnect;
            if (_connectFailCount == 0)
                ErrorLog.Notice("{0}.Connect(shouldReconnect = {1})", this.GetType().Name, shouldReconnect);
            if (Connected)
                ErrorLog.Warn("{0}.Connect() ... allready connected!", this.GetType().Name);
            else
                _client.ConnectToServerAsync(OnConnectResult);
        }

        /// <summary>
        /// Returns true if the client is set to stay connected and retry the connection if it falls over.
        /// </summary>
        public bool ShouldReconnect
        {
            get
            {
                return _shouldReconnect;
            }
        }

        /// <summary>
        /// The size of the buffer set for the incoming data
        /// </summary>
        protected int BufferSize
        {
            get
            {
                return _client.IncomingDataBuffer.Length;
            }
        }

        /// <summary>
        /// Disconnect the client. It will not reconnect until Connect is again called.
        /// </summary>
        public void Disconnect()
        {
            _shouldReconnect = false;
            _sendQueue.Clear();
            _client.DisconnectFromServer();
        }

        SocketStatus _Status;

        /// <summary>
        /// The status of the socket
        /// </summary>
        public SocketStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
#if DEBUG
                    CrestronConsole.PrintLine("{0} Status Event, Status = {1}", this.GetType().Name, value);
#endif
                    if (StatusChanged != null)
                        StatusChanged(this, value);
                }
            }
        }

        /// <summary>
        /// Send a string to the socket
        /// </summary>
        /// <param name="stringToSend">String to be converted to bytes and sent</param>
        public virtual void Send(string stringToSend)
        {
            var bytes = new byte[stringToSend.Length];

            for (int i = 0; i < stringToSend.Length; i++)
            {
                bytes[i] = unchecked((byte)stringToSend[i]);
            }

            this.Send(bytes);
        }

        /// <summary>
        /// Send data to the host
        /// </summary>
        /// <param name="byteArray">A byte array to send. Array must be the size required to send</param>
        public virtual void Send(byte[] byteArray)
        {
            if (this.Connected)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.Send() {1} bytes queued", this.GetType().Name, byteArray.Length);
#endif
                _sendQueue.Enqueue(byteArray);
                if (_sendThread == null || _sendThread.ThreadState == Thread.eThreadStates.ThreadFinished)
                    _sendThread = new Thread(SendThreadProcess, null);
            }
            else
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.Send() - Cannot Enqueue data as socket is not connected!", this.GetType().Name);
                ErrorLog.Warn("{0}.Send() - Socket not connected!", this.GetType().Name);
#endif
            }
        }

        /// <summary>
        /// The host address the client is setup to connect to
        /// </summary>
        public string HostAddress
        {
            get { return _client.AddressClientConnectedTo; }
        }

        object SendThreadProcess(object o)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.SendThreadProcess() Start", this.GetType().Name);
#endif
            while (!_sendQueue.IsEmpty)
            {
                byte[] data = _sendQueue.Dequeue();
                SocketErrorCodes err = SendPacket(_client, data);

#if DEBUG
                CrestronConsole.PrintLine("{0} Send process result - {1}", this.GetType().Name, err);
#endif

                if (err != SocketErrorCodes.SOCKET_OK)
                {
                    ErrorLog.Error("{0}.Send process - Error sending data to socket, {1}", this.GetType().Name, err);
                }

                CrestronEnvironment.AllowOtherAppsToRun();
                Thread.Sleep(0);
            }

#if DEBUG
            CrestronConsole.PrintLine("{0}.SendThreadProcess() End", this.GetType().Name);
#endif

            return null;
        }

        /// <summary>
        /// Called by the thread which manages sending the data. Override this if you want to change or delay the packets being sent.
        /// </summary>
        /// <param name="client">The TCPClient used in the connection</param>
        /// <param name="packet">The packet queued to be sent</param>
        /// <returns>Result of the client send method</returns>
        protected virtual SocketErrorCodes SendPacket(TCPClient client, byte[] packet)
        {
#if DEBUG
            CrestronConsole.Print("{0} Tx: ", this.GetType().Name);
            Tools.PrintBytes(packet, packet.Length);
#endif

            return client.SendData(packet, packet.Length);
        }

        /// <summary>
        /// Called when the socket status changes
        /// </summary>
        public event TCPSocketStatusChangeEventHandler StatusChanged;

        /// <summary>
        /// Called when the receive process thread has processed a reply
        /// </summary>
        public abstract event TCPSocketReceivedDataEventHandler ReceivedData;

        int _connectFailCount = 0;

        void OnConnectResult(TCPClient client)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.OnConnectResult() client.ClientStatus = {1}", this.GetType().Name, client.ClientStatus);
            CrestronConsole.PrintLine("{0} EthernetAdapter = {1}", this.GetType().Name, client.EthernetAdapter);
#endif

            if (client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                ErrorLog.Notice("TCP Client to {0} connected on port {1}", _client.AddressClientConnectedTo, _client.PortNumber);
                _connectFailCount = 0;
                this.Status = SocketStatus.SOCKET_STATUS_CONNECTED;
                this.OnConnect(client);
            }
            else
            {
                _connectFailCount++;

                if (_connectFailCount <= 5)
                {
                    ErrorLog.Warn("TCP Client to {0} could not connect on port {1} (Attempt {2})",
                        _client.AddressClientConnectedTo, _client.PortNumber, _connectFailCount);
                    if (_shouldReconnect)
                        RetryConnection(null);
                }
                else
                {
                    if (_connectFailCount == 6)
                        ErrorLog.Warn("TCP Client to {0} has failed to connect ... will retry every 60 seconds. Logging suppressed",
                            _client.AddressClientConnectedTo);
                    if (_shouldReconnect)
                        _retryTimer = new CTimer(RetryConnection, 60000);
                }
            }
        }

        CTimer _retryTimer;

        void RetryConnection(object o)
        {
            CrestronConsole.PrintLine("Retrying connection to {0} on port {1}", _client.AddressClientConnectedTo, _client.PortNumber);
            this.Connect(_shouldReconnect);
        }

        protected virtual void OnConnect(TCPClient client)
        {
            client.ReceiveDataAsync(OnReceive);
        }

        protected virtual void OnDisconnect(TCPClient client)
        {
            if (this.ProgramRunning)
            {
                ErrorLog.Warn("TCP Client to {0} disconnected!", client.AddressClientConnectedTo);
                _sendQueue.Clear();
                ReceiveQueue.Clear();

                if (_shouldReconnect)
                {
#if DEBUG
                    CrestronConsole.PrintLine("Retrying connection to {0}", _client.AddressClientConnectedTo);
#endif
                    this.Connect(_shouldReconnect);
                }
            }
        }

        protected virtual void OnReceive(TCPClient client, int numberOfBytesReceived)
        {
            if (numberOfBytesReceived <= 0)
            {
                this.Status = SocketStatus.SOCKET_STATUS_NO_CONNECT;
                this.OnDisconnect(client);
            }
            else
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.OnReceive() numberOfBytesReceived = {1}, Enqueuing...", this.GetType().Name, numberOfBytesReceived);
#endif
                for (int b = 0; b < numberOfBytesReceived; b++)
                {
                    ReceiveQueue.Enqueue(client.IncomingDataBuffer[b]);
                    if (_receiveThread == null || _receiveThread.ThreadState == Thread.eThreadStates.ThreadFinished)
                    {
                        _receiveThread = new Thread(ReceiveThreadProcess, null, Thread.eThreadStartOptions.CreateSuspended);
                        _receiveThread.Priority = Thread.eThreadPriority.UberPriority;
                        _receiveThread.Name = string.Format("{0} Rx Handler", this.GetType().Name);
                        _receiveThread.Start();
                    }
                }

                if (client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    client.ReceiveDataAsync(OnReceive);
            }
        }

        protected abstract object ReceiveThreadProcess(object o);

        void OnSocketStatusChange(TCPClient client, SocketStatus clientSocketStatus)
        {
            if (clientSocketStatus == SocketStatus.SOCKET_STATUS_LINK_LOST)
            {
                this.Status = SocketStatus.SOCKET_STATUS_LINK_LOST;
                ErrorLog.Warn("TCP Client to {0} link lost - {1} Link Down!", client.AddressClientConnectedTo, client.EthernetAdapter);
            }
        }

        void ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping && _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                ProgramRunning = false;
#if DEBUG
                CrestronConsole.PrintLine("{0}.ProgramStatusEventHandler - Program Stopping! ... disconnecting client", this.GetType().Name);
#endif
                this.Disconnect();
            }
        }

        void OnEthernetEvent(EthernetEventArgs ethernetEventArgs)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.OnEthernetEvent() EthernetAdapter = {1}, EthernetEventType = {2}", this.GetType().Name, ethernetEventArgs.EthernetAdapter, ethernetEventArgs.EthernetEventType);
#endif
            if (ethernetEventArgs.EthernetAdapter == _client.EthernetAdapter)
            {
                switch (ethernetEventArgs.EthernetEventType)
                {
                    case eEthernetEventType.LinkDown: _client.HandleLinkLoss(); break;
                    case eEthernetEventType.LinkUp: _client.HandleLinkUp(); break;
                }
            }
        }
    }

    public delegate void TCPSocketStatusChangeEventHandler(TCPSocketClient client, SocketStatus status);
    public delegate void TCPSocketReceivedDataEventHandler(TCPSocketClient client, byte[] data);
}