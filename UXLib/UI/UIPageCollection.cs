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
        private readonly List<UIPage> _pages = new List<UIPage>();

        public UIPage this[uint visibleJoinNumber]
        {
            get
            {
                return _pages.FirstOrDefault(p => p.VisibleJoinNumber == visibleJoinNumber);
            }
        }

        public UIPage CurrentPage
        {
            get
            {
                return _pages.FirstOrDefault(p => p.Visible == true);
            }
        }

        public void Add(UIPage newPage)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}.Add(UIPage newPage), page {1}", GetType().Name, newPage.VisibleFeedbackJoin.Number);
#endif
            if (!_pages.Exists(p => p.VisibleDigitalJoin == newPage.VisibleDigitalJoin))
            {
                _pages.Add(newPage);
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
            return _pages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}