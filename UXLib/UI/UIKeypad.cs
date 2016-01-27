using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIKeypad : IDisposable
    {
        public BoolOutputSig StartJoin { get; private set; }

        public UIKeypad(BoolOutputSig startJoin)
        {
            this.StartJoin = startJoin;

            BasicTriList device = startJoin.Owner as BasicTriList;

            this.Device.SigChange += new SigEventHandler(device_SigChange);
        }

        void device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Event == eSigEvent.BoolChange
                && args.Sig.BoolValue == true
                && args.Sig.Number >= this.StartJoin.Number
                && args.Sig.Number <= (this.StartJoin.Number + 12))
            {
                if (ButtonPressed != null)
                {
                    uint index = args.Sig.Number - this.StartJoin.Number;

                    eKeypadButton button;

                    switch (index)
                    {
                        case 0: button = eKeypadButton.Digit0; break;
                        case 1: button = eKeypadButton.Digit1; break;
                        case 2: button = eKeypadButton.Digit2; break;
                        case 3: button = eKeypadButton.Digit3; break;
                        case 4: button = eKeypadButton.Digit4; break;
                        case 5: button = eKeypadButton.Digit5; break;
                        case 6: button = eKeypadButton.Digit6; break;
                        case 7: button = eKeypadButton.Digit7; break;
                        case 8: button = eKeypadButton.Digit8; break;
                        case 9: button = eKeypadButton.Digit9; break;
                        case 10: button = eKeypadButton.Misc1; break;
                        case 11: button = eKeypadButton.Misc2; break;
                        default: button = eKeypadButton.Digit0; break;
                    }

                    ButtonPressed(this, new UIKeypadEventArgs(
                        index,
                        args.Sig.Number,
                        this.Device,
                        button));
                }
            }
        }

        public BasicTriList Device
        {
            get
            {
                return this.StartJoin.Owner as BasicTriList;
            }
        }

        public event UIKeypadEventHandler ButtonPressed;

        public virtual void Dispose()
        {
            this.Device.SigChange -= new SigEventHandler(device_SigChange);
        }
    }

    public delegate void UIKeypadEventHandler(UIKeypad keypad, UIKeypadEventArgs args);

    public class UIKeypadEventArgs : EventArgs
    {
        public BasicTriList Device;
        public uint SigNumber;
        public uint Index;
        public eKeypadButton Button;
        public UIKeypadEventArgs(uint index, uint sigNumber, BasicTriList device, eKeypadButton button)
        {
            Device = device;
            SigNumber = sigNumber;
            Index = index;
            Button = button;
        }
    }

    public enum eKeypadButton
    {
        Digit0,
        Digit1,
        Digit2,
        Digit3,
        Digit4,
        Digit5,
        Digit6,
        Digit7,
        Digit8,
        Digit9,
        Misc1,
        Misc2
    }
}