using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UILevel : UIObject
    {
        /// <summary>
        /// Create a level can have feedback only
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        public UILevel(UShortInputSig analogFeedbackJoin)
        {
            this.AnalogFeedbackJoin = analogFeedbackJoin;
        }

        /// <summary>
        /// Create a level can have feedback only
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        /// <param name="enableDigitalJoin">The digital enable join</param>
        /// <param name="visibleDigitalJoin">The digital visible join</param>
        public UILevel(UShortInputSig analogFeedbackJoin, BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin)
            : this(analogFeedbackJoin)
        {
            this.EnableDigitalJoin = enableDigitalJoin;
            this.VisibleDigitalJoin = visibleDigitalJoin;
        }
    }
}