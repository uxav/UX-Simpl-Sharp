using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Revolabs
{
    public class RevolabMicGroupCollection : UXReadOnlyCollection<string, RevolabMicCollection>
    {
        public RevolabMicGroupCollection(RevolabsExecutiveElite controller)
        {
            this.Controller = controller;
        }

        public RevolabsExecutiveElite Controller { get; protected set; }

        internal override void Add(string groupName, RevolabMicCollection value)
        {
            base.Add(groupName, value);
        }

        public override bool Contains(string groupName)
        {
            return base.Contains(groupName);
        }

        internal void Rename(string oldGroupName, string newGroupName)
        {
            if (this.Contains(oldGroupName))
            {
                this[newGroupName] = this[oldGroupName];
                this.InternalDictionary.Remove(oldGroupName);
                this.InternalDictionary[newGroupName].Rename(newGroupName);
            }
        }

        public override RevolabMicCollection this[string groupName]
        {
            get
            {
                return base[groupName];
            }
            internal set
            {
                base[groupName] = value;
            }
        }
    }
}