namespace Hishop.Weixin.Pay
{
    using Hishop.Weixin.Pay.Domain;
    using Hishop.Weixin.Pay.Util;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    public class RedPackClient
    {
        private static object LockLog = new object();
        public static readonly string SendRedPack_Url = "https://api.mch.weixin.qq.com/mmpaymkttransfers/sendredpack";

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return (errors == SslPolicyErrors.None);
        }

        public static void Debuglog(string log, [Optional, DefaultParameterValue("_DebugRedPacklog.txt")] string logname)
        {
            lock (LockLog)
            {
                try
                {
                    StreamWriter writer = System.IO.File.AppendText(HttpRuntime.AppDomainAppPath.ToString() + "log/" + (DateTime.Now.ToString("yyyyMMdd") + logname));
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ":" + log);
                    writer.WriteLine("---------------");
                    writer.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public static string PostData(string url, string postData)
        {
            Exception exception;
            string xml = string.Empty;
            try
            {
                Uri requestUri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                byte[] bytes = Encoding.UTF8.GetBytes(postData);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = postData.Length;
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        Encoding encoding = Encoding.UTF8;
                        xml = new StreamReader(stream, encoding).ReadToEnd();
                        XmlDocument document = new XmlDocument();
                        try
                        {
                            document.LoadXml(xml);
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            xml = string.Format("获取信息错误doc.load：{0}", exception.Message) + xml;
                        }
                        try
                        {
                            if (document == null)
                            {
                                return xml;
                            }
                            XmlNode node = document.SelectSingleNode("xml/return_code");
                            if (node == null)
                            {
                                return xml;
                            }
                            if (node.InnerText == "SUCCESS")
                            {
                                XmlNode node2 = document.SelectSingleNode("xml/prepay_id");
                                if (node2 != null)
                                {
                                    return node2.InnerText;
                                }
                            }
                            else
                            {
                                return document.InnerXml;
                            }
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                            xml = string.Format("获取信息错误node.load：{0}", exception.Message) + xml;
                        }
                        return xml;
                    }
                }
            }
            catch (Exception exception3)
            {
                exception = exception3;
                xml = string.Format("获取信息错误post error：{0}", exception.Message) + xml;
            }
            return xml;
        }

        public static string Send(string cert, string password, string data, string url)
        {
            return Send(cert, password, Encoding.GetEncoding("UTF-8").GetBytes(data), url);
        }

