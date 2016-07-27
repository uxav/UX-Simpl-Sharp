using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISubPageCollection : IEnumerable<UISubPage>
    {
        public UISubPageCollection()
        {
            
        }

        List<UISubPage> _subPages;
        List<UISubPage> SubPages
        {
            get
            {
                if (_subPages == null)
                    _subPages = new List<UISubPage>();
                return _subPages;
            }
        }
        
        public UISubPage this[uint joinNumber]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public void Add(UISubPage newSubPage)
        {
            if (!this.SubPages.Contains(newSubPage))
            {
                this.SubPages.Add(newSubPage);
            }
            else
            {
                throw new Exception(string.Format("SubPage with join '{0}' already exists", newSubPage.VisibleJoinNumber));
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
                ErrorLog.Error("Cannot ShowOnly subpage with join of {0} as it does not exist in the collection", newSubPage.VisibleJoinNumber);
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