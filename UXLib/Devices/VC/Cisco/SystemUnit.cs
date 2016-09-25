using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class SystemUnit
    {
        public SystemUnit(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
            Software = new SystemUnitSoftware(Codec);
            Hardware = new SystemUnitHardware(Codec);
            State = new SystemUnitState(Codec);
        }

        CiscoCodec Codec;

        public SystemUnitState State { get; protected set; }
        public SystemUnitSoftware Software { get; protected set; }
        public SystemUnitHardware Hardware { get; protected set; }
        public string ContactInfo { get; protected set; }
        public string ContactName { get; protected set; }
        public string ProductId { get; protected set; }
        public string ProductPlatform { get; protected set; }
        public string ProductType { get; protected set; }
        public TimeSpan Uptime { get; protected set; }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            switch (args.Path)
            {
                case @"Status/SystemUnit/Diagnostics":
                    break;
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/SystemUnit").Elements().Where(e => !e.HasElements))
            {
#if DEBUG
                CrestronConsole.PrintLine("SystemUnit.{0} = {1}", element.XName.LocalName, element.Value);
#endif
                switch (element.XName.LocalName)
                {
                    case "ContactInfo": ContactInfo = element.Value; break;
                    case "ContactName": ContactName = element.Value; break;
                    case "ProductId": ProductId = element.Value; break;
                    case "ProductPlatform": ProductPlatform = element.Value; break;
                    case "ProductType": ProductType = element.Value; break;
                    case "Uptime": Uptime = TimeSpan.FromSeconds(double.Parse(element.Value)); break;
                }
            }
        }

        public void Update()
        {
            Codec_HasConnected(this.Codec);
        }

        /// <summary>
        /// Reboot or shutdown the codec
        /// </summary>
        /// <param name="action">BootAction parameter to restart or shutdown</param>
        public void Boot(BootAction action)
        {
            Codec.SendCommand("SystemUnit/Boot", new CommandArgs("Action", action.ToString()));
        }

        /// <summary>
        /// Action parameter used for SystemUnit.Boot()
        /// </summary>
        public enum BootAction
        {
            Restart,
            Shutdown
        }
    }
}