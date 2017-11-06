namespace Dfqm.Plugins.Payment.Npay
{
    using Dfqm.Plugins;
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.Security;
    using System.Xml;

    public class NpayNotify : PaymentNotify
    {
        private readonly NameValueCollection parameters;

        public NpayNotify(NameValueCollection parameters)
        {
            this.parameters = parameters;
        }

        public override string GetGatewayOrderId()
        {
            return string.Empty;
        }

        public override decimal GetOrderAmount()
        {
            return decimal.Parse(this.parameters["v_amount"]);
        }

        public override string GetOrderId()
        {
            return this.parameters["v_oid"];
        }

        public override void VerifyNotify(int timeout, string configXml)
        {
            string str = this.parameters["v_date"];
            string str2 = this.parameters["v_mid"];
            string str3 = this.parameters["v_oid"];
            string str4 = this.parameters["v_amount"];
            string str5 = this.parameters["v_status"];
            string str6 = this.parameters["v_md5"];
            if (((((str == null) || (str2 == null)) || ((str3 == null) || (str4 == null))) || (str5 == null)) || (str6 == null))
            {
                this.OnNotifyVerifyFaild();
            }
            else if (!str5.Equals("00"))
            {
                this.OnNotifyVerifyFaild();
            }
            else
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(configXml);
                string password = str + str2 + str3 + str4 + str5 + document.FirstChild.SelectSingleNode("Key").InnerText;
                if (!str6.Equals(FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5")))
                {
                    this.OnNotifyVerifyFaild();
                }
                else
                {
                    this.OnFinished(false);
                }
            }
        }

        public override void WriteBack(HttpContext context, bool success)
        {
        }
    }
}

