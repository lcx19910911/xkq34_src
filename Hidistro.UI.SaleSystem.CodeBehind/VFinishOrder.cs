namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.ControlPanel.Commodities;
    using Hidistro.ControlPanel.Sales;
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Sales;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using Hishop.Plugins;
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    [ParseChildren(true)]
    public class VFinishOrder : VMemberTemplatedWebControl
    {
        private HtmlAnchor btnToPay;
        private Literal literalOrderTotal;
        private Literal litHelperText;
        private Literal litMessage;
        private Literal litOrderId;
        private Literal litOrderTotal;
        private HtmlInputHidden litPaymentType;
        private string orderId;

        protected override void AttachChildControls()
        {
            this.orderId = this.Page.Request.QueryString["orderId"];
            List<OrderInfo> orderMarkingOrderInfo = ShoppingProcessor.GetOrderMarkingOrderInfo(this.orderId);
            decimal amount = 0M;
            if (orderMarkingOrderInfo.Count == 0)
            {
                this.Page.Response.Redirect("/Vshop/MemberOrders.aspx?status=0");
            }
            bool flag = true;
            foreach (OrderInfo info in orderMarkingOrderInfo)
            {
                amount += info.GetTotal();
                foreach (LineItemInfo info2 in info.LineItems.Values)
                {
                    if (info2.Type == 0)
                    {
                        flag = false;
                    }
                    foreach (LineItemInfo info3 in info.LineItems.Values)
                    {
                        if (!ProductHelper.GetProductHasSku(info3.SkuId, info3.Quantity))
                        {
                            info.OrderStatus = OrderStatus.Closed;
                            info.CloseReason = "库存不足";
                            OrderHelper.UpdateOrder(info);
                            HttpContext.Current.Response.Write("<script>alert('库存不足，订单自动关闭！');location.href='/Vshop/MemberOrders.aspx'</script>");
                            HttpContext.Current.Response.End();
                            return;
                        }
                    }
                }
            }
            if (!(string.IsNullOrEmpty(orderMarkingOrderInfo[0].Gateway) || !(orderMarkingOrderInfo[0].Gateway == "hishop.plugins.payment.offlinerequest")))
            {
                this.litMessage = (Literal) this.FindControl("litMessage");
                this.litMessage.SetWhenIsNotNull(SettingsManager.GetMasterSettings(false).OffLinePayContent);
            }
            this.btnToPay = (HtmlAnchor) this.FindControl("btnToPay");
            if (!(string.IsNullOrEmpty(orderMarkingOrderInfo[0].Gateway) || !(orderMarkingOrderInfo[0].Gateway == "hishop.plugins.payment.weixinrequest")))
            {
                this.btnToPay.Visible = true;
                this.btnToPay.HRef = "~/pay/wx_Submit.aspx?orderId=" + this.orderId;
            }
            if (((!string.IsNullOrEmpty(orderMarkingOrderInfo[0].Gateway) && (orderMarkingOrderInfo[0].Gateway != "hishop.plugins.payment.podrequest")) && (orderMarkingOrderInfo[0].Gateway != "hishop.plugins.payment.offlinerequest")) && (orderMarkingOrderInfo[0].Gateway != "hishop.plugins.payment.weixinrequest"))
            {
                PaymentModeInfo paymentMode = ShoppingProcessor.GetPaymentMode(orderMarkingOrderInfo[0].PaymentTypeId);
                string attach = "";
                string showUrl = string.Format("http://{0}/vshop/", HttpContext.Current.Request.Url.Host);
                PaymentRequest.CreateInstance(paymentMode.Gateway, HiCryptographer.Decrypt(paymentMode.Settings), this.orderId, amount, "订单支付", "订单号-" + this.orderId, orderMarkingOrderInfo[0].EmailAddress, orderMarkingOrderInfo[0].OrderDate, showUrl, Globals.FullPath("/pay/PaymentReturn_url.aspx"), Globals.FullPath("/pay/PaymentNotify_url.aspx"), attach).SendRequest();
            }
            else
            {
                this.litOrderId = (Literal) this.FindControl("litOrderId");
                this.litOrderTotal = (Literal) this.FindControl("litOrderTotal");
                this.literalOrderTotal = (Literal) this.FindControl("literalOrderTotal");
                this.litPaymentType = (HtmlInputHidden) this.FindControl("litPaymentType");
                int result = 0;
                this.litPaymentType.SetWhenIsNotNull("0");
                if (int.TryParse(this.Page.Request.QueryString["PaymentType"], out result))
                {
                    this.litPaymentType.SetWhenIsNotNull(result.ToString());
                }
                this.litOrderId.SetWhenIsNotNull(this.orderId);
                if (flag)
                {
                    this.litOrderTotal.SetWhenIsNotNull("您需要支付：\x00a5" + amount.ToString("F2"));
                }
                this.literalOrderTotal.SetWhenIsNotNull("订单金额：<span style='color:red'>\x00a5" + amount.ToString("F2") + "</span>");
                this.litHelperText = (Literal) this.FindControl("litHelperText");
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                this.litHelperText.SetWhenIsNotNull(masterSettings.OffLinePayContent);
                PageTitle.AddSiteNameTitle("下单成功");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VFinishOrder.html";
            }
            base.OnInit(e);
        }
    }
}

