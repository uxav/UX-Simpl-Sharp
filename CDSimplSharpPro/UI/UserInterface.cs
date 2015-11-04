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
        public UILabelGroup Labels;

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

            this.Labels = new UILabelGroup("Main Labels");
            this.Pages = new UIPageGroup();

            this.Labels.Add(new UILabel("PAGE_TITLE", this.Device, 11));

            this.Pages.Add("WELCOME", this.Device.BooleanInput[11], "Welcome Page", this.Labels["PAGE_TITLE"]);
            this.Pages.Add("HOME", this.Device.BooleanInput[12], "Home Menu", this.Labels["PAGE_TITLE"]);
            this.Pages.Add("SOURCE", this.Device.BooleanInput[13], "Source Page", this.Labels["PAGE_TITLE"]);

            this.Buttons = new UIButtonGroup("Menu Buttons");

            this.Buttons.Add("WELCOME", this.Device, 1);
            this.Buttons[1].Title = "Welcome";
            this.Buttons[1].PageToShowOnRelease = this.Pages["WELCOME"];

            this.Buttons.Add("HOME", this.Device, 2);
            this.Buttons[2].Title = this.Pages["HOME"].Name;
            this.Buttons[2].PageToShowOnRelease = this.Pages["HOME"];

            this.Buttons.Add("SOURCE", this.Device, 3);
            this.Buttons[3].Title = this.Pages["SOURCE"].Name;
            this.Buttons[3].PageToShowOnRelease = this.Pages["SOURCE"];

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