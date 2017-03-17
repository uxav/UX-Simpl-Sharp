using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.Models
{
    public class UXReadOnlyCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        internal UXReadOnlyCollection()
        {
            InternalDictionary = new Dictionary<TKey, TValue>();
        }

        internal UXReadOnlyCollection(IDictionary<TKey, TValue> fromDictionary)
        {
            InternalDictionary = new Dictionary<TKey, TValue>(fromDictionary);
        }
        
        protected Dictionary<TKey, TValue> InternalDictionary;

        public virtual TValue this[TKey key] {
            get
            {
                return InternalDictionary[key];
            }
            internal set
            {
                InternalDictionary[key] = value;
            }
        }

        internal virtual void Add(TKey key, TValue value)
        {
            InternalDictionary.Add(key, value);
        }

        public virtual bool Contains(TKey key)
        {
            return InternalDictionary.ContainsKey(key);
        }

        public virtual bool Contains(TValue value)
        {
            return InternalDictionary.Values.Contains(value);
        }

        public virtual int IndexOf(TValue value)
        {
            return InternalDictionary.Values.ToList().IndexOf(value);
        }

        public int Count { get { return InternalDictionary.Count; } }

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InternalDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}