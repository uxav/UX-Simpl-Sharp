using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
{
    public class SoundWebMixer : SoundWebMultiChannelObject
    {
        public SoundWebMixer(SoundWeb device, string address, uint channelCount)
            : base(device, address)
        {
            for (uint c = 0; c < channelCount; c++)
            {
                this.channels.Add(new SoundWebMixerChannel(this, c + 1));
            }
        }

        public SoundWebMixerChannel this[uint channel]
        {
            get
            {
                return this.channels[(int)channel - 1] as SoundWebMixerChannel;
            }
        }
    }
}