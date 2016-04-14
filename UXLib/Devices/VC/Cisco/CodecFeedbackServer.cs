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

            Codec.SendCommand("HttpFeedback/Register", args, true);
        }

        public bool Registered
        {
            get
            {
                IEnumerable<XElement> statusInfo = Codec.RequestPath("Status/HttpFeedback");
                foreach (XElement element in statusInfo)
                    if (element.Elements().Where(e => e.XName.LocalName == "URL").FirstOrDefault().Value == this.ServerURL)
                        return true;
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

        void OnReceivedData(object sender, OnHttpRequestArgs args)
        {
            try
            {
#if DEBUG
                CrestronConsole.PrintLine("\r\n{0}   New Request to {1} from {2}", DateTime.Now.ToString(), server.ServerName, args.Connection.RemoteEndPointAddress);
#endif
                XDocument xml = XDocument.Load(new XmlReader(args.Request.ContentString));
                if (xml.Root.HasAttributes)
                {
                    XNamespace ns = xml.Root.Attribute("xmlns").Value;
                    XElement identification = xml.Root.Element(ns + "Identification");
                    string productID = identification.Element(ns + "ProductID").Value;
                    identification.Remove();
                    XElement element = xml.Root;
#if DEBUG
                    CrestronConsole.PrintLine(element.ToString());
#endif           
                    string path = element.XName.LocalName;
                    while (element.Elements().Count() == 1 && element.Elements().FirstOrDefault().HasElements)
                    {
                        element = element.Elements().FirstOrDefault();
                        path = string.Format("{0}/{1}", path, element.XName.LocalName);
                    }

                    if (element == xml.Root && element.Elements().FirstOrDefault() != null)
                    {
                        element = element.Elements().FirstOrDefault();
                        path = string.Format("{0}/{1}", path, element.XName.LocalName);
                    }

                    //CrestronConsole.PrintLine("Received {0} Update from {1} for path /{2}", xml.Root.XName.LocalName, productID, path);

                    if (ReceivedData != null)
                    {
                        ReceivedData(this, new CodecFeedbackServerReceiveEventArgs(path, element));
                    }

                    return;
                }
                else
                {
                    ErrorLog.Error("XML not as expected from Codec Feedback Event");
                    return;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error on codec http feedback server: {0}", e.Message);
                
                args.Response.Code = 200;
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
}