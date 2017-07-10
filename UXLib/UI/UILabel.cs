using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UILabel : UIObject
    {
        public UILabel(StringInputSig textSerialJoin)
        {
            this.TextSerialJoin = textSerialJoin;
        }

        public UILabel(StringInputSig textSerialJoin, string defaultText)
            : this(textSerialJoin)
        {
            this.Text = defaultText;
        }

        public UILabel(StringInputSig textSerialJoin, BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin)
            : this(textSerialJoin)
        {
            this.EnableDigitalJoin = enableDigitalJoin;
            this.VisibleDigitalJoin = visibleDigitalJoin;
        }

        public UILabel(StringInputSig textSerialJoin, string defaultText, BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin)
            : this(textSerialJoin, enableDigitalJoin, visibleDigitalJoin)
        {
            this.Text = defaultText;
        }

        private new UShortInputSig AnalogFeedbackJoin { get; set; }
        private new UShortOutputSig AnalogTouchJoin { get; set; }

        bool _AllCaps = false;
        public bool AllCaps
        {
            set
            {
                if (_AllCaps != value)
                {
                    _AllCaps = value;

                    this.Text = _Text;
                }
            }
            get
            {
                return _AllCaps;
            }
        }

        string _Text = string.Empty;
        public override string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                if (this.AllCaps)
                    base.Text = _Text.ToUpper();
                else
                    base.Text = _Text;
            }
        }
    }
}