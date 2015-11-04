using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UIPageGroup : IEnumerable<UIPage>
    {
        private List<UIPage> Pages;
        BoolInputSigInterlock PageVisisbleJoinSigGroup;

        public UIPage this[uint joinNumber]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UIPage this[string keyName]
        {
            get
            {
                return this.Pages.FirstOrDefault(p => p.KeyName == keyName);
            }
        }

        public UIPageGroup()
        {
            this.Pages = new List<UIPage>();
            this.PageVisisbleJoinSigGroup = new BoolInputSigInterlock();
        }

        public void Add(string key, BoolInputSig visibleJoinSig)
        {
            if (!this.PageVisisbleJoinSigGroup.Contains(visibleJoinSig))
            {
                UIPage newPage = new UIPage(key, visibleJoinSig, this.PageVisisbleJoinSigGroup);
                this.Pages.Add(newPage);
            }
            else
            {
                throw new Exception("Page with visible join value already exists");
            }
        }

        public void Add(string key, BoolInputSig visibleJoinSig, string name, StringInputSig nameStringInputSig)
        {
            if (!this.PageVisisbleJoinSigGroup.Contains(visibleJoinSig))
            {
                UIPage newPage = new UIPage(key, visibleJoinSig, this.PageVisisbleJoinSigGroup, nameStringInputSig, name);
                this.Pages.Add(newPage);
            }
            else
            {
                throw new Exception("Page with visible join value already exists");
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