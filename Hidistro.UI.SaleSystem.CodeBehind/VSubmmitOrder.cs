
using ControlPanel.Promotions;
using Hidistro.ControlPanel.Members;
using Hidistro.ControlPanel.Promotions;
using Hidistro.Core;
using Hidistro.Entities.Promotions;
using Hidistro.Entities.Sales;
using Hidistro.SaleSystem.Vshop;
using Hidistro.UI.Common.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Hidistro.UI.SaleSystem.CodeBehind
{
    [ParseChildren(true)]
    public class VSubmmitOrder : VMemberTemplatedWebControl
    {
        private HtmlAnchor aLinkToShipping;
        private int buyAmount;
        private DataTable dtActivities = ActivityHelper.GetActivities();
        public DataTable GetUserCoupons = null;
        private HtmlInputControl groupbuyHiddenBox;
        private bool isbargain = (Globals.RequestQueryNum("bargainDetialId") > 0);
        private Literal litAddAddress;
        private Literal litAddress;
        private Literal litCellPhone;
        private Literal litDisplayPointNumber;
        private Literal litOrderTotal;
        private Literal litPointNumber;
        private Literal litShipTo;
        private Literal litShowMes;
        private string productSku;
        private HtmlInputHidden regionId;
        private VshopTemplatedRepeater rptAddress;
        private VshopTemplatedRepeater rptCartProducts;
        private HtmlInputHidden selectShipTo;

        protected override void AttachChildControls()
        {
            this.litShipTo = (Literal)this.FindControl("litShipTo");
            this.litCellPhone = (Literal)this.FindControl("litCellPhone");
            this.litAddress = (Literal)this.FindControl("litAddress");
            this.litShowMes = (Literal)this.FindControl("litShowMes");
            this.GetUserCoupons = MemberProcessor.GetUserCoupons();
            this.rptCartProducts = (VshopTemplatedRepeater)this.FindControl("rptCartProducts");
            this.rptCartProducts.ItemDataBound += new RepeaterItemEventHandler(this.rptCartProducts_ItemDataBound);
            this.litOrderTotal = (Literal)this.FindControl("litOrderTotal");
            this.litPointNumber = (Literal)this.FindControl("litPointNumber");
            this.litDisplayPointNumber = (Literal)this.FindControl("litDisplayPointNumber");
            this.aLinkToShipping = (HtmlAnchor)this.FindControl("aLinkToShipping");
            this.groupbuyHiddenBox = (HtmlInputControl)this.FindControl("groupbuyHiddenBox");
            this.rptAddress = (VshopTemplatedRepeater)this.FindControl("rptAddress");
            this.selectShipTo = (HtmlInputHidden)this.FindControl("selectShipTo");
            this.regionId = (HtmlInputHidden)this.FindControl("regionId");
            this.litAddAddress = (Literal)this.FindControl("litAddAddress");
            IList<ShippingAddressInfo> shippingAddresses = MemberProcessor.GetShippingAddresses();
            //if (CS$<>9__CachedAnonymousMethodDelegate2 == null)
            //{
            //    CS$<>9__CachedAnonymousMethodDelegate2 = new Func<ShippingAddressInfo, bool>(null, (IntPtr) <AttachChildControls>b__0);
            //}
            //this.rptAddress.DataSource = Enumerable.OrderBy<ShippingAddressInfo, bool>(shippingAddresses, CS$<>9__CachedAnonymousMethodDelegate2);
            //this.rptAddress.DataBind();
            //if (CS$<>9__CachedAnonymousMethodDelegate3 == null)
            //{
            //    CS$<>9__CachedAnonymousMethodDelegate3 = new Func<ShippingAddressInfo, bool>(null, (IntPtr) <AttachChildControls>b__1);
            //}
            this.rptAddress.DataSource = from item in shippingAddresses
                                         orderby item.IsDefault
                                         select item;
            this.rptAddress.DataBind();
            ShippingAddressInfo info = shippingAddresses.FirstOrDefault<ShippingAddressInfo>(item => item.IsDefault);
            //ShippingAddressInfo info = Enumerable.FirstOrDefault<ShippingAddressInfo>(shippingAddresses, CS$<>9__CachedAnonymousMethodDelegate3);
            if (info == null)
            {
                info = (shippingAddresses.Count > 0) ? shippingAddresses[0] : null;
            }
            if (info != null)
            {
                this.litShipTo.Text = info.ShipTo;
                this.litCellPhone.Text = info.CellPhone;
                this.litAddress.Text = info.Address;
                this.selectShipTo.SetWhenIsNotNull(info.ShippingId.ToString());
                this.regionId.SetWhenIsNotNull(info.RegionId.ToString());
            }
            this.litAddAddress.Text = " href='/Vshop/AddShippingAddress.aspx?returnUrl=" + Globals.UrlEncode(HttpContext.Current.Request.Url.ToString()) + "'";
            if ((shippingAddresses == null) || (shippingAddresses.Count == 0))
            {
                this.Page.Response.Redirect(Globals.ApplicationPath + "/Vshop/AddShippingAddress.aspx?returnUrl=" + Globals.UrlEncode(HttpContext.Current.Request.Url.ToString()));
            }
            else
            {
                this.aLinkToShipping.HRef = Globals.ApplicationPath + "/Vshop/ShippingAddresses.aspx?returnUrl=" + Globals.UrlEncode(HttpContext.Current.Request.Url.ToString());
                List<ShoppingCartInfo> orderSummitCart = new List<ShoppingCartInfo>();
                if (((int.TryParse(this.Page.Request.QueryString["buyAmount"], out this.buyAmount) && !string.IsNullOrEmpty(this.Page.Request.QueryString["productSku"])) && !string.IsNullOrEmpty(this.Page.Request.QueryString["from"])) && ((this.Page.Request.QueryString["from"] == "signBuy") || (this.Page.Request.QueryString["from"] == "groupBuy")))
                {
                    this.productSku = this.Page.Request.QueryString["productSku"];
                    if (this.isbargain)
                    {
                        int bargainDetialId = Globals.RequestQueryNum("bargainDetialId");
                        orderSummitCart = ShoppingCartProcessor.GetListShoppingCart(this.productSku, this.buyAmount, bargainDetialId, 0);
                    }
                    else
                    {
                        int buyAmount = this.buyAmount;
                        int id = Globals.RequestQueryNum("limitedTimeDiscountId");
                        if (id > 0)
                        {
                            bool flag = true;
                            LimitedTimeDiscountInfo discountInfo = LimitedTimeDiscountHelper.GetDiscountInfo(id);
                            if (discountInfo == null)
                            {
                                flag = false;
                            }
                            if (flag)
                            {
                                int num4 = ShoppingCartProcessor.GetLimitedTimeDiscountUsedNum(id, this.productSku, 0, base.CurrentMemberInfo.UserId, false);
                                if (MemberHelper.CheckCurrentMemberIsInRange(discountInfo.ApplyMembers, discountInfo.DefualtGroup, discountInfo.CustomGroup, base.CurrentMemberInfo.UserId))
                                {
                                    if ((discountInfo.LimitNumber > 0) && (this.buyAmount <= (discountInfo.LimitNumber - num4)))
                                    {
                                        buyAmount = discountInfo.LimitNumber - num4;
                                        if (buyAmount > this.buyAmount)
                                        {
                                            buyAmount = this.buyAmount;
                                        }
                                    }
                                    else if (discountInfo.LimitNumber != 0)
                                    {
                                        id = 0;
                                    }
                                }
                                else
                                {
                                    id = 0;
                                }
                            }
                            else
                            {
                                id = 0;
                            }
                        }
                        orderSummitCart = ShoppingCartProcessor.GetListShoppingCart(this.productSku, buyAmount, 0, id);
                    }
                }
                else
                {
                    orderSummitCart = ShoppingCartProcessor.GetOrderSummitCart();
                }
                if (orderSummitCart == null)
                {
                    HttpContext.Current.Response.Write("<script>alert('商品已下架或没有需要结算的订单！');location.href='/Vshop/ShoppingCart.aspx'</script>");
                }
                else
                {
                    if (orderSummitCart.Count > 1)
                    {
                        this.litShowMes.Text = "<div style=\"color: #F60; \"><img  src=\"/Utility/pics/u77.png\">您所购买的商品不支持同一个物流规则发货，系统自动拆分成多个子订单处理</div>";
                    }
                    this.rptCartProducts.DataSource = orderSummitCart;
                    this.rptCartProducts.DataBind();
                    decimal num5 = 0M;
                    decimal num6 = 0M;
                    decimal num7 = 0M;
                    int num8 = 0;
                    foreach (ShoppingCartInfo info3 in orderSummitCart)
                    {
                        num8 += info3.GetPointNumber;
                        num5 += info3.Total;
                        num6 += info3.Exemption;
                        num7 += info3.ShipCost;
                    }
                    decimal num9 = num6;
                    this.litOrderTotal.Text = (num5 - num9).ToString("F2");
                    if (num8 == 0)
                    {
                        this.litDisplayPointNumber.Text = "style=\"display:none;\"";
                    }
                    this.litPointNumber.Text = num8.ToString();
                    PageTitle.AddSiteNameTitle("订单确认");
                }
            }
        }

        public decimal DiscountMoney(List<ShoppingCartInfo> infoList)
        {
            decimal num = 0M;
            decimal num2 = 0M;
            decimal num3 = 0M;
            decimal num4 = 0M;
            int num5 = 0;
            foreach (ShoppingCartInfo info in infoList)
            {
                foreach (ShoppingCartItemInfo info2 in info.LineItems)
                {
                    if (info2.Type == 0)
                    {
                        num4 += info2.SubTotal;
                        num5 += info2.Quantity;
                    }
                }
            }
            for (int i = 0; i < this.dtActivities.Rows.Count; i++)
            {
                decimal num7 = 0M;
                int num8 = 0;
                DataTable table = ActivityHelper.GetActivities_Detail(int.Parse(this.dtActivities.Rows[i]["ActivitiesId"].ToString()));
                foreach (ShoppingCartInfo info in infoList)
                {
                    foreach (ShoppingCartItemInfo info2 in info.LineItems)
                    {
                        if ((info2.Type == 0) && (ActivityHelper.GetActivitiesProducts(int.Parse(this.dtActivities.Rows[i]["ActivitiesId"].ToString()), info2.ProductId).Rows.Count > 0))
                        {
                            num7 += info2.SubTotal;
                            num8 += info2.Quantity;
                        }
                    }
                }
                if (table.Rows.Count > 0)
                {
                    for (int j = 0; j < table.Rows.Count; j++)
                    {
                        if (bool.Parse(this.dtActivities.Rows[i]["isAllProduct"].ToString()))
                        {
                            if (decimal.Parse(table.Rows[j]["MeetMoney"].ToString()) > 0M)
                            {
                                if (num4 >= decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[table.Rows.Count - 1]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num4 <= decimal.Parse(table.Rows[0]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[0]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[0]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num4 >= decimal.Parse(table.Rows[j]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[j]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[j]["ReductionMoney"].ToString());
                                }
                            }
                            else
                            {
                                if (num5 >= int.Parse(table.Rows[table.Rows.Count - 1]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString());
                                    num3 = decimal.Parse(table.Rows[table.Rows.Count - 1]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num5 <= int.Parse(table.Rows[0]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[0]["MeetMoney"].ToString());
                                    num3 = decimal.Parse(table.Rows[0]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num5 >= int.Parse(table.Rows[j]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[j]["MeetMoney"].ToString());
                                    num3 = decimal.Parse(table.Rows[j]["ReductionMoney"].ToString());
                                }
                            }
                        }
                        else
                        {
                            num4 = num7;
                            num5 = num8;
                            if (decimal.Parse(table.Rows[j]["MeetMoney"].ToString()) > 0M)
                            {
                                if (num4 >= decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[table.Rows.Count - 1]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num4 <= decimal.Parse(table.Rows[0]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[0]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[0]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num4 >= decimal.Parse(table.Rows[j]["MeetMoney"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[j]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[j]["ReductionMoney"].ToString());
                                }
                            }
                            else
                            {
                                if (num5 >= int.Parse(table.Rows[table.Rows.Count - 1]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[table.Rows.Count - 1]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[table.Rows.Count - 1]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num5 <= int.Parse(table.Rows[0]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[0]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[0]["ReductionMoney"].ToString());
                                    break;
                                }
                                if (num5 >= int.Parse(table.Rows[j]["MeetNumber"].ToString()))
                                {
                                    num2 = decimal.Parse(table.Rows[j]["MeetMoney"].ToString());
                                    num = decimal.Parse(table.Rows[j]["ReductionMoney"].ToString());
                                }
                            }
                        }
                    }
                    if ((num4 >= num2) || (num2 == 0M))
                    {
                        num3 += num;
                    }
                }
            }
            return num3;
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VSubmmitOrder.html";
            }
            base.OnInit(e);
        }

        private void rptCartProducts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item) || (e.Item.ItemType == ListItemType.AlternatingItem))
            {
                int num;
                decimal num2;
                List<ShoppingCartItemInfo> list = (List<ShoppingCartItemInfo>)DataBinder.Eval(e.Item.DataItem, "LineItems");
                Literal literal = (Literal)e.Item.Controls[0].FindControl("LitCoupon");
                Literal literal2 = (Literal)e.Item.Controls[0].FindControl("litExemption");
                Literal literal3 = (Literal)e.Item.Controls[0].FindControl("litoldExemption");
                Literal literal4 = (Literal)e.Item.Controls[0].FindControl("litoldTotal");
                Literal literal5 = (Literal)e.Item.Controls[0].FindControl("litTotal");
                Literal literal6 = (Literal)e.Item.Controls[0].FindControl("litbFreeShipping");
                string str = "";
                string str2 = " <div class=\"btn-group coupon\">";
                object obj2 = str2;
                obj2 = string.Concat(new object[] { obj2, "<button type=\"button\" class=\"btn btn-default dropdown-toggle coupondropdown\" data-toggle=\"dropdown\"   id='coupondropdown", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "'>选择优惠券<span class=\"caret\"></span></button>" });
                str2 = string.Concat(new object[] { obj2, "<ul id=\"coupon", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "\" class=\"dropdown-menu\" role=\"menu\">" });
                if (!((this.GetUserCoupons.Rows.Count <= 0) || this.isbargain))
                {
                    obj2 = str;
                    str = string.Concat(new object[] { obj2, "<li><a onclick=\"Couponasetselect('", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "','不使用','0',0,'0')\"   value=\"0\">不使用</a></li>" });
                }
                if (!this.isbargain)
                {
                    num = 0;
                    while (num < this.GetUserCoupons.Rows.Count)
                    {
                        if ((this.GetUserCoupons.Rows[num]["MemberGrades"].ToString() == "0") || (this.GetUserCoupons.Rows[num]["MemberGrades"].ToString() == base.CurrentMemberInfo.GradeId.ToString()))
                        {
                            if (bool.Parse(this.GetUserCoupons.Rows[num]["IsAllProduct"].ToString()))
                            {
                                num2 = 0M;
                                foreach (ShoppingCartItemInfo info in list)
                                {
                                    if (info.Type == 0)
                                    {
                                        num2 += info.SubTotal;
                                    }
                                }
                                if (decimal.Parse(this.GetUserCoupons.Rows[num]["ConditionValue"].ToString()) <= num2)
                                {
                                    obj2 = str;
                                    str = string.Concat(new object[] { 
                                        obj2, "<li><a onclick=\"Couponasetselect('", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "','", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券','", this.GetUserCoupons.Rows[num]["CouponValue"], "',", this.GetUserCoupons.Rows[num]["Id"], ",'", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券|", this.GetUserCoupons.Rows[num]["Id"], "|", this.GetUserCoupons.Rows[num]["ConditionValue"], "|", 
                                        this.GetUserCoupons.Rows[num]["CouponValue"], "')\" id=\"acoupon", DataBinder.Eval(e.Item.DataItem, "TemplateId"), this.GetUserCoupons.Rows[num]["Id"], "\" value=\"", this.GetUserCoupons.Rows[num]["Id"], "\">", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券</a></li>"
                                     });
                                }
                            }
                            else
                            {
                                num2 = 0M;
                                bool flag = false;
                                foreach (ShoppingCartItemInfo info in list)
                                {
                                    if ((info.Type == 0) && (MemberProcessor.GetCouponByProducts(int.Parse(this.GetUserCoupons.Rows[num]["CouponId"].ToString()), info.ProductId).Rows.Count > 0))
                                    {
                                        num2 += info.SubTotal;
                                        flag = true;
                                    }
                                }
                                if (flag && (decimal.Parse(this.GetUserCoupons.Rows[num]["ConditionValue"].ToString()) <= num2))
                                {
                                    obj2 = str;
                                    str = string.Concat(new object[] { 
                                        obj2, "<li><a onclick=\"Couponasetselect('", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "','", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券','", this.GetUserCoupons.Rows[num]["CouponValue"], "',", this.GetUserCoupons.Rows[num]["Id"], ",'", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券|", this.GetUserCoupons.Rows[num]["Id"], "|", this.GetUserCoupons.Rows[num]["ConditionValue"], "|", 
                                        this.GetUserCoupons.Rows[num]["CouponValue"], "')\" id=\"acoupon", DataBinder.Eval(e.Item.DataItem, "TemplateId"), this.GetUserCoupons.Rows[num]["Id"], "\" value=\"", this.GetUserCoupons.Rows[num]["Id"], "\">", this.GetUserCoupons.Rows[num]["CouponValue"], "元现金券</a></li>"
                                     });
                                }
                            }
                        }
                        num++;
                    }
                }
                obj2 = str2 + str;
                str2 = string.Concat(new object[] { obj2, "</ul></div><input type=\"hidden\"  class=\"ClassCoupon\"   id=\"selectCoupon", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "\"/>  " });
                if (!string.IsNullOrEmpty(str))
                {
                    literal.Text = string.Concat(new object[] { str2, "<input type=\"hidden\"   id='selectCouponValue", DataBinder.Eval(e.Item.DataItem, "TemplateId"), "' class=\"selectCouponValue\" />" });
                }
                else
                {
                    literal.Text = "<input type=\"hidden\"   id='selectCouponValue" + DataBinder.Eval(e.Item.DataItem, "TemplateId") + "' class=\"selectCouponValue\" />";
                }
                decimal num3 = 0M;
                decimal num4 = 0M;
                decimal num5 = 0M;
                decimal num6 = 0M;
                decimal num7 = 0M;
                int num8 = 0;
                foreach (ShoppingCartItemInfo info2 in list)
                {
                    if (info2.Type == 0)
                    {
                        num7 += info2.SubTotal;
                        num8 += info2.Quantity;
                    }
                }
                num6 = num7;
                if (!this.isbargain)
                {
                    for (int i = 0; i < this.dtActivities.Rows.Count; i++)
                    {
                        if ((int.Parse(this.dtActivities.Rows[i]["attendTime"].ToString()) != 0) && (int.Parse(this.dtActivities.Rows[i]["attendTime"].ToString()) <= ActivityHelper.GetActivitiesMember(base.CurrentMemberInfo.UserId, int.Parse(this.dtActivities.Rows[i]["ActivitiesId"].ToString()))))
                        {
                            continue;
                        }
                        num2 = 0M;
                        int num10 = 0;
                        DataTable table2 = ActivityHelper.GetActivities_Detail(int.Parse(this.dtActivities.Rows[i]["ActivitiesId"].ToString()));
                        foreach (ShoppingCartItemInfo info2 in list)
                        {
                            if ((info2.Type == 0) && (ActivityHelper.GetActivitiesProducts(int.Parse(this.dtActivities.Rows[i]["ActivitiesId"].ToString()), info2.ProductId).Rows.Count > 0))
                            {
                                num2 += info2.SubTotal;
                                num10 += info2.Quantity;
                            }
                        }
                        bool flag2 = false;
                        if (table2.Rows.Count > 0)
                        {
                            for (num = 0; num < table2.Rows.Count; num++)
                            {
                                if (MemberHelper.CheckCurrentMemberIsInRange(table2.Rows[num]["MemberGrades"].ToString(), table2.Rows[num]["DefualtGroup"].ToString(), table2.Rows[num]["CustomGroup"].ToString(), base.CurrentMemberInfo.UserId))
                                {
                                    if (bool.Parse(this.dtActivities.Rows[i]["isAllProduct"].ToString()))
                                    {
                                        if (decimal.Parse(table2.Rows[num]["MeetMoney"].ToString()) > 0M)
                                        {
                                            if ((num7 != 0M) && (num7 >= decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["ReductionMoney"].ToString());
                                                literal6.Text = table2.Rows[table2.Rows.Count - 1]["bFreeShipping"].ToString();
                                                break;
                                            }
                                            if ((num7 != 0M) && (num7 <= decimal.Parse(table2.Rows[0]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[0]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[0]["ReductionMoney"].ToString());
                                                break;
                                            }
                                            if ((num7 != 0M) && (num7 >= decimal.Parse(table2.Rows[num]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[num]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[num]["ReductionMoney"].ToString());
                                                literal6.Text = table2.Rows[num]["bFreeShipping"].ToString();
                                            }
                                        }
                                        else
                                        {
                                            if ((num8 != 0) && (num8 >= int.Parse(table2.Rows[table2.Rows.Count - 1]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString());
                                                num5 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                literal6.Text = table2.Rows[table2.Rows.Count - 1]["bFreeShipping"].ToString();
                                                break;
                                            }
                                            if ((num8 != 0) && (num8 <= int.Parse(table2.Rows[0]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[0]["MeetMoney"].ToString());
                                                num5 = decimal.Parse(table2.Rows[0]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                break;
                                            }
                                            if ((num8 != 0) && (num8 >= int.Parse(table2.Rows[num]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[num]["MeetMoney"].ToString());
                                                num5 = decimal.Parse(table2.Rows[num]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                literal6.Text = table2.Rows[num]["bFreeShipping"].ToString();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        num7 = num2;
                                        num8 = num10;
                                        if (decimal.Parse(table2.Rows[num]["MeetMoney"].ToString()) > 0M)
                                        {
                                            if ((num7 != 0M) && (num7 >= decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["ReductionMoney"].ToString());
                                                literal6.Text = table2.Rows[table2.Rows.Count - 1]["bFreeShipping"].ToString();
                                                break;
                                            }
                                            if ((num7 != 0M) && (num7 <= decimal.Parse(table2.Rows[0]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[0]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[0]["ReductionMoney"].ToString());
                                                break;
                                            }
                                            if ((num7 != 0M) && (num7 >= decimal.Parse(table2.Rows[num]["MeetMoney"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[num]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[num]["ReductionMoney"].ToString());
                                                literal6.Text = table2.Rows[num]["bFreeShipping"].ToString();
                                            }
                                        }
                                        else
                                        {
                                            if ((num8 != 0) && (num8 >= int.Parse(table2.Rows[table2.Rows.Count - 1]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[table2.Rows.Count - 1]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                literal6.Text = table2.Rows[table2.Rows.Count - 1]["bFreeShipping"].ToString();
                                                break;
                                            }
                                            if ((num8 != 0) && (num8 <= int.Parse(table2.Rows[0]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[0]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[0]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                break;
                                            }
                                            if ((num8 != 0) && (num8 >= int.Parse(table2.Rows[num]["MeetNumber"].ToString())))
                                            {
                                                num4 = decimal.Parse(table2.Rows[num]["MeetMoney"].ToString());
                                                num3 = decimal.Parse(table2.Rows[num]["ReductionMoney"].ToString());
                                                flag2 = true;
                                                literal6.Text = table2.Rows[num]["bFreeShipping"].ToString();
                                            }
                                        }
                                    }
                                }
                            }
                            if (flag2)
                            {
                                if (num8 > 0)
                                {
                                    num5 += num3;
                                }
                            }
                            else if (((num7 != 0M) && (num4 != 0M)) && (num7 >= num4))
                            {
                                num5 += num3;
                            }
                        }
                    }
                }
                literal2.Text = num5.ToString("F2");
                literal3.Text = num5.ToString("F2");
                decimal num12 = num6 - num5;
                literal5.Text = num12.ToString("F2");
                literal4.Text = (num6 - num5).ToString("F2");
            }
        }
    }
}

