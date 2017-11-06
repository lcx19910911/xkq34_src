namespace Hidistro.UI.Web.Admin.Settings
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.UI.Common.Controls;
    using Hidistro.UI.ControlPanel.Utility;
    using System;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class WeixinRed : AdminPage
    {
        private string _dataPath;
        protected bool _enable;
        protected FileUpload fileUploader;
        protected HtmlGenericControl labfilename;
        protected Script Script1;
        protected Script Script4;
        protected Script Script5;
        protected Script Script6;
        protected Script Script7;
        protected HtmlForm thisForm;
        protected TextBox txt_key;

        protected WeixinRed() : base("m06", "wxp09")
        {
            this._dataPath = "~/Pay/Cert";
        }

        private bool IsAllowableFileType(string FileName)
        {
            string str = ".p12";
            return (str.IndexOf(Path.GetExtension(FileName).ToLower()) != -1);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                this._enable = masterSettings.EnableWeixinRed;
                this.txt_key.Text = masterSettings.WeixinCertPassword;
                this.labfilename.InnerText = (masterSettings.WeixinCertPath != "") ? "已上传" : "";
            }
        }

        protected void Unnamed_Click(object sender, EventArgs e)
        {
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            string str = this.Page.Request.MapPath(this._dataPath);
            masterSettings.WeixinCertPassword = this.txt_key.Text;
            if (this.fileUploader.PostedFile.FileName != "")
            {
                if (!this.IsAllowableFileType(this.fileUploader.PostedFile.FileName))
                {
                    this.ShowMsg("请上传正确的文件", false);
                    return;
                }
                string str2 = DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetFileName(this.fileUploader.PostedFile.FileName);
                this.fileUploader.PostedFile.SaveAs(Path.Combine(str, str2));
                if (masterSettings.WeixinCertPath != "")
                {
                    File.Delete(masterSettings.WeixinCertPath);
                }
                masterSettings.WeixinCertPath = Path.Combine(str, str2);
            }
            else if (string.IsNullOrEmpty(masterSettings.WeixinCertPath))
            {
                this.ShowMsg("请上传正确的文件", false);
                return;
            }
            SettingsManager.Save(masterSettings);
            this.ShowMsg("设置成功", true);
        }
    }
}

