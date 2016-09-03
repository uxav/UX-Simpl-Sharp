using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectButton : UIObject
    {
        public UISmartObjectButton(UISmartObject owner, uint itemIndex, SmartObject smartObject, string pressDigitalJoinName)
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.PressDigitalJoin = this.SmartObject.BooleanOutput[pressDigitalJoinName];
            this.Owner = owner;
        }

        public UISmartObjectButton(UISmartObject owner, uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName)
            : this(owner, itemIndex, smartObject, pressDigitalJoinName)
        {
            if (feedbackDigitalJoinName != null)
                this.FeedbackDigitalJoin = this.SmartObject.BooleanInput[feedbackDigitalJoinName];
        }

        public UISmartObjectButton(UISmartObject owner, uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName)
            : this(owner, itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName)
        {
            if (textSerialJoinName != null && textSerialJoinName.Length > 0)
                this.TextSerialJoin = this.SmartObject.StringInput[textSerialJoinName];
        }

        public UISmartObjectButton(UISmartObject owner, uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName, string iconSerialJoinName)
            : this(owner, itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName, textSerialJoinName)
        {
            if (iconSerialJoinName != null && iconSerialJoinName.Length > 0)
                this.IconSerialJoin = this.SmartObject.StringInput[iconSerialJoinName];
        }

        public UISmartObjectButton(UISmartObject owner, uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName, string iconSerialJoinName,
            string enableDigitalJoinName, string visibleDigitalJoinName)
            : this(owner, itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName,
            textSerialJoinName, iconSerialJoinName)
        {
            if (enableDigitalJoinName != null && enableDigitalJoinName.Length > 0)
                this.EnableDigitalJoin = this.SmartObject.BooleanInput[enableDigitalJoinName];
            if (visibleDigitalJoinName != null && visibleDigitalJoinName.Length > 0)
                this.VisibleDigitalJoin = this.SmartObject.BooleanInput[visibleDigitalJoinName];
        }

        public object LinkedObject { get; set; }
        
        public uint ItemIndex { get; protected set; }

        public UISmartObject Owner { get; protected set; }

        protected override void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
            SmartObjectEventArgs sArgs = args as SmartObjectEventArgs;

            base.OnSigChange(currentDevice, args);
        }

        public string Title
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }

        public void SetAnalogModeJoin(string analogModeJoinName)
        {
            this.AnalogModeJoin = this.SmartObject.UShortInput[analogModeJoinName];
        }
    }
}