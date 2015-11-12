using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UIPageCollection : IEnumerable<UIPage>
    {
        private List<UIPage> Pages;
        BoolInputSigInterlock PageVisisbleJoinSigGroup;
        public UITimeOut PageTimeOut;

        public UIPage this[uint joinNumber]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UIPage this[UIKey key]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.Key == key);
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
            this.PageVisisbleJoinSigGroup = new BoolInputSigInterlock();
        }

        public UIPageCollection(UITimeOut timeout)
        {
            this.Pages = new List<UIPage>();
            this.PageVisisbleJoinSigGroup = new BoolInputSigInterlock();
            this.PageTimeOut = timeout;
        }

        public void Add(UIKey key, BoolInputSig visibleJoinSig)
        {
            if (!this.PageVisisbleJoinSigGroup.Contains(visibleJoinSig))
            {
                UIPage newPage = new UIPage(key, visibleJoinSig, this.PageVisisbleJoinSigGroup);
                this.Pages.Add(newPage);
                newPage.PageChange += new UIPage.UIPageEventHandler(Page_PageChange);
            }
            else
            {
                throw new Exception("Page with visible join value already exists");
            }
        }

        public void Add(UIKey key, BoolInputSig visibleJoinSig, string name, UILabel titleLabel)
        {
            if (!this.PageVisisbleJoinSigGroup.Contains(visibleJoinSig))
            {
                UIPage newPage = new UIPage(key, visibleJoinSig, this.PageVisisbleJoinSigGroup, titleLabel, name);
                this.Pages.Add(newPage);
                newPage.PageChange += new UIPage.UIPageEventHandler(Page_PageChange);
            }
            else
            {
                throw new Exception("Page with visible join value already exists");
            }
        }

        void Page_PageChange(UIPage page, UIPageEventArgs args)
        {
            if (page.Visible)
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
    }
}