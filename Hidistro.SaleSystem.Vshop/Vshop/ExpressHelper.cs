namespace Hidistro.Vshop
{
    using Hidistro.Entities.Sales;
    using Hidistro.SqlDal;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using System.Web;

    public static class ExpressHelper
    {
        private static string path = HttpContext.Current.Request.MapPath("~/Express.xml");

        public static void AddExpress(string name, string kuaidi100Code, string taobaoCode)
        {
            XmlDocument xmlNode = GetXmlNode();
            XmlNode node = xmlNode.SelectSingleNode("companys");
            XmlElement newChild = xmlNode.CreateElement("company");
            newChild.SetAttribute("name", name);
            newChild.SetAttribute("Kuaidi100Code", kuaidi100Code);
            newChild.SetAttribute("TaobaoCode", taobaoCode);
            newChild.SetAttribute("New", "Y");
            node.AppendChild(newChild);
            xmlNode.Save(path);
        }

        public static void DeleteExpress(string name)
        {
            XmlDocument xmlNode = GetXmlNode();
            XmlNode node = xmlNode.SelectSingleNode("companys");
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (node2.Attributes["name"].Value == name)
                {
                    node.RemoveChild(node2);
                    break;
                }
            }
            xmlNode.Save(path);
        }

        public static ExpressCompanyInfo FindNode(string company)
        {
            ExpressCompanyInfo info = null;
            XmlDocument xmlNode = GetXmlNode();
            string xpath = string.Format("//company[@name='{0}']", company);
            XmlNode node = xmlNode.SelectSingleNode(xpath);
            if (node != null)
            {
                info = new ExpressCompanyInfo {
                    Name = company,
                    Kuaidi100Code = node.Attributes["Kuaidi100Code"].Value,
                    TaobaoCode = node.Attributes["TaobaoCode"].Value
                };
            }
            return info;
        }

        public static ExpressCompanyInfo FindNodeByCode(string code)
        {
            ExpressCompanyInfo info = null;
            XmlDocument xmlNode = GetXmlNode();
            string xpath = string.Format("//company[@TaobaoCode='{0}']", code);
            XmlNode node = xmlNode.SelectSingleNode(xpath);
            if (node != null)
            {
                info = new ExpressCompanyInfo {
                    Name = node.Attributes["name"].Value,
                    Kuaidi100Code = node.Attributes["Kuaidi100Code"].Value,
                    TaobaoCode = code
                };
            }
            return info;
        }

        public static IList<ExpressCompanyInfo> GetAllExpress()
        {
            IList<ExpressCompanyInfo> list = new List<ExpressCompanyInfo>();
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            foreach (XmlNode node2 in node.ChildNodes)
            {
                ExpressCompanyInfo item = new ExpressCompanyInfo {
                    Name = node2.Attributes["name"].Value,
                    Kuaidi100Code = node2.Attributes["Kuaidi100Code"].Value,
                    TaobaoCode = node2.Attributes["TaobaoCode"].Value
                };
                list.Add(item);
            }
            return list;
        }

        public static IList<string> GetAllExpressName()
        {
            IList<string> list = new List<string>();
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            foreach (XmlNode node2 in node.ChildNodes)
            {
                list.Add(node2.Attributes["name"].Value);
            }
            return list;
        }

        private static string GetContentByAPI(string key, string computer, string expressNo)
        {
            HttpWebResponse response;
            string str = "";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("http://api.kuaidi100.com/api?id=" + key + "&com=" + computer + "&nu=" + expressNo);
            request.Timeout = 0x1f40;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
            }
            catch
            {
                return str;
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                str = (str + reader.ReadToEnd()).Replace("&amp;", "").Replace("&nbsp;", "").Replace("&", "");
            }
            return str;
        }

        public static string GetDataByKuaidi100(string computer, string expressNo)
        {
            string key = "29833628d495d7a5";
            string str2 = "";
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            string str3 = "{\"message\": \"ok\",\"content\": ";
            if (node != null)
            {
                str2 = node.Attributes["Kuaidi100NewKey"].Value;
            }
            if (!string.IsNullOrEmpty(str2))
            {
                string expressDataList = new ExpressDataDao().GetExpressDataList(computer, expressNo);
                if (!string.IsNullOrEmpty(expressDataList))
                {
                    str3 = str3 + expressDataList + ",\"type\":\"1\"";
                }
                else
                {
                    str3 = str3 + GetContentByAPI(str2, computer, expressNo) + ",\"type\":\"2\"";
                }
            }
            else
            {
                str3 = str3 + GetContentByAPI(key, computer, expressNo) + ",\"type\":\"2\"";
            }
            return (str3 + "}");
        }

        public static string GetExpressData(string computer, string expressNo)
        {
            return GetDataByKuaidi100(computer, expressNo);
        }

        public static ExpressSet GetExpressSet()
        {
            ExpressSet set = new ExpressSet();
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            if (node != null)
            {
                set.Key = node.Attributes["Kuaidi100Key"].Value;
                set.NewKey = node.Attributes["Kuaidi100NewKey"].Value;
                set.Url = node.Attributes["Url"].Value;
            }
            return set;
        }

        public static DataTable GetExpressTable()
        {
            DataTable table = new DataTable();
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("Name");
            table.Columns.Add("Kuaidi100Code");
            table.Columns.Add("TaobaoCode");
            table.Columns.Add("New");
            int num = 0;
            foreach (XmlNode node2 in node.ChildNodes)
            {
                DataRow row = table.NewRow();
                row["id"] = num;
                row["Name"] = node2.Attributes["name"].Value;
                row["Kuaidi100Code"] = node2.Attributes["Kuaidi100Code"].Value;
                row["TaobaoCode"] = node2.Attributes["TaobaoCode"].Value;
                if (node2.Attributes["New"] != null)
                {
                    row["New"] = node2.Attributes["New"].Value;
                }
                else
                {
                    row["New"] = "N";
                }
                table.Rows.Add(row);
                num++;
            }
            return table;
        }

        private static XmlDocument GetXmlNode()
        {
            XmlDocument document = new XmlDocument();
            if (!string.IsNullOrEmpty(path))
            {
                document.Load(path);
            }
            return document;
        }

        public static bool IsExitExpress(string name)
        {
            XmlNode node = GetXmlNode().SelectSingleNode("companys");
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (node2.Attributes["name"].Value == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void UpdateExpress(string oldcompanyname, string name, string kuaidi100Code, string taobaoCode)
        {
            XmlDocument xmlNode = GetXmlNode();
            XmlNode node = xmlNode.SelectSingleNode("companys");
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (node2.Attributes["name"].Value == oldcompanyname)
                {
                    node2.Attributes["name"].Value = name;
                    node2.Attributes["Kuaidi100Code"].Value = kuaidi100Code;
                    node2.Attributes["TaobaoCode"].Value = taobaoCode;
                    break;
                }
            }
            xmlNode.Save(path);
        }

        public static void UpdateExpressUrlAndKey(string key, string url)
        {
            XmlDocument xmlNode = GetXmlNode();
            XmlNode node = xmlNode.SelectSingleNode("companys");
            if (node != null)
            {
                node.Attributes["Kuaidi100NewKey"].Value = key;
                node.Attributes["Url"].Value = url;
            }
            xmlNode.Save(path);
        }
    }
}

