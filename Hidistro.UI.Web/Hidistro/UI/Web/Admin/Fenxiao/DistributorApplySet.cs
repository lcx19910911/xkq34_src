namespace Hidistro.UI.Web.Admin.Fenxiao
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.UI.Common.Controls;
    using Hidistro.UI.ControlPanel.Utility;
    using Hidistro.UI.Web.Admin.Ascx;
    using Hidistro.UI.Web.hieditor.ueditor.controls;
    using System;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class DistributorApplySet : AdminPage
    {
        protected Button btnSave;
        protected Button Button1;
        protected ucDateTimePicker calendarEndDate;
        protected ucDateTimePicker calendarStartDate;
        protected ucUeditor fckDescription;
        protected HtmlInputCheckBox HasConditions;
        protected HtmlInputCheckBox HasProduct;
        protected HtmlInputHidden hiddProductId;
        public string productHtml;
        protected HtmlInputCheckBox radioCommissionon;
        protected HtmlInputCheckBox radioDistributorApplicationCondition;
        protected HtmlInputCheckBox radiorequeston;
        protected Script Script4;
        protected Hidistro.UI.Common.Controls.Style Style1;
        public string tabnum;
        protected HtmlForm thisForm;
        protected HtmlInputText txtrequestmoney;

        protected DistributorApplySet() : base("m05", "fxp02")
        {
            this.tabnum = "0";
            this.productHtml = "";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            this.tabnum = "0";
            int result = 0;
            if ((this.radioDistributorApplicationCondition.Checked && !this.HasProduct.Checked) && !this.HasConditions.Checked)
            {
                this.ShowMsg("请选择分销商申请条件", false);
            }
            else if (this.HasConditions.Checked && (!int.TryParse(this.txtrequestmoney.Value.Trim(), out result) || (result < 1)))
            {
                this.ShowMsg("累计消费金额必须为大于0的整数金额", false);
            }
            else if (this.HasProduct.Checked && string.IsNullOrEmpty(this.hiddProductId.Value))
            {
                this.ShowMsg("请选择指定商品", false);
            }
            else
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                masterSettings.DistributorApplicationCondition = this.radioDistributorApplicationCondition.Checked;
                masterSettings.IsRequestDistributor = this.radiorequeston.Checked;
                masterSettings.EnableCommission = this.radioCommissionon.Checked;
                masterSettings.FinishedOrderMoney = result;
                masterSettings.EnableDistributorApplicationCondition = this.HasProduct.Checked;
                if (this.HasProduct.Checked)
                {
                    masterSettings.DistributorProducts = this.hiddProductId.Value;
                }
                else
                {
                    masterSettings.DistributorProducts = "";
                }
                string str = "";
                if (this.calendarStartDate.SelectedDate.HasValue)
                {
                    str = this.calendarStartDate.SelectedDate.Value.ToString();
                }
                str = str + "|";
                if (this.calendarEndDate.SelectedDate.HasValue)
                {
                    str = str + this.calendarEndDate.SelectedDate.Value.ToString();
                }
                masterSettings.DistributorProductsDate = str;
                SettingsManager.Save(masterSettings);
                HttpCookie cookie = HttpContext.Current.Request.Cookies["Admin-Product"];
                if ((cookie != null) && !string.IsNullOrEmpty(cookie.Value))
                {
                    cookie.Value = null;
                    cookie.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Set(cookie);
                }
                this.ShowMsgAndReUrl("修改成功", true, "DistributorApplySet.aspx");
            }
        }

        protected void btnSave_Description(object sender, EventArgs e)
        {
            this.tabnum = "1";
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            masterSettings.DistributorDescription = this.fckDescription.Text.Trim();
            SettingsManager.Save(masterSettings);
            this.ShowMsg("分销说明修改成功", true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.Page.IsPostBack)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                this.tabnum = base.Request.QueryString["tabnum"];
                if (string.IsNullOrEmpty(this.tabnum))
                {
                    this.tabnum = "0";
                }
                this.txtrequestmoney.Value = masterSettings.FinishedOrderMoney.ToString();
                this.fckDescription.Text = masterSettings.DistributorDescription;
                this.radioDistributorApplicationCondition.Checked = masterSettings.DistributorApplicationCondition;
                this.radiorequeston.Checked = true;
                if (!masterSettings.IsRequestDistributor)
                {
                    this.radiorequeston.Checked = false;
                }
                this.radioCommissionon.Checked = false;
                if (masterSettings.EnableCommission)
                {
                    this.radioCommissionon.Checked = true;
                }
                if (masterSettings.FinishedOrderMoney > 0)
                {
                    this.HasConditions.Checked = true;
                }
                this.HasProduct.Checked = masterSettings.EnableDistributorApplicationCondition;
                string distributorProducts = masterSettings.DistributorProducts;
                if (!string.IsNullOrEmpty(distributorProducts))
                {
                    this.hiddProductId.Value = distributorProducts;
                }
                if (!string.IsNullOrEmpty(masterSettings.DistributorProductsDate) && masterSettings.DistributorProductsDate.Contains("|"))
                {
                    if (!string.IsNullOrEmpty(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[0]))
                    {
                        this.calendarStartDate.SelectedDate = new DateTime?(Convert.ToDateTime(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[0]));
                    }
                    if (!string.IsNullOrEmpty(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[1]))
                    {
                        this.calendarEndDate.SelectedDate = new DateTime?(Convert.ToDateTime(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[1]));
                    }
                }
            }
        }
    }
}

