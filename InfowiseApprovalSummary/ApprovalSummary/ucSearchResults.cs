using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Data;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using System.Xml;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;
using Infowise.Sharepoint.V3.Fields.Controls;
using Infowise.Sharepoint.V3.WebParts;
using System.Web;
using System.Drawing;
using Infowise.Sharepoint.V3.Fields;


namespace Infowise.Sharepoint.ApprovalSummaryWP
{
    public class ucSearchResults :UserControl
    {
        
        #region Consts
        private const string PARENTLINK = "IWParentLink";
        private const string CTX1 = @"ctx = new ContextInfo();";
        private const string CTX2 = @"ctx.displayFormUrl = ""{7}"";
ctx.editFormUrl = ""{8}"";";
        private const string CTX2_2010 = @"
      var existingHash = '';
      if(window.location.href.indexOf(""#"") > -1)%7B
        existingHash = window.location.href.substr(window.location.href.indexOf(""#""));
      %7D
      ctx.existingServerFilterHash = existingHash;
      if (ctx.existingServerFilterHash.indexOf(""ServerFilter="") == 1) %7B
        ctx.existingServerFilterHash = ctx.existingServerFilterHash.replace(/-/g, '&').replace(/&&/g, '-');
        var serverFilterRootFolder = GetUrlKeyValue(""RootFolder"", true,ctx.existingServerFilterHash);
        var currentRootFolder = GetUrlKeyValue(""RootFolder"", true);
        if("""" == serverFilterRootFolder && """" != currentRootFolder)
        %7B
          ctx.existingServerFilterHash += ""&RootFolder="" + currentRootFolder;
        %7D
        window.location.hash = '';
        window.location.search = '?' + ctx.existingServerFilterHash.substr(""ServerFilter="".length + 1);
      %7D

    ctx.listBaseType = {11};
    ctx.NavigateForFormsPages = false;

      ctx.displayFormUrl = ""{4}/_layouts/listform.aspx?PageType=4&ListId={0}"";
      ctx.editFormUrl = ""{4}/_layouts/listform.aspx?PageType=6&ListId={0}"";";
        private const string CTX3 = @"
ctx.listBaseType = {11};
ctx.listTemplate = {10};
ctx.listName = ""{0}"";
ctx.view = ""{1}"";
ctx.listUrlDir = ""{2}"";
ctx.HttpPath = ""{3}\u002f_vti_bin\u002fowssvr.dll?CS=65001"";
ctx.HttpRoot = ""{4}"";
ctx.imagesPath = ""\u002f_layouts\u002fimages\u002f"";
ctx.PortalUrl = """";
ctx.SendToLocationName = """";
ctx.SendToLocationUrl = """";
ctx.RecycleBinEnabled = -1;
ctx.OfficialFileName = ""Records"";
ctx.WriteSecurity = ""1"";
ctx.SiteTitle = ""{5}"";
ctx.ListTitle = ""{6}"";
if (ctx.PortalUrl == """") ctx.PortalUrl = null;
ctx.isWebEditorPreview = 0;
ctx.ctxId = ""{12}"";
g_ViewIdToViewCounterMap[ ""{1}"" ]= 1;
ctx.CurrentUserId = {9};
ctx.isForceCheckout = {13};
ctx.verEnabled = {14};
ctx.WorkflowsAssociated = {15};

 
ctx{12} = ctx;";
        private const string CTX3_2010 = @"
ctx.listTemplate = ""{10}"";
ctx.listName = ""{0}"";
ctx.view = ""{1}"";
ctx.listUrlDir = ""{2}"";
ctx.HttpPath = ""{3}/_vti_bin/owssvr.dll?CS=65001"";
ctx.HttpRoot = ""{4}"";
ctx.imagesPath = ""/_layouts/images/"";
ctx.PortalUrl = """";
ctx.SendToLocationName = """";
ctx.SendToLocationUrl = """";
ctx.RecycleBinEnabled = -1;
ctx.OfficialFileName = """";
ctx.WriteSecurity = ""1"";
ctx.SiteTitle = ""{5}"";
ctx.ListTitle = ""{6}"";
if (ctx.PortalUrl == """") ctx.PortalUrl = null;
ctx.isWebEditorPreview = 0;
ctx.ctxId = ""{12}"";//100123;
     
 if (g_ViewIdToViewCounterMap[""{1}""] == null)
          g_ViewIdToViewCounterMap[""{1}""]= ""{12}"";

ctx.CurrentUserId = {9};
ctx.isXslView = true;
ctx.isForceCheckout = {13};
ctx.verEnabled = {14};
ctx.WorkflowsAssociated = {15};

ctx{12} = ctx;
      g_ctxDict['ctx{12}'] = ctx;
"
; 
        #endregion

        #region Private members
        SPGridView gvResults;
        Literal ltrCtx, ltrEditor;
        TextBox txtStore;
        ToolBar tb;
        PlaceHolder ph;
        #endregion

        internal event UnhandledExceptionEventHandler ErrorOccured;

        public ucSearchResults()
        {
            Logger.EnterMethod();
            this.PreRender += new EventHandler(ucSearchResults_PreRender);
        }


        void ucSearchResults_PreRender(object sender, EventArgs e)
        {
            Logger.EnterMethod();

            try
            {
                if (Items == null)
                    return;

                EnsureChildControls();

                gvResults.DataSource = Items.DefaultView;
                gvResults.DataBind();

                if (!string.IsNullOrEmpty(Page.Request.Form[txtStore.UniqueID]))
                {
                    txtStore.Text = "";
                }

                tb.Visible = HasItems;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static bool isSharePoint2010()
        {
            return SPFarm.Local.BuildVersion.Major != 12;
        }

        public bool IsSorted
        {
            get
            {
                return isSorted;
            }
        }


        DataTable items = null;
        private DataTable Items
        {
            get
            {
                if (items != null)
                    return items;

                if (items == null && ViewState["Items"] != null)
                {
                    items = (DataTable)ViewState["Items"];
                }

                if (items == null)
                    items = (NamingContainer as ApprovalSummary).GetDataSource();
                ViewState["Items"] = items;
                if (items != null && items.Rows.Count > 0)
                    AddCtx();
                return items;
            }
        }

        private void AddCtx()
        {
            Logger.EnterMethod();

            List<string> lists = new List<string>();

            ltrCtx.Text = "<script type=\"text/javascript\" language=\"javascript\">function GetSource(){return escapeProperly(window.location.href);}</script>";
            bool vbAdded = false;

            for (int i = 0; i<Items.Rows.Count; i++)
            {
                DataRow row = Items.Rows[i];
                string listID = row["ListID"].ToString();
                if (lists.Contains(listID))
                    continue;

                lists.Add(listID);

                #region Context menu handling
                string dispFormUrl = row["DisplayFormUrl"].ToString();
                string editFormUrl = row["EditFormUrl"].ToString();
                string serverRelativeUrl = row["WebServerRelativeUrl"].ToString();
                if (serverRelativeUrl.Equals("/"))
                    serverRelativeUrl = "";

                ltrCtx.Text += string.Format("<script type=\"text/javascript\" language=\"javascript\">{0}</script>", string.Format(CTX1 + (isSharePoint2010() ? CTX2_2010 : CTX2) + (isSharePoint2010() ? CTX3_2010 : CTX3), (new Guid(row["ListID"].ToString())).ToString("B"),
              row["DefaultViewID"],
              EncodeUrl(row["RootFolderUrl"].ToString()),
              EncodeUrl(serverRelativeUrl), EncodeUrl(row["WebUrl"].ToString()),
              row["ProjectProperty.Title"], row["ListProperty.Title"], EncodeUrl(dispFormUrl), EncodeUrl(editFormUrl), SPContext.Current.Web.CurrentUser == null ? -1 : SPContext.Current.Web.CurrentUser.ID, int.Parse(row["BaseTemplate"].ToString()), int.Parse(row["BaseType"].ToString()), GetCtxId(row["ListID"].ToString()),
              row["ForceCheckout"].ToString().ToLower(), (int.Parse(row["EnableVersioning"].ToString()) + int.Parse(row["EnableMinorVersions"].ToString())), (row["HasWorkflows"]).ToString().ToLower())).Replace("%7D", "}").Replace("%7B", "{");
                #endregion

                #region Document library handling
                if (bool.Parse(row["IsDocLib"].ToString()) && !vbAdded)
                {
                    ltrEditor.Text = @"<SCRIPT>if(typeof(FixTextAlignForBidi)=='function'){FixTextAlignForBidi(""right"");}</SCRIPT><SCRIPT LANGUAGE=""VBSCRIPT"" type=""text/vbscript"">
    On Error Resume Next
    Set EditDocumentButton = CreateObject(""SharePoint.OpenDocuments.3"")
    If (IsObject(EditDocumentButton)) Then
        fNewDoc3 = true
    Else
        Set EditDocumentButton = CreateObject(""SharePoint.OpenDocuments.2"")
        If (IsObject(EditDocumentButton)) Then
            fNewDoc2 = true
        Else
            Set EditDocumentButton = CreateObject(""SharePoint.OpenDocuments.1"")
        End If
    End If    
    fNewDoc = IsObject(EditDocumentButton)
</SCRIPT>
";
                    vbAdded = true;
                }
                #endregion
            }

            #region client script registration for AJAX
            if (ScriptManager.GetCurrent(Page) != null)
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "ctxCode", ltrCtx.Text, false);
                if (vbAdded)
                    ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "ctxDLCode", ltrEditor.Text, false);
            }
            #endregion
        }

        /// <summary>
        /// Encodes URLs for CTX
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string EncodeUrl(string url)
        {
            if (isSharePoint2010())
                return url;
            return SPEncode.ScriptEncode(SPEncode.UrlEncodeAsUrl(url));

        }

        internal static string GetCtxId(string listID)
        {
            Guid id = new Guid(listID);
            return ((int)Math.Abs(BitConverter.ToInt32(id.ToByteArray(), 0))).ToString();
        }

        /// <summary>
        /// Page size of grid
        /// </summary>
        public int PageSize
        {
            get;
            set;
        }

        private void BuildFields()
        {
            Logger.EnterMethod();

            try
            {
                gvResults.Columns.Clear();
                string filters = string.Empty;

                TemplateField chkFld = new IWTemplateField();
                chkFld.HeaderTemplate = new CheckTemplate(ListItemType.Header);
                var chkTemplate = new CheckTemplate(ListItemType.Item);
                chkFld.ItemTemplate = chkTemplate;
                gvResults.Columns.Add(chkFld);

                AddColumn("LinkFilename", Common.GetWssString("lstsetng_name_title") + "/" + Common.GetWssString("lstsetng_name_label"));
                AddColumn("ListProperty.Title", Common.GetWssString("listname_listupper"));
                AddColumn("Modified", Common.GetWssString("versions_ModifiedCol"));
                AddColumn("Editor", Common.GetWssString("versions_ModifiedByCol"));

                TemplateField approveFld = new IWTemplateField();
                approveFld.HeaderText = Common.GetCoreString("dcl_schema_view_approvereject");
                approveFld.HeaderStyle.Font.Bold = false;
                approveFld.ItemTemplate = new ApproveTemplate(ListItemType.Item, txtStore.ClientID);
                gvResults.Columns.Add(approveFld);

                gvResults.FilterDataFields = "ListProperty.Title,Modified,Editor";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        private void AddColumn(string fieldName, string fieldTitle)
        {
            Logger.EnterMethod(fieldName, fieldTitle);

            Logger.Log("Current field: " + fieldName);

            if (fieldName.EndsWith("Menu") || fieldName == "LinkTitle" || fieldName == "LinkFilename")
            {
                IWTemplateField linkFld = new IWTemplateField();
                linkFld.DataField = fieldName;
                linkFld.HeaderText = fieldTitle;
                linkFld.HeaderStyle.Font.Bold = false;
                linkFld.HeaderStyle.CssClass = isSharePoint2010() ? "ms-vh2" : "ms-vh2-nofilter";
                if (isSharePoint2010())
                    linkFld.ItemStyle.Height = new Unit("100%");

                var menuTmpl = new MenuFieldTemplate(ListItemType.Item, fieldName);
                linkFld.ItemTemplate = menuTmpl;
                linkFld.SortExpression = fieldName;
                gvResults.Columns.Add(linkFld);
            }
            else if (fieldName.Equals("DocIcon"))
            {
                IWTemplateField imgFld = new IWTemplateField();
                imgFld.DataField = fieldName;
                imgFld.HeaderText = fieldTitle;
                imgFld.HeaderStyle.Font.Bold = false;
                imgFld.HeaderStyle.CssClass = isSharePoint2010() ? "ms-vh2" : "ms-vh-icon";

                imgFld.ItemTemplate = new IconFieldTemplate(ListItemType.Item, fieldName, false);
                imgFld.SortExpression = fieldName;
                gvResults.Columns.Add(imgFld);
            }
            else
            {
                SPBoundField sbf = isSharePoint2010() ? new IWBoundField() : new SPBoundField();
                sbf.HeaderStyle.CssClass = isSharePoint2010() ? "ms-vh2" : "ms-vh2-nofilter";
                sbf.HeaderStyle.Font.Bold = false;
                sbf.AccessibleHeaderText = fieldTitle;
                sbf.DataField = fieldName;
                sbf.HeaderText = fieldTitle;
                sbf.SortExpression = fieldName;

                gvResults.Columns.Add(sbf);
            }
        }

        /// <summary>
        /// Check if grid has items
        /// </summary>
        public bool HasItems
        {
            get
            {
                EnsureChildControls();
                return gvResults.Rows.Count > 0;
            }
        }

        protected override void CreateChildControls()
        {
            Logger.EnterMethod();

            try
            {
                ph = new PlaceHolder();
                Controls.Add(ph);

                txtStore = new TextBox();
                txtStore.ID = "txtStore";
                txtStore.Style.Add("display", "none");
                Controls.Add(txtStore);

                if (Page.IsPostBack)
                {
                    if (!string.IsNullOrEmpty(Page.Request.Form[txtStore.UniqueID]))
                    {
                        ApproveHelper.Approve(Page.Request.Form[txtStore.UniqueID]);
                        items = null;
                        ViewState["Items"] = null;
                    }
                }



                ltrCtx = new Literal();
                Controls.Add(ltrCtx);

                ltrEditor = new Literal();
                Controls.Add(ltrEditor);

                gvResults = new SPGridView();
                gvResults.HeaderStyle.ForeColor = Color.FromArgb(120, 120, 120);
                gvResults.ID = "gvResults";
                gvResults.AutoGenerateColumns = false;
                gvResults.EnableViewState = false;
                gvResults.AllowSorting = true;
                gvResults.AllowFiltering = false;
                gvResults.Sorting += new GridViewSortEventHandler(gvResults_Sorting);

                if (Grouping)
                {
                    gvResults.AllowGrouping = true;
                    gvResults.AllowGroupCollapse = true;
                    gvResults.GroupField = "ProjectProperty.Title";
                    gvResults.GroupFieldDisplayName = Common.GetWssString("mngfield_HeadingSource");
                }

                if (PageSize > 0)
                {
                    gvResults.AllowPaging = true;
                    gvResults.PageSize = PageSize;
                    gvResults.PageIndexChanging += new GridViewPageEventHandler(gvResults_PageIndexChanging);
                }
                Controls.Add(gvResults);
                gvResults.PagerTemplate = null;

                BuildFields();

                CreateMenuBar();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        private void CreateMenuBar()
        {
            tb = (ToolBar)Page.LoadControl("/_controltemplates/ToolBar.ascx");
            LinkButton saveButton = new LinkButton();
            saveButton.ID = "approveSelected";
            saveButton.Text = Common.GetString("ApproveSelected");
            saveButton.OnClientClick = string.Format("return iw_ApproveRejectAll('{0}','{1}','Approved');", gvResults.ClientID, txtStore.ClientID);
            tb.Buttons.Controls.Add(saveButton);

            LinkButton rejectButton = new LinkButton();
            rejectButton.ID = "rejectSelected";
            rejectButton.Text = Common.GetString("RejectSelected");
            rejectButton.OnClientClick = string.Format("return iw_ApproveRejectAll('{0}','{1}','Denied');", gvResults.ClientID, txtStore.ClientID);
            tb.Buttons.Controls.Add(rejectButton);

            ph.Controls.AddAt(0, tb);
        }



        void gvResults_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            Logger.EnterMethod();

            try
            {
                gvResults.PageIndex = e.NewPageIndex;
                gvResults.DataBind();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }


        bool isSorted = false;
        void gvResults_Sorting(object sender, GridViewSortEventArgs e)
        {
            Logger.EnterMethod();

            try
            {
                if (!isSorted)
                {
                    EnsureChildControls();
                    isSorted = true;
                    
                    string orderField = e.SortExpression;

                    string orderCommand = string.Empty;
                    string orderDir = e.SortDirection == SortDirection.Ascending ? "ASC" : "DESC";
                    if (ViewState["OrderField"] != null)
                    {
                        if (ViewState["OrderField"].ToString().Equals(orderField))
                        {
                            orderDir = ViewState["OrderDir"].ToString() == "ASC" ? "DESC" : "ASC";
                        }

                    }
                    ViewState["OrderDir"] = orderDir;
                    ViewState["OrderField"] = orderField;

                    Items.DefaultView.Sort = string.Format("{0} {1}", orderField, orderDir);
                   // gvResults.Sort(orderField, e.SortDirection
                    gvResults.DataBind();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(ex, false));
            }
           
        }

        internal void Clear()
        {
            Logger.EnterMethod();

            try
            {
                EnsureChildControls();
                gvResults.Columns.Clear();
                ViewState["Items"] = null;
                ViewState["OrderField"] = null;
                ViewState["OrderDir"] = null;
                gvResults.DataSource = null;
                gvResults.DataBind();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ErrorOccured != null)
                    ErrorOccured(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }

        public bool Grouping { get; set; }
    }
}
