namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    internal static class Globals
    {
        internal static StringBuilder AddParameter(StringBuilder buf, string parameterName, string parameterValue)
        {
            if ((parameterValue != null) && !"".Equals(parameterValue))
            {
                if ("".Equals(buf.ToString()))
                {
                    buf.Append(parameterName);
                    buf.Append("=");
                    buf.Append(parameterValue);
                }
                else
                {
                    buf.Append("&");
                    buf.Append(parameterName);
                    buf.Append("=");
                    buf.Append(parameterValue);
                }
            }
            return buf;
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static bool DoPost(string url, string method, out string result)
        {
            StreamReader reader = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            try
            {
                string s = null;
                if (method.ToUpper() == "POST")
                {
                    string[] strArray = Regex.Split(url, @"\?");
                    request = (HttpWebRequest) WebRequest.Create(strArray[0]);
                    if (strArray.Length >= 2)
                    {
                        s = strArray[1];
                    }
                }
                else
                {
                    request = (HttpWebRequest) WebRequest.Create(url);
                }
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Globals.CheckValidationResult);
                request.Timeout = 0x1d4c0;
                Encoding encoding = Encoding.GetEncoding("gb2312");
                if (s != null)
                {
                    byte[] bytes = encoding.GetBytes(s);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = bytes.Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                response = (HttpWebResponse) request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), encoding);
                result = reader.ReadToEnd();
                reader.Close();
                response.Close();
            }
            catch (Exception)
            {
                result = "erro";
                return false;
            }
            return true;
        }

        internal static string GetMD5(string encypStr)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(encypStr);
            return BitConverter.ToString(provider.ComputeHash(bytes)).Replace("-", "").ToUpper();
        }

        public static string UrlEncode(string instr, string charset)
        {
            if ((instr == null) || (instr.Trim() == ""))
            {
                return "";
            }
            try
            {
                return HttpUtility.UrlEncode(instr, Encoding.UTF8);
            }
            catch (Exception)
            {
                return HttpUtility.UrlEncode(instr, Encoding.UTF8);
            }
        }
    }
}

