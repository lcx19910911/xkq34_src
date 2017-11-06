//namespace Hishop.WeiBo.Api
//{
//    using Microsoft.CSharp.RuntimeBinder;
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Net;
//    using System.Net.Http;
//    using System.Runtime.CompilerServices;
//    using System.Runtime.InteropServices;
//    using System.Text;
//    using System.Threading.Tasks;

//    public abstract class OpenAuthenticationClientBase
//    {
//        protected HttpClient http;
//        protected bool isAccessTokenSet = false;

//        public OpenAuthenticationClientBase(string clientId, string clientSecret, string callbackUrl, [Optional, DefaultParameterValue(null)] string accessToken)
//        {
//            this.ClientId = clientId;
//            this.ClientSecret = clientSecret;
//            this.CallbackUrl = callbackUrl;
//            this.AccessToken = accessToken;
//            HttpClientHandler handler = new HttpClientHandler {
//                CookieContainer = new CookieContainer(),
//                UseCookies = true
//            };
//            HttpClient client = new HttpClient(handler) {
//                BaseAddress = new Uri(this.BaseApiUrl)
//            };
//            this.http = client;
//            if (!string.IsNullOrEmpty(accessToken))
//            {
//                this.isAccessTokenSet = true;
//            }
//        }

//        public abstract void GetAccessTokenByCode(string code);
//        public abstract string GetAuthorizationUrl();
//        private string GetNonceString([Optional, DefaultParameterValue(8)] int length)
//        {
//            StringBuilder builder = new StringBuilder();
//            Random random = new Random();
//            for (int i = 0; i < length; i++)
//            {
//                builder.Append((char) random.Next(0x61, 0x7b));
//            }
//            return builder.ToString();
//        }

//        public HttpResponseMessage HttpGet(string api, [Optional, DefaultParameterValue(null)] Dictionary<string, object> parameters)
//        {
//            return this.HttpGetAsync(api, parameters).get_Result();
//        }

//        public HttpResponseMessage HttpGet(string api, [Dynamic] object parameters)
//        {
//            if (<HttpGet>o__SiteContainer0.<>p__Site1 == null)
//            {
//                <HttpGet>o__SiteContainer0.<>p__Site1 = CallSite<Func<CallSite, object, HttpResponseMessage>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(HttpResponseMessage), typeof(OpenAuthenticationClientBase)));
//            }
//            if (<HttpGet>o__SiteContainer0.<>p__Site2 == null)
//            {
//                <HttpGet>o__SiteContainer0.<>p__Site2 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Result", typeof(OpenAuthenticationClientBase), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
//            }
//            if (<HttpGet>o__SiteContainer0.<>p__Site3 == null)
//            {
//                <HttpGet>o__SiteContainer0.<>p__Site3 = CallSite<Func<CallSite, OpenAuthenticationClientBase, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "HttpGetAsync", null, typeof(OpenAuthenticationClientBase), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
//            }
//            return <HttpGet>o__SiteContainer0.<>p__Site1.Target.Invoke(<HttpGet>o__SiteContainer0.<>p__Site1, <HttpGet>o__SiteContainer0.<>p__Site2.Target.Invoke(<HttpGet>o__SiteContainer0.<>p__Site2, <HttpGet>o__SiteContainer0.<>p__Site3.Target.Invoke(<HttpGet>o__SiteContainer0.<>p__Site3, this, api, parameters)));
//        }

//        public virtual Task<HttpResponseMessage> HttpGetAsync(string api, [Optional, DefaultParameterValue(null)] Dictionary<string, object> parameters)
//        {
//            if (parameters == null)
//            {
//                parameters = new Dictionary<string, object>();
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegate10 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate10 = new Func<KeyValuePair<string, object>, bool>(null, (IntPtr) <HttpGetAsync>b__e);
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegate11 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate11 = new Func<KeyValuePair<string, object>, string>(null, (IntPtr) <HttpGetAsync>b__f);
//            }
//            string str = string.Join("&", Enumerable.Select<KeyValuePair<string, object>, string>(Enumerable.Where<KeyValuePair<string, object>>(parameters, CS$<>9__CachedAnonymousMethodDelegate10), CS$<>9__CachedAnonymousMethodDelegate11));
//            if (api.IndexOf("?") < 0)
//            {
//                api = string.Format("{0}?{1}", api, str);
//            }
//            else
//            {
//                api = string.Format("{0}&{1}", api, str);
//            }
//            api = api.Trim(new char[] { '&', '?' });
//            return this.http.GetAsync(api);
//        }

