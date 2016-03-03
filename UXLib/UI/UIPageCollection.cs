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
        public UIPageCollection()
        {
            
        }

        List<UIPage> _pages;
        List<UIPage> Pages
        {
            get
            {
                if (this._pages == null)
                    this._pages = new List<UIPage>();
                return _pages;
            }
        }

        public UIPage this[uint visibleJoinNumber]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.VisibleJoinNumber == visibleJoinNumber);
            }
        }

        public UIPage CurrentPage
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.Visible == true);
            }
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
            if (VisibilityChange != null)
                VisibilityChange(sender, args);
            if (args.EventType == eViewEventType.WillShow)
            {
                UIPage newPage = sender as UIPage;

                foreach (UIPage page in this)
                {
                    if (page.Visible && page != newPage)
                    {
                        page.Visible = false;
                    }
                }
            }
        }

        public event UIViewBaseVisibitlityEventHandler VisibilityChange;

        public IEnumerator<UIPage> GetEnumerator()
        {
            return this.Pages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}