using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Polycom
{
    public class Soundstructure : ISocketDevice, IDevice
    {
        public Soundstructure(string hostAddress)
        {
            Socket = new SoundstructureSocket(this, hostAddress);
            Socket.ReceivedPacketEvent +=new UXLib.Sockets.SimpleClientSocketReceiveEventHandler(Socket_ReceivedPacketEvent);
            Socket.SocketConnectionEvent += new SimpleClientSocketConnectionEventHandler(Socket_SocketConnectionEvent);
        }

        public SoundstructureSocket Socket { get; protected set; }
        private List<ISoundstructureItem> listedItems = new List<ISoundstructureItem>();
        public SoundstructureItemCollection VirtualChannels { get; protected set; }
        public SoundstructureItemCollection VirtualChannelGroups { get; protected set; }

        public bool Initialised { get; protected set; }

        public void Initialise()
        {
            this.Initialised = false;
            listedItems.Clear();
            this.Send("vclist");
        }

        void Initialise(object obj)
        {
            this.Initialise();
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            this.OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
                new CTimer(Initialise, 1000);
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

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public void Send(string stringToSend)
        {
            Crestron.SimplSharp.CrestronSockets.SocketErrorCodes error = this.Socket.Send(stringToSend);

            if (error != Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                throw new SocketException("An error occured trying to send a command to the Soundstructure socket");
        }

        public event SoundstructureValueChangeHandler ValueChange;

        void OnValueChange(string name, SoundstructureCommandType commandType, double value)
        {
            ISoundstructureItem item = GetItemForName(name);
            if (ValueChange != null && item != null)
                ValueChange(item, new SoundstructureValueChangeEventArgs(commandType, value));
        }

        void OnValueChange(string name, SoundstructureCommandType commandType, string commandModifier, double value)
        {
            ISoundstructureItem item = GetItemForName(name);
            if (ValueChange != null && item != null)
                ValueChange(item, new SoundstructureValueChangeEventArgs(commandType, commandModifier, value));
        }

        ISoundstructureItem GetItemForName(string name)
        {
            try
            {
                if (this.VirtualChannelGroups.Contains(name))
                    return this.VirtualChannelGroups[name];
                else if (this.VirtualChannels.Contains(name))
                    return this.VirtualChannels[name];
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("GetItemForName({0}) Error: {1}", e.Message);
            }
            return null;
        }

        public void OnReceive(string receivedString)
        {
            //CrestronConsole.PrintLine("Soundstructure Rx: {0}", receivedString);
            
            if (receivedString.Contains(' '))
            {
                List<string> elements = SoundstructureSocket.ElementsFromString(receivedString);

                switch (elements[0])
                {
                    case "vcitem":
                        // this should be a response from the vclist command which sends back all virtual channels defined
                        try
                        {
                            List<uint> values = new List<uint>();

                            for (int element = 4; element < elements.Count(); element++)
                            {
                                values.Add(Convert.ToUInt32(elements[element]));
                            }

                            VirtualChannel channel = new VirtualChannel(this, elements[1],
                                (SoundstructureVirtualChannelType)Enum.Parse(typeof(SoundstructureVirtualChannelType), elements[2], true),
                                (SoundstructurePhysicalChannelType)Enum.Parse(typeof(SoundstructurePhysicalChannelType), elements[3], true),
                                values.ToArray());
                            
                            listedItems.Add(channel);
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error("Error parsing Soundstructure vcitem: {0}", e.Message);
                        }
                        break;
                    case "vcgitem":
                        {
                            List<ISoundstructureItem> channels = new List<ISoundstructureItem>();
                            for (int e = 2; e < elements.Count(); e++)
                            {
                                if (this.VirtualChannels.Contains(elements[e]))
                                {
                                    channels.Add(this.VirtualChannels[elements[e]]);
                                }
                            }
                            VirtualChannelGroup group = new VirtualChannelGroup(this, elements[1], channels);
                            listedItems.Add(group);
                        }
                        break;
                    case "vcrename":
                        {
                            List<ISoundstructureItem> channels = new List<ISoundstructureItem>();
                            foreach (VirtualChannel channel in this.VirtualChannels)
                            {
                                if (channel.Name == elements[1])
                                {
                                    VirtualChannel newChannel = new VirtualChannel(this, elements[2],
                                        channel.VirtualChannelType, channel.PhysicalChannelType, channel.PhysicalChannelIndex.ToArray());
                                    channels.Add(newChannel);
                                }
                                else
                                {
                                    channels.Add(channel);
                                }
                            }
                            this.VirtualChannels = new SoundstructureItemCollection(channels);
                        }
                        break;
                    case "vcgrename":
                        {
                            List<ISoundstructureItem> groups = new List<ISoundstructureItem>();
                            foreach (VirtualChannelGroup group in this.VirtualChannelGroups)
                            {
                                if (group.Name == elements[1])
                                {
                                    List<ISoundstructureItem> channels = new List<ISoundstructureItem>();
                                    foreach (VirtualChannel channel in group)
                                    {
                                        channels.Add(channel);
                                    }
                                    VirtualChannelGroup newGroup = new VirtualChannelGroup(this, elements[2], channels);
                                    groups.Add(newGroup);
                                }
                                else
                                {
                                    groups.Add(group);
                                }
                            }
                            this.VirtualChannelGroups = new SoundstructureItemCollection(groups);
                        }
                        break;
                    case "val":
                        // this should be a value response from a set or get
                        {
                            try
                            {
                                SoundstructureCommandType commandType = (SoundstructureCommandType)Enum.Parse(typeof(SoundstructureCommandType), elements[1], true);

                                switch (commandType)
                                {
                                    case SoundstructureCommandType.MATRIX_MUTE:
                                        CrestronConsole.PrintLine("Matrix Mute Input: \x22{0}\x22 Output: \x22{1}\x22 Value: {2}", elements[2], elements[3], elements[4]);
                                        break;
                                    case SoundstructureCommandType.FADER:
                                        if (elements[2] == "min" || elements[2] == "max")
                                        {
                                            OnValueChange(elements[3], commandType, elements[2], Convert.ToDouble(elements[4]));
                                        }
                                        else
                                        {
                                            OnValueChange(elements[2], commandType, Convert.ToDouble(elements[3]));
                                        }
                                        break;
                                    default:
                                        OnValueChange(elements[2], commandType, Convert.ToDouble(elements[3]));
                                        break;
                                }
                            }
                            catch  (Exception e)
                            {
                                ErrorLog.Error("Soundstructure Rx: {0}, Error: {1}", receivedString, e.Message);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (receivedString == "vclist")
                {
                    this.VirtualChannels = new SoundstructureItemCollection(listedItems);
                    listedItems.Clear();

                    Socket.Send("vcglist");
                }
                else if (receivedString == "vcglist")
                {
                    this.VirtualChannelGroups = new SoundstructureItemCollection(listedItems);
                    listedItems.Clear();

                    foreach (ISoundstructureItem item in VirtualChannelGroups)
                        item.Init();

                    foreach (ISoundstructureItem item in VirtualChannels)
                    {
                        if (!ChannelIsGrouped(item))
                        {
                            item.Init();
                        }
                    }

                    Initialised = true;
                }
            }
        }

        bool ChannelIsGrouped(ISoundstructureItem channel)
        {
            foreach (VirtualChannelGroup group in VirtualChannelGroups)
            {
                if (group.Contains(channel))
                    return true;
            }
            return false;
        }

        #endregion

        #region IDevice Members

        public string Name { get; set; }

        public string DeviceManufacturer
        {
            get { return "Polycom"; }
        }

        public string DeviceModel
        {
            get { return "Soundstructure"; }
        }

        public string DeviceSerialNumber
        {
            get { return string.Empty; }
        }

        #endregion
    }

    public enum SoundstructureVirtualChannelType
    {
        MONO,
        STEREO,
        CONTROL,
        CONTROL_ARRAY
    }

    public enum SoundstructurePhysicalChannelType
    {
        CR_MIC_IN,
        CR_LINE_OUT,
        SR_MIC_IN,
        SR_LINE_OUT,
        PSTN_IN,
        PSTN_OUT,
        VOIP_IN,
        VOIP_OUT,
        SIG_GEN,
        SUBMIX,
        CLINK_IN,
        CLINK_OUT,
        DIGITAL_GPIO_IN,
        DIGITAL_GPIO_OUT,
        ANALOG_GPIO_IN,
        IR_IN
    }

    public enum SoundstructureCommandType
    {
        LINE_OUT_GAIN,
        FADER,
        MUTE,
        MATRIX_MUTE,
        MIC_IN_GAIN,
        SAFETY_MUTE,
        PHONE_CONNECT,
        PHONE_DIAL,
        PHONE_REJECT,
        PHONE_IGNORE,
        PHONE_RING
    }

    public delegate void SoundstructureValueChangeHandler(ISoundstructureItem item, SoundstructureValueChangeEventArgs args);

    public class SoundstructureValueChangeEventArgs : EventArgs
    {
        public SoundstructureValueChangeEventArgs(SoundstructureCommandType commandType, double value)
        {
            this.CommandType = commandType;
            this.Value = value;
        }

        public SoundstructureValueChangeEventArgs(SoundstructureCommandType commandType, string commmandModifier, double value)
        {
            this.CommandType = commandType;
            this.CommandModifier = commmandModifier;
            this.Value = value;
        }

        public SoundstructureCommandType CommandType;
        public string CommandModifier;
        public double Value;
    }
}