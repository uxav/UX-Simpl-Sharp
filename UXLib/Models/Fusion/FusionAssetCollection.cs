using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Extensions;

namespace UXLib.Models.Fusion
{
    public class FusionAssetCollection : IEnumerable<FusionAssetBase>
    {
        public FusionAssetCollection(FusionController fusionInstance)
        {
            Fusion = fusionInstance;
            Assets = new Dictionary<uint, FusionAssetBase>();
            Devices = new Dictionary<GenericDevice, FusionAssetBase>();
        }

        public FusionController Fusion { get; private set; }
        private Dictionary<uint, FusionAssetBase> Assets;
        private Dictionary<GenericDevice, FusionAssetBase> Devices;

        public FusionAssetBase this[uint id]
        {
            get { return this.Assets[id]; }
        }

        public FusionAssetBase Add(IFusionAsset asset)
        {
            uint newId = 0;
            for (uint id = 1; id <= 249; id++)
            {
                if (!this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails.Contains(id))
                {
                    newId = id;
                    break;
                }
            }

            if (newId > 0)
            {
                if (asset is IFusionStaticAsset)
                    this.Fusion.Room.Fusion.FusionRoom.AddAsset(eAssetType.StaticAsset, newId, asset.Name,
                        asset.AssetTypeName.ToString().SplitCamelCase(), Guid.NewGuid().ToString());
                else if (asset is IFusionDynamicAsset)
                    this.Fusion.Room.Fusion.FusionRoom.AddAsset(eAssetType.DynamicAsset, newId, asset.Name,
                        asset.AssetTypeName.ToString().SplitCamelCase(), Guid.NewGuid().ToString());

                Assets[newId] = this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails[newId].Asset;

                asset.AssignFusionAsset(this.Fusion, Assets[newId]);

                if (!(asset is Devices.IDevice) || !(asset is IFusionStaticAsset)) return Assets[newId];
                ((FusionStaticAsset)Assets[newId]).ParamMake.Value = ((Devices.IDevice)asset).DeviceManufacturer;
                ((FusionStaticAsset)Assets[newId]).ParamModel.Value = ((Devices.IDevice)asset).DeviceModel;

                return Assets[newId];
            }
            return null;
        }

        public FusionAssetBase Add(GenericDevice device)
        {
            uint newId = 0;
            for (uint id = 1; id <= 249; id++)
            {
                if (!this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails.Contains(id))
                {
                    newId = id;
                    break;
                }
            }

            if (newId > 0)
            {
                this.Fusion.Room.Fusion.FusionRoom.AddAsset(eAssetType.StaticAsset, newId, device.GetType().Name.SplitCamelCase(),
                        device.Type.ToString().SplitCamelCase(), Guid.NewGuid().ToString());

                Assets[newId] = this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails[newId].Asset;

                Devices[device] = Assets[newId];
                ((FusionStaticAsset)Devices[device]).AddSig(eSigType.String, 1, "IP Address", eSigIoMask.InputSigOnly);
                device.OnlineStatusChange += new OnlineStatusChangeEventHandler(device_OnlineStatusChange);
                device.IpInformationChange += new IpInformationChangeEventHandler(device_IpInformationChange);

                return Assets[newId];
            }
            return null;
        }

        public FusionAssetBase Add(CrestronControlSystem controlSystem)
        {
            uint newId = 0;
            for (uint id = 1; id <= 249; id++)
            {
                if (!this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails.Contains(id))
                {
                    newId = id;
                    break;
                }
            }

            if (newId > 0)
            {
                this.Fusion.Room.Fusion.FusionRoom.AddAsset(eAssetType.StaticAsset, newId, controlSystem.ControllerPrompt,
                        "Control Processor", Guid.NewGuid().ToString());

                Assets[newId] = this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails[newId].Asset;

                ((FusionStaticAsset)Assets[newId]).AddSig(eSigType.String, 1, "IP Address", eSigIoMask.InputSigOnly);
                ((FusionStaticAsset)Assets[newId]).FusionGenericAssetSerialsAsset3.StringInput[1].StringValue
                    = CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter));

                return Assets[newId];
            }
            return null;
        }

        void device_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (currentDevice is GenericDevice && Devices[currentDevice as GenericDevice] is FusionStaticAsset)
            {
                ((FusionStaticAsset)Devices[currentDevice as GenericDevice]).Connected.InputSig.BoolValue = args.DeviceOnLine;
                ((FusionStaticAsset)Devices[currentDevice as GenericDevice]).PowerOn.InputSig.BoolValue = args.DeviceOnLine;
            }
        }

        void device_IpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            if (currentDevice is GenericDevice && Devices[currentDevice as GenericDevice] is FusionStaticAsset)
            {
                if (args.Connected)
                {
                    ((FusionStaticAsset)Devices[currentDevice as GenericDevice]).FusionGenericAssetSerialsAsset3.StringInput[1].StringValue
                        = currentDevice.ConnectedIpList.First().DeviceIpAddress;
                }
            }
        }

        #region IEnumerable<FusionAssetBase> Members

        public IEnumerator<FusionAssetBase> GetEnumerator()
        {
            return this.Assets.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}