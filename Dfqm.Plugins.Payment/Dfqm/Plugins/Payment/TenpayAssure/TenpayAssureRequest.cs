namespace Dfqm.Plugins.Payment.TenpayAssure
{
    using Dfqm.Plugins;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;

    [Plugin("财付通担保交易")]
    public class TenpayAssureRequest : PaymentRequest
    {
        private const string Attach = "TenpayAssure";
        private const string Cmdno = "12";
        private const string EncodeType = "2";
        private const string Gatewayurl = "https://www.tenpay.com/cgi-bin/med/show_opentrans.cgi";
        private readonly string mch_desc;
        private readonly string mch_name;
        private readonly string mch_price;
        private readonly string mch_returl;
        private readonly string mch_vno;
        private const string MchType = "1";
        private const string NeedBuyerinfo = "2";
        private readonly string show_url;
        private const string Version = "2";

        public TenpayAssureRequest()
        {
            this.mch_price = "";
            this.mch_vno = "";
            this.mch_returl = "";
            this.show_url = "";
            this.mch_name = "";
            this.mch_desc = "";
        }

        public TenpayAssureRequest(string orderId, decimal amount, string subject, string body, string buyerEmail, DateTime date, string showUrl, string returnUrl, string notifyUrl, string attach)
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

        public override void SendGoods(string tradeno, string logisticsName, string invoiceno, string transportType)
        {
        }

        public override void SendRequest()
        {
            StringBuilder buf = new StringBuilder();
            Globals.AddParameter(buf, "attach", "TenpayAssure");
            Globals.AddParameter(buf, "chnid", this.Seller);
            Globals.AddParameter(buf, "cmdno", "12");
            Globals.AddParameter(buf, "encode_type", "2");
            Globals.AddParameter(buf, "mch_desc", this.mch_desc);
            Globals.AddParameter(buf, "mch_name", this.mch_name);
            Globals.AddParameter(buf, "mch_price", this.mch_price);
            Globals.AddParameter(buf, "mch_returl", this.mch_returl);
            Globals.AddParameter(buf, "mch_type", "1");
            Globals.AddParameter(buf, "mch_vno", this.mch_vno);
            Globals.AddParameter(buf, "need_buyerinfo", "2");
            Globals.AddParameter(buf, "seller", this.Seller);
            Globals.AddParameter(buf, "show_url", this.show_url);
            Globals.AddParameter(buf, "transport_desc", "");
            Globals.AddParameter(buf, "transport_fee", "0");
            Globals.AddParameter(buf, "version", "2");
            Globals.AddParameter(buf, "key", this.Key);
            string str = Globals.GetMD5(buf.ToString());
            string url = "https://www.tenpay.com/cgi-bin/med/show_opentrans.cgi?attach=TenpayAssure&chnid=" + this.Seller + "&cmdno=12&encode_type=2&mch_desc=" + this.mch_desc + "&mch_name=" + this.mch_name + "&mch_price=" + this.mch_price + "&mch_returl=" + this.mch_returl + "&mch_type=1&mch_vno=" + this.mch_vno + "&need_buyerinfo=2&seller=" + this.Seller + "&show_url=" + this.show_url + "&transport_desc=&transport_fee=0&version=2&sign=" + str;
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
        public string Seller { get; set; }

        public override string ShortDescription
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

