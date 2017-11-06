namespace Hidistro.UI.Web.Admin.Trade
{
    using ASPNET.WebControls;
    using Hidistro.ControlPanel.Sales;
    using Hidistro.ControlPanel.Store;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Core.Enums;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Promotions;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using Hidistro.UI.ControlPanel.Utility;
    using Hidistro.UI.Web.Admin.Ascx;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class BuyerAlreadyPaid : AdminPage
    {
        protected Button btnAcceptRefund;
        protected Button btnAcceptReplace;
        protected Button btnAcceptReturn;
        protected Button btnCloseOrder;
        protected Button btnDeleteCheck;
        protected Button btnExport;
        protected Button btnOrderGoods;
        protected Button btnProductGoods;
        protected Button btnRefuseRefund;
        protected Button btnRefuseReplace;
        protected Button btnRefuseReturn;
        protected Button btnRemark;
        protected Button btnSearchButton;
        protected ucDateTimePicker calendarEndDate;
        protected ucDateTimePicker calendarStartDate;
        protected CloseTranReasonDropDownList ddlCloseReason;
        protected RegionSelector dropRegion;
        protected HtmlInputHidden hidAdminRemark;
        protected HtmlInputHidden hidOrderId;
        protected HtmlInputHidden hidOrderTotal;
        protected HtmlInputHidden hidRefundMoney;
        protected HtmlInputHidden hidRefundType;
        protected PageSize hrefPageSize;
        protected Label lblAddress;
        protected Label lblContacts;
        protected Label lblEmail;
        protected Label lblOrderId;
        protected Label lblOrderTotal;
        protected FormatedMoneyLabel lblOrderTotalForRemark;
        protected Label lblRefundRemark;
        protected Label lblRefundType;
        protected Label lblStatus;
        protected Label lblTelephone;
        protected DropDownList OrderFromList;
        protected OrderRemarkImageRadioButtonList orderRemarkImageForRemark;
        protected Pager pager;
        protected Label replace_lblAddress;
        protected Label replace_lblComments;
        protected Label replace_lblContacts;
        protected Label replace_lblEmail;
        protected Label replace_lblOrderId;
        protected Label replace_lblOrderTotal;
        protected Label replace_lblPostCode;
        protected Label replace_lblTelephone;
        protected TextBox replace_txtAdminRemark;
        protected Label return_lblAddress;
        protected Label return_lblContacts;
        protected Label return_lblEmail;
        protected Label return_lblOrderId;
        protected Label return_lblOrderTotal;
        protected Label return_lblRefundType;
        protected Label return_lblReturnRemark;
        protected Label return_lblTelephone;
        protected TextBox return_txtAdminRemark;
        protected TextBox return_txtRefundMoney;
        protected string Reurl;
        protected Repeater rptList;
        protected int stype;
        protected TextBox txtAdminRemark;
        protected HtmlInputText txtcategoryId;
        protected TextBox txtOrderId;
        protected TextBox txtProductName;
        protected TextBox txtRemark;
        protected TextBox txtShopName;
        protected TextBox txtShopTo;
        protected TextBox txtUserName;

        protected BuyerAlreadyPaid()
            : base("m03", "ddp04")
        {
            this.Reurl = string.Empty;
        }

        private void BindOrders()
        {
            OrderQuery orderQuery = this.GetOrderQuery();
            DbQueryResult orders = OrderHelper.GetOrders(orderQuery);
            this.rptList.DataSource = orders.Data;
            this.rptList.DataBind();
            this.pager.TotalRecords = orders.TotalRecords;
            this.txtUserName.Text = orderQuery.UserName;
            this.txtOrderId.Text = orderQuery.OrderId;
            this.txtProductName.Text = orderQuery.ProductName;
            this.txtShopTo.Text = orderQuery.ShipTo;
            this.calendarStartDate.SelectedDate = orderQuery.StartDate;
            this.calendarEndDate.SelectedDate = orderQuery.EndDate;
            this.lblStatus.Text = ((int)orderQuery.Status).ToString();
            if (orderQuery.RegionId.HasValue)
            {
                this.dropRegion.SetSelectedRegionId(orderQuery.RegionId);
            }
            this.txtShopName.Text = orderQuery.StoreName;
        }

        private void bindOrderType()
        {
            int result = 0;
            int.TryParse(base.Request.QueryString["orderType"], out result);
            this.OrderFromList.SelectedIndex = result;
        }

        private void btnCloseOrder_Click(object sender, EventArgs e)
        {
            OrderInfo orderInfo = OrderHelper.GetOrderInfo(this.hidOrderId.Value);
            orderInfo.CloseReason = this.ddlCloseReason.SelectedValue;
            if ("请选择关闭的理由" == orderInfo.CloseReason)
            {
                this.ShowMsg("请选择关闭的理由", false);
            }
            else if (OrderHelper.CloseTransaction(orderInfo))
            {
                orderInfo.OnClosed();
                this.BindOrders();
                this.ShowMsg("关闭订单成功", true);
            }
            else
            {
                this.ShowMsg("关闭订单失败", false);
            }
        }

        protected void btnDeleteCheck_Click(object sender, EventArgs e)
        {
            string str = "";
            if (!string.IsNullOrEmpty(base.Request["CheckBoxGroup"]))
            {
                str = base.Request["CheckBoxGroup"];
            }
            if (str.Length <= 0)
            {
                this.ShowMsg("请先选择要删除的订单", false);
            }
            else
            {
                int num = OrderHelper.DeleteOrders("'" + str.Replace(",", "','") + "'");
                this.BindOrders();
                this.ShowMsg(string.Format("成功删除了{0}个订单", num), true);
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string str = Globals.RequestFormStr("CheckBoxGroup");
            if (string.IsNullOrEmpty(str))
            {
                this.ShowMsg("请选择需要导出的记录", false);
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("订单编号,买家会员名,买家支付宝账号,买家应付货款,买家应付邮费,买家支付积分,总金额,返点积分,买家实际支付金额,买家实际支付积分,订单状态,买家留言,收货人姓名,收货地址,运送方式,联系电话,联系手机,订单创建时间,订单付款时间,宝贝标题,宝贝种类,物流单号,物流公司,订单备注,宝贝总数量,分销商Id,分销商店铺名称,订单关闭原因,卖家服务费,买家服务费,发票抬头,定金排名,修改后的sku,修改后的收货地址,异常信息,卡券抵扣,积分抵扣,是否是O2O交易");
                builder.Append("\r\n");
                foreach (string str2 in str.Split(new char[] { ',' }))
                {
                    StringBuilder builder2 = new StringBuilder();
                    OrderInfo orderInfo = OrderHelper.GetOrderInfo(str2);
                    if (orderInfo != null)
                    {
                        builder2.Append(this.FormatOrderStr(orderInfo.OrderId));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.Username));
                        builder2.Append(",");
                        decimal total = orderInfo.GetTotal();
                        decimal adjustedFreight = orderInfo.AdjustedFreight;
                        decimal num4 = total - adjustedFreight;
                        builder2.Append("," + this.FormatOrderStr(num4.ToString("F2")));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.AdjustedFreight.ToString("F2")));
                        builder2.Append(",{买家支付积分}");
                        builder2.Append("," + this.FormatOrderStr(total.ToString("F2")));
                        builder2.Append("," + this.FormatOrderStr(""));
                        decimal num6 = total - adjustedFreight;
                        builder2.Append("," + this.FormatOrderStr(num6.ToString("F2")));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.PointExchange.ToString()));
                        string str3 = string.Empty;
                        switch (orderInfo.OrderStatus)
                        {
                            case OrderStatus.BuyerAlreadyPaid:
                                str3 = "已付款";
                                break;

                            case OrderStatus.SellerAlreadySent:
                                str3 = "已发货";
                                break;
                        }
                        builder2.Append("," + this.FormatOrderStr(str3));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.Remark.ToString()));
                        string str4 = string.Empty;
                        string oldAddress = orderInfo.OldAddress;
                        if (!string.IsNullOrEmpty(orderInfo.ShippingRegion))
                        {
                            str4 = orderInfo.ShippingRegion.Replace((char)0xff0c, ' ');
                        }
                        if (!string.IsNullOrEmpty(orderInfo.Address))
                        {
                            str4 = str4 + orderInfo.Address;
                        }
                        builder2.Append("," + this.FormatOrderStr(orderInfo.ShipTo.ToString()));
                        if (!string.IsNullOrEmpty(orderInfo.ZipCode))
                        {
                            str4 = str4 + " " + orderInfo.ZipCode;
                        }
                        if (!string.IsNullOrEmpty(orderInfo.TelPhone))
                        {
                            str4 = str4 + " " + orderInfo.TelPhone;
                        }
                        if (!string.IsNullOrEmpty(orderInfo.CellPhone))
                        {
                            str4 = str4 + " " + orderInfo.CellPhone;
                        }
                        string str6 = string.Empty;
                        if (string.IsNullOrEmpty(oldAddress))
                        {
                            builder2.Append("," + this.FormatOrderStr(str4));
                        }
                        else
                        {
                            str6 = str4;
                            builder2.Append("," + this.FormatOrderStr(oldAddress));
                        }
                        string realModeName = string.Empty;
                        if ((orderInfo.OrderStatus == OrderStatus.Finished) || (orderInfo.OrderStatus == OrderStatus.SellerAlreadySent))
                        {
                            realModeName = orderInfo.RealModeName;
                            if (string.IsNullOrEmpty(realModeName))
                            {
                                realModeName = orderInfo.ModeName;
                            }
                        }
                        else
                        {
                            realModeName = orderInfo.ModeName;
                        }
                        builder2.Append("," + this.FormatOrderStr(realModeName));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.TelPhone));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.CellPhone));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.OrderDate.ToString()));
                        string str8 = string.Empty;
                        if (orderInfo.PayDate.HasValue)
                        {
                            str8 = orderInfo.PayDate.ToString();
                        }
                        builder2.Append("," + this.FormatOrderStr(str8));
                        builder2.Append("," + this.FormatOrderStr("{宝贝标题}"));
                        builder2.Append("," + this.FormatOrderStr(""));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.ShipOrderNumber));
                        builder2.Append("," + this.FormatOrderStr(orderInfo.ExpressCompanyName));
                        builder2.Append("," + this.FormatOrderStr((orderInfo.ManagerRemark == null) ? "" : orderInfo.ManagerRemark.ToString()));
                        builder2.Append("," + this.FormatOrderStr("{宝贝总数量}"));
                        builder2.Append("," + this.FormatOrderStr((orderInfo.ReferralUserId > 0) ? orderInfo.ReferralUserId.ToString() : ""));
                        if (orderInfo.ReferralUserId > 0)
                        {
                            DistributorsInfo distributorInfo = DistributorsBrower.GetDistributorInfo(orderInfo.ReferralUserId);
                            if (distributorInfo != null)
                            {
                                builder2.Append("," + this.FormatOrderStr(""));
                            }
                            else
                            {
                                builder2.Append("," + this.FormatOrderStr(distributorInfo.StoreName));
                            }
                        }
                        else
                        {
                            builder2.Append("," + this.FormatOrderStr(""));
                        }
                        builder2.Append(",");
                        builder2.Append(",0");
                        builder2.Append(",0元");
                        builder2.Append(",");
                        builder2.Append(",");
                        builder2.Append(",");
                        builder2.Append(",");
                        builder2.Append("," + str6);
                        builder2.Append(",");
                        builder2.Append("," + ((orderInfo.CouponValue == 0M) ? "" : orderInfo.CouponValue.ToString()));
                        builder2.Append("," + ((orderInfo.PointToCash == 0M) ? "" : orderInfo.PointToCash.ToString()));
                        builder2.Append(",");
                        string str9 = builder2.ToString();
                        foreach (LineItemInfo info3 in orderInfo.LineItems.Values)
                        {
                            builder.Append(str9.Replace("{宝贝标题}", info3.ItemDescription).Replace("{宝贝总数量}", info3.Quantity.ToString())).Replace("{买家支付积分}", info3.PointNumber.ToString());
                            builder.Append("\r\n");
                        }
                    }
                }
                this.Page.Response.Clear();
                this.Page.Response.Buffer = false;
                this.Page.Response.Charset = "GB2312";
                this.Page.Response.AppendHeader("Content-Disposition", "attachment;filename=DataExport.csv");
                this.Page.Response.ContentType = "application/octet-stream";
                this.Page.Response.ContentEncoding = Encoding.GetEncoding("GB2312");
                this.Page.EnableViewState = false;
                this.Page.Response.Write(builder.ToString());
                this.Page.Response.End();
            }
        }

        private void btnOrderGoods_Click(object sender, EventArgs e)
        {
            string str = "";
            if (!string.IsNullOrEmpty(base.Request["CheckBoxGroup"]))
            {
                str = base.Request["CheckBoxGroup"];
            }
            if (str.Length <= 0)
            {
                this.ShowMsg("请选要下载配货表的订单", false);
            }
            else
            {
                List<string> list = new List<string>();
                foreach (string str2 in str.Split(new char[] { ',' }))
                {
                    list.Add("'" + str2 + "'");
                }
                DataSet orderGoods = OrderHelper.GetOrderGoods(string.Join(",", list.ToArray()));
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
                builder.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
                builder.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
                builder.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
                builder.AppendLine("<td>订单单号</td>");
                builder.AppendLine("<td>商品名称</td>");
                builder.AppendLine("<td>货号</td>");
                builder.AppendLine("<td>规格</td>");
                builder.AppendLine("<td>拣货数量</td>");
                builder.AppendLine("<td>现库存数</td>");
                builder.AppendLine("<td>备注</td>");
                builder.AppendLine("</tr>");
                foreach (DataRow row in orderGoods.Tables[0].Rows)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + row["OrderId"] + "</td>");
                    builder.AppendLine("<td>" + row["ProductName"] + "</td>");
                    builder.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + row["SKU"] + "</td>");
                    builder.AppendLine("<td>" + row["SKUContent"] + "</td>");
                    builder.AppendLine("<td>" + row["ShipmentQuantity"] + "</td>");
                    builder.AppendLine("<td>" + row["Stock"] + "</td>");
                    builder.AppendLine("<td>" + row["Remark"] + "</td>");
                    builder.AppendLine("</tr>");
                }
                builder.AppendLine("</table>");
                builder.AppendLine("</body></html>");
                base.Response.Clear();
                base.Response.Buffer = false;
                base.Response.Charset = "GB2312";
                base.Response.AppendHeader("Content-Disposition", "attachment;filename=ordergoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                base.Response.ContentEncoding = Encoding.GetEncoding("GB2312");
                base.Response.ContentType = "application/ms-excel";
                this.EnableViewState = false;
                base.Response.Write(builder.ToString());
                base.Response.End();
            }
        }

        private void btnProductGoods_Click(object sender, EventArgs e)
        {
            string str = "";
            if (!string.IsNullOrEmpty(base.Request["CheckBoxGroup"]))
            {
                str = base.Request["CheckBoxGroup"];
            }
            if (str.Length <= 0)
            {
                this.ShowMsg("请选要下载配货表的订单", false);
            }
            else
            {
                List<string> list = new List<string>();
                foreach (string str2 in str.Split(new char[] { ',' }))
                {
                    list.Add("'" + str2 + "'");
                }
                DataSet productGoods = OrderHelper.GetProductGoods(string.Join(",", list.ToArray()));
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
                builder.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
                builder.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
                builder.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
                builder.AppendLine("<td>商品名称</td>");
                builder.AppendLine("<td>货号</td>");
                builder.AppendLine("<td>规格</td>");
                builder.AppendLine("<td>拣货数量</td>");
                builder.AppendLine("<td>现库存数</td>");
                builder.AppendLine("</tr>");
                foreach (DataRow row in productGoods.Tables[0].Rows)
                {
                    builder.AppendLine("<tr>");
                    builder.AppendLine("<td>" + row["ProductName"] + "</td>");
                    builder.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + row["SKU"] + "</td>");
                    builder.AppendLine("<td>" + row["SKUContent"] + "</td>");
                    builder.AppendLine("<td>" + row["ShipmentQuantity"] + "</td>");
                    builder.AppendLine("<td>" + row["Stock"] + "</td>");
                    builder.AppendLine("</tr>");
                }
                builder.AppendLine("</table>");
                builder.AppendLine("</body></html>");
                base.Response.Clear();
                base.Response.Buffer = false;
                base.Response.Charset = "GB2312";
                base.Response.AppendHeader("Content-Disposition", "attachment;filename=productgoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
                base.Response.ContentEncoding = Encoding.GetEncoding("GB2312");
                base.Response.ContentType = "application/ms-excel";
                this.EnableViewState = false;
                base.Response.Write(builder.ToString());
                base.Response.End();
            }
        }

        private void btnRemark_Click(object sender, EventArgs e)
        {
            if (this.hidOrderId.Value == "0")
            {
                string str = Globals.RequestFormStr("CheckBoxGroup");
                if (str.Length <= 0)
                {
                    this.ShowMsg("请先选择要批量备注的订单", false);
                }
                else if (this.txtRemark.Text.Length > 300)
                {
                    this.ShowMsg("备注长度限制在300个字符以内", false);
                }
                else
                {
                    foreach (string str2 in str.Split(new char[] { ',' }))
                    {
                        if (!string.IsNullOrEmpty(str2))
                        {
                            OrderInfo orderInfo = OrderHelper.GetOrderInfo(str2);
                            orderInfo.OrderId = str2;
                            if (this.orderRemarkImageForRemark.SelectedItem != null)
                            {
                                orderInfo.ManagerMark = this.orderRemarkImageForRemark.SelectedValue;
                            }
                            orderInfo.ManagerRemark = Globals.HtmlEncode(this.txtRemark.Text);
                            OrderHelper.SaveRemark(orderInfo);
                        }
                    }
                    this.ShowMsg("批量保存备注成功", true);
                    this.BindOrders();
                }
            }
            else if (this.txtRemark.Text.Length > 300)
            {
                this.ShowMsg("备注长度限制在300个字符以内", false);
            }
            else
            {
                OrderInfo order = OrderHelper.GetOrderInfo(this.hidOrderId.Value);
                order.OrderId = this.hidOrderId.Value;
                if (this.orderRemarkImageForRemark.SelectedItem != null)
                {
                    order.ManagerMark = this.orderRemarkImageForRemark.SelectedValue;
                }
                order.ManagerRemark = Globals.HtmlEncode(this.txtRemark.Text);
                if (OrderHelper.SaveRemark(order))
                {
                    this.BindOrders();
                    this.ShowMsg("保存备注成功", true);
                }
                else
                {
                    this.ShowMsg("保存失败", false);
                }
            }
        }

        protected void btnSearchButton_Click(object sender, EventArgs e)
        {
            this.ReloadOrders(true);
        }

        private void btnSendGoods_Click(object sender, EventArgs e)
        {
            string str = "";
            if (!string.IsNullOrEmpty(base.Request["CheckBoxGroup"]))
            {
                str = base.Request["CheckBoxGroup"];
            }
            if (str.Length <= 0)
            {
                this.ShowMsg("请选要发货的订单", false);
            }
            else
            {
                this.Page.Response.Redirect(Globals.GetAdminAbsolutePath("/Sales/BatchSendOrderGoods.aspx?OrderIds=" + str));
            }
        }

        private string FormatOrderStr(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return str.Replace(",", "，").Replace("\n", "");
        }

        private OrderQuery GetOrderQuery()
        {
            int num2;
            int num3;
            OrderQuery query = new OrderQuery();
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["OrderId"]))
            {
                query.OrderId = Globals.UrlDecode(this.Page.Request.QueryString["OrderId"]);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["ProductName"]))
            {
                query.ProductName = Globals.UrlDecode(this.Page.Request.QueryString["ProductName"]);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["ShipTo"]))
            {
                query.ShipTo = Globals.UrlDecode(this.Page.Request.QueryString["ShipTo"]);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["UserName"]))
            {
                query.UserName = Globals.UrlDecode(this.Page.Request.QueryString["UserName"]);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["StartDate"]))
            {
                query.StartDate = new DateTime?(DateTime.Parse(this.Page.Request.QueryString["StartDate"]));
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["GroupBuyId"]))
            {
                query.GroupBuyId = new int?(int.Parse(this.Page.Request.QueryString["GroupBuyId"]));
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["EndDate"]))
            {
                query.EndDate = new DateTime?(DateTime.Parse(this.Page.Request.QueryString["EndDate"]).AddMilliseconds(86399.0));
            }
            query.Status = OrderStatus.BuyerAlreadyPaid;
            switch (this.stype)
            {
                case 1:
                    query.PaymentType = 0;
                    break;

                case 2:
                    query.OrderItemsStatus = Hidistro.Entities.Orders.OrderStatus.ApplyForRefund;// 6;
                    break;

                case 3:
                    query.IsPrinted = 0;
                    break;

                case 4:
                    query.IsPrinted = 1;
                    break;
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["ModeId"]))
            {
                int num = 0;
                if (int.TryParse(this.Page.Request.QueryString["ModeId"], out num))
                {
                    query.ShippingModeId = new int?(num);
                }
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["region"]) && int.TryParse(this.Page.Request.QueryString["region"], out num2))
            {
                query.RegionId = new int?(num2);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["UserId"]) && int.TryParse(this.Page.Request.QueryString["UserId"], out num3))
            {
                query.UserId = new int?(num3);
            }
            int result = 0;
            if (int.TryParse(base.Request.QueryString["orderType"], out result) && (result > 0))
            {
                query.Type = new OrderQuery.OrderType?((OrderQuery.OrderType)result);
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["StoreName"]))
            {
                query.StoreName = Globals.UrlDecode(this.Page.Request.QueryString["StoreName"]);
            }
            query.PageIndex = this.pager.PageIndex;
            query.PageSize = this.pager.PageSize;
            query.SortBy = "OrderDate";
            query.SortOrder = SortAction.Desc;
            return query;
        }

        protected string GetSpitLink(object oSplitState, object oOrderStatus, object oOrderID)
        {
            string str = string.Empty;
            if (Globals.ToNum(oSplitState) < 1)
            {
                string orderid = oOrderID.ToString();
                OrderStatus status = (OrderStatus)oOrderStatus;
                if ((status != OrderStatus.BuyerAlreadyPaid) && (status != OrderStatus.WaitBuyerPay))
                {
                    return str;
                }
                if (OrderHelper.GetItemNumByOrderID(orderid) > 1)
                {
                    str = "<a href='OrderSplit.aspx?OrderId=" + orderid + "&reurl=" + base.Server.UrlEncode(base.Request.Url.ToString()) + "' target='_blank' class='btn btn-default resetSize inputw100 bl mb5'>订单拆分</a>";
                }
            }
            return str;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.stype = Globals.RequestQueryNum("stype");
            switch (Globals.RequestFormStr("isCallback"))
            {
                case "true":
                    {
                        int num;
                        string str2;
                        string str3;
                        if (string.IsNullOrEmpty(base.Request["ReturnsId"]))
                        {
                            base.Response.Write("{\"Status\":\"0\"}");
                            base.Response.End();
                            return;
                        }
                        OrderInfo orderInfo = OrderHelper.GetOrderInfo(base.Request["orderId"]);
                        StringBuilder builder = new StringBuilder();
                        if (base.Request["type"] == "refund")
                        {
                            RefundHelper.GetRefundType(base.Request["orderId"], out num, out str3);
                        }
                        else
                        {
                            num = 0;
                            str3 = "";
                        }
                        if (num == 1)
                        {
                            str2 = "退到预存款";
                        }
                        else
                        {
                            str2 = "银行转帐";
                        }
                        builder.AppendFormat(",\"OrderTotal\":\"{0}\"", Globals.FormatMoney(orderInfo.GetTotal()));
                        if (!(base.Request["type"] == "replace"))
                        {
                            builder.AppendFormat(",\"RefundType\":\"{0}\"", num);
                            builder.AppendFormat(",\"RefundTypeStr\":\"{0}\"", str2);
                        }
                        builder.AppendFormat(",\"Contacts\":\"{0}\"", orderInfo.ShipTo);
                        builder.AppendFormat(",\"Email\":\"{0}\"", orderInfo.EmailAddress);
                        builder.AppendFormat(",\"Telephone\":\"{0}\"", (orderInfo.TelPhone + " " + orderInfo.CellPhone).Trim());
                        builder.AppendFormat(",\"Address\":\"{0}\"", orderInfo.Address);
                        builder.AppendFormat(",\"Remark\":\"{0}\"", str3.Replace("\r\n", ""));
                        builder.AppendFormat(",\"PostCode\":\"{0}\"", orderInfo.ZipCode);
                        builder.AppendFormat(",\"GroupBuyId\":\"{0}\"", (orderInfo.GroupBuyId > 0) ? orderInfo.GroupBuyId : 0);
                        base.Response.Clear();
                        base.Response.ContentType = "application/json";
                        base.Response.Write("{\"Status\":\"1\"" + builder.ToString() + "}");
                        base.Response.End();
                        break;
                    }
                case "GetStype":
                    {
                        base.Response.ContentType = "application/json";
                        DataTable allOrderID = OrderHelper.GetAllOrderID();
                        int length = allOrderID.Select(string.Concat(new object[] { "OrderStatus=", 2, " or (OrderStatus=", 1, " AND Gateway = 'hishop.plugins.payment.podrequest')" })).Length;
                        int num3 = allOrderID.Select("OrderStatus=" + 1 + " AND Gateway = 'hishop.plugins.payment.podrequest'").Length;
                        int countOrderIDByStatus = OrderHelper.GetCountOrderIDByStatus(Hidistro.Entities.Orders.OrderStatus.BuyerAlreadyPaid, Hidistro.Entities.Orders.OrderStatus.ApplyForRefund);//(2, 6);
                        int num5 = allOrderID.Select(string.Concat(new object[] { "(OrderStatus=", 2, " or (OrderStatus=", 1, " AND Gateway = 'hishop.plugins.payment.podrequest')) and IsPrinted=0" })).Length;
                        int num6 = allOrderID.Select(string.Concat(new object[] { "(OrderStatus=", 2, " or (OrderStatus=", 1, " AND Gateway = 'hishop.plugins.payment.podrequest')) and IsPrinted=1" })).Length;
                        string s = string.Concat(new object[] { "{\"type\":\"1\",\"buyalreadypaidcount\":", length, ",\"count1\":", num3, ",\"count2\":", countOrderIDByStatus, ",\"count3\":", num5, ",\"count4\":", num6, "}" });
                        base.Response.Write(s);
                        base.Response.End();
                        break;
                    }
            }
            this.Reurl = base.Request.Url.ToString();
            if (!this.Reurl.Contains("?"))
            {
                this.Reurl = this.Reurl + "?pageindex=1";
            }
            this.Reurl = Regex.Replace(this.Reurl, @"&t=(\d+)", "");
            this.Reurl = Regex.Replace(this.Reurl, @"(\?)t=(\d+)", "?");
            this.btnSearchButton.Click += new EventHandler(this.btnSearchButton_Click);
            this.btnRemark.Click += new EventHandler(this.btnRemark_Click);
            this.btnCloseOrder.Click += new EventHandler(this.btnCloseOrder_Click);
            this.btnDeleteCheck.Click += new EventHandler(this.btnDeleteCheck_Click);
            this.btnProductGoods.Click += new EventHandler(this.btnProductGoods_Click);
            this.btnOrderGoods.Click += new EventHandler(this.btnOrderGoods_Click);
            if (!this.Page.IsPostBack)
            {
                this.bindOrderType();
                this.BindOrders();
            }
        }

        private void ReloadOrders(bool isSearch)
        {
            NameValueCollection queryStrings = new NameValueCollection();
            queryStrings.Add("UserName", this.txtUserName.Text);
            queryStrings.Add("OrderId", this.txtOrderId.Text);
            queryStrings.Add("ProductName", this.txtProductName.Text);
            queryStrings.Add("ShipTo", this.txtShopTo.Text);
            queryStrings.Add("PageSize", this.pager.PageSize.ToString());
            queryStrings.Add("stype", this.stype.ToString());
            queryStrings.Add("OrderStatus", this.lblStatus.Text);
            if (this.calendarStartDate.SelectedDate.HasValue)
            {
                queryStrings.Add("StartDate", this.calendarStartDate.SelectedDate.Value.ToString());
            }
            if (this.calendarEndDate.SelectedDate.HasValue)
            {
                queryStrings.Add("EndDate", this.calendarEndDate.SelectedDate.Value.ToString());
            }
            if (!isSearch)
            {
                queryStrings.Add("pageIndex", this.pager.PageIndex.ToString());
            }
            if (!string.IsNullOrEmpty(this.Page.Request.QueryString["GroupBuyId"]))
            {
                queryStrings.Add("GroupBuyId", this.Page.Request.QueryString["GroupBuyId"]);
            }
            if (this.dropRegion.GetSelectedRegionId().HasValue)
            {
                queryStrings.Add("region", this.dropRegion.GetSelectedRegionId().Value.ToString());
            }
            queryStrings.Add("StoreName", this.txtShopName.Text.Trim());
            base.ReloadPage(queryStrings);
        }

        protected void rptList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            bool flag = false;
            OrderInfo orderInfo = OrderHelper.GetOrderInfo(e.CommandArgument.ToString());
            if (orderInfo != null)
            {
                if ((e.CommandName == "CONFIRM_PAY") && orderInfo.CheckAction(OrderActions.SELLER_CONFIRM_PAY))
                {
                    int num2 = 0;
                    int num3 = 0;
                    int groupBuyId = orderInfo.GroupBuyId;
                    if (OrderHelper.ConfirmPay(orderInfo))
                    {
                        DebitNoteInfo info2;
                        info2 = new DebitNoteInfo
                        {
                            NoteId = Globals.GetGenerateId(),
                            OrderId = e.CommandArgument.ToString(),
                            Operator = ManagerHelper.GetCurrentManager().UserName,
                            Remark = "后台" + ManagerHelper.GetCurrentManager().UserName + "收款成功"
                        };
                        OrderHelper.SaveDebitNote(info2);
                        if (orderInfo.GroupBuyId > 0)
                        {
                            int num4 = num2 + num3;
                        }
                        orderInfo.PayDate = new DateTime?(DateTime.Now);
                        this.BindOrders();
                        orderInfo.OnPayment();
                        this.ShowMsg("成功的确认了订单收款", true);
                    }
                    else
                    {
                        this.ShowMsg("确认订单收款失败", false);
                    }
                }
                else if ((e.CommandName == "FINISH_TRADE") && orderInfo.CheckAction(OrderActions.SELLER_FINISH_TRADE))
                {
                    Dictionary<string, LineItemInfo> lineItems = orderInfo.LineItems;
                    LineItemInfo info3 = new LineItemInfo();
                    foreach (KeyValuePair<string, LineItemInfo> pair in lineItems)
                    {
                        info3 = pair.Value;
                        if ((info3.OrderItemsStatus == OrderStatus.ApplyForRefund) || (info3.OrderItemsStatus == OrderStatus.ApplyForReturns))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        if (OrderHelper.ConfirmOrderFinish(orderInfo))
                        {
                            this.BindOrders();
                            DistributorsBrower.UpdateCalculationCommission(orderInfo);
                            foreach (LineItemInfo info4 in orderInfo.LineItems.Values)
                            {
                                if (info4.OrderItemsStatus.ToString() == OrderStatus.SellerAlreadySent.ToString())
                                {
                                    RefundHelper.UpdateOrderGoodStatu(orderInfo.OrderId, info4.SkuId, 5);
                                }
                            }
                            this.ShowMsg("成功的完成了该订单", true);
                        }
                        else
                        {
                            this.ShowMsg("完成订单失败", false);
                        }
                    }
                    else
                    {
                        this.ShowMsg("订单中商品有退货(款)不允许完成!", false);
                    }
                }
            }
        }

        protected void rptList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item) || (e.Item.ItemType == ListItemType.AlternatingItem))
            {
                Repeater repeater = (Repeater)e.Item.FindControl("rptSubList");
                OrderInfo orderInfo = OrderHelper.GetOrderInfo(DataBinder.Eval(e.Item.DataItem, "OrderID").ToString());
                if ((orderInfo != null) && (orderInfo.LineItems.Count > 0))
                {
                    repeater.DataSource = orderInfo.LineItems.Values;
                    repeater.DataBind();
                }
                OrderStatus status = (OrderStatus)DataBinder.Eval(e.Item.DataItem, "OrderStatus");
                string str = "";
                if (!(DataBinder.Eval(e.Item.DataItem, "Gateway") is DBNull))
                {
                    str = (string)DataBinder.Eval(e.Item.DataItem, "Gateway");
                }
                int num = (DataBinder.Eval(e.Item.DataItem, "GroupBuyId") != DBNull.Value) ? ((int)DataBinder.Eval(e.Item.DataItem, "GroupBuyId")) : 0;
                HtmlInputButton button = (HtmlInputButton)e.Item.FindControl("btnModifyPrice");
                HtmlInputButton button2 = (HtmlInputButton)e.Item.FindControl("btnSendGoods");
                Button button3 = (Button)e.Item.FindControl("btnPayOrder");
                Button button4 = (Button)e.Item.FindControl("btnConfirmOrder");
                HtmlInputButton button5 = (HtmlInputButton)e.Item.FindControl("btnCloseOrderClient");
                HtmlAnchor anchor = (HtmlAnchor)e.Item.FindControl("lkbtnCheckRefund");
                HtmlAnchor anchor2 = (HtmlAnchor)e.Item.FindControl("lkbtnCheckReturn");
                HtmlAnchor anchor3 = (HtmlAnchor)e.Item.FindControl("lkbtnCheckReplace");
                Literal literal = (Literal)e.Item.FindControl("WeiXinNickName");
                Literal literal2 = (Literal)e.Item.FindControl("litOtherInfo");
                int totalPointNumber = orderInfo.GetTotalPointNumber();
                MemberInfo member = MemberProcessor.GetMember(orderInfo.UserId, true);
                if (member != null)
                {
                    literal.Text = "买家：" + member.UserName;
                }
                StringBuilder builder = new StringBuilder();
                decimal total = orderInfo.GetTotal();
                if (total > 0M)
                {
                    builder.Append("<strong>￥" + total.ToString("F2") + "</strong>");
                    builder.Append("<small>(含运费￥" + orderInfo.AdjustedFreight.ToString("F2") + ")</small>");
                }
                if (totalPointNumber > 0)
                {
                    builder.Append("<small>" + totalPointNumber.ToString() + "积分</small>");
                }
                if (orderInfo.PaymentType == "货到付款")
                {
                    builder.Append("<span class=\"setColor bl\"><strong>货到付款</strong></span>");
                }
                if (string.IsNullOrEmpty(builder.ToString()))
                {
                    builder.Append("<strong>￥" + total.ToString("F2") + "</strong>");
                }
                literal2.Text = builder.ToString();
                switch (status)
                {
                    case OrderStatus.WaitBuyerPay:
                        button.Visible = true;
                        button.Attributes.Add("onclick", "DialogFrame('../trade/EditOrder.aspx?OrderId=" + DataBinder.Eval(e.Item.DataItem, "OrderID") + "&reurl='+ encodeURIComponent(goUrl),'修改订单价格',900,450)");
                        button5.Attributes.Add("onclick", "CloseOrderFun('" + DataBinder.Eval(e.Item.DataItem, "OrderID") + "')");
                        button5.Visible = true;
                        if (str != "hishop.plugins.payment.podrequest")
                        {
                            button3.Visible = true;
                        }
                        break;

                    case OrderStatus.ApplyForRefund:
                        anchor.Visible = true;
                        break;

                    case OrderStatus.ApplyForReturns:
                        anchor2.Visible = true;
                        break;

                    case OrderStatus.ApplyForReplacement:
                        anchor3.Visible = true;
                        break;
                }
                if (num > 0)
                {
                    GroupBuyStatus status2 = (GroupBuyStatus)DataBinder.Eval(e.Item.DataItem, "GroupBuyStatus");
                    button2.Visible = (status == OrderStatus.BuyerAlreadyPaid) && (status2 == GroupBuyStatus.Success);
                }
                else
                {
                    button2.Visible = (status == OrderStatus.BuyerAlreadyPaid) || ((status == OrderStatus.WaitBuyerPay) && (str == "hishop.plugins.payment.podrequest"));
                }
                button2.Attributes.Add("onclick", "DialogFrame('../trade/SendOrderGoods.aspx?OrderId=" + DataBinder.Eval(e.Item.DataItem, "OrderID") + "&reurl='+ encodeURIComponent(goUrl),'订单发货',750,220)");
                button4.Visible = status == OrderStatus.SellerAlreadySent;
            }
        }

        protected void rptSubList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item) || (e.Item.ItemType == ListItemType.AlternatingItem))
            {
                OrderCombineStatus status = (OrderCombineStatus)e.Item.FindControl("lbOrderCombineStatus");
                status.OrderID = DataBinder.Eval(e.Item.DataItem, "OrderID").ToString();
                status.SkuID = DataBinder.Eval(e.Item.DataItem, "SkuID").ToString();
                status.Type = Globals.ToNum(DataBinder.Eval(e.Item.DataItem, "Type").ToString());
                status.DetailUrl = "OrderDetails.aspx?OrderId=" + DataBinder.Eval(e.Item.DataItem, "OrderID").ToString() + "#returnInfo";
            }
        }
    }
}

