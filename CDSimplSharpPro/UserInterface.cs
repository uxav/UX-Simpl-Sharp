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
        public UIPages<string> Pages;

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

            if (this.Device.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Could not register User Interface with ID: {0}, ipID: {1}", this.ID, ipID);
            }

            if (this.Device != null)
            {
                this.Device.SigChange += new SigEventHandler(Device_SigChange);
            }

            Pages = new UIPages<string>();

            Pages.Add("WELCOME", "Welcome Page", this.Device.BooleanInput[1]);
            Pages.Add("MAIN", "Main Page", this.Device.BooleanInput[2]);
            Pages.Add("SOURCE", "Source Page", this.Device.BooleanInput[3]);
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

                            switch (args.Sig.Number)
                            {
                                case 1: Pages["WELCOME"].Show(); break;
                                case 2: Pages["MAIN"].Show(); break;
                                case 3: Pages["SOURCE"].Show(); break;
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