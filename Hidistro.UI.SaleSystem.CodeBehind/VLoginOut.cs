namespace Hidistro.UI.SaleSystem.CodeBehind
{
    using Hidistro.Core;
    using Hidistro.UI.Common.Controls;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web.UI;

    [ParseChildren(true)]
    public class VLoginOut : VshopTemplatedWebControl
    {
        protected override void AttachChildControls()
        {
            Globals.ClearUserCookie();
            this.Page.Response.Redirect(Globals.ApplicationPath + "/Default.aspx");
        }

        private string GetResponseResult(string url)
        {
            using (HttpWebResponse response = (HttpWebResponse) WebRequest.Create(url).GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.SkinName == null)
            {
                this.SkinName = "Skin-VLogout.html";
            }
            base.OnInit(e);
        }
    }
}

