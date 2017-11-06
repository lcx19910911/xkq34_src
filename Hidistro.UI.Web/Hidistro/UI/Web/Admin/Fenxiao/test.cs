namespace Hidistro.UI.Web.Admin.Fenxiao
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI;

    public class test : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string input = null;
            Match match = new Regex(@"<return_msg><!\[CDATA\[(?<code>(.*))\]\]></return_msg>").Match(input);
            if (match.Success)
            {
                input = match.Groups["code"].Value;
            }
            base.Response.Write(input);
        }
    }
}

