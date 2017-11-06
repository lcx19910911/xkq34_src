namespace Hidistro.SaleSystem.Vshop
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using Hidistro.Core.Enums;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Promotions;
    using Hidistro.Entities.StatisticsReport;
    using Hidistro.Entities.Store;
    using Hidistro.Entities.VShop;
    using Hidistro.Messages;
    using Hidistro.SqlDal.Members;
    using Hidistro.SqlDal.Orders;
    using Hidistro.SqlDal.Promotions;
    using Hidistro.SqlDal.VShop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web.Caching;

    public class DistributorsBrower
    {
        public static bool AddBalanceDrawRequest(BalanceDrawRequestInfo balancerequestinfo, MemberInfo memberinfo)
        {
            DistributorsInfo currentDistributors = GetCurrentDistributors(false);
            if ((((memberinfo != null) && !string.IsNullOrEmpty(memberinfo.RealName)) && ((currentDistributors != null) && (currentDistributors.UserId > 0))) && !string.IsNullOrEmpty(memberinfo.CellPhone))
            {
                if (!(string.IsNullOrEmpty(balancerequestinfo.MerchantCode) || !(currentDistributors.RequestAccount != balancerequestinfo.MerchantCode)))
                {
                    new DistributorsDao().UpdateDistributorById(balancerequestinfo.MerchantCode, memberinfo.UserId);
                }
                balancerequestinfo.UserId = memberinfo.UserId;
                balancerequestinfo.UserName = memberinfo.UserName;
                if ((balancerequestinfo.RequesType == 0) || (balancerequestinfo.RequesType == 3))
                {
                    balancerequestinfo.MerchantCode = memberinfo.OpenId;
                }
                else if (balancerequestinfo.MerchantCode.Length < 1)
                {
                    balancerequestinfo.MerchantCode = currentDistributors.RequestAccount;
                }
                balancerequestinfo.CellPhone = memberinfo.CellPhone;
                return new DistributorsDao().AddBalanceDrawRequest(balancerequestinfo);
            }
            return false;
        }

        public static void AddDistributorProductId(List<int> productList)
        {
            int currentMemberUserId = Globals.GetCurrentMemberUserId();
            if ((currentMemberUserId > 0) && (productList.Count > 0))
            {
                new DistributorsDao().RemoveDistributorProducts(productList, currentMemberUserId);
                foreach (int num2 in productList)
                {
                    new DistributorsDao().AddDistributorProducts(num2, currentMemberUserId);
                }
            }
        }

        public static bool AddDistributors(DistributorsInfo distributors)
        {
            DistributorsDao dao = new DistributorsDao();
            if (dao.GetCustomDistributorStatistic(distributors.StoreName).Rows.Count > 0)
            {
                return false;
            }
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            distributors.DistributorGradeId = DistributorGrade.OneDistributor;
            distributors.ParentUserId = new int?(currentMember.UserId);
            distributors.UserId = currentMember.UserId;
            DistributorsInfo currentDistributors = GetCurrentDistributors(true);
            if (currentDistributors != null)
            {
                if (!(string.IsNullOrEmpty(currentDistributors.ReferralPath) || currentDistributors.ReferralPath.Contains("|")))
                {
                    distributors.ReferralPath = currentDistributors.ReferralPath + "|" + currentDistributors.UserId.ToString();
                }
                else if (!(string.IsNullOrEmpty(currentDistributors.ReferralPath) || !currentDistributors.ReferralPath.Contains("|")))
                {
                    distributors.ReferralPath = currentDistributors.ReferralPath.Split(new char[] { '|' })[1] + "|" + currentDistributors.UserId.ToString();
                }
                else
                {
                    distributors.ReferralPath = currentDistributors.UserId.ToString();
                }
                distributors.ParentUserId = new int?(currentDistributors.UserId);
                if (distributors.Logo == "")
                {
                    if (!string.IsNullOrEmpty(currentDistributors.Logo))
                    {
                        distributors.Logo = currentDistributors.Logo;
                    }
                    else
                    {
                        SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                        distributors.Logo = masterSettings.DistributorLogoPic;
                    }
                }
                if (currentDistributors.DistributorGradeId == DistributorGrade.OneDistributor)
                {
                    distributors.DistributorGradeId = DistributorGrade.TowDistributor;
                }
                else if (currentDistributors.DistributorGradeId == DistributorGrade.TowDistributor)
                {
                    distributors.DistributorGradeId = DistributorGrade.ThreeDistributor;
                }
                else
                {
                    distributors.DistributorGradeId = DistributorGrade.ThreeDistributor;
                }
            }
            return new DistributorsDao().CreateDistributor(distributors);
        }

        public static void DeleteDistributorProductIds(List<int> productList)
        {
            int userId = GetCurrentDistributors(true).UserId;
            if ((userId > 0) && (productList.Count > 0))
            {
                new DistributorsDao().RemoveDistributorProducts(productList, userId);
            }
        }

        public static void DistributorGradeChange(DistributorsInfo distributor, string orderid, int newDistributorGradeid)
        {
            DistributorGradeInfo distributorGradeInfo = DistributorGradeBrower.GetDistributorGradeInfo(newDistributorGradeid);
            if ((distributorGradeInfo != null) && (distributorGradeInfo.AddCommission > 0M))
            {
                try
                {
                    Messenger.SendWeiXinMsg_DistributorGradeChange(MemberProcessor.GetMember(distributor.UserId, true), distributorGradeInfo.Name);
                }
                catch
                {
                }
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                if (masterSettings.IsAddCommission == 1)
                {
                    try
                    {
                        DateTime time = DateTime.Parse(masterSettings.AddCommissionStartTime);
                        DateTime time2 = DateTime.Parse(masterSettings.AddCommissionEndTime).AddDays(1.0);
                        if ((DateTime.Now > time) && (DateTime.Now < time2))
                        {
                            decimal num = distributor.ReferralRequestBalance + distributor.ReferralBlance;
                            DistributorGradeCommissionInfo info = new DistributorGradeCommissionInfo {
                                UserId = distributor.UserId,
                                Commission = distributorGradeInfo.AddCommission,
                                PubTime = DateTime.Now,
                                OperAdmin = "system",
                                Memo = "升级奖励",
                                OrderID = orderid,
                                OldCommissionTotal = num
                            };
                            if (!string.IsNullOrEmpty(info.OrderID))
                            {
                                info.ReferralUserId = new OrderDao().GetOrderReferralUserId(info.OrderID);
                            }
                            else
                            {
                                info.OrderID = "U" + GenerateOrderId();
                            }
                            info.CommType = 3;
                            if (info.ReferralUserId == 0)
                            {
                                info.ReferralUserId = info.UserId;
                                info.CommType = 4;
                            }
                            DistributorGradeCommissionBrower.AddCommission(info);
                            NoticeInfo info4 = new NoticeInfo {
                                Title = "恭喜分销商获得升级奖励佣金￥" + distributorGradeInfo.AddCommission.ToString("F2")
                            };
                            StringBuilder builder = new StringBuilder();
                            builder.Append("<p class='textlist'>恭喜<span style='color:#3D9BDF;'>" + distributor.StoreName + "</span>自动升级为<span style='color:red;'>" + distributorGradeInfo.Name + "</span>分销商" + ((num > 0M) ? ("(累计获得佣金" + num.ToString("F2") + "元)") : "") + "，系统额外奖励佣金" + distributorGradeInfo.AddCommission.ToString("F2") + "元！</p>");
                            builder.Append("<p class='textlist'>自" + time.ToString("yyyy年MM月dd日") + "至" + time2.ToString("yyyy年MM月dd日") + "，分销商等级提升将获得系统奖励的额外佣金。</p>");
                            builder.Append("<table class='table table-bordered' style='text-align: center;'><thead><tr class='firstRow'><th style='text-align:center;'>等级名称</th><th style='text-align:center;'>需要佣金</th><th style='text-align:center;'>奖励佣金</th></tr></thead><tbody>");
                            DataTable allDistributorGrade = DistributorGradeBrower.GetAllDistributorGrade();
                            int count = allDistributorGrade.Rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                builder.Append("<tr><td>" + allDistributorGrade.Rows[i]["Name"].ToString() + "</td><td>￥" + decimal.Parse(allDistributorGrade.Rows[i]["CommissionsLimit"].ToString()).ToString("F2") + "</td><td>￥" + decimal.Parse(allDistributorGrade.Rows[i]["AddCommission"].ToString()).ToString("F2") + "</td></tr>");
                            }
                            builder.Append("</tbody></table>");
                            info4.Memo = builder.ToString();
                            info4.Author = "system";
                            info4.AddTime = DateTime.Now;
                            info4.IsPub = 1;
                            info4.PubTime = new DateTime?(DateTime.Now);
                            info4.SendType = 0;
                            info4.SendTo = 0;
                            NoticeBrowser.SaveNotice(info4);
                        }
                    }
                    catch (Exception exception)
                    {
                        Globals.Debuglog("升级奖励异常" + exception.Message, "_Debuglog.txt");
                    }
                }
            }
        }

        public static int EditCommisionsGrade(string userids, string Grade)
        {
            return new DistributorsDao().EditCommisionsGrade(userids, Grade);
        }

        public static bool EditDisbutosInfos(string userid, string QQNum, string CellPhone, string RealName, string Password)
        {
            return new DistributorsDao().EditDisbutosInfos(userid, QQNum, CellPhone, RealName, Password);
        }

        public static bool FrozenCommision(int userid, string ReferralStatus)
        {
            try
            {
                MemberInfo member = new MemberDao().GetMember(userid);
                if (member != null)
                {
                    if (ReferralStatus == "1")
                    {
                        Messenger.SendWeiXinMsg_AccountLockOrUnLock(member, true);
                    }
                    else if (ReferralStatus == "0")
                    {
                        Messenger.SendWeiXinMsg_AccountLockOrUnLock(member, false);
                    }
                    else if (ReferralStatus == "9")
                    {
                        Messenger.SendWeiXinMsg_DistributorCancel(member);
                    }
                }
            }
            catch (Exception)
            {
            }
            bool flag = new DistributorsDao().FrozenCommision(userid, ReferralStatus);
            RemoveDistributorCache(userid);
            return flag;
        }

        public static int FrozenCommisionChecks(string userids, string ReferralStatus)
        {
            int num = new DistributorsDao().FrozenCommisionChecks(userids, ReferralStatus);
            string[] strArray = userids.Trim(new char[] { ',' }).Split(new char[] { ',' });
            int userId = 0;
            foreach (string str in strArray)
            {
                userId = Globals.ToNum(str);
                if (userId > 0)
                {
                    MemberInfo member = new MemberDao().GetMember(userId);
                    if (member != null)
                    {
                        if (ReferralStatus == "1")
                        {
                            Messenger.SendWeiXinMsg_AccountLockOrUnLock(member, true);
                        }
                        else if (ReferralStatus == "0")
                        {
                            Messenger.SendWeiXinMsg_AccountLockOrUnLock(member, false);
                        }
                        else if (ReferralStatus == "9")
                        {
                            Messenger.SendWeiXinMsg_DistributorCancel(member);
                        }
                        RemoveDistributorCache(userId);
                    }
                }
            }
            return num;
        }

        public static string GenerateOrderId()
        {
            string str = string.Empty;
            Random random = new Random();
            for (int i = 0; i < 7; i++)
            {
                int num = random.Next();
                str = str + ((char) (0x30 + ((ushort) (num % 10)))).ToString();
            }
            return (DateTime.Now.ToString("yyyyMMdd") + str);
        }

        public static DbQueryResult GetBalanceDrawRequest(BalanceDrawRequestQuery query, [Optional] string[] extendChecks)
        {
            return new DistributorsDao().GetBalanceDrawRequest(query, extendChecks);
        }

        public static BalanceDrawRequestInfo GetBalanceDrawRequestById(string serialids)
        {
            return new DistributorsDao().GetBalanceDrawRequestById(serialids);
        }

        public static bool GetBalanceDrawRequestIsCheck(int serialid)
        {
            return new DistributorsDao().GetBalanceDrawRequestIsCheck(serialid);
        }

        public static int GetBalanceDrawRequestIsCheckStatus(int serialid)
        {
            return new DistributorsDao().GetBalanceDrawRequestIsCheckStatus(serialid);
        }

        public static string GetBalanceDrawRequestStatus(int status)
        {
            switch (status)
            {
                case -1:
                    return "已驳回";

                case 0:
                    return "待审核";

                case 1:
                    return "已审核";

                case 2:
                    return "已发放";

                case 3:
                    return "付款异常";
            }
            return "未知";
        }

        public static DbQueryResult GetCommissions(CommissionsQuery query)
        {
            return new DistributorsDao().GetCommissions(query);
        }

        public static DistributorsInfo GetCurrentDistributors([Optional, DefaultParameterValue(true)] bool readCache)
        {
            return GetCurrentDistributors(Globals.GetCurrentDistributorId(), readCache);
        }

        public static DistributorsInfo GetCurrentDistributors(int userId, [Optional, DefaultParameterValue(true)] bool readCache)
        {
            DistributorsInfo distributorInfo = null;
            if (readCache)
            {
                distributorInfo = HiCache.Get(string.Format("DataCache-Distributor-{0}", userId)) as DistributorsInfo;
            }
            if ((distributorInfo == null) || (distributorInfo.UserId == 0))
            {
                distributorInfo = new DistributorsDao().GetDistributorInfo(userId);
                HiCache.Insert(string.Format("DataCache-Distributor-{0}", userId), distributorInfo, 360, CacheItemPriority.Normal);
            }
            return distributorInfo;
        }

        public static DataTable GetCurrentDistributorsCommosion()
        {
            return new DistributorsDao().GetDistributorsCommosion(Globals.GetCurrentDistributorId());
        }

        public static DataTable GetCurrentDistributorsCommosion(int userId)
        {
            return new DistributorsDao().GetCurrentDistributorsCommosion(userId);
        }

               public static int GetDistributorGrades(string ReferralUserId)
        {
            DistributorsInfo userIdDistributors = GetUserIdDistributors(int.Parse(ReferralUserId));
            List<DistributorGradeInfo> distributorGrades = new DistributorsDao().GetDistributorGrades() as List<DistributorGradeInfo>;
            foreach (DistributorGradeInfo info2 in from item in distributorGrades
                orderby item.CommissionsLimit descending
                select item)
            {
                if (userIdDistributors.DistriGradeId == info2.GradeId)
                {
                    return 0;
                }
                if (info2.CommissionsLimit <= (userIdDistributors.ReferralBlance + userIdDistributors.ReferralRequestBalance))
                {
                    userIdDistributors.DistriGradeId = info2.GradeId;
                    return info2.GradeId;
                }
            }
            return 0;
        }
        //public static int GetDistributorGrades(string ReferralUserId)
        //{
        //    DistributorsInfo userIdDistributors = GetUserIdDistributors(int.Parse(ReferralUserId));
        //    List<DistributorGradeInfo> distributorGrades = new DistributorsDao().GetDistributorGrades() as List<DistributorGradeInfo>;
        //    if (CS$<>9__CachedAnonymousMethodDelegate1 == null)
        //    {
        //        CS$<>9__CachedAnonymousMethodDelegate1 = new Func<DistributorGradeInfo, decimal>(null, (IntPtr) <GetDistributorGrades>b__0);
        //    }
        //    foreach (DistributorGradeInfo info2 in Enumerable.OrderByDescending<DistributorGradeInfo, decimal>(distributorGrades, CS$<>9__CachedAnonymousMethodDelegate1))
        //    {
        //        if (userIdDistributors.DistriGradeId == info2.GradeId)
        //        {
        //            return 0;
        //        }
        //        if (info2.CommissionsLimit <= (userIdDistributors.ReferralBlance + userIdDistributors.ReferralRequestBalance))
        //        {
        //            userIdDistributors.DistriGradeId = info2.GradeId;
        //            return info2.GradeId;
        //        }
        //    }
        //    return 0;
        //}

        public static DistributorsInfo GetDistributorInfo(int distributorid)
        {
            return new DistributorsDao().GetDistributorInfo(distributorid);
        }

        public static int GetDistributorNum(DistributorGrade grade)
        {
            return new DistributorsDao().GetDistributorNum(grade);
        }

        public static DataSet GetDistributorOrder(OrderQuery query)
        {
            return new OrderDao().GetDistributorOrder(query);
        }

        public static DataSet GetDistributorOrderByDetials(OrderQuery query)
        {
            return new OrderDao().GetDistributorOrderByDetials(query);
        }

        public static DbQueryResult GetDistributorOrderByStatus(OrderQuery query, int userId)
        {
            return new OrderDao().GetDistributorOrderByStatus(query, userId);
        }

        public static int GetDistributorOrderCount(OrderQuery query)
        {
            return new OrderDao().GetDistributorOrderCount(query);
        }

        public static DbQueryResult GetDistributors(DistributorsQuery query)
        {
            return new DistributorsDao().GetDistributors(query, null, null);
        }

        public static DataTable GetDistributorsCommission(DistributorsQuery query)
        {
            return new DistributorsDao().GetDistributorsCommission(query);
        }

        public static DataTable GetDistributorsCommosion(int userId, DistributorGrade distributorgrade)
        {
            return new DistributorsDao().GetDistributorsCommosion(userId, distributorgrade);
        }

        public static int GetDistributorSuperiorId(int userid)
        {
            return new DistributorsDao().GetDistributorSuperiorId(userid);
        }

        public static int GetDownDistributorNum(string userid)
        {
            return new DistributorsDao().GetDownDistributorNum(userid);
        }

        public static DataTable GetDownDistributors(DistributorsQuery query, out int total)
        {
            return new DistributorsDao().GetDownDistributors(query, out total);
        }

        public static DataTable GetDrawRequestNum(int[] CheckValues)
        {
            return new DistributorsDao().GetDrawRequestNum(CheckValues);
        }

        public static Dictionary<int, int> GetMulBalanceDrawRequestIsCheckStatus(int[] serialids)
        {
            return new DistributorsDao().GetMulBalanceDrawRequestIsCheckStatus(serialids);
        }

        public static int GetNotDescDistributorGrades(string ReferralUserId)
        {
            DistributorsInfo userIdDistributors = GetUserIdDistributors(int.Parse(ReferralUserId));
            decimal num2 = userIdDistributors.ReferralBlance + userIdDistributors.ReferralRequestBalance;
            DistributorGradeInfo distributorGradeInfo = DistributorGradeBrower.GetDistributorGradeInfo(userIdDistributors.DistriGradeId);
            if ((distributorGradeInfo != null) && (num2 < distributorGradeInfo.CommissionsLimit))
            {
                return userIdDistributors.DistriGradeId;
            }
            List<DistributorGradeInfo> distributorGrades = new DistributorsDao().GetDistributorGrades() as List<DistributorGradeInfo>;
            foreach (DistributorGradeInfo info3 in from item in distributorGrades
                                                   orderby item.CommissionsLimit descending
                                                   select item)
            {
                if (userIdDistributors.DistriGradeId == info3.GradeId)
                {
                    return userIdDistributors.DistriGradeId;
                }
                if (info3.CommissionsLimit <= num2)
                {
                    return info3.GradeId;
                }
            }
            return 0;
        }

        //public static int GetNotDescDistributorGrades(string ReferralUserId)
        //{
        //    DistributorsInfo userIdDistributors = GetUserIdDistributors(int.Parse(ReferralUserId));
        //    decimal num2 = userIdDistributors.ReferralBlance + userIdDistributors.ReferralRequestBalance;
        //    DistributorGradeInfo distributorGradeInfo = DistributorGradeBrower.GetDistributorGradeInfo(userIdDistributors.DistriGradeId);
        //    if ((distributorGradeInfo != null) && (num2 < distributorGradeInfo.CommissionsLimit))
        //    {
        //        return userIdDistributors.DistriGradeId;
        //    }
        //    List<DistributorGradeInfo> distributorGrades = new DistributorsDao().GetDistributorGrades() as List<DistributorGradeInfo>;
        //    if (CS$<>9__CachedAnonymousMethodDelegate3 == null)
        //    {
        //        CS$<>9__CachedAnonymousMethodDelegate3 = new Func<DistributorGradeInfo, decimal>(null, (IntPtr) <GetNotDescDistributorGrades>b__2);
        //    }
        //    foreach (DistributorGradeInfo info3 in Enumerable.OrderByDescending<DistributorGradeInfo, decimal>(distributorGrades, CS$<>9__CachedAnonymousMethodDelegate3))
        //    {
        //        if (userIdDistributors.DistriGradeId == info3.GradeId)
        //        {
        //            return userIdDistributors.DistriGradeId;
        //        }
        //        if (info3.CommissionsLimit <= num2)
        //        {
        //            return info3.GradeId;
        //        }
        //    }
        //    return 0;
        //}

        public static DataTable GetNotSendRedpackRecord(int balancedrawrequestid)
        {
            return new SendRedpackRecordDao().GetNotSendRedpackRecord(balancedrawrequestid);
        }

        public static DistributorsInfo GetNowCurrentDistributors(int userId)
        {
            return new DistributorsDao().GetDistributorInfo(userId);
        }

        public static int GetRedPackTotalAmount(int balancedrawrequestid, int userid)
        {
            return new SendRedpackRecordDao().GetRedPackTotalAmount(balancedrawrequestid, userid);
        }

        public static SendRedpackRecordInfo GetSendRedpackRecordByID([Optional, DefaultParameterValue(null)] string id, [Optional, DefaultParameterValue(null)] string sid)
        {
            return new SendRedpackRecordDao().GetSendRedpackRecordByID(id, sid);
        }

        public static DbQueryResult GetSendRedpackRecordRequest(SendRedpackRecordQuery query)
        {
            return new SendRedpackRecordDao().GetSendRedpackRecordRequest(query);
        }

        public static DataTable GetThreeDistributors(DistributorsQuery query, out int total)
        {
            return new DistributorsDao().GetThreeDistributors(query, out total);
        }

        public static decimal GetUserCommissions(int userid, DateTime fromdatetime, [Optional, DefaultParameterValue(null)] string endtime, [Optional, DefaultParameterValue(null)] string storeName, [Optional, DefaultParameterValue(null)] string OrderNum, [Optional, DefaultParameterValue("")] string level)
        {
            return new DistributorsDao().GetUserCommissions(userid, fromdatetime, endtime, storeName, OrderNum, level);
        }

        public static DistributorsInfo GetUserIdDistributors(int userid)
        {
            return new DistributorsDao().GetDistributorInfo(userid);
        }

        public static DataSet GetUserRanking(int userid)
        {
            return new DistributorsDao().GetUserRanking(userid);
        }

        public static bool HasDrawRequest(int serialid)
        {
            return new SendRedpackRecordDao().HasDrawRequest(serialid);
        }

        public static string IsCanUpdateDistributorSuperior(int userid, int tosuperuserid)
        {
            string str = "1";
            if (userid == tosuperuserid)
            {
                return "不能将自己设置为自己的上级";
            }
            int num = tosuperuserid;
            int distributorSuperiorId = 0;
            while (true)
            {
                distributorSuperiorId = GetDistributorSuperiorId(num);
                if ((distributorSuperiorId == num) || (distributorSuperiorId == 0))
                {
                    break;
                }
                num = distributorSuperiorId;
                if (num == userid)
                {
                    str = "0";
                    break;
                }
            }
            if (str != "1")
            {
                str = "不能将同一主线上的下级分销商设置为自己的上级";
            }
            return str;
        }

        private static int IsExiteDistributorsByStoreName(string stroname)
        {
            return new DistributorsDao().IsExiteDistributorsByStoreName(stroname);
        }

        public static bool IsExitsCommionsRequest()
        {
            return new DistributorsDao().IsExitsCommionsRequest(Globals.GetCurrentDistributorId());
        }

        public static DataTable OrderIDGetCommosion(string orderid)
        {
            return new DistributorsDao().OrderIDGetCommosion(orderid);
        }

        public static void RemoveDistributorCache(int userId)
        {
            HiCache.Remove(string.Format("DataCache-Distributor-{0}", userId));
        }

        public static DataTable SelectDistributors(DistributorsQuery query)
        {
            return new DistributorsDao().SelectDistributors(query, null, null);
        }

        public static string SendRedPackToBalanceDrawRequest(int serialid)
        {
            return new DistributorsDao().SendRedPackToBalanceDrawRequest(serialid);
        }

        public static bool SetBalanceDrawRequestIsCheckStatus(int[] serialids, int checkValue, [Optional, DefaultParameterValue(null)] string Remark, [Optional, DefaultParameterValue(null)] string Amount)
        {
            bool flag = new DistributorsDao().SetBalanceDrawRequestIsCheckStatus(serialids, checkValue, Remark, Amount);
            if (flag && (checkValue == -1))
            {
                try
                {
                    foreach (int num in serialids)
                    {
                        BalanceDrawRequestInfo balanceDrawRequestById = GetBalanceDrawRequestById(num.ToString());
                        if (balanceDrawRequestById != null)
                        {
                            Messenger.SendWeiXinMsg_DrawCashReject(balanceDrawRequestById);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return flag;
        }

        public static bool setCommission(OrderInfo order, DistributorsInfo DisInfo)
        {
            bool flag = false;
            decimal num = 0M;
            decimal num2 = 0M;
            decimal resultCommTatal = 0M;
            string userId = order.ReferralUserId.ToString();
            string orderId = order.OrderId;
            decimal num4 = 0M;
            ArrayList gradeIdList = new ArrayList();
            ArrayList referralUserIdList = new ArrayList();
            foreach (LineItemInfo info in order.LineItems.Values)
            {
                if (info.OrderItemsStatus.ToString() == OrderStatus.SellerAlreadySent.ToString())
                {
                    num2 += info.ItemsCommission;
                    if ((!string.IsNullOrEmpty(info.ItemAdjustedCommssion.ToString()) && (info.ItemAdjustedCommssion > 0M)) && !info.IsAdminModify)
                    {
                        num += info.ItemAdjustedCommssion;
                    }
                    num4 += (info.GetSubTotal() - info.DiscountAverage) - info.ItemAdjustedCommssion;
                }
            }
            num4 -= order.AdjustedFreight;
            if (false)
            {
                resultCommTatal = num2;
            }
            else
            {
                resultCommTatal = num2 - num;
                if (resultCommTatal < 0M)
                {
                    resultCommTatal = 0M;
                }
            }
            flag = new DistributorsDao().UpdateCalculationCommission(userId, userId, orderId, num4 + order.AdjustedFreight, resultCommTatal);
            try
            {
                if ((order != null) && (resultCommTatal > 0M))
                {
                    string userOpenIdByUserId = MemberProcessor.GetUserOpenIdByUserId(DisInfo.UserId);
                    string aliUserOpenIdByUserId = MemberProcessor.GetAliUserOpenIdByUserId(DisInfo.UserId);
                    Messenger.SendWeiXinMsg_OrderGetCommission(order, userOpenIdByUserId, aliUserOpenIdByUserId, resultCommTatal);
                }
            }
            catch (Exception exception)
            {
                Globals.Debuglog("分佣问题：" + exception.Message, "_Debuglog.txt");
            }
            int notDescDistributorGrades = GetNotDescDistributorGrades(userId);
            if (notDescDistributorGrades > 0)
            {
                gradeIdList.Add(notDescDistributorGrades);
                referralUserIdList.Add(userId);
                flag = new DistributorsDao().UpdateGradeId(gradeIdList, referralUserIdList);
                if (DisInfo.DistriGradeId != notDescDistributorGrades)
                {
                    DistributorGradeChange(DisInfo, order.OrderId, notDescDistributorGrades);
                }
            }
            return flag;
        }

        public static bool SetRedpackRecordIsUsed(int id, bool issend)
        {
            return new SendRedpackRecordDao().SetRedpackRecordIsUsed(id, issend);
        }

        public static bool UpdateCalculationCommission(OrderInfo order)
        {
            Exception exception;
            new MemberDao().SetOrderDate(order.UserId, 2);
            DistributorsInfo userIdDistributors = GetUserIdDistributors(order.ReferralUserId);
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            bool flag = false;
            if (userIdDistributors != null)
            {
                flag = setCommission(order, userIdDistributors);
                if (!string.IsNullOrEmpty(order.ReferralPath))
                {
                    string userOpenIdByUserId;
                    string aliUserOpenIdByUserId;
                    int notDescDistributorGrades;
                    ArrayList commTatalList = new ArrayList();
                    decimal num = 0M;
                    ArrayList userIdList = new ArrayList();
                    string referralUserId = order.ReferralUserId.ToString();
                    string orderId = order.OrderId;
                    ArrayList orderTotalList = new ArrayList();
                    decimal num2 = 0M;
                    ArrayList gradeIdList = new ArrayList();
                    string[] strArray = order.ReferralPath.Split(new char[] { '|' });
                    if (strArray.Length == 1)
                    {
                        DistributorsInfo distributor = GetUserIdDistributors(int.Parse(strArray[0]));
                        if (distributor != null)
                        {
                            foreach (LineItemInfo info3 in order.LineItems.Values)
                            {
                                if (info3.OrderItemsStatus.ToString() == OrderStatus.SellerAlreadySent.ToString())
                                {
                                    num += info3.SecondItemsCommission;
                                    num2 += info3.GetSubTotal();
                                }
                            }
                            commTatalList.Add(num);
                            orderTotalList.Add(num2 + order.AdjustedFreight);
                            userIdList.Add(distributor.UserId);
                            try
                            {
                                if ((order != null) && (num > 0M))
                                {
                                    userOpenIdByUserId = MemberProcessor.GetUserOpenIdByUserId(distributor.UserId);
                                    aliUserOpenIdByUserId = MemberProcessor.GetAliUserOpenIdByUserId(distributor.UserId);
                                    Messenger.SendWeiXinMsg_OrderGetCommission(order, userOpenIdByUserId, aliUserOpenIdByUserId, num);
                                }
                            }
                            catch (Exception exception1)
                            {
                                exception = exception1;
                            }
                            notDescDistributorGrades = GetNotDescDistributorGrades(distributor.UserId.ToString());
                            if (distributor.DistriGradeId != notDescDistributorGrades)
                            {
                                DistributorGradeChange(distributor, order.OrderId, notDescDistributorGrades);
                            }
                        }
                    }
                    if (strArray.Length == 2)
                    {
                        DistributorsInfo info4 = GetUserIdDistributors(int.Parse(strArray[0]));
                        foreach (LineItemInfo info3 in order.LineItems.Values)
                        {
                            if (info3.OrderItemsStatus.ToString() == OrderStatus.SellerAlreadySent.ToString())
                            {
                                num += info3.ThirdItemsCommission;
                                num2 += info3.GetSubTotal();
                            }
                        }
                        commTatalList.Add(num);
                        orderTotalList.Add(num2 + order.AdjustedFreight);
                        userIdList.Add(info4.UserId);
                        try
                        {
                            if ((order != null) && (num > 0M))
                            {
                                userOpenIdByUserId = MemberProcessor.GetUserOpenIdByUserId(info4.UserId);
                                aliUserOpenIdByUserId = MemberProcessor.GetAliUserOpenIdByUserId(info4.UserId);
                                Messenger.SendWeiXinMsg_OrderGetCommission(order, userOpenIdByUserId, aliUserOpenIdByUserId, num);
                            }
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                        }
                        notDescDistributorGrades = GetNotDescDistributorGrades(info4.UserId.ToString());
                        if (info4.DistriGradeId != notDescDistributorGrades)
                        {
                            DistributorGradeChange(info4, order.OrderId, notDescDistributorGrades);
                        }
                        DistributorsInfo info5 = GetUserIdDistributors(int.Parse(strArray[1]));
                        num = 0M;
                        num2 = 0M;
                        foreach (LineItemInfo info3 in order.LineItems.Values)
                        {
                            if (info3.OrderItemsStatus.ToString() == OrderStatus.SellerAlreadySent.ToString())
                            {
                                num += info3.SecondItemsCommission;
                                num2 += info3.GetSubTotal();
                            }
                        }
                        commTatalList.Add(num);
                        orderTotalList.Add(num2 + order.AdjustedFreight);
                        userIdList.Add(info5.UserId);
                        try
                        {
                            if ((order != null) && (num > 0M))
                            {
                                userOpenIdByUserId = MemberProcessor.GetUserOpenIdByUserId(info5.UserId);
                                aliUserOpenIdByUserId = MemberProcessor.GetAliUserOpenIdByUserId(info5.UserId);
                                Messenger.SendWeiXinMsg_OrderGetCommission(order, userOpenIdByUserId, aliUserOpenIdByUserId, num);
                            }
                        }
                        catch (Exception exception3)
                        {
                            exception = exception3;
                        }
                        int newDistributorGradeid = GetNotDescDistributorGrades(info5.UserId.ToString());
                        if (info5.DistriGradeId != newDistributorGradeid)
                        {
                            DistributorGradeChange(info5, order.OrderId, newDistributorGradeid);
                        }
                    }
                    flag = new DistributorsDao().UpdateTwoCalculationCommission(userIdList, referralUserId, orderId, orderTotalList, commTatalList);
                    for (int i = 0; i < userIdList.Count; i++)
                    {
                        int num6 = GetNotDescDistributorGrades(userIdList[i].ToString());
                        gradeIdList.Add(num6);
                    }
                    flag = new DistributorsDao().UpdateGradeId(gradeIdList, userIdList);
                }
                RemoveDistributorCache(userIdDistributors.UserId);
            }
            OrderRedPagerBrower.CreateOrderRedPager(order.OrderId, order.GetTotal(), order.UserId);
            int id = Globals.IsNumeric(order.ActivitiesId) ? Globals.ToNum(order.ActivitiesId) : 0;
            if (id > 0)
            {
                ActivityDetailInfo activityDetailInfo = new Hidistro.SqlDal.VShop.ActivityDao().GetActivityDetailInfo(id);
                if (activityDetailInfo != null)
                {
                    OrderInfo info8;
                    int couponId = activityDetailInfo.CouponId;
                    int integral = activityDetailInfo.Integral;
                    if ((couponId > 0) && (ShoppingProcessor.GetCoupon(couponId.ToString()) != null))
                    {
                        SendCouponResult result = new CouponDao().SendCouponToMember(couponId, order.UserId);
                        try
                        {
                            info8 = order;
                            if (info8 != null)
                            {
                                Messenger.SendWeiXinMsg_OrderGetCoupon(info8);
                            }
                        }
                        catch (Exception exception4)
                        {
                            exception = exception4;
                        }
                    }
                    if (integral > 0)
                    {
                        new OrderDao().AddMemberPointNumber(integral, order, null);
                        try
                        {
                            info8 = order;
                            if (info8 != null)
                            {
                                Messenger.SendWeiXinMsg_OrderGetPoint(info8, integral);
                            }
                        }
                        catch (Exception exception5)
                        {
                            exception = exception5;
                        }
                    }
                }
            }
            MemberProcessor.UpdateUserAccount(order);
            try
            {
                string retInfo = "";
                DateTime orderDate = order.OrderDate;
                DateTime? payDate = order.PayDate;
                if (order.Gateway == "hishop.plugins.payment.podrequest")
                {
                    payDate = new DateTime?(orderDate);
                }
                if (payDate.HasValue && (payDate.Value.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd")))
                {
                    bool flag2 = new ShopStatisticDao().StatisticsOrdersByRecDate(payDate.Value, UpdateAction.AllUpdate, 0, out retInfo);
                }
            }
            catch
            {
            }
            return flag;
        }

        public static bool UpdateDistributor(DistributorsInfo query)
        {
            int num = IsExiteDistributorsByStoreName(query.StoreName);
            if ((num != 0) && (num != query.UserId))
            {
                return false;
            }
            return new DistributorsDao().UpdateDistributor(query);
        }

        public static bool UpdateDistributorMessage(DistributorsInfo query)
        {
            int num = IsExiteDistributorsByStoreName(query.StoreName);
            if ((num != 0) && (num != query.UserId))
            {
                return false;
            }
            return new DistributorsDao().UpdateDistributorMessage(query);
        }

        public static string UpdateDistributorSuperior(int userid, int tosuperuserid)
        {
            string str = IsCanUpdateDistributorSuperior(userid, tosuperuserid);
            if (str == "1")
            {
                str = new DistributorsDao().UpdateDistributorSuperior(userid, tosuperuserid);
            }
            return str;
        }

        public bool UpdateGradeId(ArrayList GradeIdList, ArrayList ReferralUserIdList)
        {
            return new DistributorsDao().UpdateGradeId(GradeIdList, ReferralUserIdList);
        }

        public static bool UpdateStoreCard(int userId, string imgUrl)
        {
            return new DistributorsDao().UpdateStoreCard(userId, imgUrl);
        }
    }
}

