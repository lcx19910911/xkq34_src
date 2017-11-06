namespace Hidistro.UI.Web.Admin.promotion
{
    using ASPNET.WebControls;
    using Hidistro.ControlPanel.Promotions;
    using Hidistro.Core.Enums;
    using Hidistro.Entities.Promotions;
    using Hidistro.UI.ControlPanel.Utility;
    using System;
    using System.Data;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class MemberCouponList : AdminPage
    {
        protected Button btnSeach;
        protected Grid grdCoupondsList;
        protected PageSize hrefPageSize;
        protected Pager pager1;
        protected HtmlForm thisForm;
        protected TextBox txt_name;
        protected TextBox txt_orderNo;

        protected MemberCouponList() : base("m08", "yxp01")
        {
        }

        private void BindData()
        {
            string text = "";
            string str2 = "";
            text = this.txt_name.Text;
            str2 = this.txt_orderNo.Text;
            int total = 0;
            MemberCouponsSearch search = new MemberCouponsSearch {
                CouponName = text,
                OrderNo = str2,
                IsCount = true,
                PageIndex = this.pager1.PageIndex,
                PageSize = this.pager1.PageSize,
                SortBy = "CouponId",
                SortOrder = SortAction.Desc
            };
            DataTable memberCoupons = CouponHelper.GetMemberCoupons(search, ref total);
            if ((memberCoupons != null) && (memberCoupons.Rows.Count > 0))
            {
                memberCoupons.Columns.Add("useConditon");
                memberCoupons.Columns.Add("sStatus");
                for (int i = 0; i < memberCoupons.Rows.Count; i++)
                {
                    decimal num3 = decimal.Parse(memberCoupons.Rows[i]["ConditionValue"].ToString());
                    if (num3 == 0M)
                    {
                        memberCoupons.Rows[i]["useConditon"] = "不限制";
                    }
                    else
                    {
                        memberCoupons.Rows[i]["useConditon"] = "满" + num3.ToString("F2") + "可使用";
                    }
                    memberCoupons.Rows[i]["sStatus"] = (int.Parse(memberCoupons.Rows[i]["Status"].ToString()) == 0) ? "已领用" : "已使用";
                }
            }
            this.grdCoupondsList.DataSource = memberCoupons;
            this.grdCoupondsList.DataBind();
            this.pager1.TotalRecords = total;
        }

        protected void btnImagetSearch_Click(object sender, EventArgs e)
        {
            this.BindData();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.btnSeach.Click += new EventHandler(this.btnImagetSearch_Click);
            if (!base.IsPostBack)
            {
                this.BindData();
            }
        }
    }
}

