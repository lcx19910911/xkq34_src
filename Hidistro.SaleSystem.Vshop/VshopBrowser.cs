namespace Hidistro.SaleSystem.Vshop
{
    using Hidistro.Entities.Promotions;
    using Hidistro.Entities.VShop;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Promotions;
    using Hidistro.SqlDal.VShop;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Web;

    public static class VshopBrowser
    {
        public static Hidistro.Entities.VShop.ActivityInfo GetActivity(int activityId)
        {
            return new Hidistro.SqlDal.VShop.ActivityDao().GetActivity(activityId);
        }

        public static IList<BannerInfo> GetAllBanners()
        {
            return new BannerDao().GetAllBanners();
        }

        public static IList<NavigateInfo> GetAllNavigate()
        {
            return new BannerDao().GetAllNavigate();
        }

        public static DataTable GetHomeProducts()
        {
            return new HomeProductDao().GetHomeProducts();
        }

        public static string GetLimitedTimeDiscountName(int limitedTimeDiscountId)
        {
            string activityName = string.Empty;
            LimitedTimeDiscountInfo discountInfo = new LimitedTimeDiscountDao().GetDiscountInfo(limitedTimeDiscountId);
            if (discountInfo != null)
            {
                activityName = discountInfo.ActivityName;
            }
            return activityName;
        }

        public static string GetLimitedTimeDiscountNameStr(int limitedTimeDiscountId)
        {
            string limitedTimeDiscountName = GetLimitedTimeDiscountName(limitedTimeDiscountId);
            if (!string.IsNullOrEmpty(limitedTimeDiscountName))
            {
                limitedTimeDiscountName = "<span style='background-color: rgb(246, 187, 66); border-color: rgb(246, 187, 66); color: rgb(255, 255, 255);'>" + HttpContext.Current.Server.HtmlEncode(limitedTimeDiscountName) + "</span>";
            }
            return limitedTimeDiscountName;
        }

        public static MessageInfo GetMessage(int messageId)
        {
            return new ReplyDao().GetMessage(messageId);
        }

        public static DataTable GetVote(int voteId, out string voteName, out int checkNum, out int voteNum)
        {
            return new VoteDao().LoadVote(voteId, out voteName, out checkNum, out voteNum);
        }

        public static bool IsVote(int voteId)
        {
            return new VoteDao().IsVote(voteId);
        }

        public static bool Vote(int voteId, string itemIds)
        {
            return new VoteDao().Vote(voteId, itemIds);
        }
    }
}

