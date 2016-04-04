using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VoipLineCollection : IEnumerable<VoipLine>
    {
        public VoipLineCollection(List<VoipLine> lines)
        {
            Lines = new Dictionary<uint, VoipLine>();

            foreach (VoipLine line in lines)
            {
                if (!Lines.ContainsKey(line.Number))
                    Lines.Add(line.Number, line);
            }
        }

        Dictionary<uint, VoipLine> Lines;

        public VoipLine this[uint lineNumber]
        {
            get
            {
                return Lines[lineNumber];
            }
        }

        public int Count { get { return Lines.Count; } }

        #region IEnumerable<VoipLine> Members

        public IEnumerator<VoipLine> GetEnumerator()
        {
            return Lines.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}