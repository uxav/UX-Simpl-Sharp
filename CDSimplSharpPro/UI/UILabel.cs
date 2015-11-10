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
        public UIKey Key { get; private set; }
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

        public UILabel(UIKey key, BasicTriList device, uint joinNumber)
        {
            this.Key = key;
            this._Text = "Label";
            if (joinNumber > 0)
            {
                this.SerialFeedbackJoin = device.StringInput[joinNumber];
                this.SerialFeedbackJoin.StringValue = this._Text;
            }
        }

        public UILabel(UIKey key, BasicTriList device, uint joinNumber, uint enableJoin, uint visibleJoin)
        {
            this.Key = key;
            this._Text = "Label";
            if (joinNumber > 0)
            {
                this.SerialFeedbackJoin = device.StringInput[joinNumber];
                this.SerialFeedbackJoin.StringValue = this._Text;
            }
            if (enableJoin > 0)
            {
                this.EnableJoin = device.BooleanInput[enableJoin];
                this.EnableJoin.BoolValue = true;
            }
            if (visibleJoin > 0)
            {
                this.VisibleJoin = device.BooleanInput[visibleJoin];
                this.VisibleJoin.BoolValue = true;
            }
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