using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebMixerChannel : SoundWebChannel
    {
        public SoundWebMixerChannel(SoundWebMixer owner, uint index)
            : base(owner, index)
        {

        }

        public override void Send(string messageType, SoundWebChannelParamType paramType, string value)
        {
            ushort pVal = Convert.ToUInt16(((this.Index - 1) * 100) + (int)paramType);
            byte upper = (byte)(pVal >> 8);
            byte lower = (byte)(pVal & 0xff);
            this.Owner.Send(messageType, string.Format("{0}{1}", (char)upper, (char)lower), value);
        }

        protected override uint GetChannelFromParamID(int paramID)
        {
            return (uint)(paramID / 100) + 1;
        }

        protected override SoundWebChannelParamType GetSoundWebChannelParamType(int paramID)
        {
            return (SoundWebChannelParamType)(paramID % 100);
        }
    }
}