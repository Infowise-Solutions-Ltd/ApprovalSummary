using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;
using System.Data;

namespace Infowise.Sharepoint.V3.Fields
{
    class UrlFieldTemplate : ITemplate
    {
        ListItemType type;
        string internalName;

        public UrlFieldTemplate(ListItemType type, string internalName)
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
                    UrlControl uc = new UrlControl();
                    uc.DataBinding += new EventHandler(uc_DataBinding);
                    container.Controls.Add(uc);
                    break;
            }

        #endregion
        }

        void uc_DataBinding(object sender, EventArgs e)
        {
            UrlControl uc = (UrlControl)sender;
            SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            if (listItem[internalName] != null)
            {
                uc.UrlValue = listItem[internalName].ToString();
            }
        }
    }
}
