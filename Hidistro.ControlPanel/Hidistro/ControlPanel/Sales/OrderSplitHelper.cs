namespace Hidistro.ControlPanel.Sales
{
    using Hidistro.Core;
    using Hidistro.Entities.Orders;
    using Hidistro.SqlDal.Orders;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class OrderSplitHelper
    {
        public static string CancelSplitOrderByID(string oldorderid, int fromsplitid, int itemid)
        {
            string str = string.Empty;
            OrderSplitInfo orderSplitInfo = GetOrderSplitInfo(fromsplitid);
            if (orderSplitInfo == null)
            {
                return str;
            }
            string itemList = orderSplitInfo.ItemList;
            OrderSplitInfo orderSplitInfoByOrderIDAndNum = GetOrderSplitInfoByOrderIDAndNum(1, oldorderid);
            if (orderSplitInfoByOrderIDAndNum == null)
            {
                return str;
            }
            if (!itemList.Contains<char>(','))
            {
                orderSplitInfoByOrderIDAndNum.ItemList = orderSplitInfoByOrderIDAndNum.ItemList + "," + itemid.ToString();
                orderSplitInfoByOrderIDAndNum.UpdateTime = DateTime.Now;
                new OrderSplitDao().UpdateOrderSplitInfo(orderSplitInfoByOrderIDAndNum);
                new OrderSplitDao().DelOrderSplitInfo(fromsplitid);
                return "1";
            }
            orderSplitInfoByOrderIDAndNum.ItemList = orderSplitInfoByOrderIDAndNum.ItemList + "," + itemid.ToString();
            orderSplitInfoByOrderIDAndNum.UpdateTime = DateTime.Now;
            new OrderSplitDao().UpdateOrderSplitInfo(orderSplitInfoByOrderIDAndNum);
            orderSplitInfo.ItemList = ("," + orderSplitInfo.ItemList + ",").Replace("," + itemid.ToString() + ",", ",").Trim(new char[] { ',' });
            orderSplitInfo.UpdateTime = DateTime.Now;
            new OrderSplitDao().UpdateOrderSplitInfo(orderSplitInfo);
            return "1";
        }

        public static bool DelOrderSplitByOrderID(string oldorderid, [Optional, DefaultParameterValue(null)] DbTransaction dbTran)
        {
            return new OrderSplitDao().DelOrderSplitByOrderID(oldorderid, dbTran);
        }

        public static LineItemInfo GetLineItemInfo(int id, string orderid)
        {
            return new LineItemDao().GetLineItemInfo(id, orderid);
        }

        public static OrderSplitInfo GetOrderSplitInfo(int id)
        {
            return new OrderSplitDao().GetOrderSplitInfo(id);
        }

        public static OrderSplitInfo GetOrderSplitInfoByOrderIDAndNum(int orderidnum, string oldorderid)
        {
            return new OrderSplitDao().GetOrderSplitInfoByOrderIDAndNum(orderidnum, oldorderid);
        }

        public static IList<OrderSplitInfo> GetOrderSplitItems(string orderid)
        {
            return new OrderSplitDao().GetOrderSplitItems(orderid);
        }

        public static string OrderSplitToTemp(OrderInfo OldOrderInfo, string skuid, string neworderid, int itemid)
        {
            IList<OrderSplitInfo> orderSplitItems;
            OrderSplitInfo info2;
            string itemList;
            int id;
            decimal num3;
            decimal num4;
            string[] strArray;
            LineItemInfo lineItemInfo;
            decimal num5;
            int num6;
            int orderIDNum;
            string str7;
            string str = string.Empty;
            string orderId = OldOrderInfo.OrderId;
            string str3 = string.Empty;
            string str4 = string.Empty;
            if (OldOrderInfo == null)
            {
                return str;
            }
            if (!(neworderid == "0"))
            {
                orderSplitItems = new OrderSplitDao().GetOrderSplitItems(orderId);
                if (orderSplitItems.Count <= 0)
                {
                    return "-2";
                }
                itemList = string.Empty;
                id = 0;
                num6 = 0;
                orderIDNum = 0;
                str7 = string.Empty;
                int num8 = 0;
                foreach (OrderSplitInfo info3 in orderSplitItems)
                {
                    if (info3.OrderIDNum == 1)
                    {
                        itemList = info3.ItemList;
                        id = info3.Id;
                        num8++;
                    }
                    if (info3.Id.ToString() == neworderid)
                    {
                        str7 = info3.ItemList;
                        num6 = info3.Id;
                        orderIDNum = info3.OrderIDNum;
                        num8++;
                    }
                    if (num8 == 2)
                    {
                        break;
                    }
                }
            }
            else
            {
                int maxOrderIDNum;
                orderSplitItems = new OrderSplitDao().GetOrderSplitItems(orderId);
                if (orderSplitItems.Count == 0)
                {
                    foreach (LineItemInfo info in OldOrderInfo.LineItems.Values)
                    {
                        if (info.ID == itemid)
                        {
                            str3 = info.ID.ToString();
                        }
                        else
                        {
                            str4 = str4 + "," + info.ID.ToString();
                        }
                    }
                    str4 = str4.Trim(new char[] { ',' });
                    info2 = new OrderSplitInfo();
                    maxOrderIDNum = 1;
                    info2.OldOrderId = orderId;
                    info2.OrderIDNum = maxOrderIDNum;
                    info2.ItemList = str4;
                    info2.UpdateTime = DateTime.Now;
                    info2.AdjustedFreight = OldOrderInfo.AdjustedFreight;
                    new OrderSplitDao().NewOrderSplit(info2);
                    info2.ItemList = str3;
                    info2.OrderIDNum = maxOrderIDNum + 1;
                    info2.UpdateTime = DateTime.Now;
                    info2.AdjustedFreight = 0M;
                    new OrderSplitDao().NewOrderSplit(info2);
                    return "1";
                }
                itemList = string.Empty;
                id = 0;
                foreach (OrderSplitInfo info3 in orderSplitItems)
                {
                    if (info3.OrderIDNum == 1)
                    {
                        itemList = info3.ItemList;
                        id = info3.Id;
                        break;
                    }
                }
                LineItemInfo info4 = new LineItemDao().GetReturnMoneyByOrderIDAndProductID(orderId, skuid, itemid);
                if ((info4 != null) && ("," + itemList + ",").Contains("," + info4.ID + ","))
                {
                    num3 = 0M;
                    num4 = 0M;
                    strArray = itemList.Split(new char[] { ',' });
                    if (strArray.Length > 1)
                    {
                        foreach (string str6 in strArray)
                        {
                            lineItemInfo = new LineItemDao().GetLineItemInfo(Globals.ToNum(str6), orderId);
                            if (lineItemInfo != null)
                            {
                                num5 = 0M;
                                if (lineItemInfo.Type == 0)
                                {
                                    num5 = ((lineItemInfo.ItemAdjustedPrice * lineItemInfo.Quantity) - lineItemInfo.ItemAdjustedCommssion) - lineItemInfo.DiscountAverage;
                                }
                                if (lineItemInfo.ID == itemid)
                                {
                                    num4 = num5;
                                    str3 = lineItemInfo.ID.ToString();
                                }
                                num3 += num5;
                            }
                        }
                        if ((num3 > num4) && (num4 > 0M))
                        {
                            info2 = new OrderSplitInfo();
                            maxOrderIDNum = new OrderSplitDao().GetMaxOrderIDNum(orderId);
                            info2.Id = id;
                            info2.OldOrderId = orderId;
                            info2.OrderIDNum = 1;
                            info2.ItemList = ("," + itemList + ",").Replace("," + info4.ID.ToString() + ",", ",").Trim(new char[] { ',' });
                            info2.UpdateTime = DateTime.Now;
                            info2.AdjustedFreight = OldOrderInfo.AdjustedFreight;
                            new OrderSplitDao().UpdateOrderSplitInfo(info2);
                            info2.AdjustedFreight = 0M;
                            info2.ItemList = info4.ID.ToString();
                            info2.OrderIDNum = maxOrderIDNum + 1;
                            info2.UpdateTime = DateTime.Now;
                            new OrderSplitDao().NewOrderSplit(info2);
                            return "1";
                        }
                        return "-3";
                    }
                    return "-1";
                }
                return "-2";
            }
            num3 = 0M;
            num4 = 0M;
            strArray = itemList.Split(new char[] { ',' });
            if (strArray.Length > 1)
            {
                foreach (string str6 in strArray)
                {
                    lineItemInfo = new LineItemDao().GetLineItemInfo(Globals.ToNum(str6), orderId);
                    if (lineItemInfo != null)
                    {
                        num5 = 0M;
                        if (lineItemInfo.Type == 0)
                        {
                            num5 = ((lineItemInfo.ItemAdjustedPrice * lineItemInfo.Quantity) - lineItemInfo.ItemAdjustedCommssion) - lineItemInfo.DiscountAverage;
                        }
                        if (lineItemInfo.ID == itemid)
                        {
                            num4 = num5;
                            str3 = lineItemInfo.ID.ToString();
                        }
                        num3 += num5;
                    }
                }
                if ((num3 > num4) && (num4 > 0M))
                {
                    info2 = new OrderSplitInfo {
                        Id = id,
                        OldOrderId = orderId,
                        OrderIDNum = 1,
                        ItemList = ("," + itemList + ",").Replace("," + str3 + ",", ",").Trim(new char[] { ',' }),
                        UpdateTime = DateTime.Now,
                        AdjustedFreight = OldOrderInfo.AdjustedFreight
                    };
                    new OrderSplitDao().UpdateOrderSplitInfo(info2);
                    info2.Id = num6;
                    info2.AdjustedFreight = 0M;
                    info2.ItemList = str7 + "," + str3;
                    info2.OrderIDNum = orderIDNum;
                    info2.UpdateTime = DateTime.Now;
                    new OrderSplitDao().UpdateOrderSplitInfo(info2);
                    return "1";
                }
                return "-3";
            }
            return "-1";
        }

        public static string UpdateAndCreateOrderByOrderSplitInfo(IList<OrderSplitInfo> infoList, OrderInfo oldorderinfo)
        {
            string str = "1";
            using (DbConnection connection = DatabaseFactory.CreateDatabase().CreateConnection())
            {
                connection.Open();
                DbTransaction dbTran = connection.BeginTransaction();
                StringBuilder builder = new StringBuilder();
                try
                {
                    decimal num = 0M;
                    foreach (OrderSplitInfo info in infoList)
                    {
                        OrderInfo orderInfo = new OrderInfo();
                        if (info.OrderIDNum != 1)
                        {
                            string itemList = info.ItemList;
                            if (string.IsNullOrEmpty(itemList))
                            {
                                dbTran.Rollback();
                                return "订单拆分失败";
                            }
                            string orderid = oldorderinfo.OrderId + "-" + info.OrderIDNum.ToString();
                            decimal num2 = 0M;
                            decimal num3 = 0M;
                            string[] strArray = itemList.Split(new char[] { ',' });
                            foreach (string str4 in strArray)
                            {
                                LineItemInfo lineItemInfo = new LineItemDao().GetLineItemInfo(Globals.ToNum(str4), "");
                                if (lineItemInfo != null)
                                {
                                    num2 += lineItemInfo.ItemWeight * lineItemInfo.Quantity;
                                    num += num2;
                                    num3 += lineItemInfo.ItemAdjustedPrice * lineItemInfo.Quantity;
                                }
                                else
                                {
                                    dbTran.Rollback();
                                    return "订单详情更新失败";
                                }
                            }
                            builder.Append("," + itemList.Trim(new char[] { ',' }));
                            if (!new LineItemDao().UpdateLineItemOrderID(itemList, orderid, dbTran))
                            {
                                dbTran.Rollback();
                                return "订单详情更新失败";
                            }
                            orderInfo.OrderId = orderid;
                            orderInfo.OrderMarking = oldorderinfo.OrderMarking;
                            orderInfo.ClientShortType = oldorderinfo.ClientShortType;
                            orderInfo.Remark = oldorderinfo.Remark;
                            orderInfo.ManagerMark = oldorderinfo.ManagerMark;
                            orderInfo.AdjustedDiscount = 0M;
                            orderInfo.OrderStatus = oldorderinfo.OrderStatus;
                            orderInfo.CloseReason = oldorderinfo.CloseReason;
                            orderInfo.OrderDate = oldorderinfo.OrderDate;
                            orderInfo.PayDate = oldorderinfo.PayDate;
                            orderInfo.ShippingDate = oldorderinfo.ShippingDate;
                            orderInfo.FinishDate = oldorderinfo.FinishDate;
                            orderInfo.UserId = oldorderinfo.UserId;
                            orderInfo.Username = oldorderinfo.Username;
                            orderInfo.EmailAddress = oldorderinfo.EmailAddress;
                            orderInfo.RealName = oldorderinfo.RealName;
                            orderInfo.QQ = oldorderinfo.QQ;
                            orderInfo.Wangwang = oldorderinfo.Wangwang;
                            orderInfo.MSN = oldorderinfo.MSN;
                            orderInfo.ShippingRegion = oldorderinfo.ShippingRegion;
                            orderInfo.Address = oldorderinfo.Address;
                            orderInfo.ZipCode = oldorderinfo.ZipCode;
                            orderInfo.ShipTo = oldorderinfo.ShipTo;
                            orderInfo.TelPhone = oldorderinfo.TelPhone;
                            orderInfo.CellPhone = oldorderinfo.CellPhone;
                            orderInfo.ShipToDate = oldorderinfo.ShipToDate;
                            orderInfo.ShippingModeId = oldorderinfo.ShippingModeId;
                            orderInfo.ModeName = oldorderinfo.ModeName;
                            orderInfo.RealShippingModeId = oldorderinfo.RealShippingModeId;
                            orderInfo.RealModeName = oldorderinfo.RealModeName;
                            orderInfo.RegionId = oldorderinfo.RegionId;
                            orderInfo.Freight = info.AdjustedFreight;
                            orderInfo.AdjustedFreight = info.AdjustedFreight;
                            orderInfo.ShipOrderNumber = oldorderinfo.ShipOrderNumber;
                            orderInfo.Weight = num2;
                            orderInfo.Weight = oldorderinfo.Weight;
                            orderInfo.ExpressCompanyName = oldorderinfo.ExpressCompanyName;
                            orderInfo.ExpressCompanyAbb = oldorderinfo.ExpressCompanyAbb;
                            orderInfo.PaymentTypeId = oldorderinfo.PaymentTypeId;
                            orderInfo.PaymentType = oldorderinfo.PaymentType;
                            orderInfo.PayCharge = oldorderinfo.PayCharge;
                            orderInfo.RefundStatus = oldorderinfo.RefundStatus;
                            orderInfo.RefundAmount = oldorderinfo.RefundAmount;
                            orderInfo.RefundRemark = oldorderinfo.RefundRemark;
                            orderInfo.Gateway = oldorderinfo.Gateway;
                            orderInfo.Points = 0;
                            orderInfo.DiscountAmount = 0M;
                            orderInfo.ActivitiesId = "";
                            orderInfo.ActivitiesName = "";
                            orderInfo.ReducedPromotionId = 0;
                            orderInfo.ReducedPromotionName = "";
                            orderInfo.ReducedPromotionAmount = 0M;
                            orderInfo.IsReduced = false;
                            orderInfo.SentTimesPointPromotionId = 0;
                            orderInfo.SentTimesPointPromotionName = "";
                            orderInfo.FreightFreePromotionId = 0;
                            orderInfo.FreightFreePromotionName = "";
                            orderInfo.IsFreightFree = oldorderinfo.IsFreightFree;
                            orderInfo.GatewayOrderId = oldorderinfo.GatewayOrderId;
                            orderInfo.IsPrinted = oldorderinfo.IsPrinted;
                            orderInfo.InvoiceTitle = oldorderinfo.InvoiceTitle;
                            orderInfo.ReferralUserId = oldorderinfo.ReferralUserId;
                            orderInfo.ReferralPath = oldorderinfo.ReferralPath;
                            orderInfo.RedPagerID = null;
                            orderInfo.RedPagerActivityName = "";
                            orderInfo.RedPagerOrderAmountCanUse = 0M;
                            orderInfo.RedPagerAmount = 0M;
                            orderInfo.PointToCash = 0M;
                            orderInfo.PointExchange = 0;
                            if (!new OrderDao().CreatOrder(orderInfo, dbTran))
                            {
                                dbTran.Rollback();
                                return "生成新订单失败";
                            }
                            if (!new OrderDao().UpdateOrderSplitState(orderInfo.OrderId, 2, dbTran))
                            {
                                dbTran.Rollback();
                                return "更新订单状态失败";
                            }
                        }
                    }
                    foreach (OrderSplitInfo info in infoList)
                    {
                        if (info.OrderIDNum == 1)
                        {
                            decimal num4 = oldorderinfo.Weight - num;
                            if (num4 > 0M)
                            {
                                oldorderinfo.Weight = num4;
                            }
                            oldorderinfo.AdjustedFreight = info.AdjustedFreight;
                            if (!new OrderDao().UpdateOrder(oldorderinfo, dbTran))
                            {
                                dbTran.Rollback();
                                return "更新订单失败";
                            }
                            if (!new OrderDao().UpdateOrderSplitState(oldorderinfo.OrderId, 1, dbTran))
                            {
                                dbTran.Rollback();
                                return "更新主订单状态失败";
                            }
                            if (!new OrderSplitDao().DelOrderSplitByOrderID(oldorderinfo.OrderId, dbTran))
                            {
                                dbTran.Rollback();
                                return "删除拆分记录失败";
                            }
                        }
                    }
                    dbTran.Commit();
                    foreach (OrderSplitInfo info in infoList)
                    {
                        OrderInfo order = new OrderDao().GetOrderInfo(info.OldOrderId + ((info.OrderIDNum == 1) ? "" : ("-" + info.OrderIDNum.ToString())));
                        if (order != null)
                        {
                            if (oldorderinfo.PayDate.HasValue)
                            {
                                order.PayDate = oldorderinfo.PayDate;
                            }
                            int num5 = 0;
                            foreach (LineItemInfo info5 in order.LineItems.Values)
                            {
                                if ((info5.OrderItemsStatus.ToString() == OrderStatus.Refunded.ToString()) || (info5.OrderItemsStatus.ToString() == OrderStatus.Returned.ToString()))
                                {
                                    num5++;
                                }
                            }
                            if (order.LineItems.Values.Count == num5)
                            {
                                order.OrderStatus = OrderStatus.Closed;
                            }
                            new OrderDao().UpdateOrder(order, null);
                        }
                    }
                    return str;
                }
                catch
                {
                    dbTran.Rollback();
                    str = "系统错误";
                }
                finally
                {
                    connection.Close();
                }
            }
            return str;
        }

        public static bool UpdateOrderSplitFright(OrderSplitInfo info)
        {
            return new OrderSplitDao().UpdateOrderSplitFright(info);
        }

        public static bool UpdateOrderSplitInfo(OrderSplitInfo info)
        {
            return new OrderSplitDao().UpdateOrderSplitInfo(info);
        }
    }
}

