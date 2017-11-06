namespace Hidistro.UI.Web.Admin
{
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.UI.ControlPanel.Utility;
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI.WebControls;

    public class ImageReplace : AdminPage
    {
        protected Button btnSaveImageData;
        protected FileUpload FileUpload1;
        protected HiddenField RePlaceId;
        protected HiddenField RePlaceImg;

        protected ImageReplace() : base("m01", "00000")
        {
        }

        protected void btnSaveImageData_Click(object sender, EventArgs e)
        {
            string str = this.RePlaceImg.Value;
            int photoId = Convert.ToInt32(this.RePlaceId.Value);
            string photoPath = GalleryHelper.GetPhotoPath(photoId);
            string str3 = photoPath.Substring(photoPath.LastIndexOf("."));
            string extension = string.Empty;
            string str5 = string.Empty;
            try
            {
                HttpPostedFile postedFile = base.Request.Files[0];
                extension = Path.GetExtension(postedFile.FileName);
                if (str3 != extension)
                {
                    this.ShowMsgToTarget("上传图片类型与原文件类型不一致！", false, "parent");
                }
                else
                {
                    string str6 = Globals.GetStoragePath() + "/gallery";
                    str5 = photoPath.Substring(photoPath.LastIndexOf("/") + 1);
                    string str7 = str.Substring(str.LastIndexOf("/") - 6, 6);
                    string virtualPath = string.Empty;
                    if (str7.ToLower().Contains("weibo"))
                    {
                        virtualPath = Globals.GetStoragePath() + "/weibo/";
                    }
                    else
                    {
                        virtualPath = str6 + "/" + str7 + "/";
                    }
                    int contentLength = postedFile.ContentLength;
                    string path = base.Request.MapPath(virtualPath);
                    string text1 = str7 + "/" + str5;
                    DirectoryInfo info = new DirectoryInfo(path);
                    if (!info.Exists)
                    {
                        info.Create();
                    }
                    if (!ResourcesHelper.CheckPostedFile(postedFile, "image"))
                    {
                        this.ShowMsgToTarget("文件上传的类型不正确！", false, "parent");
                    }
                    else if (contentLength >= 0x1f4000)
                    {
                        this.ShowMsgToTarget("图片文件已超过网站限制大小！", false, "parent");
                    }
                    else
                    {
                        postedFile.SaveAs(base.Request.MapPath(virtualPath + str5));
                        GalleryHelper.ReplacePhoto(photoId, contentLength);
                        this.CloseWindow();
                    }
                }
            }
            catch
            {
                this.ShowMsgToTarget("替换文件错误!", false, "parent");
            }
        }

        protected virtual void CloseWindow()
        {
            string str = Globals.RequestQueryStr("reurl");
            if (string.IsNullOrEmpty(str))
            {
                str = "imagedata.aspx";
            }
            string str2 = "parent.location.href='" + str + "'";
            if (!this.Page.ClientScript.IsClientScriptBlockRegistered("ServerMessageScript"))
            {
                this.Page.ClientScript.RegisterStartupScript(base.GetType(), "ServerMessageScript", "<script language='JavaScript' defer='defer'>" + str2 + "</script>");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if ((!base.IsPostBack && !string.IsNullOrEmpty(this.Page.Request.QueryString["imgsrc"])) && !string.IsNullOrEmpty(this.Page.Request.QueryString["imgId"]))
            {
                string str = Globals.HtmlDecode(this.Page.Request.QueryString["imgsrc"]);
                string str2 = Globals.HtmlDecode(this.Page.Request.QueryString["imgId"]);
                this.RePlaceImg.Value = str;
                this.RePlaceId.Value = str2;
            }
            this.btnSaveImageData.Click += new EventHandler(this.btnSaveImageData_Click);
        }
    }
}

