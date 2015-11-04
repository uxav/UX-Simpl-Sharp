using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIButtonGroup : IEnumerable<UIButton>
    {
        private List<UIButton> Buttons;

        public UIButton this[string keyName]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.KeyName == keyName);
            }
        }

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

        public UIButtonGroup()
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

        public void Add(string keyName, BasicTriList device, uint join)
        {
            UIButton newButton = new UIButton(keyName, device, join);
            this.Buttons.Add(newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
        }

        public void Add(string keyName, BasicTriList device, uint join, uint enableJoin, uint visibleJoin)
        {
            UIButton newButton = new UIButton(keyName, device, join, enableJoin, visibleJoin);
            this.Buttons.Add(newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
        }

        public IEnumerator<UIButton> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public event UIButtonGroupEventHandler ButtonEvent;

        void ButtonEventHandler(UIButton button, UIButtonEventArgs args)
        {
            if (this.ButtonEvent != null)
            {
                this.ButtonEvent(this, button, args);
            }
        }
    }

    public delegate void UIButtonGroupEventHandler(UIButtonGroup group, UIButton button, UIButtonEventArgs args);
}