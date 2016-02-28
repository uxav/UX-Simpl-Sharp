using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISubPage : UIViewBase
    {
        public UITimeOut TimeOut;

        public UISubPage(BoolInputSig visibleJoinSig)
            : base (visibleJoinSig)
        {
            
        }

        public UISubPage(BoolInputSig visibleJoinSig, UILabel titleLabel)
            : base (visibleJoinSig, titleLabel)
        {

        }

        public UISubPage(BoolInputSig visibleJoinSig, UILabel titleLabel, UILabel subTitleLabel, UITimeOut timeOut)
            : base(visibleJoinSig, titleLabel, subTitleLabel)
        {
            this.TimeOut = timeOut;
            this.TimeOut.TimedOut += new UITimeOutEventHandler(TimeOut_TimedOut);
        }

        protected override void OnShow()
        {
            base.OnShow();
            if (this.TimeOut != null)
                this.TimeOut.Set();
        }

        protected override void OnHide()
        {
            base.OnHide();
            if (this.TimeOut != null)
                this.TimeOut.Cancel();
        }

        protected override void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
            if (this.TimeOut != null)
                this.TimeOut.Reset();
            base.OnSigChange(currentDevice, args);
        }

        void TimeOut_TimedOut(object timeOutObject, UITimeOutEventArgs args)
        {
            if (this.Visible)
                this.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            this.TimeOut.TimedOut -= new UITimeOutEventHandler(TimeOut_TimedOut);
            this.TimeOut.Dispose();
            base.Dispose(disposing);
        }
    }
}