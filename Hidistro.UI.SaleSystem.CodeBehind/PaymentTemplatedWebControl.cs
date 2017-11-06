namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.Core;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Sales;
    using Hidistro.SaleSystem.Vshop;
    using Hidistro.UI.Common.Controls;
    using Hishop.Plugins;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(true), PersistChildren(false)]
    public abstract class PaymentTemplatedWebControl : SimpleTemplatedWebControl
    {
        protected decimal Amount;
        protected string Gateway;
        private readonly bool isBackRequest;
        protected PaymentNotify Notify;
        protected OrderInfo Order;
        protected string OrderId;
        protected List<OrderInfo> orderlist;

        public PaymentTemplatedWebControl(bool _isBackRequest)
        {
            this.isBackRequest = _isBackRequest;
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            if (!this.isBackRequest)
            {
                if (!base.LoadHtmlThemedControl())
                {
                    throw new SkinNotFoundException(this.SkinPath);
                }
                this.AttachChildControls();
            }
            this.DoValidate();
        }

        protected abstract void DisplayMessage(string status);
        private void DoValidate()
        {
            NameValueCollection values2 = new NameValueCollection();
            values2.Add(this.Page.Request.Form);
            values2.Add(this.Page.Request.QueryString);
            NameValueCollection parameters = values2;
            this.Gateway = "hishop.plugins.payment.ws_wappay.wswappayrequest";
            this.Notify = PaymentNotify.CreateInstance(this.Gateway, parameters);
            Globals.Debuglog("订单支付：0-" + JsonConvert.SerializeObject(this.Notify), "_Debuglog.txt");
            if (this.isBackRequest)
            {
                this.Notify.ReturnUrl = Globals.FullPath("/pay/PaymentReturn_url.aspx") + "?" + this.Page.Request.Url.Query;
            }
            Globals.Debuglog("订单支付：1-" + JsonConvert.SerializeObject(this.Notify), "_Debuglog.txt");
            this.OrderId = this.Notify.GetOrderId();
            this.orderlist = ShoppingProcessor.GetOrderMarkingOrderInfo(this.OrderId);
            if (this.orderlist.Count != 0)
            {
                int modeId = 0;
                foreach (OrderInfo info in this.orderlist)
                {
                    this.Amount += info.GetAmount();
                    info.GatewayOrderId = this.Notify.GetGatewayOrderId();
                    modeId = info.PaymentTypeId;
                }
                PaymentModeInfo paymentMode = ShoppingProcessor.GetPaymentMode(modeId);
                if (paymentMode == null)
                {
                    this.ResponseStatus(true, "gatewaynotfound");
                }
                else
                {
                    this.Notify.Finished += new EventHandler<FinishedEventArgs>(this.Notify_Finished);
                    this.Notify.NotifyVerifyFaild += new EventHandler(this.Notify_NotifyVerifyFaild);
                    this.Notify.Payment += new EventHandler(this.Notify_Payment);
                    string configXml = HiCryptographer.Decrypt(paymentMode.Settings);
                    this.Notify.VerifyNotify(0x7530, configXml);
                }
            }
        }

        private void FinishOrder()
        {
            foreach (OrderInfo info in this.orderlist)
            {
                if (info.OrderStatus == OrderStatus.Finished)
                {
                    this.ResponseStatus(true, "success");
                    return;
                }
            }
            foreach (OrderInfo info in this.orderlist)
            {
                if (info.CheckAction(OrderActions.BUYER_CONFIRM_GOODS) && MemberProcessor.ConfirmOrderFinish(info))
                {
                    this.ResponseStatus(true, "success");
                }
                else
                {
                    this.ResponseStatus(false, "fail");
                }
            }
        }

        private void Notify_Finished(object sender, FinishedEventArgs e)
        {
            if (e.IsMedTrade)
            {
                this.FinishOrder();
            }
            else
            {
                this.UserPayOrder();
            }
        }

        private void Notify_NotifyVerifyFaild(object sender, EventArgs e)
        {
            this.ResponseStatus(false, "verifyfaild");
        }

        private void Notify_Payment(object sender, EventArgs e)
        {
            this.UserPayOrder();
        }

        private void ResponseStatus(bool success, string status)
        {
            if (this.isBackRequest)
            {
                this.Notify.WriteBack(HttpContext.Current, success);
            }
            else
            {
                this.DisplayMessage(status);
            }
        }

        private void UserPayOrder()
        {
            foreach (OrderInfo info in this.orderlist)
            {
                if (info.OrderStatus == OrderStatus.BuyerAlreadyPaid)
                {
                    this.ResponseStatus(true, "success");
                    return;
                }
            }
            foreach (OrderInfo info in this.orderlist)
            {
                if (info.CheckAction(OrderActions.BUYER_PAY) && MemberProcessor.UserPayOrder(info))
                {
                    info.OnPayment();
                    this.ResponseStatus(true, "success");
                }
                else
                {
                    this.ResponseStatus(false, "fail");
                }
            }
        }
    }
}

