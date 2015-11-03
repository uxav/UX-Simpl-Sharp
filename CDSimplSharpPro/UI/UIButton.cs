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
        public bool Enable
        {
            set
            {
                this.EnableJoin.BoolValue = value;
            }
            get
            {
                return this.EnableJoin.BoolValue;
            }
        }
        public bool Visible
        {
            set
            {
                this.VisibleJoin.BoolValue = value;
            }
            get
            {
                return this.VisibleJoin.BoolValue;
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

        public uint JoinNumber
        {
            get
            {
                return this.Join.Number;
            }
        }

        public event UIButtonEventHandler ButtonEvent;

        public UIButton(BasicTriList device, uint joinNumber)
        {
            this._title = string.Format("Button {0}", joinNumber);
            this.HoldTime = 500;
            if (joinNumber > 0)
            {
                this.Join = device.BooleanInput[joinNumber];
                this.SerialFeedbackJoin = device.StringInput[joinNumber];
                this.SerialFeedbackJoin.StringValue = this._title;
            }
        }

        public UIButton(BasicTriList device, uint joinNumber, uint enableJoin, uint visibleJoin)
        {
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

        private void HoldTimerUpdate(object obj)
        {
            this.CurrentHoldTime = this.CurrentHoldTime + 100;

            if (this.CurrentHoldTime == this.HoldTime)
            {
                this.HoldTimer.Dispose();

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