//namespace Hidistro.UI.SaleSystem.CodeBehind
//{
//    using Hidistro.Core;
//    using Hidistro.Core.Entities;
//    using Hidistro.Entities.Commodities;
//    using Hidistro.Entities.Members;
//    using Hidistro.Entities.VShop;
//    using Hidistro.SaleSystem.Vshop;
//    using Hidistro.UI.Common.Controls;
//    using Hidistro.UI.SaleSystem.Tags;
//    using System;
//    using System.Linq;
//    using System.Text.RegularExpressions;
//    using System.Web;
//    using System.Web.UI;
//    using System.Web.UI.HtmlControls;
//    using System.Web.UI.WebControls;

//    [ParseChildren(true)]
//    public class VProductDetails : VshopTemplatedWebControl
//    {
//        private Common_ExpandAttributes expandAttr;
//        private HyperLink linkDescription;
//        private HtmlInputHidden litCategoryId;
//        private Literal litConsultationsCount;
//        private Literal litDescription;
//        private HtmlInputHidden litHasCollected;
//        private Literal litItemParams;
//        private Literal litMarketPrice;
//        private Literal litProdcutName;
//        private Literal litProdcutTag;
//        private HtmlInputHidden litproductid;
//        private Literal litReviewsCount;
//        private Literal litSalePrice;
//        private Literal litShortDescription;
//        private Literal litSoldCount;
//        private Literal litStock;
//        private HtmlInputHidden litTemplate;
//        private int productId;
//        private VshopTemplatedRepeater rptProductImages;
//        private Common_SKUSelector skuSelector;

