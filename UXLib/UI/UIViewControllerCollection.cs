using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIViewControllerCollection : UXCollection<UIViewController>, IDisposable
    {
        public UIViewControllerCollection() { }

        public UIViewControllerCollection(UITimeOut timeout)
        {
            this.ViewTimeOut = timeout;
        }

        public UITimeOut ViewTimeOut;

        public override UIViewController this[uint joinNumber]
        {
            get
            {
                return base[joinNumber];
            }
            internal set
            {
                base[joinNumber] = value;
            }
        }

        public UIViewController CurrentView
        {
            get
            {
                return InternalDictionary.Values.FirstOrDefault(p => p.Visible == true);
            }
        }

        public void Add(UIViewController newView)
        {
            if (!this.Contains(newView))
            {
                this[newView.VisibleJoinNumber] = newView;
                newView.VisibilityChange += new UIViewControllerEventHandler(ViewController_VisibilityChange);
            }
        }

        public void ShowOnly(UIViewController newView)
        {
            foreach (UIViewController view in this)
            {
                if (view != newView)
                {
                    view.Hide();
                }
            }

            if (this.Contains(newView))
            {
                newView.Show();
            }
        }

        public void ShowOnly(uint joinNumber)
        {
            if (this.Contains(joinNumber))
            {
                this.ShowOnly(this[joinNumber]);
            }
        }

        void ViewController_VisibilityChange(UIViewController sender, UIViewVisibilityEventArgs args)
        {
            if (sender.Visible && this.ViewTimeOut != null)
            {
                this.ViewTimeOut.Set();
            }
        }

        public virtual void Dispose()
        {
            this.ViewTimeOut.Dispose();

            foreach (UIViewController view in this)
            {
                view.VisibilityChange -= new UIViewControllerEventHandler(ViewController_VisibilityChange);
                view.Dispose();
            }

            InternalDictionary.Clear();
            InternalDictionary = null;
        }
    }
}