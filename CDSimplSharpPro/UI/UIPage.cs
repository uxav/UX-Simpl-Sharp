using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UIPage
    {
        public string KeyName { get; private set; }
        string _name;
        public string Name
        {
            set
            {
                // Set the value
                this._name = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.NameSerialJoin != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.IsShowing)
                        this.NameSerialJoin.StringValue = this._name;
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
        public bool IsShowing
        {
            get
            {
                if (this.JoinGroup.CurrentSig == this.VisibleJoin)
                    return true;
                return false;
            }
        }

        BoolInputSig VisibleJoin;
        BoolInputSigInterlock JoinGroup;
        StringInputSig NameSerialJoin;

        public UIPage(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup)
        {
            this.KeyName = key;
            this.Name = "";
            this.VisibleJoin = visibleJoinSig;
            this.JoinGroup = pageVisibleJoinSigGroup;
            pageVisibleJoinSigGroup.Add(this.VisibleJoin);
        }

        public UIPage(string key, BoolInputSig visibleJoinSig, BoolInputSigInterlock pageVisibleJoinSigGroup, StringInputSig nameStringInputSig, string name)
        {
            this.KeyName = key;
            this._name = name;
            this.VisibleJoin = visibleJoinSig;
            this.JoinGroup = pageVisibleJoinSigGroup;
            pageVisibleJoinSigGroup.Add(this.VisibleJoin);
            this.NameSerialJoin = nameStringInputSig;
        }

        public void Show()
        {
            this.JoinGroup.Set(this.VisibleJoin);

            // If the page has a serial join sig then set the value to the name of the page
            if (this.NameSerialJoin != null)
                this.NameSerialJoin.StringValue = this._name;
        }
    }
}