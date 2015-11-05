using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
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

        public UIDateTimeLabel(string keyName, BasicTriList device, uint joinNumber, IFormatProvider dateFormat)
            : base(keyName, device, joinNumber)
        {
            this.DateFormat = dateFormat;
        }
    }
}