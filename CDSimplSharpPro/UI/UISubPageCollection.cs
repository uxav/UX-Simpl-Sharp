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

        public UISubPageCollection()
        {
            this.SubPages = new List<UISubPage>();
        }

        public void Add(UISubPage newSubPage)
        {
            if (!this.SubPages.Contains(newSubPage))
            {
                this.SubPages.Add(newSubPage);
            }
            else
            {
                throw new Exception(string.Format("SubPage with id '{0}' already exists", newSubPage.ID));
            }
        }

        public void ShowAll()
        {
            foreach (UISubPage subPage in SubPages)
            {
                subPage.Show();
            }
        }

        public void ShowOnly(UISubPage newSubPage)
        {
            foreach (UISubPage subPage in SubPages)
            {
                if (subPage != newSubPage)
                {
                    subPage.Hide();
                }
            }

            if (SubPages.Contains(newSubPage))
                newSubPage.Show();
            else
                ErrorLog.Error("Cannot ShowOnly subpage with ID of {0} as it does not exist in the collection", newSubPage.ID);
        }

        public void ShowOnly(uint joinNumber)
        {
            foreach (UISubPage subPage in SubPages)
            {
                if (subPage.VisibleJoinNumber != joinNumber)
                {
                    subPage.Hide();
                }
            }

            if (SubPages.Exists(p => p.VisibleJoinNumber == joinNumber))
                this[joinNumber].Show();
            else
                ErrorLog.Error("Cannot ShowOnly subpage with ID of {0} as it does not exist in the collection", joinNumber);
        }

        public void ShowOnlyWithIndex(int index)
        {
            for (int n = 0; n < SubPages.Count; n++)
            {
                if (n != index)
                {
                    SubPages[n].Hide();
                }
            }

            if (index < SubPages.Count)
                SubPages[index].Show();
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