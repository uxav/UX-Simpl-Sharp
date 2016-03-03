using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
{
    public class SoundWebNInputGainChannel : SoundWebChannel
    {
        public SoundWebNInputGainChannel(SoundWebNInputGain owner, uint index)
            : base(owner, index)
        {

        }

        public override void Send(string messageType, SoundWebChannelParamType paramType, string value)
        {
            ushort pVal = Convert.ToUInt16(32 * (int)paramType + (this.Index - 1));
            byte upper = (byte)(pVal >> 8);
            byte lower = (byte)(pVal & 0xff);
            this.Owner.Send(messageType, string.Format("{0}{1}", (char)upper, (char)lower), value);
        }

        protected override uint GetChannelFromParamID(int paramID)
        {
            return (uint)(paramID % 32) + 1;
        }

        protected override SoundWebChannelParamType GetSoundWebChannelParamType(int paramID)
        {
            return (SoundWebChannelParamType)(paramID / 32);
        }
    }
}