using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Displays
{
    public class DisplayDevice
    {
        public DisplayDevice()
        {

        }

        public DisplayDevice(UXLib.Relays.UpDownRelays relays)
        {
            this.relays = relays;
        }

        UXLib.Relays.UpDownRelays relays;

        public bool SupportsScreenControl
        {
            get
            {
                if (this.relays != null)
                    return true;
                return false;
            }
        }

        public void ScreenDown()
        {
            if (this.SupportsScreenControl)
            {
                relays.Down();
            }
        }

        public void ScreenUp()
        {
            if (this.SupportsScreenControl)
            {
                relays.Up();
            }
        }

        public virtual void Connect()
        {

        }

        public virtual void Disconnect()
        {

        }

        /// <summary>
        /// Set or get the Power for the display. Get will always return the actual power state.
        /// Get RequestedPower property to find the last set power.
        /// </summary>
        public virtual bool Power
        {
            get
            {
                if (PowerStatus == DisplayDevicePowerStatus.PowerOn
                    || PowerStatus == DisplayDevicePowerStatus.PowerWarming)
                    return true;
                else
                    return false;
            }
            set
            {
                RequestedPower = value;
            }
        }

        /// <summary>
        /// Get the last requested power state for Display.Power set.
        /// </summary>
        public virtual bool RequestedPower { get; protected set; }

        public event DisplayDevicePowerStatusChangeEventHandler PowerStatusChange;

        public virtual DisplayDevicePowerStatus PowerStatus
        {
            get
            {
                return _powerStatus;
            }
            protected set
            {
                if (value != _powerStatus)
                {
                    _powerStatus = value;
                    if (PowerStatusChange != null)
                        PowerStatusChange(this, new DisplayDevicePowerStatusEventArgs(_powerStatus));
                    switch (value)
                    {
                        case DisplayDevicePowerStatus.PowerWarming: ScreenDown(); break;
                        case DisplayDevicePowerStatus.PowerCooling: ScreenUp(); break;
                    }
                }
            }
        }
        DisplayDevicePowerStatus _powerStatus;

        public virtual DisplayDeviceInput Input
        {
            get
            {
                return _input;
            }
            set
            {
                _input = value;
            }
        }
        DisplayDeviceInput _input;
    }

    public delegate void DisplayDevicePowerStatusChangeEventHandler(DisplayDevice device, DisplayDevicePowerStatusEventArgs args);

    public class DisplayDevicePowerStatusEventArgs : EventArgs
    {
        public DisplayDevicePowerStatus PowerStatus;

        public DisplayDevicePowerStatusEventArgs(DisplayDevicePowerStatus powerStatus)
        {
            PowerStatus = powerStatus;
        }
    }

    public enum DisplayDevicePowerStatus
    {
        PowerOff,
        PowerOn,
        PowerWarming,
        PowerCooling
    }

    public enum DisplayDeviceInput
    {
        HDMI1,
        HDMI2,
        HDMI3,
        HDMI4,
        DisplayPort,
        DVI,
        VGA,
        RGBHV,
        Composite,
        YUV
    }
}