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
        private Dictionary<uint, UIController> interfaces;

        public UIController this[uint id]
        {
            get
            {
                return this.interfaces[id];
            }
        }

        public UIControllerCollection()
        {
            this.interfaces = new Dictionary<uint, UIController>();
        }

        protected UIControllerCollection(List<UIController> fromList)
        {
            interfaces = new Dictionary<uint, UIController>();
            foreach (UIController ui in fromList)
            {
                interfaces[ui.ID] = ui;
            }
        }

        public void Add(UIController ui)
        {
            this.interfaces[ui.ID] = ui;
        }

        public bool IsDefined(uint id)
        {
            return interfaces.ContainsKey(id);
        }

        public UIControllerCollection ForRoom(Models.Room room)
        {
            return new UIControllerCollection(interfaces.Values.Where(ui => ui.Room == room).ToList());
        }

        public IEnumerator<UIController> GetEnumerator()
        {
            return this.interfaces.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}