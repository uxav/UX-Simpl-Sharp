using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIViewBase
    {
        public uint ID
        {
            get { return this.VisibleJoinNumber; }
        }
        public BoolInputSig VisibleJoin { get; protected set; }
        protected BoolInputSig TransitionCompleteJoin;
        public UILabel TitleLabel { get; protected set; }
        public UILabel SubTitleLabel { get; protected set; }
        public event UIViewBaseVisibitlityEventHandler VisibilityChange;
        string _Title;
        public string Title
        {
            set
            {
                // Set the value
                this._Title = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.TitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.Visible)
                        this.TitleLabel.Text = this._Title;
                }
            }
            get
            {
                return this._Title;
            }
        }
        string _SubTitle;
        public string SubTitle
        {
            set
            {
                // Set the value
                this._SubTitle = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.SubTitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.Visible)
                        this.SubTitleLabel.Text = this._SubTitle;
                }
            }
            get
            {
                return this._SubTitle;
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
            private set
            {
                if (this.VisibleJoin.BoolValue != value)
                {
                    if (value && this.VisibilityChange != null)
                        this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillShow));
                    if (!value && this.VisibilityChange != null)
                        this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillHide));
            
                    this.VisibleJoin.BoolValue = value;

                    if (value)
                        OnShow();
                    else
                        OnHide();
                }
            }
        }

        public UIViewBase(BoolInputSig visibleJoinSig)
        {
            this.Title = "";
            this.VisibleJoin = visibleJoinSig;
        }

        public UIViewBase(BoolInputSig visibleJoinSig, UILabel titleLabel)
        {
            this._Title = "";
            this.VisibleJoin = visibleJoinSig;
            this.TitleLabel = titleLabel;
        }

        public UIViewBase(BoolInputSig visibleJoinSig, UILabel titleLabel, string title)
        {
            this._Title = title;
            this.VisibleJoin = visibleJoinSig;
            this.TitleLabel = titleLabel;
        }

        public UIViewBase(BoolInputSig visibleJoinSig, UILabel titleLabel, UILabel subTitleLabel)
        {
            this._Title = "";
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

        protected virtual void OnShow()
        {
            // If the page has a serial join sig then set the value to the name of the page
            if (this.TitleLabel != null)
                this.TitleLabel.Text = this.Title;

            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidShow));
        }

        protected virtual void OnHide()
        {
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidHide));
        }

        public void SetTransitionCompleteJoin(BoolInputSig inputSig)
        {
            this.TransitionCompleteJoin = inputSig;
            BasicTriList device = this.TransitionCompleteJoin.Owner as BasicTriList;
            device.SigChange += new SigEventHandler(device_SigChange);
        }

        void device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool && args.Sig.Number == this.TransitionCompleteJoin.Number)
            {
                OnTransitionComplete();
            }
        }

        protected virtual void OnTransitionComplete()
        {
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.TransitionComplete));
        }

        public virtual void Dispose()
        {
            if (this.TransitionCompleteJoin != null)
            {
                BasicTriList device = this.TransitionCompleteJoin.Owner as BasicTriList;
                device.SigChange -= new SigEventHandler(device_SigChange);
            }
        }
    }

    public delegate void UIViewBaseVisibitlityEventHandler(UIViewBase sender, UIViewVisibilityEventArgs args);

    public class UIViewVisibilityEventArgs : EventArgs
    {
        public eViewEventType EventType;

        public UIViewVisibilityEventArgs(eViewEventType eventType)
            : base()
        {
            this.EventType = eventType;
        }
    }

    public enum eViewEventType
    {
        WillShow,
        DidShow,
        TransitionComplete,
        WillHide,
        DidHide
    }
}