using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Relays;
using UXLib.Devices;

namespace UXLib.Devices.Displays
{
    public class ElectricScreen : IHoistDevice
    {
        protected ElectricScreen(UpDownRelays relays)
        {
            this.relays = relays;
        }

        protected ElectricScreen(UpDownRelays relays, DisplayDevice display)
            : this(relays)
        {
            this.display = display;
            this.display.PowerStatusChange += new DevicePowerStatusEventHandler(display_PowerStatusChange);
        }

        void display_PowerStatusChange(IDeviceWithPower device, DevicePowerStatusEventArgs args)
        {
            if (args.PreviousPowerStatus == DevicePowerStatus.PowerOff && (
                args.NewPowerStatus == DevicePowerStatus.PowerOn || args.NewPowerStatus == DevicePowerStatus.PowerWarming))
            {
                this.Down();
            }
            else if (args.PreviousPowerStatus == DevicePowerStatus.PowerOn && (
                args.NewPowerStatus == DevicePowerStatus.PowerOff || args.NewPowerStatus == DevicePowerStatus.PowerCooling))
            {
                this.Up();
            }
        }

        UpDownRelays relays;
        DisplayDevice display;

        public void Up()
        {
            this.relays.Up();
            this.CurrentPosition = HoistDevicePosition.Up;
        }

        public void Down()
        {
            this.relays.Down();
            this.CurrentPosition = HoistDevicePosition.Down;
        }

        public HoistDevicePosition CurrentPosition { get; protected set; }
    }
}