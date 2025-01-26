using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using Infowise.Sharepoint.V3.Fields;
using System.Web.UI.WebControls;
using Infowise.Sharepoint.V3.WebParts;
using System.Web.UI;

namespace Infowise.Sharepoint.ApprovalSummaryWP
{
    public class SettingsPane:EditorPart
    {
        Table tblMain;
        DropDownList ddlScope, ddlPage;

        protected override void CreateChildControls()
        {
            Logger.EnterMethod();

            try
            {
                #region Main table
                tblMain = new Table();
                tblMain.CellPadding = 0;
                tblMain.CellSpacing = 0;
                tblMain.BorderWidth = 0;
                tblMain.Width = new Unit("100%");
                Controls.Add(tblMain);
                #endregion

                #region Scope
                Panel pnlScopeHdr, pnlScopeBody;
                AddTableRow(true, false, out pnlScopeHdr, out pnlScopeBody);
                ddlScope = new DropDownList();
                ddlScope.ToolTip = Common.GetString("ScopeDesc");

                Label lblScope = new Label();
                lblScope.Text = Common.GetWssString("search_searchscope");
                pnlScopeHdr.Controls.Add(lblScope);
                pnlScopeBody.Controls.Add(ddlScope);

                if (ddlScope.Items.Count == 0)
                {
                    ddlScope.Items.Add(new ListItem(Common.GetString(Recursion.Current.ToString()), Recursion.Current.ToString()));
                    ddlScope.Items.Add(new ListItem(Common.GetString(Recursion.Recursive.ToString()), Recursion.Recursive.ToString()));
                    ddlScope.Items.Add(new ListItem(Common.GetString(Recursion.SiteCollection.ToString()), Recursion.SiteCollection.ToString()));
                }
                #endregion

                #region Page Size
                Panel pnlPageHdr, pnlPageBody;
                AddTableRow(true, true, out pnlPageHdr, out pnlPageBody);
                ddlPage = new DropDownList();
                ddlPage.ToolTip = Common.GetString("PageSizeDesc");

                Label lblPage = new Label();
                lblPage.Text = Common.GetString("PageSize");
                pnlPageHdr.Controls.Add(lblPage);
                pnlPageBody.Controls.Add(ddlPage);

                if (ddlPage.Items.Count == 0)
                {
                    ddlPage.Items.Add(new ListItem(Common.GetString("Unlimited"), "0"));
                    ddlPage.Items.Add(new ListItem("5", "5"));
                    ddlPage.Items.Add(new ListItem("10", "10"));
                    ddlPage.Items.Add(new ListItem("20", "20"));
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Controls.Add(new Label() { CssClass = "me-error", Text = ex.Message });
            }
        }

        public override bool ApplyChanges()
        {
            Logger.EnterMethod();

            EnsureChildControls();
            ApprovalSummary app = this.WebPartToEdit as ApprovalSummary;
            if (app == null)
                return false;

            app.Scope = (Recursion)Enum.Parse(typeof(Recursion), ddlScope.SelectedValue);
            app.PageSize = int.Parse(ddlPage.SelectedValue);

            return true;
        }

        public override void SyncChanges()
        {
            Logger.EnterMethod();

            EnsureChildControls();
            ApprovalSummary app = this.WebPartToEdit as ApprovalSummary;
            if (app == null)
                return;

            try
            {
                ddlScope.SelectedValue = app.Scope.ToString();
                ddlPage.SelectedValue = app.PageSize.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }


        /// <summary>
        /// Adds table row
        /// </summary>
        /// <param name="hasBody"></param>
        /// <param name="isLast"></param>
        /// <param name="head"></param>
        /// <param name="body"></param>
        protected void AddTableRow(bool hasBody, bool isLast, out Panel head, out Panel body)
        {
            Logger.EnterMethod(hasBody, isLast);

            TableRow row = new TableRow();
            tblMain.Rows.Add(row);

            TableCell cell = new TableCell();
            row.Cells.Add(cell);

            head = new Panel();
            head.CssClass = "UserSectionHead";
            cell.Controls.Add(head);

            if (hasBody)
            {
                Panel outerBody = new Panel();
                outerBody.CssClass = "UserSectionBody";
                cell.Controls.Add(outerBody);

                body = new Panel();
                body.CssClass = "UserControlGroup";
                outerBody.Controls.Add(body);
                body.Wrap = false;
            }
            else
                body = null;

            if (!isLast)
            {
                Panel dot = new Panel();
                dot.CssClass = "UserDottedLine";
                dot.Style.Add(HtmlTextWriterStyle.Width, "100%");
                cell.Controls.Add(dot);
            }

        }
    }
}
