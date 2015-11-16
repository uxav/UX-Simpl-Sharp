using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIActionSheet : IDisposable
    {
        UISubPage SubPage;
        public UIButtonCollection Buttons;
        Action<eActionSheetButtonAction> CallBack;

        public UIActionSheet(UISubPage subPage, string title, string subTitle, Action<eActionSheetButtonAction> callBack)
        {
            this.SubPage = subPage;
            this.SubPage.Title = title;
            this.SubPage.SubTitle = subTitle;
            this.Buttons = new UIButtonCollection();
            this.Buttons.ButtonEvent += new UIButtonCollectionEventHandler(Buttons_ButtonEvent);
            this.CallBack = callBack;
        }

        void Buttons_ButtonEvent(UIButtonCollection group, UIButtonCollectionEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Released)
            {
                UIActionSheetButton responseButton = args.Button as UIActionSheetButton;
                this.SubPage.Hide();
                this.CallBack(responseButton.Action);
            }
        }

        public virtual void AddButton(UIActionSheetButton button)
        {
            this.Buttons.Add(button);
        }

        public virtual void Show()
        {
            this.SubPage.Show();
        }

        public virtual void Dispose()
        {
            foreach (UIButton button in Buttons)
            {
                button.Dispose();
            }
        }
    }
}