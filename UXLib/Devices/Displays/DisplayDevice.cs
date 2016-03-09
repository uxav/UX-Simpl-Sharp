using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices;

namespace UXLib.Devices.Displays
{
    public class DisplayDevice : IDevice, IDeviceWithPower, ICommDevice
    {
        public string Name { get; set; }

        /// <summary>
        /// Set or get the Power for the display. Get will always return the actual power state.
        /// Get RequestedPower property to find the last set power.
        /// </summary>
        public virtual bool Power
        {
            get
            {
                if (PowerStatus == DevicePowerStatus.PowerOn
                    || PowerStatus == DevicePowerStatus.PowerWarming)
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

        public event DevicePowerStatusEventHandler PowerStatusChange;

        public virtual void OnPowerStatusChange(DevicePowerStatus newPowerStatus, DevicePowerStatus previousPowerStatus)
        {
#if DEBUG
            CrestronConsole.PrintLine("Display {0} OnPowerStatusChange(), newPowerStatus = {1}, previousPowerStatus = {2}",
                this.Name, newPowerStatus.ToString(), previousPowerStatus.ToString());
#endif
            if (PowerStatusChange != null)
                PowerStatusChange(this, new DevicePowerStatusEventArgs(newPowerStatus, previousPowerStatus));
        }

        public virtual void Send(string stringToSend)
        {
            
        }

        public virtual void OnReceive(string receivedString)
        {
            this.DeviceCommunicating = true;
        }

        private bool _deviceCommunicating;
        public bool DeviceCommunicating
        {
            get
            {
                return _deviceCommunicating;
            }
            protected set
            {
                _deviceCommunicating = value;
                if (value)
                {
                    new CTimer(CommsTimeout, 10000);
                }
            }
        }

        void CommsTimeout(object obj)
        {
            this.DeviceCommunicating = false;
        }

        public virtual DevicePowerStatus PowerStatus
        {
            get
            {
                return _powerStatus;
            }
            protected set
            {
                if (value != _powerStatus)
                {
                    DevicePowerStatus previousValue = _powerStatus;
                    _powerStatus = value;
                    OnPowerStatusChange(_powerStatus, previousValue);
                }
            }
        }
        DevicePowerStatus _powerStatus;

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

        public virtual string DeviceManufacturer
        {
            get { throw new NotImplementedException(string.Format("Check that {0} overrides DeviceManufacturer property", GetType().ToString())); }
        }

        public virtual string DeviceModel
        {
            get { throw new NotImplementedException(string.Format("Check that {0} overrides DeviceModel property", GetType().ToString())); }
        }

        public virtual string DeviceSerialNumber
        {
            get { throw new NotImplementedException(string.Format("Check that {0} overrides DeviceSerialNumber property", GetType().ToString())); }
        }
    }

    public delegate void DisplayDevicePowerStatusChangeEventHandler(DisplayDevice device, DevicePowerStatusEventArgs args);

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