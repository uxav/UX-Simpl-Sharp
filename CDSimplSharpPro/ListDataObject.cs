using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
{
    public class ListDataObject : IDisposable
    {
        public string Title;
        public string Icon;
        public object DataObject;
        public bool IsSelected;

        public ListDataObject(string title, object dataObject)
        {
            this.Title = title;
            this.Icon = "";
            this.DataObject = dataObject;
            this.IsSelected = false;
        }

        public ListDataObject(string title, string icon, object dataObject)
        {
            this.Title = title;
            this.Icon = icon;
            this.DataObject = dataObject;
            this.IsSelected = false;
        }

        public virtual void Dispose()
        {
            this.Title = null;
            this.Icon = null;
            this.DataObject = null;
        }
    }
}