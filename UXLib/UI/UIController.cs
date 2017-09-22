﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Devices;
using UXLib.Models;
using UXLib.Models.Fusion;

namespace UXLib.UI
{
    public class UIController : IFusionStaticAsset, IDevice
    {
        public UIController(uint id, BasicTriList device)
        {
            this.ID = id;
            this.Device = device;

            if (this.Device != null)
            {
                CrestronConsole.PrintLine("Registering UI Device \'{0}\'", device.GetType().ToString());
                CrestronConsole.PrintLine("UI Device \'{0}\' parent is {1}", device.GetType().Name, device.Parent.GetType().Name);

                if (device is TswFt5ButtonSystem)
                {
                    CrestronConsole.PrintLine("UI Device is TswFt5ButtonSystem device");
                    TswFt5ButtonSystem panelDevice = device as TswFt5ButtonSystem;
                    SystemReservedSigs = panelDevice.ExtenderSystemReservedSigs;
                    SystemReservedSigs.Use();
                }

                var x60Device = device as TswX60BaseClass;
                if (x60Device != null)
                {
                    x60Device.ExtenderHardButtonReservedSigs.Use();
                }

                try
                {
                    this.Device.IpInformationChange += new IpInformationChangeEventHandler(Device_IpInformationChange);
                }
                catch
                {
                    // Probably not an Ethernet device
                }
                
                this.Device.OnlineStatusChange += new OnlineStatusChangeEventHandler(DeviceOnlineStatusChanged);

                if (this.Device.Parent is CrestronControlSystem && this.Device.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register User Interface device with ID: {0}, ipID: {1}", this.ID, this.Device.ID);
                }

                device.SigChange += DeviceOnSigChange;
            }
            else
            {
                ErrorLog.Error("Cannot register User Interface device with ID: {0} as device is null", this.ID);
            }
        }

        private void DeviceOnSigChange(BasicTriList currentDevice, SigEventArgs args)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.SigChange ID 0x{1:X2} {2}", currentDevice.GetType().Name,
                currentDevice.ID, args.Sig.ToString());
#endif

