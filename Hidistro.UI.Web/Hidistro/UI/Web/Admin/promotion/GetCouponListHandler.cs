namespace Hidistro.UI.Web.Admin.promotion
{
    using Hidistro.ControlPanel.Promotions;
    using Newtonsoft.Json;
    using System;
    using System.Data;
    using System.Linq;
    using System.Web;

    public class GetCouponListHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            try
            {
                string[] allKeys = context.Request.Params.AllKeys;
                DateTime now = DateTime.Now;
                if (allKeys.Contains<string>("time"))
                {
                    now = DateTime.Parse(context.Request["time"].ToString());
                }
                DataTable unFinishedCoupon = CouponHelper.GetUnFinishedCoupon(now, Hidistro.Entities.Promotions.CouponType.活动赠送);
                var type = new
                {
                    type = "success",
                    data = unFinishedCoupon
                };
                string s = JsonConvert.SerializeObject(type);
                context.Response.Write(s);
            }
            catch (Exception exception)
            {
                context.Response.Write("{\"type\":\"error\",\"data\":\"" + exception.Message + "\"}");
            }
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

