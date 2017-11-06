namespace Dfqm.Plugins.Payment.ShengPayMobile
{
    using Dfqm.Plugins;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web;
    using System.Web.Security;

    [Plugin("盛付通移动支付")]
    public class ShengPayMobileRequest : PaymentRequest
    {
        private string BuyerContact;
        private string BuyerId;
        private string BuyerIP;
        private string BuyerName;
        private string Charset;
        private const string Currency = "CNY";
        private string Ext1;
        private string Ext2;
        private const string GatewayUrl = "http://api.shengpay.com/html5-gateway/pay.htm?page=mobile";
        private string InstCode;
        private string NotifyUrl;
        private string OrderAmount;
        private string OrderNo;
        private string OrderTime;
        private string PageUrl;
        private string PayerAuthTicket;
        private string PayerMobileNo;
        private string PayType;
        private string ProductDesc;
        private string ProductId;
        private string ProductName;
        private string ProductNum;
        private string ProductUrl;
        private string SellerId;
        private string SendTime;
        private const string ServiceCode = "B2CPayment";
        private string SignType;
        private string TraceNo;
        private string UnitPrice;
        private const string Version = "V4.1.1.1.1";

        public ShengPayMobileRequest()
        {
            this.Charset = "UTF-8";
            this.SignType = "MD5";
        }

        public ShengPayMobileRequest(string orderId, decimal amount, string subject, string body, string buyerEmail, DateTime orderTime, string showUrl, string returnUrl, string notifyUrl, string attach)
        {
            this.Charset = "UTF-8";
            this.SignType = "MD5";
            this.OrderNo = orderId;
            this.OrderAmount = amount.ToString("F2");
            this.PageUrl = returnUrl;
            this.NotifyUrl = notifyUrl;
            this.BuyerIP = GetUserIP();
            this.TraceNo = Guid.NewGuid().ToString("N");
            this.ProductUrl = showUrl;
            this.ProductName = body;
            this.ProductDesc = body;
            this.OrderTime = orderTime.ToString("yyyyMMddHHmmss");
        }

        public static string GetUserIP()
        {
            string userHostAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            switch (userHostAddress)
            {
                case null:
                case "":
                    userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    break;
            }
            if ((userHostAddress == null) || (userHostAddress == string.Empty))
            {
                userHostAddress = HttpContext.Current.Request.UserHostAddress;
            }
            if (userHostAddress.Length >= 20)
            {
                userHostAddress = "";
            }
            return userHostAddress;
        }

        public override void SendGoods(string tradeno, string logisticsName, string invoiceno, string transportType)
        {
            throw new NotImplementedException();
        }

        public override void SendRequest()
        {
            string strValue = FormsAuthentication.HashPasswordForStoringInConfigFile(("B2CPaymentV4.1.1.1.1" + this.Charset + this.TraceNo + this.SenderId + this.SendTime + this.OrderNo + this.OrderAmount + this.OrderTime + "CNY" + this.PageUrl + this.NotifyUrl + this.ProductId + this.ProductName + this.ProductNum + this.UnitPrice + this.ProductDesc + this.ProductUrl + this.SellerId + this.BuyerName + this.BuyerId + this.BuyerContact + this.BuyerIP + this.PayerMobileNo + this.PayerAuthTicket + this.Ext1 + this.Ext2 + this.SignType) + this.SellerKey, this.SignType);
            StringBuilder builder = new StringBuilder();
            builder.Append(this.CreateField("serviceCode", "B2CPayment"));
            builder.Append(this.CreateField("version", "V4.1.1.1.1"));
            builder.Append(this.CreateField("charset", this.Charset));
            builder.Append(this.CreateField("TraceNo", this.TraceNo));
            builder.Append(this.CreateField("senderId", this.SenderId));
            builder.Append(this.CreateField("sendTime", this.SendTime));
            builder.Append(this.CreateField("orderNo", this.OrderNo));
            builder.Append(this.CreateField("orderAmount", this.OrderAmount));
            builder.Append(this.CreateField("orderTime", this.OrderTime));
            builder.Append(this.CreateField("currency", "CNY"));
            builder.Append(this.CreateField("payType", this.PayType));
            builder.Append(this.CreateField("instCode", this.InstCode));
            builder.Append(this.CreateField("pageUrl", this.PageUrl));
            builder.Append(this.CreateField("notifyUrl", this.NotifyUrl));
            builder.Append(this.CreateField("productId", this.ProductId));
            builder.Append(this.CreateField("productName", this.ProductName));
            builder.Append(this.CreateField("productNum", this.ProductNum));
            builder.Append(this.CreateField("productDesc", this.ProductDesc));
            builder.Append(this.CreateField("productUrl", this.ProductUrl));
            builder.Append(this.CreateField("sellerId", this.SellerId));
            builder.Append(this.CreateField("buyerName", this.BuyerName));
            builder.Append(this.CreateField("buyerId", this.BuyerId));
            builder.Append(this.CreateField("buyerContact", this.BuyerContact));
            builder.Append(this.CreateField("buyerIp", this.BuyerIP));
            builder.Append(this.CreateField("payerMobileNo", this.PayerMobileNo));
            builder.Append(this.CreateField("payerAuthTicket", this.PayerAuthTicket));
            builder.Append(this.CreateField("ext1", this.Ext1));
            builder.Append(this.CreateField("ext2", this.Ext2));
            builder.Append(this.CreateField("signType", this.SignType));
            builder.Append(this.CreateField("SignMsg", strValue));
            this.SubmitPaymentForm(this.CreateForm(builder.ToString(), "http://api.shengpay.com/html5-gateway/pay.htm?page=mobile"));
        }

        public override string Description
        {
            get
            {
                return string.Empty;
            }
        }

        public override bool IsMedTrade
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Logo
        {
            get
            {
                return string.Empty;
            }
        }

        protected override bool NeedProtect
        {
            get
            {
                return true;
            }
        }

        [ConfigElement("商户密钥", Nullable=false)]
        public string SellerKey { get; set; }

        [ConfigElement("发送方标识", Nullable=false)]
        public string SenderId { get; set; }

        public override string ShortDescription
        {
            get
            {
                return "mobile";
            }
        }
    }
}

