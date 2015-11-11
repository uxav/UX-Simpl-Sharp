using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
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

        public void AddSource(Source newSource)
        {
            if (!this.Sources.Contains(newSource))
            {
                this.Sources.Add(newSource);
            }
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