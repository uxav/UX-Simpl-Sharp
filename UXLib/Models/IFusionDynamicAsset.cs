using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models
{
    public interface IFusionDynamicAsset : IFusionAsset
    {
        FusionDynamicAsset FusionAsset { get; }
    }
}