using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.UI
{
    public class UITextField : IDisposable
    {
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
            TextJoinToDevice.StringValue = "";
            TextJoinFromDevice = textJoinFromDevice;
            Device = hasFocusJoin.Owner as BasicTriList;
            TitleLabel = titleLabel;
            this.EnterButton = enterButton;
            this.EnterButton.Title = "Enter";
            this.EscButton = escButton;
            this.EscButton.Title = "Escape";
            this.ClearButton = clearButton;
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
                {
                    _Text = "";
                    TextJoinToDevice.StringValue = _Text;
                    if (ClearButton != null)
                        ClearButton.Visible = false;
                }
                if (!_Text.Equals(value))
                {
                    _Text = String.Copy(value);
                    if (!TextJoinFromDevice.StringValue.Equals(value) || value == "")
                        TextJoinToDevice.StringValue = _Text;
                    if (_Text.Length > 0 && ClearButton != null)
                        ClearButton.Visible = true;
                    else if (ClearButton != null)
                        ClearButton.Visible = false;
                    OnTextFieldEvent(UITextFieldEventType.TextChanged);
                }
            }
            get
            {
                return _Text;
            }
        }

        private string InitialText { set; get; }

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
                        SetFocusJoinOn.Pulse();
                        InitialText = String.Copy(this.Text);
                        OnTextFieldEvent(UITextFieldEventType.OnFocus);
                    }
                    else
                    {
                        SetFocusJoinOff.Pulse();
                        OnTextFieldEvent(UITextFieldEventType.OffFocus);
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

        protected virtual void OnTextFieldEvent(UITextFieldEventType eventType)
        {
            if (_TextFieldEvent != null)
            {
                _TextFieldEvent(this, new UITextFieldEventArgs(eventType, this.HasFocus, this.InitialText, this.Text));
            }
        }

        private event UITextFieldEventHandler _TextFieldEvent;

        int subscribeCount = 0;

        public event UITextFieldEventHandler TextFieldEvent
        {
            add
            {
                if (subscribeCount == 0)
                {
                    if (EnterButton != null)
                        this.EnterButton.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
                    if (EscButton != null)
                        this.EscButton.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
                    if (ClearButton != null)
                        this.ClearButton.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
                    Device.SigChange += new SigEventHandler(Device_SigChange);
                }

                subscribeCount++;

                _TextFieldEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                {
                    if (EnterButton != null)
                        this.EnterButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                    if (EscButton != null)
                        this.EscButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                    if (ClearButton != null)
                        this.ClearButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                    Device.SigChange -= new SigEventHandler(Device_SigChange);
                }

                _TextFieldEvent -= value;
            }
        }

        protected virtual void OnButtonEvent(UIObject currentObject, UIObjectButtonEventArgs args)
        {
            UIButton button = currentObject as UIButton;

            if (args.EventType == UIButtonEventType.Released)
            {
                if (button == this.EnterButton)
                {
                    this.HasFocus = false;
                    OnTextFieldEvent(UITextFieldEventType.Entered);
                }
                else if (button == this.EscButton)
                {
                    this.HasFocus = false;
                    this.Text = InitialText;
                    OnTextFieldEvent(UITextFieldEventType.Escaped);
                }
                else if (button == this.ClearButton)
                {
                    this.Text = "";
                    OnTextFieldEvent(UITextFieldEventType.ClearedByUser);
                }
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

        public void Escape()
        {
            this.HasFocus = false;
            this.Text = InitialText;
            OnTextFieldEvent(UITextFieldEventType.Escaped);
        }

        public void Dispose()
        {
            Text = "";
            TextJoinToDevice.StringValue = "";
            Device.SigChange -= new SigEventHandler(Device_SigChange);
            this.EnterButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
            this.EnterButton.Dispose();
            this.EscButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
            this.EscButton.Dispose();
            this.ClearButton.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
            this.ClearButton.Dispose();
        }
    }

    public delegate void UITextFieldEventHandler(UITextField textField, UITextFieldEventArgs args);

    public class UITextFieldEventArgs : EventArgs
    {
        public UITextFieldEventType EventType;
        public string InitialText;
        public string CurrentText;
        public bool HasFocus;
        public UITextFieldEventArgs(UITextFieldEventType type, bool hasFocus, string initialText, string currentText)
            : base()
        {
            this.EventType = type;
            this.HasFocus = hasFocus;
            this.InitialText = initialText;
            this.CurrentText = currentText;
        }
    }

    public enum UITextFieldEventType
    {
        OnFocus,
        OffFocus,
        Escaped,
        Entered,
        TextChanged,
        ClearedByUser
    }
}