using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Sockets;

namespace UXLib.Audio.Polycom
{
    public class SoundstructureSocket : SimpleClientSocket
    {
        public SoundstructureSocket(Soundstructure device, string ipAddress)
            : base (ipAddress, 52774, 1000)
        {
            Device = device;
            this.ReceivedPacketEvent += new SimpleClientSocketReceiveEventHandler(SoundstructureSocket_ReceivedPacketEvent);
        }

        Soundstructure Device;

        public override Crestron.SimplSharp.CrestronSockets.SocketErrorCodes Send(string str)
        {
            str = str + "\x0d";

            return base.Send(str);
        }

        void SoundstructureSocket_ReceivedPacketEvent(SimpleClientSocket socket, SimpleClientSocketReceiveEventArgs args)
        {
            string reply = Encoding.Default.GetString(args.ReceivedPacket, 0, args.ReceivedPacket.Length);

            if (reply.Contains(' '))
            {
                string[] words = reply.Split(' ');

                switch (words[0])
                {
                    case "vcitem":
                        // this should be a response from the vclist command which sends back all virtual channels defined
                        try
                        {
                            SoundstructureVirtualChannel channel = new SoundstructureVirtualChannel(words[1],
                                (SoundstructureVirtualChannelType)Enum.Parse(typeof(SoundstructureVirtualChannelType), words[2], true),
                                (SoundsrtucturePhysicalChannelType)Enum.Parse(typeof(SoundsrtucturePhysicalChannelType), words[3], true));
                            if (!Device.VirtualChannels.ContainsKey(channel.Name))
                                Device.VirtualChannels.Add(channel.Name, channel);
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error("Error parsing Soundstructure vcitem: {0}", e.Message);
                        }
                        break;
                    case "val":
                        // this should be a value response from a set or get
                        {
                            switch (words[1])
                            {
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}