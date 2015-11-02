using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro
{
    public class UIPage
    {
        public string Name;
        BoolInputSig Join;
        BoolInputSigInterlock JoinGroup;

        public UIPage(string name, BoolInputSig pageJoinSig, BoolInputSigInterlock pageJoinSigGroup)
        {
            this.Name = name;
            this.Join = pageJoinSig;
            this.JoinGroup = pageJoinSigGroup;
            pageJoinSigGroup.Add(this.Join);
        }

        public void Show()
        {
            this.JoinGroup.Set(this.Join);
        }
    }
}