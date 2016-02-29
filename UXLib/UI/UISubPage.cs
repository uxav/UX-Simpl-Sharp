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
        public UISubPage(BoolInputSig visibleDigitalJoin)
            : base(visibleDigitalJoin) { }

        public UISubPage(BoolInputSig visibleDigitalJoin, UILabel titleLabel)
            : base(visibleDigitalJoin, titleLabel) { }

        public UISubPage(BoolInputSig visibleDigitalJoin, UILabel titleLabel, UILabel subTitleLabel)
            : base(visibleDigitalJoin, titleLabel, subTitleLabel) { }

        public UISubPage(BoolInputSig visibleDigitalJoin, UILabel titleLabel, UILabel subTitleLabel, UITimeOut timeOut)
            : base(visibleDigitalJoin, titleLabel, subTitleLabel)
        {
            this.TimeOut = timeOut;
            this.TimeOut.TimedOut += new UITimeOutEventHandler(TimeOut_TimedOut);
        }

        public UISubPage(UIController uiController, uint visibleDigitalJoinNumber)
            : this(uiController.Device.BooleanInput[visibleDigitalJoinNumber]) { }

        public UISubPage(UIController uiController, uint visibleDigitalJoinNumber, UILabel titleLabel)
            : this(uiController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel) { }

        public UISubPage(UIController uiController, uint visibleDigitalJoinNumber, UILabel titleLabel, UILabel subTitleLabel)
            : this(uiController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel, subTitleLabel) { }

        public UISubPage(UIController uiController, uint visibleDigitalJoinNumber, UILabel titleLabel, UILabel subTitleLabel, UITimeOut timeOut)
            : this(uiController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel, subTitleLabel, timeOut) { }

        public UISubPage(UIViewController viewController, uint visibleDigitalJoinNumber)
            : this(viewController.UIController.Device.BooleanInput[visibleDigitalJoinNumber]) { }

        public UISubPage(UIViewController viewController, uint visibleDigitalJoinNumber, UILabel titleLabel)
            : this(viewController.UIController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel) { }

        public UISubPage(UIViewController viewController, uint visibleDigitalJoinNumber, UILabel titleLabel, UILabel subTitleLabel)
            : this(viewController.UIController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel, subTitleLabel) { }

        public UISubPage(UIViewController viewController, uint visibleDigitalJoinNumber, UILabel titleLabel, UILabel subTitleLabel, UITimeOut timeOut)
            : this(viewController.UIController.Device.BooleanInput[visibleDigitalJoinNumber], titleLabel, subTitleLabel, timeOut) { }

        public UITimeOut TimeOut;
        
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