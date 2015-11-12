using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UIPage : UIViewBase
    {
        BoolInputSigInterlock JoinGroup;
        public event UIPageEventHandler PageChange;

        public override bool Visible
        {
            get
            {
                return this.VisibleJoin.BoolValue;
            }
            set
            {
                bool previousValue = this.VisibleJoin.BoolValue;

                if (value == true)
                    this.JoinGroup.Set(this.VisibleJoin);
                else
                    this.VisibleJoin.BoolValue = false;

                // If the page has a serial join sig then set the value to the name of the page
                if (value == true && this.TitleLabel != null)
                    this.TitleLabel.Text = this.Name;

                if (previousValue != this.VisibleJoin.BoolValue && this.PageChange != null)
                {
                    this.PageChange(this, new UIPageEventArgs());
                }
            }
        }
 
        public UIPage(UIKey key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup)
            : base(key, visibleJoinSig)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }

        public UIPage(UIKey key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup, UILabel titleLabel, string name)
            : base(key, visibleJoinSig, titleLabel, name)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }

        public delegate void UIPageEventHandler(UIPage page, UIPageEventArgs args);
    }

    public class UIPageEventArgs : EventArgs
    {
        public UIPageEventArgs()
        {

        }
    }
}