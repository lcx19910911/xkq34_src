namespace Dfqm.Plugins.Payment.TenpayAssure
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal static class Globals
    {
        internal static StringBuilder AddParameter(StringBuilder buf, string parameterName, string parameterValue)
        {
            if ((parameterValue != null) && !"".Equals(parameterValue))
            {
                if ("".Equals(buf.ToString()))
                {
                    buf.Append(parameterName);
                    buf.Append("=");
                    buf.Append(parameterValue);
                }
                else
                {
                    buf.Append("&");
                    buf.Append(parameterName);
                    buf.Append("=");
                    buf.Append(parameterValue);
                }
            }
            return buf;
        }

        internal static string GetMD5(string encypStr)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(encypStr);
            return BitConverter.ToString(provider.ComputeHash(bytes)).Replace("-", "").ToUpper();
        }
    }
}

