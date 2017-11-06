namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class ReturnHelper
    {
        protected Hashtable parameters = new Hashtable();

        public ReturnHelper(NameValueCollection collection)
        {
            collection.Remove("HIGW");
            foreach (string str in collection)
            {
                string parameterValue = collection[str];
                this.setParameter(str, parameterValue);
            }
        }

        public string getParameter(string parameter)
        {
            string str = (string) this.parameters[parameter];
            return ((str == null) ? "" : str);
        }

        public virtual bool isTenpaySign()
        {
            StringBuilder builder = new StringBuilder();
            ArrayList list = new ArrayList(this.parameters.Keys);
            list.Sort();
            foreach (string str in list)
            {
                string strB = (string) this.parameters[str];
                if ((((strB != null) && ("".CompareTo(strB) != 0)) && ("sign".CompareTo(str) != 0)) && ("key".CompareTo(str) != 0))
                {
                    builder.Append(str + "=" + strB + "&");
                }
            }
            builder.Append("key=" + this.Key);
            string str3 = Globals.GetMD5(builder.ToString()).ToLower();
            return this.getParameter("sign").ToLower().Equals(str3);
        }

        public void setParameter(string parameter, string parameterValue)
        {
            if ((parameter != null) && (parameter != ""))
            {
                if (this.parameters.Contains(parameter))
                {
                    this.parameters.Remove(parameter);
                }
                this.parameters.Add(parameter, parameterValue);
            }
        }

        public string Key { get; set; }
    }
}

