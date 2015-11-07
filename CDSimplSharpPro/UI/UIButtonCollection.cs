using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIButtonCollection : IEnumerable<UIButton>
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

        public void Add(string keyName, BoolOutputSig digitalPressJoin)
        {
            UIButton newButton = new UIButton(keyName, digitalPressJoin);
            this.Buttons.Add(newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
        }

        public void Add(string keyName, BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin)
        {
            UIButton newButton = new UIButton(keyName, digitalPressJoin, digitalFeedbackJoin);
            this.Buttons.Add(newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
        }

        public void Add(string keyName, BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig)
        {
            UIButton newButton = new UIButton(keyName, digitalPressJoin, digitalFeedbackJoin, serialJoinSig);
            this.Buttons.Add(newButton);
            newButton.ButtonEvent += new UIButtonEventHandler(ButtonEventHandler);
        }

        public void Add(string keyName, BoolOutputSig digitalOutputJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig, BoolInputSig enableJoinSig, BoolInputSig visibleJoinSig)
        {
            UIButton newButton = new UIButton(keyName, digitalOutputJoin, digitalFeedbackJoin,
                serialJoinSig, enableJoinSig, visibleJoinSig);
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

    public delegate void UIButtonGroupEventHandler(UIButtonCollection group, UIButton button, UIButtonEventArgs args);
}