using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIPage : UIViewBase
    {
        public UIPage(BoolInputSig visibleDigitalJoin, BoolOutputSig visibleFeedbackJoin)
            : base(visibleDigitalJoin)
        {
            this.VisibleFeedbackJoin = visibleFeedbackJoin;
        }

        public UIPage(BoolInputSig visibleDigitalJoin, BoolOutputSig visibleFeedbackJoin, UILabel titleLabel, string title)
            : base(visibleDigitalJoin, titleLabel)
        {
            this.VisibleFeedbackJoin = visibleFeedbackJoin;
            this.Title = title;
        }

        protected override void OnShow()
        {
            if (!base.Visible)
                base.Visible = true;
            else
                base.OnShow();
        }

        public override bool Visible
        {
            get
            {
                return this.VisibleFeedbackJoin.BoolValue;
            }
            set
            {
                base.Visible = value;
            }
        }
    }
}