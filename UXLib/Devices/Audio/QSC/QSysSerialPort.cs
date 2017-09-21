using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UXLib.Extensions;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysSerialPort : IQSysCommsHandler
    {
        public QSysSerialPort(ComPort comPort)
        {
            ComPort = comPort;
            if (!ComPort.Registered)
            {
                if (ComPort.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Could not register ComPort {0} for {1}", ComPort.ID, GetType().Name);
                }
            }

            if (ComPort.Registered)
            {
                ComPort.SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate115200,
                    ComPort.eComDataBits.ComspecDataBits8, ComPort.eComParityType.ComspecParityNone,
                    ComPort.eComStopBits.ComspecStopBits1, ComPort.eComProtocolType.ComspecProtocolRS232,
                    ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                    ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone, false);
            }

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                _programRunning = type != eProgramStatusEventType.Stopping;
                if(_programRunning) return;
                
                if (_pollTimer != null && !_pollTimer.Disposed)
                {
                    _pollTimer.Stop();
                    _pollTimer.Dispose();
                }
                _rxQueue.Enqueue(0x00);
                _txQueue.Enqueue(null);
            };
        }

        public ComPort ComPort { get; private set; }
        private readonly CrestronQueue<byte[]> _txQueue = new CrestronQueue<byte[]>(50);
        private readonly CrestronQueue<byte> _rxQueue = new CrestronQueue<byte>(100);
        private Thread _txThread;
        private Thread _rxThread;
        private CTimer _pollTimer;
        private bool _programRunning = true;
        private bool _commsOk;

        public void Initialize()
        {
            _commsOk = false;
            if (CommsStatusChange != null)
                CommsStatusChange(this, false);
            if (_pollTimer != null && !_pollTimer.Disposed)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }
            _txQueue.Clear();
            _rxQueue.Clear();
            ComPort.SerialDataReceived += ComPortOnSerialDataReceived;
            _pollTimer = new CTimer(specific => Send("sg"), null, 1000, 30000);
        }

        public void Send(string str)
        {
            var bytes = new byte[str.Length + 1];

            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = unchecked((byte)str[i]);
            }

            bytes[str.Length] = 0x0a;

            _txQueue.Enqueue(bytes);

            if (_txThread == null || _txThread.ThreadState != Thread.eThreadStates.ThreadRunning)
            {
                _txThread = new Thread(specific =>
                {
                    Thread.CurrentThread.Name = string.Format("{0} Tx Handler", GetType().Name);
                    while (_programRunning)
                    {
                        var qBytes = _txQueue.Dequeue();
                        if (qBytes != null)
                        {
#if DEBUG
                            CrestronConsole.Print("QSys Tx: ");
                            Tools.PrintBytes(qBytes, qBytes.Length, true);
#endif
                            ComPort.Send(qBytes, qBytes.Length);
                        }
                    }
                    return null;
                }, null);
            }
        }

        private void ComPortOnSerialDataReceived(ComPort receivingComPort, ComPortSerialDataEventArgs args)
        {
            if (_rxThread == null || _rxThread.ThreadState != Thread.eThreadStates.ThreadRunning)
            {
                _rxThread = new Thread(specific =>
                {
                    Thread.CurrentThread.Name = string.Format("{0} Rx Handler", GetType().Name);
                    Thread.CurrentThread.Priority = Thread.eThreadPriority.HighPriority;

                    var index = 0;
                    var bytes = new Byte[1000];

                    while (_programRunning)
                    {
                        var b = _rxQueue.Dequeue();

                        if (b == 13) { }
                        // skip
                        else if (b == 10)
                        {
                            // Copy bytes to new array with length of packet and ignoring the CR.
                            var copiedBytes = new Byte[index];
                            Array.Copy(bytes, copiedBytes, index);

                            index = 0;

                            if (Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length) != "cgpa")
                            {
                                if (ReceivedControlResponse != null)
                                    ReceivedControlResponse(this, copiedBytes);
#if DEBUG
                                CrestronConsole.PrintLine("{0} Processed reply: {1}", this.GetType().Name, Encoding.ASCII.GetString(copiedBytes, 0, copiedBytes.Length));
#endif
                                if (!_commsOk)
                                {
                                    _commsOk = true;
                                    if (CommsStatusChange != null)
                                        CommsStatusChange(this, true);
                                }
                            }
                        }
                        else
                        {
                            if (index < bytes.Length)
                            {
                                bytes[index] = b;
                                index++;
                            }
                            else
                            {
                                ErrorLog.Error("{0}.ReceiveThreadProcess - Buffer overflow error", this.GetType().Name);
                                index = 0;
                                break;
                            }
                        }

                        CrestronEnvironment.AllowOtherAppsToRun();
                        Thread.Sleep(0);
                    }

                    return null;
                }, null);
            }

            foreach (var b in args.SerialData.ToByteArray())
                _rxQueue.Enqueue(b);
        }

        #region IQSysCommsHandler Members

        public event IQSysCommsReceiveHandler ReceivedControlResponse;

        public event IQSysCommsStartedHandler CommsStatusChange;

        #endregion
    }
}