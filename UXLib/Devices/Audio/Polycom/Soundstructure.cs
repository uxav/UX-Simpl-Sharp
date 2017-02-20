using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Devices;
using UXLib.Models;
using UXLib.Models.Fusion;
using UXLib.Sockets;

namespace UXLib.Devices.Audio.Polycom
{
    public class Soundstructure : ISocketDevice, IDevice, IFusionStaticAsset
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
        public SoundstructureEthernetSettings LanAdapter { get; protected set; }

        public bool Initialised { get; protected set; }

        public void Initialise()
        {
            CrestronConsole.PrintLine("{0}.Initialise()", this.GetType().Name);
            this.Initialised = false;
            this.Send("get eth_settings 1");
            listedItems.Clear();
            this.Send("vclist");
        }

        void Initialise(object obj)
        {
            this.Initialise();
        }

        public void Reboot()
        {
            this.Socket.Send(string.Format("set {0}\r", SoundstructureCommandType.SYS_REBOOT.ToString().ToLower()));
        }

        void Socket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            this.OnReceive(Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length));
        }

        void Socket_SocketConnectionEvent(SimpleClientSocket socket, Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                CrestronConsole.PrintLine("Soundstructure device Connected on {0}", this.Socket.HostAddress);
                ErrorLog.Notice("Soundstructure device Connected on {0}", this.Socket.HostAddress);
                this.Initialise();
            }
            else
            {
                this.DeviceCommunicating = false;
                this.FusionUpdate();
                if (this.DeviceCommunicatingChanged != null)
                    this.DeviceCommunicatingChanged(this, false);
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

        public bool DeviceCommunicating { get; protected set; }

        public event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;

        public void Send(string stringToSend)
        {
            Crestron.SimplSharp.CrestronSockets.SocketErrorCodes error = this.Socket.Send(stringToSend);

            if (error != Crestron.SimplSharp.CrestronSockets.SocketErrorCodes.SOCKET_OK)
                throw new SocketException("An error occured trying to send a command to the Soundstructure socket");
        }

        public event SoundstructureValueChangeHandler ValueChange;

        public event SoundstructureVoipInfoReceivedHandler VoipInfoReceived;

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
#if DEBUG
            CrestronConsole.PrintLine("Soundstructure Rx: {0}", receivedString);
#endif
            if (!this.DeviceCommunicating)
            {
                this.DeviceCommunicating = true;
                this.FusionUpdate();
                if (this.DeviceCommunicatingChanged != null)
                    this.DeviceCommunicatingChanged(this, true);
            }

            if (receivedString.Contains(' '))
            {
                List<string> elements = SoundstructureSocket.ElementsFromString(receivedString);

                switch (elements[0])
                {
                    case "error":
                        {
                            ErrorLog.Error("Soundtructure received Error: {0}", elements[1]);
                        }
                        break;
                    case "ran":
                        if (PresetRan != null)
                        {
                            PresetRan(this, elements[1]);
                        }
                        break;
                    case "vcitem":
                        // this should be a response from the vclist command which sends back all virtual channels defined
                        try
                        {
                            List<uint> values = new List<uint>();

                            for (int element = 4; element < elements.Count(); element++)
                            {
                                values.Add(Convert.ToUInt32(elements[element]));
                            }

                            SoundstructurePhysicalChannelType type = (SoundstructurePhysicalChannelType)Enum.Parse(typeof(SoundstructurePhysicalChannelType), elements[3], true);

                            if (type == SoundstructurePhysicalChannelType.VOIP_OUT)
                            {
                                listedItems.Add(new VoipOutChannel(this, elements[1], values.ToArray()));
                            }
                            else if (type == SoundstructurePhysicalChannelType.VOIP_IN)
                            {
                                listedItems.Add(new VoipInChannel(this, elements[1], values.ToArray()));
                            }
                            else
                            {
                                listedItems.Add(new VirtualChannel(this, elements[1],
                                    (SoundstructureVirtualChannelType)Enum.Parse(typeof(SoundstructureVirtualChannelType), elements[2], true),
                                    type, values.ToArray()));
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error("Error parsing Soundstructure vcitem: {0}", e.Message);
                        }
                        break;
                    case "vcgitem":
                        {
                            List<ISoundstructureItem> channels = new List<ISoundstructureItem>();
                            if (elements.Count() > 2)
                            {
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
                            else
                            {
                                ErrorLog.Warn("Ignoring Soundstructure group item {0} as it has no members", elements[1]);
                            }
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
                                if (elements[1] == "eth_settings" && elements[2] == "1")
                                {
                                    this.LanAdapter = new SoundstructureEthernetSettings(elements[3]);
                                    break;
                                }

                                bool commandOK = false;
                                SoundstructureCommandType commandType = SoundstructureCommandType.FADER;
                                
                                try
                                {
                                    commandType = (SoundstructureCommandType)Enum.Parse(typeof(SoundstructureCommandType), elements[1], true);
                                    commandOK = true;
                                }
                                catch
                                {
                                    if (elements[1].StartsWith("voip_") && this.VirtualChannels.Contains(elements[2]))
                                    {
                                        VirtualChannel channel = this.VirtualChannels[elements[2]] as VirtualChannel;
                                        if (channel.IsVoip && VoipInfoReceived != null)
                                        {
                                            string info = receivedString.Substring(receivedString.IndexOf(channel.Name) + channel.Name.Length + 2,
                                                receivedString.Length - receivedString.IndexOf(channel.Name) - channel.Name.Length - 2);
                                            VoipInfoReceived(channel, new SoundstructureVoipInfoReceivedEventArgs(elements[1], info));
                                        }
                                    }
                                }

                                if (commandOK)
                                {
                                    switch (commandType)
                                    {
                                        case SoundstructureCommandType.MATRIX_MUTE:
#if DEBUG
                                            CrestronConsole.PrintLine("Matrix Mute Input: \x22{0}\x22 Output: \x22{1}\x22 Value: {2}", elements[2], elements[3], elements[4]);
#endif
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
                                        case SoundstructureCommandType.PHONE_DIAL:
                                            // Cannot parse reply for string values and we don't currently need to track this.
                                            break;
                                        default:
                                            if (elements.Count > 3)
                                                OnValueChange(elements[2], commandType, Convert.ToDouble(elements[3]));
                                            break;
                                    }

                                    if (!Initialised && CheckAllItemsHaveInitialised())
                                    {
                                        Initialised = true;

                                        ErrorLog.Notice("Soundstructure Initialised");
                                        CrestronConsole.PrintLine("Soundstructure Initialised!");

                                        if (HasInitialised != null)
                                            HasInitialised(this);
                                    }
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
                }
            }
        }

        private bool CheckAllItemsHaveInitialised()
        {
            foreach (ISoundstructureItem item in VirtualChannelGroups)
                if (!item.Initialised) return false;
            foreach (ISoundstructureItem item in VirtualChannels)
                if (!item.Initialised) return false;

            return true;
        }

        public event SoundstructureInitialisedCompleteEventHandler HasInitialised;

        bool ChannelIsGrouped(ISoundstructureItem channel)
        {
            foreach (VirtualChannelGroup group in VirtualChannelGroups)
            {
                if (group.Contains(channel))
                    return true;
            }
            return false;
        }

        public event SoundstructurePresetRanEventHandler PresetRan;

        public void RunPreset(string presetName)
        {
            this.Socket.Send(string.Format("run \"{0}\"", presetName));
        }

        public void SetMatrixMute(ISoundstructureItem rowChannel, ISoundstructureItem colChannel, bool muteValue)
        {
            this.Socket.Set(rowChannel, colChannel, SoundstructureCommandType.MATRIX_MUTE, muteValue);
        }

        public static double ScaleRange(double Value,
           double FromMinValue, double FromMaxValue,
           double ToMinValue, double ToMaxValue)
        {
            try
            {
                return (Value - FromMinValue) *
                    (ToMaxValue - ToMinValue) /
                    (FromMaxValue - FromMinValue) + ToMinValue;
            }
            catch
            {
                return double.NaN;
            }
        }

        #region IDevice Members

        string _Name = "Polycom Sounstructure";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

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

        #region ICommDevice Members


        public void Initialize()
        {
            if (!this.Connected)
                this.Connect();
        }

        public CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        #endregion

        #region IFusionStaticAsset Members

        public Crestron.SimplSharpPro.Fusion.FusionStaticAsset FusionAsset
        {
            get;
            protected set;
        }

        #endregion

        #region IFusionAsset Members

        public AssetTypeName AssetTypeName
        {
            get { return AssetTypeName.DSP; }
        }

        public void AssignFusionAsset(FusionController fusionInstance, Crestron.SimplSharpPro.Fusion.FusionAssetBase asset)
        {
            if (asset is FusionStaticAsset)
            {
                this.FusionAsset = asset as FusionStaticAsset;

                fusionInstance.FusionRoom.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);
                fusionInstance.FusionRoom.FusionAssetStateChange += new FusionAssetStateEventHandler(FusionRoom_FusionAssetStateChange);

                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.Bool, 1, "Reboot", Crestron.SimplSharpPro.eSigIoMask.OutputSigOnly);
            }
        }

        void FusionRoom_OnlineStatusChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
                this.FusionUpdate();
        }

        void FusionRoom_FusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            if (args.UserConfigurableAssetDetailIndex == this.FusionAsset.ParamAssetNumber)
            {
                switch (args.EventId)
                {
                    case FusionAssetEventId.StaticAssetAssetBoolAssetSigEventReceivedEventId:
                        Crestron.SimplSharpPro.Fusion.BooleanSigDataFixedName sig = (Crestron.SimplSharpPro.Fusion.BooleanSigDataFixedName)args.UserConfiguredSigDetail;
                        if (sig.Number == 1 && sig.OutputSig.BoolValue)
                            this.Reboot();
                        break;
                }
            }
        }

        public void FusionUpdate()
        {
            try
            {
                if (this.FusionAsset != null)
                {
                    this.FusionAsset.PowerOn.InputSig.BoolValue = this.DeviceCommunicating;
                    this.FusionAsset.Connected.InputSig.BoolValue = this.DeviceCommunicating;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.FusionUpdate(), {1}", this.GetType(), e.Message);
            }
        }

        public void FusionError(string errorDetails)
        {
            if (this.FusionAsset != null)
            {
                this.FusionAsset.AssetError.InputSig.StringValue = errorDetails;
            }
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
        CLINK_AUX_IN,
        CLINK_AUX_OUT,
        CLINK_RAW_IN,
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
        PHONE_RING,
        VOIP_HOLD,
        VOIP_RESUME,
        VOIP_SEND,
        VOIP_ANSWER,
        VOIP_LINE,
        VOIP_DND,
        VOIP_REBOOT,
        SYS_REBOOT
    }

    public delegate void SoundstructureValueChangeHandler(ISoundstructureItem item, SoundstructureValueChangeEventArgs args);

    public delegate void SoundstructureVoipInfoReceivedHandler(ISoundstructureItem item, SoundstructureVoipInfoReceivedEventArgs args);

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

    public class SoundstructureVoipInfoReceivedEventArgs : EventArgs
    {
        public SoundstructureVoipInfoReceivedEventArgs(string command, string info)
        {
            this.Command = command;
            this.Info = info;
        }

        public string Command;
        public string Info;
    }

    public delegate void SoundstructureInitialisedCompleteEventHandler(Soundstructure SoundStructureDevice);

    public delegate void SoundstructurePresetRanEventHandler(Soundstructure soundStructureDevice, string presetName);
}