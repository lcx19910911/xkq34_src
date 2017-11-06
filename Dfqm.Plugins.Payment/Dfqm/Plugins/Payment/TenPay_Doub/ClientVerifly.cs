namespace Dfqm.Plugins.Payment.TenPay_Doub
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;

    public class ClientVerifly
    {
        protected string content;
        protected Hashtable parameters = new Hashtable();

        public string getContent()
        {
            return this.content;
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
            builder.Append("key=" + this.key);
            string str3 = Globals.GetMD5(builder.ToString()).ToLower();
            return this.getParameter("sign").ToLower().Equals(str3);
        }

        public virtual void setContent(string content)
        {
            this.content = content;
            XmlDocument document = new XmlDocument();
            document.LoadXml(content);
            XmlNodeList childNodes = document.SelectSingleNode("root").ChildNodes;
            foreach (XmlNode node2 in childNodes)
            {
                this.setParameter(node2.Name, node2.InnerXml);
            }
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

        public string key { get; set; }
    }
}

