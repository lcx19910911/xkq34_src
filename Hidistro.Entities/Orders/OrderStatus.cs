using System;


namespace Hidistro.Entities.Orders
{

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 0 - 所有
        /// </summary>
        All = 0,

        /// <summary>
        /// 1 - 等待买家付款
        /// </summary>
        WaitBuyerPay = 1,

        /// <summary>
        /// 2 - 买家已付款，等待发货
        /// </summary>
        BuyerAlreadyPaid = 2,

        /// <summary>
        /// 3 - 卖家已发货，等待确认
        /// </summary>
        SellerAlreadySent = 3,

        /// <summary>
        /// 4 - 已关闭
        /// </summary>
        Closed = 4,

        /// <summary>
        /// 5 - 已完成
        /// </summary>
        Finished = 5,

        /// <summary>
        /// 6 - 申请退款
        /// </summary>
        ApplyForRefund = 6,

        /// <summary>
        /// 7 - 申请退货
        /// </summary>
        ApplyForReturns = 7,

        /// <summary>
        /// 8 - 申请换货
        /// </summary>
        ApplyForReplacement = 8,

        /// <summary>
        /// 9 - 已退款
        /// </summary>
        Refunded = 9,

        /// <summary>
        /// 10 - 已退货
        /// </summary>
        Returned = 10,

        /// <summary>
        /// 11 - 今天
        /// </summary>
        Today = 11,

        /// <summary>
        /// 12 - 已删除
        /// </summary>
        Deleted = 12,


        /// <summary>
        /// 99 - 历史订单
        /// </summary>
        History = 0x63,




    }
}

