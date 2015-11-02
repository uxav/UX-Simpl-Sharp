using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro
{
    public class UIPages<TKey> : Dictionary<TKey, UIPage>
    {
        BoolInputSigInterlock PageVisisbleJoinSigGroup;

        public UIPages()
            : base()
        {
            this.PageVisisbleJoinSigGroup = new BoolInputSigInterlock();
        }

        public void Add(TKey key, BoolInputSig visibleJoinSig)
        {
            if (!this.ContainsKey(key))
            {
                UIPage newPage = new UIPage(visibleJoinSig, this.PageVisisbleJoinSigGroup);
                this.Add(key, newPage);
            }
            else
            {
                throw new Exception("Page with key value already exists");
            }
        }

        public void Add(TKey key, BoolInputSig visibleJoinSig, string name, StringInputSig nameStringInputSig)
        {
            if (!this.ContainsKey(key))
            {
                UIPage newPage = new UIPage(visibleJoinSig, this.PageVisisbleJoinSigGroup, nameStringInputSig, name);
                this.Add(key, newPage);
            }
            else
            {
                throw new Exception("Page with key value already exists");
            }
        }
    }
}