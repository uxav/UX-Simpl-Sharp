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

        public override bool Visible
        {
            get
            {
                return this.VisibleJoin.BoolValue;
            }
            set
            {
                if (value == true)
                    this.JoinGroup.Set(this.VisibleJoin);
                else
                    this.VisibleJoin.BoolValue = false;

                // If the page has a serial join sig then set the value to the name of the page
                if (value == true && this.TitleLabel != null)
                    this.TitleLabel.Text = this.Name;
            }
        }
 
        public UIPage(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup)
            : base(key, visibleJoinSig)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }

        public UIPage(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup, UILabel titleLabel, string name)
            : base(key, visibleJoinSig, titleLabel, name)
        {
            this.JoinGroup = pageVisibleJoinSigGroup;
            this.JoinGroup.Add(visibleJoinSig);
        }
    }
}