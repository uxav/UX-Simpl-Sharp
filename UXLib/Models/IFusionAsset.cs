using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models
{
    public interface IFusionAsset
    {
        void AssignFusionAsset(FusionAssetBase asset);
        FusionAssetBase FusionAsset { get; }
        AssetTypeName AssetTypeName { get; }
        string Name { get; }
    }

    public enum AssetTypeName
    {
        TouchPanel,
        Display,
        Source,
        DSP,
        VideoConferenceCodec
    }
}