using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UXLib.Models;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Shure
{
    public class ShureCeilingMic : ISocketDevice, IVolumeDevice
    {
        internal ShureCeilingMic(int id, string name, string hostAddress)
        {
            this.ID = id;
            this.Name = name;
            Socket = new ShureSocket(hostAddress);
            Socket.StatusChanged += new TCPSocketStatusChangeEventHandler(Socket_StatusChanged);
            Socket.ReceivedData += new TCPSocketReceivedDataEventHandler(Socket_ReceivedData);
        }

        public int ID { get; protected set; }

        void Socket_StatusChanged(TCPSocketClient client, SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                this.Send("< GET LED_STATE_MUTED >");
                this.Send("< GET LED_STATE_UNMUTED >");
                this.Send("< GET LED_COLOR_MUTED >");
                this.Send("< GET LED_COLOR_UNMUTED >");
                this.Send("< GET DEVICE_AUDIO_MUTE >");
            }
        }

        void Socket_ReceivedData(TCPSocketClient client, byte[] data)
        {
            this.OnReceive(Encoding.ASCII.GetString(data, 0, data.Length));
        }

        ShureSocket Socket { get; set; }

        ShureLEDColor _LEDMuteColor;
        public ShureLEDColor LEDMuteColor
        {
            get { return _LEDMuteColor; }
            set
            {
                this.Send(string.Format("< SET LED_COLOR_MUTED {0} >", value.ToString().ToUpper()));
            }
        }

        ShureLEDColor _LEDUnmuteColor;
        public ShureLEDColor LEDUnmuteColor
        {
            get { return _LEDUnmuteColor; }
            set
            {
                this.Send(string.Format("< SET LED_COLOR_UNMUTED {0} >", value.ToString().ToUpper()));
            }
        }

        ShureLEDState _LEDMuteState;
        public ShureLEDState LEDMuteState
        {
            get { return _LEDMuteState; }
            set
            {
                this.Send(string.Format("< SET LED_STATE_MUTED {0} >", value.ToString().ToUpper()));
            }
        }

        ShureLEDState _LEDUnmuteState;
        public ShureLEDState LEDUnmuteState
        {
            get { return _LEDUnmuteState; }
            set
            {
                this.Send(string.Format("< SET LED_STATE_UNMUTED {0} >", value.ToString().ToUpper()));
            }
        }

        private void ProcessFeedback(string command, string value)
        {
            switch (command)
            {
                case "LED_COLOR_MUTED":
                    _LEDMuteColor = (ShureLEDColor)Enum.Parse(typeof(ShureLEDColor), value, true);
                    break;
                case "LED_COLOR_UNMUTED":
                    _LEDUnmuteColor = (ShureLEDColor)Enum.Parse(typeof(ShureLEDColor), value, true);
                    break;
                case "LED_STATE_MUTED":
                    _LEDMuteState = (ShureLEDState)Enum.Parse(typeof(ShureLEDState), value, true);
                    break;
                case "LED_STATE_UNMUTED":
                    _LEDUnmuteState = (ShureLEDState)Enum.Parse(typeof(ShureLEDState), value, true);
                    break;
                case "DEVICE_AUDIO_MUTE":
                    {
                        _Muted = (value == "ON") ? true : false;
                        if (VolumeChanged != null)
                        {
                            VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                        }
                    }
                    break;
            }
        }

        private void ProcessFeedback(string command, int index, string value)
        {
            
        }

        #region ISocketDevice Members

        public void Connect()
        {
            Socket.Connect();
        }

        public bool Connected
        {
            get { return Socket.Connected; }
        }

        public void Disconnect()
        {
            Socket.Disconnect();
        }

        public string HostAddress
        {
            get { return Socket.HostAddress; }
        }

        #endregion

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;

        public void OnReceive(string receivedString)
        {
#if DEBUG
            CrestronConsole.PrintLine("Shure Mix Rx: {0}", receivedString);
#endif

            Regex r = new Regex(@"\< REP (\w+) (\w+) \>");

            Match match = r.Match(receivedString);

            if (match.Success == true)
            {
#if DEBUG
                CrestronConsole.PrintLine("  {0} = {1}", match.Groups[1], match.Groups[2]);
#endif
                ProcessFeedback(match.Groups[1].Value, match.Groups[2].Value);
            }
            else
            {
                r = new Regex(@"\< REP (\d+) (\w+) (\w+) \>");

                match = r.Match(receivedString);

                if (match.Success == true)
                {
#if DEBUG
                    CrestronConsole.PrintLine("  {0} {1} = {2}", match.Groups[2], match.Groups[1], match.Groups[3]);
#endif
                    ProcessFeedback(match.Groups[2].Value, int.Parse(match.Groups[1].Value), match.Groups[3].Value);
                }
            }
        }

        public void Send(string stringToSend)
        {
            Socket.Send(stringToSend);
        }

        #endregion

        #region IVolumeDevice Members

        public string Name
        {
            get;
            set;
        }

        public bool SupportsVolumeLevel
        {
            get { return false; }
        }

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        public ushort VolumeLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        bool _Muted;
        public bool VolumeMute
        {
            get
            {
                return _Muted;
            }
            set
            {
                this.Send(string.Format("< SET DEVICE_AUDIO_MUTE {0} >", (value) ? "ON" : "OFF"));
            }
        }

        #endregion

        public void Initialize()
        {
            if (!this.Connected)
                this.Connect();
        }

        public CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }
    }

    public enum ShureLEDState
    {
        On,
        Off,
        Flashing
    }

    public enum ShureLEDColor
    {
        Red,
        Green,
        Blue,
        Pink,
        Purple,
        Yellow,
        Orange,
        White
    }
}