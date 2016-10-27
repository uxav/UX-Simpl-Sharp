using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class CodecFeedbackServer
    {
        HttpServer server;
        CiscoCodec Codec;
        EthernetAdapterType AdapterForIPAddress;

        public CodecFeedbackServer(CiscoCodec codec, EthernetAdapterType ethernetAdapter, int feedbackListenerPort)
        {
            AdapterForIPAddress = ethernetAdapter;
            Codec = codec;
            server = new HttpServer(OnReceivedData, EthernetAdapterType.EthernetUnknownAdapter);
            server.Port = feedbackListenerPort;
            server.ServerName = "Cisco Codec Feedback Listener";
            server.Active = true;
            server.KeepAlive = true;
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
            XDocument response;

            if (deregisterFirst)
            {
#if DEBUG
                CrestronConsole.PrintLine("Deresgistering feedback mechanism with CiscoCodec");
#endif
                response = Codec.SendCommand("HttpFeedback/Deregister", args);

#if DEBUG
                CrestronConsole.PrintLine("Deregister repsonse:\r\n{0}", response.ToString());
#endif
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

            response = Codec.SendCommand("HttpFeedback/Register", args);

#if DEBUG
            CrestronConsole.PrintLine("Register repsonse:\r\n{0}", response.ToString());
#endif
        }

        public void Register(int feedbackSlot, string[] expressions)
        {
            this.Register(feedbackSlot, expressions, false);
        }

        public bool Registered
        {
            get
            {
#if DEBUG
                CrestronConsole.PrintLine("Checking codec feedback registration....");
#endif
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/HttpFeedback");
#if DEBUG
                CrestronConsole.PrintLine("");
#endif
                foreach (XElement element in statusInfo)
                {
#if DEBUG
                    CrestronConsole.PrintLine(element.ToString());
#endif

                    string url = element.Elements().Where(e => e.XName.LocalName == "URL").FirstOrDefault().Value;
#if DEBUG
                    CrestronConsole.PrintLine("URL = {0}", url);
#endif
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

        void OnReceivedData(object sender, OnHttpRequestArgs args)
        {
            try
            {
                args.Response.KeepAlive = true;
#if DEBUG
                CrestronConsole.PrintLine("\r\n{0}   New Request to {1} from {2}", DateTime.Now.ToString(), server.ServerName, args.Connection.RemoteEndPointAddress);
#endif
                if (args.Request.Header.RequestType == "POST")
                {
                    XDocument xml = XDocument.Load(new XmlReader(args.Request.ContentString));
#if DEBUG
                    CrestronConsole.PrintLine(xml.ToString());
#endif
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
                                                incomingCallArgs.Call = Codec.Calls[int.Parse(e.Value)];
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
                        CrestronConsole.PrintLine("{0}\r\n", element.ToString());
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
                    return;
                }
                return;
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Exception on codec http feedback server", e);

                args.Response.SendError(500, "Internal server error");
                return;
            }
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