namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using Dfqm.Plugins;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web;

    [Plugin("财付通双接口交易")]
    public class TenpayDoubRequest : PaymentRequest
    {
        private const string Attach = "TenpayAssure";
        private const string Gatewayurl = "https://gw.tenpay.com/gateway/pay.htm";
        private readonly string mch_desc;
        private readonly string mch_name;
        private readonly string mch_price;
        private readonly string mch_returl;
        private readonly string mch_vno;
        private readonly string show_url;

        public TenpayDoubRequest()
        {
            this.mch_price = "";
            this.mch_vno = "";
            this.mch_returl = "";
            this.show_url = "";
            this.mch_name = "";
            this.mch_desc = "";
        }

        public TenpayDoubRequest(string orderId, decimal amount, string subject, string body, string buyerEmail, DateTime date, string showUrl, string returnUrl, string notifyUrl, string attach)
        {
            this.mch_price = "";
            this.mch_vno = "";
            this.mch_returl = "";
            this.show_url = "";
            this.mch_name = "";
            this.mch_desc = "";
            this.mch_price = Convert.ToInt32((decimal) (amount * 100M)).ToString(CultureInfo.InvariantCulture);
            this.mch_vno = orderId;
            this.mch_returl = notifyUrl;
            this.show_url = returnUrl;
            this.mch_name = subject;
            this.mch_desc = body;
        }

        private static string getRealIp()
        {
            if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
            {
                return HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
        }

        public override void SendGoods(string tradeno, string logisticsName, string invoiceno, string transportType)
        {
        }

        public override void SendRequest()
        {
            StringBuilder buf = new StringBuilder();
            Globals.AddParameter(buf, "attach", "TenpayAssure");
            Globals.AddParameter(buf, "bank_type", "DEFAULT");
            Globals.AddParameter(buf, "body", this.mch_desc);
            Globals.AddParameter(buf, "fee_type", "1");
            Globals.AddParameter(buf, "input_charset", "UTF-8");
            Globals.AddParameter(buf, "notify_url", this.mch_returl);
            Globals.AddParameter(buf, "out_trade_no", this.mch_vno);
            Globals.AddParameter(buf, "partner", this.Partner);
            Globals.AddParameter(buf, "return_url", this.show_url);
            Globals.AddParameter(buf, "spbill_create_ip", getRealIp());
            Globals.AddParameter(buf, "subject", this.mch_name);
            Globals.AddParameter(buf, "total_fee", this.mch_price);
            Globals.AddParameter(buf, "trade_mode", "3");
            Globals.AddParameter(buf, "trans_type", "1");
            Globals.AddParameter(buf, "transport_fee", "0");
            Globals.AddParameter(buf, "key", this.Key);
            string str = Globals.GetMD5(buf.ToString());
            string url = "https://gw.tenpay.com/gateway/pay.htm?attach=TenpayAssure&bank_type=DEFAULT&body=" + this.mch_desc + "&fee_type=1&input_charset=UTF-8&notify_url=" + this.mch_returl + "&out_trade_no=" + this.mch_vno + "&partner=" + this.Partner + "&return_url=" + this.show_url + "&spbill_create_ip=" + getRealIp() + "&subject=" + this.mch_name + "&total_fee=" + this.mch_price + "&trade_mode=3&trans_type=1&transport_fee=0&sign=" + str;
            this.RedirectToGateway(url);
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
                return true;
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

        protected override bool NeedProtect
        {
            get
            {
                return true;
            }
        }

        [ConfigElement("商户号", Nullable=false)]
        public string Partner { get; set; }

        public override string ShortDescription
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

