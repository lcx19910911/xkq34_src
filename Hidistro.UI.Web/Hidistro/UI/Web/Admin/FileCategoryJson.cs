namespace Hidistro.UI.Web.Admin
{
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.UI.ControlPanel.Utility;
    using LitJson;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class FileCategoryJson : AdminPage
    {
        protected FileCategoryJson() : base("m01", "00000")
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Hashtable hashtable = new Hashtable();
            base.Response.AddHeader("Content-Type", "application/json; charset=UTF-8");
            if (Globals.GetCurrentManagerUserId() == 0)
            {
                base.Response.Write(JsonMapper.ToJson(hashtable));
                base.Response.End();
            }
            else
            {
                List<Hashtable> list = new List<Hashtable>();
                hashtable["category_list"] = list;
                foreach (DataRow row in GalleryHelper.GetPhotoCategories(0).Rows)
                {
                    Hashtable item = new Hashtable();
                    item["cId"] = row["CategoryId"];
                    item["cName"] = row["CategoryName"];
                    list.Add(item);
                }
                base.Response.Write(JsonMapper.ToJson(hashtable));
                base.Response.End();
            }
        }
    }
}

