using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UXLib.Sockets;
using UXLib.Devices;

namespace UXLib.Displays.Canon
{
    public class CanonWUX6000 : DisplayDevice, ISocketDevice
    {
        public CanonWUX6000(string name, string ipAddress)
        {
            this.Name = name;
            Socket = new CanonProjectorSocket(ipAddress);
            Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
            Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
        }

        public CanonProjectorSocket Socket;
        CTimer pollTimer;

        bool commsEstablished = false;

        public void Connect()
        {
            this.Socket.Connect(true);
        }

        public void Disconnect()
        {
            this.Socket.Disconnect();
        }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                pollTimer = new CTimer(PollPower, null, 2000, 2000);
            }
            else
            {
                pollTimer.Dispose();
                DeviceCommunicating = false;
            }
        }

        public override void Send(string stringToSend)
        {
            this.Socket.Send(stringToSend);
        }

        public override void OnReceive(string receivedString)
        {
            base.OnReceive(receivedString);

            if (receivedString.StartsWith("g"))
            {
                receivedString = receivedString.Split(':')[1];
                string[] words = receivedString.Split('=');

                if (words.Length == 2)
                {
                    string command = words[0];
                    string value = words[1];

                    switch (command)
                    {
                        case "POWER":
                            switch (value)
                            {
                                case "ON":
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
                                case "OFF":
                                    PowerStatus = DevicePowerStatus.PowerOff;
                                    commsEstablished = true;
                                    if (RequestedPower)
                                        SendPowerCommand(true);
                                    break;
                                case "OFF2ON":
                                    commsEstablished = true;
                                    PowerStatus = DevicePowerStatus.PowerWarming;
                                    break;
                                case "ON2OFF":
                                    commsEstablished = true;
                                    PowerStatus = DevicePowerStatus.PowerCooling;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "INPUT":
                            if (requestedInput.Length > 0 && requestedInput != value)
                                Socket.Send(string.Format("INPUT={0}", requestedInput));
                            break;
                        default:
                            //CrestronConsole.PrintLine("Projector {0} = {1}", command, value);
                            break;
                    }
                }
            }/*
            else
                CrestronConsole.PrintLine("Projector Rx ({0} bytes): {1}", receivedString.Length, receivedString);*/
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        void PollPower(object callBackObject)
        {
            this.Socket.Send("?POWER");
            if (this.PowerStatus == DevicePowerStatus.PowerOn)
                new CTimer(PollInput, null, 100);
        }

        void PollInput(object callBackObject)
        {
            this.Socket.Send("?INPUT");
        }

        void SendPowerCommand(bool power)
        {
            if (power && PowerStatus == DevicePowerStatus.PowerOff)
            {
                if (Socket.Send("POWER ON") == SocketErrorCodes.SOCKET_OK)
                    PowerStatus = DevicePowerStatus.PowerWarming;
            }
            else if (!power && PowerStatus == DevicePowerStatus.PowerOn)
            {
                if (Socket.Send("POWER OFF") == SocketErrorCodes.SOCKET_OK)
                    PowerStatus = DevicePowerStatus.PowerCooling;
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
                    Socket.Send(string.Format("INPUT={0}", requestedInput));
                    base.Input = value;
                }
            }
        }

        public string InputCommandForInput(DisplayDeviceInput input)
        {
            switch (input)
            {
                case DisplayDeviceInput.HDMI1:
                case DisplayDeviceInput.HDMI2:
                case DisplayDeviceInput.HDMI3:
                case DisplayDeviceInput.HDMI4:
                    return "HDMI";
                case DisplayDeviceInput.VGA:
                    return "A-RGB";
                case DisplayDeviceInput.DVI:
                    return "D-RGB";
                case DisplayDeviceInput.YUV:
                    return "COMP";
                case DisplayDeviceInput.Composite:
                    return "VIDEO";
                default: return "";
            }
        }

        public string IPAddress
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

        public string HostAddress
        {
            get { return this.Socket.HostAddress; }
        }

        public override string DeviceManufacturer
        {
            get { return "Canon"; }
        }

        public override string DeviceModel
        {
            get { return "WUX6000"; }
        }

        public override string DeviceSerialNumber
        {
            get { return string.Empty; }
        }
    }
}