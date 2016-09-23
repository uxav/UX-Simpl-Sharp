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
        void AssignFusionAsset(Fusion fusionInstance, FusionStaticAsset asset);
        FusionStaticAsset FusionAsset { get; }
        AssetTypeName AssetTypeName { get; }
        string Name { get; }
        void FusionUpdate();
        void FusionError(string errorDetails);
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