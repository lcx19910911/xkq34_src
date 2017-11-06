namespace Hidistro.UI.Web.Admin.Shop
{
    using System;
    using System.IO;
    using System.Web;

    public class Hi_Ajax_UploadImg : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Charset = "utf-8";
            HttpPostedFile file = context.Request.Files["Filedata"];
            string path = HttpContext.Current.Server.MapPath(context.Request["folder"]) + @"\";
            if (file != null)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                file.SaveAs(path + file.FileName);
                context.Response.Write("1");
            }
            else
            {
                context.Response.Write("0");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

