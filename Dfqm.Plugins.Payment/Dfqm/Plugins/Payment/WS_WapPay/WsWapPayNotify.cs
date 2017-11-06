namespace Dfqm.Plugins.Payment.WS_WapPay
{
    using Dfqm.Plugins;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;
    using System.Xml;

    public class WsWapPayNotify : PaymentNotify
    {
        private const string InpuitCharset = "utf-8";
        private readonly NameValueCollection parameters;

        public WsWapPayNotify(NameValueCollection parameters)
        {
            this.parameters = parameters;
        }

        public override string GetGatewayOrderId()
        {
            return this.parameters["trade_no"];
        }

        public override decimal GetOrderAmount()
        {
            return decimal.Parse("0.00");
        }

        public override string GetOrderId()
        {
            return this.parameters["out_trade_no"];
        }

        public SortedDictionary<string, string> GetRequestGet()
        {
            SortedDictionary<string, string> dictionary = new SortedDictionary<string, string>();
            string str = HttpContext.Current.Request.Url.Query.Replace("?", "");
            if (!string.IsNullOrEmpty(str))
            {
                string[] strArray = str.Split(new char[] { '&' });
                string[] strArray2 = new string[0];
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray2 = strArray[i].Split(new char[] { '=' });
                    dictionary.Add(strArray2[0], strArray2[1]);
                }
            }
            return dictionary;
        }

        public override void VerifyNotify(int timeout, string configXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(configXml);
            SortedDictionary<string, string> requestGet = this.GetRequestGet();
            requestGet.Remove("HIGW");
            string str = Function.BuildMysign(requestGet, document.FirstChild.SelectSingleNode("Key").InnerText, "MD5", "utf-8");
            string str2 = this.parameters["sign"];
            if (!str2.Equals(str))
            {
                this.OnNotifyVerifyFaild();
            }
            else
            {
                string str3 = this.parameters["result"];
                if (!str3.Equals("success"))
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
            if (context != null)
            {
                context.Response.Clear();
                context.Response.Write(success ? "success" : "fail");
                context.Response.End();
            }
        }
    }
}

