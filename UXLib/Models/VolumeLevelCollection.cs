using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public sealed class VolumeLevelCollection : UXCollection<VolumeLevel>
    {
        public VolumeLevelCollection() { }

        public VolumeLevelCollection(List<VolumeLevel> fromLevels)
        {
            uint count = 0;
            foreach (VolumeLevel level in fromLevels)
            {
                Add(count, level);
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

        public VolumeLevel Add(VolumeLevelType type, IVolumeDevice volumeDevice)
        {
            VolumeLevel level = new VolumeLevel(type, volumeDevice);
            this.Add(level);
            return level;
        }

        public VolumeLevel Add(Room room, VolumeLevelType type, IVolumeDevice volumeDevice)
        {
            VolumeLevel level = new VolumeLevel(room, type, volumeDevice);
            this.Add(level);
            return level;
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