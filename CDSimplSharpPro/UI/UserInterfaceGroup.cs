using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UserInterfaceGroup : Dictionary<uint, UserInterface>
    {
        public UserInterfaceGroup()
            : base()
        {
            
        }

        public void Add(UserInterface ui)
        {
            base.Add(ui.ID, ui);
        }
    }
}