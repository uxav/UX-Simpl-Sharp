using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
{
    public class SoundWebMixer : SoundWebObject
    {
        public SoundWebMixer(SoundWeb device, uint channelCount, string address)
        {
            this.Device = device;
            HiQAddress = address;
            this.channels = new List<SoundWebMixerChannel>();
            for (uint c = 0; c < channelCount; c++)
            {
                this.channels.Add(new SoundWebMixerChannel(this, c + 1));
            }
        }

        List<SoundWebMixerChannel> channels;

        public void Send(string messageType, string paramID, string value)
        {
            string str = messageType + this.HiQAddress + paramID + value;
            this.Device.Socket.Send(str);
        }

        public int ChannelCount
        {
            get
            {
                return this.channels.Count();
            }
        }

        public SoundWebMixerChannel this[uint channel]
        {
            get
            {
                return this.channels[(int)channel - 1];
            }
        }
    }
}