namespace Dfqm.Plugins.Payment.ShengPay
{
    using Dfqm.Plugins;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web.Security;

    [Plugin("盛付通即时交易")]
    public class ShengPayRequest : PaymentRequest
    {
        private readonly string _amount;
        private readonly string _backUrl;
        private readonly string _bankCode;
        private readonly string _merchantUserId;
        private readonly string _notifyUrl;
        private readonly string _orderNo;
        private readonly string _orderTime;
        private readonly string _payChannel;
        private readonly string _postBackUrl;
        private readonly string _productDesc;
        private readonly string _productNo;
        private readonly string _productUrl;
        private readonly string _remark1;
        private readonly string _remark2;
        private const string CurrencyType = "RMB";
        private const string DefaultChannel = "";
        private const string GatewayUrl = "http://mas.sdo.com/web-acquire-channel/cashier30.htm";
        private const string NotifyUrlType = "http";
        private const string SignType = "2";
        private const string Version = "V4.1.2.1.1";

        public ShengPayRequest()
        {
            this._payChannel = "";
            this._amount = "";
            this._orderNo = "";
            this._postBackUrl = "";
            this._notifyUrl = "";
            this._backUrl = "";
            this._merchantUserId = "";
            this._productNo = "";
            this._productDesc = "";
            this._orderTime = "";
            this._remark1 = "";
            this._remark2 = "";
            this._bankCode = "";
            this._productUrl = "";
        }

        public ShengPayRequest(string orderId, decimal amount, string subject, string body, string buyerEmail, DateTime date, string showUrl, string returnUrl, string notifyUrl, string attach)
        {
            this._payChannel = "";
            this._amount = "";
            this._orderNo = "";
            this._postBackUrl = "";
            this._notifyUrl = "";
            this._backUrl = "";
            this._merchantUserId = "";
            this._productNo = "";
            this._productDesc = "";
            this._orderTime = "";
            this._remark1 = "";
            this._remark2 = "";
            this._bankCode = "";
            this._productUrl = "";
            this._orderNo = orderId;
            this._amount = amount.ToString("F2");
            this._postBackUrl = returnUrl;
            this._notifyUrl = notifyUrl;
            this._productUrl = showUrl;
            this._orderTime = date.ToString("yyyyMMddHHmmss");
        }

        public override void SendGoods(string tradeno, string logisticsName, string invoiceno, string transportType)
        {
        }

        public override void SendRequest()
        {
            string strValue = FormsAuthentication.HashPasswordForStoringInConfigFile(("V4.1.2.1.1" + this._amount + this._orderNo + this.MerchantNo + this._merchantUserId + this._payChannel + this._postBackUrl + this._notifyUrl + this._backUrl + this._orderTime + "RMBhttp2" + this._productNo + this._productDesc + this._remark1 + this._remark2 + this._bankCode + this._productUrl) + this.Key, "MD5");
            StringBuilder builder = new StringBuilder();
            builder.Append(this.CreateField("Version", "V4.1.2.1.1"));
            builder.Append(this.CreateField("Amount", this._amount));
            builder.Append(this.CreateField("OrderNo", this._orderNo));
            builder.Append(this.CreateField("PostBackUrl", this._postBackUrl));
            builder.Append(this.CreateField("NotifyUrl", this._notifyUrl));
            builder.Append(this.CreateField("BackUrl", this._backUrl));
            builder.Append(this.CreateField("MerchantNo", this.MerchantNo));
            builder.Append(this.CreateField("PayChannel", this._payChannel));
            builder.Append(this.CreateField("MerchantUserId", this._merchantUserId));
            builder.Append(this.CreateField("ProductNo", this._productNo));
            builder.Append(this.CreateField("ProductDesc", this._productDesc));
            builder.Append(this.CreateField("OrderTime", this._orderTime));
            builder.Append(this.CreateField("CurrencyType", "RMB"));
            builder.Append(this.CreateField("NotifyUrlType", "http"));
            builder.Append(this.CreateField("SignType", "2"));
            builder.Append(this.CreateField("Remark1", this._remark1));
            builder.Append(this.CreateField("Remark2", this._remark2));
            builder.Append(this.CreateField("BankCode", this._bankCode));
            builder.Append(this.CreateField("ProductUrl", this._productUrl));
            builder.Append(this.CreateField("DefaultChannel", ""));
            builder.Append(this.CreateField("MAC", strValue));
            this.SubmitPaymentForm(this.CreateForm(builder.ToString(), "http://mas.sdo.com/web-acquire-channel/cashier30.htm"));
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
                return false;
            }
        }

        [ConfigElement("商户密钥", Nullable=false)]
        public string Key { get; set; }

        public override string Logo
        {
            get
            {
                return string.Empty;
            }
        }

        [ConfigElement("商户号", Nullable=false)]
        public string MerchantNo { get; set; }

        protected override bool NeedProtect
        {
            get
            {
                return true;
            }
        }

        public override string ShortDescription
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

