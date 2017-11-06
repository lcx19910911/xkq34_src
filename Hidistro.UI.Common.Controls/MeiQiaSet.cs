namespace Hidistro.UI.Common.Controls
{
    using Hidistro.Core;
    using Hidistro.Core.Entities;
    using System;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class MeiQiaSet : Literal
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Text = "";
            if (SettingsManager.GetMasterSettings(false).EnableSaleService)
            {
                CustomerServiceSettings masterSettings = CustomerServiceManager.GetMasterSettings(false);
                string str = string.Empty;
                StringBuilder builder = new StringBuilder();
                string str2 = string.Empty;
                if ((!string.IsNullOrEmpty(masterSettings.unitid) && !string.IsNullOrEmpty(masterSettings.unit)) && !string.IsNullOrEmpty(masterSettings.password))
                {
                    str = string.Format("<script src='//meiqia.com/js/mechat.js?unitid={0}&btn=hide' charset='UTF-8' async='async'></script>", masterSettings.unitid);
                    builder.Append("<script type=\"text/javascript\">");
                    builder.Append("function mechatFuc()");
                    builder.Append("{");
                    builder.Append("$.get(\"/Api/Hi_Ajax_OnlineServiceConfig.ashx\", function (data) {");
                    builder.Append("if (data != \"\") {");
                    builder.Append("$(data).appendTo('head');");
                    builder.Append("}");
                    builder.Append("mechatClick();");
                    builder.Append("});");
                    builder.Append("}");
                    builder.Append("</script>");
                    str2 = "<!-- 在线客服 -->\n<div class=\"customer-service\" style=\"position:fixed;bottom:100px;right:10%;width:38px;height:38px;background:url(/Utility/pics/service.png?v1026) no-repeat;background-size:100%;cursor:pointer;\" onclick=\"javascript:mechatFuc();\"></div>";
                    base.Text = str + "\n" + builder.ToString() + "\n" + str2;
                }
            }
            base.Render(writer);
        }
    }
}

