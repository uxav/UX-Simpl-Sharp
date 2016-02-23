using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.Polycom
{
    public class Soundstructure
    {
        public Soundstructure(string ipAdress)
        {
            this.VirtualChannels = new Dictionary<string, SoundstructureVirtualChannel>();
            socket = new SoundstructureSocket(this, ipAdress);
        }

        SoundstructureSocket socket;

        public Dictionary<string, SoundstructureVirtualChannel> VirtualChannels;

        public void Init()
        {
            this.VirtualChannels.Clear();
            socket.Send("vclist");
        }
    }

    public enum SoundstructureVirtualChannelType
    {
        mono,
        stereo,
        control,
        control_array
    }

    public enum SoundsrtucturePhysicalChannelType
    {
        cr_mic_in,
        cr_line_out,
        sr_mic_in,
        sr_line_out,
        pstn_in,
        pstn_out,
        voip_in,
        voip_out,
        sig_gen,
        submix,
        clink_in,
        clink_out,
        digital_gpio_in,
        digital_gpio_out,
        analog_gpio_in,
        ir_in
    }
}