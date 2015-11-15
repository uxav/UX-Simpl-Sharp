using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISubPageModalCollection : IEnumerable<UISubPageModal>
    {
        private List<UISubPageModal> SubPages;
        BoolInputSigInterlock JoinGroup;

        public UISubPageModal this[uint joinNumber]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UISubPageModal this[UIKey key]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.Key == key);
            }
        }

        public UISubPageModalCollection()
        {
            this.SubPages = new List<UISubPageModal>();
            this.JoinGroup = new BoolInputSigInterlock();
        }

        public void Add(UIKey key, BoolInputSig visibleJoinSig, UILabel titleLabel, string name, UITimeOut timeOut)
        {
            if (!this.JoinGroup.Contains(visibleJoinSig))
            {
                this.SubPages.Add(new UISubPageModal(key, visibleJoinSig, this.JoinGroup, titleLabel, name, timeOut));
            }
            else
            {
                throw new Exception("SubPage with visible join value already exists");
            }
        }

        public void Close()
        {
            foreach (UISubPageModal page in this.SubPages)
            {
                page.Hide();
            }
        }

        public IEnumerator<UISubPageModal> GetEnumerator()
        {
            return this.SubPages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private UIButton _CloseButton;
        public UIButton CloseButton
        {
            set
            {
                this._CloseButton = value;
                this._CloseButton.ButtonEvent += new UIButtonEventHandler(_CloseButton_ButtonEvent);
            }
        }

        void _CloseButton_ButtonEvent(UIButtonBase button, UIButtonEventArgs args)
        {
            if (args.EventType == eUIButtonEventType.Released)
            {
                this.Close();
            }
        }
    }
}