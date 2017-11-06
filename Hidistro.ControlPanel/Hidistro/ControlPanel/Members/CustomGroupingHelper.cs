namespace Hidistro.ControlPanel.Members
{
    using Hidistro.Entities.Members;
    using Hidistro.SqlDal.Members;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;

    public static class CustomGroupingHelper
    {
        public static string AddCustomGrouping(CustomGroupingInfo customGroupingInfo)
        {
            return new CustomGroupingDao().AddCustomGrouping(customGroupingInfo);
        }

        public static string AddCustomGroupingUser(IList<int> UserIdList, int groupId)
        {
            string str = string.Empty;
            int num = 0;
            int num2 = 0;
            CustomGroupingUserDao dao = new CustomGroupingUserDao();
            foreach (int num3 in UserIdList)
            {
                if (dao.GetGroupIdByUserId(num3, groupId) > 0)
                {
                    num++;
                }
                else if (!dao.AddCustomGroupingUser(num3, groupId))
                {
                    num2++;
                }
            }
            if (num2 > 0)
            {
                return string.Concat(new object[] { "成功添加", UserIdList.Count - num2, "条，失败", num2, "条" });
            }
            if (num > 0)
            {
                str = string.Concat(new object[] { "有效添加", UserIdList.Count - num, "条，其余", num, "条已经在该分组中" });
            }
            return str;
        }

        public static bool DelGroup(int groupid)
        {
            return new CustomGroupingDao().DelGroup(groupid);
        }

        public static bool DelGroupUser(string UserId, int groupid)
        {
            return new CustomGroupingUserDao().DelGroupUser(UserId, groupid);
        }

        public static DataTable GetCustomGroupingDataTable()
        {
            return new CustomGroupingDao().GetCustomGroupingTable();
        }

        public static IList<CustomGroupingInfo> GetCustomGroupingList()
        {
            return new CustomGroupingDao().GetCustomGroupingList();
        }

        public static IList<CustomGroupingInfo> GetCustomGroupingList([Optional, DefaultParameterValue("")] string customGroupIds)
        {
            return new CustomGroupingDao().GetCustomGroupingList(customGroupIds);
        }

        public static DataTable GetCustomGroupingUser(int groupId)
        {
            return new CustomGroupingDao().GetCustomGroupingUser(groupId);
        }

        public static CustomGroupingInfo GetGroupInfoById(int groupId)
        {
            return new CustomGroupingDao().GetGroupInfoById(groupId);
        }

        public static string GetMemberGroupList(int userId)
        {
            IList<int> memberGroupList = new CustomGroupingUserDao().GetMemberGroupList(userId);
            if ((memberGroupList != null) && (memberGroupList.Count > 0))
            {
                string str = "";
                foreach (int num in memberGroupList)
                {
                    str = str + num.ToString() + ",";
                }
                return str.Substring(0, str.Length - 1);
            }
            return "-1";
        }

        public static IList<int> GetMemberList(MemberQuery query)
        {
            IList<int> payDateIntervalMemberList;
            IList<int> collection = new List<int>();
            MemberDao dao = new MemberDao();
            if (!string.IsNullOrEmpty(query.StoreName))
            {
                collection = dao.GetStoreNameMemberList(query.StoreName);
            }
            else
            {
                collection = dao.GetAllMemberList();
            }
            IList<int> list2 = new List<int>(collection);
            if ((query.TradeMoneyStart.HasValue || query.TradeMoneyEnd.HasValue) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetTradeMoneyIntervalMemberList(query.TradeMoneyStart, query.TradeMoneyEnd);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if ((query.TradeNumStart.HasValue || query.TradeNumEnd.HasValue) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetTradeNumIntervalMemberList(query.TradeNumStart, query.TradeNumEnd);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if ((!string.IsNullOrEmpty(query.GradeIds) && !query.GradeIds.Equals("0")) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetGradeMemberList(query.GradeIds);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if ((!string.IsNullOrEmpty(query.ClientType) && !query.ClientType.Equals("0")) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetDefualtGroupMemberList(query.ClientType);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if (!string.IsNullOrEmpty(query.GroupIds) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetCustomGroupMemberList(query.GroupIds);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if ((query.RegisterStartTime.HasValue || query.RegisterEndTime.HasValue) && (collection.Count > 0))
            {
                payDateIntervalMemberList = dao.GetCreateDateIntervalMemberList(query.RegisterStartTime, query.RegisterEndTime);
                if ((payDateIntervalMemberList == null) || (payDateIntervalMemberList.Count <= 0))
                {
                    return new List<int>();
                }
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
            }
            if ((!query.StartTime.HasValue && !query.EndTime.HasValue) || (collection.Count <= 0))
            {
                return list2;
            }
            payDateIntervalMemberList = dao.GetPayDateIntervalMemberList(query.StartTime, query.EndTime);
            if ((payDateIntervalMemberList != null) && (payDateIntervalMemberList.Count > 0))
            {
                foreach (int num in collection)
                {
                    if (!payDateIntervalMemberList.Contains(num))
                    {
                        list2.Remove(num);
                    }
                }
                return list2;
            }
            return new List<int>();
        }

        public static void SetUserCustomGroup(int userId, IList<int> GroupIdList)
        {
            CustomGroupingUserDao dao = new CustomGroupingUserDao();
            IList<int> memberGroupList = dao.GetMemberGroupList(userId);
            if ((memberGroupList != null) && (memberGroupList.Count > 0))
            {
                foreach (int num in memberGroupList)
                {
                    dao.DelGroupUser(userId.ToString(), num);
                }
            }
            if ((GroupIdList != null) && (GroupIdList.Count > 0))
            {
                foreach (int num in GroupIdList)
                {
                    dao.AddCustomGroupingUser(userId, num);
                }
            }
        }

        public static string UpdateCustomGrouping(CustomGroupingInfo customGroupingInfo)
        {
            return new CustomGroupingDao().UpdateCustomGrouping(customGroupingInfo);
        }
    }
}

