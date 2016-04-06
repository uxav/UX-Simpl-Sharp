using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Polycom
{
    public class VirtualChannelGroup : ISoundstructureItem, IVolumeDevice, IEnumerable<ISoundstructureItem>
    {
        public VirtualChannelGroup(Soundstructure device, string name, List<ISoundstructureItem> fromChannels)
        {
            this.Device = device;
            this.Device.ValueChange += new SoundstructureValueChangeHandler(Device_ValueChange);
            this.Name = name;
            this.VirtualChannels = new SoundstructureItemCollection(fromChannels);

#if DEBUG
            CrestronConsole.PrintLine("Received group \x22{0}\x22 with {1} channels",
                        this.Name, this.Count());
#endif
        }

        public Soundstructure Device { get; protected set; }
        public string Name { get; protected set; }
        private SoundstructureItemCollection VirtualChannels { get; set; }

        public ISoundstructureItem this[string channelName]
        {
            get
            {
                return this.VirtualChannels[channelName];
            }
        }

        public void Init()
        {
            this.Device.Socket.Get(this, SoundstructureCommandType.FADER);
            this.Device.Socket.Get(this, SoundstructureCommandType.MUTE);
        }

        public int Count()
        {
            return this.VirtualChannels.Count();
        }

        public bool ContainsMics
        {
            get
            {
                if (VirtualChannels.OfType<VirtualChannel>().Where(c => c.IsMic).Count() > 0)
                    return true;
                return false;
            }
        }

        public bool SupportsFader
        {
            get
            {
                return true;

            }
        }

        double _Fader;
        public double Fader
        {
            get
            {
                return _Fader;
            }
            set
            {
                if (this.Device.Socket.Set(this, SoundstructureCommandType.FADER, value))
                    _Fader = value;
            }
        }

        public double FaderMin { get; protected set; }
        public double FaderMax { get; protected set; }

        public event SoundstructureItemFaderChangeEventHandler FaderChanged;

        protected virtual void OnFaderChange()
        {
            if (FaderChanged != null)
                FaderChanged(this, new SoundstructureItemFaderChangeEventArgs(this.Fader, this.FaderMin, this.FaderMax, this.Level));

            if (VolumeChanged != null)
                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
        }

        bool _mute;

        public event SoundstructureItemMuteChangeEventHandler MuteChanged;

        protected virtual void OnMuteChange()
        {
            if (MuteChanged != null)
                MuteChanged(this, this.Mute);

            if (VolumeChanged != null)
                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
        }

        void Device_ValueChange(ISoundstructureItem item, SoundstructureValueChangeEventArgs args)
        {
            if (item == this)
            {
                switch (args.CommandType)
                {
                    case SoundstructureCommandType.MUTE:
                        _mute = Convert.ToBoolean(args.Value);
                        OnMuteChange();
                        break;
                    case SoundstructureCommandType.FADER:
                        if (args.CommandModifier == "min")
                            FaderMin = args.Value;
                        else if (args.CommandModifier == "max")
                            FaderMax = args.Value;
                        else
                            _Fader = args.Value;
                        OnFaderChange();
                        break;
                }
            }
        }

        #region IEnumerable<ISoundstructureItem> Members

        public IEnumerator<ISoundstructureItem> GetEnumerator()
        {
            return this.VirtualChannels.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IVolumeDevice Members

        public ushort Level
        {
            get
            {
                return (ushort)Soundstructure.ScaleRange(this.Fader, this.FaderMin, this.FaderMax, ushort.MinValue, ushort.MaxValue);
            }
            set
            {
                this.Fader = Soundstructure.ScaleRange(value, ushort.MinValue, ushort.MaxValue, this.FaderMin, this.FaderMax);
            }
        }

        public bool Mute
        {
            get
            {
                return _mute;
            }
            set
            {
                if (this.Device.Socket.Set(this, SoundstructureCommandType.MUTE, value))
                    _mute = value;
            }
        }

        public bool SupportsMute
        {
            get
            {
                return true;
            }
        }

        public bool SupportsLevel
        {
            get
            {
                return this.SupportsFader;
            }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }
}