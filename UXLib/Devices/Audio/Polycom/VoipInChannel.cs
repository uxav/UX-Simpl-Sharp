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
                    info = info.Replace("\'", "");
                    string[] infoParts = info.Split(',');
                    foreach (string part in infoParts)
                    {
                        string paramName = part.Split('=')[0];
                        string value = part.Split('=')[1];

                        switch (paramName)
                        {
                            case "addr": IPAddress = value; break;
                            case "gw": Gateway = value; break;
                            case "nm": SubnetMask = value; break;
                            default:
                                if (paramName == "mode")
                                {
                                    if (value == "dhcp")
                                        IsDHCP = true;
                                    else
                                        IsDHCP = false;
                                }
                                break;
                        }
                    }
                    break;
            }

            base.OnVoipInfoReceived(command, info);
        }

        public string IPAddress { get; protected set; }
        public bool IsDHCP { get; protected set; }
        public string Gateway { get; protected set; }
        public string SubnetMask { get; protected set; }
    }
}