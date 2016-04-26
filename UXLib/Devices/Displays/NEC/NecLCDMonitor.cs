using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib;
using UXLib.Devices;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.NEC
{
    public class NecLCDMonitor : DisplayDevice, ISocketDevice
    {
        public NecLCDMonitor(string name, int displayID, NecDisplaySocket socket)
        {
            this.Name = name;
            this.DisplayID = displayID;
            this.Socket = socket;
            this.Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
            this.Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
        }

        public int DisplayID { get; protected set; }
        public NecDisplaySocket Socket { get; protected set; }

        CTimer pollTimer;

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
#if DEBUG
                CrestronConsole.PrintLine("NEC Display Connected");
#endif
                pollTimer = new CTimer(OnPollEvent, null, 1000, 1000);
            }
            else
            {
#if DEBUG
                CrestronConsole.PrintLine("NEC Display Disconnected");
#endif
                this.pollTimer.Stop();
                this.pollTimer.Dispose();
                this.DeviceCommunicating = false;
            }
        }

        int pollCount = 0;
        void OnPollEvent(object callBackObject)
        {
            pollCount++;

            switch (pollCount)
            {
                case 1:
                    this.Socket.SendCommand(this.DisplayID, "01D6");
                    break;
                case 2:
                    
                    break;
                case 3:
                    
                    pollCount = 0;
                    break;
            }
        }

        bool commsEstablished = false;

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            byte address = args.ReceivedPacket[3];
            if (((int)address - 64) == this.DisplayID)
            {
                this.DeviceCommunicating = true;
                string messageLenString = Encoding.Default.GetString(args.ReceivedPacket, 5, 2);
                int messageLen = Int16.Parse(messageLenString, System.Globalization.NumberStyles.HexNumber);
                byte[] message = new byte[messageLen];
                Array.Copy(args.ReceivedPacket, 7, message, 0, messageLen);
                MessageType type = (MessageType)args.ReceivedPacket[4];
                string messageStr = Encoding.Default.GetString(message, 1, message.Length - 2);
#if DEBUG
                //CrestronConsole.Print("Message Type = MessageType.{0}  ", type.ToString());
                //Tools.PrintBytes(message, message.Length);
                //CrestronConsole.PrintLine("Message = {0}, Length = {1}", messageStr, messageStr.Length);
#endif
                switch (type)
                {
                    case MessageType.CommandReply:
                        
                        switch (messageStr)
                        {
                            case @"0200D60000040001":
                                if (PowerStatus != DevicePowerStatus.PowerCooling)
                                    PowerStatus = DevicePowerStatus.PowerOn;
                                if (!RequestedPower && commsEstablished)
                                    // Send power as should be off
                                    SendPowerCommand(false);
                                else if (!commsEstablished)
                                {
                                    // We have comms and the power is on so update the status
                                    commsEstablished = true;
                                    // set requested power as true as we may not want to turn off once things have come online
                                    RequestedPower = true;
                                }
                                break;
                            case @"0200D60000040004":
                                if (PowerStatus != DevicePowerStatus.PowerWarming)
                                    PowerStatus = DevicePowerStatus.PowerOff;
                                commsEstablished = true;
                                if (RequestedPower)
                                    SendPowerCommand(true);
                                break;
                            case @"00C203D60001":
                                commsEstablished = true;
                                PowerStatus = DevicePowerStatus.PowerWarming;
                                break;
                            case @"00C203D60004":
                                commsEstablished = true;
                                PowerStatus = DevicePowerStatus.PowerCooling;
                                break;
                        }
                        break;
                }
            }
        }

        void SendPowerCommand(bool power)
        {
            if (power)
                this.Socket.SendCommand(this.DisplayID, "C203D60001");
            else
                this.Socket.SendCommand(this.DisplayID, "C203D60004");
            this.Socket.SendCommand(this.DisplayID, "01D6");
        }

        public override bool Power
        {
            get
            {
                return base.Power;
            }
            set
            {
                SendPowerCommand(value);
                base.Power = value;
            }
        }

        #region ISocketDevice Members

        public string HostAddress
        {
            get { return this.Socket.HostAddress; }
        }

        public void Connect()
        {
            this.Socket.Connect();
        }

        public void Disconnect()
        {
            this.Socket.Disconnect();
        }

        public bool Connected
        {
            get { return this.Socket.Connected; }
        }

        #endregion
    }
}