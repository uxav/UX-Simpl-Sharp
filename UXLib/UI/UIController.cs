using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIController
    {
        public uint ID { get; private set; }
        public string Name;
        public BasicTriList Device { get; private set; }
        protected Room _Room;
        public Room Room
        {
            set
            {
                if (_Room != value)
                {
                    if (_Room != null)
                    {
                        // Unsubscribe from existing room events
                        this.Room.RoomDetailsChange -= new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                        this.Room.SourceChange -= new RoomSourceChangeEventHandler(Room_SourceChange);
                    }

                    _Room = value;

                    if (_Room != null)
                    {
                        // Subscribe to new rooms events
                        this.Room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                        this.Room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);
                    }

                    this.OnRoomChange();
                }
            }
            get
            {
                return _Room;
            }
        }
        public Source Source
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

        public UIController(uint id, BasicTriList device)
        {
            this.ID = id;
            this.Device = device;

            if (this.Device != null)
            {
                this.Device.IpInformationChange += new IpInformationChangeEventHandler(Device_IpInformationChange);

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
        
        public UIController(uint id, BasicTriList device, Room defaultRoom)
        {
            _Room = defaultRoom;
            this.ID = id;
            this.Device = device;

            if (this.Device != null)
            {
                this.Device.IpInformationChange += new IpInformationChangeEventHandler(Device_IpInformationChange);

                if (this.Device.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register User Interface device with ID: {0}, ipID: {1}", this.ID, this.Device.ID);
                }
            }
            else
            {
                ErrorLog.Error("Cannot register User Interface device with ID: {0} as device is null", this.ID);
            }

            this.Room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
            this.Room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);
        }

        public virtual void Room_SourceChange(Room room, RoomSourceChangeEventArgs args)
        {
            if (this.Room == room)
            {
                OnSourceChange(args.PreviousSource, args.NewSource);
            }
        }

        void Device_IpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            
        }

        void Room_RoomDetailsChange(Room room, RoomDetailsChangeEventArgs args)
        {
            
        }

        protected virtual void OnRoomChange()
        {

        }

        protected virtual void OnRoomDetailsChange()
        {

        }

        protected virtual void OnSourceChange(Source previousSource, Source newSource)
        {

        }

        /// <summary>
        /// Debug some stuff about the panel's behaviour
        /// </summary>
        /// <param name="message">A string to send to the console</param>
        public void Debug(string message)
        {
            CrestronConsole.PrintLine("UI 0x{00:X} {1}", this.Device.ID, message);
        }

        /// <summary>
        /// Debug some stuff about the panel's behaviour
        /// </summary>
        /// <param name="message">A string to send to the notice log</param>
        public void WriteLog(string message)
        {
            ErrorLog.Notice("UI 0x{00:X} : {1}", this.Device.ID, message);
        }
    }
}