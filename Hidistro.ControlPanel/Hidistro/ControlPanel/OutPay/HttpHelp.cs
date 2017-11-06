namespace Hidistro.ControlPanel.OutPay
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web;

    public class HttpHelp
    {
        public string errstr = "";

        public string BuildGetUrl(string url, IDictionary<string, string> parameters)
        {
            if ((parameters != null) && (parameters.Count > 0))
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                    return url;
                }
                url = url + "?" + BuildQuery(parameters);
            }
            return url;
        }

        public static string BuildQuery(IDictionary<string, string> parameters)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            IEnumerator<KeyValuePair<string, string>> enumerator = parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, string> current = enumerator.Current;
                string key = current.Key;
                current = enumerator.Current;
                string str2 = current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(str2))
                {
                    if (flag)
                    {
                        builder.Append("&");
                    }
                    builder.Append(key);
                    builder.Append("=");
                    builder.Append(HttpUtility.UrlEncode(str2, Encoding.UTF8));
                    flag = true;
                }
            }
            return builder.ToString();
        }

        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public string DoGet(string url, IDictionary<string, string> parameters)
        {
            if ((parameters != null) && (parameters.Count > 0))
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters);
                }
            }
            HttpWebRequest request = this.GetWebRequest(url, "GET", null, null);
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            HttpWebResponse rsp = (HttpWebResponse) request.GetResponse();
            return this.GetResponseAsString(rsp, Encoding.UTF8);
        }

        public string DoPost(string url, IDictionary<string, string> parameters)
        {
            HttpWebRequest request = this.GetWebRequest(url, "POST", null, null);
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes(BuildQuery(parameters));
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse rsp = (HttpWebResponse) request.GetResponse();
            return this.GetResponseAsString(rsp, Encoding.UTF8);
        }

        public string DoPost(string url, string value, [Optional, DefaultParameterValue(null)] string cerPassword, [Optional, DefaultParameterValue(null)] string cerPath)
        {
            HttpWebRequest request = this.GetWebRequest(url, "POST", cerPassword, cerPath);
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse rsp = (HttpWebResponse) request.GetResponse();
            return this.GetResponseAsString(rsp, Encoding.UTF8);
        }

        public string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream responseStream = null;
            StreamReader reader = null;
            string str;
            try
            {
                responseStream = rsp.GetResponseStream();
                reader = new StreamReader(responseStream, encoding);
                str = reader.ReadToEnd();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
                if (rsp != null)
                {
                    rsp.Close();
                }
            }
            return str;
        }

        public HttpWebRequest GetWebRequest(string url, string method, [Optional, DefaultParameterValue(null)] string cerPassword, [Optional, DefaultParameterValue(null)] string cerPath)
        {
            HttpWebRequest request = null;
            if (url.Contains("https"))
            {
                this.errstr = "";
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(this.CheckValidationResult);
                request = (HttpWebRequest) WebRequest.CreateDefault(new Uri(url));
                try
                {
                    if (cerPassword != null)
                    {
                        string password = cerPassword;
                        X509Certificate2 certificate = new X509Certificate2(cerPath, password);
                        request.ClientCertificates.Add(certificate);
                    }
                }
                catch (Exception exception)
                {
                    this.errstr = exception.Message.ToString().Trim();
                }
            }
            else
            {
                request = (HttpWebRequest) WebRequest.Create(url);
            }
            request.ServicePoint.Expect100Continue = false;
            request.Method = method;
            request.KeepAlive = true;
            request.UserAgent = "Hishop";
            return request;
        }
    }
}

