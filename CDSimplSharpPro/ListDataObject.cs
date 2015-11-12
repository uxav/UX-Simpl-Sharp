using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
{
    public class ListDataObject
    {
        public string KeyName { get; private set; }
        public string Title;
        public string Icon;
        public object DataObject;
        public bool IsSelected;

        public ListDataObject(string keyName, string title, object dataObject)
        {
            this.KeyName = keyName;
            this.Title = title;
            this.Icon = "";
            this.DataObject = dataObject;
            this.IsSelected = false;
        }

        public ListDataObject(string keyName, string title, string icon, object dataObject)
        {
            this.KeyName = keyName;
            this.Title = title;
            this.Icon = icon;
            this.DataObject = dataObject;
            this.IsSelected = false;
        }
    }
}