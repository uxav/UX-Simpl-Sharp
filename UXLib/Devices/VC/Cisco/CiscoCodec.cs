using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Models;

namespace UXLib.Devices.VC.Cisco
{
    /// <summary>
    /// Class for controlling a Cisco VC Codec
    /// </summary>
    public class CiscoCodec : IDevice, ICommDevice, IFusionStaticAsset
    {
        /// <summary>
        /// Create an instance of a Cisco VC Codec
        /// </summary>
        /// <param name="hostNameOrIPAddress">The IP Address or hostname of the codec</param>
        /// <param name="username">Username to login to the system</param>
        /// <param name="password">Password to login to the system</param>
        /// <param name="ethernetAdapter">Ther EthernetAdapterType of the control system used to connect and receive data</param>
        /// <param name="feedbackListenerPort">The port to be used for the feedback server on the control system</param>
        public CiscoCodec(string hostNameOrIPAddress, string username, string password, EthernetAdapterType ethernetAdapter, int feedbackListenerPort)
        {
            HttpClient = new CodecHTTPClient(hostNameOrIPAddress, username, password);
            FeedbackServer = new CodecFeedbackServer(this, ethernetAdapter, feedbackListenerPort);
            FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            FeedbackServer.IncomingCallEvent += new CodecIncomingCallEventHandler(FeedbackServer_IncomingCallEvent);
            FeedbackServer.WidgetActionEvent += new CodecUserInterfaceWidgetActionEventHandler(FeedbackServer_WidgetActionEvent);
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
            SystemUnit = new SystemUnit(this);
            SystemUnit.State.SystemStateChange += new SystemUnitStateSystemChangeEventHandler(State_SystemStateChange);
            Audio = new Audio(this);
            Calls = new Calls(this);
            Conference = new Conference(this);
            Network = new Network(this);
            Phonebook = new Phonebook(this);
            Cameras = new Cameras(this);
            Video = new Video(this);
            Capabilities = new Capabilities(this);
            Standby = new Standby(this);
            UserInterface = new UserInterface(this);
        }

        CodecHTTPClient HttpClient { get; set; }

        /// <summary>
        /// Returns true if the HttpClient for the codec is busy running a process
        /// </summary>
        public bool HttpClientBusy { get { return this.HttpClient.Busy; } }

        internal CodecFeedbackServer FeedbackServer { get; set; }

        /// <summary>
        /// Get if Logging is enabled or not
        /// </summary>
        public bool LoggingEnabled { get; private set; }

        /// <summary>
        /// Enable logging of Codec events
        /// </summary>
        /// <param name="logger">An event logger</param>
        public void LoggingEnable(Logger logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
                LoggingEnabled = true;
            }
        }

        /// <summary>
        /// The logger assigned for logging Codec events
        /// </summary>
        public Logger Logger { get; private set; }

        /// <summary>
        /// Stop Logging of codec events
        /// </summary>
        public void LoggingStop()
        {
            LoggingEnabled = false;
        }

        /// <summary>
        /// This contains information on the main Codec system unit
        /// </summary>
        public SystemUnit SystemUnit { get; private set; }

        /// <summary>
        /// Access all Audio functions of the codec including volume and mutes
        /// </summary>
        public Audio Audio { get; private set; }

        /// <summary>
        /// For controlling calls on the codec
        /// </summary>
        public Calls Calls { get; private set; }
        
        /// <summary>
        /// Conference functions
        /// </summary>
        public Conference Conference { get; private set; }

        /// <summary>
        /// Access information of the network status of the codec
        /// </summary>
        public Network Network { get; private set; }

        /// <summary>
        /// Phonebook functions
        /// </summary>
        public Phonebook Phonebook { get; private set; }

        /// <summary>
        /// Control cameras
        /// </summary>
        public Cameras Cameras { get; private set; }

        /// <summary>
        /// Video functions and switching
        /// </summary>
        public Video Video { get; private set; }

        /// <summary>
        /// Get codec capabilities
        /// </summary>
        public Capabilities Capabilities { get; private set; }

        /// <summary>
        /// Codec standby functions and values
        /// </summary>
        public Standby Standby { get; private set; }

        /// <summary>
        /// The UserInterface functions of the codec
        /// </summary>
        public UserInterface UserInterface { get; private set; }

        Thread CheckStatus { get; set; }

        /// <summary>
        /// Connect the codec and initialize the comms to the system
        /// </summary>
        public void Initialize()
        {
            try
            {
                new Thread(GetStatusThread, null, Thread.eThreadStartOptions.Running);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error launching {0}.GetStatusThread", this.GetType().Name);
            }
        }

