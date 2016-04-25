using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.Samsung
{
    public class SamsungMDCDisplay : DisplayDevice, ISocketDevice
    {
        public SamsungMDCDisplay(string name, int displayID, SamsungMDCSocket socket)
        {
            this.Name = name;
            this.DisplayID = displayID;
            this.Socket = socket;
            this.Socket.ReceivedPacketEvent += new UXLib.Sockets.SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
            this.Socket.SocketConnectionEvent += new UXLib.Sockets.SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
        }

        public SamsungMDCDisplay(string name, int displayID, string socketAddress)
            : this(name, displayID, new SamsungMDCSocket(socketAddress)) { }

        SamsungMDCSocket Socket;
        public int DisplayID { get; protected set; }

        public void OnReceive(byte[] packet)
        {
            this.DeviceCommunicating = true;

            if (packet[1] == 0xff)
            {
                if (packet[4] == 'A') // Packet contains 'Ack'
                {
                    byte cmd = packet[5];
                    int dataLength = packet[3];
                    byte[] values = new byte[dataLength - 2];
                    for (int b = 6; b < (dataLength + 4); b++)
                        values[b - 6] = packet[b];
#if DEBUG
                    CrestronConsole.Print("Samsung Rx: ");
                    Tools.PrintBytes(packet, packet.Length, true);
#endif
                    if (Enum.IsDefined(typeof(CommandType), cmd))
                    {
                        CommandType cmdType = (CommandType)cmd;
#if DEBUG
                        CrestronConsole.PrintLine("  Command Type = {0}, dataLength = {1}", cmdType.ToString(), dataLength);
#endif
                        switch (cmdType)
                        {
                            case CommandType.Power:
                                //CrestronConsole.PrintLine("  Panel Power = {0} ({1})", values[0], !Convert.ToBoolean(values[0]));
                                if (values[0] == 0 && this.PowerStatus == DevicePowerStatus.PowerOff)
                                {
                                    SetPowerStatus(DevicePowerStatus.PowerWarming);
                                    new CTimer(SetPowerStatus, DevicePowerStatus.PowerOn, 2000);
                                }
                                else if (values[0] > 0 && this.Power)
                                {
                                    SetPowerStatus(DevicePowerStatus.PowerCooling);
                                    new CTimer(SetPowerStatus, DevicePowerStatus.PowerOff, 2000);
                                }
                                break;
                            case CommandType.Status:
                                OnVolumeChange(values[1]);
                                OnMuteChange(Convert.ToBoolean(values[2]));
                                base.Input = GetInputForCommandValue(values[3]);
                                //CrestronConsole.PrintLine("  Mute = {0}, Volume = {1}, Input = {2}, Aspect = {3}", this.Mute, this.Volume, this.Input.ToString(), values[4]);
                                break;
                            case CommandType.DisplayStatus:
                                OnVideoSyncChange(!Convert.ToBoolean(values[3]));
                                //CrestronConsole.PrintLine("  Lamp: {0}, Temp: {1}, No_Sync: {2}", values[0], values[4], values[3]);
                                break;
                            case CommandType.SerialNumber:
                                if (values.Length >= 14)
                                {
                                    _DeviceSerialNumber = Encoding.Default.GetString(values, 0, 14);
                                }
                                break;
                            case CommandType.Volume:
                                OnVolumeChange(values[0]);
                                break;
                            case CommandType.Mute:
                                OnMuteChange(Convert.ToBoolean(values[2]));
                                break;
                            case CommandType.InputSource:
                                base.Input = GetInputForCommandValue(values[0]);
                                break;
                            case CommandType.OSD:
                                _OSD = Convert.ToBoolean(values[0]);
                                break;
                            default:
#if DEBUG
                                CrestronConsole.Print("Other Command \x22{0}\x22, Data: ", cmd.ToString("X2"));
                                Tools.PrintBytes(values, values.Length);
# endif
                                break;
                        }
                    }
                }
                else if (packet[4] == 'N') // Packet contains 'Nak'
                {
                    ErrorLog.Error("Samsung MDC Received Error with command type {0}", packet[5].ToString("X2"));
                }
            }
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            // Check if the display ID of the received packet matches this instance
            if (args.ReceivedPacket[2] == this.DisplayID)
            {
                this.OnReceive(args.ReceivedPacket);
            }
        }

        CTimer pollTimer;

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                PollCommand(CommandType.SerialNumber);
                pollTimer = new CTimer(OnPollEvent, null, 1000, 1000);
            }
            else
            {
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
                    PollCommand(CommandType.Power);
                    break;
                case 2:
                    PollCommand(CommandType.Status);
                    break;
                case 3:
                    PollCommand(CommandType.DisplayStatus);
                    pollCount = 0;
                    break;
            }
        }

        void SendCommand(CommandType command, byte[] data)
        {
            byte[] packet = SamsungMDCSocket.BuildCommand(command, this.DisplayID, data);
            this.Socket.Send(packet);
        }

        void PollCommand(CommandType commandType)
        {
            this.Socket.Send(SamsungMDCSocket.BuildCommand(commandType, this.DisplayID));
        }

        void SetPowerStatus(object powerStatus)
        {
            if (powerStatus is DevicePowerStatus)
            {
                this.PowerStatus = (DevicePowerStatus)powerStatus;
#if DEBUG
                CrestronConsole.PrintLine("Samsung PowerStatus set to {0}", this.PowerStatus.ToString());
#endif
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
                byte[] data = new byte[1];
                data[0] = Convert.ToByte(!value);
                SendCommand(CommandType.Power, data);
                base.Power = value;
            }
        }

        ushort _Volume;
        public ushort Volume
        {
            get
            {
                return _Volume;
            }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    byte[] data = new byte[1];
                    data[0] = (byte)value;
                    SendCommand(CommandType.Volume, data);
                }
            }
        }

        void OnVolumeChange(ushort value)
        {
            if (value != _Volume)
            {
                _Volume = value;
                if (VolumeChange != null)
                    VolumeChange(this, _Volume);
            }
        }

        public event SamsungMDCDisplayVolumeEventHandler VolumeChange;

        bool _Mute;
        public bool Mute
        {
            get
            {
                return _Mute;
            }
            set
            {
                byte[] data = new byte[1];
                data[0] = Convert.ToByte(value);
                SendCommand(CommandType.Mute, data);
            }
        }

        void OnMuteChange(bool value)
        {
            if (value != _Mute)
            {
                _Mute = value;
                if (MuteChange != null)
                    MuteChange(this, _Mute);
            }
        }

        public event SamsungMDCDisplayMuteEventHandler MuteChange;

        bool _VideoSync;
        public bool VideoSync
        {
            get
            {
                return _VideoSync;
            }
        }

        void OnVideoSyncChange(bool value)
        {
            if (_VideoSync != value)
            {
                _VideoSync = value;
                if (VideoSyncChange != null)
                    VideoSyncChange(this, _VideoSync);
            }
        }

        public event SamsungMDCDisplayVideoSyncEventHandler VideoSyncChange;

        bool _OSD;
        public bool OSD
        {
            get
            {
                return _OSD;
            }
            set
            {
                byte[] data = new byte[1];
                data[0] = Convert.ToByte(value);
                SendCommand(CommandType.OSD, data);
            }
        }

        public override DisplayDeviceInput Input
        {
            get
            {
                return base.Input;
            }
            set
            {
                byte[] data = new byte[1];
                data[0] = GetInputCommandForInput(value);
                SendCommand(CommandType.InputSource, data);
            }
        }

        DisplayDeviceInput GetInputForCommandValue(byte value)
        {
            switch (value)
            {
                case 0x14: return DisplayDeviceInput.VGA;
                case 0x18: return DisplayDeviceInput.DVI;
                case 0x1f: return DisplayDeviceInput.DVI;
                case 0x0c: return DisplayDeviceInput.Composite;
                case 0x04: return DisplayDeviceInput.SVideo;
                case 0x08: return DisplayDeviceInput.YUV;
                case 0x21: return DisplayDeviceInput.HDMI1;
                case 0x22: return DisplayDeviceInput.HDMI1;
                case 0x23: return DisplayDeviceInput.HDMI2;
                case 0x24: return DisplayDeviceInput.HDMI2;
                case 0x25: return DisplayDeviceInput.DisplayPort;
                case 0x60: return DisplayDeviceInput.MagicInfo;
                case 0x40: return DisplayDeviceInput.TV;
                case 0x1e: return DisplayDeviceInput.RGBHV;
            }
            throw new IndexOutOfRangeException("Input value out of range");
        }

        byte GetInputCommandForInput(DisplayDeviceInput input)
        {
            switch (input)
            {
                case DisplayDeviceInput.HDMI1: return 0x21;
                case DisplayDeviceInput.HDMI2: return 0x23;
                case DisplayDeviceInput.VGA: return 0x14;
                case DisplayDeviceInput.DVI: return 0x18;
                case DisplayDeviceInput.Composite: return 0x0c;
                case DisplayDeviceInput.YUV: return 0x08;
                case DisplayDeviceInput.DisplayPort: return 0x25;
                case DisplayDeviceInput.MagicInfo: return 0x60;
                case DisplayDeviceInput.TV: return 0x40;
                case DisplayDeviceInput.RGBHV: return 0x1e;
            }
            throw new IndexOutOfRangeException("Input not supported on this device");
        }

        public override string DeviceManufacturer
        {
            get { return "Samsung"; }
        }

        string _model = "Samsung LCD";
        public override string DeviceModel
        {
            get { return _model; }
        }

        string _DeviceSerialNumber;
        public override string DeviceSerialNumber
        {
            get
            {
                return _DeviceSerialNumber;
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

    public delegate void SamsungMDCDisplayVolumeEventHandler(SamsungMDCDisplay display, ushort value);
    public delegate void SamsungMDCDisplayMuteEventHandler(SamsungMDCDisplay display, bool value);
    public delegate void SamsungMDCDisplayVideoSyncEventHandler(SamsungMDCDisplay display, bool value);

    public enum CommandType : byte
    {
        Status = 0x00,
        SerialNumber = 0x0b,
        DisplayStatus = 0x0d,
        Volume = 0x12,
        Mute = 0x13,
        InputSource = 0x14,
        ModelName = 0x8a,
        EnergySaving = 0x92,
        OSD = 0x70,
        OSDType = 0xa3,
        Power = 0xf9
    }
}