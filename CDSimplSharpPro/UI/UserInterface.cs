using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UserInterface
    {
        public uint ID { get; private set; }
        public string Name;
        public BasicTriList Device;
        public Room Room { get; private set; }
        public UIPageCollection Pages;
        public UIButtonCollection Buttons;
        public UILabelCollection Labels;
        
        public UserInterface(uint id, BasicTriList device, Room defaultRoom)
        {
            this.Room = defaultRoom;
            this.ID = id;
            this.Device = device;
            
            this.Labels = new UILabelCollection();
            this.Pages = new UIPageCollection();
            this.Buttons = new UIButtonCollection();

            if (this.Device != null)
            {
                this.Device.SigChange += new SigEventHandler(Device_SigChange);

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
        }

        void Room_RoomDetailsChange(Room room, RoomDetailsChangeEventArgs args)
        {
            this.Labels["ROOM_NAME"].Text = room.Name;
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        UIButton button = this.Buttons[args.Sig.Number];
                        if (button != null)
                        {
                            button.Down = args.Sig.BoolValue;
                        }
                        break;
                    }
            }
        }

        public void ChangeRoom(Room newRoom)
        {
            // Unsubscribe from existing room events
            this.Room.RoomDetailsChange -= new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);

            // Make this.Room the new room
            this.Room = newRoom;
            this.Labels["ROOM_NAME"].Text = this.Room.Name;

            // Subscribe to new rooms events
            this.Room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
        }
    }
}