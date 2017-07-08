using System;
using System.Globalization;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UXLib.Models;

namespace UXLib.Devices.Displays.Sony
{
    public class SonyBravia : DisplayDevice, IVolumeDevice
    {
        public SonyBravia(string ipAddress, string preSharedKey)
        {
            IpAddress = ipAddress;
            PreSharedKey = preSharedKey;
            _random = new Random();
        }

        public string IpAddress { get; protected set; }
        public string PreSharedKey { get; protected set; }
        private HttpClient _client;
        private static Random _random;

        public override CommDeviceType CommunicationType
        {
            get { return CommDeviceType.IP; }
        }

        private int GetNextID()
        {
            return _random.Next(1, int.MaxValue);
        }

        JObject Request(string path, string method, int id, string version, params object[] args)
        {
            return Request(_client, path, method, id, version, args);
        }

        JObject Request(HttpClient client, string path, string method, int id, string version, params object[] args)
        {
            var jsonData = JsonConvert.SerializeObject(new
            {
                method,
                @params = args, id, version,
            }, Formatting.Indented);

            var endpointUrl = string.Format("http://{0}/sony/{1}", IpAddress, path);
#if DEBUG
            CrestronConsole.PrintLine("New request to {0}   Method = {1}", endpointUrl, method);
            CrestronConsole.PrintLine(jsonData);
#endif
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(endpointUrl);
            request.RequestType = RequestType.Post;
            request.Header.AddHeader(new HttpHeader("X-Auth-PSK", PreSharedKey));
            request.Header.AddHeader(new HttpHeader("Content-Type", "application/json"));
            request.ContentString = jsonData;
            request.KeepAlive = true;
#if DEBUG
            CrestronConsole.Print("Dispatching... ");
#endif
            try
            {
                HttpClientResponse response = client.Dispatch(request);
#if DEBUG
                CrestronConsole.PrintLine("Done - Response {0}", response.Code);
                CrestronConsole.PrintLine(response.ContentString);
#endif
                return JObject.Parse(response.ContentString);
            }
            catch (Exception e)
            {
#if DEBUG
                CrestronConsole.PrintLine("Error: {0}", e.Message);
#endif
                ErrorLog.Error("Error in {0}.Request(), {1}", GetType().Name, e.Message);
            }

            return null;
        }

        private string _deviceSerialNumber;
        public override string DeviceSerialNumber
        {
            get { return _deviceSerialNumber; }
        }

        private string _deviceModel;
        public override string DeviceModel
        {
            get
            {
                return _deviceModel;
            }
        }

        private string _macAddress;
        public string MACAddress
        {
            get
            {
                return _macAddress;
            }
        }

        private string _version;
        public string Version
        {
            get
            {
                return _version;
            }
        }

        public override void Initialize()
        {
            try
            {
                _client = new HttpClient();
                _client.KeepAlive = true;
                _client.UseConnectionPooling = true;

                var systemInfo = Request("system", "getSystemInformation", GetNextID(), "1.0");

                _deviceModel = systemInfo["result"].First()["model"].Value<string>();
                _macAddress = systemInfo["result"].First()["macAddr"].Value<string>();
                _deviceSerialNumber = systemInfo["result"].First()["serial"].Value<string>();
                _version = systemInfo["result"].First()["generation"].Value<string>();

                var powerStatus = Request("system", "getPowerStatus", GetNextID(), "1.0");

                var power = (powerStatus["result"].First()["status"].Value<string>() == "active");

                if (power) PowerStatus = DevicePowerStatus.PowerOn;

                var wolMode = Request("system", "getWolMode", GetNextID(), "1.0");

                var wakeOnLanEnabled = wolMode["result"].First()["enabled"].Value<bool>();

                if (!wakeOnLanEnabled)
                {
                    Request("system", "setWolMode", GetNextID(), "1.0", new { @enabled = true });
                }

                if (HasInitialized != null)
                    HasInitialized(this);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in {0}.Initialize(), {1}", GetType().Name, e.Message);
            }
        }

        public event SonyBraviaDeviceHasInitialized HasInitialized;

