using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISubPageModal : UISubPage
    {
        BoolInputSigInterlock JoinGroup;
        
        public override bool Visible
        {
            get
            {
                return this.VisibleJoin.BoolValue;
            }
            set
            {
                if (value == true && !this.VisibleJoin.BoolValue)
                {
                    this.JoinGroup.Set(this.VisibleJoin);
                    if(this.TimeOut != null)
                        this.TimeOut.Set();
                }
                else if (value == false && this.VisibleJoin.BoolValue)
                {
                    this.TimeOut.Cancel();
                    this.VisibleJoin.BoolValue = false;
                }

                // If the page has a serial join sig then set the value to the name of the page
                if (value == true && this.TitleLabel != null)
                    this.TitleLabel.Text = this.Name;
            }
        }

        public UISubPageModal(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup)
            : base (key, visibleJoinSig)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }

        public UISubPageModal(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup, UILabel titleLabel, string name)
            : base (key, visibleJoinSig, titleLabel, name)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }

        public UISubPageModal(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup, UILabel titleLabel, string name, UITimeOut timeOut)
            : base(key, visibleJoinSig, titleLabel, name)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
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