//        public Task<HttpResponseMessage> HttpGetAsync(string api, [Dynamic] object parameters)
//        {
//            <>c__DisplayClassc classc;
//            Type type = parameters.GetType();
//            if ((!type.Name.Contains("AnonymousType") || !type.IsSealed) || (type.Namespace != null))
//            {
//                throw new ArgumentException("只支持匿名类型参数");
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegatea == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegatea = new Func<PropertyInfo, bool>(null, (IntPtr) <HttpGetAsync>b__7);
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegateb == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegateb = new Func<PropertyInfo, string>(null, (IntPtr) <HttpGetAsync>b__8);
//            }
//            Dictionary<string, object> dictionary = Enumerable.ToDictionary<PropertyInfo, string, object>(Enumerable.Where<PropertyInfo>(type.GetProperties(), CS$<>9__CachedAnonymousMethodDelegatea), CS$<>9__CachedAnonymousMethodDelegateb, new Func<PropertyInfo, object>(classc, (IntPtr) this.<HttpGetAsync>b__9));
//            return this.HttpGetAsync(api, dictionary);
//        }

//        public HttpResponseMessage HttpPost(string api, Dictionary<string, object> parameters)
//        {
//            return this.HttpPostAsync(api, parameters).get_Result();
//        }

//        public HttpResponseMessage HttpPost(string api, [Dynamic] object parameters)
//        {
//            if (<HttpPost>o__SiteContainer12.<>p__Site13 == null)
//            {
//                <HttpPost>o__SiteContainer12.<>p__Site13 = CallSite<Func<CallSite, object, HttpResponseMessage>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(HttpResponseMessage), typeof(OpenAuthenticationClientBase)));
//            }
//            if (<HttpPost>o__SiteContainer12.<>p__Site14 == null)
//            {
//                <HttpPost>o__SiteContainer12.<>p__Site14 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Result", typeof(OpenAuthenticationClientBase), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
//            }
//            if (<HttpPost>o__SiteContainer12.<>p__Site15 == null)
//            {
//                <HttpPost>o__SiteContainer12.<>p__Site15 = CallSite<Func<CallSite, OpenAuthenticationClientBase, string, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.InvokeSimpleName, "HttpPostAsync", null, typeof(OpenAuthenticationClientBase), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
//            }
//            return <HttpPost>o__SiteContainer12.<>p__Site13.Target.Invoke(<HttpPost>o__SiteContainer12.<>p__Site13, <HttpPost>o__SiteContainer12.<>p__Site14.Target.Invoke(<HttpPost>o__SiteContainer12.<>p__Site14, <HttpPost>o__SiteContainer12.<>p__Site15.Target.Invoke(<HttpPost>o__SiteContainer12.<>p__Site15, this, api, parameters)));
//        }

//        public virtual Task<HttpResponseMessage> HttpPostAsync(string api, Dictionary<string, object> parameters)
//        {
//            if (parameters == null)
//            {
//                parameters = new Dictionary<string, object>();
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegate20 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate20 = new Func<KeyValuePair<string, object>, string>(null, (IntPtr) <HttpPostAsync>b__1b);
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegate21 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate21 = new Func<KeyValuePair<string, object>, object>(null, (IntPtr) <HttpPostAsync>b__1c);
//            }
//            Dictionary<string, object> dictionary = new Dictionary<string, object>(Enumerable.ToDictionary<KeyValuePair<string, object>, string, object>(parameters, CS$<>9__CachedAnonymousMethodDelegate20, CS$<>9__CachedAnonymousMethodDelegate21));
//            HttpContent content = null;
//            if (CS$<>9__CachedAnonymousMethodDelegate22 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate22 = new Func<KeyValuePair<string, object>, bool>(null, (IntPtr) <HttpPostAsync>b__1d);
//            }
//            if (Enumerable.Count<KeyValuePair<string, object>>(dictionary, CS$<>9__CachedAnonymousMethodDelegate22) > 0)
//            {
//                MultipartFormDataContent content2 = new MultipartFormDataContent();
//                foreach (KeyValuePair<string, object> pair in dictionary)
//                {
//                    Type type = pair.Value.GetType();
//                    if (type == typeof(byte[]))
//                    {
//                        content2.Add(new ByteArrayContent((byte[]) pair.Value), pair.Key, this.GetNonceString(8));
//                    }
//                    else if (type == typeof(MemoryFileContent))
//                    {
//                        MemoryFileContent content3 = (MemoryFileContent) pair.Value;
//                        content2.Add(new ByteArrayContent(content3.Content), pair.Key, content3.FileName);
//                    }
//                    else if (type == typeof(FileInfo))
//                    {
//                        FileInfo info = (FileInfo) pair.Value;
//                        content2.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(info.FullName)), pair.Key, info.Name);
//                    }
//                    else
//                    {
//                        content2.Add(new StringContent(string.Format("{0}", pair.Value)), pair.Key);
//                    }
//                }
//                content = content2;
//            }
//            else
//            {
//                if (CS$<>9__CachedAnonymousMethodDelegate23 == null)
//                {
//                    CS$<>9__CachedAnonymousMethodDelegate23 = new Func<KeyValuePair<string, object>, string>(null, (IntPtr) <HttpPostAsync>b__1e);
//                }
//                if (CS$<>9__CachedAnonymousMethodDelegate24 == null)
//                {
//                    CS$<>9__CachedAnonymousMethodDelegate24 = new Func<KeyValuePair<string, object>, string>(null, (IntPtr) <HttpPostAsync>b__1f);
//                }
//                FormUrlEncodedContent content4 = new FormUrlEncodedContent(Enumerable.ToDictionary<KeyValuePair<string, object>, string, string>(dictionary, CS$<>9__CachedAnonymousMethodDelegate23, CS$<>9__CachedAnonymousMethodDelegate24));
//                content = content4;
//            }
//            return this.http.PostAsync(api, content);
//        }

