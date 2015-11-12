using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIKey
    {
        public string Name;
        public uint Number;

        public UIKey(string name, uint number)
        {
            this.Name = name;
            this.Number = number;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.Name, this.Number);
        }
    }
}