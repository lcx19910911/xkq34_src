namespace Dfqm.Plugins.Payment.BankUnion
{
    using Dfqm.Plugins;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web;

    [Plugin("银联在线")]
    public class BankUnionRequest : PaymentRequest
    {
        private const string Gateway = "https://unionpaysecure.com/api/Pay.action";
        private const string Remark1 = "Bankunion";
        private readonly string v_amount;
        private readonly string v_date;
        private const string v_moneytype = "CNY";
        private readonly string v_notifyUrl;
        private readonly string v_oid;
        private readonly string v_returnUrl;

        public BankUnionRequest()
        {
            this.v_oid = "";
            this.v_amount = "";
            this.v_date = "";
            this.v_returnUrl = "";
            this.v_notifyUrl = "";
        }

        public BankUnionRequest(string orderId, decimal amount, string subject, string body, string buyerEmail, DateTime date, string showUrl, string returnUrl, string notifyUrl, string attach)
        {
            this.v_oid = "";
            this.v_amount = "";
            this.v_date = "";
            this.v_returnUrl = "";
            this.v_notifyUrl = "";
            this.v_oid = orderId;
            this.v_amount = Math.Round((decimal) (amount * 100M), 0).ToString();
            this.v_returnUrl = returnUrl;
            this.v_date = date.ToString("yyyyMMddHHmmss");
            this.v_notifyUrl = notifyUrl;
        }

        public override void SendGoods(string tradeno, string logisticsName, string invoiceno, string transportType)
        {
        }

        public override void SendRequest()
        {
            QuickPayConf.merCode = this.Vmid;
            QuickPayConf.merName = this.VmName;
            QuickPayConf.securityKey = this.Key;
            this.ValueVo = new string[] { 
                QuickPayConf.version, QuickPayConf.charset, "01", "", QuickPayConf.merCode, QuickPayConf.merName, "", "", "", "", this.v_amount, "1", "0", "0", this.v_oid, this.v_amount, 
                "156", this.v_date, "127.0.0.1", "", "", "", "", this.v_returnUrl, this.v_notifyUrl, ""
             };
            QuickPayConf.gateWay = "https://unionpaysecure.com/api/Pay.action";
            this.SubmitPaymentForm("");
        }

        protected override void SubmitPaymentForm(string formContent)
        {
            string s = new QuickPayUtils().createPayHtml(this.ValueVo);
            HttpContext.Current.Response.ContentType = "text/html;charset=" + QuickPayConf.charset;
            HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding(QuickPayConf.charset);
            try
            {
                HttpContext.Current.Response.Write(s);
            }
            catch (Exception)
            {
            }
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

        private string[] ValueVo { get; set; }

        [ConfigElement("商户号", Nullable=false)]
        public string Vmid { get; set; }

        [ConfigElement("商户名称", Nullable=false)]
        public string VmName { get; set; }
    }
}

