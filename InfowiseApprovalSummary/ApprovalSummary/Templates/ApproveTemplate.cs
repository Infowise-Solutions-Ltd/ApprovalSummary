using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.SharePoint.WebControls;
using System.Data;

namespace Infowise.Sharepoint.V3.WebParts
{
    public class ApproveTemplate : ITemplate
    {
        ListItemType type;
        string storeID;

        public ApproveTemplate(ListItemType type, string storeID)
        {
            this.type = type;
            this.storeID = storeID;
        }

        public void InstantiateIn(Control container)
        {
            switch (type)
            {
                case ListItemType.Item:
                    HyperLink hl = new HyperLink();
                    hl.Text = Common.GetCoreString("dcl_schema_view_approvereject");
                    container.Controls.Add(hl);

                    Panel pnl = new Panel();
                    pnl.ID = "pnlApprove";
                    container.Controls.Add(pnl);

                    pnl.Style.Add("display", "none");
                    pnl.Style.Add("background-color", "#E2E4FF");
                    pnl.Style.Add("border", "1px solid #A8CCFF");
                    pnl.Style.Add("padding", "4px");
                    hl.NavigateUrl = "javascript:";
                    hl.Attributes.Add("onclick","this.nextSibling.style.display='block';this.style.display='none';return false;");

                    RadioButtonList cblApprove = new RadioButtonList();
                    cblApprove.ID = "cblApprove";
                    pnl.Controls.Add(cblApprove);

                    cblApprove.Items.Add(new ListItem(Common.GetCoreString("402"), "Approved"));
                    cblApprove.Items.Add(new ListItem(Common.GetCoreString("400"), "Rejected"));
                    cblApprove.Items.Add(new ListItem(Common.GetCoreString("401"), "Pending"));
                    cblApprove.SelectedIndex = 2;

                    Label lbl = new Label();
                    lbl.Style.Add("margin-top", "13px");
                    lbl.Style.Add("display", "block");
                    lbl.Text = Common.GetCoreString("BlogComment");
                    pnl.Controls.Add(lbl);

                    TextBox txtComments = new TextBox();
                    txtComments.ID = "txtComments";
                    txtComments.TextMode = TextBoxMode.MultiLine;
                    txtComments.Rows = 3;
                    txtComments.Style.Add("overflow", "auto");
                    txtComments.Style.Add("border", "1px solid #A8CCFF");
                    pnl.Controls.Add(txtComments);

                    pnl.Controls.Add(new HtmlGenericControl("br"));

                    Button btnOK = new Button();
                    btnOK.Text = Common.GetWssString("tb_save");
                    btnOK.CommandName = "ApproveReject";
                    btnOK.CssClass = "ms-ButtonHeightWidth";
                    btnOK.CausesValidation = false;
                    btnOK.DataBinding += new EventHandler(btnOK_DataBinding);
                    
                    btnOK.ID = "btnOK";
                    pnl.Controls.Add(btnOK);

                    pnl.Controls.Add(new Literal() { Text = "&nbsp;" });

                    HyperLink hlCancel = new HyperLink();
                    hlCancel.NavigateUrl = "javascript:";
                    hlCancel.Attributes.Add("onclick", "parentNode.style.display='none';parentNode.previousSibling.style.display='block';return false;");
                    hlCancel.Text = Common.GetWssString("form_cancel");
                    pnl.Controls.Add(hlCancel);
                    break;
            }
        }

        void btnOK_DataBinding(object sender, EventArgs e)
        {
            Button btnOK = sender as Button;
            SPGridViewRow gr = (SPGridViewRow)btnOK.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            Control cblApprove = btnOK.NamingContainer.FindControl("cblApprove");
            Control txtComments = btnOK.NamingContainer.FindControl("txtComments");
            btnOK.OnClientClick = string.Format("iw_SetApproval('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", storeID, listItem["WebID"], listItem["ListID"], listItem["ID"], cblApprove.ClientID, txtComments.ClientID);
        }
    }
}
