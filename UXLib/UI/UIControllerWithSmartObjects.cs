using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIControllerWithSmartObjects : UIController
    {
        public new BasicTriListWithSmartObject Device;

        public UIControllerWithSmartObjects(uint id, BasicTriListWithSmartObject device)
            : base(id, device)
        {
            this.Device = device;
        }

        public UIControllerWithSmartObjects(uint id, BasicTriListWithSmartObject device, Room defaultRoom)
            : base(id, device, defaultRoom)
        {
            this.Device = device;
        }

        public void LoadSmartObjects(string sgdFilePath)
        {
            this.Device.LoadSmartObjects(sgdFilePath);
        }

        public void LoadSmartObjects(ISmartObject deviceWithSmartObjects)
        {
            this.Device.LoadSmartObjects(deviceWithSmartObjects);
        }
    }
}