        public override bool Power
        {
            get
            {
                return base.Power;
            }
            set
            {
                base.Power = value;
                Request("system", "setPowerStatus", GetNextID(), "1.0", new { @status = value });

                PowerStatus =
                    Request("system", "getPowerStatus", GetNextID(), "1.0")["result"]
                        .First()["status"].Value<string>() == "active"
                        ? DevicePowerStatus.PowerOn
                        : DevicePowerStatus.PowerOff;

                if (!Power) return;
                Request("videoScreen", "setBannerMode", GetNextID(), "1.0", new { @value = "hidden" });

                new CTimer(specific =>
                {
                    JObject volumeStatus = Request("audio", "getVolumeInformation", GetNextID(), "1.0");

                    foreach (JToken token in volumeStatus["result"].First())
                    {
                        if (token["target"].Value<string>() != TargetAudioDevice.ToString().ToLower()) continue;
                        _volumeMin = token["minVolume"].Value<int>();
                        _volumeMax = token["maxVolume"].Value<int>();
                        var v = token["volume"].Value<int>();
                        if (v != _volume)
                        {
                            _volume = v;
                            if (VolumeChanged != null)
                                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                        }
                        var m = token["mute"].Value<bool>();
                        if (m == _mute) continue;
                        _mute = m;
                        if (VolumeChanged != null)
                            VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                    }
                }, 1000);
            }
        }

        public override DisplayDeviceInput Input
        {
            get
            {
                return base.Input;
            }
            set
            {
                string uriString = string.Empty;
                switch (value)
                {
                    case DisplayDeviceInput.HDMI1:
                        uriString = string.Format("extInput:hdmi?port={0}", 1); break;
                    case DisplayDeviceInput.HDMI2:
                        uriString = string.Format("extInput:hdmi?port={0}", 2); break;
                    case DisplayDeviceInput.HDMI3:
                        uriString = string.Format("extInput:hdmi?port={0}", 3); break;
                }
                if (uriString.Length <= 0) return;
                if (!Power)
                    Power = true;
                Request("avContent", "setPlayContent", GetNextID(), "1.0", new { @uri = uriString });
                base.Input = value;
            }
        }

        private int _volume;
        private int _volumeMin;
        private int _volumeMax = 100;

        public TargetVolumeDeviceType TargetAudioDevice { get; set; }

        public int Volume
        {
            get { return _volume; }
            set
            {
                if (!Power) return;
                if (value >= _volumeMin && value <= _volumeMax)
                {
                    Request("audio", "setAudioVolume", GetNextID(), "1.0",
                        new
                        {
                            @volume = value.ToString(CultureInfo.InvariantCulture),
                            @target = TargetAudioDevice.ToString().ToLower()
                        });
                    _volume = value;
                    if (VolumeChanged != null)
                        VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format("Value {0} not between {1} and {2}", value,
                        _volumeMin, _volumeMax));
                }
            }
        }

        private bool _mute;

        public bool Mute
        {
            get { return _mute; }
            set
            {
                if (!Power) return;
                Request("audio", "setAudioMute", GetNextID(), "1.0", new { @status = value });
                _mute = value;
                if (VolumeChanged != null)
                    VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
            }
        }

        #region IVolumeDevice Members

        public bool SupportsVolumeLevel
        {
            get { return true; }
        }

        public bool SupportsVolumeMute
        {
            get { return false; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        public ushort VolumeLevel
        {
            get
            {
                return (ushort)Tools.ScaleRange(Volume, _volumeMin, _volumeMax, ushort.MinValue, ushort.MaxValue);
            }
            set
            {
                var newVal = (int)Tools.ScaleRange(value, ushort.MinValue, ushort.MaxValue, _volumeMin, _volumeMax);
                if (!newVal.Equals(Volume))
                    Volume = newVal;
            }
        }

        public bool VolumeMute
        {
            get
            {
                return Mute;
            }
            set
            {
                Mute = value;
            }
        }

        #endregion
    }

    public delegate void SonyBraviaDeviceHasInitialized(SonyBravia device);

    public enum TargetVolumeDeviceType
    {
        Speaker,
        Headphone
    }
}