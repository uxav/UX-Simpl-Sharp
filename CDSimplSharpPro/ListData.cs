using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
{
    public class ListData : IEnumerable<ListDataObject>
    {
        private List<ListDataObject> Data;
        public event ListDataChangeEventHandler DataChange;

        public ListDataObject this[int index]
        {
            get
            {
                if (this.Data.ElementAtOrDefault(index) != null)
                {
                    return this.Data[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public int Count
        {
            get
            {
                return this.Data.Count;
            }
        }

        public ListData()
        {
            this.Data = new List<ListDataObject>();
        }

        public void AddObject(ListDataObject listDataObject)
        {
            if (!this.Data.Contains(listDataObject))
                this.Data.Add(listDataObject);
        }

        public void Sort()
        {
            Data = Data.OrderBy(d => d.Title).ToList();
        }

        public void Clear()
        {
            this.Data.Clear();
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.HasCleared));
        }

        public void OnDataLoadStart()
        {
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.IsStartingToLoad));
        }

        public void OnDataLoadComplete()
        {
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.HasLoaded));
        }

        public void SelectSingleItem(int index)
        {
            foreach (ListDataObject dataObject in Data)
            {
                if(dataObject != Data[index])
                    dataObject.IsSelected = false;
            }

            Data[index].IsSelected = true;
            
            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }

        public void SelectItemWithLinkedObject(object linkedObject)
        {
            ListDataObject item = Data.FirstOrDefault(o => o.DataObject == linkedObject);
            if (item != null)
            {
                foreach (ListDataObject dataObject in Data)
                {
                    if (dataObject != item)
                        dataObject.IsSelected = false;
                }

                item.IsSelected = true;
            }

            if(this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }

        public void SelectItemWithLinkedObjectValue(object linkedObject)
        {
            ListDataObject item = Data.FirstOrDefault(o => o.DataObject.Equals(linkedObject));
            if (item != null)
            {
                foreach (ListDataObject dataObject in Data)
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
            foreach (ListDataObject dataObject in Data)
            {
                dataObject.IsSelected = false;
            }

            if (this.DataChange != null)
                this.DataChange(this, new ListDataChangeEventArgs(eListDataChangeEventType.ItemSelectionHasChanged));
        }
        
        public IEnumerator<ListDataObject> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual void Dispose()
        {
            Data.Clear();
            Data = null;
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