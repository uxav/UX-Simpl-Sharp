using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Sockets
{
    public class SimpleClientSocket
    {
        public SimpleClientSocket(string address, int port, int bufferSize)
        {
            Socket = new TCPClient(address, port, bufferSize);
            Socket.SocketSendOrReceiveTimeOutInMs = 0;
            this.BufferSize = bufferSize;
            this.rxQueue = new CrestronQueue<byte>(bufferSize);
            Socket.SocketStatusChange += new TCPClientSocketStatusChangeEventHandler(socket_SocketStatusChange);
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        }

        private TCPClient Socket { get; set; }
        protected CrestronQueue<Byte> rxQueue;
        Thread rxHandler;
        bool shouldReconnect = false;
        public int BufferSize;

        protected virtual string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }
        
        /// <summary>
        /// Get status of socket connected
        /// </summary>
        public bool Connected { get { return (Socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED); } }

        /// <summary>
        /// Connect the socket
        /// </summary>
        public void Connect()
        {
            this.Connect(true);
        }

        /// <summary>
        /// Connect the socket
        /// </summary>
        /// <param name="shouldReconnect">Specify if this should be held open or not</param>
        public void Connect(bool shouldReconnect)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.Connect({1})", this.GetType().Name, shouldReconnect);
            //ErrorLog.Notice("{0}.Connect({1})", this.GetType().Name, shouldReconnect);
#endif
            this.shouldReconnect = shouldReconnect;

            if (Socket.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                SocketErrorCodes error = Socket.ConnectToServerAsync(OnConnect);
#if DEBUG
                CrestronConsole.PrintLine("socket.ConnectToServerAsync(OnConnect) = {0}", error);
#endif
            }
        }

        public void Disconnect()
        {
            shouldReconnect = false;

            if (this.Connected)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.Disconnect(), rxQueue.Count = {1}, rxHandler.ThreadState = {2}",
                    this.GetType().Name, rxQueue.Count, rxHandler.ThreadState);
#endif

                Socket.DisconnectFromServer();

                if (rxQueue.Count == 0 && rxHandler.ThreadState == Thread.eThreadStates.ThreadRunning)
                    rxQueue.Enqueue(0x00);
            }
        }

        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping && this.Connected)
                this.Disconnect();
        }

        public event SimpleClientSocketConnectionEventHandler SocketConnectionEvent;

        protected virtual void OnConnect(TCPClient socket)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.OnConnect(TCPClient socket) socket.ClientStatus = {1}", this.GetType().Name, socket.ClientStatus);
#endif
            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                ErrorLog.Notice("Socket connected to device at {0}", socket.AddressClientConnectedTo);
                rxQueue.Clear();

                if (SocketConnectionEvent != null)
                    SocketConnectionEvent(this, socket.ClientStatus);
                socket.ReceiveDataAsync(OnReceive);
            }
            else if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_NO_CONNECT)
            {
                this.Connect(shouldReconnect);
            }
        }

        void CreateRxHandlerThread()
        {
            if (!RxHandlerThreadRunning)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0} Launching RxHandler Thread ...", this.GetType().Name);
#endif
                rxHandler = new Thread(ReceiveBufferProcess, this.Socket, Thread.eThreadStartOptions.CreateSuspended);
                rxHandler.Priority = Thread.eThreadPriority.UberPriority;
                rxHandler.Name = string.Format("{0} - Rx Handler", this.Name);
                rxHandler.Start();
            }
        }

        bool RxHandlerThreadRunning
        {
            get
            {
                if (rxHandler != null && rxHandler.ThreadState == Thread.eThreadStates.ThreadRunning)
                    return true;
                return false;
            }
        }

        void socket_SocketStatusChange(TCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            CrestronConsole.PrintLine("{0}.SocketStatusChange, device at {1}, {2}", this.GetType().Name, Socket.AddressClientConnectedTo, clientSocketStatus.ToString());

            if(clientSocketStatus == SocketStatus.SOCKET_STATUS_NO_CONNECT)
            {
                if (shouldReconnect)
                {
                    ErrorLog.Warn("Socket disconnected from device at {0}", Socket.AddressClientConnectedTo);
                }
                if (SocketConnectionEvent != null)
                    SocketConnectionEvent(this, Socket.ClientStatus);
                if (shouldReconnect)
                {
                    this.Connect(shouldReconnect);
                }
            }
        }

        protected virtual void OnReceive(TCPClient socket, int byteCount)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0} Socket ({1}) OnReceive() Rx: ", this.GetType().ToString(), socket.ClientStatus);
            Tools.PrintBytes(socket.IncomingDataBuffer, byteCount);
