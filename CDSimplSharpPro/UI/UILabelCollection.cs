using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UILabelCollection : IEnumerable<UILabel>
    {
        private List<UILabel> Labels;

        public UILabel this[UIKey key]
        {
            get
            {
                return this.Labels.FirstOrDefault(b => b.Key == key);
            }
        }

        public UILabel this[uint joinNumber]
        {
            get
            {
                return this.Labels.FirstOrDefault(b => b.JoinNumber == joinNumber);
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

        public void Add(UILabel label)
        {
            if (!this.Labels.Contains(label))
            {
                this.Labels.Add(label);
            }
        }

        public void Add(UIKey key, BasicTriList device, uint join)
        {
            UILabel newLabel = new UILabel(key, device, join);
            this.Labels.Add(newLabel);
        }

        public void Add(UIKey key, BasicTriList device, uint join, uint enableJoin, uint visibleJoin)
        {
            UILabel newLabel = new UILabel(key, device, join, enableJoin, visibleJoin);
            this.Labels.Add(newLabel);
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