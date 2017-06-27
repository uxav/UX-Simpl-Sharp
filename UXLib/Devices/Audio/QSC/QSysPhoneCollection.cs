using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysPhoneCollection : IEnumerable<QSysSoftPhone>
    {
        internal QSysPhoneCollection(QSys device)
        {
            QSys = device;
            Phones = new Dictionary<int, QSysSoftPhone>();
        }

        /// <summary>
        /// The QSys device which owns the controls
        /// </summary>
        public QSys QSys { get; protected set; }

        Dictionary<int, QSysSoftPhone> Phones { get; set; }

        /// <summary>
        /// Get a phone by the key number
        /// </summary>
        /// <param name="key">Key of the registered phone</param>
        /// <returns></returns>
        public QSysSoftPhone this[int key]
        {
            get { return Phones[key]; }
        }

        /// <summary>
        /// Register a softphone to control
        /// </summary>
        /// <param name="idOffHookLED"></param>
        /// <param name="idRingingLED"></param>
        /// <param name="idConnect"></param>
        /// <param name="idDisconnect"></param>
        /// <param name="idDialString"></param>
        /// <param name="idDND"></param>
        /// <param name="idProgress"></param>
        /// <param name="idKeypadBaseName"></param>
        /// <param name="changeGroupID"></param>
        /// <returns>The instance of the phone created</returns>
        public QSysSoftPhone Register(string idOffHookLED, string idRingingLED, string idConnect, string idDisconnect,
            string idDialString, string idDND, string idProgress, string idKeypadBaseName, int changeGroupID)
        {
            int id = Phones.Count + 1;
            Phones[id] = new QSysSoftPhone(this.QSys, id, idOffHookLED, idRingingLED, idConnect, idDisconnect,
                idDialString, idDND, idProgress, idKeypadBaseName, changeGroupID);
            return this[id];
        }

        #region IEnumerable<QSysSoftPhone> Members

        public IEnumerator<QSysSoftPhone> GetEnumerator()
        {
            return Phones.Values.GetEnumerator();
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