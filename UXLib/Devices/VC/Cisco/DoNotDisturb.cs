using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class DoNotDisturb
    {
        internal DoNotDisturb(Conference conference)
        {
            Codec = conference.Codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
        }

        internal CiscoCodec Codec { get; set; }

        /// <summary>
        /// Activate the do not disturb option on the codec
        /// </summary>
        public void Activate()
        {
            Codec.SendCommand("Conference/DoNotDisturb/Activate");
            Active = true;
        }

        /// <summary>
        /// Activate the do not disturb option on the codec
        /// </summary>
        /// <param name="timeout">Timeout in minutes after which the codec will exit Do Not Disturb mode</param>
        public void Activate(int timeout)
        {
            CommandArgs args = new CommandArgs("Timeout", timeout);
            Codec.SendCommand("Conference/DoNotDisturb/Activate", args);
            Active = true;
        }

        /// <summary>
        /// Deactivate the do not disturb mode on the codec
        /// </summary>
        public void Deactivate()
        {
            Codec.SendCommand("Conference/DoNotDisturb/Deactivate");
            Active = false;
        }

        bool _Active;
        public bool Active
        {
            get { return _Active; }
            protected set
            {
                if (_Active != value)
                {
                    _Active = value;
                    if (StatusChanged != null)
                        StatusChanged(Codec, value);
                }
            }
        }

        public event DoNotDisturbChangeEventHandler StatusChanged;

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            if (args.Path == "Status/Conference" && args.Data.Elements().First().XName.LocalName == "DoNotDisturb")
            {
                Active = (args.Data.Elements().First().Value == "Active");
            }
        }
    }

    public delegate void DoNotDisturbChangeEventHandler(CiscoCodec codec, bool statusActive);
}