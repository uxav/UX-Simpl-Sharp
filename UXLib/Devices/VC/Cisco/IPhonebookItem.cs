using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public interface IPhonebookItem
    {
        string ID { get; }
        string Name { get; }
        string ParentID { get; }
        PhonebookItemType ItemType { get; }
    }

    public enum PhonebookItemType
    {
        Folder,
        Contact,
        ContactMethod
    }
}