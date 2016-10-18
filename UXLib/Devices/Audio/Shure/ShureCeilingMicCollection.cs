using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Shure
{
    public class ShureCeilingMicCollection : IEnumerable<ShureCeilingMic>, IVolumeDevice, ISocketDevice
    {
        public ShureCeilingMicCollection()
        {
            Mics = new Dictionary<int, ShureCeilingMic>();
        }

        Dictionary<int, ShureCeilingMic> Mics;

        public ShureCeilingMic this[int id]
        {
            get { return Mics[id]; }
        }

        public ShureCeilingMic Register(string hostName)
        {
            int id = Mics.Count() + 1;
            ShureCeilingMic mic = new ShureCeilingMic(id, string.Format("Mic {0}", id), hostName);
            Mics[id] = mic;
            mic.VolumeChanged += new VolumeDeviceChangeEventHandler(mic_VolumeChanged);
            return mic;
        }

        void mic_VolumeChanged(IVolumeDevice device, VolumeChangeEventArgs args)
        {
            if (VolumeChanged != null)
                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
        }

        public ShureLEDColor LEDMuteColor
        {
            set
            {
                foreach (ShureCeilingMic mic in this)
                    mic.LEDMuteColor = value;
            }
        }

        public ShureLEDColor LEDUnmuteColor
        {
            set
            {
                foreach (ShureCeilingMic mic in this)
                    mic.LEDUnmuteColor = value;
            }
        }

        public ShureLEDState LEDMuteState
        {
            set
            {
                foreach (ShureCeilingMic mic in this)
                    mic.LEDMuteState = value;
            }
        }

        public ShureLEDState LEDUnmuteState
        {
            set
            {
                foreach (ShureCeilingMic mic in this)
                    mic.LEDUnmuteState = value;
            }
        }

        #region IEnumerable<ShureCeilingMic> Members

        public IEnumerator<ShureCeilingMic> GetEnumerator()
        {
            return Mics.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IVolumeDevice Members

        public string Name
        {
            get { return "Ceiling Mics"; }
        }

        public ushort VolumeLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool VolumeMute
        {
            get
            {
                foreach (IVolumeDevice device in this.Where(m => m.Connected))
                {
                    if (device.VolumeMute == false)
                        return false;
                }
                return true;
            }
            set
            {
                foreach (IVolumeDevice device in this)
                {
                    device.VolumeMute = value;
                }
            }
        }

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public bool SupportsVolumeLevel
        {
            get { return false; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion

        #region ISocketDevice Members

        public string HostAddress
        {
            get { return this.First().HostAddress; }
        }

        public void Connect()
        {
            foreach (ISocketDevice device in this)
            {
                device.Connect();
            }
        }

        public void Disconnect()
        {
            foreach (ISocketDevice device in this)
            {
                device.Disconnect();
            }
        }

        public bool Connected
        {
            get
            {
                foreach (ISocketDevice device in this)
                {
                    if (!device.Connected)
                        return false;
                }
                return true;
            }
        }

        #endregion

        #region ICommDevice Members

        public bool DeviceCommunicating
        {
            get { throw new NotImplementedException(); }
        }

        public event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;

        public void Send(string stringToSend)
        {
            throw new NotImplementedException();
        }

        public void OnReceive(string receivedString)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            if (!this.Connected)
            {
                this.Connect();
            }
        }

        public CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        #endregion
    }
}