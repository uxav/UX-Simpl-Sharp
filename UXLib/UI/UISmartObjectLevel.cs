using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UISmartObjectLevel : UILevel
    {
        /// <summary>
        /// Create a level which can have feedback only
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        public UISmartObjectLevel(UISmartObject owner, uint itemIndex, SmartObject smartObject, string analogFeedbackJoinName)
            : base(smartObject.UShortInput[analogFeedbackJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.Owner = owner;
        }

        /// <summary>
        /// Create a level that is touch settable
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        /// <param name="analogTouchJoin">The analog 'touch' output signal join</param>
        public UISmartObjectLevel(UISmartObject owner, uint itemIndex, SmartObject smartObject,
            string analogFeedbackJoinName, string analogTouchJoinName, string pressDigitalJoinName)
            : base(smartObject.UShortInput[analogFeedbackJoinName], smartObject.UShortOutput[analogTouchJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.PressDigitalJoin = this.SmartObject.BooleanOutput[pressDigitalJoinName];
            this.Owner = owner;
        }

        /// <summary>
        /// Create a level that is touch settable
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        /// <param name="analogTouchJoin">The analog 'touch' output signal join</param>
        /// <param name="enableDigitalJoin">The digital enable join</param>
        /// <param name="visibleDigitalJoin">The digital visible join</param>
        public UISmartObjectLevel(UISmartObject owner, uint itemIndex, SmartObject smartObject,
            string analogFeedbackJoinName, string analogTouchJoinName, string pressDigitalJoinName, string enableDigitalJoinName, string visibleDigitalJoinName)
            : this(owner, itemIndex, smartObject, analogFeedbackJoinName, analogTouchJoinName, pressDigitalJoinName)
        {
            this.EnableDigitalJoin = this.SmartObject.BooleanInput[enableDigitalJoinName];
            this.VisibleDigitalJoin = this.SmartObject.BooleanInput[visibleDigitalJoinName];
        }

        public uint ItemIndex { get; protected set; }

        public UISmartObject Owner { get; protected set; }
    }
}