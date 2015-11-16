using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UILabel
    {
        public uint ID
        {
            get { return this.SerialFeedbackJoin.Number; }
        }
        string _Text;
        public string Text
        {
            set
            {
                this._Text = value;
                this.SerialFeedbackJoin.StringValue = this._Text;
            }
            get
            {
                return this._Text;
            }
        }

        public bool Enabled
        {
            set
            {
                if (this.EnableJoin != null)
                    this.EnableJoin.BoolValue = value;
            }
            get
            {
                if (this.EnableJoin != null)
                    return this.EnableJoin.BoolValue;
                return true;
            }
        }
        public bool Visible
        {
            set
            {
                if (this.VisibleJoin != null)
                    this.VisibleJoin.BoolValue = value;
            }
            get
            {
                if (this.VisibleJoin != null)
                    return this.VisibleJoin.BoolValue;
                return true;
            }
        }

        public uint JoinNumber
        {
            get
            {
                return this.SerialFeedbackJoin.Number;
            }
        }

        StringInputSig SerialFeedbackJoin;
        BoolInputSig EnableJoin;
        BoolInputSig VisibleJoin;

        public UILabel(StringInputSig stringInputSig)
        {
            this._Text = "Label";
            this.SerialFeedbackJoin = stringInputSig;
            this.SerialFeedbackJoin.StringValue = this._Text;
        }

        public UILabel(StringInputSig stringInputSig, string defaultText)
        {
            this._Text = defaultText;
            this.SerialFeedbackJoin = stringInputSig;
            this.SerialFeedbackJoin.StringValue = this._Text;
        }

        public UILabel(StringInputSig stringInputSig, BoolInputSig enableJoinSig, BoolInputSig visibleJoinSig)
        {
            this._Text = "Label";
            this.SerialFeedbackJoin = stringInputSig;
            this.SerialFeedbackJoin.StringValue = this._Text;
            this.EnableJoin = enableJoinSig;
            if (this.EnableJoin != null)
                this.Enable();
            this.VisibleJoin = visibleJoinSig;
            if (this.VisibleJoin != null)
                this.Show();
        }

        public UILabel(StringInputSig stringInputSig, string defaultText, BoolInputSig enableJoinSig, BoolInputSig visibleJoinSig)
        {
            this._Text = defaultText;
            this.SerialFeedbackJoin = stringInputSig;
            this.SerialFeedbackJoin.StringValue = this._Text;
            this.EnableJoin = enableJoinSig;
            if (this.EnableJoin != null)
                this.Enable();
            this.VisibleJoin = visibleJoinSig;
            if (this.VisibleJoin != null)
                this.Show();
        }

        public void Show()
        {
            this.Visible = true;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Enable()
        {
            this.Enabled = true;
        }

        public void Disable()
        {
            this.Enabled = false;
        }
    }
}