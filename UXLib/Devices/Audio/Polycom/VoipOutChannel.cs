using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VoipOutChannel : VoipChannel, ISoundstructurePhoneOutChannel
    {
        public VoipOutChannel(Soundstructure device, string name, uint[] values)
            : base(device, name, SoundstructurePhysicalChannelType.VOIP_OUT, values) { }

        public override void Init()
        {
            base.Init();

            this.Device.Socket.Get(this, SoundstructureCommandType.PHONE_CONNECT);
            this.Device.Socket.Send(string.Format("get voip_board_info \"{0}\"", this.Name));
            this.Device.Socket.Send(string.Format("get voip_status \"{0}\" 1", this.Name));
            this.Device.Socket.Send(string.Format("get voip_line \"{0}\"", this.Name));
            this.Device.Socket.Send(string.Format("get voip_line_label \"{0}\" 1", this.Name));
            this.Device.Socket.Send(string.Format("get voip_call_appearance_info \"{0}\" 1 1", this.Name));
            this.Device.Socket.Send(string.Format("get voip_call_appearance_info \"{0}\" 1 2", this.Name));
            this.Device.Socket.Send(string.Format("get voip_call_appearance_line \"{0}\" 1", this.Name));
            this.Device.Socket.Send(string.Format("get voip_call_appearance_state \"{0}\" 1", this.Name));
        }

        protected override void OnFeedbackReceived(SoundstructureCommandType commandType, string commandModifier, double value)
        {
            switch (commandType)
            {
                case SoundstructureCommandType.PHONE_CONNECT:
                    _OffHook = Convert.ToBoolean(value);
                    break;
            }
            
            base.OnFeedbackReceived(commandType, commandModifier, value);
        }

        protected override void OnVoipInfoReceived(string command, string info)
        {
            switch (command)
            {
                case "voip_board_info":
                    info = info.Split(' ').Last();
                    MacAddress = info.Substring(4, info.Length - 5);
                    break;
            }
            
            base.OnVoipInfoReceived(command, info);
        }

        public string MacAddress { get; protected set; }

        bool _OffHook = false;

        #region ISoundstructurePhoneOutChannel Members

        public bool OffHook
        {
            get
            {
                return _OffHook;
            }
            set
            {
                this.Device.Socket.Set(this, SoundstructureCommandType.PHONE_CONNECT, value);
            }
        }

        public void Dial(string number)
        {
            if (!OffHook)
            {
                OffHook = true;
                this.Device.Socket.Set(this, SoundstructureCommandType.VOIP_SEND, number);
            }
            else
                this.Device.Socket.Set(this, SoundstructureCommandType.PHONE_DIAL, number);
        }

        #endregion
    }
}