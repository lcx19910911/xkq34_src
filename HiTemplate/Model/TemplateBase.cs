using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HiTemplate.Model
{
    /// <summary>
    /// 模板基类
    /// </summary>
    public abstract class TemplateBase
    {

        TextWriter writer = null;

        /// <summary>
        /// 保护的构造函数
        /// </summary>
        protected TemplateBase() { }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            this.Writer.Flush();
        }

        /// <summary>
        /// 执行
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="object"></param>
        public void Write(object @object)
        {
            if (@object != null)
            {
                this.Writer.Write(@object);
            }
        }

        /// <summary>
        /// ？？
        /// </summary>
        /// <param name="string"></param>
        public void WriteLiteral(string @string)
        {
            if (@string != null)
            {
                this.Writer.Write(@string);
            }
        }

        public static void WriteLiteralTo(TextWriter writer, string literal)
        {
            if (literal != null)
            {
                writer.Write(literal);
            }
        }

        public static void WriteTo(TextWriter writer, object obj)
        {
            if (obj != null)
            {
                writer.Write(obj);
            }
        }

        /// <summary>
        /// 布局
        /// </summary>
        public string Layout { get; set; }

        public string Path { get; internal set; }

        public Func<string> RenderBody { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public string Result
        {
            get
            {
                return this.Writer.ToString();
            }
        }


        // static readonly object textWriterLocker = new object();

        /// <summary>
        /// 文件写器
        /// </summary>
        public TextWriter Writer
        {
            get
            {
                //if (this.writer == null)
                //{
                //    lock (textWriterLocker)
                //    {
                if (this.writer == null)
                {
                    this.writer = new StringWriter();
                }
                //  }
                //  }
                return this.writer;
            }
            set
            {
                this.writer = value;
            }
        }


    }

}

