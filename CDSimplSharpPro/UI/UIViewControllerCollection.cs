using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIViewControllerCollection : IEnumerable<UIViewController>
    {
        protected List<UIViewController> ViewControllers;

        public UIViewControllerCollection()
        {
            ViewControllers = new List<UIViewController>();
        }

        public void Add(UIViewController newView)
        {
            ViewControllers.Add(newView);
        }

        public UIViewController this[uint visibleJoinNumber]
        {
            get
            {
                return ViewControllers.FirstOrDefault(
                    v => v.VisibleJoinNumber == visibleJoinNumber
                    );
            }
        }

        public IEnumerator<UIViewController> GetEnumerator()
        {
            return this.ViewControllers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        } 
    }
}