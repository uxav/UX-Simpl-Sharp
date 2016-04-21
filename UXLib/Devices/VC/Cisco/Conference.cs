using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Conference
    {
        public Conference(CiscoCodec codec)
        {
            Codec = codec;
            Sites = new Sites(Codec);
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            _Presentations = new Dictionary<int, PresentationInstance>();
        }

        CiscoCodec Codec { get; set; }
        public Sites Sites { get; protected set; }

        Dictionary<int, PresentationInstance> _Presentations { get; set; }

        public ReadOnlyDictionary<int, PresentationInstance> Presentations
        {
            get { return new ReadOnlyDictionary<int, PresentationInstance>(_Presentations); }
        }

        public event CodecPresentationEventHandler PresentationChange;

        void OnPresentationChange(PresentationInstance presentation)
        {
            if (PresentationChange != null)
                PresentationChange(Codec, new CodecPresentationEventArgs(presentation));
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            int presentationID;
            bool ghost;

            switch (args.Path)
            {
                case @"Status/Conference/Presentation":
                    if (args.Data.Elements().First().XName.LocalName == "Instance")
                    {
                        presentationID = int.Parse(args.Data.Elements().First().Attribute("item").Value);
                        ghost = args.Data.Elements().First().Attribute("ghost") != null ? bool.Parse(args.Data.Elements().First().Attribute("ghost").Value) : false;

                        if (ghost && _Presentations.ContainsKey(presentationID))
                        {
                            _Presentations.Remove(presentationID);
                        }
                    }
                    else
                    {
                        presentationID = int.Parse(args.Data.Attribute("item").Value);
                        ghost = args.Data.Attribute("ghost") != null ? bool.Parse(args.Data.Attribute("ghost").Value) : false;

                        if (ghost && _Presentations.ContainsKey(presentationID))
                        {
                            _Presentations.Remove(presentationID);
                        }
                        else if (!ghost)
                        {
                            if (!_Presentations.ContainsKey(presentationID))
                                _Presentations.Add(presentationID, new PresentationInstance(Codec, presentationID));
                            PresentationInstance presentation = _Presentations[presentationID];

                            foreach (XElement e in args.Data.Elements())
                            {
                                switch (e.XName.LocalName)
                                {
                                    case "LocalSendingMode":
                                        presentation.LocalSendingMode = (PresentationSendingMode)Enum
                                            .Parse(typeof(PresentationSendingMode), e.Value, false);
                                        break;
                                    case "LocalSource": presentation.LocalSource = int.Parse(e.Value);
                                        break;
                                    case "Mode": presentation.Mode = e.Value;
                                        break;
                                }
                            }

                            OnPresentationChange(presentation);
                        }
                    }
                    break;
                case @"Status/Conference/Presentation/Instance":
                    break;
            }
        }
    }

    public class CodecPresentationEventArgs : EventArgs
    {
        public CodecPresentationEventArgs(PresentationInstance presentation)
        {
            Presentation = presentation;
        }

        public PresentationInstance Presentation;
    }

    public delegate void CodecPresentationEventHandler(CiscoCodec codec, CodecPresentationEventArgs args);
}