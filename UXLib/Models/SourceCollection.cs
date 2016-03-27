using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class SourceCollection : IEnumerable<Source>
    {
        List<Source> Sources;

        public Source this[uint id]
        {
            get
            {
                return this.Sources.FirstOrDefault(s => s.ID == id);
            }
        }

        public int NumberOfSources
        {
            get
            {
                return this.Sources.Count;
            }
        }

        public SourceCollection()
        {
            this.Sources = new List<Source>();
        }

        public SourceCollection(List<Source> listOfSources)
        {
            this.Sources = listOfSources;
        }

        public void AddSource(Source newSource)
        {
            if (!this.Sources.Contains(newSource))
            {
                this.Sources.Add(newSource);
            }
        }

        public int GroupCount(string groupName)
        {
            return Sources.Where(s => s.GroupName == groupName).ToList().Count;
        }

        public SourceCollection GetSingleSources()
        {
            List<Source> singleSources = new List<Source>();

            foreach (Source source in this)
            {
                if (this.GroupCount(source.GroupName) <= 1)
                {
                    singleSources.Add(source);
                }
            }

            return new SourceCollection(singleSources);
        }

        public List<string> GetGroupedSourcesGroupNames()
        {
            List<string> results = new List<string>();

            foreach (Source source in this)
            {
                if (this.GroupCount(source.GroupName) > 1 && !results.Contains(source.GroupName))
                {
                    results.Add(source.GroupName);
                }
            }

            return results;
        }

        public SourceCollection ForRoom(Room room)
        {
            return new SourceCollection(this.Sources.Where(s => s.Room == room).ToList());
        }

        public SourceCollection SourcesWithGroupName(string groupName)
        {
            return new SourceCollection(this.Sources.Where(s => s.GroupName == groupName).ToList());
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