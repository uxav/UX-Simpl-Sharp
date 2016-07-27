using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices
{
    public interface IHoistDevice
    {
        void Up();
        void Down();
        HoistDevicePosition CurrentPosition { get; }
    }

    public enum HoistDevicePosition
    {
        NotKnown,
        Up,
        Down
    }
}