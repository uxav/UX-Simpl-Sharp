using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro
{
    public class UserInterface
    {
        public uint ID { get; private set; }
        public string Name;
        public TswFt5ButtonSystem Device;
        public Room Room;
        private BoolInputSigInterlock PageJoins;
        public Dictionary<uint, UIPage> Pages;

        public UserInterface(CrestronControlSystem controlSystem, uint id, uint ipID, string type, Room defaultRoom)
        {
            this.Room = defaultRoom;
            this.ID = id;
            switch (type)
            {
                case "TSW1052":
                    {
                        this.Device = new Tsw1052(ipID, controlSystem);
                        break;
                    }
                default:
                    {
                        ErrorLog.Error("Could not assign interface type '{0}' to new object", type);
                        break;
                    }
            }

            if (this.Device != null)
            {
                this.Device.SigChange += new SigEventHandler(Device_SigChange);
            }

            PageJoins = new BoolInputSigInterlock();
            Pages = new Dictionary<uint, UIPage>();

            Pages.Add(1, new UIPage("Page 1", Device.BooleanInput[1], PageJoins));
            Pages.Add(2, new UIPage("Page 2", Device.BooleanInput[2], PageJoins));
            Pages.Add(3, new UIPage("Page 3", Device.BooleanInput[3], PageJoins));
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        if (args.Sig.BoolValue) // digital join high
                        {
                            CrestronConsole.PrintLine("{0} digital join {1} high", this.Name, args.Sig.Number);

                            if (args.Sig.Number >= 1 && args.Sig.Number <= 3)
                            {
                                Pages[args.Sig.Number].Show();
                            }
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