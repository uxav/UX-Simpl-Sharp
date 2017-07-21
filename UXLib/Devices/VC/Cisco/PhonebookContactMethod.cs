using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookContactMethod : IPhonebookItem
    {
        public PhonebookContactMethod(PhonebookContact contact, string methodID, string number)
        {
            ItemType = PhonebookItemType.ContactMethod;
            Contact = contact;
            ID = methodID;
            Number = number;
        }

        public PhonebookContact Contact { get; protected set; }
        public string ID { get; protected set; }
        public string ParentID
        {
            get
            {
                return this.Contact.ID;
            }
        }
        public string Number { get; protected set; }
        public string Name
        {
            get
            {
                return this.Number;
            }
        }
        public string Protocol { get; set; }
        public string Device { get; set; }
        public CallType CallType { get; set; }
        public PhonebookItemType ItemType { get; protected set; }

        public DialResult Dial()
        {
            return Contact.Dial(this);
        }

        public DialResult Dial(CallType callType)
        {
            return Contact.Dial(this, callType);
        }
    }
}