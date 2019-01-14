using System;
using System.Linq;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIViewControllerCollection : UXCollection<UIViewController>, IDisposable
    {
        public UIViewControllerCollection() { }

        public UIViewControllerCollection(UITimeOut timeout)
        {
            ViewTimeOut = timeout;
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
                return InternalDictionary.Values.FirstOrDefault(p => p.Visible);
            }
        }

        public void Add(UIViewController newView)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.Add(UIViewController newView) - View visible join = {1}",
                GetType().Name, newView.VisibleJoinNumber);
#endif
            if (!Contains(newView))
            {
                this[newView.VisibleJoinNumber] = newView;
                newView.VisibilityChange += ViewController_VisibilityChange;
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

            if (Contains(newView))
            {
                newView.Show();
            }
        }

        public void ShowOnly(uint joinNumber)
        {
            if (Contains(joinNumber))
            {
                ShowOnly(this[joinNumber]);
            }
        }

        void ViewController_VisibilityChange(UIViewController sender, UIViewVisibilityEventArgs args)
        {
            if (sender.Visible && ViewTimeOut != null)
            {
                ViewTimeOut.Set();
            }
        }

        public virtual void Dispose()
        {
            ViewTimeOut.Dispose();

            foreach (UIViewController view in this)
            {
                view.VisibilityChange -= ViewController_VisibilityChange;
                view.Dispose();
            }

            InternalDictionary.Clear();
        }
    }
}