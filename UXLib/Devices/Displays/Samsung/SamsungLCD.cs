using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Displays.Samsung
{
    public class SamsungLCD : DisplayDevice
    {
        public SamsungLCD(string name)
        {
            this.Name = name;
        }

        public override string DeviceManufacturer
        {
            get { return "Samsung"; }
        }

        string _model = "Samsung LCD";
        public override string DeviceModel
        {
            get { return _model; }
        }
    }
}