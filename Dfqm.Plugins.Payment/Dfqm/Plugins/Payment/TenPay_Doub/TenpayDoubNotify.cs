namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using Dfqm.Plugins;
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Xml;

    public class TenpayDoubNotify : PaymentNotify
    {
        private const string InputCharset = "utf-8";
        private readonly NameValueCollection parameters;

        public TenpayDoubNotify(NameValueCollection parameters)
        {
            this.parameters = parameters;
        }

        public override string GetGatewayOrderId()
        {
            return this.parameters["transaction_id"];
        }

        public override decimal GetOrderAmount()
        {
            if (!string.IsNullOrEmpty(this.parameters["total_fee"]))
            {
                return (decimal.Parse(this.parameters["total_fee"], CultureInfo.InvariantCulture) / 100M);
            }
            return 0.00M;
        }

        public override string GetOrderId()
        {
            return this.parameters["out_trade_no"];
        }

        public override void VerifyNotify(int timeout, string configXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(configXml);
            string innerText = document.FirstChild.SelectSingleNode("Key").InnerText;
            string parameterValue = document.FirstChild.SelectSingleNode("Partner").InnerText;
            ReturnHelper helper = new ReturnHelper(this.parameters) {
                Key = innerText
            };
            if (helper.isTenpaySign())
            {
                string str3 = helper.getParameter("notify_id");
                VeriflyHelper helper2 = new VeriflyHelper {
                    Key = innerText
                };
                helper2.setParameter("partner", parameterValue);
                helper2.setParameter("notify_id", str3);
                string str4 = "https://gw.tenpay.com/gateway/simpleverifynotifyid.xml";
                string result = "";
                if (!Globals.DoPost(str4 + helper2.getRequestURL(), "post", out result))
                {
                    this.OnNotifyVerifyFaild();
                }
                else
                {
                    ClientVerifly verifly = new ClientVerifly();
                    verifly.setContent(result);
                    verifly.key = innerText;
                    if (verifly.isTenpaySign() && (verifly.getParameter("retcode") == "0"))
                    {
                        string str6 = helper.getParameter("trade_mode");
                        string str7 = helper.getParameter("trade_state");
                        string str8 = str6;
                        if (str8 != null)
                        {
                            if (!(str8 == "1"))
                            {
                                if (str8 == "2")
                                {
                                    switch (str7)
                                    {
                                        case "0":
                                            this.OnPayment();
                                            break;

                                        case "5":
                                            this.OnFinished(true);
                                            break;
                                    }
                                }
                            }
                            else if (str7 == "0")
                            {
                                this.OnFinished(false);
                            }
                            else
                            {
                                this.OnNotifyVerifyFaild();
                            }
                        }
                    }
                }
            }
        }

        public override void WriteBack(HttpContext context, bool success)
        {
            if (context != null)
            {
                context.Response.Clear();
                context.Response.Write(success ? "success" : "fail");
                context.Response.End();
            }
        }
    }
}

