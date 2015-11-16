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
        public uint ID
        {
            get { return this.DeviceSmartObject.ID; }
        }
        public SmartObject DeviceSmartObject;
        public UISmartObjectButtonCollection Buttons;
        public event UISmartObjectButtonEventHandler ButtonEvent;
        BoolInputSig EnableJoin;
        BoolInputSig VisibleJoin;

        public UISmartObject(SmartObject smartObject)
        {
            this.Buttons = new UISmartObjectButtonCollection();
            this.DeviceSmartObject = smartObject;
            this.Buttons.ButtonEvent += new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);
        }

        public UISmartObject(SmartObject smartObject, BoolInputSig objectEnableJoin, BoolInputSig objectVisibleJoin)
        {
            this.Buttons = new UISmartObjectButtonCollection();
            this.DeviceSmartObject = smartObject;
            this.Buttons.ButtonEvent += new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);
            EnableJoin = objectEnableJoin;
            if (EnableJoin != null)
                EnableJoin.BoolValue = true;
            VisibleJoin = objectVisibleJoin;
            if (VisibleJoin != null)
                VisibleJoin.BoolValue = true;
        }

        public void Buttons_ButtonEvent(UISmartObjectButtonCollection buttonCollection, UISmartObjectButtonCollectionEventArgs args)
        {
            this.ButtonEvent(this, new UISmartObjectButtonEventArgs(args.Button, args.EventType, args.HoldTime));
        }

        public void AddButton(UISmartObjectButton button)
        {
            this.Buttons.Add(button);
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName,
            string titleFeedbackSigName, string iconFeedbackSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName,
                    titleFeedbackSigName, iconFeedbackSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName,
            string titleFeedbackSigName, string iconFeedbackSigName, string enableSigName, string visibleSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName,
                    titleFeedbackSigName, iconFeedbackSigName, enableSigName, visibleSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public bool Enabled
        {
            set
            {
                if (this.EnableJoin != null)
                    this.EnableJoin.BoolValue = value;
            }
            get {
                if (this.EnableJoin != null)
                    return this.EnableJoin.BoolValue;
                return false;
            }
        }

        public bool Visible
        {
            set
            {
                if (this.VisibleJoin != null)
                    this.VisibleJoin.BoolValue = value;
            }
            get
            {
                if (this.VisibleJoin != null)
                    return this.VisibleJoin.BoolValue;
                return false;
            }
        }

        public void Show()
        {
            this.Visible = true;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Enable()
        {
            this.Enabled = true;
        }

        public void Disable()
        {
            this.Enabled = false;
        }

        public virtual void Dispose()
        {
            this.Buttons.ButtonEvent -= new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);
            this.Buttons.Dispose();
            this.Buttons = null;
            this.VisibleJoin = null;
            this.EnableJoin = null;
            this.DeviceSmartObject = null;
        }
    }

    public delegate void UISmartObjectButtonEventHandler(UISmartObject sObject, UISmartObjectButtonEventArgs args);

    public class UISmartObjectButtonEventArgs : EventArgs
    {
        public eUIButtonEventType EventType;
        public uint ButtonIndex;
        public UISmartObjectButton Button;
        public long HoldTime;
        public UISmartObjectButtonEventArgs(UISmartObjectButton button, eUIButtonEventType type, long holdTime)
            : base()
        {
            this.ButtonIndex = button.ItemIndex;
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}