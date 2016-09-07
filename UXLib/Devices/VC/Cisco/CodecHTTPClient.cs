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
        private HttpClient HttpClient;
        Dictionary<string, string> Cookies;
        string UserName;
        string Password;

        public CodecHTTPClient(string host, string username, string password)
        {
            this.Host = host;
            this.HttpClient = new HttpClient();
            UserName = username;
            Password = password;
            this.HttpClient.KeepAlive = false;
            Cookies = new Dictionary<string, string>();
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

#if DEBUG
            foreach (HttpHeader item in request.Header)
            {
                CrestronConsole.PrintLine(item.Name + ": " + item.Value);
            }
            if (request.RequestType == RequestType.Post)
                CrestronConsole.PrintLine("Request Body:\r\n{0}", request.ContentString);
#endif

            try
            {
                if (this.HttpClient.ProcessBusy)
                {
                    Thread waitThread = new Thread(WaitForHttpClientThread, null, Thread.eThreadStartOptions.Running);
                    waitThread.Join(2000);
                }

                HttpClientResponse response = this.HttpClient.Dispatch(request);
#if DEBUG
                CrestronConsole.PrintLine("Response status {0}", response.Code);
                foreach (HttpHeader item in response.Header)
                {
                    CrestronConsole.PrintLine(item.Name + ": " + item.Value);
                }
#endif
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
#if DEBUG
                if (response.Code != 204 && response.ContentLength > 256)
                    CrestronConsole.PrintLine("Response body:\r\n{0} ...", response.ContentString.Replace("\n", "\r\n").Substring(0, 256));
                else if (response.Code != 204)
                    CrestronConsole.PrintLine("Response body:\r\n{0}", response.ContentString.Replace("\n", "\r\n"));
#endif
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
            catch (Exception e)
            {
                ErrorLog.Error("Error dispatching request to Cisco Codec. Exception: {0}", e.Message);
            }

            return null;
        }

        private object WaitForHttpClientThread(object obj)
        {
            while (this.HttpClient.ProcessBusy)
            {
                Thread.Sleep(10);
            }

            return null;
        }

        public bool Busy
        {
            get
            {
                return this.HttpClient.ProcessBusy;
            }
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

        public void StartSession()
        {
            HttpClientResponse response = this.Post("/xmlapi/session/begin");
            Regex r = new Regex(@"(.*?)=(.*?)(?:;|,(?!\s))");
            foreach (Match match in r.Matches(response.Header["Set-Cookie"].Value))
            {
                Cookies[match.Groups[1].Value] = match.Groups[2].Value;
            }
            if (Cookies.ContainsKey("SessionId"))
            {
                ErrorLog.Notice("CodecHTTPClient Received SessionId of {0}", Cookies["SessionId"]);
            }
            else
            {
                ErrorLog.Warn("CodecHTTPClient did not get a SessionId");
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
            int code = 0;

            try
            {
                response = this.Post("/putxml", xmlString);
                string reply = response.ContentString;
                code = response.Code;
                return XDocument.Load(new XmlReader(reply));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error occured in CodecHTTPClient.PutXML() {0}, response.Code = {1}", e.Message, code);
            }

            return null;
        }

        XDocument GetXML(string path)
        {
            HttpClientResponse response;
            int code = 0;

            try
            {
                response = Get(string.Format("/getxml?location={0}", path));
                string reply = response.ContentString;
                code = response.Code;
                return XDocument.Load(new XmlReader(reply));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error occured in CodecHTTPClient.PutXML() {0}, response.Code = {1}", e.Message, code);
            }

            return null;
        }

        public XDocument SendCommand(string path)
        {
            return this.SendCommand(path, new CommandArgs());
        }

        public XDocument SendCommand(string path, CommandArgs args)
        {
            string pathToSend = "Command";
            foreach (string pathElement in path.Split('/'))
            {
                if (pathElement.Length > 0 && pathElement != pathToSend)
                    pathToSend = pathToSend + "/" + pathElement;
            }

            return this.PutXML(BuildCommand(pathToSend, args));
        }

        public IEnumerable<XElement> RequestPath(string path)
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
            XDocument xml = GetXML(pathToSend);

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

        public void Add(string name, string value)
        {
            commands.Add(new CommandArg(name, value));
        }

        public void Add(string name, int value)
        {
            commands.Add(new CommandArg(name, value));
        }

        public void Add(string name, int itemIndex, string value)
        {
            commands.Add(new CommandArg(name, itemIndex, value));
        }

        public void Add(CommandArg arg)
        {
            commands.Add(arg);
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