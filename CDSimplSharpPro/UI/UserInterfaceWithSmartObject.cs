using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UserInterfaceWithSmartObject : UserInterface
    {
        public new BasicTriListWithSmartObject Device;

        public UserInterfaceWithSmartObject(uint id, BasicTriListWithSmartObject device, Room defaultRoom)
            : base(id, device, defaultRoom)
        {
            this.Device = device;
        }
    }
}