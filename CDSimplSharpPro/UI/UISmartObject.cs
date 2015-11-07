using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UISmartObject
    {
        public string KeyName { get; private set; }
        public SmartObject DeviceSmartObject;
        UIButtonCollection Buttons;
        public event UISmartObjectButtonEventHandler ButtonEvent;

        public UISmartObject(string keyName, SmartObject smartObject)
        {
            this.KeyName = keyName;
            this.Buttons = new UIButtonCollection();
            this.DeviceSmartObject = smartObject;
            this.Buttons.ButtonEvent += new UIButtonGroupEventHandler(Buttons_ButtonEvent);
            this.DeviceSmartObject.SigChange += new SmartObjectSigChangeEventHandler(DeviceSmartObject_SigChange);
        }

        void Buttons_ButtonEvent(UIButtonCollection group, UIButton button, UIButtonEventArgs args)
        {
            this.ButtonEvent(this, new UISmartObjectButtonEventArgs(button, args.EventType, args.HoldTime));
        }

        public void AddButton(UIButton button)
        {
            this.Buttons.Add(button);
        }

        public void AddButton(string digitalPressSigNam, string digitalFeedbackSigName, string serialFeedbackSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UIButton newButton = new UIButton(
                    digitalPressSigNam,
                    this.DeviceSmartObject.BooleanOutput[digitalPressSigNam],
                    this.DeviceSmartObject.BooleanInput[digitalFeedbackSigName],
                    this.DeviceSmartObject.StringInput[serialFeedbackSigName]
                    );
                this.Buttons.Add(newButton);
            }
        }

        public void AddButton(string digitalPressSigNam, string digitalFeedbackSigName, string serialFeedbackSigName,
            string enableSigName, string visibleSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UIButton newButton = new UIButton(
                    digitalPressSigNam,
                    this.DeviceSmartObject.BooleanOutput[digitalPressSigNam],
                    this.DeviceSmartObject.BooleanInput[digitalFeedbackSigName],
                    this.DeviceSmartObject.StringInput[serialFeedbackSigName],
                    this.DeviceSmartObject.BooleanInput[enableSigName],
                    this.DeviceSmartObject.BooleanInput[visibleSigName]
                    );
                this.Buttons.Add(newButton);
            }
        }

        void DeviceSmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        UIButton button = this.Buttons[args.Sig.Number];
                        if (button != null)
                        {
                            button.Down = args.Sig.BoolValue;
                        }
                        break;
                    }
            }
        }
    }

    public delegate void UISmartObjectButtonEventHandler(UISmartObject sObject, UISmartObjectButtonEventArgs args);

    public class UISmartObjectButtonEventArgs : EventArgs
    {
        public eUIButtonEventType EventType;
        public UIButton Button;
        public long HoldTime;
        public UISmartObjectButtonEventArgs(UIButton button, eUIButtonEventType type, long holdTime)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}