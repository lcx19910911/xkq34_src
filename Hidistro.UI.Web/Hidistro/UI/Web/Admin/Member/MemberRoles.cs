namespace Hidistro.UI.Web.Admin.Member
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.UI.ControlPanel.Utility;
    using Hidistro.UI.Web.hieditor.ueditor.controls;
    using System;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class MemberRoles : AdminPage
    {
        protected Button btnSaveImageFtp;
        protected ucUeditor fkContent;
        protected HtmlForm thisForm;

        protected MemberRoles() : base("m04", "hyp04")
        {
        }

        protected void btnSaveImageFtp_Click(object sender, EventArgs e)
        {
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            masterSettings.MemberRoleContent = this.fkContent.Text;
            SettingsManager.Save(masterSettings);
            this.ShowMsg("保存成功！", true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.btnSaveImageFtp.Click += new EventHandler(this.btnSaveImageFtp_Click);
            if (!base.IsPostBack)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                this.fkContent.Text = masterSettings.MemberRoleContent;
            }
        }
    }
}

