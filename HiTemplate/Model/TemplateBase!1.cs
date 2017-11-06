using System;
using System.Runtime.CompilerServices;

namespace HiTemplate.Model
{

    /// <summary>
    /// 泛型-模板基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TemplateBase<T> : TemplateBase
    {
        /// <summary>
        /// 保护构造函数
        /// </summary>
        protected TemplateBase() { }

        /// <summary>
        /// 泛型－模型
        /// </summary>
        public T Model { get; set; }

    }

}

