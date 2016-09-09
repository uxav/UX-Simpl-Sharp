using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Extensions;

namespace UXLib.Models
{
    public class FusionAssetCollection : IEnumerable<FusionAssetBase>
    {
        public FusionAssetCollection(Fusion fusionInstance)
        {
            Fusion = fusionInstance;
            Assets = new Dictionary<uint, FusionAssetBase>();
        }

        public Fusion Fusion { get; private set; }
        private Dictionary<uint, FusionAssetBase> Assets;

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
                this.Fusion.Room.Fusion.FusionRoom.AddAsset(eAssetType.StaticAsset, newId, asset.Name,
                    asset.AssetTypeName.ToString().SplitCamelCase(), Guid.NewGuid().ToString());
                Assets[newId] = this.Fusion.Room.Fusion.FusionRoom.UserConfigurableAssetDetails[newId].Asset;
                asset.AssignFusionAsset(Assets[newId]);

                if (asset is IFusionDeviceAsset)
                {
                    ((FusionStaticAsset)Assets[newId]).ParamMake.Value = ((IFusionDeviceAsset)asset).DeviceManufacturer;
                    ((FusionStaticAsset)Assets[newId]).ParamModel.Value = ((IFusionDeviceAsset)asset).DeviceModel;
                }

                return Assets[newId];
            }
            return null;
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