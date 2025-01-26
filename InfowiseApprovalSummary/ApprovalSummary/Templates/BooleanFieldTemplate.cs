using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;
using Infowise.Sharepoint.V3.WebParts;
using System.Data;

namespace Infowise.Sharepoint.V3.Fields
{
    class BooleanFieldTemplate : ITemplate
    {
        ListItemType type;
        string internalName;
        string imgUrl;
        Image img;

        public BooleanFieldTemplate(ListItemType type, string internalName, string imgUrl)
        {
            this.type = type;
            this.internalName = internalName;
            this.imgUrl = imgUrl;
        }
        #region ITemplate Members

        public void InstantiateIn(Control container)
        {
            switch (type)
            {
                case ListItemType.Item:
                    Label uc = new Label();
                    img = new Image();
                    uc.DataBinding += new EventHandler(uc_DataBinding);
                    container.Controls.Add(uc);
                    container.Controls.Add(img);
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
                bool value;
                if (listItem[internalName] is Boolean)
                    value = (bool)listItem[internalName];
                else
                    value = listItem[internalName].ToString() == "0" ? false : true;
                if (string.IsNullOrEmpty(imgUrl))
                {
                    uc.Text = value ? Common.GetCoreString("fld_yes") : Common.GetCoreString("fld_no");
                    img.Visible = false;
                }
                else
                {
                    if (value)
                    {
                        img.ImageUrl = imgUrl;
                    }
                    else
                        img.Visible = false;
                }
            }
            else img.Visible = false;
        }
    }
}
