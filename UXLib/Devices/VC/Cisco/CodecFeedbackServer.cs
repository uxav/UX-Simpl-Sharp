using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Devices.VC.Cisco
{
    public class CodecFeedbackServer
    {
        HttpServer server;
        CiscoCodec Codec;
        internal EthernetAdapterType AdapterForIPAddress { get; private set; }

        public CodecFeedbackServer(CiscoCodec codec, EthernetAdapterType ethernetAdapter, int feedbackListenerPort)
        {
            AdapterForIPAddress = ethernetAdapter;
            Codec = codec;
            server = new HttpServer(OnReceivedData, EthernetAdapterType.EthernetUnknownAdapter);
            server.Port = feedbackListenerPort;
            server.ServerName = "Cisco Codec Feedback Listener";
            server.Active = true;
            server.KeepAlive = true;
            server.EnableNagle = true;
#if DEBUG
            CrestronConsole.PrintLine("Created Codec Feedback HttpServer");
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.EthernetAdapterToBindTo", server.EthernetAdapterToBindTo);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.ServerName", server.ServerName);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.ValidateRequests", server.ValidateRequests);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindV4", server.BindV4);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindingV4.BindingAddress", server.BindingV4.BindingAddress);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindingV4.EnableNagle", server.BindingV4.EnableNagle);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindingV4.EndPointAddress", server.BindingV4.EndPointAddress);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindingV4.EndPointPortNumber", server.BindingV4.EndPointPortNumber);
            CrestronConsole.PrintLine("  {0,50} = {1}", "server.BindingV4.Port", server.BindingV4.Port);
