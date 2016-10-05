using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class UserInterfaceContactInfoMethod
    {
        internal UserInterfaceContactInfoMethod()
        {
        }

        internal UserInterfaceContactInfoMethod(string number)
        {
            this.Number = number;
        }

        public string Number { get; internal set; }


    }
}