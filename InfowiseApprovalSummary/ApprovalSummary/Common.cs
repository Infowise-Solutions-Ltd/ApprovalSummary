using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Web;
using Microsoft.SharePoint.Administration;

namespace Infowise.Sharepoint.V3.WebParts
{
    static class Common
    {
        public static readonly SPContentTypeId DocSetCTID = new SPContentTypeId("0x0120D520");
        public static string GetString(string key)
        {
            return SPUtility.GetLocalizedString("$Resources:" + key, "Infowise.ApprovalSummary", SPContext.Current.Web.Language);
        }

        public static string GetCoreString(string key)
        {
            return SPUtility.GetLocalizedString("$Resources:" + key, "core", SPContext.Current.Web.Language);
        }

        public static string GetWssString(string key)
        {
            return HttpContext.GetGlobalResourceObject("wss", key).ToString();
        }

        public static bool IsSharePoint2010
        {
            get
            {
                return SPFarm.Local.BuildVersion.Major != 12;
            }
        }
    }
}
