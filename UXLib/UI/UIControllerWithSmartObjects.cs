using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIControllerWithSmartObjects : UIController
    {
        public new BasicTriListWithSmartObject Device
        {
            get { return base.Device as BasicTriListWithSmartObject; }
        }

        public UIControllerWithSmartObjects(uint id, BasicTriListWithSmartObject device)
            : base(id, device)
        {
        }

        public UIControllerWithSmartObjects(uint id, BasicTriListWithSmartObject device, Room defaultRoom)
            : base(id, device, defaultRoom)
        {
        }

        public void LoadSmartObjects(string sgdFilePath)
        {
            this.Device.LoadSmartObjects(sgdFilePath);
        }

        public void LoadSmartObjects(ISmartObject deviceWithSmartObjects)
        {
            this.Device.LoadSmartObjects(deviceWithSmartObjects);
        }

        public void LoadSmartObjects(Crestron.SimplSharp.CrestronIO.Stream fileStream)
        {
            Device.LoadSmartObjects(fileStream);
        } 
    }
}