using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
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
            Device.LoadSmartObjects(sgdFilePath);

            foreach (var o in Device.SmartObjects)
            {
                o.Value.SigChange += ValueOnSigChange;
            }
        }

        private void ValueOnSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.SmartObject[{3}].SigChange ID 0x{1:X2} {2}", currentDevice.GetType().Name,
                currentDevice.ID, args.Sig.ToString(), args.SmartObjectArgs.ID);
#endif
            OnPanelActivity(this, new UIControllerActivityEventArgs(args.Sig, args.Event));
        }

        public void LoadSmartObjects(ISmartObject deviceWithSmartObjects)
        {
            Device.LoadSmartObjects(deviceWithSmartObjects);
#if DEBUG
            foreach (var o in Device.SmartObjects)
            {
                o.Value.SigChange += ValueOnSigChange;
            }
#endif
        }

        public void LoadSmartObjects(Crestron.SimplSharp.CrestronIO.Stream fileStream)
        {
            Device.LoadSmartObjects(fileStream);
#if DEBUG
            foreach (var o in Device.SmartObjects)
            {
                o.Value.SigChange += ValueOnSigChange;
            }
#endif
        }
    }
}