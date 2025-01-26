using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;
using System.Web.UI.WebControls;
using System.Data;
using System.Web;
using System.ComponentModel;
using Infowise.Sharepoint.V3.Fields;
using System.Xml;
using Infowise.Sharepoint.ApprovalSummaryWP;
using System.Collections.Generic;
using System.Text;

namespace Infowise.Sharepoint.V3.WebParts
{
    public enum Recursion { Current, SiteCollection, Recursive }
    [Guid("a23a2ceb-3e41-4d4a-930c-a433074b3a4b")]
    public class ApprovalSummary : System.Web.UI.WebControls.WebParts.WebPart
    {
        private ucSearchResults results = null;
        public ApprovalSummary()
        {
            this.ExportMode = WebPartExportMode.All;
        }

        private Recursion scope = Recursion.SiteCollection;
        private int pageSize = 0;

        [WebBrowsable(false),
        Personalizable(PersonalizationScope.Shared)]
        public Recursion Scope
        {
            set
            {
                scope = value;
            }
            get
            {
                return scope;
            }
        }

        [WebBrowsable(false),
       Personalizable(PersonalizationScope.Shared)]
        public int PageSize
        {
            set
            {
                pageSize = value;
            }
            get
            {
                return pageSize;
            }
        }

        /// <summary>
        /// Creates custom editor parts
        /// </summary>
        /// <returns></returns>
        public override EditorPartCollection CreateEditorParts()
        {
            Logger.EnterMethod();

            try
            {
                List<EditorPart> editorParts = new List<EditorPart>();
                SettingsPane fe = new SettingsPane();
                fe.ID = "filterEditor";
                fe.Title = Common.GetString("EditorTitle");

                editorParts.Add(fe);

                return new EditorPartCollection(editorParts);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ShowErrorMessage(ex);
               
                return new EditorPartCollection();
            }
        }

        private void ShowErrorMessage(Exception ex)
        {
            Controls.Add(new Label() { Text = ex.Message, CssClass = "ms-error" });
        }

        protected override void CreateChildControls()
        {
            Logger.EnterMethod();

            try
            {
                if (!Page.ClientScript.IsClientScriptIncludeRegistered("approvalSupport"))
                    Page.ClientScript.RegisterClientScriptResource(GetType(), "Infowise.Sharepoint.V3.WebParts.ApprovalSummary.approval.js");

                UpdatePanel up = null;
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                if (sm != null)
                {
                    up = new UpdatePanel();
                    up.UpdateMode = UpdatePanelUpdateMode.Conditional;
                    Controls.Add(up);
                    this.Style.Add("position", "relative");
                }

                results = new ucSearchResults();
                results.Grouping = Scope != Recursion.Current;
                results.PageSize = PageSize;
                if (up == null)
                    Controls.Add(results);
                else
                    up.ContentTemplateContainer.Controls.Add(results);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);

            }

        }

