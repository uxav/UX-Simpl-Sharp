using System;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UIViewBase : UIObject
    {
        public UIViewBase(BoolInputSig visibleDigitalJoin)
        {
            VisibleDigitalJoin = visibleDigitalJoin;
            SubscribeToSigChanges();
        }

        public UIViewBase(BoolInputSig visibleDigitalJoin, UILabel titleLabel)
            : this(visibleDigitalJoin)
        {
            TitleLabel = titleLabel;
        }

        public UIViewBase(BoolInputSig visibleDigitalJoin, UILabel titleLabel, UILabel subTitleLabel)
            : this(visibleDigitalJoin, titleLabel)
        {
            SubTitleLabel = subTitleLabel;
        }

        public UILabel TitleLabel { get; protected set; }
        public UILabel SubTitleLabel { get; protected set; }
        public UIDynamicIcon TitleIcon { get; protected set; }

        public void SetTitleIcon(UIDynamicIcon icon)
        {
            TitleIcon = icon;
        }

        public void SetTitleIcon(UIDynamicIcon icon, string defaultValue)
        {
            TitleIcon = icon;
            TitleIconName = defaultValue;
        }

        string _title;
        public string Title
        {
            set
            {
                // Set the value
                _title = value;

                // if the page has a serial join sig assigned for the Name:
                if (TitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (Visible)
                        TitleLabel.Text = _title;
                }
            }
            get
            {
                return _title;
            }
        }

        string _subTitle;
        public string SubTitle
        {
            set
            {
                // Set the value
                _subTitle = value;

                // if the page has a serial join sig assigned for the Name:
                if (SubTitleLabel != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (Visible)
                        SubTitleLabel.Text = _subTitle;
                }
            }
            get
            {
                return _subTitle;
            }
        }

        string _titleIconName;
        public string TitleIconName
        {
            set
            {
                // Set the value
                _titleIconName = value;

                // if the page has a serial join sig assigned for the Name:
                if (TitleIcon != null)
                {
                    // set the string value of the serial join only if the page is showing
                    // this allows for you to use the same serial join number as it sends updates the name when a page is shown
                    if (Visible)
                        TitleIcon.Icon = _titleIconName;
                }
            }
            get
            {
                return _titleIconName;
            }
        }

        public uint VisibleJoinNumber
        {
            get
            {
                return VisibleDigitalJoin.Number;
            }
        }

        bool visibleInTransition = false;

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                if (Visible != value && !visibleInTransition)
                {
                    visibleInTransition = true;
                    if (value && VisibilityChange != null)
                        VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillShow));
                    if (!value && VisibilityChange != null)
                        VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.WillHide));

                    base.Visible = value;
                    visibleInTransition = false;
                }
            }
        }
        
        public virtual event UIViewBaseVisibitlityEventHandler VisibilityChange;

        protected override void OnShow()
        {
            // If the page has a serial join sig then set the value to the name of the page
            if (TitleLabel != null)
                TitleLabel.Text = Title;
            if (SubTitleLabel != null)
                SubTitleLabel.Text = SubTitle;
            if (TitleIcon != null)
                TitleIcon.Icon = TitleIconName;

            if (VisibilityChange != null)
                VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidShow));
        }

        protected override void OnHide()
        {
            if (VisibilityChange != null)
                VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidHide));
        }

        public void SetTransitionCompleteJoin(BoolOutputSig transitionCompleteDigitalJoin)
        {
            TransitionCompleteDigitalJoin = transitionCompleteDigitalJoin;
        }

        protected override void OnTransitionComplete()
        {
            base.OnTransitionComplete();
            if (VisibilityChange != null)
                VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.TransitionComplete));
        }
    }

    public delegate void UIViewBaseVisibitlityEventHandler(UIViewBase sender, UIViewVisibilityEventArgs args);

    public class UIViewVisibilityEventArgs : EventArgs
    {
        public eViewEventType EventType;

        public UIViewVisibilityEventArgs(eViewEventType eventType)
            : base()
        {
            EventType = eventType;
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