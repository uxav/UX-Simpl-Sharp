using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    /// <summary>
    /// A read only collection of a type
    /// </summary>
    /// <typeparam name="T">Type of object stored with a uint key value</typeparam>
    public class UXCollection<T> : UXReadOnlyCollection<uint, T>, IEnumerable<T>, IEnumerable
    {
        internal UXCollection() { }

        public override T this[uint key]
        {
            get
            {
                return base[key];
            }
            internal set
            {
                base[key] = value;
            }
        }

        internal override void Add(uint key, T value)
        {
            base.Add(key, value);
        }

        public override int IndexOf(T value)
        {
            return base.IndexOf(value);
        }

        public List<T> ToList()
        {
            return InternalDictionary.Values.ToList();
        } 

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return InternalDictionary.Values.GetEnumerator();
        }

        #endregion
    } 
}