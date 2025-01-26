using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;

namespace Infowise.Sharepoint.V3.Fields
{
    class UrlControl:UserControl
    {
        string urlValue = null;
        public string UrlValue
        {
            get
            {
                if (urlValue == null)
                    urlValue = (string)ViewState["UrlValue"];
                return urlValue;
            }
            set
            {
                urlValue = value;
                ViewState["UrlValue"] = value;
            }
        }

        protected override void CreateChildControls()
        {
            HyperLink hl = new HyperLink();
            Controls.Add(hl);
            if (UrlValue != null)
            {
                SPFieldUrlValue url = new SPFieldUrlValue(UrlValue);
                hl.NavigateUrl = url.Url;
                hl.Text = url.Description;
            }
        }
        protected override void Render(HtmlTextWriter writer)
        {
            RenderChildren(writer);
        }
    }
}
