using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;
using System.Xml;
using System.IO;
using Infowise.Sharepoint.V3.Fields.Controls;
using Infowise.Sharepoint.V3.WebParts;
using System.Data;
using Infowise.Sharepoint.ApprovalSummaryWP;

namespace Infowise.Sharepoint.V3.Fields
{
    class MenuFieldTemplate : ITemplate
    {
        ListItemType type;
        string internalName;

        public MenuFieldTemplate(ListItemType type, string internalName)
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
                    Literal hl = new Literal();
                    hl.DataBinding += new EventHandler(hl_DataBinding);
                    container.Controls.Add(hl);
                    break;
            }

        #endregion
        }

        private ApprovalSummary Parent(Control ctrl)
        {
            if (ctrl.NamingContainer.NamingContainer.NamingContainer is ApprovalSummary)
                return (ApprovalSummary)ctrl.NamingContainer.NamingContainer.NamingContainer;

            return null;
        }

        void hl_DataBinding(object sender, EventArgs e)
        {

            Literal hl = (Literal)sender;
            SPGridViewRow gr = (SPGridViewRow)hl.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            #region Display and Edit URLs
            #region Display URL
            string displUrl, dispText;
            if (listItem[internalName] == null)
                dispText = Common.GetCoreString("NoTitle");
            else
                dispText = listItem[internalName].ToString();
            displUrl = listItem["DisplayUrl"].ToString();
            #endregion

            #region Edit URL
            string currentUserID = SPContext.Current.Web.CurrentUser == null ? "-1" : SPContext.Current.Web.CurrentUser.ID.ToString();
            string editItemUrlPrefix = listItem["WebUrl"].ToString() + "/" + listItem["EditForm"].ToString() + "?ID=";
            #endregion
            #endregion

            if (dispText != null)
            {
                string listItemPermission = listItem["PermMask"].ToString();
                string relPath = listItem["RootFolderUrl"].ToString();

                if (bool.Parse(listItem["IsDocLib"].ToString()))
                #region Document library
                {
                    string extension = Path.GetExtension(listItem["FileName"].ToString());
                    if (extension.StartsWith("."))
                        extension = extension.Remove(0, 1);
                    string progID = listItem["ProgID"].ToString();
                    string iconName, editText, openControl;
                    iconName = IconProvider.GetIcon(progID, extension, out editText, out openControl);

                    if (Common.IsSharePoint2010)
                        hl.Text = string.Format(@"<DIV id=""{4}"" class=""ms-vb itx"" onmouseover=""OnItem(this)"" CTXName=""ctx{7}"" EventType="""" Perm=""{5}"" Field=""{12}""><A onfocus=""OnLink(this)"" onmousedown=""return VerifyHref(this,event,'1','SharePoint.OpenDocuments','')"" onclick=""return DispEx(this,event,'TRUE','FALSE','FALSE','SharePoint.OpenDocuments.3','1','SharePoint.OpenDocuments','','','','1','0','0','{5}','','')"" href=""{2}"">{3}</A></DIV>
<DIV style=""LINE-HEIGHT: 22px; MARGIN: 0px; HEIGHT: 22px; TOP: 35px; LEFT: 711px"" class=""s4-ctx"" onmouseover=""OnChildItem(this.parentNode); return false;"" shown=""false""><SPAN>&nbsp;</SPAN><A onfocus=""OnChildItem(this.parentNode.parentNode); return false;"" onclick=""PopMenuFromChevron(event); return false;"" href=""javascript:;""><IMG style=""VISIBILITY: hidden"" alt="""" src=""/_layouts/images/ecbarw.png"" width=""7"" height=""4""></A><SPAN>&nbsp;</SPAN></DIV>",
                                            listItem["ContentTypeId"],
                                listItem["ContentType"],
                                listItem["FileServerRelativeUrl"],
                                dispText,
                                listItem["ID"],
                                listItemPermission,
                                listItem["FileServerRelativeUrl"],
                                ucSearchResults.GetCtxId(listItem["ListId"].ToString()),
                                extension,
                                iconName,
                                editText,
                                openControl,
                                internalName);
                    else
                        hl.Text = string.Format(@"<TABLE Id=""{4}"" class=""ms-unselectedtitle"" onmouseover=""OnItem(this)"" cellSpacing=""0"" height=""100%"" SUrl="""" UIS=""512"" CId=""{0}"" CType=""{1}"" MS=""0"" CSrc="""" HCD="""" COUId="""" OType=""0"" Icon=""{9}|{10}|{11}"" Ext=""{8}"" Type="""" Perm=""{5}"" DRef=""" + relPath + @""" Url=""{6}"" CTXName=""ctx{7}"">
<TBODY>
<TR>
<TD class=ms-vb width=""100%""><A onfocus=""OnLink(this)"" onclick=""return DispEx(this,event,'TRUE','FALSE','FALSE','SharePoint.OpenDocuments.3','0','SharePoint.OpenDocuments','','','','1','0','0','{5}')"" href=""{2}"">{3}<IMG class=ms-hidden border=0 alt="""" src=""/_layouts/images/blank.gif"" width=1 height=1></A></TD>
<TD><IMG style=""VISIBILITY: hidden"" alt=" + Common.GetWssString("multipages_edit") + @" src=""/_layouts/images/menudark.gif"" width=13></TD></TR></TBODY></TABLE>",
                              listItem["ContentTypeId"], listItem["ContentType"], listItem["FileServerRelativeUrl"], dispText, listItem["ID"], listItemPermission, listItem["FileServerRelativeUrl"], ucSearchResults.GetCtxId(listItem["ListID"].ToString()), extension, iconName, editText, openControl);
                } 
                #endregion
                else
                {
                    if (Common.IsSharePoint2010)
                    {
                        hl.Text = string.Format(@"<DIV id=""{4}"" class=""ms-vb itx"" onmouseover=""OnItem(this)"" CTXName=""ctx{7}"" Field=""{8}""><A onfocus=OnLink(this) onclick=""EditLink2(this,'{7}');return false;"" href=""{2}"" target=""_self"">{3}</A></DIV>
<DIV class=""s4-ctx"" onmouseover=""OnChildItem(this.parentNode); return false;""><SPAN>&nbsp;</SPAN><A onfocus=""OnChildItem(this.parentNode.parentNode); return false;"" onclick=""PopMenuFromChevron(event); return false;"" href=""javascript:;""></A><SPAN>&nbsp;</SPAN></DIV>",
                          listItem["ContentTypeId"], listItem["ContentType"], displUrl, dispText, listItem["ID"], listItemPermission, listItem["FileServerRelativeUrl"], ucSearchResults.GetCtxId(listItem["ListID"].ToString()), internalName);
                    }
                    else
                    {
                        hl.Text = string.Format(@"<TABLE class=""ms-unselectedtitle"" onmouseover=""OnItem(this)"" cellSpacing=""0"" height=""100%"" Id=""{4}"" SUrl="""" UIS=""512"" CId=""{0}"" CType=""{1}"" MS=""0"" CSrc="""" HCD="""" COUId="""" OType=""0"" Icon=""icgen.gif||"" Ext="""" Type="""" Perm=""{5}"" DRef=""" + relPath + @""" Url=""{6}"" CTXName=""ctx{7}"">
<TBODY>
<TR>
<TD class=ms-vb width=""100%""><A onfocus=""OnLink(this)"" onclick=""GoToLink(this);return false;"" href=""{2}"" target=""_self"">{3}<IMG class=""ms-hidden"" border=""0"" alt="""" src=""/_layouts/images/blank.gif"" width=1 height=1></A></TD>
<TD><IMG style=""VISIBILITY: hidden"" alt=" + Common.GetWssString("multipages_edit") + @" src=""/_layouts/images/menudark.gif"" width=13></TD></TR></TBODY></TABLE>",
                                                  listItem["ContentTypeId"], listItem["ContentType"], displUrl, dispText, listItem["ID"], listItemPermission, listItem["FileServerRelativeUrl"], ucSearchResults.GetCtxId(listItem["ListID"].ToString()));
                    }
                }
            }

        }
    }
}
