using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UIActionSheet
    {
        UISubPageModal SubPage;
        UILabel SubTitleLabel;
        public UIButtonCollection Buttons;
        string Title;
        string SubTitle;
        Action<eActionSheetButtonAction> CallBack;

        public UIActionSheet(UISubPageModal subPage, UILabel subTitleLabel, string title, string subTitle, Action<eActionSheetButtonAction> callBack)
        {
            this.SubPage = subPage;
            this.SubTitleLabel = subTitleLabel;
            this.Buttons = new UIButtonCollection();
            this.Buttons.ButtonEvent += new UIButtonCollectionEventHandler(Buttons_ButtonEvent);
            this.Title = title;
            this.SubTitle = subTitle;
            this.CallBack = callBack;
        }

        void Buttons_ButtonEvent(UIButtonCollection group, UIButton button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Released)
            {
                UIActionSheetButton responseButton = button as UIActionSheetButton;
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
            this.SubPage.Name = this.Title;
            this.SubTitleLabel.Text = this.SubTitle;
            this.SubPage.Show();
        }
    }
}