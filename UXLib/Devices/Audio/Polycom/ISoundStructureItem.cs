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
        double Fader { get; set; }
        bool Mute { get; set; }
    }
}