using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Displays
{
    public class DisplayDevice
    {
        public DisplayDevice(string name)
        {
            this.Name = name;
        }

        public DisplayDevice(string name, ElectricScreen screen)
        {
            this.Name = name;
            this.Screen = screen;
            this.AutoSetScreen = true;
        }

        public DisplayDevice(string name, ElectricScreen screen, bool autoSetScreen)
        {
            this.Name = name;
            this.Screen = screen;
            this.AutoSetScreen = autoSetScreen;
        }

        public string Name { get; protected set; }
        public ElectricScreen Screen { get; protected set; }
        public bool AutoSetScreen { get; set; }

        public bool SupportsScreenControl
        {
            get
            {
                if (this.Screen != null)
                    return true;
                return false;
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
                    if (this.SupportsScreenControl && this.AutoSetScreen)
                    {
                        switch (value)
                        {
                            case DisplayDevicePowerStatus.PowerWarming: Screen.Down(); break;
                            case DisplayDevicePowerStatus.PowerCooling: Screen.Up(); break;
                        }
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