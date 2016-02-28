using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UILabelCollection : IEnumerable<UILabel>
    {
        private List<UILabel> Labels;

        public UILabel this[uint joinNumber]
        {
            get
            {
                return this.Labels.FirstOrDefault(l => l.TextSerialJoin.Number == joinNumber);
            }
        }

        public int NumberOfLabels
        {
            get
            {
                return this.Labels.Count;
            }
        }

        public UILabelCollection()
        {
            this.Labels = new List<UILabel>();
        }

        public void Add(UILabel newLabel)
        {
            if (!this.Labels.Contains(newLabel))
            {
                this.Labels.Add(newLabel);
            }
        }

        public IEnumerator<UILabel> GetEnumerator()
        {
            return Labels.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}