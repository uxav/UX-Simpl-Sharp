using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Revolabs
{
    public class RevolabMicCollection : UXCollection<RevolabMic>, IVolumeDevice
    {
        internal RevolabMicCollection(RevolabsExecutiveElite controller)
        {
            this.Controller = controller;
            this.Name = "All Mics";
            this.AllMicsGroup = true;
            this.Controller.ComHandler.ReceivedData += new RevolabsdReceivedDataEventHandler(ComHandler_ReceivedData);
        }

        internal RevolabMicCollection(RevolabsExecutiveElite controller, string groupName)
        {
            this.Controller = controller;
            this.Name = groupName;
            this.AllMicsGroup = false;
            this.Controller.ComHandler.ReceivedData += new RevolabsdReceivedDataEventHandler(ComHandler_ReceivedData);
        }

        public string Name { get; protected set; }
        public bool AllMicsGroup { get; protected set; }
        public RevolabsExecutiveElite Controller { get; protected set; }

        internal void Rename(string newName)
        {
            this.Name = newName;
        }

        internal override void Add(uint channelNumber, RevolabMic value)
        {
            this[channelNumber] = value;
        }

        internal void Remove(uint channelNumber)
        {
            this.InternalDictionary.Remove(channelNumber);
        }

        public override RevolabMic this[uint channelNumber]
        {
            get
            {
                return base[channelNumber];
            }
            internal set
            {
                foreach (KeyValuePair<string, RevolabMicCollection> group in this.Controller.Groups)
                {
                    if (group.Value.Contains(value))
                        group.Value.Remove(value.ChannelNumber);
                }
                base[channelNumber] = value;
            }
        }

        public override bool Contains(uint channelNumber)
        {
            return base.Contains(channelNumber);
        }

        public IEnumerable<RevolabMic> Active
        {
            get
            {
                return this.InternalDictionary.Values.Where(m => m.Status == MicStatus.On);
            }
        }

        private bool _Mute;
        public bool Mute
        {
            get
            {
                return _Mute;
            }
            set
            {
                List<RevolabMic> mics = this.Active.ToList();
                if (mics.Count > 0)
                {
                    mics[0].Mute = value;
                }
            }
        }

        void ComHandler_ReceivedData(RevolabsComHandler handler, string data)
        {
            if (data.StartsWith("notify status_group_mute "))
            {
                string[] parts = data.Split(' ');

                if (parts[2] == this.Name || this.AllMicsGroup)
                {
                    bool value = (parts[3] == "0") ? false : true;
                    if (_Mute != value)
                    {
                        _Mute = value;
#if DEBUG
                        CrestronConsole.PrintLine("Mic Group {0} Mute = {1}", this.Name, this.Mute);
#endif
                        if (VolumeChanged != null)
                            VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                    }
                }
            }
        }

        #region IVolumeDevice Members

        public ushort VolumeLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool VolumeMute
        {
            get
            {
                return this.Mute;
            }
            set
            {
                this.Mute = value;
            }
        }

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public bool SupportsVolumeLevel
        {
            get { return false; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }
}