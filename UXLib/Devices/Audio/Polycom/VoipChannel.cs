using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VoipChannel : VirtualChannel
    {
        public VoipChannel(Soundstructure device, string name, SoundstructurePhysicalChannelType pcType, uint[] values)
            : base(device, name, SoundstructureVirtualChannelType.MONO, pcType, values)
        {
            this.Device.VoipInfoReceived += new SoundstructureVoipInfoReceivedHandler(Device_VoipInfoReceived);
        }

        public override void Init()
        {
            base.Init();
        }

        protected virtual void OnVoipInfoReceived(string command, string info)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0}, {1} = {2}", this.Name, command, info);
            CrestronConsole.Print("  elements =");
            foreach (string element in SoundstructureSocket.ElementsFromString(info))
            {
                CrestronConsole.Print(" {0}", element);
            }
            CrestronConsole.PrintLine("");
#endif
        }

        void Device_VoipInfoReceived(ISoundstructureItem item, SoundstructureVoipInfoReceivedEventArgs args)
        {
            if (item == this)
            {
                OnVoipInfoReceived(args.Command, args.Info);
            }
        }
    }
}