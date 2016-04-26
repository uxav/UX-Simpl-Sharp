using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib;
using UXLib.Devices;
using UXLib.Sockets;
using UXLib.Models;

namespace UXLib.Devices.Displays.NEC
{
    public class NecLCDMonitor : DisplayDevice, ISocketDevice, IVolumeDevice
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
                    this.Socket.SendCommand(this.DisplayID, @"01D6");
                    break;
                case 2:
                    if (this.PowerStatus == DevicePowerStatus.PowerCooling || this.PowerStatus == DevicePowerStatus.PowerWarming)
                    {
                        this.Socket.SendCommand(this.DisplayID, @"01D6");
                        pollCount = 0;
                    }
                    break;
                case 4:
                    if (this.PowerStatus == DevicePowerStatus.PowerOn)
                        this.Socket.GetParameter(this.DisplayID, @"0062");
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
                CrestronConsole.Print("Message Type = MessageType.{0}  ", type.ToString());
                Tools.PrintBytes(message, message.Length);
                CrestronConsole.PrintLine("Message = {0}, Length = {1}", messageStr, messageStr.Length);
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
                    case MessageType.SetParameterReply:
                        if (messageStr.StartsWith(@"00006200006400"))
                        {
                            ushort level = ushort.Parse(messageStr.Substring(15, 2), System.Globalization.NumberStyles.HexNumber);
                            _Level = level;
                            if (VolumeChanged != null)
                            {
                                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                            }
                        }
                        break;
                    case MessageType.GetParameterReply:
                        if (messageStr.StartsWith(@"00006200006400"))
                        {
                            ushort level = ushort.Parse(messageStr.Substring(15, 2), System.Globalization.NumberStyles.HexNumber);
                            if (VolumeChanged != null && _Level != level)
                            {
                                _Level = level;
                                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                            }
                        }
                        break;
                }
            }
        }

        public override void Send(string stringToSend)
        {
            Socket.Send(this.DisplayID, MessageType.Command, stringToSend);
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

        ushort _Level;

        void SendVolumeCommand(ushort volume)
        {
            ushort level = (ushort)Tools.ScaleRange(volume, ushort.MinValue, ushort.MaxValue, 0, 100);

            byte[] bytes = BitConverter.GetBytes(level);

            string message = string.Format("006200{0}{1}", bytes[0].ToString("X2"), bytes[1].ToString("X2"));

            this.Socket.SetParameter(this.DisplayID, message);
        }

        bool _Mute;

        void SendMuteCommand(bool mute)
        {
            this.Socket.SetParameter(this.DisplayID, string.Format("008D000{0}", Convert.ToInt16(mute)));
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

        #region IVolumeDevice Members

        public ushort Level
        {
            get
            {
                return _Level;
            }
            set
            {
                _Level = value;
                SendVolumeCommand(_Level);
            }
        }

        public bool Mute
        {
            get
            {
                return _Mute;
            }
            set
            {
                _Mute = value;
                SendMuteCommand(_Mute);
            }
        }

        public bool SupportsMute
        {
            get { return true; }
        }

        public bool SupportsLevel
        {
            get { return true; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }
}