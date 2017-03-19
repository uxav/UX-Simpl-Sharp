using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

namespace UXLib.Models
{
    /// <summary>
    /// A read only collection of a type
    /// </summary>
    /// <typeparam name="T">Type of object stored with a uint key value</typeparam>
    public class UXCollection<T> : UXReadOnlyCollection<uint, T>, IEnumerable<T>, IEnumerable
    {
        internal UXCollection()
        { }

        internal UXCollection(Dictionary<uint, T> fromDictionary)
            : base(fromDictionary) { }

        internal UXCollection(IEnumerable<T> fromList)
        {
            foreach (T item in fromList)
            {
                CType type = typeof(T).GetCType();

                if (type.GetProperties().Any(p => p.Name == "ID" && p.PropertyType == typeof(System.UInt32)))
                {
                    uint id = (uint)type.GetProperty("ID", typeof(System.UInt32).GetCType()).GetValue(item, null);
                    InternalDictionary[id] = item;
                }
            }
        }

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