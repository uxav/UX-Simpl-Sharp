using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class SourceCollection : UXCollection<Source>
    {
        internal SourceCollection() { }

        internal SourceCollection(IEnumerable<Source> listOfSources)
        {
            InternalDictionary = new Dictionary<uint, Source>();
            foreach (Source source in listOfSources)
            {
                this[source.ID] = source;
            }
        }

        public override Source this[uint sourceID]
        {
            get
            {
                return base[sourceID];
            }
            internal set
            {
                base[sourceID] = value;
            }
        }

        public Source this[SourceType type]
        {
            get
            {
                return InternalDictionary.Values.FirstOrDefault(s => s.SourceType == type);
            }
        }

        public void Add(Source source)
        {
            this[source.ID] = source;
        }

        public void Remove(Source source)
        {
            this.InternalDictionary.Remove(source.ID);
        }
        
        public override bool Contains(uint sourceID)
        {
            return base.Contains(sourceID);
        }

        public bool Contains(SourceType type)
        {
            return InternalDictionary.Values.Any(s => s.SourceType == type);
        }

        public override bool Contains(Source source)
        {
            return base.Contains(source);
        }

        public SourceCollection GetSingleSources()
        {
            List<Source> singleSources = new List<Source>();

            foreach (Source source in this)
            {
                if (this.SourcesInGroup(source.GroupName).Count <= 1)
                {
                    singleSources.Add(source);
                }
            }

            return new SourceCollection(singleSources);
        }

        public SourceCollection SourcesInGroup(string groupName)
        {
            return new SourceCollection(InternalDictionary.Values.Where(s => s.GroupName == groupName));
        }

        public SourceCollection SourcesOfType(SourceType sourceType)
        {
            return new SourceCollection(InternalDictionary.Values.Where(s => s.SourceType == sourceType));
        }

        public SourceCollection PresentationSources
        {
            get
            {
                return new SourceCollection(InternalDictionary.Values.Where(s => s.IsPresentationSource));
            }
        }

        public SourceCollection ContentShareSources
        {
            get
            {
                return new SourceCollection(InternalDictionary.Values.Where(s => s.IsPresentationSource && s.AllowedForContentShare));
            }
        }

        public SourceCollection TelevisionSources
        {
            get
            {
                return new SourceCollection(InternalDictionary.Values.Where(s => s.IsTelevisionSource));
            }
        }

        public override int IndexOf(Source source)
        {
            return base.IndexOf(source);
        }

        public ReadOnlyCollection<string> GroupNames()
        {
            return this.GroupNames(false);
        }

        public ReadOnlyCollection<string> GroupNames(bool ofSourcesInGroupsOfMoreThanOne)
        {
            UXCollection<string> r = new UXCollection<string>();
            List<string> results = new List<string>();

            foreach (Source source in this)
            {
                if ((this.SourcesInGroup(source.GroupName).Count > 1 || !ofSourcesInGroupsOfMoreThanOne) && !results.Contains(source.GroupName))
                {
                    results.Add(source.GroupName);
                }
            }

            return results.AsReadOnly();
        }

        public uint GetNextUnusedSourceID()
        {
            for (uint id = 1; id < uint.MaxValue; id++)
            {
                if (!this.Contains(id))
                    return id;
            }

            return 0;
        }

        public SourceCollection ForRoom(Room room)
        {
            return this.ForRoom(room, false);
        }

        public SourceCollection ForRoom(Room room, bool includeGlobalSources)
        {
            if (includeGlobalSources)
                return new SourceCollection(InternalDictionary.Values.Where(s => s.Room == room || s.Room == null));
            return new SourceCollection(InternalDictionary.Values.Where(s => s.Room == room));
        }

        #region IEnumerable Members

        public new IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion
    }
}