using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class CodecHTTPClient
    {
        public string Host;
        private HttpClient HttpClient;

        public CodecHTTPClient(string host, string username, string password)
        {
            this.Host = host;
            this.HttpClient = new HttpClient();
            this.HttpClient.UserName = username;
            this.HttpClient.Password = password;
            this.HttpClient.KeepAlive = false;
        }

        string Request(HttpClientRequest request)
        {
#if DEBUG
            CrestronConsole.PrintLine("\r\nHttp ({0}) Request to: {1} Path: {2}", request.RequestType.ToString(), request.Url.Hostname, request.Url.PathAndParams);
            if (request.RequestType == RequestType.Post)
                CrestronConsole.PrintLine("Request Body:\r\n{0}", request.ContentString);
#endif
            try
            {
                HttpClientResponse response = this.HttpClient.Dispatch(request);
#if DEBUG
                CrestronConsole.PrintLine("Response status {0}", response.Code);
#endif
                if (response.Code == 200)
                    return response.ContentString;

                else
                    ErrorLog.Error("Error dispatching request to Cisco Codec. Received response code: {0}", response.Code);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error dispatching request to Cisco Codec. Exception: {0}", e.Message);
            }

            return "";
        }

        string Get(string path)
        {
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(string.Format("http://{0}:80{1}", this.Host, path.StartsWith("/") ? path : "/" + path));
            return this.Request(request);
        }

        string Post(string path, string content)
        {
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(string.Format("http://{0}:80{1}", this.Host, path.StartsWith("/") ? path : "/" + path));
            request.RequestType = RequestType.Post;
            request.Header.AddHeader(new HttpHeader("content-type", "text/xml"));
            request.ContentString = content;
            return this.Request(request);
        }

        XDocument PutXML(string xmlString)
        {
            try
            {
                string reply = this.Post("/putxml", xmlString);
                return XDocument.Load(new XmlReader(reply));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error occured in CodecHTTPClient.PutXML() {0}", e.Message);
            }

            return null;
        }

        XDocument GetXML(string path)
        {
            string result = Get(string.Format("/getxml?location={0}", path));
            return XDocument.Load(new XmlReader(result));
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

            return xml.ToString();
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