//        protected override void AttachChildControls()
//        {
//            if (!int.TryParse(this.Page.Request.QueryString["productId"], out this.productId))
//            {
//                base.GotoResourceNotFound("");
//            }
//            this.rptProductImages = (VshopTemplatedRepeater) this.FindControl("rptProductImages");
//            this.litItemParams = (Literal) this.FindControl("litItemParams");
//            this.litProdcutName = (Literal) this.FindControl("litProdcutName");
//            this.litProdcutTag = (Literal) this.FindControl("litProdcutTag");
//            this.litSalePrice = (Literal) this.FindControl("litSalePrice");
//            this.litMarketPrice = (Literal) this.FindControl("litMarketPrice");
//            this.litShortDescription = (Literal) this.FindControl("litShortDescription");
//            this.litDescription = (Literal) this.FindControl("litDescription");
//            this.litStock = (Literal) this.FindControl("litStock");
//            this.skuSelector = (Common_SKUSelector) this.FindControl("skuSelector");
//            this.linkDescription = (HyperLink) this.FindControl("linkDescription");
//            this.expandAttr = (Common_ExpandAttributes) this.FindControl("ExpandAttributes");
//            this.litSoldCount = (Literal) this.FindControl("litSoldCount");
//            this.litConsultationsCount = (Literal) this.FindControl("litConsultationsCount");
//            this.litReviewsCount = (Literal) this.FindControl("litReviewsCount");
//            this.litHasCollected = (HtmlInputHidden) this.FindControl("litHasCollected");
//            this.litCategoryId = (HtmlInputHidden) this.FindControl("litCategoryId");
//            this.litproductid = (HtmlInputHidden) this.FindControl("litproductid");
//            this.litTemplate = (HtmlInputHidden) this.FindControl("litTemplate");
//            ProductInfo product = ProductBrowser.GetProduct(MemberProcessor.GetCurrentMember(), this.productId);
//            if (product != null)
//            {
//                this.litproductid.Value = this.productId.ToString();
//                this.litTemplate.Value = product.FreightTemplateId.ToString();
//                if (product == null)
//                {
//                    base.GotoResourceNotFound("此商品已不存在");
//                }
//                if (product.SaleStatus != ProductSaleStatus.OnSale)
//                {
//                    base.GotoResourceNotFound(ErrorType.前台商品下架, "此商品已下架");
//                }
//                if (this.rptProductImages != null)
//                {
//                    string locationUrl = "javascript:;";
//                    SlideImage[] imageArray = new SlideImage[] { new SlideImage(product.ImageUrl1, locationUrl), new SlideImage(product.ImageUrl2, locationUrl), new SlideImage(product.ImageUrl3, locationUrl), new SlideImage(product.ImageUrl4, locationUrl), new SlideImage(product.ImageUrl5, locationUrl) };
//                    if (CS$<>9__CachedAnonymousMethodDelegate1 == null)
//                    {
//                        CS$<>9__CachedAnonymousMethodDelegate1 = new Func<SlideImage, bool>(null, (IntPtr) <AttachChildControls>b__0);
//                    }
//                    this.rptProductImages.DataSource = Enumerable.Where<SlideImage>(imageArray, CS$<>9__CachedAnonymousMethodDelegate1);
//                    this.rptProductImages.DataBind();
//                }
//                string mainCategoryPath = product.MainCategoryPath;
//                if (!string.IsNullOrEmpty(mainCategoryPath))
//                {
//                    this.litCategoryId.Value = mainCategoryPath.Split(new char[] { '|' })[0];
//                }
//                else
//                {
//                    this.litCategoryId.Value = "0";
//                }
//                string productName = product.ProductName;
//                string productTagName = ProductBrowser.GetProductTagName(this.productId);
//                if (!string.IsNullOrEmpty(productTagName))
//                {
//                    this.litProdcutTag.Text = "<div class='y-shopicon'>" + productTagName.Trim() + "</div>";
//                    productTagName = "<span class='producttag'>【" + HttpContext.Current.Server.HtmlEncode(productTagName) + "】</span>";
//                }
//                this.litProdcutName.Text = productTagName + productName;
//                if (product.MinSalePrice != product.MaxSalePrice)
//                {
//                    this.litSalePrice.Text = product.MinSalePrice.ToString("F2") + "~" + product.MaxSalePrice.ToString("F2");
//                }
//                else
//                {
//                    this.litSalePrice.Text = product.MinSalePrice.ToString("F2");
//                }
//                if (product.MarketPrice.HasValue)
//                {
//                    this.litMarketPrice.SetWhenIsNotNull(product.MarketPrice.GetValueOrDefault(0M).ToString("F2"));
//                }
//                this.litShortDescription.Text = product.ShortDescription;
//                string description = product.Description;
//                if (!string.IsNullOrEmpty(description))
//                {
//                    description = Regex.Replace(description, "<img[^>]*\\bsrc=('|\")([^'\">]*)\\1[^>]*>", "<img alt='" + HttpContext.Current.Server.HtmlEncode(productName) + "' src='$2' />", RegexOptions.IgnoreCase);
//                }
//                if (this.litDescription != null)
//                {
//                    this.litDescription.Text = description;
//                }
//                this.litSoldCount.SetWhenIsNotNull(product.ShowSaleCounts.ToString());
//                this.litStock.Text = product.Stock.ToString();
//                this.skuSelector.ProductId = this.productId;
//                if (this.expandAttr != null)
//                {
//                    this.expandAttr.ProductId = this.productId;
//                }
//                if (this.linkDescription != null)
//                {
//                    this.linkDescription.NavigateUrl = "/Vshop/ProductDescription.aspx?productId=" + this.productId;
//                }
//                int productConsultationsCount = ProductBrowser.GetProductConsultationsCount(this.productId, false);
//                this.litConsultationsCount.SetWhenIsNotNull(productConsultationsCount.ToString());
//                this.litReviewsCount.SetWhenIsNotNull(ProductBrowser.GetProductReviewsCount(this.productId).ToString());
//                MemberInfo currentMember = MemberProcessor.GetCurrentMember();
//                bool flag = false;
//                if (currentMember != null)
//                {
//                    flag = ProductBrowser.CheckHasCollect(currentMember.UserId, this.productId);
//                }
//                this.litHasCollected.SetWhenIsNotNull(flag ? "1" : "0");
//                ProductBrowser.UpdateVisitCounts(this.productId);
//                PageTitle.AddSiteNameTitle(productName);
//                PageTitle.AddSiteDescription(product.ShortDescription);
//                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
//                string str6 = "";
//                if (!string.IsNullOrEmpty(masterSettings.GoodsPic))
//                {
//                    str6 = Globals.HostPath(HttpContext.Current.Request.Url) + masterSettings.GoodsPic;
//                }
//                this.litItemParams.Text = str6.Replace("|", "｜") + "|" + masterSettings.GoodsName.Replace("|", "｜") + "|" + masterSettings.GoodsDescription.Replace("|", "｜") + "$" + Globals.HostPath(HttpContext.Current.Request.Url).Replace("|", "｜") + product.ImageUrl1.Replace("|", "｜") + "|" + product.ProductName.Replace("|", "｜") + "|" + ((product.ShortDescription == null) ? "" : product.ShortDescription.Replace("|", "｜")) + "|" + HttpContext.Current.Request.Url.ToString().Replace("|", "｜");
//            }
//            else
//            {
//                HttpContext.Current.Response.Redirect("/default.aspx");
//                HttpContext.Current.Response.End();
//            }
//        }

