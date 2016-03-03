using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
{
    public class SoundWebMultiChannelObject : SoundWebObject
    {
        public SoundWebMultiChannelObject(SoundWeb device, string address)
        {
            this.Device = device;
            HiQAddress = address;
            this.channels = new List<SoundWebChannel>();
        }

        public int ChannelCount
        {
            get
            {
                return this.channels.Count();
            }
        }

        protected List<SoundWebChannel> channels { get; set; }
    }
}