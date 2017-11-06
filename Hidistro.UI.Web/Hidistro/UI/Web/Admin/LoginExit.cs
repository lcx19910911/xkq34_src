using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Hidistro.Core;

namespace Hidistro.UI.Web.Admin
{
    /// <summary>
    /// 用户退出登陆
    /// </summary>
    public class LoginExit : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            HttpCookie userCookie = HttpContext.Current.Request.Cookies.Get(string.Format("{0}{1}", Globals.DomainName, FormsAuthentication.FormsCookieName));

            if (null != userCookie)
            {
                userCookie.Expires = DateTime.Now;
                HttpContext.Current.Response.Cookies.Add(userCookie);
            }

            Response.Redirect("Login.aspx", true);

        }

    }


}