//        protected override void OnInit(EventArgs e)
//        {
//            if (this.SkinName == null)
//            {
//                this.SkinName = "Skin-VProductDetails.html";
//            }
//            base.OnInit(e);
//        }
//    }
//}

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Hidistro.Core;
using Hidistro.Core.Entities;
using Hidistro.Entities.Commodities;
using Hidistro.Entities.Members;
using Hidistro.Entities.VShop;
using Hidistro.SaleSystem.Vshop;
using Hidistro.UI.Common.Controls;
using Hidistro.UI.SaleSystem.Tags;

namespace Hidistro.UI.SaleSystem.CodeBehind
{
    /// <summary>
    /// 商品详情
    /// </summary>
    [System.Web.UI.ParseChildren(true)]
    public class VProductDetails : VshopTemplatedWebControl
    {

        #region 字段
        private int productId;
        private VshopTemplatedRepeater rptProductImages;
        private System.Web.UI.WebControls.Literal litProdcutName;
        private System.Web.UI.WebControls.Literal litProdcutTag;
        private System.Web.UI.WebControls.Literal litSalePrice;
        private System.Web.UI.WebControls.Literal litMarketPrice;
        private System.Web.UI.WebControls.Literal litShortDescription;
        private System.Web.UI.WebControls.Literal litDescription;
        private System.Web.UI.WebControls.Literal litStock;
        private System.Web.UI.WebControls.Literal litSoldCount;
        private System.Web.UI.WebControls.Literal litConsultationsCount;
        private System.Web.UI.WebControls.Literal litReviewsCount;
        private System.Web.UI.WebControls.Literal litItemParams;
        private Common_SKUSelector skuSelector;
        private Common_ExpandAttributes expandAttr;
        private System.Web.UI.WebControls.HyperLink linkDescription;
        private System.Web.UI.HtmlControls.HtmlInputHidden litHasCollected;
        private System.Web.UI.HtmlControls.HtmlInputHidden litCategoryId;
        private System.Web.UI.HtmlControls.HtmlInputHidden litproductid;
        private System.Web.UI.HtmlControls.HtmlInputHidden litTemplate;
        #endregion

        #region 设置皮肤
        protected override void OnInit(System.EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VProductDetails.html";
            }
            base.OnInit(e);
        }
        #endregion

