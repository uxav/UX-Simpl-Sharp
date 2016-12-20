using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.QSC
{
    /// <summary>
    /// A collection of QSysControl elements
    /// </summary>
    public class QSysControlCollection : IEnumerable<QSysControl>
    {
        internal QSysControlCollection(QSys device)
        {
            Controls = new Dictionary<string, QSysControl>();
            ChangeGroups = new List<int>();
            QSys = device;
            QSys.HasConnected += new QSysConnectedEventHandler(QSys_HasConnected);
        }

        Dictionary<string, QSysControl> Controls { get; set; }
        List<int> ChangeGroups { get; set; }

        /// <summary>
        /// The QSys device which owns the controls
        /// </summary>
        public QSys QSys { get; protected set; }

        /// <summary>
        /// Get an object by it's ID
        /// </summary>
        /// <param name="id">The ID of the object</param>
        /// <returns>The QSysControl object with the ID</returns>
        public QSysControl this[string id]
        {
            get
            {
                return Controls[id];
            }
        }

        /// <summary>
        /// See if a control is contained in this collection
        /// </summary>
        /// <param name="id">The ID of the object</param>
        /// <returns>true if exists in collection</returns>
        public bool Contains(string id)
        {
            return this.Any(c => c.ControlID == id);
        }

        /// <summary>
        /// Register a named control object
        /// </summary>
        /// <param name="id">The named ID of the control</param>
        /// <param name="controlType">Set the type of control</param>
        /// <returns>The QSysControl object with the ID</returns>
        public QSysControl Register(string id, QSysControlType controlType)
        {
            this.Controls[id] = new QSysControl(this.QSys, id, controlType);
            return this[id];
        }

        /// <summary>
        /// Register a named control object
        /// </summary>
        /// <param name="id">The named ID of the control</param>
        /// <param name="controlType">Set the type of control</param>
        /// <param name="changeGroupID">Set the control to a change group</param>
        /// <returns>The QSysControl object with the ID</returns>
        public QSysControl Register(string id, QSysControlType controlType, int changeGroupID)
        {
            if (!ChangeGroups.Any(i => i == changeGroupID))
            {
                ChangeGroups.Add(changeGroupID);
                if (this.QSys.Connected)
                {
                    this.QSys.Send(string.Format("cgc {0}", changeGroupID));
                    this.QSys.Send(string.Format("cgs {0} {1}", changeGroupID, 200)); 
                }
            }
            this.Controls[id] = new QSysControl(this.QSys, id, controlType, changeGroupID);
            return this[id];
        }

        /// <summary>
        /// Poll all controls in the collection
        /// </summary>
        public void Poll()
        {
            foreach (QSysControl control in this)
            {
                control.Poll();
            }
        }

        void QSys_HasConnected(QSys device)
        {
            foreach (int group in ChangeGroups)
            {
                this.QSys.Send(string.Format("cgc {0}", group));
                this.QSys.Send(string.Format("cgs {0} {1}", group, 100));
            }

            foreach (QSysControl control in this)
            {
                control.RegisterOnConnect();
            }
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