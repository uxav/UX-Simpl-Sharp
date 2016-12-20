using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib
{
    public class ListData : IEnumerable<ListDataObject>
    {
        List<ListDataObject> _Data { get; set; }
        public event ListDataChangeEventHandler DataChange;

        public ListDataObject this[int index]
        {
            get
            {
                if (this._Data.ElementAtOrDefault(index) != null)
                    return this._Data[index];
                return null;
            }
        }

        public int Count
        {
            get
            {
                return this._Data.Count;
            }
        }

        public int IndexOf(ListDataObject item)
        {
            return this._Data.IndexOf(item);
        }

        public ListData()
        {
            this._Data = new List<ListDataObject>();
        }

        public void LoadFromData(IEnumerable<ListDataObject> data)
        {
            _Data = new List<ListDataObject>(data);
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.HasLoaded));
        }

        public IEnumerable<ListDataObject> Data
        {
            get
            {
                return _Data.AsReadOnly();
            }
        }

        public void AddObject(ListDataObject listDataObject)
        {
            if (!this._Data.Contains(listDataObject))
                this._Data.Add(listDataObject);
        }

        public void Sort()
        {
            _Data = _Data.OrderBy(d => d.Title).ToList();
        }

        public void Clear()
        {
            this._Data = new List<ListDataObject>();
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.HasCleared));
        }

        public bool Loading { get; protected set; }

        public void OnDataLoadStart()
        {
            Loading = true;
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.IsStartingToLoad));
        }

        public void OnDataLoadComplete()
        {
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.HasLoaded));
            Loading = false;
        }

        public void SelectSingleItem(int index)
        {
            foreach (ListDataObject dataObject in _Data)
            {
                if(dataObject != _Data[index])
                    dataObject.IsSelected = false;
            }

            _Data[index].IsSelected = true;
            
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }

        public void SelectItemWithLinkedObject(object linkedObject)
        {
            ListDataObject item = _Data.FirstOrDefault(o => o.DataObject == linkedObject);
            if (item != null)
            {
                foreach (ListDataObject dataObject in _Data)
                {
                    if (dataObject != item)
                        dataObject.IsSelected = false;
                }

                item.IsSelected = true;
            }
            else
            {
                foreach (ListDataObject dataObject in _Data)
                    dataObject.IsSelected = false;
            }

            if(this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }

        public void SelectItemWithTitle(string itemTitle)
        {
            ListDataObject item = _Data.FirstOrDefault(o => o.Title == itemTitle);
            if (item != null)
            {
                foreach (ListDataObject dataObject in _Data)
                {
                    if (dataObject != item)
                        dataObject.IsSelected = false;
                }

                item.IsSelected = true;
                
                if (this.DataChange != null)
                    this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
            }
            else
                this.SelectSingleItem(0);
        }

        public void SelectItemWithKeyName(string keyName)
        {
            ListDataObject item = _Data.FirstOrDefault(o => o.KeyName == keyName);
            if (item != null)
            {
                foreach (ListDataObject dataObject in _Data)
                {
                    if (dataObject != item)
                        dataObject.IsSelected = false;
                }

                item.IsSelected = true;

                if (this.DataChange != null)
                    this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
            }
            else
                this.SelectSingleItem(0);
        }

        public void SelectItemWithLinkedObjectValue(object linkedObject)
        {
            ListDataObject item = _Data.FirstOrDefault(o => o.DataObject.Equals(linkedObject));
            if (item != null)
            {
                foreach (ListDataObject dataObject in _Data)
                {
                    if (dataObject != item)
                        dataObject.IsSelected = false;
                }

                item.IsSelected = true;
            }

            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }

        public void SelectClearAll()
        {
            foreach (ListDataObject dataObject in _Data)
            {
                dataObject.IsSelected = false;
            }

            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }
        
        public IEnumerator<ListDataObject> GetEnumerator()
        {
            return this._Data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
                this._Data.Clear();
                this._Data = null;
            }

            // Free any unmanaged objects here.
            //

            disposed = true;
        }
    }
    
    public delegate void ListDataChangeEventHandler(ListData listData, ListDataChangeEventArgs args);

    public class ListDataChangeEventArgs : EventArgs
    {
        public eListDataChangeEventType EventType;
        public ListDataChangeEventArgs(eListDataChangeEventType eventType)
            : base()
        {
            this.EventType = eventType;
        }
    }

    public enum eListDataChangeEventType
    {
        IsStartingToLoad,
        HasLoaded,
        HasCleared,
        ItemSelectionHasChanged
    }
}