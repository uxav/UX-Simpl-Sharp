using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIViewBase : UIObject
    {
        public UIViewBase(BoolInputSig visibleDigitalJoin)
        {
            this.VisibleDigitalJoin = visibleDigitalJoin;
            this.SubscribeToSigChanges();
        }

        public UIViewBase(BoolInputSig visibleDigitalJoin, UILabel titleLabel)
            : this(visibleDigitalJoin)
        {
            this.TitleLabel = titleLabel;
        }

        public UIViewBase(BoolInputSig visibleDigitalJoin, UILabel titleLabel, UILabel subTitleLabel)
            : this(visibleDigitalJoin, titleLabel)
        {
            this.SubTitleLabel = subTitleLabel;
        }

        public UILabel TitleLabel { get; protected set; }
        public UILabel SubTitleLabel { get; protected set; }
        
        string _title;
        public string Title
        {
            set
            {
                // Set the value
                this._title = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.TitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.Visible)
                        this.TitleLabel.Text = this._title;
                }
            }
            get
            {
                return this._title;
            }
        }

        string _subTitle;
        public string SubTitle
        {
            set
            {
                // Set the value
                this._subTitle = value;

                // if the page has a serial join sig assigned for the Name:
                if (this.SubTitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (this.Visible)
                        this.SubTitleLabel.Text = this._subTitle;
                }
            }
            get
            {
                return this._subTitle;
            }
        }

        public uint VisibleJoinNumber
        {
            get
            {
                return this.VisibleDigitalJoin.Number;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                if (this.Visible != value)
                {
                    if (value && this.VisibilityChange != null)
                        this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillShow));
                    if (!value && this.VisibilityChange != null)
                        this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillHide));

                    base.Visible = value;
                }
            }
        }
        
        public virtual event UIViewBaseVisibitlityEventHandler VisibilityChange;

        protected override void OnShow()
        {
            // If the page has a serial join sig then set the value to the name of the page
            if (this.TitleLabel != null)
                this.TitleLabel.Text = this.Title;
            if (this.SubTitleLabel != null)
                this.SubTitleLabel.Text = this.SubTitle;

            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidShow));
        }

        protected override void OnHide()
        {
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidHide));
        }

        public void SetTransitionCompleteJoin(BoolOutputSig transitionCompleteDigitalJoin)
        {
            this.TransitionCompleteDigitalJoin = transitionCompleteDigitalJoin;
        }

        protected override void OnTransitionComplete()
        {
            base.OnTransitionComplete();
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.TransitionComplete));
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