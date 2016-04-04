using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VoipInChannel : VoipChannel
    {
        public VoipInChannel(Soundstructure device, string name, uint[] values)
            : base(device, name, SoundstructurePhysicalChannelType.VOIP_IN, values) { }

        public override void Init()
        {
            base.Init();

            this.Device.Socket.Send(string.Format("get voip_eth_settings \"{0}\"", this.Name));
            this.Device.Socket.Send(string.Format("get voip_eth_vlan_id \"{0}\"", this.Name));
        }

        protected override void OnVoipInfoReceived(string command, string info)
        {
            switch (command)
            {
                case "voip_eth_settings":
                    info = info.Substring(1, info.Length - 2);
                    LanAdapter = new SoundstructureEthernetSettings(info);
                    break;
            }

            base.OnVoipInfoReceived(command, info);
        }

        public SoundstructureEthernetSettings LanAdapter { get; protected set; }
    }
}