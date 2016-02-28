using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectButtonCollection : IEnumerable<UISmartObjectButton>
    {
        public UISmartObjectButton this[uint itemIndex]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.ItemIndex == itemIndex);
            }
        }
        
        private List<UISmartObjectButton> Buttons;

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
                button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
            }
        }

        protected virtual void OnButtonEvent(UIObject currentObject, UIObjectButtonEventArgs args)
        {
            if (this.ButtonEvent != null)
            {
                this.ButtonEvent(this, new UISmartObjectButtonCollectionEventArgs(currentObject as UISmartObjectButton, args.EventType, args.HoldTime));
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

        public UISmartObjectButton UISmartObjectButtonBySigNumber(uint pressDigitalJoinNumber)
        {
            return this.Buttons.FirstOrDefault(b => b.PressDigitalJoin.Number == pressDigitalJoinNumber);
        }

        public virtual void Dispose()
        {
            foreach (UISmartObjectButton button in Buttons)
            {
                button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                button.Dispose();
            }
        }
    }

    public delegate void UISmartObjectButtonCollectionEventHandler(UISmartObjectButtonCollection buttonCollection, UISmartObjectButtonCollectionEventArgs args);

    public class UISmartObjectButtonCollectionEventArgs : EventArgs
    {
        public UIButtonEventType EventType;
        public UISmartObjectButton Button;
        public long HoldTime;
        public UISmartObjectButtonCollectionEventArgs(UISmartObjectButton button, UIButtonEventType type, long holdTime)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}