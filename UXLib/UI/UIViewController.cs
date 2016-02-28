using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UIViewController : IDisposable
    {
        public UIViewBase View;
        public event UIViewControllerEventHandler VisibilityChange;
        public UIController UIController { get; protected set; }

        public bool Visible
        {
            get { return View.Visible; }
        }

        public UIViewController(UIController uiController, UIViewBase view)
        {
            this.UIController = uiController;
            this.View = view;
            this.View.VisibilityChange += new UIViewBaseVisibitlityEventHandler(View_VisibilityChange);
        }

        public UIViewController(UIController uiController, UIViewController ownerViewController)
            : this(uiController, ownerViewController.View)
        {
        }

        void View_VisibilityChange(UIViewBase sender, UIViewVisibilityEventArgs args)
        {
            if (args.EventType == eViewEventType.DidShow)
                this.OnShow();
            else if (args.EventType == eViewEventType.DidHide)
                this.OnHide();
            else if (this.VisibilityChange != null)
                this.VisibilityChange(this, args);
        }

        public string Title
        {
            get
            {
                return this.View.Title;
            }
            set
            {
                this.View.Title = value;
            }
        }

        public virtual void Show()
        {
            this.View.Show();
        }

        public virtual void Hide()
        {
            this.View.Hide();
        }

        protected virtual void OnShow()
        {
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidShow));
        }

        protected virtual void OnHide()
        {
            if (this.VisibilityChange != null)
                this.VisibilityChange(this, new UIViewVisibilityEventArgs(eViewEventType.DidHide));
        }

        public uint VisibleJoinNumber
        {
            get
            {
                return this.View.VisibleJoinNumber;
            }
        }

        public virtual void Dispose()
        {
            this.View.VisibilityChange -= new UIViewBaseVisibitlityEventHandler(View_VisibilityChange);
        }
    }

    public delegate void UIViewControllerEventHandler(UIViewController sender, UIViewVisibilityEventArgs args);
}