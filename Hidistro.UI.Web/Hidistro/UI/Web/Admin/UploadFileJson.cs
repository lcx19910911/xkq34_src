namespace Hidistro.UI.Web.Admin
{
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.UI.ControlPanel.Utility;
    using LitJson;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Web;

    public class UploadFileJson : AdminPage
    {
        private string savePath;
        private string saveUrl;

        protected UploadFileJson() : base("m01", "00000")
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ManagerHelper.GetCurrentManager() == null)
            {
                this.showError("您没有权限执行此操作！");
            }
            else
            {
                this.savePath = "~/Storage/master/gallery/";
                this.saveUrl = "/Storage/master/gallery/";
                int result = 0;
                if (base.Request.Form["fileCategory"] != null)
                {
                    int.TryParse(base.Request.Form["fileCategory"], out result);
                }
                string str = string.Empty;
                if (base.Request.Form["imgTitle"] != null)
                {
                    str = base.Request.Form["imgTitle"];
                }
                HttpPostedFile postedFile = base.Request.Files["imgFile"];
                if (postedFile == null)
                {
                    this.showError("请先选择文件！");
                }
                else if (!ResourcesHelper.CheckPostedFile(postedFile, "image"))
                {
                    this.showError("不能上传空文件，且必须是有效的图片文件！");
                }
                else
                {
                    string path = base.Server.MapPath(this.savePath);
                    if (!Directory.Exists(path))
                    {
                        this.showError("上传目录不存在。");
                    }
                    else
                    {
                        path = path + string.Format("{0}/", DateTime.Now.ToString("yyyyMM"));
                        this.saveUrl = this.saveUrl + string.Format("{0}/", DateTime.Now.ToString("yyyyMM"));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string fileName = postedFile.FileName;
                        if (str.Length == 0)
                        {
                            str = fileName;
                        }
                        string str4 = Path.GetExtension(fileName).ToLower();
                        string str5 = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + str4;
                        string filename = path + str5;
                        string str7 = this.saveUrl + str5;
                        try
                        {
                            postedFile.SaveAs(filename);
                            Database database = DatabaseFactory.CreateDatabase();
                            DbCommand sqlStringCommand = database.GetSqlStringCommand("insert into Hishop_PhotoGallery(CategoryId,PhotoName,PhotoPath,FileSize,UploadTime,LastUpdateTime)values(@cid,@name,@path,@size,@time,@time1)");
                            database.AddInParameter(sqlStringCommand, "cid", DbType.Int32, result);
                            database.AddInParameter(sqlStringCommand, "name", DbType.String, str);
                            database.AddInParameter(sqlStringCommand, "path", DbType.String, str7);
                            database.AddInParameter(sqlStringCommand, "size", DbType.Int32, postedFile.ContentLength);
                            database.AddInParameter(sqlStringCommand, "time", DbType.DateTime, DateTime.Now);
                            database.AddInParameter(sqlStringCommand, "time1", DbType.DateTime, DateTime.Now);
                            database.ExecuteNonQuery(sqlStringCommand);
                        }
                        catch
                        {
                            this.showError("保存文件出错！");
                        }
                        Hashtable hashtable = new Hashtable();
                        hashtable["error"] = 0;
                        hashtable["url"] = Globals.ApplicationPath + str7;
                        base.Response.AddHeader("Content-Type", "text/html; charset=UTF-8");
                        base.Response.Write(JsonMapper.ToJson(hashtable));
                        base.Response.End();
                    }
                }
            }
        }

        private void showError(string message)
        {
            Hashtable hashtable = new Hashtable();
            hashtable["error"] = 1;
            hashtable["message"] = message;
            base.Response.AddHeader("Content-Type", "text/html; charset=UTF-8");
            base.Response.Write(JsonMapper.ToJson(hashtable));
            base.Response.End();
        }
    }
}

