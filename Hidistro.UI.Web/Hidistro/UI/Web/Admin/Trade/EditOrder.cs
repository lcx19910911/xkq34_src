namespace Hidistro.UI.Web.Admin.Trade
{
    using Hidistro.ControlPanel.Sales;
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Store;
    using Hidistro.UI.ControlPanel.Utility;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Globalization;
    using System.Web.UI.WebControls;

    [PrivilegeCheck(Privilege.EditOrders)]
    public class EditOrder : AdminPage
    {
        protected Literal litAdjustedCommssion;
        protected Literal litItemTotalPrice;
        protected Literal litLogistic;
        protected Literal litOrderGoodsTotalPrice;
        protected Literal litOrderTotal;
        private OrderInfo order;
        protected string orderId;
        protected string ReUrl;
        protected Repeater rptItemList;
        protected TextBox txtAdjustedFreight;

        protected EditOrder() : base("m03", "ddp03")
        {
            this.ReUrl = Globals.RequestQueryStr("reurl");
        }

        protected string FormatAdjustedCommssion(object itemType, object itemAdjustedCommssion)
        {
            if (Globals.ToNum(itemType) == 0)
            {
                decimal num = decimal.Parse(itemAdjustedCommssion.ToString()) * -1M;
                return (" <input type=\"text\" name=\"adjustedcommssion\" value=\"" + num.ToString("F2") + "\" title=\"输入负数为优惠金额，正常输入为涨价\" maxlength=\"7\" />");
            }
            decimal num2 = decimal.Parse(itemAdjustedCommssion.ToString()) * -1M;
            decimal num3 = decimal.Parse(itemAdjustedCommssion.ToString()) * -1M;
            return (num2.ToString("F2") + " <input type=\"hidden\" name=\"adjustedcommssion\" value=\"" + num3.ToString("F2") + "\" />");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string str = Globals.RequestFormStr("posttype");
            if (string.IsNullOrEmpty(this.ReUrl))
            {
                this.ReUrl = "manageorder.aspx";
            }
            if (str == "updateorder")
            {
                string str2 = Globals.RequestFormStr("data");
                base.Response.ContentType = "application/json";
                string s = "{\"type\":\"0\",\"tips\":\"修改失败！\"}";
                JArray array = (JArray) JsonConvert.DeserializeObject(str2);
                OrderInfo order = null;
                foreach (JObject obj2 in array)
                {
                    string str4 = obj2["o"].ToString();
                    decimal num = decimal.Parse(obj2["f"].ToString());
                    JArray array2 = (JArray) obj2["data"];
                    if (array2.Count > 0)
                    {
                        string itemid = string.Empty;
                        decimal num2 = 0.00M;
                        if (!string.IsNullOrEmpty(str4))
                        {
                            order = OrderHelper.GetOrderInfo(str4);
                            if ((order != null) && (order.OrderStatus == OrderStatus.WaitBuyerPay))
                            {
                                foreach (JObject obj3 in array2)
                                {
                                    itemid = obj3["skuid"].ToString();
                                    num2 = decimal.Parse(obj3["adjustedcommssion"].ToString());
                                    OrderHelper.UpdateAdjustCommssions(str4, itemid, num2 * -1M);
                                }
                                if (((num >= 0M) && (order != null)) && (order.AdjustedFreight != num))
                                {
                                    order.AdjustedFreight = num;
                                    OrderHelper.UpdateOrder(order);
                                }
                                OrderHelper.UpdateCalculadtionCommission(str4);
                                s = "{\"type\":\"1\",\"tips\":\"订单价格修改成功！\"}";
                            }
                            else
                            {
                                s = "{\"type\":\"0\",\"tips\":\"当前订单状态不允许修改价格！\"}";
                            }
                        }
                    }
                }
                base.Response.Write(s);
                base.Response.End();
            }
            else
            {
                this.orderId = Globals.RequestQueryStr("OrderId");
                if (string.IsNullOrEmpty(this.orderId))
                {
                    base.GotoResourceNotFound();
                }
                else
                {
                    this.order = OrderHelper.GetOrderInfo(this.orderId);
                    if (this.order == null)
                    {
                        base.GotoResourceNotFound();
                    }
                    else
                    {
                        this.rptItemList.DataSource = this.order.LineItems.Values;
                        this.rptItemList.DataBind();
                        this.litLogistic.Text = string.IsNullOrEmpty(this.order.RealModeName) ? this.order.ModeName : this.order.RealModeName;
                        this.txtAdjustedFreight.Text = this.order.AdjustedFreight.ToString("F", CultureInfo.InvariantCulture);
                        decimal num3 = 0M;
                        if (!string.IsNullOrEmpty(this.order.ActivitiesName))
                        {
                            num3 += this.order.DiscountAmount;
                        }
                        if (!string.IsNullOrEmpty(this.order.ReducedPromotionName))
                        {
                            num3 += this.order.ReducedPromotionAmount;
                        }
                        if (!string.IsNullOrEmpty(this.order.CouponName))
                        {
                            num3 += this.order.CouponAmount;
                        }
                        if (!string.IsNullOrEmpty(this.order.RedPagerActivityName))
                        {
                            num3 += this.order.RedPagerAmount;
                        }
                        if (this.order.PointToCash > 0M)
                        {
                            num3 += this.order.PointToCash;
                        }
                        this.litOrderGoodsTotalPrice.Text = this.litItemTotalPrice.Text = this.order.GetAmount().ToString("F2");
                        this.litAdjustedCommssion.Text = this.order.GetTotalDiscountAverage().ToString("F2");
                        this.litOrderTotal.Text = (this.order.GetAmount() - num3).ToString("F", CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }
}

