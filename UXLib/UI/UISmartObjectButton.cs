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
        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string pressDigitalJoinName)
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.PressDigitalJoin = this.SmartObject.BooleanOutput[pressDigitalJoinName];
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName)
            : this(itemIndex, smartObject, pressDigitalJoinName)
        {
            this.FeedbackDigitalJoin = this.SmartObject.BooleanInput[feedbackDigitalJoinName];
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName)
            : this(itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName)
        {
            this.TextSerialJoin = this.SmartObject.StringInput[textSerialJoinName];
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName, string iconSerialJoinName)
            : this(itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName, textSerialJoinName)
        {
            this.IconSerialJoin = this.SmartObject.StringInput[iconSerialJoinName];
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string pressDigitalJoinName,
            string feedbackDigitalJoinName, string textSerialJoinName, string iconSerialJoinName,
            string enableDigitalJoinName, string visibleDigitalJoinName)
            : this(itemIndex, smartObject, pressDigitalJoinName, feedbackDigitalJoinName,
            textSerialJoinName, iconSerialJoinName)
        {
            this.EnableDigitalJoin = this.SmartObject.BooleanInput[enableDigitalJoinName];
            this.VisibleDigitalJoin = this.SmartObject.BooleanInput[visibleDigitalJoinName];
        }

        public object LinkedObject { get; set; }
        
        public uint ItemIndex { get; protected set; }

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
    }
}