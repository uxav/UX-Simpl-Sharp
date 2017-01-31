using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models.Fusion
{
    public interface IFusionDynamicAsset : IFusionAsset
    {
        FusionDynamicAsset FusionAsset { get; }
    }
}