using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UIPage : UIViewBase
    {
        private readonly UIController _uiController;

        public UIPage(UIController uiController, uint pageNumber)
            : base(uiController.Device.BooleanInput[pageNumber])
        {
            _uiController = uiController;
            _uiController.Pages.Add(this);
        }

        public UIPage(UIController uiController, uint pageNumber, UILabel titleLabel, string title)
            : base(uiController.Device.BooleanInput[pageNumber], titleLabel)
        {
            _uiController = uiController;
            _uiController.Pages.Add(this);
            Title = title;
        }

        public IEnumerable<UIPage> OtherPages
        {
            get { return _uiController.Pages.Where(p => p != this); }
        }
    }
}