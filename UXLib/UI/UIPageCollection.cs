using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UIPageCollection : IEnumerable<UIPage>
    {
        private List<UIPage> Pages;
        public UITimeOut PageTimeOut;

        public UIPage this[uint joinNumber]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UIPage CurrentPage
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.Visible == true);
            }
        }

        public UIPageCollection()
        {
            this.Pages = new List<UIPage>();
        }

        public UIPageCollection(UITimeOut timeout)
        {
            this.Pages = new List<UIPage>();
            this.PageTimeOut = timeout;
        }

        public void Add(UIPage newPage)
        {
            if (!this.Pages.Exists(p => p.VisibleDigitalJoin == newPage.VisibleDigitalJoin))
            {
                this.Pages.Add(newPage);
                newPage.VisibilityChange += new UIViewBaseVisibitlityEventHandler(Page_VisibilityChange);
            }
            else
            {
                throw new Exception("Cannot Add page to page collection... Page with visible join value already exists!");
            }
        }

        void Page_VisibilityChange(UIViewBase sender, UIViewVisibilityEventArgs args)
        {
            if (sender.Visible && this.PageTimeOut != null)
            {
                this.PageTimeOut.Set();
            }
        }

        public IEnumerator<UIPage> GetEnumerator()
        {
            return this.Pages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Dispose()
        {
            this.PageTimeOut.Dispose();

            foreach (UIPage page in Pages)
            {
                page.VisibilityChange -= new UIViewBaseVisibitlityEventHandler(Page_VisibilityChange);
                page.Dispose();
            }

            this.Pages.Clear();
            this.Pages = null;
        }
    }
}