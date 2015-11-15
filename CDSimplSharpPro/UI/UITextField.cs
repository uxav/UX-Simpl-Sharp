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
        BoolOutputSig EnterJoin;
        BoolOutputSig EscJoin;
        BoolInputSig EnableJoin;
        BoolInputSig VisibleJoin;
        StringInputSig TextJoinToDevice;
        StringOutputSig TextJoinFromDevice;
        BasicTriList Device;
        UIButton ClearButton;

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

        public UITextField(BoolOutputSig hasFocusJoin, BoolInputSig setFocusJoinOn, BoolInputSig setFocusJoinOff, BoolOutputSig enterJoin, BoolOutputSig escJoin,
            BoolInputSig enableJoin, BoolInputSig visibleJoin, StringInputSig textJoinToDevice, StringOutputSig textJoinFromDevice, UIButton clearButton)
        {
            HasFocusJoin = hasFocusJoin;
            SetFocusJoinOn = setFocusJoinOn;
            SetFocusJoinOff = setFocusJoinOff;
            EnterJoin = enterJoin;
            EscJoin = escJoin;
            EnableJoin = enableJoin;
            if (EnableJoin != null)
                EnableJoin.BoolValue = true;
            VisibleJoin = visibleJoin;
            if (VisibleJoin != null)
                VisibleJoin.BoolValue = true;
            TextJoinToDevice = textJoinToDevice;
            TextJoinFromDevice = textJoinFromDevice;
            Device = hasFocusJoin.Owner as BasicTriList;
            ClearButton = clearButton;
            if (ClearButton != null)
                ClearButton.ButtonEvent += new UIButtonEventHandler(ClearButton_ButtonEvent);
            Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        void ClearButton_ButtonEvent(UIButton button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Pressed)
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
                    }
                    break;
                case eSigType.Bool:
                    if (args.Sig == HasFocusJoin)
                    {
                        this.HasFocus = args.Sig.BoolValue;
                    }
                    else if (args.Sig == EnterJoin && args.Sig.BoolValue)
                    {
                        this.HasFocus = false;
                        if (this.TextFieldEvent != null)
                            this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.Entered, this.HasFocus, this.Text));
                    }
                    else if (args.Sig == EscJoin && args.Sig.BoolValue)
                    {
                        this.HasFocus = false;
                        this.Text = PreviousValue;
                        if (this.TextFieldEvent != null)
                            this.TextFieldEvent(this, new UITextFieldEventArgs(eUITextFieldEventType.Escaped, this.HasFocus, this.Text));
                    }
                    else if (args.Sig.Number == ClearButton.JoinNumber)
                    {
                        ClearButton.Down = args.Sig.BoolValue;
                    }
                    break;
            }
        }

        public void AppendText(string textToAppend)
        {
            this.Text = this.Text + textToAppend;
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