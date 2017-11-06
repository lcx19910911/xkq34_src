using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ControlPanel.WeiBo;
using Hidistro.Core;
using Hidistro.Entities.Weibo;
using Hidistro.UI.Common.Controls;

namespace Hidistro.UI.SaleSystem.CodeBehind
{
    [ParseChildren(true)]
    public class VFriendsCircleDetail : VshopTemplatedWebControl
    {
        private Repeater ItemCtx;
        protected int MaterialID = 0;
        private Repeater TopCtx;

        protected override void AttachChildControls()
        {
            if (!int.TryParse(Globals.RequestQueryStr("id"), out this.MaterialID))
            {
                base.GotoResourceNotFound("");
            }
            else
            {
                string title = "";
                ArticleInfo articleInfo = ArticleHelper.GetArticleInfo(this.MaterialID);
                title = articleInfo.Title;
                DateTime now = DateTime.Now;
                this.TopCtx = (Repeater)this.FindControl("TopCtx");
                this.ItemCtx = (Repeater)this.FindControl("ItemCtx");
                List<ArticleInfo> list = new List<ArticleInfo> {
                    articleInfo
                };
                this.TopCtx.DataSource = list;
                this.TopCtx.DataBind();
                if (articleInfo.ArticleType == ArticleType.List)
                {
                    IList<ArticleItemsInfo> itemsInfo = articleInfo.ItemsInfo;
                    this.ItemCtx.DataSource = itemsInfo;
                    this.ItemCtx.DataBind();
                }
                PageTitle.AddSiteNameTitle(title);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "skin-VFriendsCircleDetail.html";
            }
            base.OnInit(e);
        }
    }
}

