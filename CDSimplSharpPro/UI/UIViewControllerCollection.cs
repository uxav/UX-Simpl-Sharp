using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIViewControllerCollection : IEnumerable<UIViewController>
    {
        protected List<UIViewController> ViewControllers;
        public UITimeOut ViewTimeOut;

        public UIViewController this[uint joinNumber]
        {
            get
            {
                return this.ViewControllers.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UIViewController CurrentView
        {
            get
            {
                return this.ViewControllers.FirstOrDefault(p => p.Visible == true);
            }
        }

        public UIViewControllerCollection()
        {
            ViewControllers = new List<UIViewController>();
        }

        public UIViewControllerCollection(UITimeOut timeout)
        {
            ViewControllers = new List<UIViewController>();
            this.ViewTimeOut = timeout;
        }

        public void Add(UIViewController newView)
        {
            ViewControllers.Add(newView);
            newView.VisibilityChange += new UIViewControllerEventHandler(ViewController_VisibilityChange);
        }

        public void ShowOnly(UIViewController newView)
        {
            foreach (UIViewController view in ViewControllers)
            {
                if (view != newView)
                {
                    view.Hide();
                }
            }

            if (ViewControllers.Contains(newView))
            {
                newView.Show();
            }
        }

        public UIViewController GetCurrentView()
        {
            return ViewControllers.FirstOrDefault(v => v.Visible);
        }

        void ViewController_VisibilityChange(UIViewController sender, UIViewVisibilityEventArgs args)
        {
            if (sender.Visible && this.ViewTimeOut != null)
            {
                this.ViewTimeOut.Set();
            }
        }

        public IEnumerator<UIViewController> GetEnumerator()
        {
            return this.ViewControllers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual void Dispose()
        {
            this.ViewTimeOut.Dispose();

            foreach (UIViewController view in ViewControllers)
            {
                view.VisibilityChange -= new UIViewControllerEventHandler(ViewController_VisibilityChange);
                view.Dispose();
            }

            this.ViewControllers.Clear();
            this.ViewControllers = null;
        }
    }
}