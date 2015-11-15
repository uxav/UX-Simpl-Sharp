using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISmartObjectButtonCollection : IEnumerable<UISmartObjectButton>
    {
        private List<UISmartObjectButton> Buttons;

        public UISmartObjectButton this[UIKey key]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.Key == key);
            }
        }

        public UISmartObjectButton this[uint itemIndex]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.ItemIndex == itemIndex);
            }
        }

        public int NumberOfButtons
        {
            get
            {
                return this.Buttons.Count;
            }
        }

        public UISmartObjectButtonCollection()
        {
            this.Buttons = new List<UISmartObjectButton>();
        }

        public void Add(UISmartObjectButton button)
        {
            if (!this.Buttons.Contains(button))
            {
                this.Buttons.Add(button);
                button.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
            }
        }

        public IEnumerator<UISmartObjectButton> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public event UISmartObjectButtonCollectionEventHandler ButtonEvent;

        void ButtonEventHandler(UIButtonBase button, UIButtonEventArgs args)
        {
            if (this.ButtonEvent != null)
            {
                this.ButtonEvent(this, new UISmartObjectButtonCollectionEventArgs(button as UISmartObjectButton, args.EventType, args.HoldTime));
            }
        }

        public UISmartObjectButton UISmartObjectButtonBySigNumber(uint sigNumber)
        {
            return this.Buttons.FirstOrDefault(b => b.JoinNumber == sigNumber);
        }
    }

    public delegate void UISmartObjectButtonCollectionEventHandler(UISmartObjectButtonCollection buttonCollection, UISmartObjectButtonCollectionEventArgs args);

    public class UISmartObjectButtonCollectionEventArgs : EventArgs
    {
        public eUIButtonEventType EventType;
        public UISmartObjectButton Button;
        public long HoldTime;
        public UISmartObjectButtonCollectionEventArgs(UISmartObjectButton button, eUIButtonEventType type, long holdTime)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}