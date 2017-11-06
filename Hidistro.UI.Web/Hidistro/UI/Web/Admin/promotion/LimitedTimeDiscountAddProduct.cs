namespace Hidistro.UI.Web.Admin.promotion
{
    using ASPNET.WebControls;
    using Hidistro.ControlPanel.Commodities;
    using Hidistro.ControlPanel.Promotions;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Core.Enums;
    using Hidistro.Entities.Commodities;
    using Hidistro.Entities.Promotions;
    using Hidistro.UI.ControlPanel.Utility;
    using System;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class LimitedTimeDiscountAddProduct : AdminPage
    {
        protected string actionName;
        protected Button btnSeach;
        private int? categoryId;
        protected ProductCategoriesDropDownList dropCategories;
        protected Repeater grdProducts;
        protected PageSize hrefPageSize;
        protected int id;
        protected Pager pager;
        protected HtmlForm thisForm;
        protected TextBox txtProductName;

        protected LimitedTimeDiscountAddProduct() : base("m08", "yxp24")
        {
        }

        protected void btnSeach_Click(object sender, EventArgs e)
        {
            this.DataBindDiscount();
        }

        private void DataBindDiscount()
        {
            this.id = Globals.RequestQueryNum("id");
            if (this.id > 0)
            {
                LimitedTimeDiscountInfo discountInfo = LimitedTimeDiscountHelper.GetDiscountInfo(this.id);
                if (discountInfo != null)
                {
                    this.actionName = discountInfo.ActivityName;
                }
            }
            ProductQuery query = new ProductQuery {
                Keywords = this.txtProductName.Text,
                ProductCode = "",
                CategoryId = this.dropCategories.SelectedValue,
                PageSize = this.pager.PageSize,
                PageIndex = this.pager.PageIndex,
                SortOrder = SortAction.Desc,
                SortBy = "DisplaySequence"
            };
            if (this.dropCategories.SelectedValue.HasValue && (this.dropCategories.SelectedValue > 0))
            {
                query.MaiCategoryPath = CatalogHelper.GetCategory(this.dropCategories.SelectedValue.Value).Path;
            }
            DbQueryResult discountProduct = LimitedTimeDiscountHelper.GetDiscountProduct(query);
            this.grdProducts.DataSource = discountProduct.Data;
            this.grdProducts.DataBind();
            this.pager.TotalRecords = discountProduct.TotalRecords;
        }

        private void dropSaleStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DataBindDiscount();
        }

        protected string GetDisable(string ActivityName, object limitedTimeDiscountId, int discountId)
        {
            if (!string.IsNullOrEmpty(ActivityName) && (Globals.ToNum(limitedTimeDiscountId) != discountId))
            {
                return "disabled";
            }
            return "";
        }

        protected string GetDisplay(object obj)
        {
            if (!string.IsNullOrEmpty(obj.ToString()))
            {
                return "";
            }
            return "none";
        }

        protected string GetDisplayValue(object obj)
        {
            decimal num;
            if (decimal.TryParse(obj.ToString(), out num) && (num > 0M))
            {
                return "";
            }
            return "none";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!base.IsPostBack)
            {
                this.dropCategories.IsUnclassified = true;
                this.dropCategories.DataBind();
                this.DataBindDiscount();
            }
        }
    }
}