//        public Task<HttpResponseMessage> HttpPostAsync(string api, [Dynamic] object parameters)
//        {
//            <>c__DisplayClass19 class2;
//            Type type = parameters.GetType();
//            if ((!type.Name.Contains("AnonymousType") || !type.IsSealed) || (type.Namespace != null))
//            {
//                throw new ArgumentException("只支持匿名类型参数");
//            }
//            if (CS$<>9__CachedAnonymousMethodDelegate18 == null)
//            {
//                CS$<>9__CachedAnonymousMethodDelegate18 = new Func<PropertyInfo, string>(null, (IntPtr) <HttpPostAsync>b__16);
//            }
//            Dictionary<string, object> dictionary = Enumerable.ToDictionary<PropertyInfo, string, object>(type.GetProperties(), CS$<>9__CachedAnonymousMethodDelegate18, new Func<PropertyInfo, object>(class2, (IntPtr) this.<HttpPostAsync>b__17));
//            return this.HttpPostAsync(api, dictionary);
//        }

//        public string AccessToken { get; set; }

//        protected abstract string AccessTokenUrl { get; }

//        protected abstract string AuthorizationCodeUrl { get; }

//        protected abstract string BaseApiUrl { get; }

//        public string CallbackUrl { get; protected set; }

//        public string ClientId { get; protected set; }

//        public string ClientName { get; protected set; }

//        public string ClientSecret { get; protected set; }

//        public bool IsAuthorized
//        {
//            get
//            {
//                return (this.isAccessTokenSet && !string.IsNullOrEmpty(this.AccessToken));
//            }
//        }
//    }
//}

