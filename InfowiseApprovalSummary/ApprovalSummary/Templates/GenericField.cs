using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;
using System.Xml;
using Microsoft.SharePoint.Utilities;
using Infowise.Sharepoint.V3.WebParts;

namespace Infowise.Sharepoint.V3.Fields
{
    class GenericFieldTemplate : ITemplate
    {
        ListItemType type;
        string internalName;

        public GenericFieldTemplate(ListItemType type, string internalName)
        {
            this.type = type;
            this.internalName = internalName;
        }

        #region ITemplate Members

        public void InstantiateIn(Control container)
        {

            switch (type)
            {
                case ListItemType.Item:
                    Literal uc = new Literal();
                    uc.DataBinding += new EventHandler(uc_DataBinding);
                    container.Controls.Add(uc);
                    break;
            }
        }

        void uc_DataBinding(object sender, EventArgs e)
        {

            Literal uc = (Literal)sender;
            SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
            SPDataSourceViewResultItem item = (SPDataSourceViewResultItem)gr.DataItem;
            SPListItem listItem = (SPListItem)item.ResultItem;

            if (internalName == "Edit")
            {
                (uc.Parent as WebControl).CssClass = "ms-vb-icon";

                if (listItem.ParentList is SPDocumentLibrary)
                {
                    var docLib = listItem.ParentList as SPDocumentLibrary;
                    if (Common.IsSharePoint2010)
                    {
                        string editUrl = string.Format("{0}/_layouts/listform.aspx?PageType=6&amp;ListId={1}&amp;ID={2}", listItem.Web.Url,
                            listItem.ParentList.ID,
                            listItem.ID);
                        uc.Text = string.Format("<a onclick=\"EditItemWithCheckoutAlert(null, '{0}', '{3}', '0', '{1}', '{0}', '', '1');return false;\" href=\"{0}\" target=_self><img border=0 alt=\"{2}\" src=\"/_layouts/images/edititem.gif\"></a>",
                            editUrl,
                            listItem.File.Url,
                            Common.GetCoreString("Edit"),
                            docLib.ForceCheckout ? "1" : "0");
                    }
                    else
                    {
                        uc.Text = string.Format("<a onclick=\"STSNavigateWithCheckoutAlert(this.href, '{2}','0','{3}','{4}') ;return false;\" href=\"{0}\" target=_self><img border=0 alt=\"{1}\" src=\"/_layouts/images/edititem.gif\"></a>",
                            listItem.ParentList.Forms[PAGETYPE.PAGE_EDITFORM].ServerRelativeUrl + "?ID=" + listItem.ID,
                            Common.GetCoreString("edit_doc_prop"),
                            docLib.ForceCheckout ? "1" : "0",
                            SPEncode.ScriptEncode(SPEncode.UrlEncodeAsUrl(listItem.File.ServerRelativeUrl)),
                            SPEncode.ScriptEncode(SPEncode.UrlEncodeAsUrl(listItem.Web.Url)));
                    }
                }
                else
                {
                    if (Common.IsSharePoint2010)
                    {
                        string editUrl = string.Format("{0}/_layouts/listform.aspx?PageType=6&amp;ListId={1}&amp;ID={2}", listItem.Web.Url,
                            listItem.ParentList.ID,
                            listItem.ID);
                        uc.Text = string.Format("<a onclick=\"EditItemWithCheckoutAlert(null, '{0}', '0', '', '{1}', '{0}', '', '1');return false;\" href=\"{0}\" target=_self><img border=0 alt=\"{2}\" src=\"/_layouts/images/edititem.gif\"></a>",
                            editUrl,
                            listItem.Url,
                            Common.GetCoreString("Edit"));
                    }
                    else
                    {
                        uc.Text = string.Format("<a onclick=\"GoToLink(this);return false;\" href=\"{0}\" target=_self><img border=0 alt=\"{1}\" src=\"/_layouts/images/edititem.gif\"></a>",
                            SPContext.Current.Site.MakeFullUrl(listItem.ParentList.Forms[PAGETYPE.PAGE_EDITFORM].ServerRelativeUrl) + "?ID=" + listItem.ID,
                            Common.GetCoreString("Edit"));
                    }
                }
                return;
            }


            SPField field = listItem.Fields.GetFieldByInternalName(internalName);

            if (field.TypeAsString == "InfowiseProgressField")
            {
                FieldHelper.RenderProgressFieldHtml(field, listItem, uc);
                return;
            }

            if (listItem[internalName] != null)
            {
                uc.Text = field.GetFieldValueAsHtml(listItem[internalName]);

                switch (field.TypeAsString)//special handling for our fields
                {
                    case "DateTime":
                        DateTime date = (DateTime)listItem[internalName];
                        SPFieldDateTime dateField = (SPFieldDateTime)field;

                        string valueDate;
                        if (dateField.DisplayFormat == SPDateTimeFieldFormatType.DateOnly)
                            valueDate = date.ToString("d", SPContext.Current.Web.Locale);
                        else
                            valueDate = date.ToString("g", SPContext.Current.Web.Locale);
                        uc.Text = valueDate;
                        break;

                }

            }
        }

        #endregion
    }
}
