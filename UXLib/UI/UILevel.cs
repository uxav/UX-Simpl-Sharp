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
        /// Create a level which can have feedback only
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        public UILevel(UShortInputSig analogFeedbackJoin)
        {
            this.LevelMinimumValue = ushort.MinValue;
            this.LevelMaximumValue = ushort.MaxValue;
            this.AnalogFeedbackJoin = analogFeedbackJoin;
        }

        /// <summary>
        /// Create a level that is touch settable
        /// </summary>
        /// <param name="analogFeedbackJoin">The analog input signal join</param>
        /// <param name="analogTouchJoin">The analog 'touch' output signal join</param>
        public UILevel(UShortInputSig analogFeedbackJoin, UShortOutputSig analogTouchJoin)
            : this(analogFeedbackJoin)
        {
            this.AnalogTouchJoin = analogTouchJoin;
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

        /// <summary>
        /// Set the feedback value
        /// </summary>
        /// <param name="level"></param>
        public void SetLevel(ushort level)
        {
            this.AnalogFeedbackValue = level;
        }

        /// <summary>
        /// Set the feedback value from a scaled value
        /// </summary>
        /// <param name="scaledValue"></param>
        /// <param name="minimumValue"></param>
        /// <param name="maximumValue"></param>
        public void SetLevel(double scaledValue, double minimumValue, double maximumValue)
        {
            try
            {
                int newVal = (int)ScaleRange(scaledValue, minimumValue, maximumValue, this.LevelMinimumValue, this.LevelMaximumValue);
                this.AnalogFeedbackValue = (ushort)newVal;
            }
            catch
            {
                ErrorLog.Error("Cannot scale level back to UILevel ID: {0}", this.AnalogFeedbackJoin.Number);
            }
        }

        public ushort LevelMinimumValue { get; set; }
        public ushort LevelMaximumValue { get; set; }

        public static double ScaleRange(double Value,
           double FromMinValue, double FromMaxValue,
           double ToMinValue, double ToMaxValue)
        {
            try
            {
                return (Value - FromMinValue) *
                    (ToMaxValue - ToMinValue) /
                    (FromMaxValue - FromMinValue) + ToMinValue;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}