namespace Hishop.WeiBo.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class OpenAuthenticationClientBase
    {
        protected HttpClient http;
        protected bool isAccessTokenSet;

        public OpenAuthenticationClientBase(string clientId, string clientSecret, string callbackUrl, string accessToken = null)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.CallbackUrl = callbackUrl;
            this.AccessToken = accessToken;
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };
            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(this.BaseApiUrl)
            };
            this.http = client;
            if (!string.IsNullOrEmpty(accessToken))
            {
                this.isAccessTokenSet = true;
            }
        }

        public abstract void GetAccessTokenByCode(string code);
        public abstract string GetAuthorizationUrl();
        private string GetNonceString(int length = 8)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                builder.Append((char)random.Next(0x61, 0x7b));
            }
            return builder.ToString();
        }

        public HttpResponseMessage HttpGet(string api, Dictionary<string, object> parameters = null)
        {
            return this.HttpGetAsync(api, parameters).Result;
        }

        public HttpResponseMessage HttpGet(string api, object parameters)
        {
            return (HttpResponseMessage)this.HttpGetAsync(api, (dynamic)parameters).Result;
        }

        public virtual Task<HttpResponseMessage> HttpGetAsync(string api, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            string str = string.Join("&", (IEnumerable<string>)(from p in parameters.Where<KeyValuePair<string, object>>(delegate(KeyValuePair<string, object> p)
            {
                if ((p.Value != null) && !p.Value.GetType().IsValueType)
                {
                    return p.Value.GetType() == typeof(string);
                }
                return true;
            })
                                                                select string.Format("{0}={1}", Uri.EscapeDataString(p.Key), Uri.EscapeDataString(string.Format("{0}", p.Value)))));
            if (api.IndexOf("?") < 0)
            {
                api = string.Format("{0}?{1}", api, str);
            }
            else
            {
                api = string.Format("{0}&{1}", api, str);
            }
            api = api.Trim(new char[] { '&', '?' });
            return this.http.GetAsync(api);
        }

        public Task<HttpResponseMessage> HttpGetAsync(string api, object parameters)
        {
            Type type = parameters.GetType();
            if ((!type.Name.Contains("AnonymousType") || !type.IsSealed) || (type.Namespace != null))
            {
                throw new ArgumentException("只支持匿名类型参数");
            }
            Dictionary<string, object> dictionary = type.GetProperties().Where<PropertyInfo>(delegate(PropertyInfo p)
            {
                if (!p.PropertyType.IsValueType)
                {
                    return (p.PropertyType == typeof(string));
                }
                return true;
            }).ToDictionary<PropertyInfo, string, object>(k => k.Name, v => string.Format("{0}", v.GetValue((dynamic)parameters, (dynamic)null)));
            return this.HttpGetAsync(api, dictionary);
        }

        public HttpResponseMessage HttpPost(string api, Dictionary<string, object> parameters)
        {
            return this.HttpPostAsync(api, parameters).Result;
        }

        public HttpResponseMessage HttpPost(string api, object parameters)
        {
            return (HttpResponseMessage)this.HttpPostAsync(api, (dynamic)parameters).Result;
        }

        public virtual Task<HttpResponseMessage> HttpPostAsync(string api, Dictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            Dictionary<string, object> source = new Dictionary<string, object>(parameters.ToDictionary<KeyValuePair<string, object>, string, object>(k => k.Key, v => v.Value));
            HttpContent content = null;
            if (source.Count<KeyValuePair<string, object>>(delegate(KeyValuePair<string, object> p)
            {
                if (!(p.Value.GetType() == typeof(byte[])))
                {
                    return (p.Value.GetType() == typeof(FileInfo));
                }
                return true;
            }) > 0)
            {
                MultipartFormDataContent content2 = new MultipartFormDataContent();
                foreach (KeyValuePair<string, object> pair in source)
                {
                    Type type = pair.Value.GetType();
                    if (type == typeof(byte[]))
                    {
                        content2.Add(new ByteArrayContent((byte[])pair.Value), pair.Key, this.GetNonceString(8));
                    }
                    else if (type == typeof(MemoryFileContent))
                    {
                        MemoryFileContent content3 = (MemoryFileContent)pair.Value;
                        content2.Add(new ByteArrayContent(content3.Content), pair.Key, content3.FileName);
                    }
                    else if (type == typeof(FileInfo))
                    {
                        FileInfo info = (FileInfo)pair.Value;
                        content2.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(info.FullName)), pair.Key, info.Name);
                    }
                    else
                    {
                        content2.Add(new StringContent(string.Format("{0}", pair.Value)), pair.Key);
                    }
                }
                content = content2;
            }
            else
            {
                FormUrlEncodedContent content4 = new FormUrlEncodedContent(source.ToDictionary<KeyValuePair<string, object>, string, string>(k => k.Key, v => string.Format("{0}", v.Value)));
                content = content4;
            }
            return this.http.PostAsync(api, content);
        }

        public Task<HttpResponseMessage> HttpPostAsync(string api, object parameters)
        {
            Type type = parameters.GetType();
            if ((!type.Name.Contains("AnonymousType") || !type.IsSealed) || (type.Namespace != null))
            {
                throw new ArgumentException("只支持匿名类型参数");
            }
            Dictionary<string, object> dictionary = type.GetProperties().ToDictionary<PropertyInfo, string, object>(k => k.Name, v => v.GetValue(parameters, null));
            return this.HttpPostAsync(api, dictionary);
        }

        public string AccessToken { get; set; }

        protected abstract string AccessTokenUrl { get; }

        protected abstract string AuthorizationCodeUrl { get; }

        protected abstract string BaseApiUrl { get; }

        public string CallbackUrl { get; protected set; }

        public string ClientId { get; protected set; }

        public string ClientName { get; protected set; }

        public string ClientSecret { get; protected set; }

        public bool IsAuthorized
        {
            get
            {
                return (this.isAccessTokenSet && !string.IsNullOrEmpty(this.AccessToken));
            }
        }
    }
}



