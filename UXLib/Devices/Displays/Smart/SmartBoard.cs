using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.Smart
{
    public class SmartBoard : DisplayDevice, ISocketDevice
    {
        public SmartBoard(string name, string ipAddress, int port)
        {
            this.Name = name;
            this.Socket = new SimpleClientSocket(ipAddress, port, 1000);
            this.Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            this.Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
        }

        public SimpleClientSocket Socket { get; protected set; }
        CTimer pollTimer { get; set; }

        bool commsEstablished = false;

        public void Connect()
        {
            this.Socket.Connect(true);
        }

        public void Disconnect()
        {
            this.Socket.Disconnect();
        }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                pollTimer = new CTimer(PollPower, null, 1000, 10000);
            }
            else if (pollTimer != null)
            {
                pollTimer.Dispose();
            }
        }

        public override void Send(string stringToSend)
        {
            this.Socket.Send(stringToSend + "\x0d");
        }

        public override void OnReceive(string receivedString)
        {
            base.OnReceive(receivedString);

            if (receivedString.StartsWith(">"))
            {
                receivedString = receivedString.Split('>')[1];
                string[] words = receivedString.Split('=');

                if (words.Length == 2)
                {
                    string command = words[0];
                    string value = words[1];

                    switch (command)
                    {
                        case "powerstate":
                            switch (value)
                            {
                                case "on":
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
                                case "off":
                                    PowerStatus = DevicePowerStatus.PowerOff;
                                    commsEstablished = true;
                                    if (RequestedPower)
                                        SendPowerCommand(true);
                                    break;
                                default:
                                    //CrestronConsole.PrintLine("Smart Rx: powerstate={0}", value);
                                    break;
                            }
                            break;
                        case "input":
                            //CrestronConsole.PrintLine("Smart Rx: {0} = {1}", command, value);
                            if (requestedInput.Length > 0 && requestedInput != value)
                                Socket.Send(string.Format("set input={0}\x0d", requestedInput));
                            break;
                        case "volume":
                            if (_volume != uint.Parse(value) && !volumeOK)
                                Volume = _volume;
                            else if (_volume == uint.Parse(value) && !volumeOK)
                                volumeOK = true;
                            break;
                        default:
                            //CrestronConsole.PrintLine("Smart Rx: {0} = {1}", command, value);
                            break;
                    }
                }
                else
                {
                    //CrestronConsole.PrintLine("Smart Rx: {0}", receivedString);
                }
            }
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        void PollPower(object callBackObject)
        {
            this.Socket.Send("get powerstate\x0d");
            if (this.PowerStatus == DevicePowerStatus.PowerOn)
                new CTimer(PollInput, null, 100);
        }

        void PollInput(object callBackObject)
        {
            this.Socket.Send("get input\x0d");
            if (this.PowerStatus == DevicePowerStatus.PowerOn)
                new CTimer(PollVolume, null, 100);
        }

        void PollVolume(object callBackObject)
        {
            this.Socket.Send("get volume\x0d");
        }

        void SendPowerCommand(bool power)
        {
            if (power && PowerStatus == DevicePowerStatus.PowerOff)
            {
                if (Socket.Send("set powerstate=on\x0d") == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                    PowerStatus = DevicePowerStatus.PowerWarming;
            }
            else if (!power && PowerStatus == DevicePowerStatus.PowerOn)
            {
                if (Socket.Send("set powerstate=off\x0d") == Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                    PowerStatus = DevicePowerStatus.PowerCooling;
            }
        }

        public override bool Power
        {
            get
            {
                return base.Power;
            }
            set
            {
                base.Power = value;
                SendPowerCommand(value);
            }
        }

        string requestedInput = "";

        public override DisplayDeviceInput Input
        {
            get
            {
                return base.Input;
            }
            set
            {
                if (InputCommandForInput(value).Length > 0)
                {
                    requestedInput = InputCommandForInput(value);
                    Socket.Send(string.Format("set input={0}\x0d", requestedInput));
                    base.Input = value;
                }
            }
        }

        public string InputCommandForInput(DisplayDeviceInput input)
        {
            switch (input)
            {
                case DisplayDeviceInput.HDMI1:
                    return "hdmi1";
                case DisplayDeviceInput.HDMI2:
                    return "hdmi2";
                case DisplayDeviceInput.HDMI3:
                    return "hdmi3/pc";
                case DisplayDeviceInput.DisplayPort:
                    return "displayport";
                case DisplayDeviceInput.VGA:
                    return "vga1";
                case DisplayDeviceInput.DVI:
                    return "dvi";
                case DisplayDeviceInput.Composite:
                    return "video";
                default: return "";
            }
        }

        uint _volume;
        bool volumeOK = true;
        public uint Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                volumeOK = false;
                this.Socket.Send(string.Format("set volume={0}\x0d", _volume));
            }
        }

        public string HostAddress
        {
            get
            {
                return this.Socket.HostAddress;
            }
        }

        public bool Connected
        {
            get
            {
                return this.Socket.Connected;
            }
        }

        public override CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        public override void Initialize()
        {
            if (this.Connected)
                this.Disconnect();
            this.Connect();
        }
    }
}