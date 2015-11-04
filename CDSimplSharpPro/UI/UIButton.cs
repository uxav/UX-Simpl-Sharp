using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIButton
    {
        public string KeyName { get; private set; }
        string _title;
        public string Title
        {
            set
            {
                this._title = value;
                this.SerialFeedbackJoin.StringValue = this._title;
            }
            get
            {
                return this._title;
            }
        }
        public bool Feedback
        {
            set
            {
                this.Join.BoolValue = value;
            }
            get
            {
                return this.Join.BoolValue;
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
                if(this.EnableJoin != null)
                    return this.EnableJoin.BoolValue;
                return true;
            }
        }
        public bool Visible
        {
            set
            {
                if(this.VisibleJoin != null)
                    this.VisibleJoin.BoolValue = value;
            }
            get
            {
                if(this.VisibleJoin != null)
                    return this.VisibleJoin.BoolValue;
                return true;
            }
        }
        public bool Down
        {
            set
            {
                if (value == true)
                {
                    if (this.HoldTime > 0 && this.HoldTimer == null || this.HoldTimer.Disposed)
                    {
                        this.CurrentHoldTime = 0;
                        this.HoldTimer = new CTimer(HoldTimerUpdate, null, 100, 100);
                    }

                    if (this.ButtonEvent != null)
                    {
                        this.ButtonEvent(this, new UIButtonEventArgs(eUIButtonEventType.Pressed, this.CurrentHoldTime));
                    }
                }
                else if (value == false)
                {
                    if(this.HoldTimer != null)
                        this.HoldTimer.Dispose();

                    if (this.PageToShowOnRelease != null)
                    {
                        this.PageToShowOnRelease.Show();
                    }

                    if (this.ButtonEvent != null)
                    {
                        this.ButtonEvent(this, new UIButtonEventArgs(eUIButtonEventType.Released, this.CurrentHoldTime));
                    }

                    if (this.CurrentHoldTime < this.HoldTime)
                    {
                        if (this.ButtonEvent != null)
                        {
                            this.ButtonEvent(this, new UIButtonEventArgs(eUIButtonEventType.Tapped, this.CurrentHoldTime));
                        }
                    }
                }
            }
        }
        private CTimer HoldTimer;
        private long CurrentHoldTime;
        public long HoldTime;

        BoolInputSig Join;
        StringInputSig SerialFeedbackJoin;
        BoolInputSig EnableJoin;
        BoolInputSig VisibleJoin;

        public UIPage PageToShowOnRelease;

        public uint JoinNumber
        {
            get
            {
                return this.Join.Number;
            }
        }

        public event UIButtonEventHandler ButtonEvent;

        public UIButton(string keyName, BasicTriList device, uint joinNumber)
        {
            this.KeyName = keyName;
            this._title = string.Format("Button {0}", joinNumber);
            this.HoldTime = 500;
            if (joinNumber > 0)
            {
                this.Join = device.BooleanInput[joinNumber];
                this.SerialFeedbackJoin = device.StringInput[joinNumber];
                this.SerialFeedbackJoin.StringValue = this._title;
            }
        }

        public UIButton(string keyName, BasicTriList device, uint joinNumber, uint enableJoin, uint visibleJoin)
        {
            this.KeyName = keyName;
            this._title = string.Format("Button {0}", joinNumber);
            this.HoldTime = 500;
            if (joinNumber > 0)
            {
                this.Join = device.BooleanInput[joinNumber];
                this.SerialFeedbackJoin = device.StringInput[joinNumber];
                this.SerialFeedbackJoin.StringValue = this._title;
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

        private void HoldTimerUpdate(object obj)
        {
            this.CurrentHoldTime = this.CurrentHoldTime + 100;

            if (this.CurrentHoldTime == this.HoldTime)
            {
                if (this.ButtonEvent != null)
                {
                    this.ButtonEvent(this, new UIButtonEventArgs(eUIButtonEventType.Held, this.CurrentHoldTime));
                }
            }
        }
    }

    public delegate void UIButtonEventHandler(UIButton button, UIButtonEventArgs args);

    public class UIButtonEventArgs : EventArgs
    {
        public eUIButtonEventType EventType;
        public long HoldTime;
        public UIButtonEventArgs(eUIButtonEventType type, long holdTime)
        {
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }

    public enum eUIButtonEventType
    {
        Pressed,
        Tapped,
        Held,
        Released
    }
}