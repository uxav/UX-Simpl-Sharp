using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Devices;
using UXLib.Models;

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

                if (device is TswFt5ButtonSystem)
                {
                    CrestronConsole.PrintLine("UI Device is TswFt5ButtonSystem device");
                    TswFt5ButtonSystem panelDevice = device as TswFt5ButtonSystem;
                    SystemReservedSigs = panelDevice.ExtenderSystemReservedSigs;
                    SystemReservedSigs.Use();
                }

                this.Device.IpInformationChange += new IpInformationChangeEventHandler(Device_IpInformationChange);
                this.Device.OnlineStatusChange += new OnlineStatusChangeEventHandler(Device_OnlineStatusChange);

                if (this.Device.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register User Interface device with ID: {0}, ipID: {1}", this.ID, this.Device.ID);
                }
            }
            else
            {
                ErrorLog.Error("Cannot register User Interface device with ID: {0} as device is null", this.ID);
            }
        }

        void Device_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (!args.DeviceOnLine)
                ErrorLog.Error("UI Device {0} with ID {1} went offline!", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"));
            else
                ErrorLog.Notice("UI Device {0} with ID {1} is online", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"));
        }

        public UIController(uint id, BasicTriList device, Room defaultRoom)
            : this(id, device)
        {
            _room = defaultRoom;
            _room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
            _room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);
        }

        public uint ID { get; protected set; }
        public string Name { get; set; }
        public BasicTriList Device { get; protected set; }
        public TswFtSystemReservedSigs SystemReservedSigs { get; protected set; }
        Room _room;
        public Room Room
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
                this.Room.Source = value;
            }
        }

        void Room_SourceChange(Room room, RoomSourceChangeEventArgs args)
        {
            if (this.Room == room)
            {
                OnSourceChange(args.PreviousSource, args.NewSource);
            }
        }

        void Device_IpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            if (args.Connected)
                ErrorLog.Notice("UI Device {0} with ID {1} is online with IP Address {2}", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"),
                    args.DeviceIpAddress);
            else
                ErrorLog.Notice("UI Device {0} with ID {1} is offline with IP Address {2}", currentDevice.GetType().Name, currentDevice.ID.ToString("X2"),
                    args.DeviceIpAddress);
        }

        void Room_RoomDetailsChange(Room room, RoomDetailsChangeEventArgs args)
        {
            
        }

        public event RoomChangeEventHandler RoomChanged;

        protected virtual void RoomWillChange(Room newRoom)
        {
            if (RoomChanged != null)
                RoomChanged(this, new RoomChangeEventArgs(newRoom, RoomChangeEventType.WillChange));
        }

        protected virtual void OnRoomChange(Room newRoom)
        {
            if (RoomChanged != null)
                RoomChanged(this, new RoomChangeEventArgs(newRoom, RoomChangeEventType.HasChanged));
        }

        protected virtual void OnRoomDetailsChange()
        {

        }

        protected virtual void OnSourceChange(Source previousSource, Source newSource)
        {

        }

        protected virtual void OnVolumeChange(VolumeLevelType volumeType, ushort volumeLevel)
        {

        }

        public void Debug(DateTime startTime, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            TimeSpan ts = DateTime.Now - startTime;
            CrestronConsole.PrintLine("{0} - {1}",
                string.Format("{0} ({1}) {2:00}:{3:0000}", this.GetType().Name, this.ID, ts.Seconds, ts.Milliseconds),
                formattedMessage);
        }

        public void Debug(string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            CrestronConsole.PrintLine("{0} - {1}",
                string.Format("{0} ({1})", this.GetType().Name, this.ID),
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

        public void AssignFusionAsset(Fusion fusionInstance, Crestron.SimplSharpPro.Fusion.FusionAssetBase asset)
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
        public RoomChangeEventArgs(Room newRoom, RoomChangeEventType eventType)
        {
            NewRoom = newRoom;
            EventType = eventType;
        }

        public Room NewRoom { get; protected set; }
        public RoomChangeEventType EventType { get; protected set; }
    }

    public enum RoomChangeEventType
    {
        WillChange,
        HasChanged
    }
}