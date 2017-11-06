namespace Hidistro.UI.Web.Admin.promotion
{
    using Hidistro.ControlPanel.Members;
    using Hidistro.ControlPanel.Promotions;
    using Hidistro.Core;
    using Hidistro.Core.Enums;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Promotions;
    using Hidistro.SaleSystem.Vshop;
    using System;
    using System.Web;

    public class LimitedTimeDiscountHandler : IHttpHandler
    {
        private void ChangeDiscountProductStatus(HttpContext context)
        {
            string ids = Globals.RequestFormStr("id");
            int status = Globals.RequestFormNum("status");
            if (LimitedTimeDiscountHelper.ChangeDiscountProductStatus(ids, status))
            {
                context.Response.Write("{\"msg\":\"success\"}");
            }
            else
            {
                context.Response.Write("{\"msg\":\"fial\"}");
            }
        }

        private void ChangeStatus(HttpContext context)
        {
            int id = Globals.RequestFormNum("id");
            int num2 = Globals.RequestFormNum("status");
            if (LimitedTimeDiscountHelper.UpdateDiscountStatus(id, (num2 == 3) ? DiscountStatus.Normal : DiscountStatus.Stop))
            {
                context.Response.Write("{\"msg\":\"success\"}");
            }
            else
            {
                context.Response.Write("{\"msg\":\"fial\"}");
            }
        }

        private void DeleteDiscountProduct(HttpContext context)
        {
            string str = Globals.RequestFormStr("limitedTimeDiscountProductIds");
            if (!string.IsNullOrEmpty(str))
            {
                if (LimitedTimeDiscountHelper.DeleteDiscountProduct(str))
                {
                    context.Response.Write("{\"msg\":\"success\"}");
                }
                else
                {
                    context.Response.Write("{\"msg\":\"fail\"}");
                }
            }
        }

        private void IsDiscountProduct(HttpContext context)
        {
            int productId = Globals.RequestFormNum("productId");
            if (productId > 0)
            {
                LimitedTimeDiscountProductInfo discountProductInfoByProductId = LimitedTimeDiscountHelper.GetDiscountProductInfoByProductId(productId);
                if (discountProductInfoByProductId != null)
                {
                    LimitedTimeDiscountInfo discountInfo = LimitedTimeDiscountHelper.GetDiscountInfo(discountProductInfoByProductId.LimitedTimeDiscountId);
                    MemberInfo currentMember = MemberProcessor.GetCurrentMember();
                    int limitedTimeDiscountId = 0;
                    if (discountInfo != null)
                    {
                        if (currentMember != null)
                        {
                            if (MemberHelper.CheckCurrentMemberIsInRange(discountInfo.ApplyMembers, discountInfo.DefualtGroup, discountInfo.CustomGroup, currentMember.UserId))
                            {
                                int num3 = ShoppingCartProcessor.GetLimitedTimeDiscountUsedNum(limitedTimeDiscountId, null, productId, currentMember.UserId, false);
                                if ((discountInfo.LimitNumber - num3) > 0)
                                {
                                    limitedTimeDiscountId = discountInfo.LimitedTimeDiscountId;
                                }
                            }
                        }
                        else
                        {
                            limitedTimeDiscountId = discountInfo.LimitedTimeDiscountId;
                        }
                    }
                    if (discountInfo != null)
                    {
                        context.Response.Write(string.Concat(new object[] { "{\"msg\":\"success\",\"ActivityName\":\"", discountInfo.ActivityName, "\",\"FinalPrice\":\"", discountProductInfoByProductId.FinalPrice.ToString("f2"), "\",\"LimitedTimeDiscountId\":\"", limitedTimeDiscountId, "\",\"LimitNumber\":\"", discountInfo.LimitNumber, "\"}" }));
                    }
                }
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            switch (Globals.RequestFormStr("action").ToLower())
            {
                case "changestatus":
                    this.ChangeStatus(context);
                    return;

                case "changediscountproductstatus":
                    this.ChangeDiscountProductStatus(context);
                    return;

                case "savediscountproduct":
                    this.SaveDiscountProduct(context);
                    return;

                case "isdiscountproduct":
                    this.IsDiscountProduct(context);
                    return;

                case "savediscountproductinfo":
                    this.SaveDiscountProductInfo(context);
                    return;

                case "updatediscountproductlist":
                    this.UpdateDiscountProductList(context);
                    return;

                case "deletediscountproduct":
                    this.DeleteDiscountProduct(context);
                    return;
            }
        }

        private void SaveDiscountProduct(HttpContext context)
        {
            Globals.RequestFormNum("id");
            foreach (string str2 in Globals.RequestFormStr("discountProductList").Trim(new char[] { ',' }).Split(new char[] { ',' }))
            {
                string[] strArray2 = str2.Split(new char[] { '^' });
                LimitedTimeDiscountProductInfo info = new LimitedTimeDiscountProductInfo();
                if (((!string.IsNullOrEmpty(strArray2[0]) && !string.IsNullOrEmpty(strArray2[1])) && !string.IsNullOrEmpty(strArray2[4])) && (!string.IsNullOrEmpty(strArray2[2]) || !string.IsNullOrEmpty(strArray2[3])))
                {
                    int id = Globals.ToNum(strArray2[0]);
                    LimitedTimeDiscountInfo discountInfo = LimitedTimeDiscountHelper.GetDiscountInfo(id);
                    info.LimitedTimeDiscountId = id;
                    info.ProductId = Globals.ToNum(strArray2[1]);
                    info.Discount = (string.IsNullOrEmpty(strArray2[2]) || (strArray2[2] == "undefined")) ? 0M : decimal.Parse(strArray2[2]);
                    info.Minus = (string.IsNullOrEmpty(strArray2[3]) || (strArray2[2] == "undefined")) ? 0M : decimal.Parse(strArray2[3]);
                    info.FinalPrice = decimal.Parse(strArray2[4]);
                    if (discountInfo != null)
                    {
                        info.BeginTime = discountInfo.BeginTime;
                        info.EndTime = discountInfo.EndTime;
                    }
                    info.CreateTime = DateTime.Now;
                    info.Status = 1;
                    LimitedTimeDiscountHelper.AddLimitedTimeDiscountProduct(info);
                }
            }
            context.Response.Write("{\"msg\":\"success\"}");
        }

        private void SaveDiscountProductInfo(HttpContext context)
        {
            decimal num3;
            decimal num4;
            decimal num5;
            int num = Globals.RequestFormNum("ProductId");
            int num2 = Globals.RequestFormNum("LimitedTimeDiscountId");
            decimal.TryParse(Globals.RequestFormStr("Discount"), out num3);
            decimal.TryParse(Globals.RequestFormStr("Minus"), out num4);
            decimal.TryParse(Globals.RequestFormStr("FinalPrice"), out num5);
            LimitedTimeDiscountProductInfo info = new LimitedTimeDiscountProductInfo {
                ProductId = num,
                LimitedTimeDiscountId = num2,
                Discount = num3,
                Minus = num4,
                FinalPrice = num5
            };
            if (LimitedTimeDiscountHelper.UpdateLimitedTimeDiscountProduct(info))
            {
                context.Response.Write("{\"msg\":\"success\"}");
            }
            else
            {
                context.Response.Write("{\"msg\":\"fial\"}");
            }
        }

        private void UpdateDiscountProductList(HttpContext context)
        {
            int id = Globals.RequestFormNum("LimitedTimeDiscountId");
            foreach (string str2 in Globals.RequestFormStr("discountProductList").Trim(new char[] { ',' }).Split(new char[] { ',' }))
            {
                string[] strArray2 = str2.Split(new char[] { '^' });
                for (int i = 0; i < strArray2.Length; i++)
                {
                    if (((!string.IsNullOrEmpty(strArray2[0]) && !string.IsNullOrEmpty(strArray2[1])) && !string.IsNullOrEmpty(strArray2[4])) && (!string.IsNullOrEmpty(strArray2[2]) || !string.IsNullOrEmpty(strArray2[3])))
                    {
                        LimitedTimeDiscountProductInfo info = new LimitedTimeDiscountProductInfo();
                        int num3 = Globals.ToNum(strArray2[0]);
                        LimitedTimeDiscountHelper.GetDiscountInfo(id);
                        info.LimitedTimeDiscountProductId = num3;
                        info.Discount = (string.IsNullOrEmpty(strArray2[2]) || (strArray2[2] == "undefined")) ? 0M : decimal.Parse(strArray2[2]);
                        info.Minus = (string.IsNullOrEmpty(strArray2[3]) || (strArray2[2] == "undefined")) ? 0M : decimal.Parse(strArray2[3]);
                        info.FinalPrice = decimal.Parse(strArray2[4]);
                        LimitedTimeDiscountHelper.UpdateLimitedTimeDiscountProductById(info);
                    }
                }
            }
            context.Response.Write("{\"msg\":\"success\"}");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

