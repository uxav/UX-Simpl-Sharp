using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class VolumeLevelCollection : UXCollection<VolumeLevel>
    {
        public VolumeLevelCollection() { }

        public VolumeLevelCollection(List<VolumeLevel> fromLevels)
        {
            InternalDictionary = new Dictionary<uint, VolumeLevel>();
            uint count = 0;
            foreach (VolumeLevel level in fromLevels)
            {
                this[count] = level;
                count++;
            }
        }

        public override VolumeLevel this[uint index]
        {
            get
            {
                return base[index];
            }
            internal set
            {
                base[index] = value;
            }
        }

        public VolumeLevel this[VolumeLevelType type]
        {
            get
            {
                return InternalDictionary.Values.FirstOrDefault(l => l.LevelType == type);
            }
        }

        public override bool Contains(uint index)
        {
            return base.Contains(index);
        }

        public override bool Contains(VolumeLevel volumeLevel)
        {
            return base.Contains(volumeLevel);
        }

        public bool Contains(VolumeLevelType type)
        {
            return InternalDictionary.Values.Any(l => l.LevelType == type);
        }

        public VolumeLevelCollection LevelsForType(VolumeLevelType type)
        {
            return new VolumeLevelCollection(InternalDictionary.Values.Where(l => l.LevelType == type).ToList());
        }

        public void Add(Room room, VolumeLevelType type, IVolumeDevice volumeDevice)
        {
            this.Add(new VolumeLevel(room, type, volumeDevice));
        }

        public void Add(VolumeLevel newLevel)
        {
            for (uint key = 0; key <= uint.MaxValue; key++)
            {
                if (!this.Contains(key))
                {
                    this[key] = newLevel;
                    break;
                }
            }
        }
    }
}