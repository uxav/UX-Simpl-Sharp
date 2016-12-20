using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Extensions;

namespace UXLib.Devices.Audio.Revolabs
{
    public class RevolabsComHandler
    {
        internal RevolabsComHandler(ComPort comPort)
        {
            ComPort = comPort;
            RxQueue = new CrestronQueue<byte>(1000);

            if (!ComPort.Registered)
            {
                if (ComPort.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register com port {0}", ComPort.ID);
                }
            }

            ComPort.SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate115200,
                ComPort.eComDataBits.ComspecDataBits8,
                ComPort.eComParityType.ComspecParityNone,
                ComPort.eComStopBits.ComspecStopBits1,
                ComPort.eComProtocolType.ComspecProtocolRS232,
                ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                false);

            TxQueue = new CrestronQueue<string>(10);
        }

        ComPort ComPort { get; set; }
        CrestronQueue<string> TxQueue { get; set; }
        CrestronQueue<byte> RxQueue { get; set; }
        Thread TxThread { get; set; }
        Thread RxThread { get; set; }

        public bool Initialized { get; private set; }
        public void Initialize()
        {
            if (!Initialized)
            {
                TxThread = new Thread(SendBufferProcess, null, Thread.eThreadStartOptions.CreateSuspended);
                TxThread.Priority = Thread.eThreadPriority.UberPriority;
                TxThread.Name = "Revolabs ComPort - Tx Handler";
                TxThread.Start();
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
                RxThread = new Thread(ReceiveBufferProcess, null, Thread.eThreadStartOptions.CreateSuspended);
                RxThread.Priority = Thread.eThreadPriority.UberPriority;
                RxThread.Name = "Revolabs ComPort - Rx Handler";
                RxThread.Start();
                ComPort.SerialDataReceived += new ComPortDataReceivedEvent(ComPort_SerialDataReceived);
                Initialized = true;
            }
        }

        void ComPort_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
        {
            foreach (byte b in args.SerialData.ToByteArray())
                RxQueue.Enqueue(b);
        }

        object SendBufferProcess(object o)
        {
            while (true)
            {
                try
                {
                    string data = TxQueue.Dequeue();
#if DEBUG
                    CrestronConsole.Print("Revolabs Tx: {0}", data);
#endif
                    if (programStopping)
                        return null;
                    else
                        this.ComPort.Send(data);

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

                    if (b == 10)
                    {
                        // skip line feed
                    }

                    // If find byte = CR
                    else if (b == 13)
                    {
                        // Copy bytes to new array with length of packet and ignoring the CR.
                        Byte[] copiedBytes = new Byte[byteIndex];
                        Array.Copy(bytes, copiedBytes, byteIndex);

                        byteIndex = 0;
#if DEBUG
                        CrestronConsole.Print("Revolabs Rx: ");
                        Tools.PrintBytes(copiedBytes, copiedBytes.Length, true);
#endif
                        if (ReceivedData != null)
                            ReceivedData(this, Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length));
                        CrestronEnvironment.AllowOtherAppsToRun();
                    }
                    else
                    {
                        bytes[byteIndex] = b;
                        byteIndex++;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                    {
#if DEBUG
                        CrestronConsole.Print("Error in Revolabs Rx: ");
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

        public void Send(string stringToSend)
        {
            if(stringToSend.Length > 0 && stringToSend[stringToSend.Length - 1] != 0x0d)
            {
                stringToSend = stringToSend + "\r";
            }

            if (stringToSend.Length > 0)
            {
                TxQueue.Enqueue(stringToSend);
            }
        }

        public event RevolabsdReceivedDataEventHandler ReceivedData;
    }

    public delegate void RevolabsdReceivedDataEventHandler(RevolabsComHandler handler, string data);
}