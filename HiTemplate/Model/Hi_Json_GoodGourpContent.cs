using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HiTemplate.Model
{
    /// <summary>
    /// 商品组内容
    /// </summary>
    public class Hi_Json_GoodGourpContent
    {

        /// <summary>
        /// 商品大小
        /// </summary>
        public int goodsize { get; set; }

        /// <summary>
        /// 商品列表
        /// </summary>
        public IList<HiShop_Model_Good> goodslist { get; set; }

        /// <summary>
        /// 商品组合
        /// </summary>
        public GoodGourp group { get; set; }

        /// <summary>
        /// 显示布局
        /// </summary>
        public int layout { get; set; }

        /// <summary>
        /// 显示图标
        /// </summary>
        public bool showIco { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public bool showName { get; set; }

        /// <summary>
        /// 显示价钱
        /// </summary>
        public bool showPrice { get; set; }

    }

}

