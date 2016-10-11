using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.Devices.Audio.Revolabs
{
    public class RevolabsExecutiveElite : ICommDevice
    {
        public RevolabsExecutiveElite(ComPort comPort)
        {
            this.CommunicationType = CommDeviceType.Serial;
            ComHandler = new RevolabsComHandler(comPort);
            ComHandler.ReceivedData += new RevolabsdReceivedDataEventHandler(ComHandler_ReceivedData);
            Mics = new RevolabMicCollection(this);
            Groups = new RevolabMicGroupCollection(this);
        }

        public RevolabsExecutiveElite(string hostNameOrIPAddress)
        {
            this.CommunicationType = CommDeviceType.IP;
        }

        public  RevolabsComHandler ComHandler { get; internal set; }
        public RevolabMicCollection Mics { get; internal set; }
        public RevolabMicGroupCollection Groups { get; internal set; }

        void ComHandler_ReceivedData(RevolabsComHandler handler, string data)
        {
            OnReceive(data);
        }

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public void Send(string stringToSend)
        {
            this.ComHandler.Send(stringToSend);
        }

        public virtual void OnReceive(string receivedString)
        {
            if (receivedString.StartsWith("val "))
            {
                string[] parts = receivedString.Split(' ');
                if (parts.Length > 1)
                {
                    string command = parts[1];
                    if (parts[2] == "ch")
                    {
                        uint ch = uint.Parse(parts[3]);

                        if (!this.Mics.Contains(ch))
                            this.Mics[ch] = new RevolabMic(this, ch);

                        if (command == "micgroup")
                        {
                            if (!this.Groups.Contains(parts[4]))
                                this.Groups[parts[4]] = new RevolabMicCollection(this, parts[4]);

                            this.Groups[parts[4]].Add(ch, this.Mics[ch]);
                        }
                        else
                            this.Mics[ch].UpdateValue(command, parts[4]);
                    }
                }
            }
            else if (receivedString.StartsWith("notify "))
            {
                string[] parts = receivedString.Split(' ');
                if (parts.Length > 1)
                {
                    string command = parts[1];

                    if (command == "mute_group_member_change")
                    {
                        UpdateGroups();
                    }
                    else if (command == "mute_group_change")
                    {
                        UpdateGroups();
                    }
                    else
                    {
                        Regex r = new Regex(@"ch(\d) (\S*)");
                        foreach (Match match in r.Matches(receivedString))
                        {
                            uint ch = uint.Parse(match.Groups[1].Value);
                            if (this.Mics.Contains(ch))
                            {
                                this.Mics[ch].NotifyValue(command, match.Groups[2].Value);
                            }
                        }
                    }
                }
            }
        }

        void UpdateGroups()
        {
            for (uint mic = 1; mic <= 8; mic++)
            {
                this.Send(string.Format("get micgroup ch {0}", mic));
            }
        }

        public void Initialize()
        {
            this.ComHandler.Initialize();
            this.ComHandler.Send("regnotify");

            for (uint mic = 1; mic <= 8; mic++)
            {
                this.Send(string.Format("get micstatus ch {0}", mic));
            }
        }

        public CommDeviceType CommunicationType
        {
            get;
            protected set;
        }

        #endregion
    }
}