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

        public UISmartObjectButton this[string keyName]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.KeyName == keyName);
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

        void ButtonEventHandler(UIButton button, UIButtonEventArgs args)
        {
            if (this.ButtonEvent != null)
            {
                this.ButtonEvent(button as UISmartObjectButton, args);
            }
        }

        public UISmartObjectButton UISmartObjectButtonBySigNumber(uint sigNumber)
        {
            return this.Buttons.FirstOrDefault(b => b.JoinNumber == sigNumber);
        }
    }

    public delegate void UISmartObjectButtonCollectionEventHandler(UISmartObjectButton button, UIButtonEventArgs args);
}