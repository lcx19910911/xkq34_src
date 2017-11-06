namespace Hidistro.SaleSystem.Vshop
{
    using Hidistro.Core;
    using Hidistro.Entities.Bargain;
    using Hidistro.Entities.Commodities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Promotions;
    using Hidistro.Entities.Sales;
    using Hidistro.SqlDal.Bargain;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Promotions;
    using Hidistro.SqlDal.Sales;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class ShoppingCartProcessor
    {
        public static void AddLineItem(string skuId, int quantity, int categoryid, int Templateid, [Optional, DefaultParameterValue(0)] int type, [Optional, DefaultParameterValue(0)] int exchangeId, [Optional, DefaultParameterValue(0)] int limitedTimeDiscountId)
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (quantity <= 0)
            {
                quantity = 1;
            }
            if (limitedTimeDiscountId == 0)
            {
                int productId = 0;
                SKUItem skuItem = new SkuDao().GetSkuItem(skuId);
                if (skuItem != null)
                {
                    productId = skuItem.ProductId;
                    int num2 = Globals.ToNum(new LimitedTimeDiscountDao().GetLimitedTimeDiscountIdByProductId(currentMember.UserId, skuId, productId));
                    if (num2 > 0)
                    {
                        limitedTimeDiscountId = num2;
                    }
                }
            }
            new ShoppingCartDao().AddLineItem(currentMember, skuId, quantity, categoryid, Templateid, type, exchangeId, limitedTimeDiscountId);
        }

        public static void ClearShoppingCart()
        {
            new ShoppingCartDao().ClearShoppingCart(Globals.GetCurrentMemberUserId());
        }

        public static int GetLimitedTimeDiscountUsedNum(int limitedTimeDiscountId, string skuId, int productId, int userid, bool isContainsShippingCart)
        {
            return new ShoppingCartDao().GetLimitedTimeDiscountUsedNum(limitedTimeDiscountId, skuId, productId, userid, isContainsShippingCart);
        }

        public static List<ShoppingCartInfo> GetListShoppingCart(string productSkuId, int buyAmount, [Optional, DefaultParameterValue(0)] int bargainDetialId, [Optional, DefaultParameterValue(0)] int limitedTimeDiscountId)
        {
            List<ShoppingCartInfo> list = new List<ShoppingCartInfo>();
            ShoppingCartInfo item = new ShoppingCartInfo();
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            ShoppingCartItemInfo info3 = null;
            if (bargainDetialId > 0)
            {
                info3 = new ShoppingCartDao().GetCartItemInfo(currentMember, productSkuId, buyAmount, 0, bargainDetialId, 0);
                if (info3 == null)
                {
                    return null;
                }
                BargainDetialInfo bargainDetialInfo = new BargainDao().GetBargainDetialInfo(bargainDetialId);
                if (bargainDetialInfo == null)
                {
                    return null;
                }
                item.TemplateId = info3.FreightTemplateId.ToString();
                item.Amount = bargainDetialInfo.Number * bargainDetialInfo.Price;
                item.Total = item.Amount;
                item.Exemption = 0M;
                item.ShipCost = 0M;
                item.GetPointNumber = info3.PointNumber * info3.Quantity;
                item.MemberPointNumber = currentMember.Points;
                item.LineItems.Add(info3);
                list.Add(item);
                return list;
            }
            info3 = new ShoppingCartDao().GetCartItemInfo(currentMember, productSkuId, buyAmount, 0, 0, limitedTimeDiscountId);
            if (info3 == null)
            {
                return null;
            }
            item.TemplateId = info3.FreightTemplateId.ToString();
            item.Amount = info3.SubTotal;
            item.Total = item.Amount;
            item.Exemption = 0M;
            item.ShipCost = 0M;
            item.GetPointNumber = info3.PointNumber * info3.Quantity;
            item.MemberPointNumber = currentMember.Points;
            item.LineItems.Add(info3);
            list.Add(item);
            return list;
        }

        public static List<ShoppingCartInfo> GetOrderSummitCart()
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (currentMember == null)
            {
                return null;
            }
            List<ShoppingCartInfo> orderSummitCart = new ShoppingCartDao().GetOrderSummitCart(currentMember);
            if (orderSummitCart.Count == 0)
            {
                return null;
            }
            return orderSummitCart;
        }

        public static ShoppingCartInfo GetShoppingCart()
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (currentMember == null)
            {
                return null;
            }
            ShoppingCartInfo shoppingCart = new ShoppingCartDao().GetShoppingCart(currentMember);
            if (shoppingCart.LineItems.Count == 0)
            {
                return null;
            }
            return shoppingCart;
        }

        public static ShoppingCartInfo GetShoppingCart(int Templateid)
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (currentMember == null)
            {
                return null;
            }
            ShoppingCartInfo shoppingCart = new ShoppingCartDao().GetShoppingCart(currentMember, Templateid);
            if (shoppingCart.LineItems.Count == 0)
            {
                return null;
            }
            return shoppingCart;
        }

        public static ShoppingCartInfo GetShoppingCart(string productSkuId, int buyAmount)
        {
            ShoppingCartInfo info = new ShoppingCartInfo();
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            ShoppingCartItemInfo item = new ShoppingCartDao().GetCartItemInfo(currentMember, productSkuId, buyAmount, 0, 0, 0);
            if (item == null)
            {
                return null;
            }
            info.LineItems.Add(item);
            return info;
        }

        public static List<ShoppingCartInfo> GetShoppingCartAviti([Optional, DefaultParameterValue(0)] int type)
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (currentMember == null)
            {
                return null;
            }
            List<ShoppingCartInfo> shoppingCartAviti = new ShoppingCartDao().GetShoppingCartAviti(currentMember, type);
            if (shoppingCartAviti.Count == 0)
            {
                return null;
            }
            return shoppingCartAviti;
        }

        public static int GetSkuStock(string skuId, [Optional, DefaultParameterValue(0)] int type, [Optional, DefaultParameterValue(0)] int exchangeId)
        {
            int stock = new SkuDao().GetSkuItem(skuId).Stock;
            if (type > 0)
            {
                int productId = int.Parse(skuId.Split(new char[] { '_' })[0]);
                PointExchangeProductInfo productInfo = new PointExChangeDao().GetProductInfo(exchangeId, productId);
                if (productInfo == null)
                {
                    return stock;
                }
                MemberInfo currentMember = MemberProcessor.GetCurrentMember();
                int eachMaxNumber = 0;
                int num4 = new PointExChangeDao().GetUserProductExchangedCount(exchangeId, productId, currentMember.UserId);
                int productExchangedCount = new PointExChangeDao().GetProductExchangedCount(exchangeId, productId);
                int num6 = ((productInfo.ProductNumber - productExchangedCount) >= 0) ? (productInfo.ProductNumber - productExchangedCount) : 0;
                if (productInfo.EachMaxNumber > 0)
                {
                    if (num4 < productInfo.EachMaxNumber)
                    {
                        if (productInfo.EachMaxNumber <= num6)
                        {
                            eachMaxNumber = productInfo.EachMaxNumber;
                        }
                        else
                        {
                            eachMaxNumber = num6;
                        }
                    }
                    else
                    {
                        eachMaxNumber = 0;
                    }
                }
                else
                {
                    eachMaxNumber = num6;
                }
                if (eachMaxNumber > 0)
                {
                    stock = eachMaxNumber;
                }
            }
            return stock;
        }

        public static void RemoveLineItem(string skuId, int type, int limitedTimeDiscountId)
        {
            new ShoppingCartDao().RemoveLineItem(Globals.GetCurrentMemberUserId(), skuId, type, limitedTimeDiscountId);
        }

        public static void UpdateLineItemQuantity(string skuId, int quantity, int type, int limitedTimeDiscountId)
        {
            MemberInfo currentMember = MemberProcessor.GetCurrentMember();
            if (quantity <= 0)
            {
                RemoveLineItem(skuId, type, limitedTimeDiscountId);
            }
            new ShoppingCartDao().UpdateLineItemQuantity(currentMember, skuId, quantity, type, limitedTimeDiscountId);
        }
    }
}

