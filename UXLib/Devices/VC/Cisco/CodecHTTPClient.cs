using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Devices.VC.Cisco
{
    public class CodecHTTPClient
    {
        public string Host;
        Dictionary<string, string> Cookies;
        string UserName;
        string Password;

        public CodecHTTPClient(string host, string username, string password)
        {
            this.Host = host;
            UserName = username;
            Password = password;
            Cookies = new Dictionary<string, string>();
        }

        HttpClient _HttpClient;
        private HttpClient HttpClient
        {
            get
            {
                if (_HttpClient == null)
                {
                    _HttpClient = new HttpClient();
                    _HttpClient.UseConnectionPooling = true;
                }

                if (_HttpClient.ProcessBusy)
                {
#if DEBUG
                    CrestronConsole.PrintLine("** {0}.HttpClient is busy, creating a new instance", this.GetType().Name);
#endif
                    return new HttpClient();
                }

                return _HttpClient;
            }
        }

        HttpClientResponse Request(HttpClientRequest request)
        {
#if DEBUG
            CrestronConsole.PrintLine("\r\nHttp ({0}) Request to: {1} Path: {2}", request.RequestType.ToString(), request.Url.Hostname, request.Url.PathAndParams);
#endif
            if (this.Cookies.ContainsKey("SessionId") && this.Cookies["SessionId"].Length > 0)
            {
                request.Header.AddHeader(new HttpHeader("Cookie", "SessionId=" + this.Cookies["SessionId"]));
            }
            else
            {
                string auth = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(this.UserName + ":" + this.Password));
                request.Header.AddHeader(new HttpHeader("Authorization", "Basic " + auth));
            }

            request.KeepAlive = false;

#if DEBUG
            foreach (HttpHeader item in request.Header)
            {
                CrestronConsole.PrintLine(item.Name + ": " + item.Value);
            }
            if (request.RequestType == RequestType.Post)
                CrestronConsole.PrintLine("Request Body:\r\n{0}", request.ContentString);
#endif
            HttpClientResponse response = null;
            
#if DEBUG
            CrestronConsole.PrintLine("Dispatching request....");
#endif
            try
            {
                response = this.HttpClient.Dispatch(request);
#if DEBUG
                CrestronConsole.PrintLine("{0} Response status {1}", request.Url.PathAndParams, response.Code);
                foreach (HttpHeader item in response.Header)
                {
                    CrestronConsole.PrintLine(item.Name + ": " + item.Value);
                }
#endif
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("{0}.HttpClient.Dispatch(request) error, {1}", this.GetType().Name, e.Message);
                throw e;
            }

            if (response.Code == 200 && response.Header != null
                && response.Header.ContainsHeaderValue("Content-Type") && response.Header["Content-Type"].Value.Contains("text/html")
                && this.Cookies.ContainsKey("SessionId"))
            {
#if DEBUG
                CrestronConsole.PrintLine("Getting new session id as response was not as expeected");
#endif
                this.Cookies.Clear();
                this.StartSession();
                return Request(request);
            }

            if (response != null && response.Code >= 400)
            {
                ErrorLog.Error("CiscoCodec.HttpClient.Request Response.Code = {0}", response.Code);
            }
            else if (response == null)
            {
                ErrorLog.Error("CiscoCodec.HttpClient.Request Response == null");
            }

            return response;
        }

        HttpClientResponse Get(string path)
        {
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(string.Format("http://{0}:80{1}", this.Host, path.StartsWith("/") ? path : "/" + path));
            return this.Request(request);
        }

        HttpClientResponse Post(string path)
        {
            return this.Post(path, string.Empty);
        }

        HttpClientResponse Post(string path, string content)
        {
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(string.Format("http://{0}:80{1}", this.Host, path.StartsWith("/") ? path : "/" + path));
            request.RequestType = RequestType.Post;
            if (content.Length > 0)
            {
                request.Encoding = Encoding.UTF8;
                request.Header.AddHeader(new HttpHeader("content-type", "text/xml"));
                request.ContentString = content;
            }
            return this.Request(request);
        }

        internal string StartSession()
        {
            HttpClientResponse response = null;
            try
            {
                this.Cookies.Clear();
                response = this.Post("/xmlapi/session/begin");
            }
            catch (Exception e)
            {
                throw e;
            }

            try
            {
                Regex r = new Regex(@"(.*?)=(.*?)(?:;|,(?!\s))");
                foreach (Match match in r.Matches(response.Header["Set-Cookie"].Value))
                {
                    Cookies[match.Groups[1].Value] = match.Groups[2].Value;
                }
                if (Cookies.ContainsKey("SessionId"))
                {
                    return Cookies["SessionId"];
                }
                else
                {
                    ErrorLog.Warn("CodecHTTPClient did not get a SessionId");
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("{0}.StartSession() Error with processing session response, {1}", this.GetType().Name, e.Message);
                return string.Empty;
            }
        }

        public bool HasSessionKey
        {
            get
            {
                return (this.Cookies.ContainsKey("SessionId") && this.Cookies["SessionId"].Length > 0);
            }
        }

        XDocument PutXML(string xmlString)
        {
            HttpClientResponse response;
            response = this.Post("/putxml", xmlString);
            string reply = response.ContentString;
            return XDocument.Load(new XmlReader(reply));
        }

        XDocument GetXML(string path)
        {
            HttpClientResponse response;
            response = Get(string.Format("/getxml?location={0}", path));
            string reply = response.ContentString;
            return XDocument.Load(new XmlReader(reply));
        }

        public XDocument SendCommand(string path)
        {
            return this.SendCommand(path, new CommandArgs());
        }

        public XDocument SendCommand(string path, CommandArgs args)
        {
            try
            {
                string pathToSend = "Command";
                foreach (string pathElement in path.Split('/'))
                {
                    if (pathElement.Length > 0 && pathElement != pathToSend)
                        pathToSend = pathToSend + "/" + pathElement;
                }

                return this.PutXML(BuildCommand(pathToSend, args));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.SendCommand(), path = {1}, {2}", this.GetType().Name, path, e.Message);
                throw e;
            }
        }

        public XDocument SendConfiguration(string path, CommandArgs args)
        {
            try
            {
                string pathToSend = "Configuration";
                foreach (string pathElement in path.Split('/'))
                {
                    if (pathElement.Length > 0 && pathElement != pathToSend)
                        pathToSend = pathToSend + "/" + pathElement;
                }

                return this.PutXML(BuildCommand(pathToSend, args));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.SendConfiguration(), path = {1}, {2}", this.GetType().Name, path, e.Message);
                throw e;
            }
        }

        public IEnumerable<XElement> RequestPath(string path)
        {
            XDocument xml = null;
            try
            {
                string pathToSend = "";
                foreach (string pathElement in path.Split('/'))
                {
                    if (pathElement.Length > 0 && pathToSend.Length > 0)
                        pathToSend = pathToSend + "/" + pathElement;
                    else if (pathElement.Length > 0)
                        pathToSend = pathElement;
                }

                string[] pathElements = pathToSend.Split('/');
                xml = GetXML(pathToSend);

                XElement element = xml.Root;
                if (element.XName.LocalName == "EmptyResult")
                    return null;

                foreach (string word in pathElements)
                {
                    if (element.XName.LocalName != word && word != pathElements.Last())
                        element = element.Elements().Where(x => x.XName.LocalName == word).FirstOrDefault();
                }

                if (element.Elements().Where(x => x.XName.LocalName == pathElements.Last()).Count() > 0)
                    return element.Elements().Where(x => x.XName.LocalName == pathElements.Last());

                return xml.Elements();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.RequestPath(), path = {1}, {2}", this.GetType().Name, path, e.Message);
#if DEBUG
                CrestronConsole.PrintLine("Error in {0}.RequestPath(), path = {1}, {2}", this.GetType().Name, path, e.Message);
                CrestronConsole.Print("Response = \r\n{0}", xml.ToString());
#endif
                throw e;
            }
        }

        public static string BuildCommand(string path, CommandArgs args)
        {
            StringWriterWithEncoding xml = new StringWriterWithEncoding(Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;

            using (XmlWriter xw = new XmlWriter(xml, settings))
            {
                xw.WriteStartDocument();

                foreach (string pathElements in path.Split('/'))
                {
                    if(pathElements.Length > 0)
                        xw.WriteStartElement(pathElements);
                }

                xw.WriteAttributeString("command", "True");

                foreach (CommandArg arg in args)
                {
                    xw.WriteStartElement(arg.Name);
                    if (arg.ItemIndex > 0)
                        xw.WriteAttributeString("item", arg.ItemIndex.ToString());
                    xw.WriteValue(arg.Value);
                    xw.WriteEndElement();
                }

                xw.WriteEndDocument();
            }

            return xml.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"");
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }

    public class CommandArg
    {
        public string Name;
        public string Value;
        public int ItemIndex;

        public CommandArg(string name, string value)
        {
            this.Name = name;
            this.Value = value;
            this.ItemIndex = 0;
        }

        public CommandArg(string name, int value)
        {
            this.Name = name;
            this.Value = value.ToString();
            this.ItemIndex = 0;
        }

        public CommandArg(string name, int itemIndex, string value)
        {
            this.Name = name;
            this.ItemIndex = itemIndex;
            this.Value = value;
        }
    }

    public class CommandArgs : IEnumerable<CommandArg>
    {
        List<CommandArg> commands;

        public CommandArgs()
        {
            commands = new List<CommandArg>();
        }

        public CommandArgs(CommandArg arg)
        {
            commands = new List<CommandArg>();
            commands.Add(arg);
        }

        public CommandArgs(string name, string value)
        {
            commands = new List<CommandArg>();
            commands.Add(new CommandArg(name, value));
        }

        public CommandArgs(string name, int value)
        {
            commands = new List<CommandArg>();
            commands.Add(new CommandArg(name, value));
        }

        public CommandArgs(string name, int itemindex, string value)
        {
            commands = new List<CommandArg>();
            commands.Add(new CommandArg(name, itemindex, value));
        }

        public CommandArg this[int index]
        {
            get
            {
                return commands[index];
            }
        }

        public CommandArg this[string name]
        {
            get
            {
                return commands.Where(a => a.Name == name).FirstOrDefault();
            }
        }

        public CommandArgs Add(string name, string value)
        {
            commands.Add(new CommandArg(name, value));
            return this;
        }

        public CommandArgs Add(string name, int value)
        {
            commands.Add(new CommandArg(name, value));
            return this;
        }

        public CommandArgs Add(string name, int itemIndex, string value)
        {
            commands.Add(new CommandArg(name, itemIndex, value));
            return this;
        }

        public CommandArgs Add(CommandArg arg)
        {
            commands.Add(arg);
            return this;
        }

        public bool ContainsArg(string name)
        {
            foreach (CommandArg arg in this)
                if (arg.Name == name)
                    return true;
            return false;
        }

        public IEnumerator<CommandArg> GetEnumerator()
        {
            return this.commands.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}