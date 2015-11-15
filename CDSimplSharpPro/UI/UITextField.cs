using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.CrestronThread;

namespace CDSimplSharpPro.UI
{
    public class UITextField
    {
        BoolOutputSig HasFocusJoin;
        BoolInputSig SetFocusJoinOn;
        BoolInputSig SetFocusJoinOff;
        BoolInputSig EnableJoin;
        BoolInputSig VisibleJoin;
        StringInputSig TextJoinToDevice;
        StringOutputSig TextJoinFromDevice;
        BasicTriList Device;
        UILabel TitleLabel;
        public UIButton EnterButton;
        public UIButton EscButton;
        public UIButton ClearButton;

        public event UItextFieldEventHandler TextFieldEvent;

        public bool Visible
        {
            set
            {
                VisibleJoin.BoolValue = value;
            }
            get
            {
                return VisibleJoin.BoolValue;
            }
        }

        public bool Enabled
        {
            set
            {
                EnableJoin.BoolValue = value;
            }
            get
            {
                return EnableJoin.BoolValue;
            }
        }

        string _Text;
        public string Text
        {
            set
            {
                if (_Text == null)
                    _Text = "";
                if (!_Text.Equals(value))
                {
                    _Text = String.Copy(value);
                    TextJoinToDevice.StringValue = _Text;
                    if (_Text.Length > 0 && ClearButton != null)
                        ClearButton.Visible = true;
                    else if (ClearButton != null)
                        ClearButton.Visible = false;
                    if (this.TextFieldEvent != null)
                        this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.TextChanged, this.HasFocus, this.Text));
                }
            }
            get
            {
                return _Text;
            }
        }

        private string PreviousValue { set; get; }

        bool _HasFocus;
        public bool HasFocus
        {
            set
            {
                if (_HasFocus != value)
                {
                    _HasFocus = value;
                    if (value)
                    {
                        if (this.TextFieldEvent != null)
                            this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.WillGoOnFocus, true, this.Text));
                        Thread.Sleep(200);
                        SetFocusJoinOn.Pulse();
                        PreviousValue = String.Copy(this.Text);
                        if (this.TextFieldEvent != null)
                            this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.OnFocus, true, this.Text));
                    }
                    else
                    {
                        SetFocusJoinOff.Pulse();
                        if (this.TextFieldEvent != null)
                            this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.OffFocus, true, this.Text));
                    }
                }
            }
            get
            {
                return _HasFocus;
            }
        }

        public string Title
        {
            set
            {
                if (TitleLabel != null)
                    TitleLabel.Text = value;
            }
            get
            {
                if (TitleLabel != null)
                    return TitleLabel.Text;
                return "";
            }
        }

        public UITextField(BoolOutputSig hasFocusJoin, BoolInputSig setFocusJoinOn, BoolInputSig setFocusJoinOff,
            BoolInputSig enableJoin, BoolInputSig visibleJoin, StringInputSig textJoinToDevice,
            StringOutputSig textJoinFromDevice, UILabel titleLabel, UIButton enterButton, UIButton escButton, UIButton clearButton)
        {
            HasFocusJoin = hasFocusJoin;
            SetFocusJoinOn = setFocusJoinOn;
            SetFocusJoinOff = setFocusJoinOff;
            EnableJoin = enableJoin;
            if (EnableJoin != null)
                EnableJoin.BoolValue = true;
            VisibleJoin = visibleJoin;
            if (VisibleJoin != null)
                VisibleJoin.BoolValue = true;
            TextJoinToDevice = textJoinToDevice;
            TextJoinFromDevice = textJoinFromDevice;
            Device = hasFocusJoin.Owner as BasicTriList;
            TitleLabel = titleLabel;
            this.EnterButton = enterButton;
            if (EnterButton != null)
            {
                this.EnterButton.Title = "Enter";
                this.EnterButton.ButtonEvent += new UIButtonEventHandler(EnterButton_ButtonEvent);
            }
            this.EscButton = escButton;
            if (EscButton != null)
            {
                this.EscButton.Title = "Escape";
                this.EscButton.ButtonEvent += new UIButtonEventHandler(EscButton_ButtonEvent);
            }
            this.ClearButton = clearButton;
            if (ClearButton != null)
                this.ClearButton.ButtonEvent += new UIButtonEventHandler(ClearButton_ButtonEvent);
            Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public void Setup(string title, string startText)
        {
            this.Title = title;
            this.Text = startText;
        }

        public void Setup(string title, string startText, string enterButtonTitle, string escButtonTitle)
        {
            this.Title = title;
            this.Text = startText;
            this.EnterButton.Title = enterButtonTitle;
            this.EscButton.Title = escButtonTitle;
        }

        void EnterButton_ButtonEvent(UIButtonBase button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Tapped)
            {
                this.HasFocus = false;
                if (this.TextFieldEvent != null)
                    this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.Entered, this.HasFocus, this.Text));
            }
        }

        void EscButton_ButtonEvent(UIButtonBase button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Tapped)
            {
                this.HasFocus = false;
                this.Text = PreviousValue;
                if (this.TextFieldEvent != null)
                    this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.Escaped, this.HasFocus, this.Text));
            }
        }

        void ClearButton_ButtonEvent(UIButtonBase button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Tapped)
            {
                this.Text = "";
                if (this.TextFieldEvent != null)
                    this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.ClearedByUser, this.HasFocus, this.Text));
            }
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.String:
                    if (args.Sig == TextJoinFromDevice)
                    {
                        this.Text = args.Sig.StringValue;
                        return;
                    }
                    break;
                case eSigType.Bool:
                    if (args.Sig == HasFocusJoin)
                    {
                        this.HasFocus = args.Sig.BoolValue;
                        return;
                    }
                    break;
            }
        }

        public void AppendText(string textToAppend)
        {
            this.Text = this.Text + textToAppend;
        }

        public void BackSpace()
        {
            if (this.Text.Length > 0)
            {
                this.Text = this.Text.Remove(this.Text.Length - 1, 1);
            }
        }
    }

    public delegate void UItextFieldEventHandler(UITextField textField, UITextFieldEventArgs args);

    public class UITextFieldEventArgs : EventArgs
    {
        public eUITextFieldEventType EventType;
        public string CurrentText;
        public bool HasFocus;
        public UITextFieldEventArgs(eUITextFieldEventType type, bool hasFocus, string currentText)
            : base()
        {
            this.EventType = type;
            this.HasFocus = hasFocus;
            this.CurrentText = currentText;
        }
    }

    public enum eUITextFieldEventType
    {
        WillGoOnFocus,
        OnFocus,
        OffFocus,
        Escaped,
        Entered,
        TextChanged,
        ClearedByUser
    }
}