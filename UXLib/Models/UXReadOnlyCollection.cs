using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.Models
{
    public abstract class UXReadOnlyCollection<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        internal UXReadOnlyCollection()
        {
            
        }

        internal UXReadOnlyCollection(IDictionary<TKey, TValue> fromDictionary)
        {
            InternalDictionary = new Dictionary<TKey, TValue>(fromDictionary);
        }

        protected readonly Dictionary<TKey, TValue> InternalDictionary = new Dictionary<TKey, TValue>();

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

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InternalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}