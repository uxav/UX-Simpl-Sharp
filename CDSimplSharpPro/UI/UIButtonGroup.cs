using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIButtonGroup : Dictionary<uint, UIButton>
    {
        public string Name { get; private set; }

        public UIButtonGroup(string name)
            : base()
        {
            this.Name = name;
        }

        public void Add(UIButton button)
        {
            if (!this.Values.Contains(button))
            {
                base.Add(button.JoinNumber, button);
                button.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
            }
        }

        public void Add(BasicTriList device, uint digitalJoin, uint serialJoin, uint enableJoin, uint visibleJoin)
        {
            UIButton newButton = new UIButton(device, digitalJoin, serialJoin, enableJoin, visibleJoin);
            base.Add(newButton.JoinNumber, newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
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