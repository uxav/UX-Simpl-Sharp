using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class UserInterfaceContactInfo
    {
        internal UserInterfaceContactInfo()
        {
            _ContactMethods = new Dictionary<uint, UserInterfaceContactInfoMethod>();
            Name = string.Empty;
        }

        public string Name { get; internal set; }

        internal Dictionary<uint, UserInterfaceContactInfoMethod> _ContactMethods;
        public ReadOnlyDictionary<uint, UserInterfaceContactInfoMethod> ContactMethods
        {
            get
            {
                return new ReadOnlyDictionary<uint, UserInterfaceContactInfoMethod>(_ContactMethods);
            }
        }
    }
}