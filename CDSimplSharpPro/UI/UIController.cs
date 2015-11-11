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
    public class UIController
    {
        public uint ID { get; private set; }
        public string Name;
        public BasicTriList Device;
        Room _Room;
        public Room Room
        {
            set
            {
                if (_Room != value && value != null)
                {
                    // Unsubscribe from existing room events
                    this.Room.RoomDetailsChange -= new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                    this.Room.SourceChange -= new RoomSourceChangeEventHandler(Room_SourceChange);

                    // Set the Room Name label
                    this.Labels[UILabelKeys.RoomName].Text = this.Room.Name;

                    // Subscribe to new rooms events
                    this.Room.RoomDetailsChange += new RoomDetailsChangeEventHandler(Room_RoomDetailsChange);
                    this.Room.SourceChange += new RoomSourceChangeEventHandler(Room_SourceChange);

                    this.RoomHasChanged(value);
                }
            }
            get
            {
                return _Room;
            }
        }
        public UIPageCollection Pages;
        public UISubPageModalCollection Modals;
        public UIButtonCollection Buttons;
        public UILabelCollection Labels;
        
        public UIController(uint id, BasicTriList device, Room defaultRoom)
        {
            _Room = defaultRoom;
            this.ID = id;
            this.Device = device;

            this.Labels = new UILabelCollection();
            this.Pages = new UIPageCollection();
            this.Modals = new UISubPageModalCollection();
            this.Buttons = new UIButtonCollection();

            if (this.Device != null)
            {
                this.Device.SigChange += new SigEventHandler(Device_SigChange);
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
            string previousSourceName;
            string newSourceName;

            if (args.PreviousSource != null)
                previousSourceName = args.PreviousSource.Name;
            else
                previousSourceName = "No Source";

            if (args.NewSource != null)
                newSourceName = args.NewSource.Name;
            else
                newSourceName = "No Source";
            
            CrestronConsole.PrintLine("UIController base class source changed from {0} to {1}", previousSourceName, newSourceName);
        }

        void Device_IpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            
        }

        void Room_RoomDetailsChange(Room room, RoomDetailsChangeEventArgs args)
        {
            this.Labels[UILabelKeys.RoomName].Text = room.Name;
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
#if (DEBUG)
            CrestronConsole.PrintLine("**Sig Change** {0}", args.Sig.ToString());
#endif
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

        public virtual void RoomHasChanged(Room newRoom)
        {

        }
    }
}