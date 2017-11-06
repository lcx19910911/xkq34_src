using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using RazorEngine;
using RazorEngine.Templating;

namespace HiTemplate
{

    /// <summary>
    /// 商品呈现－基类
    /// </summary>
    public class RazorModuleWebControl : WebControl
    {
        /// <summary>
        /// 呈现模块-虚函数
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="jsonData"></param>
        protected virtual void RenderModule(HtmlTextWriter writer, object jsonData)
        {

            string name = "TemplateCacheKey-" + this.ID;

            string key = "TemplateFileCacheKey-" + this.ID;

            //从缓存中读取数据
            string templateFileContent = HttpContext.Current.Cache[key] as string;


            if (string.IsNullOrEmpty(templateFileContent) || (templateFileContent.Length == 0))
            {

                //真实模板路径
                string phyTemplateFilePath = HttpContext.Current.Request.MapPath(this.TemplateFile);

                //读取模板文件
                templateFileContent = File.ReadAllText(phyTemplateFilePath);

                //写入缓存
                HttpContext.Current.Cache.Insert(key, templateFileContent, new CacheDependency(phyTemplateFilePath), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.AboveNormal, null);


                //编译模板文件内容
                string compliedTemplateFileContent = Engine.Razor.RunCompile((ITemplateSource)new LoadedTemplateSource(templateFileContent, null), name, null, jsonData, null);

       
                //writer.Write(System.Text.RegularExpressions.Regex.Replace(compliedTemplateFileContent, "<img[^>]*\\bsrc=('|\")([^'\">]*)\\1[^>]*>", "<img src=\"/Utility/images/blank.gif\" data-echo='$2' />", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

                //输出
                //160519修改
                // writer.Write(compliedTemplateFileContent.Replace("src=", "data-url="));
                writer.Write(compliedTemplateFileContent);
            }
            else
            {
                templateFileContent = Engine.Razor.IsTemplateCached(name, null) ? Engine.Razor.Run(name, null, jsonData, null) : Engine.Razor.RunCompile(((ITemplateSource)new LoadedTemplateSource(templateFileContent, null)), name, null, jsonData, null);




                //输出
                //160519修改
                //writer.Write(System.Text.RegularExpressions.Regex.Replace(templateFileContent, "<img[^>]*\\bsrc=('|\")([^'\">]*)\\1[^>]*>", "<img src=\"/Utility/images/blank.gif\" data-echo='$2' />", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
                writer.Write(templateFileContent);
            }
        }

        #region 属性

        /// <summary>
        /// 数据地址
        /// </summary>
        [Bindable(true)]
        public string DataUrl { get; set; }

        /// <summary>
        /// 布局
        /// </summary>
        [Bindable(true)]
        public string Layout { get; set; }

        /// <summary>
        /// 显示LOGO
        /// </summary>
        [Bindable(true)]
        public string ShowIco { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Bindable(true)]
        public string ShowName { get; set; }

        /// <summary>
        /// 显示价格
        /// </summary>
        [Bindable(true)]
        public string ShowPrice { get; set; }

        /// <summary>
        /// 模板文件
        /// </summary>
        [Bindable(true)]
        public string TemplateFile { get; set; }

        #endregion

    }
}


