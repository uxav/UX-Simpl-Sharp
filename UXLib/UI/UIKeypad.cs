using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIKeypad
    {
        public UIKeypad(BoolOutputSig digitalStartJoin)
        {
            this.Buttons = new UIButtonCollection();

            for (uint join = digitalStartJoin.Number; join <= digitalStartJoin.Number + 11; join++)
            {
                UIButton newButton = new UIButton(((BasicTriList)digitalStartJoin.Owner).BooleanOutput[join]);
                this.Buttons.Add(newButton);
            }
        }

        public UIButtonCollection Buttons { get; protected set; }

        private event UIKeypadEventHandler _ButtonEvent;

        int subscribeCount = 0;

        public event UIKeypadEventHandler ButtonEvent
        {
            add
            {
                if (subscribeCount == 0)
                    this.Buttons.ButtonEvent += new UIButtonCollectionEventHandler(Buttons_ButtonEvent);

                subscribeCount++;

                _ButtonEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    this.Buttons.ButtonEvent -= new UIButtonCollectionEventHandler(Buttons_ButtonEvent);

                _ButtonEvent -= value;
            }
        }

        void Buttons_ButtonEvent(UIButtonCollection buttonCollection, UIButtonCollectionEventArgs args)
        {
            _ButtonEvent(this, new UIKeypadEventArgs(
                args.EventType,
                args.ButtonIndexInCollection,
                args.Button.PressDigitalJoin.Number,
                args.Button.Device,
                (UIKeypadButton)args.ButtonIndexInCollection));
        }
    }

    public delegate void UIKeypadEventHandler(UIKeypad keypad, UIKeypadEventArgs args);

    public class UIKeypadEventArgs : EventArgs
    {
        public BasicTriList Device;
        public UIButtonEventType EventType;
        public uint SigNumber;
        public int Index;
        public UIKeypadButton Button;
        public UIKeypadEventArgs(UIButtonEventType eventType, int index, uint sigNumber, BasicTriList device, UIKeypadButton button)
        {
            EventType = eventType;
            Device = device;
            SigNumber = sigNumber;
            Index = index;
            Button = button;
        }
    }

    public enum UIKeypadButton
    {
        Digit0 = 0,
        Digit1 = 1,
        Digit2 = 2,
        Digit3 = 3,
        Digit4 = 4,
        Digit5 = 5,
        Digit6 = 6,
        Digit7 = 7,
        Digit8 = 8,
        Digit9 = 9,
        Misc1 = 10,
        Misc2 = 11
    }
}