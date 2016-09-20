using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Extensions;

namespace UXLib.Devices.Displays.Samsung
{
    public class SamsungMDCComPortHandler
    {
        public SamsungMDCComPortHandler(ComPort comPort)
        {
            ComPort = comPort;
            RxQueue = new CrestronQueue<byte>();

            if (!ComPort.Registered)
            {
                if (ComPort.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register com port {0}", ComPort.ID);
                }
            }

            ComPort.SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate9600,
                ComPort.eComDataBits.ComspecDataBits8,
                ComPort.eComParityType.ComspecParityNone,
                ComPort.eComStopBits.ComspecStopBits1,
                ComPort.eComProtocolType.ComspecProtocolRS232,
                ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                false);

            TxQueue = new CrestronQueue<byte[]>();
        }

        ComPort ComPort;
        CrestronQueue<byte[]> TxQueue { get; set; }
        CrestronQueue<byte> RxQueue { get; set; }
        Thread TxThread { get; set; }
        Thread RxThread { get; set; }

        public bool Initialized { get; private set; }
        public void Initialize()
        {
            if (!Initialized)
            {
                TxThread = new Thread(SendBufferProcess, null, Thread.eThreadStartOptions.Running);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
                RxThread = new Thread(ReceiveBufferProcess, null, Thread.eThreadStartOptions.Running);
                ComPort.SerialDataReceived += new ComPortDataReceivedEvent(ComPort_SerialDataReceived);
                Initialized = true;
            }
        }

        void ComPort_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
        {
            foreach (byte b in args.SerialData.ToByteArray())
                RxQueue.Enqueue(b);
        }

        public event SamsungMDCComPortReceivedPacketEventHandler ReceivedPacket;

        object SendBufferProcess(object o)
        {
            while (true)
            {
                try
                {
                    Byte[] packet = TxQueue.Dequeue();
#if DEBUG
                    CrestronConsole.Print("Samsung Tx: ");
                    Tools.PrintBytes(packet, packet.Length);
#endif
                    if (programStopping)
                        return null;
                    else
                        this.ComPort.Send(packet, packet.Length);

                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                    {
                        ErrorLog.Exception(string.Format("{0} - Exception in tx buffer thread", GetType().Name), e);
                    }
                }
            }
        }

        object ReceiveBufferProcess(object obj)
        {
            Byte[] bytes = new Byte[1000];
            int byteIndex = 0;
            int dataLength = 0;

            while (true)
            {
                try
                {
                    byte b = RxQueue.Dequeue();

                    if (programStopping)
                        return null;

                    if (b == 0xAA)
                    {
                        byteIndex = 0;
                        dataLength = 0;
                    }
                    else
                        byteIndex++;

                    bytes[byteIndex] = b;
                    if (byteIndex == 3)
                        dataLength = bytes[byteIndex];

                    if (byteIndex == (dataLength + 4))
                    {
                        int chk = bytes[byteIndex];

                        int test = 0;
                        for (int i = 1; i < byteIndex; i++)
                            test = test + bytes[i];

                        if (chk == (byte)test)
                        {
                            byte[] copiedBytes = new byte[byteIndex];
                            Array.Copy(bytes, copiedBytes, byteIndex);
                            if (ReceivedPacket != null)
                                ReceivedPacket(this, copiedBytes);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                    {
#if DEBUG
                        CrestronConsole.Print("Error in Samsung Rx: ");
                        Tools.PrintBytes(bytes, byteIndex);
#endif
                        ErrorLog.Exception(string.Format("{0} - Exception in rx thread", GetType().Name), e);
                    }
                }
            }
        }

        bool programStopping = false;
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
            {
                programStopping = true;
                TxThread.Abort();
                RxThread.Abort();
            }
        }

        public void Send(byte[] bytes, int length)
        {
            // Packet must start with correct header
            if (bytes[0] == 0xAA)
            {
                int dLen = bytes[3];
                byte[] packet = new byte[dLen + 5];
                Array.Copy(bytes, packet, bytes.Length);
                int chk = 0;
                for (int i = 1; i < bytes.Length; i++)
                    chk = chk + bytes[i];
                packet[packet.Length - 1] = (byte)chk;
                TxQueue.Enqueue(packet);
            }
            else
            {
                throw new FormatException("Packet did not begin with correct value");
            }
        }
    }

    public delegate void SamsungMDCComPortReceivedPacketEventHandler(SamsungMDCComPortHandler handler, byte[] receivedPacket);
}