using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UIViewBase
    {
        public UIKey Key { get; private set; }
        public BoolInputSig VisibleJoin { get; private set; }
        public UILabel TitleLabel;
        string _name;
        public string Name
        {
            set
            {
                // Set the value
                this._name = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.TitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.Visible)
                        this.TitleLabel.Text = this._name;
                }
            }
            get
            {
                return this._name;
            }
        }
        public uint VisibleJoinNumber
        {
            get
            {
                return this.VisibleJoin.Number;
            }
        }
        public virtual bool Visible
        {
            get
            {
                return this.VisibleJoin.BoolValue;
            }
            set
            {
                this.VisibleJoin.BoolValue = value;

                // If the page has a serial join sig then set the value to the name of the page
                if (value == true && this.TitleLabel != null)
                    this.TitleLabel.Text = this.Name;
            }
        }

        public UIViewBase(UIKey key, BoolInputSig visibleJoinSig)
        {
            this.Key = key;
            this.Name = "";
            this.VisibleJoin = visibleJoinSig;
        }

        public UIViewBase(UIKey key, BoolInputSig visibleJoinSig, UILabel titleLabel, string name)
        {
            this.Key = key;
            this._name = name;
            this.VisibleJoin = visibleJoinSig;
            this.TitleLabel = titleLabel;
        }

        public virtual void Show()
        {
            this.Visible = true;
        }

        public virtual void Hide()
        {
            this.Visible = false;
        }
    }
}