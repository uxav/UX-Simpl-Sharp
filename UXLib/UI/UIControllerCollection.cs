using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIControllerCollection : IEnumerable<UIController>
    {
        private List<UIController> interfaces;

        public UIController this[uint id]
        {
            get
            {
                return this.interfaces.FirstOrDefault(i => i.ID == id);
            }
        }

        public UIControllerCollection()
        {
            this.interfaces = new List<UIController>();
        }

        public void Add(UIController ui)
        {
            this.interfaces.Add(ui);
        }

        public IEnumerator<UIController> GetEnumerator()
        {
            return this.interfaces.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}