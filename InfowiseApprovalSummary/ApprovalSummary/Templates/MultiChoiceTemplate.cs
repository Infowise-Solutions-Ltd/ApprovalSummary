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
    class MultiChoiceTemplate : ITemplate
    {
        ListItemType type;
        string internalName;

        public MultiChoiceTemplate(ListItemType type, string internalName)
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
                    Label uc = new Label();
                    uc.DataBinding += new EventHandler(uc_DataBinding);
                    container.Controls.Add(uc);
                    break;
            }

        #endregion
        }

        void uc_DataBinding(object sender, EventArgs e)
        {
            Logger.EnterMethod(sender, e);

            Label uc = (Label)sender;
            SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            if (listItem[internalName] != null)
            {
                string value = listItem[internalName].ToString().Replace(";#", "; ");
                if (value.StartsWith("; "))
                    value = value.Substring(2);
                if (value.EndsWith("; "))
                    value = value.Remove(value.Length - 2);
                uc.Text = value;
            }
        }
    }
}
