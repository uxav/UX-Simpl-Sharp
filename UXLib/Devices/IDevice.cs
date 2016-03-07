using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices
{
    public interface IDevice
    {
        string Name { get; set; }
        string DeviceManufacturer { get; }
        string DeviceModel { get; }
        string DeviceSerialNumber { get; }
    }
}