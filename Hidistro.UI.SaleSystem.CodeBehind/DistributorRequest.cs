namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.ControlPanel.Sales;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using System;
    using System.Data;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    [ParseChildren(true)]
    public class DistributorRequest : VMemberTemplatedWebControl
    {
        private HtmlImage idImg;
        private HtmlInputHidden litIsEnable;

        protected override void AttachChildControls()
        {
            PageTitle.AddSiteNameTitle("申请分销");
            this.Page.Session["stylestatus"] = "2";
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (string.IsNullOrEmpty(currentMember.UserBindName))
            {
                this.Page.Response.Redirect("/BindUserMessage.aspx?returnUrl=/Vshop/DistributorValid.aspx", true);
                this.Page.Response.End();
            }
            DistributorsInfo userIdDistributors = DistributorsBrower.GetUserIdDistributors(currentMember.UserId);
            if ((userIdDistributors != null) && (userIdDistributors.ReferralStatus == 0))
            {
                this.Page.Response.Redirect("/Vshop/DistributorCenter.aspx", true);
                this.Page.Response.End();
            }
            bool flag = false;
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
            if (masterSettings.DistributorApplicationCondition)
            {
                decimal expenditure = currentMember.Expenditure;
                int finishedOrderMoney = masterSettings.FinishedOrderMoney;
                if (finishedOrderMoney > 0)
                {
                    decimal num3 = 0M;
                    DataTable userOrderPaidWaitFinish = OrderHelper.GetUserOrderPaidWaitFinish(currentMember.UserId);
                    decimal total = 0M;
                    OrderInfo orderInfo = null;
                    for (int i = 0; i < userOrderPaidWaitFinish.Rows.Count; i++)
                    {
                        orderInfo = OrderHelper.GetOrderInfo(userOrderPaidWaitFinish.Rows[i]["orderid"].ToString());
                        if (orderInfo != null)
                        {
                            total = orderInfo.GetTotal();
                            if (total > 0M)
                            {
                                num3 += total;
                            }
                        }
                    }
                    if ((currentMember.Expenditure + num3) >= finishedOrderMoney)
                    {
                        flag = true;
                    }
                }
                if ((!flag && masterSettings.EnableDistributorApplicationCondition) && ((!string.IsNullOrEmpty(masterSettings.DistributorProductsDate) && !string.IsNullOrEmpty(masterSettings.DistributorProducts)) && masterSettings.DistributorProductsDate.Contains("|")))
                {
                    DateTime time = new DateTime();
                    DateTime time2 = new DateTime();
                    bool flag2 = DateTime.TryParse(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[0].ToString(), out time);
                    bool flag3 = DateTime.TryParse(masterSettings.DistributorProductsDate.Split(new char[] { '|' })[1].ToString(), out time2);
                    if ((((flag2 && flag3) && (DateTime.Now.CompareTo(time) >= 0)) && (DateTime.Now.CompareTo(time2) < 0)) && MemberProcessor.CheckMemberIsBuyProds(currentMember.UserId, masterSettings.DistributorProducts, new DateTime?(time), new DateTime?(time2)))
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (!flag)
            {
                this.Page.Response.Redirect("/Vshop/DistributorRegCheck.aspx", true);
                this.Page.Response.End();
            }
            int result = 0;
            this.idImg = (HtmlImage) this.FindControl("idImg");
            string logo = string.Empty;
            if (int.TryParse(this.Page.Request.QueryString["ReferralId"], out result) && (result > 0))
            {
                DistributorsInfo info4 = DistributorsBrower.GetUserIdDistributors(result);
                if ((info4 != null) && !string.IsNullOrEmpty(info4.Logo))
                {
                    logo = info4.Logo;
                }
            }
            if (string.IsNullOrEmpty(logo))
            {
                logo = masterSettings.DistributorLogoPic;
            }
            this.idImg.Src = logo;
            if ((userIdDistributors != null) && (userIdDistributors.ReferralStatus != 0))
            {
                this.litIsEnable = (HtmlInputHidden) this.FindControl("litIsEnable");
                this.litIsEnable.Value = userIdDistributors.ReferralStatus.ToString();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VDistributorRequest.html";
            }
            base.OnInit(e);
        }
    }
}

