using System;

namespace UXLib.UI
{
    public interface IUIVisibleObject
    {
        void Hide();
        void Show();
        bool Visible { get; set; }
    }
}
