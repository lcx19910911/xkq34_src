namespace Hidistro.Messages
{
    using System;

    internal static class MessageType
    {
        internal const string AcceptDistributorRequest = "AcceptDistributorRequest";
        internal const string AccountLock = "AccountLock";
        internal const string AccountUnLock = "AccountUnLock";
        internal const string BecomeDistributor = "BecomeDistributor";
        internal const string ChangedDealPassword = "ChangedDealPassword";
        internal const string ChangedPassword = "ChangedPassword";
        internal const string Common = "Common";
        internal const string CouponWillExpired = "CouponWillExpired";
        internal const string DistributorCancel = "DistributorCancel";
        internal const string DistributorCreate = "DistributorCreate";
        internal const string DistributorGradeChange = "DistributorGradeChange";
        internal const string DrawCashReject = "DrawCashReject";
        internal const string DrawCashRelease = "DrawCashRelease";
        internal const string DrawCashRequest = "DrawCashRequest";
        internal const string ForgottenPassword = "ForgottenPassword";
        internal const string MemberGradeChange = "MemberGradeChange";
        internal const string ModifiedLoginPassword = "ModifiedLoginPassword";
        internal const string NewUserAccountCreated = "NewUserAccountCreated";
        internal const string OrderClosed = "OrderClosed";
        internal const string OrderCreate = "OrderCreate";
        internal const string OrderCreated = "OrderCreated";
        internal const string OrderDeliver = "OrderDeliver";
        internal const string OrderGetCommission = "OrderGetCommission";
        internal const string OrderGetCoupon = "OrderGetCoupon";
        internal const string OrderGetPoint = "OrderGetPoint";
        internal const string OrderPay = "OrderPay";
        internal const string OrderPayment = "OrderPayment";
        internal const string OrderRefund = "OrderRefund";
        internal const string OrderShipping = "OrderShipping";
        internal const string PasswordReset = "PasswordReset";
        internal const string PrizeRelease = "PrizeRelease";
        internal const string ProductAsk = "ProductAsk";
        internal const string ProductCreate = "ProductCreate";
        internal const string RefundSuccess = "RefundSuccess";
        internal const string ServiceRequest = "ServiceRequest";
    }
}

