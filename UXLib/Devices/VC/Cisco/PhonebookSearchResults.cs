using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookSearchResults : IEnumerable<PhonebookContact>
    {
        public PhonebookSearchResults(bool error)
        {
            StatusOk = false;
            this.Contacts = new List<PhonebookContact>();
        }

        public PhonebookSearchResults(IEnumerable<PhonebookContact> contacts, int offset, int limit)
        {
            this.Contacts = new List<PhonebookContact>(contacts);
            Offset = offset;
            Limit = limit;
        }

        List<PhonebookContact> Contacts;
        public bool StatusOk = true;
        public int Offset { get; protected set; }
        public int Limit { get; protected set; }

        public PhonebookContact this[int index]
        {
            get
            {
                return Contacts[index];
            }
        }

        public PhonebookContact this[string contactID]
        {
            get
            {
                return Contacts.Where(e => e.ContactID == contactID).FirstOrDefault();
            }
        }

        public int Count { get { return Contacts.Count; } }

        #region IEnumerable<PhonebookContact> Members

        public IEnumerator<PhonebookContact> GetEnumerator()
        {
            return this.Contacts.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}