#endif
        }

        public void Register(int feedbackSlot, string[] expressions, bool deregisterFirst)
        {
            CommandArgs args = new CommandArgs();
            args.Add("FeedbackSlot", feedbackSlot.ToString());

            if (deregisterFirst || deregisterOnBoot)
            {
                if (deregisterOnBoot)
                {
                    CrestronConsole.PrintLine("Deregistering codec feedback on first boot to combat potential issue with codec lockups");
                }
#if DEBUG
                CrestronConsole.PrintLine("Deresgistering feedback mechanism with CiscoCodec");
#endif
                Codec.SendCommand("HttpFeedback/Deregister", args);

                if (deregisterOnBoot)
                    deregisterOnBoot = false;
            }

            args.Add("ServerUrl", this.ServerURL);

            int count = 1;

            foreach (string expression in expressions)
            {
                args.Add("Expression", count, expression);
                count++;
            }
#if DEBUG
            CrestronConsole.PrintLine("Resgistering feedback mechanism with CiscoCodec");
#endif

            Codec.SendCommand("HttpFeedback/Register", args);
        }

        bool deregisterOnBoot = true;

        public void Register(int feedbackSlot, string[] expressions)
        {
            this.Register(feedbackSlot, expressions, false);
        }

        public bool Registered
        {
            get
            {
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/HttpFeedback");
                foreach (XElement element in statusInfo)
                {
                    string url = element.Elements().Where(e => e.XName.LocalName == "URL").FirstOrDefault().Value;
                    if (url == this.ServerURL)
                        return true;
                }
                return false;
            }
        }

        public string ServerURL
        {
            get
            {
                return string.Format("http://{0}:{1}/",
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(this.AdapterForIPAddress)), this.server.Port
                    );
            }
        }

        public bool Active
        {
            get
            {
                return server.Active;
            }
            set
            {
                server.Active = value;
            }
        }

        public event CodecIncomingCallEventHandler IncomingCallEvent;

        public event CodecUserInterfaceWidgetActionEventHandler WidgetActionEvent;

        object ProcessData(object a)
        {
            OnHttpRequestArgs args = (OnHttpRequestArgs)a;

            try
            {
                args.Response.KeepAlive = true;
#if DEBUG
                CrestronConsole.PrintLine("\r\n>>> CODEC EVENT to {0} from {1}", server.ServerName, args.Connection.RemoteEndPointAddress);
#endif

                if (args.Request.Header.RequestType == "POST")
                {
                    XDocument xml = XDocument.Load(new XmlReader(args.Request.ContentString));

                    XElement identification;
                    string productID;

                    if (xml.Root.HasAttributes)
                    {
                        XNamespace ns = xml.Root.Attribute("xmlns").Value;
                        identification = xml.Root.Element(ns + "Identification");
                        productID = identification.Element(ns + "ProductID").Value;
                    }
                    else
                    {
                        identification = xml.Root.Element("Identification");
                        productID = identification.Element("ProductID").Value;
                    }

                    identification.Remove();
                    XElement element = xml.Root;

#if DEBUG
                    //CrestronConsole.PrintLine(element.ToString());
#endif

                    if (Codec.LoggingEnabled)
                    {
                        Codec.Logger.Log("New Post from {0} at {1}{2}{3}", productID, args.Connection.RemoteEndPointAddress,
                            CrestronEnvironment.NewLine, element.ToString());
                    }

                    if (element.XName.LocalName == "Event" && element.HasElements)
                    {
                        foreach (XElement eventElement in element.Elements())
                        {
                            switch (eventElement.XName.LocalName)
                            {
                                case "IncomingCallIndication":
                                    CodecIncomingCallEventArgs incomingCallArgs = new CodecIncomingCallEventArgs();
                                    foreach (XElement e in eventElement.Elements())
                                    {
                                        switch (e.XName.LocalName)
                                        {
                                            case "RemoteURI":
                                                incomingCallArgs.RemoteURI = e.Value;
                                                break;
                                            case "DisplayNameValue":
                                                incomingCallArgs.DisplayNameValue = e.Value;
                                                break;
                                            case "CallId":
                                                incomingCallArgs.Call = Codec.Calls.GetOrInsert(int.Parse(e.Value));
                                                break;
                                        }
                                    }

                                    try
                                    {
                                        foreach (XElement e in this.Codec.RequestPath("Configuration/Conference/AutoAnswer").Elements())
                                        {
                                            switch (e.XName.LocalName)
                                            {
                                                case "Delay": incomingCallArgs.AutoAnswerDelay = int.Parse(e.Value); break;
                                                case "Mode": incomingCallArgs.AutoAnswerMode = (e.Value == "On"); break;
                                                case "Mute": incomingCallArgs.AutoAnswerMute = (e.Value == "On"); break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        ErrorLog.Error("Error getting auto answer config in Incoming Call event notification handler");
                                    }

                                    try
                                    {
                                        if (IncomingCallEvent != null)
                                            IncomingCallEvent(Codec, incomingCallArgs);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorLog.Exception("Error calling IncomingCallEvent in codec", e);
                                    }

                                    break;
                                case "UserInterface":
                                    try
                                    {
                                        foreach (XElement widget in eventElement.Element("Extensions").Elements("Widget"))
                                        {
                                            foreach (XElement action in widget.Elements("Action"))
                                            {
#if DEBUG
                                                CrestronConsole.PrintLine(action.ToString());
#endif
                                                if (WidgetActionEvent != null)
                                                    WidgetActionEvent(this.Codec,
                                                        new CodecUserInterfaceWidgetActionEventArgs(
                                                            action.Element("WidgetId").Value,
                                                            action.Element("Value").Value,
                                                            (UserInterfaceActionType)Enum.Parse(typeof(UserInterfaceActionType),
                                                            action.Element("Type").Value, true)));
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorLog.Exception("Error in codec event handler for UserInterface Widgets", e);
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string path = element.XName.LocalName;
                        while (element.Elements().Count() == 1 && element.Elements().FirstOrDefault().HasElements)
                        {
                            element = element.Elements().FirstOrDefault();
                            path = string.Format("{0}/{1}", path, element.XName.LocalName);

                            if (path == @"Status/Conference/Site")
                                break;
                            if (path == @"Status/Conference/Call")
                                break;
                            if (path == @"Status/Call")
                                break;
                        }

                        if (element == xml.Root && element.Elements().FirstOrDefault() != null)
                        {
                            element = element.Elements().FirstOrDefault();
                            path = string.Format("{0}/{1}", path, element.XName.LocalName);
                        }

#if DEBUG
                        CrestronConsole.PrintLine("Received {0} Update from {1} for path /{2}", xml.Root.XName.LocalName, productID, path);
                        //ErrorLog.Notice("Received {0} Update from {1} for path /{2}", xml.Root.XName.LocalName, productID, path);
                        //CrestronConsole.PrintLine("{0}\r\n", element.ToString());
#endif
                        if (ReceivedData != null)
                        {
                            try
                            {
                                ReceivedData(this, new CodecFeedbackServerReceiveEventArgs(path, element));
                            }
                            catch (Exception e)
                            {
                                ErrorLog.Exception("Error calling ReceivedData event in CodecFeedbackServer", e);
                            }
                        }
                    }
                }
                else if (args.Request.Header.RequestType == "GET")
                {
                    args.Response.SendError(405, "Method not allowed");
                    return null;
                }
                return null;
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Exception on codec http feedback server", e);

                if (Codec.LoggingEnabled)
                {
                    Codec.Logger.Log("ERROR processing post from {0}{1}Content:{1}{2}{1}StackTrace:{1}{3}", args.Connection.RemoteEndPointAddress,
                        CrestronEnvironment.NewLine, args.Request.ContentString, e.StackTrace);
                }

                args.Response.SendError(500, "Internal server error");
                return null;
            }
        }

        void OnReceivedData(object sender, OnHttpRequestArgs args)
        {
            Thread pThread = new Thread(ProcessData, args, Thread.eThreadStartOptions.CreateSuspended);
            pThread.Priority = Thread.eThreadPriority.UberPriority;
            pThread.Start();
        }

        public event CodecFeedbackServerReceiveEventHandler ReceivedData;
    }

    public delegate void CodecFeedbackServerReceiveEventHandler(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args);

    public class CodecFeedbackServerReceiveEventArgs : EventArgs
    {
        internal CodecFeedbackServerReceiveEventArgs(string path, XElement element)
        {
            this.Path = path;
            this.Data = element;
        }

        public string Path;
        public XElement Data;
    }

    public delegate void CodecIncomingCallEventHandler(CiscoCodec codec, CodecIncomingCallEventArgs args);

    public class CodecIncomingCallEventArgs : EventArgs
    {
        internal CodecIncomingCallEventArgs()
        {
            AutoAnswerDelay = 0;
            AutoAnswerMute = false;
            AutoAnswerMode = false;
        }

        public string RemoteURI { get; internal set; }
        public string DisplayNameValue { get; internal set; }
        public Call Call { get; internal set; }
        public bool AutoAnswerMode { get; internal set; }
        public bool AutoAnswerMute { get; internal set; }
        public int AutoAnswerDelay { get; internal set; }
    }

    public delegate void CodecUserInterfaceWidgetActionEventHandler(CiscoCodec codec, CodecUserInterfaceWidgetActionEventArgs args);

    public class CodecUserInterfaceWidgetActionEventArgs : EventArgs
    {
        public CodecUserInterfaceWidgetActionEventArgs(string id, string value, UserInterfaceActionType action)
        {
            WidgetID = id;
            Value = value;
            Action = action;
        }

        public string WidgetID { get; private set; }
        public string Value { get; private set; }
        public UserInterfaceActionType Action { get; private set; }
    }
}