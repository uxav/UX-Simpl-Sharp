/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2017 UX Digital Systems Ltd
 * 
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Extensions;

namespace UXLib.Devices.Displays.VividTouch
{
    public class VividTouchComPortHandler
    {
        private readonly ComPort _comPort;
        private readonly CrestronQueue<byte> _rxQueue = new CrestronQueue<byte>(1000);
        private readonly CrestronQueue<byte[]> _txQueue = new CrestronQueue<byte[]>(100);
        private Thread _rxThread;
        private Thread _txThread;
        private bool _programStopping;

        public VividTouchComPortHandler(ComPort comPort)
        {
            _comPort = comPort;

            if (_comPort.Registered) return;
            _comPort.Register();

            _comPort.SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate115200,
                ComPort.eComDataBits.ComspecDataBits8,
                ComPort.eComParityType.ComspecParityNone, ComPort.eComStopBits.ComspecStopBits1,
                ComPort.eComProtocolType.ComspecProtocolRS232,
                ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone, false);

            _comPort.SerialDataReceived += ComPortOnSerialDataReceived;
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type != eProgramStatusEventType.Stopping) return;

                _programStopping = true;
                _rxQueue.Enqueue(0x00);
            };
        }

        public event ReceivedDataEventHandler ReceivedData;

        protected virtual void OnReceivedData(byte[] receiveddata)
        {
            ReceivedDataEventHandler handler = ReceivedData;
            if (handler != null) handler(receiveddata);
        }

        private object RxThread(object userSpecific)
        {
            Byte[] bytes = new Byte[1000];
            int byteIndex = 0;

            while (!_programStopping)
            {
                CrestronEnvironment.AllowOtherAppsToRun();
                Thread.Sleep(0);

                try
                {
                    byte b = _rxQueue.Dequeue();

                    if (byteIndex == 7 && b == 0x08)
                    {
                        bytes[byteIndex] = b;
                        var copiedBytes = new Byte[byteIndex + 1];
                        Array.Copy(bytes, copiedBytes, byteIndex + 1);

                        byteIndex = 0;
#if DEBUG
                        CrestronConsole.PrintLine("VT Board Rx: ");
                        Tools.PrintBytes(copiedBytes, copiedBytes.Length, true);
#endif
                        OnReceivedData(copiedBytes);
                    }
                    else if (byteIndex == 7 && b != 0x08)
                        byteIndex = 0;
                    else
                    {
                        bytes[byteIndex] = b;
                        byteIndex++;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "ThreadAbortException")
                        ErrorLog.Error("{0} - Error in thread: {1}, byteIndex = {2}", GetType().ToString(), e.Message, byteIndex);
                }
            }

            return null;
        }

        private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args)
        {
#if DEBUG
            //CrestronConsole.PrintLine("VT Board Rx: ");
#endif
            foreach (var b in args.SerialData.ToByteArray())
            {
#if DEBUG
                //CrestronConsole.Print(@"\x" + b.ToString("X2"));
#endif
                _rxQueue.Enqueue(b);
            }

            if (_rxThread != null && _rxThread.ThreadState == Thread.eThreadStates.ThreadRunning) return;
            
            _rxThread = new Thread(RxThread, null)
            {
                Name = string.Format("{0} Rx Handler", GetType().Name),
                Priority = Thread.eThreadPriority.HighPriority
            };
        }

        public void Send(uint id, VividTouchMessageType messageType, byte[] bytes)
        {
            var message = new byte[bytes.Length + 5];

            message[0] = 0x07;
            message[1] = (byte)id;
            message[2] = (byte)messageType;
            Array.Copy(bytes, 0, message, 3, bytes.Length);
            message[bytes.Length + 3] = 0x08;
            message[bytes.Length + 4] = 0x0d;

#if DEBUG
            CrestronConsole.PrintLine("VT Board Tx: ");
            Tools.PrintBytes(message, message.Length, true);
#endif
            _txQueue.Enqueue(message);

            if(_txThread == null || _txThread.ThreadState != Thread.eThreadStates.ThreadRunning)
            {
                _txThread = new Thread(specific =>
                {
                    while (!_txQueue.IsEmpty)
                    {
                        var m = _txQueue.Dequeue();
                        _comPort.Send(m, m.Length);
                        Thread.Sleep(200);
                    }
                    return null;
                }, null)
                {
                    Name = string.Format("{0} Tx Handler", GetType().Name),
                    Priority = Thread.eThreadPriority.MediumPriority
                };
            }
        }

        public bool TxBusy
        {
            get { return !_txQueue.IsEmpty; }
        }
    }

    public delegate void ReceivedDataEventHandler(byte[] receivedData);

    public enum VividTouchMessageType : byte
    {
        Reply = 0x00,
        Read = 0x01,
        Write = 0x02
    }
}