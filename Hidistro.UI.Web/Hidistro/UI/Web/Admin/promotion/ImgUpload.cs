namespace Hidistro.UI.Web.Admin.promotion
{
    using Hidistro.Core;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    public class ImgUpload : Page
    {
        protected HtmlForm form1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (base.Request.QueryString["delimg"] != null)
            {
                string path = base.Server.HtmlEncode(base.Request.QueryString["delimg"]);
                path = base.Server.MapPath(path);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                base.Response.Write("0");
                base.Response.End();
            }
            int num = int.Parse(base.Request.QueryString["imgurl"]);
            string str2 = base.Request.QueryString["oldurl"].ToString();
            try
            {
                if (num < 1)
                {
                    HttpPostedFile file = base.Request.Files["Filedata"];
                    string str3 = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo);
                    string str4 = "/Storage/master/topic/";
                    if (!Directory.Exists(base.Server.MapPath(str4)))
                    {
                        Directory.CreateDirectory(base.Server.MapPath(str4));
                    }
                    string str5 = str3 + Path.GetExtension(file.FileName);
                    file.SaveAs(Globals.MapPath(str4 + str5));
                    if (!string.IsNullOrEmpty(str2))
                    {
                        string str6 = base.Server.MapPath(str2);
                        if (File.Exists(str6))
                        {
                            File.Delete(str6);
                        }
                    }
                    base.Response.StatusCode = 200;
                    base.Response.Write(str3 + "|/Storage/master/topic/" + str5);
                }
                else
                {
                    base.Response.Write("0");
                }
            }
            catch (Exception)
            {
                base.Response.StatusCode = 500;
                base.Response.Write("服务器错误");
                base.Response.End();
            }
            finally
            {
                base.Response.End();
            }
        }
    }
}

