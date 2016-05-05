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
            socket = new TCPClient(address, port, bufferSize);
            this.BufferSize = bufferSize;
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        }

        TCPClient socket;
        protected CrestronQueue<Byte> rxQueue = new CrestronQueue<byte>();
        Thread rxHandler;
        bool shouldReconnect = false;
        public int BufferSize;
        
        /// <summary>
        /// Get status of socket connected
        /// </summary>
        public bool Connected
        {
            get
            {
                if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                    return true;
                return false;
            }
        }

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
            this.shouldReconnect = shouldReconnect;
            if (socket.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                SocketErrorCodes error = socket.ConnectToServerAsync(OnConnect);
                if (error != SocketErrorCodes.SOCKET_OPERATION_PENDING
                    && error != SocketErrorCodes.SOCKET_OK)
                {
                    ErrorLog.Error("Error in socket.ConnectToServerAsync(), {0}", error.ToString());
                }
            }
            else
            {
                ErrorLog.Notice("Socket {0} already connected!", socket.AddressClientConnectedTo);
            }
        }

        public void Disconnect()
        {
            shouldReconnect = false;

            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                socket.DisconnectFromServer();
        }

        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping && this.Connected)
                this.Disconnect();
        }

        public event SimpleClientSocketConnectionEventHandler SocketConnectionEvent;

        protected virtual void OnConnect(TCPClient socket)
        {
            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                tryCount = 0;
#if DEBUG
                CrestronConsole.PrintLine("Socket connected to device at {0}", socket.AddressClientConnectedTo);
#endif
                ErrorLog.Notice("Socket connected to device at {0}", socket.AddressClientConnectedTo);
                rxQueue.Clear();
                if (rxHandler == null || rxHandler.ThreadState != Thread.eThreadStates.ThreadRunning)
                    rxHandler = new Thread(ReceiveBufferProcess, null, Thread.eThreadStartOptions.Running);
                rxHandler.Priority = Thread.eThreadPriority.LowestPriority;
                if (SocketConnectionEvent != null)
                    SocketConnectionEvent(this, socket.ClientStatus);
                socket.ReceiveDataAsync(OnReceive);
            }
            else
            {
                new CTimer(TryAgainConnect, 2000);
            }
        }

        uint tryCount = 0;

        void TryAgainConnect(object obj)
        {
            tryCount++;
            if (tryCount == 10)
            {
                ErrorLog.Error("{0}, socket connection to {1} has failed to connect", this.GetType().ToString(), socket.AddressClientConnectedTo);
            }
            this.Connect(shouldReconnect);
        }

        void OnReceive(TCPClient socket, int byteCount)
        {
#if DEBUG
            //CrestronConsole.PrintLine("{0} Socket OnReceive() Rx: ", this.GetType().ToString());
            //Tools.PrintBytes(socket.IncomingDataBuffer, byteCount);
#endif
            for (int b = 0; b < byteCount; b++)
            {
                rxQueue.Enqueue(socket.IncomingDataBuffer[b]);
            }

            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                socket.ReceiveDataAsync(OnReceive);
            else if (byteCount == 0)
            {
                if (SocketConnectionEvent != null)
                    SocketConnectionEvent(this, socket.ClientStatus);
                if (rxHandler.ThreadState == Thread.eThreadStates.ThreadRunning)
                    rxHandler.Abort();
                if (shouldReconnect)
                {
                    ErrorLog.Notice("Socket dropped at {0}, {1}, waiting for 1 second..", socket.AddressClientConnectedTo, socket.ClientStatus.ToString());
                    new CTimer(TryAgainConnect, 1000);
                }
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
            catch
            {
                ErrorLog.Error("{0} - Error in OnReceivedPacket()", GetType().ToString());
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
            SocketErrorCodes err = socket.SendData(bytes, bytes.Length);

            if (err != SocketErrorCodes.SOCKET_OK)
                ErrorLog.Error("{0} Send to {1} Error: {2}", this.GetType().ToString(), socket.AddressClientConnectedTo, err.ToString());

            return err;
        }

        public string HostAddress
        {
            get
            {
                return this.socket.AddressClientConnectedTo;
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