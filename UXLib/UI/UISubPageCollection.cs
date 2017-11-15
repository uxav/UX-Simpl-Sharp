using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UISubPageCollection : IEnumerable<UISubPage>
    {
        private readonly List<UISubPage> _subPages = new List<UISubPage>();
        
        public UISubPage this[uint joinNumber]
        {
            get
            {
                return _subPages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public void Add(UISubPage newSubPage)
        {
            if (!_subPages.Contains(newSubPage))
            {
                _subPages.Add(newSubPage);
            }
            else
            {
                throw new Exception(string.Format("SubPage with join '{0}' already exists", newSubPage.VisibleJoinNumber));
            }
        }

        public void ShowAll()
        {
            foreach (UISubPage subPage in _subPages)
            {
                subPage.Show();
            }
        }

        public void ShowOnly(UISubPage newSubPage)
        {
            foreach (UISubPage subPage in _subPages)
            {
                if (subPage != newSubPage)
                {
                    subPage.Hide();
                }
            }

            if (_subPages.Contains(newSubPage))
                newSubPage.Show();
            else
                ErrorLog.Error("Cannot ShowOnly subpage with join of {0} as it does not exist in the collection", newSubPage.VisibleJoinNumber);
        }

        public void ShowOnly(uint joinNumber)
        {
            foreach (UISubPage subPage in _subPages)
            {
                if (subPage.VisibleJoinNumber != joinNumber)
                {
                    subPage.Hide();
                }
            }

            if (_subPages.Exists(p => p.VisibleJoinNumber == joinNumber))
                this[joinNumber].Show();
            else
                ErrorLog.Error("Cannot ShowOnly subpage with ID of {0} as it does not exist in the collection", joinNumber);
        }

        public void ShowOnlyWithIndex(int index)
        {
            for (int n = 0; n < _subPages.Count; n++)
            {
                if (n != index)
                {
                    _subPages[n].Hide();
                }
            }

            if (index < _subPages.Count)
                _subPages[index].Show();
        }

        public void HideAll()
        {
            foreach (UISubPage subPage in _subPages)
            {
                subPage.Hide();
            }
        }

        public IEnumerator<UISubPage> GetEnumerator()
        {
            return _subPages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}