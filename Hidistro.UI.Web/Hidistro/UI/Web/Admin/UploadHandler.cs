namespace Hidistro.UI.Web.Admin
{
    using Hidistro.Core;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;

    public class UploadHandler : IHttpHandler
    {
        private void DeleteImage()
        {
            string path = HttpContext.Current.Request.Form["del"];
            string str2 = Globals.PhysicalPath(path);
            try
            {
                if (File.Exists(str2))
                {
                    File.Delete(str2);
                    HttpContext.Current.Response.Write("true");
                }
            }
            catch (Exception)
            {
                HttpContext.Current.Response.Write("false");
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            switch (context.Request["action"])
            {
                case "upload":
                    this.UploadImage();
                    return;

                case "delete":
                    this.DeleteImage();
                    return;
            }
            context.Response.Write("false");
        }

        private void UploadImage()
        {
            try
            {
                HttpPostedFile file = HttpContext.Current.Request.Files["Filedata"];
                string str = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo);
                string str2 = HttpContext.Current.Request["uploadpath"];
                string str3 = str + Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(str2))
                {
                    str2 = Globals.GetVshopSkinPath(null) + "/images/ad/";
                    str3 = "imgCustomBg" + Path.GetExtension(file.FileName);
                    foreach (string str4 in Directory.GetFiles(Globals.MapPath(str2), "imgCustomBg.*"))
                    {
                        File.Delete(str4);
                    }
                }
                if (!Directory.Exists(Globals.MapPath(str2)))
                {
                    Directory.CreateDirectory(Globals.MapPath(str2));
                }
                file.SaveAs(Globals.MapPath(str2 + str3));
                HttpContext.Current.Response.Write(str2 + str3);
            }
            catch (Exception)
            {
                HttpContext.Current.Response.Write("服务器错误");
                HttpContext.Current.Response.End();
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

