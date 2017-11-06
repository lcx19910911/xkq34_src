﻿namespace Hidistro.SqlDal.Settings
{
    using Hidistro.Core;
    using Hidistro.Entities;
    using Hidistro.Entities.Settings;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Text;

    public class ShippingModeDao
    {
        private Database database = DatabaseFactory.CreateDatabase();
        public string Error = "";

        public bool CreateShippingTemplate(FreightTemplate freightTemplate)
        {
            this.Error = "";
            bool flag = false;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("INSERT INTO Hishop_FreightTemplate_Templates(Name,FreeShip,MUnit,HasFree)VALUES(@Name,@FreeShip,@MUnit,@HasFree)");
            this.database.AddInParameter(sqlStringCommand, "Name", DbType.String, freightTemplate.Name);
            this.database.AddInParameter(sqlStringCommand, "FreeShip", DbType.Int16, freightTemplate.FreeShip);
            this.database.AddInParameter(sqlStringCommand, "MUnit", DbType.Int16, freightTemplate.MUnit);
            this.database.AddInParameter(sqlStringCommand, "HasFree", DbType.Int16, freightTemplate.HasFree);
            using (DbConnection connection = this.database.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    try
                    {
                        this.database.ExecuteNonQuery(sqlStringCommand, transaction);
                        sqlStringCommand = this.database.GetSqlStringCommand("SELECT @@Identity");
                        object obj2 = this.database.ExecuteScalar(sqlStringCommand, transaction);
                        int result = 0;
                        if ((obj2 != null) && (obj2 != DBNull.Value))
                        {
                            int.TryParse(obj2.ToString(), out result);
                            this.Error = result.ToString();
                            flag = result > 0;
                        }
                        if (flag && !freightTemplate.FreeShip)
                        {
                            StringBuilder builder;
                            int num2;
                            int num3;
                            int num4;
                            DbCommand command = this.database.GetSqlStringCommand(" ");
                            this.database.AddInParameter(command, "TemplateId", DbType.Int32, result);
                            if ((freightTemplate.FreeShippings != null) && (freightTemplate.FreeShippings.Count > 0))
                            {
                                builder = new StringBuilder();
                                num2 = 0;
                                num3 = 0;
                                builder.Append("DECLARE @ERR INT; Set @ERR =0;");
                                builder.Append(" DECLARE @FreeId Int;");
                                foreach (FreeShipping shipping in freightTemplate.FreeShippings)
                                {
                                    builder.Append(" INSERT INTO Hishop_FreightTemplate_FreeShipping(TemplateId,ModeId,ConditionNumber,ConditionType) VALUES( @TemplateId,").Append("@ModeId").Append(num2).Append(",@ConditionNumber").Append(num2).Append(",@ConditionType").Append(num2).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                    this.database.AddInParameter(command, "ModeId" + num2, DbType.Int32, shipping.ModeId);
                                    this.database.AddInParameter(command, "ConditionNumber" + num2, DbType.String, shipping.ConditionNumber);
                                    this.database.AddInParameter(command, "ConditionType" + num2, DbType.Int32, shipping.ConditionType);
                                    builder.Append("Set @FreeId =@@identity;");
                                    if (shipping.FreeShippingRegions != null)
                                    {
                                        foreach (FreeShippingRegion region in shipping.FreeShippingRegions)
                                        {
                                            builder.Append(" INSERT INTO Hishop_FreightTemplate_FreeShippingRegions(FreeId,TemplateId,RegionId) VALUES(@FreeId,@TemplateId,").Append("@RegionId").Append(num3).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                            this.database.AddInParameter(command, "RegionId" + num3, DbType.Int32, region.RegionId);
                                            num3++;
                                        }
                                    }
                                    num2++;
                                }
                                command.CommandText = builder.Append("SELECT @ERR;").ToString();
                                num4 = (int) this.database.ExecuteScalar(command, transaction);
                                if (num4 != 0)
                                {
                                    this.Error = "指定包邮信息部份有错误，请查检！";
                                    transaction.Rollback();
                                    flag = false;
                                }
                            }
                            if ((flag && (freightTemplate.SpecifyRegionGroups != null)) && (freightTemplate.SpecifyRegionGroups.Count > 0))
                            {
                                command = this.database.GetSqlStringCommand(" ");
                                this.database.AddInParameter(command, "TemplateId", DbType.Int32, result);
                                builder = new StringBuilder();
                                num2 = 0;
                                num3 = 0;
                                builder.Append("DECLARE @ERR INT; Set @ERR =0;");
                                builder.Append(" DECLARE @GroupId Int;");
                                foreach (SpecifyRegionGroup group in freightTemplate.SpecifyRegionGroups)
                                {
                                    builder.Append(" INSERT INTO Hishop_FreightTemplate_SpecifyRegionGroups(TemplateId,ModeId,FristNumber,FristPrice,AddNumber,AddPrice,IsDefault) VALUES(@TemplateId,").Append("@ModeId").Append(num2).Append(",@FristNumber").Append(num2).Append(",@FristPrice").Append(num2).Append(",@AddNumber").Append(num2).Append(",@AddPrice").Append(num2).Append(",@IsDefault").Append(num2).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                    this.database.AddInParameter(command, "ModeId" + num2, DbType.Int16, group.ModeId);
                                    this.database.AddInParameter(command, "FristNumber" + num2, DbType.Decimal, group.FristNumber);
                                    this.database.AddInParameter(command, "FristPrice" + num2, DbType.Currency, group.FristPrice);
                                    this.database.AddInParameter(command, "AddPrice" + num2, DbType.Currency, group.AddPrice);
                                    this.database.AddInParameter(command, "AddNumber" + num2, DbType.Decimal, group.AddNumber);
                                    this.database.AddInParameter(command, "IsDefault" + num2, DbType.Int16, group.IsDefault);
                                    builder.Append("Set @GroupId =@@identity;");
                                    if (group.SpecifyRegions != null)
                                    {
                                        foreach (SpecifyRegion region2 in group.SpecifyRegions)
                                        {
                                            builder.Append(" INSERT INTO Hishop_FreightTemplate_SpecifyRegions(TemplateId,GroupId,RegionId) VALUES(@TemplateId,@GroupId").Append(",@RegionId").Append(num3).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                            this.database.AddInParameter(command, "RegionId" + num3, DbType.Int32, region2.RegionId);
                                            num3++;
                                        }
                                    }
                                    num2++;
                                }
                                command.CommandText = builder.Append("SELECT @ERR;").ToString();
                                num4 = (int) this.database.ExecuteScalar(command, transaction);
                                if (num4 != 0)
                                {
                                    this.Error = "运送方式部份信息有错误，请查检！";
                                    transaction.Rollback();
                                    flag = false;
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        if (transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }
                        flag = false;
                    }
                    return flag;
                }
                finally
                {
                    connection.Close();
                }
            }
            return flag;
        }

        public bool DeleteShippingTemplate(int templateId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("DELETE FROM Hishop_FreightTemplate_Templates Where TemplateId=@TemplateId");
            this.database.AddInParameter(sqlStringCommand, "TemplateId", DbType.Int32, templateId);
            return (this.database.ExecuteNonQuery(sqlStringCommand) > 0);
        }

        public int DeleteShippingTemplates(string templateIds)
        {
            int[] numArray = Array.ConvertAll<string, int>(templateIds.Split(new char[] { ',' }), s => int.Parse(s));
            string shippingTemplateLinkProduct = this.GetShippingTemplateLinkProduct(numArray);
            List<int> list = new List<int>();
            foreach (int num in numArray)
            {
                if (!("^" + shippingTemplateLinkProduct).Contains("^" + num.ToString() + "|"))
                {
                    list.Add(num);
                }
            }
            if (list.Count > 0)
            {
                DbCommand sqlStringCommand = this.database.GetSqlStringCommand("DELETE FROM Hishop_FreightTemplate_Templates Where TemplateId in(" + string.Join<int>(",", list) + ")");
                return this.database.ExecuteNonQuery(sqlStringCommand);
            }
            return 0;
        }

        public DataTable GetAllFreightItems()
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("select t.Name,t.FreeShip,t.MUnit,t.HasFree,sp.*,s.RegionId from Hishop_FreightTemplate_SpecifyRegionGroups as sp left join Hishop_FreightTemplate_SpecifyRegions as s on sp.GroupId=s.GroupId  left join Hishop_FreightTemplate_Templates as t on t.TemplateId=sp.TemplateId ");
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return DataHelper.ConverDataReaderToDataTable(reader);
            }
        }

        public DataTable GetFreeTemplateShipping(string RegionId, int TemplateId, int ModeId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Concat(new object[] { "select fs.*,f.RegionId from  dbo.Hishop_FreightTemplate_FreeShippingRegions as f left join dbo.Hishop_FreightTemplate_FreeShipping as fs on f.FreeId=fs.FreeId where f.RegionId='", RegionId, "' and fs.TemplateId=", TemplateId, " and fs.ModeId=", ModeId }));
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return DataHelper.ConverDataReaderToDataTable(reader);
            }
        }

        public FreightTemplate GetFreightTemplate(int templateId, bool includeDetail)
        {
            FreightTemplate template = new FreightTemplate();
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(" SELECT * FROM Hishop_FreightTemplate_Templates Where TemplateId =@TemplateId");
            if (includeDetail)
            {
                sqlStringCommand.CommandText = sqlStringCommand.CommandText + " SELECT * FROM Hishop_FreightTemplate_FreeShipping g,vw_Hishop_FreightTemplate_FreeShippingRegions r Where g.FreeId=r.FreeId and TemplateId =@TemplateId";
                sqlStringCommand.CommandText = sqlStringCommand.CommandText + " SELECT g.*,r.RegionIds FROM Hishop_FreightTemplate_SpecifyRegionGroups g  LEFT JOIN  vw_Hishop_FreightTemplate_SpecifyRegions r on (g.GroupId=r.GroupId) where  g.TemplateId =@TemplateId";
            }
            this.database.AddInParameter(sqlStringCommand, "TemplateId", DbType.Int32, templateId);
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                if (reader.Read())
                {
                    if (reader["TemplateId"] != DBNull.Value)
                    {
                        template.TemplateId = (int) reader["TemplateId"];
                    }
                    template.Name = (string) reader["Name"];
                    template.FreeShip = (bool) reader["FreeShip"];
                    template.HasFree = (bool) reader["HasFree"];
                    template.MUnit = (byte) reader["MUnit"];
                }
                if (!includeDetail)
                {
                    return template;
                }
                reader.NextResult();
                template.FreeShippings = new List<FreeShipping>();
                while (reader.Read())
                {
                    FreeShipping item = new FreeShipping {
                        TemplateId = (int) reader["TemplateId"],
                        ModeId = (byte) reader["ModeId"],
                        FreeId = (int) ((decimal) reader["FreeId"]),
                        ConditionNumber = (string) reader["ConditionNumber"],
                        ConditionType = (byte) reader["ConditionType"],
                        RegionIds = (string) reader["RegionIds"]
                    };
                    template.FreeShippings.Add(item);
                }
                reader.NextResult();
                template.SpecifyRegionGroups = new List<SpecifyRegionGroup>();
                while (reader.Read())
                {
                    SpecifyRegionGroup group = new SpecifyRegionGroup {
                        TemplateId = (int) reader["TemplateId"],
                        GroupId = (int) reader["GroupId"],
                        FristNumber = (decimal) reader["FristNumber"],
                        FristPrice = (decimal) reader["FristPrice"],
                        AddNumber = (decimal) reader["AddNumber"],
                        AddPrice = (decimal) reader["AddPrice"],
                        ModeId = (byte) reader["ModeId"],
                        IsDefault = (bool) reader["IsDefault"]
                    };
                    string str = "";
                    if (DBNull.Value != reader["RegionIds"])
                    {
                        str = (string) reader["RegionIds"];
                    }
                    group.RegionIds = str;
                    template.SpecifyRegionGroups.Add(group);
                }
            }
            return template;
        }

        public string GetShippingTemplateLinkProduct(int[] templateIds)
        {
            DataTable table;
            string str = "";
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("select ProductId as pid,FreightTemplateId as tid from Hishop_Products  Where FreightTemplateId in(" + string.Join<int>(",", templateIds) + ")");
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                table = DataHelper.ConverDataReaderToDataTable(reader);
            }
            if ((table != null) && (table.Rows.Count > 0))
            {
                foreach (DataRow row in table.Rows)
                {
                    object obj2 = str;
                    str = string.Concat(new object[] { obj2, "^", row["tid"], "|", row["pid"] });
                }
            }
            if (str != "")
            {
                str = str.Remove(0, 1);
            }
            return str;
        }

        public IList<SpecifyRegionGroup> GetSpecifyRegionGroups(int templateId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand("SELECT g.*,r.RegionIds FROM Hishop_FreightTemplate_SpecifyRegionGroups g  LEFT JOIN  vw_Hishop_FreightTemplate_SpecifyRegions r on (g.GroupId=r.GroupId) where  g.TemplateId =" + templateId);
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return ReaderConvert.ReaderToList<SpecifyRegionGroup>(reader);
            }
        }

        public DataTable GetSpecifyRegionGroupsModeId(string TemplateIds, string RegionId)
        {
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(" select distinct sp.ModeId from Hishop_FreightTemplate_SpecifyRegionGroups as sp left join Hishop_FreightTemplate_SpecifyRegions as s on sp.GroupId=s.GroupId where sp.TemplateId in(" + TemplateIds + ") and (s.RegionId='" + RegionId + "' or s.RegionId is null)");
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                return DataHelper.ConverDataReaderToDataTable(reader);
            }
        }

        public FreightTemplate GetTemplateMessage(int TemplateId)
        {
            if (TemplateId <= 0)
            {
                return null;
            }
            FreightTemplate template = null;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(string.Format("SELECT * FROM Hishop_FreightTemplate_Templates where TemplateId={0}", TemplateId));
            using (IDataReader reader = this.database.ExecuteReader(sqlStringCommand))
            {
                if (reader.Read())
                {
                    template = DataMapper.PopulateTemplateMessage(reader);
                }
            }
            return template;
        }

        public bool UpdateShippingTemplate(FreightTemplate freightTemplate)
        {
            if (freightTemplate.TemplateId == 0)
            {
                this.Error = "模板ID不存在！";
                return false;
            }
            bool flag = false;
            DbCommand sqlStringCommand = this.database.GetSqlStringCommand(new StringBuilder("UPDATE Hishop_FreightTemplate_Templates SET Name=@Name,FreeShip=@FreeShip,MUnit=@MUnit,HasFree=@HasFree WHERE TemplateId=@TemplateId;").ToString());
            this.database.AddInParameter(sqlStringCommand, "Name", DbType.String, freightTemplate.Name);
            this.database.AddInParameter(sqlStringCommand, "FreeShip", DbType.Int16, freightTemplate.FreeShip);
            this.database.AddInParameter(sqlStringCommand, "MUnit", DbType.Int16, freightTemplate.MUnit);
            this.database.AddInParameter(sqlStringCommand, "TemplateId", DbType.Int32, freightTemplate.TemplateId);
            this.database.AddInParameter(sqlStringCommand, "HasFree", DbType.Int16, freightTemplate.HasFree);
            using (DbConnection connection = this.database.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {
                    flag = this.database.ExecuteNonQuery(sqlStringCommand, transaction) > 0;
                    DbCommand command = this.database.GetSqlStringCommand(" ");
                    StringBuilder builder2 = new StringBuilder();
                    if (flag)
                    {
                        this.database.AddInParameter(command, "TemplateId", DbType.Int32, freightTemplate.TemplateId);
                        builder2.Append("delete from Hishop_FreightTemplate_SpecifyRegionGroups WHERE TemplateId=@TemplateId;");
                        builder2.Append("delete from Hishop_FreightTemplate_FreeShipping WHERE TemplateId=@TemplateId;");
                        builder2.Append("delete from Hishop_FreightTemplate_FreeShippingRegions WHERE TemplateId=@TemplateId;");
                        builder2.Append("delete from Hishop_FreightTemplate_SpecifyRegions WHERE TemplateId=@TemplateId;");
                        command.CommandText = builder2.ToString();
                        this.database.ExecuteNonQuery(command, transaction);
                    }
                    if (flag && !freightTemplate.FreeShip)
                    {
                        int num;
                        int num2;
                        int num3;
                        if ((freightTemplate.FreeShippings != null) && (freightTemplate.FreeShippings.Count > 0))
                        {
                            command = this.database.GetSqlStringCommand(" ");
                            this.database.AddInParameter(command, "TemplateId", DbType.Int32, freightTemplate.TemplateId);
                            builder2.Clear();
                            num = 0;
                            num2 = 0;
                            builder2.Append("DECLARE @ERR INT; Set @ERR =0;");
                            builder2.Append(" DECLARE @FreeId Int;");
                            foreach (FreeShipping shipping in freightTemplate.FreeShippings)
                            {
                                builder2.Append(" INSERT INTO Hishop_FreightTemplate_FreeShipping(TemplateId,ModeId,ConditionNumber,ConditionType) VALUES( @TemplateId,").Append("@ModeId").Append(num).Append(",@ConditionNumber").Append(num).Append(",@ConditionType").Append(num).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                this.database.AddInParameter(command, "ModeId" + num, DbType.Int32, shipping.ModeId);
                                this.database.AddInParameter(command, "ConditionNumber" + num, DbType.String, shipping.ConditionNumber);
                                this.database.AddInParameter(command, "ConditionType" + num, DbType.Int32, shipping.ConditionType);
                                builder2.Append("Set @FreeId =@@identity;");
                                if (shipping.FreeShippingRegions != null)
                                {
                                    foreach (FreeShippingRegion region in shipping.FreeShippingRegions)
                                    {
                                        builder2.Append(" INSERT INTO Hishop_FreightTemplate_FreeShippingRegions(FreeId,TemplateId,RegionId) VALUES(@FreeId,@TemplateId").Append(",@RegionId").Append(num2).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                        this.database.AddInParameter(command, "RegionId" + num2, DbType.Int32, region.RegionId);
                                        num2++;
                                    }
                                }
                                num++;
                            }
                            command.CommandText = builder2.Append("SELECT @ERR;").ToString();
                            num3 = (int) this.database.ExecuteScalar(command, transaction);
                            if (num3 != 0)
                            {
                                transaction.Rollback();
                                flag = false;
                            }
                        }
                        if ((flag && (freightTemplate.SpecifyRegionGroups != null)) && (freightTemplate.SpecifyRegionGroups.Count > 0))
                        {
                            command = this.database.GetSqlStringCommand(" ");
                            this.database.AddInParameter(command, "TemplateId", DbType.Int32, freightTemplate.TemplateId);
                            builder2.Clear();
                            num = 0;
                            num2 = 0;
                            builder2.Append("DECLARE @ERR INT; Set @ERR =0;");
                            builder2.Append(" DECLARE @GroupId Int;");
                            foreach (SpecifyRegionGroup group in freightTemplate.SpecifyRegionGroups)
                            {
                                builder2.Append(" INSERT INTO Hishop_FreightTemplate_SpecifyRegionGroups(TemplateId,ModeId,FristNumber,FristPrice,AddNumber,AddPrice,IsDefault) VALUES(@TemplateId,").Append("@ModeId").Append(num).Append(",@FristNumber").Append(num).Append(",@FristPrice").Append(num).Append(",@AddNumber").Append(num).Append(",@AddPrice").Append(num).Append(",@IsDefault").Append(num).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                this.database.AddInParameter(command, "ModeId" + num, DbType.Int16, group.ModeId);
                                this.database.AddInParameter(command, "FristNumber" + num, DbType.Decimal, group.FristNumber);
                                this.database.AddInParameter(command, "FristPrice" + num, DbType.Currency, group.FristPrice);
                                this.database.AddInParameter(command, "AddPrice" + num, DbType.Currency, group.AddPrice);
                                this.database.AddInParameter(command, "AddNumber" + num, DbType.Decimal, group.AddNumber);
                                this.database.AddInParameter(command, "IsDefault" + num, DbType.Int16, group.IsDefault);
                                builder2.Append("Set @GroupId =@@identity;");
                                if (group.SpecifyRegions != null)
                                {
                                    foreach (SpecifyRegion region2 in group.SpecifyRegions)
                                    {
                                        builder2.Append(" INSERT INTO Hishop_FreightTemplate_SpecifyRegions(TemplateId,GroupId,RegionId) VALUES(@TemplateId,@GroupId").Append(",@RegionId").Append(num2).Append("); SELECT @ERR=@ERR+@@ERROR;");
                                        this.database.AddInParameter(command, "RegionId" + num2, DbType.Int32, region2.RegionId);
                                        num2++;
                                    }
                                }
                                num++;
                            }
                            command.CommandText = builder2.Append("SELECT @ERR;").ToString();
                            num3 = (int) this.database.ExecuteScalar(command, transaction);
                            if (num3 != 0)
                            {
                                transaction.Rollback();
                                flag = false;
                            }
                        }
                    }
                    if (flag)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                    return flag;
                }
                catch
                {
                    if (transaction.Connection != null)
                    {
                        transaction.Rollback();
                    }
                    flag = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return flag;
        }
    }
}

