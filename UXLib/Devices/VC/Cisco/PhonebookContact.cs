using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookContact : IPhonebookItem
    {
        public PhonebookContact(CiscoCodec codec, string contactID, string name)
        {
            ItemType = PhonebookItemType.Contact;
            Codec = codec;
            ID = contactID;
            Name = name;
            ParentID = string.Empty;
        }

        public PhonebookContact(CiscoCodec codec, string contactID, string name, string folderID)
            : this(codec, contactID, name)
        {
            ParentID = folderID;
        }

        CiscoCodec Codec;
        public string ID { get; protected set; }
        public string Name { get; protected set; }
        public string ParentID { get; protected set; }
        public string Title { get; set; }
        public PhonebookItemType ItemType { get; protected set; }

        List<PhonebookContactMethod> _Methods;
        public ReadOnlyCollection<PhonebookContactMethod> Methods
        {
            get
            {
                return new ReadOnlyCollection<PhonebookContactMethod>(_Methods);
            }
        }

        public DialResult Dial()
        {
            if (Methods.Count > 0)
                return Methods.First().Dial();
            return new DialResult(0, "Contact has no methods");
        }

        public DialResult Dial(PhonebookContactMethod method)
        {
            return Codec.Calls.Dial(method.Number);
        }

        public void AddMethods(IEnumerable<PhonebookContactMethod> methods)
        {
            _Methods = new List<PhonebookContactMethod>(methods);
        }
    }
}