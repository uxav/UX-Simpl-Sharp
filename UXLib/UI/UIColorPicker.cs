using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIColorPicker
    {
        public UIColorPicker(BasicTriList device, uint redJoin, uint greenJoin, uint blueJoin)
        {
            Owner = device;
            _RedJoin = redJoin;
            _GreenJoin = greenJoin;
            _BlueJoin = blueJoin;
        }

        public UIColorPicker(BasicTriList device, uint redJoin, uint greenJoin, uint blueJoin, UIColor defaultColor)
            : this(device, redJoin, greenJoin, blueJoin)
        {
            this.Color = defaultColor;
        }

        public UIColorPicker(SmartObject smartObject, string sigNameFormat, uint redJoin, uint greenJoin, uint blueJoin)
        {
            Owner = smartObject;
            _SigNameFormat = sigNameFormat;
            _RedJoin = redJoin;
            _GreenJoin = greenJoin;
            _BlueJoin = blueJoin;
        }

        public UIColorPicker(SmartObject smartObject, string sigNameFormat, uint redJoin, uint greenJoin, uint blueJoin, UIColor defaultColor)
            : this(smartObject, sigNameFormat, redJoin, greenJoin, blueJoin)
        {
            this.Color = defaultColor;
        }

        public object Owner { get; private set; }

        uint _RedJoin;
        uint _GreenJoin;
        uint _BlueJoin;
        string _SigNameFormat;

        public UIColor Color
        {
            get
            {
                if(this.Owner is SmartObject)
                    return new UIColor(
                        ((SmartObject)this.Owner).UShortOutput[string.Format(_SigNameFormat, _RedJoin)].UShortValue,
                        ((SmartObject)this.Owner).UShortOutput[string.Format(_SigNameFormat, _GreenJoin)].UShortValue,
                        ((SmartObject)this.Owner).UShortOutput[string.Format(_SigNameFormat, _BlueJoin)].UShortValue);
                else
                    return new UIColor(
                        ((BasicTriList)this.Owner).UShortOutput[_RedJoin].UShortValue,
                        ((BasicTriList)this.Owner).UShortOutput[_GreenJoin].UShortValue,
                        ((BasicTriList)this.Owner).UShortOutput[_BlueJoin].UShortValue);
            }
            set
            {
                if (this.Owner is SmartObject)
                {
                    ((SmartObject)this.Owner).UShortInput[string.Format(_SigNameFormat, _RedJoin)].UShortValue = (ushort)value.Red;
                    ((SmartObject)this.Owner).UShortInput[string.Format(_SigNameFormat, _GreenJoin)].UShortValue = (ushort)value.Green;
                    ((SmartObject)this.Owner).UShortInput[string.Format(_SigNameFormat, _BlueJoin)].UShortValue = (ushort)value.Blue;
                }
                else
                {
                    ((BasicTriList)this.Owner).UShortInput[_RedJoin].UShortValue = (ushort)value.Red;
                    ((BasicTriList)this.Owner).UShortInput[_GreenJoin].UShortValue = (ushort)value.Green;
                    ((BasicTriList)this.Owner).UShortInput[_BlueJoin].UShortValue = (ushort)value.Blue;
                }
            }
        }
    }
}