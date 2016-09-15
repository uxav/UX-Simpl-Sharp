using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIControllerCollection : UXCollection<UIController>
    {
        public UIControllerCollection() { }

        public UIControllerCollection(IEnumerable<UIController> uiControllers)
        {
            foreach (UIController ui in uiControllers)
            {
                this[ui.ID] = ui;
            }
        }

        public void Add(UIController ui)
        {
            this[ui.ID] = ui;
        }

        public override bool Contains(uint id)
        {
            return base.Contains(id);
        }

        public UIControllerCollection ForRoom(Models.Room room)
        {
            return new UIControllerCollection(InternalDictionary.Values.Where(ui => ui.Room == room));
        }
    }
}