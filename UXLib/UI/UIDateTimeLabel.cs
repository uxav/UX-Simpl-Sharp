using System;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UIDateTimeLabel : UILabel
    {
        private IFormatProvider DateFormat;

        public DateTime DateTime
        {
            set
            {
                this.Text = string.Format(DateFormat, "{0}", value);
            }
        }

        public UIDateTimeLabel(StringInputSig textSerialJoin, IFormatProvider dateFormat)
            : base(textSerialJoin)
        {
            this.DateFormat = dateFormat;
        }
    }
}