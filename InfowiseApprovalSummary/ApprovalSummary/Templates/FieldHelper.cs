using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Xml;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;

namespace Infowise.Sharepoint.V3.Fields
{
    static class FieldHelper
    {
        public enum ProgressMode
        {
            KPI,
            ProgressBar,
            Countdown
        }

        public static void RenderColorFieldHtml(SPField field, SPListItem listItem, Literal uc)
        {
            Logger.EnterMethod(field, listItem, uc);

            if (listItem[field.InternalName] == null)
                return;

            XmlDocument schemaDoc = new XmlDocument();
            schemaDoc.LoadXml(field.SchemaXml);

            string colorMapping = schemaDoc.DocumentElement.GetAttribute("ColorMapping");
            bool applyToRow = string.IsNullOrEmpty(schemaDoc.DocumentElement.GetAttribute("ApplyToRow")) ? true : bool.Parse(schemaDoc.DocumentElement.GetAttribute("ApplyToRow"));
            bool applyToBg = string.IsNullOrEmpty(schemaDoc.DocumentElement.GetAttribute("ApplyToBg")) ? true : bool.Parse(schemaDoc.DocumentElement.GetAttribute("ApplyToBg"));
            bool showIcon = string.IsNullOrEmpty(schemaDoc.DocumentElement.GetAttribute("ShowIcon")) ? false : bool.Parse(schemaDoc.DocumentElement.GetAttribute("ShowIcon"));
            bool showLabel = string.IsNullOrEmpty(schemaDoc.DocumentElement.GetAttribute("ShowLabel")) ? false : bool.Parse(schemaDoc.DocumentElement.GetAttribute("ShowLabel"));

            if (string.IsNullOrEmpty(colorMapping))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(colorMapping);

            var colorNode = xmlDoc.SelectSingleNode("//Categories/Category[@Name='" + listItem[field.InternalName].ToString() + "']");
            if (colorNode == null || colorNode.Attributes["Color"] == null)
                return;

            if (showIcon)
            {
                string img = string.Format("<img title=\"{0}\" src=\"{1}\"/>", uc.Text, colorNode.Attributes["Color"].Value);
                if (showLabel)
                    uc.Text = img + "&nbsp;" + uc.Text;
                else
                    uc.Text = img;
            }
            else
            {
                if (applyToRow)
                {
                    SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
                    gr.Style.Add("background-color", colorNode.Attributes["Color"].Value);
                }
                else
                {
                    DataControlFieldCell cell = uc.Parent as DataControlFieldCell;
                    if (applyToBg)
                        cell.Style.Add("background-color", colorNode.Attributes["Color"].Value);
                    else
                        cell.Style.Add("color", colorNode.Attributes["Color"].Value);
                }

                if (!showLabel)
                    uc.Text = "";
            }

            
        }

        public static void RenderProgressFieldHtml(SPField field, SPListItem listItem, Literal uc)
        {
            Logger.EnterMethod(field, listItem, uc);

            try
            {
                if (uc.Page.ClientScript.IsClientScriptIncludeRegistered("colorField"))
                    uc.Page.ClientScript.RegisterClientScriptInclude("colorField", "/_layouts/InfowiseColorFieldHelper.js");

                uc.Text = string.Format(@"<img src=""/_layouts/images/Infowise/ColorField/kpiprogressbar.gif"" id=""iwprf{0}|{1}""/>
                    <script type=""text/javascript"">
						if(typeof(iwFetchProgress) == ""function"") {{iwFetchProgress(""iwprf{0}|{1}"", ""{2}"", ""{3}"");}}
                     </script>",
                        listItem.ID, field.InternalName, listItem.Web.Url, listItem.ParentList.ID);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

    }
}
