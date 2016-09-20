using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronConnected;

namespace UXLib.Devices.Displays
{
    public class CrestronConnectedDisplay : DisplayDevice
    {
        public CrestronConnectedDisplay(string name, uint ipId, CrestronControlSystem controlSystem)
        {
            Display = new RoomViewConnectedDisplay(ipId, controlSystem);
            Display.BaseEvent += new BaseEventHandler(Display_BaseEvent);

            if (Display.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Could not register CrestronConnectedDisplay with IP ID {0}", ipId.ToString("X2"));
            }

            this.Name = name;
        }

        public RoomViewConnectedDisplay Display { get; protected set; }

        public override bool Power
        {
            get
            {
                return Display.PowerOnFeedback.BoolValue;
            }
            set
            {
                if (value)
                    Display.PowerOn();
                else
                    Display.PowerOff();
                base.Power = value;
            }
        }

        public override bool Blank
        {
            get
            {
                return Display.ImageMuteOnFeedback.BoolValue;
            }
            set
            {
                if (value)
                    Display.ImageMuteOn();
                else
                    Display.ImageMuteOff();
            }
        }

        public override ushort Usage
        {
            get
            {
                return Display.LampHoursFeedback.UShortValue;
            }
        }

        void Display_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            switch (args.EventId)
            {
                case RoomViewConnectedDisplay.WarmingUpFeedbackEventId:
                    if (Display.WarmingUpFeedback.BoolValue)
                        PowerStatus = DevicePowerStatus.PowerWarming;
                    else if (Display.PowerOnFeedback.BoolValue)
                    {
                        PowerStatus = DevicePowerStatus.PowerOn;
                        if (Power != RequestedPower)
                            Power = RequestedPower;
                    }
                    break;
                case RoomViewConnectedDisplay.CoolingDownFeedbackEventId:
                    if (Display.CoolingDownFeedback.BoolValue)
                        PowerStatus = DevicePowerStatus.PowerCooling;
                    else if (Display.PowerOffFeedback.BoolValue)
                    {
                        PowerStatus = DevicePowerStatus.PowerOff;
                        if (Power != RequestedPower)
                            Power = RequestedPower;
                    }
                    break;
                case RoomViewConnectedDisplay.LampHoursFeedbackEventId:
                    OnUsageChange(this.Usage);
                    break;
            }
        }

        public override string DeviceManufacturer
        {
            get { return "Crestron Connected Display"; }
        }

        public override string DeviceModel
        {
            get { return "Unknown"; }
        }

        public override string DeviceSerialNumber
        {
            get { return Display.MacAddressFeedback.StringValue; }
        }

        public override CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}