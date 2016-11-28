using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIDynamicIcon : UIObject
    {
        public UIDynamicIcon(StringInputSig iconSerialJoin)
        {
            this.IconSerialJoin = iconSerialJoin;
        }

        public UIDynamicIcon(StringInputSig iconSerialJoin, string defaultIcon)
            : this(iconSerialJoin)
        {
            this.Icon = defaultIcon;
        }

        public UIDynamicIcon(StringInputSig iconSerialJoin, BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin)
            : this(iconSerialJoin)
        {
            this.EnableDigitalJoin = enableDigitalJoin;
            this.VisibleDigitalJoin = visibleDigitalJoin;
        }

        public UIDynamicIcon(StringInputSig iconSerialJoin, string defaultIcon, BoolInputSig enableDigitalJoin, BoolInputSig visibleDigitalJoin)
            : this(iconSerialJoin, enableDigitalJoin, visibleDigitalJoin)
        {
            this.Icon = defaultIcon;
        }
    }
}