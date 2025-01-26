using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using System.Data;

namespace Infowise.Sharepoint.V3.WebParts
{
    public class CheckTemplate :ITemplate
    {
        ListItemType type;

        public CheckTemplate(ListItemType type)
        {
            this.type = type;
        }
        public void InstantiateIn(Control container)
        {
            switch (type)
            {
                case ListItemType.Header:
                    Image img = new Image();
                    img.ImageUrl = "/_layouts/images/checkall.gif";
                    img.Style.Add("cursor", "pointer");
                    img.Attributes.Add("onclick", "iw_selectAllApproval(this)");
                    container.Controls.Add(img);
                    break;
                case ListItemType.Item:
                    CheckBox chk = new CheckBox();
                    container.Controls.Add(chk);
                    chk.DataBinding += new EventHandler(container_DataBinding);
                    break;
            }
        }

        void container_DataBinding(object sender, EventArgs e)
        {
            if (type != ListItemType.Item)
                return;

            DataControlFieldCell cell = sender as DataControlFieldCell;
            CheckBox chk = sender as CheckBox;
            SPGridViewRow gr = (SPGridViewRow)chk.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            chk.ID = string.Format("iwaccheck_{0}|{1}|{2}", listItem["WebID"], listItem["ListID"], listItem["ID"]);
        }
    }
}
