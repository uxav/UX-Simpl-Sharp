using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIViewController
    {
        public UIViewBase View;
        public event UIViewControllerEventHandler VisibilityChange;
        public List<object> ViewObjects;

        public UIViewController(UIViewBase view)
        {
            this.View = view;
            this.View.VisibilityChange += new UIViewBaseVisibitlityEventHandler(View_VisibilityChange);
            this.ViewObjects = new List<object>();
        }

        void View_VisibilityChange(UIViewBase sender, UIViewVisibilityEventArgs args)
        {
            if (args.EventType == eViewEventType.DidShow)
                this.OnShow();
            else if (args.EventType == eViewEventType.DidHide)
                this.OnHide();
        }

        protected virtual void Show()
        {
            this.View.Show();
        }

        protected virtual void Hide()
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

        public void Dispose()
        {
            this.View.VisibilityChange -= new UIViewBaseVisibitlityEventHandler(View_VisibilityChange);
            this.ViewObjects = null;
        }
    }

    public delegate void UIViewControllerEventHandler(UIViewController sender, UIViewVisibilityEventArgs args);
}