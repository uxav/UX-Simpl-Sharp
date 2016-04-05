using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIKeypad : UIObject
    {
        public UIKeypad(BoolOutputSig digitalStartJoin)
        {
            this.PressDigitalJoin = digitalStartJoin;
        }

        protected override void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
            if (args.Event == eSigEvent.BoolChange
                && args.Sig.BoolValue == true
                && args.Sig.Number >= this.PressDigitalJoin.Number
                && args.Sig.Number <= (this.PressDigitalJoin.Number + 11))
            {
                if (_ButtonPressed != null)
                {
                    uint index = args.Sig.Number - this.PressDigitalJoin.Number;

                    UIKeypadButton button;

                    switch (index)
                    {
                        case 0: button = UIKeypadButton.Digit0; break;
                        case 1: button = UIKeypadButton.Digit1; break;
                        case 2: button = UIKeypadButton.Digit2; break;
                        case 3: button = UIKeypadButton.Digit3; break;
                        case 4: button = UIKeypadButton.Digit4; break;
                        case 5: button = UIKeypadButton.Digit5; break;
                        case 6: button = UIKeypadButton.Digit6; break;
                        case 7: button = UIKeypadButton.Digit7; break;
                        case 8: button = UIKeypadButton.Digit8; break;
                        case 9: button = UIKeypadButton.Digit9; break;
                        case 10: button = UIKeypadButton.Misc1; break;
                        case 11: button = UIKeypadButton.Misc2; break;
                        default: button = UIKeypadButton.Digit0; break;
                    }

                    _ButtonPressed(this, new UIKeypadEventArgs(
                        index,
                        args.Sig.Number,
                        this.Device,
                        button));
                }
            }

            base.OnSigChange(currentDevice, args);
        }

        private event UIKeypadEventHandler _ButtonPressed;

        int subscribeCount = 0;

        public event UIKeypadEventHandler ButtonPressed
        {
            add
            {
                if (subscribeCount == 0)
                    this.SubscribeToSigChanges();

                subscribeCount++;

                _ButtonPressed += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    this.UnSubscribeToSigChanges();

                _ButtonPressed -= value;
            }
        }
    }

    public delegate void UIKeypadEventHandler(UIKeypad keypad, UIKeypadEventArgs args);

    public class UIKeypadEventArgs : EventArgs
    {
        public BasicTriList Device;
        public uint SigNumber;
        public uint Index;
        public UIKeypadButton Button;
        public UIKeypadEventArgs(uint index, uint sigNumber, BasicTriList device, UIKeypadButton button)
        {
            Device = device;
            SigNumber = sigNumber;
            Index = index;
            Button = button;
        }
    }

    public enum UIKeypadButton
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