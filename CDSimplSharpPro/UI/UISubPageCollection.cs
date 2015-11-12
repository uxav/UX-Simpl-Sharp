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

        public void ShowAll()
        {
            foreach (UISubPage subPage in SubPages)
            {
                subPage.Show();
            }
        }

        public void ShowOnly(UIKey key)
        {
            foreach (UISubPage subPage in SubPages)
            {
                if (subPage.Key != key)
                {
                    subPage.Hide();
                }
            }

            this[key].Show();
        }

        public void ShowOnlyWithIndex(uint index)
        {
            for (uint n = 1; n <= SubPages.Count; n++)
            {
                if (n != index)
                {
                    SubPages[(int)n - 1].Hide();
                }
            }

            SubPages[(int)index - 1].Show();
        }

        public void HideAll()
        {
            foreach (UISubPage subPage in SubPages)
            {
                subPage.Hide();
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