namespace Hidistro.UI.Web.Vshop
{
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities.Members;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using Hidistro.UI.SaleSystem.CodeBehind;
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    public class Default : Page
    {
        protected string AlinfollowUrl = "";
        public string cssSrc = "/Templates/vshop/";
        public string Desc = string.Empty;
        protected HomePage H_Page;
        protected string htmlTitleName = string.Empty;
        public string imgUrl = string.Empty;
        protected Hidistro.UI.Common.Controls.MeiQiaSet MeiQiaSet;
        public string ShowCopyRight = string.Empty;
        public bool showMenu;
        public string siteName = string.Empty;
        public SiteSettings siteSettings;
        protected WeixinSet weixin;
        protected string WeixinfollowUrl = "";

        public void BindWXInfo()
        {
            int currentDistributorId = Globals.GetCurrentDistributorId();
            if (currentDistributorId > 0)
            {
                DistributorsInfo distributorInfo = DistributorsBrower.GetDistributorInfo(currentDistributorId);
                if (distributorInfo != null)
                {
                    this.siteName = distributorInfo.StoreName;
                    this.imgUrl = "http://" + HttpContext.Current.Request.Url.Host + distributorInfo.Logo;
                    this.Desc = distributorInfo.StoreDescription;
                }
            }
            if (string.IsNullOrEmpty(this.siteName))
            {
                this.siteName = this.siteSettings.SiteName;
                this.imgUrl = "http://" + HttpContext.Current.Request.Url.Host + this.siteSettings.DistributorLogoPic;
                this.Desc = this.siteSettings.ShopIntroduction;
            }
            this.htmlTitleName = string.Format(CultureInfo.InvariantCulture, "{0} - {1}", new object[] { "店铺主页", this.siteName });
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.siteSettings = SettingsManager.GetMasterSettings(true);
            this.ShowCopyRight = this.siteSettings.ShowCopyRight;
            this.htmlTitleName = this.siteSettings.SiteName;
            string userAgent = this.Page.Request.UserAgent;
            if (this.siteSettings.EnableAliPayFuwuGuidePageSet)
            {
                this.AlinfollowUrl = this.siteSettings.AliPayFuwuGuidePageSet;
            }
            if (this.siteSettings.EnableGuidePageSet)
            {
                this.WeixinfollowUrl = this.siteSettings.GuidePageSet;
            }
            if (!base.IsPostBack)
            {
                HiAffiliation.LoadPage();
                string getCurrentWXOpenId = Globals.GetCurrentWXOpenId;
                int num = Globals.RequestQueryNum("go");
                if ((userAgent.ToLower().Contains("micromessenger") && string.IsNullOrEmpty(getCurrentWXOpenId)) && (this.siteSettings.IsValidationService && (num != 1)))
                {
                    this.Page.Response.Redirect("Follow.aspx?ReferralId=" + Globals.GetCurrentDistributorId());
                    this.Page.Response.End();
                }
                if (((Globals.GetCurrentMemberUserId() == 0) && this.siteSettings.IsAutoToLogin) && userAgent.ToLower().Contains("micromessenger"))
                {
                    Uri url = HttpContext.Current.Request.Url;
                    string urlToEncode = Globals.GetWebUrlStart() + "/default.aspx?ReferralId=" + Globals.RequestQueryNum("ReferralId").ToString();
                    base.Response.Redirect("/UserLogining.aspx?returnUrl=" + Globals.UrlEncode(urlToEncode));
                    base.Response.End();
                }
                this.showMenu = this.siteSettings.EnableShopMenu;
                this.cssSrc = this.cssSrc + this.siteSettings.VTheme + "/css/head.css";
                this.BindWXInfo();
            }
        }
    }
}

