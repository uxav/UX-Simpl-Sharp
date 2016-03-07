using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.Polycom
{
    public class VirtualChannel : ISoundstructureItem
    {
        public VirtualChannel(Soundstructure device, string name, SoundstructureVirtualChannelType vcType, SoundstructurePhysicalChannelType pcType, uint[] values)
        {
            this.Device = device;
            this.Device.ValueChange += new SoundstructureValueChangeHandler(Device_ValueChange);
            this.Name = name;
            this.VirtualChannelType = vcType;
            this.PhysicalChannelType = pcType;
            _physicalChannelIndex = new List<uint>(values);

#if DEBUG
            CrestronConsole.Print("Received channel with name: {0}, Virtual Type: {1}, Physical Type: {2} Values:",
                        this.Name, this.VirtualChannelType.ToString(), this.PhysicalChannelType.ToString());

            foreach (uint value in this.PhysicalChannelIndex)
            {
                CrestronConsole.Print(" {0}", value);
            }

            CrestronConsole.PrintLine("");
#endif

            this.Device.Socket.Get(this, SoundstructureCommandType.FADER);
            this.Device.Socket.Get(this, SoundstructureCommandType.MUTE);
        }

        public Soundstructure Device { get; protected set; }
        public string Name { get; protected set; }

        public SoundstructureVirtualChannelType VirtualChannelType { get; protected set; }
        public SoundstructurePhysicalChannelType PhysicalChannelType { get; protected set; }

        List<uint> _physicalChannelIndex;
        public ReadOnlyCollection<uint> PhysicalChannelIndex
        {
            get
            {
                return _physicalChannelIndex.AsReadOnly();
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
                {
                    _faderValue = value;
                }
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
    }
}