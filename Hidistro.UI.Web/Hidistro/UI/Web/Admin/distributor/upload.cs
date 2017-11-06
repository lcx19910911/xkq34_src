namespace Hidistro.UI.Web.Admin.distributor
{
    using Hidistro.Core;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    public class upload : Page
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
            Image image = null;
            Image image2 = null;
            Bitmap bitmap = null;
            Graphics graphics = null;
            MemoryStream stream = null;
            int num = int.Parse(base.Request.QueryString["imgurl"]);
            try
            {
                if (num < 9)
                {
                    HttpPostedFile file = base.Request.Files["Filedata"];
                    string str2 = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo);
                    string str3 = "/Storage/data/FriendExtension/";
                    string str4 = str2 + Path.GetExtension(file.FileName);
                    file.SaveAs(Globals.MapPath(str3 + str4));
                    base.Response.StatusCode = 200;
                    base.Response.Write(str2 + "|/Storage/data/FriendExtension/" + str4);
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
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                if (graphics != null)
                {
                    graphics.Dispose();
                }
                if (image2 != null)
                {
                    image2.Dispose();
                }
                if (image != null)
                {
                    image.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                }
                base.Response.End();
            }
        }
    }
}

