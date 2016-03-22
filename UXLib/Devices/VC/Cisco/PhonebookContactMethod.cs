using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookContactMethod
    {
        public PhonebookContactMethod(PhonebookContact contact, string methodID, string number)
        {
            Contact = contact;
            ContactMethodId = methodID;
            Number = number;
        }

        public PhonebookContact Contact { get; protected set; }
        public string ContactMethodId { get; protected set; }
        public string Number { get; protected set; }
        public string Protocol { get; set; }
        public string Device { get; set; }

        public int Dial()
        {
            return this.Contact.Dial(this);
        }
    }
}