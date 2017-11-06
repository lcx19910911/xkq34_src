using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.UI;
using HiTemplate.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HiTemplate
{
    /// <summary>
    /// 商品模块
    /// </summary>
    public class GoodsMobule : RazorModuleWebControl
    {

        /// <summary>
        /// 获取数据商品JSON
        /// </summary>
        /// <returns></returns>
        public string GetDataJson()
        {
            string jsonstr = "";
            try
            {

                //应用程序路径
                string applicationPath = Urls.ApplicationPath;

                //数据请求地址
                if (string.IsNullOrEmpty(base.DataUrl))
                {
                    applicationPath = applicationPath + "/api/Hi_Ajax_GoodsList.ashx";
                }
                else
                {
                    applicationPath = applicationPath + base.DataUrl;
                }

                //参数
                string query = string.Format("ShowPrice={0}&Layout={1}&ShowIco={2}&ShowName={3}&IDs={4}", base.ShowPrice, base.Layout, base.ShowIco, base.ShowName, this.IDs);

                //将参数转成字节序列
                byte[] query2bytes = Encoding.UTF8.GetBytes(query);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(applicationPath);

                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";

                request.ContentLength = query2bytes.Length;

                request.GetRequestStream().Write(query2bytes, 0, query2bytes.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                StringBuilder builder = new StringBuilder();

                while (-1 != reader.Peek())
                {
                    builder.Append(reader.ReadLine());
                }

                jsonstr = builder.ToString();

            }
            catch (Exception ex)
            {
                jsonstr = "错误：" + ex.Message;
            }

            //返回数据
            return jsonstr;

        }

        /// <summary>
        /// 呈现控件
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {

            //日志路径
            //string log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoodsModlueLog.txt");

            Hi_Json_GoodGourpContent content = null;

            try
            {

                String jsonData = this.GetDataJson();

                if (!jsonData.Contains("错误"))
                {
                    //File.AppendAllText(log, "请求的参数：" + this.IDs + Environment.NewLine);
                    // File.AppendAllText(log, "请求的内容：" + jsonData + Environment.NewLine);
                    content = ((JObject)JsonConvert.DeserializeObject(jsonData)).ToObject<Hi_Json_GoodGourpContent>();
                    base.RenderModule(writer, content);
                }
                else
                {
                    writer.Write("发生错误:" + jsonData);
                }

            }
            catch (Exception ex)
            {

                writer.Write("发生异常:" + ex.Message);
                writer.Write("堆栈信息:" + ex.StackTrace);
                // File.AppendAllText(log, "请求出错：" + ex.Message + Environment.NewLine);

                //content = ((JObject)JsonConvert.DeserializeObject(this.GetDataJson())).ToObject<Hi_Json_GoodGourpContent>();

                //base.RenderModule(writer, new Hi_Json_GoodGourpContent());

            }


        }

        /// <summary>
        /// 商品ID
        /// </summary>
        [Bindable(true)]
        public string IDs { get; set; }


    }

}

