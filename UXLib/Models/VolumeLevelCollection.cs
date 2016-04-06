using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class VolumeLevelCollection : IEnumerable<VolumeLevel>
    {
        public VolumeLevelCollection(List<VolumeLevel> fromLevels)
        {
            Levels = new List<VolumeLevel>(fromLevels);
        }

        List<VolumeLevel> Levels { get; set; }

        public VolumeLevel this[int index]
        {
            get
            {
                return Levels[index];
            }
        }

        public VolumeLevelCollection LevelsForType(VolumeLevelType type)
        {
            return new VolumeLevelCollection(Levels.Where(l => l.LevelType == type).ToList());
        }

        public int Count
        {
            get
            {
                return Levels.Count;
            }
        }

        #region IEnumerable<VolumeLevel> Members

        public IEnumerator<VolumeLevel> GetEnumerator()
        {
            return Levels.GetEnumerator();
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