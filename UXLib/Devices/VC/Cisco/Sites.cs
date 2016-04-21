using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Sites : IEnumerable<Site>
    {
        public Sites(CiscoCodec codec)
        {
            Codec = codec;
            _Sites = new Dictionary<int, Site>();
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
        }

        CiscoCodec Codec { get; set; }

        Dictionary<int, Site> _Sites { get; set; }

        public event CodecConfernceSiteEventHandler SiteChange;

        public Site this[int siteID]
        {
            get { return _Sites[siteID]; }
        }

        void OnSiteChange(Site site, bool ghosted)
        {
            if (SiteChange != null)
                SiteChange(Codec, new CodecConferenceSiteEventArgs(site, ghosted));
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            try
            {
                int siteID;
                bool ghost;

                switch (args.Path)
                {
                    case @"Status/Conference":
                        if (args.Data.Elements().First().XName.LocalName == "Site")
                        {
                            siteID = int.Parse(args.Data.Elements().First().Attribute("item").Value);
                            ghost = args.Data.Elements().First().Attribute("ghost") != null ? bool.Parse(args.Data.Elements().First().Attribute("ghost").Value) : false;

                            if (ghost && _Sites.ContainsKey(siteID))
                            {
                                OnSiteChange(_Sites[siteID], true);
                                _Sites.Remove(siteID);
                            }
                        }
                        break;
                    case @"Status/Conference/Site":
                        siteID = int.Parse(args.Data.Attribute("item").Value);
                        ghost = args.Data.Attribute("ghost") != null ? bool.Parse(args.Data.Attribute("ghost").Value) : false;

                        if (ghost && _Sites.ContainsKey(siteID))
                        {
                            OnSiteChange(_Sites[siteID], true);
                            _Sites.Remove(siteID);
                        }
                        else if (!ghost)
                        {
                            if (!_Sites.ContainsKey(siteID))
                                _Sites.Add(siteID, new Site(Codec, siteID));

                            Site site = _Sites[siteID];

                            foreach (XElement e in args.Data.Elements())
                            {
                                switch (e.XName.LocalName)
                                {
                                    case "Appearance":
                                        site.Appearance = int.Parse(e.Value);
                                        break;
                                    case "Hold":
                                        site.Hold = bool.Parse(e.Value);
                                        break;
                                    case "MicrophonesMuted":
                                        site.MicrophonesMuted = bool.Parse(e.Value);
                                        break;
                                    case "AttendedTransfer":
                                        site.AttendedTransfer = bool.Parse(e.Value);
                                        break;
                                    case "Capabilities":
                                        foreach (XElement e2 in e.Elements())
                                        {
                                            switch (e2.XName.LocalName)
                                            {
                                                case "Presentation":
                                                    site.Capabilities.Presentation = bool.Parse(e2.Value);
                                                    break;
                                            }
                                        }
                                        break;
                                }

                                OnSiteChange(site, false);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception in Site.FeedbackServer_ReceivedData, {0}",
                    e.Message);
                ErrorLog.Exception("Exception in Sites.FeedbackServer_ReceivedData", e);
            }
        }

        #region IEnumerable<Site> Members

        public IEnumerator<Site> GetEnumerator()
        {
            return _Sites.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    public class CodecConferenceSiteEventArgs : EventArgs
    {
        public CodecConferenceSiteEventArgs(Site site)
            : this(site, false) { }

        public CodecConferenceSiteEventArgs(Site site, bool ghosted)
        {
            Site = site;
            Ghost = ghosted;
        }

        public Site Site;
        public bool Ghost;
    }

    public delegate void CodecConfernceSiteEventHandler(CiscoCodec codec, CodecConferenceSiteEventArgs args);
}