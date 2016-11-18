using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UXLib.Devices.Displays;
using UXLib.Models;

namespace UXLib.Devices.Displays.Sony
{
    public class SonyBravia : DisplayDevice, IVolumeDevice
    {
        public SonyBravia(string ipAddress, string preSharedKey)
        {
            this.IPAddress = ipAddress;
            this.PreSharedKey = preSharedKey;
            random = new Random();
        }

        public string IPAddress { get; protected set; }
        public string PreSharedKey { get; protected set; }
        private HttpClient client { get; set; }
        private Random random { get; set; }

        public override UXLib.Devices.CommDeviceType CommunicationType
        {
            get { return UXLib.Devices.CommDeviceType.IP; }
        }

        private int GetNextID()
        {
            return random.Next(1, int.MaxValue);
        }

        JObject Request(string path, string method, int id, string version, params object[] args)
        {
            return Request(this.client, path, method, id, version, args);
        }

        JObject Request(HttpClient client, string path, string method, int id, string version, params object[] args)
        {
            string jsonData = JsonConvert.SerializeObject(new
            {
                @method = method,
                @params = args,
                @id = id,
                @version = version,
            }, Formatting.Indented);

            string endpointUrl = string.Format("http://{0}/sony/{1}", this.IPAddress, path);
#if DEBUG
            CrestronConsole.PrintLine("New request to {0}   Method = {1}", endpointUrl, method);
            CrestronConsole.PrintLine(jsonData);
#endif
            HttpClientRequest request = new HttpClientRequest();
            request.Url = new UrlParser(endpointUrl);
            request.RequestType = RequestType.Post;
            request.Header.AddHeader(new HttpHeader("X-Auth-PSK", this.PreSharedKey));
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
                ErrorLog.Error("Error in {0}.Request(), {1}", this.GetType().Name, e.Message);
            }

            return null;
        }

        private string _DeviceSerialNumber;
        public override string DeviceSerialNumber
        {
            get { return _DeviceSerialNumber; }
        }

        private string _DeviceModel;
        public override string DeviceModel
        {
            get
            {
                return _DeviceModel;
            }
        }

        private string macAddress;

        public override void Initialize()
        {
            this.client = new HttpClient();
            this.client.KeepAlive = true;
            this.client.UseConnectionPooling = true;

            JObject systemInfo = Request("system", "getSystemInformation", GetNextID(), "1.0");

            _DeviceModel = systemInfo["result"].First()["model"].Value<string>();
            macAddress = systemInfo["result"].First()["macAddr"].Value<string>();
            _DeviceSerialNumber = systemInfo["result"].First()["serial"].Value<string>();

            JObject powerStatus = Request("system", "getPowerStatus", GetNextID(), "1.0");
 
            bool power = (powerStatus["result"].First()["status"].Value<string>() == "active");

            if (power) this.PowerStatus = UXLib.Devices.DevicePowerStatus.PowerOn;

            JObject wolMode = Request("system", "getWolMode", GetNextID(), "1.0");
 
            bool wakeOnLanEnabled = wolMode["result"].First()["enabled"].Value<bool>();

            if (!wakeOnLanEnabled)
            {
                Request("system", "setWolMode", GetNextID(), "1.0", new { @enabled = true });
            }
        }

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

                if (Request("system", "getPowerStatus", GetNextID(), "1.0")["result"].First()["status"].Value<string>() == "active")
                {
                    PowerStatus = UXLib.Devices.DevicePowerStatus.PowerOn;
                }
                else
                {
                    PowerStatus = UXLib.Devices.DevicePowerStatus.PowerOff;
                }

                if (this.Power)
                {
                    Request("videoScreen", "setBannerMode", GetNextID(), "1.0", new { @value = "hidden" });

                    JObject volumeStatus = Request("audio", "getVolumeInformation", GetNextID(), "1.0");

                    foreach (JToken token in volumeStatus["result"].First())
                    {
                        if (token["target"].Value<string>() == "speaker")
                        {
                            _VolumeMin = token["minVolume"].Value<int>();
                            _VolumeMax = token["maxVolume"].Value<int>();
                            int v = token["volume"].Value<int>();
                            if (v != _Volume)
                            {
                                this.Volume = _Volume;
                            }
                            bool m = token["mute"].Value<bool>();
                            if (m != _Mute)
                            {
                                this.Mute = _Mute;
                            }
                        }
                    }
                }
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
                }
                if (uriString.Length > 0)
                {
                    if (!this.Power)
                        this.Power = true;
                    Request("avContent", "setPlayContent", GetNextID(), "1.0", new { @uri = uriString });
                    base.Input = value;
                }
            }
        }

        public override UXLib.Devices.DevicePowerStatus PowerStatus
        {
            get
            {
                return base.PowerStatus;
            }
            protected set
            {
                base.PowerStatus = value;
            }
        }

        private int _Volume;
        private int _VolumeMin = 0;
        private int _VolumeMax = 100;

        public int Volume
        {
            get { return _Volume; }
            set
            {
                if (this.Power)
                {
                    if (value >= _VolumeMin && value <= _VolumeMax)
                    {
                        Request("audio", "setAudioVolume", GetNextID(), "1.0", new { @volume = value.ToString(), @target = "speaker" });
                        _Volume = value;
                        if (VolumeChanged != null)
                            VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                    }
                    else
                    {
                        throw new IndexOutOfRangeException(string.Format("Value {0} not between {1} and {2}", value, _VolumeMin, _VolumeMax));
                    }
                }
            }
        }

        private bool _Mute;

        public bool Mute
        {
            get { return _Mute; }
            set
            {
                if (this.Power)
                {
                    Request("audio", "setAudioMute", GetNextID(), "1.0", new { @status = value });
                    _Mute = value;
                    if (VolumeChanged != null)
                        VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                }
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
                return (ushort)UXLib.Tools.ScaleRange(this.Volume, _VolumeMin, 40, ushort.MinValue, ushort.MaxValue);
            }
            set
            {
                int newVal = (int)UXLib.Tools.ScaleRange(value, ushort.MinValue, ushort.MaxValue, _VolumeMin, 40);
                if (newVal != this.Volume)
                    this.Volume = newVal;
            }
        }

        public bool VolumeMute
        {
            get
            {
                return this.Mute;
            }
            set
            {
                this.Mute = value;
            }
        }

        #endregion
    }
}