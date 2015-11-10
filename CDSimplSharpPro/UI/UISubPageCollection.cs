using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISubPageCollection : IEnumerable<UISubPage>
    {
        private List<UISubPage> SubPages;
        
        public UISubPage this[uint joinNumber]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UISubPage this[UIKey key]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.Key == key);
            }
        }

        public UISubPageCollection()
        {
            this.SubPages = new List<UISubPage>();
        }

        public void Add(UISubPage page)
        {
            if (!this.SubPages.Exists(p => p.Key == page.Key))
            {
                this.SubPages.Add(page);
            }
            else
            {
                throw new Exception(string.Format("SubPage with key name '{0}' already exists", page.Key.Name));
            }
        }

        public IEnumerator<UISubPage> GetEnumerator()
        {
            return this.SubPages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}