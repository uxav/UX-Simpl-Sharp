using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharp.CrestronIO;

namespace UXLib.Models.Fusion
{
    interface IXmlSerializable
    {
        void Serialize(XmlWriter xml);
        void Deserialize (XElement xml);
    }

    internal class XmlSerialization
    {
        public static string Serialize<T>(T obj) where T : IXmlSerializable
        {

            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            var xwriter = XmlWriter.Create(new StringWriter(sb), settings);

            obj.Serialize(xwriter);

            xwriter.Flush();
            return sb.ToString();
        }
        
        public static void DeSerialize(IXmlSerializable theClass, string passed_xml)
        {
           // remove xml special characters
           string Regex = @"\s*&\s+";
           var xml = System.Text.RegularExpressions.Regex.Replace((passed_xml).Trim(), Regex, "");
            try
            {
                var xdoc = XDocument.Parse(xml);
                var root = xdoc.Element(XName.Get(theClass.GetType().Name));
                if (root == null) return;

                theClass.Deserialize(root);
            }
            catch (Exception ex)
            {
                ErrorLog.Error("Unable to Deserialize " + theClass.GetType().Name, ex);
            }
        }
    }
}
