using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infowise.Sharepoint.V3.Fields;
using Microsoft.SharePoint;

namespace Infowise.Sharepoint.V3.WebParts
{
    static class ApproveHelper
    {

        internal static void Approve(string p)
        {
            Logger.EnterMethod(p);

            string[] items = p.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in items)
            {
                string[] parts = item.Split('|');
                Guid webID = new Guid(parts[0]);
                Guid listID = new Guid(parts[1]);
                int itemID = int.Parse(parts[2]);
                string approval = parts[3];

                string comments = null;
                if (parts.Length > 4)
                    comments = parts[4].Replace("<!IW:SC>",";");

                Approve(webID, listID, itemID, approval, comments);
            }
        }

        internal static void Approve(Guid webID, Guid listID, int itemID, string approval, string comments)
        {
            Logger.EnterMethod(webID, listID, itemID, approval, comments);

            if (webID.Equals(SPContext.Current.Web.ID))
                Approve(SPContext.Current.Web, listID, itemID, approval, comments);
            else
            {
                using (SPWeb web = SPContext.Current.Site.OpenWeb(webID))
                {
                    Approve(web, listID, itemID, approval, comments);
                }
            }
        }

        private static void Approve(SPWeb sPWeb, Guid listID, int itemID, string approval, string comments)
        {
            SPList list = sPWeb.Lists[listID];
            if(!list.EnableModeration)
                return;

            SPListItem item = list.GetItemById(itemID);
            var status = GetModerationInfo(approval);
            if (status != item.ModerationInformation.Status || !string.IsNullOrEmpty(comments))
            {
                item.ModerationInformation.Status = status;
                item.ModerationInformation.Comment = comments;
                item.Update();
            }
        }

        private static SPModerationStatusType GetModerationInfo(string approval)
        {
            return (SPModerationStatusType)Enum.Parse(typeof(SPModerationStatusType), approval);
        }
    }
}
