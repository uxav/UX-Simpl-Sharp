using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.Polycom
{
    public class SoundstructureVirtualChannel
    {
        public SoundstructureVirtualChannel(string name, SoundstructureVirtualChannelType vcType, SoundsrtucturePhysicalChannelType pcType)
        {
            this.Name = name;
            this.VirtualChannelType = vcType;
            this.PhysicalChannelType = pcType;
        }

        public string Name { get; protected set; }

        public SoundstructureVirtualChannelType VirtualChannelType { get; protected set; }
        public SoundsrtucturePhysicalChannelType PhysicalChannelType { get; protected set; }
    }
}