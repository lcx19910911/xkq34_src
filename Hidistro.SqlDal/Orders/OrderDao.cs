namespace Hidistro.SqlDal.Orders
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities;
    using Hidistro.Entities.Commodities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Promotions;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Members;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.InteropServices;
    using System.Text;

    public class OrderDao
    {
        private Database database = DatabaseFactory.CreateDatabase();

        public bool AddMemberPointNumber(int PointNumber, OrderInfo orderInfo, DbTransaction dbTran)
        {
            IntegralDetailInfo point = new IntegralDetailInfo {
                IntegralChange = PointNumber,
                IntegralSource = "获取积分-订单号：" + orderInfo.OrderId,
                IntegralSourceType = 1
            };
            string activitiesName = orderInfo.ActivitiesName;
            if (!string.IsNullOrEmpty(activitiesName))
            {
                point.Remark = "活动送积分：" + activitiesName;
            }
            else
            {
                point.Remark = "购物获取积分";
            }
            point.Userid = orderInfo.UserId;
            point.GoToUrl = Globals.ApplicationPath + "/Vshop/MemberOrderDetails.aspx?OrderId=" + orderInfo.OrderId;
            point.IntegralStatus = Convert.ToInt32(IntegralDetailStatus.OrderToIntegral);
            if (!new IntegralDetailDao().AddIntegralDetail(point, dbTran))
            {
                dbTran.Rollback();
                return false;
            }
            return true;
        }

        public bool CheckRefund(string orderId, string Operator, string adminRemark, int refundType, bool accept)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("UPDATE Hishop_Orders SET OrderStatus = @OrderStatus WHERE OrderId = @OrderId;");
            builder.Append(" update Hishop_OrderRefund set Operator=@Operator,AdminRemark=@AdminRemark,HandleStatus=@HandleStatus,HandleTime=@HandleTime where HandleStatus=0 and OrderId = @OrderId;");
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(builder.ToString());
            if (accept)
            {
                this.database.AddInParameter(sqlStringCommand, "OrderStatus", DbType.Int32, 9);
                this.database.AddInParameter(sqlStringCommand, "HandleStatus", DbType.Int32, 1);
            }
            else
            {
                this.database.AddInParameter(sqlStringCommand, "OrderStatus", DbType.Int32, 2);
                this.database.AddInParameter(sqlStringCommand, "HandleStatus", DbType.Int32, 2);
            }
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.AddInParameter(sqlStringCommand, "Operator", DbType.String, Operator);
            this.database.AddInParameter(sqlStringCommand, "AdminRemark", DbType.String, adminRemark);
            this.database.AddInParameter(sqlStringCommand, "HandleTime", DbType.DateTime, DateTime.Now);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool CombineOrderToPay(string orderIds, string orderMarking)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            string query = string.Format("update Hishop_Orders set OrderMarking=@OrderMarking WHERE OrderId IN({0})", orderIds);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderMarking", DbType.String, orderMarking);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool CreatOrder(OrderInfo orderInfo, DbTransaction dbTran)
        {
            DbCommand storedProcCommand = this.database.GetStoredProcCommand("ss_CreateOrder");
            this.database.AddInParameter(storedProcCommand, "OrderId", DbType.String, orderInfo.OrderId);
            this.database.AddInParameter(storedProcCommand, "OrderMarking", DbType.String, orderInfo.OrderMarking);
            this.database.AddInParameter(storedProcCommand, "OrderDate", DbType.DateTime, orderInfo.OrderDate);
            this.database.AddInParameter(storedProcCommand, "UserId", DbType.Int32, orderInfo.UserId);
            this.database.AddInParameter(storedProcCommand, "UserName", DbType.String, orderInfo.Username);
            this.database.AddInParameter(storedProcCommand, "Wangwang", DbType.String, orderInfo.Wangwang);
            this.database.AddInParameter(storedProcCommand, "RealName", DbType.String, orderInfo.RealName);
            this.database.AddInParameter(storedProcCommand, "EmailAddress", DbType.String, orderInfo.EmailAddress);
            this.database.AddInParameter(storedProcCommand, "Remark", DbType.String, orderInfo.Remark);
            this.database.AddInParameter(storedProcCommand, "ClientShortType", DbType.Int32, orderInfo.ClientShortType);
            this.database.AddInParameter(storedProcCommand, "AdjustedDiscount", DbType.Currency, orderInfo.AdjustedDiscount);
            this.database.AddInParameter(storedProcCommand, "OrderStatus", DbType.Int32, (int) orderInfo.OrderStatus);
            this.database.AddInParameter(storedProcCommand, "ShippingRegion", DbType.String, orderInfo.ShippingRegion);
            this.database.AddInParameter(storedProcCommand, "Address", DbType.String, orderInfo.Address);
            this.database.AddInParameter(storedProcCommand, "ZipCode", DbType.String, orderInfo.ZipCode);
            this.database.AddInParameter(storedProcCommand, "ShipTo", DbType.String, orderInfo.ShipTo);
            this.database.AddInParameter(storedProcCommand, "TelPhone", DbType.String, orderInfo.TelPhone);
            this.database.AddInParameter(storedProcCommand, "CellPhone", DbType.String, orderInfo.CellPhone);
            this.database.AddInParameter(storedProcCommand, "ShipToDate", DbType.String, orderInfo.ShipToDate);
            this.database.AddInParameter(storedProcCommand, "ShippingModeId", DbType.Int32, orderInfo.ShippingModeId);
            this.database.AddInParameter(storedProcCommand, "ModeName", DbType.String, orderInfo.ModeName);
            this.database.AddInParameter(storedProcCommand, "RegionId", DbType.Int32, orderInfo.RegionId);
            this.database.AddInParameter(storedProcCommand, "Freight", DbType.Currency, orderInfo.Freight);
            this.database.AddInParameter(storedProcCommand, "AdjustedFreight", DbType.Currency, orderInfo.AdjustedFreight);
            this.database.AddInParameter(storedProcCommand, "ShipOrderNumber", DbType.String, orderInfo.ShipOrderNumber);
            this.database.AddInParameter(storedProcCommand, "Weight", DbType.Int32, orderInfo.Weight);
            this.database.AddInParameter(storedProcCommand, "ExpressCompanyName", DbType.String, orderInfo.ExpressCompanyName);
            this.database.AddInParameter(storedProcCommand, "ExpressCompanyAbb", DbType.String, orderInfo.ExpressCompanyAbb);
            this.database.AddInParameter(storedProcCommand, "PaymentTypeId", DbType.Int32, orderInfo.PaymentTypeId);
            this.database.AddInParameter(storedProcCommand, "PaymentType", DbType.String, orderInfo.PaymentType);
            this.database.AddInParameter(storedProcCommand, "PayCharge", DbType.Currency, orderInfo.PayCharge);
            this.database.AddInParameter(storedProcCommand, "RefundStatus", DbType.Int32, (int) orderInfo.RefundStatus);
            this.database.AddInParameter(storedProcCommand, "Gateway", DbType.String, orderInfo.Gateway);
            this.database.AddInParameter(storedProcCommand, "OrderTotal", DbType.Currency, orderInfo.GetTotal());
            this.database.AddInParameter(storedProcCommand, "OrderPoint", DbType.Int32, orderInfo.Points);
            this.database.AddInParameter(storedProcCommand, "OrderCostPrice", DbType.Currency, orderInfo.GetCostPrice());
            this.database.AddInParameter(storedProcCommand, "OrderProfit", DbType.Currency, orderInfo.GetProfit());
            this.database.AddInParameter(storedProcCommand, "Amount", DbType.Currency, orderInfo.GetAmount());
            this.database.AddInParameter(storedProcCommand, "ReducedPromotionId", DbType.Int32, orderInfo.ReducedPromotionId);
            this.database.AddInParameter(storedProcCommand, "ReducedPromotionName", DbType.String, orderInfo.ReducedPromotionName);
            this.database.AddInParameter(storedProcCommand, "ReducedPromotionAmount", DbType.Currency, orderInfo.ReducedPromotionAmount);
            this.database.AddInParameter(storedProcCommand, "IsReduced", DbType.Boolean, orderInfo.IsReduced);
            this.database.AddInParameter(storedProcCommand, "SentTimesPointPromotionId", DbType.Int32, orderInfo.SentTimesPointPromotionId);
            this.database.AddInParameter(storedProcCommand, "SentTimesPointPromotionName", DbType.String, orderInfo.SentTimesPointPromotionName);
            this.database.AddInParameter(storedProcCommand, "TimesPoint", DbType.Currency, orderInfo.TimesPoint);
            this.database.AddInParameter(storedProcCommand, "IsSendTimesPoint", DbType.Boolean, orderInfo.IsSendTimesPoint);
            this.database.AddInParameter(storedProcCommand, "FreightFreePromotionId", DbType.Int32, orderInfo.FreightFreePromotionId);
            this.database.AddInParameter(storedProcCommand, "FreightFreePromotionName", DbType.String, orderInfo.FreightFreePromotionName);
            this.database.AddInParameter(storedProcCommand, "IsFreightFree", DbType.Boolean, orderInfo.IsFreightFree);
            this.database.AddInParameter(storedProcCommand, "CouponName", DbType.String, orderInfo.CouponName);
            this.database.AddInParameter(storedProcCommand, "CouponCode", DbType.String, orderInfo.CouponCode);
            this.database.AddInParameter(storedProcCommand, "CouponAmount", DbType.Currency, orderInfo.CouponAmount);
            this.database.AddInParameter(storedProcCommand, "CouponValue", DbType.Currency, orderInfo.CouponValue);
            this.database.AddInParameter(storedProcCommand, "RedPagerActivityName", DbType.String, orderInfo.RedPagerActivityName);
            this.database.AddInParameter(storedProcCommand, "RedPagerID", DbType.String, orderInfo.RedPagerID);
            this.database.AddInParameter(storedProcCommand, "RedPagerOrderAmountCanUse", DbType.Currency, orderInfo.RedPagerOrderAmountCanUse);
            this.database.AddInParameter(storedProcCommand, "RedPagerAmount", DbType.Currency, orderInfo.RedPagerAmount);
            if (orderInfo.GroupBuyId > 0)
            {
                this.database.AddInParameter(storedProcCommand, "GroupBuyId", DbType.Int32, orderInfo.GroupBuyId);
                this.database.AddInParameter(storedProcCommand, "NeedPrice", DbType.Currency, orderInfo.NeedPrice);
                this.database.AddInParameter(storedProcCommand, "GroupBuyStatus", DbType.Int32, 1);
            }
            else
            {
                this.database.AddInParameter(storedProcCommand, "GroupBuyId", DbType.Int32, DBNull.Value);
                this.database.AddInParameter(storedProcCommand, "NeedPrice", DbType.Currency, DBNull.Value);
                this.database.AddInParameter(storedProcCommand, "GroupBuyStatus", DbType.Int32, DBNull.Value);
            }
            if (orderInfo.CountDownBuyId > 0)
            {
                this.database.AddInParameter(storedProcCommand, "CountDownBuyId ", DbType.Int32, orderInfo.CountDownBuyId);
            }
            else
            {
                this.database.AddInParameter(storedProcCommand, "CountDownBuyId ", DbType.Int32, DBNull.Value);
            }
            if (orderInfo.BundlingID > 0)
            {
                this.database.AddInParameter(storedProcCommand, "BundlingID ", DbType.Int32, orderInfo.BundlingID);
                this.database.AddInParameter(storedProcCommand, "BundlingPrice", DbType.Currency, orderInfo.BundlingPrice);
            }
            else
            {
                this.database.AddInParameter(storedProcCommand, "BundlingID ", DbType.Int32, DBNull.Value);
                this.database.AddInParameter(storedProcCommand, "BundlingPrice", DbType.Currency, DBNull.Value);
            }
            this.database.AddInParameter(storedProcCommand, "Tax", DbType.Currency, orderInfo.Tax);
            this.database.AddInParameter(storedProcCommand, "InvoiceTitle", DbType.String, orderInfo.InvoiceTitle);
            this.database.AddInParameter(storedProcCommand, "ReferralUserId", DbType.Int32, orderInfo.ReferralUserId);
            this.database.AddInParameter(storedProcCommand, "ReferralPath", DbType.String, orderInfo.ReferralPath);
            this.database.AddInParameter(storedProcCommand, "DiscountAmount", DbType.Decimal, orderInfo.DiscountAmount);
            this.database.AddInParameter(storedProcCommand, "ActivitiesId", DbType.String, orderInfo.ActivitiesId);
            this.database.AddInParameter(storedProcCommand, "ActivitiesName", DbType.String, orderInfo.ActivitiesName);
            this.database.AddInParameter(storedProcCommand, "FirstCommission", DbType.Decimal, orderInfo.FirstCommission);
            this.database.AddInParameter(storedProcCommand, "SecondCommission", DbType.Decimal, orderInfo.SecondCommission);
            this.database.AddInParameter(storedProcCommand, "ThirdCommission", DbType.Decimal, orderInfo.ThirdCommission);
            this.database.AddInParameter(storedProcCommand, "PointToCash", DbType.Decimal, orderInfo.PointToCash);
            this.database.AddInParameter(storedProcCommand, "PointExchange", DbType.Int32, orderInfo.PointExchange);
            this.database.AddInParameter(storedProcCommand, "BargainDetialId", DbType.Int32, orderInfo.BargainDetialId);
            return (this.database.ExecuteNonQuery(storedProcCommand, dbTran) > 0);
        }

        public int DeleteOrders(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("update Hishop_OrderItems set DeleteBeforeState=OrderItemsStatus  WHERE OrderId IN({0});update Hishop_Orders set DeleteBeforeState=OrderStatus  WHERE OrderId IN({0})", orderIds));
            this.database.ExecuteNonQuery(sqlStringCommand);
            sqlStringCommand = this.database.GetSqlStringCommand(string.Format("update  Hishop_OrderItems set OrderItemsStatus={0} WHERE OrderId IN({1})", 12, orderIds));
            this.database.ExecuteNonQuery(sqlStringCommand);
            sqlStringCommand = this.database.GetSqlStringCommand(string.Format("update Hishop_Orders set OrderStatus={0}  WHERE OrderId IN({1})", 12, orderIds));
            return this.database.ExecuteNonQuery(sqlStringCommand);
        }

        public bool DeleteReturnRecordForSendGoods(string orderid)
        {
            string[] strArray = new string[] { "delete from Hishop_OrderReturns where OrderID=@OrderID and (HandleStatus=", 1.ToString(), " or HandleStatus=", 6.ToString(), ")" };
            string query = string.Concat(strArray);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderid);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool EditOrderShipNumber(string orderId, string shipNumber)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("UPDATE Hishop_Orders SET ShipOrderNumber=@ShipOrderNumber WHERE OrderId =@OrderId");
            this.database.AddInParameter(sqlStringCommand, "ShipOrderNumber", DbType.String, shipNumber);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool ExistsOrderByBargainDetialId(int userId, int bargainDetialId)
        {
            string query = "select count(*) from Hishop_Orders where BargainDetialId=@BargainDetialId and UserId=@UserId";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "BargainDetialId", DbType.Int32, bargainDetialId);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            return (int.Parse(this.database.ExecuteScalar(sqlStringCommand).ToString()) > 0);
        }

        public DataTable GetAllOrderID()
        {
            string query = "select OrderId,IsPrinted,OrderStatus,Gateway from Hishop_Orders with (nolock) ";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
        }

        public OrderInfo GetCalculadtionCommission(OrderInfo order, int isModifyOrders)
        {
            DistributorsDao dao = new DistributorsDao();
            DistributorGradeDao dao2 = new DistributorGradeDao();
            DistributorsInfo distributorInfo = null;
            if (order.ReferralUserId > 0)
            {
                distributorInfo = dao.GetDistributorInfo(order.ReferralUserId);
            }
            if (distributorInfo != null)
            {
                decimal num = 0M;
                decimal num2 = 0M;
                decimal num3 = 0M;
                bool flag = true;
                bool flag2 = false;
                bool flag3 = false;
                DataView defaultView = dao2.GetAllDistributorGrade().DefaultView;
                flag = distributorInfo.ReferralStatus == 0;
                if (distributorInfo.DistriGradeId.ToString() != "0")
                {
                    defaultView.RowFilter = " GradeId=" + distributorInfo.DistriGradeId;
                    if (defaultView.Count > 0)
                    {
                        num = decimal.Parse(defaultView[0]["FirstCommissionRise"].ToString());
                    }
                }
                if (!string.IsNullOrEmpty(distributorInfo.ReferralPath) && (distributorInfo.ReferralPath != "0"))
                {
                    DistributorsInfo info2;
                    string[] strArray = distributorInfo.ReferralPath.Split(new char[] { '|' });
                    if (strArray.Length == 1)
                    {
                        info2 = dao.GetDistributorInfo(Globals.ToNum(strArray[0]));
                        if (info2 != null)
                        {
                            flag2 = info2.ReferralStatus == 0;
                            if (info2.DistriGradeId.ToString() != "0")
                            {
                                defaultView.RowFilter = " GradeId=" + info2.DistriGradeId;
                                if (defaultView.Count > 0)
                                {
                                    num2 = decimal.Parse(defaultView[0]["SecondCommissionRise"].ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        info2 = dao.GetDistributorInfo(Globals.ToNum(strArray[1]));
                        if (info2 != null)
                        {
                            flag2 = info2.ReferralStatus == 0;
                            if (info2.DistriGradeId.ToString() != "0")
                            {
                                defaultView.RowFilter = " GradeId=" + info2.DistriGradeId;
                                if (defaultView.Count > 0)
                                {
                                    num2 = decimal.Parse(defaultView[0]["SecondCommissionRise"].ToString());
                                }
                            }
                        }
                        DistributorsInfo info3 = dao.GetDistributorInfo(Globals.ToNum(strArray[0]));
                        if (info3 != null)
                        {
                            flag3 = info3.ReferralStatus == 0;
                            if (info3.DistriGradeId.ToString() != "0")
                            {
                                defaultView.RowFilter = " GradeId=" + info3.DistriGradeId;
                                if (defaultView.Count > 0)
                                {
                                    num3 = decimal.Parse(defaultView[0]["ThirdCommissionRise"].ToString());
                                }
                            }
                        }
                    }
                }
                if ((flag2 || flag3) && !SettingsManager.GetMasterSettings(false).EnableCommission)
                {
                    flag2 = false;
                    flag3 = false;
                }
                Dictionary<string, LineItemInfo> lineItems = order.LineItems;
                LineItemInfo info = new LineItemInfo();
                DataView view2 = new CategoryDao().GetCategories().DefaultView;
                string s = null;
                string str2 = null;
                string str3 = null;
                foreach (KeyValuePair<string, LineItemInfo> pair in lineItems)
                {
                    info = pair.Value;
                    if (info.Type == 0)
                    {
                        info.ItemsCommission = 0M;
                        info.SecondItemsCommission = 0M;
                        info.ThirdItemsCommission = 0M;
                        decimal num4 = (info.GetSubTotal() - info.DiscountAverage) - info.ItemAdjustedCommssion;
                        if (num4 > 0M)
                        {
                            info = this.GetNewLineItemInfo(info);
                            if (info.IsSetCommission)
                            {
                                order.FirstCommission = (info.FirstCommission + num) / 100M;
                                order.SecondCommission = (info.SecondCommission + num2) / 100M;
                                order.ThirdCommission = (info.ThirdCommission + num3) / 100M;
                                info.ItemsCommission = flag ? ((info.FirstCommission > 0M) ? (order.FirstCommission * num4) : 0M) : 0M;
                                info.SecondItemsCommission = flag2 ? ((info.SecondCommission > 0M) ? (order.SecondCommission * num4) : 0M) : 0M;
                                info.ThirdItemsCommission = flag3 ? ((info.ThirdCommission > 0M) ? (order.ThirdCommission * num4) : 0M) : 0M;
                            }
                            else
                            {
                                DataTable productCategories = new ProductDao().GetProductCategories(info.ProductId);
                                if ((productCategories.Rows.Count > 0) && (productCategories.Rows[0][0].ToString() != "0"))
                                {
                                    view2.RowFilter = " CategoryId=" + productCategories.Rows[0][0];
                                    s = view2[0]["FirstCommission"].ToString();
                                    str2 = view2[0]["SecondCommission"].ToString();
                                    str3 = view2[0]["ThirdCommission"].ToString();
                                    order.FirstCommission = (decimal.Parse(s) + num) / 100M;
                                    order.SecondCommission = (decimal.Parse(str2) + num2) / 100M;
                                    order.ThirdCommission = (decimal.Parse(str3) + num3) / 100M;
                                    if (!((string.IsNullOrEmpty(s) || string.IsNullOrEmpty(str2)) || string.IsNullOrEmpty(str3)))
                                    {
                                        info.ItemsCommission = flag ? ((decimal.Parse(s) > 0M) ? (order.FirstCommission * num4) : 0M) : 0M;
                                        info.SecondItemsCommission = flag2 ? ((decimal.Parse(str2) > 0M) ? (order.SecondCommission * num4) : 0M) : 0M;
                                        info.ThirdItemsCommission = flag3 ? ((decimal.Parse(str3) > 0M) ? (order.ThirdCommission * num4) : 0M) : 0M;
                                    }
                                }
                            }
                        }
                        else
                        {
                            info.ItemsCommission = 0M;
                            info.SecondItemsCommission = 0M;
                            info.ThirdItemsCommission = 0M;
                        }
                    }
                    if (!string.IsNullOrEmpty(distributorInfo.ReferralPath) && (distributorInfo.ReferralPath != "0"))
                    {
                        if (distributorInfo.ReferralPath.Split(new char[] { '|' }).Length == 1)
                        {
                            info.ThirdItemsCommission = 0M;
                        }
                    }
                    else
                    {
                        info.SecondItemsCommission = 0M;
                        info.ThirdItemsCommission = 0M;
                    }
                }
            }
            return order;
        }

        public decimal GetCommossionByOrderId(string orderId, int userId)
        {
            string query = "select CommTotal from Hishop_Commissions WHERE OrderId=@OrderId AND UserId=@UserId";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int16, userId);
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if ((obj2 == null) || (obj2 is DBNull))
            {
                return 0M;
            }
            return (decimal) obj2;
        }

        public int GetCountOrderIDByStatus(OrderStatus? orderstatus, OrderStatus? itemstatus)
        {
            string str = string.Empty;
            if (orderstatus.HasValue)
            {
                str = " OrderStatus=" + ((int) orderstatus.Value);
            }
            if (itemstatus.HasValue)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str + " and ";
                }
                object obj2 = str;
                str = string.Concat(new object[] { obj2, " OrderId in(SELECT OrderId FROM Hishop_OrderItems WHERE OrderItemsStatus=", (int) itemstatus.Value, ")" });
            }
            if (!string.IsNullOrEmpty(str))
            {
                str = " where " + str;
            }
            string query = "select count(0) from Hishop_Orders with (nolock) " + str;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return Globals.ToNum(this.database.ExecuteScalar(sqlStringCommand));
        }

        public int GetCouponId(int ActDid, decimal OrderTotal, int OrderNumber)
        {
            string query = "select top 1 CouponId from Hishop_Activities_Detail where ActivitiesId=@ActivitiesId and  MeetMoney=<@OrderTotal and MeetNumber<=@OrderNumber order by  MeetMoney,MeetNumber DESC";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "ActivitiesId", DbType.Int32, ActDid);
            this.database.AddInParameter(sqlStringCommand, "OrderTotal", DbType.Decimal, OrderTotal);
            this.database.AddInParameter(sqlStringCommand, "OrderNumber", DbType.Int32, OrderNumber);
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            int num = 0;
            if ((obj2 != null) && (obj2 != DBNull.Value))
            {
                num = (int) obj2;
            }
            return num;
        }

        public DbQueryResult GetDeleteOrders(OrderQuery query)
        {
            StringBuilder builder = new StringBuilder("1=1");
            if (query.Type.HasValue)
            {
                if (((OrderQuery.OrderType) query.Type.Value) == OrderQuery.OrderType.GroupBuy)
                {
                    builder.Append(" And GroupBuyId > 0 ");
                }
                else
                {
                    builder.Append(" And GroupBuyId is null ");
                }
            }
            if ((query.OrderId != string.Empty) && (query.OrderId != null))
            {
                builder.AppendFormat(" AND OrderId = '{0}'", DataHelper.CleanSearchString(query.OrderId));
            }
            if (query.UserId.HasValue)
            {
                builder.AppendFormat(" AND UserId = '{0}'", query.UserId.Value);
            }
            if (query.PaymentType.HasValue)
            {
                builder.AppendFormat(" AND PaymentTypeId = '{0}'", query.PaymentType.Value);
            }
            if (query.GroupBuyId.HasValue)
            {
                builder.AppendFormat(" AND GroupBuyId = {0}", query.GroupBuyId.Value);
            }
            if (!string.IsNullOrEmpty(query.ProductName))
            {
                builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE ItemDescription LIKE '%{0}%')", DataHelper.CleanSearchString(query.ProductName));
            }
            if (query.OrderItemsStatus.HasValue)
            {
                if (((OrderStatus) query.OrderItemsStatus.Value) == OrderStatus.ApplyForRefund)
                {
                    builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE OrderItemsStatus in({0},{1}))", (int) query.OrderItemsStatus.Value, 7);
                }
                else
                {
                    builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE OrderItemsStatus={0})", (int) query.OrderItemsStatus.Value);
                }
            }
            if (!string.IsNullOrEmpty(query.ShipTo))
            {
                builder.AppendFormat(" AND (ShipTo LIKE '%{0}%' or CellPhone='{0}')", DataHelper.CleanSearchString(query.ShipTo));
            }
            if (query.RegionId.HasValue)
            {
                builder.AppendFormat(" AND ShippingRegion like '%{0}%'", DataHelper.CleanSearchString(RegionHelper.GetFullRegion(query.RegionId.Value, "，")));
            }
            if (!string.IsNullOrEmpty(query.UserName))
            {
                builder.AppendFormat(" AND  UserName  = '{0}' ", DataHelper.CleanSearchString(query.UserName));
            }
            builder.AppendFormat(" AND OrderStatus = {0}", 12);
            if (query.StartDate.HasValue)
            {
                builder.AppendFormat(" AND datediff(dd,'{0}',OrderDate)>=0", DataHelper.GetSafeDateTimeFormat(query.StartDate.Value));
            }
            if (query.EndDate.HasValue)
            {
                builder.AppendFormat(" AND datediff(dd,'{0}',OrderDate)<=0", DataHelper.GetSafeDateTimeFormat(query.EndDate.Value));
            }
            if (query.ShippingModeId.HasValue)
            {
                builder.AppendFormat(" AND ShippingModeId = {0}", query.ShippingModeId.Value);
            }
            if (query.IsPrinted.HasValue)
            {
                builder.AppendFormat(" AND ISNULL(IsPrinted, 0)={0}", query.IsPrinted.Value);
            }
            if (query.ShippingModeId > 0)
            {
                builder.AppendFormat(" AND ShippingModeId={0}", query.ShippingModeId);
            }
            if (!string.IsNullOrEmpty(query.StoreName))
            {
                builder.AppendFormat(" AND StoreName like '%{0}%' ", DataHelper.CleanSearchString(query.StoreName));
            }
            if (!string.IsNullOrEmpty(query.Gateway))
            {
                builder.AppendFormat(" AND Gateway='{0}' ", DataHelper.CleanSearchString(query.Gateway));
            }
            if (query.DeleteBeforeState > 0)
            {
                builder.AppendFormat(" AND DeleteBeforeState='{0}' ", DataHelper.CleanSearchString(query.DeleteBeforeState.ToString()));
            }
            return DataHelper.PagingByRownumber(query.PageIndex, query.PageSize, query.SortBy, query.SortOrder, query.IsCount, "vw_Hishop_Order", "OrderId", builder.ToString(), "*");
        }

        public DataSet GetDistributorOrder(OrderQuery query)
        {
            string str = string.Empty;
            if (query.Status == OrderStatus.Finished)
            {
                str = str + " AND OrderStatus=" + ((int) query.Status);
            }
            string str2 = "SELECT OrderId, OrderDate,FinishDate, OrderStatus,PaymentTypeId, OrderTotal,Gateway,FirstCommission,SecondCommission,ThirdCommission FROM Hishop_Orders o WHERE ReferralUserId = @UserId";
            str2 = (str2 + str + " ORDER BY OrderDate DESC") + " SELECT ID,OrderId,SkuId, ThumbnailsUrl, ItemDescription, SKUContent, SKU, ProductId,Quantity,ItemListPrice,ItemAdjustedCommssion,OrderItemsStatus,ItemsCommission,Type,ReturnMoney,IsAdminModify,LimitedTimeDiscountId FROM Hishop_OrderItems WHERE OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE ReferralUserId = @UserId" + str + ")";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str2);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, query.UserId);
            DataSet set = this.database.ExecuteDataSet(sqlStringCommand);
            DataColumn parentColumn = set.Tables[0].Columns["OrderId"];
            DataColumn childColumn = set.Tables[1].Columns["OrderId"];
            DataRelation relation = new DataRelation("OrderItems", parentColumn, childColumn);
            set.Relations.Add(relation);
            return set;
        }

        public DataSet GetDistributorOrderByDetials(OrderQuery query)
        {
            string str = string.Empty;
            if (query.Status == OrderStatus.Finished)
            {
                str = str + " AND OrderStatus=" + ((int) query.Status);
            }
            string str2 = "SELECT OrderId, OrderDate,FinishDate, OrderStatus,PaymentTypeId, OrderTotal,Gateway,FirstCommission,SecondCommission,ThirdCommission FROM Hishop_Orders o WHERE OrderId in (select OrderId from Hishop_Commissions where UserId=@UserId and ReferralUserId=@ReferralUserId) ";
            str2 = (str2 + str) + " ORDER BY OrderDate DESC" + " SELECT OrderId,SkuId, ThumbnailsUrl, ItemDescription, SKUContent, SKU, ProductId,Quantity,ItemListPrice,ItemAdjustedCommssion,OrderItemsStatus,ItemsCommission,Type,ReturnMoney,IsAdminModify,LimitedTimeDiscountId FROM Hishop_OrderItems WHERE OrderId IN (SELECT OrderId FROM Hishop_Commissions WHERE ReferralUserId = @ReferralUserId and  UserId=@UserId ) ORDER BY OrderId DESC";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str2);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, query.UserId);
            this.database.AddInParameter(sqlStringCommand, "ReferralUserId", DbType.Int32, query.ReferralUserId);
            DataSet set = this.database.ExecuteDataSet(sqlStringCommand);
            DataColumn parentColumn = set.Tables[0].Columns["OrderId"];
            DataColumn childColumn = set.Tables[1].Columns["OrderId"];
            DataRelation relation = new DataRelation("OrderItems", parentColumn, childColumn);
            set.Relations.Add(relation);
            return set;
        }

        public DbQueryResult GetDistributorOrderByStatus(OrderQuery query, int userId)
        {
            string filter = string.Empty;
            if (userId > 0)
            {
                filter = filter + string.Format("  UserId={0}", userId);
            }
            if (query.Status == OrderStatus.Finished)
            {
                filter = filter + " AND OrderStatus=" + ((int) query.Status);
            }
            return DataHelper.PagingByRownumber(query.PageIndex, query.PageSize, query.SortBy, query.SortOrder, query.IsCount, "vw_UserOrderByPage", "OrderId", filter, "*");
        }

        public int GetDistributorOrderCount(OrderQuery query)
        {
            string str = string.Empty;
            switch (query.Status)
            {
                case OrderStatus.Finished:
                    str = str + " AND OrderStatus=" + ((int) query.Status);
                    break;

                case OrderStatus.Today:
                {
                    string str2 = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
                    str = str + " AND OrderDate>='" + str2 + "'";
                    break;
                }
            }
            string str3 = "SELECT COUNT(*)  FROM Hishop_Orders o WHERE ReferralUserId = @ReferralUserId";
            str3 = str3 + str;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str3);
            sqlStringCommand.CommandType = CommandType.Text;
            this.database.AddInParameter(sqlStringCommand, "ReferralUserId", DbType.Int32, query.UserId);
            return (int) this.database.ExecuteScalar(sqlStringCommand);
        }

        public string GetexChangeName(int exChangeId)
        {
            string query = "select Name from Hishop_PointExChange_PointExChanges where id=" + exChangeId;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if ((obj2 == null) || (obj2 is DBNull))
            {
                return "";
            }
            return obj2.ToString();
        }

        public string GetFirstProductName(string OrderId)
        {
            string query = string.Format("select top 1 ItemDescription from Hishop_OrderItems where OrderId= '{0}'", OrderId);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return Convert.ToString(this.database.ExecuteScalar(sqlStringCommand));
        }

        public LineItemInfo GetNewLineItemInfo(LineItemInfo info)
        {
            ProductInfo productDetails = new ProductDao().GetProductDetails(info.ProductId);
            if (productDetails != null)
            {
                info.IsSetCommission = productDetails.IsSetCommission;
                info.FirstCommission = productDetails.FirstCommission;
                info.SecondCommission = productDetails.SecondCommission;
                info.ThirdCommission = productDetails.ThirdCommission;
            }
            return info;
        }

        public DataSet GetOrderGoods(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            this.database = DatabaseFactory.CreateDatabase();
            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT OrderId, ItemDescription AS ProductName, SKU, SKUContent, ShipmentQuantity,");
            builder.Append(" (SELECT Stock FROM Hishop_SKUs WHERE SkuId = oi.SkuId) + oi.ShipmentQuantity AS Stock, (SELECT Remark FROM Hishop_Orders WHERE OrderId = oi.OrderId) AS Remark");
            builder.Append(" FROM Hishop_OrderItems oi WHERE OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE OrderStatus = 2 or (OrderStatus = 1 AND Gateway='hishop.plugins.payment.podrequest'))");
            builder.Append(" AND (OrderItemsStatus=2 OR OrderItemsStatus=1)");
            builder.AppendFormat(" AND OrderId IN ({0}) ORDER BY OrderId;", orderIds);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(builder.ToString());
            return this.database.ExecuteDataSet(sqlStringCommand);
        }

        public OrderInfo GetOrderInfo(string orderId)
        {
            OrderInfo info = null;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("SELECT * FROM Hishop_Orders Where OrderId = @OrderId;  SELECT i.*,o.OrderStatus FROM Hishop_OrderItems i,Hishop_Orders o Where i.OrderId=o.OrderId AND i.OrderId = @OrderId ");
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                if (reader.Read())
                {
                    info = DataMapper.PopulateOrder(reader);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    info.LineItems.Add(reader["Id"].ToString(), DataMapper.PopulateLineItem(reader));
                }
            }
            return info;
        }

        public OrderInfo GetOrderInfoForLineItems(string orderId)
        {
            OrderInfo info = null;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("select isnull(b.OpenId,'') as  BuyerWXOpenId, isnull(c.OpenId,'') as  SalerWXOpenId  ,a.*\r\n                    from Hishop_Orders a  \r\n                    left join aspnet_Members b on a.UserId= b.UserId\r\n                    left join aspnet_Members c on a.ReferralUserId= c.UserId\r\n                    where a.OrderId = @OrderId;  \r\n               SELECT i.*,o.OrderStatus FROM Hishop_OrderItems i,Hishop_Orders o Where i.OrderId=o.OrderId AND i.OrderId = @OrderId ");
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                if (reader.Read())
                {
                    info = DataMapper.PopulateOrder(reader);
                }
                reader.NextResult();
                info.ItemCount = 0;
                while (reader.Read())
                {
                    info.LineItems.Add(reader["ID"].ToString(), DataMapper.PopulateLineItem(reader));
                    info.ItemCount++;
                }
            }
            return info;
        }

        public DataTable GetOrderMarkingAllOrderID(string OrderMarking)
        {
            string query = "select OrderId from Hishop_Orders where OrderStatus=1 and OrderMarking='" + OrderMarking + "'";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
        }

        public int GetOrderReferralUserId(string OrderId)
        {
            string query = "select ReferralUserId from Hishop_Orders where OrderId=@OrderId";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, OrderId);
            int num = 0;
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if ((obj2 != null) && (obj2 != DBNull.Value))
            {
                num = (int) obj2;
            }
            return num;
        }

        public DbQueryResult GetOrders(OrderQuery query)
        {
            StringBuilder builder = new StringBuilder("1=1");
            if (query.Type.HasValue)
            {
                if (((OrderQuery.OrderType) query.Type.Value) == OrderQuery.OrderType.GroupBuy)
                {
                    builder.Append(" And GroupBuyId > 0 ");
                }
                else
                {
                    builder.Append(" And GroupBuyId is null ");
                }
            }
            if ((query.OrderId != string.Empty) && (query.OrderId != null))
            {
                builder.AppendFormat(" AND OrderId = '{0}'", DataHelper.CleanSearchString(query.OrderId));
            }
            if (query.UserId.HasValue)
            {
                builder.AppendFormat(" AND UserId = '{0}'", query.UserId.Value);
            }
            if (query.PaymentType.HasValue)
            {
                builder.AppendFormat(" AND PaymentTypeId = '{0}'", query.PaymentType.Value);
            }
            if (query.GroupBuyId.HasValue)
            {
                builder.AppendFormat(" AND GroupBuyId = {0}", query.GroupBuyId.Value);
            }
            if (!string.IsNullOrEmpty(query.ProductName))
            {
                builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE ItemDescription LIKE '%{0}%')", DataHelper.CleanSearchString(query.ProductName));
            }
            if (query.OrderItemsStatus.HasValue)
            {
                if (((OrderStatus) query.OrderItemsStatus.Value) == OrderStatus.ApplyForRefund)
                {
                    builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE OrderItemsStatus in({0},{1}))", (int) query.OrderItemsStatus.Value, 7);
                }
                else
                {
                    builder.AppendFormat(" AND OrderId IN (SELECT OrderId FROM Hishop_OrderItems WHERE OrderItemsStatus={0})", (int) query.OrderItemsStatus.Value);
                }
            }
            if (!string.IsNullOrEmpty(query.ShipTo))
            {
                builder.AppendFormat(" AND (ShipTo LIKE '%{0}%' or CellPhone='{0}')", DataHelper.CleanSearchString(query.ShipTo));
            }
            if (query.RegionId.HasValue)
            {
                builder.AppendFormat(" AND ShippingRegion like '%{0}%'", DataHelper.CleanSearchString(RegionHelper.GetFullRegion(query.RegionId.Value, "，")));
            }
            if (!string.IsNullOrEmpty(query.UserName))
            {
                builder.AppendFormat(" AND  UserName  = '{0}' ", DataHelper.CleanSearchString(query.UserName));
            }
            if (query.Status == OrderStatus.History)
            {
                builder.AppendFormat(" AND OrderStatus != {0} AND OrderStatus != {1} AND OrderStatus != {2} AND OrderDate < '{3}'", new object[] { 1, 4, 9, DateTime.Now.AddMonths(-3) });
            }
            else if (query.Status == OrderStatus.BuyerAlreadyPaid)
            {
                builder.AppendFormat(" AND (OrderStatus = {0} OR (OrderStatus = 1 AND Gateway = 'hishop.plugins.payment.podrequest'))", (int) query.Status);
            }
            else if (query.Status != OrderStatus.All)
            {
                builder.AppendFormat(" AND OrderStatus = {0}", (int) query.Status);
            }
            builder.AppendFormat(" AND OrderStatus != {0}", 12);
            if (query.StartDate.HasValue)
            {
                builder.AppendFormat(" AND datediff(dd,'{0}',OrderDate)>=0", DataHelper.GetSafeDateTimeFormat(query.StartDate.Value));
            }
            if (query.EndDate.HasValue)
            {
                builder.AppendFormat(" AND datediff(dd,'{0}',OrderDate)<=0", DataHelper.GetSafeDateTimeFormat(query.EndDate.Value));
            }
            if (query.ShippingModeId.HasValue)
            {
                builder.AppendFormat(" AND ShippingModeId = {0}", query.ShippingModeId.Value);
            }
            if (query.IsPrinted.HasValue)
            {
                builder.AppendFormat(" AND ISNULL(IsPrinted, 0)={0}", query.IsPrinted.Value);
            }
            if (query.ShippingModeId > 0)
            {
                builder.AppendFormat(" AND ShippingModeId={0}", query.ShippingModeId);
            }
            if (!string.IsNullOrEmpty(query.StoreName))
            {
                builder.AppendFormat(" AND StoreName like '%{0}%' ", DataHelper.CleanSearchString(query.StoreName));
            }
            if (!string.IsNullOrEmpty(query.Gateway))
            {
                builder.AppendFormat(" AND Gateway='{0}' ", DataHelper.CleanSearchString(query.Gateway));
            }
            return DataHelper.PagingByRownumber(query.PageIndex, query.PageSize, query.SortBy, query.SortOrder, query.IsCount, "vw_Hishop_Order", "OrderId", builder.ToString(), "*");
        }

        public DataSet GetOrdersAndLines(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            this.database = DatabaseFactory.CreateDatabase();
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT * FROM Hishop_Orders WHERE  OrderId IN ({0}) order by  ShipOrderNumber asc,OrderDate desc ", orderIds);
            builder.AppendFormat(" SELECT * FROM Hishop_OrderItems WHERE OrderId IN ({0});", orderIds);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(builder.ToString());
            return this.database.ExecuteDataSet(sqlStringCommand);
        }

        public DataSet GetOrdersByOrderIDList(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            this.database = DatabaseFactory.CreateDatabase();
            string query = string.Empty;
            string str2 = " OrderId,ShipTo,RegionId,ExpressCompanyName,ExpressCompanyAbb,ShipOrderNumber,Remark,OrderStatus,ShippingRegion,Address";
            query = string.Format("with v as (SELECT " + str2 + ", row_number() over (partition by ShipTo+CONVERT(VARCHAR(11), RegionId)+ExpressCompanyAbb+[Address]+CellPhone order by  RegionId desc) as rownumber from Hishop_Orders where   OrderId in ({0})) select " + str2 + ",OrderStatus,rownumber from v", orderIds);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return this.database.ExecuteDataSet(sqlStringCommand);
        }

        public bool GetOrderUserAliOpenId(string OrderId, out string BuyerAliOpenId, out string SalerAliOpenId)
        {
            BuyerAliOpenId = "";
            SalerAliOpenId = "";
            string query = string.Format("select top 1 isnull(b.AlipayOpenid,'') as  BuyerWXOpenId, isnull(c.AlipayOpenid,'') as  SalerWXOpenId  from Hishop_Orders a   left join aspnet_Members b on a.UserId= b.UserId  left join aspnet_Members c on a.ReferralUserId= c.UserId  where a.OrderId = '{0}'", OrderId);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            DataTable table = this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
            if (table.Rows.Count > 0)
            {
                BuyerAliOpenId = Convert.ToString(table.Rows[0]["BuyerWXOpenId"]);
                SalerAliOpenId = Convert.ToString(table.Rows[0]["SalerWXOpenId"]);
                return true;
            }
            return false;
        }

        public bool GetOrderUserOpenId(string OrderId, out string BuyerWXOpenId, out string SalerWXOpenId)
        {
            BuyerWXOpenId = "";
            SalerWXOpenId = "";
            string query = string.Format("select top 1 isnull(b.OpenId,'') as  BuyerWXOpenId, isnull(c.OpenId,'') as  SalerWXOpenId  from Hishop_Orders a   left join aspnet_Members b on a.UserId= b.UserId  left join aspnet_Members c on a.ReferralUserId= c.UserId  where a.OrderId = '{0}'", OrderId);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            DataTable table = this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
            if (table.Rows.Count > 0)
            {
                BuyerWXOpenId = Convert.ToString(table.Rows[0]["BuyerWXOpenId"]);
                SalerWXOpenId = Convert.ToString(table.Rows[0]["SalerWXOpenId"]);
                return true;
            }
            return false;
        }

        public DataSet GetProductGoods(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            this.database = DatabaseFactory.CreateDatabase();
            StringBuilder builder = new StringBuilder();
            builder.Append("SELECT ItemDescription AS ProductName, SKU, SKUContent, sum(ShipmentQuantity) as ShipmentQuantity,");
            builder.Append(" (SELECT Stock FROM Hishop_SKUs WHERE SkuId = oi.SkuId) + sum(ShipmentQuantity) AS Stock FROM Hishop_OrderItems oi");
            builder.Append(" WHERE OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE OrderStatus = 2 or (OrderStatus = 1 AND Gateway='hishop.plugins.payment.podrequest'))");
            builder.Append(" AND OrderItemsStatus=2");
            builder.AppendFormat(" AND OrderId in ({0}) GROUP BY ItemDescription, SkuId, SKU, SKUContent;", orderIds);
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(builder.ToString());
            return this.database.ExecuteDataSet(sqlStringCommand);
        }

        public string GetReplaceComments(string orderId)
        {
            string query = "select Comments from Hishop_OrderReplace where HandleStatus=0 and OrderId='" + orderId + "'";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if ((obj2 == null) || (obj2 is DBNull))
            {
                return "";
            }
            return obj2.ToString();
        }

        public DataTable GetSendGoodsOrders(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("SELECT * FROM Hishop_Orders WHERE (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway = 'hishop.plugins.payment.podrequest')) AND OrderId IN ({0}) order by OrderDate desc", orderIds));
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return DataHelper.ConverDataReaderToDataTable(reader);
            }
        }

        public OrderInfo GetUserLastOrder(int userId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("SELECT top 1 * FROM Hishop_Orders Where UserId=@UserId and OrderStatus=5 order by orderdate desc");
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return ReaderConvert.ReaderToModel<OrderInfo>(reader);
            }
        }

        public DataSet GetUserOrder(int userId, OrderQuery query)
        {
            string str = string.Empty;
            if (query.Status == OrderStatus.WaitBuyerPay)
            {
                str = str + " AND OrderStatus = 1 AND Gateway <> 'hishop.plugins.payment.podrequest'";
            }
            else if (query.Status == OrderStatus.BuyerAlreadyPaid)
            {
                str = str + " AND (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway = 'hishop.plugins.payment.podrequest'))";
            }
            else if (query.Status == OrderStatus.SellerAlreadySent)
            {
                str = str + " AND OrderStatus = 3 ";
            }
            string str2 = "SELECT OrderId,OrderMarking, OrderDate, OrderStatus,PaymentTypeId, OrderTotal,   Gateway,(SELECT count(0) FROM vshop_OrderRedPager WHERE OrderId = o.OrderId and ExpiryDays<getdate() and AlreadyGetTimes<MaxGetTimes) as HasRedPage,(SELECT SUM(Quantity) FROM Hishop_OrderItems WHERE OrderId = o.OrderId) as ProductSum FROM Hishop_Orders o WHERE UserId = @UserId";
            str2 = (str2 + str + " ORDER BY OrderDate DESC") + " SELECT OrderId, ThumbnailsUrl, ItemDescription, SKUContent, SKU,OrderItemsStatus, ProductId,Quantity,ReturnMoney,SkuID FROM Hishop_OrderItems WHERE OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE UserId = @UserId" + str + ")";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str2);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            DataSet set = this.database.ExecuteDataSet(sqlStringCommand);
            DataColumn parentColumn = set.Tables[0].Columns["OrderId"];
            DataColumn childColumn = set.Tables[1].Columns["OrderId"];
            DataRelation relation = new DataRelation("OrderItems", parentColumn, childColumn);
            set.Relations.Add(relation);
            return set;
        }

        public DbQueryResult GetUserOrderByPage(int userId, OrderQuery query)
        {
            string str = string.Empty;
            if (query.Status == OrderStatus.WaitBuyerPay)
            {
                str = str + " AND OrderStatus = 1 AND Gateway <> 'hishop.plugins.payment.podrequest'";
            }
            else if (query.Status == OrderStatus.BuyerAlreadyPaid)
            {
                str = str + " AND (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway = 'hishop.plugins.payment.podrequest'))";
            }
            else if (query.Status == OrderStatus.SellerAlreadySent)
            {
                str = str + " AND OrderStatus = 3 ";
            }
            else
            {
                str = str + " AND OrderStatus<>12 ";
            }
            int num = ((query.PageIndex - 1) * query.PageSize) + 1;
            int num2 = (num + query.PageSize) - 1;
            string str2 = "SELECT OrderId FROM Hishop_Orders  WHERE OrderId in ( SELECT orderid from ( SELECT ROW_NUMBER() OVER (ORDER BY OrderDate DESC) AS ordernumber,OrderId  FROM Hishop_Orders o WHERE UserId = @UserId";
            object obj2 = str2 + str;
            str2 = string.Concat(new object[] { obj2, ") AS W where W.ordernumber between ", num, " AND  ", num2, ")" });
            string str3 = "SELECT * from( SELECT ROW_NUMBER() OVER (ORDER BY OrderDate DESC) AS ordernumber,OrderId,OrderMarking,OrderDate,OrderStatus,PaymentTypeId,OrderTotal,Gateway,(SELECT count(0) FROM vshop_OrderRedPager WHERE OrderId = o.OrderId and ExpiryDays<getdate() and AlreadyGetTimes<MaxGetTimes   ) as HasRedPage,(SELECT SUM(Quantity) FROM Hishop_OrderItems WHERE OrderId = o.OrderId) as ProductSum FROM Hishop_Orders o WHERE UserId = @UserId";
            obj2 = str3 + str;
            str3 = (string.Concat(new object[] { obj2, ") AS W where W.ordernumber between ", num, " AND  ", num2 }) + " SELECT ID, OrderId, ThumbnailsUrl, ItemDescription, SKUContent, SKU,OrderItemsStatus, ProductId,Quantity,ReturnMoney,SkuID,ItemAdjustedPrice,Type,PointNumber,LimitedTimeDiscountId FROM Hishop_OrderItems WHERE OrderId IN  (" + str2 + ")   ") + "  SELECT COUNT(*) FROM Hishop_Orders WHERE UserId = @UserId" + str;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str3);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            DataSet set = this.database.ExecuteDataSet(sqlStringCommand);
            DataColumn parentColumn = set.Tables[0].Columns["OrderId"];
            DataColumn childColumn = set.Tables[1].Columns["OrderId"];
            DataRelation relation = new DataRelation("OrderItems", parentColumn, childColumn);
            set.Relations.Add(relation);
            return new DbQueryResult { Data = set, TotalRecords = int.Parse(set.Tables[2].Rows[0][0].ToString()) };
        }

        public int GetUserOrderCount(int userId, OrderQuery query)
        {
            string str = string.Empty;
            if (query.Status == OrderStatus.WaitBuyerPay)
            {
                str = str + " AND OrderStatus = 1 AND Gateway <> 'hishop.plugins.payment.podrequest'";
            }
            else if (query.Status == OrderStatus.SellerAlreadySent)
            {
                str = str + " AND OrderStatus = 3  ";
            }
            else if (query.Status == OrderStatus.BuyerAlreadyPaid)
            {
                str = str + " AND (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway = 'hishop.plugins.payment.podrequest'))";
            }
            string str2 = "SELECT COUNT(1)  FROM Hishop_Orders o WHERE UserId = @UserId";
            str2 = str2 + str;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str2);
            sqlStringCommand.CommandType = CommandType.Text;
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            return (int) this.database.ExecuteScalar(sqlStringCommand);
        }

        public DataTable GetUserOrderPaidWaitFinish(int userId)
        {
            string str = " AND (OrderStatus = 2 or (OrderStatus = 3 and Gateway<>'hishop.plugins.payment.podrequest')) ";
            string query = "SELECT OrderId FROM Hishop_Orders WHERE UserId = @UserId";
            query = query + str + " ORDER BY OrderDate DESC";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            return this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
        }

        public DataSet GetUserOrderReturn(int userId, OrderQuery query)
        {
            string str = string.Empty + " AND (OrderStatus = 2 OR OrderStatus = 3) ";
            string str2 = "SELECT OrderId, OrderDate, OrderStatus,PaymentTypeId, OrderTotal, (SELECT SUM(Quantity) FROM Hishop_OrderItems WHERE OrderId = o.OrderId) as ProductSum FROM Hishop_Orders o WHERE UserId = @UserId";
            str2 = (str2 + str + " ORDER BY OrderDate DESC") + " SELECT OrderId, ThumbnailsUrl,Quantity, ItemDescription,OrderItemsStatus, SKUContent, SKU, ProductId,SkuID FROM Hishop_OrderItems WHERE IsHandled=0 and Type=0 and (OrderItemsStatus=2 OR OrderItemsStatus=3) AND OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE UserId = @UserId " + str + ") ";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(str2);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            DataSet set = this.database.ExecuteDataSet(sqlStringCommand);
            DataColumn parentColumn = set.Tables[0].Columns["OrderId"];
            DataColumn childColumn = set.Tables[1].Columns["OrderId"];
            DataRelation relation = new DataRelation("OrderItems", parentColumn, childColumn);
            set.Relations.Add(relation);
            return set;
        }

        public int GetUserOrderReturnCount(int userId)
        {
            object obj2 = string.Empty;
            string str = string.Concat(new object[] { obj2, " AND (OrderItemsStatus = ", 6, " OR OrderItemsStatus =", 7, ")" });
            string query = "SELECT COUNT(*) FROM Hishop_OrderItems WHERE OrderId IN (SELECT OrderId FROM Hishop_Orders WHERE UserId=@UserId)";
            query = query + str;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int32, userId);
            return (int) this.database.ExecuteScalar(sqlStringCommand);
        }

        public int GetUserOrders(int userId)
        {
            string query = "select count(OrderId) from Hishop_Orders WHERE UserId=@UserId";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "UserId", DbType.Int16, userId);
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if ((obj2 == null) || (obj2 is DBNull))
            {
                return 0;
            }
            return (int) obj2;
        }

        public bool InsertCalculationCommission(ArrayList UserIdList, ArrayList ReferralBlanceList, string orderid, ArrayList OrdersTotalList, string userid)
        {
            string query = "";
            query = query + "begin try  " + "  begin tran TranUpdate";
            for (int i = 0; i < UserIdList.Count; i++)
            {
                object obj2 = query;
                query = string.Concat(new object[] { obj2, " INSERT INTO [Hishop_Commissions]([UserId],[ReferralUserId],[OrderId],[OrderTotal],[CommTotal],[CommType],[State])VALUES(", UserIdList[i], ",", userid, ",'", orderid, "',", OrdersTotalList[i], ",", ReferralBlanceList[i], ",1,0);" });
            }
            query = query + " COMMIT TRAN TranUpdate" + "  end try \r\n                    begin catch \r\n                        ROLLBACK TRAN TranUpdate\r\n                    end catch ";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool InsertPointExchange_Changed(PointExchangeChangedInfo info, DbTransaction dbTran, [Optional, DefaultParameterValue(1)] int itemCount)
        {
            if (itemCount < 1)
            {
                return false;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < itemCount; i++)
            {
                builder.Append("INSERT INTO  Hishop_PointExchange_Changed ([exChangeId],[exChangeName],[ProductId],[PointNumber],[Date],[MemberID],[MemberGrades]) VALUES (@exChangeId,@exChangeName,@ProductId,@PointNumber,@Date,@MemberID,@MemberGrades);");
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(builder.ToString());
            this.database.AddInParameter(sqlStringCommand, "exChangeId", DbType.Int32, info.exChangeId);
            this.database.AddInParameter(sqlStringCommand, "ProductId", DbType.Int32, info.ProductId);
            this.database.AddInParameter(sqlStringCommand, "exChangeName", DbType.String, info.exChangeName);
            this.database.AddInParameter(sqlStringCommand, "PointNumber", DbType.Int32, info.PointNumber);
            this.database.AddInParameter(sqlStringCommand, "Date", DbType.DateTime, DateTime.Now);
            this.database.AddInParameter(sqlStringCommand, "MemberID", DbType.Int32, info.MemberID);
            this.database.AddInParameter(sqlStringCommand, "MemberGrades", DbType.String, info.MemberGrades);
            if (dbTran != null)
            {
                return (this.database.ExecuteNonQuery(sqlStringCommand, dbTran) > 0);
            }
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public int RealDeleteOrders(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("DELETE FROM Hishop_OrderItems WHERE OrderId IN({0});DELETE FROM Hishop_OrderReturns WHERE OrderId IN({0});", orderIds));
            this.database.ExecuteNonQuery(sqlStringCommand);
            sqlStringCommand = this.database.GetSqlStringCommand(string.Format("DELETE FROM Hishop_Orders WHERE OrderId IN({0})", orderIds));
            return this.database.ExecuteNonQuery(sqlStringCommand);
        }

        public int RestoreOrders(string orderIds)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("update Hishop_OrderItems set OrderItemsStatus =DeleteBeforeState  WHERE OrderId IN({0}); update Hishop_Orders set OrderStatus= DeleteBeforeState  WHERE OrderId IN({0}) ", orderIds));
            return this.database.ExecuteNonQuery(sqlStringCommand);
        }

        public bool SetOrderExpressComputerpe(string orderIds, string expressCompanyName, string expressCompanyAbb)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("UPDATE Hishop_Orders SET ExpressCompanyName=@ExpressCompanyName,ExpressCompanyAbb=@ExpressCompanyAbb WHERE (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway='hishop.plugins.payment.podrequest')) AND OrderId IN ({0})", orderIds));
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyName", DbType.String, expressCompanyName);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyAbb", DbType.String, expressCompanyAbb);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool SetOrderShippingMode(string orderIds, int realShippingModeId, string realModeName)
        {
            if (!(string.IsNullOrEmpty(orderIds) || orderIds.Contains("'")))
            {
                orderIds = "'" + orderIds.Replace(",", "','") + "'";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("UPDATE Hishop_Orders SET RealShippingModeId=@RealShippingModeId,RealModeName=@RealModeName WHERE (OrderStatus = 2 OR (OrderStatus = 1 AND Gateway='hishop.plugins.payment.podrequest')) AND OrderId IN ({0})", orderIds));
            this.database.AddInParameter(sqlStringCommand, "RealShippingModeId", DbType.Int32, realShippingModeId);
            this.database.AddInParameter(sqlStringCommand, "RealModeName", DbType.String, realModeName);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool SetPrintOrderExpress(string orderId, string expressCompanyName, string expressCompanyAbb, string shipOrderNumber)
        {
            string query = string.Empty;
            if (string.IsNullOrEmpty(shipOrderNumber))
            {
                query = "UPDATE Hishop_Orders SET ExpressCompanyName=@ExpressCompanyName,ExpressCompanyAbb=@ExpressCompanyAbb WHERE  OrderId=@OrderId";
            }
            else
            {
                query = "UPDATE Hishop_Orders SET IsPrinted=1,ShipOrderNumber=@ShipOrderNumber,ExpressCompanyName=@ExpressCompanyName,ExpressCompanyAbb=@ExpressCompanyAbb WHERE  OrderId=@OrderId";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.AddInParameter(sqlStringCommand, "ShipOrderNumber", DbType.String, shipOrderNumber);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyName", DbType.String, expressCompanyName);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyAbb", DbType.String, expressCompanyAbb);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool UpdateCalculadtionCommission(OrderInfo orderinfo, [Optional, DefaultParameterValue(null)] DbTransaction dbTran)
        {
            foreach (LineItemInfo info in orderinfo.LineItems.Values)
            {
                if ((info.OrderItemsStatus == OrderStatus.Refunded) || (info.OrderItemsStatus == OrderStatus.Returned))
                {
                    new LineItemDao().UpdateCommissionItem(info.ID, 0M, 0M, 0M, dbTran);
                }
                else
                {
                    new LineItemDao().UpdateCommissionItem(info.ID, info.ItemsCommission, info.SecondItemsCommission, info.ThirdItemsCommission, dbTran);
                }
            }
            return true;
        }

        public bool UpdateCoupon_MemberCoupons(OrderInfo orderinfo, DbTransaction dbTran)
        {
            string query = "update Hishop_Coupon_MemberCoupons set OrderNo=@OrderNo, Status=@Status,UsedDate=@UsedDate WHERE Id=@Id;\r\n                        update Hishop_Coupon_Coupons set UsedNum=isnull(UsedNum,0)+1 where CouponId=(select top 1 CouponId From Hishop_Coupon_MemberCoupons where Id=@Id);";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderNo", DbType.String, orderinfo.OrderId);
            this.database.AddInParameter(sqlStringCommand, "Id", DbType.Int32, orderinfo.RedPagerID);
            this.database.AddInParameter(sqlStringCommand, "Status", DbType.Int32, 1);
            this.database.AddInParameter(sqlStringCommand, "UsedDate", DbType.DateTime, DateTime.Now);
            if (dbTran != null)
            {
                return (this.database.ExecuteNonQuery(sqlStringCommand, dbTran) > 0);
            }
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public void UpdateItemsStatus(string orderId, int status, string ItemStr)
        {
            string query = string.Empty;
            if (ItemStr == "all")
            {
                query = "Update Hishop_OrderItems Set OrderItemsStatus=@OrderItemsStatus Where OrderId =@OrderId";
            }
            else
            {
                query = "Update Hishop_OrderItems Set OrderItemsStatus=@OrderItemsStatus Where OrderId =@OrderId and SkuId IN (" + ItemStr + ")";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderItemsStatus", DbType.Int32, status);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.ExecuteNonQuery(sqlStringCommand);
        }

        public bool UpdateOrder(OrderInfo order, [Optional, DefaultParameterValue(null)] DbTransaction dbTran)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("UPDATE Hishop_Orders SET  OrderStatus = @OrderStatus, CloseReason=@CloseReason, PayDate = @PayDate, ShippingDate=@ShippingDate, FinishDate = @FinishDate, RegionId = @RegionId, ShippingRegion = @ShippingRegion, Address = @Address, ZipCode = @ZipCode,ShipTo = @ShipTo, TelPhone = @TelPhone, CellPhone = @CellPhone, ShippingModeId=@ShippingModeId ,ModeName=@ModeName, RealShippingModeId = @RealShippingModeId, RealModeName = @RealModeName, ShipOrderNumber = @ShipOrderNumber,  ExpressCompanyName = @ExpressCompanyName,ExpressCompanyAbb = @ExpressCompanyAbb, PaymentTypeId=@PaymentTypeId,PaymentType=@PaymentType, Gateway = @Gateway, ManagerMark=@ManagerMark,ManagerRemark=@ManagerRemark,IsPrinted=@IsPrinted, OrderTotal = @OrderTotal, OrderProfit=@OrderProfit,Amount=@Amount,OrderCostPrice=@OrderCostPrice, AdjustedFreight = @AdjustedFreight, PayCharge = @PayCharge, AdjustedDiscount=@AdjustedDiscount,OrderPoint=@OrderPoint,GatewayOrderId=@GatewayOrderId,OldAddress=@OldAddress WHERE OrderId = @OrderId");
            decimal total = order.GetTotal();
            decimal point = Globals.GetPoint(total);
            this.database.AddInParameter(sqlStringCommand, "OrderStatus", DbType.Int32, (int) order.OrderStatus);
            this.database.AddInParameter(sqlStringCommand, "CloseReason", DbType.String, order.CloseReason);
            this.database.AddInParameter(sqlStringCommand, "PayDate", DbType.DateTime, order.PayDate);
            this.database.AddInParameter(sqlStringCommand, "ShippingDate", DbType.DateTime, order.ShippingDate);
            this.database.AddInParameter(sqlStringCommand, "FinishDate", DbType.DateTime, order.FinishDate);
            this.database.AddInParameter(sqlStringCommand, "RegionId", DbType.String, order.RegionId);
            this.database.AddInParameter(sqlStringCommand, "ShippingRegion", DbType.String, order.ShippingRegion);
            this.database.AddInParameter(sqlStringCommand, "Address", DbType.String, order.Address);
            this.database.AddInParameter(sqlStringCommand, "ZipCode", DbType.String, order.ZipCode);
            this.database.AddInParameter(sqlStringCommand, "ShipTo", DbType.String, order.ShipTo);
            this.database.AddInParameter(sqlStringCommand, "TelPhone", DbType.String, order.TelPhone);
            this.database.AddInParameter(sqlStringCommand, "CellPhone", DbType.String, order.CellPhone);
            this.database.AddInParameter(sqlStringCommand, "ShippingModeId", DbType.Int32, order.ShippingModeId);
            this.database.AddInParameter(sqlStringCommand, "ModeName", DbType.String, order.ModeName);
            this.database.AddInParameter(sqlStringCommand, "RealShippingModeId", DbType.Int32, order.RealShippingModeId);
            this.database.AddInParameter(sqlStringCommand, "RealModeName", DbType.String, order.RealModeName);
            this.database.AddInParameter(sqlStringCommand, "ShipOrderNumber", DbType.String, order.ShipOrderNumber);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyName", DbType.String, order.ExpressCompanyName);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyAbb", DbType.String, order.ExpressCompanyAbb);
            this.database.AddInParameter(sqlStringCommand, "PaymentTypeId", DbType.Int32, order.PaymentTypeId);
            this.database.AddInParameter(sqlStringCommand, "PaymentType", DbType.String, order.PaymentType);
            this.database.AddInParameter(sqlStringCommand, "Gateway", DbType.String, order.Gateway);
            this.database.AddInParameter(sqlStringCommand, "ManagerMark", DbType.Int32, order.ManagerMark);
            this.database.AddInParameter(sqlStringCommand, "ManagerRemark", DbType.String, order.ManagerRemark);
            this.database.AddInParameter(sqlStringCommand, "IsPrinted", DbType.Boolean, order.IsPrinted);
            this.database.AddInParameter(sqlStringCommand, "OrderTotal", DbType.Currency, total);
            this.database.AddInParameter(sqlStringCommand, "OrderProfit", DbType.Currency, order.GetProfit());
            this.database.AddInParameter(sqlStringCommand, "Amount", DbType.Currency, order.GetAmount());
            this.database.AddInParameter(sqlStringCommand, "OrderCostPrice", DbType.Currency, order.GetCostPrice());
            this.database.AddInParameter(sqlStringCommand, "AdjustedFreight", DbType.Currency, order.AdjustedFreight);
            this.database.AddInParameter(sqlStringCommand, "PayCharge", DbType.Currency, order.PayCharge);
            this.database.AddInParameter(sqlStringCommand, "AdjustedDiscount", DbType.Currency, order.AdjustedDiscount);
            this.database.AddInParameter(sqlStringCommand, "OrderPoint", DbType.Int32, point);
            this.database.AddInParameter(sqlStringCommand, "GatewayOrderId", DbType.String, order.GatewayOrderId);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, order.OrderId);
            this.database.AddInParameter(sqlStringCommand, "OldAddress", DbType.String, order.OldAddress);
            if (dbTran != null)
            {
                return (this.database.ExecuteNonQuery(sqlStringCommand, dbTran) > 0);
            }
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool UpdateOrderCompany(string orderId, string companycode, string companyname, string shipNumber)
        {
            string query = "UPDATE Hishop_Orders SET ShipOrderNumber=@ShipOrderNumber,ExpressCompanyAbb=@ExpressCompanyAbb,ExpressCompanyName=@ExpressCompanyName WHERE OrderId =@OrderId";
            if (string.IsNullOrEmpty(shipNumber))
            {
                query = "UPDATE Hishop_Orders SET ExpressCompanyAbb=@ExpressCompanyAbb,ExpressCompanyName=@ExpressCompanyName WHERE OrderId =@OrderId";
            }
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyAbb", DbType.String, companycode);
            this.database.AddInParameter(sqlStringCommand, "ShipOrderNumber", DbType.String, shipNumber);
            this.database.AddInParameter(sqlStringCommand, "ExpressCompanyName", DbType.String, companyname);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public bool UpdateOrderSplitState(string orderid, int splitstate, [Optional, DefaultParameterValue(null)] DbTransaction dbTran)
        {
            string query = "update Hishop_Orders set SplitState=@SplitState where OrderID=@OrderID";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderid);
            this.database.AddInParameter(sqlStringCommand, "SplitState", DbType.Int32, splitstate);
            return (this.database.ExecuteNonQuery(sqlStringCommand, dbTran) > 0);
        }

        public void UpdatePayOrderStock(OrderInfo orderinfo)
        {
            int bargainDetialId = orderinfo.BargainDetialId;
            if (bargainDetialId > 0)
            {
                int quantity = 0;
                foreach (LineItemInfo info in orderinfo.LineItems.Values)
                {
                    quantity = info.Quantity;
                    break;
                }
                if (quantity > 0)
                {
                    string query = "update Hishop_Bargain set TranNumber=TranNumber+@Num where Id=(select BargainId from Hishop_BargainDetial where id=" + bargainDetialId + " AND IsDelete=0)";
                    DbCommand sqlStringCommand = this.database.GetSqlStringCommand(query);
                    this.database.AddInParameter(sqlStringCommand, "Num", DbType.Int32, quantity);
                    this.database.ExecuteNonQuery(sqlStringCommand);
                }
            }
            this.UpdatePayOrderStock(orderinfo.OrderId);
        }

        public void UpdatePayOrderStock(string orderId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("Update Hishop_SKUs Set Stock = CASE WHEN (Stock - (SELECT SUM(oi.ShipmentQuantity) FROM Hishop_OrderItems oi Where oi.SkuId =Hishop_SKUs.SkuId AND OrderId =@OrderId))<=0 Then 0 ELSE Stock - (SELECT SUM(oi.ShipmentQuantity) FROM Hishop_OrderItems oi  Where oi.SkuId =Hishop_SKUs.SkuId AND OrderId =@OrderId) END WHERE Hishop_SKUs.SkuId  IN (Select SkuId FROM Hishop_OrderItems Where OrderId =@OrderId)");
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            this.database.ExecuteNonQuery(sqlStringCommand);
        }

        public bool UpdateRefundOrderStock(string orderId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("Update Hishop_SKUs Set Stock = Stock + (SELECT SUM(oi.ShipmentQuantity) FROM Hishop_OrderItems oi  Where oi.SkuId =Hishop_SKUs.SkuId AND OrderId =@OrderId) WHERE Hishop_SKUs.SkuId  IN (Select SkuId FROM Hishop_OrderItems Where OrderId =@OrderId)");
            this.database.AddInParameter(sqlStringCommand, "OrderId", DbType.String, orderId);
            return (this.database.ExecuteNonQuery(sqlStringCommand) >= 1);
        }
    }
}

