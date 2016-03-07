using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.Polycom
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

            this.Device.Socket.Get(this, SoundstructureCommandType.FADER);
            this.Device.Socket.Get(this, SoundstructureCommandType.MUTE);
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

        public int Count()
        {
            return this.VirtualChannels.Count();
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
                        break;
                    case SoundstructureCommandType.FADER:
                        _faderValue = args.Value;
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