using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class AnalogPhoneOutChannel : VirtualChannel, ITelcoInterface
    {
        public AnalogPhoneOutChannel(Soundstructure device, string name, uint[] values)
            : base(device, name, SoundstructureVirtualChannelType.MONO, SoundstructurePhysicalChannelType.PSTN_OUT, values)
        {
            
        }

        public override void Init()
        {
            base.Init();

            Device.Socket.Get(this, SoundstructureCommandType.PHONE_CONNECT);
        }

        protected override void OnFeedbackReceived(SoundstructureCommandType commandType, string commandModifier, double value)
        {
            switch (commandType)
            {
                case SoundstructureCommandType.PHONE_CONNECT:
                    _OffHook = Convert.ToBoolean(value);

                    if (OffHookChange != null)
                        OffHookChange(this, _OffHook);
#if DEBUG
                    CrestronConsole.PrintLine("{0} OffHook = {1}", Name, OffHook);
#endif
                    break;
            }

            base.OnFeedbackReceived(commandType, commandModifier, value);
        }

        bool _OffHook = false;

        #region ITelcoInterface Members

        public bool OffHook
        {
            get
            {
                return _OffHook;
            }
            set
            {
                Device.Socket.Set(this, SoundstructureCommandType.PHONE_CONNECT, value);
            }
        }

        public void Dial(string number)
        {
            if (!OffHook)
                OffHook = true;
            Device.Socket.Set(this, SoundstructureCommandType.PHONE_DIAL, number);
        }

        public void Reject()
        {
            Device.Socket.Set(this, SoundstructureCommandType.PHONE_REJECT);
        }

        public void Answer()
        {
            OffHook = true;
        }

        public void Ignore()
        {
            Device.Socket.Set(this, SoundstructureCommandType.PHONE_IGNORE);
        }

        public event TelcoOffHookStatusChangeEventHandler OffHookChange;

        #endregion
    }
}