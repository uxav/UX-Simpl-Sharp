using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class SoundstructureEthernetSettings
    {
        public SoundstructureEthernetSettings(string fromValueString)
        {
            try
            {
                string info = fromValueString;
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
                        case "dns":
                            if (value.Contains(' '))
                                foreach (string d in value.Split(' '))
                                    _DNS.Add(d);
                            else
                                _DNS.Add(value);
                            break;
                        default:
                            if (paramName == "mode")
                            {
                                if (value == "dhcp")
                                    DHCPEnabled = true;
                                else
                                    DHCPEnabled = false;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error parsing {0} information, {1}", this.GetType(), e.Message);
            }
        }

        public string IPAddress { get; protected set; }
        public string SubnetMask { get; protected set; }
        public string Gateway { get; protected set; }
        public bool DHCPEnabled { get; protected set; }
        List<string> _DNS = new List<string>();
        public ReadOnlyCollection<string> DNS
        {
            get
            {
                return new ReadOnlyCollection<string>(_DNS);
            }
        }
    }
}