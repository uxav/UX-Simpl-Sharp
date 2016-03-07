using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices
{
    public interface IDeviceWithPower
    {
        bool Power { get; set; }
        bool RequestedPower { get; }
        DevicePowerStatus PowerStatus { get; }
        void OnPowerStatusChange(DevicePowerStatus newPowerStatus, DevicePowerStatus previousPowerStatus);
        event DevicePowerStatusEventHandler PowerStatusChange;
    }

    public delegate void DevicePowerStatusEventHandler(IDeviceWithPower device, DevicePowerStatusEventArgs args);

    public class DevicePowerStatusEventArgs : EventArgs
    {
        public DevicePowerStatus NewPowerStatus;
        public DevicePowerStatus PreviousPowerStatus;

        public DevicePowerStatusEventArgs(DevicePowerStatus newPowerStatus, DevicePowerStatus previousPowerStatus)
        {
            NewPowerStatus = newPowerStatus;
            PreviousPowerStatus = previousPowerStatus;
        }
    }

    public enum DevicePowerStatus
    {
        PowerOff,
        PowerOn,
        PowerWarming,
        PowerCooling
    }
}