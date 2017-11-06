using System;
using System.Web;

namespace HiTemplate
{

    public static class Urls
    {

        public static string ApplicationPath
        {

            get
            {
                string url = "http://";

                url = url + HttpContext.Current.Request.Url.Host;

                if (HttpContext.Current.Request.Url.Port != 80)
                {
                    url = url + ":" + HttpContext.Current.Request.Url.Port;
                }

                return url;

            }

        }

    }

}


