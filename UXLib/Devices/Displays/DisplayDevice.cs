using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Devices;
using UXLib.Models;

namespace UXLib.Devices.Displays
{
    public abstract class DisplayDevice : IDevice, IDeviceWithPower, ICommDevice, IFusionStaticAsset
    {
        public virtual string Name { get; set; }

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
            {
                PowerStatusChange(this, new DevicePowerStatusEventArgs(newPowerStatus, previousPowerStatus));
                FusionUpdate();
            }
        }

        public virtual void Send(string stringToSend)
        {
            throw new NotImplementedException();
        }

        public virtual void OnReceive(string receivedString)
        {
            this.DeviceCommunicating = true;
        }

        public virtual void OnReceive(byte[] bytes)
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
                FusionUpdate();
            }
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
                FusionUpdate();
            }
        }
        DisplayDeviceInput _input;

        public virtual bool Blank { get; set; }

        protected void OnUsageChange(ushort hours)
        {
            if (this.UsageChanged != null)
            {
                this.UsageChanged(this, hours);
            }
        }

        public event DisplayDeviceUsageChangeEventHandler UsageChanged;

        public virtual ushort Usage
        {
            get
            {
                return 0;
            }
        }

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

        public abstract void Initialize();

        public abstract CommDeviceType CommunicationType { get; }

        #region IFusionAsset Members

        public void AssignFusionAsset(Fusion fusionInstance, FusionAssetBase asset)
        {
            if (asset is FusionStaticAsset)
            {
                this.FusionAsset = asset as FusionStaticAsset;

                fusionInstance.FusionRoom.OnlineStatusChange += new Crestron.SimplSharpPro.OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);
                fusionInstance.FusionRoom.FusionAssetStateChange += new FusionAssetStateEventHandler(FusionRoom_FusionAssetStateChange);

                this.FusionAsset.AddSig(Crestron.SimplSharpPro.eSigType.String, 1, "Input", Crestron.SimplSharpPro.eSigIoMask.InputSigOnly);
            }
        }

        void FusionRoom_OnlineStatusChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
                this.FusionUpdate();
        }

        void FusionRoom_FusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            if (args.UserConfigurableAssetDetailIndex == this.FusionAsset.ParamAssetNumber)
            {
                switch (args.EventId)
                {
                    case FusionAssetEventId.StaticAssetPowerOnReceivedEventId:
                        if (((Crestron.SimplSharpPro.Fusion.BooleanSigDataFixedName)args.UserConfiguredSigDetail).OutputSig.BoolValue)
                            this.Power = true;
                        break;
                    case FusionAssetEventId.StaticAssetPowerOffReceivedEventId:
                        if (((Crestron.SimplSharpPro.Fusion.BooleanSigDataFixedName)args.UserConfiguredSigDetail).OutputSig.BoolValue)
                            this.Power = false;
                        break;
                }
            }
        }

        public FusionStaticAsset FusionAsset
        {
            get;
            protected set;
        }

        public AssetTypeName AssetTypeName
        {
            get { return AssetTypeName.Display; }
        }

        public virtual void FusionUpdate()
        {
            try
            {
                if (this.FusionAsset != null)
                {
                    this.FusionAsset.PowerOn.InputSig.BoolValue = this.Power;
                    this.FusionAsset.Connected.InputSig.BoolValue = this.DeviceCommunicating;
                    this.FusionAsset.FusionGenericAssetSerialsAsset3.StringInput[1].StringValue = this.Input.ToString();
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.FusionUpdate(), {1}", this.GetType(), e.Message);
            }
        }

        public virtual void FusionError(string errorDetails)
        {
            if (this.FusionAsset != null)
            {
                this.FusionAsset.AssetError.InputSig.StringValue = errorDetails;
            }
        }

        #endregion
    }

    public delegate void DisplayDevicePowerStatusChangeEventHandler(DisplayDevice device, DevicePowerStatusEventArgs args);

    public delegate void DisplayDeviceUsageChangeEventHandler(DisplayDevice device, ushort usageHours);

    public enum DisplayDeviceInput
    {
        Unknown,
        HDMI1,
        HDMI2,
        HDMI3,
        HDMI4,
        DisplayPort,
        DisplayPort2,
        DVI,
        DVI2,
        VGA,
        RGBHV,
        Composite,
        YUV,
        SVideo,
        MagicInfo,
        TV
    }
}