        protected override void AttachChildControls()
        {

            #region 资源存在判断

            if (!int.TryParse(this.Page.Request.QueryString["productId"], out this.productId))
            {
                base.GotoResourceNotFound("");
            }

            ProductInfo product = ProductBrowser.GetProduct(MemberProcessor.GetCurrentMember(), this.productId);
            if (null == product)
            {
                base.GotoResourceNotFound("此商品已不存在");
            }

            if (product.SaleStatus != ProductSaleStatus.OnSale)
            {
                base.GotoResourceNotFound(ErrorType.前台商品下架, "此商品已下架");
            }

            #endregion

            #region 关联控件
            this.rptProductImages = (VshopTemplatedRepeater)this.FindControl("rptProductImages");
            this.litItemParams = (System.Web.UI.WebControls.Literal)this.FindControl("litItemParams");
            this.litProdcutName = (System.Web.UI.WebControls.Literal)this.FindControl("litProdcutName");
            this.litProdcutTag = (System.Web.UI.WebControls.Literal)this.FindControl("litProdcutTag");
            this.litSalePrice = (System.Web.UI.WebControls.Literal)this.FindControl("litSalePrice");
            this.litMarketPrice = (System.Web.UI.WebControls.Literal)this.FindControl("litMarketPrice");
            this.litShortDescription = (System.Web.UI.WebControls.Literal)this.FindControl("litShortDescription");
            this.litDescription = (System.Web.UI.WebControls.Literal)this.FindControl("litDescription");
            this.litStock = (System.Web.UI.WebControls.Literal)this.FindControl("litStock");
            this.skuSelector = (Common_SKUSelector)this.FindControl("skuSelector");
            this.linkDescription = (System.Web.UI.WebControls.HyperLink)this.FindControl("linkDescription");
            this.expandAttr = (Common_ExpandAttributes)this.FindControl("ExpandAttributes");
            this.litSoldCount = (System.Web.UI.WebControls.Literal)this.FindControl("litSoldCount");
            this.litConsultationsCount = (System.Web.UI.WebControls.Literal)this.FindControl("litConsultationsCount");
            this.litReviewsCount = (System.Web.UI.WebControls.Literal)this.FindControl("litReviewsCount");
            this.litHasCollected = (System.Web.UI.HtmlControls.HtmlInputHidden)this.FindControl("litHasCollected");
            this.litCategoryId = (System.Web.UI.HtmlControls.HtmlInputHidden)this.FindControl("litCategoryId");
            this.litproductid = (System.Web.UI.HtmlControls.HtmlInputHidden)this.FindControl("litproductid");
            this.litTemplate = (System.Web.UI.HtmlControls.HtmlInputHidden)this.FindControl("litTemplate");
            #endregion

            this.litproductid.Value = this.productId.ToString();
            this.litTemplate.Value = product.FreightTemplateId.ToString();


            if (this.rptProductImages != null)
            {
                string locationUrl = "javascript:;";

                SlideImage[] source = new SlideImage[]
					{
						new SlideImage(product.ImageUrl1, locationUrl),
						new SlideImage(product.ImageUrl2, locationUrl),
						new SlideImage(product.ImageUrl3, locationUrl),
						new SlideImage(product.ImageUrl4, locationUrl),
						new SlideImage(product.ImageUrl5, locationUrl)
					};

                this.rptProductImages.DataSource =
                    from item in source
                    where !string.IsNullOrWhiteSpace(item.ImageUrl)
                    select item;

                this.rptProductImages.DataBind();

            }

            string mainCategoryPath = product.MainCategoryPath;

            if (!string.IsNullOrEmpty(mainCategoryPath))
            {
                this.litCategoryId.Value = mainCategoryPath.Split(new char[] { '|' })[0];
            }
            else
            {
                this.litCategoryId.Value = "0";
            }

            string productName = product.ProductName;

            string tagName = ProductBrowser.GetProductTagName(this.productId);

            if (!string.IsNullOrEmpty(tagName))
            {
                this.litProdcutTag.Text = "<div class='y-shopicon'>" + tagName.Trim() + "</div>";
                tagName = "<span class='producttag'>【" + System.Web.HttpContext.Current.Server.HtmlEncode(tagName) + "】</span>";
            }


            this.litProdcutName.Text = tagName + productName;

            this.litSalePrice.Text = product.MinSalePrice.ToString("F2");

            if (product.MarketPrice.HasValue)
            {
                this.litMarketPrice.SetWhenIsNotNull(product.MarketPrice.GetValueOrDefault(0m).ToString("F2"));
            }

            this.litShortDescription.Text = product.ShortDescription;

            string description = product.Description;
            
            //160519修改
            //if (!string.IsNullOrEmpty(description))
            //{
            //    description = System.Text.RegularExpressions.Regex.Replace(description, "<img[^>]*\\bsrc=('|\")([^'\">]*)\\1[^>]*>", "<img alt='" + System.Web.HttpContext.Current.Server.HtmlEncode(productName) + "' src=\"/Utility/images/blank.gif\" data-echo='$2' />", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //}

                if (this.litDescription != null)
                {
                    this.litDescription.Text = description;
                }

            this.litSoldCount.SetWhenIsNotNull(product.ShowSaleCounts.ToString());

            this.litStock.Text = product.Stock.ToString();

            this.skuSelector.ProductId = this.productId;

            if (this.expandAttr != null)
            {
                this.expandAttr.ProductId = this.productId;
            }

            if (this.linkDescription != null)
            {
                this.linkDescription.NavigateUrl = "/Vshop/ProductDescription.aspx?productId=" + this.productId;
            }

            int num = ProductBrowser.GetProductConsultationsCount(this.productId, false);

            this.litConsultationsCount.SetWhenIsNotNull(num.ToString());

            num = ProductBrowser.GetProductReviewsCount(this.productId);


            this.litReviewsCount.SetWhenIsNotNull(num.ToString());

            MemberInfo currentMember = MemberProcessor.GetCurrentMember();

            bool flag = false;
            if (currentMember != null)
            {
                flag = ProductBrowser.CheckHasCollect(currentMember.UserId, this.productId);
            }

            this.litHasCollected.SetWhenIsNotNull(flag ? "1" : "0");

            ProductBrowser.UpdateVisitCounts(this.productId);

            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);

            string hostPath = "";
            if (!string.IsNullOrEmpty(masterSettings.GoodsPic))
            {
                hostPath = Globals.HostPath(System.Web.HttpContext.Current.Request.Url) + masterSettings.GoodsPic;
            }

            this.litItemParams.Text = string.Concat(new object[]
				{
					hostPath,
					"|",
					masterSettings.GoodsName,
					"|",
					masterSettings.GoodsDescription,
					"$",
					Globals.HostPath(System.Web.HttpContext.Current.Request.Url),
					product.ImageUrl1,
					"|",
					this.litProdcutName.Text,
					"|",
					product.ShortDescription,
					"|",
					System.Web.HttpContext.Current.Request.Url});


            PageTitle.AddSiteNameTitle(productName);

            PageTitle.AddSiteDescription(product.ShortDescription);

            // System.Web.HttpContext.Current.Response.Redirect("/vshop/");
            // System.Web.HttpContext.Current.Response.End();
        }
    }
}
