using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;
using Infowise.Sharepoint.V3.Fields.Controls;
using System.IO;
using System.Data;
using Infowise.Sharepoint.V3.WebParts;

namespace Infowise.Sharepoint.V3.Fields
{
    class IconFieldTemplate : ITemplate
    {
        ListItemType type;
        string internalName;
        Image img;
        bool allowVizit;
        Literal ltrVz = null;

        public IconFieldTemplate(ListItemType type, string internalName, bool allowVizit)
        {
            this.type = type;
            this.internalName = internalName;
            this.allowVizit = allowVizit;
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
                    if (allowVizit)
                    {
                        Panel pnlVizit = new Panel();
                        pnlVizit.CssClass = "vizit-buttons";
                        container.Controls.Add(pnlVizit);

                        Panel pnlPre = new Panel();
                        pnlPre.Style.Add("float", SPContext.Current.Web.RegionalSettings.IsRightToLeft ? "right" : "left");
                        pnlVizit.Controls.Add(pnlPre);

                        ltrVz = new Literal();
                        pnlPre.Controls.Add(ltrVz);

                        Panel pnlIcon = new Panel();
                        pnlVizit.Controls.Add(pnlIcon);
                        pnlIcon.Style.Add("float", SPContext.Current.Web.RegionalSettings.IsRightToLeft ? "left" : "right");
                        pnlIcon.Style.Add("position", "relative");
                        pnlIcon.Controls.Add(uc);
                        pnlIcon.Controls.Add(img);
                    }
                    else
                    {
                        container.Controls.Add(uc);
                        container.Controls.Add(img);
                    }
                    break;
            }
        }


        #endregion

        void uc_DataBinding(object sender, EventArgs e)
        {
            Label uc = (Label)sender;
            SPGridViewRow gr = (SPGridViewRow)uc.NamingContainer;
            DataRowView item = (DataRowView)gr.DataItem;
            DataRow listItem = item.Row;

            string iconName = "icgen.gif";
            var ctID = new SPContentTypeId(listItem["ContentTypeId"].ToString());

            if (ctID.IsChildOf(SPBuiltInContentTypeId.Folder))
                iconName = "folder.gif";

            if (bool.Parse(listItem["IsDocLib"].ToString()))
            {
                if (ctID.IsChildOf(Common.DocSetCTID))
                    iconName = "icdocset.gif";
                else
                {
                    string extension = Path.GetExtension(listItem["FileName"].ToString());
                    if (extension.StartsWith("."))
                        extension = extension.Remove(0, 1);
                    string progID = listItem["ProgID"].ToString();
                    string editText, openControl;
                    iconName = IconProvider.GetIcon(progID, extension, out editText, out openControl);
                }
            }

            img.ImageUrl = string.Format("/_layouts/images/{0}", iconName);
            if (ltrVz != null)
                ltrVz.Text = string.Format(@"<vizit-buttons List=""{0}"" Item=""{1}"" ContentTypeId=""{2}"" IsFolder=""{3}""></vizit-buttons>
            <script type=""text/javascript"">
              (function() {{
              if (!window.Vizit) return;
              var docRef = {{""list"":""{0}"",""item"":""{1}""}};
              Vizit.Write(docRef, {{ spacer: false, contentTypeId: '{2}' }});
              }})();
            </script>", listItem["ListId"], listItem["ID"], listItem["ContentTypeId"], ctID.IsChildOf(SPBuiltInContentTypeId.Folder) ? "1" : "0");
        }
    }
}
