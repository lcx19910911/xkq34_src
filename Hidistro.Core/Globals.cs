namespace Hidistro.Core
{
    using Hidistro.Core.Configuration;
    using Hidistro.Core.Entities;
    using Hidistro.Core.Enums;
    using Hidistro.Core.Urls;
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Net.Mail;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Security;

    public static class Globals
    {
        private static object LockLog = new object();
        private static object Orderidlock = new object();
        private static string provOrderid = "";
        public const string UserVerifyCookieName = "Vshop-Member-Verify";

        public static string AppendQuerystring(string url, string querystring)
        {
            return AppendQuerystring(url, querystring, false);
        }

        public static string AppendQuerystring(string url, string querystring, bool urlEncoded)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            string str = "?";
            if (url.IndexOf('?') > -1)
            {
                if (!urlEncoded)
                {
                    str = "&";
                }
                else
                {
                    str = "&amp;";
                }
            }
            return (url + str + querystring);
        }

        public static int[] BubbleSort(int[] r)
        {
            for (int i = 0; i < r.Length; i++)
            {
                bool flag = false;
                for (int j = r.Length - 2; j >= i; j--)
                {
                    if (r[j + 1] > r[j])
                    {
                        int num3 = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = num3;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return r;
                }
            }
            return r;
        }

        public static bool CheckReg(string str, string reg)
        {
            return Regex.IsMatch(str, reg);
        }

        public static bool CheckVerifyCode(string verifyCode)
        {
            if (HttpContext.Current.Request.Cookies["VerifyCode"] == null)
            {
                RemoveVerifyCookie();
                return false;
            }
            bool flag = string.Compare(HiCryptographer.Decrypt(HttpContext.Current.Request.Cookies["VerifyCode"].Value), verifyCode, true, CultureInfo.InvariantCulture) == 0;
            RemoveVerifyCookie();
            return flag;
        }

        public static void ClearFWCookie()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["fwfollow"];
            if (!((cookie == null) || string.IsNullOrEmpty(cookie.Value)))
            {
                cookie.Value = null;
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Set(cookie);
            }
        }

        public static void ClearReferralIdCookie()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["Vshop-ReferralId"];
            if (((cookie != null) && !string.IsNullOrEmpty(cookie.Value)) && (int.Parse(cookie.Value) == 0))
            {
                cookie.Value = null;
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Set(cookie);
            }
        }

        public static void ClearUserCookie()
        {
            try
            {
                ClearFWCookie();
                ClearWXCookie();
                HttpCookie cookie = HttpContext.Current.Request.Cookies["Vshop-Member"];
                if (!((cookie == null) || string.IsNullOrEmpty(cookie.Value)))
                {
                    cookie.Value = null;
                    cookie.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Set(cookie);
                }
                HttpCookie cookie2 = HttpContext.Current.Request.Cookies["Vshop-Member-Verify"];
                if (!((cookie2 == null) || string.IsNullOrEmpty(cookie2.Value)))
                {
                    cookie2.Value = null;
                    cookie2.Expires = DateTime.Now.AddYears(-1);
                    HttpContext.Current.Response.Cookies.Set(cookie2);
                }
                ClearReferralIdCookie();
            }
            catch
            {
            }
        }

        public static void ClearWXCookie()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies["wxfollow"];
            if (!((cookie == null) || string.IsNullOrEmpty(cookie.Value)))
            {
                cookie.Value = null;
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Set(cookie);
            }
            HttpCookie cookie2 = HttpContext.Current.Request.Cookies[GetCurrentWXOpenIdCookieName()];
            if (!((cookie2 == null) || string.IsNullOrEmpty(cookie2.Value)))
            {
                cookie2.Value = null;
                cookie2.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Set(cookie2);
            }
        }

        public static string CreateVerifyCode(int length)
        {
            string text = string.Empty;
            Random random = new Random();
            while (text.Length < length)
            {
                char ch;
                int num = random.Next();
                if ((num % 3) == 0)
                {
                    ch = (char) (0x61 + ((ushort) (num % 0x1a)));
                }
                else if ((num % 4) == 0)
                {
                    ch = (char) (0x41 + ((ushort) (num % 0x1a)));
                }
                else
                {
                    ch = (char) (0x30 + ((ushort) (num % 10)));
                }
                if (((((ch != '0') && (ch != 'o')) && ((ch != '1') && (ch != '7'))) && (((ch != 'l') && (ch != '9')) && (ch != 'g'))) && (ch != 'I'))
                {
                    text = text + ch.ToString();
                }
            }
            RemoveVerifyCookie();
            HttpCookie cookie = new HttpCookie("VerifyCode") {
                Value = HiCryptographer.Encrypt(text)
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
            return text;
        }

        public static void Debuglog(string log, [Optional, DefaultParameterValue("_Debuglog.txt")] string logname)
        {
            lock (LockLog)
            {
                try
                {
                    StreamWriter writer = File.AppendText(HttpRuntime.AppDomainAppPath.ToString() + "App_Data/" + (DateTime.Now.ToString("yyyyMMdd") + logname));
                    writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ":" + log);
                    writer.WriteLine("---------------");
                    writer.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public static string EncryptStr(string str)
        {
            string sKey = "hishoptk";
            string str3 = ConfigurationManager.AppSettings["IV"];
            if (!(string.IsNullOrEmpty(str3) || (str3.Length < 8)))
            {
                sKey = str3.Substring(0, 8);
            }
            return HiCryptographer.Md5Encrypt(DesSecurity.DesEncrypt(str, sKey));
        }

        public static void EntityCoding(object entity, bool encode)
        {
            if (entity != null)
            {
                PropertyInfo[] properties = entity.GetType().GetProperties();
                foreach (PropertyInfo info in properties)
                {
                    if (info.GetCustomAttributes(typeof(HtmlCodingAttribute), true).Length != 0)
                    {
                        if (!(info.CanWrite && info.CanRead))
                        {
                            throw new Exception("使用HtmlEncodeAttribute修饰的属性必须是可读可写的");
                        }
                        if (!info.PropertyType.Equals(typeof(string)))
                        {
                            throw new Exception("非字符串类型的属性不能使用HtmlEncodeAttribute修饰");
                        }
                        string str = info.GetValue(entity, null) as string;
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (encode)
                            {
                                info.SetValue(entity, HtmlEncode(str), null);
                            }
                            else
                            {
                                info.SetValue(entity, HtmlDecode(str), null);
                            }
                        }
                    }
                }
            }
        }

        public static string FormatMoney(decimal money)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { money.ToString("F", CultureInfo.InvariantCulture) });
        }

        public static string FullPath(string local)
        {
            if (string.IsNullOrEmpty(local))
            {
                return local;
            }
            if (local.ToLower(CultureInfo.InvariantCulture).StartsWith("http://"))
            {
                return local;
            }
            if (HttpContext.Current == null)
            {
                return local;
            }
            return FullPath(HostPath(HttpContext.Current.Request.Url), local);
        }

        public static string FullPath(string hostPath, string local)
        {
            return (hostPath + local);
        }

        public static string GetAdminAbsolutePath(string path)
        {
            if (path.StartsWith("/"))
            {
                return (ApplicationPath + "/" + HiConfiguration.GetConfig().AdminFolder + path);
            }
            return (ApplicationPath + "/" + HiConfiguration.GetConfig().AdminFolder + "/" + path);
        }

        public static string GetBarginShow(object bargainDetialId)
        {
            string str = string.Empty;
            if (ToNum(bargainDetialId) > 0)
            {
                str = "<span class='red' bid='" + bargainDetialId + "'> [砍价]</span>";
            }
            return str;
        }

        public static int GetClientShortType()
        {
            int num = 0;
            string userAgent = HttpContext.Current.Request.UserAgent;
            if (userAgent.ToLower().Contains("alipay"))
            {
                return 2;
            }
            if (userAgent.ToLower().Contains("micromessenger"))
            {
                num = 1;
            }
            return num;
        }

        public static int GetCommitionType()
        {
            return 0;
        }

        public static int GetCurrentDistributorId()
        {
            int result = 0;
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("Vshop-ReferralId");
            if ((cookie == null) || string.IsNullOrEmpty(cookie.Value))
            {
                return 0;
            }
            int.TryParse(cookie.Value, out result);
            return result;
        }

        public static int GetCurrentManagerUserId()
        {
            int roleId = 0;
            bool isDefault = false;
            return GetCurrentManagerUserId(out roleId, out isDefault);
        }

        public static int GetCurrentManagerUserId(out int roleId, out bool isDefault)
        {
            roleId = 0;
            isDefault = false;
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(string.Format("{0}{1}", DomainName, FormsAuthentication.FormsCookieName));
            if ((cookie == null) || string.IsNullOrEmpty(cookie.Value))
            {
                return 0;
            }
            int result = 0;
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            try
            {
                int.TryParse(ticket.Name, out result);
                string[] strArray = ticket.UserData.Split(new char[] { '_' });
                int.TryParse(strArray[0], out roleId);
                bool.TryParse(strArray[1], out isDefault);
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static int GetCurrentMemberUserId()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies.Get("Vshop-Member");
            if ((cookie == null) || string.IsNullOrEmpty(cookie.Value))
            {
                return 0;
            }
            HttpCookie cookie2 = HttpContext.Current.Request.Cookies.Get("Vshop-Member-Verify");
            if ((cookie2 == null) || string.IsNullOrEmpty(cookie2.Value))
            {
                return 0;
            }
            int result = 0;
            if (EncryptStr(cookie.Value) == cookie2.Value)
            {
                int.TryParse(cookie.Value, out result);
            }
            return result;
        }

        public static string GetCurrentWXOpenIdCookieName()
        {
            return "xkdopenidv304";
        }

        public static string GetGenerateId()
        {
            lock (Orderidlock)
            {
                string str = DateTime.Now.ToString("yyMMddHHmmssfff");
                if (str == provOrderid)
                {
                    Thread.Sleep(1);
                    str = DateTime.Now.ToString("yyMMddHHmmssfff");
                }
                provOrderid = str;
                return str;
            }
        }

        public static int GetPoint(decimal money)
        {
            decimal num = 1M;
            int num2 = 0;
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
            if (!masterSettings.shopping_score_Enable)
            {
                return 0;
            }
            decimal num3 = money;
            if (((num3 * num) / masterSettings.PointsRate) > 2147483647M)
            {
                num2 = 0x7fffffff;
            }
            else if (masterSettings.PointsRate != 0M)
            {
                num2 = (int) Math.Round((decimal) ((num3 * num) / masterSettings.PointsRate), 0);
            }
            if (masterSettings.shopping_reward_Enable && (num3 >= ((decimal) masterSettings.shopping_reward_OrderValue)))
            {
                num2 += masterSettings.shopping_reward_Score;
                if (num2 > 0x7fffffff)
                {
                    num2 = 0x7fffffff;
                }
            }
            return num2;
        }

        public static SiteUrls GetSiteUrls()
        {
            return SiteUrls.Instance();
        }

        public static string GetStoragePath()
        {
            return "/Storage/master";
        }

        public static int GetStrLen(string strData)
        {
            return Encoding.GetEncoding("GB2312").GetByteCount(strData);
        }

        public static string GetVshopSkinPath([Optional, DefaultParameterValue(null)] string themeName)
        {
            return (ApplicationPath + "/Templates/common").ToLower(CultureInfo.InvariantCulture);
        }

        public static string GetWebUrlStart()
        {
            Uri url = HttpContext.Current.Request.Url;
            return (url.Scheme + "://" + url.Host + ((url.Port == 80) ? "" : (":" + url.Port.ToString())));
        }

        public static string HostPath(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }
            string str = (uri.Port == 80) ? string.Empty : (":" + uri.Port.ToString(CultureInfo.InvariantCulture));
            return string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}", new object[] { uri.Scheme, uri.Host, str });
        }

        public static string HtmlDecode(string textToFormat)
        {
            if (string.IsNullOrEmpty(textToFormat))
            {
                return textToFormat;
            }
            return HttpUtility.HtmlDecode(textToFormat);
        }

        public static string HtmlEncode(string textToFormat)
        {
            if (string.IsNullOrEmpty(textToFormat))
            {
                return textToFormat;
            }
            return HttpUtility.HtmlEncode(textToFormat);
        }

        public static bool IsNumeric(string lstr)
        {
            return (!string.IsNullOrEmpty(lstr) && Regex.IsMatch(lstr, @"^\d+(\.)?\d*$"));
        }

        public static bool IsOrdersID(string lstr)
        {
            string pattern = @"^(\d{15}|\d{19})(?:-\d+)?$";
            return (!string.IsNullOrEmpty(lstr) && Regex.IsMatch(lstr, pattern));
        }

        public static bool isUnsignedInteger(string lstr)
        {
            return (!string.IsNullOrEmpty(lstr) && Regex.IsMatch(lstr, "^([1-9]//d*|0)$"));
        }

        public static string LinkUrl(string url)
        {
            string str = url;
            if (str.Contains("?"))
            {
                return (str + "&ReferralId=" + RequestQueryStr("UserId"));
            }
            return (str + "?ReferralId=" + RequestQueryStr("UserId"));
        }

        public static string MapPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                return current.Request.MapPath(path);
            }
            return PhysicalPath(path.Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("~", ""));
        }

        public static string PhysicalPath(string path)
        {
            if (null == path)
            {
                return string.Empty;
            }
            return (RootPath().TrimEnd(new char[] { Path.DirectorySeparatorChar }) + Path.DirectorySeparatorChar.ToString() + path.TrimStart(new char[] { Path.DirectorySeparatorChar }));
        }

        public static void RedirectToSSL(HttpContext context)
        {
            if ((null != context) && !context.Request.IsSecureConnection)
            {
                Uri url = context.Request.Url;
                context.Response.Redirect("https://" + url.ToString().Substring(7));
            }
        }

        public static void RefreshWeiXinToken()
        {
            string key = "weixinToken";
            HttpRuntime.Cache.Remove(key);
        }

        private static void RemoveVerifyCookie()
        {
            HttpContext.Current.Response.Cookies["VerifyCode"].Value = null;
            HttpContext.Current.Response.Cookies["VerifyCode"].Expires = new DateTime(0x777, 10, 12);
        }

        public static int RequestFormNum(string sTemp)
        {
            string s = HttpContext.Current.Request.Form[sTemp];
            return ToNum(s);
        }

        public static string RequestFormStr(string sTemp)
        {
            string str = HttpContext.Current.Request.Form[sTemp];
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return str.Trim();
        }

        public static decimal RequestQueryDecimal(string sTemp)
        {
            decimal result = 0M;
            string s = HttpContext.Current.Request.QueryString[sTemp];
            decimal.TryParse(s, out result);
            return result;
        }

        public static int RequestQueryNum(string sTemp)
        {
            string s = HttpContext.Current.Request.QueryString[sTemp];
            return ToNum(s);
        }

        public static string RequestQueryStr(string sTemp)
        {
            string str = HttpContext.Current.Request.QueryString[sTemp];
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return str.Trim();
        }

        private static string RootPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string newValue = Path.DirectorySeparatorChar.ToString();
            baseDirectory = baseDirectory.Replace("/", newValue);
            string filesPath = HiConfiguration.GetConfig().FilesPath;
            if (filesPath == null)
            {
                return baseDirectory;
            }
            filesPath = filesPath.Replace("/", newValue);
            if (((filesPath.Length > 0) && filesPath.StartsWith(newValue)) && baseDirectory.EndsWith(newValue))
            {
                return (baseDirectory + filesPath.Substring(1));
            }
            return (baseDirectory + filesPath);
        }

        public static string ServerIP()
        {
            string str = HttpContext.Current.Request.ServerVariables.Get("Local_Addr").ToString();
            if (str.Length < 7)
            {
                ManagementObjectCollection instances = new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances();
                foreach (ManagementObject obj2 in instances)
                {
                    if ((bool) obj2["IPEnabled"])
                    {
                        string[] strArray = (string[]) obj2["IPAddress"];
                        if (strArray.Length > 0)
                        {
                            str = strArray[0];
                        }
                        return str;
                    }
                }
            }
            return str;
        }

        public static void SetDistributorCookie(int distributorid)
        {
            HttpCookie cookie = new HttpCookie("Vshop-ReferralId") {
                Value = distributorid.ToString(),
                Expires = DateTime.Now.AddYears(1)
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static string String2Json(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s.ToCharArray()[i];
                switch (ch)
                {
                    case '/':
                    {
                        builder.Append(@"\/");
                        continue;
                    }
                    case '\\':
                    {
                        builder.Append(@"\\");
                        continue;
                    }
                    case '\b':
                    {
                        builder.Append(@"\b");
                        continue;
                    }
                    case '\t':
                    {
                        builder.Append(@"\t");
                        continue;
                    }
                    case '\n':
                    {
                        builder.Append(@"\n");
                        continue;
                    }
                    case '\f':
                    {
                        builder.Append(@"\f");
                        continue;
                    }
                    case '\r':
                    {
                        builder.Append(@"\r");
                        continue;
                    }
                    case '"':
                    {
                        builder.Append("\\\"");
                        continue;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string StripAllTags(string strToStrip)
        {
            strToStrip = Regex.Replace(strToStrip, @"</p(?:\s*)>(?:\s*)<p(?:\s*)>", "\n\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            strToStrip = Regex.Replace(strToStrip, @"<br(?:\s*)/>", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            strToStrip = Regex.Replace(strToStrip, "\"", "''", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            strToStrip = StripHtmlXmlTags(strToStrip);
            return strToStrip;
        }

        public static string StripForPreview(string content)
        {
            content = Regex.Replace(content, "<br>", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            content = Regex.Replace(content, "<br/>", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            content = Regex.Replace(content, "<br />", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            content = Regex.Replace(content, "<p>", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            content = content.Replace("'", "&#39;");
            return StripHtmlXmlTags(content);
        }

        public static string StripHtmlXmlTags(string content)
        {
            return Regex.Replace(content, "<[^>]+>", "", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static string StripScriptTags(string content)
        {
            content = Regex.Replace(content, "<script((.|\n)*?)</script>", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            content = Regex.Replace(content, "'javascript:", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return Regex.Replace(content, "\"javascript:", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        public static string SubStr(string s, int i, string smore)
        {
            int num = 0;
            int num2 = 0;
            string str = s;
            if (GetStrLen(s) <= i)
            {
                return str;
            }
            foreach (char ch in s)
            {
                if (num >= i)
                {
                    break;
                }
                num2++;
                if (ch > '\x007f')
                {
                    num += 2;
                }
                else
                {
                    num++;
                }
            }
            str = s.Substring(0, num2 - GetStrLen(smore));
            return (str + smore);
        }

        public static string ToDelimitedString(ICollection collection, string delimiter)
        {
            if (collection == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            if (collection is Hashtable)
            {
                foreach (object obj2 in ((Hashtable) collection).Keys)
                {
                    builder.Append(obj2.ToString() + delimiter);
                }
            }
            if (collection is ArrayList)
            {
                foreach (object obj2 in (ArrayList) collection)
                {
                    builder.Append(obj2.ToString() + delimiter);
                }
            }
            if (collection is string[])
            {
                foreach (string str in (string[]) collection)
                {
                    builder.Append(str + delimiter);
                }
            }
            if (collection is MailAddressCollection)
            {
                foreach (MailAddress address in (MailAddressCollection) collection)
                {
                    builder.Append(address.Address + delimiter);
                }
            }
            return builder.ToString().TrimEnd(new char[] { Convert.ToChar(delimiter, CultureInfo.InvariantCulture) });
        }

        public static int ToNum(object s)
        {
            string str = (s == null) ? "0" : s.ToString();
            if (!string.IsNullOrEmpty(str) && IsNumeric(str))
            {
                try
                {
                    return Convert.ToInt32(str);
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        public static string UnHtmlEncode(string formattedPost)
        {
            RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            formattedPost = Regex.Replace(formattedPost, "&quot;", "\"", options);
            formattedPost = Regex.Replace(formattedPost, "&lt;", "<", options);
            formattedPost = Regex.Replace(formattedPost, "&gt;", ">", options);
            return formattedPost;
        }

        public static string UrlDecode(string urlToDecode)
        {
            if (string.IsNullOrEmpty(urlToDecode))
            {
                return urlToDecode;
            }
            return HttpUtility.UrlDecode(urlToDecode, Encoding.UTF8);
        }

        public static string UrlEncode(string urlToEncode)
        {
            if (string.IsNullOrEmpty(urlToEncode))
            {
                return urlToEncode;
            }
            return HttpUtility.UrlEncode(urlToEncode, Encoding.UTF8);
        }

        public static void ValidateSecureConnection(SSLSettings ssl, HttpContext context)
        {
            if (HiConfiguration.GetConfig().SSL == ssl)
            {
                RedirectToSSL(context);
            }
        }

        public static string ApplicationPath
        {
            get
            {
                string applicationPath = "/";
                if (HttpContext.Current != null)
                {
                    try
                    {
                        applicationPath = HttpContext.Current.Request.ApplicationPath;
                    }
                    catch
                    {
                        applicationPath = AppDomain.CurrentDomain.BaseDirectory;
                    }
                }
                if (applicationPath == "/")
                {
                    return string.Empty;
                }
                return applicationPath.ToLower(CultureInfo.InvariantCulture);
            }
        }

        public static string DomainName
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return string.Empty;
                }
                return HttpContext.Current.Request.Url.Host;
            }
        }

        public static string GetCurrentWXOpenId
        {
            get
            {
                string str = string.Empty;
                HttpCookie cookie = HttpContext.Current.Request.Cookies.Get(GetCurrentWXOpenIdCookieName());
                if (cookie != null)
                {
                    str = cookie.Value;
                }
                return str;
            }
            set
            {
                string str = value;
                if (!string.IsNullOrEmpty(str))
                {
                    HttpCookie cookie = new HttpCookie(GetCurrentWXOpenIdCookieName()) {
                        Value = str.Trim(),
                        Expires = DateTime.Now.AddYears(1)
                    };
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }

        public static string IPAddress
        {
            get
            {
                string userHostAddress;
                HttpRequest request = HttpContext.Current.Request;
                if (string.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
                {
                    userHostAddress = request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    userHostAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                if (string.IsNullOrEmpty(userHostAddress))
                {
                    userHostAddress = request.UserHostAddress;
                }
                return userHostAddress;
            }
        }
    }
}

