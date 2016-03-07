using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.Polycom
{
    public class SoundstructureItemCollection : IEnumerable<ISoundstructureItem>
    {
        public SoundstructureItemCollection(List<ISoundstructureItem> fromChannels)
        {
            items = new Dictionary<string, ISoundstructureItem>();
            foreach (ISoundstructureItem item in fromChannels)
            {
                if (!items.ContainsKey(item.Name))
                {
                    items.Add(item.Name, item);
                }
            }
        }

        Dictionary<string, ISoundstructureItem> items;

        public ISoundstructureItem this[string channelName]
        {
            get
            {
                if (this.items.ContainsKey(channelName))
                    return items[channelName];
                return null;
            }
        }

        public bool Contains(string channelName)
        {
            return this.items.ContainsKey(channelName);
        }

        #region IEnumerable<VirtualChannel> Members

        public IEnumerator<ISoundstructureItem> GetEnumerator()
        {
            return items.Values.GetEnumerator();
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