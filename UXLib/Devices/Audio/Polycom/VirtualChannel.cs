using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
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

        public void Init()
        {
            if (this.SupportsFader)
                this.Device.Socket.Get(this, SoundstructureCommandType.FADER);
            if (this.SupportsMute)
                this.Device.Socket.Get(this, SoundstructureCommandType.MUTE);
        }

        public bool SupportsFader
        {
            get
            {
                switch (this.PhysicalChannelType)
                {
                    case SoundstructurePhysicalChannelType.SR_MIC_IN:
                    case SoundstructurePhysicalChannelType.CR_MIC_IN:
                    case SoundstructurePhysicalChannelType.CR_LINE_OUT:
                    case SoundstructurePhysicalChannelType.SR_LINE_OUT:
                    case SoundstructurePhysicalChannelType.PSTN_IN:
                    case SoundstructurePhysicalChannelType.PSTN_OUT:
                    case SoundstructurePhysicalChannelType.VOIP_IN:
                    case SoundstructurePhysicalChannelType.VOIP_OUT:
                    case SoundstructurePhysicalChannelType.CLINK_OUT:
                    case SoundstructurePhysicalChannelType.CLINK_IN:
                    case SoundstructurePhysicalChannelType.SUBMIX:
                        return true;
                }
                return false;
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
                    if (value <= FaderMax && value >= FaderMin)
                        _faderValue = value;
                }
            }
        }

        public double FaderMin { get; protected set; }
        public double FaderMax { get; protected set; }

        public bool SupportsMute
        {
            get
            {
                switch (this.PhysicalChannelType)
                {
                    case SoundstructurePhysicalChannelType.SR_MIC_IN:
                    case SoundstructurePhysicalChannelType.CR_MIC_IN:
                    case SoundstructurePhysicalChannelType.CR_LINE_OUT:
                    case SoundstructurePhysicalChannelType.SR_LINE_OUT:
                    case SoundstructurePhysicalChannelType.PSTN_IN:
                    case SoundstructurePhysicalChannelType.PSTN_OUT:
                    case SoundstructurePhysicalChannelType.VOIP_IN:
                    case SoundstructurePhysicalChannelType.VOIP_OUT:
                    case SoundstructurePhysicalChannelType.CLINK_OUT:
                    case SoundstructurePhysicalChannelType.CLINK_IN:
                    case SoundstructurePhysicalChannelType.SUBMIX:
                    case SoundstructurePhysicalChannelType.SIG_GEN:
                        return true;
                }
                return false;
            }
        }

        public bool IsMic
        {
            get
            {
                switch (this.PhysicalChannelType)
                {
                    case SoundstructurePhysicalChannelType.SR_MIC_IN:
                    case SoundstructurePhysicalChannelType.CR_MIC_IN:
                        return true;
                }
                return false;
            }
        }

        public bool IsVoip
        {
            get
            {
                switch (this.PhysicalChannelType)
                {
                    case SoundstructurePhysicalChannelType.VOIP_IN:
                    case SoundstructurePhysicalChannelType.VOIP_OUT:
                        return true;
                }
                return false;
            }
        }

        public bool IsPSTN
        {
            get
            {
                switch (this.PhysicalChannelType)
                {
                    case SoundstructurePhysicalChannelType.PSTN_IN:
                    case SoundstructurePhysicalChannelType.PSTN_OUT:
                        return true;
                }
                return false;
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
            try
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
            catch (Exception e)
            {
                ErrorLog.Error("VirtualChannel Error in Device_ValueChange(): {0}", e.Message);
            }
        }
    }
}