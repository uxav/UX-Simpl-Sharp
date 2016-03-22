using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Devices.VC.Cisco
{
    public class CodecSSHClient : SshClient
    {
        public CodecSSHClient(KeyboardInteractiveConnectionInfo info)
            : base(info)
        {
            Host = info.Host;
            this.ErrorOccurred += new EventHandler<ExceptionEventArgs>(CodecSSHClient_ErrorOccurred);
        }

        string Host;
        private ShellStream Stream;
        private CrestronQueue<string> RxQueue = new CrestronQueue<string>();
        private CrestronQueue<XDocument> DataQueue = new CrestronQueue<XDocument>();
        private Thread RxHandler;
        private CTimer KeepAliveTimer;

        public event CodecSSHClientConnectedEventHandler OnConnect;

        public new bool IsConnected { get; protected set; }

        protected override void OnConnected()
        {
            base.OnConnected();
            this.IsConnected = true;

            Stream = this.CreateShellStream("", 80, 24, 200, 300, 1024);
            if (RxHandler == null || RxHandler.ThreadState != Thread.eThreadStates.ThreadRunning)
                RxHandler = new Thread(ProcessRxData, null, Thread.eThreadStartOptions.Running);
            Stream.DataReceived += new EventHandler<Crestron.SimplSharp.Ssh.Common.ShellDataEventArgs>(Stream_DataReceived);
            Stream.WriteLine("xPreferences outputmode xml");
            KeepAliveTimer = new CTimer(SendKeepAlive, null, 60000, 60000);
            ErrorLog.Notice("Cisco codec {0} connected by SSH on {1}", Host, this.EthernetAdapter.ToString());

            if (this.OnConnect != null)
            {
                OnConnect(this);
            }
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            this.IsConnected = false;
            KeepAliveTimer.Stop();
            KeepAliveTimer.Dispose();
            RxQueue.Enqueue(null);
            ErrorLog.Notice("Cisco codec SSH Client {0} disconnected gracefully", Host);
        }

        void CodecSSHClient_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(SshConnectionException))
            {
                this.IsConnected = false;
                KeepAliveTimer.Stop();
                KeepAliveTimer.Dispose();
                RxQueue.Enqueue(null);
                ErrorLog.Error("Cisco codec SSH Client {0} connection broken!", Host);
            }
        }

        void SendKeepAlive(object callbackObject)
        {
            this.SendKeepAlive();
        }        

        XDocument Send(string s)
        {
            if (KeepAliveTimer != null)
                KeepAliveTimer.Reset();
            RxQueue.Clear();
            DataQueue.Clear();
            Stream.WriteLine(s);
            XDocument result = DataQueue.Dequeue(5000);
            if (result == null)
                ErrorLog.Error("Error in CodecSSHClient.RequestData, result == null");
            return result;
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

            return this.Send(BuildCommand(pathToSend, args));
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
            XDocument xml = Send(BuildCommand(pathToSend, new CommandArgs()));

            if (xml != null && xml.Root.HasElements)
            {
                XElement element = xml.Root.Elements().FirstOrDefault();

                foreach (string word in pathElements)
                {
                    if (element.XName.LocalName != word && word != pathElements.Last())
                        element = element.Element(word);
                }

                if(element.Elements(pathElements.Last()).Count() > 0)
                    return element.Elements(pathElements.Last());

                return xml.Root.Elements();
            }
            else
                return null;
        }

        void Stream_DataReceived(object sender, Crestron.SimplSharp.Ssh.Common.ShellDataEventArgs e)
        {
            RxQueue.Enqueue(Encoding.UTF8.GetString(e.Data, 0, e.Data.Length));
        }

        void OnReceive(string str)
        {
            if (str.Contains("Command not recognized."))
                DataQueue.Enqueue(null);
            /*else
                CrestronConsole.Print(str);*/
        }

        void OnReceive(XDocument xmlDocument)
        {
            
            DataQueue.Enqueue(xmlDocument);
        }

        object ProcessRxData(object callbackObject)
        {
            string rx = string.Empty;

            while (true)
            {
                try
                {
                    string s = RxQueue.Dequeue();

                    if (s == null) // Quit thread on null
                        return null;
                    else
                    {
                        if (s.StartsWith("<XmlDoc") && rx == "")
                            rx = s;
                        else if (rx.StartsWith("<XmlDoc"))
                            rx = rx + s;
                        else
                            OnReceive(s);

                        if (rx.Length > 0 && rx.Contains(@"</XmlDoc>"))
                        {
                            try
                            {
                                XDocument xml = XDocument.Load(new XmlReader(rx));
                                OnReceive(xml);
                                rx = string.Empty;
                            }
                            catch(Exception e)
                            {

                                ErrorLog.Error("Error loading XML document in CodecSSHClient.ProcessRxData, {0}", e.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("Error in CocecSSHClient ProcessRxData() Thread, Error: {0}", e.Message);
                }
            }
        }

        public static string BuildCommand(string path, CommandArgs args)
        {
            string result = string.Empty;

            foreach (string pathElement in path.Split('/'))
            {
                if (pathElement.Length > 0)
                {
                    if (pathElement == "Command")
                        result = "xCommand";
                    else if (pathElement == "Status")
                        result = "xStatus";
                    else if (pathElement == "Configuration")
                        result = "xConfiguration";
                    else
                        result = result + " " + pathElement;
                }
            }

            foreach (CommandArg arg in args)
            {
                result = result + " " + arg.Name + ": ";

                if (arg.Value.Contains(" "))
                    result = result + "\x22" + arg.Value + "\x22";
                else
                    result = result + arg.Value;
            }

            return result;
        }
    }

    public delegate void CodecSSHClientConnectedEventHandler(CodecSSHClient client);
}