            OnPanelActivity(this, new UIControllerActivityEventArgs(args.Sig, args.Event));
        }

        public virtual void DeviceOnlineStatusChanged(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            
        }

        public UIController(uint id, BasicTriList device, UXLib.Models.Room defaultRoom)
            : this(id, device)
        {
            if (defaultRoom != null)
            {
                _room = defaultRoom;
                _defaultRoom = defaultRoom;
                _room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                _room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);
            }
        }

        public uint ID { get; protected set; }
        public string Name { get; set; }
        public BasicTriList Device { get; protected set; }
        public TswFtSystemReservedSigs SystemReservedSigs { get; protected set; }
        private UXLib.Models.Room _defaultRoom;
        public UXLib.Models.Room DefaultRoom
        {
            get { return _defaultRoom; }
        }

        UXLib.Models.Room _room;
        public UXLib.Models.Room Room
        {
            set
            {
                if (_room != value)
                {
                    if (_room != null)
                    {
                        // Unsubscribe from existing room events
                        this.Room.RoomDetailsChange -= new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                        this.Room.SourceChange -= new RoomSourceChangeEventHandler(Room_SourceChange);

                        RoomWillChange(value);
                    }

                    _room = value;
#if DEBUG
                    CrestronConsole.PrintLine("{0} ID {1} Room set to {2}", GetType().Name,
                        ID, value);
#endif
                    if (_defaultRoom == null)
                    {
                        _defaultRoom = value;
#if DEBUG
                        CrestronConsole.PrintLine("{0} ID {1} Room set to {2}", GetType().Name,
                            ID, value);
#endif
                    }

                    if (_room != null)
                    {
                        // Subscribe to new rooms events
                        this.Room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                        this.Room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);
                    }

                    OnRoomChange(value);
                }
            }
            get
            {
                return _room;
            }
        }

        public virtual Source Source
        {
            get
            {
                return this.Room.Source;
            }
            set
            {
                if (this.Room.Source != value)
                    this.Room.Source = value;
                else if (value != null)
                    UIShouldShowSourceControl(value);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ID {1} ({2} 0x{3:X2})", GetType().Name, ID, Device.GetType().Name, Device.ID);
        }

        public virtual void UIShouldShowSourceControl(Source source)
        {
#if DEBUG
            CrestronConsole.PrintLine("UIController {0} UIShouldShowSourceControl() for Source {1}", ID, source);
#endif 
        }

        void Room_SourceChange(UXLib.Models.Room room, RoomSourceChangeEventArgs args)
        {
            try
            {
                if (this.Room == room)
                {
                    OnSourceChange(args.PreviousSource, args.NewSource);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Error in {0}.Room_SourceChange(UXLib.Models.Room room, RoomSourceChangeEventArgs args)",
                    this.GetType().Name), e);
            }
        }

        void Device_IpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            if (currentDevice is CrestronApp)
            {
                // ignore
            }
            else
            {
                if (args.Connected)
                    ErrorLog.Notice("UI Device {0} with ID {1} is online with IP Address {2}", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"),
                        args.DeviceIpAddress);
                else
                    ErrorLog.Notice("UI Device {0} with ID {1} is offline with IP Address {2}", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"),
                        args.DeviceIpAddress);
            }
        }

        void Room_RoomDetailsChange(UXLib.Models.Room room, RoomDetailsChangeEventArgs args)
        {
            OnRoomDetailsChange();
        }

        public event RoomChangeEventHandler RoomChanged;

        protected virtual void RoomWillChange(UXLib.Models.Room newRoom)
        {
            if (RoomChanged != null)
                RoomChanged(this, new RoomChangeEventArgs(newRoom, RoomChangeEventType.WillChange));
        }

        protected virtual void OnRoomChange(UXLib.Models.Room newRoom)
        {
            if (RoomChanged != null)
                RoomChanged(this, new RoomChangeEventArgs(newRoom, RoomChangeEventType.HasChanged));
        }

        protected virtual void OnRoomDetailsChange()
        {

        }

        protected virtual void OnSourceChange(Source previousSource, Source newSource)
        {
            try
            {
                if (newSource != null)
                    UIShouldShowSourceControl(newSource);
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Error in {0}.OnSourceChange(Source previousSource, Source newSource)", this.GetType().Name), e);
            }
        }

        protected virtual void OnVolumeChange(VolumeLevelType volumeType, ushort volumeLevel)
        {

        }

        public event UIControllerActivityEventHandler PanelActivity;

        protected virtual void OnPanelActivity(UIController uicontroller, UIControllerActivityEventArgs args)
        {
            var handler = PanelActivity;
            if (handler != null) handler(uicontroller, args);
        }

        public void Debug(DateTime startTime, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            TimeSpan ts = DateTime.Now - startTime;
            CrestronConsole.PrintLine("{0} - {1}",
                string.Format("{0} ({1}) {2:00}:{3:0000}", this.GetType().Name, this.ID, ts.Seconds, ts.Milliseconds),
                formattedMessage);
        }

        public virtual void Debug(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            CrestronConsole.PrintLine("{0} - {1}",
                string.Format("{0} ({1})", GetType().Name, ID),
                formattedMessage);
        }

        public void Wake()
        {
            if (this.SystemReservedSigs != null)
                this.SystemReservedSigs.BacklightOn();
        }

        public void Sleep()
        {
            if (this.SystemReservedSigs != null)
                this.SystemReservedSigs.BacklightOff();
        }

        #region IFusionStaticAsset Members

        public Crestron.SimplSharpPro.Fusion.FusionStaticAsset FusionAsset
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IFusionAsset Members

        public AssetTypeName AssetTypeName
        {
            get { throw new NotImplementedException(); }
        }

        public void AssignFusionAsset(FusionController fusionInstance, Crestron.SimplSharpPro.Fusion.FusionAssetBase asset)
        {
            throw new NotImplementedException();
        }

        public void FusionUpdate()
        {
            throw new NotImplementedException();
        }

        public void FusionError(string errorDetails)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDevice Members


        public string DeviceManufacturer
        {
            get;
            protected set;
        }

        public string DeviceModel
        {
            get;
            protected set;
        }

        public string DeviceSerialNumber
        {
            get;
            protected set;
        }

        #endregion
    }

    public delegate void RoomChangeEventHandler(UIController uiController, RoomChangeEventArgs args);

    public class RoomChangeEventArgs : EventArgs
    {
        public RoomChangeEventArgs(UXLib.Models.Room newRoom, RoomChangeEventType eventType)
        {
            NewRoom = newRoom;
            EventType = eventType;
        }

        public UXLib.Models.Room NewRoom { get; protected set; }
        public RoomChangeEventType EventType { get; protected set; }
    }

    public enum RoomChangeEventType
    {
        WillChange,
        HasChanged
    }

    public delegate void UIControllerActivityEventHandler(UIController uiController, UIControllerActivityEventArgs args);

    public class UIControllerActivityEventArgs : EventArgs
    {
        internal UIControllerActivityEventArgs(Sig sig, eSigEvent sigEvent)
        {
            Sig = sig;
            Event = sigEvent;
        }

        public Sig Sig { get; private set; }
        public eSigEvent Event { get; private set; }
    }
}