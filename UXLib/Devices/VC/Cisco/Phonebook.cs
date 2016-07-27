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
        public Phonebook(CiscoCodec codec)
        {
            Codec = codec;
        }

        CiscoCodec Codec;

        public static CommandArgs BuildCommandArgs(PhonebookType phonebookType, string searchString, bool recursive)
        {
            CommandArgs args = new CommandArgs("PhonebookType", phonebookType.ToString());
            args.Add("Recursive", recursive.ToString());
            args.Add("SearchString", searchString);
            return args;
        }

        public static CommandArgs BuildCommandArgs(PhonebookType phonebookType, string folderId)
        {
            CommandArgs args = new CommandArgs("PhonebookType", phonebookType.ToString());
            if (phonebookType == PhonebookType.Local)
                args.Add("Recursive", "False");
            args.Add("FolderId", folderId);
            return args;
        }

        public PhonebookSearchResults Search(CommandArgs searchCommandArgs)
        {
            XDocument xml = Codec.SendCommand("Phonebook/Search", searchCommandArgs, true);
            
            XElement element = xml.Root.Element("PhonebookSearchResult");
#if DEBUG
            CrestronConsole.PrintLine(element.ToString());
#endif
            if (element.Attribute("status").Value == "OK")
            {
                List<IPhonebookItem> items = new List<IPhonebookItem>();

                if (element.Element("ResultSet") != null)
                {
#if DEBUG
                    CrestronConsole.PrintLine("Phonebook results contain element \"ResultSet\"");
#endif
                    element = element.Element("ResultSet");
                }
                int offset = int.Parse(element.Element("ResultInfo").Element("Offset").Value);
                int limit = int.Parse(element.Element("ResultInfo").Element("Limit").Value);
                int totalRows = int.Parse(element.Element("ResultInfo").Element("TotalRows").Value);
                
#if DEBUG
                CrestronConsole.PrintLine("Offset = {0}, Limit = {1}, TotalRows = {2}", offset, limit, totalRows);
#endif
                if (totalRows > 0)
                {
                    IEnumerable<XElement> contactsData = element.Elements("Contact");
                    IEnumerable<XElement> foldersData = element.Elements("Folder");

                    foreach (XElement c in contactsData)
                    {
                        PhonebookContact contact;

                        if (c.Element("FolderId") != null)
                        {
                            contact = new PhonebookContact(Codec,
                                c.Element("ContactId").Value,
                                c.Element("Name").Value,
                                c.Element("FolderId").Value);
                        }
                        else
                        {
                            contact = new PhonebookContact(Codec,
                                c.Element("ContactId").Value,
                                c.Element("Name").Value);
                        }

                        items.Add(contact);

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
                                    case "CallType": method.CallType = (CallType)Enum.Parse(typeof(CallType), e.Value, false); break;
                                }
                            }
                        }

                        contact.AddMethods(methods);
                    }

                    foreach (XElement f in foldersData)
                    {
                        PhonebookFolder folder;

                        if (f.Element("ParentFolderId") != null)
                        {
                            folder = new PhonebookFolder(Codec,
                                f.Element("FolderId").Value,
                                f.Element("Name").Value,
                                f.Element("ParentFolderId").Value);
                        }
                        else
                        {
                            folder = new PhonebookFolder(Codec,
                                f.Element("FolderId").Value,
                                f.Element("Name").Value); 
                        }

                        items.Add(folder);
                    }
                }

                return new PhonebookSearchResults(items, offset, limit);
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