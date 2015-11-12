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

        public ListDataObject this[string keyName]
        {
            get
            {
                return this.Data.FirstOrDefault(d => d.KeyName == keyName);
            }
        }

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

        public void AddObject(string keyName, string title, object dataObject)
        {
            if (this.Data.FirstOrDefault(d => d.KeyName == keyName) != null)
            {
                ListDataObject newObject = new ListDataObject(keyName, title, dataObject);
                this.Data.Add(newObject);
            }
            else
            {
                ListDataObject existingObject = this.Data.FirstOrDefault(d => d.KeyName == keyName);
                existingObject.Title = title;
                existingObject.DataObject = dataObject;
            }
        }

        public void RemoveObject(string keyName)
        {
            ListDataObject existingObject = this.Data.FirstOrDefault(d => d.KeyName == keyName);
            if (existingObject != null)
            {
                this.Data.Remove(existingObject);
            }
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
        
        public IEnumerator<ListDataObject> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
        HasCleared
    }
}