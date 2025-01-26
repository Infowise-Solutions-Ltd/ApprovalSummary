using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;

namespace Infowise.Sharepoint.V3.Fields.Controls
{
    class IconProvider
    {
        private static IconProvider instance = null;
        private XmlDocument doc;
        private IconProvider()
        {
            doc = new XmlDocument();
            string path = HttpContext.Current.Server.MapPath("~/_layouts/"); 
            path = Path.Combine(path, @"..\xml\docicon.xml");
            doc.Load(path);
        }

        public static string GetIcon(string progID, string ext, out string editText, out string openControl)
        {
            if (instance == null)
                instance = new IconProvider();

            Mapping result = instance.GetByProgId(progID);
            if (result == null)
                result = instance.GetByExtension(ext);
            if (result == null)
                result = instance.GetDefaultMapping();

            editText = result.EditText;
            openControl = result.OpenControl;
            return result.IconName;
        }

        /// <summary>
        /// Gets default mapping
        /// </summary>
        /// <returns></returns>
        private Mapping GetDefaultMapping()
        {
            var node = doc.DocumentElement.SelectSingleNode("Default").FirstChild as XmlElement;
            return new Mapping(node.GetAttribute("Value"), string.Empty, string.Empty);
        }

        private Mapping GetByExtension(string ext)
        {
            var parent = doc.DocumentElement.SelectSingleNode("ByExtension");
            if (parent == null)
                return null;
            var node = parent.SelectSingleNode(string.Format("Mapping [@Key='{0}']", ext)) as XmlElement;
            if (node == null)
                return null;

            return new Mapping(node.GetAttribute("Value"), node.GetAttribute("EditText"), node.GetAttribute("OpenControl"));
        }

        private Mapping GetByProgId(string progID)
        {
            var parent = doc.DocumentElement.SelectSingleNode("ByProgID");
            if (parent == null)
                return null;
            var node = parent.SelectSingleNode(string.Format("Mapping [@Key='{0}']", progID)) as XmlElement;
            if (node == null)
                return null;

            return new Mapping(node.GetAttribute("Value"), node.GetAttribute("EditText"), node.GetAttribute("OpenControl"));
        }

        private class Mapping
        {
            public string IconName, EditText, OpenControl;
            public Mapping(string iconName, string editText, string openControl)
            {
                IconName = iconName;
                EditText = editText;
                OpenControl = openControl;
            }
        }
    }
}
