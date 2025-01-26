using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;

namespace Infowise.Sharepoint.V3.Fields
{
    class LookupFieldTemplate : ITemplate
    {
        ListItemType type;
        string dispFormUrl;
        string internalName;

        public LookupFieldTemplate(ListItemType type, string dispFormUrl, string internalName)
        {
            this.type = type;
            this.dispFormUrl = dispFormUrl;
            this.internalName = internalName;
        }

        #region ITemplate Members

        public void InstantiateIn(Control container)
        {

            switch (type)
            {
                case ListItemType.Item:
                    UrlControl uc = new UrlControl();
                    uc.DataBinding += new EventHandler(uc_DataBinding);
                    container.Controls.Add(uc);
                    break;
            }
        }

        void uc_DataBinding(object sender, EventArgs e)
        {

            UrlControl uc = (UrlControl)sender;
            SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
            SPDataSourceViewResultItem item = (SPDataSourceViewResultItem)gr.DataItem;
            SPListItem listItem = (SPListItem)item.ResultItem;
            if (listItem[internalName] != null)
            {
                SPField field = listItem.Fields.GetFieldByInternalName(internalName);
                string lookup = listItem[internalName].ToString();
                string urlValue;
                if (field is SPFieldUser)
                {
                    SPFieldUserValue uv = new SPFieldUserValue(SPContext.Current.Web, lookup);
                    try
                    {
                        urlValue = string.Format("{0}, {1}", string.Format("{0}{1}", dispFormUrl, uv.LookupId), uv.User.Name);
                    }
                    catch
                    {
                        urlValue = string.Format("{0}, {1}", string.Format("{0}{1}", dispFormUrl, uv.LookupId), lookup);
                    }
                }
                else
                {
                    SPFieldLookupValue luv = new SPFieldLookupValue(lookup);
                    urlValue = string.Format("{0}, {1}", string.Format("{0}{1}", dispFormUrl, luv.LookupId), luv.LookupValue);
                }
                uc.UrlValue = urlValue;
            }
        }

        #endregion
    }
}
