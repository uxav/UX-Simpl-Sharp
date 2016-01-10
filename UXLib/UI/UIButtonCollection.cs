using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIButtonCollection : IEnumerable<UIButton>
    {
        private List<UIButton> Buttons;

        public UIButton this[uint joinNumber]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.JoinNumber == joinNumber);
            }
        }

        public int NumberOfButtons
        {
            get
            {
                return this.Buttons.Count;
            }
        }

        public UIButtonCollection()
        {
            this.Buttons = new List<UIButton>();
        }

        public void Add(UIButton button)
        {
            if (!this.Buttons.Contains(button))
            {
                this.Buttons.Add(button);
                button.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
            }
        }

        public IEnumerator<UIButton> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public event UIButtonCollectionEventHandler ButtonEvent;

        void ButtonEventHandler(UIButtonBase button, UIButtonEventArgs args)
        {
            if (this.ButtonEvent != null)
            {
                this.ButtonEvent(this, new UIButtonCollectionEventArgs(button as UIButton, args.EventType, args.HoldTime));
            }
        }

        public virtual void Dispose()
        {
            foreach (UIButton button in Buttons)
            {
                button.ButtonEvent -= new UIButtonEventHandler(ButtonEventHandler);
                button.Dispose();
            }
        }
    }

    public delegate void UIButtonCollectionEventHandler(UIButtonCollection buttonCollection, UIButtonCollectionEventArgs args);

    public class UIButtonCollectionEventArgs : EventArgs
    {
        public eUIButtonEventType EventType;
        public UIButton Button;
        public long HoldTime;
        public UIButtonCollectionEventArgs(UIButton button, eUIButtonEventType type, long holdTime)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}