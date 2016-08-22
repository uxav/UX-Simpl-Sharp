using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysControlCollection : IEnumerable<QSysControl>
    {
        internal QSysControlCollection(QSys device)
        {
            Controls = new Dictionary<string, QSysControl>();
            QSys = device;
        }

        Dictionary<string, QSysControl> Controls { get; set; }

        public QSys QSys { get; protected set; }

        public QSysControl this[string id]
        {
            get
            {
                return Controls[id];
            }
        }

        public void Register(string id, QSysControlType controlType)
        {
            this.Controls[id] = new QSysControl(this.QSys, id, controlType);
        }

        #region IEnumerable<QsysControl> Members

        public IEnumerator<QSysControl> GetEnumerator()
        {
            return Controls.Values.GetEnumerator();
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