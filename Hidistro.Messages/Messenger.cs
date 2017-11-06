namespace Hidistro.Messages
{
    using Hidistro.Core;
    using Hidistro.Core.Configuration;
    using Hidistro.Core.Entities;
    using Hidistro.Entities;
    using Hidistro.Entities.Members;
    using Hidistro.Entities.Orders;
    using Hidistro.Entities.Promotions;
    using Hidistro.Entities.VShop;
    using Hidistro.SqlDal.Commodities;
    using Hidistro.SqlDal.Members;
    using Hidistro.SqlDal.Orders;
    using Hishop.AlipayFuwu.Api.Model;
    using Hishop.AlipayFuwu.Api.Util;
    using Hishop.Plugins;
    using Hishop.Weixin.MP.Api;
    using Hishop.Weixin.MP.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public static class Messenger
    {
        internal static EmailSender CreateEmailSender(SiteSettings settings)
        {
            string str;
            return CreateEmailSender(settings, out str);
        }

        internal static EmailSender CreateEmailSender(SiteSettings settings, out string msg)
        {
            try
            {
                msg = "";
                if (!settings.EmailEnabled)
                {
                    return null;
                }
                return EmailSender.CreateInstance(settings.EmailSender, HiCryptographer.Decrypt(settings.EmailSettings));
            }
            catch (Exception exception)
            {
                msg = exception.Message;
                return null;
            }
        }

        private static void FuwuSend(string FuwuTemplateId, SiteSettings settings, AliTemplateMessage templateMessage)
        {
            if ((!string.IsNullOrWhiteSpace(FuwuTemplateId) && (settings.AlipayAppid.Length > 15)) && (templateMessage != null))
            {
                AliOHHelper.TemplateSend(templateMessage);
            }
        }

        private static AliTemplateMessage GenerateFuwuMessage_AccountChangeMsg(string templateId, SiteSettings settings, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string ChangeTypeDesc, [Optional, DefaultParameterValue("")] string RemarkData)
        {
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "帐号更新提醒" : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = string.IsNullOrEmpty(member.UserBindName) ? member.UserName : member.UserBindName
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = "消息类型[" + ChangeTypeDesc + "]，" + RemarkData
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_CouponWillExpiredMsg(string templateId, SiteSettings settings, CouponInfo_MemberWeiXin couponInfo, [Optional, DefaultParameterValue("")] string FirstData)
        {
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "您有一张优惠券即将过期" : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = (couponInfo.ConditionValue > 0M) ? string.Format("订单满{0}元可用", couponInfo.ConditionValue.ToString("F2")) : "不限制"
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = couponInfo.IsAllProduct ? "适用全部商品" : "适用部分商品，优惠券可以订单支付时抵扣等额现金。"
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_DistributorCreateMsg(string templateId, SiteSettings settings, DistributorsInfo distributor, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData)
        {
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "您好，有一位新分销商申请了店铺" : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = member.UserName + "，" + member.CellPhone
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = string.IsNullOrEmpty(member.UserBindName) ? member.UserName : member.UserBindName
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_DrawCashResultMsg(string templateId, SiteSettings settings, BalanceDrawRequestInfo balance, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string IsCheckDesc)
        {
            string weixinToken = settings.WeixinToken;
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "分销商提现" : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = balance.StoreName + "申请金额：￥" + balance.Amount.ToString("F2")
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = "提现帐号[" + balance.MerchantCode + "],当前状态：[" + IsCheckDesc + "]，备注：" + balance.Remark
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_OrderMsg(string templateId, SiteSettings settings, OrderInfo order, [Optional, DefaultParameterValue("")] string FirstData)
        {
            string firstProductName = new OrderDao().GetFirstProductName(order.OrderId);
            string weixinToken = settings.WeixinToken;
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? firstProductName : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = order.OrderId
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = OrderInfo.GetOrderStatusName(order.OrderStatus)
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = "订单总金额￥" + order.GetTotal().ToString("F2")
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_PersonalMsg(string templateId, SiteSettings settings, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string ContentData)
        {
            string weixinToken = settings.WeixinToken;
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "个人消息通知" : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = string.IsNullOrEmpty(ContentData) ? "获得了奖品" : ContentData
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = ""
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_RefundSuccessMsg(string templateId, SiteSettings settings, OrderInfo order, RefundInfo refundInfo, [Optional, DefaultParameterValue("")] string FirstData)
        {
            string productName = new ProductDao().GetProductDetails(refundInfo.ProductId).ProductName;
            string weixinToken = settings.WeixinToken;
            if (string.IsNullOrEmpty(refundInfo.RefundRemark))
            {
                refundInfo.RefundRemark = "";
            }
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[5];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? ("订单号：" + order.OrderId) : FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = refundInfo.RefundMoney.ToString("F2")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = productName
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword3",
                Value = order.OrderId
            };
            partArray[3] = part4;
            AliTemplateMessage.MessagePart part5 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = refundInfo.RefundRemark.Replace("\r", "").Replace("\n", "")
            };
            partArray[4] = part5;
            message2.Data = partArray;
            return message2;
        }

        private static AliTemplateMessage GenerateFuwuMessage_ServiceMsg(string templateId, SiteSettings settings, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string TitleStr, [Optional, DefaultParameterValue("")] string RemarkData)
        {
            AliTemplateMessage message2 = new AliTemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            AliTemplateMessage.MessagePart[] partArray = new AliTemplateMessage.MessagePart[4];
            AliTemplateMessage.MessagePart part = new AliTemplateMessage.MessagePart
            {
                Name = "first",
                Value = FirstData
            };
            partArray[0] = part;
            AliTemplateMessage.MessagePart part2 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[1] = part2;
            AliTemplateMessage.MessagePart part3 = new AliTemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = TitleStr
            };
            partArray[2] = part3;
            AliTemplateMessage.MessagePart part4 = new AliTemplateMessage.MessagePart
            {
                Name = "remark",
                Value = RemarkData
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_AccountChangeMsg(string templateId, SiteSettings settings, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string ChangeTypeDesc, [Optional, DefaultParameterValue("")] string RemarkData)
        {
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[5];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "帐号更新提醒" : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "account",
                Value = string.IsNullOrEmpty(member.UserBindName) ? member.UserName : member.UserBindName
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "time",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "type",
                Value = ChangeTypeDesc
            };
            partArray[3] = part4;
            TemplateMessage.MessagePart part5 = new TemplateMessage.MessagePart
            {
                Name = "remark",
                Value = RemarkData
            };
            partArray[4] = part5;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_CouponWillExpiredMsg(string templateId, SiteSettings settings, CouponInfo_MemberWeiXin couponInfo, [Optional, DefaultParameterValue("")] string FirstData)
        {
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[4];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "您有一张优惠券即将过期" : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "orderTicketStore",
                Value = couponInfo.IsAllProduct ? "全部商品" : "部分商品"
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "orderTicketRule",
                Value = (couponInfo.ConditionValue > 0M) ? string.Format("订单满{0}元可用", couponInfo.ConditionValue.ToString("F2")) : "不限制"
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "remark",
                Value = "优惠券可以订单支付时抵扣等额现金。"
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_DistributorCreateMsg(string templateId, SiteSettings settings, DistributorsInfo distributor, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData)
        {
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[4];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "您好，有一位新分销商申请了店铺" : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = string.IsNullOrEmpty(member.UserBindName) ? member.UserName : member.UserBindName
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = member.CellPhone
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "keyword3",
                Value = DateTime.Now.ToString("yyyy-MM-dd")
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_DrawCashResultMsg(string templateId, SiteSettings settings, BalanceDrawRequestInfo balance, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string IsCheckDesc)
        {
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[7];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "分销商提现" : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = balance.StoreName
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = balance.Amount.ToString("F2")
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "keyword3",
                Value = balance.StoreName + "[" + balance.MerchantCode + "]"
            };
            partArray[3] = part4;
            TemplateMessage.MessagePart part5 = new TemplateMessage.MessagePart
            {
                Name = "keyword4",
                Value = balance.RequestTime.ToString("yyyy-MM-dd")
            };
            partArray[4] = part5;
            TemplateMessage.MessagePart part6 = new TemplateMessage.MessagePart
            {
                Name = "keyword5",
                Value = IsCheckDesc
            };
            partArray[5] = part6;
            TemplateMessage.MessagePart part7 = new TemplateMessage.MessagePart
            {
                Name = "remark",
                Value = balance.Remark
            };
            partArray[6] = part7;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_OrderMsg(string templateId, SiteSettings settings, OrderInfo order, [Optional, DefaultParameterValue("")] string FirstData)
        {
            string firstProductName = new OrderDao().GetFirstProductName(order.OrderId);
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[5];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? ("订单号：" + order.OrderId) : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = firstProductName
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = order.GetTotal().ToString("F2")
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "keyword3",
                Value = OrderInfo.GetOrderStatusName(order.OrderStatus)
            };
            partArray[3] = part4;
            TemplateMessage.MessagePart part5 = new TemplateMessage.MessagePart
            {
                Name = "remark",
                Value = ""
            };
            partArray[4] = part5;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_PersonalMsg(string templateId, SiteSettings settings, MemberInfo member, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string ContentData, [Optional, DefaultParameterValue("")] string title)
        {
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[4];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = string.IsNullOrEmpty(FirstData) ? "个人消息通知" : FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = string.IsNullOrEmpty(title) ? "标题" : title
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = DateTime.Now.ToString("yyyy-MM-dd")
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "keyword3",
                Value = string.IsNullOrEmpty(ContentData) ? "获得了奖品" : ContentData
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static TemplateMessage GenerateWeixinMessage_RefundSuccessMsg(string templateId, SiteSettings settings, OrderInfo order, RefundInfo refundInfo, [Optional, DefaultParameterValue("")] string FirstData)
        {
            string productName = new ProductDao().GetProductDetails(refundInfo.ProductId).ProductName;
            string weixinToken = settings.WeixinToken;
            try
            {
                if (string.IsNullOrEmpty(refundInfo.RefundRemark))
                {
                    refundInfo.RefundRemark = "";
                }
                TemplateMessage message2 = new TemplateMessage
                {
                    Url = "",
                    TemplateId = templateId,
                    Touser = ""
                };
                TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[7];
                TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
                {
                    Name = "first",
                    Value = string.IsNullOrEmpty(FirstData) ? ("订单号：" + order.OrderId) : FirstData
                };
                partArray[0] = part;
                TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
                {
                    Name = "keynote1",
                    Value = refundInfo.RefundMoney.ToString("f2")
                };
                partArray[1] = part2;
                TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
                {
                    Name = "keynote2",
                    Value = "请联系商家"
                };
                partArray[2] = part3;
                TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
                {
                    Name = "keynote3",
                    Value = "请联系商家"
                };
                partArray[3] = part4;
                TemplateMessage.MessagePart part5 = new TemplateMessage.MessagePart
                {
                    Name = "keynote4",
                    Value = productName
                };
                partArray[4] = part5;
                TemplateMessage.MessagePart part6 = new TemplateMessage.MessagePart
                {
                    Name = "keynote5",
                    Value = order.OrderId
                };
                partArray[5] = part6;
                TemplateMessage.MessagePart part7 = new TemplateMessage.MessagePart
                {
                    Name = "keynote6",
                    Value = refundInfo.RefundRemark.Replace("\r", "").Replace("\n", "")
                };
                partArray[6] = part7;
                message2.Data = partArray;
                return message2;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static TemplateMessage GenerateWeixinMessage_ServiceMsg(string templateId, SiteSettings settings, [Optional, DefaultParameterValue("")] string FirstData, [Optional, DefaultParameterValue("")] string TitleStr, [Optional, DefaultParameterValue("")] string RemarkData)
        {
            string weixinToken = settings.WeixinToken;
            TemplateMessage message2 = new TemplateMessage
            {
                Url = "",
                TemplateId = templateId,
                Touser = ""
            };
            TemplateMessage.MessagePart[] partArray = new TemplateMessage.MessagePart[4];
            TemplateMessage.MessagePart part = new TemplateMessage.MessagePart
            {
                Name = "first",
                Value = FirstData
            };
            partArray[0] = part;
            TemplateMessage.MessagePart part2 = new TemplateMessage.MessagePart
            {
                Name = "keyword1",
                Value = TitleStr
            };
            partArray[1] = part2;
            TemplateMessage.MessagePart part3 = new TemplateMessage.MessagePart
            {
                Name = "keyword2",
                Value = DateTime.Now.ToString("yyyy-MM-dd")
            };
            partArray[2] = part3;
            TemplateMessage.MessagePart part4 = new TemplateMessage.MessagePart
            {
                Name = "remark",
                Value = RemarkData
            };
            partArray[3] = part4;
            message2.Data = partArray;
            return message2;
        }

        private static MailMessage GenericOrderEmail(MessageTemplate template, SiteSettings settings, string UserName, string userEmail, string orderId, decimal total, string memo, string shippingType, string shippingName, string shippingAddress, string shippingZip, string shippingPhone, string shippingCell, string shippingEmail, string shippingBillno, decimal refundMoney, string closeReason)
        {
            MailMessage emailTemplate = MessageTemplateHelper.GetEmailTemplate(template, userEmail);
            if (emailTemplate == null)
            {
                return null;
            }
            emailTemplate.Subject = GenericOrderMessageFormatter(settings, UserName, emailTemplate.Subject, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
            emailTemplate.Body = GenericOrderMessageFormatter(settings, UserName, emailTemplate.Body, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
            return emailTemplate;
        }

        private static string GenericOrderMessageFormatter(SiteSettings settings, string UserName, string stringToFormat, string orderId, decimal total, string memo, string shippingType, string shippingName, string shippingAddress, string shippingZip, string shippingPhone, string shippingCell, string shippingEmail, string shippingBillno, decimal refundMoney, string closeReason)
        {
            stringToFormat = stringToFormat.Replace("$SiteName$", settings.SiteName.Trim());
            stringToFormat = stringToFormat.Replace("$UserName$", UserName);
            stringToFormat = stringToFormat.Replace("$OrderId$", orderId);
            stringToFormat = stringToFormat.Replace("$Total$", total.ToString("F"));
            stringToFormat = stringToFormat.Replace("$Memo$", memo);
            stringToFormat = stringToFormat.Replace("$Shipping_Type$", shippingType);
            stringToFormat = stringToFormat.Replace("$Shipping_Name$", shippingName);
            stringToFormat = stringToFormat.Replace("$Shipping_Addr$", shippingAddress);
            stringToFormat = stringToFormat.Replace("$Shipping_Zip$", shippingZip);
            stringToFormat = stringToFormat.Replace("$Shipping_Phone$", shippingPhone);
            stringToFormat = stringToFormat.Replace("$Shipping_Cell$", shippingCell);
            stringToFormat = stringToFormat.Replace("$Shipping_Email$", shippingEmail);
            stringToFormat = stringToFormat.Replace("$Shipping_Billno$", shippingBillno);
            stringToFormat = stringToFormat.Replace("$RefundMoney$", refundMoney.ToString("F"));
            stringToFormat = stringToFormat.Replace("$CloseReason$", closeReason);
            return stringToFormat;
        }

        private static void GenericOrderMessages(SiteSettings settings, string UserName, string userEmail, string orderId, decimal total, string memo, string shippingType, string shippingName, string shippingAddress, string shippingZip, string shippingPhone, string shippingCell, string shippingEmail, string shippingBillno, decimal refundMoney, string closeReason, MessageTemplate template, out MailMessage email, out string smsMessage, out string innerSubject, out string innerMessage)
        {
            email = null;
            smsMessage = null;
            innerSubject = (string)(innerMessage = null);
            if ((template != null) && (settings != null))
            {
                if (template.SendEmail && settings.EmailEnabled)
                {
                    email = GenericOrderEmail(template, settings, UserName, userEmail, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
                }
                if (template.SendSMS && settings.SMSEnabled)
                {
                    smsMessage = GenericOrderMessageFormatter(settings, UserName, template.SMSBody, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
                }
                if (template.SendInnerMessage)
                {
                    innerSubject = GenericOrderMessageFormatter(settings, UserName, template.InnerMessageSubject, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
                    innerMessage = GenericOrderMessageFormatter(settings, UserName, template.InnerMessageBody, orderId, total, memo, shippingType, shippingName, shippingAddress, shippingZip, shippingPhone, shippingCell, shippingEmail, shippingBillno, refundMoney, closeReason);
                }
            }
        }

        private static MailMessage GenericUserEmail(MessageTemplate template, SiteSettings settings, string UserName, string userEmail, string password, string dealPassword)
        {
            MailMessage emailTemplate = MessageTemplateHelper.GetEmailTemplate(template, userEmail);
            if (emailTemplate == null)
            {
                return null;
            }
            emailTemplate.Subject = GenericUserMessageFormatter(settings, emailTemplate.Subject, UserName, userEmail, password, dealPassword);
            emailTemplate.Body = GenericUserMessageFormatter(settings, emailTemplate.Body, UserName, userEmail, password, dealPassword);
            return emailTemplate;
        }

        private static string GenericUserMessageFormatter(SiteSettings settings, string stringToFormat, string UserName, string userEmail, string password, string dealPassword)
        {
            stringToFormat = stringToFormat.Replace("$SiteName$", settings.SiteName.Trim());
            stringToFormat = stringToFormat.Replace("$UserName$", UserName.Trim());
            stringToFormat = stringToFormat.Replace("$Email$", userEmail.Trim());
            stringToFormat = stringToFormat.Replace("$Password$", password);
            stringToFormat = stringToFormat.Replace("$DealPassword$", dealPassword);
            return stringToFormat;
        }

        private static void GenericUserMessages(SiteSettings settings, string UserName, string userEmail, string password, string dealPassword, MessageTemplate template, out MailMessage email, out string smsMessage, out string innerSubject, out string innerMessage)
        {
            email = null;
            smsMessage = null;
            innerSubject = (string)(innerMessage = null);
            if ((template != null) && (settings != null))
            {
                if (template.SendEmail && settings.EmailEnabled)
                {
                    email = GenericUserEmail(template, settings, UserName, userEmail, password, dealPassword);
                }
                if (template.SendSMS && settings.SMSEnabled)
                {
                    smsMessage = GenericUserMessageFormatter(settings, template.SMSBody, UserName, userEmail, password, dealPassword);
                }
                if (template.SendInnerMessage)
                {
                    innerSubject = GenericUserMessageFormatter(settings, template.InnerMessageSubject, UserName, userEmail, password, dealPassword);
                    innerMessage = GenericUserMessageFormatter(settings, template.InnerMessageBody, UserName, userEmail, password, dealPassword);
                }
            }
        }

        private static string GetUserCellPhone(MemberInfo user)
        {
            if (user == null)
            {
                return null;
            }
            return user.CellPhone;
        }

        private static void Send(MessageTemplate template, SiteSettings settings, TemplateMessage templateMessage)
        {
            if ((template.SendWeixin && !string.IsNullOrWhiteSpace(template.WeixinTemplateId)) && (templateMessage != null))
            {
                TemplateApi.SendMessage(TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret), templateMessage);
            }
        }

        private static void Send_Fuwu_ToListUser(List<string> SendToUserList, AliTemplateMessage templateMessage)
        {
            if (templateMessage != null)
            {
                foreach (string str in SendToUserList)
                {
                    templateMessage.Touser = str;
                    AliOHHelper.log(AliOHHelper.templateSendMessage(templateMessage, "查看详情"));
                    try
                    {
                        AliOHHelper.log(AliOHHelper.TemplateSend(templateMessage).Body);
                    }
                    catch (Exception exception)
                    {
                        AliOHHelper.log(exception.Message.ToString());
                    }
                }
            }
        }

        private static void Send_Fuwu_ToMoreUser(string TemplateDetailType, string BuyerWXOpenId, string SalerWXOpenId, MessageTemplate template, SiteSettings settings, bool sendFirst, AliTemplateMessage templateMessage)
        {
            if (string.IsNullOrEmpty(templateMessage.TemplateId))
            {
                AliOHHelper.log("模板ID为空值,当前模板类型" + TemplateDetailType);
            }
            else
            {
                List<string> sendToUserList = new List<string>();
                AliOHHelper.log("当前模板类型：" + TemplateDetailType + ",会员" + BuyerWXOpenId + "|分销商" + SalerWXOpenId + "|" + templateMessage.TemplateId);
                if (((settings.AlipayAppid.Length > 15) && !string.IsNullOrWhiteSpace(template.WeixinTemplateId)) && (templateMessage != null))
                {
                    string fieldName = "";
                    if (TemplateDetailType == "OrderCreate")
                    {
                        fieldName = "Msg1";
                    }
                    else if (TemplateDetailType == "OrderPay")
                    {
                        fieldName = "Msg2";
                    }
                    else if (TemplateDetailType == "ServiceRequest")
                    {
                        fieldName = "Msg3";
                    }
                    else if (TemplateDetailType == "DrawCashRequest")
                    {
                        fieldName = "Msg4";
                    }
                    else if (TemplateDetailType == "ProductAsk")
                    {
                        fieldName = "Msg5";
                    }
                    else if (TemplateDetailType == "DistributorCreate")
                    {
                        fieldName = "Msg6";
                    }
                    if (fieldName != "")
                    {
                        sendToUserList = MessageTemplateHelper.GetFuwuAdminUserMsgList(fieldName);
                    }
                    if (!(string.IsNullOrEmpty(BuyerWXOpenId) || !template.IsSendWeixin_ToMember))
                    {
                        sendToUserList.Add(BuyerWXOpenId);
                    }
                    if (!string.IsNullOrEmpty(SalerWXOpenId) && template.IsSendWeixin_ToDistributor)
                    {
                        if (SalerWXOpenId == "*")
                        {
                            new DistributorsDao().SelectDistributorsAliOpenId(ref sendToUserList);
                        }
                        else
                        {
                            sendToUserList.Add(SalerWXOpenId);
                        }
                    }
                    Send_Fuwu_ToListUser(sendToUserList.Distinct<string>().ToList<string>(), templateMessage);
                }
            }
        }

        private static void Send_WeiXin_ToListUser(List<string> SendToUserList, TemplateMessage templateMessage, SiteSettings settings, [Optional, DefaultParameterValue("")] string accessToken)
        {
            if (templateMessage != null)
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret);
                }
                foreach (string str in SendToUserList)
                {
                    templateMessage.Touser = str;
                    try
                    {
                        if (TemplateApi.SendMessageDebug(accessToken, templateMessage).Contains("invalid credential"))
                        {
                            accessToken = TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret);
                            string str2 = TemplateApi.SendMessageDebug(accessToken, templateMessage);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static void Send_WeiXin_ToMoreUser(string TemplateDetailType, string BuyerWXOpenId, string SalerWXOpenId, MessageTemplate template, SiteSettings settings, bool sendFirst, TemplateMessage templateMessage)
        {
            List<string> sendToUserList = new List<string>();
            AliOHHelper.log("当前微信模板类型：" + TemplateDetailType + ",会员" + BuyerWXOpenId + "|分销商" + SalerWXOpenId + "|" + templateMessage.TemplateId);
            if ((template.SendWeixin && !string.IsNullOrWhiteSpace(template.WeixinTemplateId)) && (templateMessage != null))
            {
                string accessToken = TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret);
                string fieldName = "";
                if (TemplateDetailType == "OrderCreate")
                {
                    fieldName = "Msg1";
                }
                else if (TemplateDetailType == "OrderPay")
                {
                    fieldName = "Msg2";
                }
                else if (TemplateDetailType == "ServiceRequest")
                {
                    fieldName = "Msg3";
                }
                else if (TemplateDetailType == "DrawCashRequest")
                {
                    fieldName = "Msg4";
                }
                else if (TemplateDetailType == "ProductAsk")
                {
                    fieldName = "Msg5";
                }
                else if (TemplateDetailType == "DistributorCreate")
                {
                    fieldName = "Msg6";
                }
                if (fieldName != "")
                {
                    sendToUserList = MessageTemplateHelper.GetAdminUserMsgList(fieldName);
                }
                if (!(string.IsNullOrEmpty(BuyerWXOpenId) || !template.IsSendWeixin_ToMember))
                {
                    sendToUserList.Add(BuyerWXOpenId);
                }
                if (!string.IsNullOrEmpty(SalerWXOpenId) && template.IsSendWeixin_ToDistributor)
                {
                    if (SalerWXOpenId == "*")
                    {
                        new DistributorsDao().SelectDistributorsOpenId(ref sendToUserList);
                    }
                    else
                    {
                        sendToUserList.Add(SalerWXOpenId);
                    }
                }
                Send_WeiXin_ToListUser(sendToUserList.Distinct<string>().ToList<string>(), templateMessage, settings, accessToken);
            }
        }

        private static bool Send_WeiXin_ToOneUser(string TemplateDetailType, string accessToken, string OpenId, MessageTemplate template, SiteSettings settings, bool sendFirst, TemplateMessage templateMessage)
        {
            bool flag = false;
            List<string> list = new List<string>();
            if ((template.SendWeixin && !string.IsNullOrWhiteSpace(template.WeixinTemplateId)) && (templateMessage != null))
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret);
                }
                list.Add(OpenId);
                foreach (string str in list)
                {
                    templateMessage.Touser = str;
                    try
                    {
                        if (TemplateApi.SendMessageDebug(accessToken, templateMessage).Contains("invalid credential"))
                        {
                            accessToken = TokenApi.GetToken_Message(settings.WeixinAppId, settings.WeixinAppSecret);
                            string str2 = TemplateApi.SendMessageDebug(accessToken, templateMessage);
                        }
                        flag = true;
                    }
                    catch (Exception)
                    {
                        flag = false;
                    }
                }
            }
            return flag;
        }

        public static void SendFuwuMsg_AccountLockOrUnLock(MemberInfo member, bool IsLock)
        {
            string detailType = "AccountLock";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = null;
                if (IsLock)
                {
                    templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您的分销商资格已被冻结", "账户冻结", "您的分销商资格已被冻结，如有疑问，请联系客服！");
                }
                else
                {
                    templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您的分销商资格已解冻", "账户解冻", "您的分销商资格账户已解冻，如有疑问，请联系客服！");
                }
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, member.AlipayOpenid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static bool SendFuwuMsg_CouponWillExpired(CouponInfo_MemberWeiXin coupon)
        {
            string detailType = "CouponWillExpired";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType == null)
            {
                return false;
            }
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
            AliTemplateMessage templateMessage = GenerateFuwuMessage_CouponWillExpiredMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, coupon, string.Format("您有一张优惠券将于{0}到期", coupon.EndDate.ToString("yyyy-MM-dd HH:mm:ss")));
            FuwuSend(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, templateMessage);
            return true;
        }

        public static void SendFuwuMsg_DistributorCancel(MemberInfo member)
        {
            string detailType = "DistributorCancel";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您已经被取消分销资质", "账户被取消分销商资质", "您的分销商资格已被取消，如有疑问，请联系客服！");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, member.AlipayOpenid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_DistributorCreate(DistributorsInfo distributor, MemberInfo member)
        {
            string buyerWXOpenId = "";
            if (member.AlipayOpenid != null)
            {
                buyerWXOpenId = member.AlipayOpenid;
            }
            else
            {
                member.AlipayOpenid = "";
            }
            string detailType = "DistributorCreate";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_DistributorCreateMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, "您好，有一位新分销商申请了店铺");
                Send_Fuwu_ToMoreUser(detailType, buyerWXOpenId, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
                int topNum = 0;
                int distributorNumOfTotal = new MemberDao().GetDistributorNumOfTotal(member.ReferralUserId, out topNum);
                List<string> sendToUserList = new List<string>();
                if (!string.IsNullOrEmpty(member.AlipayOpenid))
                {
                    AliTemplateMessage message2 = GenerateFuwuMessage_DistributorCreateMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, string.Format("恭喜您，成为{0}的第{1}位分销商", masterSettings.SiteName, distributorNumOfTotal));
                    sendToUserList.Add(member.AlipayOpenid);
                    Send_Fuwu_ToListUser(sendToUserList, message2);
                }
                if ((member.ReferralUserId > 0) && (member.ReferralUserId != member.UserId))
                {
                    string aliOpenIDByUserId = new MemberDao().GetAliOpenIDByUserId(member.ReferralUserId);
                    if (!string.IsNullOrEmpty(aliOpenIDByUserId))
                    {
                        sendToUserList.Clear();
                        sendToUserList.Add(aliOpenIDByUserId);
                        AliTemplateMessage message3 = GenerateFuwuMessage_DistributorCreateMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, string.Format("恭喜您，{0}成功开店，成为您的第{1}位下级分销商", member.UserName, topNum));
                        Send_Fuwu_ToListUser(sendToUserList, message3);
                    }
                }
            }
        }

        public static void SendFuwuMsg_DistributorGradeChange(MemberInfo member, string gradeName)
        {
            if (!string.IsNullOrEmpty(member.AlipayOpenid))
            {
                string detailType = "DistributorGradeChange";
                MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
                if (fuwuMessageTemplateByDetailType != null)
                {
                    SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                    AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "恭喜您成功升级！", "分销商账户升级", "恭喜您成功升级为[" + gradeName + "]，您将享受到更多的分销商特权！");
                    Send_Fuwu_ToMoreUser(detailType, "", member.AlipayOpenid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
                }
            }
        }

        public static void SendFuwuMsg_DrawCashReject(BalanceDrawRequestInfo balance)
        {
            string detailType = "DrawCashReject";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_DrawCashResultMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, balance, "分销商提现结果被驳回", "驳回");
                string aliOpenIDByUserId = new MemberDao().GetAliOpenIDByUserId(balance.UserId);
                Send_Fuwu_ToMoreUser(detailType, aliOpenIDByUserId, aliOpenIDByUserId, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_DrawCashRelease(BalanceDrawRequestInfo balance)
        {
            string detailType = "DrawCashRelease";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_DrawCashResultMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, balance, "分销商提现已通过", "通过");
                string aliOpenIDByUserId = new MemberDao().GetAliOpenIDByUserId(balance.UserId);
                Send_Fuwu_ToMoreUser(detailType, aliOpenIDByUserId, aliOpenIDByUserId, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_DrawCashRequest(BalanceDrawRequestInfo balance)
        {
            string str = "其它";
            if (balance.RequesType == 0)
            {
                str = "微信钱包";
            }
            else if (balance.RequesType == 1)
            {
                str = "(支付宝)" + balance.MerchantCode;
            }
            else if (balance.RequesType == 2)
            {
                str = "(" + balance.BankName + ")" + balance.MerchantCode;
            }
            else if (balance.RequesType == 3)
            {
                str = "微信红包";
            }
            string detailType = "DrawCashRequest";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_ServiceMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, "分销商" + balance.UserName + "（" + balance.StoreName + "）申请提现", "分销商提现申请", "申请金额：￥" + balance.Amount.ToString("F2") + "   提现账户：" + str);
                string aliOpenIDByUserId = new MemberDao().GetAliOpenIDByUserId(balance.UserId);
                Send_Fuwu_ToMoreUser(detailType, aliOpenIDByUserId, aliOpenIDByUserId, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_MemberGradeChange(MemberInfo member)
        {
            string detailType = "MemberGradeChange";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "恭喜您的会员等级升级", "账户升级", "恭喜您成功升级，您将享受到更多的会员特权！");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, member.AlipayOpenid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_MemberRegister(MemberInfo member)
        {
            string detailType = "PrizeRelease";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                AliTemplateMessage message;
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                int topNum = 0;
                int memberNumOfTotal = new MemberDao().GetMemberNumOfTotal(member.ReferralUserId, out topNum);
                List<string> sendToUserList = new List<string>();
                if (!string.IsNullOrEmpty(member.AlipayOpenid))
                {
                    message = GenerateFuwuMessage_PersonalMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "会员注册通知", string.Format("恭喜您，成为{0}的第{1}位会员", masterSettings.SiteName, memberNumOfTotal));
                    sendToUserList.Add(member.AlipayOpenid);
                    Send_Fuwu_ToListUser(sendToUserList, message);
                }
                if ((member.ReferralUserId > 0) && (member.ReferralUserId != member.UserId))
                {
                    string aliOpenIDByUserId = new MemberDao().GetAliOpenIDByUserId(member.ReferralUserId);
                    if (!string.IsNullOrEmpty(aliOpenIDByUserId))
                    {
                        message = GenerateFuwuMessage_PersonalMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "下级会员注册通知", string.Format("恭喜您！您成功邀请到{0}成为您店铺的第{1}位下级会员！", member.UserName, topNum));
                        sendToUserList.Clear();
                        sendToUserList.Add(aliOpenIDByUserId);
                        Send_Fuwu_ToListUser(sendToUserList, message);
                    }
                }
            }
        }

        public static string SendFuwuMsg_OrderCreate(OrderInfo order)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderCreate";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType == null)
            {
                return ("消息模板不存在。模板：" + detailType);
            }
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单【" + order.OrderId + "】已经提交成功。");
            Send_Fuwu_ToMoreUser(detailType, str, str2, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            return "OK";
        }

        public static void SendFuwuMsg_OrderDeliver(OrderInfo order)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderDeliver";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，您的订单" + order.OrderId + "已经发货！");
                Send_Fuwu_ToMoreUser(detailType, str, str2, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_OrderGetCommission(OrderInfo order, string AliOpneid, decimal CommissionAmount)
        {
            string detailType = "OrderGetCommission";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，分销订单" + order.OrderId + "已经完成，您获得佣金￥" + CommissionAmount.ToString("F2"));
                Send_Fuwu_ToMoreUser(detailType, "", AliOpneid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_OrderGetCoupon(OrderInfo order)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderGetCoupon";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单" + order.OrderId + "已经完成，您获得优惠券一张");
                Send_Fuwu_ToMoreUser(detailType, str, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_OrderGetPoint(OrderInfo order, int integral)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderGetPoint";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单" + order.OrderId + "已经完成，您获得积分" + integral.ToString());
                Send_Fuwu_ToMoreUser(detailType, str, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_OrderPay(OrderInfo order)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderPay";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_OrderMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单【" + order.OrderId + "】已付款成功。请等待卖家发货！");
                Send_Fuwu_ToMoreUser(detailType, str, str2, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_PasswordReset(MemberInfo member)
        {
            string detailType = "PasswordReset";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您重置了商城账户的登录密码", "密码重置", "您成功修改了账户的登录密码，请牢记。如有问题，请联系客服。");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_PasswordReset(MemberInfo member, string password)
        {
            string detailType = "PasswordReset";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您重置了商城账户的登录密码 ", "密码重置", "您账户修改登录密码:" + password + "，请牢记。如有问题，请联系客服。");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_PrizeRelease(MemberInfo member, string GameTitle)
        {
            if (GameTitle == null)
            {
                GameTitle = "";
            }
            string detailType = "PrizeRelease";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_PersonalMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您好，您的奖品已经发货！", "您参与的抽奖活动【" + GameTitle + "】所获得奖品已经发货，请注意收货");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, member.AlipayOpenid, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_ProductAsk(string ProductName, string SalerOpenId, string AskContent)
        {
            string detailType = "ProductAsk";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AskContent = AskContent.Replace("\r", "").Replace("\n", "");
                AliTemplateMessage templateMessage = GenerateFuwuMessage_ServiceMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, "您有最新的商品咨询待处理", "商品咨询", "商品名称：" + ProductName + "  咨询内容：" + AskContent);
                Send_Fuwu_ToMoreUser(detailType, SalerOpenId, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_ProductCreate(string ProductName)
        {
            string detailType = "ProductCreate";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_ServiceMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, "有新品上线了。", "新品上架提醒", "商品名称：" + ProductName);
                Send_Fuwu_ToMoreUser(detailType, "", "*", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_RefundSuccess(RefundInfo refundInfo)
        {
            string str;
            string str2;
            OrderInfo orderInfo = new OrderDao().GetOrderInfo(refundInfo.OrderId);
            new OrderDao().GetOrderUserAliOpenId(refundInfo.OrderId, out str, out str2);
            string detailType = "RefundSuccess";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_RefundSuccessMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, orderInfo, refundInfo, "您的退款申请已经发放，敬请关注！");
                Send_Fuwu_ToMoreUser(detailType, str, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_ServiceRequest(OrderInfo order, int refundType)
        {
            string str;
            string str2;
            new OrderDao().GetOrderUserAliOpenId(order.OrderId, out str, out str2);
            string str3 = "退款";
            if (refundType == 1)
            {
                str3 = "退货";
            }
            string detailType = "ServiceRequest";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_ServiceMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, "买家申请" + str3 + "，请注意处理", "买家" + str3 + "申请", "买家由于：" + order.RefundRemark + ",对订单" + order.OrderId + " 申请" + str3 + "，请及时处理。");
                Send_Fuwu_ToMoreUser(detailType, str, str2, fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendFuwuMsg_SysBindUserName(MemberInfo member, string password)
        {
            string detailType = "PasswordReset";
            MessageTemplate fuwuMessageTemplateByDetailType = MessageTemplateHelper.GetFuwuMessageTemplateByDetailType(detailType);
            if (fuwuMessageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                AliTemplateMessage templateMessage = GenerateFuwuMessage_AccountChangeMsg(fuwuMessageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "管理员为您绑定了商城账户及密码 ", "系统帐号绑定", "您的系统账户登录密码:" + password + "，请牢记。如有问题，请联系客服。");
                Send_Fuwu_ToMoreUser(detailType, member.AlipayOpenid, "", fuwuMessageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        internal static bool SendMail(MailMessage email, EmailSender sender)
        {
            string str;
            return SendMail(email, sender, out str);
        }

        internal static bool SendMail(MailMessage email, EmailSender sender, out string msg)
        {
            try
            {
                msg = "";
                return sender.Send(email, Encoding.GetEncoding(HiConfiguration.GetConfig().EmailEncoding));
            }
            catch (Exception exception)
            {
                msg = exception.Message;
                return false;
            }
        }

        public static void SendWeiXinMsg_AccountLock(MemberInfo member)
        {
            SendWeiXinMsg_AccountLockOrUnLock(member, true);
        }

        public static void SendWeiXinMsg_AccountLockOrUnLock(MemberInfo member, bool IsLock)
        {
            new Thread(() => SendFuwuMsg_AccountLockOrUnLock(member, IsLock)).Start();
            string detailType = "AccountLock";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = null;
                if (IsLock)
                {
                    templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您的分销商资格已被冻结", "账户冻结", "您的分销商资格已被冻结，如有疑问，请联系客服！");
                }
                else
                {
                    templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您的分销商资格已解冻", "账户解冻", "您的分销商资格账户已解冻，如有疑问，请联系客服！");
                }
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, member.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_AccountUnLock(MemberInfo member)
        {
            SendWeiXinMsg_AccountLockOrUnLock(member, false);
        }

        public static bool SendWeiXinMsg_CouponWillExpired(CouponInfo_MemberWeiXin coupon)
        {
            new Thread(new ThreadStart(delegate
            {
                SendFuwuMsg_CouponWillExpired(coupon);
            })).Start();
            string detailType = "CouponWillExpired";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType == null)
            {
                return false;
            }
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            TemplateMessage templateMessage = GenerateWeixinMessage_CouponWillExpiredMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, coupon, string.Format("您有一张优惠券将于{0}到期", coupon.EndDate.ToString("yyyy-MM-dd HH:mm:ss")));
            string accessToken = TokenApi.GetToken_Message(masterSettings.WeixinAppId, masterSettings.WeixinAppSecret);
            return Send_WeiXin_ToOneUser(detailType, accessToken, coupon.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
        }

        public static void SendWeiXinMsg_DistributorCancel(MemberInfo member)
        {
            new Thread(() => SendFuwuMsg_DistributorCancel(member)).Start();
            string detailType = "DistributorCancel";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您已经被取消分销资质", "账户被取消分销商资质", "您的分销商资格已被取消，如有疑问，请联系客服！");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, member.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_DistributorCreate(DistributorsInfo distributor, MemberInfo member)
        {
            new Thread(() => SendFuwuMsg_DistributorCreate(distributor, member)).Start();
            string detailType = "DistributorCreate";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_DistributorCreateMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, "您好，有一位新分销商申请了店铺");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
                int topNum = 0;
                int distributorNumOfTotal = new MemberDao().GetDistributorNumOfTotal(member.ReferralUserId, out topNum);
                List<string> sendToUserList = new List<string>();
                if (!string.IsNullOrEmpty(member.OpenId))
                {
                    TemplateMessage message2 = GenerateWeixinMessage_DistributorCreateMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, string.Format("恭喜您，成为{0}的第{1}位分销商", masterSettings.SiteName, distributorNumOfTotal));
                    sendToUserList.Add(member.OpenId);
                    Send_WeiXin_ToListUser(sendToUserList, message2, masterSettings, "");
                }
                if ((member.ReferralUserId > 0) && (member.ReferralUserId != member.UserId))
                {
                    string openIDByUserId = new MemberDao().GetOpenIDByUserId(member.ReferralUserId);
                    if (!string.IsNullOrEmpty(openIDByUserId))
                    {
                        sendToUserList.Clear();
                        sendToUserList.Add(openIDByUserId);
                        TemplateMessage message3 = GenerateWeixinMessage_DistributorCreateMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, distributor, member, string.Format("恭喜您，{0}成功开店，成为您的第{1}位下级分销商", member.UserName, topNum));
                        Send_WeiXin_ToListUser(sendToUserList, message3, masterSettings, "");
                    }
                }
            }
        }

        public static void SendWeiXinMsg_DistributorGradeChange(MemberInfo member, string gradeName)
        {
            new Thread(() => SendFuwuMsg_DistributorGradeChange(member, gradeName)).Start();
            if (!string.IsNullOrEmpty(member.OpenId))
            {
                string detailType = "DistributorGradeChange";
                MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
                if (messageTemplateByDetailType != null)
                {
                    SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                    TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "恭喜您成功升级！", "分销商账户升级", "恭喜您成功升级为[" + gradeName + "]，您将享受到更多的分销商特权！");
                    Send_WeiXin_ToMoreUser(detailType, "", member.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
                }
            }
        }

        public static void SendWeiXinMsg_DrawCashReject(BalanceDrawRequestInfo balance)
        {
            new Thread(() => SendFuwuMsg_DrawCashReject(balance)).Start();
            string detailType = "DrawCashReject";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                balance.Remark = "驳回原因：" + balance.Remark;
                TemplateMessage templateMessage = GenerateWeixinMessage_DrawCashResultMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, balance, "分销商提现结果被驳回", "被驳回");
                Send_WeiXin_ToMoreUser(detailType, balance.UserOpenId, balance.UserOpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_DrawCashRelease(BalanceDrawRequestInfo balance)
        {
            new Thread(() => SendFuwuMsg_DrawCashRelease(balance)).Start();
            string detailType = "DrawCashRelease";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_DrawCashResultMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, balance, "分销商提现已通过", "通过");
                Send_WeiXin_ToMoreUser(detailType, balance.UserOpenId, balance.UserOpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_DrawCashRequest(BalanceDrawRequestInfo balance)
        {
            new Thread(() => SendFuwuMsg_DrawCashRequest(balance)).Start();
            string str = "其它";
            if (balance.RequesType == 0)
            {
                str = "微信钱包";
            }
            else if (balance.RequesType == 1)
            {
                str = "(支付宝)" + balance.MerchantCode;
            }
            else if (balance.RequesType == 2)
            {
                str = "(" + balance.BankName + ")" + balance.MerchantCode;
            }
            else if (balance.RequesType == 3)
            {
                str = "微信红包";
            }
            string detailType = "DrawCashRequest";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_ServiceMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, "分销商" + balance.UserName + "（" + balance.StoreName + "）申请提现", "分销商提现申请", "申请金额：￥" + balance.Amount.ToString("F2") + "   提现账户：" + str);
                Send_WeiXin_ToMoreUser(detailType, balance.UserOpenId, balance.UserOpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_MemberGradeChange(MemberInfo member)
        {
            new Thread(() => SendFuwuMsg_MemberGradeChange(member)).Start();
            string detailType = "MemberGradeChange";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "恭喜您的会员等级升级", "账户升级", "恭喜您成功升级，您将享受到更多的会员特权！");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, member.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_MemberRegister(MemberInfo member)
        {
            new Thread(() => SendFuwuMsg_MemberRegister(member)).Start();
            string detailType = "PrizeRelease";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                TemplateMessage message;
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
                int topNum = 0;
                int memberNumOfTotal = new MemberDao().GetMemberNumOfTotal(member.ReferralUserId, out topNum);
                List<string> sendToUserList = new List<string>();
                if (!string.IsNullOrEmpty(member.OpenId))
                {
                    message = GenerateWeixinMessage_PersonalMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您好，您有新的消息！", string.Format("恭喜您，成为{0}的第{1}位会员", masterSettings.SiteName, memberNumOfTotal), "会员注册成功");
                    sendToUserList.Add(member.OpenId);
                    Send_WeiXin_ToListUser(sendToUserList, message, masterSettings, "");
                }
                if ((member.ReferralUserId > 0) && (member.ReferralUserId != member.UserId))
                {
                    string openIDByUserId = new MemberDao().GetOpenIDByUserId(member.ReferralUserId);
                    if (!string.IsNullOrEmpty(openIDByUserId))
                    {
                        message = GenerateWeixinMessage_PersonalMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您好，您有新的消息！", string.Format("恭喜您！您成功邀请到{0}成为您店铺的第{1}位下级会员！", member.UserName, topNum), "下级会员注册通知");
                        sendToUserList.Clear();
                        sendToUserList.Add(openIDByUserId);
                        Send_WeiXin_ToListUser(sendToUserList, message, masterSettings, "");
                    }
                }
            }
        }

        public static string SendWeiXinMsg_OrderCreate(OrderInfo order)
        {
            string str;
            string str2;
            new Thread(new ThreadStart(delegate
            {
                SendFuwuMsg_OrderCreate(order);
            })).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderCreate";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType == null)
            {
                return ("消息模板不存在。模板：" + detailType);
            }
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
            TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单【" + order.OrderId + "】已经提交成功。");
            Send_WeiXin_ToMoreUser(detailType, str, str2, messageTemplateByDetailType, masterSettings, true, templateMessage);
            return "OK";
        }

        public static void SendWeiXinMsg_OrderDeliver(OrderInfo order)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_OrderDeliver(order)).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderDeliver";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，您的订单" + order.OrderId + "已经发货！");
                Send_WeiXin_ToMoreUser(detailType, str, str2, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_OrderGetCommission(OrderInfo order, string WxOpenId, string AliOpneid, decimal CommissionAmount)
        {
            AliOHHelper.log("AliOpneid:" + AliOpneid);
            new Thread(() => SendFuwuMsg_OrderGetCommission(order, AliOpneid, CommissionAmount)).Start();
            string detailType = "OrderGetCommission";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，分销订单" + order.OrderId + "已经完成，您获得佣金￥" + CommissionAmount.ToString("F2"));
                Send_WeiXin_ToMoreUser(detailType, "", WxOpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_OrderGetCoupon(OrderInfo order)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_OrderGetCoupon(order)).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderGetCoupon";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单" + order.OrderId + "已经完成，您获得优惠券一张");
                Send_WeiXin_ToMoreUser(detailType, str, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_OrderGetPoint(OrderInfo order, int integral)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_OrderGetPoint(order, integral)).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderGetPoint";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单" + order.OrderId + "已经完成，您获得积分" + integral.ToString());
                Send_WeiXin_ToMoreUser(detailType, str, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_OrderPay(OrderInfo order)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_OrderPay(order)).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string detailType = "OrderPay";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_OrderMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, order, "您好，订单【" + order.OrderId + "】已付款成功。请等待卖家发货！");
                Send_WeiXin_ToMoreUser(detailType, str, str2, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_PasswordReset(MemberInfo member)
        {
            new Thread(() => SendFuwuMsg_PasswordReset(member)).Start();
            string detailType = "PasswordReset";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您重置了商城账户的登录密码", "密码重置", "您成功修改了账户的登录密码，请牢记。如有问题，请联系客服。");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_PasswordReset(MemberInfo member, string password)
        {
            new Thread(() => SendFuwuMsg_PasswordReset(member, password)).Start();
            string detailType = "PasswordReset";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您重置了商城账户的登录密码 ", "密码重置", "您账户修改登录密码:" + password + "，请牢记。如有问题，请联系客服。");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_PrizeRelease(MemberInfo member, string GameTitle)
        {
            new Thread(() => SendFuwuMsg_PrizeRelease(member, GameTitle)).Start();
            if (GameTitle == null)
            {
                GameTitle = "";
            }
            string detailType = "PrizeRelease";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_PersonalMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "您好，您的奖品已经发货！", "您参与的抽奖活动【" + GameTitle + "】所获得奖品已经发货，请注意收货", "");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, member.OpenId, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_ProductAsk(string ProductName, string SalerOpenId, string AskContent)
        {
            new Thread(() => SendFuwuMsg_ProductAsk(ProductName, SalerOpenId, AskContent)).Start();
            string detailType = "ProductAsk";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                AskContent = AskContent.Replace("\r", "").Replace("\n", "");
                TemplateMessage templateMessage = GenerateWeixinMessage_ServiceMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, "您有最新的商品咨询待处理", "商品咨询", "商品名称：" + ProductName + "  咨询内容：" + AskContent);
                Send_WeiXin_ToMoreUser(detailType, SalerOpenId, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_ProductCreate(string ProductName)
        {
            new Thread(() => SendFuwuMsg_ProductCreate(ProductName)).Start();
            string detailType = "ProductCreate";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_ServiceMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, "有新品上线了。", "新品上架提醒", "商品名称：" + ProductName);
                Send_WeiXin_ToMoreUser(detailType, "", "*", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_RefundSuccess(RefundInfo refundInfo)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_RefundSuccess(refundInfo)).Start();
            OrderInfo orderInfo = new OrderDao().GetOrderInfo(refundInfo.OrderId);
            new OrderDao().GetOrderUserOpenId(refundInfo.OrderId, out str, out str2);
            string detailType = "RefundSuccess";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_RefundSuccessMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, orderInfo, refundInfo, "您的退款申请已经发放，敬请关注！");
                if (templateMessage != null)
                {
                    Send_WeiXin_ToMoreUser(detailType, str, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
                }
            }
        }

        public static void SendWeiXinMsg_ServiceRequest(OrderInfo order, int refundType)
        {
            string str;
            string str2;
            new Thread(() => SendFuwuMsg_ServiceRequest(order, refundType)).Start();
            new OrderDao().GetOrderUserOpenId(order.OrderId, out str, out str2);
            string str3 = "退款";
            if (refundType == 1)
            {
                str3 = "退货";
            }
            string detailType = "ServiceRequest";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_ServiceMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, "买家申请" + str3 + "，请注意处理", "买家" + str3 + "申请", "买家由于：" + order.RefundRemark + ",对订单" + order.OrderId + " 申请" + str3 + "，请及时处理。");
                Send_WeiXin_ToMoreUser(detailType, str, str2, messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }

        public static void SendWeiXinMsg_SysBindUserName(MemberInfo member, string password)
        {
            new Thread(() => SendFuwuMsg_SysBindUserName(member, password)).Start();
            string detailType = "PasswordReset";
            MessageTemplate messageTemplateByDetailType = MessageTemplateHelper.GetMessageTemplateByDetailType(detailType);
            if (messageTemplateByDetailType != null)
            {
                SiteSettings masterSettings = SettingsManager.GetMasterSettings(false);
                TemplateMessage templateMessage = GenerateWeixinMessage_AccountChangeMsg(messageTemplateByDetailType.WeixinTemplateId, masterSettings, member, "管理员为您绑定了商城账户及密码", "系统帐号绑定", "您的系统账户登录密码:" + password + "，请牢记。如有问题，请联系客服。");
                Send_WeiXin_ToMoreUser(detailType, member.OpenId, "", messageTemplateByDetailType, masterSettings, true, templateMessage);
            }
        }
    }
}

