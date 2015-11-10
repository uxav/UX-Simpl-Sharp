using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISubPage : UIViewBase
    {
        public UITimeOut TimeOut;

        public override bool Visible
        {
            get
            {
                return this.VisibleJoin.BoolValue;
            }
            set
            {
                if (this.VisibleJoin.BoolValue && !value)
                    this.TimeOut.Cancel();
                else if (!this.VisibleJoin.BoolValue && value)
                {
                    this.VisibleJoin.BoolValue = value;
                    if (this.TimeOut != null)
                    {
                        this.TimeOut.Set();
                    }
                }
                // If the page has a serial join sig then set the value to the name of the page
                if (value == true && this.TitleLabel != null)
                    this.TitleLabel.Text = this.Name;
            }
        }

        public UISubPage(UIKey key, BoolInputSig visibleJoinSig)
            : base (key, visibleJoinSig)
        {
            
        }

        public UISubPage(UIKey key, BoolInputSig visibleJoinSig, UILabel titleLabel, string name)
            : base (key, visibleJoinSig, titleLabel, name)
        {

        }

        public UISubPage(UIKey key, BoolInputSig visibleJoinSig, UILabel titleLabel, string name, UITimeOut timeOut)
            : base(key, visibleJoinSig, titleLabel, name)
        {
            this.TimeOut = timeOut;
            this.TimeOut.TimedOut += new UITimeOutEventHandler(TimeOut_TimedOut);
        }

        void TimeOut_TimedOut(object timeOutObject, UITimeOutEventArgs args)
        {
            if (this.Visible)
                this.Visible = false;
        }
    }
}