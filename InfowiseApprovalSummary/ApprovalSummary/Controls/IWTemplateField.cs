using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Infowise.Sharepoint.V3.WebParts;

namespace Infowise.Sharepoint.V3.Fields
{
    public class IWTemplateField : TemplateField
    {
        public string DataField { get; set; }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            if (cellType == DataControlCellType.Header && Common.IsSharePoint2010)
            {
                cell.Text = string.Format(@"<DIV class=""ms-vh-div"" name=""{0}""><A href=""javascript:__doPostBack('{2}','Sort${1}')"">{0}</A><IMG border=""0"" alt="""" src=""/_layouts/images/blank.gif""><IMG border=""0"" alt="""" src=""/_layouts/images/blank.gif""></DIV>",
                    HeaderText, DataField, Control.UniqueID);
            }
            else
                base.InitializeCell(cell, cellType, rowState, rowIndex);
        }

        protected override DataControlField CreateField()
        {
            return new IWTemplateField();
        }
    }
}
