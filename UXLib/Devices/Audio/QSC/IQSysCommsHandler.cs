namespace UXLib.Devices.Audio.QSC
{
    public interface IQSysCommsHandler
    {
        void Initialize();
        void Send(string str);
        event IQSysCommsReceiveHandler ReceivedControlResponse;
        event IQSysCommsStartedHandler CommsStatusChange;
    }

    public delegate void IQSysCommsReceiveHandler(IQSysCommsHandler handler, byte[] receivedData);

    public delegate void IQSysCommsStartedHandler(IQSysCommsHandler handler, bool connected);
}