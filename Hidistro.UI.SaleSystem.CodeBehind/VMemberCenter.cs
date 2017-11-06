namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.ControlPanel.Members;
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    [ParseChildren(true)]
    public class VMemberCenter : VMemberTemplatedWebControl
    {
        private Image image;
        private HtmlInputHidden IsSign;
        private Literal litBindUser;
        private Literal litExpenditure;
        private Literal litPoints;
        private Literal litrGradeName;
        private Literal litUserName;
        private HtmlInputHidden txtShowDis;
        private HtmlInputHidden txtWaitForstr;
        private HtmlContainerControl UpClassInfo;
        private HtmlInputHidden UserBindName;

        protected override void AttachChildControls()
        {
            PageTitle.AddSiteNameTitle("会员中心");
            MemberInfo currentMemberInfo = base.CurrentMemberInfo;
            if (currentMemberInfo == null)
            {
                this.Page.Response.Redirect("/logout.aspx");
            }
            else
            {
                int currentMemberUserId = Globals.GetCurrentMemberUserId();
                this.UserBindName = (HtmlInputHidden) this.FindControl("UserBindName");
                this.UserBindName.Value = currentMemberInfo.UserBindName;
                this.UpClassInfo = (HtmlContainerControl) this.FindControl("UpClassInfo");
                this.litUserName = (Literal) this.FindControl("litUserName");
                this.litPoints = (Literal) this.FindControl("litPoints");
                this.litPoints.Text = currentMemberInfo.Points.ToString();
                this.image = (Image) this.FindControl("image");
                this.litBindUser = (Literal) this.FindControl("litBindUser");
                this.litExpenditure = (Literal) this.FindControl("litExpenditure");
                this.litExpenditure.SetWhenIsNotNull("￥" + currentMemberInfo.Expenditure.ToString("F2"));
                if (!string.IsNullOrEmpty(currentMemberInfo.UserBindName))
                {
                    this.litBindUser.Text = " style=\"display:none\"";
                }
                MemberGradeInfo memberGrade = MemberProcessor.GetMemberGrade(currentMemberInfo.GradeId);
                this.litrGradeName = (Literal) this.FindControl("litrGradeName");
                if (memberGrade != null)
                {
                    this.litrGradeName.Text = memberGrade.Name;
                }
                else
                {
                    this.litrGradeName.Text = "普通会员";
                }
                this.litUserName.Text = string.IsNullOrEmpty(currentMemberInfo.RealName) ? currentMemberInfo.UserName : currentMemberInfo.RealName;
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                this.IsSign = (HtmlInputHidden) this.FindControl("IsSign");
                if (!masterSettings.sign_score_Enable)
                {
                    this.IsSign.Value = "-1";
                }
                else if (!UserSignHelper.IsSign(currentMemberInfo.UserId))
                {
                    this.IsSign.Value = "1";
                }
                if (!string.IsNullOrEmpty(currentMemberInfo.UserHead))
                {
                    this.image.ImageUrl = currentMemberInfo.UserHead;
                }
                this.txtWaitForstr = (HtmlInputHidden) this.FindControl("txtWaitForstr");
                OrderQuery query = new OrderQuery {
                    Status = OrderStatus.WaitBuyerPay
                };
                int userOrderCount = MemberProcessor.GetUserOrderCount(currentMemberUserId, query);
                query.Status = OrderStatus.SellerAlreadySent;
                int num3 = MemberProcessor.GetUserOrderCount(currentMemberUserId, query);
                query.Status = OrderStatus.BuyerAlreadyPaid;
                int num4 = MemberProcessor.GetUserOrderCount(currentMemberUserId, query);
                int waitCommentByUserID = ProductBrowser.GetWaitCommentByUserID(currentMemberUserId);
                int userOrderReturnCount = MemberProcessor.GetUserOrderReturnCount(currentMemberUserId);
                this.txtWaitForstr.Value = userOrderCount.ToString() + "|" + num4.ToString() + "|" + num3.ToString() + "|" + waitCommentByUserID.ToString() + "|" + userOrderReturnCount.ToString();
                DistributorsInfo userIdDistributors = DistributorsBrower.GetUserIdDistributors(currentMemberUserId);
                this.txtShowDis = (HtmlInputHidden) this.FindControl("txtShowDis");
                if ((userIdDistributors == null) || (userIdDistributors.ReferralStatus != 0))
                {
                    this.txtShowDis.Value = "false";
                }
                else
                {
                    this.txtShowDis.Value = "true";
                }
                IList<MemberGradeInfo> memberGrades = MemberHelper.GetMemberGrades();
                MemberGradeInfo info4 = null;
                foreach (MemberGradeInfo info5 in memberGrades)
                {
                    int? nullable3;
                    int? nullable4;
                    double? tranVol = memberGrade.TranVol;
                    double? nullable2 = info5.TranVol;
                    if (((tranVol.GetValueOrDefault() < nullable2.GetValueOrDefault()) || !(tranVol.HasValue & nullable2.HasValue)) || (((nullable3 = memberGrade.TranTimes).GetValueOrDefault() < (nullable4 = info5.TranTimes).GetValueOrDefault()) || !(nullable3.HasValue & nullable4.HasValue)))
                    {
                        tranVol = memberGrade.TranVol;
                        nullable2 = info5.TranVol;
                        if (((tranVol.GetValueOrDefault() < nullable2.GetValueOrDefault()) && (tranVol.HasValue & nullable2.HasValue)) || (((nullable3 = memberGrade.TranTimes).GetValueOrDefault() < (nullable4 = info5.TranTimes).GetValueOrDefault()) && (nullable3.HasValue & nullable4.HasValue)))
                        {
                            if (info4 == null)
                            {
                                info4 = info5;
                            }
                            else
                            {
                                tranVol = info4.TranVol;
                                nullable2 = info5.TranVol;
                                if (((tranVol.GetValueOrDefault() > nullable2.GetValueOrDefault()) && (tranVol.HasValue & nullable2.HasValue)) || (((nullable3 = info4.TranTimes).GetValueOrDefault() > (nullable4 = info5.TranTimes).GetValueOrDefault()) && (nullable3.HasValue & nullable4.HasValue)))
                                {
                                    info4 = info5;
                                }
                            }
                        }
                    }
                }
                if (info4 == null)
                {
                    this.UpClassInfo.Visible = false;
                }
                else
                {
                    int num7 = 0;
                    if (info4.TranTimes.HasValue)
                    {
                        num7 = info4.TranTimes.Value - currentMemberInfo.OrderNumber;
                    }
                    if (num7 <= 0)
                    {
                        num7 = 1;
                    }
                    decimal num8 = 0M;
                    if (info4.TranVol.HasValue)
                    {
                        num8 = ((decimal) info4.TranVol.Value) - currentMemberInfo.Expenditure;
                    }
                    if (num8 <= 0M)
                    {
                        num8 = 0.01M;
                    }
                    this.UpClassInfo.InnerHtml = "再交易<span>" + num7.ToString() + "次 </span>或消费<span> " + Math.Round((decimal) (num8 + 0.49M), 0).ToString() + "元 </span>升级";
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VMemberCenter.html";
            }
            base.OnInit(e);
        }
    }
}

