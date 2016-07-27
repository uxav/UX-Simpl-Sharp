using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class PhonebookFolder : IPhonebookItem
    {
        public PhonebookFolder(CiscoCodec codec, string folderID, string name)
        {
            ItemType = PhonebookItemType.Folder;
            Codec = codec;
            ID = folderID;
            Name = name;
            ParentID = string.Empty;
        }

        public PhonebookFolder(CiscoCodec codec, string folderID, string name, string parentFolderID)
            : this(codec, folderID, name)
        {
            ParentID = parentFolderID;
        }

        CiscoCodec Codec;
        public string ID { get; protected set; }
        public string Name { get; protected set; }
        public string ParentID { get; protected set; }
        public PhonebookItemType ItemType { get; protected set; }
    }
}