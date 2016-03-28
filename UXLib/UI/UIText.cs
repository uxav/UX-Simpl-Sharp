using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.UI
{
    public class UIText
    {
        public UIText()
        {
            xml = new XDocument(new XElement("FONT", ""));
        }

        public UIText(string fromText)
        {
            try
            {
                xml = XDocument.Load(new XmlReader(fromText));
            }
            catch
            {
                xml = new XDocument(new XElement("FONT", fromText));
            }
        }

        public UIText(UIText item)
        {
            xml = XDocument.Load(new XmlReader(item.ToString()));
        }

        XDocument xml;

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            XElement element = xml.Element("FONT");
            element.WriteTo(new XmlTextWriter(sw));
            return sw.ToString();
        }

        public string Text
        {
            get
            {
                XElement element = xml.Element("FONT");
                return element.Value;
            }
            set
            {
                XElement element = xml.Element("FONT");
                element.Value = value;
            }
        }

        public UIText Color(UIColor color)
        {
            XElement element = xml.Element("FONT");
            element.SetAttributeValue("color", color.ToHex());
            return this;
        }

        public UIText Color(uint red, uint green, uint blue)
        {
            return Color(new UIColor(red, green, blue));
        }

        public UIText Color(string hexValue)
        {
            return Color(new UIColor(hexValue));
        }

        public UIText Color()
        {
            return Color(new UIColor());
        }

        public UIText Face(string faceName)
        {
            XElement element = xml.Element("FONT");
            element.SetAttributeValue("face", faceName);
            return this;
        }

        public UIText Size(uint size)
        {
            XElement element = xml.Element("FONT");
            element.SetAttributeValue("size", size);
            return this;
        }

        public UIText PadBy(uint numberOfSpaces)
        {
            XElement element = xml.Element("FONT");
            string temp = "";
            for(uint c = 0; c < numberOfSpaces; c ++)
                temp = temp + " ";
            element.Value = temp + element.Value;
            return this;
        }
    }
}