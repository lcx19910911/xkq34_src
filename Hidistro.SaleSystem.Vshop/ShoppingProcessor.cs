namespace Hidistro.SaleSystem.Vshop
{
    using Hidistro.Core;
    using Hidistro.Entities;
    using Hidistro.Entities.Commodities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Promotions;
    using Hidistro.Entities.Sales;
    using Hidistro.SqlDal;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Members;
    using Hidistro.SqlDal.Orders;
    using Hidistro.SqlDal.Promotions;
    using Hidistro.SqlDal.Sales;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Runtime.InteropServices;

    public static class ShoppingProcessor
    {
        private static object createOrderLocker = new object();

        public static decimal CalcFreight(int regionId, decimal totalWeight, ShippingModeInfo shippingModeInfo)
        {
            decimal price = 0M;
            int topRegionId = RegionHelper.GetTopRegionId(regionId);
            decimal num3 = totalWeight;
            int num4 = 1;
            if ((num3 > shippingModeInfo.Weight) && (shippingModeInfo.AddWeight.HasValue && (shippingModeInfo.AddWeight.Value > 0M)))
            {
                decimal num6 = num3 - shippingModeInfo.Weight;
                if ((num6 % shippingModeInfo.AddWeight) == 0M)
                {
                    num4 = Convert.ToInt32(Math.Truncate((decimal) ((num3 - shippingModeInfo.Weight) / shippingModeInfo.AddWeight.Value)));
                }
                else
                {
                    num4 = Convert.ToInt32(Math.Truncate((decimal) ((num3 - shippingModeInfo.Weight) / shippingModeInfo.AddWeight.Value))) + 1;
                }
            }
            if ((shippingModeInfo.ModeGroup == null) || (shippingModeInfo.ModeGroup.Count == 0))
            {
                if ((num3 > shippingModeInfo.Weight) && shippingModeInfo.AddPrice.HasValue)
                {
                    return ((num4 * shippingModeInfo.AddPrice.Value) + shippingModeInfo.Price);
                }
                return shippingModeInfo.Price;
            }
            int? nullable = null;
            foreach (ShippingModeGroupInfo info in shippingModeInfo.ModeGroup)
            {
                foreach (ShippingRegionInfo info2 in info.ModeRegions)
                {
                    if (topRegionId == info2.RegionId)
                    {
                        nullable = new int?(info2.GroupId);
                        break;
                    }
                }
                if (nullable.HasValue)
                {
                    if (num3 > shippingModeInfo.Weight)
                    {
                        price = (num4 * info.AddPrice) + info.Price;
                    }
                    else
                    {
                        price = info.Price;
                    }
                    break;
                }
            }
            if (nullable.HasValue)
            {
                return price;
            }
            if ((num3 > shippingModeInfo.Weight) && shippingModeInfo.AddPrice.HasValue)
            {
                return ((num4 * shippingModeInfo.AddPrice.Value) + shippingModeInfo.Price);
            }
            return shippingModeInfo.Price;
        }

        public static decimal CalcPayCharge(decimal cartMoney, PaymentModeInfo paymentModeInfo)
        {
            if (!paymentModeInfo.IsPercent)
            {
                return paymentModeInfo.Charge;
            }
            return (cartMoney * (paymentModeInfo.Charge / 100M));
        }

        private static void checkCanGroupBuy(int quantity, int groupBuyId)
        {
            GroupBuyInfo info = null;
            if (info.Status != GroupBuyStatus.UnderWay)
            {
                throw new OrderException("当前团购状态不允许购买");
            }
            if ((info.StartDate > DateTime.Now) || (info.EndDate < DateTime.Now))
            {
                throw new OrderException("当前不在团购时间范围内");
            }
            int num = info.MaxCount - info.SoldCount;
            if (quantity > num)
            {
                throw new OrderException("剩余可购买团购数量不够");
            }
        }

        public static bool CombineOrderToPay(string orderIds, string orderMarking)
        {
            return new OrderDao().CombineOrderToPay(orderIds, orderMarking);
        }

        public static OrderInfo ConvertShoppingCartToOrder(ShoppingCartInfo shoppingCart, bool isCountDown, bool isSignBuy)
        {
            if (shoppingCart.LineItems.Count == 0)
            {
                return null;
            }
            OrderInfo info = new OrderInfo {
                Points = shoppingCart.GetPoint(),
                ReducedPromotionId = shoppingCart.ReducedPromotionId,
                ReducedPromotionName = shoppingCart.ReducedPromotionName,
                ReducedPromotionAmount = shoppingCart.ReducedPromotionAmount,
                IsReduced = shoppingCart.IsReduced,
                SentTimesPointPromotionId = shoppingCart.SentTimesPointPromotionId,
                SentTimesPointPromotionName = shoppingCart.SentTimesPointPromotionName,
                IsSendTimesPoint = shoppingCart.IsSendTimesPoint,
                TimesPoint = shoppingCart.TimesPoint,
                FreightFreePromotionId = shoppingCart.FreightFreePromotionId,
                FreightFreePromotionName = shoppingCart.FreightFreePromotionName,
                IsFreightFree = shoppingCart.IsFreightFree
            };
            string str = string.Empty;
            if (shoppingCart.LineItems.Count > 0)
            {
                foreach (ShoppingCartItemInfo info2 in shoppingCart.LineItems)
                {
                    str = str + string.Format("'{0}',", info2.SkuId);
                }
            }
            if (shoppingCart.LineItems.Count > 0)
            {
                foreach (ShoppingCartItemInfo info2 in shoppingCart.LineItems)
                {
                    LineItemInfo info3 = new LineItemInfo {
                        SkuId = info2.SkuId,
                        ProductId = info2.ProductId,
                        SKU = info2.SKU,
                        Quantity = info2.Quantity,
                        ShipmentQuantity = info2.ShippQuantity
                    };
                    if ((info2.LimitedTimeDiscountId > 0) && !true)
                    {
                        info2.LimitedTimeDiscountId = 0;
                    }
                    info3.ItemCostPrice = new SkuDao().GetSkuItem(info2.SkuId).CostPrice;
                    info3.ItemListPrice = info2.MemberPrice;
                    info3.ItemAdjustedPrice = info2.AdjustedPrice;
                    info3.ItemDescription = info2.Name;
                    info3.ThumbnailsUrl = info2.ThumbnailUrl60;
                    info3.ItemWeight = info2.Weight;
                    info3.SKUContent = info2.SkuContent;
                    info3.PromotionId = info2.PromotionId;
                    info3.PromotionName = info2.PromotionName;
                    info3.MainCategoryPath = info2.MainCategoryPath;
                    info3.Type = info2.Type;
                    info3.ExchangeId = info2.ExchangeId;
                    info3.PointNumber = info2.PointNumber * info3.Quantity;
                    info3.ThirdCommission = info2.ThirdCommission;
                    info3.SecondCommission = info2.SecondCommission;
                    info3.FirstCommission = info2.FirstCommission;
                    info3.IsSetCommission = info2.IsSetCommission;
                    info3.LimitedTimeDiscountId = info2.LimitedTimeDiscountId;
                    info.LineItems.Add(info3.SkuId + info3.Type + info3.LimitedTimeDiscountId, info3);
                }
            }
            info.Tax = 0.00M;
            info.InvoiceTitle = "";
            return info;
        }

        public static bool CreatOrder(OrderInfo orderInfo)
        {
            bool flag = false;
            if (orderInfo.GetTotal() == 0M)
            {
                orderInfo.OrderStatus = OrderStatus.BuyerAlreadyPaid;
            }
            Database database = DatabaseFactory.CreateDatabase();
            //if (CS$<>9__CachedAnonymousMethodDelegate2 == null)
            //{
            //    CS$<>9__CachedAnonymousMethodDelegate2 = new Func<KeyValuePair<string, LineItemInfo>, int>(null, (IntPtr) <CreatOrder>b__1);
            //}
            //int quantity = Enumerable.Sum<KeyValuePair<string, LineItemInfo>>(orderInfo.LineItems, CS$<>9__CachedAnonymousMethodDelegate2);
            int quantity = orderInfo.LineItems.Sum<KeyValuePair<string, LineItemInfo>>((Func<KeyValuePair<string, LineItemInfo>, int>)(item => item.Value.Quantity));
            lock (createOrderLocker)
            {
                if (orderInfo.GroupBuyId > 0)
                {
                    checkCanGroupBuy(quantity, orderInfo.GroupBuyId);
                }
                using (DbConnection connection = database.CreateConnection())
                {
                    connection.Open();
                    DbTransaction dbTran = connection.BeginTransaction();
                    try
                    {
                        try
                        {
                            IntegralDetailInfo info4;
                            orderInfo.ClientShortType = (ClientShortType) Globals.GetClientShortType();
                            if (!new OrderDao().CreatOrder(orderInfo, dbTran))
                            {
                                dbTran.Rollback();
                                return false;
                            }
                            if ((orderInfo.LineItems.Count > 0) && !new LineItemDao().AddOrderLineItems(orderInfo.OrderId, orderInfo.LineItems.Values, dbTran))
                            {
                                dbTran.Rollback();
                                return false;
                            }
                            if (!string.IsNullOrEmpty(orderInfo.CouponCode) && !new CouponDao().AddCouponUseRecord(orderInfo, dbTran))
                            {
                                dbTran.Rollback();
                                return false;
                            }
                            ICollection values = orderInfo.LineItems.Values;
                            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
                            foreach (LineItemInfo info2 in values)
                            {
                                if ((info2.Type == 1) && (info2.ExchangeId > 0))
                                {
                                    PointExchangeChangedInfo info3;
                                    info3 = new PointExchangeChangedInfo {
                                        exChangeId = info2.ExchangeId,
                                        exChangeName = new OrderDao().GetexChangeName(info2.ExchangeId),
                                        ProductId = info2.ProductId,
                                        PointNumber = info2.PointNumber,
                                        MemberID = orderInfo.UserId,
                                        Date = DateTime.Now,
                                        MemberGrades = currentMember.GradeId
                                    };
                                    if (!new OrderDao().InsertPointExchange_Changed(info3, dbTran, info2.Quantity))
                                    {
                                        dbTran.Rollback();
                                        return false;
                                    }
                                    info4 = new IntegralDetailInfo {
                                        IntegralChange = -info2.PointNumber,
                                        IntegralSource = "积分兑换商品-订单号：" + orderInfo.OrderMarking,
                                        IntegralSourceType = 2,
                                        Remark = "积分兑换商品",
                                        Userid = orderInfo.UserId,
                                        GoToUrl = Globals.ApplicationPath + "/Vshop/MemberOrderDetails.aspx?OrderId=" + orderInfo.OrderId,
                                        IntegralStatus = Convert.ToInt32(IntegralDetailStatus.IntegralExchange)
                                    };
                                    if (!new IntegralDetailDao().AddIntegralDetail(info4, dbTran))
                                    {
                                        dbTran.Rollback();
                                        return false;
                                    }
                                }
                            }
                            if (orderInfo.PointExchange > 0)
                            {
                                info4 = new IntegralDetailInfo {
                                    IntegralChange = -orderInfo.PointExchange,
                                    IntegralSource = "积分抵现-订单号：" + orderInfo.OrderMarking,
                                    IntegralSourceType = 2,
                                    Remark = "积分抵现",
                                    Userid = orderInfo.UserId,
                                    GoToUrl = Globals.ApplicationPath + "/Vshop/MemberOrderDetails.aspx?OrderId=" + orderInfo.OrderId,
                                    IntegralStatus = Convert.ToInt32(IntegralDetailStatus.NowArrived)
                                };
                                if (!new IntegralDetailDao().AddIntegralDetail(info4, dbTran))
                                {
                                    dbTran.Rollback();
                                    return false;
                                }
                            }
                            if ((orderInfo.RedPagerID > 0) && !new OrderDao().UpdateCoupon_MemberCoupons(orderInfo, dbTran))
                            {
                                dbTran.Rollback();
                                return false;
                            }
                            dbTran.Commit();
                            flag = true;
                        }
                        catch
                        {
                            dbTran.Rollback();
                            throw;
                        }
                        return flag;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return flag;
        }

        public static OrderInfo GetCalculadtionCommission(OrderInfo order)
        {
            return new OrderDao().GetCalculadtionCommission(order, 0);
        }

        public static DataTable GetCoupon(decimal orderAmount)
        {
            return null;
        }

        public static CouponInfo GetCoupon(string couponCode)
        {
            return new CouponDao().GetCouponDetails(int.Parse(couponCode));
        }

        public static OrderInfo GetOrderInfo(string orderId)
        {
            return new OrderDao().GetOrderInfoForLineItems(orderId);
        }

        public static List<OrderInfo> GetOrderMarkingOrderInfo(string OrderMarking)
        {
            List<OrderInfo> list = new List<OrderInfo>();
            DataTable orderMarkingAllOrderID = new OrderDao().GetOrderMarkingAllOrderID(OrderMarking);
            for (int i = 0; i < orderMarkingAllOrderID.Rows.Count; i++)
            {
                list.Add(new OrderDao().GetOrderInfo(orderMarkingAllOrderID.Rows[i]["OrderId"].ToString()));
            }
            return list;
        }

        public static DataTable GetOrderReturnTable(int userid, string ReturnsId, int type)
        {
            return new RefundDao().GetOrderReturnTable(userid, ReturnsId, type);
        }

        public static PaymentModeInfo GetPaymentMode(int modeId)
        {
            return new PaymentModeDao().GetPaymentMode(modeId);
        }

        public static IList<PaymentModeInfo> GetPaymentModes()
        {
            return new PaymentModeDao().GetPaymentModes();
        }

        public static SKUItem GetProductAndSku(MemberInfo member, int productId, string options)
        {
            return new SkuDao().GetProductAndSku(member, productId, options);
        }

        public static bool GetReturnInfo(int userid, string OrderId, int ProductId, string SkuID)
        {
            return new RefundDao().GetReturnInfo(userid, OrderId, ProductId, SkuID);
        }

        public static bool GetReturnMes(int userid, string OrderId, int ProductId, int HandleStatus)
        {
            return new RefundDao().GetReturnMes(userid, OrderId, ProductId, HandleStatus);
        }

        public static int GetUserOrders(int userId)
        {
            return new OrderDao().GetUserOrders(userId);
        }

        public static bool InsertCalculationCommission(ArrayList UserIdList, ArrayList ReferralBlanceList, string orderid, ArrayList OrdersTotalList, string userid)
        {
            return new OrderDao().InsertCalculationCommission(UserIdList, ReferralBlanceList, orderid, OrdersTotalList, userid);
        }

        public static bool InsertOrderRefund(RefundInfo refundInfo)
        {
            return new RefundDao().InsertOrderRefund(refundInfo);
        }

        public static string UpdateAdjustCommssions(string orderId, string itemid, decimal commssionmoney, decimal adjustcommssion)
        {
            string str = string.Empty;
            using (DbConnection connection = DatabaseFactory.CreateDatabase().CreateConnection())
            {
                connection.Open();
                DbTransaction dbTran = connection.BeginTransaction();
                try
                {
                    OrderInfo orderInfo = GetOrderInfo(orderId);
                    if (orderId == null)
                    {
                        return "订单编号不合法";
                    }
                    int userId = DistributorsBrower.GetCurrentDistributors(true).UserId;
                    if ((orderInfo.ReferralUserId != userId) || (orderInfo.OrderStatus != OrderStatus.WaitBuyerPay))
                    {
                        return "不是您的订单";
                    }
                    LineItemInfo lineItem = orderInfo.LineItems[itemid];
                    if ((lineItem == null) || (lineItem.ItemsCommission < adjustcommssion))
                    {
                        return "修改金额过大";
                    }
                    lineItem.ItemAdjustedCommssion = adjustcommssion;
                    lineItem.IsAdminModify = false;
                    if (!new LineItemDao().UpdateLineItem(orderId, lineItem, dbTran))
                    {
                        dbTran.Rollback();
                    }
                    if (!new OrderDao().UpdateOrder(orderInfo, dbTran))
                    {
                        dbTran.Rollback();
                        return "更新订单信息失败";
                    }
                    dbTran.Commit();
                    str = "1";
                }
                catch (Exception exception)
                {
                    str = exception.ToString();
                    dbTran.Rollback();
                }
                finally
                {
                    connection.Close();
                }
                return str;
            }
        }

        public static bool UpdateCalculadtionCommission(OrderInfo order)
        {
            return new OrderDao().UpdateCalculadtionCommission(order, null);
        }

        public static bool UpdateOrder(OrderInfo order, [Optional, DefaultParameterValue(null)] DbTransaction dbTran)
        {
            return new OrderDao().UpdateOrder(order, dbTran);
        }

        public static bool UpdateOrderGoodStatu(string orderid, string skuid, int OrderItemsStatus)
        {
            return new RefundDao().UpdateOrderGoodStatu(orderid, skuid, OrderItemsStatus);
        }

        public static CouponInfo UseCoupon(decimal orderAmount, string claimCode)
        {
            if (!string.IsNullOrEmpty(claimCode))
            {
                CouponInfo coupon = GetCoupon(claimCode);
                if (coupon.ConditionValue <= orderAmount)
                {
                    return coupon;
                }
            }
            return null;
        }
    }
}