#endif
            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                for (int b = 0; b < byteCount; b++)
                {
                    rxQueue.Enqueue(socket.IncomingDataBuffer[b]);
                }

                if (!RxHandlerThreadRunning)
                    CreateRxHandlerThread();

                socket.ReceiveDataAsync(OnReceive);
            }
        }

        public event SimpleClientSocketReceiveEventHandler ReceivedPacketEvent;

        public void OnReceivedPacket(byte[] bytes)
        {
            try
            {
                // if an event is registered
                if (this.ReceivedPacketEvent != null)
                    this.ReceivedPacketEvent(this, new SimpleClientSocketReceiveEventArgs(bytes));
            }
            catch(Exception e)
            {
                ErrorLog.Error("{0}.OnReceivedPacket() Error: {1}", this.GetType().Name, e.Message);
            }
        }

        /// <summary>
        /// This thread callback will look through the received bytes and look for a delimeter.
        /// The default delimiter is byte value 13 (0x0d - CR). Override this function if you need a different delimiter.
        /// </summary>
        /// <remarks>
        /// This by default will also skip bytes with value 10 (0x0a - LF)
        /// </remarks>
        protected virtual object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[this.BufferSize];
            int byteIndex = 0;

            while (true)
            {
                try
                {
                    byte b = rxQueue.Dequeue();

                    if (((TCPClient)obj).ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
#if DEBUG
                        CrestronConsole.PrintLine("{0}.ReceiveBufferProcess exiting thread, Socket.ClientStatus = {1}",
                            this.GetType().Name, ((TCPClient)obj).ClientStatus);
#endif
                        return null;
                    }

                    // skip any LF chars
                    if (b == 10) { }
                    // If find byte = CR
                    else if (b == 13)
                    {
                        // Copy bytes to new array with length of packet and ignoring the CR.
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        byteIndex = 0;

                        OnReceivedPacket(copiedBytes);

                        CrestronEnvironment.AllowOtherAppsToRun();
                        Thread.Sleep(10);
                        if (rxQueue.IsEmpty)
                            return null;
                    }
                    else
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
#if DEBUG
                    CrestronConsole.PrintLine("{0}.ReceiveBufferProcess {1}", this.GetType().Name, e.Message);
#endif
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("{0} - Error in thread: {1}, byteIndex = {2}", GetType().ToString(), e.Message, byteIndex);
                }
            }
        }

        public virtual SocketErrorCodes Send(string str)
        {
            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = unchecked((byte)str[i]);
            }

            return this.Send(bytes);
        }

        public virtual SocketErrorCodes Send(byte[] bytes)
        {
            if (this.Socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                SocketErrorCodes err = Socket.SendData(bytes, bytes.Length);

                if (err != SocketErrorCodes.SOCKET_OK)
                    ErrorLog.Error("{0} Send to {1} Error: {2}", this.GetType().ToString(), Socket.AddressClientConnectedTo, err.ToString());
                return err;
            }
            return SocketErrorCodes.SOCKET_NOT_CONNECTED;
        }

        public string HostAddress
        {
            get
            {
                return this.Socket.AddressClientConnectedTo;
            }
        }
    }

    public delegate void SimpleClientSocketConnectionEventHandler(SimpleClientSocket socket, SocketStatus status);

    public delegate void SimpleClientSocketReceiveEventHandler(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args);

    public class SimpleClientSocketReceiveEventArgs : EventArgs
    {
        public Byte[] ReceivedPacket;

        public SimpleClientSocketReceiveEventArgs(Byte[] packet)
        {
            ReceivedPacket = packet;
        }
    }
}