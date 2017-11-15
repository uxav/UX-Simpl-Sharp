using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIPageCollection : IEnumerable<UIPage>
    {
        private static readonly Dictionary<BasicTriList, Dictionary<uint, UIPage>> Pages =
            new Dictionary<BasicTriList, Dictionary<uint, UIPage>>();

        private readonly UIController _uiController;

        internal UIPageCollection(UIController uiController)
        {
            _uiController = uiController;
            Pages[_uiController.Device] = new Dictionary<uint, UIPage>();
        }

        public UIPage this[uint visibleJoinNumber]
        {
            get { return Pages[_uiController.Device][visibleJoinNumber]; }
        }

        public UIPage CurrentPage
        {
            get { return Pages[_uiController.Device].Values.FirstOrDefault(p => p.Visible); }
        }

        internal void Add(UIPage newPage)
        {
            if (Pages[_uiController.Device].ContainsKey(newPage.VisibleJoinNumber))
                throw new Exception(string.Format("UIPage with number {0} already exists for this device",
                    newPage.VisibleJoinNumber));
            Pages[_uiController.Device][newPage.VisibleDigitalJoin.Number] = newPage;
            newPage.VisibilityChange += Page_VisibilityChange;
        }

        void Page_VisibilityChange(UIViewBase sender, UIViewVisibilityEventArgs args)
        {
            if (args.EventType == eViewEventType.WillShow)
            {
                var page = sender as UIPage;
                if (page != null)
                {
                    foreach (var otherPage in page.OtherPages)
                    {
                        otherPage.Visible = false;
                    }
                }
            }
            if (VisibilityChange != null)
                VisibilityChange(sender, args);
        }

        public event UIViewBaseVisibitlityEventHandler VisibilityChange;

        public IEnumerator<UIPage> GetEnumerator()
        {
            return Pages[_uiController.Device].Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}