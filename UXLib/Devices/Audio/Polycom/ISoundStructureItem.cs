using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Polycom
{
    public interface ISoundstructureItem : IVolumeDevice
    {
        Soundstructure Device { get; }
        bool SupportsFader { get; }
        double Fader { get; set; }
        double FaderMin { get; }
        double FaderMax { get; }
        void Init();
        bool Initialised { get; }
        event SoundstructureItemFaderChangeEventHandler FaderChanged;
        event SoundstructureItemMuteChangeEventHandler MuteChanged;
    }

    public delegate void SoundstructureItemFaderChangeEventHandler(ISoundstructureItem item, SoundstructureItemFaderChangeEventArgs args);

    public class SoundstructureItemFaderChangeEventArgs : EventArgs
    {
        public SoundstructureItemFaderChangeEventArgs(double fader, double faderMin, double faderMax, ushort faderScaled)
        {
            this.FaderValue = fader;
            this.FaderMinValue = faderMin;
            this.FaderMaxValue = faderMax;
            this.FaderScaledValue = faderScaled;
        }

        public double FaderValue;
        public double FaderMinValue;
        public double FaderMaxValue;
        public ushort FaderScaledValue;
    }

    public delegate void SoundstructureItemMuteChangeEventHandler(ISoundstructureItem item, bool muteValue);
}