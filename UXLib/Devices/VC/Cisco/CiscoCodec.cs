using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Devices.VC.Cisco
{
    /// <summary>
    /// Class for controlling a Cisco VC Codec
    /// </summary>
    public class CiscoCodec
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
            this.password = password;
            KeyboardInteractiveConnectionInfo sshInfo = new KeyboardInteractiveConnectionInfo(hostNameOrIPAddress, 22, username);
            sshInfo.AuthenticationPrompt += new EventHandler<Crestron.SimplSharp.Ssh.Common.AuthenticationPromptEventArgs>(sshInfo_AuthenticationPrompt);
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
        }

        CodecHTTPClient HttpClient { get; set; }

        public bool HttpClientBusy { get { return this.HttpClient.Busy; } }

        internal CodecFeedbackServer FeedbackServer { get; set; }

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

        Thread CheckStatus { get; set; }

        /// <summary>
        /// Connect the codec and initialize the comms to the system
        /// </summary>
        public void Initialize()
        {
            new Thread(GetStatusThread, null, Thread.eThreadStartOptions.Running);
        }

        /// <summary>
        /// Register the feedback server and information required
        /// </summary>
        /// <param name="deregisterFirst">set as true if you want to deregister the slot first</param>
        public void Registerfeedback(bool deregisterFirst)
        {
            this.FeedbackServer.Register(1, new string[] {
                "/Configuration",
                "/Status/SystemUnit",
                "/Status/Audio",
                "/Status/Standby",
                "/Status/Video/Input",
                "/Status/Video/Selfview",
                "/Status/Cameras/SpeakerTrack",
                "/Event/IncomingCallIndication",
                "/Status/Call",
                "/Status/Conference",
                "/Status/UserInterface"
            }, deregisterFirst);
        }

        /// <summary>
        /// Register the feedback server and information required
        /// </summary>
        public void Registerfeedback()
        {
            this.Registerfeedback(false);
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
                        if (!this.HttpClient.HasSessionKey)
                            this.HttpClient.StartSession();

                        this.Registerfeedback(this.FeedbackServer.Registered);
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Error("Could not connect to CiscoCodec", e.Message);
                    }

                    try
                    {
                        if (HasConnected != null && !hasConnectedOnce)
                        {
                            hasConnectedOnce = true;
                            HasConnected(this);
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Exception("Error calling CiscoCodec.HasConnected event", e);
                    }

                    CheckStatus = new Thread(CheckStatusThread, null, Thread.eThreadStartOptions.Running);

                    return null;
                }
                catch (Exception e)
                {
                    if (count >= 5)
                        ErrorLog.Exception("Error in CiscoCodec.GetStatusThread", e);

                    Thread.Sleep(10000);
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
                        ErrorLog.Exception("Error in CiscoCodec.CheckStatusThread", e);
                }
            }
        }

        string password;
        void sshInfo_AuthenticationPrompt(object sender, Crestron.SimplSharp.Ssh.Common.AuthenticationPromptEventArgs e)
        {
            foreach (var prompt in e.Prompts)
            {
                if (prompt.Request.Equals("Password: ", StringComparison.InvariantCultureIgnoreCase))
                {
                    prompt.Response = password;
                }
            }
        }

        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
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
        /// <remarks>This will by default use the SSHClient</remarks>
        public XDocument SendCommand(string path, CommandArgs args)
        {
            return HttpClient.SendCommand(path, args);
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
        public PresentationSendingMode PresentationSendingMode { get; private set; }

        /// <summary>
        /// Get the current presentation source 
        /// </summary>
        public int PresentationSource { get; private set; }

        void State_SystemStateChange(CiscoCodec Codec, SystemState State)
        {
#if DEBUG
            CrestronConsole.PrintLine("Codec State.{0}", State.ToString());
#endif
            new Thread(GetStatusThread, null, Thread.eThreadStartOptions.Running);
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
    }

    /// <summary>
    /// Event handler for the codec connected event
    /// </summary>
    /// <param name="codec">The instance of the Codec</param>
    public delegate void CodecConnectedEventHandler(CiscoCodec codec);

    /// <summary>
    /// Mode to use in presentations
    /// </summary>
    public enum PresentationSendingMode
    {
        Off,
        LocalRemote,
        LocalOnly
    }
}