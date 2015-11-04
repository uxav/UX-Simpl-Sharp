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
        public UIPageGroup Pages;
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

            this.Pages = new UIPageGroup();

            this.Pages.Add("WELCOME", this.Device.BooleanInput[11]);
            this.Pages.Add("MAIN", this.Device.BooleanInput[14], "Home Menu", this.Device.StringInput[11]);
            this.Pages.Add("SOURCE", this.Device.BooleanInput[17], "Source Page", this.Device.StringInput[11]);

            this.Buttons = new UIButtonGroup("Menu Buttons");

            this.Buttons.Add(this.Device, 1, 2, 3);
            this.Buttons[1].Title = "Welcome";
            this.Buttons.Add(this.Device, 4, 5, 6);
            this.Buttons[4].Title = this.Pages["MAIN"].Name;
            this.Buttons.Add(this.Device, 7, 8, 9);
            this.Buttons[7].Title = this.Pages["SOURCE"].Name;

            this.Buttons.ButtonEvent += new UIButtonGroupEventHandler(Buttons_ButtonEvent);

            CrestronConsole.PrintLine("Setup {0} number of buttons", Buttons.NumberOfButtons);

            foreach (UIButton button in this.Buttons)
            {
                CrestronConsole.PrintLine("Setup button with join {0}", button.JoinNumber);
            }

            CrestronConsole.PrintLine("Button with join 1 has a title of: {0}", Buttons[1].Title);
        }

        void Buttons_ButtonEvent(UIButtonGroup group, UIButton button, UIButtonEventArgs args)
        {
            CrestronConsole.PrintLine("Button named '{0}' in '{1}' was {2}", button.Title, group.Name, args.EventType);
            if (args.EventType == eUIButtonEventType.Released)
            {
                CrestronConsole.PrintLine("Button was held for {0} milliseconds", args.HoldTime);
            }
            else if (args.EventType == eUIButtonEventType.Tapped)
            {
                UIPage page = this.Pages[button.JoinNumber + 10];
                if (page != null)
                {
                    page.Show();
                }
            }
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

        public eDeviceRegistrationUnRegistrationResponse Register()
        {
            return this.Device.Register();
        }
    }
}