        internal DataTable GetDataSource()
        {
            Logger.EnterMethod();

            try
            {
                DataTable resultsTable = new DataTable();
                resultsTable.Columns.Add("WebId");
                resultsTable.Columns.Add("ListId");
                resultsTable.Columns.Add("ID");
                resultsTable.Columns.Add("Modified");
                resultsTable.Columns.Add("Editor");
                resultsTable.Columns.Add("ListProperty.Title");
                resultsTable.Columns.Add("ProjectProperty.Title");
                resultsTable.Columns.Add("WebUrl");
                resultsTable.Columns.Add("LinkFilename");
                resultsTable.Columns.Add("FileRef");
                resultsTable.Columns.Add("DisplayUrl");
                resultsTable.Columns.Add("EditForm");
                resultsTable.Columns.Add("RootFolderUrl");
                resultsTable.Columns.Add("PermMask");
                resultsTable.Columns.Add("IsDocLib");
                resultsTable.Columns.Add("FileName");
                resultsTable.Columns.Add("ProgID");
                resultsTable.Columns.Add("ContentTypeId");
                resultsTable.Columns.Add("ContentType");
                resultsTable.Columns.Add("FileServerRelativeUrl");
                resultsTable.Columns.Add("WebServerRelativeUrl");
                resultsTable.Columns.Add("DisplayFormUrl");
                resultsTable.Columns.Add("EditFormUrl");
                resultsTable.Columns.Add("DefaultViewID");
                resultsTable.Columns.Add("BaseTemplate");
                resultsTable.Columns.Add("BaseType");
                resultsTable.Columns.Add("ForceCheckout");
                resultsTable.Columns.Add("EnableVersioning");
                resultsTable.Columns.Add("EnableMinorVersions");
                resultsTable.Columns.Add("HasWorkflows");



                CopyDataToTable(resultsTable, GetDataTable(1));
                CopyDataToTable(resultsTable, GetDataTable(0));
                CopyDataToTable(resultsTable, GetDataTable(5));

                return resultsTable;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Copies data from source table to target table
        /// </summary>
        /// <param name="resultsTable"></param>
        /// <param name="source"></param>
        private void CopyDataToTable(DataTable resultsTable, DataTable source)
        {
            Logger.EnterMethod(resultsTable, source);

            if (source == null)
                return;

            SPWeb curWeb = null;
            resultsTable.BeginLoadData();
            foreach (DataRow row in source.Rows)
            {
                Guid webID = new Guid(row["WebId"].ToString());
                if (webID != SPContext.Current.Web.ID)
                {
                    if (curWeb != null && webID != curWeb.ID)
                    {
                        curWeb.Close();
                        curWeb.Dispose();
                        curWeb = null;
                    }

                    if (curWeb == null)
                    {
                        curWeb = SPContext.Current.Site.OpenWeb(webID);
                        curWeb.Lists.IncludeRootFolder = true;
                    }

                    CopyDataToTable(resultsTable, source, row, curWeb);
                }
                else
                    CopyDataToTable(resultsTable, source, row, SPContext.Current.Web);
            }
            resultsTable.EndLoadData();
        }

        SPList curList = null;
        string defaultViewID = "";
        string rootFolderPath = "";
        /// <summary>
        /// Copies data for a specific project
        /// </summary>
        /// <param name="resultsTable"></param>
        /// <param name="source"></param>
        /// <param name="row"></param>
        /// <param name="projectWeb"></param>
        private void CopyDataToTable(DataTable resultsTable, DataTable source, DataRow row, SPWeb projectWeb)
        {
            Guid listID = new Guid(row["ListId"].ToString());
            if (curList == null || curList.ID != listID)
            {
                curList = projectWeb.Lists[listID];
                defaultViewID = curList.DefaultView.ID.ToString("B");

                rootFolderPath = curList.RootFolder.ServerRelativeUrl;
                if (rootFolderPath.StartsWith("/"))
                    rootFolderPath = rootFolderPath.Substring(1);
            }
           
            if (!curList.EnableModeration)
                return;

            if (row["_ModerationStatus"].ToString() != "2")
                return;

            if (CanUserApprove(row["PermMask"].ToString()))//item.DoesUserHavePermissions(SPBasePermissions.ApproveItems))
            {
                DataRow targetRow = resultsTable.NewRow();
                foreach (DataColumn column in source.Columns)
                {
                    if (column.ColumnName.Equals("FSObjType")
                    || column.ColumnName.Equals("FileLeafRef")
                    || column.ColumnName.Equals("ServerUrl")
                    || column.ColumnName.Equals("FileRef")
                    || column.ColumnName.Equals("_ModerationStatus")
                        || column.ColumnName.Equals("Title"))
                        continue;

                    if (column.ColumnName == "Modified")
                        targetRow[column.ColumnName] = DateTime.Parse(row[column.ColumnName].ToString()).ToString("g", SPContext.Current.Web.Locale);
                    else
                        targetRow[column.ColumnName] = row[column.ColumnName];
                }

                SPForm displayForm = curList.Forms[PAGETYPE.PAGE_DISPLAYFORM];
                string url = projectWeb.Url + "/" + displayForm.Url;
                string webUrl = projectWeb.Url;
                targetRow["WebUrl"] = webUrl;
                targetRow["WebServerRelativeUrl"] = projectWeb.ServerRelativeUrl;
                targetRow["EditForm"] = curList.Forms[PAGETYPE.PAGE_EDITFORM].Url;
                targetRow["FileRef"] = url + "?ID=" + row["ID"].ToString() + "&Source=" + HttpUtility.UrlEncode(SPContext.Current.Site.MakeFullUrl(Page.Request.RawUrl));
                targetRow["IsDocLib"] = (curList is SPDocumentLibrary).ToString();
                targetRow["DisplayFormUrl"] = curList.Forms[PAGETYPE.PAGE_DISPLAYFORM].ServerRelativeUrl;
                targetRow["EditFormUrl"] = curList.Forms[PAGETYPE.PAGE_EDITFORM].ServerRelativeUrl;
                targetRow["DefaultViewID"] = defaultViewID;
                targetRow["BaseTemplate"] = (int)curList.BaseTemplate;
                targetRow["BaseType"] = (int)curList.BaseType;
                targetRow["ForceCheckout"] = curList.ForceCheckout;
                targetRow["EnableVersioning"] = Convert.ToInt32(curList.EnableVersioning);
                targetRow["EnableMinorVersions"] = Convert.ToInt32(curList.EnableMinorVersions);
                targetRow["HasWorkflows"] = curList.WorkflowAssociations.Count > 0;
                targetRow["RootFolderUrl"] = rootFolderPath;

                if (curList is SPDocumentLibrary)// item.File != null)
                    targetRow["DisplayUrl"] = projectWeb.Url + row["FileRef"].ToString();
                else
                    targetRow["DisplayUrl"] = url + "?ID=" + row["ID"].ToString();

                
                if (!(curList is SPDocumentLibrary))
                {
                    targetRow["LinkFilename"] = row["Title"];//item.Title;
                    targetRow["FileServerRelativeUrl"] = row["ServerUrl"]; //item.Url;
                }
                else
                {
                    targetRow["LinkFilename"] = row["FileLeafRef"];// item[SPBuiltInFieldId.LinkFilename].ToString();
                    targetRow["FileName"] = row["FileLeafRef"]; //item.File.Name;
                    targetRow["ProgID"] = row["ProgId"];// item.File.ProgID;
                    targetRow["FileServerRelativeUrl"] = row["ServerUrl"]; //item.File.ServerRelativeUrl;
                }

                targetRow["Editor"] = row["Editor"].ToString().Split('#')[1];
                resultsTable.Rows.Add(targetRow);
            }
        }

        private bool CanUserApprove(string p)
        {
            if (p.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                p = p.Remove(0, 2);
            Int64 i = Int64.Parse(p, System.Globalization.NumberStyles.AllowHexSpecifier);
            Int64 c = Int64.Parse("0000000000000010", System.Globalization.NumberStyles.AllowHexSpecifier);
            return ((i & c) == c);
        }

        /// <summary>
        /// Gets items using site data query
        /// </summary>
        /// <param name="baseType">Base type of lists to query</param>
        /// <returns></returns>
        private DataTable GetDataTable(int baseType)
        {
            Logger.EnterMethod(baseType);

            try
            {
                SPSiteDataQuery query = new SPSiteDataQuery();
                if (scope != Recursion.Current)
                    query.Webs = string.Format("<Webs Scope=\"{0}\" />", scope.ToString());
                query.Lists = string.Format("<Lists BaseType=\"{0}\" MaxListLimit=\"0\"/>", baseType);
                query.Query = "<Where><Neq><FieldRef Name=\"ContentTypeId\"/><Value Type=\"Text\">0x0120</Value></Neq></Where>";
                StringBuilder vf = new StringBuilder();
                vf.Append(@"<FieldRef Name=""ID""/><FieldRef Name=""Title"" Nullable=""TRUE""/><FieldRef Name=""Modified""/><FieldRef Name=""Editor""/><ListProperty Name=""Title"" /><ListProperty Name=""ListId"" /><ProjectProperty Name=""Title"" /><ProjectProperty Name=""WebId"" /><FieldRef Name=""ContentType""/><FieldRef Name=""ContentTypeId""/><FieldRef Name=""PermMask""/><FieldRef Name=""FSObjType""/><FieldRef Name=""ProgId"" Nullable=""TRUE""/>");
                vf.Append(@"<FieldRef Name=""FileLeafRef""/>");
                vf.Append(@"<FieldRef Name=""ServerUrl""/>");
                vf.Append(@"<FieldRef Name=""_ModerationStatus"" Nullable=""TRUE""/>");
                if (baseType == 1)
                    vf.Append("<FieldRef Name=\"FileRef\" Nullable=\"TRUE\"/>");

                query.ViewFields = vf.ToString();
                return SPContext.Current.Web.GetSiteData(query);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
        }
    }
}
