namespace Hidistro.SaleSystem.Vshop
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Entities.Commodities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Sales;
    using Hidistro.Messages;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Members;
    using Hidistro.SqlDal.Orders;
    using Hidistro.SqlDal.Promotions;
    using Hidistro.SqlDal.Sales;
    using Hidistro.SqlDal.VShop;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Web.Caching;

    public static class MemberProcessor
    {
        public static bool AddFuwuFollowUser(string openid)
        {
            return new MemberDao().addFuwuFollowUser(openid);
        }

        public static bool AddIntegralDetail(IntegralDetailInfo point)
        {
            return new IntegralDetailDao().AddIntegralDetail(point, null);
        }

        public static int AddShippingAddress(ShippingAddressInfo shippingAddress)
        {
            ShippingAddressDao dao = new ShippingAddressDao();
            MemberDao dao2 = new MemberDao();
            int shippingId = dao.AddShippingAddress(shippingAddress);
            if (dao.SetDefaultShippingAddress(shippingId, Globals.GetCurrentMemberUserId()))
            {
                dao2.SaveMemberInfoByAddress(shippingAddress);
                return 1;
            }
            return 0;
        }

        public static bool BindUserName(int UserId, string UserBindName, string Password)
        {
            MemberDao dao = new MemberDao();
            return dao.BindUserName(UserId, UserBindName, Password);
        }

        public static bool CancelOrder(OrderInfo order)
        {
            bool flag = false;
            if (order.CheckAction(OrderActions.SELLER_CLOSE))
            {
                order.OrderStatus = OrderStatus.Closed;
                flag = new OrderDao().UpdateOrder(order, null);
                new OrderDao().UpdateItemsStatus(order.OrderId, 4, "all");
                Point.SetPointByOrderId(order);
            }
            return flag;
        }

        public static bool CheckCurrentMemberIsInRange(string Grades, string DefualtGroup, string CustomGroup)
        {
            return new MemberDao().CheckCurrentMemberIsInRange(Grades, DefualtGroup, CustomGroup, 0);
        }

        public static bool CheckMemberIsBuyProds(int userId, string prodIds, DateTime? startTime, DateTime? endTime)
        {
            return new MemberDao().CheckMemberIsBuyProds(userId, prodIds, startTime, endTime);
        }

        public static bool ConfirmOrderFinish(OrderInfo order)
        {
            bool flag = false;
            if (order.CheckAction(OrderActions.BUYER_CONFIRM_GOODS))
            {
                DateTime now = DateTime.Now;
                order.OrderStatus = OrderStatus.Finished;
                order.FinishDate = new DateTime?(now);
                if (!order.PayDate.HasValue)
                {
                    order.PayDate = new DateTime?(now);
                    new MemberDao().SetOrderDate(order.UserId, 1);
                }
                flag = new OrderDao().UpdateOrder(order, null);
                HiCache.Remove(string.Format("DataCache-Member-{0}", order.UserId));
            }
            return flag;
        }

        public static bool CreateMember(MemberInfo member)
        {
            bool flag = new MemberDao().CreateMember(member);
            if (flag)
            {
                try
                {
                    Messenger.SendWeiXinMsg_MemberRegister(member);
                }
                catch (Exception)
                {
                }
                CouponProcessor.RegisterSendCoupon(member.SessionId);
            }
            return flag;
        }

        public static bool Delete(int userId)
        {
            bool flag = new MemberDao().Delete(userId);
            if (flag)
            {
                HiCache.Remove(string.Format("DataCache-Member-{0}", userId));
            }
            return flag;
        }

        public static bool DelFuwuFollowUser(string openid)
        {
            return new MemberDao().DelFuwuFollowUser(openid);
        }

        public static bool DelShippingAddress(int shippingid, int userid)
        {
            return new ShippingAddressDao().DelShippingAddress(shippingid, userid);
        }

        public static bool DelUserMessage(MemberInfo newuser, int olduserid)
        {
            return new MemberDao().DelUserMessage(newuser, olduserid);
        }

        public static bool DelUserMessage(int userid, string openid, string userhead, int olduserid)
        {
            return new MemberDao().DelUserMessage(userid, openid, userhead, olduserid);
        }

        public static string GetAliUserOpenIdByUserId(int UserId)
        {
            MemberDao dao = new MemberDao();
            return dao.GetAliOpenIDByUserId(UserId);
        }

        public static MemberInfo GetBindusernameMember(string UserBindName)
        {
            MemberDao dao = new MemberDao();
            return dao.GetBindusernameMember(UserBindName);
        }

        public static DataTable GetCouponByProducts(int couponId, int ProductId)
        {
            return new CouponDao().GetCouponByProducts(couponId, ProductId);
        }

        public static MemberInfo GetCurrentMember()
        {
            return new MemberDao().GetMember(Globals.GetCurrentMemberUserId());
        }

        public static int GetDefaultMemberGrade()
        {
            return new MemberGradeDao().GetDefaultMemberGrade();
        }

        public static ShippingAddressInfo GetDefaultShippingAddress()
        {
            IList<ShippingAddressInfo> shippingAddresses = new ShippingAddressDao().GetShippingAddresses(Globals.GetCurrentMemberUserId());
            foreach (ShippingAddressInfo info in shippingAddresses)
            {
                if (info.IsDefault)
                {
                    return info;
                }
            }
            return null;
        }

        public static int GetDistributorNumOfTotal(int topUserId, out int topNum)
        {
            return new MemberDao().GetDistributorNumOfTotal(topUserId, out topNum);
        }

        public static decimal GetIntegral(int userId)
        {
            return new PointDetailDao().GetIntegral(userId);
        }

        public static DbQueryResult GetIntegralDetail(IntegralDetailQuery query)
        {
            return new IntegralDetailDao().GetIntegralDetail(query);
        }

        public static MemberInfo GetMember()
        {
            return GetMember(Globals.GetCurrentMemberUserId(), true);
        }

        public static MemberInfo GetMember(string sessionId)
        {
            return new MemberDao().GetMember(sessionId);
        }

        public static MemberInfo GetMember(int userId, [Optional, DefaultParameterValue(true)] bool isUserCache)
        {
            MemberInfo member = null;
            if (isUserCache)
            {
                member = HiCache.Get(string.Format("DataCache-Member-{0}", userId)) as MemberInfo;
                if (member == null)
                {
                    member = new MemberDao().GetMember(userId);
                    HiCache.Insert(string.Format("DataCache-Member-{0}", userId), member, 360, CacheItemPriority.Normal);
                }
                return member;
            }
            return new MemberDao().GetMember(userId);
        }

        public static MemberGradeInfo GetMemberGrade(int gradeId)
        {
            return new MemberGradeDao().GetMemberGrade(gradeId);
        }

        public static IList<MemberGradeInfo> GetMemberGrades(string grids)
        {
            return new MemberGradeDao().GetMemberGrades(grids);
        }

        public static List<MemberInfo> GetMemberInfoList(string userIds)
        {
            return new MemberDao().GetMemberInfoList(userIds);
        }

        public static int GetMemberNumOfTotal(int topUserId, out int topNum)
        {
            return new MemberDao().GetMemberNumOfTotal(topUserId, out topNum);
        }

        public static DataTable GetMembersByUserId(int referralUserId, int pageIndex, int pageSize, out int total)
        {
            return new MemberDao().GetMembersByUserId(referralUserId, pageIndex, pageSize, out total);
        }

        public static MemberInfo GetOpenIdMember(string OpenId, [Optional, DefaultParameterValue("wx")] string From)
        {
            MemberDao dao = new MemberDao();
            return dao.GetOpenIdMember(OpenId, From);
        }

        public static int GetPoint(decimal money)
        {
            if (money == 0M)
            {
                return 0;
            }
            int num = 0;
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            if (!masterSettings.shopping_score_Enable)
            {
                return 0;
            }
            if (masterSettings.PointsRate != 0M)
            {
                num = (int) Math.Round((decimal) (money / masterSettings.PointsRate), 0);
                if (num > 0x7fffffff)
                {
                    return 0x7fffffff;
                }
            }
            if (masterSettings.shopping_reward_Enable && (money >= ((decimal) masterSettings.shopping_reward_OrderValue)))
            {
                num += masterSettings.shopping_reward_Score;
                if (num > 0x7fffffff)
                {
                    num = 0x7fffffff;
                }
            }
            return num;
        }

        public static ShippingAddressInfo GetShippingAddress(int shippingId)
        {
            return new ShippingAddressDao().GetShippingAddress(shippingId, Globals.GetCurrentMemberUserId());
        }

        public static int GetShippingAddressCount()
        {
            return new ShippingAddressDao().GetShippingAddresses(Globals.GetCurrentMemberUserId()).Count;
        }

        public static IList<ShippingAddressInfo> GetShippingAddresses()
        {
            return new ShippingAddressDao().GetShippingAddresses(Globals.GetCurrentMemberUserId());
        }

        public static DataTable GetUserCoupons()
        {
            return new CouponDao().GetUserCoupons();
        }

        public static DataTable GetUserCoupons(int userId, [Optional, DefaultParameterValue(0)] int useType)
        {
            return new CouponDao().GetUserCoupons(userId, useType);
        }

        public static int GetUserFollowStateByUserId(int UserId, [Optional, DefaultParameterValue("wx")] string type)
        {
            return new MemberDao().GetUserFollowStateByUserId(UserId, type);
        }

        public static OrderInfo GetUserLastOrder(int userId)
        {
            return new OrderDao().GetUserLastOrder(userId);
        }

        public static MemberInfo GetusernameMember(string username)
        {
            return new MemberDao().GetusernameMember(username);
        }

        public static string GetUserOpenIdByUserId(int UserId)
        {
            MemberDao dao = new MemberDao();
            return dao.GetOpenIDByUserId(UserId);
        }

        public static DataSet GetUserOrder(int userId, OrderQuery query)
        {
            return new OrderDao().GetUserOrder(userId, query);
        }

        public static DbQueryResult GetUserOrderByPage(int userId, OrderQuery query)
        {
            return new OrderDao().GetUserOrderByPage(userId, query);
        }

        public static int GetUserOrderCount(int userId, OrderQuery query)
        {
            return new OrderDao().GetUserOrderCount(userId, query);
        }

        public static DataSet GetUserOrderReturn(int userId, OrderQuery query)
        {
            return new OrderDao().GetUserOrderReturn(userId, query);
        }

        public static int GetUserOrderReturnCount(int userId)
        {
            return new OrderDao().GetUserOrderReturnCount(userId);
        }

        public static bool IsExitOpenId(string Opneid)
        {
            return new MemberDao().IsExitOpenId(Opneid);
        }

        public static bool IsFuwuFollowUser(string openid)
        {
            return new MemberDao().IsFuwuFollowUser(openid);
        }

        public static bool ReSetUserHead(string userid, string wxName, string wxHead, [Optional, DefaultParameterValue("")] string Openid)
        {
            return new MemberDao().ReSetUserHead(userid, wxName, wxHead, Openid);
        }

        public static bool SetAlipayInfos(MemberInfo user)
        {
            return (new MemberDao().SetAlipayInfos(user) > 0);
        }

        public static bool SetDefaultShippingAddress(int shippingId, int UserId)
        {
            return new ShippingAddressDao().SetDefaultShippingAddress(shippingId, UserId);
        }

        public static bool SetMemberSessionId(MemberInfo member)
        {
            MemberDao dao = new MemberDao();
            return dao.SetMemberSessionId(member.SessionId, member.SessionEndTime, member.OpenId);
        }

        public static bool SetMemberSessionId(string sessionId, DateTime sessionEndTime, string openId)
        {
            return new MemberDao().SetMemberSessionId(sessionId, sessionEndTime, openId);
        }

        public static int SetMultiplePwd(string userids, string pwd)
        {
            return new MemberDao().SetMultiplePwd(userids, pwd);
        }

        public static bool SetPwd(string userid, string pwd)
        {
            return new MemberDao().SetPwd(userid, pwd);
        }

        public static bool UpdateMember(MemberInfo member)
        {
            HiCache.Remove(string.Format("DataCache-Member-{0}", member.UserId));
            return new MemberDao().Update(member);
        }

        public static bool UpdateShippingAddress(ShippingAddressInfo shippingAddress)
        {
            return new ShippingAddressDao().UpdateShippingAddress(shippingAddress);
        }

        public static void UpdateUserAccount(OrderInfo order)
        {
            Exception exception;
            Func<MemberGradeInfo, bool> predicate = null;
            Func<MemberGradeInfo, bool> func2 = null;
            MemberDao dao = new MemberDao();
            decimal money = order.GetTotal() - order.Freight;
            int point = GetPoint(money);
            if (point > 0)
            {
                IntegralDetailInfo info = new IntegralDetailInfo {
                    IntegralChange = point,
                    IntegralSource = "购物送积分",
                    IntegralSourceType = 1,
                    IntegralStatus = 1,
                    Userid = order.UserId,
                    Remark = "订单号：" + order.OrderId
                };
                new IntegralDetailDao().AddIntegralDetail(info, null);
                try
                {
                    if (order != null)
                    {
                        Messenger.SendWeiXinMsg_OrderGetPoint(order, point);
                    }
                }
                catch (Exception exception1)
                {
                    exception = exception1;
                }
            }
            MemberInfo member = new MemberDao().GetMember(order.UserId);
            member.Expenditure += order.GetTotal();
            member.OrderNumber++;
            dao.Update(member);
            MemberGradeInfo memberGrade = GetMemberGrade(member.GradeId);
            if (memberGrade != null)
            {
                bool flag = false;
                if (memberGrade.TranVol.HasValue)
                {
                    flag = memberGrade.TranVol.Value < double.Parse(member.Expenditure.ToString());
                }
                bool flag2 = false;
                if (memberGrade.TranTimes.HasValue)
                {
                    int? tranTimes = memberGrade.TranTimes;
                    int orderNumber = member.OrderNumber;
                    flag2 = (tranTimes.GetValueOrDefault() < orderNumber) && tranTimes.HasValue;
                }
                if (flag || flag2)
                {
                    List<MemberGradeInfo> memberGrades = new MemberGradeDao().GetMemberGrades("") as List<MemberGradeInfo>;
                    MemberGradeInfo info3 = null;
                    if (flag)
                    {
                        if (predicate == null)
                        {
                            predicate = m => ((decimal) m.TranVol.Value) <= member.Expenditure;
                        }
                        info3 = (from m in memberGrades
                            where m.TranVol.HasValue
                            orderby m.TranVol descending
                            select m).FirstOrDefault<MemberGradeInfo>(predicate);
                    }
                    MemberGradeInfo info4 = null;
                    if (flag2)
                    {
                        if (func2 == null)
                        {
                            func2 = m => m.TranTimes.Value <= member.OrderNumber;
                        }
                        info4 = (from m in memberGrades
                            where m.TranTimes.HasValue
                            orderby m.TranTimes descending
                            select m).FirstOrDefault<MemberGradeInfo>(func2);
                    }
                    else
                    {
                        info4 = info3;
                    }
                    MemberGradeInfo info5 = null;
                    if (info3 == null)
                    {
                        info3 = info4;
                    }
                    if (info3 != null)
                    {
                        double? tranVol = info3.TranVol;
                        double? nullable3 = info4.TranVol;
                        if ((tranVol.GetValueOrDefault() > nullable3.GetValueOrDefault()) && (tranVol.HasValue & nullable3.HasValue))
                        {
                            info5 = info3;
                        }
                        else
                        {
                            info5 = info4;
                        }
                        if ((memberGrade.GradeId != info5.GradeId) && (((tranVol = memberGrade.TranVol).GetValueOrDefault() <= (nullable3 = info5.TranVol).GetValueOrDefault()) || !(tranVol.HasValue & nullable3.HasValue)))
                        {
                            member.GradeId = info5.GradeId;
                            dao.Update(member);
                            try
                            {
                                if (member != null)
                                {
                                    Messenger.SendWeiXinMsg_MemberGradeChange(member);
                                }
                            }
                            catch (Exception exception2)
                            {
                                exception = exception2;
                            }
                        }
                    }
                }
            }
        }
        //public static void UpdateUserAccount(OrderInfo order)
        //{
        //    Exception exception;
        //    Func<MemberGradeInfo, bool> func = null;
        //    Func<MemberGradeInfo, bool> func2 = null;
        //    MemberDao dao = new MemberDao();
        //    decimal money = order.GetTotal() - order.Freight;
        //    int point = GetPoint(money);
        //    if (point > 0)
        //    {
        //        IntegralDetailInfo info = new IntegralDetailInfo {
        //            IntegralChange = point,
        //            IntegralSource = "购物送积分",
        //            IntegralSourceType = 1,
        //            IntegralStatus = 1,
        //            Userid = order.UserId,
        //            Remark = "订单号：" + order.OrderId
        //        };
        //        new IntegralDetailDao().AddIntegralDetail(info, null);
        //        try
        //        {
        //            if (order != null)
        //            {
        //                Messenger.SendWeiXinMsg_OrderGetPoint(order, point);
        //            }
        //        }
        //        catch (Exception exception1)
        //        {
        //            exception = exception1;
        //        }
        //    }
        //    MemberInfo member = new MemberDao().GetMember(order.UserId);
        //    member.Expenditure += order.GetTotal();
        //    member.OrderNumber++;
        //    dao.Update(member);
        //    MemberGradeInfo memberGrade = GetMemberGrade(member.GradeId);
        //    if (memberGrade != null)
        //    {
        //        bool flag = false;
        //        if (memberGrade.TranVol.HasValue)
        //        {
        //            flag = memberGrade.TranVol.Value < double.Parse(member.Expenditure.ToString());
        //        }
        //        bool flag2 = false;
        //        if (memberGrade.TranTimes.HasValue)
        //        {
        //            int? tranTimes = memberGrade.TranTimes;
        //            int orderNumber = member.OrderNumber;
        //            flag2 = (tranTimes.GetValueOrDefault() < orderNumber) && tranTimes.HasValue;
        //        }
        //        if (flag || flag2)
        //        {
        //            <>c__DisplayClassc classc;
        //            List<MemberGradeInfo> memberGrades = new MemberGradeDao().GetMemberGrades("") as List<MemberGradeInfo>;
        //            MemberGradeInfo info3 = null;
        //            if (flag)
        //            {
        //                if (CS$<>9__CachedAnonymousMethodDelegate6 == null)
        //                {
        //                    CS$<>9__CachedAnonymousMethodDelegate6 = new Func<MemberGradeInfo, bool>(null, (IntPtr) <UpdateUserAccount>b__0);
        //                }
        //                if (CS$<>9__CachedAnonymousMethodDelegate7 == null)
        //                {
        //                    CS$<>9__CachedAnonymousMethodDelegate7 = new Func<MemberGradeInfo, double?>(null, (IntPtr) <UpdateUserAccount>b__1);
        //                }
        //                if (func == null)
        //                {
        //                    func = new Func<MemberGradeInfo, bool>(classc, (IntPtr) this.<UpdateUserAccount>b__2);
        //                }
        //                info3 = Enumerable.FirstOrDefault<MemberGradeInfo>(Enumerable.OrderByDescending<MemberGradeInfo, double?>(Enumerable.Where<MemberGradeInfo>(memberGrades, CS$<>9__CachedAnonymousMethodDelegate6), CS$<>9__CachedAnonymousMethodDelegate7), func);
        //            }
        //            MemberGradeInfo info4 = null;
        //            if (flag2)
        //            {
        //                if (CS$<>9__CachedAnonymousMethodDelegate9 == null)
        //                {
        //                    CS$<>9__CachedAnonymousMethodDelegate9 = new Func<MemberGradeInfo, bool>(null, (IntPtr) <UpdateUserAccount>b__3);
        //                }
        //                if (CS$<>9__CachedAnonymousMethodDelegatea == null)
        //                {
        //                    CS$<>9__CachedAnonymousMethodDelegatea = new Func<MemberGradeInfo, int?>(null, (IntPtr) <UpdateUserAccount>b__4);
        //                }
        //                if (func2 == null)
        //                {
        //                    func2 = new Func<MemberGradeInfo, bool>(classc, (IntPtr) this.<UpdateUserAccount>b__5);
        //                }
        //                info4 = Enumerable.FirstOrDefault<MemberGradeInfo>(Enumerable.OrderByDescending<MemberGradeInfo, int?>(Enumerable.Where<MemberGradeInfo>(memberGrades, CS$<>9__CachedAnonymousMethodDelegate9), CS$<>9__CachedAnonymousMethodDelegatea), func2);
        //            }
        //            else
        //            {
        //                info4 = info3;
        //            }
        //            MemberGradeInfo info5 = null;
        //            if (info3 == null)
        //            {
        //                info3 = info4;
        //            }
        //            if (info3 != null)
        //            {
        //                double? tranVol = info3.TranVol;
        //                double? nullable3 = info4.TranVol;
        //                if ((tranVol.GetValueOrDefault() > nullable3.GetValueOrDefault()) && (tranVol.HasValue & nullable3.HasValue))
        //                {
        //                    info5 = info3;
        //                }
        //                else
        //                {
        //                    info5 = info4;
        //                }
        //                if ((memberGrade.GradeId != info5.GradeId) && (((tranVol = memberGrade.TranVol).GetValueOrDefault() <= (nullable3 = info5.TranVol).GetValueOrDefault()) || !(tranVol.HasValue & nullable3.HasValue)))
        //                {
        //                    member.GradeId = info5.GradeId;
        //                    dao.Update(member);
        //                    try
        //                    {
        //                        if (member != null)
        //                        {
        //                            Messenger.SendWeiXinMsg_MemberGradeChange(member);
        //                        }
        //                    }
        //                    catch (Exception exception2)
        //                    {
        //                        exception = exception2;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public static bool UpdateUserFollowStateByUserId(int UserId, int state, [Optional, DefaultParameterValue("wx")] string type)
        {
            return new MemberDao().UpdateUserFollowStateByUserId(UserId, state, type);
        }

        public static bool UserPayOrder(OrderInfo order)
        {
            OrderDao dao = new OrderDao();
            order.OrderStatus = OrderStatus.BuyerAlreadyPaid;
            order.PayDate = new DateTime?(DateTime.Now);
            bool flag = dao.UpdateOrder(order, null);
            string str = "";
            if (flag)
            {
                dao.UpdatePayOrderStock(order);
                new MemberDao().SetOrderDate(order.UserId, 1);
                foreach (LineItemInfo info in order.LineItems.Values)
                {
                    ProductDao dao2 = new ProductDao();
                    str = str + "'" + info.SkuId + "',";
                    ProductInfo productDetails = dao2.GetProductDetails(info.ProductId);
                    productDetails.SaleCounts += info.Quantity;
                    productDetails.ShowSaleCounts += info.Quantity;
                    dao2.UpdateProduct(productDetails, null);
                }
                if (!string.IsNullOrEmpty(str))
                {
                    dao.UpdateItemsStatus(order.OrderId, 2, str.Substring(0, str.Length - 1));
                }
                if (!string.IsNullOrEmpty(order.ActivitiesId))
                {
                    new ActivitiesDao().UpdateActivitiesTakeEffect(order.ActivitiesId);
                }
                if (GetMember(order.UserId, true) != null)
                {
                    Messenger.SendWeiXinMsg_OrderPay(order);
                }
            }
            return flag;
        }
    }
}

