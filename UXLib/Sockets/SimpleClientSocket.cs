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
        }

        TCPClient socket;
        CrestronQueue<Byte> rxQueue = new CrestronQueue<byte>();
        Thread rxHandler;
        bool shouldReconnect = false;
        public int BufferSize;

        public void Connect()
        {
            if (socket.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                socket.ConnectToServerAsync(OnConnect);
            }
            else
            {
                ErrorLog.Notice("Socket {0} already connected!", socket.AddressClientConnectedTo);
            }
        }

        public void Connect(bool shouldReconnect)
        {
            this.shouldReconnect = shouldReconnect;
            this.Connect();
        }

        public void Disconnect()
        {
            this.shouldReconnect = false;

            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                socket.DisconnectFromServer();
            if (rxHandler.ThreadState == Thread.eThreadStates.ThreadRunning)
                rxHandler.Abort();
        }

        public event SimpleClientSocketConnectionEventHandler SocketConnectionEvent;

        protected virtual void OnConnect(TCPClient socket)
        {
            if (socket.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                rxQueue.Clear();
                if (rxHandler != null && rxHandler.ThreadState != Thread.eThreadStates.ThreadRunning)
                    rxHandler = new Thread(ReceiveBufferProcess, null, Thread.eThreadStartOptions.Running);
                if (SocketConnectionEvent != null)
                    SocketConnectionEvent(this, socket.ClientStatus);
                socket.ReceiveDataAsync(OnReceive);
            }
            else
            {
                ErrorLog.Error("Error connecting to socket at {0}, ClientStatus.{1}",
                    socket.AddressClientConnectedTo, socket.ClientStatus.ToString());
                this.Connect();
            }
        }

        void OnReceive(TCPClient socket, int byteCount)
        {
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
                if (shouldReconnect)
                    this.Connect();
            }
        }

        public event SimpleClientSocketReceiveEventHandler ReceivedPacketEvent;

        protected virtual object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[this.BufferSize];
            int byteIndex = 0;

            while (true)
            {
                try
                {
                    byte b = rxQueue.Dequeue();

                    if (b == 13)
                    {
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        if (this.ReceivedPacketEvent != null)
                        {
                            this.ReceivedPacketEvent(this, new SimpleClientSocketReceiveEventArgs(copiedBytes));
                        }

                        byteIndex = 0;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("Error in thread: {0}", e.Message);
                }
            }
        }

        public void Send(string str)
        {
            var bytes = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = unchecked((byte)str[i]);
            }

            this.Send(bytes);
        }

        public void Send(byte[] bytes)
        {
            SocketErrorCodes err = socket.SendData(bytes, bytes.Length);

            if (err != SocketErrorCodes.SOCKET_OK)
                ErrorLog.Error("SimpleSocketClient at {0} Send Error: {1}", socket.AddressClientConnectedTo, err.ToString());
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