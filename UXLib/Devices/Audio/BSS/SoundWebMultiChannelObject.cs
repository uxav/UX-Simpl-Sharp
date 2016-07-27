using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebMultiChannelObject : SoundWebObject, IEnumerable<SoundWebChannel>
    {
        public SoundWebMultiChannelObject(SoundWeb device, string address)
        {
            this.Device = device;
            HiQAddress = address;
            this.channels = new List<SoundWebChannel>();
        }

        public int ChannelCount
        {
            get
            {
                return this.channels.Count();
            }
        }

        protected List<SoundWebChannel> channels { get; set; }


        #region IEnumerable<SoundWebChannel> Members

        public IEnumerator<SoundWebChannel> GetEnumerator()
        {
            return this.channels.GetEnumerator();
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