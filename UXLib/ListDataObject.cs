using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib
{
    public class ListDataObject : IDisposable
    {
        public string Title;
        public string Icon;
        public object DataObject;
        public bool IsSelected;
        public bool Enabled;

        public ListDataObject(string title, object dataObject)
            : this(title, UXLib.UI.UIMediaIcons.Blank, dataObject) { }

        public ListDataObject(string title, string icon, object dataObject)
            : this(title, icon, dataObject, true) { }

        public ListDataObject(string title, string icon, object dataObject, bool enabled)
        {
            this.Title = title;
            this.Icon = icon;
            this.DataObject = dataObject;
            this.IsSelected = false;
            this.Enabled = enabled;
        }

        /// <summary>
        /// Unregister from any sig changes and dispose of resources
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        bool disposed = false;

        public bool Disposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>
        /// Override this to free resources
        /// </summary>
        /// <param name="disposing">true is Dispose() has been called</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.Title = null;
                this.Icon = null;
                this.DataObject = null;
            }

            // Free any unmanaged objects here.
            //

            disposed = true;
        }
    }
}