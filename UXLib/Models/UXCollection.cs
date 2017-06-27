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
    public abstract class UXCollection<T> : UXReadOnlyCollection<uint, T>, IEnumerable<T>
    {
        internal UXCollection()
        { }

        internal UXCollection(IDictionary<uint, T> fromDictionary)
            : base(fromDictionary) { }

        internal UXCollection(IEnumerable<T> fromList)
        {
            foreach (var item in fromList)
            {
                var type = typeof(T).GetCType();

                if (!type.GetProperties().Any(p => p.Name == "ID" && p.PropertyType == typeof (UInt32)))
                    continue;
                var id = (uint) type.GetProperty("ID", typeof (UInt32).GetCType()).GetValue(item, null);
                InternalDictionary[id] = item;
            }
        }

        public List<T> ToList()
        {
            return InternalDictionary.Values.ToList();
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return InternalDictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    } 
}