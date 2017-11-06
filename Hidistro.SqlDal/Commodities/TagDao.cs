namespace Hidistro.SqlDal.Commodities
{
    using Hidistro.Core;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public class TagDao
    {
        private Database database = DatabaseFactory.CreateDatabase();

        public bool AddProductTags(int productId, IList<int> tagIds, DbTransaction tran)
        {
            bool flag = false;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("INSERT INTO Hishop_ProductTag VALUES(@TagId,@ProductId)");
            this.database.AddInParameter(sqlStringCommand, "TagId", DbType.Int32);
            this.database.AddInParameter(sqlStringCommand, "ProductId", DbType.Int32);
            foreach (int num in tagIds)
            {
                this.database.SetParameterValue(sqlStringCommand, "ProductId", productId);
                this.database.SetParameterValue(sqlStringCommand, "TagId", num);
                if (tran != null)
                {
                    flag = this.database.ExecuteNonQuery(sqlStringCommand, tran) > 0;
                }
                else
                {
                    flag = this.database.ExecuteNonQuery(sqlStringCommand) > 0;
                }
                if (!flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        public int AddTags(string tagname)
        {
            int num = 0;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("INSERT INTO Hishop_Tags VALUES(@TagName);SELECT @@IDENTITY");
            this.database.AddInParameter(sqlStringCommand, "TagName", DbType.String, Globals.SubStr(tagname, 8, ""));
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if (obj2 != null)
            {
                num = Convert.ToInt32(obj2.ToString());
            }
            return num;
        }

        public bool DeleteProductTags(int productId, DbTransaction tran)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("DELETE FROM Hishop_ProductTag WHERE ProductId=@ProductId");
            this.database.AddInParameter(sqlStringCommand, "ProductId", DbType.Int32, productId);
            if (tran != null)
            {
                return (this.database.ExecuteNonQuery(sqlStringCommand, tran) >= 0);
            }
            return (this.database.ExecuteNonQuery(sqlStringCommand) >= 0);
        }

        public bool DeleteTags(int tagId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("DELETE FROM Hishop_ProductTag WHERE TagID=@TagID;DELETE FROM Hishop_Tags WHERE TagID=@TagID;");
            this.database.AddInParameter(sqlStringCommand, "TagID", DbType.Int32, tagId);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public string GetProductTagName(int productId)
        {
            string str = string.Empty;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("select top 1 b.TagName from Hishop_ProductTag a inner join Hishop_Tags b on a.tagid=b.tagid where ProductId=@ProductId");
            this.database.AddInParameter(sqlStringCommand, "ProductId", DbType.Int32, productId);
            DataTable table = this.database.ExecuteDataSet(sqlStringCommand).Tables[0];
            if (table.Rows.Count > 0)
            {
                str = table.Rows[0]["TagName"].ToString();
            }
            return str;
        }

        public string GetTagName(int tagId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("SELECT TagName  FROM  Hishop_Tags WHERE TagID = {0}", tagId));
            object obj2 = this.database.ExecuteScalar(sqlStringCommand);
            if (obj2 != null)
            {
                return obj2.ToString();
            }
            return string.Empty;
        }

        public DataTable GetTags()
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("SELECT *  FROM  Hishop_Tags");
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return DataHelper.ConverDataReaderToDataTable(reader);
            }
        }

        public int GetTags(string tagName)
        {
            int num = 0;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("SELECT TagID  FROM  Hishop_Tags WHERE TagName=@TagName");
            this.database.AddInParameter(sqlStringCommand, "TagName", DbType.String, tagName);
            IDataReader reader = this.database.ExecuteReader(sqlStringCommand);
            if (reader.Read())
            {
                num = Convert.ToInt32(reader["TagID"].ToString());
            }
            return num;
        }

        public bool UpdateTags(int tagId, string tagname)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("UPDATE Hishop_Tags SET TagName=@TagName WHERE TagID=@TagID");
            this.database.AddInParameter(sqlStringCommand, "TagName", DbType.String, Globals.SubStr(tagname, 8, ""));
            this.database.AddInParameter(sqlStringCommand, "TagID", DbType.Int32, tagId);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }
    }
}

