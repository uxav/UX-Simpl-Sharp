using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebNInputGain : SoundWebMultiChannelObject
    {
        public SoundWebNInputGain(SoundWeb device, string address, uint channelCount)
            : base(device, address)
        {
            for (uint c = 0; c < channelCount; c++)
            {
                this.channels.Add(new SoundWebNInputGainChannel(this, c + 1));
            }
        }

        public SoundWebNInputGainChannel this[uint channel]
        {
            get
            {
                return this.channels[(int)channel - 1] as SoundWebNInputGainChannel;
            }
        }
    }
}