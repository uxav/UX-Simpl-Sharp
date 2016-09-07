using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class SourceCollection : IEnumerable<Source>
    {
        internal SourceCollection()
        {
            this.Sources = new List<Source>();
        }

        internal SourceCollection(IEnumerable<Source> listOfSources)
        {
            this.Sources = new List<Source>(listOfSources);
        }

        List<Source> Sources;

        public Source this[uint id]
        {
            get
            {
                return this.Sources.FirstOrDefault(s => s.ID == id);
            }
        }

        public int Count
        {
            get
            {
                return this.Sources.Count;
            }
        }

        public void AddSource(Source newSource)
        {
            if (!this.Sources.Contains(newSource))
            {
                this.Sources.Add(newSource);
            }
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
            return new SourceCollection(this.Sources.Where(s => s.GroupName == groupName).ToList());
        }

        public SourceCollection SourcesOfType(SourceType sourceType)
        {
            return new SourceCollection(this.Sources.Where(s => s.SourceType == sourceType).ToList());
        }

        public int IndexOf(Source source)
        {
            return this.Sources.IndexOf(source);
        }

        public ReadOnlyCollection<string> GetGroupedSourcesGroupNames()
        {
            List<string> results = new List<string>();

            foreach (Source source in this)
            {
                if (this.SourcesInGroup(source.GroupName).Count > 1 && !results.Contains(source.GroupName))
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
                bool exists = false;
                foreach (Source source in Sources)
                {
                    if (source.ID == id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    return id;
            }

            return 0;
        }

        public SourceCollection ForRoom(Room room)
        {
            return new SourceCollection(this.Sources.Where(s => s.Room == room).ToList());
        }

        public IEnumerator<Source> GetEnumerator()
        {
            return Sources.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}