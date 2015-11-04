using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UserInterfaceCollection : IEnumerable<UserInterface>
    {
        private List<UserInterface> interfaces;

        public UserInterface this[uint id]
        {
            get
            {
                return this.interfaces.FirstOrDefault(i => i.ID == id);
            }
        }

        public UserInterfaceCollection()
        {
            this.interfaces = new List<UserInterface>();
        }

        public void Add(UserInterface ui)
        {
            this.interfaces.Add(ui);
        }

        public IEnumerator<UserInterface> GetEnumerator()
        {
            return this.interfaces.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}