        /// <summary>
        /// Register the feedback server and information required
        /// </summary>
        /// <param name="deregisterFirst">set as true if you want to deregister the slot first</param>
        public void Registerfeedback()
        {
            this.FeedbackServer.Register(1, new string[] {
                "/Configuration",
                "/Status/SystemUnit",
                "/Status/Audio",
                "/Status/Standby",
                "/Status/Video/Input",
                "/Status/Video/Selfview",
                "/Status/Cameras/SpeakerTrack",
                "/Status/Call",
                "/Status/Conference",
                "/Status/UserInterface",
                "/Event/IncomingCallIndication",
                "/Event/UserInterface/Extensions/Widget"
            });
        }

        bool hasConnectedOnce = false;

        /// <summary>
        /// Event raised when the codec connects
        /// </summary>
        public event CodecConnectedEventHandler HasConnected;

        object GetStatusThread(object threadObject)
        {
            int count = 0;

            while (true)
            {
                count++;

                try
                {
                    try
                    {
                        if (programStopping)
                            return null;

                        if (!this.HttpClient.HasSessionKey)
                            this.HttpClient.StartSession();

                        if (!this.FeedbackServer.Registered)
                            this.Registerfeedback();

                        this.DeviceCommunicating = true;
                        
                        try
                        {
                            if (HasConnected != null && !hasConnectedOnce)
                            {
                                hasConnectedOnce = true;
                                ErrorLog.Notice("{0} has connected successfully.. launching thread to continue status checking", this.GetType().Name);
                                HasConnected(this);

                                if (CheckStatus == null || CheckStatus.ThreadState != Thread.eThreadStates.ThreadRunning)
                                    CheckStatus = new Thread(CheckStatusThread, null, Thread.eThreadStartOptions.Running);
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Exception("Error calling CiscoCodec.HasConnected event", e);
                        }

                        return null;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception e)
                {
                    if (count >= 5)
                        ErrorLog.Exception("Error in CiscoCodec.GetStatusThread", e);
                }
            }
        }

        object CheckStatusThread(object threadObject)
        {
            Thread.Sleep(60000);
            
            while (true)
            {
                try
                {
                    if (!this.HttpClient.Busy)
                    {
                        if (!this.HttpClient.HasSessionKey)
                            this.HttpClient.StartSession();

                        bool registered = this.FeedbackServer.Registered;
#if DEBUG
                    CrestronConsole.PrintLine("Feedback Registered = {0}", registered);
#endif
                        if (!registered)
                        {
                            ErrorLog.Warn("The CiscoCodec was not registered for feedback on CheckStatusThread. Codec could have unregistered itself due to Post errors or connectivity problems");
#if DEBUG
                        CrestronConsole.PrintLine("Registering Feedback");
#endif
                            this.Registerfeedback();
                        }

                        this.DeviceCommunicating = true;

                        this.Calls.Update();

                        if (this.Calls.Active.Count() > 0)
                            Thread.Sleep(5000);
                        else
                            Thread.Sleep(60000);
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                    {
                        ErrorLog.Exception("Error in CiscoCodec.CheckStatusThread", e);
                        this.DeviceCommunicating = false;
                    }
                }
            }
        }

        bool programStopping = false;

        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            programStopping = true;
            if (CheckStatus != null)
                CheckStatus.Abort();
            if (FeedbackServer != null && FeedbackServer.Active)
                FeedbackServer.Active = false;
        }

        /// <summary>
        /// Post a command
        /// </summary>
        /// <param name="path">The XPath of the command</param>
        /// <returns>XDocument containing the XML response</returns>
        /// <remarks>This will by default use the SSHClient</remarks>
        public XDocument SendCommand(string path)
        {
            return this.HttpClient.SendCommand(path);
        }

        /// <summary>
        /// Post a command with arguments
        /// </summary>
        /// <param name="path">The XPath of the command</param>
        /// <param name="args">The arguments in the form of a built CommandArgs instance</param>
        /// <returns>XDocument containing the XML response</returns>
        public XDocument SendCommand(string path, CommandArgs args)
        {
            return HttpClient.SendCommand(path, args);
        }

        /// <summary>
        /// Send a xConfiguation command
        /// </summary>
        /// <param name="path">Xpath of the config</param>
        /// <param name="args">config arguments</param>
        /// <returns>XDocument containing the XML response</returns>
        public XDocument SendConfiguration(string path, CommandArgs args)
        {
            return HttpClient.SendConfiguration(path, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The XPath of the request</param>
        /// <param name="useHttp">Set as true to force using the HttpClient otherwise it will use the SSHClient</param>
        /// <returns>An IEnumberable containing returned XElements of data</returns>
        public IEnumerable<XElement> RequestPath(string path)
        {
            return HttpClient.RequestPath(path);
        }
        
        /// <summary>
        /// Get call history information
        /// </summary>
        /// <param name="count">Count of items to get</param>
        /// <returns>CallHistory instance containing the information</returns>
        public CallHistory GetCallHistory(int count)
        {
            return new CallHistory(this, count);
        }

        /// <summary>
        /// Start a presentation
        /// </summary>
        /// <remarks>Will use the default presentation source</remarks>
        public void PresentationStart()
        {
            SendCommand("Presentation/Start");
        }

        /// <summary>
        /// Start a presentation
        /// </summary>
        /// <param name="presentationSource">The number of the codec source to use</param>
        public void PresentationStart(int presentationSource)
        {
            PresentationStart(presentationSource, PresentationSendingMode.LocalRemote);
        }

        /// <summary>
        /// Start a presentation
        /// </summary>
        /// <param name="presentationSource">The number of the codec source to use</param>
        /// <param name="sendingMode">PresentationSendingMode option to use</param>
        public void PresentationStart(int presentationSource, PresentationSendingMode sendingMode)
        {
            CommandArgs args = new CommandArgs("PresentationSource", presentationSource);
            args.Add("SendingMode", sendingMode.ToString());

            SendCommand("Presentation/Start", args);
        }

        /// <summary>
        /// Stop the current presentation
        /// </summary>
        public void PresentationStop()
        {
            SendCommand("Presentation/Stop");
        }

        /// <summary>
        /// Get the current Sending Mode for Presentation
        /// </summary>
        public PresentationSendingMode PresentationSendingMode
        {
            get;
            private set;
        }

        /// <summary>
        /// PresentationSendingMode has changed
        /// </summary>
        public event PresentationModeChangedEventHandler PresentationSendingModeChanged;

        /// <summary>
        /// Get the current presentation source 
        /// </summary>
        public int PresentationSource { get; private set; }

        void State_SystemStateChange(CiscoCodec Codec, SystemState State)
        {
#if DEBUG
            CrestronConsole.PrintLine("Codec State.{0}", State.ToString());
#endif
            try
            {
                new Thread(GetStatusThread, null, Thread.eThreadStartOptions.Running);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error launching {0}.GetStatusThread", this.GetType().Name);
            }
        }
        
        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            try
            {
                switch (args.Path)
                {
                    case @"Status/Conference/Presentation":
#if DEBUG
                        CrestronConsole.PrintLine("Received feedback for {0}", args.Path);
                        CrestronConsole.PrintLine(args.Data.ToString());
#endif
                        if (args.Data.XName.LocalName == "Presentation")
                        {
                            if (args.Data.Element("LocalSendingMode") != null)
                                PresentationSendingMode = (PresentationSendingMode)Enum.Parse(typeof(PresentationSendingMode), args.Data.Element("LocalSendingMode").Value, true);
                            if (args.Data.Element("LocalSource") != null)
                                PresentationSource = int.Parse(args.Data.Element("LocalSource").Value);

                            if (PresentationSendingModeChanged != null)
                                PresentationSendingModeChanged(this);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Error in CiscoCodec.FeedbackServer_ReceivedData, path = {0}", args.Path), e);
            }
        }

        public event CodecIncomingCallEventHandler IncomingCall;

        void FeedbackServer_IncomingCallEvent(CiscoCodec codec, CodecIncomingCallEventArgs args)
        {
            if (IncomingCall != null)
                IncomingCall(this, args);
        }

        public event CodecUserInterfaceWidgetActionEventHandler WidgetActionEvent;

        void FeedbackServer_WidgetActionEvent(CiscoCodec codec, CodecUserInterfaceWidgetActionEventArgs args)
        {
            if (this.WidgetActionEvent != null)
                this.WidgetActionEvent(codec, args);
        }

        public void WidgetSetValue(string widgetID, string value)
        {
            this.SendCommand("UserInterface/Extensions/Widget/SetValue", new CommandArgs()
                .Add("WidgetId", widgetID)
                .Add("Value", value));
        }

        public void WidgetSetValue(string widgetID, int value)
        {
            this.SendCommand("UserInterface/Extensions/Widget/SetValue", new CommandArgs()
                .Add("WidgetId", widgetID)
                .Add("Value", value));
        }

        public void WidgetUnsetValue(string widgetID)
        {
            this.SendCommand("UserInterface/Extensions/Widget/UnsetValue",
                new CommandArgs("WidgetId", widgetID));
        }

        #region IDevice Members

        public string Name
        {
            get
            {
                if (this.SystemUnit.ProductId != null && this.SystemUnit.ProductId.Length > 0)
                    return this.SystemUnit.ProductId;
                return "Cisco Codec";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DeviceManufacturer
        {
            get { return "Cisco"; }
        }

        public string DeviceModel
        {
            get { return "SX80"; }
        }

        public string DeviceSerialNumber
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ICommDevice Members

        bool _DeviceCommunicating;
        public bool DeviceCommunicating
        {
            get { return _DeviceCommunicating; }
            protected set
            {
                if (_DeviceCommunicating != value)
                {
                    _DeviceCommunicating = value;
                    FusionUpdate();
                    if (this.DeviceCommunicatingChanged != null)
                        this.DeviceCommunicatingChanged(this, value);
                }
            }
        }

        public event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;

        public void Send(string stringToSend)
        {
            throw new NotImplementedException();
        }

        public void OnReceive(string receivedString)
        {
            throw new NotImplementedException();
        }

        public CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        #endregion

        #region IFusionAsset Members

        public void AssignFusionAsset(Fusion fusionInstance, FusionAssetBase asset)
        {
            if (asset is FusionStaticAsset)
            {
                this.FusionAsset = asset as FusionStaticAsset;

                fusionInstance.FusionRoom.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);
                fusionInstance.FusionRoom.FusionAssetStateChange += new FusionAssetStateEventHandler(FusionRoom_FusionAssetStateChange);

                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.Bool, 1, "Out of Standby", Crestron.SimplSharpPro.eSigIoMask.InputSigOnly);
                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.Bool, 2, "In Call", Crestron.SimplSharpPro.eSigIoMask.InputSigOnly);
                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.Bool, 3, "Mic Muted", Crestron.SimplSharpPro.eSigIoMask.InputSigOnly);
                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.String, 1, "Serial Number", Crestron.SimplSharpPro.eSigIoMask.InputSigOnly);
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
                CrestronConsole.PrintLine("{0}.FusionRoom_FusionAssetStateChange", this.GetType());
                CrestronConsole.PrintLine("  args.EventId = {0}", args.EventId);
                CrestronConsole.PrintLine("  args.UserConfiguredSigDetail = {0}", args.UserConfiguredSigDetail.GetType());
            }
        }

        public FusionStaticAsset FusionAsset
        {
            get;
            protected set;
        }

        public AssetTypeName AssetTypeName
        {
            get { return AssetTypeName.VideoConferenceCodec; }
        }

        public virtual void FusionUpdate()
        {
            try
            {
                if (this.FusionAsset != null)
                {
                    this.FusionAsset.PowerOn.InputSig.BoolValue = this.DeviceCommunicating;
                    this.FusionAsset.Connected.InputSig.BoolValue = this.DeviceCommunicating;
                    this.FusionAsset.FusionGenericAssetDigitalsAsset1.BooleanInput[1].BoolValue = (this.Standby.StandbyState != StandbyState.Standby);
                    this.FusionAsset.FusionGenericAssetDigitalsAsset1.BooleanInput[2].BoolValue = (this.Calls.Count > 0);
                    this.FusionAsset.FusionGenericAssetDigitalsAsset1.BooleanInput[3].BoolValue = this.Audio.Microphones.Mute;
                    this.FusionAsset.FusionGenericAssetSerialsAsset3.StringInput[1].StringValue = this.SystemUnit.Hardware.ModuleSerialNumber;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.FusionUpdate(), {1}", this.GetType(), e.Message);
            }
        }

        public virtual void FusionError(string errorDetails)
        {
            if (this.FusionAsset != null)
            {
                this.FusionAsset.AssetError.InputSig.StringValue = errorDetails;
            }
        }

        #endregion
    }

    /// <summary>
    /// Event handler for the codec connected event
    /// </summary>
    /// <param name="codec">The instance of the Codec</param>
    public delegate void CodecConnectedEventHandler(CiscoCodec codec);

    public delegate void PresentationModeChangedEventHandler(CiscoCodec codec);

    /// <summary>
    /// Mode to use in presentations
    /// </summary>
    public enum PresentationSendingMode
    {
        Off,
        LocalRemote,
        LocalOnly
    }

    public enum UserInterfaceActionType
    {
        Pressed,
        Changed,
        Released,
        Clicked
    }
}