using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Phonebook
    {
        public Phonebook(Codec codec)
        {
            Codec = codec;
        }

        Codec Codec;

        public PhonebookSearchResults Search(PhonebookType phonebookType, string searchString)
        {
            CommandArgs args = new CommandArgs("PhonebookType", phonebookType.ToString());
            args.Add("SearchString", searchString);

            XDocument xml = Codec.SendCommand("Phonebook/Search", args, false);
            
            XElement element = xml.Root.Element("PhonebookSearchResult");
            //CrestronConsole.PrintLine(element.ToString());

            if (element.Attribute("status").Value == "OK")
            {
                List<PhonebookContact> contacts = new List<PhonebookContact>();
                element = element.Element("ResultSet");
                int offset = int.Parse(element.Element("ResultInfo").Element("Offset").Value);
                int limit = int.Parse(element.Element("ResultInfo").Element("Limit").Value);
                int totalRows = int.Parse(element.Element("ResultInfo").Element("TotalRows").Value);
                //CrestronConsole.PrintLine("Offset = {0}, Limit = {1}, TotalRows = {2}", offset, limit, totalRows);

                if (totalRows > 0)
                {
                    IEnumerable<XElement> contactsData = element.Elements("Contact");

                    foreach (XElement c in contactsData)
                    {
                        PhonebookContact contact = new PhonebookContact(Codec,
                            c.Element("ContactId").Value,
                            c.Element("Name").Value);
                        contacts.Add(contact);

                        foreach (XElement e in c.Elements())
                        {
                            switch (e.Name)
                            {
                                case "Title": contact.Title = e.Value; break;
                            }
                        }

                        List<PhonebookContactMethod> methods = new List<PhonebookContactMethod>();
                        IEnumerable<XElement> methodsData = c.Elements("ContactMethod");

                        foreach (XElement m in methodsData)
                        {
                            PhonebookContactMethod method = new PhonebookContactMethod(contact,
                                m.Element("ContactMethodId").Value,
                                m.Element("Number").Value);
                            methods.Add(method);

                            foreach (XElement e in m.Elements())
                            {
                                switch (e.Name)
                                {
                                    case "Device": method.Device = e.Value; break;
                                    case "Protocol": method.Protocol = e.Value; break;
                                }
                            }
                        }

                        contact.AddMethods(methods);
                    }
                }

                return new PhonebookSearchResults(contacts, offset, limit);
            }
            
            return new PhonebookSearchResults(true);
        }
    }

    public enum PhonebookType
    {
        Corporate,
        Local
    }
}