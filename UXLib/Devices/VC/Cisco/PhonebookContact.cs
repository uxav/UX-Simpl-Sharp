using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookContact
    {
        public PhonebookContact(Codec codec, string contactID, string name)
        {
            Codec = codec;
            ContactID = contactID;
            Name = name;
        }

        Codec Codec;
        public string ContactID { get; protected set; }
        public string Name { get; protected set; }
        public string Title { get; set; }

        List<PhonebookContactMethod> _Methods;
        public ReadOnlyCollection<PhonebookContactMethod> Methods
        {
            get
            {
                return new ReadOnlyCollection<PhonebookContactMethod>(_Methods);
            }
        }

        public int Dial()
        {
            if (Methods.Count > 0)
                return Methods.First().Dial();
            return 0;
        }

        public int Dial(PhonebookContactMethod method)
        {
            return Codec.Calls.Dial(method.Number);
        }

        public void AddMethods(IEnumerable<PhonebookContactMethod> methods)
        {
            _Methods = new List<PhonebookContactMethod>(methods);
        }
    }
}