namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.Core;
    using Hidistro.Entities.Members;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    [ParseChildren(true)]
    public class VStoreCard : VshopTemplatedWebControl
    {
        private HtmlControl editPanel;
        private HtmlImage imglogo;
        private HtmlInputHidden ShareInfo;
        private int userId = 0;

        protected override void AttachChildControls()
        {
            if (!int.TryParse(this.Page.Request.QueryString["ReferralId"], out this.userId))
            {
                base.GotoResourceNotFound("");
            }
            DistributorsInfo userIdDistributors = DistributorsBrower.GetUserIdDistributors(this.userId);
            if (userIdDistributors == null)
            {
                base.GotoResourceNotFound("");
            }
            this.imglogo = (HtmlImage) this.FindControl("QrcodeImg");
            int currentMemberUserId = Globals.GetCurrentMemberUserId();
            this.editPanel = (HtmlControl) this.FindControl("editPanel");
            this.editPanel.Visible = false;
            if (currentMemberUserId == this.userId)
            {
                this.imglogo.Attributes.Add("Admin", "true");
                MemberInfo currentMember = MemberProcessor.GetCurrentMember();
                DateTime cardCreatTime = userIdDistributors.CardCreatTime;
                string str = File.ReadAllText(HttpRuntime.AppDomainAppPath.ToString() + "Storage/Utility/StoreCardSet.js");
                JObject obj2 = JsonConvert.DeserializeObject(str) as JObject;
                DateTime time2 = new DateTime();
                if ((obj2 != null) && (obj2["writeDate"] != null))
                {
                    time2 = DateTime.Parse(obj2["writeDate"].ToString());
                }
                if (string.IsNullOrEmpty(userIdDistributors.StoreCard) || (cardCreatTime < time2))
                {
                    StoreCardCreater creater = new StoreCardCreater(str, currentMember.UserHead, userIdDistributors.Logo, Globals.HostPath(HttpContext.Current.Request.Url) + "/Follow.aspx?ReferralId=" + this.userId.ToString(), currentMember.UserName, userIdDistributors.StoreName, this.userId);
                    string imgUrl = "";
                    if (creater.ReadJson() && creater.CreadCard(out imgUrl))
                    {
                        DistributorsBrower.UpdateStoreCard(this.userId, imgUrl);
                    }
                }
            }
            if (string.IsNullOrEmpty(userIdDistributors.StoreCard))
            {
                userIdDistributors.StoreCard = "/Storage/master/DistributorCards/StoreCard" + this.userId.ToString() + ".jpg";
            }
            this.ShareInfo = (HtmlInputHidden) this.FindControl("ShareInfo");
            this.imglogo.Src = userIdDistributors.StoreCard;
            PageTitle.AddSiteNameTitle("掌柜名片");
        }

        protected override void OnInit(EventArgs e)
        {
            string str = HttpContext.Current.Request["action"];
            if (str == "ReCreadt")
            {
                HttpContext.Current.Response.ContentType = "application/json";
                string str2 = HttpContext.Current.Request["imageUrl"];
                string s = "";
                if (string.IsNullOrEmpty(str2))
                {
                    s = "{\"success\":\"false\",\"message\":\"图片地址为空\"}";
                }
                try
                {
                    MemberInfo currentMember = MemberProcessor.GetCurrentMember();
                    DistributorsInfo userIdDistributors = DistributorsBrower.GetUserIdDistributors(currentMember.UserId);
                    StoreCardCreater creater = new StoreCardCreater(File.ReadAllText(HttpRuntime.AppDomainAppPath.ToString() + "Storage/Utility/StoreCardSet.js"), str2, str2, Globals.HostPath(HttpContext.Current.Request.Url) + "/Follow.aspx?ReferralId=" + this.userId.ToString(), currentMember.UserName, userIdDistributors.StoreName, currentMember.UserId);
                    string imgUrl = "";
                    if (creater.ReadJson() && creater.CreadCard(out imgUrl))
                    {
                        s = "{\"success\":\"true\",\"message\":\"生成成功\"}";
                        DistributorsBrower.UpdateStoreCard(this.userId, imgUrl);
                    }
                    else
                    {
                        s = "{\"success\":\"false\",\"message\":\"" + imgUrl + "\"}";
                    }
                }
                catch (Exception exception)
                {
                    s = "{\"success\":\"false\",\"message\":\"" + exception.Message + "\"}";
                }
                HttpContext.Current.Response.Write(s);
                HttpContext.Current.Response.End();
            }
            if (this.SkinName == null)
            {
                this.SkinName = "skin-VStoreCard.html";
            }
            base.OnInit(e);
        }
    }
}

