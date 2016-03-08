using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VirtualChannelGroup : ISoundstructureItem, IEnumerable<ISoundstructureItem>
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

        public bool SupportsFader
        {
            get
            {
                return true;

            }
        }

        double _faderValue;
        public double Fader
        {
            get
            {
                return _faderValue;
            }
            set
            {
                if (this.Device.Socket.Set(this, SoundstructureCommandType.FADER, value))
                    _faderValue = value;
            }
        }

        public double FaderMin { get; protected set; }
        public double FaderMax { get; protected set; }

        public bool SupportsMute
        {
            get
            {
                return true;
            }
        }

        bool _mute;
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

        void Device_ValueChange(ISoundstructureItem item, SoundstructureValueChangeEventArgs args)
        {
            if (item == this)
            {
                switch (args.CommandType)
                {
                    case SoundstructureCommandType.MUTE:
                        _mute = Convert.ToBoolean(args.Value);
#if DEBUG
                        CrestronConsole.PrintLine("{0} Mute = {1}", this.Name, _mute);
#endif
                        break;
                    case SoundstructureCommandType.FADER:
                        if (args.CommandModifier == "min")
                            FaderMin = args.Value;
                        else if (args.CommandModifier == "max")
                            FaderMax = args.Value;
                        else
                        {
                            _faderValue = args.Value;
#if DEBUG
                            CrestronConsole.PrintLine("{0} Fader = {1:0.00}", this.Name, this.Fader);
#endif
                        }
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
    }
}