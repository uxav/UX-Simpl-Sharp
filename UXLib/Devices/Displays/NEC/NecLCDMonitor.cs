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
    public class NecLCDMonitor : DisplayDevice, ISocketDevice, IVolumeDevice, ICommDevice
    {
        public NecLCDMonitor(string name, int displayID, NecDisplaySocket socket)
        {
            this.Name = name;
            this.DisplayID = displayID;
            this.Socket = socket;
            this.Socket.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
            this.Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
        }

        public NecLCDMonitor(string name, int displayID, NecComPortHandler comPortHandler)
        {
            this.Name = name;
            this.DisplayID = displayID;
            this.ComPort = comPortHandler;
            this.ComPort.ReceivedPacket += new NecComPortReceivedPacketEventHandler(ComPort_ReceivedPacket);
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        }

        public int DisplayID { get; protected set; }
        private NecDisplaySocket Socket { get; set; }
        private NecComPortHandler ComPort { get; set; }

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
            else if(this.pollTimer != null)
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
                    this.SendCommand(this.DisplayID, @"01D6");
                    break;
                case 2:
                    if (this.PowerStatus == DevicePowerStatus.PowerCooling || this.PowerStatus == DevicePowerStatus.PowerWarming)
                    {
                        this.SendCommand(this.DisplayID, @"01D6");
                        pollCount = 0;
                    }
                    break;
                case 3:
                    if (this.PowerStatus == DevicePowerStatus.PowerOn)
                        this.GetParameter(this.DisplayID, @"0060");
                    break;
                case 4:
                    if (this.PowerStatus == DevicePowerStatus.PowerOn)
                        this.GetParameter(this.DisplayID, @"0062");
                    pollCount = 0;
                    break;
            }
        }

        bool commsEstablished = false;

        public override void OnReceive(byte[] bytes)
        {
            byte address = bytes[3];
            if (((int)address - 64) == this.DisplayID)
            {
                this.DeviceCommunicating = true;
                string messageLenString = Encoding.Default.GetString(bytes, 5, 2);
                int messageLen = Int16.Parse(messageLenString, System.Globalization.NumberStyles.HexNumber);
                byte[] message = new byte[messageLen];
                Array.Copy(bytes, 7, message, 0, messageLen);
                MessageType type = (MessageType)bytes[4];
                string messageStr = Encoding.Default.GetString(message, 1, message.Length - 2);
#if DEBUG
                CrestronConsole.Print("Message Type = MessageType.{0}  ", type.ToString());
                Tools.PrintBytes(message, message.Length);
                CrestronConsole.PrintLine("Message = {0}, Length = {1}", messageStr, messageStr.Length);
#endif
                try
                {
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
                                _Level = (ushort)Tools.ScaleRange(
                                    ushort.Parse(messageStr.Substring(14, 2), System.Globalization.NumberStyles.HexNumber)
                                    , 0, 100, ushort.MinValue, ushort.MaxValue);
                                if (VolumeChanged != null)
                                    VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                            }
                            break;
                        case MessageType.GetParameterReply:
                            if (messageStr.StartsWith(@"00006200006400"))
                            {
                                _Level = (ushort)Tools.ScaleRange(
                                    ushort.Parse(messageStr.Substring(14, 2), System.Globalization.NumberStyles.HexNumber)
                                    , 0, 100, ushort.MinValue, ushort.MaxValue);
                                if (VolumeChanged != null)
                                    VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                            }
                            else if (messageStr.StartsWith(@"000060"))
                            {
                                byte value = byte.Parse(messageStr.Substring(14, 2), System.Globalization.NumberStyles.HexNumber);
                                if (value != requestedInput && requestedInput > 0)
                                {
                                    SendInputCommand(requestedInput);
                                }
                                else if (value == requestedInput && requestedInput > 0)
                                {
                                    requestedInput = 0x00;
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Exception(string.Format("Error in NecLCDMonitor OnReceive(), type = {0}, messageStr = {1}", type.ToString(), messageStr), e);
                }
            }
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            OnReceive(args.ReceivedPacket);
        }

        void ComPort_ReceivedPacket(NecComPortHandler handler, byte[] receivedPacket)
        {
            OnReceive(receivedPacket);
        }

        public void Initialize()
        {
            if (this.ComPort != null)
            {
                this.ComPort.Initialize();
                pollTimer = new CTimer(OnPollEvent, null, 1000, 1000);
            }
        }

        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (this.pollTimer != null && !this.pollTimer.Disposed)
            {
                this.pollTimer.Stop();
                this.pollTimer.Dispose();
                this.DeviceCommunicating = false;
            }
        }

        void SendPowerCommand(bool power)
        {
            if (power)
                this.SendCommand(this.DisplayID, "C203D60001");
            else
                this.SendCommand(this.DisplayID, "C203D60004");
            this.SendCommand(this.DisplayID, "01D6");
        }

        byte requestedInput = 0x00;

        void SendInputCommand(byte command)
        {
            requestedInput = command;
            string value = "00" + command.ToString("X2");
            CrestronConsole.PrintLine("Send display input command {0}", value);
            this.SetParameter(this.DisplayID, "0060" + value);
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

        public override UXLib.Devices.Displays.DisplayDeviceInput Input
        {
            get
            {
                return base.Input;
            }
            set
            {
                base.Input = value;
                SendInputCommand(GetInputCommandForInput(value));
            }
        }

        DisplayDeviceInput GetInputForCommandValue(byte value)
        {
            switch (value)
            {
                case 0x01: return DisplayDeviceInput.VGA;
                case 0x03: return DisplayDeviceInput.DVI;
                case 0x04: return DisplayDeviceInput.DVI2;
                case 0x0f: return DisplayDeviceInput.DisplayPort;
                case 0x10: return DisplayDeviceInput.DisplayPort2;
                case 0x11: return DisplayDeviceInput.HDMI1;
                case 0x12: return DisplayDeviceInput.HDMI2;
                case 0x82: return DisplayDeviceInput.HDMI3;
                case 0x83: return DisplayDeviceInput.HDMI4;
            }
            throw new IndexOutOfRangeException("Input value out of range");
        }

        byte GetInputCommandForInput(DisplayDeviceInput input)
        {
            switch (input)
            {
                case DisplayDeviceInput.DisplayPort: return 0x0f;
                case DisplayDeviceInput.DisplayPort2: return 0x10;
                case DisplayDeviceInput.HDMI1: return 0x11;
                case DisplayDeviceInput.HDMI2: return 0x12;
                case DisplayDeviceInput.HDMI3: return 0x82;
                case DisplayDeviceInput.HDMI4: return 0x83;
                case DisplayDeviceInput.DVI: return 0x03;
                case DisplayDeviceInput.DVI2: return 0x04;
                case DisplayDeviceInput.VGA: return 0x01;
            }
            throw new IndexOutOfRangeException("Input not supported on this device");
        }

        ushort _Level;

        void SendVolumeCommand(ushort volume)
        {
            ushort level = (ushort)Tools.ScaleRange(volume, ushort.MinValue, ushort.MaxValue, 0, 100);

            byte[] bytes = BitConverter.GetBytes(level);

            string message = string.Format("006200{0}{1}", bytes[0].ToString("X2"), bytes[1].ToString("X2"));

            this.SetParameter(this.DisplayID, message);
        }

        bool _Mute;

        void SendMuteCommand(bool mute)
        {
            this.SetParameter(this.DisplayID, string.Format("008D000{0}", Convert.ToInt16(mute)));
        }

        public void SendCommand(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            if (this.Socket != null)
                this.Socket.Send(address, MessageType.Command, str);
            else if(this.ComPort != null)
                this.ComPort.Send(address, MessageType.Command, str);
        }

        public void SetParameter(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            if (this.Socket != null)
                this.Socket.Send(address, MessageType.SetParameter, str);
            else if (this.ComPort != null)
                this.ComPort.Send(address, MessageType.SetParameter, str);
        }

        public void GetParameter(int address, string message)
        {
            string str = "\x02" + message + "\x03";
            if (this.Socket != null)
                this.Socket.Send(address, MessageType.GetParameter, str);
            else if (this.ComPort != null)
                this.ComPort.Send(address, MessageType.GetParameter, str);
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

        public ushort VolumeLevel
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

        public bool VolumeMute
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

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public bool SupportsVolumeLevel
        {
            get { return true; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }
}