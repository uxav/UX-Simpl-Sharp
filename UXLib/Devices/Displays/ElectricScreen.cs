using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices.Relays;
using UXLib.Devices;

namespace UXLib.Devices.Displays
{
    public class ElectricScreen : IHoistDevice
    {
        protected ElectricScreen(UpDownRelays relays)
        {
            _relays = relays;
        }

        protected ElectricScreen(UpDownRelays relays, IDeviceWithPower display)
            : this(relays)
        {
            display.PowerStatusChange += display_PowerStatusChange;
        }

        void display_PowerStatusChange(IDeviceWithPower device, DevicePowerStatusEventArgs args)
        {
            if (args.PreviousPowerStatus == DevicePowerStatus.PowerOff && (
                args.NewPowerStatus == DevicePowerStatus.PowerOn || args.NewPowerStatus == DevicePowerStatus.PowerWarming))
            {
                Down();
            }
            else if (args.PreviousPowerStatus == DevicePowerStatus.PowerOn && (
                args.NewPowerStatus == DevicePowerStatus.PowerOff || args.NewPowerStatus == DevicePowerStatus.PowerCooling))
            {
                Up();
            }
        }

        private readonly UpDownRelays _relays;

        public void Up()
        {
            _relays.Up();
            CurrentPosition = HoistDevicePosition.Up;
        }

        public void Down()
        {
            _relays.Down();
            CurrentPosition = HoistDevicePosition.Down;
        }

        public HoistDevicePosition CurrentPosition { get; protected set; }
    }
}