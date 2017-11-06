namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class VeriflyHelper
    {
        protected Hashtable parameters = new Hashtable();

        protected virtual void createSign()
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
            string parameterValue = Globals.GetMD5(builder.ToString());
            this.setParameter("sign", parameterValue);
        }

        public string getParameter(string parameter)
        {
            string str = (string) this.parameters[parameter];
            return ((str == null) ? "" : str);
        }

        public virtual string getRequestURL()
        {
            this.createSign();
            StringBuilder builder = new StringBuilder();
            ArrayList list = new ArrayList(this.parameters.Keys);
            list.Sort();
            foreach (string str in list)
            {
                string instr = (string) this.parameters[str];
                if ((instr != null) && ("key".CompareTo(str) != 0))
                {
                    builder.Append(str + "=" + Globals.UrlEncode(instr, "") + "&");
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
            }
            return ("?" + builder.ToString());
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

