namespace Dfqm.Plugins.Payment.TenpayAssure
{
    using Dfqm.Plugins;
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Xml;

    public class TenpayAssureNotify : PaymentNotify
    {
        private readonly NameValueCollection parameters;

        public TenpayAssureNotify(NameValueCollection parameters)
        {
            this.parameters = parameters;
        }

        public override string GetGatewayOrderId()
        {
            return this.parameters["cft_tid"];
        }

        public override decimal GetOrderAmount()
        {
            return (decimal.Parse(this.parameters["total_fee"], CultureInfo.InvariantCulture) / 100M);
        }

        public override string GetOrderId()
        {
            return this.parameters["mch_vno"];
        }

        public override void VerifyNotify(int timeout, string configXml)
        {
            string parameterValue = this.parameters["version"];
            string str2 = this.parameters["cmdno"];
            string str3 = this.parameters["retcode"];
            string str4 = this.parameters["status"];
            string str5 = this.parameters["seller"];
            string str6 = this.parameters["total_fee"];
            string str7 = this.parameters["trade_price"];
            string str8 = this.parameters["transport_fee"];
            string str9 = this.parameters["buyer_id"];
            string str10 = this.parameters["chnid"];
            string str11 = this.parameters["cft_tid"];
            string str12 = this.parameters["mch_vno"];
            string str13 = this.parameters["attach"];
            string str14 = this.parameters["sign"];
            if (!str3.Equals("0"))
            {
                this.OnNotifyVerifyFaild();
            }
            else
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(configXml);
                StringBuilder buf = new StringBuilder();
                Globals.AddParameter(buf, "attach", str13);
                Globals.AddParameter(buf, "buyer_id", str9);
                Globals.AddParameter(buf, "cft_tid", str11);
                Globals.AddParameter(buf, "chnid", str10);
                Globals.AddParameter(buf, "cmdno", str2);
                Globals.AddParameter(buf, "mch_vno", str12);
                Globals.AddParameter(buf, "retcode", str3);
                Globals.AddParameter(buf, "seller", str5);
                Globals.AddParameter(buf, "status", str4);
                Globals.AddParameter(buf, "total_fee", str6);
                Globals.AddParameter(buf, "trade_price", str7);
                Globals.AddParameter(buf, "transport_fee", str8);
                Globals.AddParameter(buf, "version", parameterValue);
                Globals.AddParameter(buf, "key", document.FirstChild.SelectSingleNode("Key").InnerText);
                if (!str14.Equals(Globals.GetMD5(buf.ToString())))
                {
                    this.OnNotifyVerifyFaild();
                }
                else
                {
                    string str15 = str4;
                    if (str15 != null)
                    {
                        if (!(str15 == "3"))
                        {
                            if (str15 == "5")
                            {
                                this.OnFinished(true);
                            }
                        }
                        else
                        {
                            this.OnPayment();
                        }
                    }
                }
            }
        }

        public override void WriteBack(HttpContext context, bool success)
        {
        }
    }
}

