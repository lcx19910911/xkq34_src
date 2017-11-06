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
    /// 商品列表
    /// </summary>
    public class GoodsListModule : RazorModuleWebControl
    {
        /// <summary>
        /// 获取商品ＪＳＯＮ
        /// </summary>
        /// <returns></returns>
        public string GetDataJson()
        {

            string dataJson = "";

            try
            {
                string queryParams = string.Format("GroupID={0}&GoodListSize={1}&FirstPriority={2}&SecondPriority={3}&ShowPrice={4}&Layout={5}&ShowIco={6}&ShowName={7}", this.GroupID, this.GoodListSize, this.FirstPriority, this.SecondPriority, base.ShowPrice, base.Layout, base.ShowIco, base.ShowName);

                byte[] query2bytes = Encoding.UTF8.GetBytes(queryParams);

                string applicationPath = Urls.ApplicationPath;

                if (string.IsNullOrEmpty(base.DataUrl))
                {
                    applicationPath = applicationPath + "/api/Hi_Ajax_GoodsListGroup.ashx";
                }
                else
                {
                    applicationPath = applicationPath + base.DataUrl;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(applicationPath);

                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";

                request.ContentLength = query2bytes.Length;

                try
                {
                    request.GetRequestStream().Write(query2bytes, 0, query2bytes.Length);

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                    StringBuilder builder = new StringBuilder();
                    while (-1 != reader.Peek())
                    {
                        builder.Append(reader.ReadLine());
                    }

                    dataJson = builder.ToString();

                }
                catch (Exception ex)
                {
                    dataJson = "错误：" + ex.Message;
                }

            }
            catch (Exception ex)
            {
                //exception = exception2;
                dataJson = "错误：" + ex.Message;
            }

            return dataJson;
        }

        /// <summary>
        /// 呈现控件
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                string jsonData = this.GetDataJson();

                Hi_Json_GoodGourpContent hi_Json_GoodGourpContent = ((JObject)JsonConvert.DeserializeObject(jsonData)).ToObject<Hi_Json_GoodGourpContent>();

                base.RenderModule(writer, hi_Json_GoodGourpContent);
            }
            catch (Exception ex)
            {
                writer.Write(ex.Message);
                writer.Write(ex.StackTrace);
                //base.RenderModule(writer, new Hi_Json_GoodGourpContent());
            }
        }

        /// <summary>
        /// 第一优先级
        /// </summary>
        [Bindable(true)]
        public string FirstPriority { get; set; }

        /// <summary>
        /// 商品列表大小
        /// </summary>
        [Bindable(true)]
        public string GoodListSize { get; set; }

        /// <summary>
        /// 组ＩＤ
        /// </summary>
        [Bindable(true)]
        public string GroupID { get; set; }

        /// <summary>
        /// 第二优先级
        /// </summary>
        [Bindable(true)]
        public string SecondPriority { get; set; }

        /// <summary>
        /// 显示排序
        /// </summary>
        [Bindable(true)]
        public string ShowOrder { get; set; }

        /// <summary>
        /// 第三优先级
        /// </summary>
        [Bindable(true)]
        public string ThirdPriority { get; set; }
    }
}

