using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public interface ISoundstructureItem
    {
        Soundstructure Device { get; }
        string Name { get; }
        bool SupportsFader { get; }
        double Fader { get; set; }
        double FaderMin { get; }
        double FaderMax { get; }
        bool SupportsMute { get; }
        bool Mute { get; set; }
        void Init();
    }
}