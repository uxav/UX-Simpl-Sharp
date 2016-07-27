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

        public CodecFeedbackServer(CiscoCodec codec, EthernetAdapterType ethernetAdapterType, int port)
        {
            Codec = codec;
            server = new HttpServer();
            server.Port = port;
            server.ServerName = "Cisco Codec Feedback Listener";
            server.EthernetAdapterToBindTo = ethernetAdapterType;
            server.KeepAlive = true;
            server.OnHttpRequest += new OnHttpRequestHandler(OnReceivedData);
            server.Active = true;
        }

        public void Register(int feedbackSlot, string[] expressions)
        {
            CommandArgs args = new CommandArgs();

            args.Add("FeedbackSlot", feedbackSlot.ToString());
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

            XDocument response = Codec.SendCommand("HttpFeedback/Register", args, true);

#if DEBUG
            CrestronConsole.PrintLine("Register repsonse:\r\n{0}", response.ToString());
#endif
        }

        public bool Registered
        {
            get
            {
#if DEBUG
                CrestronConsole.PrintLine("Checking codec feedback registration....");
#endif
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/HttpFeedback", true);
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
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(this.server.EthernetAdapterToBindTo)), this.server.Port
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

        void OnReceivedData(object sender, OnHttpRequestArgs args)
        {
            try
            {
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
                        switch (element.Elements().First().XName.LocalName)
                        {
                            case "IncomingCallIndication":
                                CodecIncomingCallEventArgs incomingCallArgs = new CodecIncomingCallEventArgs();
                                foreach (XElement e in element.Elements().First().Elements())
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

                                if (IncomingCallEvent != null)
                                    IncomingCallEvent(Codec, incomingCallArgs);

                                break;
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
                            ReceivedData(this, new CodecFeedbackServerReceiveEventArgs(path, element));
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
        public CodecFeedbackServerReceiveEventArgs(string path, XElement element)
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
        public CodecIncomingCallEventArgs()
        {

        }

        public string RemoteURI;
        public string DisplayNameValue;
        public Call Call;
    }
}