        public static string Send(string cert, string password, byte[] data, string url)
        {
            System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

            string result = "";//返回结果

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream reqStream = null;

            try
            {
                /***************************************************************
                * 下面设置HttpWebRequest的相关属性
                * ************************************************************/
                request = (HttpWebRequest)WebRequest.Create(url);
                //设置POST的数据类型和长度
                request.ContentType = "text/xml";
                request.ContentLength = data.Length;
                request.Method = "POST";
                request.Timeout = 8000;

                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 200;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    ////设置代理服务器
                    //WebProxy proxy = new WebProxy();                          //定义一个网关对象
                    //proxy.Address = new Uri("http://10.152.18.220:8080");              //网关服务器端口:端口
                    //request.Proxy = proxy;

                    //是否使用证书

                    //string path = HttpContext.Current.Request.PhysicalApplicationPath;
                    X509Certificate2 x509Cert = new X509Certificate2(cert, password, X509KeyStorageFlags.MachineKeySet);//path + WxPayConfig.SSLCERT_PATH, WxPayConfig.SSLCERT_PASSWORD);

                    //windows2003 无法使用上面的方法

                    //需要修改调用证书方法

                    //X509Certificate2 certificate = new X509Certificate2(PATH_TO_CERTIFICATE, PASSWORD, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);

                    request.ClientCertificates.Add(x509Cert);
                    // Log.Debug("WxPayApi", "PostXml used cert");
                }

                //往服务器写入数据
                reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                //获取服务端返回
                response = (HttpWebResponse)request.GetResponse();

                //获取服务端返回数据
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd().Trim();
                sr.Close();
            }
            //catch (System.Threading.ThreadAbortException e)
            //{
            //    Log.Error("HttpService", "Thread - caught ThreadAbortException - resetting.");
            //    Log.Error("Exception message: {0}", e.Message);
            //    System.Threading.Thread.ResetAbort();
            //}
            //catch (WebException e)
            //{
            //    Log.Error("HttpService", e.ToString());
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        Log.Error("HttpService", "StatusCode : " + ((HttpWebResponse)e.Response).StatusCode);
            //        Log.Error("HttpService", "StatusDescription : " + ((HttpWebResponse)e.Response).StatusDescription);
            //    }
            //    throw new WxPayException(e.ToString());
            //}
            catch (Exception e)
            {
                // Log.Error("HttpService", e.ToString());
                throw new Exception(e.ToString());
            }
            finally
            {
                //关闭连接和流
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        //public static string Send(string cert, string password, byte[] data, string url)
        //{
        //    Stream responseStream;
        //    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RedPackClient.CheckValidationResult);
        //    X509Certificate2 certificate = new X509Certificate2(cert, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
        //    X509Certificate2 certificate2 = new X509Certificate2(cert, password);
        //    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        //    store.Open(OpenFlags.ReadWrite);
        //    store.Remove(certificate2);
        //    store.Add(certificate2);
        //    store.Close();
        //    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        //    if (request == null)
        //    {
        //        throw new ApplicationException(string.Format("Invalid url string: {0}", url));
        //    }
        //    request.UserAgent = "Hishop";
        //    request.ContentType = "text/xml";
        //    request.ClientCertificates.Add(certificate);
        //    request.Method = "POST";
        //    request.ContentLength = data.Length;
        //    Stream requestStream = request.GetRequestStream();
        //    requestStream.Write(data, 0, data.Length);
        //    requestStream.Close();
        //    try
        //    {
        //        responseStream = request.GetResponse().GetResponseStream();
        //    }
        //    catch (Exception exception)
        //    {
        //        throw exception;
        //    }
        //    string str = string.Empty;
        //    using (StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
        //    {
        //        str = reader.ReadToEnd();
        //    }
        //    responseStream.Close();
        //    return str;
        //}

        public string SendRedpack(SendRedPackInfo sendredpack)
        {
            string str = string.Empty;
            PayDictionary parameters = new PayDictionary();
            parameters.Add("nonce_str", Utils.CreateNoncestr());
            if (sendredpack.EnableSP)
            {
                if (sendredpack.SendRedpackRecordID > 0)
                {
                    parameters.Add("mch_billno", sendredpack.Main_Mch_ID + DateTime.Now.ToString("yyyymmdd") + sendredpack.SendRedpackRecordID.ToString().PadLeft(10, '0'));
                }
                else
                {
                    parameters.Add("mch_billno", sendredpack.Main_Mch_ID + DateTime.Now.ToString("yyyymmdd") + DateTime.Now.ToString("MMddHHmmss"));
                }
                parameters.Add("mch_id", sendredpack.Main_Mch_ID);
                parameters.Add("sub_mch_id", sendredpack.Sub_Mch_Id);
                parameters.Add("wxappid", sendredpack.Main_AppId);
                parameters.Add("msgappid", sendredpack.Main_AppId);
            }
            else
            {
                if (sendredpack.SendRedpackRecordID > 0)
                {
                    parameters.Add("mch_billno", sendredpack.Mch_Id + DateTime.Now.ToString("yyyymmdd") + sendredpack.SendRedpackRecordID.ToString().PadLeft(10, '0'));
                }
                else
                {
                    parameters.Add("mch_billno", sendredpack.Mch_Id + DateTime.Now.ToString("yyyymmdd") + DateTime.Now.ToString("MMddHHmmss"));
                }
                parameters.Add("mch_id", sendredpack.Mch_Id);
                parameters.Add("wxappid", sendredpack.WXAppid);
                parameters.Add("nick_name", sendredpack.Nick_Name);
                parameters.Add("min_value", sendredpack.Total_Amount);
                parameters.Add("max_value", sendredpack.Total_Amount);
            }
            parameters.Add("send_name", sendredpack.Send_Name);
            parameters.Add("re_openid", sendredpack.Re_Openid);
            parameters.Add("total_amount", sendredpack.Total_Amount);
            parameters.Add("total_num", sendredpack.Total_Num);
            parameters.Add("wishing", sendredpack.Wishing);
            parameters.Add("client_ip", sendredpack.Client_IP);
            parameters.Add("act_name", sendredpack.Act_Name);
            parameters.Add("remark", sendredpack.Remark);
            string str2 = SignHelper.SignPackage(parameters, sendredpack.PartnerKey);
            parameters.Add("sign", str2);
            string log = SignHelper.BuildXml(parameters, false);
            Debuglog(log, "_DebugRedPacklog.txt");
            string msg = Send(sendredpack.WeixinCertPath, sendredpack.WeixinCertPassword, log, SendRedPack_Url);
            writeLog(parameters, str2, SendRedPack_Url, msg);
            if (!(string.IsNullOrEmpty(msg) || !msg.Contains("SUCCESS")))
            {
                return "1";
            }
            Match match = new Regex(@"<return_msg><!\[CDATA\[(?<code>(.*))\]\]></return_msg>").Match(msg);
            if (match.Success)
            {
                str = match.Groups["code"].Value;
            }
            return str;
        }

        public string SendRedpack(string appId, string mch_id, string sub_mch_id, string nick_name, string send_name, string re_openid, string wishing, string client_ip, string act_name, string remark, int amount, string partnerkey, string weixincertpath, string weixincertpassword, int sendredpackrecordid, bool enablesp, string main_appId, string main_mch_id, string main_paykey)
        {
            SendRedPackInfo sendredpack = new SendRedPackInfo
            {
                WXAppid = appId,
                Mch_Id = mch_id,
                Sub_Mch_Id = mch_id,
                Main_AppId = main_appId,
                Main_Mch_ID = main_mch_id,
                Main_PayKey = main_paykey,
                EnableSP = enablesp,
                Nick_Name = nick_name,
                Send_Name = send_name,
                Re_Openid = re_openid,
                Wishing = wishing,
                Client_IP = client_ip,
                Act_Name = act_name,
                Remark = remark,
                Total_Amount = amount,
                PartnerKey = partnerkey,
                WeixinCertPath = weixincertpath,
                WeixinCertPassword = weixincertpassword,
                SendRedpackRecordID = sendredpackrecordid
            };
            return this.SendRedpack(sendredpack);
        }

        public static void writeLog(IDictionary<string, string> param, string sign, string url, string msg)
        {
            DataTable table = new DataTable
            {
                TableName = "log"
            };
            table.Columns.Add(new DataColumn("OperTime"));
            foreach (KeyValuePair<string, string> pair in param)
            {
                table.Columns.Add(new DataColumn(pair.Key));
            }
            table.Columns.Add(new DataColumn("Msg"));
            table.Columns.Add(new DataColumn("Sign"));
            table.Columns.Add(new DataColumn("Url"));
            DataRow row = table.NewRow();
            row["OperTime"] = DateTime.Now;
            foreach (KeyValuePair<string, string> pair in param)
            {
                row[pair.Key] = pair.Value;
            }
            row["Msg"] = msg;
            row["Sign"] = sign;
            row["Url"] = url;
            table.Rows.Add(row);
            table.WriteXml(HttpContext.Current.Server.MapPath("~/wxpay.xml"));
        }
    }
}

