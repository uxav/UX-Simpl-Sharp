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
        public Room Room;
        public UIPageGroup<string> Pages;
        public UIButtonGroup Buttons;

        public UserInterface(uint id, BasicTriList device, Room defaultRoom)
        {
            this.Room = defaultRoom;
            this.ID = id;
            this.Device = device;

            if (this.Device.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Could not register User Interface with ID: {0}, ipID: {1}", this.ID, this.Device.ID);
            }

            if (this.Device != null)
            {
                this.Device.SigChange += new SigEventHandler(Device_SigChange);
            }

            this.Pages = new UIPageGroup<string>();

            this.Pages.Add("WELCOME", this.Device.BooleanInput[1]);
            this.Pages.Add("MAIN", this.Device.BooleanInput[2], "Home Menu", this.Device.StringInput[2]);
            this.Pages.Add("SOURCE", this.Device.BooleanInput[3], "Source Name", this.Device.StringInput[3]);

            this.Buttons = new UIButtonGroup("Menu Buttons");

            this.Buttons.Add(this.Device, 1, 2, 3);
            this.Buttons.Add(this.Device, 4, 5, 6);
            this.Buttons.Add(this.Device, 7, 8, 9);

            this.Buttons.ButtonEvent += new UIButtonGroupEventHandler(Buttons_ButtonEvent);
        }

        void Buttons_ButtonEvent(UIButtonGroup group, UIButton button, UIButtonEventArgs args)
        {
            CrestronConsole.PrintLine("Button named '{0}' in '{1}' was {2}", button.Title, group.Name, args.EventType);
            if (args.EventType == eUIButtonEventType.Held)
            {
                CrestronConsole.PrintLine("Button was held for {0} milliseconds", args.HoldTime);
            }
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        if (Buttons.ContainsKey(args.Sig.Number))
                        {
                            Buttons[args.Sig.Number].Down = args.Sig.BoolValue;
                        }
                        break;
                    }
            }
        }

        public eDeviceRegistrationUnRegistrationResponse Register()
        {
            return this.Device.Register();
        }
    }
}