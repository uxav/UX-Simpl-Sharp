using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class VolumeLevelCollection : IEnumerable<VolumeLevel>
    {
        public VolumeLevelCollection()
        {
            Levels = new List<VolumeLevel>();
        }

        public VolumeLevelCollection(List<VolumeLevel> fromLevels)
        {
            Levels = new List<VolumeLevel>(fromLevels);

#if DEBUG
            CrestronConsole.PrintLine("Created new VolumeLevelCollection with {0} items.. ", Levels.Count);
            foreach (VolumeLevel level in Levels)
            {
                CrestronConsole.PrintLine("  {0}: {1} ({2})", Levels.IndexOf(level), level.Device.Name, level.LevelType, ToString());
            }
#endif
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

        public void Add(Room room, VolumeLevelType type, IVolumeDevice volumeDevice)
        {
            Levels.Add(new VolumeLevel(room, type, volumeDevice));
        }

        public void Add(VolumeLevel level)
        {
            Levels.Add(level);
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