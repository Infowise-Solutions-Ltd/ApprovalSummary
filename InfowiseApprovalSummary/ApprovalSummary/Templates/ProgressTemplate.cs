using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Infowise.Sharepoint.V3.Fields
{
    class ProgressTemplate :ITemplate
    {
        ClientScriptManager csm = null;
        bool absolute;
        public ProgressTemplate(ClientScriptManager csm, bool absolute = true)
        {
            this.csm = csm;
            this.absolute = absolute;

        }

        #region ITemplate Members


        public void InstantiateIn(Control container)
        {
            Logger.EnterMethod(container);

            Panel pnl = new Panel();
            container.Controls.Add(pnl);
            pnl.Style.Add("background-color", "#fff");
            pnl.Style.Add("border", "1px solid #f0f0f0");
            pnl.Style.Add("padding", "20px");

            if (absolute)
            {
                pnl.Style.Add("position", "absolute");
                pnl.Style.Add("top", "5px");
                pnl.Style.Add("left", "5px");
            }

            Image img = new Image();
            img.ImageUrl = csm.GetWebResourceUrl(GetType(), "Infowise.Sharepoint.V3.Webparts.Resources.kpiprogressbar.gif");
            pnl.Controls.Add(img);
        }

        #endregion
    }



}
